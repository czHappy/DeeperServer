using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Timers;
using Common;
using MySql.Data.MySqlClient;
using GameServer.Tool;
using GameServer.Model;
using GameServer.DAO;
namespace GameServer.Servers
{
    class Client
    {
        private Socket clientSocket;
        private Server server;
        private Message msg = new Message();

        private Room room;
        private User user;
        private Result result;

        private ResultManager resultManager = new ResultManager();

        private Role role ;//角色
        private bool isReady;//是否准备好

        //心跳协议
        public long lastTickTime = long.MinValue;
        
        public Client() { }
        public Client(Socket clientSocket, Server server)
        {
            this.clientSocket = clientSocket;
            this.server = server;
           // mysqlConn = ConnHelper.Connect();
            // this.roleType = RoleType.Default;
            this.role = new Role(RoleType.Default);
            this.isReady = false;
            //心跳处理初始化
            lastTickTime = Sys.GetTimeStamp();

        }
        //增加一个获取user的方法
        public User User
        {
            get
            {
                return user;
            }
            set
            {
                user = value;
            }
        }
        public Room Room
        {
            set { room = value; }
            get { return room; }
        }

        public Role Role { get => role; set => role = value; }
        public bool IsReady { get => isReady; set => isReady = value; }

        public void InitData()
        {
            this.isReady = false;
            this.role.ChangeRole(RoleType.Default);
        }

        public bool TakeDamage(int damage)
        {
            role.Hp -= (damage - role.Defensive);
            role.Hp = Math.Max(role.Hp, 0);
            if (role.Hp <= 0) return true;
            return false;
        }
        public bool IsDie()
        {
            return role.Hp <= 0;
        }
       // public MySqlConnection MySQLConn
        //{
        //    get { return mysqlConn; }
        //}
        public void SetUserData(User user,Result result)
        {
            this.user = user;
            this.result = result;
        }
        public string GetUserData()
        {
            //忘记加roletype
            return user.Id+","+ user.Username + "," + result.TotalCount + "," + result.WinCount+ "-" + (int)role.RoleType + "-" + isReady.ToString();
        }
       

        public int GetUserId()
        {
            return user.Id;
        }


        public void Start()
        {
            if (clientSocket == null || clientSocket.Connected == false) return;
            clientSocket.BeginReceive(msg.Data, msg.StartIndex, msg.RemainSize, SocketFlags.None, ReceiveCallback, null);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                if (clientSocket == null || clientSocket.Connected == false) return;
                int count = clientSocket.EndReceive(ar);

                if (count == 0)
                {
                    Console.WriteLine("count = 0 连接断开！" + this.GetUserId().ToString());
                    
                    Close();
                }
                msg.ReadMessage(count,OnProcessMessage);
                Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Close();
            }
        }

        private void OnProcessMessage(RequestCode requestCode,ActionCode actionCode,string data)
        {
            
            server.HandleRequest(requestCode, actionCode, data, this);
        }

        public void Close()
        {
            //ConnHelper.CloseConnection(mysqlConn);
            
            //把他所在房间关闭
            if (room != null)
            {
                room.QuitRoom(this);

                if (room == null)
                {
                    Console.WriteLine("房间为空！");
                }
                Console.WriteLine(room.State);
                //************处理某一个客户端断线
                if (room != null && room.State == RoomState.Battle)
                {
                    Console.WriteLine("进入暴毙！");
                    try
                    {
                        this.UpdateResultToDB(false);
                    }
                    catch (Exception e)
                    {

                        Console.WriteLine("该客户端强退，更新其战绩时发生异常！" + e.Message);
                    }
                    
                   
                    if (room.ClientRoom.Count == 1)
                    {
                        try
                        {
                            room.ClientRoom[0].UpdateResult(true);
                        }
                        catch (Exception e)
                        {

                            Console.WriteLine("该客户端强退2，更新其战绩时发生异常！" + e.Message);
                        }
                        
                        room.ClientRoom[0].Send(ActionCode.GameOver, ((int)ReturnCode.Success).ToString());
                        room.Close();
                    }
                    else
                    {
                        room.BroadcastMessage(this, ActionCode.NotifyDeath, this.GetUserId().ToString());
                    }
                }
            }

            if (clientSocket != null)
                clientSocket.Close();

            server.RemoveClient(this);
        }

        public void Send(ActionCode actionCode, string data)
        {
            try
            {
                byte[] bytes = Message.PackData(actionCode, data);
                clientSocket.Send(bytes);
            }catch(Exception e)
            {
                Console.WriteLine("无法发送消息:" + e);
            }
        }


        public void UpdateResult(bool isVictory)
        {
            UpdateResultToDB(isVictory);
            UpdateResultToClient();
        }

        private void UpdateResultToDB(bool isVictory)
        {
            result.TotalCount++;
            if (isVictory)
            {
                result.WinCount++;
            }
            //resultDAO.UpdateOrAddResult(mysqlConn, result);
            resultManager.UpdateResult(result);
        }

        private void UpdateResultToClient()
        {
            Send(ActionCode.UpdateResult, string.Format("{0},{1}", result.TotalCount, result.WinCount));
        }

    }
}
