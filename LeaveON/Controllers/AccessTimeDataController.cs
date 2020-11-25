using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TimeManagement.Models;
using Repository.Models;
using Microsoft.AspNet.Identity;
using System.Security.Claims;
using System.Globalization;

namespace LeaveON.Controllers
{
  public class AccessTimeDataController : Controller
  {
    private BioStarEntities dbBioStar = new BioStarEntities();
    LeaveONEntities dbLeaveOn = new LeaveONEntities();
    // GET: AccessTimeData
    public async Task<ActionResult> PersonalData(string ReqMonthYear)
    {
      //DateTime myDate = DateTime.ParseExact("2009-05-08 14:40:52,531", "yyyy-MM-dd HH:mm:ss,fff",
      //                                 System.Globalization.CultureInfo.InvariantCulture);
      DateTime reqDate = DateTime.Now;
      if (!string.IsNullOrEmpty(ReqMonthYear))
      {
        reqDate = DateTime.ParseExact(ReqMonthYear, "MM-yyyy",
                                 System.Globalization.CultureInfo.CurrentCulture);
      }
      //reqDate = reqDate.AddYears(-1);
      string userId = User.Identity.GetUserId();
      int bioStarEmpNum = dbLeaveOn.AspNetUsers.FirstOrDefault(x => x.Id == userId).BioStarEmpNum.Value;

      IQueryable<UD_TB_AccessTime_Data> topRows = dbBioStar.UD_TB_AccessTime_Data.Where(x => x.EmployeeNumber == bioStarEmpNum && ((x.Date_IN.Value.Month == reqDate.Month && x.Date_IN.Value.Year == reqDate.Year) ||
                                                                                 x.Date_OUT.Value.Month == reqDate.Month && x.Date_OUT.Value.Year == reqDate.Year)).AsQueryable<UD_TB_AccessTime_Data>();
      List<UD_TB_AccessTime_Data> LsttopRows = topRows.ToList<UD_TB_AccessTime_Data>();
      //return View(await db.UD_TB_AccessTime_Data.ToListAsync());
      if (string.IsNullOrEmpty(ReqMonthYear))
      {
        return View(await topRows.ToListAsync());
      }
      else
      {
        return PartialView("_PersonalData", await topRows.OrderBy(i => i.Date_IN).ToListAsync());
      }

    }

