using Repository.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Web.Mvc;

namespace Intranet.Controllers
{
    public class HomeController : Controller
    {
        private LeaveONEntities db = new LeaveONEntities();
        public ActionResult Index(string ReturnUrl)
        {
            //Experiment1();
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
            //ReturnUrl = "https://localhost:44380/LeavesResponse/Edit/60"; example
            string ReturnUrlValue = "/";

            if (string.IsNullOrEmpty(ReturnUrl)) ReturnUrl = "/";

            string ADUserValue = currentUser.UserPrincipalName;//"hp";
            //return Redirect("http://localhost:44380/Account/Login?ReturnUrl=" + ReturnUrlValue + "&ADUser=" + ADUserValue); //this for development/testing
            //http://lms-test.intechww.com/
            //return Redirect("http://lms-test.intechww.com:9902/Account/Login?ReturnUrl=" + ReturnUrlValue + "&ADUser=" + ADUserValue);//this for production
            //https://localhost:1002/Account/Login?ReturnUrl=/&ADUser=bsserviceaccount@intechww.com
            //return Redirect("http://lms.intechww.com:1002/Account/Login?ReturnUrl=" + ReturnUrl + "&ADUser=" + ADUserValue);//this for production
            
            //return Redirect("http://lms.intechww.com:1002/Account/Login?ReturnUrl=" + ReturnUrl + "&ADUser=" + ADUserValue);//this for production *new
            return Redirect("https://lms.intechww.com:1002/Account/Login?ReturnUrl=" + ReturnUrl + "&ADUser=" + ADUserValue);//this for production *new test


            //return Redirect("https://localhost:9902/Account/Login?ReturnUrl=" + ReturnUrlValue);

            //return Redirect("https://localhost:9902/");
            //return Redirect("http://localhost:9902/");

            //return Redirect("https://localhost:44380/LeavesResponse/Edit/60");
            //return Redirect("https://localhost:44380/Account/Login?ReturnUrl=" + ReturnUrl + "&ADUser=" + ADUserValue);//this for production

        }
        public void Experiment1()
        {
            var path = Server.MapPath(@"~/UsersAndProperties.txt");
            //List<User> users = new List<User>();
            List<string> userprops = new List<string>();
            try
            {
                DirectoryEntry root = new DirectoryEntry("LDAP://RootDSE");
                root = new DirectoryEntry("LDAP://" + root.Properties["defaultNamingContext"][0]);
                DirectorySearcher search = new DirectorySearcher(root);
                search.Filter = "(&(objectClass=user)(objectCategory=person))";

                //search.PropertiesToLoad.Add("samaccountname");
                //search.PropertiesToLoad.Add("displayname");
                //search.PropertiesToLoad.Add("mail");
                //search.PropertiesToLoad.Add("telephoneNumber");
                //search.PropertiesToLoad.Add("department");
                //search.PropertiesToLoad.Add("title");

                SearchResultCollection results = search.FindAll();
                if (results != null)
                {
                    foreach (SearchResult result in results)
                    {
                        foreach (DictionaryEntry property in result.Properties)
                        {
                            //Debug.Write(property.Key + ": ");
                            userprops.Add(property.Key + ": ");
                            foreach (var val in (property.Value as ResultPropertyValueCollection))
                            {
                                //Debug.Write(val + "; ");
                                userprops.Add(val + "; ");
                            }
                            //Debug.WriteLine("");
                            userprops.Add(Environment.NewLine + "");
                        }
                        userprops.Add(Environment.NewLine + "------------------------------------");
                    }
                }
                System.IO.File.WriteAllLines(path, userprops);
            }
            catch (Exception ex)
            {

            }
        }
      

    }
}