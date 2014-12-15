using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VinEcom.Invoices.Web.Models
{
    public class ArraysHelper
    {
        public static List<int> BuidArrayIntFromString(string values)
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
        public static List<string> BuidArrayStringFromString(string values)
        {
            var arrs = values.Split(',');
            var listInts = arrs.ToList();

            return listInts.Distinct().ToList();
        }
        public static string FormatStringArray(string values, bool isText = false)
        {
            var arrs = values.Split(',');
            string arr = "";
            foreach (var s in arrs)
            {
                if (!string.IsNullOrWhiteSpace(s))
                {
                    if (isText)
                    {
                        arr += "'" + s + "',";
                    }
                    else
                    {
                        arr += s + ",";
                    }
                }
            }
            //luon luon xoa dau phay o cuoi cung
            if (arr.LastIndexOf(',') == arr.Length - 1)
            {
                arr = arr.Substring(0, arr.Length - 1);
            }

            return arr;
        }
    }
}