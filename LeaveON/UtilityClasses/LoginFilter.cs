using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace LeaveON.UtilityClasses
{
  public class LoginFilter : ActionFilterAttribute
  {
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
      //string CurrentController = HttpContext.Current.Request.RequestContext.RouteData.Values["Controller"].ToString();
      //string CurrentAction = HttpContext.Current.Request.RequestContext.RouteData.Values["Action"].ToString();

      
      //if (HttpContext.Current.Session["CurrentUser"] == null || CurrentController == "UserManagement" && CurrentAction == "Login")
      //{
      //  //Let things happend automatically.
      //  //return;
      //  filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "controller", "Account" }, { "action", "Login" },
      //    { "model", null }, { "returnUrl", "" } });
      //  return;
      //}




    }
  }
}
