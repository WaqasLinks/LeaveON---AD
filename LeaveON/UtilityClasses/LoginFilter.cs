using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
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

      List<string> loginsList = new List<string>();
      //string id1 = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
      //string id4 = Request.LogonUserIdentity.Name;
      PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
      UserPrincipal currentUser = UserPrincipal.FindByIdentity(ctx, HttpContext.Current.User.Identity.Name);
      loginsList.Add(currentUser.Name);
      loginsList.Add(currentUser.DisplayName);
      loginsList.Add(currentUser.GivenName);
      loginsList.Add(currentUser.SamAccountName);
      loginsList.Add(currentUser.UserPrincipalName);



      var path = System.Web.HttpContext.Current.Server.MapPath(@"~/myLog.txt");
      System.IO.File.AppendAllLines(path, loginsList);


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
