using Repository.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace LMS.Mail
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
        public static void SendEmailUsingLeavON(Leave leave1, string LeavON_Email, string LeavON_Password, AspNetUser sender, AspNetUser receiver, String MessageType)
        {

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
                //mail.From = new MailAddress(sender.Email);
                mail.To.Add(new MailAddress(receiver.Email));
                mail.CC.Add(new MailAddress("hrsupport@intechww.com"));

                //KeyValuePair<string,string> keyValuePair = new KeyValuePair<string, string>();
                List<string> phoneList = System.IO.File.ReadAllLines(Path.Combine(HttpContext.Current.Server.MapPath("~/App_Data/Uploads"), FileName)).ToList<string>();

                //string path = HttpContext.Current.Server.MapPath("~/files/sample.html");
                //string content = System.IO.File.ReadAllText(path);

                switch (MessageType)
                {
                    case "LeaveRequest":
                        mail.Subject = sender.UserName + " posted a Leave request";
                        mail.Body = "Dear , " + receiver.UserName +
                            Environment.NewLine + "I have sent you a leave request. kindly follow the link " + 
                            "https://lms.intechww.com:1001/?ReturnUrl=https://lms.intechww.com:1002/LeavesResponse/Edit/" + leave1.Id + " for detail." +
                            Environment.NewLine + leave1.LeaveType.Name +
                            Environment.NewLine + leave1.Reason +
                            Environment.NewLine + leave1.StartDate +
                            Environment.NewLine + leave1.EndDate +
                            Environment.NewLine + "Best Regards " +
                            Environment.NewLine +
                            sender.UserName +  //string.Format(body, model.FromName, model.FromEmail, model.Message);
                            Environment.NewLine + "This is system generated email, don't reply it";

                        break;
                    case "LeaveResponse":

                        mail.Subject = sender.UserName + " posted a Leave response";
                        mail.Body = "Dear " + receiver.UserName + "," +
                            Environment.NewLine + "I have just sent you feed back regarding your leave request. kindly follow the link " +
                            //"http://lms.intechww.com:1002/LeavesRequest/Index" + " for detail." +
                            //https://localhost:44339/?ReturnUrl=https://localhost:44380/LeavesResponse/Edit/60
                            "https://lms.intechww.com:1001/?ReturnUrl=https://lms.intechww.com:1002/LeavesRequest/Edit/" + leave1.Id + " for detail." +
                             Environment.NewLine + leave1.LeaveType.Name +

                            Environment.NewLine + leave1.Reason +
                            Environment.NewLine + leave1.StartDate +
                            Environment.NewLine + leave1.EndDate +
                            Environment.NewLine + "Best Regards " +
                            Environment.NewLine +
                            sender.UserName +  //string.Format(body, model.FromName, model.FromEmail, model.Message);
                            Environment.NewLine + "This is system generated email, don't reply it";



                        break;

                    default:
                        //return quitely
                        break;

                }






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
