using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;
using MTOLVN.Areas.Site.Models;
using MTOLVN.Models;
using Newtonsoft.Json;

namespace MTOLVN.Areas.Site.Controllers
{

    [Authorize]
    [OutputCache(Duration = 3600, Location = OutputCacheLocation.Client, NoStore = true)]
    public class AccountController : Controller
    {
        [HttpGet, AllowAnonymous]
        public ActionResult ValidOAuthredirectUrIs(FormCollection model)
        {
            return null;
        }

        [HttpPost]
        public ActionResult LoginAuthor(FormCollection model)
        {
            return null;
        }

        /// <summary>
        /// Xu lon nap tien vao tai khoan
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="time"></param>
        /// <param name="status"></param>
        /// <param name="ticket"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult DepositProcess(string transactionId, string time, string status, string ticket)
        {
            //khong xu? ly don hang da mua thanh cong
            #region checkprocessdone
            //neu don hang duoc xu ly roi thi
            var orderCheck = OrderManager.GetOrderByTransactionId(transactionId);
            // 4 la status buy hang thanh cong
            if (orderCheck.Status == 4)
            {
                UserManager.SendEmailDepositSuccess(orderCheck.OrderId);
                var model = OrderManager.GetOrderByTransactionId(transactionId);
                return View(model);
            }
            #endregion
            var ipAddress = UserManager.GetIpAddress();
            if (ipAddress == "::1")
            {
                ipAddress = "127.0.0.1";
            }

            // thuc hien query order
            var pay123 = new Pay123();
            string result = pay123.QueryOrderAndVerify(transactionId, ipAddress, time, status, ticket);
            var lists = JsonConvert.DeserializeObjectAsync<List<string>>(result).Result;

            if (lists[0].Equals("1"))
            {
                #region paramters
                var gateTransactionId = lists[1];
                var transactionStatus = lists[2];
                decimal moneyOfOrder;
                decimal moneyPaided;

                Decimal.TryParse(lists[3], out moneyOfOrder);
                Decimal.TryParse(lists[4], out moneyPaided);
                #endregion

                var order = OrderManager.GetOrderByTransactionId(transactionId);
                if (transactionStatus == "1")
                {
                    //co order
                    //status !=4 (4 la status nap tien thanh cong)
                    //insert duoc bang gatetransaction                        
                    OrderManager.Log("DepositProcess | transactionStatus == 1 | " + "/transactionId:" + transactionId + "/time:" + time + "/status:" + status + "/ticket:" + ticket);

                    if (order != null && order.Status != 4 && OrderManager.InsertGateTransaction(order.OrderId, gateTransactionId, moneyPaided) > 0)
                    {
                        OrderManager.Log("DepositProcess | transactionStatus == 1 OrderManager.InsertGateTransaction(order.OrderId, gateTransactionId, moneyPaided) > 0 /order.Status=" + order.Status + "/gateTransactionId=" + gateTransactionId + "/moneyPaided=" + moneyPaided + "/transactionId:" + transactionId + "/time:" + time + "/status:" + status + "/ticket:" + ticket);
                        //tien hang update status
                        //complete order
                        OrderManager.NextStepOrder(order.OrderId, (int)ActionOrder.ReviewOrder); // update status len 1
                        const int moneyAccept = 1500000;
                        if (moneyPaided <= moneyAccept)
                        {
                            if (OrderManager.NextStepOrder(order.OrderId, (int)ActionOrder.ProcessDepositCompleted) > 0)
                            {
                                OrderManager.Log("DepositProcess | transactionStatus == 1 (int)ActionOrder.ProcessDepositCompleted) > 0 /order.Status=" + order.Status + "/gateTransactionId=" + gateTransactionId + "/moneyPaided=" + moneyPaided + "/transactionId:" + transactionId + "/time:" + time + "/status:" + status + "/ticket:" + ticket);
                                UserManager.SendEmailDepositSuccess(order.OrderId);
                            }
                        }
                        else
                        {
                            ViewBag.Msg = "Hiện tại hệ thống chỉ tự động +tiền cho các giao dịch dưới " + String.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:C0}", moneyAccept) + " . Quý khách giao dịch lớn hơn vui lòng chờ xử lý";
                        }
                    }

                }
                else
                {
                    //khong duoc update status thanh cong
                    if (transactionStatus != "2" && transactionStatus != "3" && transactionStatus != "4")
                    {
                        //update status fail to order 
                        OrderManager.UpdateStatusOrder(order.OrderId, int.Parse(transactionStatus));
                    }
                }

                var model = OrderManager.GetOrderByTransactionId(transactionId);
                return View(model);
            }
            //neu chinh link thi return
            return new HttpStatusCodeResult(HttpStatusCode.NoContent);
        }


