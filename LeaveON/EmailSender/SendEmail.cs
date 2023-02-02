using Repository.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace LeaveON.EmailSender
{
  public static class SendEmail
  {

    public const string LeavON_Email = "LMS@intechww.com";
    public const string LeavON_Password = "Pakistan12345678*";
    /// <summary>
    /// Function will send email.
    /// </summary>
    /// <param name="senderEmail">sender email</param>
    /// <param name="senderPassword">sender password</param>
    /// <param name="receiver">receiver as LMS.Models.Employee Type is required</param>
    /// <param name="MessageType">"LeaveRequest" or "LeaveResponse"</param>
    /// <returns>return type is void</returns>
    /// <remarks>Text put here will not display in a Visual Studio summary box.  
    /// It is meant to add in further detail for anyone who might read this  
    /// code in the future </remarks>
    public static void SendEmailUsingLeavON(Leave userLeave, string LeavON_Email, string LeavON_Password, AspNetUser sender, AspNetUser receiver, String MessageType)
    {
      return;
      MailMessage mail = new MailMessage();
      SmtpClient smtpServer = new SmtpClient("mail.smtp2go.com");
      // SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
      // SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
      // mail.smtp2go.com
      smtpServer.UseDefaultCredentials = false;
      smtpServer.Credentials = new System.Net.NetworkCredential(LeavON_Email, LeavON_Password);
      //smtpServer.Host = "smtp.gmail.com"; not neccesry now. as mention above
      smtpServer.Port = 587; //465;//587; // Gmail works on this port
      smtpServer.EnableSsl = true;
      try
      {

        mail.From = new MailAddress(LeavON_Email);
        mail.To.Add(new MailAddress(receiver.Email));
        mail.CC.Add(new MailAddress("hrsupport@intechww.com"));
        //mail.CC.Add(new MailAddress("waqqasjavaid@gmail.com"));
        //-----------------
        string emailTemplate = System.IO.File.ReadAllText(Path.Combine(HttpContext.Current.Server.MapPath("~/Templates"), "Email.html"));
        emailTemplate = emailTemplate.Replace("<%EmpName%>", userLeave.AspNetUser.UserName);
        if (userLeave.IsAccepted1 == 0) emailTemplate = emailTemplate.Replace("<%Status%>", "rejected");
        if (userLeave.IsAccepted1 == 1) emailTemplate = emailTemplate.Replace("<%Status%>", "accepted");
        if (userLeave.IsAccepted1 == 2) emailTemplate = emailTemplate.Replace("<%Status%>", "accepted with comments");
        if (userLeave.IsAccepted1 == null) emailTemplate = emailTemplate.Replace("<%Status%>", "pending");
        emailTemplate = emailTemplate.Replace("<%EmpNo%>", userLeave.AspNetUser.BioStarEmpNum.ToString());
        emailTemplate = emailTemplate.Replace("<%Dept%>", userLeave.AspNetUser.DepartmentName);
        emailTemplate = emailTemplate.Replace("<%Purpose%>", userLeave.Reason);
        emailTemplate = emailTemplate.Replace("<%EmergencyContact%>", userLeave.EmergencyContact);
        emailTemplate = emailTemplate.Replace("<%Location%>", userLeave.AspNetUser.CntryName);
        emailTemplate = emailTemplate.Replace("<%FromDate%>", userLeave.StartDate.ToString());
        emailTemplate = emailTemplate.Replace("<%ToDate%>", userLeave.EndDate.ToString());
        emailTemplate = emailTemplate.Replace("<%LoggedInUser%>", sender.UserName);

        switch (MessageType)
        {
          case "LeaveRequest":
            mail.Subject = sender.UserName + " posted a Leave request";
            emailTemplate = emailTemplate.Replace("<%Link%>", "https://lms.intechww.com:1001/?ReturnUrl=https://lms.intechww.com:1002/LeavesResponse/Edit/" + userLeave.Id);
            emailTemplate = emailTemplate.Replace("<%LineManager%>", receiver.UserName);
            break;
          case "LeaveResponse":

            mail.Subject = sender.UserName + " posted a Leave response";
            emailTemplate = emailTemplate.Replace("<%Link%>", "https://lms.intechww.com:1001/?ReturnUrl=https://lms.intechww.com:1002/LeavesRequest/Edit/" + userLeave.Id);
            emailTemplate = emailTemplate.Replace("<%LineManager%>", sender.UserName);
            break;

          default:
            //return quitely
            break;
        }

        mail.Body = emailTemplate;
        mail.IsBodyHtml = true;
        smtpServer.Send(mail);
      }
      catch (Exception ex)
      {

        switch (ex.HResult)
        {
          case -2146233088://sender email is wrong
                           //return quitely                  
            break;
          default:
            //return quitely
            break;

        }
      }
    }
    public static void SendLeaveRequestEmail(string senderEmail, string senderPassword, AspNetUser receiver)
    {
      return;
      MailMessage mail = new MailMessage();

      SmtpClient smtpServer = new SmtpClient("mail.smtp2go.com");
      //smtpServer.UseDefaultCredentials = false;
      smtpServer.Credentials = new System.Net.NetworkCredential(senderEmail, senderPassword);
      //smtpServer.Host = "smtp.gmail.com"; not neccesry now. as mention above
      smtpServer.Port = 587; // Gmail works on this port
      smtpServer.EnableSsl = true;

      try
      {

        mail.From = new MailAddress(senderEmail);
        mail.To.Add(new MailAddress(receiver.Email));
        mail.Subject = "Email for Leave approval";
        mail.Body = "Dear Sir, " + receiver.UserName + Environment.NewLine + "I have sent you a leave request. kindly login to LeaveON account " + "http://localhost:44380/LeavesResponse/Index" + " for detial." + Environment.NewLine + "best regards " + Environment.NewLine + senderEmail;  //string.Format(body, model.FromName, model.FromEmail, model.Message);


        smtpServer.Send(mail);
      }
      catch (Exception ex)
      {

        switch (ex.HResult)
        {
          case -2146233088://sender email is wrong
                           //return quitely                  
            break;
          default:
            //return quitely
            break;

        }
      }
    }//SendLeaveRequestEmail

  }
}

//HOD 
//username: kashifzafar.nuces@gmail.com
//password: kashifzafar123

//employee 
//username: aliafzal.nuces@gmail.com
//password: aliafzal123

//----------------------------------

//HOD 
//username: arshadhussain.nuces@gmail.com
//password: arshadhussain123

//employee 
//username: anjumali.nuces@gmail.com
//password: anjumali123
