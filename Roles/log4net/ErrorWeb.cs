using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MobileMemberShip
{
    public class ErrorWeb : HandleErrorAttribute
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ErrorWeb));
        public override void OnException(System.Web.Mvc.ExceptionContext filterContext)
        {
            //If the exeption is already handled we do nothing
            if (filterContext.ExceptionHandled)
            {
                return;
            }
            else
            {
                try
                {
                    //Determine the return type of the action
                    var nameSpace = (filterContext.RouteData.DataTokens["Namespaces"] as string[]).ElementAt(0);
                    string actionName = filterContext.RouteData.Values["action"].ToString();
                    Type controllerType = filterContext.Controller.GetType();
                    var method = controllerType.GetMethod(actionName);
                    var returnType = method.ReturnType;

                    log.ErrorFormat("[{0}.{1}]: {2}", nameSpace, actionName, filterContext.Exception.Message);
                    log.ErrorFormat("[{0}.{1}]: {2}", nameSpace, actionName, filterContext.Exception.StackTrace);

                    //If the action that generated the exception returns JSON
                    if (returnType.Equals(typeof(JsonResult)))
                    {
                        var data = new { error = "Server Server Lỗi" };
                        filterContext.Result = new JsonResult()
                        {
                            Data = data
                        };
                    }

                    //If the action that generated the exception returns a view
                    //Thank you Sumesh for the comment
                    if (returnType.Equals(typeof(ActionResult))
                    || (returnType).IsSubclassOf(typeof(ActionResult)))
                    {
                        filterContext.Result = new ViewResult
                        {
                            ViewName = "Error"
                        };
                    }
                }
                catch (Exception)
                {
                    log.ErrorFormat("[System]: {0}", filterContext.Exception.Message);
                    log.ErrorFormat("[System]: {0}", filterContext.Exception.StackTrace);

                    //If the action that generated the exception returns a view
                    //Thank you Sumesh for the comment
                    filterContext.Result = new ViewResult
                    {
                        ViewName = "Error"
                    };
                }
            }

            //Make sure that we mark the exception as handled
            filterContext.ExceptionHandled = true;
        }
    }
}