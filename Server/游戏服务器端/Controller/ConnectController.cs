using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Servers;
using Common;
using GameServer.Tool;
namespace GameServer.Controller
{
    class ConnectController : BaseController
    {
        public ConnectController()
        {
            requestCode = RequestCode.Connect;
        }

        
        public string SendHeartbeat(string data, Client client, Server server)
        {
            //收到心跳包后更新一下该客户端的上次心跳时间
            client.lastTickTime = Sys.GetTimeStamp();
            return null;
        }
    }
}
