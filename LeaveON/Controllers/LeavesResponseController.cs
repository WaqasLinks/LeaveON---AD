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
using LMS.Mail;

namespace LeaveON.Controllers
{
  
  [Authorize(Roles = "Admin,Manager")]
  public class LeavesResponseController : Controller
  {
    private LeaveONEntities db = new LeaveONEntities();

    // GET: Leaves
    public async Task<ActionResult> Index()
    {
      //var leaves = db.Leaves.Include(l => l.LeaveType).Include(l => l.UserLeavePolicy);
      //var leaves = db.Leaves.Include(l => l.LeaveType);
      string LoggedInUserId = User.Identity.GetUserId();
      var leaves = db.Leaves.Where(x => x.IsQuotaRequest == false && (x.LineManager1Id == LoggedInUserId || x.LineManager2Id == LoggedInUserId));
      return View(await leaves.ToListAsync());
    }
    public async Task<ActionResult> QuotaResponseHistory()
    {
      //var leaves = db.Leaves.Include(l => l.LeaveType).Include(l => l.UserLeavePolicy);
      //var leaves = db.Leaves.Include(l => l.LeaveType);
      string LoggedInUserId = User.Identity.GetUserId();
      var leaves = db.Leaves.Where(x => x.IsQuotaRequest==true && ( x.LineManager1Id == LoggedInUserId || x.LineManager2Id == LoggedInUserId));
      return View(await leaves.ToListAsync());
    }

    // GET: Leaves/Details/5
    public async Task<ActionResult> Details(decimal id)
    {
      if (id == null)
      {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }
      Leave leave = await db.Leaves.FindAsync(id);
      if (leave == null)
      {
        return HttpNotFound();
      }
      return View(leave);
    }

    // GET: Leaves/Create
    public ActionResult Create()
    {
      //ViewBag.UserId = "d0c9d0b1-d0e8-4d56-a410-72e74af3ced8";
      ViewBag.LeaveTypeId = new SelectList(db.LeaveTypes, "Id", "Name");
      ViewBag.UserLeavePolicyId = new SelectList(db.UserLeavePolicies, "Id", "UserId");
      ViewBag.LineManagers = new SelectList(db.AspNetUsers, "Id", "UserName");
      ViewBag.UserName = "LoggedIn User";
      return View();
    }
    public List<AspNetUser> GetSeniorStaff()
    {
      List<AspNetUser> Seniors = new List<AspNetUser>();
      foreach (AspNetUser user in db.AspNetUsers.ToList<AspNetUser>())
      {
        foreach (AspNetRole role in user.AspNetRoles.ToList<AspNetRole>())
        {
          if (role.Name == "Admin" || role.Name == "Manager")
          {
            AspNetUser userFound = Seniors.Find(x => x.Id == user.Id);
            if (userFound == null)
            {
              Seniors.Add(user);
            }
          }
        }
      }
      return Seniors;
    }
    // POST: Leaves/Create
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Create([Bind(Include = "Id,UserId,LeaveTypeId,Reason,StartDate,EndDate,TotalDays,EmergencyContact,LineManager1Id,LineManager2Id")] Leave leave)
    {
      leave.DateCreated = DateTime.Now;
      if (ModelState.IsValid)
      {
        db.Leaves.Add(leave);
        await db.SaveChangesAsync();
        return RedirectToAction("Index");
      }

      ViewBag.LeaveTypeId = new SelectList(db.LeaveTypes, "Id", "Name", leave.LeaveTypeId);
      //ViewBag.UserLeavePolicyId = new SelectList(db.UserLeavePolicies, "Id", "UserId", leave.UserLeavePolicyId);
      ViewBag.LineManagers = new SelectList(db.AspNetUsers, "Id", "UserName");
      return View(leave);
    }

