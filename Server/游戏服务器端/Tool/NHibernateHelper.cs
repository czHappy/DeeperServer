using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Cfg;

namespace GameServer.Tool
{
    class NHibernateHelper
    {
        private static ISessionFactory _sessionFactory;//会话工厂
        public static ISessionFactory SessionFactory //_sessionFactory的get方法
        {
            get
            {
                if (_sessionFactory == null)
                {
                    try
                    {
                        var conf = new Configuration();
                        try
                        {
                            conf.Configure();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("配置错误" + e.Message + e.StackTrace + e.InnerException + e.Source + e.TargetSite);
                            throw e;
                        }
                       
                        //Console.WriteLine("配置完毕！");

                        try
                        {
                            conf.AddAssembly("Server");//解析User.hbm.xml....等 
                        }
                        catch (Exception e)
                        {

                            Console.WriteLine("程序集装入错误！" + e.Message);
                        }

                        //Console.WriteLine("程序集装入完毕！");

                        try
                        {
                            _sessionFactory = conf.BuildSessionFactory();
                        }
                        catch (Exception e)
                        {

                            Console.WriteLine("创建会话错误！" + e.Message);
                        }
                        

                        Console.WriteLine("called!");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("创建会话工厂失败！" + e.Message);
                    }
                }
                return _sessionFactory;
            }
        }

        //打开会话方法
        public static ISession OpenSeesion()
        {

            Console.WriteLine("准备打开会话！");
            return SessionFactory.OpenSession();
            //_sessionFactory不需要关闭，到程序结束才关闭，所以写不写关闭方法无所谓
        }

    }
}