    public async Task<ActionResult> DepartmentData(string ReqMonthYear, string DepartmentId)
    {



      //DateTime myDate = DateTime.ParseExact("2009-05-08 14:40:52,531", "yyyy-MM-dd HH:mm:ss,fff",
      //                                 System.Globalization.CultureInfo.InvariantCulture);
      ViewBag.MonthSelectList = GetMonthSelectList();
      DateTime reqDate;
      int intDepartmentId;
      if (!string.IsNullOrEmpty(ReqMonthYear))
      {

        intDepartmentId = int.Parse(DepartmentId);
        //user.DepartmentId;//User.Identity.GetUserId();//
        reqDate = DateTime.ParseExact(ReqMonthYear, "MM-yyyy",
                                 System.Globalization.CultureInfo.CurrentCulture);

      }
      else
      {
        //in case of empty parameters or First Time

        reqDate = DateTime.Now;
        string userId = User.Identity.GetUserId();
        intDepartmentId = dbLeaveOn.AspNetUsers.FirstOrDefault(x => x.Id == userId).DepartmentId.Value;
        List<string> SelectedDeps = new List<string>();
        SelectedDeps.Add(intDepartmentId.ToString());
        ViewBag.SelectedDepartments = SelectedDeps;
        ViewBag.Departments = new SelectList(dbLeaveOn.Departments, "Id", "Name");

      }
      IQueryable<UD_TB_AccessTime_Data> depData = null;
      if (!string.IsNullOrEmpty(ReqMonthYear))
      {
        var identity = (ClaimsIdentity)User.Identity;
        IEnumerable<Claim> claims = identity.Claims;
        Claim claim = claims.Where(x => x.Value == DepartmentId).FirstOrDefault();

        if (claim is null) return null;


        //reqDate = reqDate.AddYears(-1);
        //string userId = User.Identity.GetUserId();

        //int bioStarEmpNum = dbLeaveOn.AspNetUsers.FirstOrDefault(x => x.Id == userId).BioStarEmpNum.Value;

        //IQueryable<UD_TB_AccessTime_Data> allUsersData = null;
        List<AspNetUser> users = dbLeaveOn.AspNetUsers.Where(x => x.DepartmentId == intDepartmentId).ToList<AspNetUser>();

        List<int> userIds = users.Select(x => x.BioStarEmpNum.Value).ToList<int>();
        //foreach (AspNetUser user in users)
        //{

        depData = dbBioStar.UD_TB_AccessTime_Data.Where(x => userIds.Contains(x.EmployeeNumber.Value) && ((x.Date_IN.Value.Month == reqDate.Month && x.Date_IN.Value.Year == reqDate.Year) ||
                                                                                   x.Date_OUT.Value.Month == reqDate.Month && x.Date_OUT.Value.Year == reqDate.Year)).AsQueryable<UD_TB_AccessTime_Data>();

        //}
      }
      //return View(await db.UD_TB_AccessTime_Data.ToListAsync());
      if (string.IsNullOrEmpty(ReqMonthYear))
      {
        //in case of null param or first time
        if (!(depData is null))
        {
          return View(await depData.OrderBy(i => i.Date_IN).ToListAsync());
        }
        else
        {
          return View();
        }

      }
      else
      {
        return PartialView("_DepartmentData", await depData.OrderBy(i => i.Date_IN).ToListAsync());
      }

    }
    public List<SelectListItem> GetMonthSelectList()
    {
      int thisMonth = DateTime.Now.Month;
      //int monthCtr = thisMonth;
      int thisYear = DateTime.Now.Year;

      List<SelectListItem> monthSelectList = new List<SelectListItem>();
      for (int i = 1; i <= 13; i++)
      {
        if (thisMonth < 1)
        {
          thisMonth = 12;
          thisYear -= 1;
        }
        SelectListItem newItem = new SelectListItem();
        newItem.Value = thisMonth.ToString("00") + "-" + thisYear;
        newItem.Text = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(thisMonth) + " " + thisYear;
        thisMonth -= 1;
        monthSelectList.Add(newItem);
      }
      return monthSelectList;
    }
    public async Task<ActionResult> UserData(string ReqMonthYear, string UserId)
    {
      //DateTime myDate = DateTime.ParseExact("2009-05-08 14:40:52,531", "yyyy-MM-dd HH:mm:ss,fff",
      //                                 System.Globalization.CultureInfo.InvariantCulture);

    
      ViewBag.MonthSelectList = GetMonthSelectList();
      //ViewBag.SelectedMonth = monthSelectList[0];
      DateTime reqDate;
      decimal dEmpNum;
      if (!string.IsNullOrEmpty(ReqMonthYear))
      {

        dEmpNum = decimal.Parse(UserId);
        //user.DepartmentId;//User.Identity.GetUserId();//
        reqDate = DateTime.ParseExact(ReqMonthYear, "MM-yyyy",
                                 System.Globalization.CultureInfo.CurrentCulture);

      }
      else
      {
        //in case of empty parameters or First Time

        reqDate = DateTime.Now;
        string userId = User.Identity.GetUserId();
        dEmpNum = (decimal)dbLeaveOn.AspNetUsers.FirstOrDefault(x => x.Id == userId).BioStarEmpNum;
        //dUserId = decimal.Parse(userId);
        //dUserId = (decimal)dbBioStar.UD_TB_AD_USER.FirstOrDefault(x => x.EmployeeNumber == dUserId).EmployeeNumber;

        List<string> SelectedEmps = new List<string>();
        SelectedEmps.Add(dEmpNum.ToString());
        ViewBag.SelectedEmployees = SelectedEmps;
        //ViewBag.Employees = new SelectList(dbBioStar.UD_TB_AD_USER, "EmployeeNumber", "EmployeeName");
        ViewBag.Employees = new SelectList(dbLeaveOn.AspNetUsers, "BioStarEmpNum", "UserName").OrderBy(i => i.Text);

      }
      IQueryable<UD_TB_AccessTime_Data> empData = null;
      List<UD_TB_AccessTime_Data> LstEmpData = null;
      if (!string.IsNullOrEmpty(ReqMonthYear))
      {
        //var identity = (ClaimsIdentity)User.Identity;
        //IEnumerable<Claim> claims = identity.Claims;
        //Claim claim = claims.Where(x => x.Value == DepartmentId).FirstOrDefault();

        //if (claim is null) return null;


        //reqDate = reqDate.AddYears(-1);
        //string userId = User.Identity.GetUserId();

        //int bioStarEmpNum = dbLeaveOn.AspNetUsers.FirstOrDefault(x => x.Id == userId).BioStarEmpNum.Value;

        //IQueryable<UD_TB_AccessTime_Data> allUsersData = null;
        List<AspNetUser> users = dbLeaveOn.AspNetUsers.Where(x => x.BioStarEmpNum == dEmpNum).ToList<AspNetUser>();
        string GuidUserId = users[0].Id;
        List<int> userIds = users.Select(x => x.BioStarEmpNum.Value).ToList<int>();
        //foreach (AspNetUser user in users)
        //{

        empData = dbBioStar.UD_TB_AccessTime_Data.Where(x => userIds.Contains(x.EmployeeNumber.Value) && ((x.Date_IN.Value.Month == reqDate.Month && x.Date_IN.Value.Year == reqDate.Year) ||
                                                                                   x.Date_OUT.Value.Month == reqDate.Month && x.Date_OUT.Value.Year == reqDate.Year)).AsQueryable<UD_TB_AccessTime_Data>();
        //var groupByData = empData.ToList().GroupBy(x => x.Date_IN.Value.Day);
        //}
        LstEmpData = await empData.ToListAsync<UD_TB_AccessTime_Data>();

        //get leaves days

        int LastDayOfMonth = DateTime.DaysInMonth(reqDate.Year, reqDate.Month);
        //IQueryable<Leave> thisMonthsLeaves = dbLeaveOn.Leaves.Where(x => x.StartDate.Day >= 1 && x.EndDate.Day <= LastDayOfMonth && x.StartDate.Year == reqDate.Year && x.UserId == users[0].Id).AsQueryable<Leave>();
        //IQueryable<Leave> thisMonthsLeaves = dbLeaveOn.Leaves.Where(x => x.StartDate.Day >= 1 && x.EndDate.Day <= LastDayOfMonth && x.StartDate.Month == reqDate.Month && x.StartDate.Year == reqDate.Year && x.UserId == users[0].Id).AsQueryable<Leave>();
        List<Leave> thisMonthsLeaves = dbLeaveOn.Leaves.Where(x => x.StartDate.Day >= 1 && x.EndDate.Day <= LastDayOfMonth && x.StartDate.Month == reqDate.Month && x.StartDate.Year == reqDate.Year && x.UserId == GuidUserId).ToList<Leave>();
        //List<Leave> thisMonthsLeaves = dbLeaveOn.Leaves.Where(x => x.UserId == UserGuidId).ToList<Leave>();
        List<UD_TB_AccessTime_Data> leaveDays = new List<UD_TB_AccessTime_Data>();

        string empName = users[0].UserName;
        string biostarEmpName = LstEmpData[0].EmployeeName;
        int iEmpNum = users[0].BioStarEmpNum.Value;
        int UserLeavePolicyId = users[0].UserLeavePolicyId.Value;
        foreach (Leave leave in thisMonthsLeaves)
        {
          for (int i = 0; i < leave.TotalDays; i++)
          {
            UD_TB_AccessTime_Data accessTime_Data = new UD_TB_AccessTime_Data
            {
              EmployeeName = biostarEmpName,
              EmployeeNumber = leave.AspNetUser.BioStarEmpNum,
              Date_IN = leave.StartDate.AddDays(i),
              Day_IN = leave.StartDate.AddDays(i).ToString("dddd"),
              Status = leave.Reason
            };
            leaveDays.Add(accessTime_Data);
          }
        }

        //get natioanl off days
        foreach (AnnualOffDay annualOffDay in dbLeaveOn.AnnualOffDays.Where(x => x.OffDay.Value.Month == reqDate.Month && x.OffDay.Value.Year == reqDate.Year && x.UserLeavePolicyId == UserLeavePolicyId).ToList<AnnualOffDay>())
        {
          UD_TB_AccessTime_Data accessTime_Data = new UD_TB_AccessTime_Data
          {
            EmployeeName = biostarEmpName,
            EmployeeNumber = iEmpNum,
            Date_IN = annualOffDay.OffDay,
            Day_IN = annualOffDay.OffDay.Value.ToString("dddd"),
            Status = annualOffDay.Description
          };
          //dayCntr += 1;
          leaveDays.Add(accessTime_Data);
        }
        LstEmpData.AddRange(leaveDays);
      }
      //return View(await db.UD_TB_AccessTime_Data.ToListAsync());
      if (string.IsNullOrEmpty(ReqMonthYear))
      {
        //in case of null param or first time
        if (!(LstEmpData is null))
        {
          return View(LstEmpData.OrderBy(i => i.Date_IN).ToList());

        }
        else
        {
          return View();
        }
      }
      else
      {
        return PartialView("_UserData", LstEmpData.OrderBy(i => i.Date_IN).ToList());
      }

    }