        [HttpGet]
        public ActionResult Deposit()
        {
            LoadViewBag();
            return View();
        }

        public void LoadViewBag()
        {
            if (Request.IsAuthenticated)
            {
                var user = UserManager.FindUserById(User.Identity.Name);
                if (user != null)
                {
                    ViewBag.Email = user.Email;
                    ViewBag.Phone = user.PhoneNumber;
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deposit(DepositWithBank depositWithBank)
        {
            if (ModelState.IsValid)
            {

                try
                {
                    string amount = depositWithBank.Amount.ToString().Replace(".", string.Empty);
                    decimal moneyDecimal;
                    if (!decimal.TryParse(amount, out  moneyDecimal))
                    {
                        throw new Exception();
                    }

                    var cusphone = "";
                    if (!string.IsNullOrEmpty(depositWithBank.PhoneNumber))
                        cusphone = depositWithBank.PhoneNumber;

                    var merchantcode = ConfigurationManager.AppSettings["MERCHANTCODE"];
                    var bankcode = depositWithBank.BankCode;
                    var passcode = ConfigurationManager.AppSettings["PASSCODE"];
                    var securecode = ConfigurationManager.AppSettings["SECURECODE"];

                    //const string domain = "http://localhost:2571";
                    //const string domain = "http://muatheonline.vn";

                    if (Request.Url != null)
                    {
                        #region buidquery
                        var domain = "http://" + Request.Url.Authority;
                        const string custName = "";
                        const string custAddress = "";
                        const string custGender = "U";
                        const string custDob = "";
                        var ipAddress = Request.UserHostAddress; //get ip
                        if (ipAddress == "::1")
                        {
                            ipAddress = "127.0.0.1";
                        }

                        var model = new CreateOrder()
                        {
                            mTransactionID = depositWithBank.TransactionId,
                            merchantCode = merchantcode,
                            bankCode = bankcode,
                            totalAmount = amount,
                            clientIP = ipAddress,
                            custName = custName,
                            custAddress = custAddress,
                            custGender = custGender,
                            custDOB = custDob,
                            custPhone = cusphone,
                            custMail = depositWithBank.Email,
                            description = "MUATHEONLINE.VN",
                            cancelURL = domain + "/tai-khoan/naptien", //huy bo giao dich
                            redirectURL = domain + "/tai-khoan/trangthai-naptien", //giao dich thanh cong
                            errorURL = domain + "/tai-khoan/trangthai-naptien",
                            passcode = passcode
                        };
                        //BUID DU LIEU DE POST LEN
                        var tempkey = model.mTransactionID
                                      + model.merchantCode
                                      + model.bankCode
                                      + model.totalAmount
                                      + model.clientIP
                                      + model.custName
                                      + model.custAddress
                                      + model.custGender
                                      + model.custDOB
                                      + model.custPhone
                                      + model.custMail
                                      + model.cancelURL
                                      + model.redirectURL
                                      + model.errorURL
                                      + model.passcode
                                      + securecode;
                        model.checksum = OrderManager.EncodeSha1(tempkey);

                        //model to dictionary contant key and values
                        Dictionary<string, object> dictionary = model.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(x => x.Name, x => x.GetValue(model, null));
                        #endregion
                        var pay123 = new Pay123();
                        var result = pay123.CreateOrder(dictionary);
                        var lists = JsonConvert.DeserializeObjectAsync<List<string>>(result).Result; //string json to list
                        var code = lists[0];
                        if (code == "5000")
                        {
                            ModelState.AddModelError("", "Xin lỗi quý khách. Hệ thống bận");
                            return View(depositWithBank);
                        }
                        if (pay123.CreateOrderVerifySumRedirectToPay("1", lists[1], lists[2], lists[3]))
                        {

                            //2. luu transactionID vao database
                            var gateTransactionId = lists[1];
                            string description = "NAP TAI KHOAN " + depositWithBank.Amount.ToString().ToUpper();

                            #region createorder
                            using (var db = new DefaultConnection())
                            {
                                //check xem co don hang nao vua tao khong
                                var orderExist = db.Orders.FirstOrDefault(o => o.Email == depositWithBank.Email && o.Status == 0 && o.TotalMoneyPaided == 0);
                                if (orderExist != null)
                                {
                                    orderExist.UserId = User.Identity.Name;
                                    orderExist.Description = description;
                                    orderExist.PhoneNumber = depositWithBank.PhoneNumber;
                                    orderExist.TransactionId = depositWithBank.TransactionId;
                                    orderExist.GateTransactionId = gateTransactionId;
                                    orderExist.Date = DateTime.Now;
                                    orderExist.IpOrder = UserManager.GetIpAddress();
                                    db.SaveChanges();
                                }
                                else
                                {
                                    var orderCreate = new Order
                                    {
                                        TransactionId = depositWithBank.TransactionId,
                                        Description = description,
                                        Date = DateTime.Now,
                                        UserId = User.Identity.Name,
                                        TotalMoneyPaided = 0, //quan trong neu trigger tinh tien se bi loi
                                        Email = depositWithBank.Email,
                                        PhoneNumber = depositWithBank.PhoneNumber,
                                        GateTransactionId = gateTransactionId,
                                        IpOrder = UserManager.GetIpAddress(),
                                        Status = (int)ActionOrder.CreateOrderTemp,
                                    };
                                    db.Orders.Add(orderCreate);
                                    db.SaveChanges();

                                }
                            }
                            #endregion
                            return Redirect(lists[2]);
                        }
                    }
                    throw new Exception("Invalid url");
                    //ModelState.AddModelError("", "Mua hàng thất bại, vui lòng liên hệ Quản trị 0968.868.862");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Xảy ra lỗi " + ex.Message);
                }

            }

            //nap lai Viewbag
            LoadViewBag();
            return View(depositWithBank);
        }


        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToAction("Profile", "Account");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                //model.Password = System.Web.Security
                var userId = UserManager.Find(model.Username, model.Password);
                if (!string.IsNullOrEmpty(userId))
                {
                    FormsAuthentication.SetAuthCookie(userId, model.RememberMe);
                    if (Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Sai tên đăng nhập hoặc mật khẩu.");
            }
            return View(model);
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        ////
        //// POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (!UserManager.CheckExistEmail(model.Email))
                {
                    AddErrors("Địa chỉ email này đã được đăng ký.");
                }
                else if (!UserManager.CheckExistPhone(model.PhoneNumber))
                {
                    AddErrors("Số điện thoại này đã được đăng ký.");
                }
                else
                {
                    var insertId = UserManager.InsertUser(model);
                    if (insertId != null)
                    {
                        FormsAuthentication.SetAuthCookie(insertId, false);
                        return RedirectToAction("Index", "Home");
                    }
                    AddErrors("Đăng ký tài khoản thất bại");
                }
            }
            return View(model);
        }
        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }


        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = UserManager.ForgotPassword(model.Email);
                if (user)
                {
                    ModelState.AddModelError("", "Mật khẩu đã được gửi tới email của bạn, vui lòng kiểm tra hộp thư hoặc thư spam.");
                    return View();
                }
                ModelState.AddModelError("", "Không tìm thấy tài khoản này.");
                return View();
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                string uid = User.Identity.Name;
                var result = UserManager.ChangePassword(uid, model.NewPassword, model.OldPassword);
                if (result)
                {
                    FormsAuthentication.SignOut();
                    ModelState.AddModelError("", "Mật khẩu của bạn đã thay đổi thành công");
                    return View();
                }
                ModelState.AddModelError("", "Mật khẩu cũ không đúng");
                return View();
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public new ActionResult Profile()
        {
            var user = UserManager.GetUser(User.Identity.Name);
            var model = new EditProfile()
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FirstName,
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender,
                Address = user.Address,
                BirthDate = user.BirthDate ?? DateTime.Now
            };
            return View(model);
        }

