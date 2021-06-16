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

namespace LeaveON.Controllers
{
  
  [Authorize(Roles = "Admin,Manager,User")]
  public class LeavesRequestController : Controller
  {
    private LeaveONEntities db = new LeaveONEntities();

    // GET: Leaves
    //public async Task<ActionResult> Index(string country)
    public async Task<ActionResult> Index()
    {
      //var leaves = db.Leaves.Include(l => l.LeaveType).Include(l => l.UserLeavePolicy);
      string LoggedInUserId = User.Identity.GetUserId();
      IQueryable<Leave> leaves = db.Leaves.Where(x=>x.UserId== LoggedInUserId && (x.IsQuotaRequest == false || x.IsQuotaRequest == null)).AsQueryable<Leave>();
      return View(await leaves.ToListAsync());
    }
    public async Task<ActionResult> QuotaRequestHistory()
    {
      //var leaves = db.Leaves.Include(l => l.LeaveType).Include(l => l.UserLeavePolicy);
      string LoggedInUserId = User.Identity.GetUserId();
      IQueryable<Leave> leaves = db.Leaves.Where(x => x.UserId == LoggedInUserId && x.IsQuotaRequest==true).AsQueryable<Leave>();
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
      //ViewBag.LeaveTypeId = new SelectList(db.LeaveTypes, "Id", "Name");
      string userId = User.Identity.GetUserId();
      int policyId = db.AspNetUsers.FirstOrDefault(x => x.Id == userId).UserLeavePolicyId.GetValueOrDefault();
      // db.UserLeavePolicyDetails.Where(x => x.UserLeavePolicyId == policyId);

      //ViewBag.LeaveTypeId = new SelectList(db.LeaveTypes.Where(x => x.UserLeavePolicyDetails.Where(y => y.UserLeavePolicyId == policyId)), "Id", "Name");
      ViewBag.LeaveTypeId = new SelectList(db.LeaveTypes.Where(x => x.UserLeavePolicyDetails.Any(y => y.UserLeavePolicyId == policyId) || x.Id == Consts.CompensatoryLeaveTypeId), "Id", "Name");
      //ViewBag.Leave1TypeId = new SelectList(db.UserLeavePolicyDetails.Where(x => x.UserLeavePolicyId == policyId).ToList <UserLeavePolicyDetail>(), "Id", "Name");

      //ViewBag.LeaveTypeId = new SelectList(customLeaveTypes, "Id", "Name");
      //ViewBag.UserLeavePolicyId = new SelectList(db.UserLeavePolicies, "Id", "UserId");

      if (policyId > 0)
      {
        ViewBag.UserLeavePolicyId = policyId;
      }
      else
      {
        ViewBag.PolicyAlert = "No Policy is implemented for your account, Contact Admin";
      }
           
      //List<AspNetUser> Seniors = GetSeniorStaff();
      //ViewBag.LineManagers = new SelectList(Seniors, "Id", "UserName");
      ViewBag.LineManagers = new SelectList(db.AspNetUsers.OrderBy(x=>x.UserName), "Id", "UserName");
      ViewBag.UserName = User.Identity.Name;//"LoggedIn User";
      ViewBag.LeaveUserId = userId;
      //UserLeavePoliciesController UserLeavePolicies = new UserLeavePoliciesController();//.FileUploadMsgView("some string");
      //var result= UserLeavePolicies.Edit(7);
      //ViewBag.UserLeavePolicy= UserLeavePolicies.Edit(7);
      return View();
    }

