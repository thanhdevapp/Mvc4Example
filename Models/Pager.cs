using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace VinEcom.Invoices.Web.Models
{
    public class Pager
    {
        public RouteValueDictionary ValueDictionary { get; set; }
        public int CurrentPage { get; set; }
    }
}