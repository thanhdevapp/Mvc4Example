using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VinEcom.Invoices.Web.Models
{
    public class ReconcileFilter
    {
        public string fromDate { get; set; }
        public string toDate { get; set; }
        public DateTime fromDateSql { get; set; }
        public DateTime toDateSql { get; set; }
        public string keyword { get; set; }
        public string columnfilter { get; set; }
        public string mathfilter { get; set; }
        public string orderbycolumn { get; set; }
        public string optionorder { get; set; }
        public int pageIndex { get; set; }
        public string orderNos { get; set; }
        public string merchantIds { get; set; }
        public string customerIds { get; set; }

        public ReconcileFilter()
        {
            this.pageIndex = 1;
        }
    }
}