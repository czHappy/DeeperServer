using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
namespace GameServer.Model
{
    class Role
    {
        private RoleType _roleType;//角色类型
        private int _damage;//攻击力
        private int _defensive;//防御
        private int _hp;//血量
        private int _magic;//蓝量

        public RoleType RoleType { get => _roleType; set => _roleType = value; }
        public int Damage { get => _damage; set => _damage = value; }
        public int Defensive { get => _defensive; set => _defensive = value; }
        public int Hp { get => _hp; set => _hp = value; }
        public int Magic { get => _magic; set => _magic = value; }

        public Role(RoleType rt)
        {
            this.ChangeRole(rt);
        }
    
        public void ChangeRole(RoleType rt)
        {
            this.RoleType = rt;
            if (rt == RoleType.Default)
            {
                this.Damage = 15;
                this.Defensive = 3;
                this.Hp = 100;
                this.Magic = 100;
            }
            else if (rt == RoleType.Blue)
            {
                this.Damage = 10;
                this.Defensive = 5;
                this.Hp = 120;
                this.Magic = 80;
            }
            else if (rt == RoleType.Red)
            {
                this.Damage = 18;
                this.Defensive = 2;
                this.Hp = 90;
                this.Magic = 120;
            }
        }
    }
}
