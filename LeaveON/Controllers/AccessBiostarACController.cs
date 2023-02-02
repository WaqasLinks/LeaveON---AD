
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
  [Authorize(Roles = "Admin,Manager,User")]
  public class AccessBiostarACController : Controller
  {
    //new change
    private BioStarEntities dbBioStar = new BioStarEntities();
    LeaveONEntities dbLeaveOn = new LeaveONEntities();
    //enum ReadersIn {538600343, 38677, 538595648, 35816,540093375,540093369,540093374 , 547241993  ,540133115 , 538848767 }

    List<int> LstCardReadersIn = new List<int> { 538600343, 38677, 538595648, 35816, 540093375, 540093369, 540093374, 547241993, 540133115, 538848767, 540095692 };

    private Task<List<TimeData>> ConnectToDBandReturnWorkingHours(string ReqMonthYear, List<int> UserIds)
    {
      //ReqMonthYear = "07-2019";
      List<string> dateAttr = ReqMonthYear.Split('-').ToList();

      //string connection = @"Data Source=10.1.10.28;Initial Catalog=BiostarAC;User Id=sa;Password=@Intech#123;";
      string connection = System.Configuration.ConfigurationManager.ConnectionStrings["BioStarEntities"].ConnectionString;
      SqlConnection con = new SqlConnection(connection);
      SqlCommand cmd;
      SqlDataReader dr;
      List<TimeData> LstTimeData = new List<TimeData>();
      TimeSpan TotalTime = new TimeSpan();
      TimeSpan TotalWorkingHours = new TimeSpan();
     
      DateTime reqDate = DateTime.ParseExact(dateAttr[1] + "/" + dateAttr[0] + "/01", "yyyy/MM/dd", CultureInfo.InvariantCulture);

      int ThisMonthTotalDays = DateTime.DaysInMonth(int.Parse(dateAttr[1]), int.Parse(dateAttr[0]));
      con.Open();
      foreach (int Id in UserIds)
      {
        int UserId = Id;
        //cmd = new SqlCommand("select * from T_LG202106 where USRID = '9919' and SRVDT>= '2021-06-01' AND SRVDT<= '2021-06-30' order by EVTLGUID", con);
        //cmd = new SqlCommand("select * from T_LG201901 where USRID = '2205' order by EVTLGUID", con);


        cmd = new SqlCommand("select USRID,SRVDT,DEVDT,DEVUID from T_LG" + dateAttr[1] + dateAttr[0] + " where USRID ='" + UserId + "' and TNAKEY <> 0 order by DEVDT", con);//last good

        //cmd = new SqlCommand("select USRID,SRVDT,DEVDT,DEVUID from T_LG" + "2022" + "08" + " where USRID ='" + 2064 + "' and SRVDT BETWEEN '2022/08/31' AND '2022/09/01' order by DEVDT", con);

        dr = cmd.ExecuteReader();
        DataTable dt = new DataTable();

        //------
        string UserName = string.Empty;
        string timeZone = string.Empty;
        string countryName = string.Empty;
        AspNetUser aspNetUser = dbLeaveOn.AspNetUsers.FirstOrDefault(x => x.BioStarEmpNum.Value == UserId);
        UserName = aspNetUser.UserName.Substring(0, aspNetUser.UserName.IndexOf('@')).Replace(".", " ");
        string userLeavePolicyDescription = string.Empty;
        if (aspNetUser.UserLeavePolicy != null) userLeavePolicyDescription = aspNetUser.UserLeavePolicy.Description;
        if (aspNetUser.IsRelocated == true)
        {
          //in case relocate
          timeZone = dbLeaveOn.CountryNames.FirstOrDefault(x => x.Name == aspNetUser.CntryNameTemp).TimeZone;
          countryName = aspNetUser.CntryNameTemp;
        }
        else
        {
          timeZone = aspNetUser.CountryName.TimeZone;
          countryName = aspNetUser.CntryName;
        }
        //---------

        //////////////////
        //Creating dummy datatable for testing

        DataColumn dc = new DataColumn("USRID", typeof(String));
        dt.Columns.Add(dc);

        dc = new DataColumn("SRVDT", typeof(DateTime));
        dt.Columns.Add(dc);

        dc = new DataColumn("DEVDT", typeof(DateTime));
        dt.Columns.Add(dc);

        dc = new DataColumn("DEVUID", typeof(Int32));
        dt.Columns.Add(dc);
        dc = new DataColumn("DETAILS", typeof(string));
        dt.Columns.Add(dc);
        while (dr.Read())
        {
          // for each row from the database, add the retrieved table name to the list
          DataRow dtrw = dt.NewRow();
          //Convert.ToDateTime(ConvertToCountryTimeZone(dt, k, timeZone)).Day
          dtrw[0] = dr["USRID"];
          dtrw[1] = dr["SRVDT"];
          dtrw[2] = ConvertToCountryTimeZone(Convert.ToInt32( dr["DEVDT"]), timeZone);
          dtrw[3] = dr["DEVUID"];
          
          if (LstCardReadersIn.Contains( Convert.ToInt32(dr["DEVUID"])))
          {
            dtrw[4] = "IN";
          }
          else
          {
            dtrw[4] = "OUT";
          }
          dt.Rows.Add(dtrw);//this will add the row at the end of the datatable          
        }
        dr.Close();
        DataView view = dt.DefaultView;
        view.Sort = "DEVDT ASC";
        dt = view.ToTable();



        //dt.Load(dr);

        int rowsCount = dt.Rows.Count;
        if (rowsCount <= 0) continue;


        DateTime firstDateTime =(DateTime) dt.Rows[0]["DEVDT"];//ConvertToCountryTimeZone(dt, 0, timeZone);//(DateTime)dt.Rows[0]["SRVDT"];
        DateTime lastDateTime = (DateTime)dt.Rows[rowsCount-1]["DEVDT"];//ConvertToCountryTimeZone(dt, rowsCount - 1, timeZone);//(DateTime)dt.Rows[rowsCount - 1]["SRVDT"];
        int firstDay = firstDateTime.Day;
        int lastDay = lastDateTime.Day;
        int dayCounter = 1;
        List<int> LstEmptyDays = new List<int>();
        //---
        TimeData attendance;
        DateTime timeIn;
        DateTime timeOut;
        DateTime firsTimeIn = DateTime.ParseExact("2001-01-01 01:01:01", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);//firstDateTime;//DateTime.Today;
        DateTime lastTimeOut = DateTime.ParseExact("2001-01-01 01:01:01", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);//lastDateTime;//DateTime.Today;
        DateTime blankDateTime = DateTime.ParseExact("2001-01-01 01:01:01", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);//DateTime.Today;
        int EmptyDays = 0;
        bool IsCardIn = false;
        bool IsCardOut = false;
        bool lastsemiIn = false;
        TimeSpan ThidDayWorkingHours = new TimeSpan();
        int k = 0;//-1;
        //for (int k = dayCounter; k < thisDay; k++)
        //{
        //  LstEmptyDays.Add(k);
        //}

        for (int j = 0; j <= rowsCount - 1; j++)
        {

          //if (k >= j)             //                                |
          //{                           //                                |
          //  continue;   // Skip the remainder of this iteration. -----+
          //}
          firstDateTime = (DateTime)dt.Rows[j]["DEVDT"];//ConvertToCountryTimeZone(dt, j, timeZone);//(DateTime)dt.Rows[j]["SRVDT"];

          if (firstDateTime.Day != firstDay)
          {//its mean new date started. so add all previois date calcuation here and add to list

            if (firsTimeIn.Year != 2001 && lastTimeOut.Year != 2001)//(IsCardIn == true && IsCardOut == true)
            {
              TotalTime = TotalTime.Add(lastTimeOut - firsTimeIn);
              //UserName = dbLeaveOn.AspNetUsers.FirstOrDefault(x => x.BioStarEmpNum.Value == UserId).UserName;
              //UserName = UserName.Substring(0, UserName.IndexOf('@')).Replace(".", " ");

              attendance = new TimeData() { EmployeeName = UserName, EmployeeNumber = UserId, TimeZone = countryName, Policy = userLeavePolicyDescription, Date = firsTimeIn.Date, Day = firsTimeIn.DayOfWeek.ToString(), TimeIn = firsTimeIn, TimeOut = lastTimeOut, WorkingHours = ThidDayWorkingHours, TotalTime = (lastTimeOut - firsTimeIn) };

              LstTimeData.Add(attendance);
              TotalWorkingHours = TotalWorkingHours.Add(ThidDayWorkingHours);

            }

            //------reIntiallize variables to next date calculations
            firsTimeIn = DateTime.ParseExact("2001-01-01 01:01:01", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);//firstDateTime;//DateTime.Today;
            lastTimeOut = DateTime.ParseExact("2001-01-01 01:01:01", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);//lastDateTime;//DateTime.Today;
            ThidDayWorkingHours = new TimeSpan();
            IsCardIn = false; IsCardOut = false;

            //--- this loop is to cater empty days. for example time data started from 7th days. so to cater fist 6 days this loop is required
            //for (int k = dayCounter; k < firstDay; k++)
            //{
            //  LstEmptyDays.Add(k);
            //}

            //dayCounter = firstDay + 1;
            firstDay = firstDateTime.Day;


          }

          k = j+1;
          //while ((k <= rowsCount - 1) && LstCardReadersIn.Contains((int)dt.Rows[k]["DEVUID"]))//find last semi in
          //{
          //  k++; //Last semi in
          //  lastsemiIn = true;
          //}
          //if (lastsemiIn==true)
          //{
          //  while ((k <= rowsCount - 1) && !LstCardReadersIn.Contains((int)dt.Rows[k]["DEVUID"]))//find last semi out
          //  {
          //    k++; 
          //    lastsemiIn = false;
          //  }
          //  k--;//Last semi out
          //}


          //------get actual working hour of this date--------
          //if ((j + 1 <= rowsCount - 1) && (int)dt.Rows[j]["TNAKEY"] == 1 && (int)dt.Rows[j + 1]["TNAKEY"] == 2 &&
          if ((k <= rowsCount - 1) && LstCardReadersIn.Contains((int)dt.Rows[j]["DEVUID"]) && !LstCardReadersIn.Contains((int)dt.Rows[k]["DEVUID"]) &&
            Convert.ToDateTime(dt.Rows[j]["DEVDT"]).Day == firstDay && Convert.ToDateTime(dt.Rows[k]["DEVDT"]).Day == firstDay)
          {
            if (IsCardIn == false)
            {//get first time in
              firsTimeIn = (DateTime)dt.Rows[j]["DEVDT"];//ConvertToCountryTimeZone(dt, j, timeZone);//(DateTime)dt.Rows[j]["SRVDT"];
              lastTimeOut = (DateTime)dt.Rows[k]["DEVDT"];//ConvertToCountryTimeZone(dt, k, timeZone);
              IsCardIn = true;
              //UserId = Convert.ToInt32(dt.Rows[j]["USRID"]);
            }

            timeIn = (DateTime)dt.Rows[j]["DEVDT"];//ConvertToCountryTimeZone(dt, j, timeZone);//(DateTime)dt.Rows[j]["SRVDT"];
            timeOut = (DateTime)dt.Rows[k]["DEVDT"];//ConvertToCountryTimeZone(dt, k, timeZone);//(DateTime)dt.Rows[j + 1]["SRVDT"];
            TimeSpan workingHour = (timeOut - timeIn);
            ThidDayWorkingHours = ThidDayWorkingHours.Add(workingHour);

            //IsCardIn = true; IsCardOut = true;

            //if (IsCardIn == true && (int)dt.Rows[j]["TNAKEY"] == 2)
            if (IsCardIn == true && !LstCardReadersIn.Contains((int)dt.Rows[k]["DEVUID"]))
            {//get last time out
              IsCardOut = true;
              lastTimeOut = (DateTime)dt.Rows[k]["DEVDT"];//ConvertToCountryTimeZone(dt, k, timeZone);//(DateTime)dt.Rows[j]["SRVDT"];
            }
          }
          

        }//datetime for end here

        //add last date to list here as loop ended

        if (firsTimeIn.Year != 2001 && lastTimeOut.Year != 2001)//(IsCardIn == true && IsCardOut == true)
        {
          TotalTime = TotalTime.Add(lastTimeOut - firsTimeIn);

          attendance = new TimeData() { EmployeeName = UserName, EmployeeNumber = UserId, TimeZone = countryName, Policy = userLeavePolicyDescription, Date = firsTimeIn.Date, Day = firsTimeIn.DayOfWeek.ToString(), TimeIn = firsTimeIn, TimeOut = lastTimeOut, WorkingHours = ThidDayWorkingHours, TotalTime = (lastTimeOut - firsTimeIn) };

          LstTimeData.Add(attendance);
          TotalWorkingHours = TotalWorkingHours.Add(ThidDayWorkingHours);
        }

        //dayCounter = firstDay + 1;
        //for (int k = dayCounter; k <= ThisMonthTotalDays; k++)
        //{
        //  LstEmptyDays.Add(k);
        //}

        //----------------------
        //int LastDayOfMonth = DateTime.DaysInMonth(reqDate.Year, reqDate.Month);
        //IQueryable<Leave> thisMonthsLeaves = dbLeaveOn.Leaves.Where(x => x.StartDate.Day >= 1 && x.EndDate.Day <= LastDayOfMonth && x.StartDate.Year == reqDate.Year && x.UserId == users[0].Id).AsQueryable<Leave>();
        //IQueryable<Leave> thisMonthsLeaves = dbLeaveOn.Leaves.Where(x => x.StartDate.Day >= 1 && x.EndDate.Day <= LastDayOfMonth && x.StartDate.Month == reqDate.Month && x.StartDate.Year == reqDate.Year && x.UserId == users[0].Id).AsQueryable<Leave>();
        List<Leave> thisMonthsLeaves = dbLeaveOn.Leaves.Where(x => x.StartDate.Day >= 1 && x.EndDate.Day <= ThisMonthTotalDays && x.StartDate.Month == reqDate.Month && x.StartDate.Year == reqDate.Year && x.UserId == aspNetUser.Id).ToList<Leave>();
        //List<Leave> thisMonthsLeaves = dbLeaveOn.Leaves.Where(x => x.UserId == UserGuidId).ToList<Leave>();
        List<TimeData> offDays = new List<TimeData>();

        //------get Leave days
        //string empName = users[0].UserName;
        //string biostarEmpName = "";//LstEmpData[0].EmployeeName;
        int iEmpNum = aspNetUser.BioStarEmpNum.Value;
        if (aspNetUser.UserLeavePolicyId != null)
        {
          int UserLeavePolicyId = aspNetUser.UserLeavePolicyId.Value;
          foreach (Leave leave in thisMonthsLeaves)
          {
            for (int i = 0; i < leave.TotalDays; i++)
            {
              TimeData leaveDay = new TimeData
              {
                EmployeeName = UserName,
                EmployeeNumber = leave.AspNetUser.BioStarEmpNum.Value,
                TimeZone = countryName,
                Policy = userLeavePolicyDescription,
                Date = leave.StartDate.AddDays(i),
                Day = leave.StartDate.AddDays(i).ToString("dddd"),
                Status = leave.Reason
              };
              offDays.Add(leaveDay);
            }
          }

          //------get natioanl holidays
          foreach (AnnualOffDay annualHoliday in dbLeaveOn.AnnualOffDays.Where(x => x.OffDay.Value.Month == reqDate.Month && x.OffDay.Value.Year == reqDate.Year && x.UserLeavePolicyId == UserLeavePolicyId).ToList<AnnualOffDay>())
          {
            TimeData annualOff = new TimeData
            {
              EmployeeName = UserName,
              EmployeeNumber = iEmpNum,
              TimeZone = countryName,
              Policy = userLeavePolicyDescription,
              Date = annualHoliday.OffDay.Value,
              Day = annualHoliday.OffDay.Value.ToString("dddd"),
              Status = annualHoliday.Description
            };
            //dayCntr += 1;
            offDays.Add(annualOff);
          }

          //------- get weekEndDays



          //foreach (TimeData offday in leaveNholidays.ToList())
          //{
          //  int? delThisDay = LstEmptyDays.FirstOrDefault(x => x == offday.Date.Day);

          //  if (delThisDay != null && delThisDay > 0)
          //  {
          //    LstEmptyDays.Remove(delThisDay.Value);
          //  }

          //}


        }
        //----------------------

        //foreach (int emptyday in LstEmptyDays)
        //{
        //  blankDateTime = DateTime.ParseExact(dateAttr[1] + "/" + dateAttr[0] + "/" + emptyday.ToString("00"), "yyyy/MM/dd", CultureInfo.InvariantCulture);
        //  attendance = new TimeData() { EmployeeName = UserName, EmployeeNumber = UserId, Date = blankDateTime, Day = blankDateTime.DayOfWeek.ToString(), Status = "Absent" };
        //  LstTimeData.Add(attendance);
        //}
        List<int> LstThisMonthsWeekEnds = new List<int>();
        if (aspNetUser.UserLeavePolicy == null || string.IsNullOrEmpty(aspNetUser.UserLeavePolicy.WeeklyOffDays))
        {
          LstThisMonthsWeekEnds = GetWeekEndList(int.Parse(dateAttr[1]), int.Parse(dateAttr[0]), "6,0");
        }
        else
        {
          LstThisMonthsWeekEnds = GetWeekEndList(int.Parse(dateAttr[1]), int.Parse(dateAttr[0]), aspNetUser.UserLeavePolicy.WeeklyOffDays);
        }


        foreach (int weekEndDay in LstThisMonthsWeekEnds)
        {
          TimeData thisWeekEnd = LstTimeData.FirstOrDefault(x => x.Date.Day == weekEndDay);
          if (thisWeekEnd != null)
          {
            thisWeekEnd.Status = "Holiday";
          }
          else
          {

            DateTime weekEndDate = DateTime.ParseExact(dateAttr[1] + "/" + dateAttr[0] + "/" + weekEndDay.ToString("00"), "yyyy/MM/dd", CultureInfo.InvariantCulture);
            TimeData weekEndOffDate = new TimeData
            {
              EmployeeName = UserName,
              EmployeeNumber = iEmpNum,
              TimeZone = countryName,
              Policy = userLeavePolicyDescription,
              Date = weekEndDate,
              Day = weekEndDate.ToString("dddd"),
              Status = "Holiday"
            };
            //dayCntr += 1;
            offDays.Add(weekEndOffDate);
          }
        }
        LstTimeData.AddRange(offDays);

        for (int absentDay = 1; absentDay <= ThisMonthTotalDays; absentDay++)
        {
          if (LstTimeData.FirstOrDefault(x => x.Date.Day == absentDay) == null)
          {
            blankDateTime = DateTime.ParseExact(dateAttr[1] + "/" + dateAttr[0] + "/" + absentDay.ToString("00"), "yyyy/MM/dd", CultureInfo.InvariantCulture);
            attendance = new TimeData() { EmployeeName = UserName, EmployeeNumber = UserId, TimeZone = countryName, Policy = userLeavePolicyDescription, Date = blankDateTime, Day = blankDateTime.DayOfWeek.ToString(), Status = "Absent" };
            LstTimeData.Add(attendance);
          }
          //LstEmptyDays.Add(k);
        }
      }
      //to avaid showing current month all data which is not happend yet
      foreach (var itm in LstTimeData.ToList())
      {
        if (itm.Date > DateTime.Now.Date)
        {
          LstTimeData.Remove(itm);
        }
      }

      ViewBag.TotalHours = TotalTime.TotalHours.ToString("N2");
      ViewBag.TotalWorkingHours = TotalWorkingHours.TotalHours.ToString("N2");
      con.Close();
      return Task.FromResult(LstTimeData);
    }
    protected List<int> GetWeekEndList(int year, int month, string WeekEndDays)
    {
      List<int> LstWeekEndDays = WeekEndDays.Split(',').Select(int.Parse).ToList();
      List<int> LstThisMonthsWeekEnds = new List<int>();
      foreach (DayOfWeek weekEnd in LstWeekEndDays)
      {
        //DayOfWeek dayName= weekEnd;
        CultureInfo ci = new CultureInfo("en-US");
        for (int i = 1; i <= ci.Calendar.GetDaysInMonth(year, month); i++)
        {
          if (new DateTime(year, month, i).DayOfWeek == weekEnd)
            LstThisMonthsWeekEnds.Add(i);
        }
      }
      LstThisMonthsWeekEnds.Sort();
      return LstThisMonthsWeekEnds;
    }
    private DateTime ConvertToCountryTimeZone(DataTable dt, int rowNo, string timeZone)
    {
      //(DateTime)dt.Rows[rowsCount - 1]["DEVDT"];
      DateTime ConvertedDateTime;
      try
      {
        if (timeZone == "")
        {
          int unixTimeStamp = (int)dt.Rows[rowNo]["DEVDT"];
          DateTimeOffset utcDateTime = DateTimeOffset.FromUnixTimeSeconds(unixTimeStamp);
          TimeZoneInfo customeTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
          ConvertedDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime.DateTime, customeTimeZone);
        }
        else
        {
          int unixTimeStamp = (int)dt.Rows[rowNo]["DEVDT"];
          DateTimeOffset utcDateTime = DateTimeOffset.FromUnixTimeSeconds(unixTimeStamp);
          TimeZoneInfo customeTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZone);//"Pakistan Standard Time");
          ConvertedDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime.DateTime, customeTimeZone);
        }
        return ConvertedDateTime;
      }
      catch (Exception ex)
      {

        throw ex;
      }



    }
    private DateTime ConvertToCountryTimeZone(int UnixDateTime, string timeZone)
    {
      //(DateTime)dt.Rows[rowsCount - 1]["DEVDT"];
      DateTime ConvertedDateTime;
      try
      {
        if (timeZone == "")
        {
          //int unixTimeStamp = (int)dt.Rows[rowNo]["DEVDT"];
          DateTimeOffset utcDateTime = DateTimeOffset.FromUnixTimeSeconds(UnixDateTime);
          TimeZoneInfo customeTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
          ConvertedDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime.DateTime, customeTimeZone);
        }
        else
        {
          //int unixTimeStamp = (int)dt.Rows[rowNo]["DEVDT"];
          DateTimeOffset utcDateTime = DateTimeOffset.FromUnixTimeSeconds(UnixDateTime);
          TimeZoneInfo customeTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZone);//"Pakistan Standard Time");
          ConvertedDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime.DateTime, customeTimeZone);
        }
        return ConvertedDateTime;
      }
      catch (Exception ex)
      {

        throw ex;
      }



    }
    public async Task<ActionResult> ConnectToDBandReturnOffHours(string reqDate, string UserId)
    {

      DateTime from_Date = DateTime.ParseExact(reqDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
      DateTime to_Date = from_Date.AddDays(1);
      //ReqMonthYear = "06-2019";

      //reqDate = reqDate.Replace("/","-") + ".000";
      //List<string> dateAttr = ReqMonthYear.Split('-').ToList();
      //string connection = @"Data Source=10.1.10.28;Initial Catalog=BiostarAC;User Id=sa;Password=@Intech#123;";
      string connection = System.Configuration.ConfigurationManager.ConnectionStrings["BioStarEntities"].ConnectionString;
      SqlConnection con = new SqlConnection(connection);
      SqlCommand cmd;
      SqlDataReader dr;
      con.Open();

      //cmd = new SqlCommand("select * from T_LG202106 where USRID = '9919' and SRVDT>= '2021-06-01' AND SRVDT<= '2021-06-30' order by EVTLGUID", con);
      //cmd = new SqlCommand("select * from T_LG201901 where USRID = '2205' order by EVTLGUID", con);
      //cmd = new SqlCommand("select * from T_LG" + dateAttr[1] + dateAttr[0] + " where USRID ='" + UserId + "' and SRVDT>='#" +reqDate + "#' and  SRVDT<='#" + reqDate + "#' order by EVTLGUID", con);
      //cmd = new SqlCommand("select * from T_LG" + dateAttr[1] + dateAttr[0] + " where USRID ='" + UserId + "' and SRVDT between @fromDate and @toDate order by EVTLGUID", con);
      //cmd = new SqlCommand("select * from T_LG" + dateAttr[1] + dateAttr[0] + " where USRID ='" + UserId + "' and SRVDT>='" + from_Date.ToString("yyyy-MM-dd HH:mm:ss.fffffff") + "' and  SRVDT<='" + to_Date.ToString("yyyy-MM-dd HH:mm:ss.fffffff") + "'  order by EVTLGUID", con);
      cmd = new SqlCommand("select * from T_LG" + from_Date.Year + from_Date.Month.ToString("00") + " where USRID ='" + UserId + "' and SRVDT between @fromDate and @toDate order by DEVDT", con);
      //cmd.Parameters.AddWithValue("@fromDate", from_Date.ToString("yyyy-MM-dd HH:mm:ss.fffffff"));
      //cmd.Parameters.AddWithValue("@toDate", to_Date.ToString("yyyy-MM-dd HH:mm:ss.fffffff"));
      cmd.Parameters.AddWithValue("@fromDate", from_Date);
      cmd.Parameters.AddWithValue("@toDate", to_Date);
      dr = cmd.ExecuteReader();
      DataTable dt = new DataTable();
      dt.Load(dr);
      //con.Close();
      int rowsCount = dt.Rows.Count;
      //--------
      string UserName = string.Empty;
      string timeZone = string.Empty;
      string countryName = string.Empty;
      int intUserId = int.Parse(UserId);
      AspNetUser aspNetUser = dbLeaveOn.AspNetUsers.FirstOrDefault(x => x.BioStarEmpNum.Value == intUserId);
      UserName = aspNetUser.UserName.Substring(0, aspNetUser.UserName.IndexOf('@')).Replace(".", " ");
      string userLeavePolicyDescription = string.Empty;
      if (aspNetUser.UserLeavePolicy != null) userLeavePolicyDescription = aspNetUser.UserLeavePolicy.Description;
      if (aspNetUser.IsRelocated == true)
      {
        //in case relocate
        timeZone = dbLeaveOn.CountryNames.FirstOrDefault(x => x.Name == aspNetUser.CntryNameTemp).TimeZone;
        countryName = aspNetUser.CntryNameTemp;
      }
      else
      {
        timeZone = aspNetUser.CountryName.TimeZone;
        countryName = aspNetUser.CntryName;
      }
      //----------


      DateTime LastDate = ConvertToCountryTimeZone(dt, rowsCount - 1, timeZone);//(DateTime)dt.Rows[rowsCount - 1]["SRVDT"];
      DateTime thisDateTime = ConvertToCountryTimeZone(dt, 0, timeZone);//(DateTime)dt.Rows[0]["SRVDT"];
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
        //&& LstInCardReaders.Contains((int)dt.Rows[j]["DEVUID"]) && !LstInCardReaders.Contains((int)dt.Rows[j + 1]["DEVUID"])
        //if ((j + 1 <= rowsCount - 1) && (int)dt.Rows[j]["TNAKEY"] == 2 && (int)dt.Rows[j + 1]["TNAKEY"] == 1)
        if ((j + 1 <= rowsCount - 1) && !LstCardReadersIn.Contains((int)dt.Rows[j]["DEVUID"]) && LstCardReadersIn.Contains((int)dt.Rows[j + 1]["DEVUID"]))
        {
          timeOut = ConvertToCountryTimeZone(dt, j, timeZone);//(DateTime)dt.Rows[j]["SRVDT"];
          timeIn = ConvertToCountryTimeZone(dt, j + 1, timeZone);//(DateTime)dt.Rows[j + 1]["SRVDT"];

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

      List<TimeData> LstAttendances = new List<TimeData>();
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
    [Authorize(Roles = "Admin,Manager")]
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
      List<TimeData> depData = null;
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
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> MonthsWiseData(string ReqFromMonth, string ReqToMonth)
    {



      //DateTime myDate = DateTime.ParseExact("2009-05-08 14:40:52,531", "yyyy-MM-dd HH:mm:ss,fff",
      //                                 System.Globalization.CultureInfo.InvariantCulture);
      ViewBag.MonthSelectList = GetMonthSelectList();
      DateTime reqFromDate;
      DateTime reqToDate;
      //int intDepartmentId;
      if (!string.IsNullOrEmpty(ReqFromMonth))
      {

        //intDepartmentId = int.Parse(DepartmentId);
        //user.DepartmentId;//User.Identity.GetUserId();//
        reqFromDate = DateTime.ParseExact(ReqFromMonth, "MM-yyyy",
                                 System.Globalization.CultureInfo.CurrentCulture);
        reqToDate = DateTime.ParseExact(ReqToMonth, "MM-yyyy",
                                 System.Globalization.CultureInfo.CurrentCulture);
        //reqDate = reqDate.AddYears(-2);
      }
      else
      {
        //in case of empty parameters or First Time
        /*---------*/
        reqFromDate = DateTime.Now;
        reqToDate = DateTime.Now;
        //string userId = User.Identity.GetUserId();
        //DepartmentName = dbLeaveOn.AspNetUsers.FirstOrDefault(x => x.Id == userId).DepartmentName;
        //List<string> SelectedDeps = new List<string>();
        //SelectedDeps.Add(DepartmentName);
        //ViewBag.SelectedDepartments = SelectedDeps;
        //ViewBag.Departments = new SelectList(dbLeaveOn.DepartmentNames, "Name", "Name");
        /*---------*/
      }
      List<TimeData> depData = null;
      List<TimeData> totalDepData = null;
      if (!string.IsNullOrEmpty(ReqFromMonth))
      {
        /*--------*/
        //var identity = (ClaimsIdentity)User.Identity;
        //IEnumerable<Claim> claims = identity.Claims;
        //Claim claim = claims.Where(x => x.Value == DepartmentName).FirstOrDefault();

        //if (claim is null) return null;
        /*--------*/
        List<AspNetUser> users = dbLeaveOn.AspNetUsers.ToList<AspNetUser>();

        List<int> userIds = users.Select(x => x.BioStarEmpNum.Value).ToList<int>();
        //foreach (AspNetUser user in users)
        //{

        string ReqMonthYearFormated = string.Empty;
        totalDepData = new List<TimeData>();
        for (var month = reqFromDate.Date; month.Date <= reqToDate.Date; month = month.AddMonths(1))
        {
          ReqMonthYearFormated = month.Month.ToString("00") + "-" + month.Year;
          depData = await ConnectToDBandReturnWorkingHours(ReqMonthYearFormated, userIds);
          totalDepData.AddRange(depData);
        }
        //}
      }
      //return View(await db.Attendance.ToListAsync());
      if (string.IsNullOrEmpty(ReqFromMonth))
      {
        //in case of null param or first time
        if (!(totalDepData is null))
        {
          //return View(await totalDepData.OrderBy(i => i.Date).ToList());

          return View(await Task.FromResult(totalDepData.OrderBy(i => i.Date).ToList()));
        }
        else
        {
          return View();
        }

      }
      else
      {
        return PartialView("_MonthsWiseData", await Task.FromResult(totalDepData.OrderBy(i => i.Date).ToList()));
      }

    }
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> WhoIsIn(string reqDate)
    {
      //reqDate = "12-06-2019";
      DateTime from_Date = DateTime.Now.Date;//DateTime.ParseExact(reqDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
      DateTime to_Date = from_Date.AddDays(1);
      //ReqMonthYear = "06-2019";

      //reqDate = reqDate.Replace("/","-") + ".000";
      //List<string> dateAttr = ReqMonthYear.Split('-').ToList();
      //string connection = @"Data Source=10.1.10.28;Initial Catalog=BiostarAC;User Id=sa;Password=@Intech#123;";
      string connection = System.Configuration.ConfigurationManager.ConnectionStrings["BioStarEntities"].ConnectionString;
      SqlConnection con = new SqlConnection(connection);
      SqlCommand cmd;
      SqlDataReader dr;
      con.Open();

      //cmd = new SqlCommand("select * from T_LG202106 where USRID = '9919' and SRVDT>= '2021-06-01' AND SRVDT<= '2021-06-30' order by EVTLGUID", con);
      //cmd = new SqlCommand("select * from T_LG201901 where USRID = '2205' order by EVTLGUID", con);
      //cmd = new SqlCommand("select * from T_LG" + dateAttr[1] + dateAttr[0] + " where USRID ='" + UserId + "' and SRVDT>='#" +reqDate + "#' and  SRVDT<='#" + reqDate + "#' order by EVTLGUID", con);
      //cmd = new SqlCommand("select * from T_LG" + dateAttr[1] + dateAttr[0] + " where USRID ='" + UserId + "' and SRVDT between @fromDate and @toDate order by EVTLGUID", con);
      //cmd = new SqlCommand("select * from T_LG" + dateAttr[1] + dateAttr[0] + " where USRID ='" + UserId + "' and SRVDT>='" + from_Date.ToString("yyyy-MM-dd HH:mm:ss.fffffff") + "' and  SRVDT<='" + to_Date.ToString("yyyy-MM-dd HH:mm:ss.fffffff") + "'  order by EVTLGUID", con);
      cmd = new SqlCommand("SELECT USRID, MIN(DEVDT) DEVDT from T_LG" + from_Date.Year + from_Date.Month.ToString("00") + " where SRVDT between @fromDate and @toDate GROUP BY USRID order by DEVDT asc", con);
      //cmd.Parameters.AddWithValue("@fromDate", from_Date.ToString("yyyy-MM-dd HH:mm:ss.fffffff"));
      //cmd.Parameters.AddWithValue("@toDate", to_Date.ToString("yyyy-MM-dd HH:mm:ss.fffffff"));
      cmd.Parameters.AddWithValue("@fromDate", from_Date);
      cmd.Parameters.AddWithValue("@toDate", to_Date);
      dr = cmd.ExecuteReader();
      DataTable dt = new DataTable();
      dt.Load(dr);
      con.Close();
      int rowsCount = dt.Rows.Count;


      List<TimeData> LstAttendances = new List<TimeData>();

      TimeData attendance;
      DateTime FirsTimeIn = DateTime.Today;
      DateTime LastTimeOut = DateTime.Today;
      
      long UserId;
      for (int j = 0; j <= rowsCount - 1; j++)
      {

        //------get actual off hour of this date--------
        //thisDateTime = (DateTime)dt.Rows[j]["SRVDT"];

        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[j]["USRID"])))
        {
          try
          {
            UserId = Convert.ToInt64(dt.Rows[j]["USRID"]);
          }
          catch (Exception ex)
          {

            throw ex;
          }
          
        }
        else
        {
          continue;
        }

        //--------
        string UserName = string.Empty;
        string timeZone = string.Empty;
        string countryName = string.Empty;
        string userLeavePolicyDescription = string.Empty;
        AspNetUser aspNetUser = dbLeaveOn.AspNetUsers.FirstOrDefault(x => x.BioStarEmpNum.Value == UserId);
        if (aspNetUser!=null)
        { 
          UserName = aspNetUser.UserName.Substring(0, aspNetUser.UserName.IndexOf('@')).Replace(".", " ");
          
          if (aspNetUser.UserLeavePolicy != null) userLeavePolicyDescription = aspNetUser.UserLeavePolicy.Description;
          if (aspNetUser.IsRelocated == true)
          {
            //in case relocate
            timeZone = dbLeaveOn.CountryNames.FirstOrDefault(x => x.Name == aspNetUser.CntryNameTemp).TimeZone;
            countryName = aspNetUser.CntryNameTemp;
          }
          else
          {
            timeZone = aspNetUser.CountryName.TimeZone;
            countryName = aspNetUser.CntryName;
          }
        }

        //----------


        //if (dbLeaveOn.AspNetUsers.FirstOrDefault(x => x.BioStarEmpNum.Value == UserId) != null)
        //{
        //  timeZone = dbLeaveOn.AspNetUsers.FirstOrDefault(x => x.BioStarEmpNum.Value == UserId).CountryName.TimeZone;
        //  countryName = dbLeaveOn.AspNetUsers.FirstOrDefault(x => x.BioStarEmpNum.Value == UserId).CountryName.Name;
        //}
        //else
        //{
        //  timeZone = string.Empty;
        //}


        FirsTimeIn = ConvertToCountryTimeZone(dt, j, timeZone);//(DateTime)(dt.Rows[j]["SRVDT"]);
        AspNetUser user = dbLeaveOn.AspNetUsers.FirstOrDefault(x => x.BioStarEmpNum.Value == UserId);
        if (aspNetUser != null)
        {
          if (aspNetUser.UserLeavePolicy != null) userLeavePolicyDescription = aspNetUser.UserLeavePolicy.Description;
          UserName = dbLeaveOn.AspNetUsers.FirstOrDefault(x => x.BioStarEmpNum.Value == UserId).UserName;
          UserName = UserName.Substring(0, UserName.IndexOf('@')).Replace(".", " ");

          attendance = new TimeData() { EmployeeName = UserName, EmployeeNumber = Convert.ToInt32(UserId), TimeZone = countryName, Policy = userLeavePolicyDescription, Date = FirsTimeIn.Date, Day = FirsTimeIn.DayOfWeek.ToString(), TimeIn = FirsTimeIn };

          LstAttendances.Add(attendance);

        }


      }


      return View("_UserDataToday", await Task.FromResult(LstAttendances));
    }

  }
}
