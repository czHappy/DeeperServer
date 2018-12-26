using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using GameServer.Servers;
namespace GameServer.Controller
{
    class RoomController:BaseController
    {
        public RoomController()
        {
            requestCode = RequestCode.Room;
        }

        public string CreateRoom(string data, Client client, Server server)
        {
            client.InitData();
            server.CreateRoom(client,int.Parse(data));
            // return ((int)ReturnCode.Success).ToString()+","+ ((int)RoleType.Blue).ToString();
            return ((int)ReturnCode.Success).ToString();
        }
        public string ListRoom(string data, Client client, Server server)
        {
            StringBuilder sb = new StringBuilder();
            foreach(Room room in server.GetRoomList())
            {
                if (room.IsWaitingJoin())
                {
                    sb.Append(room.GetHouseInfo()+"|");
                }
            }
            if (sb.Length == 0)
            {
                sb.Append("0");
            }
            else
            {
                sb.Remove(sb.Length - 1, 1);
            }

            return sb.ToString();
        }
        
        //加入房间
        public string JoinRoom(string data, Client client, Server server)
        {

            client.InitData();
            //这里的id应该是房主的id
            int id = int.Parse(data);
            Room room = server.GetRoomById(id);
            if(room == null)
            {
                return ((int)ReturnCode.NotFound).ToString();
            }
            //防止人满了还能加入
            else if (room.IsWaitingJoin() == false)
            {
                return ((int)ReturnCode.Fail).ToString();
            }
            else
            {
                room.AddClient(client);
                string roomData = room.GetRoomData();//"returncode,roletype-id,username,tc,wc|id,username,tc,wc"
                room.BroadcastMessage(client, ActionCode.UpdateRoom, roomData);
                return ((int)ReturnCode.Success).ToString() + "@" +  roomData;
            }
        }
        public string Ready(string data, Client client, Server server)
        {
            //别忘了玩家退出之后ready状态需要销毁
            Room room = client.Room;
            if (!client.IsReady)
            {
                client.IsReady = true;
                //向其他玩家广播我已经准备好了
                room.BroadcastMessage(client, ActionCode.UpdateReady, client.GetUserId().ToString()+ "@1");
                //如果所有人都准备好
                if (room.GetReadyNum() == room.Type)
                {
                    room.State = RoomState.Battle;
                    room.BroadcastMessage(null, ActionCode.StartGame,"ready");
                    room.StartTimer();//房间开始计时，3秒后开始游戏
                }
                return ((int)ReturnCode.Success).ToString();
            }
            else
            {
                return ((int)ReturnCode.Fail).ToString();
            }

        }
        //某个客户端选择了角色，将其id和角色类型传递给服务器，服务器在房间内广播
        public string ChooseRole(string data, Client client, Server server)
        {
            // data = id,roleType
            string[] info = data.Split(',');
            int roleType = int.Parse(info[1]);
            client.Role.ChangeRole((RoleType)roleType);
            Room room = client.Room;
            if (room != null)
            {
                room.BroadcastMessage(null, ActionCode.ChooseRole, data);
            }
            return null;
        }
        /*配套客户端的OnResponse*/
        //data = id
        public string CancelReady(string data, Client client, Server server)
        {

            Room room = client.Room;
           if (client.IsReady)
            {
                client.IsReady = false;
                
           }

            room.BroadcastMessage(client, ActionCode.UpdateReady, client.GetUserId().ToString()+ "@0");
            return ((int)ReturnCode.Success).ToString();
        }

        public string QuitRoom(string data, Client client, Server server)
        {
            //bool isHouseOwner = client.IsHouseOwner();
            Room room = client.Room;
            room.RemoveClient(client);
            //client.InitData();//退出房间后重新初始化状态
            //如果房间里没人了就关闭房间
            if(room == null)
                return ((int)ReturnCode.Success).ToString();

            //如果房间里还有人的话就给剩下的人发送房间数据 让他们更新一下
            if (room.ClientRoom.Count != 0)
                room.BroadcastMessage(client, ActionCode.UpdateRoom, room.GetRoomData());

            return ((int)ReturnCode.Success).ToString();

        }
    }
}