        [HttpGet]
        public ActionResult EditProfile(string id)
        {

            var user = UserManager.GetUser(User.Identity.Name);
            var modelEdit = new EditProfile()
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FirstName,
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender,
                Address = user.Address,
                BirthDate = user.BirthDate ?? DateTime.Now
            };
            return View(modelEdit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(EditProfile updateModel)
        {
            if (ModelState.IsValid)
            {
                using (var db = new DefaultConnection())
                {
                    var user = db.Users.Find(User.Identity.Name);

                    if (user.Email != updateModel.Email && !UserManager.CheckExistEmail(updateModel.Email))
                    {
                        AddErrors("Địa chỉ email này đã được đăng ký.");
                    }
                    else if (user.PhoneNumber != updateModel.PhoneNumber && !UserManager.CheckExistPhone(updateModel.PhoneNumber))
                    {
                        AddErrors("Số điện thoại này đã được đăng ký.");
                    }
                    else
                    {

                        user.Email = updateModel.Email;
                        user.PhoneNumber = updateModel.PhoneNumber;
                        user.Address = updateModel.Address;
                        user.FirstName = updateModel.FullName;
                        user.Gender = updateModel.Gender;
                        user.BirthDate = updateModel.BirthDate;
                        db.SaveChanges();
                        //var log = db.Database.Log;
                        //using (var w = new StreamWriter(Server.MapPath("~/App_Data/log.txt")))
                        //{
                        //    w.Write(log);
                        //}
                        return RedirectToAction("Profile");
                    }
                }

            }
            return View("EditProfile", updateModel);
        }

        public ActionResult OrderHistory()
        {
            using (var db = new DefaultConnection())
            {
                var id = User.Identity.Name;
                var orderById = db.Orders.Where(o => o.UserId == id).ToList();
                return View(orderById);
            }
        }
        // POST: /Account/LogOff
        [HttpGet]
        //[ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        private void AddErrors(String error)
        {
            ModelState.AddModelError("", error);
        }

    }

