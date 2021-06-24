using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LeaveON.UtilityClasses
{
  public class Attendance
  {
    [Key]
    public int Sr { get; set; }
    public string EmployeeName { get; set; }
    public string EmployeeNumber { get; set; }

    [DisplayFormat(DataFormatString = "{0: dd/MM/yyyy}")]
    public DateTime Date { get; set; }
    public string Day { get; set; }

    [DisplayName("Time In")]
    [DisplayFormat(DataFormatString = "{0:hh:mm:ss tt}")]
    public DateTime TimeIn { get; set; }

    [DisplayName("Time Out")]
    [DisplayFormat(DataFormatString = "{0:hh:mm:ss tt}")]
    public DateTime TimeOut { get; set; }
    public TimeSpan WorkingHours { get; set; }
    public TimeSpan TotalTime { get; set; }
    public string Status { get; set; }

  }
  public class OffTimeDetial
  {
    public DateTime TimeOut { get; set; }
    public DateTime TimeIn { get; set; }
    public TimeSpan OffHours { get; set; }
    
  }
}
