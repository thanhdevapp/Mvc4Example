using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace VinEcom.Invoices.Web.Controllers
{
    public class ReportController : Controller
    {
        //
        // GET: /Report/
        public string BuidQuery(string sql, ref bool b)
        {
            if (!b)
            {
                sql += " where ";
                b = true;
            }
            else
            {
                sql += " and ";
            }
            return sql;
        }

        //public ActionResult Index(string fromDate, string toDate, string keyword, string columnfilter,
        //    string mathfilter, string orderbycolumn, string optionorder,
        //    int pageIndex = 1, List<string> orderNos = null, List<string> merchantIds = null,
        //    List<string> customerIds = null)
        //{
        //    const int pageSize = 10;

        //    #region validate param

        //    DateTime starDate;
        //    if (!DateTime.TryParseExact(fromDate, "dd/MM/yyyy", null, DateTimeStyles.None, out starDate))
        //    {
        //        //bao loi starDate
        //    }
        //    DateTime endDate;
        //    if (!DateTime.TryParseExact(toDate, "dd/MM/yyyy", null, DateTimeStyles.None, out endDate))
        //    {
        //        //bao loi toDate
        //    }
        //    if (endDate < starDate)
        //    {
        //        //bao loi 
        //    }

        //    #endregion

        //    var model = InvoiceDAL.GetReports(starDate, endDate, keyword, columnfilter, mathfilter, orderbycolumn,
        //        optionorder, pageIndex, orderNos, merchantIds, customerIds);
        //    var pageListCount = Convert.ToInt32(Math.Ceiling((decimal) ReconcileDAL.PageCount/pageSize));
        //    ViewBag.PageListCount = pageListCount;
        //    ViewBag.CurrentPage = 1;
        //    return View(model);
        //}
    }
}