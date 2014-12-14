 public static string BuidArrayStringFromString(string values)
 {
            var arrs = values.Split(',');
            string arr = "";
            foreach (var s in arrs)
            {
                if (!string.IsNullOrWhiteSpace(s))
                {
                    arr += s + ",";
                }
            }
            //luon luon xoa dau phay o cuoi cung
            if (arr.LastIndexOf(',') == arr.Length - 1)
            {
                arr = arr.Substring(0, arr.Length - 1);
            }

            return arr;
}
