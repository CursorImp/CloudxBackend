using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Web;
using Taxi_BLL;
using Taxi_Model;
using Utils;

namespace SignalRHub.Classes
{
    public class ClsEmail
    {
        public static List<Attachment> Attachments = new List<Attachment>();

        public static string attachFile;
        public static void Send(string subject, string Emailmessage, string FromEmail, string ToEmail, List<Attachment> attachments, Gen_SubCompany objSubCompany, string attachmentFile)
        {
            //byte[] pdfBytes = File.ReadAllBytes(file.ContentLength);
            // byte[] pdfBytes = new byte[file.ContentLength];
            //using (Stream stream = file.InputStream)
            //{
            //    stream.Read(pdfBytes, 0, file.ContentLength);
            //}
            ////ByteArrayContent pdfContent = new ByteArrayContent(pdfBytes);
            //Attachments = attachments;
            //attachFile = attachmentFile;
            Attachments = attachments;
            attachFile = attachmentFile;

            Send(subject, Emailmessage, FromEmail, ToEmail,objSubCompany);

        }

        public static void Send(string subject, string Emailmessage, string FromEmail, string ToEmail,Gen_SubCompany objSubCompany)
        {


            //Gen_SysPolicy_Configuration obj = Taxi_AppMain.General.GetObject<Gen_SysPolicy_Configuration>(c => c.SysPolicyId == 1);
            //if (obj == null)
            //{
            //    throw new Exception("Email Configuration is not defined in Settings.");


            //}
            //else
            {
                //if (string.IsNullOrEmpty(AppVars.objPolicyConfiguration.SmtpHost) || string.IsNullOrEmpty(AppVars.objPolicyConfiguration.Port) || string.IsNullOrEmpty(AppVars.objPolicyConfiguration.UserName) || string.IsNullOrEmpty(AppVars.objPolicyConfiguration.Password))
                //{
                //    throw new Exception("InComplete Email Configuration. Please contact it to Admin.");

                //}
                //   }

                //string emptyName = "...";

                //string smptHost = "smtp.gmail.com";
                //int port = 587;
                //string userName = "marhamflyer@gmail.com";
                //string pwd = "nlubglwjzbmrzsog";
                //string emailcc = "";
                //bool enableSSL =true;
                //string companyName = "Zeeshan C LTD";
                string emptyName = "...";

                string smptHost = "";
                int port = 587;
                string userName = "";
                string pwd = "";
                string emailcc = "";
                bool enableSSL = true;
                string companyName = "";
                //FromEmail = userName;

             ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                if (objSubCompany != null && objSubCompany.SmtpHost.ToStr().Trim().Length > 0)
                {

                    smptHost = objSubCompany.UseDifferentEmailForInvoices==true?objSubCompany.SmtpInvoiceHost : objSubCompany.SmtpHost.ToStr().Trim();
                    port = objSubCompany.UseDifferentEmailForInvoices==true?objSubCompany.SmtpInvoicePort.ToInt() : objSubCompany.SmtpPort.ToInt();
                    userName = objSubCompany.UseDifferentEmailForInvoices==true?objSubCompany.SmtpInvoiceUserName.ToStr().Trim() : objSubCompany.SmtpUserName.ToStr().Trim();
                    pwd = objSubCompany.UseDifferentEmailForInvoices==true?objSubCompany.SmtpInvoicePassword.ToStr().Trim() : objSubCompany.SmtpPassword.ToStr().Trim();
                    emailcc = objSubCompany.UseDifferentEmailForInvoices==true?objSubCompany.AccountEmailCC.ToStr().Trim() : objSubCompany.EmailCC.ToStr().Trim();
                    enableSSL = objSubCompany.UseDifferentEmailForInvoices==true?objSubCompany.SmtpInvoiceSSL.ToBool() : objSubCompany.SmtpHasSSL.ToBool();
                    companyName = objSubCompany.UseDifferentEmailForInvoices==true?objSubCompany.CompanyName.ToStr().Trim() : objSubCompany.CompanyName.ToStr().Trim();
                    
                    
                    //port = objSubCompany.SmtpPort.ToInt();
                    //userName = objSubCompany.SmtpUserName.ToStr().Trim();
                    //pwd = objSubCompany.SmtpPassword.ToStr().Trim();
                    //emailcc = objSubCompany.EmailCC.ToStr().Trim();
                    //enableSSL = objSubCompany.SmtpHasSSL.ToBool();
                    //companyName = objSubCompany.CompanyName.ToStr().Trim();
                    //FromEmail = userName;


                    //if (subject.ToLower().Contains("invoice") && objSubCompany.IsTpCompany.ToBool() && objSubCompany.UseDifferentEmailForInvoices.ToBool())
                    //{

                    //    smptHost = objSubCompany.SmtpInvoiceHost.ToStr().Trim();
                    //    port = objSubCompany.SmtpInvoicePort.ToInt();
                    //    userName = objSubCompany.SmtpInvoiceUserName.ToStr().Trim();
                    //    pwd = objSubCompany.SmtpInvoicePassword.ToStr().Trim();
                    //    enableSSL = objSubCompany.SmtpInvoiceSSL.ToBool();
                    //    //FromEmail = userName;
                    //}


                }


                //if (AppVars.listUserRights.Count(c => c.functionId == "USE THIRD PARTY EMAIL SERVER") > 0)
                //{
                //    ClsEASendEmail em = new ClsEASendEmail(FromEmail, ToEmail, subject, Emailmessage, smptHost, emailcc);

                //    em.AddAttachment(attachFile);


                //    em.Send(companyName, userName, pwd);


                //}
                //else
                {
                    if (string.IsNullOrEmpty(FromEmail))
                    {
                        if (new TaxiDataContext().ExecuteQuery<string>("select SetVal from AppSettings where setkey= 'EnableThirdPartyEmailSetting'").FirstOrDefault().ToStr() == "true")
                        {
                            string fromEmail = new TaxiDataContext().ExecuteQuery<string>($"select SmtpEmailAddress from Gen_SubCompany WHERE ID={objSubCompany.Id}").FirstOrDefault().ToStr();
                            FromEmail = !string.IsNullOrEmpty(fromEmail) ? fromEmail : FromEmail;
                        }
                        else
                        {
                            FromEmail = objSubCompany.UseDifferentEmailForInvoices == true ? objSubCompany.SmtpInvoiceUserName : objSubCompany.EmailAddress;
                            //FromEmail = userName;
                        }
                    }
                    using (MailMessage message = new MailMessage())
                    {


                        foreach (var item in Attachments)
                        {
                            emptyName = item.Name;

                            message.Attachments.Add(item);
                        }

                        System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient(smptHost, port);
                        smtp.EnableSsl = enableSSL;

                        NetworkCredential mailAuthentication = new NetworkCredential(userName, pwd);


                        char[] arr = new char[] { ',' };
                        string[] toArr = ToEmail.Split(arr);

                        foreach (var item in toArr)
                        {

                            message.To.Add(new MailAddress(item.Trim()));
                        }

                        if (string.IsNullOrEmpty(subject))
                            subject = emptyName;

                        if (string.IsNullOrEmpty(Emailmessage))
                            Emailmessage = emptyName;


                        if (emailcc.Length > 0)
                        {

                            message.CC.Add(emailcc);
                        }



                        message.From = new MailAddress(FromEmail.Trim(), companyName);
                        message.IsBodyHtml = true;
                        message.Subject = subject;
                        message.Body = Emailmessage;
                        //      smtp.DeliveryMethod= SmtpDeliveryMethod.

                        smtp.Credentials = mailAuthentication;

                        FieldInfo transport = smtp.GetType().GetField("transport", BindingFlags.NonPublic | BindingFlags.Instance);
                        FieldInfo authModules = transport.GetValue(smtp).GetType().GetField("authenticationModules", BindingFlags.NonPublic | BindingFlags.Instance);

                        Array modulesArray = authModules.GetValue(transport.GetValue(smtp)) as Array;
                        modulesArray.SetValue(modulesArray.GetValue(3), 1);

                        try
                        {

                            smtp.Send(message);
                        }
                        catch (Exception ex)
                        {

                        }
                    }

                }



            }
        }
        public static bool TestEmailSetting(string subject, string FromEmail, string ToEmail, Gen_SubCompany objSubCompany)
        {
            bool resp = false;
            try
            {

                string emptyName = "...";



                string smptHost = objSubCompany.SmtpHost;
                int port = 587;
                if (!string.IsNullOrEmpty(objSubCompany.SmtpPort)) { port = objSubCompany.SmtpPort.ToInt(); }
                string userName = objSubCompany.SmtpUserName;
                string pwd = objSubCompany.SmtpPassword;
                string emailcc = "";
                bool enableSSL = true;
                string companyName = objSubCompany.CompanyName;
                FromEmail = userName;

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                if (objSubCompany != null && objSubCompany.SmtpHost.ToStr().Trim().Length > 0)
                {

                    smptHost = objSubCompany.SmtpHost.ToStr().Trim();
                    port = objSubCompany.SmtpPort.ToInt();
                    userName = objSubCompany.SmtpUserName.ToStr().Trim();
                    pwd = objSubCompany.SmtpPassword.ToStr().Trim();
                    emailcc = objSubCompany.EmailCC.ToStr().Trim();
                    enableSSL = objSubCompany.SmtpHasSSL.ToBool();
                    companyName = objSubCompany.CompanyName.ToStr().Trim();
                    FromEmail = userName;


                    if (objSubCompany.IsTpCompany.ToBool() && objSubCompany.UseDifferentEmailForInvoices.ToBool())
                    {

                        smptHost = objSubCompany.SmtpInvoiceHost.ToStr().Trim();
                        port = objSubCompany.SmtpInvoicePort.ToInt();
                        userName = objSubCompany.SmtpInvoiceUserName.ToStr().Trim();
                        pwd = objSubCompany.SmtpInvoicePassword.ToStr().Trim();
                        enableSSL = objSubCompany.SmtpInvoiceSSL.ToBool();
                        FromEmail = userName;
                    }


                }
                using (MailMessage message = new MailMessage())
                {


                    System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient(smptHost, port);
                    smtp.EnableSsl = enableSSL;

                    NetworkCredential mailAuthentication = new NetworkCredential(userName, pwd);


                    char[] arr = new char[] { ',' };
                    string[] toArr = ToEmail.Split(arr);

                    foreach (var item in toArr)
                    {

                        message.To.Add(new MailAddress(item.Trim()));
                    }

                    if (string.IsNullOrEmpty(subject))
                        subject = emptyName;


                    if (emailcc.Length > 0)
                    {

                        message.CC.Add(emailcc);
                    }



                    message.From = new MailAddress(FromEmail.Trim(), companyName);
                    message.IsBodyHtml = true;
                    message.Subject = subject;
                    message.Body = subject;
                    //      smtp.DeliveryMethod= SmtpDeliveryMethod.

                    smtp.Credentials = mailAuthentication;

                    FieldInfo transport = smtp.GetType().GetField("transport", BindingFlags.NonPublic | BindingFlags.Instance);
                    FieldInfo authModules = transport.GetValue(smtp).GetType().GetField("authenticationModules", BindingFlags.NonPublic | BindingFlags.Instance);

                    Array modulesArray = authModules.GetValue(transport.GetValue(smtp)) as Array;
                    modulesArray.SetValue(modulesArray.GetValue(3), 1);

                    smtp.Send(message);
                    resp = true;
                }

                return resp;
            }
            catch (Exception ex)
            {
                return false;


            }

        }
    }
}