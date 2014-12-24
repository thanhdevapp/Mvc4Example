using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;
using log4net;
using MobileMemberShip.Areas.ServerApi.Models;
using Newtonsoft.Json;

namespace MobileMemberShip
{
    public class ErrorApi : ExceptionFilterAttribute {

        private static readonly ILog log = LogManager.GetLogger(typeof(ErrorApi));

        public override void OnException(HttpActionExecutedContext context)
        {

            log.ErrorFormat("[{0}]: {1}", context.Request.RequestUri, context.Exception.Message);
            log.ErrorFormat("[{0}]: {1}", context.Request.RequestUri, context.Exception.StackTrace);
            //base.OnException(context);
            var data = new ReturnData<string>();
            data.success = false;
            data.error = ((int)CoreConstant.ConstantStatus.MessagerApi.serverError).ToString();
            context.Response = new HttpResponseMessage(HttpStatusCode.OK);
            context.Response.Content = new StringContent(JsonConvert.SerializeObject(data),System.Text.Encoding.UTF8,"application/json");           
        }
    }
}