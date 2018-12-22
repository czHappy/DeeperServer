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
    class ResultManager
    {

        public Result GetResultByUserid(int userId)
        {
            Result res = null;
            try
            {
                using (ISession session = NHibernateHelper.OpenSeesion())
                {
                    ICriteria criteria = session.CreateCriteria(typeof(Result));//指定查询的表，即与User类映射的表
                    criteria.Add(Restrictions.Eq("UserId", userId));
                    res = criteria.UniqueResult<Result>();
                    if (res == null) res = new Result(-1, userId, 0, 0);
                    return res;
                }
            }
            catch (Exception e)
            {

                Console.WriteLine("在GetResultByUserid的时候出现异常：" + e.Message);
            }
            return null;
        }

        public void AddResult(int userId)
        {
            Result result = new Result(userId);

            try
            {
                using (ISession session = NHibernateHelper.OpenSeesion())
                {
                    using (ITransaction transaction = session.BeginTransaction())
                    {
                        session.Save(result);
                        transaction.Commit();
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("在AddUser的时候出现异常：" + e.Message);
            }
        }

        public void UpdateResult(Result res)
        {
            try
            {
                using (ISession session = NHibernateHelper.OpenSeesion())
                {
                    using (ITransaction transaction = session.BeginTransaction())
                    {
                        session.Update(res);
                        transaction.Commit();
                    }
                }
            }
            catch (Exception e)
            {

                Console.WriteLine("更新战绩出现异常！" + e.Message);
            }
           

        }
    }
}
