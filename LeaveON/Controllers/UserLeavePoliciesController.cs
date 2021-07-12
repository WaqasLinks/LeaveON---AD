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
using System.IO;
using LeaveON.Models;
using Microsoft.AspNet.Identity;

namespace LeaveON.Controllers
{


  public class UserLeavePoliciesController : Controller
  {
    private LeaveONEntities db = new LeaveONEntities();

    // GET: UserLeavePolicies
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Index()
    {
      //var userLeavePolicies = db.UserLeavePolicies.Include(u => u.AspNetUser);
      var userLeavePolicies = db.UserLeavePolicies;
      return View(await userLeavePolicies.ToListAsync());
    }

    // GET: UserLeavePolicies/Details/5
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Details(decimal id)
    {
      if (id == null)
      {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }
      UserLeavePolicy userLeavePolicy = await db.UserLeavePolicies.FindAsync(id);
      if (userLeavePolicy == null)
      {
        return HttpNotFound();
      }
      return View(userLeavePolicy);
    }
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public ActionResult AddNewRow(string IndexId)
    {
      ViewBag.LeaveTypes = new SelectList(db.LeaveTypes, "Id", "Name");
      ViewBag.LockAndHide = "False";
      return PartialView("_newRow", IndexId);
    }
    // GET: UserLeavePolicies/Create
    [Authorize(Roles = "Admin")]
    public ActionResult Create()
    {
      ViewBag.Employees = new SelectList(db.AspNetUsers.OrderBy(i => i.UserName), "Id", "UserName");
      //ViewBag.LeaveTypes = new SelectList(db.LeaveTypes, "Id", "Name");
      ViewBag.Departments = new SelectList(db.CountryNames, "Name", "Name");
      List<SelectListItem> AgreementTypes = new List<SelectListItem>()
      {
          new SelectListItem{Text = "Fiscal year", Value = "1"},
          new SelectListItem{Text = "Calender year", Value = "2"},
          new SelectListItem{Text = "Contract year", Value = "3"},
      };
      ViewBag.AgreementTypes = AgreementTypes;

      UserLeavePolicyViewModel userLeavePolicyViewModel = new UserLeavePolicyViewModel();
      return View(userLeavePolicyViewModel);
      //return View();
    }

    // POST: UserLeavePolicies/Create
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Create([Bind(Prefix = "UserLeavePolicy", Include = "Id,UserId,Description,WeeklyOffDays,AgreementType,FiscalYearStart,FiscalYearEnd,FiscalYearPeriod,Remarks")] UserLeavePolicy userLeavePolicy,
      [Bind(Prefix = "UserLeavePolicyDetail", Include = "LeaveTypeId,Allowed")] List<UserLeavePolicyDetail> userLeavePolicyDetail,
      [Bind(Prefix = "AnnualOffDay", Include = "OffDay,Description")] List<AnnualOffDay> AnnualOffDays, string[] Departments, string[] Employees, string PolicyFor)
    {
      int maxId = db.UserLeavePolicies.DefaultIfEmpty().Max(p => p == null ? 0 : p.Id);
      userLeavePolicy.WeeklyOffDays = userLeavePolicy.WeeklyOffDays;//"6,0";
      userLeavePolicy.Id = maxId;
      userLeavePolicy.CountryId = 5;//from which user is Login. but admin who can view all coutries there we have to user a list of country so that he choose a country
      //userLeavePolicy.AnnualOffDays = string.Join(",", AnnualOffDays);
      userLeavePolicy.DepartmentPolicy = (PolicyFor == "1") ? true : false;

      //foreach (AnnualOffDay itm in AnnualOffDays)
      //{
      //  itm.UserLeavePolicyId = userLeavePolicy.Id;
      //}

      userLeavePolicy.AnnualOffDays = AnnualOffDays;

      foreach (UserLeavePolicyDetail item in userLeavePolicyDetail.ToList<UserLeavePolicyDetail>())
      {
        if (item.Allowed == null)
        {
          //userLeavePolicyDetail.Remove(item);
          item.Allowed = 0;
        }
      }
      userLeavePolicy.UserLeavePolicyDetails = userLeavePolicyDetail;



      if (ModelState.IsValid)
      {
        db.UserLeavePolicies.Add(userLeavePolicy);
        if (PolicyFor == "1")//department
        {
          //int[] DepartmentsList = Array.ConvertAll(Departments, int.Parse);
          
          CountryName dep;
          foreach (string itm in Departments)
          {
            dep = db.CountryNames.FirstOrDefault(x => x.Name == itm);
            foreach (AspNetUser user in dep.AspNetUsers)
            {
              user.UserLeavePolicyId = userLeavePolicy.Id;
              db.AspNetUsers.Attach(user);
              db.Entry(user).Property(x => x.UserLeavePolicyId).IsModified = true;
            }
          }
        }
        else
        {
          AspNetUser user;
          foreach (string empId in Employees)
          {
            user = db.AspNetUsers.FirstOrDefault(x => x.Id == empId);
            user.UserLeavePolicyId = userLeavePolicy.Id;
            db.AspNetUsers.Attach(user);
            db.Entry(user).Property(x => x.UserLeavePolicyId).IsModified = true;

          }
        }
        await db.SaveChangesAsync();
        return RedirectToAction("Index");
      }

      //ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Hometown", userLeavePolicy.UserId);
      return View(userLeavePolicy);
    }

    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<ActionResult> Create(UserLeavePolicyViewModel userLeavePolicyViewModel)
    //{
    //  if (ModelState.IsValid)
    //  {
    //    db.UserLeavePolicies.Add(userLeavePolicyViewModel.userLeavePolicy);
    //    await db.SaveChangesAsync();
    //    return RedirectToAction("Index");
    //  }

