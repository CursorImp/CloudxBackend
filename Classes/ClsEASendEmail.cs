using EASendMail;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace SignalRHub
{
    public class ClsEASendEmail
    {
        SmtpMail oMail = null;
        public string smtpHostX;

        public static string @KK = string.Empty;

        public string GetKK()
        {

            if (ClsEASendEmail.KK.Length > 0)
            {
                return @KK;
            }
            else
            {
                using (Taxi_Model.TaxiDataContext db = new Taxi_Model.TaxiDataContext())
                {
                    try
                    {
                        @KK = db.ExecuteQuery<string>("select top 1 ApiKey from EmailKeys").FirstOrDefault();
                    }
                    catch (Exception ex)
                    {
                        try
                        {

                            if (ex.Message.ToStr().ToLower().Contains("invalid"))
                            {
                                string con = System.Configuration.ConfigurationManager.AppSettings["ConnectionString"].ToStr();
                                string query = "CREATE TABLE[dbo].[EmailKeys]([Id][int] NOT NULL,[ApiKey] [varchar](max) NULL,CONSTRAINT[PK_EmailKeys] PRIMARY KEY CLUSTERED([Id] ASC)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
                                using (SqlConnection conn = new SqlConnection(Cryptography.Decrypt(con, "softeuroconnskey", true)))
                                {
                                    conn.Open();
                                    SqlCommand cmd = new SqlCommand();
                                    cmd.CommandText = query;
                                    cmd.Connection = conn;
                                    cmd.ExecuteNonQuery();

                                    conn.Close();

                                }


                                @KK = db.ExecuteQuery<string>("select top 1 ApiKey from EmailKeys").FirstOrDefault();

                            }
                        }
                        catch (Exception ex2)
                        {

                        }

                    }
                    if (@KK.ToStr().Length > 0)
                    {
                        @KK = Cryptography.Decrypt(@KK, "tcloudX@@!", true);



                    }
                }


                return @KK;
            }
        }

        public ClsEASendEmail(string fromAddress, string ToAddress
            , string Subject, string Body, string SmtpHost, string emailCC)
        {
            oMail = new SmtpMail(GetKK());

            oMail.From = fromAddress;

            // oMail.To = ToAddress;

            oMail.HtmlBody = Body;
            oMail.Subject = Subject;

            //  oMail.TextBody = Body;

            this.smtpHostX = SmtpHost;


            char[] arr = new char[] { ',' };
            string[] toArr = emailCC.Split(arr);

            foreach (var item in toArr)
            {

                if (item.Length > 0)
                    oMail.Cc.Add(new MailAddress(item.Trim()));
            }

            toArr = ToAddress.Split(arr);

            foreach (var item in toArr)
            {

                if (item.Length > 0)
                    oMail.To.Add(new MailAddress(item.Trim()));
            }
        }

        public void AddAttachment(string fileName)
        {
            if (fileName.ToStr().Trim().Length > 0 && File.Exists(fileName))
                oMail.AddAttachment(fileName);
        }

        public bool Send(string alias, string UserName, string Password)
        {
            bool rtn = false;

            try
            {
                SmtpServer oServer = new SmtpServer(this.smtpHostX);
                oServer.User = UserName;
                oServer.Password = Password;


                if (alias.ToStr().Trim().Length > 0)
                    oServer.Alias = alias;


                // Most mordern SMTP servers require SSL/TLS connection now.
                // ConnectTryTLS means if server supports SSL/TLS, SSL/TLS will be used automatically.
                oServer.ConnectType = SmtpConnectType.ConnectSTARTTLS;

                EASendMail.SmtpClient oSmtp = new EASendMail.SmtpClient();
                oSmtp.SendMail(oServer, oMail);
                oSmtp.Dispose();

                rtn = true;
            }
            catch
            {

            }

            return rtn;

        }




    }
}