    // GET: Leaves/Edit/5
    public async Task<ActionResult> Edit(decimal id)
    {
      if (id == null)
      {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }
      Leave leave = await db.Leaves.FindAsync(id);
      if (leave == null)
      {
        return HttpNotFound();
      }

      //ViewBag.LineManager1 = db.AspNetUsers.FirstOrDefault(x => x.Id == leave.LineManager1Id).UserName;
      //ViewBag.LineManager2 = db.AspNetUsers.FirstOrDefault(x => x.Id == leave.LineManager2Id).UserName;
      List<AspNetUser> Seniors = GetSeniorStaff();
      bool IsLineManager1 = false;
      if (User.Identity.GetUserId() == leave.LineManager1Id)
      {
        IsLineManager1 = true;
      }
      else
      {
        IsLineManager1 = false;
      }
      ViewBag.IsLineManager1 = IsLineManager1;

      ViewBag.LineManagers = new SelectList(Seniors, "Id", "UserName");
      ViewBag.LeaveTypeId = new SelectList(db.LeaveTypes, "Id", "Name", leave.LeaveTypeId);
      ViewBag.ApplicantName = db.AspNetUsers.FirstOrDefault(x => x.Id == leave.UserId).UserName;
      //ViewBag.UserLeavePolicyId = new SelectList(db.UserLeavePolicies, "Id", "UserId", leave.UserLeavePolicyId);
      ViewBag.UserLeavePolicyId = leave.AspNetUser.UserLeavePolicy.Id;
      ViewBag.LeaveUserId = leave.AspNetUser.Id;
      return View(leave);
    }