    public async Task<ActionResult> UserDataDetail(int EmployeeNumber, DateTime DateIn, DateTime DateOut)
    {

      //DateTime myDate = DateTime.ParseExact("2009-05-08 14:40:52,531", "yyyy-MM-dd HH:mm:ss,fff",
      //                                 System.Globalization.CultureInfo.InvariantCulture);

      //reqDate = DateTime.ParseExact(ReqMonthYear, "MM-yyyy",
      //                         System.Globalization.CultureInfo.CurrentCulture);

      IQueryable<UD_TB_AccessTime_Data> empData = null;

      empData = dbBioStar.UD_TB_AccessTime_Data.Where(x => x.EmployeeNumber == EmployeeNumber && ((x.Date_IN.Value.Day == DateIn.Day && x.Date_IN.Value.Month == DateIn.Month && x.Date_IN.Value.Year == DateIn.Year) ||
                                                                                 x.Date_OUT.Value.Day == DateOut.Day && x.Date_OUT.Value.Month == DateOut.Month && x.Date_OUT.Value.Year == DateOut.Year)).AsQueryable<UD_TB_AccessTime_Data>();
      List<UD_TB_AccessTime_Data> empDataList = empData.ToList<UD_TB_AccessTime_Data>();
      //return PartialView("_UserData", await empData.OrderBy(i => i.Date_IN).ToListAsync());
      return View("UserDataDetail", await empData.OrderBy(i => i.Date_IN).ToListAsync());

    }