    // GET: Leaves/CreateCompensatory
    public ActionResult CreateCompensatoryQuotaRequest()
    {
      //ViewBag.UserId = "d0c9d0b1-d0e8-4d56-a410-72e74af3ced8";
      ViewBag.LeaveTypeId = new SelectList(db.LeaveTypes, "Id", "Name",Consts.CompensatoryLeaveTypeId);
      ViewBag.CompensatoryLeaveTypeId = Consts.CompensatoryLeaveTypeId;
      string userId = User.Identity.GetUserId();
      int policyId= db.AspNetUsers.FirstOrDefault(x => x.Id == userId).UserLeavePolicyId.GetValueOrDefault();
      //bool found=false;
      //foreach (AspNetUser user in db.AspNetUsers.ToList<AspNetUser>())
      //{
      //  if (user.Id == userId)
      //  {
      //    policyId = user.UserLeavePolicyId;
      //    break;
      //  }
      //}

      // db.UserLeavePolicyDetails.Where(x => x.UserLeavePolicyId == policyId);
      //ViewBag.LeaveTypeId = new SelectList(db.LeaveTypes.Where(x=>x.UserLeavePolicyDetails.Where(y=>y.UserLeavePolicyId== policyId)), "Id", "Name");
      //ViewBag.UserLeavePolicyId = new SelectList(db.UserLeavePolicies, "Id", "UserId");
      if (policyId > 0)
      {
        ViewBag.UserLeavePolicyId = policyId;
      }
      else
      {
        ViewBag.PolicyAlert = "No Policy is implemented for your account, Contact Admin";
      }
      //List<AspNetUser> Seniors = GetSeniorStaff();
      //ViewBag.LineManagers = new SelectList(Seniors, "Id", "UserName");
      ViewBag.LineManagers = new SelectList(db.AspNetUsers, "Id", "UserName");
      ViewBag.UserName = User.Identity.Name;//"LoggedIn User";
      ViewBag.LeaveUserId =userId;
      //UserLeavePoliciesController UserLeavePolicies = new UserLeavePoliciesController();//.FileUploadMsgView("some string");
      //var result= UserLeavePolicies.Edit(7);
      //ViewBag.UserLeavePolicy= UserLeavePolicies.Edit(7);
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
    // POST: Leaves/Create
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Create([Bind(Include = "Id,UserId,LeaveTypeId,Reason,StartDate,EndDate,TotalDays,EmergencyContact,LineManager1Id,LineManager2Id")] Leave leave, string StartDate)
    {
      leave.UserId = User.Identity.GetUserId();
      leave.AspNetUser = db.AspNetUsers.FirstOrDefault(x => x.Id == leave.UserId);
      leave.DateCreated = DateTime.Now;
      leave.IsQuotaRequest = false;
      leave.LeaveType = db.LeaveTypes.FirstOrDefault(x => x.Id == leave.LeaveTypeId);
      if (ModelState.IsValid)
      {

        db.Leaves.Add(leave);
        /////////////////
        //LeaveBalance leaveBalance = CalculateLeaveBalance(ref leave);

        //if (leaveBalance == null)
        //{
        //  //new
        //  leaveBalance = new LeaveBalance(ref leave);
        //  leaveBalance.Taken = leave.TotalDays;
        //  leaveBalance.Balance -= leave.TotalDays;
        //  leaveBalance.UserId = leave.UserId;
        //  leaveBalance.LeaveTypeId = leave.LeaveTypeId;
        //  leaveBalance.UserLeavePolicyId = leave.AspNetUser.UserLeavePolicyId;
        //  db.LeaveBalances.Add(leaveBalance);
        //}
        //else
        //{
        //  //old
        //  leaveBalance.Taken += leave.TotalDays;
        //  leaveBalance.Balance -= leave.TotalDays;
        //  db.Entry(leaveBalance).State = EntityState.Modified;
        //}
        /////////////////
        await db.SaveChangesAsync();
        AspNetUser admin1 = db.AspNetUsers.FirstOrDefault(x => x.Id == leave.LineManager1Id);
        SendEmail.SendEmailUsingLeavON(leave, SendEmail.LeavON_Email, SendEmail.LeavON_Password, leave.AspNetUser, admin1, "LeaveRequest");
        AspNetUser admin2 = db.AspNetUsers.FirstOrDefault(x => x.Id == leave.LineManager2Id);
        SendEmail.SendEmailUsingLeavON(leave, SendEmail.LeavON_Email, SendEmail.LeavON_Password, leave.AspNetUser, admin2, "LeaveRequest");
        return RedirectToAction("Index");
      }

      ViewBag.LeaveTypeId = new SelectList(db.LeaveTypes, "Id", "Name", leave.LeaveTypeId);
      //ViewBag.UserLeavePolicyId = new SelectList(db.UserLeavePolicies, "Id", "UserId", leave.UserLeavePolicyId);


      var itmes = db.AspNetUsers.Include(x => x.AspNetRoles.Select(rl => rl.Name)).ToList();

      ViewBag.LineManagers = new SelectList(db.AspNetUsers, "Id", "UserName");
      return View(leave);
    }
    // POST: Leaves/Create
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> CreateCompensatoryQuotaRequest([Bind(Include = "Id,UserId,LeaveTypeId,Reason,StartDate,EndDate,TotalDays,EmergencyContact,LineManager1Id,LineManager2Id")] Leave leave, string StartDate,int CompensatoryLeaveTypeId)
    {
      leave.UserId = User.Identity.GetUserId();
      leave.AspNetUser = db.AspNetUsers.FirstOrDefault(x => x.Id == leave.UserId);
      leave.DateCreated = DateTime.Now;
      leave.LeaveTypeId= CompensatoryLeaveTypeId;
      leave.LeaveType = db.LeaveTypes.FirstOrDefault(x => x.Id == CompensatoryLeaveTypeId);
      leave.IsQuotaRequest = true;
      if (ModelState.IsValid)
      {

        db.Leaves.Add(leave);
        ///////////////
        //LeaveBalance leaveBalance = CalculateLeaveBalanceQuota(ref leave);

        //if (leaveBalance == null)
        //{
        //  //new
        //  leaveBalance = new LeaveBalance(ref leave);
          
        //  leaveBalance.UserId = leave.UserId;
        //  leaveBalance.LeaveTypeId = leave.LeaveTypeId;
        //  //leaveBalance.UserLeavePolicyId = leave.AspNetUser.UserLeavePolicyId;
        //  db.LeaveBalances.Add(leaveBalance);
        //}
        //else
        //{
        //  leaveBalance.Balance += leave.TotalDays;
        //  db.Entry(leaveBalance).State = EntityState.Modified;
        //}
        ///////////////
        
        await db.SaveChangesAsync();
        AspNetUser admin1 = db.AspNetUsers.FirstOrDefault(x => x.Id == leave.LineManager1Id);
        SendEmail.SendEmailUsingLeavON(leave, SendEmail.LeavON_Email, SendEmail.LeavON_Password, leave.AspNetUser, admin1, "LeaveRequest");
        AspNetUser admin2 = db.AspNetUsers.FirstOrDefault(x => x.Id == leave.LineManager2Id);
        SendEmail.SendEmailUsingLeavON(leave, SendEmail.LeavON_Email, SendEmail.LeavON_Password, leave.AspNetUser, admin2, "LeaveRequest");
        return RedirectToAction("QuotaRequestHistory");
      }

      ViewBag.LeaveTypeId = new SelectList(db.LeaveTypes, "Id", "Name", leave.LeaveTypeId);
      //ViewBag.UserLeavePolicyId = new SelectList(db.UserLeavePolicies, "Id", "UserId", leave.UserLeavePolicyId);


      var itmes = db.AspNetUsers.Include(x => x.AspNetRoles.Select(rl => rl.Name)).ToList();

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
      if (!(leave.IsAccepted1 == null || leave.IsAccepted2 == null))
      {
        return RedirectToAction("Index");
      }
        List<AspNetUser> Seniors = GetSeniorStaff();
      ViewBag.LineManagers = new SelectList(Seniors, "Id", "UserName");
      ViewBag.UserName = User.Identity.Name;//"LoggedIn User";
      //ViewBag.LeaveTypeId = new SelectList(db.LeaveTypes, "Id", "Name", leave.LeaveTypeId);

      string userId = User.Identity.GetUserId();
      int policyId = db.AspNetUsers.FirstOrDefault(x => x.Id == userId).UserLeavePolicyId.GetValueOrDefault();
      //List<UserLeavePolicyDetail> customLeaveTypes = db.UserLeavePolicyDetails.Where(x => x.UserLeavePolicyId == policyId).ToList<UserLeavePolicyDetail>();
      //ViewBag.LeaveTypeId = new SelectList(customLeaveTypes, "Id", "Name");
      ViewBag.LeaveTypeId = new SelectList(db.LeaveTypes.Where(x => x.UserLeavePolicyDetails.Any(y => y.UserLeavePolicyId == policyId) || x.Id==Consts.CompensatoryLeaveTypeId), "Id", "Name");
      if (policyId > 0)
      {
        ViewBag.UserLeavePolicyId = policyId;
      }
      else
      {
        ViewBag.PolicyAlert = "No Policy is implemented for your account, Contact Admin";
      }

      //ViewBag.UserLeavePolicyId = new SelectList(db.UserLeavePolicies, "Id", "UserId", leave.UserLeavePolicyId);
      return View(leave);
    }
    //GET
    public async Task<ActionResult> EditCompensatoryQuotaRequest(decimal id)
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
      if (!(leave.IsAccepted1 == null || leave.IsAccepted2 == null))
      {
        return RedirectToAction("Index");
      }
      List<AspNetUser> Seniors = GetSeniorStaff();
      ViewBag.LineManagers = new SelectList(Seniors, "Id", "UserName");
      ViewBag.UserName = User.Identity.Name;//"LoggedIn User";
      ViewBag.LeaveTypeId = new SelectList(db.LeaveTypes, "Id", "Name", leave.LeaveTypeId);
      //ViewBag.UserLeavePolicyId = new SelectList(db.UserLeavePolicies, "Id", "UserId", leave.UserLeavePolicyId);
      string userId = User.Identity.GetUserId();
      int policyId = db.AspNetUsers.FirstOrDefault(x => x.Id == userId).UserLeavePolicyId.GetValueOrDefault();
      return View(leave);
    }
    
