using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using GameServer.Servers;
using GameServer.DAO;
using GameServer.Model;

namespace GameServer.Controller
{
    class UserController:BaseController
    {
        private UserDAO userDAO = new UserDAO();
        private ResultDAO resultDAO = new ResultDAO();
        public UserController()
        {
            requestCode = RequestCode.User;
        }

        //登录不仅有验证，还有 如果成功会把自己的账户信息上传至服务器client类
        //*******此处修改，增加了返回客户端id，便于绑定
        public string Login(string data, Client client, Server server)
        {
            string[] strs = data.Split(',');
            User user =  userDAO.VerifyUser(client.MySQLConn, strs[0], strs[1]);
            if (user == null)
            {
                //Enum.GetName(typeof(ReturnCode), ReturnCode.Fail);
                return ((int)ReturnCode.Fail).ToString();
            }
            else
            {
                Result res = resultDAO.GetResultByUserid(client.MySQLConn, user.Id);
                client.SetUserData(user, res);
                //return  string.Format("{0},{1},{2},{3}", ((int)ReturnCode.Success).ToString(), user.Username, res.TotalCount, res.WinCount);
                return string.Format("{0},{1},{2},{3},{4}", ((int)ReturnCode.Success).ToString(),user.Id, user.Username, res.TotalCount, res.WinCount);
            }
        }
        public string Register(string data, Client client, Server server)
        {
            string[] strs = data.Split(',');
            string username = strs[0];
            string password = strs[1];
            bool res = userDAO.GetUserByUsername(client.MySQLConn,username);
            if (res)
            {
                return ((int)ReturnCode.Fail).ToString();
            }
            userDAO.AddUser(client.MySQLConn, username, password);
            return ((int)ReturnCode.Success).ToString();
        }
    }
}
