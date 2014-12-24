using System;
using System.Collections.Generic;

namespace MobileMemberShip.Areas.ServerApi.Models
{    
    public class ReturnData<T>
    {
        public bool success { get; set; }
        public string error { get; set; }
        public T data { get; set; }
    }

    public class ReturnPageData<T>
    {
        public ReturnPageData()
        {
            paging = new Paging();
        }
        public bool success { get; set; }
        public string error { get; set; }
        public Paging paging { get; set; }
        public T data { get; set; }
    }

    public class Paging
    {
        public int page { get; set; }

        public int pagesize { get; set; }

        public int total { get; set; }
    }
}