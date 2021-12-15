using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using GameServer.Servers;
namespace GameServer.Controller
{
    class GameController:BaseController
    {
        //移动 数据已经封装好应该包含移动对象的id + 移动数据  转发
        public GameController()
        {
            requestCode = RequestCode.Game;
        }
        //准备游戏按钮 
        public string Move(string data, Client client, Server server)
        {
            Room room = client.Room;
            if (room != null)
                room.BroadcastMessage(client, ActionCode.Move, data);
            return null;
        }
        //射击  数据已经封装好应该包含移动对象的id + 子弹移动数据 转发
        public string Shoot(string data, Client client, Server server)
        {
            Room room = client.Room;
            if (room != null)
                room.BroadcastMessage(client, ActionCode.Shoot, data);
            return null;
        }
        //攻击处理伤害 数据包含受伤对象的id + 伤害数据
        public string Attack(string data, Client client, Server server)
        {
            // cz,100
            string[] info = data.Split(',');
            int id = int.Parse(info[0]);
            int damage = int.Parse(info[1]);
            Room room = client.Room;
            if (room == null) return null;
            room.TakeDamage(id, damage);
            return null;
        }
        //血量通知  朴素的想法是每个客户端不断定时请求该房间内其他客户端血量数据 而且这个频率比较高 可能耗费大量资源
        public string GetHP(string data, Client client, Server server)
        {
            StringBuilder sb = new StringBuilder();
            //TODO
            
            return sb.ToString();

        } 


        //结束战斗  
        public string QuitBattle(string data, Client client, Server server)
        {
            Room room = client.Room;


            if (room != null)
            {
                

                room.BroadcastMessage(null, ActionCode.QuitBattle,data);
                room.ClientRoom.Remove(client);

                if (room.ClientRoom.Count == 1 && room.Type != 1)
                {
                    Console.WriteLine("他选择退出，现在只剩下一个人：id = " + room.ClientRoom[0].GetUserId());
                    room.ClientRoom[0].UpdateResult(true);
                    room.ClientRoom[0].Send(ActionCode.GameOver, ((int)ReturnCode.Success).ToString());
                    room.Close();//房间销毁
                }

                if (room.ClientRoom.Count == 0)
                    room.Close();
            }
            return null;
        }

        


    }
}
