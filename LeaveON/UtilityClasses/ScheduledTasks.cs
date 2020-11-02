//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Repository.Models;
using Microsoft.AspNet.Identity;
using LMS.Constants;
using LMS.Mail;
using LeaveON.Migrations;
using System.Web.Configuration;
using System.Timers;
using System.Diagnostics;

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
        ResetLeavePolicyValues();
        SetOnLeaveEmployeesStatus();
      }
    }
    void ResetLeavePolicyValues()
    {
      DateTime CurrentSystemTime = DateTime.Now;
      foreach (UserLeavePolicy userLeavePolicy in db.UserLeavePolicies.ToList<UserLeavePolicy>())
      {
        if (CurrentSystemTime.CompareTo(userLeavePolicy.FiscalYearEnd) >= 0)
        {

          userLeavePolicy.FiscalYearStart=userLeavePolicy.FiscalYearStart.Value.AddYears(1);
          userLeavePolicy.FiscalYearEnd=userLeavePolicy.FiscalYearEnd.Value.AddYears(1);
          db.UserLeavePolicies.Attach(userLeavePolicy);
          db.Entry(userLeavePolicy).Property(x => x.FiscalYearStart).IsModified = true;
          db.Entry(userLeavePolicy).Property(x => x.FiscalYearEnd).IsModified = true;
          foreach (UserLeavePolicyDetail userLeavePolicyDetail in userLeavePolicy.UserLeavePolicyDetails.ToList<UserLeavePolicyDetail>())
          {
            //userLeavePolicyDetail.Allowed
            List<LeaveBalance> leaveBalances = db.LeaveBalances.Where(x => x.UserLeavePolicyId == userLeavePolicyDetail.UserLeavePolicyId && x.LeaveTypeId == userLeavePolicyDetail.LeaveTypeId).ToList<LeaveBalance>();
            foreach (LeaveBalance leaveBalance in leaveBalances)
            {
              if (leaveBalance is null) continue;
              leaveBalance.Balance = userLeavePolicyDetail.Allowed;
              leaveBalance.Taken = 0;
              db.Entry(leaveBalance).Property(x => x.Balance).IsModified = true;
              db.Entry(leaveBalance).Property(x => x.Taken).IsModified = true;
            }
            

          }
          db.SaveChanges();
        }
      }
      
    }
    void SetOnLeaveEmployeesStatus()
    {
      
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
