using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VinEcom.Invoices.Web.Models;

namespace VinEcom.Invoices.Web.Controllers
{
    public class ReconcileController : Controller
    {
        //
        // GET: /Reconcile/

        #region cookie

        public HttpCookie HttpCookie { get; set; }

        public void SetCookie(string key, string value)
        {
            if (HttpCookie != null)
            {
                var temp = HttpCookie[key];
                HttpCookie[key] = temp + value;
                HttpCookie.Expires = DateTime.Now.AddDays(1d);
            }
            else
            {
                HttpCookie = new HttpCookie("TempCookies");
                HttpCookie[key] = value;
                HttpCookie.Expires = DateTime.Now.AddDays(1d);
            }
        }

        public string GetCookie(string key)
        {
            string value = null;
            if (Response.Cookies["TempCookies"] != null)
            {
                HttpCookie = Request.Cookies["TempCookies"];
                if (HttpCookie != null && HttpCookie[key] != null)
                {
                    value = HttpCookie[key];
                }
            }
            return value;
        }

        #endregion

        public List<string> BuidArrayFromString(string values)
        {
            var arr = values.Split(',');
            return arr.Distinct().ToList();
        }

        public List<int> BuidArrayIntFromString(string values)
        {
            var arrs = values.Split(',');
            var listInts = new List<int>();
            foreach (var arr in arrs)
            {
                int a;
                if (int.TryParse(arr, out a))
                {
                    listInts.Add(a);
                }
            }

            return listInts.Distinct().ToList();
        }

        public void CheckedList(string listArray, string doact)
        {
            //var array = listArray.Split(',');
            if (!string.IsNullOrEmpty(doact))
            {
                if (doact.Equals("remove") && Session["list"] != null)
                {
                    //check and add
                    CheckExistAndRemove(listArray);
                }
                if (doact.Equals("add"))
                {
                    if (Session["list"] == null)
                    {
                        Session["list"] = listArray;
                    }
                    else
                    {
                        //check and add
                        CheckExistAndAdd(listArray);
                    }
                }
                var o = Session["list"];
                if (o != null) Response.Write(o.ToString());
            }
        }

        public void CheckExistAndAdd(string listArray)
        {
            if (Session != null)
            {
                var temps = BuidArrayFromString(Session["list"].ToString());
                foreach (var itemInput in listArray.Split(',').Distinct().ToList())
                {
                    if (!temps.Contains(itemInput))
                    {
                        temps.Add(itemInput);
                    }
                }
                Session["list"] = ArrayToString(temps);
            }
        }

        public void CheckExistAndRemove(string listArray)
        {
            if (Session != null)
            {
                var temps = BuidArrayFromString(Session["list"].ToString());
                foreach (var itemInput in listArray.Split(',').Distinct().ToList())
                {
                    if (temps.Contains(itemInput))
                    {
                        temps.Remove(itemInput);
                    }
                }
                Session["list"] = ArrayToString(temps);
            }
        }

        //convert array to string
        public string ArrayToString(List<string> array)
        {
            string str = "";
            foreach (var temp in array)
            {
                str += temp + ",";
            }
            str.Replace("on", string.Empty);
            str.Replace(",,", string.Empty);
            str.Replace(",,,", string.Empty);
            return str;
        }

        public ActionResult ExportInvoice()
        {
            if (Session["list"] != null)
            {
                var strListReconcileKey = BuidArrayIntFromString(Session["list"].ToString());
                Session["list"] = null; //export xong delete session
                //ReconcileDAL.ExportToInvoice(strListReconcileKey);
            }
            return RedirectToAction("Index");
        }

        [ChildActionOnly]
        public ActionResult InvoiceProd(int? reconcileKey)
        {
            using (var db = new ReconcileDBEntities1())
            {
                var model = db.InvoiceProductions.Where(o => o.ReconcileKey == reconcileKey).ToList();
                return PartialView(model);
            }
        }

        /// <summary>
        /// hien thi danh sach va filter
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="endDate"></param>
        /// <param name="keyword"></param>
        /// <param name="columnsearch"></param>
        /// <param name="pageIndex"></param>
        /// <param name="orderNos"></param>
        /// <param name="merchantIds"></param>
        /// <param name="customerIds"></param>
        /// <param name="reconcileFilter"></param>
        /// <returns></returns>
        public ActionResult Index(ReconcileFilter reconcileFilter)
        {
            const int pageSize = 10;

            #region validate param

            DateTime date1;
            if (!DateTime.TryParseExact(reconcileFilter.fromDate, "dd/MM/yyyy", null, DateTimeStyles.None, out date1))
            {
                //bao loi starDate
            }
            DateTime date2;
            if (!DateTime.TryParseExact(reconcileFilter.toDate, "dd/MM/yyyy", null, DateTimeStyles.None, out date2))
            {
                //bao loi toDate
            }
            if (date2 < date1)
            {
                //bao loi 
            }
            #endregion
            //sau khi convert thanh cong thi set value
            reconcileFilter.fromDateSql = date1;
            reconcileFilter.toDateSql = date2;
            //save de load lai len form html cac gia tri tim kiem
            ViewBag.ReconcileFilter = reconcileFilter;

            var model = ReconcileDAL.GetReconciles(reconcileFilter);
            var pageListCount = Convert.ToInt32(Math.Ceiling((decimal)ReconcileDAL.PageCount / pageSize));
            ViewBag.PageListCount = pageListCount;
            ViewBag.CurrentPage = 1;
            ViewBag.DebugQuery = ReconcileDAL.Sqlquery;
            return View(model);
        }

