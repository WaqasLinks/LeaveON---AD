using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Repository.Models;

namespace LeaveON.UtilityClasses
{
  public class Dashboard
  {
    public string EmployeeName { get; set; }
    [DisplayName("Emp.No.")]
    public int EmployeeNumber { get; set; }
    public string Country { get; set; }
    public string Policy { get; set; }

    [DisplayFormat(DataFormatString = "{0: dd/MM/yyyy}")]
    public DateTime Date { get; set; }
    public string Day { get; set; }

    [DisplayName("Time In")]
    [DisplayFormat(DataFormatString = "{0:hh:mm:ss tt}")]
    public DateTime TimeIn { get; set; }

    [DisplayName("Time Out")]
    [DisplayFormat(DataFormatString = "{0:hh:mm:ss tt}")]
    public DateTime TimeOut { get; set; }
    [DisplayName("Working Hours")]
    public TimeSpan WorkingHours { get; set; }
    public TimeSpan TotalTime { get; set; }
    public string Status { get; set; }

    public decimal MyAllowedLeaves { get; set; }
    public decimal MyTakenLeaves { get; set; }
    public decimal MyBalanceLeaves { get; set; }
    public int MyLeavesRefused { get; set; }
    public int MyLeavesPending { get; set; }
    public int MyLeavesApproved { get; set; }

  }
}