    // POST: Leaves/Edit/5
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit([Bind(Include = "Id,UserId,LeaveTypeId,Reason,StartDate,EndDate,TotalDays,EmergencyContact,ResponseDate1,ResponseDate2,IsAccepted1,IsAccepted2,LineManager1Id,LineManager2Id,Remarks1,Remarks2,DateCreated,DateModified,UserLeavePolicyId")] Leave leave, string IsLineManager1)
    {
      //assign values to variable as we will reassing these values to the object
      leave.IsQuotaRequest = false;
      Nullable<int> IsAccepted1 = null;
      Nullable<int> IsAccepted2 = null;
      string Remarks1 = string.Empty;
      string Remarks2 = string.Empty;
      if (IsLineManager1 == "True")
      {
        IsAccepted1 = leave.IsAccepted1;
        Remarks1 = leave.Remarks1;
        
      }
      else
      {
        IsAccepted2 = leave.IsAccepted2;
        Remarks2 = leave.Remarks2;
      }
      //--------------------------------------------------
      Leave leaveOld = db.Leaves.FirstOrDefault(x => x.Id == leave.Id);
      leave = leaveOld;

      if (IsLineManager1 == "True")
      {
        leave.IsAccepted1 = IsAccepted1;
        leave.Remarks1 = Remarks1;
        leave.ResponseDate1 = DateTime.Now;
        
        if (IsAccepted1 == 2)
        {
          leave.Remarks1 = string.Empty;
          leave.TotalDays = decimal.Parse(Remarks1);
        }
        
        if (leave.LineManager1Id == leave.LineManager2Id)
        {
          leave.IsAccepted2 = IsAccepted1;
          leave.Remarks2 = leave.Remarks1;
          leave.ResponseDate2 = DateTime.Now;
          if (leave.IsAccepted2 > 0) CalculateAndChangeLeaveBalance(ref leave);
        }
      }
      else
      {
        leave.IsAccepted2 = IsAccepted2;
        leave.Remarks2 = Remarks2;
        leave.ResponseDate2 = DateTime.Now;
        if (IsAccepted2 == 2)
        {
          leave.Remarks2 = string.Empty;
          leave.TotalDays = decimal.Parse(Remarks2);
        }
        if (leave.IsAccepted2 > 0) CalculateAndChangeLeaveBalance(ref leave);
      }

      if (ModelState.IsValid)
      {
        db.Entry(leave).State = EntityState.Modified;

        await db.SaveChangesAsync();
        if (IsLineManager1 == "True")
        {
          AspNetUser admin = db.AspNetUsers.FirstOrDefault(x => x.Id == leave.LineManager1Id);
          SendEmail.SendEmailUsingLeavON(leave, SendEmail.LeavON_Email, SendEmail.LeavON_Password, admin, leave.AspNetUser , "LeaveResponse");

          if (leave.LineManager1Id == leave.LineManager2Id)
          {
            admin = db.AspNetUsers.FirstOrDefault(x => x.Id == leave.LineManager2Id);
            SendEmail.SendEmailUsingLeavON(leave, SendEmail.LeavON_Email, SendEmail.LeavON_Password, admin, leave.AspNetUser, "LeaveResponse");
          }

        }
        else
        {
          AspNetUser admin = db.AspNetUsers.FirstOrDefault(x => x.Id == leave.LineManager2Id);
          SendEmail.SendEmailUsingLeavON(leave, SendEmail.LeavON_Email, SendEmail.LeavON_Password, admin, leave.AspNetUser, "LeaveResponse");
        }
        
        //AspNetUser admin2 = db.AspNetUsers.FirstOrDefault(x => x.Id == leave.LineManager2Id);
        //SendEmail.SendEmailUsingLeavON(SendEmail.LeavON_Email, SendEmail.LeavON_Password, leave.AspNetUser, admin2, "LeaveRequest");

        return RedirectToAction("Index");
      }
      ViewBag.LeaveTypeId = new SelectList(db.LeaveTypes, "Id", "Name", leave.LeaveTypeId);
      //ViewBag.UserLeavePolicyId = new SelectList(db.UserLeavePolicies, "Id", "UserId", leave.UserLeavePolicyId);
      return View(leave);
    }
    // GET: Leaves/Edit/5
    public async Task<ActionResult> EditCompensatoryQuotaResponse(decimal id)
    {
      if (id == null)
      {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }
      Leave leave = await db.Leaves.FindAsync(id);
      if (leave == null)
      {
        return HttpNotFound();
      }

      //ViewBag.LineManager1 = db.AspNetUsers.FirstOrDefault(x => x.Id == leave.LineManager1Id).UserName;
      //ViewBag.LineManager2 = db.AspNetUsers.FirstOrDefault(x => x.Id == leave.LineManager2Id).UserName;
      List<AspNetUser> Seniors = GetSeniorStaff();
      bool IsLineManager1 = false;
      if (User.Identity.GetUserId() == leave.LineManager1Id)
      {
        IsLineManager1 = true;
      }
      else
      {
        IsLineManager1 = false;
      }
      ViewBag.IsLineManager1 = IsLineManager1;

      ViewBag.LineManagers = new SelectList(Seniors, "Id", "UserName");
      ViewBag.LeaveTypeId = new SelectList(db.LeaveTypes, "Id", "Name", leave.LeaveTypeId);
      ViewBag.ApplicantName = db.AspNetUsers.FirstOrDefault(x => x.Id == leave.UserId).UserName;
      //ViewBag.UserLeavePolicyId = new SelectList(db.UserLeavePolicies, "Id", "UserId", leave.UserLeavePolicyId);
      ViewBag.UserLeavePolicyId = leave.AspNetUser.UserLeavePolicyId;
      ViewBag.LeaveUserId = leave.AspNetUser.Id;
      return View(leave);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> EditCompensatoryQuotaResponse([Bind(Include = "Id,UserId,LeaveTypeId,Reason,StartDate,EndDate,TotalDays,EmergencyContact,ResponseDate1,ResponseDate2,IsAccepted1,IsAccepted2,LineManager1Id,LineManager2Id,Remarks1,Remarks2,DateCreated,DateModified,UserLeavePolicyId")] Leave leave, string IsLineManager1)
    {
      //----------------------------get new value----------------------------------------------
      //assign values to variable as we will reassing these values to the object
      //leave.IsQuotaRequest = true; no need to assing ture. when we get old leave few line ahead there is ture in IsQotaRequest
      Nullable<int> IsAccepted1 = null;
      Nullable<int> IsAccepted2 = null;
      string Remarks1 = string.Empty;
      string Remarks2 = string.Empty;
      DateTime startDate= leave.StartDate;
      DateTime endDate= leave.EndDate;
      decimal totalDays;
      if (leave.TotalDays == null)
      {
        totalDays = (decimal)(endDate - startDate).TotalDays+1;
      }
      else
      {
        totalDays = leave.TotalDays.Value;
      }
      
      if (IsLineManager1 == "True")
      {
        IsAccepted1 = leave.IsAccepted1;
        if (IsAccepted1==2) Remarks1 = (leave.Remarks1 == null) ? string.Empty : leave.Remarks1.Trim();
      }
      else
      {
        IsAccepted2 = leave.IsAccepted2;
        if (IsAccepted2 == 2) Remarks2 = (leave.Remarks2 == null) ? string.Empty : leave.Remarks2.Trim();
      }
      //--------------------------get old leave and put new values to it------------------------
      Leave leaveOld = db.Leaves.FirstOrDefault(x => x.Id == leave.Id);
      leave = leaveOld;
      leave.TotalDays = totalDays;
      if (IsLineManager1 == "True")
      {
        leave.IsAccepted1 = IsAccepted1;
        leave.Remarks1 = Remarks1;
        //if (!(string.IsNullOrEmpty(Remarks1))) leave.TotalDays = decimal.Parse(Remarks1);
        leave.ResponseDate1 = DateTime.Now;
        if (IsAccepted1 == 2)
        {
          //leave.Remarks1 = string.Empty;
          //if (!(string.IsNullOrEmpty(Remarks1))) leave.TotalDays = decimal.Parse(Remarks1);
          leave.StartDate = startDate;
          leave.EndDate = endDate;
          leave.TotalDays = totalDays;
        }
        
        if (leave.LineManager1Id == leave.LineManager2Id)
        {
          leave.IsAccepted2 = IsAccepted1;
          leave.Remarks2 = leave.Remarks1;
          leave.ResponseDate2 = DateTime.Now;
          // calculatin will perform when linemanager 2 will aprove so it is in if condition
          if (leave.IsAccepted2 > 0) CalculateAndChangeLeaveBalanceQuota(ref leave);
        }
        
      }
      else
      {
        leave.IsAccepted2 = IsAccepted2;
        leave.Remarks2 = Remarks2;
        leave.ResponseDate2 = DateTime.Now;
        if (IsAccepted2 == 2)
        {
          //leave.Remarks2 = string.Empty;
          //if (!(string.IsNullOrEmpty(Remarks2))) leave.TotalDays = decimal.Parse(Remarks2);
          leave.StartDate = startDate;
          leave.EndDate = endDate;
          leave.TotalDays = totalDays;
        }
        if (leave.IsAccepted2>0) CalculateAndChangeLeaveBalanceQuota(ref leave);

      }

      if (ModelState.IsValid)
      {
        db.Entry(leave).State = EntityState.Modified;

        await db.SaveChangesAsync();
        //------------------------------sending mail----------------------------------------------
        if (IsLineManager1 == "True")
        {
          AspNetUser admin = db.AspNetUsers.FirstOrDefault(x => x.Id == leave.LineManager1Id);
          SendEmail.SendEmailUsingLeavON(leave, SendEmail.LeavON_Email, SendEmail.LeavON_Password, admin, leave.AspNetUser, "LeaveResponse");
          if (leave.LineManager1Id == leave.LineManager2Id)
          {
            admin = db.AspNetUsers.FirstOrDefault(x => x.Id == leave.LineManager2Id);
            SendEmail.SendEmailUsingLeavON(leave, SendEmail.LeavON_Email, SendEmail.LeavON_Password, admin, leave.AspNetUser, "LeaveResponse");
          }
        }
        else
        {
          AspNetUser admin = db.AspNetUsers.FirstOrDefault(x => x.Id == leave.LineManager2Id);
          SendEmail.SendEmailUsingLeavON(leave, SendEmail.LeavON_Email, SendEmail.LeavON_Password, admin, leave.AspNetUser, "LeaveResponse");
        }

        //AspNetUser admin2 = db.AspNetUsers.FirstOrDefault(x => x.Id == leave.LineManager2Id);
        //SendEmail.SendEmailUsingLeavON(SendEmail.LeavON_Email, SendEmail.LeavON_Password, leave.AspNetUser, admin2, "LeaveRequest");

        return RedirectToAction("QuotaResponseHistory");
      }
      ViewBag.LeaveTypeId = new SelectList(db.LeaveTypes, "Id", "Name", leave.LeaveTypeId);
      //ViewBag.UserLeavePolicyId = new SelectList(db.UserLeavePolicies, "Id", "UserId", leave.UserLeavePolicyId);
      return View(leave);
    }
    public void CalculateAndChangeLeaveBalance(ref Leave leave)
    {
      ///////////////
      LeaveBalance leaveBalance = CalculateLeaveBalance(ref leave);

      if (leaveBalance == null)
      {
        //new
        leaveBalance = new LeaveBalance(ref leave);
        leaveBalance.Taken = leave.TotalDays;
        leaveBalance.Balance -= leave.TotalDays;
        leaveBalance.UserId = leave.UserId;
        leaveBalance.LeaveTypeId = leave.LeaveTypeId;
        leaveBalance.UserLeavePolicyId = leave.AspNetUser.UserLeavePolicyId;
        db.LeaveBalances.Add(leaveBalance);
      }
      else
      {
        //old
        leaveBalance.Taken += leave.TotalDays;
        leaveBalance.Balance -= leave.TotalDays;
        db.Entry(leaveBalance).State = EntityState.Modified;
      }
      ///////////////
    }
    public void CalculateAndChangeLeaveBalanceQuota(ref Leave leave)
    {
      LeaveBalance leaveBalance = CalculateLeaveBalanceQuota(ref leave);

      if (leaveBalance == null)
      {
        //new
        leaveBalance = new LeaveBalance(ref leave);

        leaveBalance.UserId = leave.UserId;
        leaveBalance.LeaveTypeId = leave.LeaveTypeId;
        //leaveBalance.UserLeavePolicyId = leave.AspNetUser.UserLeavePolicyId;
        db.LeaveBalances.Add(leaveBalance);
      }
      else
      {
        leaveBalance.Balance += leave.TotalDays;
        db.Entry(leaveBalance).State = EntityState.Modified;
      }
    }
    public LeaveBalance CalculateLeaveBalance(ref Leave leave)
    {
      //check there is some weakend


      //check there is some anual leaves/public holidays


      //add sbubtract leave and add or update to leaveBalnce table


      List<int> weeklyOffDays = leave.AspNetUser.UserLeavePolicy.WeeklyOffDays.Split(',').Select(int.Parse).ToList();
      List<AnnualOffDay> AnnualOffDays = leave.AspNetUser.UserLeavePolicy.AnnualOffDays.ToList<AnnualOffDay>();

      double TotalOffDays = (leave.EndDate - leave.StartDate).TotalDays + 1;
      double NaturalOffDays = 0;
      bool found = false;
      for (DateTime DateIdx = leave.StartDate; DateIdx <= leave.EndDate; DateIdx = DateIdx.AddDays(1))
      {

        foreach (int d in weeklyOffDays)
        {
          if (d == (int)DateIdx.DayOfWeek)
          {
            //weekend off days
            NaturalOffDays += 1;
            found = true;
            break;
          }
        }
        //this condition is due to: example: if national holiday comes on sunday. then count it one day of. if this statement is not here it will conunt two days. which is wrong
        if (found == true) { found = false; continue; }

        for (int i = 0; i < AnnualOffDays.Count; i++)
        {
          if (DateIdx.Date.CompareTo(AnnualOffDays[i].OffDay) == 0)
          {
            //annual off days
            NaturalOffDays += 1;
            break;
          }
        }
      }
      leave.TotalDays = (int)(TotalOffDays - NaturalOffDays);
      string UserId = leave.UserId;
      int LeaveTypeId = leave.LeaveTypeId;
      //List<int> weeklyOffDays = leave.AspNetUser.UserLeavePolicy.WeeklyOffDays.Split(',').Select(int.Parse).ToList();

      LeaveBalance lb = leave.AspNetUser.LeaveBalances.FirstOrDefault(x => x.UserId == UserId && x.LeaveTypeId == LeaveTypeId);
      return lb;

    }
    public LeaveBalance CalculateLeaveBalanceQuota(ref Leave leave)
    {
      string UserId = leave.UserId;
      int LeaveTypeId = leave.LeaveTypeId;
      //List<int> weeklyOffDays = leave.AspNetUser.UserLeavePolicy.WeeklyOffDays.Split(',').Select(int.Parse).ToList();

      LeaveBalance lb = leave.AspNetUser.LeaveBalances.FirstOrDefault(x => x.UserId == UserId && x.LeaveTypeId == LeaveTypeId);
      return lb;

    }
    public static int CalculateLeaveDays(DateTime startDate, DateTime endDate, List<DateTime> excludeDates)
    {
      int count = 0;
      for (DateTime DateIdx = startDate; DateIdx < endDate; DateIdx = DateIdx.AddDays(1))
      {
        if (DateIdx.DayOfWeek != DayOfWeek.Sunday && DateIdx.DayOfWeek != DayOfWeek.Saturday)
        {
          bool excluded = false;
          for (int i = 0; i < excludeDates.Count; i++)
          {
            if (DateIdx.Date.CompareTo(excludeDates[i].Date) == 0)
            {
              excluded = true;
              break;
            }
          }

          if (!excluded)
          {
            count++;
          }
        }
      }

      return count;
    }

    // GET: Leaves/Delete/5
    public async Task<ActionResult> Delete(decimal id)
    {
      if (id == null)
      {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }
      Leave leave = await db.Leaves.FindAsync(id);
      if (leave == null)
      {
        return HttpNotFound();
      }
      return View(leave);
    }

    // POST: Leaves/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> DeleteConfirmed(decimal id)
    {
      Leave leave = await db.Leaves.FindAsync(id);
      db.Leaves.Remove(leave);
      await db.SaveChangesAsync();
      return RedirectToAction("Index");
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        db.Dispose();
      }
      base.Dispose(disposing);
    }
  }
}
