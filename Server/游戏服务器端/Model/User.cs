using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Model
{
    //如果使用nhibernate需要把他们设置成虚函数
    class User
    {
        public User()
        {

        }
        public User(int id,string username,string password)
        {
            this.Id = id;
            this.Username = username;
            this.Password = password;
        }
        public virtual int Id { get; set; }
        public virtual string Username { get; set; }
        public virtual string Password { get; set; }
    }
}
