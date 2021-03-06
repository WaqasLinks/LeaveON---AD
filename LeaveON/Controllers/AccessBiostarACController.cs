
using LeaveON.UtilityClasses;
using Microsoft.AspNet.Identity;
using Repository.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TimeManagement.Models;

namespace LeaveON.Controllers
{
  public class AccessBiostarACController : Controller
  {
    private BioStarEntities dbBioStar = new BioStarEntities();
    LeaveONEntities dbLeaveOn = new LeaveONEntities();
    private Task<List<Attendance>> ConnectToDBandReturnWorkingHours(string ReqMonthYear, List<int> UserIds)
    {
      List<string> dateAttr = ReqMonthYear.Split('-').ToList();

      string connection = @"Data Source=10.1.10.27;Initial Catalog=BiostarAC;User Id=sa;Password=@intech#123;";
      //string connection = System.Configuration.ConfigurationManager.ConnectionStrings["CallCenterSalesEntities"].ConnectionString;
      SqlConnection con = new SqlConnection(connection);
      SqlCommand cmd;
      SqlDataReader dr;
      List<Attendance> LstAttendances = new List<Attendance>();
      TimeSpan TotalTime = new TimeSpan();
      TimeSpan TotalWorkingHours = new TimeSpan();
      string UserName = string.Empty;
      DateTime reqDate = DateTime.ParseExact(dateAttr[1] + "/" + dateAttr[0] + "/01", "yyyy/MM/dd", CultureInfo.InvariantCulture);
      int ThisMonthTotalDays = DateTime.DaysInMonth(int.Parse(dateAttr[1]), int.Parse(dateAttr[0]));
      con.Open();
      foreach (int Id in UserIds)
      {
        int UserId = Id;
        //cmd = new SqlCommand("select * from T_LG202106 where USRID = '9919' and SRVDT>= '2021-06-01' AND SRVDT<= '2021-06-30' order by EVTLGUID", con);
        //cmd = new SqlCommand("select * from T_LG201901 where USRID = '2205' order by EVTLGUID", con);
        cmd = new SqlCommand("select * from T_LG" + dateAttr[1] + dateAttr[0] + " where USRID ='" + UserId + "' order by EVTLGUID", con);
        dr = cmd.ExecuteReader();
        DataTable dt = new DataTable();
        dt.Load(dr);
        int rowsCount = dt.Rows.Count;
        if (rowsCount <= 0) continue;
        DateTime LastDate = (DateTime)dt.Rows[rowsCount - 1]["SRVDT"];
        DateTime thisDateTime = (DateTime)dt.Rows[0]["SRVDT"];
        int thisDay = thisDateTime.Day;
        int LastDay = LastDate.Day;
        int dayCounter = 1;
        List<int> LstEmptyDays = new List<int>();
        //---
        Attendance attendance;
        DateTime timeIn;
        DateTime timeOut;
        DateTime FirsTimeIn = DateTime.Today;
        DateTime LastTimeOut = DateTime.Today;
        DateTime blankDateTime = DateTime.Today;
        int EmptyDays = 0;
        bool IsFirstDone = false;
        TimeSpan ThidDayWorkingHours = new TimeSpan();
        //for (int k = dayCounter; k < thisDay; k++)
        //{
        //  LstEmptyDays.Add(k);
        //}
        AspNetUser aspNetUser = dbLeaveOn.AspNetUsers.FirstOrDefault(x => x.BioStarEmpNum.Value == UserId);
        UserName = aspNetUser.UserName.Substring(0, aspNetUser.UserName.IndexOf('@')).Replace(".", " ");
        for (int j = 0; j <= rowsCount - 1; j++)
        {

          thisDateTime = (DateTime)dt.Rows[j]["SRVDT"];

          if (thisDateTime.Day != thisDay)
          {//its mean new date started. so add all previois date calcuation here and add to list

            TotalTime = TotalTime.Add(LastTimeOut - FirsTimeIn);
            //UserName = dbLeaveOn.AspNetUsers.FirstOrDefault(x => x.BioStarEmpNum.Value == UserId).UserName;
            //UserName = UserName.Substring(0, UserName.IndexOf('@')).Replace(".", " ");
            attendance = new Attendance() { EmployeeName = UserName, EmployeeNumber = UserId, Date = FirsTimeIn.Date, Day = FirsTimeIn.DayOfWeek.ToString(), TimeIn = FirsTimeIn, TimeOut = LastTimeOut, WorkingHours = ThidDayWorkingHours, TotalTime = (LastTimeOut - FirsTimeIn) };
            LstAttendances.Add(attendance);
            TotalWorkingHours = TotalWorkingHours.Add(ThidDayWorkingHours);

            //reIntiallize variables to next date calculations
            ThidDayWorkingHours = new TimeSpan();
            IsFirstDone = false;
            for (int k = dayCounter; k < thisDay; k++)
            {
              LstEmptyDays.Add(k);
            }
            dayCounter = thisDay + 1;
            thisDay = thisDateTime.Day;


          }
          if (IsFirstDone == false && (int)dt.Rows[j]["TNAKEY"] == 1)
          {//get first time in
            FirsTimeIn = (DateTime)dt.Rows[j]["SRVDT"];
            IsFirstDone = true;
            UserId = Convert.ToInt32(dt.Rows[j]["USRID"]);
          }
          if ((int)dt.Rows[j]["TNAKEY"] == 2)
          {//get last time out
            LastTimeOut = (DateTime)dt.Rows[j]["SRVDT"];
          }

          //------get actual working hour of this date--------
          if ((j + 1 <= rowsCount - 1) && (int)dt.Rows[j]["TNAKEY"] == 1 && (int)dt.Rows[j + 1]["TNAKEY"] == 2 &&
            Convert.ToDateTime((DateTime)dt.Rows[j]["SRVDT"]).Day == thisDay && Convert.ToDateTime((DateTime)dt.Rows[j + 1]["SRVDT"]).Day == thisDay)
          {
            timeIn = (DateTime)dt.Rows[j]["SRVDT"];
            timeOut = (DateTime)dt.Rows[j + 1]["SRVDT"];
            TimeSpan workingHour = (timeOut - timeIn);

            ThidDayWorkingHours = ThidDayWorkingHours.Add(workingHour);
          }

        }
        //add last date to list here as loop ended
        TotalTime = TotalTime.Add(LastTimeOut - FirsTimeIn);
        attendance = new Attendance() { EmployeeName = UserName, EmployeeNumber = UserId, Date = FirsTimeIn.Date, Day = FirsTimeIn.DayOfWeek.ToString(), TimeIn = FirsTimeIn, TimeOut = LastTimeOut, WorkingHours = ThidDayWorkingHours, TotalTime = (LastTimeOut - FirsTimeIn) };
        LstAttendances.Add(attendance);
        TotalWorkingHours = TotalWorkingHours.Add(ThidDayWorkingHours);
        dayCounter = thisDay + 1;
        for (int k = dayCounter; k <= ThisMonthTotalDays; k++)
        {
          LstEmptyDays.Add(k);
        }

        //----------------------
        //int LastDayOfMonth = DateTime.DaysInMonth(reqDate.Year, reqDate.Month);
        //IQueryable<Leave> thisMonthsLeaves = dbLeaveOn.Leaves.Where(x => x.StartDate.Day >= 1 && x.EndDate.Day <= LastDayOfMonth && x.StartDate.Year == reqDate.Year && x.UserId == users[0].Id).AsQueryable<Leave>();
        //IQueryable<Leave> thisMonthsLeaves = dbLeaveOn.Leaves.Where(x => x.StartDate.Day >= 1 && x.EndDate.Day <= LastDayOfMonth && x.StartDate.Month == reqDate.Month && x.StartDate.Year == reqDate.Year && x.UserId == users[0].Id).AsQueryable<Leave>();
        List<Leave> thisMonthsLeaves = dbLeaveOn.Leaves.Where(x => x.StartDate.Day >= 1 && x.EndDate.Day <= ThisMonthTotalDays && x.StartDate.Month == reqDate.Month && x.StartDate.Year == reqDate.Year && x.UserId == aspNetUser.Id).ToList<Leave>();
        //List<Leave> thisMonthsLeaves = dbLeaveOn.Leaves.Where(x => x.UserId == UserGuidId).ToList<Leave>();
        List<Attendance> offDays = new List<Attendance>();

        //string empName = users[0].UserName;
        string biostarEmpName = "Test";//LstEmpData[0].EmployeeName;
        int iEmpNum = aspNetUser.BioStarEmpNum.Value;
        if (aspNetUser.UserLeavePolicyId != null)
        {
          int UserLeavePolicyId = aspNetUser.UserLeavePolicyId.Value;
          foreach (Leave leave in thisMonthsLeaves)
          {
            for (int i = 0; i < leave.TotalDays; i++)
            {
              Attendance leaveDay = new Attendance
              {
                EmployeeName = biostarEmpName,
                EmployeeNumber = leave.AspNetUser.BioStarEmpNum.Value,
                Date = leave.StartDate.AddDays(i),
                Day = leave.StartDate.AddDays(i).ToString("dddd"),
                Status = leave.Reason
              };
              offDays.Add(leaveDay);
            }
          }

          //get natioanl off days
          foreach (AnnualOffDay annualOffDay in dbLeaveOn.AnnualOffDays.Where(x => x.OffDay.Value.Month == reqDate.Month && x.OffDay.Value.Year == reqDate.Year && x.UserLeavePolicyId == UserLeavePolicyId).ToList<AnnualOffDay>())
          {
            Attendance annualOff = new Attendance
            {
              EmployeeName = biostarEmpName,
              EmployeeNumber = iEmpNum,
              Date = annualOffDay.OffDay.Value,
              Day = annualOffDay.OffDay.Value.ToString("dddd"),
              Status = annualOffDay.Description
            };
            //dayCntr += 1;
            offDays.Add(annualOff);
          }


          foreach (Attendance offday in offDays.ToList())
          {
            int? delThisDay = LstEmptyDays.FirstOrDefault(x => x == offday.Date.Day);
            
            if (delThisDay !=null && delThisDay >0)
            {
              LstEmptyDays.Remove(delThisDay.Value);
            }
            
          }
          LstAttendances.AddRange(offDays);
        }
        //----------------------



        foreach (int emptyday in LstEmptyDays)
        {
          blankDateTime = DateTime.ParseExact(dateAttr[1] + "/" + dateAttr[0] + "/" + emptyday.ToString("00"), "yyyy/MM/dd", CultureInfo.InvariantCulture);
          attendance = new Attendance() { EmployeeName = UserName, EmployeeNumber = UserId, Date = blankDateTime, Day = blankDateTime.DayOfWeek.ToString(), Status = "Holiday" };
          LstAttendances.Add(attendance);
        }
      }
      ViewBag.TotalHours = TotalTime.TotalHours.ToString("N2");
      ViewBag.TotalWorkingHours = TotalWorkingHours.TotalHours.ToString("N2");
      con.Close();
      return Task.FromResult(LstAttendances);
    }
    public async Task<ActionResult> ConnectToDBandReturnOffHours(string reqDate, string UserId)
    {

      DateTime from_Date = DateTime.ParseExact(reqDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
      DateTime to_Date = from_Date.AddDays(1);
      //ReqMonthYear = "06-2019";

      //reqDate = reqDate.Replace("/","-") + ".000";
      //List<string> dateAttr = ReqMonthYear.Split('-').ToList();
      string connection = @"Data Source=10.1.10.27;Initial Catalog=BiostarAC;User Id=sa;Password=@intech#123;";
      //string connection = System.Configuration.ConfigurationManager.ConnectionStrings["CallCenterSalesEntities"].ConnectionString;
      SqlConnection con = new SqlConnection(connection);
      SqlCommand cmd;
      SqlDataReader dr;
      con.Open();

      //cmd = new SqlCommand("select * from T_LG202106 where USRID = '9919' and SRVDT>= '2021-06-01' AND SRVDT<= '2021-06-30' order by EVTLGUID", con);
      //cmd = new SqlCommand("select * from T_LG201901 where USRID = '2205' order by EVTLGUID", con);
      //cmd = new SqlCommand("select * from T_LG" + dateAttr[1] + dateAttr[0] + " where USRID ='" + UserId + "' and SRVDT>='#" +reqDate + "#' and  SRVDT<='#" + reqDate + "#' order by EVTLGUID", con);
      //cmd = new SqlCommand("select * from T_LG" + dateAttr[1] + dateAttr[0] + " where USRID ='" + UserId + "' and SRVDT between @fromDate and @toDate order by EVTLGUID", con);
      //cmd = new SqlCommand("select * from T_LG" + dateAttr[1] + dateAttr[0] + " where USRID ='" + UserId + "' and SRVDT>='" + from_Date.ToString("yyyy-MM-dd HH:mm:ss.fffffff") + "' and  SRVDT<='" + to_Date.ToString("yyyy-MM-dd HH:mm:ss.fffffff") + "'  order by EVTLGUID", con);
      cmd = new SqlCommand("select * from T_LG" + from_Date.Year + from_Date.Month.ToString("00") + " where USRID ='" + UserId + "' and SRVDT between @fromDate and @toDate order by EVTLGUID", con);
      //cmd.Parameters.AddWithValue("@fromDate", from_Date.ToString("yyyy-MM-dd HH:mm:ss.fffffff"));
      //cmd.Parameters.AddWithValue("@toDate", to_Date.ToString("yyyy-MM-dd HH:mm:ss.fffffff"));
      cmd.Parameters.AddWithValue("@fromDate", from_Date);
      cmd.Parameters.AddWithValue("@toDate", to_Date);
      dr = cmd.ExecuteReader();
      DataTable dt = new DataTable();
      dt.Load(dr);
      //con.Close();
      int rowsCount = dt.Rows.Count;
      DateTime LastDate = (DateTime)dt.Rows[rowsCount - 1]["SRVDT"];
      DateTime thisDateTime = (DateTime)dt.Rows[0]["SRVDT"];
      int thisDay = thisDateTime.Day;

      int LastDay = LastDate.Day;

      List<OffTimeDetial> LstOffTimeDetial = new List<OffTimeDetial>();

      //for (int i = 1; i <= LastDay; i++)
      //{

      OffTimeDetial offTimeDetail;
      DateTime timeIn;
      DateTime timeOut;
      DateTime FirsTimeIn = DateTime.Today;
      DateTime LastTimeOut = DateTime.Today;
      TimeSpan TotalTime = new TimeSpan();
      bool IsFirstDone = false;
      TimeSpan ThidDayTotalOffTime = new TimeSpan();
      TimeSpan TotalOffHours = new TimeSpan();
      for (int j = 0; j <= rowsCount - 1; j++)
      {

        //------get actual off hour of this date--------

        if ((j + 1 <= rowsCount - 1) && (int)dt.Rows[j]["TNAKEY"] == 2 && (int)dt.Rows[j + 1]["TNAKEY"] == 1)
        {
          timeOut = (DateTime)dt.Rows[j]["SRVDT"];
          timeIn = (DateTime)dt.Rows[j + 1]["SRVDT"];

          TimeSpan offHour = (timeIn - timeOut);

          ThidDayTotalOffTime = ThidDayTotalOffTime.Add(offHour);
          offTimeDetail = new OffTimeDetial() { TimeOut = timeOut, TimeIn = timeIn, OffHours = offHour };
          LstOffTimeDetial.Add(offTimeDetail);
        }

      }
      ViewBag.ThidDayTotalOffTime = ThidDayTotalOffTime;



      con.Close();
      //return Task.FromResult(LstAttendances);
      //return View("UserData",await Task.FromResult(LstAttendances));
      return View("OffTimeDetail", await Task.FromResult(LstOffTimeDetial));
    }
    // GET: AccessBiostarAC
    public async Task<ActionResult> UserData(string ReqMonthYear, string UserId)
    {
      ViewBag.MonthSelectList = GetMonthSelectList();
      //ViewBag.SelectedMonth = monthSelectList[0];
      DateTime reqDate;
      int dEmpNum;
      List<Attendance> LstAttendances = new List<Attendance>();
      if (!string.IsNullOrEmpty(ReqMonthYear))
      {

        dEmpNum = int.Parse(UserId);
        //user.DepartmentId;//User.Identity.GetUserId();//
        reqDate = DateTime.ParseExact(ReqMonthYear, "MM-yyyy",
                                 System.Globalization.CultureInfo.CurrentCulture);
        //reqDate = reqDate.AddYears(-2);
      }
      else
      {
        //in case of empty parameters or First Time

        reqDate = DateTime.Now;
        string userId = User.Identity.GetUserId();
        dEmpNum = (int)dbLeaveOn.AspNetUsers.FirstOrDefault(x => x.Id == userId).BioStarEmpNum;
        //dUserId = decimal.Parse(userId);
        //dUserId = (decimal)dbBioStar.UD_TB_AD_USER.FirstOrDefault(x => x.EmployeeNumber == dUserId).EmployeeNumber;

        List<string> SelectedEmps = new List<string>();
        SelectedEmps.Add(dEmpNum.ToString());
        ViewBag.SelectedEmployees = SelectedEmps;
        //ViewBag.Employees = new SelectList(dbBioStar.UD_TB_AD_USER, "EmployeeNumber", "EmployeeName");
        ViewBag.Employees = new SelectList(dbLeaveOn.AspNetUsers, "BioStarEmpNum", "UserName").OrderBy(i => i.Text);

      }

      //IQueryable<UD_TB_AccessTime_Data> empData = null;
      //List<UD_TB_AccessTime_Data> LstEmpData = null;

      if (!string.IsNullOrEmpty(ReqMonthYear))
      {
        //var identity = (ClaimsIdentity)User.Identity;
        //IEnumerable<Claim> claims = identity.Claims;
        //Claim claim = claims.Where(x => x.Value == DepartmentId).FirstOrDefault();

        //if (claim is null) return null;


        
        //string userId = User.Identity.GetUserId();

        //int bioStarEmpNum = dbLeaveOn.AspNetUsers.FirstOrDefault(x => x.Id == userId).BioStarEmpNum.Value;

        //IQueryable<UD_TB_AccessTime_Data> allUsersData = null;
        List<AspNetUser> users = dbLeaveOn.AspNetUsers.Where(x => x.BioStarEmpNum == dEmpNum).ToList<AspNetUser>();
        string GuidUserId = users[0].Id;
        List<int> userIds = users.Select(x => x.BioStarEmpNum.Value).ToList<int>();
        //foreach (AspNetUser user in users)
        //{
        string ReqMonthYearFormated = reqDate.Month.ToString("00") + "-" + reqDate.Year;
        List<int> User_Ids = new List<int>();
        User_Ids.Add(dEmpNum);
        LstAttendances = await ConnectToDBandReturnWorkingHours(ReqMonthYearFormated, User_Ids);
        
      }
      //return View(await db.UD_TB_AccessTime_Data.ToListAsync());
      if (string.IsNullOrEmpty(ReqMonthYear))
      {
        //in case of null param or first time
        if (!(LstAttendances is null))
        {
          return View(LstAttendances.OrderBy(i => i.Date).ToList());

        }
        else
        {
          return View();
        }
      }
      else
      {
        return PartialView("_UserData", LstAttendances.OrderBy(i => i.Date).ToList());
      }

      //return View();
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
    public async Task<ActionResult> DepartmentData(string ReqMonthYear, string DepartmentName)
    {



      //DateTime myDate = DateTime.ParseExact("2009-05-08 14:40:52,531", "yyyy-MM-dd HH:mm:ss,fff",
      //                                 System.Globalization.CultureInfo.InvariantCulture);
      ViewBag.MonthSelectList = GetMonthSelectList();
      DateTime reqDate;
      //int intDepartmentId;
      if (!string.IsNullOrEmpty(ReqMonthYear))
      {

        //intDepartmentId = int.Parse(DepartmentId);
        //user.DepartmentId;//User.Identity.GetUserId();//
        reqDate = DateTime.ParseExact(ReqMonthYear, "MM-yyyy",
                                 System.Globalization.CultureInfo.CurrentCulture);
        //reqDate = reqDate.AddYears(-2);
      }
      else
      {
        //in case of empty parameters or First Time

        reqDate = DateTime.Now;
        string userId = User.Identity.GetUserId();
        DepartmentName = dbLeaveOn.AspNetUsers.FirstOrDefault(x => x.Id == userId).DepartmentName;
        List<string> SelectedDeps = new List<string>();
        SelectedDeps.Add(DepartmentName);
        ViewBag.SelectedDepartments = SelectedDeps;
        //ViewBag.Departments = new SelectList(dbLeaveOn.Departments, "Id", "Name");
        ViewBag.Departments = new SelectList(dbLeaveOn.DepartmentNames, "Name", "Name");
      }
      List<Attendance> depData = null;
      if (!string.IsNullOrEmpty(ReqMonthYear))
      {
        var identity = (ClaimsIdentity)User.Identity;
        IEnumerable<Claim> claims = identity.Claims;
        Claim claim = claims.Where(x => x.Value == DepartmentName).FirstOrDefault();

        if (claim is null) return null;


        
        //string userId = User.Identity.GetUserId();

        //int bioStarEmpNum = dbLeaveOn.AspNetUsers.FirstOrDefault(x => x.Id == userId).BioStarEmpNum.Value;

        //IQueryable<Attendance> allUsersData = null;
        List<AspNetUser> users = dbLeaveOn.AspNetUsers.Where(x => x.DepartmentName == DepartmentName).ToList<AspNetUser>();

        List<int> userIds = users.Select(x => x.BioStarEmpNum.Value).ToList<int>();
        //foreach (AspNetUser user in users)
        //{
        string ReqMonthYearFormated = reqDate.Month.ToString("00") + "-" + reqDate.Year;
        depData = await ConnectToDBandReturnWorkingHours(ReqMonthYearFormated, userIds);
        //}
      }
      //return View(await db.Attendance.ToListAsync());
      if (string.IsNullOrEmpty(ReqMonthYear))
      {
        //in case of null param or first time
        if (!(depData is null))
        {
          //return View(await depData.OrderBy(i => i.Date).ToList());

          return View(await Task.FromResult(depData.OrderBy(i => i.Date).ToList()));
        }
        else
        {
          return View();
        }

      }
      else
      {
        return PartialView("_UserData", await Task.FromResult(depData.OrderBy(i => i.Date).ToList()));
      }

    }
    public async Task<ActionResult> WhoIsIn(string reqDate)
    {
      //reqDate = "12-06-2019";
      DateTime from_Date = DateTime.ParseExact(reqDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
      DateTime to_Date = from_Date.AddDays(1);
      //ReqMonthYear = "06-2019";

      //reqDate = reqDate.Replace("/","-") + ".000";
      //List<string> dateAttr = ReqMonthYear.Split('-').ToList();
      string connection = @"Data Source=10.1.10.27;Initial Catalog=BiostarAC;User Id=sa;Password=@intech#123;";
      //string connection = System.Configuration.ConfigurationManager.ConnectionStrings["CallCenterSalesEntities"].ConnectionString;
      SqlConnection con = new SqlConnection(connection);
      SqlCommand cmd;
      SqlDataReader dr;
      con.Open();

      //cmd = new SqlCommand("select * from T_LG202106 where USRID = '9919' and SRVDT>= '2021-06-01' AND SRVDT<= '2021-06-30' order by EVTLGUID", con);
      //cmd = new SqlCommand("select * from T_LG201901 where USRID = '2205' order by EVTLGUID", con);
      //cmd = new SqlCommand("select * from T_LG" + dateAttr[1] + dateAttr[0] + " where USRID ='" + UserId + "' and SRVDT>='#" +reqDate + "#' and  SRVDT<='#" + reqDate + "#' order by EVTLGUID", con);
      //cmd = new SqlCommand("select * from T_LG" + dateAttr[1] + dateAttr[0] + " where USRID ='" + UserId + "' and SRVDT between @fromDate and @toDate order by EVTLGUID", con);
      //cmd = new SqlCommand("select * from T_LG" + dateAttr[1] + dateAttr[0] + " where USRID ='" + UserId + "' and SRVDT>='" + from_Date.ToString("yyyy-MM-dd HH:mm:ss.fffffff") + "' and  SRVDT<='" + to_Date.ToString("yyyy-MM-dd HH:mm:ss.fffffff") + "'  order by EVTLGUID", con);
      cmd = new SqlCommand("SELECT USRID, MIN(SRVDT) SRVDT from T_LG" + from_Date.Year + from_Date.Month.ToString("00") + " where SRVDT between @fromDate and @toDate AND TNAKEY=1 GROUP BY USRID order by SRVDT asc", con);
      //cmd.Parameters.AddWithValue("@fromDate", from_Date.ToString("yyyy-MM-dd HH:mm:ss.fffffff"));
      //cmd.Parameters.AddWithValue("@toDate", to_Date.ToString("yyyy-MM-dd HH:mm:ss.fffffff"));
      cmd.Parameters.AddWithValue("@fromDate", from_Date);
      cmd.Parameters.AddWithValue("@toDate", to_Date);
      dr = cmd.ExecuteReader();
      DataTable dt = new DataTable();
      dt.Load(dr);
      con.Close();
      int rowsCount = dt.Rows.Count;
      DateTime LastDate = (DateTime)dt.Rows[rowsCount - 1]["SRVDT"];
      DateTime thisDateTime = (DateTime)dt.Rows[0]["SRVDT"];
      int thisDay = thisDateTime.Day;

      int LastDay = LastDate.Day;

      List<Attendance> LstAttendances = new List<Attendance>();

      Attendance attendance;
      DateTime timeIn;
      DateTime timeOut;
      DateTime FirsTimeIn = DateTime.Today;
      DateTime LastTimeOut = DateTime.Today;
      TimeSpan TotalTime = new TimeSpan();
      bool IsFirstDone = false;
      TimeSpan ThidDayTotalOffTime = new TimeSpan();
      TimeSpan TotalOffHours = new TimeSpan();
      string UserName = string.Empty;

      int UserId;
      for (int j = 0; j <= rowsCount - 1; j++)
      {

        //------get actual off hour of this date--------
        //thisDateTime = (DateTime)dt.Rows[j]["SRVDT"];
        UserId = Convert.ToInt32(dt.Rows[j]["USRID"]);
        FirsTimeIn = (DateTime)(dt.Rows[j]["SRVDT"]);
        AspNetUser user = dbLeaveOn.AspNetUsers.FirstOrDefault(x => x.BioStarEmpNum.Value == UserId);
        if (user != null)
        {
          UserName = dbLeaveOn.AspNetUsers.FirstOrDefault(x => x.BioStarEmpNum.Value == UserId).UserName;
          UserName = UserName.Substring(0, UserName.IndexOf('@')).Replace(".", " ");
          attendance = new Attendance() { EmployeeName = UserName, EmployeeNumber = UserId, Date = FirsTimeIn.Date, Day = FirsTimeIn.DayOfWeek.ToString(), TimeIn = FirsTimeIn };
          LstAttendances.Add(attendance);

        }


      }


      return View("_UserDataToday", await Task.FromResult(LstAttendances));
    }
  }
}
