using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Timers;
using GameServer.Controller;
using Common;
using GameServer.Tool;
namespace GameServer.Servers
{
    class Server
    {
        private IPEndPoint ipEndPoint;
        private Socket serverSocket;
        private List<Client> clientList = new List<Client>();
        private List<Room> roomList = new List<Room>();
        private ControllerManager controllerManager;

        private Timer timer = new Timer(10000);
        public  const long heartbeatTime = 5000;

        public Server() {}
        public Server(string ipStr,int port)
        {
            controllerManager = new ControllerManager(this);
            SetIpAndPort(ipStr, port);
        }

        public void SetIpAndPort(string ipStr, int port)
        {
            ipEndPoint = new IPEndPoint(IPAddress.Parse(ipStr), port);
        }

        public void Start()
        {
            //开启心跳
            timer.AutoReset = false;
            timer.Enabled = true;
            timer.Elapsed += new ElapsedEventHandler(HandleMainTimer);

            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(ipEndPoint);
            serverSocket.Listen(0);
            serverSocket.BeginAccept(AcceptCallBack, null);
        }   

        private void HandleMainTimer(object sender, ElapsedEventArgs e)
        {
            //处理心跳机制
            HeartBeat();

            timer.Start();
        }

        public void HeartBeat()
        {
            Console.WriteLine("【主定时器执行】");
            long timeNow = Sys.GetTimeStamp();
            foreach (Client client in clientList)
            {
                if (client == null) continue;
                if (client.lastTickTime < timeNow - heartbeatTime)
                {
                    Console.WriteLine("心跳引起断开连接 id ：" + client.GetUserId().ToString());
                    lock (client)
                    {
                        client.Close();
                    }
                }
            }
        }

        private void AcceptCallBack(IAsyncResult ar  )
        {
            Socket clientSocket = serverSocket.EndAccept(ar);
            Client client = new Client(clientSocket,this);
            client.Start();
            clientList.Add(client);
            serverSocket.BeginAccept(AcceptCallBack, null);
        }

        public void RemoveClient(Client client)
        {
            lock (clientList)
            {
                clientList.Remove(client);
            }
        }

        public void SendResponse(Client client,ActionCode actionCode,string data)
        {
            client.Send(actionCode, data);
        }

        public void HandleRequest(RequestCode requestCode, ActionCode actionCode, string data, Client client)
        {
            controllerManager.HandleRequest(requestCode, actionCode, data, client);
        }

        public void CreateRoom(Client client,int type)
        {
            Room room = new Room(this,type);
            room.AddClient(client);
            roomList.Add(room);
        }
        
        public void RemoveRoom(Room room)
        {
            if (roomList != null && room != null)
            {
                roomList.Remove(room);
            }
        }

        public List<Room> GetRoomList()
        {
            return roomList;
        }

        public Room GetRoomById(int id)
        {
            foreach(Room room in roomList)
            {
                if (room.GetId() == id) return room;
            }
            return null;
        }
    }
}
