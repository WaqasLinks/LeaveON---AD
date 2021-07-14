using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeaveON.EmailSender
{
  public static class ReplaceValuesInTemplate
  {
    public static string Replace(string emailTemplate)
    {
      //emailTemplate = emailTemplate.Replace("<%EmpName%>", userLeave.AspNetUser.UserName);
      ////if (leave1.IsAccepted1 == 0) emailTemplate = emailTemplate.Replace("<%Status%>", "rejected");
      ////if (leave1.IsAccepted1 == 1) emailTemplate = emailTemplate.Replace("<%Status%>", "accepted");
      ////if (leave1.IsAccepted1 == 2) emailTemplate = emailTemplate.Replace("<%Status%>", "accepted with comments");

      //emailTemplate = emailTemplate.Replace("<%EmpNo%>", userLeave.AspNetUser.BioStarEmpNum.ToString());
      //emailTemplate = emailTemplate.Replace("<%Dept%>", userLeave.AspNetUser.DepartmentName);
      //emailTemplate = emailTemplate.Replace("<%Purpose%>", userLeave.Reason);
      ////emailTemplate = emailTemplate.Replace("<%LineManager%>", "");
      //emailTemplate = emailTemplate.Replace("<%EmergencyContact%>", userLeave.EmergencyContact);
      ////emailTemplate = emailTemplate.Replace("<%Location%>", "");
      //emailTemplate = emailTemplate.Replace("<%FromDate%>", userLeave.StartDate.ToString());
      //emailTemplate = emailTemplate.Replace("<%ToDate%>", userLeave.EndDate.ToString());
      //emailTemplate = emailTemplate.Replace("<%LoggedInUser%>", userLeave.AspNetUser.UserName);
      return emailTemplate;
    }

  }
}
