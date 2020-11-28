using Repository.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Intranet.Controllers
{
    public class HomeController : Controller
    {
        private LeaveONEntities db = new LeaveONEntities();
        public ActionResult Index()
        {
            List<string> loginsList = new List<string>();
            //string id1 = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            //string id4 = Request.LogonUserIdentity.Name;
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
            UserPrincipal currentUser = UserPrincipal.FindByIdentity(ctx, User.Identity.Name);
            //loginsList.Add(currentUser.Name);
            //loginsList.Add(currentUser.DisplayName);
            //loginsList.Add(currentUser.GivenName);
            //loginsList.Add(currentUser.SamAccountName);
            loginsList.Add(currentUser.UserPrincipalName);
            var path = Server.MapPath(@"~/myLog.txt");
            System.IO.File.AppendAllLines(path, loginsList);

            //return View();
            //return Redirect("http://OtherSite/Test/Index?id=4");
            //https://localhost:44380/Account/Login?ReturnUrl=%2F

            //return Redirect("https://localhost:44380/");
            //ADUser

            //Response.Redirect("~/Pages/Product.aspx?id=" + id + "&Customize=" + custTotle);

            string ReturnUrlValue = "/";
            string ADUserValue = currentUser.UserPrincipalName;//"hp";
            //return Redirect("http://localhost:44380/Account/Login?ReturnUrl=" + ReturnUrlValue + "&ADUser=" + ADUserValue); //this for development/testing
            //http://lms.intechww.com/
            return Redirect("http://lms.intechww.com:9902/Account/Login?ReturnUrl=" + ReturnUrlValue + "&ADUser=" + ADUserValue);//this for production
            //return Redirect("https://localhost:9902/Account/Login?ReturnUrl=" + ReturnUrlValue);
            //return Redirect("https://localhost:9902/");
            //return Redirect("http://localhost:9902/");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

    }
}