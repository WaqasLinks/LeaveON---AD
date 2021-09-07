//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;

using System;
using System.Collections.Generic;
using System.Linq;
using Repository.Models;

using System.Web.Configuration;
using System.Timers;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Reflection;
using System.Globalization;

namespace LeaveON.UtilityClasses
{
    public class ScheduledTasks// : Controller
    {
        // Added this class visible variable to hold the timer interval so it's not gotten from the
        // web.config file on each Elapsed event of the timer
        private static double TimerIntervalInMilliseconds =
            Convert.ToDouble(WebConfigurationManager.AppSettings["TimerIntervalInMilliseconds"]);

        private LeaveONEntities db = new LeaveONEntities();
        public void InitTimerForScheduleTasks()
        {
            // This will raise the Elapsed event every 'x' millisceonds (whatever you set in the
            // Web.Config file for the added TimerIntervalInMilliseconds AppSetting
            Timer timer = new Timer(TimerIntervalInMilliseconds);

            timer.Enabled = true;

            // Setup Event Handler for Timer Elapsed Event
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);

            timer.Start();
        }
        // Added the following procedure:
        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Get the TimerStartTime web.config value
            DateTime MyScheduledRunTime = DateTime.Parse(WebConfigurationManager.AppSettings["TimerStartTime"]);

            // Get the current system time
            DateTime CurrentSystemTime = DateTime.Now;

            Debug.WriteLine(string.Concat("Timer Event Handler Called: ", CurrentSystemTime.ToString()));

            // This makes sure your code will only run once within the time frame of (Start Time) to
            // (Start Time+Interval). The timer's interval and this (Start Time+Interval) must stay in sync
            // or your code may not run, could run once, or may run multiple times per day.
            DateTime LatestRunTime = MyScheduledRunTime.AddMilliseconds(TimerIntervalInMilliseconds);

