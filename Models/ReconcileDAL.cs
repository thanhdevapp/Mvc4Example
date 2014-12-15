using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;

namespace VinEcom.Invoices.Web.Models
{
    public class ReconcileDAL
    {
        public static string BuidQuery(string sql, ref bool b)
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
        public static int PageCount { get; set; }
        public static string Sqlquery { get; set; }
        public static List<Reconcile> GetReconciles(ReconcileFilter reconcileFilter)
        {
            using (var db = new ReconcileDBEntities1())
            {
                bool isWhere = false;
                bool isFilter = false;
                int pageSize = 10;

                string sqlComand = @"SELECT * FROM Reconcile";
                if (!string.IsNullOrEmpty(reconcileFilter.columnfilter) && !string.IsNullOrEmpty(reconcileFilter.keyword) && !string.IsNullOrEmpty(reconcileFilter.mathfilter))
                {
                    var tempmath = reconcileFilter.mathfilter;
                    sqlComand = BuidQuery(sqlComand, ref isWhere);
                    switch (reconcileFilter.mathfilter)
                    {
                        case "contains":
                            reconcileFilter.mathfilter = " like N'%" + reconcileFilter.keyword + "%'";
                            break;
                        case "morethan":
                            reconcileFilter.mathfilter = " > " + reconcileFilter.keyword;
                            break;
                        case "lessthan":
                            reconcileFilter.mathfilter = " < " + reconcileFilter.keyword;
                            break;
                        case "morethanorequals":
                            reconcileFilter.mathfilter = " >= " + reconcileFilter.keyword;
                            break;
                        case "lessthanorequals":
                            reconcileFilter.mathfilter = " <= " + reconcileFilter.keyword;
                            break;
                        case "equals":
                            reconcileFilter.mathfilter = " = " + reconcileFilter.keyword;
                            break;
                        case "notequals":
                            reconcileFilter.mathfilter = " <> " + reconcileFilter.keyword;
                            break;

                    }
                    sqlComand += " " + reconcileFilter.columnfilter + reconcileFilter.mathfilter;
                    reconcileFilter.mathfilter = tempmath; //tra ve gia tri ban dau thi hien thi len html
                }
                //trong von 2 nam gan day
                if ((DateTime.Now.Year - reconcileFilter.fromDateSql.Year) <= 2)
                {
                    isFilter = true;
                    sqlComand = BuidQuery(sqlComand, ref isWhere);
                    sqlComand += " PaidDate >= '" + reconcileFilter.fromDateSql.ToString("yyyy-MM-dd") + "'";
                }
                if ((DateTime.Now.Year - reconcileFilter.toDateSql.Year) <= 2)
                {
                    isFilter = true;
                    sqlComand = BuidQuery(sqlComand, ref isWhere);
                    sqlComand += " PaidDate <= '" + reconcileFilter.toDateSql.ToString("yyyy-MM-dd") + "'";
                }

                //tim kiem theo list order
                if (!string.IsNullOrEmpty(reconcileFilter.orderNos))
                {
                    isFilter = true;
                    sqlComand = BuidQuery(sqlComand, ref isWhere);
                    sqlComand += " OrderNo IN (" + ArraysHelper.FormatStringArray(reconcileFilter.orderNos) + ")";
                }
                //tim kiem theo merchant
                if (!string.IsNullOrEmpty(reconcileFilter.merchantIds))
                {
                    isFilter = true;
                    sqlComand = BuidQuery(sqlComand, ref isWhere);
                    sqlComand += " MerchantID IN (" + ArraysHelper.FormatStringArray(reconcileFilter.merchantIds, true) + ")";
                }
                //tim kiem theo khach hang
                //if (!string.IsNullOrEmpty(reconcileFilter.customerIds))
                //{
                //    isFilter = true;
                //    sqlComand = BuidQuery(sqlComand, ref isWhere);
                //    sqlComand += " OrderNo IN (" + ArraysHelper.FormatStringArray(reconcileFilter.customerIds) + ")";
                //}

                //order by
                if (!string.IsNullOrEmpty(reconcileFilter.orderbycolumn))
                {
                    isFilter = true;
                    //optionorder asc and desc
                    sqlComand += " order by " + reconcileFilter.orderbycolumn + " " + reconcileFilter.optionorder;
                }
                int skipFrom = (reconcileFilter.pageIndex - 1) * pageSize;
                var model = db.Reconciles.SqlQuery(sqlComand).Skip(skipFrom)
                        .Take(pageSize)
                        .ToList();
                PageCount = isFilter ? model.Count : db.Reconciles.Count();

                //for debug
                Sqlquery = sqlComand;
                return model;
            }
        }
    }
}