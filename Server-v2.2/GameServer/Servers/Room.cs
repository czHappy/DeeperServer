using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using System.Threading;

namespace GameServer.Servers
{
    enum RoomState
    {
        WaitingJoin,
        WaitingBattle,
        Battle,
        End
    }
    class Room
    {
        //房间类型就是房间人数
        private int type;
    
        //private const int MAX_HP = 200;
        private List<Client> clientRoom = new List<Client>();
        private RoomState state = RoomState.WaitingJoin;
        private Server server;

        public int ReadyNum { get => readyNum; set => readyNum = value; }
        public int Type { get => type; set => type = value; }
        public List<Client> ClientRoom { get => clientRoom; set => clientRoom = value; }
        internal RoomState State { get => state; set => state = value; }


        //获取房间已经准备的人数
        public int GetReadyNum()
        {

            int num = 0;
            foreach(Client c in clientRoom)
            {
                if (c.IsReady)
                {
                    num++;
                }
            }
            return num;
        }
  
        public Room(Server server)
        {
            this.server = server;
            this.type = 4;
        }

        public Room(Server server,int type)
        {
            this.server = server;
            this.type = type;
        }

        public bool IsWaitingJoin()
        {
            return State == RoomState.WaitingJoin;
        }

        public void AddClient(Client client)
        {
          
            ClientRoom.Add(client);
            client.Room = this;
            //这里把2改成type，人满了就把状态转变成等待战斗
            if (ClientRoom.Count>= type)
            {
                State = RoomState.WaitingBattle;
            }
        }

        public void RemoveClient(Client client)
        { 
            QuitRoom(client); 

            if(this == null)
            {
                Console.WriteLine("房间野指针出现");
            }

            //这里把2改成type
            if (ClientRoom.Count >= Type)
            {
                State = RoomState.WaitingBattle;
            }
            else
            {
                State = RoomState.WaitingJoin;
            }
        }
        //返回房间信息（创建者信息）以及房间类型
        //user.Id+","+ user.Username + "," + result.TotalCount + "," + result.WinCount + "," + Room.type
        public string GetHouseInfo()
        {
            if(ClientRoom == null || ClientRoom.Count == 0)
            {
                return "";
            }
            return ClientRoom[0].GetUserData() + "," + this.Type + "," + ClientRoom.Count;
        }

        //返回房间的房主的Id
        public int GetId()
        {
            if (ClientRoom.Count > 0)
            {
                return ClientRoom[0].GetUserId();
            }
            return -1;
        }

        //返回房间内所有玩家信息 userdata|userdata|userdata
        public String GetRoomData()
        {
            StringBuilder sb = new StringBuilder();
            foreach(Client client in ClientRoom)
            {
                sb.Append(client.GetUserData() + "|");
            }
            if (sb.Length > 0)
            {
                sb.Remove(sb.Length - 1, 1);
            }
            return sb.ToString();
        }

        //在房间内广播消息到客户端，除了excludeClient
        public void BroadcastMessage(Client excludeClient,ActionCode actionCode,string data)
        {
            //Console.WriteLine(actionCode);
            
            foreach (Client client in ClientRoom)
            {
                if (client != excludeClient)
                {
                    server.SendResponse(client, actionCode, data);
                }
            }
        }


        //退出房间
        public void QuitRoom(Client client)
        {
       
           
            if(this == null)
            {
                Console.WriteLine("room 被置空，错误！");
            }
            if(ClientRoom.Count == 0)
            {
                Console.WriteLine("room 没人，忽略！");
                return;
            }
            ClientRoom.Remove(client);

            if (ClientRoom.Count == 0)
            {
                Console.WriteLine("id = "+client.GetUserId() + "退出房间导致房间内人数为0，关闭房间！");
                Close();
            }
        }
        //房间销毁，
        public void Close()
        {
            server.RemoveRoom(this);
            if(this == null)
            {
                Console.WriteLine("room 被彻底销毁！");
            }
        }
        //定时器
        public void StartTimer()
        {
            new Thread(RunTimer).Start();
        }
        //定时器线程函数
        private void RunTimer()
        {
            Thread.Sleep(1000);
            for (int i = 3; i > 0; i--)
            {
                BroadcastMessage(null, ActionCode.ShowTimer, i.ToString());
                Thread.Sleep(1000);
            }
            //3秒过后就可以开始玩了
            BroadcastMessage(null, ActionCode.StartPlay, "StartPlay");
        }

        //打伤害
        //每次都找是否比较麻烦，实现存一个映射O（1）找如何？
        public void TakeDamage(int id, int damage)
        {
            
            bool isDie = false;
            //clientRoom.Count会变，死一个就少一个
            foreach (Client client in clientRoom)
            {
            
                //return userName
                if (client.User.Id == id)
                {
                    //返回值是 该角色是否死亡，并且扣血动作已经完成
                    if (client.TakeDamage(damage))
                    {
                        isDie = true;
                        Console.WriteLine("该角色被击杀");
                        try
                        {
                            client.UpdateResult(false);
                        }
                        catch (Exception e)
                        {

                            Console.WriteLine("更新战绩异常：" + e.Message+e.TargetSite);
                        }

                        client.Send(ActionCode.GameOver, ((int)ReturnCode.Fail).ToString());

                        //通知其他玩家该角色已经死亡
                        BroadcastMessage(client, ActionCode.NotifyDeath, id.ToString());

                        // ClientRoom.RemoveAt(i);//该客户端已经死亡，移出房间
                        this.QuitRoom(client);
                        break;//跳出
                    }
                }
            }
            //如果没人死亡则返回，否则看是否只剩下一个人
            if (isDie == false) return;
           
            //如果所有其他角色死亡则结束整个游戏  除去单人模式
            if (ClientRoom.Count == 1 && this.Type != 1 )
            {
                Console.WriteLine("只剩下一个人：id = " + ClientRoom[0].GetUserId());
                try
                {
                    ClientRoom[0].UpdateResult(true);
                }
                catch (Exception e)
                {

                    Console.WriteLine("更新战绩异常：" + e.Message + e.TargetSite);
                }
                
                ClientRoom[0].Send(ActionCode.GameOver, ((int)ReturnCode.Success).ToString());
                //Close();//房间销毁
                this.QuitRoom(ClientRoom[0]);
                if(this != null)
                    this.state = RoomState.End;
            }

            
        }
    }
}