    //  ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Hometown", userLeavePolicyViewModel.userLeavePolicy.UserId);
    //  return View(userLeavePolicyViewModel.userLeavePolicy);
    //}

    // GET: UserLeavePolicies/Edit/5
    [Authorize(Roles = "Admin,Manager,User")]
    public async Task<ActionResult> Edit(decimal id, string Caller, string leaveUserId = "UserLeavePolicy")
    {
      //string userId = User.Identity.GetUserId();
      
      //id =(decimal)db.AspNetUsers.FirstOrDefault(x => x.Id == userId).UserLeavePolicyId;
      
      //if (id == null)
      //{
      //  return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      //}
      ViewBag.Employees = new SelectList(db.AspNetUsers.OrderBy(i=>i.UserName), "Id", "UserName");
      ViewBag.LeaveTypes = new SelectList(db.LeaveTypes, "Id", "Name");
      //always remember viewbag name should not be as model name. other wise probelm. if same multilist will not show selected values
      ViewBag.Departments = new SelectList(db.CountryNames, "Name", "Name");

      UserLeavePolicyViewModel userLeavePolicyViewModel = new UserLeavePolicyViewModel();

      UserLeavePolicy userLeavePolicy = await db.UserLeavePolicies.FindAsync(id);

      userLeavePolicyViewModel.userLeavePolicy = userLeavePolicy;
      userLeavePolicyViewModel.userLeavePolicyDetail = userLeavePolicy.UserLeavePolicyDetails.AsQueryable<UserLeavePolicyDetail>();
      userLeavePolicyViewModel.countries = db.CountryNames;//.Where(x => x.CountryId == 1).AsQueryable<Department>();//TODO Convert 1 to current user country variable
                                                                                                                   //userLeavePolicyViewModel.departments= depFilterd;
      userLeavePolicyViewModel.annualOffDays = db.AnnualOffDays.Where(x => x.UserLeavePolicyId == userLeavePolicy.Id).AsQueryable();

      IQueryable<AspNetUser> usersFilterd = db.AspNetUsers.Where(x => x.UserLeavePolicyId == id);


      //foreach (AspNetUser usr in usersFilterd)
      //{
      //  //depFilterd = db.Departments.Where(x => x.Id == usr.DepartmentId).Distinct<Department>().ToList<Department>();
      //}
      //List<int> SelectedDeps = new List<int>(new int[] { 1,2 });
      List<string> SelectedDeps = new List<string>();
      List<string> SelectedEmps = new List<string>();
      SelectedDeps = usersFilterd.Select(p => p.CntryName).Distinct<string>().ToList<string>();
      SelectedEmps = usersFilterd.Select(p => p.Id).Distinct<string>().ToList<string>();

      ViewBag.SelectedDepartments = SelectedDeps;
      ViewBag.SelectedEmployees = SelectedEmps;

      if (userLeavePolicy.DepartmentPolicy == true)
      {
        ViewBag.DepStatus = true;
        ViewBag.EmpStatus = false;
      }
      else
      {
        ViewBag.DepStatus = false;
        ViewBag.EmpStatus = true;
      }


      List<SelectListItem> WeekSelectList = new List<SelectListItem>()
      {

          new SelectListItem{Text = "Saturday", Value = "6"},
          new SelectListItem{Text = "Sunday", Value = "0"},
          new SelectListItem{Text = "Monday", Value = "1"},
          new SelectListItem{Text = "Tuesday", Value = "2"},
          new SelectListItem{Text = "Wednesday", Value = "3"},
          new SelectListItem{Text = "Thursday", Value = "4"},
          new SelectListItem{Text = "Friday", Value = "5"}
      };

      ViewBag.WeeklyOffDays = WeekSelectList;

      List<SelectListItem> AgreementTypes = new List<SelectListItem>()
      {
          new SelectListItem{Text = "Fiscal year", Value = "1"},
          new SelectListItem{Text = "Calender year", Value = "2"},
          new SelectListItem{Text = "Contract year", Value = "3"},
      };

      ViewBag.AgreementTypes = AgreementTypes;




      List<string> DaysSelected = new List<string>();
      foreach (string day in userLeavePolicy.WeeklyOffDays.Split(','))
      {
        //int intDay = int.Parse(day);
        DaysSelected.Add(day);
        //DaysSelected.Add()
      }
      ViewBag.DaysSelected = DaysSelected;
      //List<AnnualOffDay> AnnualOffDaysList = new List<AnnualOffDay>();
      //int cntr = 0;

      //foreach (string day in userLeavePolicy.AnnualOffDays.Split(','))
      //{
      //  cntr += 1;
      //  AnnualOffDaysList.Add(new AnnualOffDay { Id = cntr, OffDay = day, Description = "" });
      //}

      //ViewBag.AnnualLeaves = userLeavePolicy.AnnualOffDays;//AnnualOffDaysList;

      if (userLeavePolicy == null)
      {
        return HttpNotFound();
      }
      //ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Hometown", userLeavePolicy.UserId);
      //return View(userLeavePolicy);



      //return PartialView("_newRow", IndexId); //for ref only

      if (Caller == "UserLeavePolicy")
      {
        ViewBag.LockAndHide = "False";
        return View(userLeavePolicyViewModel); //orginal
      }
      else
      {
        //string CurrentLoginUserId = User.Identity.GetUserId();
        ViewBag.LeaveUserId = leaveUserId;
        ViewBag.CompensatoryLeaveBalance = db.LeaveBalances.FirstOrDefault(x => x.LeaveTypeId == LMS.Constants.Consts.CompensatoryLeaveTypeId && x.UserId == leaveUserId);

        ViewBag.LockAndHide = "True";
        return PartialView("_Edit", userLeavePolicyViewModel);
      }

      //switch (Caller)
      //{
      //  case "LeaveRequest"://Logged In User Id will be sent
      //    ViewBag.CompensatoryLeaveBalance = db.LeaveBalances.FirstOrDefault(x => x.LeaveTypeId == LMS.Constants.Consts.CompensatoryLeaveTypeId && x.UserId == leaveUserId);
      //    ViewBag.LockAndHide = "True";
      //    return PartialView("_Edit", userLeavePolicyViewModel);
      //    //break;
      //  case "LeaveResponse"://Id of Leave Requested user will be sent
      //    string CurrentLoginUserId = User.Identity.GetUserId();
      //    ViewBag.CurrentLoginUserId = CurrentLoginUserId;
      //    ViewBag.CompensatoryLeaveBalance = db.LeaveBalances.FirstOrDefault(x => x.LeaveTypeId == LMS.Constants.Consts.CompensatoryLeaveTypeId && x.UserId == CurrentLoginUserId);
      //    ViewBag.LockAndHide = "True";
      //    return PartialView("_Edit", userLeavePolicyViewModel);
      //    break;
      //  default://user leave policy screen
      //    ViewBag.LockAndHide = "False";
      //    return View(userLeavePolicyViewModel); //orginal
      //    //break;
      //}

    }