        //public static List<Reconcile> DataSample(int recordSize = 50)
        //{
        //    var dataList = new List<Reconcile>();
        //    var rdRandom = new Random();

        //    for (int i = 1; i <= recordSize; i++)
        //    {
        //        //tao object sample
        //        var reconcile = new Reconcile();
        //        reconcile.OrderDate = DateTime.Now.ToString("d");
        //        reconcile.PaidDate = DateTime.Now.AddHours(rdRandom.Next(1, 100)).ToString("d");
        //        reconcile.OrderNo = i.ToString();
        //        reconcile.VinID = "VIN" + rdRandom.Next(1, 10000).ToString();
        //        string[] names =
        //        {
        //            "Nguyen Van A"
        //            , "Nguyen Van B"
        //            , "Nguyen Van C"
        //            , "Nguyen Van D"
        //            , "Nguyen Van E"
        //            , "Nguyen Van F"
        //            , "Nguyen Van G"
        //            , "Nguyen Van H"
        //        };
        //        string[] units =
        //        {
        //            "Chiếc"
        //            , "Cái"
        //            , "Hộp"
        //            , "Thùng"
        //            , "Khác"
        //        };
        //        string[] products =
        //        {
        //            "Phone"
        //            , "Laptop"
        //            , "Máy tính"
        //            , "Case"
        //            , "Printer"
        //        };
        //        reconcile.ProdName = products[rdRandom.Next(0, products.Length - 1)];
        //        reconcile.CusName = names[rdRandom.Next(0, names.Length - 1)];
        //        reconcile.Amount = rdRandom.Next(100000, 5000000);
        //        reconcile.VATAmount = rdRandom.Next(10000, 500000);
        //        reconcile.CusAddress = "72 Nguyen Trai - HN";
        //        reconcile.MerchantName = "VINECOM";
        //        reconcile.ProdPrice = rdRandom.Next(100000, 1000000);
        //        reconcile.DiscountAmount = rdRandom.Next(100000, 1000000);
        //        reconcile.VATRate = rdRandom.Next(0, 30).ToString();
        //        reconcile.Quantity = rdRandom.Next(1, 30).ToString();
        //        reconcile.Unit = units[rdRandom.Next(0, units.Length - 1)];
        //        dataList.Add(reconcile);
        //    }
        //    return dataList;
        //}

        //[HttpGet]
        //public ActionResult GerneralData(int start = 0, int length = 10)
        //{
        //    List<Reconcile> dataList = new List<Reconcile>();
        //    int total = 0;
        //    Random rdRandom = new Random();
        //    for (int i = 1; i <= 100; i++)
        //    {
        //        //tao object sample
        //        var reconcile = new Reconcile();
        //        reconcile.OrderDate = DateTime.Now.ToString("d");
        //        reconcile.PaidDate = DateTime.Now.AddHours(rdRandom.Next(1, 100)).ToString("d");
        //        reconcile.OrderNo = new Random().Next(1, 10000).ToString();
        //        reconcile.VinID = "VIN" + new Random().Next(1, 10000).ToString();
        //        string[] names =
        //        {
        //            "Nguyen Van A"
        //            , "Nguyen Van B"
        //            , "Nguyen Van C"
        //            , "Nguyen Van D"
        //            , "Nguyen Van E"
        //            , "Nguyen Van F"
        //            , "Nguyen Van G"
        //            , "Nguyen Van H"
        //        };
        //        string[] units =
        //        {
        //            "Chiếc"
        //            , "Cái"
        //            , "Hộp"
        //            , "Thùng"
        //            , "Khác"
        //        };
        //        reconcile.ProdName = names[rdRandom.Next(0, names.Length - 1)];
        //        reconcile.Amount = rdRandom.Next(100000, 5000000);
        //        reconcile.VATAmount = rdRandom.Next(10000, 500000);
        //        reconcile.CusAddress = "72 Nguyen Trai - HN";
        //        reconcile.MerchantName = "VINECOM";
        //        reconcile.ProdPrice = rdRandom.Next(100000, 1000000);
        //        reconcile.DiscountAmount = rdRandom.Next(100000, 1000000);
        //        reconcile.VATRate = rdRandom.Next(0, 30).ToString();
        //        reconcile.Quantity = rdRandom.Next(1, 30).ToString();
        //        reconcile.Unit = units[rdRandom.Next(0, units.Length - 1)];
        //        dataList.Add(reconcile);
        //        total = i;
        //    }
        //    //tao objec json
        //    var jsonObject = new JsonObject();
        //    jsonObject.draw = 1; //thoi gian doi tra du lieu
        //    jsonObject.recordsFiltered = (total - start);
        //    jsonObject.recordsTotal = total;
        //    jsonObject.data = dataList.Skip(start)
        //        .Take(length)
        //        .ToList();
        //    return Json(jsonObject, JsonRequestBehavior.AllowGet);
        //}


        //
        // GET: /Reconcile/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Reconcile/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Reconcile/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Reconcile/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Reconcile/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Reconcile/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Reconcile/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}