    public class UserManager
    {
        /// <summary>
        /// Login with username and password
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>userid</returns>
        public static string Find(string username, string password)
        {
            using (var db = new DefaultConnection())
            {
                password = GenerateMd5(password);
                var user = db.Users.FirstOrDefault(x => (x.Email.Equals(username) || x.PhoneNumber.Equals(username)) && x.PasswordHash == password);
                if (user != null)
                {
                    user.Username = username; //gan username khi login
                    user.LastAccessTime = DateTime.Now;
                    user.IpLocation = GetIpAddress();
                    db.SaveChanges();
                    return user.Id;
                }
            }
            return string.Empty;
        }
        /// <summary>
        /// login with user and password
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>User</returns>
        public static User Find2(string username, string password)
        {
            using (var db = new DefaultConnection())
            {
                password = GenerateMd5(password);
                var user = db.Users.FirstOrDefault(x => (x.Email.Equals(username) || x.PhoneNumber.Equals(username)) && x.PasswordHash == password);
                if (user != null)
                {
                    user.Username = username; //gan username khi login
                    user.LastAccessTime = DateTime.Now;
                    user.IpLocation = GetIpAddress();
                    db.SaveChanges();
                    return user;
                }
            }
            return null;
        }

        public static User FindUserById(string uid)
        {
            using (var db = new DefaultConnection())
            {
                var user = db.Users.FirstOrDefault(x => x.Id == uid);
                if (user != null)
                {
                    return user;
                }
            }
            return null;
        }

