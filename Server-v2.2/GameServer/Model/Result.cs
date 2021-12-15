using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Model
{
    class Result
    {
        //public Result()
        //{

        //}
        //public Result(int useId)
        //{
        //    this.UserId = UserId;
        //    //
        //    TotalCount = 0;
        //    WinCount = 0;
        //    Version = 1;
        //}
            
        //public Result(int id, int userId,int totalCount,int winCount)
        //{
        //    this.Id = id;
        //    this.UserId = userId;
        //    this.TotalCount = totalCount;
        //    this.WinCount = winCount;
        //}
        public virtual int Id { get; set; }
        public virtual int UserId { get; set; }
        public virtual int TotalCount { get; set; }
        public virtual int WinCount { get; set; }
        public virtual int Version { get; set; }
    }
}