    public async Task<ActionResult> WhoIsIn(string Refresh)
    {
      DateTime DateTimeNow = DateTime.Now;//Convert.ToDateTime("12/12/2019");

      IQueryable<UD_TB_AccessTime_Data> topRows = dbBioStar.UD_TB_AccessTime_Data.Where(x => x.Date_IN.Value.Year == DateTimeNow.Year && x.Date_IN.Value.Month == DateTimeNow.Month && x.Date_IN.Value.Day == DateTimeNow.Day ||
                                                                                 x.Date_OUT.Value.Year == DateTimeNow.Year && x.Date_OUT.Value.Month == DateTimeNow.Month && x.Date_OUT.Value.Day == DateTimeNow.Day).AsQueryable<UD_TB_AccessTime_Data>();
      List<UD_TB_AccessTime_Data> LsttopRows = topRows.ToList<UD_TB_AccessTime_Data>();
      //return View(await db.UD_TB_AccessTime_Data.ToListAsync());

      if (string.IsNullOrEmpty(Refresh))
      {
        return View(await topRows.ToListAsync());
      }
      else
      {
        return PartialView("_WhoIsIn", await topRows.ToListAsync());
      }

    }
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        dbBioStar.Dispose();
      }
      base.Dispose(disposing);
    }
  }
}