    // POST: Leaves/Edit/5
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit([Bind(Include = "Id,UserId,LeaveTypeId,Reason,StartDate,EndDate,TotalDays,EmergencyContact,ResponseDate1,ResponseDate2,IsAccepted1,IsAccepted2,LineManager1Id,LineManager2Id,Remarks1,Remarks2,DateCreated,DateModified,UserLeavePolicyId")] Leave leave,decimal LastLeaveDaysCount)
    {
      
      leave.UserId = User.Identity.GetUserId();
      leave.AspNetUser = db.AspNetUsers.FirstOrDefault(x => x.Id == leave.UserId);
      leave.DateModified = DateTime.Now;
      if (ModelState.IsValid)
      {
        db.Entry(leave).State = EntityState.Modified;
        ///////////////
        LeaveBalance leaveBalance = CalculateLeaveBalance(ref leave);

        if (leaveBalance == null)
        {
          //new
          //leaveBalance = new LeaveBalance(ref leave);
          //leaveBalance.Taken = leave.TotalDays;
          //leaveBalance.Balance -= leave.TotalDays;
          //leaveBalance.UserId = leave.UserId;
          //leaveBalance.LeaveTypeId = leave.LeaveTypeId;
          //leaveBalance.UserLeavePolicyId = leave.AspNetUser.UserLeavePolicyId;
          //db.LeaveBalances.Add(leaveBalance);
        }
        else
        {
          //first reseting Balance to its correct state
          leaveBalance.Balance = leaveBalance.Balance + LastLeaveDaysCount;//leave.TotalDays;
          decimal Allowed = (decimal)leaveBalance.UserLeavePolicy.UserLeavePolicyDetails.FirstOrDefault(x => x.UserLeavePolicyId == leave.AspNetUser.UserLeavePolicyId && x.LeaveTypeId == leave.LeaveTypeId).Allowed;
          leaveBalance.Taken = Allowed- leaveBalance.Balance;
          /////////////
          //old
          leaveBalance.Taken += leave.TotalDays;
          leaveBalance.Balance -= leave.TotalDays;
          db.Entry(leaveBalance).State = EntityState.Modified;
        }
        ///////////////
        await db.SaveChangesAsync();
        return RedirectToAction("Index");
      }
      ViewBag.LeaveTypeId = new SelectList(db.LeaveTypes, "Id", "Name", leave.LeaveTypeId);
      //ViewBag.UserLeavePolicyId = new SelectList(db.UserLeavePolicies, "Id", "UserId", leave.UserLeavePolicyId);
      return View(leave);
    }