    // POST: UserLeavePolicies/Edit/5
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit([Bind(Prefix = "UserLeavePolicy", Include = "Id,UserId,Description,WeeklyOffDays,AgreementType,FiscalYearStart,FiscalYearEnd,FiscalYearPeriod,Remarks")] UserLeavePolicy userLeavePolicy,
      [Bind(Prefix = "UserLeavePolicyDetail", Include = "LeaveTypeId,Allowed")] List<UserLeavePolicyDetail> userLeavePolicyDetail, [Bind(Prefix = "AnnualOffDay", Include = "Id,OffDay,Description")] List<AnnualOffDay> AnnualOffDays, List<string> DepartmentList, List<string> EmployeeList, string PolicyFor)
    {

      //UserLeavePolicy userLeavePolicyOld = await db.UserLeavePolicies.FindAsync(userLeavePolicy.Id);
      //db.UserLeavePolicyDetails.RemoveRange(userLeavePolicyOld.UserLeavePolicyDetails);
      //await db.SaveChangesAsync();
      userLeavePolicy.WeeklyOffDays = userLeavePolicy.WeeklyOffDays;//"6,0";
      userLeavePolicy.CountryId = 5;//from which user is Login. but admin who can view all coutries there we have to user a list of country so that he choose a country
      //userLeavePolicy.AnnualOffDays = string.Join(",", AnnualOffDays);
      userLeavePolicy.DepartmentPolicy = (PolicyFor == "1") ? true : false;
      foreach (UserLeavePolicyDetail item in userLeavePolicyDetail.ToList<UserLeavePolicyDetail>())
      {
        if (item.Allowed == null)
        {
          //userLeavePolicyDetail.Remove(item);
          item.Allowed = 0;
          
        }
        item.UserLeavePolicyId = userLeavePolicy.Id;
      }

      if (ModelState.IsValid)
      {
        if (PolicyFor == "1")//department
        {
          CountryName dep;
          IQueryable<AspNetUser> usersFilterd = db.AspNetUsers.Where(x => x.UserLeavePolicyId == userLeavePolicy.Id);
          List<string> oldDeps = new List<string>();
          oldDeps = usersFilterd.Select(p => p.CntryName).Distinct<string>().ToList<string>();

          foreach (string itm in oldDeps)//get all employees of the deps, selected from UI
          {
            dep = db.CountryNames.FirstOrDefault(x => x.Name == itm);
            foreach (AspNetUser aspNetUser in dep.AspNetUsers)
            {
              //EmployeeList.Add(aspNetUser.Id);
              aspNetUser.UserLeavePolicyId = null;
              db.AspNetUsers.Attach(aspNetUser);
              db.Entry(aspNetUser).Property(x => x.UserLeavePolicyId).IsModified = true;
            }
          }


          EmployeeList = new List<string>();
          foreach (string itm in DepartmentList)//get all employees of the deps, selected from UI
          {
            dep = db.CountryNames.FirstOrDefault(x => x.Name == itm);
            foreach (AspNetUser aspNetUser in dep.AspNetUsers)
            {
              //EmployeeList.Add(aspNetUser.Id);
              aspNetUser.UserLeavePolicyId = userLeavePolicy.Id;
              db.AspNetUsers.Attach(aspNetUser);
              db.Entry(aspNetUser).Property(x => x.UserLeavePolicyId).IsModified = true;
            }
          }
          await db.SaveChangesAsync();
        }
        else
        {
          AspNetUser user;

          IQueryable<AspNetUser> oldUsers = db.AspNetUsers.Where(x => x.UserLeavePolicyId == userLeavePolicy.Id);
          //List<int> oldDeps = new List<int>();
          //oldDeps = usersFilterd.Select(p => p.CountryId).Distinct<int>().ToList<int>();

          //foreach (int itm in oldDeps)//get all employees of the deps, selected from UI
          //{
          //  dep = db.Countries.FirstOrDefault(x => x.Id == itm);
          if (oldUsers != null && oldUsers.Count() > 0)
          { 
            foreach (AspNetUser aspNetUser in oldUsers)
            {
              //EmployeeList.Add(aspNetUser.Id);
              aspNetUser.UserLeavePolicyId = null;
              db.AspNetUsers.Attach(aspNetUser);
              db.Entry(aspNetUser).Property(x => x.UserLeavePolicyId).IsModified = true;
            }
          }
          //}
          if (EmployeeList != null && EmployeeList.Count() > 0)
          {
            foreach (string userId in EmployeeList)
            {
              user = db.AspNetUsers.FirstOrDefault(x => x.Id == userId);
              user.UserLeavePolicyId = userLeavePolicy.Id;
              db.AspNetUsers.Attach(user);
              db.Entry(user).Property(x => x.UserLeavePolicyId).IsModified = true;
            }
          }
          await db.SaveChangesAsync();
        }


        ///////////////To delete old leve policy detils. if we dont.... new lines will be added with old ones///////////
        IQueryable<UserLeavePolicyDetail> oldLPD = db.UserLeavePolicyDetails.AsQueryable().Where(x => x.UserLeavePolicyId == userLeavePolicy.Id);
        db.UserLeavePolicyDetails.RemoveRange(oldLPD);
        ///////////////
        db.Entry(userLeavePolicy).State = EntityState.Modified;
        db.UserLeavePolicyDetails.AddRange(userLeavePolicyDetail);

        await db.SaveChangesAsync();
        return RedirectToAction("Index");
      }
      //ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Hometown", userLeavePolicy.UserId);
      return View(userLeavePolicy);
    }
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public ActionResult UploadFiles()
    {
      if (Request.Files.Count > 0)
      {
        try
        {
          HttpFileCollectionBase postedFiles = Request.Files;
          HttpPostedFileBase postedFile = postedFiles[0];
          List<AnnualOffDay> annualLeaves = new List<AnnualOffDay>();
          string filePath = string.Empty;
          if (postedFile != null)
          {
            string path = Server.MapPath("~/Uploads/");
            if (!Directory.Exists(path))
            {
              Directory.CreateDirectory(path);
            }

            filePath = path + Path.GetFileName(postedFile.FileName);
            string extension = Path.GetExtension(postedFile.FileName);
            postedFile.SaveAs(filePath);

            //Read the contents of CSV file.
            string csvData = System.IO.File.ReadAllText(filePath);
            int cntr = 0;

            //Execute a loop over the rows.
            foreach (string row in csvData.Split('\n'))
            {
              cntr += 1;
              if (!string.IsNullOrEmpty(row) && cntr > 1)
              {
                annualLeaves.Add(new AnnualOffDay
                {
                  //Id = Convert.ToInt32(row.Split(',')[0]),
                  OffDay = DateTime.Parse(row.Split(',')[0]),
                  Description = row.Split(',')[1]
                });
              }
            }
          }


          //for (int i = 0; i < files.Count; i++)
          //{
          //  string path = AppDomain.CurrentDomain.BaseDirectory + "Uploads/";
          //  string filename = Path.GetFileName(Request.Files[i].FileName);

          //  HttpPostedFileBase file = files[i];
          //  string fname;
          //  if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
          //  {
          //    string[] testfiles = file.FileName.Split(new char[] { '\\' });
          //    fname = testfiles[testfiles.Length - 1];
          //  }
          //  else
          //  {
          //    fname = file.FileName;
          //  }

          //  fname = Path.Combine(Server.MapPath("~/Uploads/"), fname);
          //  file.SaveAs(fname);
          //}

          //return Json("File Uploaded Successfully!");
          //ViewBag.AnnualLeaves = annualLeaves.OrderBy(i => i.Id).ToList();
          return PartialView("_DisplayAnnualLeaves", annualLeaves);
          //return PartialView("_DisplayAnnualLeaves");
        }
        catch (Exception ex)
        {
          return Json("Error occurred. Error details: " + ex.Message);
        }
      }
      else
      {
        return Json("No files selected.");
      }
    }

    // GET: UserLeavePolicies/Delete/5
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(decimal id)
    {
      if (id == null)
      {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }
      UserLeavePolicy userLeavePolicy = await db.UserLeavePolicies.FindAsync(id);
      if (userLeavePolicy == null)
      {
        return HttpNotFound();
      }
      return View(userLeavePolicy);
    }

    // POST: UserLeavePolicies/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> DeleteConfirmed(decimal id)
    {
      UserLeavePolicy userLeavePolicy = await db.UserLeavePolicies.FindAsync(id);
      db.UserLeavePolicies.Remove(userLeavePolicy);
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