        /// <summary>
        /// tim lai mat khau cho nguoi dung
        /// tao mat khau moi va gui email
        /// </summary>
        /// <param name="username"></param>
        /// <param name="phonenumber"></param>
        /// <returns></returns>
        public static bool ForgotPassword(string username)
        {
            using (var db = new DefaultConnection())
            {
                var user = db.Users.FirstOrDefault(x => (x.Email.Equals(username) || x.PhoneNumber.Equals(username)));
                if (user != null)
                {
                    var ip = HttpContext.Current.Request.UserHostAddress;
                    var newPassword = CreatePassword(6);
                    //insert password moi
                    user.PasswordHash = GenerateMd5(newPassword);
                    if (db.SaveChanges() > 0)
                    {
                        SendEmailForgotPasswordNotify(user.Username, newPassword, username, ip);
                        return true;
                    }

                }

            }
            return false;
        }
        /// <summary>
        /// find user and changepassword
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="newpassword"></param>
        /// <param name="oldpassword"></param>
        /// <returns></returns>
        public static bool ChangePassword(string uid, string newpassword, string oldpassword)
        {
            using (var db = new DefaultConnection())
            {
                string oldhasspass = GenerateMd5(oldpassword);
                var user = db.Users.FirstOrDefault(x => x.Id == uid && x.PasswordHash == oldhasspass);
                if (user != null)
                {
                    var ip = HttpContext.Current.Request.UserHostAddress;
                    //insert password moi
                    user.PasswordHash = GenerateMd5(newpassword);
                    if (db.SaveChanges() > 0)
                    {
                        SendEmailChangePasswordSuccess(user.Email, user.Username, ip);
                        return true;
                    }

                }

            }
            return false;
        }

        public static bool CheckExistEmail(string email)
        {
            using (var db = new DefaultConnection())
            {
                var user = db.Users.FirstOrDefault(x => x.Email.Equals(email));
                if (user != null)
                {
                    return false;
                }
                return true;
            }
        }


        public static bool CheckExistPhone(string phone)
        {
            using (var db = new DefaultConnection())
            {
                var user = db.Users.FirstOrDefault(x => x.PhoneNumber.Equals(phone));
                if (user != null)
                {
                    return false;
                }
                return true;
            }
        }


        /// <summary>
        /// insert User
        /// </summary>
        /// <param name="model"></param>
        /// <returns>id insert</returns>
        public static string InsertUser(RegisterViewModel model)
        {
            using (var db = new DefaultConnection())
            {
                var userInsert = new User()
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = model.Email,
                    EmailConfirmed = false,
                    Username = model.Email,
                    Gender = model.Gender,
                    FirstName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    PhoneNumberConfirmed = false,
                    PasswordHash = GenerateMd5(model.Password),
                    AccountBalance = 0,
                    AccessFailedCount = 0,
                    IpLocation = GetIpAddress(),
                    CreateDate = DateTime.Now
                };
                db.Users.Add(userInsert);
                if (db.SaveChanges() > 0)
                    return userInsert.Id;
                return null;
            }
        }
        /// <summary>
        /// return email login
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static User GetUser(string id)
        {
            using (var db = new DefaultConnection())
            {
                var userSelect = db.Users.FirstOrDefault(u => u.Id == id);
                if (userSelect != null)
                {
                    return userSelect;
                }
                return null;
            }
        }

