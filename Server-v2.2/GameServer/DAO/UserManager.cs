using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Criterion;
using GameServer.Model;
using GameServer.Tool;

namespace GameServer.DAO
{
    class UserManager
    {
        public User VerifyUser(string name, string passwd)
        {
            try
            {
                using (ISession session = NHibernateHelper.OpenSeesion())
                {
                    ICriteria criteria = session.CreateCriteria(typeof(User));//指定查询的表，即与User类映射的表
                    criteria.Add(Restrictions.Eq("Username", name));
                    criteria.Add(Restrictions.Eq("Password", passwd));
                    User user = criteria.UniqueResult<User>();
                    return user;
                }
            }
            catch (Exception e)
            {

                Console.WriteLine("验证用户名失败！" + e.Message);
            }
            return null;
            
        }

        public bool GetUserByUsername(string username)
        { 
            try
            {
                using (ISession session = NHibernateHelper.OpenSeesion())
                {
                    ICriteria criteria = session.CreateCriteria(typeof(User));//指定查询的表，即与User类映射的表
                    criteria.Add(Restrictions.Eq("Username", username));
                    User user = criteria.UniqueResult<User>();
                    if(user != null)
                        return true;
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("在GetUserByUsername的时候出现异常：" + e.Message);
            }
           
            return false;
        }

        public void AddUser(string username, string password)
        {
            User user = new User();
            user.Username = username;
            user.Password = password;
            try
            {
                using (ISession session = NHibernateHelper.OpenSeesion())
                {
                    using (ITransaction transaction = session.BeginTransaction())
                    {
                        session.Save(user);
                        transaction.Commit();
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("在AddUser的时候出现异常：" + e.Message);
            }
        }
       
        public int GetUserIdByName(string name)
        {
            try
            {
                using (ISession session = NHibernateHelper.OpenSeesion())
                {
                    ICriteria criteria = session.CreateCriteria(typeof(User));//指定查询的表，即与User类映射的表
                    criteria.Add(Restrictions.Eq("Username", name));
                    
                    User user = criteria.UniqueResult<User>();
                    return user.Id;
                }
            }
            catch (Exception e)
            {

                Console.WriteLine("获取用户Id失败！" + e.Message );
            }
            return -1;
        }
    }
}
