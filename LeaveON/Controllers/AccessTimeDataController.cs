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
      reqDate = reqDate.AddYears(-1);
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


        reqDate = reqDate.AddYears(-1);
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
    public async Task<ActionResult> UserData(string ReqMonthYear, string UserId)
    {



      //DateTime myDate = DateTime.ParseExact("2009-05-08 14:40:52,531", "yyyy-MM-dd HH:mm:ss,fff",
      //                                 System.Globalization.CultureInfo.InvariantCulture);
      DateTime reqDate;

      decimal dUserId;
      if (!string.IsNullOrEmpty(ReqMonthYear))
      {

        dUserId = decimal.Parse(UserId);
        //user.DepartmentId;//User.Identity.GetUserId();//
        reqDate = DateTime.ParseExact(ReqMonthYear, "MM-yyyy",
                                 System.Globalization.CultureInfo.CurrentCulture);

      }
      else
      {
        //in case of empty parameters or First Time

        reqDate = DateTime.Now;
        string userId = User.Identity.GetUserId();
        dUserId = (decimal)dbLeaveOn.AspNetUsers.FirstOrDefault(x => x.Id == userId).BioStarEmpNum;
        //dUserId = decimal.Parse(userId);
        //dUserId = (decimal)dbBioStar.UD_TB_AD_USER.FirstOrDefault(x => x.EmployeeNumber == dUserId).EmployeeNumber;
        
        List<string> SelectedEmps = new List<string>();
        SelectedEmps.Add(dUserId.ToString());
        ViewBag.SelectedEmployees = SelectedEmps;
        ViewBag.Employees = new SelectList(dbBioStar.UD_TB_AD_USER, "EmployeeNumber", "EmployeeName");

      }
      IQueryable<UD_TB_AccessTime_Data> empData = null;
      if (!string.IsNullOrEmpty(ReqMonthYear))
      {
        //var identity = (ClaimsIdentity)User.Identity;
        //IEnumerable<Claim> claims = identity.Claims;
        //Claim claim = claims.Where(x => x.Value == DepartmentId).FirstOrDefault();

        //if (claim is null) return null;


        reqDate = reqDate.AddYears(-1);
        //string userId = User.Identity.GetUserId();

        //int bioStarEmpNum = dbLeaveOn.AspNetUsers.FirstOrDefault(x => x.Id == userId).BioStarEmpNum.Value;

        //IQueryable<UD_TB_AccessTime_Data> allUsersData = null;
        List<AspNetUser> users = dbLeaveOn.AspNetUsers.Where(x => x.BioStarEmpNum == dUserId).ToList<AspNetUser>();

        List<int> userIds = users.Select(x => x.BioStarEmpNum.Value).ToList<int>();
        //foreach (AspNetUser user in users)
        //{

        empData = dbBioStar.UD_TB_AccessTime_Data.Where(x => userIds.Contains(x.EmployeeNumber.Value) && ((x.Date_IN.Value.Month == reqDate.Month && x.Date_IN.Value.Year == reqDate.Year) ||
                                                                                   x.Date_OUT.Value.Month == reqDate.Month && x.Date_OUT.Value.Year == reqDate.Year)).AsQueryable<UD_TB_AccessTime_Data>();
        //var groupByData = empData.ToList().GroupBy(x => x.Date_IN.Value.Day);
        
        //}
      }
      //return View(await db.UD_TB_AccessTime_Data.ToListAsync());
      if (string.IsNullOrEmpty(ReqMonthYear))
      {
        //in case of null param or first time
        if (!(empData is null))
        {
          return View(await empData.OrderBy(i => i.Date_IN).ToListAsync());
        }
        else
        {
          return View();
        }

      }
      else
      {
        return PartialView("_UserData", await empData.OrderBy(i => i.Date_IN).ToListAsync());
      }

    }

    public async Task<ActionResult> UserDataDetail(int EmployeeNumber, DateTime DateIn, DateTime DateOut)
    {

      //DateTime myDate = DateTime.ParseExact("2009-05-08 14:40:52,531", "yyyy-MM-dd HH:mm:ss,fff",
      //                                 System.Globalization.CultureInfo.InvariantCulture);
      
        //reqDate = DateTime.ParseExact(ReqMonthYear, "MM-yyyy",
        //                         System.Globalization.CultureInfo.CurrentCulture);

        IQueryable<UD_TB_AccessTime_Data> empData = null;
      
        empData = dbBioStar.UD_TB_AccessTime_Data.Where(x => x.EmployeeNumber== EmployeeNumber && ((x.Date_IN.Value.Day == DateIn.Day && x.Date_IN.Value.Month == DateIn.Month && x.Date_IN.Value.Year == DateIn.Year) ||
                                                                                   x.Date_OUT.Value.Day == DateOut.Day && x.Date_OUT.Value.Month == DateOut.Month && x.Date_OUT.Value.Year == DateOut.Year)).AsQueryable<UD_TB_AccessTime_Data>();
      List<UD_TB_AccessTime_Data> empDataList = empData.ToList <UD_TB_AccessTime_Data> ();
      //return PartialView("_UserData", await empData.OrderBy(i => i.Date_IN).ToListAsync());
      return View("UserDataDetail", await empData.OrderBy(i => i.Date_IN).ToListAsync());

    }

    public async Task<ActionResult> WhoIsIn(string Refresh)
    {
      DateTime DateTimeNow = Convert.ToDateTime("12/12/2019");//DateTime.Now;
      
      IQueryable<UD_TB_AccessTime_Data> topRows = dbBioStar.UD_TB_AccessTime_Data.Where(x=> x.Date_IN.Value.Year == DateTimeNow.Year && x.Date_IN.Value.Month ==  DateTimeNow.Month && x.Date_IN.Value.Day == DateTimeNow.Day ||
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