        public static string GenerateMd5(string yourString)
        {
            return string.Join("", MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(yourString)).Select(s => s.ToString("x2")));
        }
        /// <summary>
        /// Lay user hien tai dang login
        /// </summary>
        /// <returns>userSelect.Email</returns>
        public string GetCurrentUser()
        {
            if (HttpContext.Current.Request.IsAuthenticated)
            {
                var uId = HttpContext.Current.User.Identity.Name; //get id
                using (var db = new DefaultConnection())
                {
                    var userSelect = db.Users.FirstOrDefault(u => u.Id == uId);
                    if (userSelect != null)
                    {
                        return userSelect.Email;
                    }
                }
            }
            return "";
        }

        public static string CreatePassword(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            var res = new StringBuilder();
            var rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }
        public static void SendEmailForgotPasswordNotify(string toEmail, string newPassword, string user, string ip)
        {
            var pathHtml = HttpContext.Current.Server.MapPath("/FormatEmail/bodyEmailForgotPasswordNotify.html");
            var body = System.IO.File.ReadAllText(pathHtml);
            body = String.Format(body, user, newPassword, ip);
            new MailModel().SentMail(new MailModel() { Body = body, From = "Muatheonline.vn <sales@muatheonline.vn>", Subject = "Thông tin tìm lại mật khẩu mới tại muatheonline.vn", To = toEmail });
        }


        public static void SendEmailChangePasswordSuccess(string toEmail, string user, string ip)
        {
            var pathHtml = HttpContext.Current.Server.MapPath("/FormatEmail/bodyEmailChangePassSuccessNotify.html");
            var body = System.IO.File.ReadAllText(pathHtml);
            body = String.Format(body, user, DateTime.Now.ToString("hh:mm dd-MM-yyyy"), ip);
            new MailModel().SentMail(new MailModel() { Body = body, From = "Muatheonline.vn <sales@muatheonline.vn>", Subject = "Thông tin tìm lại mật khẩu mới tại muatheonline.vn", To = toEmail });
        }

        public static void SendEmailDepositSuccess(int orderId)
        {
            using (var db = new DefaultConnection())
            {
                var order = db.Orders.FirstOrDefault(o => o.OrderId == orderId);
                if (order != null && order.StatusSendMail != 1 && order.Status == 4)
                {
                    var pathHtml = HttpContext.Current.Server.MapPath("/FormatEmail/bodyEmailDepositNotify.html");
                    var body = System.IO.File.ReadAllText(pathHtml);
                    var user = FindUserById(order.UserId);
                    var username = "Guest";
                    if (user != null)
                    {
                        username = user.Username;
                    }
                    body = String.Format(body, String.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:C0}", order.TotalMoneyPaided), order.TransactionId, order.Date.ToString("hh:mm dd-MM-yyyy"), username, order.IpOrder);
                    new MailModel().SentMailAndReportAdmin(new MailModel() { Body = body, From = "Muatheonline.vn <sales@muatheonline.vn>", Subject = "Thông nạp tài khoản muatheonline.vn", To = order.Email });
                    order.StatusSendMail = 1;
                    db.SaveChanges();
                }

            }
        }

        public static void SendEmailDepositWaitingConfirm(int orderId)
        {
            using (var db = new DefaultConnection())
            {
                var order = db.Orders.FirstOrDefault(o => o.OrderId == orderId);
                if (order != null && order.StatusSendMail != 1 && order.Status == 4)
                {
                    var pathHtml = HttpContext.Current.Server.MapPath("/FormatEmail/bodyEmailDepositWaitingNotify.html");
                    var body = System.IO.File.ReadAllText(pathHtml);
                    var user = FindUserById(order.UserId);
                    var username = "Guest";
                    if (user != null)
                    {
                        username = user.Username;
                    }
                    body = String.Format(body, String.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:C0}", order.TotalMoneyPaided), order.TransactionId, order.Date.ToString("hh:mm dd-MM-yyyy"), username, order.IpOrder);
                    new MailModel().SentMailAndReportAdmin(new MailModel() { Body = body, From = "Muatheonline.vn <sales@muatheonline.vn>", Subject = "Đơn hàng nhạp tiền chờ duyệt", To = order.Email });
                    order.StatusSendMail = 1;
                    db.SaveChanges();
                }

            }
        }

        public static string GetIpAddress()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return context.Request.ServerVariables["REMOTE_ADDR"];
        }
    }

}