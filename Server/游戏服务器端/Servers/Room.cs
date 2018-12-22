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
        private int readyNum;

        public int ReadyNum { get => readyNum; set => readyNum = value; }
        public int Type { get => type; set => type = value; }
        public List<Client> ClientRoom { get => clientRoom; set => clientRoom = value; }


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
            return state == RoomState.WaitingJoin;
        }
        public void AddClient(Client client)
        {
          
            ClientRoom.Add(client);
            client.Room = this;
            //这里把2改成type，人满了就把状态转变成等待战斗
            if (ClientRoom.Count>= type)
            {
                state = RoomState.WaitingBattle;
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
                state = RoomState.WaitingBattle;
            }
            else
            {
                state = RoomState.WaitingJoin;
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
            foreach(Client client in ClientRoom)
            {
                if (client != excludeClient)
                {
                    server.SendResponse(client, actionCode, data);
                }
            }
        }

        //判断是否是房主
        public bool IsHouseOwner(Client client)
        {
            return client == ClientRoom[0];
        }

        //退出房间
        public void QuitRoom(Client client)
        {
            //退出房间其状态复位
            client.InitData();
            ClientRoom.Remove(client);
            if(ClientRoom.Count == 0)
            {
                Close();
            }
        }
        //房间销毁，
        public void Close()
        {
            server.RemoveRoom(this);
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
            for (int i = 0; i < ClientRoom.Count; i++)
            {

                //return userName
                if (ClientRoom[i].User.Id == id)
                {
                    //返回值是 该角色是否死亡，并且扣血动作已经完成
                    if (ClientRoom[i].TakeDamage(damage))
                    {
                        isDie = true;
                        ClientRoom[i].UpdateResult(false);
                        ClientRoom[i].Send(ActionCode.GameOver, ((int)ReturnCode.Fail).ToString());
                        ClientRoom.RemoveAt(i);//该客户端已经死亡，移出房间
                        //通知其他玩家该角色已经死亡
                        BroadcastMessage(null, ActionCode.NotifyDeath, "r");
                        i--;//注意，此处若不减1导致下一个客户端无法被访问
                    }
                }
            }
            //如果没人死亡则返回，否则看是否只剩下一个人
            if (isDie == false) return;
           
            //如果所有其他角色死亡则结束整个游戏  除去单人模式
            if (ClientRoom.Count == 1 && this.Type != 1 )
            {

                ClientRoom[0].UpdateResult(true);
                ClientRoom[0].Send(ActionCode.GameOver, ((int)ReturnCode.Success).ToString());
                Close();//房间销毁
            }


            //如果其中一个角色死亡，要结束游戏
            /*
            //若该角色死亡则结束该客户端游戏
            foreach (Client client in clientRoom)
            {
                if (client.IsDie())
                {
                    client.UpdateResult(false);
                    client.Send(ActionCode.GameOver, ((int)ReturnCode.Fail).ToString());
                }
                
                else
                {
                    client.UpdateResult(true);
                    client.Send(ActionCode.GameOver, ((int)ReturnCode.Success).ToString());
                }
                
            }*/
            
        }
    }
}