            // If within the (Start Time) to (Start Time+Interval) time frame - run the processes
            if ((CurrentSystemTime.CompareTo(MyScheduledRunTime) >= 0) && (CurrentSystemTime.CompareTo(LatestRunTime) <= 0))
            {
                Debug.WriteLine(String.Concat("Timer Event Handling MyScheduledRunTime Actions: ", DateTime.Now.ToString()));
                // RUN YOUR PROCESSES HERE
                //Experiment();
                //Experiment1();
                SyncAppWithAD();

            }
        }

        public void SyncAppWithAD()
        {

            using (var context = new PrincipalContext(ContextType.Domain, "intechww.com"))// "tenf.loc"))
            {

                byte empFound = 0;
                int counter = 0;
                int insertedEmp = 0;
                int UpdatedEmp = 0;
                //List<string> loginsList = new List<string>();
                var path = @"D:\LeaveON - AD\Intranet\ADUserList.txt";
              
                using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                {

                    /////////////find in app database

                    var AllIntechUsers = searcher.FindAll();
                    List<AspNetUser> LstAspNetUsers = db.AspNetUsers.ToList<AspNetUser>();

                    //-------
                    //int cntr = 0;
                    //int non = 0;
                    //int authFalse = 0;
                    //AuthenticablePrincipal auth1;
                    //foreach (var result in AllIntechUsers)
                    //{
                    //    auth1 = result as AuthenticablePrincipal;
                    //    if (auth1.UserPrincipalName == null)
                    //    {
                    //        non += 1;
                    //        continue;
                    //    }
                    //    if (auth1 != null && auth1.Enabled == true)
                    //    {
                    //        cntr += 1;
                    //    }
                    //    if (auth1 != null && auth1.Enabled == false)
                    //    {
                    //        authFalse += 1;
                    //    }

                    //}
                    //-------
                    DateTime DateMark = DateTime.ParseExact("30/12/2019", "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    DateTime lastLogonStr;
                    AuthenticablePrincipal auth;
                    List<string> departmentsList = new List<string>();
                    List<string> countriesList = new List<string>();
                    foreach (var result in AllIntechUsers)
                    {
                        DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry;
                        //Console.WriteLine("First Name: " + de.Properties["givenName"].Value);
                        //Console.WriteLine("Last Name : " + de.Properties["sn"].Value);
                        //Console.WriteLine("SAM account name   : " + de.Properties["samAccountName"].Value);
                        //Console.WriteLine("User principal name: " + de.Properties["userPrincipalName"].Value);
                        //Console.WriteLine();
                        //if (de.Properties["userPrincipalName"].Value == null)
                        //{
                        //    continue;
                        //}
                        //DateTime WhenCreated = DateTime.Parse(de.Properties["whenCreated"].Value.ToString().Trim());
                        //DateTime LastLogon = DateTime.ParseExact("01/01/2019", "dd/MM/yyyy", CultureInfo.InvariantCulture); //= DateTime.Parse(de.Properties["LastLogon"].Value.ToString().Trim());
                        auth = result as AuthenticablePrincipal;
                        
                        if (auth == null || auth.UserPrincipalName == null || string.IsNullOrEmpty(auth.UserPrincipalName) || auth.Enabled == false)
                        {
                            continue;//we dont need this. simply move to next
                        }
                        if (auth.UserPrincipalName.ToLower().Contains("muzammil.riaz"))
                        {
                            var abc = "";
                            var abbb = de.Properties["EmployeeId"].Value;
                        }
                        counter += 1;
                        object adsLargeInteger = de.Properties["lastLogon"].Value;
                        
                        if (adsLargeInteger == null)
                        {
                            continue;
                        }
                        else
                        {
                            long highPart =
                                   (Int32)
                                       adsLargeInteger.GetType()
                                           .InvokeMember("HighPart", BindingFlags.GetProperty, null, adsLargeInteger, null);
                            long lowPart =
                                       (Int32)
                                           adsLargeInteger.GetType()
                                               .InvokeMember("LowPart", BindingFlags.GetProperty, null, adsLargeInteger, null);
                            long lastLogonL = (long)((uint)lowPart + (((long)highPart) << 32)); // Get value as long
                            lastLogonStr = DateTime.FromFileTime(lastLogonL); // get value as DateTime string

                        }

                        //TimeSpan TimeDifference = LastLogon - DateMarker;
                        if (lastLogonStr > DateMark && !string.IsNullOrEmpty(Convert.ToString(de.Properties["co"].Value)))
                        {
                            departmentsList.Add(Convert.ToString(de.Properties["department"].Value));
                            countriesList.Add(Convert.ToString(de.Properties["co"].Value));
                            AspNetUser aspNetUser = LstAspNetUsers.FirstOrDefault(x => x.UserName.Replace(" ", "").ToUpper() == auth.UserPrincipalName.Replace(" ", "").ToUpper());
                           

                            if (aspNetUser == null)
                            {//Insert
                             //it means if user is created before "01/01/2019" then totaDays will be in minus. so not add very old users. only add new users. which are after "01/01/2019"
                             //this is just to fast the process
                             //if (TimeDifference.TotalDays < 0) continue;
                                insertedEmp += 1;
                                InsertEmployee(de);


                            }
                            else
                            {//Update
                                if (aspNetUser.IsActive != IsActive(de) || string.IsNullOrEmpty(aspNetUser.DepartmentName) || 
                                    aspNetUser.DepartmentName != Convert.ToString(de.Properties["department"].Value) || 
                                    aspNetUser.CntryName != Convert.ToString(de.Properties["co"].Value) || aspNetUser.BioStarEmpNum ==0)
                                {
                                    UpdateEmployee(aspNetUser, de);
                                }
                            }
                        }
                        //}
                        ////////////////////////////
                    }

                    //-----------add department name which does not exist in LMS-DB------------
                    List<string> distinctDepartmentNames = departmentsList.Distinct().ToList();
                    foreach (string itm in distinctDepartmentNames)
                    {
                        DepartmentName departmentName = db.DepartmentNames.FirstOrDefault(x => x.Name == itm);
                        if (departmentName == null && !string.IsNullOrEmpty(itm.Trim()))
                        {
                            departmentName = new DepartmentName() { Name = itm };
                            db.DepartmentNames.Add(departmentName);
                        }
                    }
                    //-------------remove department name which does not exist in AD-------------
                    foreach (var itm in db.DepartmentNames.ToList())
                    {
                        string foundName = distinctDepartmentNames.FirstOrDefault(x => x == itm.Name);
                        if (string.IsNullOrEmpty(foundName))
                        {
                            db.DepartmentNames.Remove(itm);
                        }
                    }

                    //-----------add country name which does not exist in LMS-DB------------
                    List<string> distinctCountriesNames = countriesList.Distinct().ToList();
                    foreach (string itm in distinctCountriesNames)
                    {
                        CountryName countryName = db.CountryNames.FirstOrDefault(x => x.Name == itm);
                        if (countryName == null && !string.IsNullOrEmpty(itm.Trim()))
                        {
                            countryName = new CountryName() { Name = itm };
                            db.CountryNames.Add(countryName);
                        }
                    }

                    db.SaveChanges();

                    //System.IO.File.WriteAllLines(path, loginsList);
                    ////////////////////////////now find in AD

                    //foreach (Employee item in _LstEmployees)
                    //{
                    //    foreach (var result in AllIntechUsers)
                    //    {
                    //        DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry;
                    //        empFound = false;
                    //        if (item.EmployeeName == Convert.ToString(de.Properties["userPrincipalName"].Value))
                    //        {
                    //            empFound = true;
                    //            break;
                    //        }
                    //    }
                    //    if (empFound == false)
                    //    {
                    //        DeleteEmployee(item.EmployeeCode);
                    //    }
                    //}

                }



                //if (insertedEmp > 0 || UpdatedEmp > 0)
                //{
                //    MessageBox.Show(insertedEmp + " new user(s) found." + System.Environment.NewLine + UpdatedEmp + " user(s) status updated." + System.Environment.NewLine + "Please close and reopen this form to view updated data");
                //}
                //else
                //{
                //    MessageBox.Show("Data is up-to-date");
                //}


            }

        }

        private bool IsActive(DirectoryEntry de)
        {
            if (de.NativeGuid == null) return false;

            int flags = (int)de.Properties["userAccountControl"].Value;

            return !Convert.ToBoolean(flags & 0x0002);
        }
        private void InsertEmployee(DirectoryEntry de)
        {
            //return;
            AspNetUser emp = new AspNetUser();
            
            emp.UserName = Convert.ToString(de.Properties["userPrincipalName"].Value);
            emp.Email = Convert.ToString(de.Properties["userPrincipalName"].Value);
            emp.Id = Guid.NewGuid().ToString();
            emp.BioStarEmpNum = Convert.ToInt32(de.Properties["facsimileTelephoneNumber"].Value);//null; //0000;
            emp.EmailConfirmed = false;
            emp.PasswordHash = "ABaTT1CcvSEzwTzDXHnXFm+9cJ3Zaa65Z6QMZ4ZygNVyX8TIvSevNuJGKX7k81VQVQ==";
            emp.SecurityStamp = "e93564e2-08f0-47cd-a822-4b99ca4c08d2";
            emp.PhoneNumberConfirmed = false;
            emp.TwoFactorEnabled = false;
            emp.LockoutEnabled = true;
            emp.AccessFailedCount = 0;
            emp.DateCreated = DateTime.Now;
            
            emp.DepartmentName = Convert.ToString(de.Properties["department"].Value);
            emp.CntryName = Convert.ToString(de.Properties["co"].Value);
            emp.IsActive = IsActive(de);
            
            db.AspNetUsers.Add(emp);

            //----add user role
            //if (String.IsNullOrEmpty( emp.CntryName ))
            //{
            //    var abc = 1;
            //    return;
            //}
            //db.SaveChangesAsync();
            db.SaveChanges();
        }

        private void UpdateEmployee(AspNetUser oldEmp, DirectoryEntry de)
        {
            //return;
            //AspNetUser emp;
            //emp = new AspNetUser();
            //emp.IsActive = IsActive(de);
            oldEmp.IsActive = IsActive(de);
            oldEmp.CntryName = Convert.ToString(de.Properties["co"].Value);
            oldEmp.DepartmentName = Convert.ToString(de.Properties["department"].Value);
            oldEmp.BioStarEmpNum = Convert.ToInt32(de.Properties["facsimileTelephoneNumber"].Value);
            db.AspNetUsers.Attach(oldEmp);

            db.Entry(oldEmp).Property(x => x.IsActive).IsModified = true;
            db.Entry(oldEmp).Property(x => x.DepartmentName).IsModified = true;
            db.Entry(oldEmp).Property(x => x.CntryName).IsModified = true;
            db.Entry(oldEmp).Property(x => x.BioStarEmpNum).IsModified = true;
            //db.SaveChangesAsync();
            db.SaveChanges();
            //db.Entry(emp).State = EntityState.Modified;
            //db.SaveChangesAsync();

        }
        public void Experiment()
        {
            var context = new DirectoryContext(DirectoryContextType.Forest, "intechww.com");
            List<string> Lstabc1 = new List<string>();
            using (var schema = System.DirectoryServices.ActiveDirectory.ActiveDirectorySchema.GetSchema(context))
            {
                var userClass = schema.FindClass("user");

                foreach (ActiveDirectorySchemaProperty property in userClass.GetAllProperties())
                {
                    var abc = property.Name;
                    if (property.Name.ToLower().Contains("region"))
                    {

                        Lstabc1.Add(property.Name);
                    }
                    // property.Name is what you're looking for
                }
            }
        }
    }
}

//public void LeavePolicyValues()
//{
//  string LoggedInUserId = User.Identity.GetUserId();
//  int LoggedInUserLeavePolicyId = db.AspNetUsers.FirstOrDefault(x => x.Id == LoggedInUserId).UserLeavePolicyId.Value;
//  UserLeavePolicy userLeavePolicy = db.UserLeavePolicies.FirstOrDefault(x => x.Id == LoggedInUserLeavePolicyId);
//  List<UserLeavePolicyDetail> LoggedInUserLeavePolicyDetails = db.UserLeavePolicyDetails.Where(x => x.UserLeavePolicyId == LoggedInUserLeavePolicyId).ToList();
//  List<LeaveBalance> LoggedInUserLeaveBalances = db.LeaveBalances.Where(x => x.UserId == LoggedInUserId).ToList();
//  foreach (UserLeavePolicyDetail leavePolicy in LoggedInUserLeavePolicyDetails)
//  {
//    foreach (LeaveBalance leaveBalance in LoggedInUserLeaveBalances)
//    {
//      //          status 1 mean values reset
//      //staus 0 mean values has to reset
//      //date kay sath 1 or 0 ka check lagay ga
//    }
//  }
//}