    // POST: Leaves/Edit/5
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> EditCompensatoryQuotaRequest([Bind(Include = "Id,UserId,LeaveTypeId,Reason,StartDate,EndDate,TotalDays,EmergencyContact,ResponseDate1,ResponseDate2,IsAccepted1,IsAccepted2,LineManager1Id,LineManager2Id,Remarks1,Remarks2,DateCreated,DateModified,UserLeavePolicyId")] Leave leave, decimal LastLeaveDaysCount)
    {

      leave.UserId = User.Identity.GetUserId();
      leave.AspNetUser = db.AspNetUsers.FirstOrDefault(x => x.Id == leave.UserId);
      leave.DateModified = DateTime.Now;
      leave.IsQuotaRequest = true;
      if (ModelState.IsValid)
      {
        db.Entry(leave).State = EntityState.Modified;
        ///////////////
        LeaveBalance leaveBalance = CalculateLeaveBalanceQuota(ref leave);

                     
        if (leaveBalance == null)
        {
          //new
          //leaveBalance = new LeaveBalance(ref leave);
          //leaveBalance.Taken = leave.TotalDays;
          //leaveBalance.Balance -= leave.TotalDays;
          //leaveBalance.UserId = leave.UserId;
          //leaveBalance.LeaveTypeId = leave.LeaveTypeId;
          //leaveBalance.UserLeavePolicyId = leave.AspNetUser.UserLeavePolicyId;
          //db.LeaveBalances.Add(leaveBalance);
        }
        else
        {
          //first reseting Balance to its correct state
          leaveBalance.Balance -=  LastLeaveDaysCount;//leave.TotalDays;
          leaveBalance.Balance += leave.TotalDays;
          db.Entry(leaveBalance).State = EntityState.Modified;
        }
        ///////////////
        await db.SaveChangesAsync();
        return RedirectToAction("QuotaRequestHistory");
      }
      ViewBag.LeaveTypeId = new SelectList(db.LeaveTypes, "Id", "Name", leave.LeaveTypeId);
      //ViewBag.UserLeavePolicyId = new SelectList(db.UserLeavePolicies, "Id", "UserId", leave.UserLeavePolicyId);
      return View(leave);
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
