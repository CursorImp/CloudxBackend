using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Net;
using Utils;

using System.Threading;
using Taxi_BLL;
using Taxi_Model;
using System.Text.RegularExpressions;
using System.IO;
using Newtonsoft;
using System.Diagnostics;
using DotNetCoords;
using System.Web.Script.Serialization;

using System.Collections;
using System.Globalization;
using System.Web;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.Linq.Mapping;

using System.Timers;
using System.Xml;



namespace SignalRHub
{
    [HubName("DispatchHub")]
    public class DispatchHub : Hub
    {
        public DispatchHub()
        {
           Global.LoadDataList();
            //
          
        }

        private HubProcessor Instance
        {
            get { return HubProcessor.Instance; }
        }






      

        #region "Hub Inherited Methods"
        public override Task OnConnected()
        {
            try
            {
                Instance.Connect(Context);
            //    File.AppendAllText(AppContext.BaseDirectory + "\\hublog_OnConnected.txt", DateTime.Now.ToStr() + ": " + "onConnect() -- Query String : SignalRClientsType = " + Context.QueryString["SignalRClientsType"] + " , SignalRUserType = " + Context.QueryString["SignalRUserType"] + " , SignalRClientDomainId = " + Context.QueryString["SignalRClientDomainId"] + "  ||  ConnectionID = " + Context.ConnectionId + " ~~ Total Connections = " + Instance.Connections.Count + Environment.NewLine);
            }
            catch (Exception ex)
            {
                //  File.AppendAllText(AppContext.BaseDirectory + "\\exception_OnConnected.txt", DateTime.Now.ToStr() + ": " + ex.Message + "|" + ex.InnerException.StackTrace + "|" + ex.InnerException.Message + Environment.NewLine);
                //
            }

            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            try
            {
                Instance.Reconnect(Context);
                ////////log.Info("onReconnected() -- Connection ID: " + Context.ConnectionId + " Total Connections  " + HubProcessor.Connections.Count);
            }
            catch (Exception ex)
            {
                //////////log.Error("onReconnected() -- " + ex.Message);
            }
            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            try
            {





                //if (Context.QueryString["SignalRUserType"] != null && Context.QueryString["SignalRUserType"].ToStr() != "3")
                //{

                string s = Context.QueryString["SignalRUserType"].ToStr();

                Instance.Disconnect(Context);


             //   File.AppendAllText(AppContext.BaseDirectory + "\\hublog_OnDisconnected.txt", DateTime.Now.ToStr() + "," + "onDisconnect() -- Total Desktop Connections : " + Instance.ReturnDesktopConnections().Count + ", Mobile Connections : " + Instance.ReturnDriverConnections().Count + " ~~ Total Connections = " + Instance.Connections.Count + Environment.NewLine);

                /// File.AppendAllText(AppContext.BaseDirectory + "\\hublog_DesktopDisconnected.txt", DateTime.Now.ToStr() + ": userttype : " + s + "," + "onDisconnect() -- Connection ID: " + Context.ConnectionId + " ~~ Desktop Connection : " + Instance.ReturnDesktopConnections().Count + ", Mobile Connections : " + Instance.ReturnDriverConnections().Count + " Total Connections = " + Instance.Connections.Count + Environment.NewLine);

                // }
                //  File.AppendAllText(AppContext.BaseDirectory + "\\hublog_OnConnectedDisconnected.txt", DateTime.Now.ToStr() + ": " + "onDisconnect() -- Connection ID: " + Context.ConnectionId + " ~~ Total Connections = " + Instance.Connections.Count + Instance.Connections.Count + Environment.NewLine);

            }
            catch (Exception ex)
            {
                File.AppendAllText(AppContext.BaseDirectory + "\\hublog_OnDisconnected_CATC.txt", DateTime.Now.ToStr() + "," + "TEST" + Environment.NewLine);

                //   File.AppendAllText(AppContext.BaseDirectory + "\\exception_OnConnectedDisconnected.txt", DateTime.Now.ToStr() + ": " + ex.Message + "|" + ex.InnerException.StackTrace + "|" + ex.InnerException.Message + Environment.NewLine);
            }

            return base.OnDisconnected(stopCalled);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
        #endregion

        public void Send(string from, string message)
        {
            if (from.ToLower() == "pda")
            {
              //  // //   //
                Clients.All.addMessageOnDesktop(from, message);
            }
            else
            {
                Clients.All.addMessageOnPDA(from, message);
            }
            //
            // //
        }



        string physicalPath = AppContext.BaseDirectory;
     

        int retryDriverLocTimeout = 1;
        DateTime? lastSaveDriverLocationTimeout = null;
        string companyTelNo = string.Empty;
        private DateTime? lastConnectionDateTime = null;
        //List<clsZones> listOfZone = null;



      

        private enum FORM_MODE
        {
            PAYMENT_FORM,
            THREE_D_SECURE,
            RESULTS
        }

    
        protected internal string m_szPaREQ;
        protected internal string m_szTermURL;
        protected internal string m_szACSURL;
        protected internal string m_szCrossReference;

        public struct COLS_DRIVERLOCATIONS
        {
            public static string VEHICLE_IMAGE = "VEHICLEIMAGE";
            public static string NO = "No";
            public static string DRIVERID = "DRIVERID";

            public static string LOCATION = "Location";
            public static string STATUS = "Status";
            public static string VEHICLE_COLOR = "VEHICLE_COLOR";
            public static string BG_COLOR = "bgcolor";
            public static string PLOT = "Plot";
            public static string PLOTID = "PlotId";


            public static string PLOTDATETIME = "PLOTDATETIME";
            public static string LATITUDE = "LATITUDE";
            public static string LONGITUDE = "LONGITUDE";

            public static string SPEED = "SPEED";
            public static string ETA = "ETA";
            public static string LASTGPSCONTACT = "LASTGPSCONTACT";


        }

        string[] chargesLimit = null;

        Gen_PaymentColumnSetting objPaymentColumns = null;

        private bool LoginDrvOnExpiredDoc = false;

        int PHCVehicleDays = 0;
        int PHCDriverDays = 0;
        int MOTDays = 0;
        int InsuranceDays = 0;
        int MOT2Days = 0;
        int LicenseDays = 0;
     
       
      








      



        private void SetChargesLimit()
        {
            if (chargesLimit == null)
            {
                string charges = Instance.objPolicy.CreditCardExtraChargesLimits.ToStr().Trim();

                if (charges.Length > 0)
                {
                    chargesLimit = charges.Split(new char[] { '|' });
                }
            }
        }

    
        private void RestartProgram()
        {
            try
            {
                //   Instance.Reconnect(Context);
                //Program.IsRestart = true;
                AddLog();
                //SaveData();
                //this.Close();
            }
            catch
            {

            }
        }

        private void AddLog()
        {
            try
            {
                File.AppendAllText(physicalPath + "\\handlesrestartlog.txt", DateTime.Now.ToStr() + ": handles " + Process.GetCurrentProcess().HandleCount + Environment.NewLine);
            }
            catch
            {

            }
        }

    


      

    

        public string ReConnectCallerID(string message)
        {
            //  Global.r

            try
            {

                return "false";
            }
            catch
            {
                return "false";

            }

        }




     

       

        public void MessageToPDA(string message)
        {
            try
            {
                //byte[] inputBuffer = Encoding.UTF8.GetBytes(message);

                //string dataValue = message;
                //dataValue = dataValue.Trim();

                //string[] values = dataValue.Split(new char[] { '=' });

                if (message.StartsWith("request pda="))
                    requestPDA(message);
                else if (message.StartsWith("request refreshzones"))
                {
                    try
                    {
                        Global.LoadDataList(true);
                        //Clients.Caller.cMessageToDesktop("ok");
                    }
                    catch (Exception ex)
                    {
                        Clients.Caller.cMessageToDesktop("exceptionOccured" + ex.Message);
                    }
                }
                else if (message.StartsWith("request dispatchsms="))
                {
                    Instance.listofSMS.Add(message);
                }
                else if (message.StartsWith("request force logout="))
                {


                    string[] values = message.Split('=');


                    try
                    {


                        Instance.listofJobs.Add(new clsPDA
                        {
                            JobId = 0,
                            DriverId = values[2].ToInt(),
                            MessageDateTime = DateTime.Now.AddSeconds(-40),
                            JobMessage = "force logout",
                            MessageTypeId = eMessageTypes.MESSAGING,
                            DriverNo = values[1].ToStr()
                        });


                    }
                    catch (Exception ex)
                    {

                    }

                    SocketIO.SendToSocket(values[2].ToStr(), "force logout", "forceLogout");


                }

                else if (message.StartsWith("request broadcast="))
                {

                    try
                    {

                        File.AppendAllText(AppContext.BaseDirectory + "\\requestbroadcast.txt", DateTime.Now.ToStr() + ": Message :" + message + " : cnt" + message.Split('=').Count() + Environment.NewLine);
                    }
                    catch (Exception ex)
                    {

                    }

                    //string data = message.Split('=')[1];

                    //List<string> listOfConnections = new List<string>();
                    //listOfConnections = Instance.ReturnDesktopConnections();



                    //Clients.Clients(listOfConnections).cMessageToDesktop(data);

                    string data = message.Split('=')[1];




                    if (message.Split('=').Count() > 2)
                    {





                        if (data == "refresh required dashboard")
                        {
                            data = "jsonrefresh required dashboard";

                            DateTime? dt = DateTime.Now.ToDateorNull();
                            DateTime recentDays = dt.Value.AddDays(-1);
                            DateTime dtNow = DateTime.Now;
                            DateTime prebookingdays = dt.Value.AddDays(Instance.objPolicy.HourControllerReport.ToInt()).ToDate();



                            using (TaxiDataContext db = new TaxiDataContext())
                            {
                                //

                                List<stp_GetBookingsDataResult> query = db.ExecuteQuery<stp_GetBookingsDataResult>("exec stp_getbookingsdata {0},{1},{2},{3}", recentDays, prebookingdays, 0, Instance.objPolicy.DaysInTodayBooking.ToInt()).ToList();

                                data += "|>>>|" + Newtonsoft.Json.JsonConvert.SerializeObject(query);
                            }



                        }
                        else if (data == "refresh active booking dashboard")
                        {
                            data = "jsonrefresh active booking dashboard";
                            DateTime? dt = DateTime.Now.ToDateorNull();
                            DateTime recentDays = dt.Value.AddDays(-1);

                            int BookingHours = Instance.objPolicy.DaysInTodayBooking.ToInt();

                            DateTime tillDate = dt.Value.AddHours(BookingHours).Date;

                            if (BookingHours > 0)
                                tillDate = DateTime.Now.ToDateTime().AddHours(BookingHours);


                            using (TaxiDataContext db = new TaxiDataContext())
                            {
                                //
                                List<stp_GetBookingsDataResult> query = db.ExecuteQuery<stp_GetBookingsDataResult>("exec stp_getbookingsdata {0},{1},{2},{3}", recentDays, tillDate, 0, BookingHours).ToList();

                                //   var query = db.stp_GetBookingsData(recentDays, dt.Value.AddHours(BookingHours).Date, 0, BookingHours).ToList();
                                data += "|>>>|" + Newtonsoft.Json.JsonConvert.SerializeObject(query);
                            }
                            //

                        }
                    }

                    General.BroadCastMessage(data);
                  


                    if (message.ToStr().Trim().Contains("=syncdrivers"))
                    {
                        General.BroadCastMessage(RefreshTypes.REFRESH_DASHBOARD_DRIVER);

                        // Clients.Clients(listOfConnections).cMessageToDesktop(RefreshTypes.REFRESH_DASHBOARD_DRIVER);
                    }
                }
                else if (message.StartsWith("**"))
                {
                    //Clients.Caller.cMessageToDesktop("exceptionOccured" + ex.Message);



                    // List<string> listOfConnections = new List<string>();
                    // listOfConnections = Instance.ReturnDesktopConnections();

                    // var finalList= listOfConnections.Remove()

                    // Clients.Clients(listOfConnections).cMessageToDesktop(message.Trim());


                    General.BroadCastMessage(message.Trim());


                    if (message.ToStr().Contains("**autodespatchmode"))
                    {
                        Instance.objPolicy = General.GetObject<Gen_SysPolicy_Configuration>(c => c.SysPolicyId == 1);

                    }
                }



                //try
                //{

                //    File.AppendAllText(AppContext.BaseDirectory + "\\MessageToPDA.txt", DateTime.Now.ToStr() + ": Message :" + message + " :  Total Connections = " + Instance.ReturnDesktopConnections().Count + Environment.NewLine);
                //}
                //catch (Exception ex)
                //{



                //}

            }
            catch(Exception ex)
            {
                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\MessageToPDA_catch.txt", DateTime.Now.ToStr() + ": Message :" + message + " :  Total Connections = " + Instance.ReturnDesktopConnections().Count + ",exception:"+ex.Message+ Environment.NewLine);
                }
                catch (Exception ex2)
                {

                    //

                }

            }

        }


        public string MessageCustomerApp(string message)
        {
            string response = string.Empty;
            try
            {
                string[] values = message.Split(new char[] { '=' });


                if (message.StartsWith("request booking="))
                {
                    try
                    {
                        clsBookingsData objBooking = new clsBookingsData();
                        string jobId = message.Split('=')[1];


                        if (values.Count() > 2 && values[2] == "authorize")
                        {
                            General.BroadCastMessage("**requestauthorize web>>" + Instance.objPolicy.DefaultClientId.ToStr() + ">>" + "XXX" + ">>" + jobId);
                        }
                        else
                        {
                            if (jobId.ToStr().Trim().Length > 0 && jobId.ToStr().Trim().IsNumeric())
                            {
                                using (TaxiDataContext db = new TaxiDataContext())
                                {
                                    objBooking = db.ExecuteQuery<clsBookingsData>("exec stp_GetOnlineBookingsData {0}", jobId).FirstOrDefault();

                                }

                            }

                            if (objBooking.JourneyTypeId == Enums.JOURNEY_TYPES.ONEWAY)
                            {
                                if (Instance.objPolicy.DaysInTodayBooking.ToInt() == 0 && objBooking.PickupDateTemp.ToDate() <= DateTime.Now.ToDate())
                                {
                                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(objBooking);
                                    General.BroadCastMessage("refresh seractive booking dashboard" + " >>> " + json);
                                    //

                                }
                                else if (Instance.objPolicy.DaysInTodayBooking.ToInt() > 0 && objBooking.PickupDateTemp <= DateTime.Now.AddHours(Instance.objPolicy.DaysInTodayBooking.ToInt()))
                                {
                                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(objBooking);
                                    General.BroadCastMessage("refresh seractive booking dashboard" + " >>> " + json);
                                }
                                else
                                {
                                    General.BroadCastMessage("**refresh required dashboard");
                                }
                            }
                            else
                            {
                                General.BroadCastMessage("**refresh required dashboard");
                            }
                        }


                        try
                        {
                            

                            if (objBooking != null && objBooking.BookingTypeId.ToInt() == 11)

                            {
                                string content = "Booking Created from CMAC";

                                if (values.Count() > 4 && values[4].ToStr().ToLower() == "false")
                                    content = "Booking Updated from CMAC";

                                var notification = new { NotificationLabel = "Notification", NotificationContent = "<html> <b><span style=font-size:medium><color=Blue>" + content + "</span></b></html>", Data = objBooking, HtmlData = "<html> <b><span style=font-size:medium><color=Blue>" + content + "</span></b></html>", Width = 300, Height = 75, Value = content };

                                General.BroadCastMessage("**internalmessage>>gennotification>>" + Newtonsoft.Json.JsonConvert.SerializeObject(notification));



                                if (content == "Booking Created from CMAC")
                                {
                                    try
                                    {
                                        string body = General.GetMessage(HubProcessor.Instance.objPolicy.AdvanceBookingSMSText.ToStr(), null, jobId.ToLong());
                                        //AddSMS("07956214979", body, 1);

                                        using (TaxiDataContext db = new TaxiDataContext())
                                        {

                                            var objSub = db.Gen_SubCompanies.Select(args => new { args.SmtpHasSSL, args.SmtpHost, args.SmtpUserName, args.SmtpPassword, args.SmtpPort }).FirstOrDefault();


                                            if (objSub.SmtpUserName.ToStr().Trim().Length > 0)
                                            {
                                                ClsEASendEmail obj = new ClsEASendEmail(objSub.SmtpUserName.ToStr(), "info@atobtransfers.co.uk", "BOOKING CREATED FROM CMAC", body, objSub.SmtpHost.ToStr(), "");
                                                var res = obj.Send(objSub.SmtpUserName.ToStr(), objSub.SmtpUserName.ToStr(), objSub.SmtpPassword.ToStr());

                                                if (res)
                                                {

                                                    try
                                                    {
                                                        File.AppendAllText(physicalPath + "\\MessageCustomerApp_emailsentsuccessCMAC.txt", DateTime.Now.ToStr() + ",values:" + message + Environment.NewLine);
                                                    }
                                                    catch
                                                    {

                                                    }

                                                }
                                                else
                                                {
                                                    try
                                                    {
                                                        File.AppendAllText(physicalPath + "\\MessageCustomerApp_emailsentfailedCMAC.txt", DateTime.Now.ToStr() + ",values:" + message + Environment.NewLine);
                                                    }
                                                    catch
                                                    {

                                                    }

                                                }
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    File.AppendAllText(physicalPath + "\\MessageCustomerApp_emailcrednotfoundCMAC.txt", DateTime.Now.ToStr() + ",values:" + message + Environment.NewLine);
                                                }
                                                catch
                                                {

                                                }
                                            }
                                        }
                                    }
                                    catch
                                    {

                                    }
                                    


                                }

                            }
                        }
                        catch
                        {

                        }


                        General.CallGetDashboardData();

                        try
                        {
                            File.AppendAllText(physicalPath + "\\RequestBookingOnlineSuccess.txt", DateTime.Now.ToStr() + ",values:" + message + Environment.NewLine);
                        }
                        catch
                        {

                        }


                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            File.AppendAllText(physicalPath + "\\RequestBookingOnlineFailed.txt", DateTime.Now.ToStr() + " ,values:" + message + " , exception: " + ex.Message + Environment.NewLine);
                        }
                        catch
                        {

                        }

                    }
                }

                else if (message.StartsWith("request canceljob="))
                {
                    try
                    {
                        using (TaxiDataContext db = new TaxiDataContext())
                        {

                            //
                            long jobId = values[1].ToLong();
                            // Taxi_Model.Booking objBooking = General.GetObject<Taxi_Model.Booking>(c => c.Id == values[1].ToLong());
                            var objBooking = db.Bookings.Where(c => c.Id == jobId)
                               .Select(args => new { args.Id, args.BookingNo, args.DriverId, args.BookingStatusId,args.CustomerName,args.FromAddress,args.ToAddress,args.PickupDateTime }).FirstOrDefault();

                            if (objBooking != null)
                            {
                           
                                response = "true";

                                long bookingId = objBooking.Id;

                                if (values.Count() <= 3)
                                {

                                    string cancelledBy = "App";
                                    if (values.Count() > 2)
                                        cancelledBy = values[2].ToStr();


                                   

                                        db.stp_CancelBooking(bookingId, "Job is cancelled by Customer (From " + cancelledBy + ")", "Customer");
                                   



                                }

                                General.BroadCastMessage("**internalmessage>>" + "request canceljob" + ">>" + objBooking.Id + ">>" + "Job " + objBooking.BookingNo + " is cancelled by Customer" + ">>" + objBooking.CustomerName.ToStr() + ">>" + objBooking.FromAddress.ToStr() + ">>" + objBooking.ToAddress.ToStr() + ">>" + string.Format("{0:dd/MM/yyyy HH:mm}", objBooking.PickupDateTime) + ">>" + "This booking is cancelled from " + values[2].ToStr() + ">>" + objBooking.DriverId.ToInt());


                                try
                                {
                                    int driverId = objBooking.DriverId.ToInt();

                                    if (objBooking.BookingStatusId.ToInt() != Enums.BOOKINGSTATUS.WAITING && driverId > 0)
                                    {
                                        if (General.GetQueryable<Fleet_DriverQueueList>(c => c.DriverId == driverId && c.CurrentJobId == objBooking.Id).Count() > 0)
                                        {
                                            new Thread(delegate ()
                                            {
                                                CancelCurrentBookingFromPDA(bookingId, driverId);
                                            }).Start();
                                        }
                                        else
                                        {
                                            ReCallFOJBookingFromPDA(bookingId, driverId);
                                        }
                                    }
                                }
                                catch 
                                {

                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            File.AppendAllText(physicalPath + "\\requestcanceljob_exception.txt", DateTime.Now.ToStr() + " ,values:" + message + " , exception: " + ex.Message + Environment.NewLine);
                        }
                        catch
                        {

                        }
                    }
                }



          



                else if (message.StartsWith("request app verification code="))
                {
                    response = "true";

                    if (values.Length > 4)
                    {
                        string email = values.Count() >= 5 ? values[5].ToStr().Trim().ToLower() : "";

                        if (email.ToStr().Trim().Length > 0 && General.GetQueryable<Customer>(c => (c.Email != null && c.Email.ToLower() == email) && (c.LoginPassword != null && c.LoginPassword.Length > 0)
                            ).Count() > 0)
                        {
                            response = "This Email is already Registered. Please try a different one";
                        }
                        else
                        {
                            if (values.Count() >= 8 && values[7].ToStr() == "0")
                            {
                                string userId = values[4].ToStr().Trim().ToLower();
                            }
                            else
                            {
                                Random c = new Random();
                                string code = c.Next(1000, 9999).ToStr();

                                string mobileNo = values[1].ToStr();
                                string UDID = values[2].ToStr();
                                string deviceInfo = values[3].ToStr();

                                if (UDID.Length > 36)
                                    UDID = UDID.Substring(0, 35);

                                string companyName = string.Empty;
                                using (TaxiDataContext db = new TaxiDataContext())
                                {
                                    db.stp_RegisteringApp(deviceInfo, UDID, code.ToInt(), mobileNo);

                                    companyName = db.Gen_SubCompanies.Select(a=>a.CompanyName).FirstOrDefault().ToStr();
                                }

                                AddSMS(mobileNo, "Your " + companyName + " App Verification code is :" + code, Enums.SMSACCOUNT_TYPE.MODEMSMS);

                                //General.General.BroadCastMessage("**app verification code>>" + values[1] + ">>" + values[2].ToStr().Trim() + ">>" + values[3].ToStr().Trim());
                            }
                        }
                    }
                    else
                    {
                        Random c = new Random();
                        string code = c.Next(1000, 9999).ToStr();

                        string mobileNo = values[1].ToStr();
                        string UDID = values[2].ToStr();
                        string deviceInfo = values[3].ToStr();

                        if (UDID.Length > 36)
                            UDID = UDID.Substring(0, 35);

                        string companyName = string.Empty;
                        using (TaxiDataContext db = new TaxiDataContext())
                        {
                            db.stp_RegisteringApp(deviceInfo, UDID, code.ToInt(), mobileNo);

                            companyName = db.Gen_SubCompanies.Select(a => a.CompanyName).FirstOrDefault().ToStr();
                        }

                        AddSMS(mobileNo, "Your " + companyName + " App Verification code is :" + code, Enums.SMSACCOUNT_TYPE.MODEMSMS);
                    }
                }

                else if (message.StartsWith("request app user details login="))
                {
                    string userId = values[1].ToStr().Trim().ToLower();
                    string password = values[2].ToStr().Trim().ToLower();
                    string mobileNo = values[3].ToStr().Trim().ToLower();
                    response = "false";

                    if (values.Count() >= 5)
                    {


                        //

                        using (TaxiDataContext db = new TaxiDataContext())
                        {

                            var objApp = db.Customers.Where(c => (c.Email != null && c.Email.ToLower() == values[4].ToLower()) && c.LoginPassword == password && (c.BlackList == null || c.BlackList == false))
                                                .OrderByDescending(c => c.Id).FirstOrDefault();

                            if (objApp != null)
                            {
                                if (message.ToLower().Contains("=yes="))
                                {
                                    AppUser obj = new AppUser();
                                    obj.Address = objApp.Address1.ToStr().Trim().ToUpper();
                                    obj.Email = objApp.Email.ToStr();
                                    obj.UserName = objApp.Name.ToStr();
                                    obj.PhoneNo = objApp.MobileNo.ToStr();
                                    obj.Telephone = objApp.TelephoneNo.ToStr();

                                    obj.PickDetails = objApp.CreditCardDetails.ToStr().Trim();
                                    obj.CustomerId = objApp.Id.ToStr();
                                    // //


                                    //  JobPromotion objP = db.ExecuteQuery<JobPromotion>("select PromotionCode,PromotionTitle,PromotionMessage,PromotionStartDateTime=cast(PromotionStartDateTime as varchar(100)),PromotionEndDateTime=cast( PromotionEndDateTime as varchar(100)),PromotionTypeID,Charges from bookingpromotion where PromotionStartDateTime<=getdate() and PromotionEndDateTime>=getdate() and customerid=" + objApp.Id.ToInt()).FirstOrDefault();

                                    string promo = "";

                                    //if (objP != null)
                                    //{

                                    //    promo = (new JavaScriptSerializer()).Serialize(objP);
                                    //}
                                    //else
                                    //{

                                    promo = "null";
                                    //  }

                                    obj.JobPromotion = promo;


                                    response = "true:" + new JavaScriptSerializer().Serialize(obj);
                                }
                                else
                                {
                                    response = "true";

                                }

                            }
                        }
                    }
                    else
                    {

                        var objApp = General.GetQueryable<Customer>(c => c.Name.ToLower() == userId && c.LoginPassword == password && (c.BlackList == null || c.BlackList == false))
                                                 .OrderByDescending(c => c.Id).FirstOrDefault();

                        if (objApp != null)
                            response = "true";
                    }


                }
             


                else if (message.StartsWith("request authorize user details app code="))
                {
                    string phone = values[2].ToStr().Trim();
                    int code = 0;
                    response = "false";
                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        if (Int32.TryParse(values[1].ToStr(), out code))
                        {

                            var objApp = db.AppsRegistereds.Where(c => c.PhoneNumber == phone && c.RegistrationCode == code)
                                                 .OrderByDescending(c => c.RegisteredOn).FirstOrDefault();

                            if (objApp != null)
                                response = "true";
                        }

                        if (response == "true")
                        {
                            string mobileno= values[2].ToStr().Trim();

                            int Id=  db.Customers.FirstOrDefault(c => c.MobileNo != null && c.MobileNo == mobileno).DefaultIfEmpty().Id;


                            CustomerBO objCustomer = new CustomerBO();
                            if(Id==0)
                            objCustomer.New();
                            else
                            {
                                objCustomer.GetByPrimaryKey(Id);
                                objCustomer.Edit();
                            }


                            objCustomer.Current.Name = values[3].ToStr().Trim();
                            objCustomer.Current.MobileNo = mobileno;
                            objCustomer.Current.TelephoneNo = "";
                            objCustomer.Current.TotalCalls = 0;
                            objCustomer.Current.BlackList = false;
                            objCustomer.Current.BlackListResion = "";
                            objCustomer.Current.Address1 = "";
                            objCustomer.Current.Address2 = "";
                            objCustomer.Current.Email = values[4].ToStr().Trim();
                            objCustomer.Current.LoginPassword = values[5].ToStr().Trim();
                            objCustomer.Current.AddOn = DateTime.Now;
                            objCustomer.CheckDataValidation = false;
                            objCustomer.Save();

                            response = response + "=" + objCustomer.Current.Id;
                        }
                    }
                }


                else if (message.StartsWith("request app tracking="))
                {
                    response = string.Empty;




                    int driverId = values[1].ToInt();

                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        var row = (from a in db.Fleet_DriverQueueLists
                                   join b in db.Fleet_Driver_Locations on a.DriverId equals b.DriverId
                                   join c in db.Fleet_Drivers on b.DriverId equals c.Id
                                   join d in db.Gen_SubCompanies on c.SubcompanyId equals d.Id
                                   join e in db.Fleet_VehicleTypes on c.VehicleTypeId equals e.Id
                                   join f in db.Fleet_DriverWorkingStatus on a.DriverWorkStatusId equals f.Id
                                   where a.DriverId == driverId && a.Status == true
                                   select new
                                   {
                                       f.WorkStatus,
                                       b.Latitude,
                                       b.Longitude,
                                       b.LocationName,
                                       DriverNo = c.DriverNo,
                                       ContactNo = d.TelephoneNo,
                                       DriverName = c.DriverName,
                                       Vehicle = e.VehicleType + "|" + c.VehicleNo + "-" + c.VehicleMake + "-" + c.VehicleModel + "|" + c.VehicleColor,
                                       Rating = c.AvgRating
                                   }).FirstOrDefault();
                        //   //   tradrv
                        if (message.Contains("=json"))
                        {
                            response = "{ \"Latitude\" :\"" + row.Latitude.ToStr() +
                              "\", \"Longitude\":\"" + row.Longitude.ToStr() +
                              "\", \"WorkStatus\":\"" + row.WorkStatus.ToStr().Replace(" ", "").Trim() + "\"," +
                              "\"LocationName\":\"" + row.LocationName.ToStr() + "\"" +
                              ",\"DriverImage\":\"" + "http://tradrv.co.uk/DispatchDriverImages/" + Instance.objPolicy.DefaultClientId.ToStr().Replace("_", "").Trim() + "_" + row.DriverNo.ToStr() + ".jpg\"," +
                               "\"DriverName\":\"" + row.DriverName.ToStr() + "\"," +
                                  "\"Vehicle\":\"" + row.Vehicle.ToStr() + "\"," +
                                     "\"Rating\":\"" + row.Rating.ToStr() + "\"" +

                              ",\"Contact\":\"" + row.ContactNo.ToStr().Trim() + "\" }";
                        }
                        else
                        {
                            response = row.Latitude.ToStr() + "="
                                       + row.Longitude.ToStr() + "=" +
                                         row.WorkStatus.ToStr().Replace(" ", "").Trim() +
                                         "= " + row.LocationName.ToStr()
                                         + "=" + "http://tradrv.co.uk/DispatchDriverImages/" + Instance.objPolicy.DefaultClientId.ToStr() + "_" + row.DriverNo.ToStr() + ".jpg";
                        }
                    }



                   
                }


            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(physicalPath + "\\Messagecustomerapp_exception.txt", DateTime.Now.ToStr() + " : Message : " + message.ToStr() + " , Exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }


            }
            return response;
        }





        public string MessageIVR(string message)
        {
            string response = string.Empty;
            string[] values = null;
            try
            {
                values = message.Split(new string[] { ">>" }, StringSplitOptions.RemoveEmptyEntries);


                try
                {
                    File.AppendAllText(physicalPath + "\\log_MessageIVR.txt", DateTime.Now.ToStr() + " : " + message + Environment.NewLine);
                }
                catch
                {

                }

                if (message.StartsWith("request ivrconfirmbooking>>"))
                {

                   

                    bool IsSerialized = false;
                    IVRNotificationClient objIVR = Newtonsoft.Json.JsonConvert.DeserializeObject<IVRNotificationClient>(values[1]);

                    string notificationmsg = objIVR.NotificationMessage.ToStr();

                    IVRNotification objnotify = Newtonsoft.Json.JsonConvert.DeserializeObject<IVRNotification>(notificationmsg);


                    if (objnotify != null)
                    {

                        DateTime dtX;

                        

                        if (DateTime.TryParseExact(objnotify.PickUpDateTime, "dd/M/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out dtX))
                        {
                            long jobId = objnotify.BookingId;
                            if (dtX < DateTime.Now.AddHours(3))
                            {
                                

                                clsBookingsData objBooking = null;
                                using (TaxiDataContext db = new TaxiDataContext())
                                {
                                    if (jobId.ToStr().Trim().Length > 0 && jobId.ToStr().Trim().IsNumeric())
                                    {

                                        objBooking = db.ExecuteQuery<clsBookingsData>("exec stp_GetIVRBookingsData {0}", jobId).FirstOrDefault();

                                        
                                    }

                                    if (objBooking != null)
                                    {

                                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(objBooking);
                                        objIVR.BookingJson = json;
                                        IsSerialized = true;

                                        General.BroadCastMessage("**internalmessage>>ivrnotification>>" + new JavaScriptSerializer().Serialize(objIVR));

                                        try
                                        {
                                            File.AppendAllText(physicalPath + "\\log_ivrserialized.txt", DateTime.Now.ToStr() + " : " + message + Environment.NewLine);
                                        }
                                        catch
                                        {

                                        }
                                    }
                                }
                            }

                        }


                    }

                    if(IsSerialized==false)
                        General.BroadCastMessage("**internalmessage>>ivrnotification>>" + values[1].ToStr());


                }


            }
            catch(Exception ex)
            {


                try
                {
                    File.AppendAllText(physicalPath + "\\log_messageivrException.txt", DateTime.Now.ToStr() + " : " + message +   ", exception:"+ex.Message+ Environment.NewLine);

                    General.BroadCastMessage("**internalmessage>>ivrnotification>>" + values[1].ToStr());
                }
                catch
                {

                }

            }
            return response;
        }

        public void MessageToPDAByDriverId(string message, string driverID)
        {
            try
            {


                string[] values = message.Split(new char[] { '=' });

                if (message.StartsWith("update settings<<<"))
                    updateDriverSettings(message, driverID);
                else if (message.Contains("logout auth status>>"))
                {
                    logoutAuthStatus(message, driverID);


                    Instance.listofJobs.Add(new clsPDA
                    {
                        DriverId = values[1].ToInt(),
                        JobId = values[2].ToLong(),
                        MessageDateTime = DateTime.Now.AddSeconds(-30),
                        JobMessage = values[3].Split(new String[] { ">>" }, StringSplitOptions.RemoveEmptyEntries)[1].ToStr().Trim(),
                        MessageTypeId = values[4].ToInt()
                    });
                }
                else if (message.Contains("auth status>>yes>>") || message.Contains("auth status>>no>>"))
                {
                    authStatus(message, driverID);


                    Instance.listofJobs.Add(new clsPDA
                    {
                        DriverId = values[1].ToInt(),
                        JobId = values[2].ToLong(),
                        MessageDateTime = DateTime.Now.AddSeconds(-30),
                        JobMessage = values[3].Split(new String[] { ">>" }, StringSplitOptions.RemoveEmptyEntries)[1].ToStr().Trim(),
                        MessageTypeId = values[4].ToInt()
                    });

                    SocketIO.SendToSocket(values[1].ToStr(), values[3].Split(new String[] { ">>" }, StringSplitOptions.RemoveEmptyEntries)[1].ToStr().Trim(), "authStatus");
                    try
                    {
                        File.AppendAllText(physicalPath + "\\MessageToPDAByDriverId.txt", DateTime.Now.ToStr() + " : DriverID:" + values[1].ToStr() + ",Message:" + values[3].Split(new String[] { ">>" }, StringSplitOptions.RemoveEmptyEntries)[1].ToStr().Trim() + Environment.NewLine);


                    }
                    catch
                    {

                    }
                }




            }
            catch
            {

            }

        }

        public void requestRingback(string message)
        {
            try
            {
                string[] values = message.Split(new char[] { '=' });

                string bookingId = values[0].ToStr();
                string custName = values[1].ToStr();
                string mobNo = "";

                string driverId = values[3].ToStr();
                string driverNO = values[4].ToStr();



                using (TaxiDataContext db = new TaxiDataContext())
                {
                    try
                    {
                        db.CommandTimeout = 5;



                        if (companyTelNo.ToStr().Trim().Length == 0)
                            companyTelNo = db.Gen_SubCompanies.FirstOrDefault(c => c.Id != 0).DefaultIfEmpty().TelephoneNo.ToStr().Trim();


                        if (companyTelNo.Contains(" "))
                        {
                            companyTelNo = companyTelNo.Replace(" ", "").Trim();

                        }

                        var objBook = db.stp_GetBookingDetails(bookingId.ToLong()).FirstOrDefault();

                        if (objBook != null)
                        {
                            mobNo = objBook.CustomerMobileNo.ToStr().Trim();


                            if (mobNo.ToStr().Trim().Length == 0)
                                mobNo = objBook.CustomerPhoneNo.ToStr().Trim();


                        }
                    }
                    catch
                    {


                    }
                }

                if (mobNo.Length == 0)
                {
                    mobNo = values[3].ToStr();

                    if (mobNo.Contains("/"))
                    {
                        string[] arr = mobNo.Split(new char[] { '/' });


                        if (arr.Count() > 1)
                            mobNo = arr[0];

                        if (mobNo.ToStr().Trim().StartsWith("07") == false)
                        {

                            mobNo = arr[1].ToStr();
                        }


                    }
                }




                string tokenNO = Instance.objPolicy.CallRecordingToken.ToStr();




                if (mobNo.ToStr().Trim().Length > 0)
                {

                    RingbackVIPCallerRequest requestData = new RingbackVIPCallerRequest()
                    {
                        token = tokenNO,
                        destination = mobNo,
                        extension = "850",
                        callerId = companyTelNo,
                        ringback = true,
                    };

                    RingbackVIPCaller ringbackVIPCaller = new RingbackVIPCaller();

                    string resp = string.Empty;

                    if (tokenNO.ToStr().Contains(","))
                    {
                        resp = ringbackVIPCaller.RingBackEmerald(requestData);
                    }
                    else
                    {
                        resp = ringbackVIPCaller.RingbackYestech(requestData);
                    }


                    Clients.Caller.responseRingback(resp);


                    try
                    {
                        File.AppendAllText(AppContext.BaseDirectory + "ringbacklog.txt", "datastring:" + message + "RESULT : " + resp + " on time :" + DateTime.Now.ToStr());

                        using (TaxiDataContext db = new TaxiDataContext())
                        {
                            db.CommandTimeout = 3;

                            db.stp_BookingLog(bookingId.ToLong(), "Driver", "RINGBACK - Driver (" + driverNO + ")");

                        }
                    }
                    catch
                    {


                    }
                }
                else
                {
                    Clients.Caller.responseRingback("failed:RingBack No number found");
                    //Byte[] byteResponse = Encoding.UTF8.GetBytes("failed:RingBack No number found");
                    //tcpClient.NoDelay = true;
                    //tcpClient.SendTimeout = 6000;

                    //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);

                    try
                    {
                        File.AppendAllText(AppContext.BaseDirectory + AppContext.BaseDirectory + "ringbacklog.txt", "datastring:" + message + "RESULT : RINGBACK No number found" + " on time :" + DateTime.Now.ToStr());

                    }
                    catch
                    {


                    }

                }
            }
            catch (Exception ex)
            {
                Clients.Caller.responseRingback("failed:RingBack Delivery Failed");
                //Byte[] byteResponse = Encoding.UTF8.GetBytes("failed:RingBack Delivery Failed");
                //tcpClient.NoDelay = true;
                //tcpClient.SendTimeout = 6000;

                //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);


                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "ringbacklog_exception.txt", "datastring:" + message + "RESULT : " + ex.Message + ",failed:RingBack Delivery Failed" + " on time :" + DateTime.Now.ToStr());

                }
                catch
                {


                }
            }
        }

        public void requestAllDrivers()
        {
            try
            {
                using (TaxiDataContext db = new TaxiDataContext())
                {

                    var result = db.Fleet_Drivers.Where(c => c.HasPDA != null && c.HasPDA == true && c.IsActive == true).OrderBy(c => c.DriverNo)
                                    .Select(args => args.Id + "," + args.DriverNo + "," + args.DriverName + "," + args.Fleet_VehicleType.VehicleType.ToUpper()).ToArray<string>();

                    Clients.Caller.driverList(result);
                }

                //Byte[] byteResponse = Encoding.UTF8.GetBytes(string.Join(">>", result));
                //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);
            }
            catch (Exception ex)
            {
                Clients.Caller.driverList(ex.Message);
            }
        }

        public void requestDriverSettings(string message)
        {
            try
            {
                //update settings in json Format 
                string dataValue = message;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });
                int driverId = values[1].ToInt();

                using (TaxiDataContext db = new TaxiDataContext())
                {
                    //var obj = General.GetObject<Fleet_Driver_PDASetting>(c => c.DriverId == driverId.ToInt());                    
                    var obj = db.Fleet_Driver_PDASettings.Where(c => c.DriverId == driverId.ToInt()).FirstOrDefault();

                    if (obj != null)
                    {
                        DriverPDASettings pda = new DriverPDASettings();
                        //if (Debugger.IsAttached)
                        //{
                        //    pda.Ip = "202.142.188.106";
                        //}
                        //else
                        //{
                        //pda.Ip = Instance.objPolicy.ListenerIP.ToStr().Trim();
                        //}
                        pda.DrvId = driverId.ToStr();

                        pda.DrvNo = obj.Fleet_Driver.DefaultIfEmpty().DriverNo.ToStr();
                        pda.DrvName = obj.Fleet_Driver.DefaultIfEmpty().DriverName.ToStr();
                        pda.VehType = obj.Fleet_Driver.DefaultIfEmpty().Fleet_VehicleType.VehicleType.ToStr().ToUpper();
                        pda.GPSInterval = "4";

                        pda.EnableJobExtraCharges = ((obj.EnableJobExtraCharges.ToBool() ? "1" : "0"));  // Extra Charges
                        pda.ShowCompletedJobs = ((obj.ShowCompletedJob.ToBool() ? "1" : "0")); // Show Completed Jobs

                        pda.EnableBidding = (obj.EnableBidding.ToBool() ? "1" : "0"); // Enable Bidding

                        pda.ShowPlots = (obj.ShowPlots.ToBool() ? "1" : "0"); // Show Plots  -- index 10

                        pda.ShowNavigation = ((obj.ShowNavigation.ToBool() ? "1" : "0")); // Show Plots -- index 11

                        pda.JobTimeout = ((obj.JobTimeOutInterval.ToStr())); // Show Plots -- index 12
                        pda.ZoneInterval = ((obj.AutoRefreshZoneInterval.ToInt() < 10 ? "15" : obj.AutoRefreshZoneInterval.ToStr())); // Zone Update Interval -- index 13
                        pda.SoundOnZoneChange = ((obj.NotifyOnZoneChange.ToBool() ? "1" : "0")); // Sound On Zone Change -- index 14
                        pda.MessageStayOnScreen = ((obj.MessageStayOnScreen.ToBool() ? "1" : "0")); // Message Stay -- index 15

                        pda.EnableCompanyCars = ((obj.HasCompanyCars.ToBool() ? "1" : "0")); // Show Plots -- index 16

                        pda.EnableFareMeter = ((obj.EnableFareMeter.ToBool() ? "1" : "0")); // Show Plots -- index 18
                        pda.ShowCustomerNo = ((obj.ShowCustomerMobileNo.ToBool() ? "1" : "0")); // Show Plots -- index 19
                        pda.HidePickupAndDest = ((obj.HidePickAndDestination.ToBool() ? "1" : "0")); // Show Plots -- index 20

                        if (pda.HidePickupAndDest == "1")
                        {
                            if (obj.OldPdaVersion.ToInt() > 0)
                            {
                                pda.HidePickupAndDest = obj.OldPdaVersion.ToInt().ToStr();


                            }

                            //if (obj.OldPdaVersion.ToInt() == 1)
                            //{
                            //    pda.HidePickupAndDest = "2";
                            //}
                            //else if (obj.OldPdaVersion.ToInt() == 2)
                            //{
                            //    pda.HidePickupAndDest = "3";
                            //}
                            //else if (obj.OldPdaVersion.ToInt() == 3)
                            //{
                            //    pda.HidePickupAndDest = "4";
                            //}
                        }

                        pda.EnableLogoutOnRejectJob = ((obj.LogoutOnRejectJob.ToBool() ? "1" : "0")); // Show Plots -- index 21

                        pda.FontSize = "20"; // index no 23
                        pda.NavigationType = (obj.NavigationApp.ToStr()); // DeviceId -- index 24

                        pda.EnableFlagDown = ((obj.EnableFlagDown.ToBool() ? "1" : "0")); // -- index 25
                        pda.MessageStayOnScreen = ((obj.MessageStayOnScreen.ToBool() ? "1" : "0")); // -- index 26

                        pda.DisablePanic = ((obj.DisablePanicButton.ToBool() ? "1" : "0")); // index 27
                        pda.DisableRank = ((obj.DisableDriverRank.ToBool() ? "1" : "0")); // index 28

                        pda.MeterVoice = ((obj.EnableFareMeterVoice.ToBool() ? "1" : "0")); // index 28
                        pda.DisableChangeJobPlot = ((obj.DisableChangeJobPlots.ToBool() ? "1" : "0"));// index 30

                        pda.EnableJ15Jobs = ((obj.EnableJ15J30Jobs.ToBool() ? "1" : "0")); // index 31
                        pda.EnableLogoutAuth = ((obj.EnableLogoutAuthorization.ToBool() ? "1" : "0")); // index 32
                        pda.EnableIgnoreArrive = ((obj.IgnoreArriveAction.ToBool() ? "1" : "0")); // index 33
                        pda.BiddingType = ((obj.BiddingType.ToStr().Trim() == string.Empty ? " " : obj.BiddingType.ToStr().Trim())); // index 34

                        pda.FareMeterType = ((obj.FareMeterType.ToStr().Trim() == string.Empty ? " " : obj.FareMeterType.ToStr().Trim())); // index 35

                        pda.EnableOptMeter = ((obj.OptionalFareMeter.ToBool() ? "1" : "0")); // index 36
                        pda.DisableMeterForAccJob = ((obj.DisableFareMeterOnAccJob.ToBool() ? "1" : "0"));// index 37

                        pda.Courier = "0"; // index 38

                        pda.ShowFaresOnExtraCharges = ((obj.ShowFaresOnExtraCharges.ToBool() ? "1" : "0")); // index 39
                        pda.EnableCallCustomer = ((obj.EnableCallCustomer.ToBool() ? "1" : "0")); // index 40

                        pda.EnableRecoverJob = ((obj.EnableRecoverJob.ToBool() ? "1" : "0"));// index 41

                        pda.EnableMeterWaitingCharges = ((obj.EnableFareMeterWaitingCharges.ToBool() ? "1" : "0")); // index 42

                        pda.LogoutOnOverShift = ((obj.LogoutOnOverShift.ToBool() ? "1" : "0")); // // Shift Logout                 
                        pda.DisableBase = ((obj.DisableBase.ToBool() ? "1" : "0")); // //Disable Base // DeviceId -- index 44               
                        pda.DisableBreak = ((obj.DisableOnBreak.ToBool() ? "1" : "0")); // //Disable OnBreak -- index 45
                        pda.DisableRejectJob = ((obj.DisableRejectJob.ToBool() ? "1" : "0")); // //Disable Reject Job

                        pda.DisableChangeDest = ((obj.DisableChangeDestination.ToBool() ? "1" : "0"));

                        pda.ShowJobasAlert = ((obj.ShowJobAsAlert.ToBool() ? "1" : "0"));
                        pda.DisableNoPickup = ((obj.DisableNoPickup.ToBool() ? "1" : "0"));
                        pda.DisableAlarm = ((obj.DisableSetAlarm.ToBool() ? "1" : "0"));
                        pda.ShowSpecialReqOnFront = ((obj.ShowSpecReqOnFront.ToBool() ? "1" : "0"));

                        pda.DisableFareOnAccJob = ((obj.DisableFareOnAccJob.ToBool() ? "1" : "0"));
                        pda.DisableSTC = ((obj.DisableSTC.ToBool() ? "1" : "0"));

                        //version 10.0
                        pda.ShowAlertOnJobLate = ((obj.NotifyOnJobLate.ToBool() ? "1" : "0"));
                        pda.EnableAutoRotate = ((obj.EnableAutoRotateScreen.ToBool() ? "1" : "0"));
                        pda.ShowPlotOnOffer = ((obj.ShowPlotOnJobOffer.ToBool() ? "1" : "0"));

                        pda.OnBreakDur = ((obj.BreakTime.ToStr()));

                        pda.ManualFares = (obj.EnableManualFares.ToBool() ? "1" : "0");

                        pda.EnablePriceBid = ((obj.EnablePriceBidding.ToBool() ? "1" : "0"));

                        pda.DrvWaitingMins = (obj.Fleet_Driver.DefaultIfEmpty().Fleet_VehicleType.DefaultIfEmpty().DriverWaitingChargesPerHour.ToStr());
                        pda.AccWaitingMins = (obj.Fleet_Driver.DefaultIfEmpty().Fleet_VehicleType.AccountWaitingChargesPerHour.ToStr());

                        //need to comment
                        pda.DisableJobAuth = ((obj.DisableRejectJobAuth.ToBool() ? "1" : "0"));


                        pda.showDestAfterPob = (obj.ShowDestinationAfterPOB.ToBool() ? "1" : "0");

                        pda.EnableBidOnPlots = Global.EnableBidOnPlots;
                        pda.DriverPay = Global.DriverPay;
                        pda.enableCallOffice = Global.enableCallOffice;
                        pda.isRingback = Global.enableRingBack;
                        pda.SyncBookingHistory = "1";
                   //     pda.EnableWaitingAfterArrive = "1";
                        // new 598
                        pda.isDriverConnectEnabled = ((obj.EnableDriverConnect.ToBool() ? "1" : "0"));

             //           pda.EnablePickLocation = "1";
              //          pda.EnableExtrasOnFixedFare = "1,0,1";
             //           pda.ExtrasOnSTC = "1";
                      


                      
                        try
                        {
                            string cred = "voipserver1469.vipvoipuk.net,250-voipserver1469,QnqUdyTEpZFsrZ,30001";
                            //  string cred = Instance.objPolicy.DriverConnectCredentials.ToStr().Trim();


                            if (pda.isDriverConnectEnabled.ToStr() == "0")
                                cred = "";

                            string[] drvConnectArr = cred.ToStr().Trim().Split(',');

                            if (drvConnectArr.Count() > 0)
                            {
                                pda.drvConHost = drvConnectArr[0].ToStr();
                                pda.drvConusername = drvConnectArr[1].ToStr();
                                pda.drvConPass = drvConnectArr[2].ToStr();
                                pda.drvConPort = drvConnectArr[3].ToStr();
                            }
                        }
                        catch
                        {


                        }

                        //}
                        //

                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(pda);

                        StringBuilder contents = new StringBuilder();
                        contents.Append("update settings<<<" + json);

                        //send message back to PDA
                        //Clients.Caller.cMessageToPDA(contents.ToString());
                        Clients.Caller.driverSettings(contents.ToString());

                        //Byte[] byteResponse = Encoding.UTF8.GetBytes(contents.ToString());
                        //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);
                    }
                    else
                    {
                        DriverPDASettings pda = new DriverPDASettings();

                        //pda.Ip = Instance.objPolicy.ListenerIP.ToStr().Trim();

                        pda.DrvId = driverId.ToStr();
                        //var objDrv = General.GetObject<Fleet_Driver>(c => c.Id == driverId.ToInt());

                        var objDrv = db.Fleet_Drivers.Where(c => c.Id == driverId.ToInt()).FirstOrDefault();

                        pda.DrvNo = objDrv.DriverNo.ToStr();
                        pda.DrvName = objDrv.DriverName.ToStr();
                        pda.VehType = objDrv.Fleet_VehicleType.VehicleType.ToStr().ToUpper();
                        pda.GPSInterval = "4";

                        pda.EnableJobExtraCharges = (("0"));  // Extra Charges
                        pda.ShowCompletedJobs = (("1")); // Show Completed Jobs

                        pda.EnableBidding = ("0"); // Enable Bidding

                        pda.ShowPlots = ("1"); // Show Plots  -- index 10

                        pda.ShowNavigation = (("1")); // Show Plots -- index 11

                        pda.JobTimeout = (("60")); // Show Plots -- index 12
                        pda.ZoneInterval = (("40")); // Zone Update Interval -- index 13
                        pda.SoundOnZoneChange = (("0")); // Sound On Zone Change -- index 14
                        pda.MessageStayOnScreen = (("1")); // Message Stay -- index 15

                        pda.EnableCompanyCars = ("0"); // Show Plots -- index 16

                        pda.EnableFareMeter = (("0")); // Show Plots -- index 18
                        pda.ShowCustomerNo = (("1")); // Show Plots -- index 19
                        pda.HidePickupAndDest = (("0")); // Show Plots -- index 20

                        pda.EnableLogoutOnRejectJob = (("0")); // Show Plots -- index 21

                        pda.FontSize = "20"; // index no 23
                        pda.NavigationType = ("1"); // DeviceId -- index 24

                        pda.EnableFlagDown = (("0")); // -- index 25
                        pda.MessageStayOnScreen = (("1")); // -- index 26

                        pda.DisablePanic = (("0")); // index 27
                        pda.DisableRank = (("0")); // index 28

                        pda.MeterVoice = (("0")); // index 28
                        pda.DisableChangeJobPlot = (("0"));// index 30

                        pda.EnableJ15Jobs = (("1")); // index 31
                        pda.EnableLogoutAuth = (("0")); // index 32
                        pda.EnableIgnoreArrive = (("0")); // index 33
                        pda.BiddingType = "nearest driver";

                        pda.FareMeterType = "peak";

                        pda.EnableOptMeter = (("0")); // index 36
                        pda.DisableMeterForAccJob = (("0"));// index 37

                        pda.Courier = "0"; // index 38

                        pda.ShowFaresOnExtraCharges = (("0")); // index 39
                        pda.EnableCallCustomer = (("1")); // index 40

                        pda.EnableRecoverJob = (("1"));// index 41

                        pda.EnableMeterWaitingCharges = (("1")); // index 42

                        pda.LogoutOnOverShift = (("0")); // // Shift Logout                 
                        pda.DisableBase = (("0")); // //Disable Base // DeviceId -- index 44               
                        pda.DisableBreak = (("0")); // //Disable OnBreak -- index 45
                        pda.DisableRejectJob = (("0")); // //Disable Reject Job

                        pda.DisableChangeDest = (("0"));

                        pda.ShowJobasAlert = (("0"));
                        pda.DisableNoPickup = (("0"));
                        pda.DisableAlarm = (("0"));
                        pda.ShowSpecialReqOnFront = (("0"));

                        pda.DisableFareOnAccJob = (("0"));
                        pda.DisableSTC = (("0"));

                        // version 10.0
                        pda.ShowAlertOnJobLate = (("0"));
                        pda.EnableAutoRotate = (("0"));
                        pda.ShowPlotOnOffer = (("1"));

                        pda.OnBreakDur = (("60"));

                        pda.ManualFares = ("0");

                        pda.EnablePriceBid = (("0"));

                        pda.DrvWaitingMins = "0";
                        pda.AccWaitingMins = "0";

                        //need to comment
                        pda.DisableJobAuth = (("0"));

                        pda.showDestAfterPob = "0";
                        pda.enableCallOffice = Global.enableCallOffice;
                        pda.isRingback = Global.enableRingBack;

                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(pda);

                        StringBuilder contents = new StringBuilder();
                        contents.Append("update settings<<<" + json);

                        //send message back to PDA
                        //Clients.Caller.cMessageToPDA(contents.ToString());
                        Clients.Caller.driverSettings(contents.ToString());

                        //Byte[] byteResponse = Encoding.UTF8.GetBytes(contents.ToString());
                        //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);

                        //Byte[] byteResponse = Encoding.UTF8.GetBytes("");
                        //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Clients.Caller.driverSettings(ex.Message);
            }
        }

        public void requestCallOffice(string mesg)
        {
            string response = string.Empty;

            try
            {


                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                long jobId = values[0].ToLong();
                int subcompanyId = values[1].ToInt();

                if (Global.customerOfficeNumber.ToStr().Trim().Length == 0)
                {

                    using (TaxiDataContext db = new TaxiDataContext())
                    {

                        //
                        if (subcompanyId == 0)
                        {
                            subcompanyId = db.Bookings.Where(c => c.Id == jobId).Select(c => c.SubcompanyId).FirstOrDefault().ToInt();
                        }

                        if (subcompanyId > 0)
                            response = db.Gen_SubCompanies.Where(c => c.Id == subcompanyId).Select(c => c.TelephoneNo).FirstOrDefault().ToStr().Trim();




                        //try
                        //{

                        //    File.AppendAllText(Application.StartupPath+"\\navigate.txt",DateTime.Now.ToStr()+" response" +response+" datavalue"+dataValue+Environment.NewLine);
                        //}
                        //catch
                        //{

                        //}


                    }
                }
                else
                    response = Global.customerOfficeNumber.ToStr().Trim();

                if (response.ToStr().Trim().Length > 0)
                    response = "success:" + response;
                else
                    response = "failed:" + response;
                // response = "failed:";
                Clients.Caller.callOffice(response);

                try
                {

                    File.AppendAllText(physicalPath + "\\requestcalloffice.txt", DateTime.Now.ToStr() + "request:" + mesg + ", response" + response + " datavalue" + dataValue + Environment.NewLine);
                }
                catch
                {

                }
            }
            catch (Exception ex)
            {
                try
                {
                    Clients.Caller.callOffice("failed:Invalid Data");
                    File.AppendAllText(physicalPath + "\\requestcalloffice.txt", DateTime.Now.ToStr() + "request:" + mesg + ", response" + response + " ,exception : " + ex.Message + Environment.NewLine);
                }
                catch
                {

                }


            }
        }

        public void requestShiftLogin(string dataValue)
        {
            try
            {
                //await Task.Run(() =>
                //{
                try
                {
                    string[] values = dataValue.Split(new char[] { '=' });
                    values = dataValue.Split(new char[] { ',' });

                    int valueCnt = values.Count();

                    int driverId = values[1].ToInt();
                    string driverno = values[2].ToStr();
                    string password = values[3].ToStr();
                    decimal? pdaversion = null;
                    string msg = "true";
                    string alertMsg = string.Empty;

                    int fleetMasterId = 0;
                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        try
                        {
                            var objDriver = db.Fleet_Drivers.FirstOrDefault(c => c.Id == driverId && c.LoginPassword == password && c.IsActive == true);

                            if (objDriver != null)
                            {
                                DateTime now = DateTime.Now;

                                if (valueCnt > 6)
                                {
                                    DateTime prevDate = DateTime.Now.AddDays(-1);

                                    Fleet_DriverQueueList objQueue = db.Fleet_DriverQueueLists.Where(c => c.DriverId == driverId).OrderByDescending(c => c.Id)
                                            .FirstOrDefault(c => c.Status == true && c.LoginDateTime.Value.Date >= prevDate.Date);

                                    if (objQueue != null && !string.IsNullOrEmpty(objDriver.DeviceId.ToStr().Trim())
                                      && values[5].ToStr().Trim().Length > 0 && values[5].ToStr().Trim() != objDriver.DeviceId.ToStr().Trim())
                                    {

                                        msg = msg.Replace("true", "");
                                        msg = "Driver is already login from another pda" + ",";
                                    }
                                }

                                if (!objDriver.PDALoginBlocked.ToBool())
                                {
                                    msg = msg.Replace("true", "");
                                    msg += "Your Rent is due. Please call office,";
                                }
                                else
                                {
                                    if (objDriver.RentLimit.ToDecimal() > 0 && Global.DriverPay.ToStr() == "1")
                                    {
                                        try
                                        {
                                            string version = string.Empty;
                                            if (dataValue.Contains("ver=") && (values.Count() == 5 || values.Count() == 7))
                                            {


                                                if (values.Count() == 5)
                                                {
                                                    version = values[4].Replace("ver=", "").Trim();
                                                }
                                                else if (values.Count() == 7)
                                                {
                                                    version = values[6].Replace("ver=", "").Trim();

                                                    if (version.ToStr().Trim().Length > 0 && version.IsNumeric() == false)
                                                    {
                                                        string verVal = version.ToStr().Trim();
                                                        version = string.Empty;
                                                        for (int i = 0; i < verVal.Count(); i++)
                                                        {
                                                            if (verVal[i].ToStr().IsNumeric() || verVal[i].ToStr() == ".")
                                                            {
                                                                version += verVal[i].ToStr();

                                                            }
                                                            else
                                                                break;
                                                        }
                                                    }
                                                }

                                                if (!string.IsNullOrEmpty(version))
                                                {
                                                    if (version.IsNumeric())
                                                    {
                                                        pdaversion = version.ToDecimal();
                                                    }
                                                    else
                                                    {
                                                        if (version.Contains("."))
                                                        {
                                                            try
                                                            {
                                                                string[] arr = version.Split(new char[] { '.' });

                                                                if (arr[0].IsNumeric())
                                                                {
                                                                    pdaversion = arr[0].ToDecimal();
                                                                }

                                                                if (arr.Count() > 1 && arr[1][0].ToStr().IsNumeric() && arr[1][1].ToStr().IsNumeric())
                                                                {
                                                                    pdaversion = (arr[0] + "." + arr[1][0] + arr[1][1]).ToDecimal();
                                                                }
                                                            }
                                                            catch
                                                            {

                                                            }
                                                        }
                                                    }
                                                }
                                            }


                                            if (pdaversion.ToDecimal() >= 41.55m && pdaversion.ToDecimal() <= 45)
                                            {
                                                //
                                                string driverPay = GetDriverPay(driverId, objDriver, pdaversion.ToStr());

                                                if (driverPay.Length > 0)
                                                {
                                                    msg = msg.Replace("true", "balance:");
                                                    msg += driverPay;

                                                    Clients.Caller.shiftLogin(msg.ToStr());
                                                    return;
                                                }
                                            }

                                            //DateTime? fromDate = Instance.objPolicy.RentFromDateTime.ToDateTimeorNull();
                                            //DateTime? toDate = Instance.objPolicy.RentToDateTime.ToDateTimeorNull();

                                            //int subtracted = (7 - (int)fromDate.Value.DayOfWeek);
                                            //int fromRentWeek = (int)fromDate.Value.DayOfWeek;
                                            //if (fromRentWeek == 0)
                                            //    fromRentWeek = 7;

                                            //int dayofWEEK = (int)DateTime.Now.DayOfWeek;
                                            //int DaysToSubtract = 7 - (int)toDate.Value.DayOfWeek;

                                            //if (dayofWEEK == 0)
                                            //    dayofWEEK = 7;

                                            //if (dayofWEEK < fromRentWeek)
                                            //    subtracted = fromRentWeek - dayofWEEK;
                                            //else if (dayofWEEK == fromRentWeek)
                                            //{
                                            //    subtracted = 0;
                                            //}
                                            //else
                                            //    subtracted = dayofWEEK - fromRentWeek;

                                            //DateTime dtFrom = DateTime.Now.AddDays((-7)).AddDays(7 - subtracted).ToDate();
                                            //dtFrom = dtFrom.AddHours(fromDate.Value.Hour);
                                            //dtFrom = dtFrom.AddMinutes(fromDate.Value.Minute);

                                            //int toRentWeek = (int)toDate.Value.DayOfWeek;

                                            //DateTime dtTo = dtFrom.AddDays(7).ToDate();
                                            //List<DateTime> allDates = new List<DateTime>();

                                            //int starting = dtFrom.Day;
                                            //int ending = dtTo.Day;

                                            //for (int i = starting; i <= ending; i++)
                                            //{
                                            //    allDates.Add(new DateTime(dtFrom.Year, dtFrom.Month, i));
                                            //}
                                            ////
                                            //dtTo = allDates.FirstOrDefault(c => c.DayOfWeek.ToInt() == toRentWeek).ToDate();
                                            //dtTo = dtTo.AddHours(toDate.Value.Hour);
                                            //dtTo = dtTo.AddMinutes(toDate.Value.Minute);

                                            //var totalEarning = db.Bookings.Where(c => c.DriverId == driverId && c.BookingStatusId == Enums.BOOKINGSTATUS.DISPATCHED &&
                                            //    (c.PickupDateTime >= dtFrom && c.PickupDateTime <= dtTo)).Sum(c => c.TotalCharges);

                                            //try
                                            //{
                                            //    File.AppendAllText(physicalPath + "\\rentlimitlog.txt", DateTime.Now.ToStr() + ":" + dtFrom.ToStr() + ",:" + dtTo.ToStr() + ",:" + totalEarning + Environment.NewLine);
                                            //}
                                            //catch
                                            //{

                                            //}

                                            //DateTime? suspensionTime = DateTime.Now;

                                            //if (Instance.objPolicy.DriverSuspensionDateTime != null)
                                            //{
                                            //    suspensionTime = suspensionTime.Value.AddHours(Instance.objPolicy.DriverSuspensionDateTime.Value.Hour);
                                            //}

                                            //if (DateTime.Now >= suspensionTime)
                                            //{
                                            //    if (totalEarning > 0 && totalEarning >= objDriver.RentLimit.ToDecimal())
                                            //    {
                                            //        msg = msg.Replace("true", "");
                                            //        msg += "Rent Limit Reached. Please Contact Office,";
                                            //    }
                                            //}
                                        }
                                        catch (Exception ex)
                                        {
                                            try
                                            {
                                                File.AppendAllText(physicalPath + "\\RentlimitException.txt", DateTime.Now.ToStr() + ":" + ex.Message + Environment.NewLine);
                                            }
                                            catch
                                            {

                                            }
                                        }
                                    } // END RENT LIMIT
                                }

                                string expiredmsg = string.Empty;

                                if (objDriver.DrivingLicenseExpiryDate != null)
                                {
                                    if (objDriver.DrivingLicenseExpiryDate.Value < now)
                                    {
                                        if (LoginDrvOnExpiredDoc == false)
                                        {
                                            msg = msg.Replace("true", "");
                                            msg += "Driving License is Expired" + ",";
                                        }
                                        else
                                        {
                                            expiredmsg += "Driving License is Expired : " + string.Format("{0:dd/MM/yy HH:mm}", objDriver.DrivingLicenseExpiryDate.Value) + ",";
                                        }
                                    }

                                    if (objDriver.DrivingLicenseExpiryDate >= now.Date && objDriver.DrivingLicenseExpiryDate <= now.AddDays(this.LicenseDays))
                                    {
                                        if (string.IsNullOrEmpty(alertMsg))
                                            alertMsg = "alert";

                                        alertMsg += ",License expiry=" + string.Format("{0:dd/MM/yyyy}", objDriver.DrivingLicenseExpiryDate);
                                    }
                                }
                                if (objDriver.InsuranceExpiryDate != null)
                                {
                                    if (objDriver.InsuranceExpiryDate.Value < now)
                                    {
                                        if (LoginDrvOnExpiredDoc == false)
                                        {
                                            msg = msg.Replace("true", "");
                                            msg += "Driver Insurance is Expired" + ",";
                                        }
                                        else
                                        {
                                            expiredmsg += "Driver Insurance is Expired : " + string.Format("{0:dd/MM/yy HH:mm}", objDriver.InsuranceExpiryDate.Value) + ",";
                                        }
                                    }

                                    if (objDriver.InsuranceExpiryDate >= now.Date && objDriver.InsuranceExpiryDate <= now.AddDays(this.InsuranceDays))
                                    {
                                        if (string.IsNullOrEmpty(alertMsg))
                                            alertMsg = "alert";

                                        alertMsg += ",Insurance Expiry=" + string.Format("{0:dd/MM/yyyy}", objDriver.InsuranceExpiryDate);
                                    }
                                }



                                if (objDriver.RoadTaxiExpiryDate != null)
                                {
                                    if (objDriver.RoadTaxiExpiryDate.Value < now)
                                    {
                                        if (LoginDrvOnExpiredDoc == false)
                                        {
                                            msg = msg.Replace("true", "");
                                            msg += "Driver Road Tax is Expired" + ",";
                                        }
                                        else
                                        {
                                            expiredmsg += "Driver Road Tax is Expired : " + string.Format("{0:dd/MM/yy HH:mm}", objDriver.RoadTaxiExpiryDate.Value) + ",";
                                        }
                                    }

                                    if (objDriver.RoadTaxiExpiryDate >= now.Date && objDriver.RoadTaxiExpiryDate <= now.AddDays(this.InsuranceDays))
                                    {
                                        if (string.IsNullOrEmpty(alertMsg))
                                            alertMsg = "alert";

                                        alertMsg += ",Road Tax Expiry=" + string.Format("{0:dd/MM/yyyy}", objDriver.RoadTaxiExpiryDate);
                                    }
                                }


                                if (objDriver.MOTExpiryDate != null)
                                {
                                    if (objDriver.MOTExpiryDate.Value < now)
                                    {
                                        if (LoginDrvOnExpiredDoc == false)
                                        {
                                            msg = msg.Replace("true", "");
                                            msg += "Driver MOT is Expired" + ",";
                                        }
                                        else
                                        {
                                            expiredmsg += "Driver MOT is Expired : " + string.Format("{0:dd/MM/yy HH:mm}", objDriver.MOTExpiryDate.Value) + ",";
                                        }
                                    }

                                    if (objDriver.MOTExpiryDate >= now.Date && objDriver.MOTExpiryDate <= now.AddDays(this.MOTDays))
                                    {
                                        if (string.IsNullOrEmpty(alertMsg))
                                            alertMsg = "alert";

                                        alertMsg += ",MOT Expiry=" + string.Format("{0:dd/MM/yyyy}", objDriver.MOTExpiryDate);
                                    }
                                }
                                if (objDriver.MOT2ExpiryDate != null)
                                {
                                    if (objDriver.MOT2ExpiryDate.Value < now && LoginDrvOnExpiredDoc == false)
                                    {
                                        msg = msg.Replace("true", "");
                                        msg += "Driver MOT 2 is Expired" + ",";
                                    }

                                    if (objDriver.MOT2ExpiryDate >= now.Date && objDriver.MOT2ExpiryDate <= now.AddDays(this.MOT2Days))
                                    {
                                        if (string.IsNullOrEmpty(alertMsg))
                                            alertMsg = "alert";

                                        alertMsg += ",MOT 2 Expiry=" + string.Format("{0:dd/MM/yyyy}", objDriver.MOT2ExpiryDate);
                                    }
                                }
                                if (objDriver.PCODriverExpiryDate != null)
                                {
                                    if (objDriver.PCODriverExpiryDate.Value < now)
                                    {
                                        if (LoginDrvOnExpiredDoc == false)
                                        {
                                            msg = msg.Replace("true", "");
                                            msg += "Driver PCO is Expired" + ",";
                                        }
                                        else
                                        {
                                            expiredmsg += "Driver PCO is Expired : " + string.Format("{0:dd/MM/yy HH:mm}", objDriver.PCODriverExpiryDate.Value) + ",";
                                        }
                                    }

                                    if (objDriver.PCODriverExpiryDate >= now.Date && objDriver.PCODriverExpiryDate <= now.AddDays(this.PHCDriverDays))
                                    {
                                        if (string.IsNullOrEmpty(alertMsg))
                                            alertMsg = "alert";

                                        alertMsg += ",PCO Expiry=" + string.Format("{0:dd/MM/yyyy}", objDriver.PCODriverExpiryDate);
                                    }
                                }








                                if (objDriver.PCOVehicleExpiryDate != null)
                                {
                                    if (objDriver.PCOVehicleExpiryDate.Value < now)
                                    {
                                        if (LoginDrvOnExpiredDoc == false)
                                        {
                                            msg = msg.Replace("true", "");
                                            msg += "Driver Vehicle PHC is Expired";
                                        }
                                        else
                                        {
                                            expiredmsg += "Driver Vehicle PHC is Expired : " + string.Format("{0:dd/MM/yy HH:mm}", objDriver.PCOVehicleExpiryDate.Value) + ",";
                                        }
                                    }

                                    if (objDriver.PCOVehicleExpiryDate >= now.Date && objDriver.PCOVehicleExpiryDate <= now.AddDays(this.PHCVehicleDays))
                                    {
                                        if (string.IsNullOrEmpty(alertMsg))
                                            alertMsg = "alert";

                                        alertMsg += ",Vehicle PHC Expiry=" + string.Format("{0:dd/MM/yyyy}", objDriver.PCOVehicleExpiryDate);
                                    }
                                }

                                if (!string.IsNullOrEmpty(expiredmsg))
                                {
                                    string pdaMsg = "request pda=" + objDriver.Id + "=" + 0 + "="
                                                         + "Message>>" + expiredmsg + ">>" + String.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Now) + "=4";

                                    string[] splitArr = pdaMsg.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);

                                    Instance.listofJobs.Add(new clsPDA
                                    {
                                        DriverId = splitArr[1].ToInt(),
                                        JobId = 0,
                                        MessageDateTime = DateTime.Now.AddSeconds(-50),
                                        JobMessage = splitArr[3].ToStr().Trim(),
                                        MessageTypeId = splitArr[4].ToInt()
                                    });
                                }

                                //if (valueCnt > 6)
                                //{
                                //    string vehNo = values[4];
                                //    if (vehNo.ToStr().Equals("9988") == false)
                                //    {
                                //        //string vehNo = "APH-319411";
                                //        Fleet_Master objFleet = db.Fleet_Masters.FirstOrDefault(c => c.Plateno == vehNo);

                                //        if (objFleet == null)
                                //        {
                                //            msg = msg.Replace("true", "");
                                //            msg += "Invalid Vehicle Plate No";
                                //        }
                                //        else
                                //        {
                                //            //DateTime prevDate = DateTime.Now.AddDays(-1);
                                //            if (objFleet.Fleet_DriverQueueLists.Count(c => c.DriverId != driverId && c.Status == true) > 0)
                                //            {
                                //                msg = msg.Replace("true", "");
                                //                msg += "Vehicle already in use";
                                //            }
                                //            else
                                //            {
                                //                fleetMasterId = objFleet.Id;
                                //            }
                                //        }
                                //    }
                                //}
                                if (valueCnt > 6)
                                {
                                    string vehNo = values[4];
                                    if (vehNo.ToStr().Equals("9988") == false)
                                    {
                                        //string vehNo = "APH-319411";
                                        Fleet_Master objFleet = db.Fleet_Masters.FirstOrDefault(c => c.Plateno == vehNo || c.VehicleID == vehNo || c.VehicleNo == vehNo);

                                        if (objFleet == null)
                                        {
                                            msg = msg.Replace("true", "");
                                            msg += "Invalid Vehicle ID";
                                        }
                                        else
                                        {
                                            //DateTime prevDate = DateTime.Now.AddDays(-1);
                                            if (objFleet.Fleet_DriverQueueLists.Count(c => c.DriverId != driverId && c.Status == true) > 0)
                                            {
                                                msg = msg.Replace("true", "");
                                                msg += "Vehicle already in use";
                                            }
                                            else if (db.Fleet_Driver_CompanyVehicles.Where(c => c.DriverId == driverId).Count() > 0 && db.Fleet_Driver_CompanyVehicles.Count(c => c.DriverId == driverId && c.FleetMasterId == objFleet.Id) == 0)
                                            {
                                                msg = msg.Replace("true", "");
                                                msg += "Driver Doesn't have this Company Vehicle";

                                            }
                                            else
                                            {
                                                fleetMasterId = objFleet.Id;
                                            }
                                        }
                                    }
                                }

                                if (msg.EndsWith(","))
                                {
                                    msg = msg.Remove(msg.LastIndexOf(','));
                                }
                            }
                            else
                            {
                                msg = "false";
                            }

                            if (msg == "true" && !string.IsNullOrEmpty(alertMsg) && Instance.objPolicy.PDAVersion.ToDecimal() >= 1.7m)
                            {
                                msg = alertMsg;
                            }

                            if (msg == "true" || msg.StartsWith("alert,"))
                            {
                                bool IsExist = false;
                                foreach (var item in objDriver.Fleet_Driver_Shifts)
                                {
                                    if (item.Driver_Shift_ID != null && item.FromTime != null && item.ToTime != null)
                                    {
                                        if (item.Driver_Shift_ID.ToInt() == 7)
                                            IsExist = true;

                                        if (item.Driver_Shift_ID.ToInt() != 7 && IsExist == false)
                                        {
                                            string str = DateTime.Now.TimeOfDay.ToStr();

                                            str = str.Substring(0, str.LastIndexOf(':'));
                                            str = str.Replace(":", "").Trim();

                                            int time = str.ToInt();

                                            str = item.FromTime.Value.TimeOfDay.ToStr();
                                            str = str.Substring(0, str.LastIndexOf(':'));
                                            str = str.Replace(":", "").Trim();
                                            int fromTime = str.ToInt();

                                            str = item.ToTime.Value.TimeOfDay.ToStr();
                                            str = str.Substring(0, str.LastIndexOf(':'));
                                            str = str.Replace(":", "").Trim();
                                            int toTime = str.ToInt();

                                            if (time < 1000)
                                            {
                                                // PEAK FARES
                                                if (fromTime < 1000 && toTime < 1000)
                                                {
                                                    if (time >= fromTime && time <= toTime)
                                                    {
                                                        IsExist = true;
                                                    }
                                                }
                                                // 6 AM (600) TO 15 PM (1500)
                                                else if (fromTime < 1000 && toTime > 1000)
                                                {
                                                    if (time >= fromTime && time <= toTime)
                                                    {
                                                        IsExist = true;
                                                    }
                                                }
                                                // 6 PM (1800) TO 6 AM (600)
                                                else if (fromTime > 1000 && toTime < 1000)
                                                {
                                                    if (time <= toTime)
                                                    {
                                                        IsExist = true;
                                                    }
                                                }

                                                // OFF PEAK FARES
                                                if (fromTime < 1000 && toTime < 1000)
                                                {
                                                    if (time >= fromTime
                                                            && time <= toTime)
                                                    {
                                                        IsExist = true;
                                                    }
                                                }
                                                // 6 AM (600) TO 15 PM (1500)
                                                else if (fromTime < 1000 && toTime > 1000)
                                                {
                                                    if (time >= fromTime
                                                            && time <= toTime)
                                                    {
                                                        IsExist = true;
                                                    }
                                                }
                                                // 6 PM (1800) TO 6 AM (600)
                                                else if (fromTime > 1000 && toTime < 1000)
                                                {

                                                    if (time <= toTime)
                                                    {
                                                        IsExist = true;
                                                    }
                                                }
                                            }
                                            else if (time >= 1000)
                                            {
                                                if ((fromTime < 1000 && toTime >= 1000)
                                                        || (fromTime >= 1000 && toTime >= 1000))
                                                {
                                                    // 6 AM (600) TO 6PM (1700)
                                                    if (time >= fromTime && time <= toTime)
                                                    {
                                                        IsExist = true;
                                                    }
                                                    else if ((fromTime >= 1000 && toTime < 1000))
                                                    {
                                                        if (time >= fromTime)
                                                        {
                                                            IsExist = true;
                                                        }
                                                    }
                                                    //else if ((toTime > fromTime && time < (toTime - fromTime))
                                                    //    || (fromTime > toTime && time > (fromTime - toTime)))
                                                    //{
                                                    //    IsExist = true;
                                                    //}
                                                }
                                                else if ((fromTime < 1000 && toTime >= 1000)
                                                        || (fromTime >= 1000 && toTime >= 1000))
                                                {
                                                    // 6 AM (600) TO 6PM (1700)
                                                    if (time >= fromTime
                                                            && time <= toTime)
                                                    {
                                                        IsExist = true;
                                                    }
                                                }
                                                else if ((fromTime >= 1000 && toTime < 1000))
                                                {
                                                    // 6 AM (600) TO 6PM (1700)
                                                    if (time >= fromTime)
                                                    {
                                                        IsExist = true;
                                                    }
                                                }
                                            }
                                        }

                                        if (item.Driver_Shift_ID.ToInt() != 7)
                                        {
                                            msg += ">>" + item.Driver_Shift.ShiftName.ToStr().Trim()
                                           + "," + string.Format("{0:HH:mm}", item.FromTime) + "," + string.Format("{0:HH:mm}", item.ToTime);
                                        }
                                    }
                                }

                                if (!IsExist)
                                {
                                    if (msg.StartsWith("true"))
                                    {
                                        msg = msg.Replace("true", "overshift").Trim();
                                    }
                                    else if (msg.StartsWith("alert,"))
                                    {
                                        msg = msg.Replace("alert", "overshift").Trim();
                                    }
                                }
                            }

                            if (msg != "true" && msg != "false" && msg.Contains(">>") == false)
                                msg += ">> ";

                            //send message back to PDA
                            Clients.Caller.shiftLogin(msg.ToStr());

                            //Byte[] byteResponse = Encoding.UTF8.GetBytes(msg.ToStr());
                            //tcpClient.Client.Send(byteResponse);
                            //tcpClient.Client.NoDelay = true;

                            //tcpClient.Close();
                            //clientStream.Close();
                            //clientStream.Dispose();
                            // tcpClient.GetStream().Close();
                            // tcpClient.GetStream().Dispose();
                            // tcpClient = null;
                            GC.Collect();

                            if (msg.StartsWith("true") || msg.StartsWith("alert,"))
                            {
                                //   decimal? pdaversion = null;

                                if (dataValue.Contains("ver=") && (values.Count() == 5 || values.Count() == 7))
                                {
                                    string ver = string.Empty;

                                    if (values.Count() == 5)
                                    {
                                        ver = values[4].Replace("ver=", "").Trim();
                                    }
                                    else if (values.Count() == 7)
                                    {
                                        ver = values[6].Replace("ver=", "").Trim();

                                        if (ver.ToStr().Trim().Length > 0 && ver.IsNumeric() == false)
                                        {
                                            string verVal = ver.ToStr().Trim();
                                            ver = string.Empty;
                                            for (int i = 0; i < verVal.Count(); i++)
                                            {
                                                if (verVal[i].ToStr().IsNumeric() || verVal[i].ToStr() == ".")
                                                {
                                                    ver += verVal[i].ToStr();

                                                }
                                                else
                                                    break;
                                            }
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(ver))
                                    {
                                        if (ver.IsNumeric())
                                        {
                                            pdaversion = ver.ToDecimal();
                                        }
                                        else
                                        {
                                            if (ver.Contains("."))
                                            {
                                                try
                                                {
                                                    string[] arr = ver.Split(new char[] { '.' });

                                                    if (arr[0].IsNumeric())
                                                    {
                                                        pdaversion = arr[0].ToDecimal();
                                                    }

                                                    if (arr.Count() > 1 && arr[1][0].ToStr().IsNumeric() && arr[1][1].ToStr().IsNumeric())
                                                    {
                                                        pdaversion = (arr[0] + "." + arr[1][0] + arr[1][1]).ToDecimal();
                                                    }
                                                }
                                                catch
                                                {

                                                }
                                            }
                                        }
                                    }
                                }

                                if (fleetMasterId == 0)
                                {
                                    db.stp_LoginLogoutDriver(driverId, true, pdaversion);
                                }
                                else
                                {
                                    db.stp_LoginLogoutDriverVeh(driverId, fleetMasterId, true, pdaversion);
                                }

                                if (valueCnt == 6)
                                {
                                    db.stp_UpdateDriverDeviceId(driverId, values[5].ToStr().Trim());
                                }

                                General.BroadCastMessage("**login>>Drv " + driverno + " is Login" + ">>" + driverno);

                                /*Uncomment later
                                if (Instance.objPolicy.AutoLogoutInActiveDrvMins.ToInt() > 0)
                                {
                                    new BroadcasterData().BroadCastToLocal("**refresh map");
                                    // RefreshMap();
                                }
                                */

                                // OFFLINE MESSAGE
                                if (true)
                                {





                                    try
                                    {

                                        var offlineMessageList = db.Fleet_Driver_OfflineJobs.Where(c => c.DriverId == driverId && c.BookingId == null && (c.OfflineMessage != null && c.OfflineMessage.StartsWith("update settings")))
                                            .OrderByDescending(c => c.Id)
                                            .Select(args => new { args.BookingId, args.DriverId, args.OfflineMessage, args.TempJobId, args.UpdatedOn })
                                            .FirstOrDefault();
                                        StringBuilder s = new StringBuilder();
                                        string jobIds = string.Empty;

                                        //foreach (var itemJob in offlineMessageList)
                                        //{
                                        //long jobId = itemJob.BookingId.ToLong();

                                        if (offlineMessageList != null && offlineMessageList.OfflineMessage.ToStr().ToLower().StartsWith("update settings"))
                                        {

                                            Instance.listofJobs.Add(
                                                new clsPDA
                                                {
                                                    DriverId = offlineMessageList.DriverId.ToInt(),
                                                    JobMessage = offlineMessageList.OfflineMessage.Replace("}=12", "}").Trim(),
                                                    MessageTypeId = 12,
                                                    MessageDateTime = DateTime.Now.AddSeconds(-45)

                                                        //
                                                    }
                                                );

                                            s.AppendLine("delete from Fleet_Driver_OfflineJobs where driverid=" + offlineMessageList.DriverId.ToInt() + " and offlinemessage like 'update settings%';");


                                        }



                                        //}


                                        string resp = s.ToStr();

                                        ////if (resp.ToStr().Trim().EndsWith(","))
                                        ////    resp = resp.Remove(resp.ToStr().LastIndexOf(","));


                                        if (resp.ToStr().Trim().Length > 0)
                                        {
                                            using (TaxiDataContext dbx = new TaxiDataContext())
                                            {
                                                dbx.ExecuteQuery<int>(resp);
                                            }
                                            try
                                            {
                                                File.AppendAllText(physicalPath + "\\" + "offlinemsgreceived.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + resp + ", jobIds: " + "" + ",driverId:" + 0 + Environment.NewLine);
                                            }
                                            catch
                                            {

                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        try
                                        {
                                            File.AppendAllText(physicalPath + "\\" + "offlinemsgreceived_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + ex.Message + ", jobIds: " + "" + ",driverId:" + driverId + Environment.NewLine);
                                        }
                                        catch
                                        {

                                        }
                                    }

                                    //if (resp.ToStr().Trim().Length > 10)
                                    //{


                                    //    try
                                    //    {
                                    //        resp = "alertprejoblist:[" + resp + "]";


                                    //        Instance.listofJobs.Add(new clsPDA
                                    //        {
                                    //            JobId = 0,
                                    //            DriverId = driverId,
                                    //            MessageDateTime = DateTime.Now,
                                    //            JobMessage = resp,
                                    //            MessageTypeId = eMessageTypes.JOB,
                                    //            DriverNo = driverno
                                    //        });

                                    //        if (jobIds.ToStr().Trim().EndsWith(","))
                                    //            jobIds = jobIds.Remove(jobIds.ToStr().LastIndexOf(","));


                                    //        string query = "update booking set bookingstatusid=17 where id in(" + jobIds + ") and bookingstatusid=4 and driverid=" + driverId;
                                    //        //
                                    //        db.ExecuteQuery<int>(query);
                                    //        db.ExecuteQuery<int>("exec stp_receivedofflinejob {0},{1},{2}", jobIds, driverId, query);

                                    //        //
                                    //        File.AppendAllText(physicalPath + "\\" + "offlinejobreceived.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", jobIds: " + jobIds + ",driverId:" + driverId + Environment.NewLine);

                                    //    }
                                    //    catch (Exception ex)
                                    //    {
                                    //        try
                                    //        {
                                    //            File.AppendAllText(physicalPath + "\\" + "offlinejobreceived_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + ex.Message + ", jobIds: " + jobIds + ",driverId:" + driverId + Environment.NewLine);
                                    //        }
                                    //        catch
                                    //        {

                                    //        }

                                    //    }
                                    //    //


                                    //}


                                }



                                if (Instance.objPolicy.DespatchOfflineJobs.ToBool() &&

                                (
                                (pdaversion.ToDecimal() >= 41.70m && pdaversion.ToDecimal() < 45m) // android
                                || pdaversion.ToDecimal() >= 45.25m) // iphone
                                )
                                {

                                    try
                                    {
                                        Instance.listofJobs.RemoveAll(c => c.DriverId == driverId && c.JobMessage.StartsWith("PreJobId:"));

                                    }
                                    catch
                                    {
                                        //

                                    }



                                    var offlineMessageList = db.Fleet_Driver_OfflineJobs.Where(c => c.DriverId == driverId && c.BookingId != null && (c.OfflineMessage == null || c.OfflineMessage == ""))
                                        .Select(args => new { args.BookingId, args.DriverId, args.OfflineMessage, args.TempJobId, args.UpdatedOn }).ToList();
                                    StringBuilder s = new StringBuilder();
                                    string jobIds = string.Empty;
                                    foreach (var itemJob in offlineMessageList)
                                    {
                                        long jobId = itemJob.BookingId.ToLong();

                                        if (jobId != 0 && itemJob.OfflineMessage.ToStr().Trim() == "")
                                        {
                                            if (Instance.listofJobs.Count(c => c.DriverId == driverId && c.JobId == jobId) > 0)
                                                Instance.listofJobs.RemoveAll(c => c.DriverId == driverId && c.JobId == jobId);

                                            Taxi_Model.Booking objBooking = db.Bookings.FirstOrDefault(c => c.Id == itemJob.BookingId && c.DriverId == driverId && c.BookingStatusId == Enums.BOOKINGSTATUS.PENDING);
                                            if (objBooking == null)
                                                continue;


                                            if (objBooking.DriverId.ToInt() == driverId)
                                            {

                                                if (objBooking != null)
                                                {
                                                    jobIds += objBooking.Id + ",";

                                                    jobId = objBooking.Id;



                                                    string journey = "O/W";
                                                    if (objBooking.JourneyTypeId.ToInt() == 2)
                                                    {
                                                        journey = "Return";
                                                    }
                                                    else if (objBooking.JourneyTypeId.ToInt() == 3)
                                                    {
                                                        journey = "W/R";
                                                    }


                                                    string IsExtra = (objBooking.CompanyId != null || objBooking.FromLocTypeId == Enums.LOCATION_TYPES.AIRPORT || objBooking.ToLocTypeId == Enums.LOCATION_TYPES.AIRPORT) ? "1" : "0";
                                                    int i = 0;
                                                    string viaP = "";

                                                    if (objBooking.Booking_ViaLocations.Count > 0)
                                                    {
                                                        viaP = "(" + (++i).ToStr() + ")" + string.Join(Environment.NewLine + "(" + (++i).ToStr() + ")", objBooking.Booking_ViaLocations.Select(c => c.ViaLocValue.ToStr()).ToArray<string>());
                                                    }


                                                    string specialRequirements = objBooking.SpecialRequirements.ToStr();
                                                    if (objBooking.SecondaryPaymentTypeId != null && objBooking.CashFares.ToDecimal() > 0)
                                                    {

                                                        specialRequirements += " , Additional Cash Payment : " + objBooking.CashFares.ToDecimal();
                                                    }

                                                    decimal pdafares = objBooking.GetType().GetProperty(Instance.objPolicy.PDAFaresPropertyName.ToStr().Trim()).GetValue(objBooking, null).ToDecimal();


                                                    msg = string.Empty;

                                                    string mobileNo = objBooking.CustomerMobileNo.ToStr();
                                                    string telNo = objBooking.CustomerPhoneNo.ToStr();


                                                    // decimal drvPdaVersion = 20.00m;

                                                    if (string.IsNullOrEmpty(mobileNo) && !string.IsNullOrEmpty(telNo))
                                                    {
                                                        mobileNo = telNo;
                                                    }
                                                    else if (!string.IsNullOrEmpty(mobileNo) && !string.IsNullOrEmpty(telNo))
                                                    {
                                                        mobileNo += "/" + telNo;
                                                    }


                                                    string showFaresValue = objBooking.IsQuotedPrice.ToBool() == true ? "1" : objBooking.Gen_PaymentType.ShowFaresOnPDA.ToStr().Trim();



                                                    string showFares = ",\"ShowFares\":\"" + showFaresValue + "\"";
                                                    string showSummary = ",\"ShowSummary\":\"" + showFaresValue + "\"";
                                                    //   string showSummary = string.Empty;

                                                    string agentDetails = string.Empty;
                                                    string parkingandWaiting = string.Empty;
                                                    if (objBooking.CompanyId != null)
                                                    {
                                                        agentDetails = ",\"AgentFees\":\"" + String.Format("{0:0.00}", objBooking.AgentCommission) + "\"";
                                                        parkingandWaiting = ",\"Parking\":\"" + string.Format("{0:0.00}", objBooking.ParkingCharges) + "\",\"Waiting\":\"" + String.Format("{0:0.00}", objBooking.WaitingCharges) + "\"";

                                                    }
                                                    else
                                                    {

                                                        parkingandWaiting = ",\"Parking\":\"" + string.Format("{0:0.00}", objBooking.CongtionCharges) + "\",\"Waiting\":\"" + String.Format("{0:0.00}", objBooking.MeetAndGreetCharges) + "\"";
                                                        //

                                                    }



                                                    string fromAddress = objBooking.FromAddress.ToStr().Trim();
                                                    string toAddress = objBooking.ToAddress.ToStr().Trim();

                                                    if (objBooking.FromLocTypeId.ToInt() == Enums.LOCATION_TYPES.POSTCODE || objBooking.FromLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT)
                                                    {
                                                        fromAddress = objBooking.FromStreet.ToStr() + " " + objBooking.FromAddress.ToStr();

                                                    }

                                                    if (objBooking.ToLocTypeId.ToInt() == Enums.LOCATION_TYPES.POSTCODE || objBooking.ToLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT)
                                                    {
                                                        toAddress = objBooking.ToStreet.ToStr() + " " + objBooking.ToAddress.ToStr();
                                                    }

                                                    string paymentType = objBooking.Gen_PaymentType.PaymentCategoryId == null ? objBooking.Gen_PaymentType.DefaultIfEmpty().PaymentType.ToStr()
                                                                                  : objBooking.Gen_PaymentType.Gen_PaymentCategory.CategoryName.ToStr();



                                                    string companyName = string.Empty;

                                                    if (objBooking.CompanyId != null)
                                                        companyName = objBooking.Gen_Company.DefaultIfEmpty().CompanyName.ToStr();



                                                    string pickUpPlot = "";
                                                    string dropOffPlot = "";

                                                    pickUpPlot = objBooking.ZoneId != null ? "<<<" + objBooking.Gen_Zone1.DefaultIfEmpty().ZoneName.ToStr() : "";
                                                    dropOffPlot = objBooking.DropOffZoneId != null ? "<<<" + objBooking.Gen_Zone.DefaultIfEmpty().ZoneName.ToStr() : "";



                                                    string fromdoorno = objBooking.FromDoorNo.ToStr().Trim();


                                                    string pickupDateTime = string.Format("{0:dd/MM/yyyy   HH:mm}", objBooking.PickupDateTime);


                                                    string appendString = "";


                                                    try
                                                    {
                                                        appendString = ",\"ShowOnlyPlot\":\"" + "0" + "\"" +
                                                         ",\"ExtraCharges\":\"" + objBooking.ExtraDropCharges.ToDecimal() + "\"" +
                                                          ",\"BookingFee\":\"" + 0.00 + "\"" +
                                                          ",\"BgColor\":\"" + "" + "\"";


                                                    }
                                                    catch
                                                    {

                                                    }

                                                    //if (objBooking.CompanyId != null && Global.enableBookingRefOnAccJob == "1")
                                                    //{
                                                    //    if (specialRequirements.Length == 0)
                                                    //        specialRequirements = "Booking Ref- " + objBooking.BookingNo.ToStr();
                                                    //    else
                                                    //        specialRequirements = "Booking Ref- " + objBooking.BookingNo.ToStr() + " , " + specialRequirements;
                                                    //}


                                                    msg = "{ \"JobId\" :\"" + objBooking.Id.ToStr() +
                                                 "\", \"Pickup\":\"" + (!string.IsNullOrEmpty(objBooking.FromDoorNo) ? fromdoorno + "-" + fromAddress + pickUpPlot : fromAddress + pickUpPlot) +
                                                 "\", \"Destination\":\"" + (!string.IsNullOrEmpty(objBooking.ToDoorNo) ? objBooking.ToDoorNo + "-" + toAddress + dropOffPlot : toAddress + dropOffPlot) + "\"," +
                                                 "\"PickupDateTime\":\"" + pickupDateTime + "\"" +
                                                 ",\"Cust\":\"" + objBooking.CustomerName + "\",\"Mob\":\"" + mobileNo + " " + "\",\"Fare\":\"" + string.Format("{0:0.00}", pdafares) + "\",\"Vehicle\":\"" + objBooking.Fleet_VehicleType.VehicleType + "\",\"Account\":\"" + companyName + " " + "\"" +
                                                   ",\"Lug\":\"" + objBooking.NoofLuggages.ToInt() + "\",\"Passengers\":\"" + objBooking.NoofPassengers.ToInt() + "\",\"Journey\":\"" + journey + "\",\"Payment\":\"" + paymentType + "\",\"Special\":\"" + specialRequirements + " " + "\",\"Extra\":\"" + IsExtra + "\",\"Via\":\"" + viaP + " " + "\"" +
                                                parkingandWaiting + ",\"DriverFares\":\"" + String.Format("{0:0.00}", objBooking.FareRate) + "\"" +
                                                agentDetails +
                                                   ",\"Did\":\"" + driverId + "\",\"BabySeats\":\"" + objBooking.BabySeats.ToStr() + "\"" + showFares + showSummary + appendString + " }";


                                                    if (msg.Contains("\r\n"))
                                                    {
                                                        msg = msg.Replace("\r\n", " ").Trim();
                                                    }
                                                    else
                                                    {
                                                        if (msg.Contains("\n"))
                                                        {
                                                            msg = msg.Replace("\n", " ").Trim();

                                                        }

                                                    }

                                                    if (msg.Contains("&"))
                                                    {
                                                        msg = msg.Replace("&", "And");
                                                    }

                                                    if (msg.Contains(">"))
                                                        msg = msg.Replace(">", " ");


                                                    if (msg.Contains("="))
                                                        msg = msg.Replace("=", " ");




                                                    msg += ",";
                                                    s.Append(msg);

                                                    // END BOOKING IF

                                                }
                                            }
                                        }

                                    }


                                    string resp = s.ToStr();

                                    if (resp.ToStr().Trim().EndsWith(","))
                                        resp = resp.Remove(resp.ToStr().LastIndexOf(","));


                                    if (resp.ToStr().Trim().Length > 10)
                                    {


                                        try
                                        {

                                            File.AppendAllText(physicalPath + "\\" + "offlinejobreceivedstart.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", jobIds: " + jobIds + ",driverId:" + driverId + Environment.NewLine);

                                            resp = "alertprejoblist:[" + resp + "]";


                                            Instance.listofJobs.Add(new clsPDA
                                            {
                                                JobId = 0,
                                                DriverId = driverId,
                                                MessageDateTime = DateTime.Now,
                                                JobMessage = resp,
                                                MessageTypeId = eMessageTypes.JOB,
                                                DriverNo = driverno
                                            });

                                            if (jobIds.ToStr().Trim().EndsWith(","))
                                                jobIds = jobIds.Remove(jobIds.ToStr().LastIndexOf(","));


                                            string query = "update booking set bookingstatusid=17 where id in(" + jobIds + ") and bookingstatusid=4 and driverid=" + driverId;
                                            //
                                            db.ExecuteQuery<int>(query);
                                            db.ExecuteQuery<int>("exec stp_receivedofflinejob {0},{1},{2}", jobIds, driverId, query);

                                            //
                                            File.AppendAllText(physicalPath + "\\" + "offlinejobreceived.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", jobIds: " + jobIds + ",driverId:" + driverId + Environment.NewLine);

                                            General.BroadCastMessage("**refresh required dashboard");
                                        }
                                        catch (Exception ex)
                                        {
                                            try
                                            {
                                                File.AppendAllText(physicalPath + "\\" + "offlinejobreceived_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + ex.Message + ", jobIds: " + jobIds + ",driverId:" + driverId + Environment.NewLine);
                                            }
                                            catch
                                            {

                                            }

                                        }
                                        //


                                    }


                                }



                            }
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                File.AppendAllText(physicalPath + "\\" + "offlinejobreceived_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + ex.Message + ", jobIds: " + "" + ",driverId:" + driverId + Environment.NewLine);
                            }
                            catch
                            {

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Clients.Caller.shiftLogin("exceptionoccurred");
                    try
                    {
                        File.AppendAllText(physicalPath + "\\" + "requestshiftlogin_exception1.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + ex.Message + ", data: " + dataValue + Environment.NewLine);
                    }
                    catch
                    {

                    }
                }
                // });
            }
            catch (Exception ex)
            {
                Clients.Caller.shiftLogin("exceptionoccurred");
                try
                {
                    File.AppendAllText(physicalPath + "\\" + "requestshiftlogin_exception2.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + ex.Message + ", data: " + dataValue + Environment.NewLine);
                }
                catch
                {
                    //
                }
            }
        }

        public void requestLogout(string dataValue)
        {
            try
            {
                string[] values = dataValue.Split(new char[] { '=' });

                if (values.Count() >= 4)
                {
                    Clients.Caller.shiftLogout(true);

                    //Byte[] byteResponse = Encoding.UTF8.GetBytes("true");
                    //tcpClient.NoDelay = true;
                    //tcpClient.SendTimeout = 5000;
                    //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);
                }

                if (values.Count() >= 9)
                {
                    (new TaxiDataContext()).stp_LogoutDriverPenalty(values[1].ToInt(), false, true);
                    General.BroadCastMessage("**logout>>" + values[1] + ">>" + values[7] + ">>Driver " + values[7] + " is Logout(OverBreak)");
                }
                else
                {
                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        db.stp_LoginLogoutDriver(values[1].ToInt(), false, null);
                    }

                    General.BroadCastMessage("**logout>>Driver " + values[2] + " is Logout");
                }

                RemoveLogoutDriver(values[1].ToInt());
            }
            catch (Exception ex)
            {
                Clients.Caller.shiftLogout(ex.Message);
            }
        }

        public void requestSendingMessage(string msg)
        {
            try
            {
                byte[] inputBuffer = Encoding.UTF8.GetBytes(msg);

                string dataValue = msg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                if (values.Count() >= 7)
                {
                    //send acknowledgement message to PDA
                    Clients.Caller.shiftIncomingMessage("true");

                    //Byte[] byteResponse = Encoding.UTF8.GetBytes("true");
                    //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);
                }

                (new TaxiDataContext()).stp_SendMessage(values[1].ToInt(), values[2].ToInt(), values[3].ToStr(), "", values[4].ToStr(), values[5].ToStr());
                General.BroadCastMessage("**message>>" + values[1].ToStr() + ">>" + values[3].ToStr() + ">>" + values[4].ToStr());
            }
            catch (Exception ex)
            {
                Clients.Caller.shiftIncomingMessage(ex.Message);
            }
        }
        public void LatLong(string mesg)
        {
            try
            {



                try
                {
                    Clients.Caller.LatLongChanged("true");
                }
                catch
                {

                }


                byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

                if (mesg.ToStr().Trim().StartsWith("meter=>>>"))
                    return;

                if (mesg.ToStr().Trim().StartsWith("meter="))
                {



                    try
                    {




                        var arr = mesg.ToStr().Split(new string[] { ">>>" }, StringSplitOptions.RemoveEmptyEntries);

                        string faremeterstring = arr[0];

                        mesg = arr[1];
                        string[] meterArray = faremeterstring.Split('=');

                        JobMeter objAction = new JavaScriptSerializer().Deserialize<JobMeter>(meterArray[1].ToStr());

                        if (Instance.objPolicy.FareMeterType.ToInt() == 3)
                        {
                            try
                            {

                                File.AppendAllText(physicalPath + "\\Logs\\OfflineFareMeter\\" + objAction.JobID.ToStr() + ".txt", "logon:" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", miles:" + objAction.Miles + ", fares:" + objAction.Fares + ",waitingtime:" + objAction.WaitingTime.ToInt() + ",speed:" + objAction.Speed.ToStr() + ",lat lng:" + objAction.lg.ToStr() + "," + objAction.lt.ToStr() + Environment.NewLine);
                            }
                            catch
                            {


                            }

                        }
                        else
                        {




                            string response = string.Empty;


                            decimal rtnFares = 0.00m;


                            decimal miles = objAction.Miles.ToDecimal();
                            string IsWaiting = objAction.IsWaiting.ToStr();
                            decimal waitingCharges = objAction.WaitingCharges.ToDecimal();
                            int waitingTime = objAction.WaitingTime.ToInt();
                            string vehicleType = objAction.VehicleType.ToStr();
                            //  decimal waitingSpeed = objAction.WaitingSpeed.ToDecimal();
                            int SpeedSecs = objAction.SpeedSecs.ToInt();

                            DateTime pickupDate = DateTime.Now;


                            if (objAction.PickupDateTime.ToStr().Trim().Length > 0)
                            {
                                try
                                {
                                    string pickupDateTime = string.Format("{0:dd/MM/yyyy HH:mm}", objAction.PickupDateTime.ToStr().Trim().Replace("   ", " ").Trim());

                                    pickupDateTime = string.Format("{0:dd/MM/yyyy HH:mm}", objAction.PickupDateTime.ToStr().Trim().Replace("  ", " ").Trim());

                                    pickupDate = DateTime.Parse(pickupDateTime, CultureInfo.GetCultureInfo("en-gb"));

                                    //  pickupDate = DateTime.Now;

                                    //    try
                                    //    {
                                    ////        //
                                    //        pickupDate = DateTime.Now;
                                    //        File.AppendAllText(physicalPath + "\\" + "log_successlatlongmeter.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", new date: "+ pickupDate + ",parsed value : "+ pickupDateTime + Environment.NewLine);
                                    //    }
                                    //    catch
                                    //    {


                                    //    }
                                }
                                catch (Exception ex)
                                {

                                    try
                                    {

                                        pickupDate = DateTime.Now;
                                        File.AppendAllText(physicalPath + "\\" + "exception_formatpickupdatetime_faremeter.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + mesg + "," + ex.Message + ", pickup : " + objAction.PickupDateTime.ToStr().Trim() + Environment.NewLine);
                                    }
                                    catch
                                    {


                                    }

                                }
                            }
                            else
                            {
                                pickupDate = DateTime.Now;

                            }


                            //  InitializeMeterList();
                            //
                            var objFareMeter = Global.listofMeter.FirstOrDefault(c => c.VehicleType.ToLower() == vehicleType.ToLower()).DefaultIfEmpty();
                            //

                            //  DateTime dateValue = new DateTime(1900, 1, 1, 0, 0, 0);
                            //   pickupDate = string.Format("{0:dd/MM/yyyy HH:mm}", dateValue.ToDate() + pickupDate.TimeOfDay).ToDateTime();

                            if (miles > 0 && Instance.objPolicy.RoundJourneyMiles.ToDecimal() > 0)
                            {
                                miles = Math.Ceiling(miles / Instance.objPolicy.RoundJourneyMiles.ToDecimal()) * Instance.objPolicy.RoundJourneyMiles.ToDecimal();
                            }


                            if (objAction.Speed.ToDecimal() > 0 || objAction.Fares.ToDecimal() == 0 || objAction.IsWaiting == "1")
                            {
                                using (TaxiDataContext db = new TaxiDataContext())
                                {
                                    try
                                    {
                                        var objFare = db.stp_CalculateMeterFares(objFareMeter.VehicleTypeId, objAction.CompanyId.ToInt(), miles, pickupDate, objAction.SubCompanyId.ToInt()).FirstOrDefault();


                                        if (objFare != null)
                                        {
                                            rtnFares = objFare.totalFares.ToDecimal();


                                            if (Instance.objPolicy.RoundMileageFares.ToBool() == false)
                                            {
                                                decimal roundUp = Instance.objPolicy.RoundUpTo.ToDecimal();
                                                if (roundUp > 0)
                                                {

                                                    if (objFare.Result.ToStr().IsNumeric() && objFare.CompanyFareExist.ToBool())
                                                    {

                                                        rtnFares = rtnFares.ToDecimal();
                                                    }
                                                    else
                                                    {

                                                        rtnFares = (decimal)Math.Ceiling(rtnFares / roundUp) * roundUp;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                string ff = string.Format("{0:f2}", rtnFares);
                                                if (ff == string.Empty)
                                                    ff = "0";

                                                rtnFares = ff.ToDecimal();
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        try
                                        {

                                            File.AppendAllText(physicalPath + "\\" + "exception_meterstringcatch.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", LATLONG message: " + mesg + "," + ex.Message + Environment.NewLine);
                                        }
                                        catch
                                        {


                                        }

                                    }


                                }
                            }
                            else
                            {
                                rtnFares = objAction.Fares.ToDecimal();

                            }





                            if (objAction.IsWaiting == "1")
                            {

                                if (objFareMeter.AutoStopWaitingOnSpeed.ToDecimal() > 0 && objAction.Speed.ToDecimal() >= objFareMeter.AutoStopWaitingOnSpeed.ToDecimal())
                                {
                                    IsWaiting = "0";
                                }
                                else
                                {
                                    if (waitingTime > 0)
                                    {
                                        if (objFareMeter.AccWaitingChargesPerMin == null || objFareMeter.AccWaitingChargesPerMin.ToInt() == 0)
                                        {
                                            decimal waitingMins = waitingTime / 60;
                                            waitingMins = Math.Ceiling(waitingMins);
                                            waitingCharges = waitingMins * objFareMeter.DrvWaitingChargesPerMin.ToDecimal();
                                        }
                                        else
                                        {


                                            decimal waitingMins = Math.Floor((waitingTime / objFareMeter.AccWaitingChargesPerMin.ToDecimal()));
                                            // waitingMins = Math.Ceiling(waitingMins);
                                            waitingCharges = waitingMins * objFareMeter.DrvWaitingChargesPerMin.ToDecimal();
                                        }
                                    }
                                }

                            }
                            else
                            {

                                if (objFareMeter.AutoStartWaiting.ToBool() && SpeedSecs >= objFareMeter.AutoStartWaitingBelowSpeedSeconds.ToInt())
                                {
                                    IsWaiting = "1";

                                }



                            }

                            objAction.WaitingSpeed = objFareMeter.AutoStartWaitingBelowSpeed.ToDecimal();





                            if (rtnFares < objAction.Fares.ToDecimal() && rtnFares < (objAction.Fares.ToDecimal() - 2))
                                rtnFares = objAction.Fares.ToDecimal();


                            objAction.Fares = rtnFares;
                            objAction.IsWaiting = IsWaiting;
                            objAction.WaitingTime = waitingTime;
                            objAction.WaitingCharges = waitingCharges;




                            string res = new JavaScriptSerializer().Serialize(objAction);



                            response = "meter=" + res;

                            try
                            {
                                Clients.Caller.fareMeter(response);
                            }
                            catch
                            {

                            }






                            try
                            {
                                //
                                File.AppendAllText(physicalPath + "\\Logs\\FareMeter\\" + objAction.JobID.ToStr() + ".txt", "logon:" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", miles:" + objAction.Miles + ", fares:" + objAction.Fares + ",waitingtime:" + objAction.WaitingTime.ToInt() + ",speed:" + objAction.Speed.ToStr() + ",lat lng:" + objAction.lg.ToStr() + "," + objAction.lt.ToStr() + Environment.NewLine);
                            }
                            catch
                            {


                            }





                        }
                    }
                    catch (Exception ex)
                    {
                        try
                        {

                            File.AppendAllText(physicalPath + "\\" + "exception_faremeter.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", data:" + mesg + ":" + ex.Message + Environment.NewLine);
                            //  RestartProgram();

                        }
                        catch
                        {


                        }

                    }


                }

                string dataValue = mesg;
                dataValue = dataValue.Trim();
                // old   // lat=34234=L=0.5=d=DRIVERID=S=speed=j=jobid
                // new only meter case   // meter=json>>>lat=34234=L=0.5=d=DRIVERID=S=speed=j=jobid
                string[] values = dataValue.Split(new char[] { '=' });

                double latitude = Convert.ToDouble(values[1]);
                double longitude = Convert.ToDouble(values[3]);
                double speed = Convert.ToDouble(values[7]);
                int driverId = values[5].ToInt();

                long jobId = 0;

                if (values.Count() >= 10 && values[9].ToStr().IsNumeric())
                {
                    jobId = values[9].ToLong();
                }

                if (Instance.listofJobs.Count(c => c.DriverId == values[5].ToInt() && DateTime.Now.Subtract(c.MessageDateTime).TotalSeconds < 300) > 0)
                {



                    clsPDA objcls = Instance.listofJobs.LastOrDefault(c => c.DriverId == values[5].ToInt() && DateTime.Now.Subtract(c.MessageDateTime).TotalSeconds < 300);

                    if (objcls != null)
                    {


                        try
                        {





                            if (jobId == 0 || objcls.JobId == 0 || objcls.MessageTypeId != eMessageTypes.JOB)
                            {

                                if (objcls.MessageTypeId == eMessageTypes.JOB)
                                {
                                    List<string> listOfConnections = new List<string>();
                                    listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(objcls.DriverId));
                                    Clients.Clients(listOfConnections).despatchBooking(objcls.JobMessage.ToStr());
                                }
                                else if (objcls.MessageTypeId == eMessageTypes.RECALLJOB)
                                {
                                    List<string> listOfConnections = new List<string>();
                                    listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(objcls.DriverId));
                                    Clients.Clients(listOfConnections).forceRecoverJob(objcls.JobMessage.ToStr());

                                }

                                else if (objcls.MessageTypeId == eMessageTypes.UPDATEJOB)
                                {
                                    List<string> listOfConnections = new List<string>();
                                    listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(objcls.DriverId));
                                    Clients.Clients(listOfConnections).updateJob(objcls.JobMessage.ToStr());

                                }
                                else if (objcls.MessageTypeId == eMessageTypes.UPDATE_SETTINGS)
                                {
                                    List<string> listOfConnections = new List<string>();
                                    listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(objcls.DriverId));
                                    Clients.Clients(listOfConnections).updateSetting(objcls.JobMessage.ToStr());

                                }

                                //else 
                                //{
                                //    List<string> listOfConnections = new List<string>();
                                //    listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(objcls.DriverId));
                                //    Clients.Clients(listOfConnections).sendMessage(objcls.JobMessage.ToStr());

                                //}
                            }


                            //try
                            //{
                            //    var CONNid = Instance.ReturnDriverConnections(Convert.ToInt32(objcls.DriverId));

                            //    File.AppendAllText(AppContext.BaseDirectory + "\\LATLNG.txt", DateTime.Now + "," + "connectionID() " + CONNid[0] + Environment.NewLine);
                            //}
                            //catch (Exception ex)
                            //{
                            //    try
                            //    {
                            //        Thread.Sleep(5000);

                            //        File.AppendAllText(AppContext.BaseDirectory + "\\LATLNG.txt", DateTime.Now + "," + ex.Message + Environment.NewLine);
                            //    }
                            //    catch
                            //    {


                            //    }
                            //}
                        }
                        catch (Exception ex)
                        {


                        }

                        if (jobId != 0 && (objcls.MessageTypeId == eMessageTypes.JOB) && jobId == objcls.JobId)
                        {

                            string pickup = string.Empty;
                            string destination = string.Empty;
                            try
                            {
                                if (objcls.JobMessage.ToStr().StartsWith("JobId:{ \"JobId\""))
                                {
                                    ClsJobMessageParser objParser = new JavaScriptSerializer().Deserialize<ClsJobMessageParser>(objcls.JobMessage.ToStr().Substring(6));

                                    if (objParser != null)
                                    {
                                        pickup = objParser.Pickup.ToStr();
                                        destination = objParser.Destination.ToStr();
                                    }
                                }
                                else
                                {
                                    pickup = objcls.JobMessage.Substring(objcls.JobMessage.IndexOf(":Pickup:") + 8);
                                    pickup = pickup.Remove(pickup.IndexOf(":Destination:"));

                                    destination = objcls.JobMessage.Substring(objcls.JobMessage.IndexOf(":Destination:") + 13);
                                    destination = destination.Remove(destination.IndexOf(":PickupDateTime:"));
                                }

                                General.BroadCastMessage("**job received>>" + objcls.DriverNo + ">>" + pickup + ">>" + destination);
                            }
                            catch
                            {

                            }

                            Instance.listofJobs.Remove(objcls);
                        }

                        else if (objcls.MessageTypeId == eMessageTypes.FORCE_ACTION_BUTTON)
                        {
                            if (jobId == 0 || jobId != objcls.JobId)
                            {
                                Instance.listofJobs.Remove(objcls);
                            }
                            else
                            {
                                //

                                //Send message to PDA  
                                if (objcls.JobMessage.ToStr().Contains("<<Arrive Job>>"))
                                {
                                    Clients.Caller.forceArriveJob(objcls.JobMessage.ToStr());
                                }
                                else if (objcls.JobMessage.ToStr().Contains("<<POB Job>>"))
                                {
                                    Clients.Caller.forcePobJob(objcls.JobMessage.ToStr());
                                }

                                Instance.listofJobs.Remove(objcls);

                            }
                        }
                        else if (objcls.MessageTypeId == eMessageTypes.AUTHORIZATION)
                        {
                            //Send message to PDA
                            Clients.Caller.authStatus(objcls.JobMessage.ToStr());

                            //Byte[] byteResponse2 = Encoding.UTF8.GetBytes(objcls.JobMessage.ToStr());
                            //   //tcpClient.GetStream().Write(byteResponse2, 0, byteResponse2.Length);

                            try
                            {
                                if (objcls.JobMessage.Contains("yes") && (jobId == 0 || jobId != objcls.JobId))
                                {
                                    Instance.listofJobs.Remove(objcls);
                                }
                                //else if (objcls.JobMessage.Contains("no") && objcls.JobId == jobId)
                                //{
                                else if (objcls.JobMessage.Contains("no"))
                                {
                                    Instance.listofJobs.Remove(objcls);
                                }
                            }
                            catch
                            {

                            }
                        }
                        else if (objcls.MessageTypeId == eMessageTypes.LOGOUTAUTHORIZATION)
                        {
                            //Send message to PDA
                            try
                            {

                                Clients.Caller.authStatus(objcls.JobMessage.ToStr());


                                Clients.Caller.logoutAuthStatus(objcls.JobMessage.ToStr());


                                try
                                {
                                    File.AppendAllText(physicalPath + "\\logoutauthstatusdelete1.txt", DateTime.Now + ":" + ",driverid=" + objcls.JobMessage.ToStr() + Environment.NewLine);
                                }
                                catch
                                {

                                }
                                //
                                //    Instance.listofJobs.Remove(objcls);
                            }
                            catch
                            {

                            }
                            //
                            //   //    //Byte[] byteResponse2 = Encoding.UTF8.GetBytes(objcls.JobMessage.ToStr());
                            //tcpClient.GetStream().Write(byteResponse2, 0, byteResponse2.Length);

                            if (DateTime.Now.Subtract(objcls.MessageDateTime).TotalSeconds > 6)
                            {
                                Instance.listofJobs.Remove(objcls);
                                try
                                {
                                    File.AppendAllText(physicalPath + "\\logoutauthstatusdelete2.txt", DateTime.Now + ":" + ",driverid=" + objcls.JobMessage.ToStr() + Environment.NewLine);
                                }
                                catch
                                {

                                }
                            }


                        }
                        else if (objcls.MessageTypeId == eMessageTypes.ONBIDDESPATCH)
                        {
                            int AvailCnter = 0;
                            if (jobId > 0)
                            {
                                using (TaxiDataContext db = new TaxiDataContext())
                                {
                                    try
                                    {
                                        db.CommandTimeout = 6;

                                        AvailCnter = db.Fleet_DriverQueueLists.Count(c => c.Status == true && c.DriverId == objcls.DriverId && c.DriverWorkStatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE);

                                        if (AvailCnter > 0)
                                        {
                                            int valCnt = db.Bookings.Count(c => c.BookingStatusId == Enums.BOOKINGSTATUS.PENDING && c.DriverId == objcls.DriverId && c.PickupDateTime > DateTime.Now.AddDays(-1));

                                            if (valCnt > 0)
                                            {
                                                AvailCnter = 0;
                                            }
                                        }

                                        if (AvailCnter == 0)
                                        {
                                            //try
                                            //{
                                            //    File.AppendAllText(physicalPath + "\\onbiddespatchALREADYJOB.txt", DateTime.Now + ":" + ",jobid=" + objcls.JobId + Environment.NewLine);
                                            //}
                                            //catch
                                            //{

                                            //}

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        AvailCnter = 1;

                                        //try
                                        //{
                                        //    File.AppendAllText(physicalPath + "\\onbiddespatchALREADYJOB_catch.txt", DateTime.Now + ":" + ex.Message + ",jobid=" + objcls.JobId + Environment.NewLine);
                                        //}
                                        //catch
                                        //{

                                        //}
                                    }
                                }
                            }
                            else
                                AvailCnter = 1;

                            if (AvailCnter > 0)
                            {

                                string msg = string.Empty;

                                if (Instance.objPolicy.BiddingType.ToInt() == Enums.BIDDING_TYPES.NEAREST_DRIVER)
                                    msg = "Bidding Job has been Despatched to Nearest driver";
                                else if (Instance.objPolicy.BiddingType.ToInt() == Enums.BIDDING_TYPES.LONGEST_WAITING_QUEUE)
                                    msg = "Job Despatch successfully to longest waiting driver";
                                else if (Instance.objPolicy.BiddingType.ToInt() == Enums.BIDDING_TYPES.FASTEST_FINGER)
                                    msg = "Job Received to Fastest Finger driver";

                                try
                                {
                                    //Send message to PDA
                                    Clients.Caller.despatchBooking(objcls.JobMessage.ToStr());

                                    General.SP_SaveBid(objcls.JobId, objcls.DriverId, objcls.Price, 2, "", "Job Despatched");



                                    objcls.MessageTypeId = eMessageTypes.JOB;

                                    General.BroadCastMessage("**onbid despatch>>" + objcls.JobId + ">>" + objcls.DriverId + ">>" + msg);
                                }
                                catch
                                {
                                    try
                                    {
                                        //Send message to PDA
                                        Clients.Caller.despatchBooking(objcls.JobMessage.ToStr());



                                        General.BroadCastMessage("**onbid despatch>>" + objcls.JobId + ">>" + objcls.DriverId + ">>" + msg);

                                        Instance.listofJobs.Remove(objcls);
                                    }
                                    catch
                                    {
                                        try
                                        {
                                            General.BroadCastMessage("**onbid despatch>>" + objcls.JobId + ">>" + objcls.DriverId + ">>" + msg);
                                        }
                                        catch
                                        {

                                        }
                                    }
                                }
                            }
                            else
                            {
                                try
                                {
                                    Instance.listofJobs.Remove(objcls);
                                }
                                catch
                                {

                                }
                            }
                        }
                        else if (objcls.MessageTypeId == eMessageTypes.STC_ALLOCATED)
                        {
                            int AvailCnter = 0;
                            if (jobId > 0)
                            {
                                using (TaxiDataContext db = new TaxiDataContext())
                                {
                                    try
                                    {
                                        db.CommandTimeout = 6;

                                        AvailCnter = db.Fleet_DriverQueueLists.Count(c => c.Status == true && c.DriverId == objcls.DriverId && c.DriverWorkStatusId == Enums.Driver_WORKINGSTATUS.SOONTOCLEAR);

                                        if (AvailCnter > 0)
                                        {
                                            //int valCnt = db.Bookings.Count(c => c.BookingStatusId == Enums.BOOKINGSTATUS.PENDING && c.DriverId == objcls.DriverId && c.PickupDateTime > DateTime.Now.AddDays(-1));

                                            //if (valCnt > 0)
                                            //{
                                            //    AvailCnter = 0;
                                            //}


                                            General.BroadCastMessage("**onbid allocate>>" + objcls.JobId + ">>" + objcls.DriverId + ">>"+ objcls.DriverNo.ToStr()+" "+ ">>"+ objcls.JobMessage);
                                        }

                                       
                                    }
                                    catch (Exception ex)
                                    {
                                       
                                    }
                                }
                            }
                           

                            if (AvailCnter > 0)
                            {

                              
                                try
                                {
                                    //Send message to PDA


                                    ClsNotification not = new ClsNotification();
                                    if (objcls.JobMessage.ToStr().StartsWith("failed:"))
                                    {
                                       
                                        not.HasError = true;
                                        not.Message = objcls.JobMessage.ToStr().Replace("failed:", "").Trim();

                                    }
                                    else
                                    {
                                        not.Message = objcls.JobMessage.ToStr().Replace("success:", "").Trim();
                                        not.AllocatedJobId = objcls.JobId;
                                    }

                                    Clients.Caller.notification(new JavaScriptSerializer().Serialize(not));

                                  


                                    try
                                    {
                                        Instance.listofJobs.Remove(objcls);
                                    }
                                    catch
                                    {

                                    }
                                   
                                }
                                catch
                                {
                                    
                                }
                            }
                            else
                            {
                                try
                                {
                                    Instance.listofJobs.Remove(objcls);
                                }
                                catch
                                {

                                }
                            }
                        }
                        else
                        {

                            //Send message to PDA


                            if (objcls.MessageTypeId == eMessageTypes.JOB)
                            {
                                string jobMessage = objcls.JobMessage.ToStr();
                                Clients.Caller.despatchBooking(jobMessage);
                                Instance.listofJobs.Remove(objcls);


                                //try
                                //{


                                //    File.AppendAllText(AppContext.BaseDirectory + "\\LATLNGjob.txt", DateTime.Now + ", DriverId :" + driverId + ",job :" + jobMessage + Environment.NewLine);
                                //}
                                //catch
                                //{

                                //}
                                // List<string> listOfConnections = new List<string>();
                                // listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(objcls.DriverId));
                                // Clients.Clients(listOfConnections).despatchBooking(objcls.JobMessage.ToStr());
                            }
                            else if (objcls.MessageTypeId == eMessageTypes.RECALLJOB)
                            {
                                //List<string> listOfConnections = new List<string>();
                                //listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(objcls.DriverId));
                                //Clients.Clients(listOfConnections).forceRecoverJob(objcls.JobMessage.ToStr());

                                Clients.Caller.forceRecoverJob(objcls.JobMessage.ToStr());
                                Instance.listofJobs.Remove(objcls);
                            }
                            else if (objcls.MessageTypeId == eMessageTypes.CLEAREDJOB)
                            {

                                Clients.Caller.forceClearJob(objcls.JobMessage.ToStr());
                                Instance.listofJobs.Remove(objcls);
                            }
                            else if (objcls.MessageTypeId == eMessageTypes.UPDATE_SETTINGS)
                            {

                                Clients.Caller.updateSetting(objcls.JobMessage.ToStr());
                                Instance.listofJobs.Remove(objcls);

                            }
                            else if (objcls.MessageTypeId == eMessageTypes.UPDATEJOB)
                            {
                                //List<string> listOfConnections = new List<string>();
                                //listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(objcls.DriverId));
                                //Clients.Clients(listOfConnections).updateJob(objcls.JobMessage.ToStr());

                                Clients.Caller.updateJob(objcls.JobMessage.ToStr());
                                Instance.listofJobs.Remove(objcls);
                            }

                            else if (objcls.MessageTypeId == eMessageTypes.BIDALERT && values.Count() >= 10 && (values[9] == "bidack" || dataValue.EndsWith("bidack") || dataValue.EndsWith("bidack=")))
                            {
                                //
                                Instance.listofJobs.Remove(objcls);
                            }
                            else if (objcls.MessageTypeId == eMessageTypes.BIDPRICEALERT && values.Count() >= 11 && values[10] == "bidprack")
                            {
                                Instance.listofJobs.Remove(objcls);
                            }
                            else if (objcls.MessageTypeId == eMessageTypes.MESSAGING)
                            {

                                if (objcls.JobMessage.ToStr().Trim().ToLower() == "force logout")
                                {
                                    //
                                    Clients.Caller.forceLogout(objcls.JobMessage.ToStr());
                                }
                                else
                                    Clients.Caller.sendMessage(objcls.JobMessage.ToStr());

                                Instance.listofJobs.Remove(objcls);
                            }
                            else if ((objcls.MessageTypeId == eMessageTypes.UPDATEPLOT || objcls.MessageTypeId == eMessageTypes.UPDATEJOB)
                                        && (jobId == 0 || (jobId != 0 && values.Count() >= 11 && values[10] == "modj")))
                            {
                                long REjOBiD = objcls.JobId;
                                int driveriD = objcls.DriverId;
                                string sMsg = objcls.JobMessage.ToStr();

                                Instance.listofJobs.Remove(objcls);


                            }
                            else if (objcls.MessageTypeId == eMessageTypes.BIDALERT && values.Count() >= 11 && values[10] == "bidack")
                            {
                                Instance.listofJobs.Remove(objcls);
                            }
                            else if (objcls.MessageTypeId == eMessageTypes.JOB && values.Count() >= 11 && values[10] == "fojack")
                            {
                                Instance.listofJobs.Remove(objcls);
                            }

                            else if (objcls.MessageTypeId == eMessageTypes.UPDATE_SETTINGS && values.Count() >= 11 && values[10].StartsWith("updatesettingsack"))
                            {
                                Instance.listofJobs.Remove(objcls);
                            }
                            else
                            {

                                if (objcls.MessageTypeId == eMessageTypes.BIDALERT)
                                {

                                    if(objcls.DriverId==122)
                                    {

                                    }

                                    Clients.Caller.bidAlert(objcls.JobMessage.ToStr());

                                }
                                else
                                {
                                    Clients.Caller.LatLongChanged(objcls.JobMessage.ToStr());

                                }
                                Instance.listofJobs.Remove(objcls);


                            }



                        }
                    }
                }
                else
                {
                    //Send message to PDA
                    // Clients.Caller.LatLongChanged("");


                }




                using (TaxiDataContext db = new TaxiDataContext())
                {
                    try
                    {


                        if (lastSaveDriverLocationTimeout == null || lastSaveDriverLocationTimeout < DateTime.Now)
                        {
                            db.CommandTimeout = 3;


                            int oldzoneId = db.stp_SaveDriverLocationByZone(driverId, latitude, longitude, speed, jobId).FirstOrDefault().Column1.ToInt();
                            lastConnectionDateTime = DateTime.Now;
                            lastSaveDriverLocationTimeout = null;
                            retryDriverLocTimeout = 1;


                            if (oldzoneId.ToInt() == -2 && jobId > 0 && Global.AutoSTC.ToStr() == "1")
                            {
                                try
                                {



                                    if (Global.listofSTCReminder.Count(c => c.JobId == jobId && c.DriverId == driverId) > 0)
                                    {
                                        oldzoneId = 0;

                                        //try
                                        //{
                                        //    File.AppendAllText(AppContext.BaseDirectory + "\\AddSTCReminder_step1.txt", DateTime.Now.ToStr() + ": jobid:" + jobId + ",driverid:" + driverId + Environment.NewLine);
                                        //}
                                        //catch
                                        //{


                                        //}

                                    }
                                }
                                catch
                                {

                                }
                            }


                            if (oldzoneId == 0 || speed > 0)
                            //(oldzoneId == 0 ||
                            //      speed > 0
                            //      )
                            //      )
                            {

                                Global.LoadDataList();


                                string returnLoc = string.Empty;
                                string postcode = string.Empty;
                                bool hasChanges = false;
                                int zoneId = 0;
                                string newZoneName = string.Empty;

                                try
                                {
                                    if (latitude > 0 && oldzoneId != -2)
                                    {
                                        int[] plot = (from a in Instance.listOfZone.Where(c => (c.DisableRank == null || c.DisableRank == false)

                                                      &&
                                                      (
                                                                  (c.shapeType != "" && c.shapeType == "circle")
                                                                   || ((latitude >= c.MinLat && latitude <= c.MaxLat)
                                                                             && (longitude <= c.MaxLng && longitude >= c.MinLng))

                                                                             )
                                                                             )
                                                      orderby a.PlotKind
                                                      select a.Id
                                                      ).ToArray<int>();

                                        if (plot.Count() > 0)
                                        {


                                            foreach (int plotId in plot)
                                            {
                                                if (General.FindPoint(latitude, longitude, Instance.listofPolyVertices.Where(c => c.ZoneId == plotId).ToList()))
                                                {
                                                    zoneId = plotId;
                                                    break;

                                                }

                                            }

                                        }

                                    }


                                    if (zoneId == 0 && latitude > 0 && oldzoneId != -2)
                                    {

                                        if (Instance.objPolicy.EnablePOI.ToBool())
                                        {

                                            try
                                            {
                                                returnLoc = db.PostCodesNearLatLong(latitude, longitude).FirstOrDefault().DefaultIfEmpty().Street.ToStr();

                                                if (returnLoc.Length > 0)
                                                {

                                                    postcode = GetPostCodeMatchWithBase(returnLoc, true);

                                                    if (!string.IsNullOrEmpty(returnLoc) && string.IsNullOrEmpty(postcode))
                                                    {
                                                        postcode = GetPostCodeMatch(returnLoc);

                                                    }
                                                }
                                            }
                                            catch
                                            {

                                            }
                                        }




                                        if (!string.IsNullOrEmpty(returnLoc))
                                        {

                                            if (Instance.objPolicy.AutoZonePlotType.ToStr() == "postcode")
                                            {
                                                newZoneName = GetHalfPostCodeMatch(returnLoc.ToStr().ToUpper());
                                            }

                                            if (string.IsNullOrEmpty(newZoneName))
                                            {

                                                string[] LocString = returnLoc.ToStr().ToUpper().Split(',');


                                                if (LocString.Count() == 4)
                                                {
                                                    newZoneName = LocString[1].Trim().Split(' ').LastOrDefault().ToUpper().Trim();
                                                }
                                                else if (LocString.Count() == 3)
                                                {
                                                    newZoneName = LocString[1].Trim().Split(' ').LastOrDefault().ToUpper().Trim();
                                                }
                                                else if (LocString.Count() == 5)
                                                {
                                                    newZoneName = LocString[2].Trim().Split(' ').LastOrDefault().ToUpper().Trim();
                                                }
                                                else if (LocString.Count() == 6)
                                                {
                                                    newZoneName = LocString[4].Trim().Split(' ').LastOrDefault().ToUpper().Trim();
                                                }
                                                else if (LocString.Count() == 1)
                                                {
                                                    newZoneName = postcode;

                                                    if (newZoneName.ToStr().Length == 0)
                                                    {
                                                        try
                                                        {
                                                            if (LocString[0].Contains(" "))
                                                            {
                                                                newZoneName = LocString[0].Split(new char[] { ' ' })[0];
                                                            }
                                                            else
                                                            {
                                                                newZoneName = LocString[0].ToStr().Trim();

                                                            }
                                                        }
                                                        catch
                                                        {


                                                        }

                                                    }
                                                }
                                                else if (LocString.Count() == 2)
                                                {
                                                    newZoneName = LocString[0].Trim().Split(' ').LastOrDefault().ToUpper().Trim();
                                                }
                                                else if (LocString.Count() == 0)
                                                {
                                                    newZoneName = postcode;
                                                }
                                            }





                                            if (newZoneName.ToStr().Trim().Length > 0)
                                                zoneId = -1;


                                        }
                                    }

                                    if (zoneId > 0 || zoneId == -1)
                                    {
                                        Fleet_Driver_Location item = db.Fleet_Driver_Locations.FirstOrDefault(c => c.DriverId == driverId);

                                        if (item != null)
                                        {
                                            hasChanges = false;
                                            item.LocationName = "";
                                            bool SendRankChangedNotification = false;

                                            if ((item.DisableAutoPlotting.ToBool() == false || Instance.objPolicy.EnableFixedPlotting == false))
                                            {





                                                if (item.ZoneId != null && item.NewZoneName == "SIN BIN")

                                                {


                                                    if (item.PlotDate != null
                                                       && item.SinBinTillOn != null && item.SinBinTillOn.Value < DateTime.Now)
                                                    {
                                                        try
                                                        {

                                                            db.stp_UnBlockDriver(item.DriverId, null);

                                                        }
                                                        catch
                                                        {


                                                        }

                                                    }
                                                    else
                                                    {
                                                        zoneId = -2;
                                                    }


                                                }
                                                //else
                                                //{
                                                //    zoneId = -1;

                                                //}

                                                if (zoneId > 0 && (item.PrevZoneId.ToInt() != zoneId || item.ZoneId == null))
                                                {



                                                    hasChanges = true;

                                                    item.PrevZoneId = zoneId;
                                                    item.ZoneId = zoneId;

                                                    //   item.PreviousZone = "";
                                                    item.NewZoneName = "";
                                                    //item.PlotDate = DateTime.Now;
                                                    if (Instance.objPolicy != null && Instance.objPolicy.EnableBookingOtherCharges.ToBool() && item.LastActiveZoneName.ToStr().ToLower().Trim() != "")
                                                    {
                                                        item.PlotDate = new DateTime(1753, 2, 1, 1, 1, 1);

                                                        SendRankChangedNotification = true;



                                                    }
                                                    else
                                                        item.PlotDate = DateTime.Now;
                                                }

                                                if (zoneId == -1)
                                                {
                                                    if (string.IsNullOrEmpty(item.PreviousZone) || item.PreviousZone.ToStr() != newZoneName.ToStr())
                                                    {
                                                        hasChanges = true;

                                                        item.PreviousZone = newZoneName;
                                                        item.NewZoneName = newZoneName;
                                                        item.ZoneId = null;
                                                        //  item.PreviousZone = null;
                                                        item.PlotDate = DateTime.Now;
                                                    }
                                                }


                                            }
                                            else
                                            {
                                                try
                                                {



                                                    if (item.LastActiveZoneName.ToStr().Trim() == "" &&
                                                                    Global.listofSTCReminder.Count(c => c.JobId == jobId && c.DriverId == driverId) > 0
                                                                    && zoneId > 0 && item.PrevZoneId.ToInt() != zoneId && item.ZoneId == zoneId)
                                                    {

                                                        Global.RemoveSTCReminder(jobId, driverId);


                                                        Clients.Caller.callstc(driverId.ToStr() + "=" + jobId + "=" + zoneId);

                                                        try
                                                        {

                                                            File.AppendAllText(physicalPath + "\\" + "callautostc.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + mesg + Environment.NewLine);
                                                        }
                                                        catch
                                                        {


                                                        }

                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    try
                                                    {

                                                        File.AppendAllText(physicalPath + "\\" + "stcreminder_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + mesg + "," + ex.Message + Environment.NewLine);
                                                    }
                                                    catch
                                                    {


                                                    }

                                                }


                                            }


                                            if (hasChanges)
                                            {
                                                db.SubmitChanges();



                                                General.BroadCastMessage("**refresh plots");

                                                try
                                                {
                                                    Clients.Caller.requestZoneUpdates("true");

                                                    if (SendRankChangedNotification)
                                                        BroadCastPostionChanged(driverId);
                                                }
                                                catch
                                                {

                                                }
                                            }
                                        }
                                    }



                                }
                                catch (Exception ex)
                                {
                                    //     WriteLog("LOG X:" + DateTime.Now.ToStr() + ":" + ex.Message + Environment.NewLine);

                                    try
                                    {
                                     
                                        File.AppendAllText(physicalPath + "\\" + "exception_updatezone.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + mesg + "," + ex.Message + Environment.NewLine);
                                    }
                                    catch
                                    {


                                    }

                                }





                            }


                        }
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            lastSaveDriverLocationTimeout = DateTime.Now.AddSeconds(4);
                            retryDriverLocTimeout++;
                            //   File.AppendAllText("excep_savedriverlocation.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt") + ":" + dataValue + " ," + ex.Message + ",retrycnt:" + retryDriverLocTimeout.ToStr() + Environment.NewLine);
                        }
                        catch
                        {

                        }

                        //    if (retryDriverLocTimeout >= 3)
                        //        RestartProgram();
                    }
                }

                GC.Collect();

                return;
            }
            catch (Exception ex)
            {


                Clients.Caller.exceptionOccured(ex.Message);

                try
                {
                    Clients.Caller.exceptionOccured(ex.Message);
                    File.AppendAllText(physicalPath + "\\" + "exception_latlong.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + mesg + "," + ex.Message + Environment.NewLine);
                }
                catch
                {


                }
            }
        }

        private void BroadCastPostionChanged(int driverId)
        {
            try
            {

                //using (TaxiDataContext db = new TaxiDataContext())
                //{

                //    List<DriverModel> driverList = db.ExecuteQuery<DriverModel>("exec stp_GetAvailableDriverByPlot {0},{1}", driverId, 0).ToList();


                //    foreach (var item in driverList)
                //    {
                //        //
                //        Instance.listofJobs.Add(new clsPDA
                //        {
                //            JobId = 0,
                //            DriverId = item.DriverId,
                //            MessageDateTime = DateTime.Now.AddSeconds(-40),
                //            JobMessage = "Message>>Your Position has changed>>" + string.Format("{0:dd/MM/yyyy}", DateTime.Now),
                //            MessageTypeId = eMessageTypes.MESSAGING,
                //            // DriverNo = values[1].ToStr()
                //        });
                //    }
                //}

            }
            catch
            {


            }

        }


        public void requestPlotsDetails(string mesg)
        {

            List<ClsPlotDetails> list = new List<ClsPlotDetails>();
            try
            {

                try
                {

                    File.AppendAllText(physicalPath + "\\" + "requestPlotsDetails.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + mesg + Environment.NewLine);
                }
                catch
                {


                }

                ClsPlotDetails obj = new JavaScriptSerializer().Deserialize<ClsPlotDetails>(mesg);

                string plot = obj.PlotName.ToStr().Trim();


                int? zoneId = null;
                using (TaxiDataContext db = new TaxiDataContext())
                {



                    try
                    {
                        zoneId = Instance.listOfZone.Where(c => c.Area == plot).Select(c => c.Id).FirstOrDefault();
                    }
                    catch
                    {

                    }

                    if (zoneId == null)
                        zoneId = db.Gen_Zones.Where(c => c.ZoneName == plot).Select(c => c.Id).FirstOrDefault();


                    list = db.ExecuteQuery<ClsPlotDetails>("exec stp_GetAreaPlotsDetailsByVehicle {0},{1},{2},{3},{4}", obj.DriverId, zoneId, plot, 15, 30).ToList();








                    if (list == null)
                        list = new List<ClsPlotDetails>();



                    if (obj.Version.ToStr().Trim().Length > 0 && obj.Version.ToStr().Trim().IsNumeric() &&
                        (
                        (obj.Version.ToDecimal() >= 41.70m && obj.Version.ToDecimal() < 45) || obj.Version.ToDecimal() >= 45.20m)

                        )
                    {
                        //

                        ClsPlotFullDetails objDetails = new ClsPlotFullDetails();
                        objDetails.plotDetails = list;
                        objDetails.driverDetails = db.ExecuteQuery<ClsPlotDriverDetails>("exec stp_GetDriversInPlot {0},{1},{2}", obj.DriverId, zoneId, plot).ToList();

                        if ((objDetails.plotDetails == null || objDetails.plotDetails.Count == 0)
                            && (objDetails.driverDetails == null || objDetails.driverDetails.Count == 0)
                            )
                        {
                            Clients.Caller.plotDetails("");


                        }
                        else
                            Clients.Caller.plotDetails(objDetails);

                    }
                    else
                    {

                        if (list.Count == 0)
                            Clients.Caller.plotDetails("");
                        else
                            Clients.Caller.plotDetails(list);


                    }
                }
            }
            catch (Exception ex)
            {

                try
                {
                    Clients.Caller.plotDetails("");
                    File.AppendAllText(physicalPath + "\\" + "requestPlotsDetails_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + mesg + " ,exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {


                }
            }

        }


        public void requestBidDetails(string mesg)
        {
            //
            List<ClsPlotDetails> list = new List<ClsPlotDetails>();
            try
            {

                try
                {

                    File.AppendAllText(physicalPath + "\\" + "requestBidDetails.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + mesg + Environment.NewLine);
                }
                catch
                {


                }

                ClsPlotDetails obj = new JavaScriptSerializer().Deserialize<ClsPlotDetails>(mesg);

                string plot = obj.PlotName.ToStr().Trim();
                using (TaxiDataContext db = new TaxiDataContext())
                {
                    int? zoneId = obj.PlotId;


                    //


                    var arr = db.ExecuteQuery<ClsPlotBidDetail>("exec stp_getbiddingdetailjobs {0},{1}", obj.DriverId.ToInt(), zoneId)
                        .ToList();

                    foreach (var item in arr)
                    {
                        list.Add(new ClsPlotDetails { PlotName = item.ZoneName, PlotId = item.ZoneId.ToInt(), JobId = item.JobId, PickupAddress = item.FromAddress, DropOff = item.ToAddress, Vehicle = item.VehicleType, Time = string.Format("{0:HH:mm}", item.PickupDateTime) });
                    }

                }



                //


                if (list == null)
                    list = new List<ClsPlotDetails>();

                if (list.Count == 0)
                    Clients.Caller.bidDetails("");
                else
                    Clients.Caller.bidDetails(list);
            }
            catch (Exception ex)
            {

                try
                {
                    Clients.Caller.plotDetails("");
                    File.AppendAllText(physicalPath + "\\" + "requestbidDetails_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + mesg + " ,exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {


                }
            }
        }



        public void SendLocationData(object json)
        {
            try
            {
                try
                {
                    File.AppendAllText(physicalPath + "\\" + "SendDatamethodcalled.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + json.ToString() + Environment.NewLine);
                }
                catch
                {
                    //
                }
                //
                SendDataRequest req = new JavaScriptSerializer().Deserialize<SendDataRequest>(json.ToString());


                string mesg = req.LatLong;



                //

                try
                {
                    Clients.Caller.SendLocationDataChanged("true");
                }
                catch
                {

                }





                if (req.MeterString.ToStr().Trim().StartsWith("meter=>>>"))
                    return;

                string faremeterstring = req.MeterString.ToStr();

                try
                {

                    //try
                    //{

                    //    File.AppendAllText(physicalPath + "\\" + "faremeter1.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", data:" + faremeterstring + Environment.NewLine);

                    //}
                    //catch
                    //{


                    //}


                    if (faremeterstring.ToStr().Trim().Length > 0)
                    {





                        JobMeter objAction = new JavaScriptSerializer().Deserialize<JobMeter>(faremeterstring);

                        if (Instance.objPolicy.FareMeterType.ToInt() == 3)
                        {
                            try
                            {

                                File.AppendAllText(physicalPath + "\\Logs\\OfflineFareMeter\\" + objAction.JobID.ToStr() + ".txt", "logon:" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", miles:" + objAction.Miles + ", fares:" + objAction.Fares + ",waitingtime:" + objAction.WaitingTime.ToInt() + ",speed:" + objAction.Speed.ToStr() + ",lat lng:" + objAction.lg.ToStr() + "," + objAction.lt.ToStr() + Environment.NewLine);
                            }
                            catch
                            {


                            }

                        }


                        //try
                        //{

                        //    File.AppendAllText(physicalPath + "\\" + "_faremeter2.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", data:" + faremeterstring + Environment.NewLine);

                        //}
                        //catch
                        //{


                        //}
                    }

                }
                catch (Exception ex)
                {
                    try
                    {

                        File.AppendAllText(physicalPath + "\\" + "exception_faremeter.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", data:" + faremeterstring + ":" + ex.Message + Environment.NewLine);

                    }
                    catch
                    {


                    }

                }





                string dataValue = req.LatLong;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                double latitude = Convert.ToDouble(values[1]);
                double longitude = Convert.ToDouble(values[3]);
                double speed = Convert.ToDouble(values[7]);
                int driverId = values[5].ToInt();

                long jobId = 0;




                if (values.Count() >= 10 && values[9].ToStr().IsNumeric())
                {
                    jobId = values[9].ToLong();
                }





                if (Instance.listofJobs.Count(c => c.DriverId == values[5].ToInt() && DateTime.Now.Subtract(c.MessageDateTime).TotalSeconds < 300) > 0)
                {


                    clsPDA objcls = Instance.listofJobs.LastOrDefault(c => c.DriverId == values[5].ToInt() && DateTime.Now.Subtract(c.MessageDateTime).TotalSeconds < 300);

                    if (objcls != null)
                    {




                        try
                        {

                            if (jobId == 0 || objcls.JobId == 0 || objcls.MessageTypeId != eMessageTypes.JOB)
                            {

                                if (objcls.MessageTypeId == eMessageTypes.JOB)
                                {
                                    List<string> listOfConnections = new List<string>();
                                    listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(objcls.DriverId));
                                    Clients.Clients(listOfConnections).despatchBooking(objcls.JobMessage.ToStr());
                                }
                                else if (objcls.MessageTypeId == eMessageTypes.RECALLJOB)
                                {
                                    List<string> listOfConnections = new List<string>();
                                    listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(objcls.DriverId));
                                    Clients.Clients(listOfConnections).forceRecoverJob(objcls.JobMessage.ToStr());

                                }

                                else if (objcls.MessageTypeId == eMessageTypes.UPDATEJOB)
                                {
                                    List<string> listOfConnections = new List<string>();
                                    listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(objcls.DriverId));
                                    Clients.Clients(listOfConnections).updateJob(objcls.JobMessage.ToStr());

                                }
                                else if (objcls.MessageTypeId == eMessageTypes.UPDATE_SETTINGS)
                                {
                                    List<string> listOfConnections = new List<string>();
                                    listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(objcls.DriverId));
                                    Clients.Clients(listOfConnections).updateSetting(objcls.JobMessage.ToStr());

                                }


                            }



                        }
                        catch (Exception ex)
                        {


                        }

                        if (jobId != 0 && (objcls.MessageTypeId == eMessageTypes.JOB) && jobId == objcls.JobId)
                        {

                            string pickup = string.Empty;
                            string destination = string.Empty;
                            try
                            {
                                if (objcls.JobMessage.ToStr().StartsWith("JobId:{ \"JobId\""))
                                {
                                    ClsJobMessageParser objParser = new JavaScriptSerializer().Deserialize<ClsJobMessageParser>(objcls.JobMessage.ToStr().Substring(6));

                                    if (objParser != null)
                                    {
                                        pickup = objParser.Pickup.ToStr();
                                        destination = objParser.Destination.ToStr();
                                    }
                                }
                                else
                                {
                                    pickup = objcls.JobMessage.Substring(objcls.JobMessage.IndexOf(":Pickup:") + 8);
                                    pickup = pickup.Remove(pickup.IndexOf(":Destination:"));

                                    destination = objcls.JobMessage.Substring(objcls.JobMessage.IndexOf(":Destination:") + 13);
                                    destination = destination.Remove(destination.IndexOf(":PickupDateTime:"));
                                }

                                General.BroadCastMessage("**job received>>" + objcls.DriverNo + ">>" + pickup + ">>" + destination);
                            }
                            catch
                            {

                            }

                            Instance.listofJobs.Remove(objcls);
                        }

                        else if (objcls.MessageTypeId == eMessageTypes.FORCE_ACTION_BUTTON)
                        {
                            if (jobId == 0 || jobId != objcls.JobId)
                            {
                                Instance.listofJobs.Remove(objcls);
                            }
                            else
                            {
                                //

                                //Send message to PDA  
                                if (objcls.JobMessage.ToStr().Contains("<<Arrive Job>>"))
                                {
                                    Clients.Caller.forceArriveJob(objcls.JobMessage.ToStr());
                                }
                                else if (objcls.JobMessage.ToStr().Contains("<<POB Job>>"))
                                {
                                    Clients.Caller.forcePobJob(objcls.JobMessage.ToStr());
                                }

                                Instance.listofJobs.Remove(objcls);

                            }
                        }
                        else if (objcls.MessageTypeId == eMessageTypes.AUTHORIZATION)
                        {
                            //Send message to PDA
                            Clients.Caller.authStatus(objcls.JobMessage.ToStr());

                            //Byte[] byteResponse2 = Encoding.UTF8.GetBytes(objcls.JobMessage.ToStr());
                            //   //tcpClient.GetStream().Write(byteResponse2, 0, byteResponse2.Length);

                            try
                            {
                                if (objcls.JobMessage.Contains("yes") && (jobId == 0 || jobId != objcls.JobId))
                                {
                                    Instance.listofJobs.Remove(objcls);
                                }
                                //else if (objcls.JobMessage.Contains("no") && objcls.JobId == jobId)
                                //{
                                else if (objcls.JobMessage.Contains("no"))
                                {
                                    Instance.listofJobs.Remove(objcls);
                                }
                            }
                            catch
                            {

                            }
                        }
                        else if (objcls.MessageTypeId == eMessageTypes.LOGOUTAUTHORIZATION)
                        {
                            //Send message to PDA
                            try
                            {

                                Clients.Caller.authStatus(objcls.JobMessage.ToStr());


                                Clients.Caller.logoutAuthStatus(objcls.JobMessage.ToStr());
                                //
                            }
                            catch
                            {

                            }
                            //
                            //   //    //Byte[] byteResponse2 = Encoding.UTF8.GetBytes(objcls.JobMessage.ToStr());
                            //tcpClient.GetStream().Write(byteResponse2, 0, byteResponse2.Length);

                            if (DateTime.Now.Subtract(objcls.MessageDateTime).TotalMinutes > 1)
                            {
                                Instance.listofJobs.Remove(objcls);
                            }


                        }
                        else if (objcls.MessageTypeId == eMessageTypes.ONBIDDESPATCH)
                        {
                            int AvailCnter = 0;
                            if (jobId > 0)
                            {




                                using (TaxiDataContext db = new TaxiDataContext())
                                {
                                    try
                                    {
                                        db.CommandTimeout = 6;

                                        AvailCnter = db.Fleet_DriverQueueLists.Count(c => c.Status == true && c.DriverId == objcls.DriverId && c.DriverWorkStatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE);

                                        if (AvailCnter > 0)
                                        {
                                            int valCnt = db.Bookings.Count(c => c.BookingStatusId == Enums.BOOKINGSTATUS.PENDING && c.DriverId == objcls.DriverId && c.PickupDateTime > DateTime.Now.AddDays(-1));

                                            if (valCnt > 0)
                                            {
                                                AvailCnter = 0;
                                            }
                                        }

                                        if (AvailCnter == 0)
                                        {
                                            //try
                                            //{
                                            //    File.AppendAllText(physicalPath + "\\onbiddespatchALREADYJOB.txt", DateTime.Now + ":" + ",jobid=" + objcls.JobId + Environment.NewLine);
                                            //}
                                            //catch
                                            //{

                                            //}

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        AvailCnter = 1;

                                        //try
                                        //{
                                        //    File.AppendAllText(physicalPath + "\\onbiddespatchALREADYJOB_catch.txt", DateTime.Now + ":" + ex.Message + ",jobid=" + objcls.JobId + Environment.NewLine);
                                        //}
                                        //catch
                                        //{

                                        //}
                                    }
                                }
                            }
                            else
                                AvailCnter = 1;

                            if (AvailCnter > 0)
                            {

                                string msg = string.Empty;

                                if (Instance.objPolicy.BiddingType.ToInt() == Enums.BIDDING_TYPES.NEAREST_DRIVER)
                                    msg = "Bidding Job has been Despatched to Nearest driver";
                                else if (Instance.objPolicy.BiddingType.ToInt() == Enums.BIDDING_TYPES.LONGEST_WAITING_QUEUE)
                                    msg = "Job Despatch successfully to longest waiting driver";
                                else if (Instance.objPolicy.BiddingType.ToInt() == Enums.BIDDING_TYPES.FASTEST_FINGER)
                                    msg = "Job Received to Fastest Finger driver";

                                try
                                {
                                    //Send message to PDA
                                    Clients.Caller.despatchBooking(objcls.JobMessage.ToStr());

                                    General.SP_SaveBid(objcls.JobId, objcls.DriverId, objcls.Price, 2, "", "Job Despatched");



                                    objcls.MessageTypeId = eMessageTypes.JOB;

                                    General.BroadCastMessage("**onbid despatch>>" + objcls.JobId + ">>" + objcls.DriverId + ">>" + msg);
                                }
                                catch
                                {
                                    try
                                    {
                                        //Send message to PDA
                                        Clients.Caller.despatchBooking(objcls.JobMessage.ToStr());



                                        General.BroadCastMessage("**onbid despatch>>" + objcls.JobId + ">>" + objcls.DriverId + ">>" + msg);

                                        Instance.listofJobs.Remove(objcls);
                                    }
                                    catch
                                    {
                                        try
                                        {
                                            General.BroadCastMessage("**onbid despatch>>" + objcls.JobId + ">>" + objcls.DriverId + ">>" + msg);
                                        }
                                        catch
                                        {

                                        }
                                    }
                                }
                            }
                            else
                            {
                                try
                                {
                                    Instance.listofJobs.Remove(objcls);
                                }
                                catch
                                {

                                }
                            }
                        }
                        //else if (objcls.MessageTypeId == eMessageTypes.PAYMENT_RESPONSE)
                        //{
                        //    //Clients.Caller.paymenterespone(objcls.paymentresponse) //GMK
                        //    //Clients.Caller.notification(new JavaScriptSerializer().Serialize(not));

                        //    ResponseData res = new ResponseData();

                        //    res.Data = "123456";
                        //    res.IsSuccess = true;
                        //    res.Message = "";

                        //    //----------------------
                        //    objcls.JobMessage = Newtonsoft.Json.JsonConvert.SerializeObject(res);

                        //    Clients.Caller.paymenterespone(objcls.JobMessage);

                        //    //Instance.listofJobs.Add(new clsPDA
                        //    //{
                        //    //    JobId = 0,
                        //    //    DriverId = 0,
                        //    //    MessageDateTime = DateTime.Now.AddSeconds(-40),
                        //    //    JobMessage = Newtonsoft.Json.JsonConvert.SerializeObject(res),///"Message>>Your Position has changed>>" + string.Format("{0:dd/MM/yyyy}", DateTime.Now),
                        //    //    MessageTypeId = eMessageTypes.MESSAGING,
                        //    //    // DriverNo = values[1].ToStr()
                        //    //});                            


                        //    Clients.Caller.paymentresponse(new JavaScriptSerializer().Serialize(res));

                        //}
                        else if (objcls.MessageTypeId == eMessageTypes.STC_ALLOCATED)
                        {
                            int AvailCnter = 0;
                            if (jobId > 0)
                            {
                                using (TaxiDataContext db = new TaxiDataContext())
                                {
                                    try
                                    {
                                        db.CommandTimeout = 6;

                                        AvailCnter = db.Fleet_DriverQueueLists.Count(c => c.Status == true && c.DriverId == objcls.DriverId && c.DriverWorkStatusId == Enums.Driver_WORKINGSTATUS.SOONTOCLEAR);

                                        if (AvailCnter > 0)
                                        {
                                            //int valCnt = db.Bookings.Count(c => c.BookingStatusId == Enums.BOOKINGSTATUS.PENDING && c.DriverId == objcls.DriverId && c.PickupDateTime > DateTime.Now.AddDays(-1));

                                            //if (valCnt > 0)
                                            //{
                                            //    AvailCnter = 0;
                                            //}


                                            General.BroadCastMessage("**onbid allocate>>" + objcls.JobId + ">>" + objcls.DriverId + ">>" + objcls.DriverNo.ToStr() + " " + ">>" + objcls.JobMessage);
                                        }


                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                            }


                            if (AvailCnter > 0)
                            {


                                try
                                {
                                    //Send message to PDA


                                    ClsNotification not = new ClsNotification();
                                    if (objcls.JobMessage.ToStr().StartsWith("failed:"))
                                    {

                                        not.HasError = true;
                                        not.Message = objcls.JobMessage.ToStr().Replace("failed:", "").Trim();

                                    }
                                    else
                                    {
                                        not.Message = objcls.JobMessage.ToStr().Replace("success:", "").Trim();
                                        not.AllocatedJobId = objcls.JobId;
                                    }

                                    Clients.Caller.notification(new JavaScriptSerializer().Serialize(not));




                                    try
                                    {
                                        Instance.listofJobs.Remove(objcls);
                                    }
                                    catch
                                    {

                                    }

                                }
                                catch
                                {

                                }
                            }
                            else
                            {
                                try
                                {
                                    Instance.listofJobs.Remove(objcls);
                                }
                                catch
                                {

                                }
                            }
                        }
                        else
                        {

                            //Send message to PDA


                            if (objcls.MessageTypeId == eMessageTypes.JOB)
                            {
                                string jobMessage = objcls.JobMessage.ToStr();
                                Clients.Caller.despatchBooking(jobMessage);
                                Instance.listofJobs.Remove(objcls);


                                //try
                                //{


                                //    File.AppendAllText(AppContext.BaseDirectory + "\\LATLNGjob.txt", DateTime.Now + ", DriverId :" + driverId + ",job :" + jobMessage + Environment.NewLine);
                                //}
                                //catch
                                //{

                                //}
                                // List<string> listOfConnections = new List<string>();
                                // listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(objcls.DriverId));
                                // Clients.Clients(listOfConnections).despatchBooking(objcls.JobMessage.ToStr());
                            }
                            else if (objcls.MessageTypeId == eMessageTypes.RECALLJOB)
                            {
                                //List<string> listOfConnections = new List<string>();
                                //listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(objcls.DriverId));
                                //Clients.Clients(listOfConnections).forceRecoverJob(objcls.JobMessage.ToStr());

                                Clients.Caller.forceRecoverJob(objcls.JobMessage.ToStr());
                                Instance.listofJobs.Remove(objcls);
                            }
                            else if (objcls.MessageTypeId == eMessageTypes.CLEAREDJOB)
                            {

                                Clients.Caller.forceClearJob(objcls.JobMessage.ToStr());
                                Instance.listofJobs.Remove(objcls);
                            }
                            else if (objcls.MessageTypeId == eMessageTypes.UPDATE_SETTINGS)
                            {

                                Clients.Caller.updateSetting(objcls.JobMessage.ToStr());
                                Instance.listofJobs.Remove(objcls);

                            }
                            else if (objcls.MessageTypeId == eMessageTypes.UPDATEJOB)
                            {
                                //List<string> listOfConnections = new List<string>();
                                //listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(objcls.DriverId));
                                //Clients.Clients(listOfConnections).updateJob(objcls.JobMessage.ToStr());

                                Clients.Caller.updateJob(objcls.JobMessage.ToStr());
                                Instance.listofJobs.Remove(objcls);
                            }

                            else if (objcls.MessageTypeId == eMessageTypes.BIDALERT && values.Count() >= 10 && (values[9] == "bidack" || dataValue.EndsWith("bidack") || dataValue.EndsWith("bidack=")))
                            {
                                //
                                Instance.listofJobs.Remove(objcls);
                            }
                            else if (objcls.MessageTypeId == eMessageTypes.BIDPRICEALERT && values.Count() >= 11 && values[10] == "bidprack")
                            {
                                Instance.listofJobs.Remove(objcls);
                            }
                            else if (objcls.MessageTypeId == eMessageTypes.MESSAGING)
                            {

                                if (objcls.JobMessage.ToStr().Trim().ToLower() == "force logout")
                                {
                                    //
                                    Clients.Caller.forceLogout(objcls.JobMessage.ToStr());
                                }
                                else
                                    Clients.Caller.sendMessage(objcls.JobMessage.ToStr());

                                Instance.listofJobs.Remove(objcls);
                            }
                            else if ((objcls.MessageTypeId == eMessageTypes.UPDATEPLOT || objcls.MessageTypeId == eMessageTypes.UPDATEJOB)
                                        && (jobId == 0 || (jobId != 0 && values.Count() >= 11 && values[10] == "modj")))
                            {
                                long REjOBiD = objcls.JobId;
                                int driveriD = objcls.DriverId;
                                string sMsg = objcls.JobMessage.ToStr();

                                Instance.listofJobs.Remove(objcls);


                            }
                            else if (objcls.MessageTypeId == eMessageTypes.BIDALERT && values.Count() >= 11 && values[10] == "bidack")
                            {
                                Instance.listofJobs.Remove(objcls);
                            }
                            else if (objcls.MessageTypeId == eMessageTypes.JOB && values.Count() >= 11 && values[10] == "fojack")
                            {
                                Instance.listofJobs.Remove(objcls);
                            }

                            else if (objcls.MessageTypeId == eMessageTypes.UPDATE_SETTINGS && values.Count() >= 11 && values[10].StartsWith("updatesettingsack"))
                            {
                                Instance.listofJobs.Remove(objcls);
                            }
                            else
                            {

                                if (objcls.MessageTypeId == eMessageTypes.BIDALERT)
                                {

                                    if (objcls.DriverId == 122)
                                    {

                                    }

                                    Clients.Caller.bidAlert(objcls.JobMessage.ToStr());

                                }
                                else
                                {
                                    Clients.Caller.LatLongChanged(objcls.JobMessage.ToStr());

                                }
                                Instance.listofJobs.Remove(objcls);


                            }



                        }
                    }
                }
                else
                {
                    //Send message to PDA
                    // Clients.Caller.LatLongChanged("");


                }




                using (TaxiDataContext db = new TaxiDataContext())
                {
                    try
                    {


                        if (lastSaveDriverLocationTimeout == null || lastSaveDriverLocationTimeout < DateTime.Now)
                        {
                            db.CommandTimeout = 3;


                            int oldzoneId = db.stp_SaveDriverLocationByZone(driverId, latitude, longitude, speed, jobId).FirstOrDefault().Column1.ToInt();
                            lastConnectionDateTime = DateTime.Now;
                            lastSaveDriverLocationTimeout = null;
                            retryDriverLocTimeout = 1;


                            if (oldzoneId == 0 || speed > 0)
                            //(oldzoneId == 0 ||
                            //      speed > 0
                            //      )
                            //      )
                            {

                                Global.LoadDataList();


                                string returnLoc = string.Empty;
                                string postcode = string.Empty;
                                bool hasChanges = false;
                                int zoneId = 0;
                                string newZoneName = string.Empty;

                                try
                                {
                                    if (latitude > 0 && oldzoneId != -2)
                                    {
                                        int[] plot = (from a in Instance.listOfZone.Where(c => (c.DisableRank == null || c.DisableRank == false)

                                                      &&
                                                      (
                                                                  (c.shapeType != "" && c.shapeType == "circle")
                                                                   || ((latitude >= c.MinLat && latitude <= c.MaxLat)
                                                                             && (longitude <= c.MaxLng && longitude >= c.MinLng))

                                                                             )
                                                                             )
                                                      orderby a.PlotKind
                                                      select a.Id
                                                      ).ToArray<int>();

                                        if (plot.Count() > 0)
                                        {


                                            foreach (int plotId in plot)
                                            {
                                                if (General.FindPoint(latitude, longitude, Instance.listofPolyVertices.Where(c => c.ZoneId == plotId).ToList()))
                                                {
                                                    zoneId = plotId;
                                                    break;

                                                }

                                            }

                                        }

                                    }


                                    if (zoneId == 0 && latitude > 0 && oldzoneId != -2)
                                    {

                                        if (Instance.objPolicy.EnablePOI.ToBool())
                                        {

                                            try
                                            {
                                                returnLoc = db.PostCodesNearLatLong(latitude, longitude).FirstOrDefault().DefaultIfEmpty().Street.ToStr();

                                                if (returnLoc.Length > 0)
                                                {

                                                    postcode = GetPostCodeMatchWithBase(returnLoc, true);

                                                    if (!string.IsNullOrEmpty(returnLoc) && string.IsNullOrEmpty(postcode))
                                                    {
                                                        postcode = GetPostCodeMatch(returnLoc);

                                                    }
                                                }
                                            }
                                            catch
                                            {

                                            }
                                        }




                                        if (!string.IsNullOrEmpty(returnLoc))
                                        {

                                            if (Instance.objPolicy.AutoZonePlotType.ToStr() == "postcode")
                                            {
                                                newZoneName = GetHalfPostCodeMatch(returnLoc.ToStr().ToUpper());
                                            }

                                            if (string.IsNullOrEmpty(newZoneName))
                                            {

                                                string[] LocString = returnLoc.ToStr().ToUpper().Split(',');


                                                if (LocString.Count() == 4)
                                                {
                                                    newZoneName = LocString[1].Trim().Split(' ').LastOrDefault().ToUpper().Trim();
                                                }
                                                else if (LocString.Count() == 3)
                                                {
                                                    newZoneName = LocString[1].Trim().Split(' ').LastOrDefault().ToUpper().Trim();
                                                }
                                                else if (LocString.Count() == 5)
                                                {
                                                    newZoneName = LocString[2].Trim().Split(' ').LastOrDefault().ToUpper().Trim();
                                                }
                                                else if (LocString.Count() == 6)
                                                {
                                                    newZoneName = LocString[4].Trim().Split(' ').LastOrDefault().ToUpper().Trim();
                                                }
                                                else if (LocString.Count() == 1)
                                                {
                                                    newZoneName = postcode;

                                                    if (newZoneName.ToStr().Length == 0)
                                                    {
                                                        try
                                                        {
                                                            if (LocString[0].Contains(" "))
                                                            {
                                                                newZoneName = LocString[0].Split(new char[] { ' ' })[0];
                                                            }
                                                            else
                                                            {
                                                                newZoneName = LocString[0].ToStr().Trim();

                                                            }
                                                        }
                                                        catch
                                                        {


                                                        }

                                                    }
                                                }
                                                else if (LocString.Count() == 2)
                                                {
                                                    newZoneName = LocString[0].Trim().Split(' ').LastOrDefault().ToUpper().Trim();
                                                }
                                                else if (LocString.Count() == 0)
                                                {
                                                    newZoneName = postcode;
                                                }
                                            }





                                            if (newZoneName.ToStr().Trim().Length > 0)
                                                zoneId = -1;


                                        }
                                    }

                                    if (zoneId > 0 || zoneId == -1)
                                    {
                                        Fleet_Driver_Location item = db.Fleet_Driver_Locations.FirstOrDefault(c => c.DriverId == driverId);

                                        if (item != null)
                                        {
                                            hasChanges = false;
                                            item.LocationName = "";
                                            bool SendRankChangedNotification = false;

                                            if ((item.DisableAutoPlotting.ToBool() == false || Instance.objPolicy.EnableFixedPlotting == false))
                                            {





                                                if (item.ZoneId != null && item.NewZoneName == "SIN BIN")

                                                {


                                                    if (item.PlotDate != null
                                                       && item.SinBinTillOn != null && item.SinBinTillOn.Value < DateTime.Now)
                                                    {
                                                        try
                                                        {

                                                            db.stp_UnBlockDriver(item.DriverId, null);

                                                        }
                                                        catch
                                                        {


                                                        }

                                                    }
                                                    else
                                                    {
                                                        zoneId = -2;
                                                    }


                                                }
                                                //else
                                                //{
                                                //    zoneId = -1;

                                                //}

                                                if (zoneId > 0 && (item.PrevZoneId.ToInt() != zoneId || item.ZoneId == null))
                                                {



                                                    hasChanges = true;

                                                    item.PrevZoneId = zoneId;
                                                    item.ZoneId = zoneId;

                                                    //   item.PreviousZone = "";
                                                    item.NewZoneName = "";
                                                    //item.PlotDate = DateTime.Now;
                                                    if (Instance.objPolicy != null && Instance.objPolicy.EnableBookingOtherCharges.ToBool() && item.LastActiveZoneName.ToStr().ToLower().Trim() != "")
                                                    {
                                                        item.PlotDate = new DateTime(1753, 2, 1, 1, 1, 1);

                                                        SendRankChangedNotification = true;



                                                    }
                                                    else
                                                        item.PlotDate = DateTime.Now;
                                                }

                                                if (zoneId == -1)
                                                {
                                                    if (string.IsNullOrEmpty(item.PreviousZone) || item.PreviousZone.ToStr() != newZoneName.ToStr())
                                                    {
                                                        hasChanges = true;

                                                        item.PreviousZone = newZoneName;
                                                        item.NewZoneName = newZoneName;
                                                        item.ZoneId = null;
                                                        //  item.PreviousZone = null;
                                                        item.PlotDate = DateTime.Now;
                                                    }
                                                }


                                            }


                                            if (hasChanges)
                                            {
                                                db.SubmitChanges();



                                                General.BroadCastMessage("**refresh plots");

                                                try
                                                {
                                                    Clients.Caller.requestZoneUpdates("true");

                                                    if (SendRankChangedNotification)
                                                        BroadCastPostionChanged(driverId);
                                                }
                                                catch
                                                {

                                                }
                                            }
                                        }
                                    }



                                }
                                catch (Exception ex)
                                {
                                    //     WriteLog("LOG X:" + DateTime.Now.ToStr() + ":" + ex.Message + Environment.NewLine);

                                    try
                                    {

                                        File.AppendAllText(physicalPath + "\\" + "exception_updatezone.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + mesg + "," + ex.Message + Environment.NewLine);
                                    }
                                    catch
                                    {


                                    }

                                }





                            }


                        }
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            lastSaveDriverLocationTimeout = DateTime.Now.AddSeconds(4);
                            retryDriverLocTimeout++;
                            //   File.AppendAllText("excep_savedriverlocation.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt") + ":" + dataValue + " ," + ex.Message + ",retrycnt:" + retryDriverLocTimeout.ToStr() + Environment.NewLine);
                        }
                        catch
                        {

                        }

                        //    if (retryDriverLocTimeout >= 3)
                        //        RestartProgram();
                    }
                }

                //  GC.Collect();

                //16022022
                ///New work PlotBidding -----------------------------------------------------------------------------------
                ///
                requestPlotsBidding(req.PlotBidding.ToStr());

                //16022022
                ///New work GpsError -----------------------------------------------------------------------------------
                ///
                //try
                //{


                //    File.AppendAllText(physicalPath + "\\" + "SendData GpsError.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + req.GpsError + Environment.NewLine);
                //}
                //catch
                //{


                //}


                return;
            }
            catch (Exception ex)
            {




                try
                {
                    //  Clients.Caller.exceptionOccured(ex.Message);
                    File.AppendAllText(physicalPath + "\\" + "exception_senddata.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + json.ToStr() + "," + ex.Message + Environment.NewLine);
                }
                catch
                {


                }
            }
        }

  

        public void requestPlotsBidding(string mesg)
        {

            if (mesg.ToStr().Trim().Length == 0)
                return;


            List<ClsPlotBidding> list = new List<ClsPlotBidding>();
            string response = string.Empty;
            try
            {
                //
                try
                {
                    File.AppendAllText(physicalPath + "\\requestPlotsBidding.txt", DateTime.Now + ": datavalue=" + mesg + Environment.NewLine);

                }
                catch
                {

                }





                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });


                string[] arr = null;

                using (TaxiDataContext db = new TaxiDataContext())
                {
                    try
                    {
                        db.CommandTimeout = 4;

                        int driverId = values[0].ToInt();

                        string postedFrom = string.Empty;




                        int? statusId = null;





                        string statusName = "";


                        if (values.Count() > 3 && values[3].ToStr().IsNumeric() && values[3].ToStr().ToDecimal() > 100)
                        {
                            statusId = db.Fleet_DriverQueueLists.Where(c => c.DriverId == driverId && c.Status == true).Select(c => c.DriverWorkStatusId).FirstOrDefault();

                            statusName = "Available";

                        }



                        if (statusId != null)
                        {

                            if (statusId == Enums.Driver_WORKINGSTATUS.ONBREAK)
                                statusName = "OnBreak";

                            else if (statusId == Enums.Driver_WORKINGSTATUS.FOJ)
                                statusName = "FOJ";
                            else if (statusId == Enums.Driver_WORKINGSTATUS.NOTAVAILABLE)
                                statusName = "PassengerOnBoard";
                            else if (statusId == Enums.Driver_WORKINGSTATUS.ONROUTE)
                                statusName = "ONROUTE";
                            else if (statusId == Enums.Driver_WORKINGSTATUS.ARRIVED)
                                statusName = "ARRIVED";
                            else if (statusId == Enums.Driver_WORKINGSTATUS.SOONTOCLEAR)
                                statusName = "SoonToClear";
                            else if (statusId == Enums.Driver_WORKINGSTATUS.SINBIN)
                                statusName = "SINBIN";
                        }

                        var result = db.stp_GetAreaPlotsByVehicle(driverId, Instance.objPolicy.PlotsJobExpiryValue1, Instance.objPolicy.PlotsJobExpiryValue2).ToList();

                        //

                        int? driverZoneId = db.Fleet_Driver_Locations.Where(c => c.DriverId == driverId)
                            .Select(a => a.ZoneId).FirstOrDefault();

                        string driverZoneName = string.Empty;
                        string rank = "";
                        if (driverZoneId != null)
                        {
                            //
                            try
                            {

                                driverZoneName = Instance.listOfZone.FirstOrDefault(c => c.Id == driverZoneId).Area;

                                var objRank = db.ExecuteQuery<ClsDriverRank>("exec stp_getdriverrank {0},{1}", driverId, driverZoneId).FirstOrDefault();



                                if (objRank == null)
                                    rank = "-";
                                else
                                    rank = objRank.Rank.ToStr();
                                //

                                //                                try
                                //                                {
                                //                                    File.AppendAllText(physicalPath + "\\stp_getdriverrank.txt", DateTime.Now + ": driverid=" + driverId+", zoneid:"+driverZoneId + ",rank:"+rank+ Environment.NewLine);
                                ////
                                //                                }
                                //                                catch
                                //                                {
                                //                                    //
                                //                                }
                            }
                            catch (Exception ex)
                            {
                                //try
                                //{
                                //    rank = "-";
                                //    File.AppendAllText(physicalPath + "\\stp_getdriverrank_exception.txt", DateTime.Now + ": driverid=" + driverId + ", zoneid:" + driverZoneId + ",Rank:"+rank+ ",exception:"+ex.Message+Environment.NewLine);

                                //}
                                //catch
                                //{

                                //}

                            }
                        }

                        stp_GetAreaPlotsByVehicleResult driverPlotRow = null;
                        if (driverZoneName.ToStr().Trim().Length > 0)
                        {

                            driverPlotRow = result.Where(c => c.ZoneName == driverZoneName).FirstOrDefault();
                            if (driverPlotRow != null)
                            {
                                result.Remove(driverPlotRow);

                                //arr = (from a in result.Where(c => c.ZoneName != driverZoneName)
                                //       orderby a.Drivers descending, a.orderno
                                //       select (a.ZoneName + "," +
                                //                a.ExpiryJobs1 + "," + a.ExpiryJobs2
                                //           + "," + a.Drivers)).ToArray<string>();



                                //response = driverPlotRow.ZoneName + "," + driverPlotRow.ExpiryJobs1 + "," + driverPlotRow.ExpiryJobs2 + "," + driverPlotRow.Drivers;

                                //if (arr.Count() > 0)
                                //    response += ">>";

                                //response += string.Join(">>", arr);



                            }
                            else
                            {

                                //arr = (from a in result
                                //       orderby a.Drivers descending, a.orderno
                                //       select (a.ZoneName + "," +
                                //                a.ExpiryJobs1 + "," + a.ExpiryJobs2
                                //           + "," + a.Drivers)).ToArray<string>();

                                //response = string.Join(">>", arr);

                            }

                        }
                        //else
                        //{
                        //    arr = (from a in result
                        //           orderby a.Drivers descending, a.orderno
                        //           select (a.ZoneName + "," +
                        //                    a.ExpiryJobs1 + "," + a.ExpiryJobs2
                        //               + "," + a.Drivers)).ToArray<string>();

                        //    response = string.Join(">>", arr);

                        //}

                        if (driverPlotRow != null)
                        {
                            list.Add(new ClsPlotBidding { ZoneName = driverPlotRow.ZoneName, Drivers = driverPlotRow.Drivers.ToInt(), J15 = driverPlotRow.ExpiryJobs1.ToInt(), J30 = driverPlotRow.ExpiryJobs2.ToInt(), BidDetails = 1, Rank = rank, DriverWorkStatus = statusName });

                        }
                        else
                        {

                            if (statusId != null)
                            {
                                list.Add(new ClsPlotBidding { ZoneName = "-", Drivers = 0, J15 = 0, J30 = 0, BidDetails = 1, Rank = rank, DriverWorkStatus = statusName });

                            }
                        }





                        var arr2 = (from a in db.ExecuteQuery<stp_getbiddingAvailablejobsResult>("exec stp_getbiddingjobs {0}", driverId)
                             .Where(c => c.zonename != "")

                                    select new

                                    {
                                        Distance = a.biddingradius > 0 && a.JobLatitude != null && a.JobLatitude != 0 ? new LatLng(a.latitude, a.longitude).DistanceMiles(new LatLng(Convert.ToDouble(a.JobLatitude), Convert.ToDouble(a.JobLongitude))) : 0,
                                        a.zonename,
                                        a.biddingradius,
                                        a.zoneid

                                    }).Where(c => c.Distance <= c.biddingradius)
                                    .GroupBy(args => new
                                    {
                                        args.zonename,
                                        args.zoneid

                                    })



                                 .Select(args => new
                                 {
                                     args.Key.zoneid,
                                     args.Key.zonename,
                                     Jobs = args.Count()


                                 }).ToList();


                        //.Select(args => (args.zoneid + "<<" + args.zonename + "<<" + args.Jobs)).ToArray<string>();
                        foreach (var item in arr2)
                        {
                            var item2 = list.FirstOrDefault(c => c.ZoneName == item.zonename);

                            if (item2 != null)
                            {

                                item2.ZoneId = item.zoneid.ToInt();
                                item2.Bid = item.Jobs;
                                item2.BidDetails = 1;
                                item2.Rank = rank;
                                item2.DriverWorkStatus = statusName;
                            }
                            else
                                list.Add(new ClsPlotBidding { ZoneId = item.zoneid.ToInt(), ZoneName = item.zonename, Drivers = 0, J15 = 0, J30 = 0, Bid = item.Jobs, BidDetails = 1, Rank = rank, DriverWorkStatus = statusName });




                        }

                        //
                        foreach (var item in result)
                        {
                            var item2 = list.FirstOrDefault(c => c.ZoneName == item.ZoneName);

                            if (item2 != null)
                            {
                                item2.Drivers = item.Drivers.ToInt();
                                item2.J15 = item.ExpiryJobs1.ToInt();
                                item2.J30 = item.ExpiryJobs2.ToInt();
                                item2.DriverWorkStatus = statusName;
                            }
                            else
                            {
                                list.Add(new ClsPlotBidding { ZoneName = item.ZoneName, Drivers = item.Drivers.ToInt(), J15 = item.ExpiryJobs1.ToInt(), J30 = item.ExpiryJobs2.ToInt(), Rank = rank, DriverWorkStatus = statusName });


                            }

                        }

                        response = new JavaScriptSerializer().Serialize(list);

                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            File.AppendAllText(physicalPath + "\\exception_plotsBidding.txt", DateTime.Now.ToStr() + ": " + ex.Message + Environment.NewLine);


                            

                        }
                        catch
                        {

                        }
                    }
                }

                //send message back to PDA
                Clients.Caller.plotsBiddings(response);


            }
            catch (Exception ex)
            {
                Clients.Caller.plotsBiddings("exceptionoccurred");
            }
        }

   


        public void requestPlotsBiddingDetails(string mesg)
        {

            List<ClsPlotDetails> list = new List<ClsPlotDetails>();
            try
            {

                //try
                //{

                //    File.AppendAllText(physicalPath + "\\" + "requestPlotsBiddingDetails.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + mesg + Environment.NewLine);
                //}
                //catch
                //{


                //}

                ClsPlotDetails obj = new JavaScriptSerializer().Deserialize<ClsPlotDetails>(mesg);

                string plot = obj.PlotName.ToStr().Trim();
                using (TaxiDataContext db = new TaxiDataContext())
                {
                    int? zoneId = obj.PlotId;


                    //try
                    //{
                    //    zoneId = Instance.listOfZone.Where(c => c.Area == plot).Select(c => c.Id).FirstOrDefault();
                    //}
                    //catch
                    //{

                    //}

                    //if (zoneId == null)
                    //    zoneId = db.Gen_Zones.Where(c => c.ZoneName == plot).Select(c => c.Id).FirstOrDefault();




                    var arr = db.ExecuteQuery<ClsPlotBidDetail>("exec stp_getbiddingdetailjobs {0},{1}", obj.DriverId.ToInt(), zoneId)
                        .ToList();
                    //     .OrderBy(c => c.PickupDateTime);




                    //.Select(args => (args.zoneid + "<<" + (args.zonename.Length > 0 ? args.zonename : " - ") + "<<" + "0" + "<<" + args.JobId + "<<" + args.PickupDateTime + "<<" + args.FromAddress + "<<" + args.ToAddress + "<<" + args.FareRate + "<<" + args.VehicleType))

                    //  .ToArray<string>();


                    if (obj.Version.ToStr().Trim().Length > 0)
                    {


                        if (obj.Version.ToStr().Trim().ToDecimal() >= 41.72m)
                        {

                            foreach (var item in arr)
                            {

                                list.Add(new ClsPlotDetails { PlotName = item.ZoneName, PlotId = item.ZoneId.ToInt(), JobId = item.JobId, PickupAddress = "Pickup:" + item.FromAddress, DropOff = "DropOff:" + item.ToAddress });


                            }
                        }
                        else
                        {
                            foreach (var item in arr)
                            {

                                list.Add(new ClsPlotDetails { PlotName = item.ZoneName, PlotId = item.ZoneId.ToInt(), JobId = item.JobId, PickupAddress = item.FromAddress, DropOff = item.ToAddress });
                            }


                        }
                    }
                    else
                    {
                        foreach (var item in arr)
                        {

                            list.Add(new ClsPlotDetails { PlotName = item.ZoneName, PlotId = item.ZoneId.ToInt(), JobId = item.JobId, PickupAddress = item.FromAddress, DropOff = item.ToAddress });
                        }


                    }
                    // = db.ExecuteQuery<ClsPlotDetails>("exec stp_GetAreaPlotsDetailsByVehicle {0},{1},{2},{3},{4}", obj.DriverId, zoneId, plot, 15, 30).ToList();
                }



                //


                if (list == null)
                    list = new List<ClsPlotDetails>();

                if (list.Count == 0)
                    Clients.Caller.plotBiddingDetails("");
                else
                    Clients.Caller.plotBiddingDetails(list);
            }
            catch (Exception ex)
            {

                try
                {
                    Clients.Caller.plotDetails("");
                    File.AppendAllText(physicalPath + "\\" + "requestPlotsBiddingDetails_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + mesg + " ,exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {


                }
            }

        }



        public void requestChangePaymentType(string mesg)
        {
            string resp = "";
            ClsChangePaymentType obj = null;
            try
            {

                obj = new JavaScriptSerializer().Deserialize<ClsChangePaymentType>(mesg);
                if (obj != null)
                {
                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        db.DeferredLoadingEnabled = true;
                        long jobId = obj.JobId.ToLong();
                        var book = db.Bookings.Where(c => c.Id == jobId).Select(args => new { args.CustomerName, args.CustomerMobileNo, args.CustomerPhoneNo, args.CustomerEmail, args.BookingTypeId })
                          .FirstOrDefault();




                        string objCust = null;

                        string mobileNo = book.CustomerMobileNo.ToStr().Trim();
                        string phoneNo = book.CustomerPhoneNo.ToStr().Trim();
                        string email = book.CustomerEmail.ToStr().Trim();

                        if (objCust == null && mobileNo.Length > 0)
                        {
                            objCust = db.Customers.Where(c => c.MobileNo == mobileNo
                            && (c.CreditCardDetails != null && c.CreditCardDetails != ""))
                             .Select(c => c.CreditCardDetails).FirstOrDefault();

                        }

                        if (objCust == null && phoneNo.Length > 0)
                        {
                            objCust = db.Customers.Where(c => c.TelephoneNo == phoneNo
                               && (c.CreditCardDetails != null && c.CreditCardDetails != ""))
                               .Select(c => c.CreditCardDetails).FirstOrDefault();

                        }


                        if (objCust == null && email.Length > 0)
                        {
                            objCust = db.Customers.Where(c => c.Email == email
                               && (c.CreditCardDetails != null && c.CreditCardDetails != ""))
                               .Select(c => c.CreditCardDetails).FirstOrDefault();

                        }


                        if (objCust != null && objCust.ToStr().Trim().Length > 0)
                        {

                            db.ExecuteQuery<int>("update booking set paymenttypeid=2,customercreditcarddetails='" + objCust + "' where id=" + jobId);
                            // resp = "success:Payment type has been changed successfully!";
                            resp = "true";
                        }
                        else
                        {

                            if(db.Gen_SysPolicy_PaymentDetails.Where(c=>c.PaymentGatewayId==9).Count()>0)
                                resp = "true";
                            //else
                            //resp = "false:Your Card is not Registered";

                        }

                    }
                }
            }
            catch
            {
            }

            Clients.Caller.changePaymentType(resp);

        }

        public void requestPlots(string mesg)
        {
            string response = string.Empty;
            try
            {

                try
                {
                    File.AppendAllText(physicalPath + "\\requestPlots.txt", DateTime.Now + ": datavalue=" + mesg + Environment.NewLine);

                }
                catch
                {

                }





                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });


                string[] arr = null;

                using (TaxiDataContext db = new TaxiDataContext())
                {
                    try
                    {
                        db.CommandTimeout = 4;

                        string driverId = values[1].ToStr().Trim();

                        string postedFrom = string.Empty;


                        string newDriverId = string.Empty;

                        if (driverId.ToStr().IsNumeric() == false)
                        {
                            foreach (char item in driverId)
                            {
                                if (char.IsDigit(item))
                                {
                                    newDriverId += item;
                                }
                                else
                                    break;
                            }
                        }
                        else
                            newDriverId = driverId;


                        int driverId2 = newDriverId.ToInt();

                        var result = db.stp_GetAreaPlotsByVehicle(driverId2, Instance.objPolicy.PlotsJobExpiryValue1, Instance.objPolicy.PlotsJobExpiryValue2).ToList();



                        int? driverZoneId = db.Fleet_Driver_Locations.Where(c => c.DriverId == driverId2)
                            .Select(a => a.ZoneId).FirstOrDefault();

                        string driverZoneName = string.Empty;
                        if (driverZoneId != null)
                        {
                            try
                            {

                                driverZoneName = Instance.listOfZone.FirstOrDefault(c => c.Id == driverZoneId).Area;
                            }
                            catch
                            {

                            }
                        }

                        if (driverZoneName.ToStr().Trim().Length > 0)
                        {

                            var driverPlotRow = result.Where(c => c.ZoneName == driverZoneName).FirstOrDefault();
                            if (driverPlotRow != null)
                            {
                                result.Remove(driverPlotRow);

                                arr = (from a in result.Where(c => c.ZoneName != driverZoneName)
                                       orderby a.Drivers descending, a.orderno
                                       select (a.ZoneName + "," +
                                                a.ExpiryJobs1 + "," + a.ExpiryJobs2
                                           + "," + a.Drivers)).ToArray<string>();



                                response = driverPlotRow.ZoneName + "," + driverPlotRow.ExpiryJobs1 + "," + driverPlotRow.ExpiryJobs2 + "," + driverPlotRow.Drivers;

                                if (arr.Count() > 0)
                                    response += ">>";

                                response += string.Join(">>", arr);



                            }
                            else
                            {

                                arr = (from a in result
                                       orderby a.Drivers descending, a.orderno
                                       select (a.ZoneName + "," +
                                                a.ExpiryJobs1 + "," + a.ExpiryJobs2
                                           + "," + a.Drivers)).ToArray<string>();

                                response = string.Join(">>", arr);

                            }

                        }
                        else
                        {
                            arr = (from a in result
                                   orderby a.Drivers descending, a.orderno
                                   select (a.ZoneName + "," +
                                            a.ExpiryJobs1 + "," + a.ExpiryJobs2
                                       + "," + a.Drivers)).ToArray<string>();

                            response = string.Join(">>", arr);

                        }

                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            File.AppendAllText(physicalPath + "\\exception_plots.txt", DateTime.Now.ToStr() + ": " + ex.Message + Environment.NewLine);
                        }
                        catch
                        {

                        }
                    }
                }

                //send message back to PDA
                Clients.Caller.plots(response);

                //Byte[] byteResponse = Encoding.UTF8.GetBytes(driverPlot + string.Join(">>", arr));
                //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);
            }
            catch (Exception ex)
            {
                Clients.Caller.plots(ex.Message);
            }
        }

        public void actionButton(string mesg)
        {
            byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

            string dataValue = mesg;
            dataValue = dataValue.Trim();

            string[] values = dataValue.Split(new char[] { '=' });

            int valCnt = values.Count();

            try
            {
                //try
                //{
                //
                //    File.AppendAllText(physicalPath + "\\actionbutton.txt", DateTime.Now.ToStr() + " request" + dataValue + Environment.NewLine);
                //}
                //catch
                //{

                //}
                int jobStatusId = values[3].ToInt();

                string respo = "true";
                if (valCnt >= 7)
                {
                    

                    if (jobStatusId == Enums.BOOKINGSTATUS.ONROUTE)
                    {
                        int driverId = values[2].ToInt();

                        using (TaxiDataContext db = new TaxiDataContext())
                        {
                            if (dataValue.ToStr().Contains("verify"))
                            {
                                if ((db.Bookings.Count(c => c.Id == values[1].ToLong()
                                    && (c.BookingStatusId == Enums.BOOKINGSTATUS.PENDING || c.BookingStatusId == Enums.BOOKINGSTATUS.NOTACCEPTED
                                    || c.BookingStatusId == Enums.BOOKINGSTATUS.BID
                                    || c.BookingStatusId == Enums.BOOKINGSTATUS.FOJ
                                    || c.BookingStatusId == Enums.BOOKINGSTATUS.PENDING_START
                                    || (c.BookingStatusId == Enums.BOOKINGSTATUS.ONROUTE && c.DriverId == driverId)
                                    )) > 0)
                                    )
                                {
                                    if (db.Fleet_DriverQueueLists.Count(c => c.Status == true && c.DriverId != values[2].ToInt() && c.CurrentJobId == values[1].ToLong()
                                        && c.DriverWorkStatusId == Enums.Driver_WORKINGSTATUS.ONROUTE) > 0)
                                    {
                                        respo = "false:This Job is no longer available";

                                        db.stp_UpdateJobStatus(values[1].ToLong(), Enums.BOOKINGSTATUS.ONROUTE);

                                        General.BroadCastMessage("refresh active dashboard");

                                        try
                                        {
                                            File.AppendAllText("settledoubledrvlog.txt", "datastring:" + dataValue + " on time :" + DateTime.Now.ToStr());
                                        }
                                        catch
                                        {

                                        }
                                    }
                                    else
                                    {
                                        string alterProcedureScript = @"
                        ALTER PROCEDURE [dbo].[stp_UpdateJob]                                                                                        
                                                
(                                                                                        
                                               
  @jobId as bigint,                                                                                        
                                                
  @DriverId as int,                                                                                        
                                                
  @JobStatusId as int,                                                                                        
                                                
  @DriverWorkStatusId int,                                                                                        
                                                
  @SinBinTimer int                                                                                        
                                                
)                                                                                        
                                                
                                                
AS                                                                                        
                                               
SET NOCOUNT ON                                                                                        
                                                
Begin                                                                                        
                                                
      Declare @DestZoneId int                                                                                        
                                                
      Declare @DriverCurrentJobId bigint                                                                                        
                                                
      DECLARE @BookingTypeId int                                                                                        
                                                
      declare @sinbinMins int                                                                                       
                                                
                                                
 if( (@JobStatusId=10 or @JobStatusId=11 or @JobStatusId=12))                                                                                       
    select @sinbinMins=SinBinMinutes from Gen_SysPolicy_SinBinSettings where sinbintypeid=@JobStatusId                                                                                        
                                                
                                                
         if(@JobStatusId=10 or @JobStatusId=11 or @JobStatusId=12 or @JobStatusId=13 or @jobstatusid=2)                                                                                        
                                                
                                                
            set @DriverCurrentJobId=NULL                                                                                        
                                                
                                                
        else                                                                                        
                                                
                                                
       set @DriverCurrentJobId=@JobId                                                                                        
                                                
       if(@JobStatusId=5 or @JobStatusId=11)                                                                                        
                
       begin               
                                                
                                                
        declare @fleetMasterId int               
  --set @fleetMasterId=null                                                          
                     declare @currstatusid int                             
                                                
            select @fleetMasterId=fleetMasterId,@currstatusid=DriverWorkStatusId from fleet_DriverQueueList where driverId=@driverId and status=1                                                                                    
                                                
                                   
            Declare @AcceptedDateTime DateTime                                  
                                                
                                                
            if(@JobStatusId=5)                                          
                                                
              SET  @AccepteddateTime=getdate()                                                           
                                                 
              DECLARE @pickup varchar(200)                                                              
                                                
              DECLARE @Destination varchar(200)                                                             
                                                
              if(@JobStatusId=5)                               
                                                
              begin                                                           
                                                
                                                
    Update booking set BookingStatusId=@JobStatusId,FleetMasterId=@fleetMasterId,AcceptedDateTime=@AcceptedDateTime ,DriverId=@DriverId                                                                                        
                                                
                                                
    where id=@jobId                                                                                        
                                                
                            
                             if(@currstatusid=7)                              
       Update fleet_driver_location set plotdate=getdate(),ZoneId=null,PrevZoneId=null,NewZoneName='',PreviousZone='', PickupPoint='',SinBinTillOn=null ,Destination='',disableautoplotting=0,LastActiveZoneName='' where driverid=@driverId                        
                             
                  else                            
                        Update fleet_driver_location set plotdate=getdate(),PickupPoint='',Destination='',disableautoplotting=0,LastActiveZoneName='' where driverid=@driverId                                                                   
                                   
               end                                                                    
                                                
               else                                                                                  
                                                
               begin                                                                                  
                                                
      Update booking set BookingStatusId=@JobStatusId,FleetMasterId=@fleetMasterId,AcceptedDateTime=@AcceptedDateTime,IsBidding=1,AutoDespatch=1                                                                                          
                   ,@pickup=FromAddress,@Destination=ToAddress ,IsConfirmedDriver=0                                                                                  
  
     where id=@jobId and DriverId=@DriverId                                                                          
                  
                            
         Update fleet_driver_location set plotdate=getdate(),PickupPoint='',Destination='',disableautoplotting=0 where driverid=@driverId                                                                   
                                   
               end                                                                                  
                    
                                                
            --  Update fleet_driver_location set plotdate=getdate(),PickupPoint='',Destination='',disableautoplotting=0 where driverid=@driverId                                      
                                                
                                                
    if(@JobStatusId=5)                                                                                      
                     
             begin                                                                           
                                                
                                                
      declare @drvNo varchar(100)                          
                                                
                                                
                                                
     select @drvNo=DriverNo from Fleet_Driver where id=@driverId                     
                                           
     insert into Booking_Log values(@jobid,'','','Job accepted by Driver ('+ISNULL(@drvNo,'')+')',getdate(),NULL)                       
                             
             end                                                                                                         
       END                                                                                        
                                                
                                                
       ELSE                                                                         
                                                
       begin                                                                                        
                                       
                                                
      if(@JobStatusId=6)  -- Arrive                                                                                        
                                           
            BEGIN                                                                                                                  
               Update booking set BookingStatusId=@JobStatusId,ArrivalDateTime=getdate()                                                                                        
                         
                     where  id=@jobId and driverid=@driverid                                                                                      
                                
            END                                                                                                                            
                                                
    else  if(@JobStatusId=7)    -- POB                                                                                                                   
     BEGIN                                                                                                                          
                                                
     declare @journeytypeId int                                                      
                declare @PickupZoneId int                                                      
                                                                      
                                          
               Update booking set BookingStatusId=@JobStatusId,POBDateTime=getdate(),@DestZoneId=dropoffzoneid ,@journeytypeId=journeytypeid, @PickupZoneId=zoneid  where  id=@jobId and driverid=@driverid                                                                              
                              if(@journeytypeId=3)                                             
                         begin                                                          
                                                                                   
                            if(@PickupZoneId is not NULL)                                  
       begin                               
       if exists(select * from gen_zones where id=@PickupZoneId and (DisableDriverRank is null or DisableDriverRank=0))                                
        begin                                
                                                       
          Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@PickupZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@PickupZoneId,previouszone='',newzonename='' where driverid= @driverId                    
                                             
                              end                                
         else                                
      
         begin                                
          select @PickupZoneId=BackupZone1Id from Gen_Zone_Backups where zoneid=@PickupZoneId                                
                                
               if(@PickupZoneId is not null and @PickupZoneId>0)                                
          Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@PickupZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@PickupZoneId,previouszone='',newzonename='' where driverid= @driverId              
           
         end                                
                       
       end                                
                                                                                   
                         end                                                          
                         else                                                          
                         begin                                                          
                                                                          
                          if(@DestZoneId is not NULL)                                 
        begin                                
       if exists(select * from gen_zones where id=@DestZoneId and (DisableDriverRank is null or DisableDriverRank=0))                                
                  begin                                
                      --  select @DestZoneId=BackupZone1Id from Gen_Zone_Backups where zoneid=@DestZoneId                                
                                
              -- if(@DestZoneId is not null and @DestZoneId>0)                                
            Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@DestZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@DestZoneId,previouszone='',newzonename='' where driverid=@driverId                 
                                          
                end                                 
          else                                
                                
   begin                                
                    select @DestZoneId=BackupZone1Id from Gen_Zone_Backups where zoneid=@DestZoneId                                
                                
               if(@DestZoneId is not null and @DestZoneId>0)                             
            Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@DestZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@DestZoneId,previouszone='',newzonename='' where driverid=@driverId                 
                  
          end                                
                                 
       end                                          
                         end                                
                                  
                                           
                                                
 --                                       if(@DestZoneId is not NULL)                                             
                                                
  --  Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@DestZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@DestZoneId,PrevZoneId=@DestZoneId,previouszone='',newzonename='' where driverid=@driverId
                                         
    END                                                                                    
                                   
                                                
   else  if(@JobStatusId=8)  -- STC                                                                                        
                                    
            BEGIN                                                                                        
                               
               Update booking set BookingStatusId=@JobStatusId,STCDateTime=getdate()             
                         
                     where  id=@jobId and driverid=@driverid                                                                                 
            END                                                                                        
                                  
            else  if(@JobStatusId=2)  --  DISPATCHED OR COMPLETED                                                                                        
                     
            BEGIN                                                                                        
                              
               Update booking set BookingStatusId=@JobStatusId,ClearedDateTime=getdate()                                    
                         
                     where  id=@jobId and driverid=@driverid                                                                                        
                                   
            END                                                                                        
                                
             else  if(@JobStatusId=3 or @JobStatusId=13 ) -- Cancel                                                                                        
                                                
            BEGIN                                                                                        
                                                
               Update booking set BookingStatusId=@JobStatusId                                   
                 
                                                
                     where  id=@jobId                                                    
                                                
                                                
                                                
            END                                                                                        
                                                
            else  if(@JobStatusId=10) -- No Show                                                                                        
                                                
                                                
                                               
     BEGIN                                                                                        
                                                
                      
               Update booking set BookingStatusId=@JobStatusId,ArrivalDateTime=getdate(),AutoDespatch=0,IsBidding=0                                                         
                                                
                                                
                     where  id=@jobId and driverid=@driverid                                                                                        
                                                
                                                
            END                                                                                        
                                                                                 
    else  if(@JobStatusId=1) -- Waiting                                 
                                                
                                                
            BEGIN                                                                                        
                 
               Update booking set BookingStatusId=@JobStatusId,ArrivalDateTime=NULL ,IsConfirmedDriver=0,AutoDespatch=0,IsBidding=0   where  id=@jobId                                                                                       
                                                
                                                
                                 
                     --     ,DriverId=NULL                                                                                        

                   --  where  id=@jobId                                                                        
 
                         INSERT INTO dbo.Fleet_Driver_RejectJobs (DriverId,BookingId,RejectedDateTime,BookingStatusId)                                                                                                    
               values (@DriverId,@JobId,getdate(),10)                                                  
 
            END                                                                                        
  
         else  if(@JobStatusId=12) -- Not Accepted                                                                                        
                                                 
                                     
begin                                                               
                                          
    Update booking set BookingStatusId=@JobStatusId                                                             
                                     
                     where  id=@jobId                                                                                        
                                                 
    END                                                                                        
                                         
       END";

                                        (new TaxiDataContext()).ExecuteCommand(alterProcedureScript);
                                        db.stp_UpdateJob(values[1].ToLong(), values[2].ToInt(), values[3].ToInt(), values[4].ToInt(), Instance.objPolicy.SinBinTimer.ToInt());
                                    }
                                }
                                else
                                {
                                    respo = "false:This Job is no longer available";

                                    try
                                    {
                                        File.AppendAllText("settledoubledrvlogcond2.txt", "datastring:" + dataValue + " on time :" + DateTime.Now.ToStr());
                                    }
                                    catch
                                    {

                                    }
                                }
                            }
                            else
                            {
                                string alterProcedureScript = @"
                        ALTER PROCEDURE [dbo].[stp_UpdateJob]                                                                                        
                                                
(                                                                                        
                                               
  @jobId as bigint,                                                                                        
                                                
  @DriverId as int,                                                                                        
                                                
  @JobStatusId as int,                                                                                        
                                                
  @DriverWorkStatusId int,                                                                                        
                                                
  @SinBinTimer int                                                                                        
                                                
)                                                                                        
                                                
                                                
AS                                                                                        
                                               
SET NOCOUNT ON                                                                                        
                                                
Begin                                                                                        
                                                
      Declare @DestZoneId int                                                                                        
                                                
      Declare @DriverCurrentJobId bigint                                                                                        
                                                
      DECLARE @BookingTypeId int                                                                                        
                                                
      declare @sinbinMins int                                                                                       
                                                
                                                
 if( (@JobStatusId=10 or @JobStatusId=11 or @JobStatusId=12))                                                                                       
    select @sinbinMins=SinBinMinutes from Gen_SysPolicy_SinBinSettings where sinbintypeid=@JobStatusId                                                                                        
                                                
                                                
         if(@JobStatusId=10 or @JobStatusId=11 or @JobStatusId=12 or @JobStatusId=13 or @jobstatusid=2)                                                                                        
                                                
                                                
            set @DriverCurrentJobId=NULL                                                                                        
                                                
                                                
        else                                                                                        
                                                
                                                
       set @DriverCurrentJobId=@JobId                                                                                        
                                                
       if(@JobStatusId=5 or @JobStatusId=11)                                                                                        
                
       begin               
                                                
                                                
        declare @fleetMasterId int               
  --set @fleetMasterId=null                                                          
                     declare @currstatusid int                             
                                                
            select @fleetMasterId=fleetMasterId,@currstatusid=DriverWorkStatusId from fleet_DriverQueueList where driverId=@driverId and status=1                                                                                    
                                                
                                   
            Declare @AcceptedDateTime DateTime                                  
                                                
                                                
            if(@JobStatusId=5)                                          
                                                
              SET  @AccepteddateTime=getdate()                                                           
                                                 
              DECLARE @pickup varchar(200)                                                              
                                                
              DECLARE @Destination varchar(200)                                                             
                                                
              if(@JobStatusId=5)                               
                                                
              begin                                                           
                                                
                                                
    Update booking set BookingStatusId=@JobStatusId,FleetMasterId=@fleetMasterId,AcceptedDateTime=@AcceptedDateTime ,DriverId=@DriverId                                                                                        
                                                
                                                
    where id=@jobId                                                                                        
                                                
                            
                             if(@currstatusid=7)                              
       Update fleet_driver_location set plotdate=getdate(),ZoneId=null,PrevZoneId=null,NewZoneName='',PreviousZone='', PickupPoint='',SinBinTillOn=null ,Destination='',disableautoplotting=0,LastActiveZoneName='' where driverid=@driverId                        
                             
                  else                            
                        Update fleet_driver_location set plotdate=getdate(),PickupPoint='',Destination='',disableautoplotting=0,LastActiveZoneName='' where driverid=@driverId                                                                   
                                   
               end                                                                    
                                                
               else                                                                                  
                                                
               begin                                                                                  
                                                
      Update booking set BookingStatusId=@JobStatusId,FleetMasterId=@fleetMasterId,AcceptedDateTime=@AcceptedDateTime,IsBidding=1,AutoDespatch=1                                                                                          
                   ,@pickup=FromAddress,@Destination=ToAddress ,IsConfirmedDriver=0                                                                                  
  
     where id=@jobId and DriverId=@DriverId                                                                          
                  
                            
         Update fleet_driver_location set plotdate=getdate(),PickupPoint='',Destination='',disableautoplotting=0 where driverid=@driverId                                                                   
                                   
               end                                                                                  
                    
                                                
            --  Update fleet_driver_location set plotdate=getdate(),PickupPoint='',Destination='',disableautoplotting=0 where driverid=@driverId                                      
                                                
                                                
    if(@JobStatusId=5)                                                                                      
                     
             begin                                                                           
                                                
                                                
      declare @drvNo varchar(100)                          
                                                
                                                
                                                
     select @drvNo=DriverNo from Fleet_Driver where id=@driverId                     
                                           
     insert into Booking_Log values(@jobid,'','','Job accepted by Driver ('+ISNULL(@drvNo,'')+')',getdate(),NULL)                       
                             
             end                                                                                                         
       END                                                                                        
                                                
                                                
       ELSE                                                                         
                                                
       begin                                                                                        
                                       
                                                
      if(@JobStatusId=6)  -- Arrive                                                                                        
                                           
            BEGIN                                                                                                                  
               Update booking set BookingStatusId=@JobStatusId,ArrivalDateTime=getdate()                                                                                        
                         
                     where  id=@jobId and driverid=@driverid                                                                                      
                                
            END                                                                                                                            
                                                
    else  if(@JobStatusId=7)    -- POB                                                                                                                   
     BEGIN                                                                                                                          
                                                
     declare @journeytypeId int                                                      
                declare @PickupZoneId int                                                      
                                                                      
                                          
               Update booking set BookingStatusId=@JobStatusId,POBDateTime=getdate(),@DestZoneId=dropoffzoneid ,@journeytypeId=journeytypeid, @PickupZoneId=zoneid  where  id=@jobId and driverid=@driverid                                                                              
                              if(@journeytypeId=3)                                             
                         begin                                                          
                                                                                   
                            if(@PickupZoneId is not NULL)                                  
       begin                               
       if exists(select * from gen_zones where id=@PickupZoneId and (DisableDriverRank is null or DisableDriverRank=0))                                
        begin                                
                                                       
          Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@PickupZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@PickupZoneId,previouszone='',newzonename='' where driverid= @driverId                    
                                             
                              end                                
         else                                
      
         begin                                
          select @PickupZoneId=BackupZone1Id from Gen_Zone_Backups where zoneid=@PickupZoneId                                
                                
               if(@PickupZoneId is not null and @PickupZoneId>0)                                
          Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@PickupZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@PickupZoneId,previouszone='',newzonename='' where driverid= @driverId              
           
         end                                
                       
       end                                
                                                                                   
                         end                                                          
                         else                                                          
                         begin                                                          
                                                                          
                          if(@DestZoneId is not NULL)                                 
        begin                                
       if exists(select * from gen_zones where id=@DestZoneId and (DisableDriverRank is null or DisableDriverRank=0))                                
                  begin                                
                      --  select @DestZoneId=BackupZone1Id from Gen_Zone_Backups where zoneid=@DestZoneId                                
                                
              -- if(@DestZoneId is not null and @DestZoneId>0)                                
            Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@DestZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@DestZoneId,previouszone='',newzonename='' where driverid=@driverId                 
                                          
                end                                 
          else                                
                                
   begin                                
                    select @DestZoneId=BackupZone1Id from Gen_Zone_Backups where zoneid=@DestZoneId                                
                                
               if(@DestZoneId is not null and @DestZoneId>0)                             
            Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@DestZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@DestZoneId,previouszone='',newzonename='' where driverid=@driverId                 
                  
          end                                
                                 
       end                                          
                         end                                
                                  
                                           
                                                
 --                                       if(@DestZoneId is not NULL)                                             
                                                
  --  Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@DestZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@DestZoneId,PrevZoneId=@DestZoneId,previouszone='',newzonename='' where driverid=@driverId
                                         
    END                                                                                    
                                   
                                                
   else  if(@JobStatusId=8)  -- STC                                                                                        
                                    
            BEGIN                                                                                        
                               
               Update booking set BookingStatusId=@JobStatusId,STCDateTime=getdate()             
                         
                     where  id=@jobId and driverid=@driverid                                                                                 
            END                                                                                        
                                  
            else  if(@JobStatusId=2)  --  DISPATCHED OR COMPLETED                                                                                        
                     
            BEGIN                                                                                        
                              
               Update booking set BookingStatusId=@JobStatusId,ClearedDateTime=getdate()                                    
                         
                     where  id=@jobId and driverid=@driverid                                                                                        
                                   
            END                                                                                        
                                
             else  if(@JobStatusId=3 or @JobStatusId=13 ) -- Cancel                                                                                        
                                                
            BEGIN                                                                                        
                                                
               Update booking set BookingStatusId=@JobStatusId                                   
                 
                                                
                     where  id=@jobId                                                    
                                                
                                                
                                                
            END                                                                                        
                                                
            else  if(@JobStatusId=10) -- No Show                                                                                        
                                                
                                                
                                               
     BEGIN                                                                                        
                                                
                      
               Update booking set BookingStatusId=@JobStatusId,ArrivalDateTime=getdate(),AutoDespatch=0,IsBidding=0                                                         
                                                
                                                
                     where  id=@jobId and driverid=@driverid                                                                                        
                                                
                                                
            END                                                                                        
                                                                                 
    else  if(@JobStatusId=1) -- Waiting                                 
                                                
                                                
            BEGIN                                                                                        
                 
               Update booking set BookingStatusId=@JobStatusId,ArrivalDateTime=NULL ,IsConfirmedDriver=0,AutoDespatch=0,IsBidding=0   where  id=@jobId                                                                                       
                                                
                                                
                                 
                     --     ,DriverId=NULL                                                                                        

                   --  where  id=@jobId                                                                        
 
                         INSERT INTO dbo.Fleet_Driver_RejectJobs (DriverId,BookingId,RejectedDateTime,BookingStatusId)                                                                                                    
               values (@DriverId,@JobId,getdate(),10)                                                  
 
            END                                                                                        
  
         else  if(@JobStatusId=12) -- Not Accepted                                                                                        
                                                 
                                     
begin                                                               
                                          
    Update booking set BookingStatusId=@JobStatusId                                                             
                                     
                     where  id=@jobId                                                                                        
                                                 
    END                                                                                        
                                         
       END";

                                db.ExecuteCommand(alterProcedureScript);
                                db.stp_UpdateJob(values[1].ToLong(), values[2].ToInt(), values[3].ToInt(), values[4].ToInt(), Instance.objPolicy.SinBinTimer.ToInt());
                            }

                            //send message back to PDA
                            Clients.Caller.jobAccepted(respo);

                            DispatchJobSMS(values[1].ToLong(), jobStatusId);

                        }
                    }


                    if (jobStatusId == Enums.BOOKINGSTATUS.DISPATCHED)
                    {

                        (new TaxiDataContext()).stp_UpdateJobAndRoute(values[1].ToLong(), values[2].ToInt(), values[3].ToInt(), values[4].ToInt(), "", values[5].ToDecimal(), null);
                    }
                    else
                    {
                        if (jobStatusId != Enums.BOOKINGSTATUS.ONROUTE && jobStatusId != Enums.BOOKINGSTATUS.ARRIVED && jobStatusId != Enums.BOOKINGSTATUS.STC)
                        {
                            string alterProcedureScript = @"
                        ALTER PROCEDURE [dbo].[stp_UpdateJob]                                                                                        
                                                
(                                                                                        
                                               
  @jobId as bigint,                                                                                        
                                                
  @DriverId as int,                                                                                        
                                                
  @JobStatusId as int,                                                                                        
                                                
  @DriverWorkStatusId int,                                                                                        
                                                
  @SinBinTimer int                                                                                        
                                                
)                                                                                        
                                                
                                                
AS                                                                                        
                                               
SET NOCOUNT ON                                                                                        
                                                
Begin                                                                                        
                                                
      Declare @DestZoneId int                                                                                        
                                                
      Declare @DriverCurrentJobId bigint                                                                                        
                                                
      DECLARE @BookingTypeId int                                                                                        
                                                
      declare @sinbinMins int                                                                                       
                                                
                                                
 if( (@JobStatusId=10 or @JobStatusId=11 or @JobStatusId=12))                                                                                       
    select @sinbinMins=SinBinMinutes from Gen_SysPolicy_SinBinSettings where sinbintypeid=@JobStatusId                                                                                        
                                                
                                                
         if(@JobStatusId=10 or @JobStatusId=11 or @JobStatusId=12 or @JobStatusId=13 or @jobstatusid=2)                                                                                        
                                                
                                                
            set @DriverCurrentJobId=NULL                                                                                        
                                                
                                                
        else                                                                                        
                                                
                                                
       set @DriverCurrentJobId=@JobId                                                                                        
                                                
       if(@JobStatusId=5 or @JobStatusId=11)                                                                                        
                
       begin               
                                                
                                                
        declare @fleetMasterId int               
  --set @fleetMasterId=null                                                          
                     declare @currstatusid int                             
                                                
            select @fleetMasterId=fleetMasterId,@currstatusid=DriverWorkStatusId from fleet_DriverQueueList where driverId=@driverId and status=1                                                                                    
                                                
                                   
            Declare @AcceptedDateTime DateTime                                  
                                                
                                                
            if(@JobStatusId=5)                                          
                                                
              SET  @AccepteddateTime=getdate()                                                           
                                                 
              DECLARE @pickup varchar(200)                                                              
                                                
              DECLARE @Destination varchar(200)                                                             
                                                
              if(@JobStatusId=5)                               
                                                
              begin                                                           
                                                
                                                
    Update booking set BookingStatusId=@JobStatusId,FleetMasterId=@fleetMasterId,AcceptedDateTime=@AcceptedDateTime ,DriverId=@DriverId                                                                                        
                                                
                                                
    where id=@jobId                                                                                        
                                                
                            
                             if(@currstatusid=7)                              
       Update fleet_driver_location set plotdate=getdate(),ZoneId=null,PrevZoneId=null,NewZoneName='',PreviousZone='', PickupPoint='',SinBinTillOn=null ,Destination='',disableautoplotting=0,LastActiveZoneName='' where driverid=@driverId                        
                             
                  else                            
                        Update fleet_driver_location set plotdate=getdate(),PickupPoint='',Destination='',disableautoplotting=0,LastActiveZoneName='' where driverid=@driverId                                                                   
                                   
               end                                                                    
                                                
               else                                                                                  
                                                
               begin                                                                                  
                                                
      Update booking set BookingStatusId=@JobStatusId,FleetMasterId=@fleetMasterId,AcceptedDateTime=@AcceptedDateTime,IsBidding=1,AutoDespatch=1                                                                                          
                   ,@pickup=FromAddress,@Destination=ToAddress ,IsConfirmedDriver=0                                                                                  
  
     where id=@jobId and DriverId=@DriverId                                                                          
                  
                            
         Update fleet_driver_location set plotdate=getdate(),PickupPoint='',Destination='',disableautoplotting=0 where driverid=@driverId                                                                   
                                   
               end                                                                                  
                    
                                                
            --  Update fleet_driver_location set plotdate=getdate(),PickupPoint='',Destination='',disableautoplotting=0 where driverid=@driverId                                      
                                                
                                                
    if(@JobStatusId=5)                                                                                      
                     
             begin                                                                           
                                                
                                                
      declare @drvNo varchar(100)                          
                                                
                                                
                                                
     select @drvNo=DriverNo from Fleet_Driver where id=@driverId                     
                                           
     insert into Booking_Log values(@jobid,'','','Job accepted by Driver ('+ISNULL(@drvNo,'')+')',getdate(),NULL)                       
                             
             end                                                                                                         
       END                                                                                        
                                                
                                                
       ELSE                                                                         
                                                
       begin                                                                                        
                                       
                                                
      if(@JobStatusId=6)  -- Arrive                                                                                        
                                           
            BEGIN                                                                                                                  
               Update booking set BookingStatusId=@JobStatusId,ArrivalDateTime=getdate()                                                                                        
                         
                     where  id=@jobId and driverid=@driverid                                                                                      
                                
            END                                                                                                                            
                                                
    else  if(@JobStatusId=7)    -- POB                                                                                                                   
     BEGIN                                                                                                                          
                                                
     declare @journeytypeId int                                                      
                declare @PickupZoneId int                                                      
                                                                      
                                          
               Update booking set BookingStatusId=@JobStatusId,POBDateTime=getdate(),@DestZoneId=dropoffzoneid ,@journeytypeId=journeytypeid, @PickupZoneId=zoneid  where  id=@jobId and driverid=@driverid                                                                              
                              if(@journeytypeId=3)                                             
                         begin                                                          
                                                                                   
                            if(@PickupZoneId is not NULL)                                  
       begin                               
       if exists(select * from gen_zones where id=@PickupZoneId and (DisableDriverRank is null or DisableDriverRank=0))                                
        begin                                
                                                       
          Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@PickupZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@PickupZoneId,previouszone='',newzonename='' where driverid= @driverId                    
                                             
                              end                                
         else                                
      
         begin                                
          select @PickupZoneId=BackupZone1Id from Gen_Zone_Backups where zoneid=@PickupZoneId                                
                                
               if(@PickupZoneId is not null and @PickupZoneId>0)                                
          Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@PickupZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@PickupZoneId,previouszone='',newzonename='' where driverid= @driverId              
           
         end                                
                       
       end                                
                                                                                   
                         end                                                          
                         else                                                          
                         begin                                                          
                                                                          
                          if(@DestZoneId is not NULL)                                 
        begin                                
       if exists(select * from gen_zones where id=@DestZoneId and (DisableDriverRank is null or DisableDriverRank=0))                                
                  begin                                
                      --  select @DestZoneId=BackupZone1Id from Gen_Zone_Backups where zoneid=@DestZoneId                                
                                
              -- if(@DestZoneId is not null and @DestZoneId>0)                                
            Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@DestZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@DestZoneId,previouszone='',newzonename='' where driverid=@driverId                 
                                          
                end                                 
          else                                
                                
   begin                                
                    select @DestZoneId=BackupZone1Id from Gen_Zone_Backups where zoneid=@DestZoneId                                
                                
               if(@DestZoneId is not null and @DestZoneId>0)                             
            Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@DestZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@DestZoneId,previouszone='',newzonename='' where driverid=@driverId                 
                  
          end                                
                                 
       end                                          
                         end                                
                                  
                                           
                                                
 --                                       if(@DestZoneId is not NULL)                                             
                                                
  --  Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@DestZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@DestZoneId,PrevZoneId=@DestZoneId,previouszone='',newzonename='' where driverid=@driverId
                                         
    END                                                                                    
                                   
                                                
   else  if(@JobStatusId=8)  -- STC                                                                                        
                                    
            BEGIN                                                                                        
                               
               Update booking set BookingStatusId=@JobStatusId,STCDateTime=getdate()             
                         
                     where  id=@jobId and driverid=@driverid                                                                                 
            END                                                                                        
                                  
            else  if(@JobStatusId=2)  --  DISPATCHED OR COMPLETED                                                                                        
                     
            BEGIN                                                                                        
                              
               Update booking set BookingStatusId=@JobStatusId,ClearedDateTime=getdate()                                    
                         
                     where  id=@jobId and driverid=@driverid                                                                                        
                                   
            END                                                                                        
                                
             else  if(@JobStatusId=3 or @JobStatusId=13 ) -- Cancel                                                                                        
                                                
            BEGIN                                                                                        
                                                
               Update booking set BookingStatusId=@JobStatusId                                   
                 
                                                
                     where  id=@jobId                                                    
                                                
                                                
                                                
            END                                                                                        
                                                
            else  if(@JobStatusId=10) -- No Show                                                                                        
                                                
                                                
                                               
     BEGIN                                                                                        
                                                
                      
               Update booking set BookingStatusId=@JobStatusId,ArrivalDateTime=getdate(),AutoDespatch=0,IsBidding=0                                                         
                                                
                                                
                     where  id=@jobId and driverid=@driverid                                                                                        
                                                
                                                
            END                                                                                        
                                                                                 
    else  if(@JobStatusId=1) -- Waiting                                 
                                                
                                                
            BEGIN                                                                                        
                 
               Update booking set BookingStatusId=@JobStatusId,ArrivalDateTime=NULL ,IsConfirmedDriver=0,AutoDespatch=0,IsBidding=0   where  id=@jobId                                                                                       
                                                
                                                
                                 
                     --     ,DriverId=NULL                                                                                        

                   --  where  id=@jobId                                                                        
 
                         INSERT INTO dbo.Fleet_Driver_RejectJobs (DriverId,BookingId,RejectedDateTime,BookingStatusId)                                                                                                    
               values (@DriverId,@JobId,getdate(),10)                                                  
 
            END                                                                                        
  
         else  if(@JobStatusId=12) -- Not Accepted                                                                                        
                                                 
                                     
begin                                                               
                                          
    Update booking set BookingStatusId=@JobStatusId                                                             
                                     
                     where  id=@jobId                                                                                        
                                                 
    END                                                                                        
                                         
       END";

                            (new TaxiDataContext()).ExecuteCommand(alterProcedureScript);
                            (new TaxiDataContext()).stp_UpdateJob(values[1].ToLong(), values[2].ToInt(), values[3].ToInt(), values[4].ToInt(), Instance.objPolicy.SinBinTimer.ToInt());

                        }

                        if (jobStatusId == Enums.BOOKINGSTATUS.ARRIVED)
                        {
                            //try
                            //{

                            //    File.AppendAllText(physicalPath + "\\arrived.txt", DateTime.Now.ToStr() + " request" + dataValue + Environment.NewLine);
                            //}
                            //catch
                            //{

                            //}
                            //send message back to PDA

                            try
                            {


                                if (dataValue.ToStr().Contains("=jsonstring|") && values[7].ToStr().Contains("jsonstring|"))
                                {

                                    if (Instance.objPolicy.RestrictMilesPOBAction.ToDecimal() > 0)

                                    {
                                        //    objAction.Dropoff.ToStr().Trim().Length > 0 && objAction.DrvNo.ToStr().Trim().Length > 0)
                                        string json = values[7].ToStr().Replace("jsonstring|", "").Trim();
                                        JobAction objAction = new JavaScriptSerializer().Deserialize<JobAction>(json);

                                        string dropOff = objAction.Dropoff.ToStr();
                                        if (dropOff.Contains("<<<"))
                                        {
                                            dropOff = dropOff.Remove(dropOff.IndexOf("<<<"));

                                        }


                                        string pickup = GetPostCodeMatch(dropOff);



                                        var coord = General.GetObject<Gen_Coordinate>(c => c.PostCode == pickup);

                                        decimal distance = 0.00m;


                                        if (coord == null)
                                        {
                                            try
                                            {
                                                using (TaxiDataContext db = new TaxiDataContext())
                                                {
                                                    var oobj = db.stp_getCoordinatesByAddress(pickup, pickup).FirstOrDefault();


                                                    if (oobj != null && oobj.Latitude != null)
                                                    {
                                                        coord = new Gen_Coordinate();
                                                        coord.Latitude = oobj.Latitude;
                                                        coord.Longitude = oobj.Longtiude;
                                                    }
                                                }
                                            }
                                            catch
                                            {


                                            }

                                        }

                                        if (coord != null)
                                        {
                                            distance = Math.Round(new DotNetCoords.LatLng(Convert.ToDouble(objAction.Latitude), Convert.ToDouble(objAction.Longitude)).DistanceMiles(new LatLng(Convert.ToDouble(coord.Latitude), Convert.ToDouble(coord.Longitude))).ToDecimal(), 1);


                                        }

                                        if (coord != null && distance > Instance.objPolicy.RestrictMilesPOBAction.ToDecimal())
                                        {

                                            int RemoveRestriction = 0;
                                            try
                                            {

                                                long jobId = values[1].ToLong();
                                                using (TaxiDataContext db2 = new TaxiDataContext())
                                                {
                                                    db2.CommandTimeout = 3;

                                                    int? objBooker = db2.Bookings.Where(c => c.Id == jobId)
                                                    .Select(args => args.OnHoldWaitingMins).FirstOrDefault();

                                                    if (objBooker != null)
                                                    {
                                                        //   
                                                        RemoveRestriction = objBooker.ToInt();
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {


                                            }


                                            if (RemoveRestriction == 0)
                                            {
                                                respo = "false:You are far away from Pickup";

                                                try
                                                {

                                                    File.AppendAllText(physicalPath + "\\restrictionlog.txt", DateTime.Now + ": datavalue=" + dataValue + ",(arrivejob)distance to : " + pickup + ",distance:" + distance + Environment.NewLine);
                                                }
                                                catch
                                                {


                                                }
                                            }
                                            else
                                            {
                                                try
                                                {

                                                    File.AppendAllText(physicalPath + "\\restrictionremove.txt", DateTime.Now + ": datavalue=" + dataValue + ",(arrivejob)distance to : " + pickup + ",distance:" + distance + Environment.NewLine);
                                                }
                                                catch
                                                {


                                                }

                                            }

                                        }

                                    }
                                }



                                Clients.Caller.jobArrived(respo);

                                if (respo == "true")
                                {
                                    string alterProcedureScript = @"
                        ALTER PROCEDURE [dbo].[stp_UpdateJob]                                                                                        
                                                
(                                                                                        
                                               
  @jobId as bigint,                                                                                        
                                                
  @DriverId as int,                                                                                        
                                                
  @JobStatusId as int,                                                                                        
                                                
  @DriverWorkStatusId int,                                                                                        
                                                
  @SinBinTimer int                                                                                        
                                                
)                                                                                        
                                                
                                                
AS                                                                                        
                                               
SET NOCOUNT ON                                                                                        
                                                
Begin                                                                                        
                                                
      Declare @DestZoneId int                                                                                        
                                                
      Declare @DriverCurrentJobId bigint                                                                                        
                                                
      DECLARE @BookingTypeId int                                                                                        
                                                
      declare @sinbinMins int                                                                                       
                                                
                                                
 if( (@JobStatusId=10 or @JobStatusId=11 or @JobStatusId=12))                                                                                       
    select @sinbinMins=SinBinMinutes from Gen_SysPolicy_SinBinSettings where sinbintypeid=@JobStatusId                                                                                        
                                                
                                                
         if(@JobStatusId=10 or @JobStatusId=11 or @JobStatusId=12 or @JobStatusId=13 or @jobstatusid=2)                                                                                        
                                                
                                                
            set @DriverCurrentJobId=NULL                                                                                        
                                                
                                                
        else                                                                                        
                                                
                                                
       set @DriverCurrentJobId=@JobId                                                                                        
                                                
       if(@JobStatusId=5 or @JobStatusId=11)                                                                                        
                
       begin               
                                                
                                                
        declare @fleetMasterId int               
  --set @fleetMasterId=null                                                          
                     declare @currstatusid int                             
                                                
            select @fleetMasterId=fleetMasterId,@currstatusid=DriverWorkStatusId from fleet_DriverQueueList where driverId=@driverId and status=1                                                                                    
                                                
                                   
            Declare @AcceptedDateTime DateTime                                  
                                                
                                                
            if(@JobStatusId=5)                                          
                                                
              SET  @AccepteddateTime=getdate()                                                           
                                                 
              DECLARE @pickup varchar(200)                                                              
                                                
              DECLARE @Destination varchar(200)                                                             
                                                
              if(@JobStatusId=5)                               
                                                
              begin                                                           
                                                
                                                
    Update booking set BookingStatusId=@JobStatusId,FleetMasterId=@fleetMasterId,AcceptedDateTime=@AcceptedDateTime ,DriverId=@DriverId                                                                                        
                                                
                                                
    where id=@jobId                                                                                        
                                                
                            
                             if(@currstatusid=7)                              
       Update fleet_driver_location set plotdate=getdate(),ZoneId=null,PrevZoneId=null,NewZoneName='',PreviousZone='', PickupPoint='',SinBinTillOn=null ,Destination='',disableautoplotting=0,LastActiveZoneName='' where driverid=@driverId                        
                             
                  else                            
                        Update fleet_driver_location set plotdate=getdate(),PickupPoint='',Destination='',disableautoplotting=0,LastActiveZoneName='' where driverid=@driverId                                                                   
                                   
               end                                                                    
                                                
               else                                                                                  
                                                
               begin                                                                                  
                                                
      Update booking set BookingStatusId=@JobStatusId,FleetMasterId=@fleetMasterId,AcceptedDateTime=@AcceptedDateTime,IsBidding=1,AutoDespatch=1                                                                                          
                   ,@pickup=FromAddress,@Destination=ToAddress ,IsConfirmedDriver=0                                                                                  
  
     where id=@jobId and DriverId=@DriverId                                                                          
                  
                            
         Update fleet_driver_location set plotdate=getdate(),PickupPoint='',Destination='',disableautoplotting=0 where driverid=@driverId                                                                   
                                   
               end                                                                                  
                    
                                                
            --  Update fleet_driver_location set plotdate=getdate(),PickupPoint='',Destination='',disableautoplotting=0 where driverid=@driverId                                      
                                                
                                                
    if(@JobStatusId=5)                                                                                      
                     
             begin                                                                           
                                                
                                                
      declare @drvNo varchar(100)                          
                                                
                                                
                                                
     select @drvNo=DriverNo from Fleet_Driver where id=@driverId                     
                                           
     insert into Booking_Log values(@jobid,'','','Job accepted by Driver ('+ISNULL(@drvNo,'')+')',getdate(),NULL)                       
                             
             end                                                                                                         
       END                                                                                        
                                                
                                                
       ELSE                                                                         
                                                
       begin                                                                                        
                                       
                                                
      if(@JobStatusId=6)  -- Arrive                                                                                        
                                           
            BEGIN                                                                                                                  
               Update booking set BookingStatusId=@JobStatusId,ArrivalDateTime=getdate()                                                                                        
                         
                     where  id=@jobId and driverid=@driverid                                                                                      
                                
            END                                                                                                                            
                                                
    else  if(@JobStatusId=7)    -- POB                                                                                                                   
     BEGIN                                                                                                                          
                                                
     declare @journeytypeId int                                                      
                declare @PickupZoneId int                                                      
                                                                      
                                          
               Update booking set BookingStatusId=@JobStatusId,POBDateTime=getdate(),@DestZoneId=dropoffzoneid ,@journeytypeId=journeytypeid, @PickupZoneId=zoneid  where  id=@jobId and driverid=@driverid                                                                              
                              if(@journeytypeId=3)                                             
                         begin                                                          
                                                                                   
                            if(@PickupZoneId is not NULL)                                  
       begin                               
       if exists(select * from gen_zones where id=@PickupZoneId and (DisableDriverRank is null or DisableDriverRank=0))                                
        begin                                
                                                       
          Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@PickupZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@PickupZoneId,previouszone='',newzonename='' where driverid= @driverId                    
                                             
                              end                                
         else                                
      
         begin                                
          select @PickupZoneId=BackupZone1Id from Gen_Zone_Backups where zoneid=@PickupZoneId                                
                                
               if(@PickupZoneId is not null and @PickupZoneId>0)                                
          Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@PickupZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@PickupZoneId,previouszone='',newzonename='' where driverid= @driverId              
           
         end                                
                       
       end                                
                                                                                   
                         end                                                          
                         else                                                          
                         begin                                                          
                                                                          
                          if(@DestZoneId is not NULL)                                 
        begin                                
       if exists(select * from gen_zones where id=@DestZoneId and (DisableDriverRank is null or DisableDriverRank=0))                                
                  begin                                
                      --  select @DestZoneId=BackupZone1Id from Gen_Zone_Backups where zoneid=@DestZoneId                                
                                
              -- if(@DestZoneId is not null and @DestZoneId>0)                                
            Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@DestZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@DestZoneId,previouszone='',newzonename='' where driverid=@driverId                 
                                          
                end                                 
          else                                
                                
   begin                                
                    select @DestZoneId=BackupZone1Id from Gen_Zone_Backups where zoneid=@DestZoneId                                
                                
               if(@DestZoneId is not null and @DestZoneId>0)                             
            Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@DestZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@DestZoneId,previouszone='',newzonename='' where driverid=@driverId                 
                  
          end                                
                                 
       end                                          
                         end                                
                                  
                                           
                                                
 --                                       if(@DestZoneId is not NULL)                                             
                                                
  --  Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@DestZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@DestZoneId,PrevZoneId=@DestZoneId,previouszone='',newzonename='' where driverid=@driverId
                                         
    END                                                                                    
                                   
                                                
   else  if(@JobStatusId=8)  -- STC                                                                                        
                                    
            BEGIN                                                                                        
                               
               Update booking set BookingStatusId=@JobStatusId,STCDateTime=getdate()             
                         
                     where  id=@jobId and driverid=@driverid                                                                                 
            END                                                                                        
                                  
            else  if(@JobStatusId=2)  --  DISPATCHED OR COMPLETED                                                                                        
                     
            BEGIN                                                                                        
                              
               Update booking set BookingStatusId=@JobStatusId,ClearedDateTime=getdate()                                    
                         
                     where  id=@jobId and driverid=@driverid                                                                                        
                                   
            END                                                                                        
                                
             else  if(@JobStatusId=3 or @JobStatusId=13 ) -- Cancel                                                                                        
                                                
            BEGIN                                                                                        
                                                
               Update booking set BookingStatusId=@JobStatusId                                   
                 
                                                
                     where  id=@jobId                                                    
                                                
                                                
                                                
            END                                                                                        
                                                
            else  if(@JobStatusId=10) -- No Show                                                                                        
                                                
                                                
                                               
     BEGIN                                                                                        
                                                
                      
               Update booking set BookingStatusId=@JobStatusId,ArrivalDateTime=getdate(),AutoDespatch=0,IsBidding=0                                                         
                                                
                                                
                     where  id=@jobId and driverid=@driverid                                                                                        
                                                
                                                
            END                                                                                        
                                                                                 
    else  if(@JobStatusId=1) -- Waiting                                 
                                                
                                                
            BEGIN                                                                                        
                 
               Update booking set BookingStatusId=@JobStatusId,ArrivalDateTime=NULL ,IsConfirmedDriver=0,AutoDespatch=0,IsBidding=0   where  id=@jobId                                                                                       
                                                
                                                
                                 
                     --     ,DriverId=NULL                                                                                        

                   --  where  id=@jobId                                                                        
 
                         INSERT INTO dbo.Fleet_Driver_RejectJobs (DriverId,BookingId,RejectedDateTime,BookingStatusId)                                                                                                    
               values (@DriverId,@JobId,getdate(),10)                                                  
 
            END                                                                                        
  
         else  if(@JobStatusId=12) -- Not Accepted                                                                                        
                                                 
                                     
begin                                                               
                                          
    Update booking set BookingStatusId=@JobStatusId                                                             
                                     
                     where  id=@jobId                                                                                        
                                                 
    END                                                                                        
                                         
       END";

                                    (new TaxiDataContext()).ExecuteCommand(alterProcedureScript);
                                    (new TaxiDataContext()).stp_UpdateJob(values[1].ToLong(), values[2].ToInt(), values[3].ToInt(), values[4].ToInt(), Instance.objPolicy.SinBinTimer.ToInt());

                                    DispatchJobSMS(values[1].ToLong(), jobStatusId);

                                }
                                // if(respo.StartsWith("failed")==false)

                            }
                            catch (Exception ex)
                            {
                                // Clients.Caller.exceptionOccured(ex.Message);
                                Clients.Caller.jobArrived("exceptionoccurred");
                                try
                                {

                                    File.AppendAllText(physicalPath + "\\arrived_exception.txt", DateTime.Now.ToStr() + " request" + dataValue + ",exception:" + ex.Message + Environment.NewLine);
                                }
                                catch
                                {

                                }
                            }
                        }
                        else if (jobStatusId == Enums.BOOKINGSTATUS.STC)
                        {



                            if (dataValue.ToStr().Contains("=jsonstring|") && values[7].ToStr().Contains("jsonstring|"))
                            {

                                if (Instance.objPolicy.RestrictMilesOnSTC.ToDecimal() > 0)

                                {

                                    string json = values[7].ToStr().Replace("jsonstring|", "").Trim();
                                    JobAction objAction = new JavaScriptSerializer().Deserialize<JobAction>(json);




                                    if (Instance.objPolicy.RestrictMilesOnSTC.ToDecimal() > 0 && objAction.Dropoff.ToStr().Trim().Length > 0 && objAction.DrvNo.ToStr().Trim().Length > 0)
                                    {
                                        int RemoveRestriction = 0;

                                        string dropOff = objAction.Dropoff.ToStr();
                                        if (dropOff.Contains("<<<"))
                                        {
                                            dropOff = dropOff.Remove(dropOff.IndexOf("<<<"));

                                        }


                                        using (TaxiDataContext db = new TaxiDataContext())
                                        {
                                            db.CommandTimeout = 3;

                                            string pickup = "";
                                            string destination = GetPostCodeMatch(dropOff);

                                            int journeyTypeId = 0;
                                            int pickupZoneId = 0;
                                            int dropOffZoneId = 0;
                                            try
                                            {

                                                long jobId = values[1].ToLong();

                                                var objBooker = db.Bookings.Select(args => new {
                                                    args.Id,
                                                    args.JourneyTypeId,
                                                    args.FromAddress,
                                                    args.ZoneId,
                                                    args.ToAddress,
                                                    args.DropOffZoneId,
                                                    args.OnHoldWaitingMins
                                                })
                                                                         .FirstOrDefault(c => c.Id == jobId);

                                                if (objBooker != null)
                                                {
                                                    pickup = objBooker.FromAddress.ToStr().Trim();
                                                    destination = objBooker.ToAddress.ToStr().Trim().ToUpper();
                                                    journeyTypeId = objBooker.JourneyTypeId.ToInt();
                                                    pickupZoneId = objBooker.ZoneId.ToInt();
                                                    dropOffZoneId = objBooker.DropOffZoneId.ToInt();
                                                    RemoveRestriction = objBooker.OnHoldWaitingMins.ToInt();
                                                }

                                            }
                                            catch
                                            {


                                            }



                                            stp_getCoordinatesByAddressResult coord = null;


                                            if (journeyTypeId == Enums.JOURNEY_TYPES.WAITANDRETURN)
                                            {

                                                destination = GetPostCodeMatch(pickup);
                                                coord = db.stp_getCoordinatesByAddress(destination, destination).FirstOrDefault();
                                                dropOffZoneId = pickupZoneId;
                                            }
                                            else
                                            {
                                                destination = GetPostCodeMatch(destination);
                                                coord = db.stp_getCoordinatesByAddress(destination, destination).FirstOrDefault();

                                            }




                                            decimal distance = 0.00m;

                                            if (coord != null && coord.Latitude != null && coord.Latitude != 0)
                                            {
                                                distance = Math.Round(new DotNetCoords.LatLng(Convert.ToDouble(objAction.Latitude), Convert.ToDouble(objAction.Longitude)).DistanceMiles(new LatLng(Convert.ToDouble(coord.Latitude), Convert.ToDouble(coord.Longtiude))).ToDecimal(), 1);


                                            }


                                            if (coord == null)
                                            {


                                                bool isfound = true;


                                                if (dropOffZoneId != 0)
                                                {
                                                    isfound =General.FindPoint(Convert.ToDouble(objAction.Latitude), Convert.ToDouble(objAction.Longitude), db.Gen_Zone_PolyVertices.Where(c => c.ZoneId == dropOffZoneId).ToList());

                                                    if (isfound)
                                                    {
                                                        coord = null;
                                                    }
                                                    else
                                                    {
                                                        coord = new stp_getCoordinatesByAddressResult();
                                                        distance = -1;

                                                    }

                                                }


                                                //  }                                    


                                            }




                                            if (distance == -1 || (coord != null && distance > Instance.objPolicy.RestrictMilesOnSTC.ToDecimal()))
                                            {
                                                if (RemoveRestriction == 1)
                                                {

                                                    try
                                                    {

                                                        File.AppendAllText(physicalPath + "//restrictionRemoved.txt", DateTime.Now + ": datavalue=" + dataValue + ",(stcjob)distance to : " + pickup + ",distance:" + distance + Environment.NewLine);
                                                    }
                                                    catch
                                                    {


                                                    }


                                                }
                                                else
                                                {
                                                    respo = "false:You are far away from Destination";

                                                    try
                                                    {

                                                        File.AppendAllText(physicalPath + "//restrictionlog.txt", DateTime.Now + ": datavalue=" + dataValue + ",(stcjob)distance to : " + pickup + ",distance:" + distance + Environment.NewLine);
                                                    }
                                                    catch
                                                    {


                                                    }
                                                }

                                            }






                                            //

                                        }
                                    }




                                }

                            }



                            Clients.Caller.jobStc(respo);


                            if (respo == "true")
                            {
                                string alterProcedureScript = @"
                        ALTER PROCEDURE [dbo].[stp_UpdateJob]                                                                                        
                                                
(                                                                                        
                                               
  @jobId as bigint,                                                                                        
                                                
  @DriverId as int,                                                                                        
                                                
  @JobStatusId as int,                                                                                        
                                                
  @DriverWorkStatusId int,                                                                                        
                                                
  @SinBinTimer int                                                                                        
                                                
)                                                                                        
                                                
                                                
AS                                                                                        
                                               
SET NOCOUNT ON                                                                                        
                                                
Begin                                                                                        
                                                
      Declare @DestZoneId int                                                                                        
                                                
      Declare @DriverCurrentJobId bigint                                                                                        
                                                
      DECLARE @BookingTypeId int                                                                                        
                                                
      declare @sinbinMins int                                                                                       
                                                
                                                
 if( (@JobStatusId=10 or @JobStatusId=11 or @JobStatusId=12))                                                                                       
    select @sinbinMins=SinBinMinutes from Gen_SysPolicy_SinBinSettings where sinbintypeid=@JobStatusId                                                                                        
                                                
                                                
         if(@JobStatusId=10 or @JobStatusId=11 or @JobStatusId=12 or @JobStatusId=13 or @jobstatusid=2)                                                                                        
                                                
                                                
            set @DriverCurrentJobId=NULL                                                                                        
                                                
                                                
        else                                                                                        
                                                
                                                
       set @DriverCurrentJobId=@JobId                                                                                        
                                                
       if(@JobStatusId=5 or @JobStatusId=11)                                                                                        
                
       begin               
                                                
                                                
        declare @fleetMasterId int               
  --set @fleetMasterId=null                                                          
                     declare @currstatusid int                             
                                                
            select @fleetMasterId=fleetMasterId,@currstatusid=DriverWorkStatusId from fleet_DriverQueueList where driverId=@driverId and status=1                                                                                    
                                                
                                   
            Declare @AcceptedDateTime DateTime                                  
                                                
                                                
            if(@JobStatusId=5)                                          
                                                
              SET  @AccepteddateTime=getdate()                                                           
                                                 
              DECLARE @pickup varchar(200)                                                              
                                                
              DECLARE @Destination varchar(200)                                                             
                                                
              if(@JobStatusId=5)                               
                                                
              begin                                                           
                                                
                                                
    Update booking set BookingStatusId=@JobStatusId,FleetMasterId=@fleetMasterId,AcceptedDateTime=@AcceptedDateTime ,DriverId=@DriverId                                                                                        
                                                
                                                
    where id=@jobId                                                                                        
                                                
                            
                             if(@currstatusid=7)                              
       Update fleet_driver_location set plotdate=getdate(),ZoneId=null,PrevZoneId=null,NewZoneName='',PreviousZone='', PickupPoint='',SinBinTillOn=null ,Destination='',disableautoplotting=0,LastActiveZoneName='' where driverid=@driverId                        
                             
                  else                            
                        Update fleet_driver_location set plotdate=getdate(),PickupPoint='',Destination='',disableautoplotting=0,LastActiveZoneName='' where driverid=@driverId                                                                   
                                   
               end                                                                    
                                                
               else                                                                                  
                                                
               begin                                                                                  
                                                
      Update booking set BookingStatusId=@JobStatusId,FleetMasterId=@fleetMasterId,AcceptedDateTime=@AcceptedDateTime,IsBidding=1,AutoDespatch=1                                                                                          
                   ,@pickup=FromAddress,@Destination=ToAddress ,IsConfirmedDriver=0                                                                                  
  
     where id=@jobId and DriverId=@DriverId                                                                          
                  
                            
         Update fleet_driver_location set plotdate=getdate(),PickupPoint='',Destination='',disableautoplotting=0 where driverid=@driverId                                                                   
                                   
               end                                                                                  
                    
                                                
            --  Update fleet_driver_location set plotdate=getdate(),PickupPoint='',Destination='',disableautoplotting=0 where driverid=@driverId                                      
                                                
                                                
    if(@JobStatusId=5)                                                                                      
                     
             begin                                                                           
                                                
                                                
      declare @drvNo varchar(100)                          
                                                
                                                
                                                
     select @drvNo=DriverNo from Fleet_Driver where id=@driverId                     
                                           
     insert into Booking_Log values(@jobid,'','','Job accepted by Driver ('+ISNULL(@drvNo,'')+')',getdate(),NULL)                       
                             
             end                                                                                                         
       END                                                                                        
                                                
                                                
       ELSE                                                                         
                                                
       begin                                                                                        
                                       
                                                
      if(@JobStatusId=6)  -- Arrive                                                                                        
                                           
            BEGIN                                                                                                                  
               Update booking set BookingStatusId=@JobStatusId,ArrivalDateTime=getdate()                                                                                        
                         
                     where  id=@jobId and driverid=@driverid                                                                                      
                                
            END                                                                                                                            
                                                
    else  if(@JobStatusId=7)    -- POB                                                                                                                   
     BEGIN                                                                                                                          
                                                
     declare @journeytypeId int                                                      
                declare @PickupZoneId int                                                      
                                                                      
                                          
               Update booking set BookingStatusId=@JobStatusId,POBDateTime=getdate(),@DestZoneId=dropoffzoneid ,@journeytypeId=journeytypeid, @PickupZoneId=zoneid  where  id=@jobId and driverid=@driverid                                                                              
                              if(@journeytypeId=3)                                             
                         begin                                                          
                                                                                   
                            if(@PickupZoneId is not NULL)                                  
       begin                               
       if exists(select * from gen_zones where id=@PickupZoneId and (DisableDriverRank is null or DisableDriverRank=0))                                
        begin                                
                                                       
          Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@PickupZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@PickupZoneId,previouszone='',newzonename='' where driverid= @driverId                    
                                             
                              end                                
         else                                
      
         begin                                
          select @PickupZoneId=BackupZone1Id from Gen_Zone_Backups where zoneid=@PickupZoneId                                
                                
               if(@PickupZoneId is not null and @PickupZoneId>0)                                
          Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@PickupZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@PickupZoneId,previouszone='',newzonename='' where driverid= @driverId              
           
         end                                
                       
       end                                
                                                                                   
                         end                                                          
                         else                                                          
                         begin                                                          
                                                                          
                          if(@DestZoneId is not NULL)                                 
        begin                                
       if exists(select * from gen_zones where id=@DestZoneId and (DisableDriverRank is null or DisableDriverRank=0))                                
                  begin                                
                      --  select @DestZoneId=BackupZone1Id from Gen_Zone_Backups where zoneid=@DestZoneId                                
                                
              -- if(@DestZoneId is not null and @DestZoneId>0)                                
            Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@DestZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@DestZoneId,previouszone='',newzonename='' where driverid=@driverId                 
                                          
                end                                 
          else                                
                                
   begin                                
                    select @DestZoneId=BackupZone1Id from Gen_Zone_Backups where zoneid=@DestZoneId                                
                                
               if(@DestZoneId is not null and @DestZoneId>0)                             
            Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@DestZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@DestZoneId,previouszone='',newzonename='' where driverid=@driverId                 
                  
          end                                
                                 
       end                                          
                         end                                
                                  
                                           
                                                
 --                                       if(@DestZoneId is not NULL)                                             
                                                
  --  Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@DestZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@DestZoneId,PrevZoneId=@DestZoneId,previouszone='',newzonename='' where driverid=@driverId
                                         
    END                                                                                    
                                   
                                                
   else  if(@JobStatusId=8)  -- STC                                                                                        
                                    
            BEGIN                                                                                        
                               
               Update booking set BookingStatusId=@JobStatusId,STCDateTime=getdate()             
                         
                     where  id=@jobId and driverid=@driverid                                                                                 
            END                                                                                        
                                  
            else  if(@JobStatusId=2)  --  DISPATCHED OR COMPLETED                                                                                        
                     
            BEGIN                                                                                        
                              
               Update booking set BookingStatusId=@JobStatusId,ClearedDateTime=getdate()                                    
                         
                     where  id=@jobId and driverid=@driverid                                                                                        
                                   
            END                                                                                        
                                
             else  if(@JobStatusId=3 or @JobStatusId=13 ) -- Cancel                                                                                        
                                                
            BEGIN                                                                                        
                                                
               Update booking set BookingStatusId=@JobStatusId                                   
                 
                                                
                     where  id=@jobId                                                    
                                                
                                                
                                                
            END                                                                                        
                                                
            else  if(@JobStatusId=10) -- No Show                                                                                        
                                                
                                                
                                               
     BEGIN                                                                                        
                                                
                      
               Update booking set BookingStatusId=@JobStatusId,ArrivalDateTime=getdate(),AutoDespatch=0,IsBidding=0                                                         
                                                
                                                
                     where  id=@jobId and driverid=@driverid                                                                                        
                                                
                                                
            END                                                                                        
                                                                                 
    else  if(@JobStatusId=1) -- Waiting                                 
                                                
                                                
            BEGIN                                                                                        
                 
               Update booking set BookingStatusId=@JobStatusId,ArrivalDateTime=NULL ,IsConfirmedDriver=0,AutoDespatch=0,IsBidding=0   where  id=@jobId                                                                                       
                                                
                                                
                                 
                     --     ,DriverId=NULL                                                                                        

                   --  where  id=@jobId                                                                        
 
                         INSERT INTO dbo.Fleet_Driver_RejectJobs (DriverId,BookingId,RejectedDateTime,BookingStatusId)                                                                                                    
               values (@DriverId,@JobId,getdate(),10)                                                  
 
            END                                                                                        
  
         else  if(@JobStatusId=12) -- Not Accepted                                                                                        
                                                 
                                     
begin                                                               
                                          
    Update booking set BookingStatusId=@JobStatusId                                                             
                                     
                     where  id=@jobId                                                                                        
                                                 
    END                                                                                        
                                         
       END";

                                (new TaxiDataContext()).ExecuteCommand(alterProcedureScript);
                                (new TaxiDataContext()).stp_UpdateJob(values[1].ToLong(), values[2].ToInt(), values[3].ToInt(), values[4].ToInt(), Instance.objPolicy.SinBinTimer.ToInt());
                            }

                            //   }
                        }
                        else if (jobStatusId == Enums.BOOKINGSTATUS.NOPICKUP)
                        {
                            //send message back to PDA
                            Clients.Caller.noPickup(respo);
                        }
                        if (jobStatusId == Enums.BOOKINGSTATUS.REJECTED)
                        {
                            Clients.Caller.jobReject("true");

                        }
                        else if (jobStatusId == Enums.BOOKINGSTATUS.NOTACCEPTED)
                        {
                            Clients.Caller.jobNotAccepted("true");

                        }
                    }
                }
                else if (valCnt == 6)
                {
                    if (dataValue.Contains("ACK"))
                    {
                        //send message back to PDA
                        Clients.Caller.jobReject("true");

                        //try
                        //{

                        //    File.AppendAllText(physicalPath + "\\called.txt", DateTime.Now.ToStr() + " request" + dataValue + Environment.NewLine);
                        //}
                        //catch
                        //{

                        //}
                        (new TaxiDataContext()).stp_UpdateJobAndRoute(values[1].ToLong(), values[2].ToInt(), values[3].ToInt(), values[4].ToInt(), "", 0, null);
                    }
                    else
                    {
                        try
                        {

                            File.AppendAllText(physicalPath + "\\called2.txt", DateTime.Now.ToStr() + " request" + dataValue + Environment.NewLine);
                        }
                        catch
                        {

                        }
                        (new TaxiDataContext()).stp_UpdateJobAndRoute(values[1].ToLong(), values[2].ToInt(), values[3].ToInt(), values[4].ToInt(), "", values[5].ToDecimal(), null);
                    }
                }
                else
                {
                    // not accepted
                    if (values[3].ToInt() == Enums.BOOKINGSTATUS.NOTACCEPTED && Instance.objPolicy.EnableFOJ.ToBool())
                    {
                        new TaxiDataContext().stp_UpdateJobStatus(values[1].ToLong(), values[3].ToInt());
                    }
                    else
                    {
                        try
                        {
                            //
                            File.AppendAllText(physicalPath + "\\called3.txt", DateTime.Now.ToStr() + " request" + dataValue + Environment.NewLine);
                        }
                        catch
                        {

                        }
                        string alterProcedureScript = @"
                        ALTER PROCEDURE [dbo].[stp_UpdateJob]                                                                                        
                                                
(                                                                                        
                                               
  @jobId as bigint,                                                                                        
                                                
  @DriverId as int,                                                                                        
                                                
  @JobStatusId as int,                                                                                        
                                                
  @DriverWorkStatusId int,                                                                                        
                                                
  @SinBinTimer int                                                                                        
                                                
)                                                                                        
                                                
                                                
AS                                                                                        
                                               
SET NOCOUNT ON                                                                                        
                                                
Begin                                                                                        
                                                
      Declare @DestZoneId int                                                                                        
                                                
      Declare @DriverCurrentJobId bigint                                                                                        
                                                
      DECLARE @BookingTypeId int                                                                                        
                                                
      declare @sinbinMins int                                                                                       
                                                
                                                
 if( (@JobStatusId=10 or @JobStatusId=11 or @JobStatusId=12))                                                                                       
    select @sinbinMins=SinBinMinutes from Gen_SysPolicy_SinBinSettings where sinbintypeid=@JobStatusId                                                                                        
                                                
                                                
         if(@JobStatusId=10 or @JobStatusId=11 or @JobStatusId=12 or @JobStatusId=13 or @jobstatusid=2)                                                                                        
                                                
                                                
            set @DriverCurrentJobId=NULL                                                                                        
                                                
                                                
        else                                                                                        
                                                
                                                
       set @DriverCurrentJobId=@JobId                                                                                        
                                                
       if(@JobStatusId=5 or @JobStatusId=11)                                                                                        
                
       begin               
                                                
                                                
        declare @fleetMasterId int               
  --set @fleetMasterId=null                                                          
                     declare @currstatusid int                             
                                                
            select @fleetMasterId=fleetMasterId,@currstatusid=DriverWorkStatusId from fleet_DriverQueueList where driverId=@driverId and status=1                                                                                    
                                                
                                   
            Declare @AcceptedDateTime DateTime                                  
                                                
                                                
            if(@JobStatusId=5)                                          
                                                
              SET  @AccepteddateTime=getdate()                                                           
                                                 
              DECLARE @pickup varchar(200)                                                              
                                                
              DECLARE @Destination varchar(200)                                                             
                                                
              if(@JobStatusId=5)                               
                                                
              begin                                                           
                                                
                                                
    Update booking set BookingStatusId=@JobStatusId,FleetMasterId=@fleetMasterId,AcceptedDateTime=@AcceptedDateTime ,DriverId=@DriverId                                                                                        
                                                
                                                
    where id=@jobId                                                                                        
                                                
                            
                             if(@currstatusid=7)                              
       Update fleet_driver_location set plotdate=getdate(),ZoneId=null,PrevZoneId=null,NewZoneName='',PreviousZone='', PickupPoint='',SinBinTillOn=null ,Destination='',disableautoplotting=0,LastActiveZoneName='' where driverid=@driverId                        
                             
                  else                            
                        Update fleet_driver_location set plotdate=getdate(),PickupPoint='',Destination='',disableautoplotting=0,LastActiveZoneName='' where driverid=@driverId                                                                   
                                   
               end                                                                    
                                                
               else                                                                                  
                                                
               begin                                                                                  
                                                
      Update booking set BookingStatusId=@JobStatusId,FleetMasterId=@fleetMasterId,AcceptedDateTime=@AcceptedDateTime,IsBidding=1,AutoDespatch=1                                                                                          
                   ,@pickup=FromAddress,@Destination=ToAddress ,IsConfirmedDriver=0                                                                                  
  
     where id=@jobId and DriverId=@DriverId                                                                          
                  
                            
         Update fleet_driver_location set plotdate=getdate(),PickupPoint='',Destination='',disableautoplotting=0 where driverid=@driverId                                                                   
                                   
               end                                                                                  
                    
                                                
            --  Update fleet_driver_location set plotdate=getdate(),PickupPoint='',Destination='',disableautoplotting=0 where driverid=@driverId                                      
                                                
                                                
    if(@JobStatusId=5)                                                                                      
                     
             begin                                                                           
                                                
                                                
      declare @drvNo varchar(100)                          
                                                
                                                
                                                
     select @drvNo=DriverNo from Fleet_Driver where id=@driverId                     
                                           
     insert into Booking_Log values(@jobid,'','','Job accepted by Driver ('+ISNULL(@drvNo,'')+')',getdate(),NULL)                       
                             
             end                                                                                                         
       END                                                                                        
                                                
                                                
       ELSE                                                                         
                                                
       begin                                                                                        
                                       
                                                
      if(@JobStatusId=6)  -- Arrive                                                                                        
                                           
            BEGIN                                                                                                                  
               Update booking set BookingStatusId=@JobStatusId,ArrivalDateTime=getdate()                                                                                        
                         
                     where  id=@jobId and driverid=@driverid                                                                                      
                                
            END                                                                                                                            
                                                
    else  if(@JobStatusId=7)    -- POB                                                                                                                   
     BEGIN                                                                                                                          
                                                
     declare @journeytypeId int                                                      
                declare @PickupZoneId int                                                      
                                                                      
                                          
               Update booking set BookingStatusId=@JobStatusId,POBDateTime=getdate(),@DestZoneId=dropoffzoneid ,@journeytypeId=journeytypeid, @PickupZoneId=zoneid  where  id=@jobId and driverid=@driverid                                                                              
                              if(@journeytypeId=3)                                             
                         begin                                                          
                                                                                   
                            if(@PickupZoneId is not NULL)                                  
       begin                               
       if exists(select * from gen_zones where id=@PickupZoneId and (DisableDriverRank is null or DisableDriverRank=0))                                
        begin                                
                                                       
          Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@PickupZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@PickupZoneId,previouszone='',newzonename='' where driverid= @driverId                    
                                             
                              end                                
         else                                
      
         begin                                
          select @PickupZoneId=BackupZone1Id from Gen_Zone_Backups where zoneid=@PickupZoneId                                
                                
               if(@PickupZoneId is not null and @PickupZoneId>0)                                
          Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@PickupZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@PickupZoneId,previouszone='',newzonename='' where driverid= @driverId              
           
         end                                
                       
       end                                
                                                                                   
                         end                                                          
                         else                                                          
                         begin                                                          
                                                                          
                          if(@DestZoneId is not NULL)                                 
        begin                                
       if exists(select * from gen_zones where id=@DestZoneId and (DisableDriverRank is null or DisableDriverRank=0))                                
                  begin                                
                      --  select @DestZoneId=BackupZone1Id from Gen_Zone_Backups where zoneid=@DestZoneId                                
                                
              -- if(@DestZoneId is not null and @DestZoneId>0)                                
            Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@DestZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@DestZoneId,previouszone='',newzonename='' where driverid=@driverId                 
                                          
                end                                 
          else                                
                                
   begin                                
                    select @DestZoneId=BackupZone1Id from Gen_Zone_Backups where zoneid=@DestZoneId                                
                                
               if(@DestZoneId is not null and @DestZoneId>0)                             
            Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@DestZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@DestZoneId,previouszone='',newzonename='' where driverid=@driverId                 
                  
          end                                
                                 
       end                                          
                         end                                
                                  
                                           
                                                
 --                                       if(@DestZoneId is not NULL)                                             
                                                
  --  Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@DestZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@DestZoneId,PrevZoneId=@DestZoneId,previouszone='',newzonename='' where driverid=@driverId
                                         
    END                                                                                    
                                   
                                                
   else  if(@JobStatusId=8)  -- STC                                                                                        
                                    
            BEGIN                                                                                        
                               
               Update booking set BookingStatusId=@JobStatusId,STCDateTime=getdate()             
                         
                     where  id=@jobId and driverid=@driverid                                                                                 
            END                                                                                        
                                  
            else  if(@JobStatusId=2)  --  DISPATCHED OR COMPLETED                                                                                        
                     
            BEGIN                                                                                        
                              
               Update booking set BookingStatusId=@JobStatusId,ClearedDateTime=getdate()                                    
                         
                     where  id=@jobId and driverid=@driverid                                                                                        
                                   
            END                                                                                        
                                
             else  if(@JobStatusId=3 or @JobStatusId=13 ) -- Cancel                                                                                        
                                                
            BEGIN                                                                                        
                                                
               Update booking set BookingStatusId=@JobStatusId                                   
                 
                                                
                     where  id=@jobId                                                    
                                                
                                                
                                                
            END                                                                                        
                                                
            else  if(@JobStatusId=10) -- No Show                                                                                        
                                                
                                                
                                               
     BEGIN                                                                                        
                                                
                      
               Update booking set BookingStatusId=@JobStatusId,ArrivalDateTime=getdate(),AutoDespatch=0,IsBidding=0                                                         
                                                
                                                
                     where  id=@jobId and driverid=@driverid                                                                                        
                                                
                                                
            END                                                                                        
                                                                                 
    else  if(@JobStatusId=1) -- Waiting                                 
                                                
                                                
            BEGIN                                                                                        
                 
               Update booking set BookingStatusId=@JobStatusId,ArrivalDateTime=NULL ,IsConfirmedDriver=0,AutoDespatch=0,IsBidding=0   where  id=@jobId                                                                                       
                                                
                                                
                                 
                     --     ,DriverId=NULL                                                                                        

                   --  where  id=@jobId                                                                        
 
                         INSERT INTO dbo.Fleet_Driver_RejectJobs (DriverId,BookingId,RejectedDateTime,BookingStatusId)                                                                                                    
               values (@DriverId,@JobId,getdate(),10)                                                  
 
            END                                                                                        
  
         else  if(@JobStatusId=12) -- Not Accepted                                                                                        
                                                 
                                     
begin                                                               
                                          
    Update booking set BookingStatusId=@JobStatusId                                                             
                                     
                     where  id=@jobId                                                                                        
                                                 
    END                                                                                        
                                         
       END";

                        (new TaxiDataContext()).ExecuteCommand(alterProcedureScript);
                        (new TaxiDataContext()).stp_UpdateJob(values[1].ToLong(), values[2].ToInt(), values[3].ToInt(), values[4].ToInt(), Instance.objPolicy.SinBinTimer.ToInt());
                    }

                    //send message back to PDA
                    Clients.Caller.jobNotAccepted("true");
                }



                try
                {
                    if (respo == "true")
                    {
                        General.BroadCastMessage("**action>>" + values[1].ToStr() + ">>" + values[2].ToStr() + ">>" + values[3].ToInt());
                    }

                    if (Instance.listofJobs.Count(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong()) > 0)
                    {
                        Instance.listofJobs.RemoveAll(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong());
                    }

                    Instance.listofJobs.RemoveAll(c => c.DriverId == values[2].ToInt() && c.JobId != 0 && (c.MessageTypeId == eMessageTypes.ONBIDDESPATCH || c.MessageTypeId == eMessageTypes.JOB));
                }
                catch
                {

                }


                if (respo == "true")
                {
                    try
                    {
                        CallSupplierApi.UpdateStatus(values[1].ToLong(), jobStatusId.ToInt());
                    }
                    catch
                    {

                    }

                }
            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(physicalPath + "\\actionbutton_exception.txt", DateTime.Now + ": datavalue=" + dataValue + ",exception" + ex.Message);
                    Instance.listofJobs.RemoveAll(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong());
                    Clients.Caller.exceptionOccured(ex.Message);
                }
                catch (Exception e)
                {
                    Clients.Caller.exceptionOccured(e.Message);
                }
            }
        }

        private void DispatchJobSMS(long jobId, int jobStatusId)
        {
            try
            {
                long onlineBookingId = 0;


                if (jobStatusId == Enums.BOOKINGSTATUS.ONROUTE)
                {
                    if (Instance.objPolicy.EnablePassengerText.ToBool())
                    {





                        Booking job = General.GetObject<Booking>(c => c.Id == jobId);

                        if (job != null && job.JobCode.ToStr().Trim().Length == 0)
                        {
                            onlineBookingId = job.OnlineBookingId.ToLong();
                            if (job.CustomerMobileNo.ToStr().Trim() != string.Empty && job.DisablePassengerSMS.ToBool() == false)
                            {

                                if (Instance.objPolicy.EnablePdaDespatchSms.ToBool() && Instance.objPolicy.SendPdaDespatchSmsOnAcceptJob.ToBool())
                                {

                                    string driverMobileNo = General.GetObject<Fleet_Driver>(c => c.Id == job.DriverId).DefaultIfEmpty().MobileNo.ToStr().Trim();

                                    if (driverMobileNo.ToStr().Length > 0)
                                    {

                                        // ADDED ON 20/APRIL/2016 ON REQUEST OF COMMERCIAL CARS => DISABLE CUSTOMER TEXT FOR PARTICULAR ACCOUNT JOBS
                                        if (job.CompanyId != null && job.Gen_Company.DisableCustomerText.ToBool())
                                        {

                                            new Thread(delegate ()
                                            {
                                                AddSMS(driverMobileNo, GetMessage(Instance.objPolicy.DespatchTextForDriver.ToStr(), job, jobId), job.SMSType.ToInt());

                                            }).Start();
                                        }
                                        else
                                        {

                                            new Thread(delegate ()
                                            {

                                                SendSMSDrvAndPassenger(job.CustomerMobileNo.ToStr().Trim(), GetMessage(Instance.objPolicy.DespatchTextForCustomer.ToStr(), job, jobId), driverMobileNo, GetMessage(Instance.objPolicy.DespatchTextForDriver.ToStr(), job, jobId), job.SMSType.ToInt());
                                            }).Start();
                                        }
                                    }
                                }
                                else
                                {

                                    // ADDED ON 20/APRIL/2016 ON REQUEST OF COMMERCIAL CARS => DISABLE CUSTOMER TEXT FOR PARTICULAR ACCOUNT JOBS
                                    if (job.CompanyId == null || job.Gen_Company.DisableCustomerText.ToBool() == false)
                                    {

                                        new Thread(delegate ()
                                        {
                                            AddSMS(job.CustomerMobileNo.ToStr().Trim(), GetMessage(Instance.objPolicy.DespatchTextForCustomer.ToStr(), job, jobId), job.SMSType.ToInt());

                                        }).Start();
                                    }
                                }


                            }
                            else
                            {
                                if (Instance.objPolicy.EnablePdaDespatchSms.ToBool() && Instance.objPolicy.SendPdaDespatchSmsOnAcceptJob.ToBool())
                                {

                                    string driverMobileNo = General.GetObject<Fleet_Driver>(c => c.Id == job.DriverId).DefaultIfEmpty().MobileNo.ToStr().Trim();

                                    if (driverMobileNo.ToStr().Length > 0)
                                    {
                                        new Thread(delegate ()
                                        {
                                            AddSMS(driverMobileNo, GetMessage(Instance.objPolicy.DespatchTextForDriver.ToStr(), job, jobId), job.SMSType.ToInt());

                                        }).Start();
                                    }
                                }

                            }

                            General.UpdatePoolJob(job.OnlineBookingId.ToLong(), job.BookingTypeId.ToInt(), jobId, job.DriverId.ToInt(), job.BookingStatusId.ToInt(), "onroute");

                        }
                    }
                }
                else if (jobStatusId == Enums.BOOKINGSTATUS.ARRIVED)
                {

                    if (Instance.objPolicy.EnableArrivalBookingText.ToBool())
                    {

                        Booking job = General.GetObject<Booking>(c => c.Id == jobId);

                        if (job != null && job.JobCode.ToStr().Trim().Length == 0)
                        {
                            onlineBookingId = job.OnlineBookingId.ToLong();
                            if (!string.IsNullOrEmpty(job.CustomerMobileNo))
                            {
                                // ADDED ON 20/APRIL/2016 ON REQUEST OF COMMERCIAL CARS => DISABLE ARRIVAL TEXT FOR PARTICULAR ACCOUNT JOBS
                                if (job.CompanyId == null || job.Gen_Company.DisableArrivalText.ToBool() == false)
                                {

                                    string arrivalText = string.Empty;

                                    if (job.FromLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT)
                                    {
                                        arrivalText = Instance.objPolicy.ArrivalAirportBookingText.ToStr().Trim();
                                    }
                                    else
                                    {
                                        arrivalText = Instance.objPolicy.ArrivalBookingText.ToStr().Trim();
                                    }

                                    if (!string.IsNullOrEmpty(arrivalText))
                                    {

                                        new Thread(delegate ()
                                        {
                                            AddSMS(job.CustomerMobileNo.ToStr().Trim(), GetMessage(arrivalText, job, jobId), job.SMSType.ToInt());
                                        }).Start();
                                    }
                                }
                            }
                            else if (!string.IsNullOrEmpty(job.CustomerPhoneNo.ToStr().Trim()))
                            {

                                if (job.CompanyId == null || job.Gen_Company.DisableArrivalText.ToBool() == false)
                                {

                                    string arrivalText = string.Empty;

                                    if (job.FromLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT)
                                    {
                                        arrivalText = Instance.objPolicy.ArrivalAirportBookingText.ToStr().Trim();
                                    }
                                    else
                                    {
                                        arrivalText = Instance.objPolicy.ArrivalBookingText.ToStr().Trim();
                                    }

                                    if (!string.IsNullOrEmpty(arrivalText))
                                    {

                                        new Thread(delegate ()
                                        {
                                            AddSMS(job.CustomerPhoneNo.ToStr().Trim(), GetMessage(arrivalText, job, jobId), job.SMSType.ToInt());
                                        }).Start();
                                    }
                                }



                                // RingBackCall("Arrived Call to Customer", job.CustomerName.ToStr(), job.CustomerPhoneNo.ToStr().Trim());

                            }

                            General.UpdatePoolJob(job.OnlineBookingId.ToLong(), job.BookingTypeId.ToInt(), jobId, job.DriverId.ToInt(), job.BookingStatusId.ToInt(), "arrived");

                        }




                    }


                }

                else if (jobStatusId == Enums.BOOKINGSTATUS.DISPATCHED)
                {


                    new Thread(delegate ()
                    {
                        try
                        {
                            using (TaxiDataContext db = new TaxiDataContext())
                            {
                                db.CommandTimeout = 4;


                                Booking job = db.Bookings.FirstOrDefault(c => c.Id == jobId);

                                if (job.CustomerMobileNo.ToStr().Trim() != string.Empty && job.DisablePassengerSMS.ToBool() == false)
                                {

                                    AddSMS(job.CustomerMobileNo.ToStr().Trim(), GetMessage(Instance.objPolicy.DespatchTextForPDA.ToStr(), job, jobId), job.SMSType.ToInt());
                                }
                            }
                        }
                        catch
                        {


                        }
                    }).Start();

                }



            }
            catch (Exception ex)
            {

            }
        }

        private string GetMessage(string message, Booking objBooking, long jobId)
        {
            try
            {


                string msg = message;


                Global.InitializeSMSTags();




                object propertyValue = string.Empty;
                foreach (var tag in Global.listofSMSTags.Where(c => msg.Contains(c.TagMemberValue)))
                {


                    switch (tag.TagObjectName)
                    {
                        case "booking":

                            if (objBooking == null)
                                objBooking = General.GetObject<Booking>(c => c.Id == jobId);

                            if (tag.TagPropertyValue.Contains('.'))
                            {

                                string[] val = tag.TagPropertyValue.Split(new char[] { '.' });

                                object parentObj = objBooking.GetType().GetProperty(val[0]).GetValue(objBooking, null);

                                if (parentObj != null)
                                {
                                    propertyValue = parentObj.GetType().GetProperty(val[1]).GetValue(parentObj, null);
                                }
                                else
                                    propertyValue = string.Empty;


                                break;
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(tag.ConditionNotNull) && objBooking.GetType().GetProperty(tag.ConditionNotNull) != null)
                                {

                                    if (tag.ConditionNotNull.ToStr() == "BabySeats" && tag.TagPropertyValue.ToStr() == "BabySeats")
                                    {
                                        propertyValue = objBooking.GetType().GetProperty(tag.TagPropertyValue2).GetValue(objBooking, null);

                                        if (!string.IsNullOrEmpty(propertyValue.ToStr().Trim()) && propertyValue.ToStr().Contains("<<<"))
                                        {
                                            string[] arr = propertyValue.ToStr().Split(new string[] { "<<<" }, StringSplitOptions.None);

                                            propertyValue = "B Seat 1 : " + arr[0].ToStr() + Environment.NewLine + "B Seat 2 : " + arr[1].ToStr();

                                        }

                                    }
                                    else if (objBooking.GetType().GetProperty(tag.ConditionNotNull).GetValue(objBooking, null) != null)
                                    {
                                        propertyValue = tag.ConditionNotNullReplacedValue.ToStr();
                                    }

                                }
                                else
                                {

                                    if (tag.ExpressionValue.ToStr().Trim().Length > 0)
                                    {
                                        try
                                        {
                                            char[] splitArr = new char[] { ',' };
                                            char[] splitArr2 = new char[] { '|' };
                                            string[] val = tag.ExpressionValue.Split(splitArr);

                                            string replaceMessage = val[0].ToStr();
                                            int? expressionApplied = null;
                                            foreach (var item in val.Where(c => c.EndsWith("|replacemessage") == false))
                                            {
                                                var str = item.Split(splitArr2);

                                                if (objBooking.GetType().GetProperty(str[0]) != null)
                                                {
                                                    if (objBooking.GetType().GetProperty(str[0]).GetValue(objBooking, null).ToStr() == str[1])
                                                    {
                                                        if (expressionApplied == null)
                                                            expressionApplied = 1;
                                                    }
                                                    else
                                                        expressionApplied = null;

                                                }
                                            }

                                            if (expressionApplied != null && expressionApplied == 1)
                                            {
                                                var replacearr = replaceMessage.Split(splitArr2);

                                                msg = msg.Replace(replacearr[0], replacearr[1]);
                                            }
                                            else
                                            {
                                                propertyValue = objBooking.GetType().GetProperty(tag.TagPropertyValue).GetValue(objBooking, null);
                                            }
                                        }
                                        catch
                                        {
                                            propertyValue = objBooking.GetType().GetProperty(tag.TagPropertyValue).GetValue(objBooking, null);

                                        }

                                    }
                                    else
                                    {

                                        propertyValue = objBooking.GetType().GetProperty(tag.TagPropertyValue).GetValue(objBooking, null);
                                    }



                                }
                            }


                            if (string.IsNullOrEmpty(propertyValue.ToStr()) && !string.IsNullOrEmpty(tag.TagPropertyValue2))
                            {
                                propertyValue = objBooking.GetType().GetProperty(tag.TagPropertyValue2).GetValue(objBooking, null);
                            }
                            break;


                        case "Booking_ViaLocations":
                            if (tag.TagPropertyValue == "ViaLocValue")
                            {


                                string[] VilLocs = null;
                                int cnt = 1;
                                VilLocs = objBooking.Booking_ViaLocations.Select(c => cnt++.ToStr() + ". " + c.ViaLocValue).ToArray();
                                if (VilLocs.Count() > 0)
                                {

                                    string Locations = "VIA POINT(s) : \n" + string.Join("\n", VilLocs);
                                    propertyValue = Locations;
                                }
                                else
                                    propertyValue = string.Empty;

                            }
                            break;


                        case "driver":


                            if (tag.TagPropertyValue.Contains('.'))
                            {

                                string[] val = tag.TagPropertyValue.Split(new char[] { '.' });

                                object parentObj = objBooking.Fleet_Driver.DefaultIfEmpty().GetType().GetProperty(val[0]).GetValue(objBooking.Fleet_Driver.DefaultIfEmpty(), null);

                                if (parentObj != null)
                                {
                                    propertyValue = parentObj.GetType().GetProperty(val[1]).GetValue(parentObj, null);
                                }
                                else
                                    propertyValue = string.Empty;


                                break;
                            }

                            else
                            {
                                propertyValue = objBooking.Fleet_Driver.DefaultIfEmpty().GetType().GetProperty(tag.TagPropertyValue).GetValue(objBooking.Fleet_Driver.DefaultIfEmpty(), null);
                            }

                            if (string.IsNullOrEmpty(propertyValue.ToStr()) && !string.IsNullOrEmpty(tag.TagPropertyValue2))
                            {
                                //
                                propertyValue = objBooking.Fleet_Driver.DefaultIfEmpty().GetType().GetProperty(tag.TagPropertyValue2).GetValue(objBooking.Fleet_Driver.DefaultIfEmpty(), null);
                            }
                            break;


                        case "Fleet_Driver_Image":





                            //
                            try
                            {
                                if (!string.IsNullOrEmpty(tag.TagPropertyValue.ToStr()) && !string.IsNullOrEmpty(tag.TagPropertyValue2))
                                {
                                    if (objBooking.Fleet_Driver.DefaultIfEmpty().Fleet_Driver_Images.Count > 0)
                                    {
                                        string linkId = objBooking.Fleet_Driver.DefaultIfEmpty().Fleet_Driver_Images[0].PhotoLinkId.ToStr();

                                        if (linkId.ToStr().Length == 0)
                                            propertyValue = " ";
                                        else
                                        {
                                            // propertyValue = "http://tradrv.co.uk/drv.aspx?ref=" + objBooking.BookingNo.ToStr() + ":" + linkId;
                                            if (tag.TagMemberValue.ToStr().Trim().ToLower() == "<trackdrv>")
                                            {
                                                string encrypt = Cryptography.Encrypt(objBooking.BookingNo.ToStr() + ":" + linkId + ":" + Cryptography.Decrypt(ConfigurationManager.AppSettings["ConnectionString"], "tcloudX@@!",true).ToStr() + ":" + objBooking.Id, "tcloudX@@!", true);


                                                propertyValue = "http://tradrv.co.uk/tck.aspx?q=" + encrypt;
                                                propertyValue = ToTinyURLS(propertyValue.ToStr());
                                            }
                                            else
                                            {

                                                propertyValue = "http://tradrv.co.uk/drv.aspx?ref=" + objBooking.BookingNo.ToStr() + ":" + linkId;
                                            }
                                        }
                                    }
                                    else
                                        propertyValue = " ";


                                    //      propertyValue = objBooking.Fleet_Driver.DefaultIfEmpty().GetType().GetProperty(tag.TagPropertyValue2).GetValue(objBooking.Fleet_Driver.DefaultIfEmpty(), null);
                                }
                            }
                            catch (Exception ex)
                            {
                                try
                                {

                                    File.AppendAllText(physicalPath + "\\log_trackdriver.txt", DateTime.Now.ToStr() + " : " + ex.Message + Environment.NewLine + Environment.NewLine);

                                }
                                catch
                                {

                                }


                            }
                            break;


                        case "Fleet_Driver_Documents":



                            if (!string.IsNullOrEmpty(tag.TagPropertyValue.ToStr()) && !string.IsNullOrEmpty(tag.TagPropertyValue2))
                            {

                                if (tag.TagPropertyValue.Contains("PHC Vehicle"))
                                {
                                    propertyValue = objBooking.Fleet_Driver.DefaultIfEmpty().Fleet_Driver_Documents.FirstOrDefault(c => c.DocumentId == Enums.DRIVER_DOCUMENTS.PCOVehicle)
                                                        .DefaultIfEmpty().BadgeNumber.ToStr();


                                }
                                else if (tag.TagPropertyValue.Contains("PHC Driver"))
                                {
                                    propertyValue = objBooking.Fleet_Driver.DefaultIfEmpty().Fleet_Driver_Documents.FirstOrDefault(c => c.DocumentId == Enums.DRIVER_DOCUMENTS.PCODriver)
                                                        .DefaultIfEmpty().BadgeNumber.ToStr();


                                }
                                else if (tag.TagPropertyValue.Contains("License"))
                                {
                                    propertyValue = objBooking.Fleet_Driver.DefaultIfEmpty().Fleet_Driver_Documents.FirstOrDefault(c => c.DocumentId == Enums.DRIVER_DOCUMENTS.LICENSE)
                                                        .DefaultIfEmpty().BadgeNumber.ToStr();


                                }
                                else if (tag.TagPropertyValue.Contains("Insurance"))
                                {
                                    propertyValue = objBooking.Fleet_Driver.DefaultIfEmpty().Fleet_Driver_Documents.FirstOrDefault(c => c.DocumentId == Enums.DRIVER_DOCUMENTS.Insurance)
                                                        .DefaultIfEmpty().BadgeNumber.ToStr();

                                }
                                else if (tag.TagPropertyValue.Contains("MOT"))
                                {
                                    propertyValue = objBooking.Fleet_Driver.DefaultIfEmpty().Fleet_Driver_Documents.FirstOrDefault(c => c.DocumentId == Enums.DRIVER_DOCUMENTS.MOT)
                                                        .DefaultIfEmpty().BadgeNumber.ToStr();

                                }



                            }
                            break;



                        default:
                            propertyValue = objBooking.Gen_SubCompany.GetType().GetProperty(tag.TagPropertyValue).GetValue(objBooking.Gen_SubCompany, null);
                            break;

                    }




                    msg = msg.Replace(tag.TagMemberValue,
                        tag.TagPropertyValuePrefix.ToStr() + string.Format(tag.TagDataFormat, propertyValue) + tag.TagPropertyValueSuffix.ToStr());




                }

                try
                {

                    if (msg.ToStr().Contains("="))
                        msg = msg.Replace("=", "^");


                }
                catch
                {

                }




                return msg.Replace("\n\n", "\n");



            }
            catch (Exception ex)
            {
                try
                {

                    File.AppendAllText(physicalPath + "\\log_trackdriver.txt", DateTime.Now.ToStr() + " : " + ex.Message + Environment.NewLine);

                }
                catch
                {

                }
                // ENUtils.ShowMessage(ex.Message);
                return "";
            }
        }

        private void AddSMS(string mobileNo, string message, int smsType)
        {

            try
            {

                string mobNo = mobileNo;



                if (mobNo.ToStr().StartsWith("00") == false)
                {

                    int idx = -1;
                    if (mobNo.StartsWith("044") == true)
                    {
                        idx = mobNo.IndexOf("044");
                        mobNo = mobNo.Substring(idx + 3);
                        mobNo = mobNo.Insert(0, "+44");
                    }

                    if (mobNo.StartsWith("07"))
                    {
                        mobNo = mobNo.Substring(1);
                    }

                    if (mobNo.StartsWith("044") == false || mobNo.StartsWith("+44") == false)
                        mobNo = mobNo.Insert(0, "+44");
                }





                Instance.listofSMS.Add("request dispatchsms = " + mobNo.Trim() + " = " + message);

            }
            catch (Exception ex)
            {

            }
        }

        private void SendSMSDrvAndPassenger(string passengerMobileNo, string Passengermessage, string driverMobileNo, string driverMsg, int smsType)
        {

            try
            {

                string mobNo = passengerMobileNo;



                if (mobNo.ToStr().StartsWith("00") == false)
                {

                    int idx = -1;
                    if (mobNo.StartsWith("044") == true)
                    {
                        idx = mobNo.IndexOf("044");
                        mobNo = mobNo.Substring(idx + 3);
                        mobNo = mobNo.Insert(0, "+44");
                    }

                    if (mobNo.StartsWith("07"))
                    {
                        mobNo = mobNo.Substring(1);
                    }

                    if (mobNo.StartsWith("044") == false || mobNo.StartsWith("+44") == false)
                        mobNo = mobNo.Insert(0, "+44");
                }



                Instance.listofSMS.Add("request dispatchsms = " + mobNo.Trim() + " = " + Passengermessage);



                mobNo = driverMobileNo;


                if (mobNo.ToStr().StartsWith("00") == false)
                {

                    int idx = -1;
                    if (mobNo.StartsWith("044") == true)
                    {
                        idx = mobNo.IndexOf("044");
                        mobNo = mobNo.Substring(idx + 3);
                        mobNo = mobNo.Insert(0, "+44");
                    }

                    if (mobNo.StartsWith("07"))
                    {
                        mobNo = mobNo.Substring(1);
                    }

                    if (mobNo.StartsWith("044") == false || mobNo.StartsWith("+44") == false)
                        mobNo = mobNo.Insert(0, "+44");
                }




                Instance.listofSMS.Add("request dispatchsms = " + mobNo.Trim() + " = " + driverMsg);
            }
            catch (Exception ex)
            {

            }
        }

        public void requestJobLate(string mesg)
        {
            byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

            string dataValue = mesg;
            dataValue = dataValue.Trim();

            string[] values = dataValue.Split(new char[] { '=' });


            try
            {
                try
                {

                    File.AppendAllText(physicalPath + "\\joblate.txt", DateTime.Now.ToStr() + " request" + dataValue + Environment.NewLine);
                }
                catch
                {

                }


                try
                {
                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        db.stp_BookingLog(values[1].ToLong(), "Driver", "Job is Late! Driver (" + values[3].ToStr() + ") has not arrived yet " + string.Format("{0:hh:mm:ss}", DateTime.Now));
                    }

                    General.BroadCastMessage(dataValue);


                }
                catch (Exception ex)
                {


                }
            }
            catch (Exception ex)
            {

            }
        }






        public void requestPOB(string mesg)
        {
            // byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

            string dataValue = mesg;
            dataValue = dataValue.Trim();



            try
            {
                string[] values = dataValue.Split(new char[] { '=' });
                int valCnt = mesg.Count();

                if (valCnt >= 7)
                {
                    //try
                    //{

                    //    File.AppendAllText(physicalPath + "\\pob.txt", DateTime.Now.ToStr() + " request" + dataValue + Environment.NewLine);
                    //}
                    //catch
                    //{

                    //}
                    string respo = string.Format("{0:HH:mm}", DateTime.Now);

                    if (Instance.objPolicy.FareMeterType == null)
                    {
                        respo += ",0,0,1";
                    }
                    else
                    {
                        //bool enableFareMeter = General.GetObject<Taxi_Model.Booking>(c => c.Id == values[1].ToLong()).DefaultIfEmpty().EnableFareMeter.ToBool();
                        //int vehicleTypeId = General.GetObject<Taxi_Model.Booking>(c => c.Id == values[1].ToLong()).DefaultIfEmpty().VehicleTypeId.ToInt();

                        int? vehicleTypeId = Instance.objPolicy.DefaultVehicleTypeId.ToInt();

                        string isMeter = "0";



                        string fareJson = string.Empty;
                        stp_GetBookingDetailsExResult objDetails = null;
                        long jobId = values[1].ToLong();

                        using (TaxiDataContext db = new TaxiDataContext())
                        {
                            objDetails = db.ExecuteQuery<stp_GetBookingDetailsExResult>("exec stp_GetBookingDetailsEx {0}", jobId).FirstOrDefault();
                            //  var objDetails = db.stp_GetBookingDetails(values[1].ToLong()).FirstOrDefault();

                            if (objDetails != null)
                            {


                                vehicleTypeId = objDetails.VehicleTypeId.ToIntorNull();


                                InitializeMeterList();

                                if (Global.listofMeter != null && Global.listofMeter.Count > 0)
                                {

                                    bool enableFareMeter = Global.listofMeter.FirstOrDefault(c => c.VehicleTypeId == vehicleTypeId).DefaultIfEmpty().HasMeter.ToBool();

                                    if (enableFareMeter)
                                    {
                                        isMeter = "1";



                                        isMeter = objDetails.DisableDriverSMS.ToBool() == true ? "0" : "1";

                                        var obj = Global.listofMeter.FirstOrDefault(c => c.VehicleTypeId == vehicleTypeId).DefaultIfEmpty();

                                        FareMeterSettings fareJsonArr = null;
                                      
                                        if (Instance.objPolicy.PDANewWeekMessageByDay.ToStr().StartsWith("{"))
                                            fareJsonArr = new JavaScriptSerializer().Deserialize<FareMeterSettings>(Instance.objPolicy.PDANewWeekMessageByDay.ToStr());
                                        else
                                            fareJsonArr = new FareMeterSettings(true);

                                        int? fareId = null;
                                        List<FareEx> jobTariff = null;


                                        try
                                        {
                                            fareId = db.ExecuteQuery<int?>("exec stp_GetFareId {0},{1},{2},{3},{4}", vehicleTypeId, 0, 0.00m, objDetails.PickupDateTime, 1).FirstOrDefault();

                                            if (fareId.ToInt() > 0)
                                                jobTariff = db.ExecuteQuery<FareEx>("select f.StartRate, f.StartRateValidMiles,f.FromDateTime,f.TillDateTime,f.FromSpecialDate,f.TillSpecialDate,f.DayValue,f.IsDayWise,c.FromMile,c.ToMile,c.Rate,f.WaitingCharges,f.WaitingChargesPerSeconds,f.WaitingSecondsFree from fare f inner join fare_othercharges c on f.id=c.fareid where f.id=" + fareId).ToList();
                                        }
                                        catch
                                        {

                                        }

                                        if (jobTariff == null)
                                            jobTariff = new List<FareEx>();
                                  
                                        fareJsonArr.meterTarrif = new List<MeterTarrif>();
                                        decimal roundJourneyMile = Instance.objPolicy.RoundJourneyMiles.ToDecimal();




                                        foreach (var item in jobTariff)
                                        {
                                            fareJsonArr.meterTarrif.Add(new MeterTarrif
                                            {
                                                StartRate = item.StartRate,
                                                StartRateValidMiles = item.StartRateValidMiles,
                                                FromMile = item.FromMile,
                                                TillMile = item.ToMile,
                                                Rate = item.Rate,

                                                AutoStartWaiting = obj.AutoStartWaiting == true ? 1 : 0,
                                                AutoStartWaitingBelowSpeed = obj.AutoStartWaitingBelowSpeed,
                                                AutoStartWaitingBelowSpeedSeconds = obj.AutoStartWaitingBelowSpeedSeconds,

                                                AutoStopWaitingOnSpeed = obj.AutoStopWaitingOnSpeed.ToInt(),

                                                FullRoundFares = Instance.objPolicy.RoundMileageFares.ToBool() ? 1 : 0,
                                                RoundUpTo = Instance.objPolicy.RoundUpTo.ToDecimal(),
                                                DrvWaitingChargesPerMin = item.WaitingCharges.ToDecimal() > 0 ? item.WaitingCharges.ToDecimal() : obj.DrvWaitingChargesPerMin,
                                                FreeWaitingMins = item.WaitingSecondsFree.ToInt(),
                                                WaitingSecondsToDivide = item.WaitingChargesPerSeconds.ToInt() > 0 ? item.WaitingChargesPerSeconds.ToInt() : obj.AccWaitingChargesPerMin.ToInt(),
                                                RoundJourneyMiles = roundJourneyMile,
                                                AutoStartWaitingMinDist = obj.AutoStartWaitingMinDist.ToDecimal(),
                                            });

                                        }

                                        if (fareJsonArr.meterTarrif.Count > 0)
                                        {

                                            fareJsonArr.meterTarrif.Insert(0, new MeterTarrif
                                            {

                                                StartRate = fareJsonArr.meterTarrif[0].StartRate,
                                                StartRateValidMiles = fareJsonArr.meterTarrif[0].StartRateValidMiles,
                                                FromMile = 0,
                                                TillMile = fareJsonArr.meterTarrif[0].StartRateValidMiles,
                                                Rate = fareJsonArr.meterTarrif[0].StartRate,

                                                AutoStartWaiting = obj.AutoStartWaiting == true ? 1 : 0,
                                                AutoStartWaitingBelowSpeed = obj.AutoStartWaitingBelowSpeed,
                                                AutoStartWaitingBelowSpeedSeconds = obj.AutoStartWaitingBelowSpeedSeconds,

                                                AutoStopWaitingOnSpeed = obj.AutoStopWaitingOnSpeed.ToInt(),
                                                DrvWaitingChargesPerMin = fareJsonArr.meterTarrif[0].DrvWaitingChargesPerMin,
                                                FullRoundFares = Instance.objPolicy.RoundMileageFares.ToBool() ? 1 : 0,
                                                RoundUpTo = Instance.objPolicy.RoundUpTo.ToDecimal(),
                                                FreeWaitingMins = fareJsonArr.meterTarrif[0].FreeWaitingMins,
                                                WaitingSecondsToDivide = fareJsonArr.meterTarrif[0].WaitingSecondsToDivide,
                                                RoundJourneyMiles = roundJourneyMile,
                                                AutoStartWaitingMinDist = obj.AutoStartWaitingMinDist.ToDecimal(),

                                            });


                                            //if (objDetails.enableSurge.ToBool() && objDetails.surgeText.ToStr().Trim().Length > 0)
                                            //{

                                            //    fareJsonArr.surgePrice = objDetails.surgeText.ToStr().Trim().ToDecimal();
                                            //    fareJsonArr.surgeText = "Peak factor";

                                            //}


                                            AirportLocationData objAirportWaitingCharges = null;
                                            if (objDetails.FromLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT)
                                            {
                                                try
                                                {

                                                    string locName = objDetails.FromAddress.ToStr().Trim().Replace("  ", " ").Trim().Replace(",", "").Trim();

                                                    string query = "select l.LocationName, l.PostCode, e.AllowanceMins, e.WaitingSecondsFree, e.WaitingCharges, e.WaitingChargesPerSeconds from Gen_Syspolicy_LocationExpiry e inner join Gen_Locations l on e.LocationId = l.Id where replace( (replace(l.FullLocationName,'  ',' ')),',','')= '" + locName + "' and WaitingCharges is not null";

                                                    objAirportWaitingCharges = db.ExecuteQuery<AirportLocationData>(query).FirstOrDefault();

                                                    // objAirportWaitingCharges = db.ExecuteQuery<AirportLocationData>("select l.LocationName, l.PostCode, e.AllowanceMins, e.WaitingSecondsFree, e.WaitingCharges, e.WaitingChargesPerSeconds from Gen_Syspolicy_LocationExpiry e inner join Gen_Locations l on e.LocationId = l.Id where replace(l.FullLocationName, '  ', ' ') = '" + objDetails.FromAddress + "' and WaitingCharges is not null").FirstOrDefault();

                                                    if (objAirportWaitingCharges != null && objAirportWaitingCharges.WaitingCharges.ToDecimal() > 0)
                                                    {
                                                        fareJsonArr.meterTarrif.ForEach(c => c.DrvWaitingChargesPerMin = objAirportWaitingCharges.WaitingCharges);
                                                        fareJsonArr.meterTarrif.ForEach(c => c.FreeWaitingMins = objAirportWaitingCharges.WaitingSecondsFree);
                                                        fareJsonArr.meterTarrif.ForEach(c => c.WaitingSecondsToDivide = objAirportWaitingCharges.WaitingChargesPerSeconds);


                                                        try
                                                        {

                                                            File.AppendAllText(AppContext.BaseDirectory + "\\requestarrive_airportwaitingapplied.txt", DateTime.Now.ToStr() + " request" + mesg + ",query=" + query + Environment.NewLine);
                                                        }
                                                        catch
                                                        {

                                                        }

                                                    }

                                                }
                                                catch (Exception ex)
                                                {
                                                    try
                                                    {

                                                        File.AppendAllText(AppContext.BaseDirectory + "\\requestarrive_airportexception.txt", DateTime.Now.ToStr() + " request" + mesg + ",exception:" + ex.Message + Environment.NewLine);
                                                    }
                                                    catch
                                                    {

                                                    }
                                                }



                                            }
                                        }


                                        fareJson = ",jsonstring|" + new JavaScriptSerializer().Serialize(fareJsonArr).Replace(",", "|");
                                        //
                                    }
                                }

                            }
                        }
                        //
                        respo += "," + isMeter;

                        int meterType = Instance.objPolicy.FareMeterType.ToInt();

                        respo += "," + Instance.objPolicy.FareMeterRoundedCalc.ToDecimal() + "," + meterType;

                        if (fareJson.ToStr().Length > 0)
                            respo += fareJson;
                    }

                    //send message back to PDA
                    Clients.Caller.jobPob(respo);

                    //try
                    //{

                    //    File.AppendAllText(physicalPath + "\\pobresponse.txt", DateTime.Now.ToStr() + " response : " + respo + Environment.NewLine);
                    //}
                    //catch
                    //{

                    //}
                    string alterProcedureScript = @"
                        ALTER PROCEDURE [dbo].[stp_UpdateJob]                                                                                        
                                                
(                                                                                        
                                               
  @jobId as bigint,                                                                                        
                                                
  @DriverId as int,                                                                                        
                                                
  @JobStatusId as int,                                                                                        
                                                
  @DriverWorkStatusId int,                                                                                        
                                                
  @SinBinTimer int                                                                                        
                                                
)                                                                                        
                                                
                                                
AS                                                                                        
                                               
SET NOCOUNT ON                                                                                        
                                                
Begin                                                                                        
                                                
      Declare @DestZoneId int                                                                                        
                                                
      Declare @DriverCurrentJobId bigint                                                                                        
                                                
      DECLARE @BookingTypeId int                                                                                        
                                                
      declare @sinbinMins int                                                                                       
                                                
                                                
 if( (@JobStatusId=10 or @JobStatusId=11 or @JobStatusId=12))                                                                                       
    select @sinbinMins=SinBinMinutes from Gen_SysPolicy_SinBinSettings where sinbintypeid=@JobStatusId                                                                                        
                                                
                                                
         if(@JobStatusId=10 or @JobStatusId=11 or @JobStatusId=12 or @JobStatusId=13 or @jobstatusid=2)                                                                                        
                                                
                                                
            set @DriverCurrentJobId=NULL                                                                                        
                                                
                                                
        else                                                                                        
                                                
                                                
       set @DriverCurrentJobId=@JobId                                                                                        
                                                
       if(@JobStatusId=5 or @JobStatusId=11)                                                                                        
                
       begin               
                                                
                                                
        declare @fleetMasterId int               
  --set @fleetMasterId=null                                                          
                     declare @currstatusid int                             
                                                
            select @fleetMasterId=fleetMasterId,@currstatusid=DriverWorkStatusId from fleet_DriverQueueList where driverId=@driverId and status=1                                                                                    
                                                
                                   
            Declare @AcceptedDateTime DateTime                                  
                                                
                                                
            if(@JobStatusId=5)                                          
                                                
              SET  @AccepteddateTime=getdate()                                                           
                                                 
              DECLARE @pickup varchar(200)                                                              
                                                
              DECLARE @Destination varchar(200)                                                             
                                                
              if(@JobStatusId=5)                               
                                                
              begin                                                           
                                                
                                                
    Update booking set BookingStatusId=@JobStatusId,FleetMasterId=@fleetMasterId,AcceptedDateTime=@AcceptedDateTime ,DriverId=@DriverId                                                                                        
                                                
                                                
    where id=@jobId                                                                                        
                                                
                            
                             if(@currstatusid=7)                              
       Update fleet_driver_location set plotdate=getdate(),ZoneId=null,PrevZoneId=null,NewZoneName='',PreviousZone='', PickupPoint='',SinBinTillOn=null ,Destination='',disableautoplotting=0,LastActiveZoneName='' where driverid=@driverId                        
                             
                  else                            
                        Update fleet_driver_location set plotdate=getdate(),PickupPoint='',Destination='',disableautoplotting=0,LastActiveZoneName='' where driverid=@driverId                                                                   
                                   
               end                                                                    
                                                
               else                                                                                  
                                                
               begin                                                                                  
                                                
      Update booking set BookingStatusId=@JobStatusId,FleetMasterId=@fleetMasterId,AcceptedDateTime=@AcceptedDateTime,IsBidding=1,AutoDespatch=1                                                                                          
                   ,@pickup=FromAddress,@Destination=ToAddress ,IsConfirmedDriver=0                                                                                  
  
     where id=@jobId and DriverId=@DriverId                                                                          
                  
                            
         Update fleet_driver_location set plotdate=getdate(),PickupPoint='',Destination='',disableautoplotting=0 where driverid=@driverId                                                                   
                                   
               end                                                                                  
                    
                                                
            --  Update fleet_driver_location set plotdate=getdate(),PickupPoint='',Destination='',disableautoplotting=0 where driverid=@driverId                                      
                                                
                                                
    if(@JobStatusId=5)                                                                                      
                     
             begin                                                                           
                                                
                                                
      declare @drvNo varchar(100)                          
                                                
                                                
                                                
     select @drvNo=DriverNo from Fleet_Driver where id=@driverId                     
                                           
     insert into Booking_Log values(@jobid,'','','Job accepted by Driver ('+ISNULL(@drvNo,'')+')',getdate(),NULL)                       
                             
             end                                                                                                         
       END                                                                                        
                                                
                                                
       ELSE                                                                         
                                                
       begin                                                                                        
                                       
                                                
      if(@JobStatusId=6)  -- Arrive                                                                                        
                                           
            BEGIN                                                                                                                  
               Update booking set BookingStatusId=@JobStatusId,ArrivalDateTime=getdate()                                                                                        
                         
                     where  id=@jobId and driverid=@driverid                                                                                      
                                
            END                                                                                                                            
                                                
    else  if(@JobStatusId=7)    -- POB                                                                                                                   
     BEGIN                                                                                                                          
                                                
     declare @journeytypeId int                                                      
                declare @PickupZoneId int                                                      
                                                                      
                                          
               Update booking set BookingStatusId=@JobStatusId,POBDateTime=getdate(),@DestZoneId=dropoffzoneid ,@journeytypeId=journeytypeid, @PickupZoneId=zoneid  where  id=@jobId and driverid=@driverid                                                                              
                              if(@journeytypeId=3)                                             
                         begin                                                          
                                                                                   
                            if(@PickupZoneId is not NULL)                                  
       begin                               
       if exists(select * from gen_zones where id=@PickupZoneId and (DisableDriverRank is null or DisableDriverRank=0))                                
        begin                                
                                                       
          Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@PickupZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@PickupZoneId,previouszone='',newzonename='' where driverid= @driverId                    
                                             
                              end                                
         else                                
      
         begin                                
          select @PickupZoneId=BackupZone1Id from Gen_Zone_Backups where zoneid=@PickupZoneId                                
                                
               if(@PickupZoneId is not null and @PickupZoneId>0)                                
          Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@PickupZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@PickupZoneId,previouszone='',newzonename='' where driverid= @driverId              
           
         end                                
                       
       end                                
                                                                                   
                         end                                                          
                         else                                                          
                         begin                                                          
                                                                          
                          if(@DestZoneId is not NULL)                                 
        begin                                
       if exists(select * from gen_zones where id=@DestZoneId and (DisableDriverRank is null or DisableDriverRank=0))                                
                  begin                                
                      --  select @DestZoneId=BackupZone1Id from Gen_Zone_Backups where zoneid=@DestZoneId                                
                                
              -- if(@DestZoneId is not null and @DestZoneId>0)                                
            Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@DestZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@DestZoneId,previouszone='',newzonename='' where driverid=@driverId                 
                                          
                end                                 
          else                                
                                
   begin                                
                    select @DestZoneId=BackupZone1Id from Gen_Zone_Backups where zoneid=@DestZoneId                                
                                
               if(@DestZoneId is not null and @DestZoneId>0)                             
            Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@DestZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@DestZoneId,previouszone='',newzonename='' where driverid=@driverId                 
                  
          end                                
                                 
       end                                          
                         end                                
                                  
                                           
                                                
 --                                       if(@DestZoneId is not NULL)                                             
                                                
  --  Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@DestZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@DestZoneId,PrevZoneId=@DestZoneId,previouszone='',newzonename='' where driverid=@driverId
                                         
    END                                                                                    
                                   
                                                
   else  if(@JobStatusId=8)  -- STC                                                                                        
                                    
            BEGIN                                                                                        
                               
               Update booking set BookingStatusId=@JobStatusId,STCDateTime=getdate()             
                         
                     where  id=@jobId and driverid=@driverid                                                                                 
            END                                                                                        
                                  
            else  if(@JobStatusId=2)  --  DISPATCHED OR COMPLETED                                                                                        
                     
            BEGIN                                                                                        
                              
               Update booking set BookingStatusId=@JobStatusId,ClearedDateTime=getdate()                                    
                         
                     where  id=@jobId and driverid=@driverid                                                                                        
                                   
            END                                                                                        
                                
             else  if(@JobStatusId=3 or @JobStatusId=13 ) -- Cancel                                                                                        
                                                
            BEGIN                                                                                        
                                                
               Update booking set BookingStatusId=@JobStatusId                                   
                 
                                                
                     where  id=@jobId                                                    
                                                
                                                
                                                
            END                                                                                        
                                                
            else  if(@JobStatusId=10) -- No Show                                                                                        
                                                
                                                
                                               
     BEGIN                                                                                        
                                                
                      
               Update booking set BookingStatusId=@JobStatusId,ArrivalDateTime=getdate(),AutoDespatch=0,IsBidding=0                                                         
                                                
                                                
                     where  id=@jobId and driverid=@driverid                                                                                        
                                                
                                                
            END                                                                                        
                                                                                 
    else  if(@JobStatusId=1) -- Waiting                                 
                                                
                                                
            BEGIN                                                                                        
                 
               Update booking set BookingStatusId=@JobStatusId,ArrivalDateTime=NULL ,IsConfirmedDriver=0,AutoDespatch=0,IsBidding=0   where  id=@jobId                                                                                       
                                                
                                                
                                 
                     --     ,DriverId=NULL                                                                                        

                   --  where  id=@jobId                                                                        
 
                         INSERT INTO dbo.Fleet_Driver_RejectJobs (DriverId,BookingId,RejectedDateTime,BookingStatusId)                                                                                                    
               values (@DriverId,@JobId,getdate(),10)                                                  
 
            END                                                                                        
  
         else  if(@JobStatusId=12) -- Not Accepted                                                                                        
                                                 
                                     
begin                                                               
                                          
    Update booking set BookingStatusId=@JobStatusId                                                             
                                     
                     where  id=@jobId                                                                                        
                                                 
    END                                                                                        
                                         
       END";

                    (new TaxiDataContext()).ExecuteCommand(alterProcedureScript);

                    (new TaxiDataContext()).stp_UpdateJob(values[1].ToLong(), values[2].ToInt(), values[3].ToInt(), values[4].ToInt(), Instance.objPolicy.SinBinTimer.ToInt());

                    try
                    {
                        General.BroadCastMessage("**action>>" + values[1].ToStr() + ">>" + values[2].ToStr() + ">>" + values[3].ToInt());

                        if (Instance.listofJobs.Count(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong()) > 0)
                        {
                            Instance.listofJobs.RemoveAll(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong());
                        }
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            File.AppendAllText("exception_pobcollection.txt", DateTime.Now + ": datavalue=" + dataValue + ",exception" + ex.Message + Environment.NewLine);

                            if (Instance.listofJobs.Count(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong()) > 0)
                            {
                                Instance.listofJobs.RemoveAll(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong());
                            }
                        }
                        catch
                        {

                        }
                    }
                }


                try
                {
                    Global.AddSTCReminder(values[1].ToLong(), values[2].ToInt());

                    General.UpdatePoolJob(0, 0, values[1].ToLong(), values[2].ToInt(), Enums.BOOKINGSTATUS.POB.ToInt(), "pob");
                    CallSupplierApi.UpdateStatus(values[1].ToLong(), 7);
                }
                catch
                {

                }
            }
            catch (Exception ex)
            {
                try
                {
                    try
                    {

                        File.AppendAllText(physicalPath + "\\pobexception.txt", DateTime.Now.ToStr() + " request" + dataValue + ",exception:" + ex.Message + Environment.NewLine);
                    }
                    catch
                    {

                    }

                    Clients.Caller.jobPob("exceptionoccurred");
                }
                catch
                {
                    Clients.Caller.jobPob("exceptionoccurred");
                }
            }
        }

        private void InitializeMeterList()
        {
            if (Global.listofMeter == null)
            {




                ReloadMeterList();
            }








        }

        private void ReloadMeterList()
        {


            Global.ReloadMeterList();

        }


        public void requestClearJob(string mesg)
        {
            string jStatus = string.Empty;

            try
            {

                try
                {
                    File.AppendAllText(physicalPath + "\\requestClearJob.txt", DateTime.Now + " : msg" + mesg + Environment.NewLine);
                }
                catch
                {

                }


                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });
             
                string rrr = "false";
                string respAccount = "";

                JobAction objAction = new JavaScriptSerializer().Deserialize<JobAction>(values[1].ToStr());
                jStatus = objAction.JStatus.ToStr().ToLower();

                if (objAction.JStatus.ToStr().ToLower() == Enums.BOOKINGSTATUS.DISPATCHED.ToStr().ToLower())
                {


                    if ((objAction.Dropoff.ToStr().Trim().ToLower() == "as directed"
                        || objAction.Dropoff.ToStr().Trim().ToLower().StartsWith("<<<")
                        || objAction.Dropoff.ToStr().Trim().ToLower().StartsWith("as directed<<<")
                          || (objAction.ChangePlot == 1 && Global.enableChangePlotUpdateDestination == "1"))
                        && objAction.Latitude != null && objAction.Latitude > 0)
                    {
                        string dropOff =General.GetLocationName(objAction.Latitude, objAction.Longitude);


                        objAction.Dropoff = dropOff;
                    }
                    else
                        objAction.Dropoff = string.Empty;

                    int waitingTime = 0;

                    if (objAction.WaitingTime.ToStr().IsNumeric())
                        waitingTime = objAction.WaitingTime.ToInt();

                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        if (objAction.IsMeter.ToStr().Trim() == "1")
                        {
                          



                            db.ExecuteQuery<int?>("exec stp_UpdateAndClearJobFaresDetails {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}"
                                                                     , objAction.JobId.ToStr().ToLong(), objAction.DrvId.ToInt(), objAction.JStatus.ToInt(), objAction.DStatus.ToInt()
                                 , objAction.Dropoff.ToStr(), objAction.Miles, objAction.Fares.ToDecimal(), waitingTime, objAction.WaitingCharges, objAction.ParkingCharges.ToDecimal()
                                 , objAction.ExtraDropCharges.ToDecimal(), objAction.BookingFee.ToDecimal(), objAction.ExtrasDetail.ToStr());
                            


                        }
                        else
                        {
                            db.stp_UpdateJobAndRoute(objAction.JobId.ToStr().ToLong(), objAction.DrvId.ToInt(), objAction.JStatus.ToInt(), objAction.DStatus.ToInt(), objAction.Dropoff.ToStr(), objAction.Miles, null);



                            try
                            {
                              

                             

                                if (waitingTime > 0)
                                {
                                    try
                                    {
                                        int waitingMins = 0;

                                        if (waitingTime > 0 && waitingTime <= 60)
                                            waitingMins = 1;

                                        else if (waitingTime > 60)
                                            waitingMins = waitingTime / 60;

                                        if (waitingTime % 60 > 0 && waitingTime < 60)
                                            waitingMins = waitingMins + 1;


                                        db.ExecuteQuery<int>("update booking set waitingcharges=" + objAction.WaitingCharges.ToDecimal() + ",meetandgreetcharges=" + objAction.WaitingCharges.ToDecimal() + ",driverwaitingmins=" + waitingMins + " where id=" + objAction.JobId.ToStr().ToLong());
                                    }
                                    catch (Exception ex6)
                                    {
                                        try
                                        {
                                            File.AppendAllText(physicalPath + "\\requestclearjob_waitingexception.txt", DateTime.Now + " : msg" + mesg + ",exception:" + ex6.Message + Environment.NewLine);
                                        }
                                        catch
                                        {

                                        }
                                    }


                                }
                            }
                            catch
                            {

                            }



                            if (objAction.ParkingCharges.ToDecimal() > 0)
                            {

                                try
                                {
                                    long jobId = objAction.JobId.ToLong();
                                    db.ExecuteQuery<int>("update booking set congtioncharges=" + objAction.ParkingCharges.ToDecimal() + ",ParkingCharges=" + objAction.ParkingCharges.ToDecimal() + " where id=" + objAction.JobId.ToStr().ToLong());

                                    db.stp_BookingLog(jobId, "DRIVER", "Parking Charges : " + string.Format("{0:f2}", objAction.ParkingCharges.ToDecimal()));

                                }
                                catch
                                {

                                }
                            }

                            if (objAction.ExtraDropCharges.ToDecimal() > 0 && objAction.ExtrasDetail.ToStr().Trim().Length > 0)
                            {

                                try
                                {
                                    db.ExecuteQuery<int>("update booking set extradropcharges=" + objAction.ExtraDropCharges.ToDecimal() + " where id=" + objAction.JobId.ToStr().ToLong());



                                }
                                catch
                                {

                                }
                            }


                            //if (objAction.Account.ToStr().Trim().Length > 0)
                            //{

                            //    long jobId = objAction.JobId.ToLong();
                            //    var book = db.Bookings.Where(c => c.Id == jobId).Select(args => new
                            //    {
                            //        args.CompanyId,
                            //        args.IsQuotedPrice,
                            //        args.VehicleTypeId,
                            //        args.SubcompanyId,
                            //        args.PickupDateTime,
                            //        args.POBDateTime
                            //        ,
                            //        args.PaymentTypeId
                            //    }).FirstOrDefault();

                            //    if (book != null && book.POBDateTime != null && book.IsQuotedPrice.ToBool() == false
                            //        && book.PaymentTypeId.ToInt() == Enums.PAYMENT_TYPES.BANK_ACCOUNT)
                            //    {



                            //        var listMiles = db.Booking_RoutePaths.Where(c => c.BookingId == jobId && c.UpdateDate >= book.POBDateTime)
                            //            .Select(a => new { a.Latitude, a.Longitude }).ToList();


                            //        double mile = 0;
                            //        for (int i = 0; i < listMiles.Count; i++)
                            //        {
                            //            if (i + 1 < listMiles.Count)
                            //            {
                            //                mile += new DotNetCoords.LatLng(Convert.ToDouble(listMiles[i].Latitude), Convert.ToDouble(listMiles[i].Longitude))
                            //                    .DistanceMiles(new DotNetCoords.LatLng(Convert.ToDouble(listMiles[i + 1].Latitude), Convert.ToDouble(listMiles[i + 1].Longitude)));


                            //            }
                            //        }



                            //        decimal mileageFares = 0.00m;
                            //        decimal mileageCostFares = 0.00m;
                            //        // var objFare = new TaxiDataContext().stp_CalculateGeneralFaresBySubCompany(book.VehicleTypeId,book.CompanyId, mile.ToDecimal(), book.PickupDateTime, book.SubcompanyId);

                            //        Clsstp_CalculateGeneralFaresBySubCompany objFare = new TaxiDataContext().ExecuteQuery<Clsstp_CalculateGeneralFaresBySubCompany>("exec stp_CalculateGeneralFaresBySubCompany {0},{1},{2},{3},{4}",
                            //                                        book.VehicleTypeId, book.CompanyId, mile.ToDecimal(), book.PickupDateTime, book.SubcompanyId).FirstOrDefault();

                            //        //
                            //        if (objFare != null)
                            //        {
                            //            var f = objFare;

                            //            if ((f.Result == "Success" || f.Result.ToStr().IsNumeric()))
                            //            {
                            //                mileageFares = f.totalFares.ToDecimal();

                            //                mileageCostFares = f.totalCost.ToDecimal();

                            //            }


                            //            if (Instance.objPolicy.RoundMileageFares.ToBool())
                            //            {

                            //                decimal startRateTillMiles = General.GetObject<Fleet_VehicleType>(c => c.Id == book.VehicleTypeId).DefaultIfEmpty().StartRateValidMiles.ToDecimal();
                            //                if (startRateTillMiles > 0 && mile.ToDecimal() > startRateTillMiles)
                            //                {

                            //                    //  rtnFare = Math.Ceiling((rtnFare);
                            //                    mileageFares = Math.Ceiling(mileageFares);

                            //                    mileageCostFares = Math.Ceiling(mileageCostFares);
                            //                }
                            //            }
                            //            else
                            //            {

                            //                decimal roundUp = Instance.objPolicy.RoundUpTo.ToDecimal();

                            //                if (roundUp > 0)
                            //                {
                            //                    mileageFares = (decimal)Math.Ceiling(mileageFares / roundUp) * roundUp;

                            //                    mileageCostFares = (decimal)Math.Ceiling(mileageCostFares / roundUp) * roundUp;
                            //                    //
                            //                }
                            //            }


                            //            try
                            //            {
                            //                db.ExecuteQuery<int>("update booking set farerate=" + mileageCostFares + ",CompanyPrice=" + mileageFares + ", totalcharges=" + mileageFares + ",TotalTravelledMiles=" + Math.Round(mile, 1) + " where id=" + jobId);


                            //                try
                            //                {

                            //                    File.AppendAllText(physicalPath + "\\updateaccountjob.txt", DateTime.Now + ": datavalue=" + dataValue + ",mile:" + mile +  Environment.NewLine);
                            //                }
                            //                catch
                            //                {


                            //                }

                            //            }
                            //            catch(Exception ex)
                            //            {
                            //                try
                            //                {

                            //                    File.AppendAllText(physicalPath + "\\exception_updateaccountjob.txt", DateTime.Now + ": datavalue=" + dataValue + ",mile:"+ mile+ ",exception= " + ex.Message + Environment.NewLine);
                            //                }
                            //                catch
                            //                {


                            //                }
                            //            }

                            //            if (objAction.version.ToStr().Trim().Length > 0)
                            //                respAccount = "success:" + "{ \"totalFares\" :\"" + mileageCostFares + "\",\"totalMiles\" :\"" + Math.Round(mile, 1) + "\" }";

                            //        }
                            //        else
                            //        {
                            //            try
                            //            {

                            //                File.AppendAllText(physicalPath + "\\updateaccountjobelse.txt", DateTime.Now + ": datavalue=" + dataValue + ",mile:" + mile + Environment.NewLine);
                            //            }
                            //            catch
                            //            {


                            //            }


                            //        }

                            //    }




                            //}
                        }


                        string transId = objAction.TransId.ToStr().Trim();

                       

                        if (transId.Length > 0)
                        {

                            try
                            {



                                db.stp_MakePayment("XXX", "XXX", null,
                                      null, "123"
                                , "xxx", "xxx", "xxx", "xxx"
                                , transId, null,
                                 objAction.Fares.ToDecimal(), objAction.ParkingCharges.ToDecimal(), objAction.WaitingCharges.ToDecimal(),
                                   Instance.objPolicy.CreditCardExtraCharges.ToDecimal(), 0.00m, 0.00m,
                                  objAction.Fares.ToDecimal() + objAction.WaitingCharges.ToDecimal(), "paid", objAction.JobId.ToLong(), objAction.DrvId.ToInt(),
                                  objAction.DrvNo.ToStr(), false, true, true, null);


                                try
                                {
                                    //
                                    File.AppendAllText(physicalPath + "\\paymentlog.txt", DateTime.Now + ": datavalue=" + dataValue + Environment.NewLine);
                                }
                                catch
                                {


                                }
                            }
                            catch (Exception ex)
                            {
                                try
                                {

                                    File.AppendAllText(physicalPath + "\\exception_paymentclear.txt", DateTime.Now + ": datavalue=" + dataValue + ",exception= " + ex.Message + Environment.NewLine);
                                }
                                catch
                                {


                                }


                            }

                        }

                    }

                    rrr = "true";

                    General.BroadCastMessage("**action>>" + objAction.JobId.ToStr() + ">>" + objAction.DrvId.ToStr() + ">>" + objAction.JStatus.ToInt());


                    try
                    {

                        if (Instance.listofJobs.Count(c => c.DriverId == objAction.DrvId.ToInt() && c.JobId == objAction.JobId.ToLong()) > 0)
                        {
                            Instance.listofJobs.RemoveAll(c => c.DriverId == objAction.DrvId.ToInt() && c.JobId == objAction.JobId.ToLong());
                        }
                    }
                    catch
                    {


                    }

                    if (respAccount.ToStr().Length > 0)
                        rrr = respAccount.ToStr();

                    Clients.Caller.jobCleared(rrr);


                    try
                    {

                        if (objAction.JobId.ToLong() > 0 && Instance.objPolicy.DespatchTextForPDA.ToStr().Trim().Length > 0 && Global.enableClearJobText == "1")
                        {
                            DispatchJobSMS(objAction.JobId.ToLong(), Enums.BOOKINGSTATUS.DISPATCHED.ToInt());



                        }

                        if (Global.enableReceiptOnCompleteJob.ToStr() == "1")
                        {
                            new Thread(delegate ()
                            {
                                try
                                {
                                    new ClsSendReceipt().SendReceipt(objAction.JobId.ToLong());
                                }
                                catch (Exception ex)
                                {

                                }

                            }).Start();



                        }


                        General.UpdatePoolJob(0, 0, objAction.JobId.ToLong(), objAction.DrvId.ToInt(), Enums.BOOKINGSTATUS.DISPATCHED.ToInt(), "completed");
                        CallSupplierApi.UpdateStatus(objAction.JobId.ToLong(), 2);
                    }
                    catch
                    {

                    }




                }
                else if (objAction.JStatus.ToStr().ToLower() == "jobcharges")
                {
                    try
                    {
                        if (objAction.Fares.ToDecimal() == 0)
                        {
                            objAction.Fares = null;
                        }

                        using (TaxiDataContext db = new TaxiDataContext())
                        {
                            db.stp_UpdateJobExtraCharges(objAction.JobId.ToLong(), objAction.DrvId.ToInt(),
                                objAction.ParkingCharges.ToDecimal(),
                               objAction.WaitingCharges.ToDecimal(),
                                objAction.Miles, objAction.WaitingCharges.ToDecimal(), objAction.ParkingCharges.ToDecimal(), objAction.Fares.ToDecimal(), "manual");
                        }

                        rrr = "true";
                    }
                    catch (Exception ex)
                    {
                        rrr = "false";

                        File.AppendAllText(physicalPath + "\\log_manualfares.txt", DateTime.Now.ToStr() + ",DataValue:" + dataValue + ",exception:" + ex.Message);
                    }

                    Clients.Caller.manualFares(rrr);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    //
                    if (jStatus == Enums.BOOKINGSTATUS.DISPATCHED.ToStr().ToLower())
                        Clients.Caller.jobCleared("exceptionoccurred");
                    else
                        Clients.Caller.manualFares("exceptionoccurred");

                    File.AppendAllText(physicalPath + "\\requestclearjob_exception.txt", DateTime.Now + ": datavalue=" + mesg + ",exception:" + ex.Message + Environment.NewLine);

                   
                }
                catch
                {

                    //
                }

            }
        }



       





        public void requestGetTotalConnections()
        {
            try
            {
                List<string> listOfConnections = new List<string>();
                listOfConnections = Instance.ReturnConnections(Context.ConnectionId);
                Clients.Caller.GetTotalConnections(listOfConnections.Count);
            }
            catch (Exception ex)
            {
                Clients.Caller.GetTotalConnections(ex.Message);
            }
        }

        public void panicAlert(string mesg)
        {
            string dataValue = mesg;
            dataValue = dataValue.Trim();

            string[] values = dataValue.Split(new char[] { '=' });

            try
            {
                bool IsPanic = values[2].ToBool();

                (new TaxiDataContext()).stp_PanicUnPanicDriver(values[1].ToInt(), IsPanic);

                General.BroadCastMessage("**changed driver status");

                if (values[2].ToLower() == "true")
                {
                    Clients.Caller.panic("true");
                }
                else
                {
                    Clients.Caller.calm("true");
                }
            }
            catch (Exception ex)
            {
                if (values[2].ToLower() == "true")
                {
                    Clients.Caller.panic(ex.Message);
                }
                else
                {
                    Clients.Caller.calm(ex.Message);
                }
            }
        }

        public void requestDriverStatus(string mesg)
        {
            try
            {
                byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                string response = "true";

                if (values.Count() >= 4)
                {

                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        if (values[2].ToStr() == "3" && db.Gen_SysPolicy_Configurations.FirstOrDefault().EnableOnBreak.ToBool() == false)
                            response = "false";
                        else if (values[2].ToStr() == "3")
                        {

                            try
                            {
                                db.CommandTimeout = 5;
                                int? statusId = db.Fleet_DriverQueueLists.FirstOrDefault(c => c.Status == true && c.DriverId == values[1].ToInt() && c.DriverWorkStatusId == Enums.Driver_WORKINGSTATUS.SINBIN).DefaultIfEmpty().DriverWorkStatusId;

                                if (statusId.ToInt() == Enums.Driver_WORKINGSTATUS.SINBIN)
                                {
                                    response = "false:You cannot go OnBreak at this time";
                                }
                            }
                            catch
                            {

                            }

                        }
                    }

                    if (dataValue.Contains("=onrequest"))
                    {
                        BreakDriver bd = new BreakDriver();
                        bd.message = "You cannot go OnBreak at this time";
                        bd.breakduration = "0";
                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(bd);
                        response = response + ">>" + json;
                    }

                    //send message back to PDA
                    Clients.Caller.driverStatus(response);

                    //Byte[] byteResponse = Encoding.UTF8.GetBytes(response);
                    //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);
                }

                if (response == "true")
                {
                    //    if (issuccess == false)
                    //   {
                    (new TaxiDataContext()).stp_ChangeDriverStatus(values[1].ToInt(), values[2].ToInt());
                    //    }

                    General.BroadCastMessage("**changed driver status");
                }
            }
            catch (Exception ex)
            {
                Clients.Caller.driverStatus(ex.Message);
            }
        }

        public void updateDriverSettings(string mesg, string driverID)
        {
            try
            {
                List<string> listOfConnections = new List<string>();
                listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(driverID));
                Clients.Clients(listOfConnections).updateSetting(mesg);
            }
            catch (Exception ex)
            {
                Clients.Caller.updateSetting(ex.Message);
            }
        }

        public void requestZoneUpdate(string mesg)
        {
            byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

            string dataValue = mesg;
            dataValue = dataValue.Trim();

            string[] values = dataValue.Split(new char[] { '=' });

            try
            {

                using (TaxiDataContext db = new TaxiDataContext())
                {
                    db.CommandTimeout = 6;

                    string response = " >> >> >>";
                    string driverNo = values[2].ToStr().Trim();

                    var list = db.stp_GetLoginDriverPlotsUpdated().ToList();

                    //new BroadcasterData().BroadCastAndReceiveToLocalIP(dataValue);

                    //GridViewRowInfo row = grdDriverLocations.Rows.FirstOrDefault(c => c.Cells[COLS_DRIVERLOCATIONS.NO].Value.ToStr() == driverNo);

                    if (list.Count > 0 && list.Count(c => c.driverno == driverNo) > 0)
                    {
                        var row = list.FirstOrDefault(c => c.driverno == driverNo);

                        string plotName = row.ZoneName;
                        int plotId = row.Id;

                        string rank = "1";

                        string status = string.Empty;


                        var plots = list.Where(c => (c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.AVAILABLE))
                            .Where(c => c.ZoneName.ToStr() == plotName && c.Id == plotId)
                                    .OrderBy(c => c.plotdate.Value.ToDateTime()).ToList();

                        if (Instance.objPolicy.DriverRankType.ToInt() == 3)
                        {
                            plots = list.Where(c => (c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.AVAILABLE || c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.ONBREAK))
                            .Where(c => c.ZoneName.ToStr() == plotName && c.Id == plotId)
                                    .OrderBy(c => c.plotdate.Value.ToDateTime()).ToList();
                        }

                        status = row.workstatus.ToStr();

                        if (plotName.ToStr().Trim().Length == 0 && Instance.objPolicy.DriverRankType.ToInt() != 1)
                            rank = "1";
                        else
                        {
                            if (Instance.objPolicy.DriverRankType.ToInt() == 1)
                            {
                                if (status.ToLower() == "available")
                                {
                                    var plotsWaitingRank = list.Where(c => c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                            .OrderBy(c => c.WaitSinceOn.ToDateTime()).ToList();

                                    for (int i = 0; i < plotsWaitingRank.Count; i++)
                                    {
                                        if (plotsWaitingRank[i].driverno.ToStr() == driverNo && status.ToLower() == "available")
                                        {
                                            rank = (i + 1).ToStr();

                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    rank = "-";
                                }
                            }
                            else
                            {
                                for (int i = 0; i < plots.Count; i++)
                                {
                                    if (plots[i].driverno.ToStr() == driverNo && status.ToLower() == "available")
                                    {
                                        rank = (i + 1).ToStr();
                                        break;
                                    }
                                    else
                                    {
                                        rank = "-";
                                    }
                                }
                            }

                            if (Instance.objPolicy.DriverRankType.ToInt() == 2) // show plot and overall waiting rank X/Y
                            {

                                var plotsWaitingRank = list.Where(c => c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                        .OrderBy(c => c.WaitSinceOn.ToDateTime()).ToList();

                                for (int i = 0; i < plotsWaitingRank.Count; i++)
                                {
                                    if (plotsWaitingRank[i].driverno.ToStr() == driverNo && status.ToLower() == "available")
                                    {
                                        rank = rank + "/" + (i + 1).ToStr();
                                        break;
                                    }
                                }
                            }

                            if (Instance.objPolicy.DriverRankType.ToInt() == 3) // show driver current plot rank and how many available driver of driver current plot
                            {
                                for (int i = 0; i < plots.Count; i++)
                                {
                                    if (plots[i].driverno.ToStr() == driverNo && (status.ToLower() == "available" || status.ToLower() == "onbreak"))
                                    {
                                        rank = (i + 1).ToStr() + "/" + plots.Count;
                                        break;
                                    }
                                    else
                                    {
                                        rank = "-" + "/" + plots.Count;
                                    }
                                }
                            }

                            if (plots.Count == 0 && Instance.objPolicy.DriverRankType.ToInt() != 1)
                            {
                                rank = "-";
                            }
                        }



                        if (plotName.ToStr().Trim().Length == 0)
                        {

                            rank = "-";
                        }



                        response = plotName;
                        response += ">>" + (rank).ToStr();

                        if (string.IsNullOrEmpty(status))
                            status = "Available";

                        response += ">>" + status + ">>";

                        response += "<<" + "http://www.eurosoft-download.co.uk/taxi/cab.apk," + Instance.objPolicy.NewPDAVersionAvailable.ToDecimal();
                    }
                    else
                    {
                        if (Instance.objPolicy.AutoLogoutInActiveDrvMins.ToInt() > 0)
                        {
                            int drvId = values[1].ToInt();

                            if (Instance.listofJobs.Count(c => c.DriverId == drvId && c.JobMessage == "force logout") == 0)
                            {
                                if (db.Fleet_DriverQueueLists.Where(c => c.DriverId == drvId && c.Status == true).Count() == 0)
                                {
                                    db.stp_LoginLogoutDriver(drvId, true, null);
                                    General.BroadCastMessage("**login>>Drv " + driverNo + " is Login" + ">>" + driverNo);
                                }
                            }
                        }
                    }

                    //send message back to PDA
                    Clients.Caller.requestZone(response);


                    //try
                    //{

                    //    File.AppendAllText(AppContext.BaseDirectory + "\\requestzoneupdate.txt", DateTime.Now.ToStr() + ": request =" + mesg + " , response = " + response + Environment.NewLine);
                    //}
                    //catch (Exception ex)
                    //{



                    //}

                    //Byte[] byteResponse = Encoding.UTF8.GetBytes(response);
                    //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);

                    //try
                    //{
                    //    File.AppendAllText("requestzoneandupdate.txt", DateTime.Now.ToStr() +":"+dataValue + Environment.NewLine);
                    //}
                    //catch
                    //{

                    //}
                }
            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText("exception_requestzone.txt", DateTime.Now.ToStr() + ":" + dataValue + ":" + ex.Message + Environment.NewLine);
                    Clients.Caller.requestZone(ex.Message);
                }
                catch (Exception e)
                {
                    Clients.Caller.requestZone(e.Message);
                }
            }
        }

        public void fojActionButton(string mesg)
        {
            try
            {
                byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                string IsAvalable = "false";


                if (new TaxiDataContext().Bookings.Count(c => c.Id == values[1].ToLong() && c.DriverId == values[2].ToInt()) > 0)
                    IsAvalable = "true";

                //send message back to PDA
                if (mesg.EndsWith("=16"))
                    Clients.Caller.fojJobAccepted(IsAvalable);
                else
                    Clients.Caller.fojJobRejected(IsAvalable);

              

                int jobstatusId = values[3].ToInt();

                if (IsAvalable == "true")
                {
                    new TaxiDataContext().stp_UpdateJobStatus(values[1].ToLong(), jobstatusId);

                    General.BroadCastMessage("**action>>" + values[1].ToStr() + ">>" + values[2].ToStr() + ">>" + values[3].ToInt());

                    if (Instance.listofJobs.Count(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong()) > 0)
                    {
                        Instance.listofJobs.RemoveAll(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong());
                    }
                }
            }
            catch (Exception ex)
            {
                //send message back to PDA
                if (mesg.EndsWith("=16"))
                    Clients.Caller.fojJobAccepted(ex.Message);
                else
                    Clients.Caller.fojJobRejected(ex.Message);
            }
        }

        public void fojJobStart(string mesg)
        {
            try
            {
                byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                int valCnt = values.Count();

                long jobId = values[1].ToLong();
                int driverId = values[2].ToInt();

                string IsAvalable = "false";

                if (new TaxiDataContext().Bookings.Count(c => c.Id == jobId && c.DriverId == driverId &&
                    (c.BookingStatusId == Enums.BOOKINGSTATUS.FOJ || c.BookingStatusId == Enums.BOOKINGSTATUS.DISPATCHED || c.BookingStatusId == Enums.BOOKINGSTATUS.CANCELLED)) > 0)
                    IsAvalable = "true";

                //send message back to PDA
                Clients.Caller.fojJobStarted(IsAvalable);

                //Byte[] byteResponse = Encoding.UTF8.GetBytes(IsAvalable);
                //tcpClient.NoDelay = true;
                //tcpClient.SendTimeout = 5000;
                //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);

                int jobstatusId = values[2].ToInt();

                if (IsAvalable == "true")
                {
                    if (dataValue.Contains("iphone") || new TaxiDataContext().Fleet_Driver_PDASettings.Count(c => c.DriverId == driverId &&
                        (c.CurrentPdaVersion < 19m || c.CurrentPdaVersion == 19.9m)) > 0)
                    {
                        new TaxiDataContext().stp_UpdateJobStatus(values[1].ToLong(), Enums.BOOKINGSTATUS.ONROUTE);
                        string alterProcedureScript = @"
                        ALTER PROCEDURE [dbo].[stp_UpdateJob]                                                                                        
                                                
(                                                                                        
                                               
  @jobId as bigint,                                                                                        
                                                
  @DriverId as int,                                                                                        
                                                
  @JobStatusId as int,                                                                                        
                                                
  @DriverWorkStatusId int,                                                                                        
                                                
  @SinBinTimer int                                                                                        
                                                
)                                                                                        
                                                
                                                
AS                                                                                        
                                               
SET NOCOUNT ON                                                                                        
                                                
Begin                                                                                        
                                                
      Declare @DestZoneId int                                                                                        
                                                
      Declare @DriverCurrentJobId bigint                                                                                        
                                                
      DECLARE @BookingTypeId int                                                                                        
                                                
      declare @sinbinMins int                                                                                       
                                                
                                                
 if( (@JobStatusId=10 or @JobStatusId=11 or @JobStatusId=12))                                                                                       
    select @sinbinMins=SinBinMinutes from Gen_SysPolicy_SinBinSettings where sinbintypeid=@JobStatusId                                                                                        
                                                
                                                
         if(@JobStatusId=10 or @JobStatusId=11 or @JobStatusId=12 or @JobStatusId=13 or @jobstatusid=2)                                                                                        
                                                
                                                
            set @DriverCurrentJobId=NULL                                                                                        
                                                
                                                
        else                                                                                        
                                                
                                                
       set @DriverCurrentJobId=@JobId                                                                                        
                                                
       if(@JobStatusId=5 or @JobStatusId=11)                                                                                        
                
       begin               
                                                
                                                
        declare @fleetMasterId int               
  --set @fleetMasterId=null                                                          
                     declare @currstatusid int                             
                                                
            select @fleetMasterId=fleetMasterId,@currstatusid=DriverWorkStatusId from fleet_DriverQueueList where driverId=@driverId and status=1                                                                                    
                                                
                                   
            Declare @AcceptedDateTime DateTime                                  
                                                
                                                
            if(@JobStatusId=5)                                          
                                                
              SET  @AccepteddateTime=getdate()                                                           
                                                 
              DECLARE @pickup varchar(200)                                                              
                                                
              DECLARE @Destination varchar(200)                                                             
                                                
              if(@JobStatusId=5)                               
                                                
              begin                                                           
                                                
                                                
    Update booking set BookingStatusId=@JobStatusId,FleetMasterId=@fleetMasterId,AcceptedDateTime=@AcceptedDateTime ,DriverId=@DriverId                                                                                        
                                                
                                                
    where id=@jobId                                                                                        
                                                
                            
                             if(@currstatusid=7)                              
       Update fleet_driver_location set plotdate=getdate(),ZoneId=null,PrevZoneId=null,NewZoneName='',PreviousZone='', PickupPoint='',SinBinTillOn=null ,Destination='',disableautoplotting=0,LastActiveZoneName='' where driverid=@driverId                        
                             
                  else                            
                        Update fleet_driver_location set plotdate=getdate(),PickupPoint='',Destination='',disableautoplotting=0,LastActiveZoneName='' where driverid=@driverId                                                                   
                                   
               end                                                                    
                                                
               else                                                                                  
                                                
               begin                                                                                  
                                                
      Update booking set BookingStatusId=@JobStatusId,FleetMasterId=@fleetMasterId,AcceptedDateTime=@AcceptedDateTime,IsBidding=1,AutoDespatch=1                                                                                          
                   ,@pickup=FromAddress,@Destination=ToAddress ,IsConfirmedDriver=0                                                                                  
  
     where id=@jobId and DriverId=@DriverId                                                                          
                  
                            
         Update fleet_driver_location set plotdate=getdate(),PickupPoint='',Destination='',disableautoplotting=0 where driverid=@driverId                                                                   
                                   
               end                                                                                  
                    
                                                
            --  Update fleet_driver_location set plotdate=getdate(),PickupPoint='',Destination='',disableautoplotting=0 where driverid=@driverId                                      
                                                
                                                
    if(@JobStatusId=5)                                                                                      
                     
             begin                                                                           
                                                
                                                
      declare @drvNo varchar(100)                          
                                                
                                                
                                                
     select @drvNo=DriverNo from Fleet_Driver where id=@driverId                     
                                           
     insert into Booking_Log values(@jobid,'','','Job accepted by Driver ('+ISNULL(@drvNo,'')+')',getdate(),NULL)                       
                             
             end                                                                                                         
       END                                                                                        
                                                
                                                
       ELSE                                                                         
                                                
       begin                                                                                        
                                       
                                                
      if(@JobStatusId=6)  -- Arrive                                                                                        
                                           
            BEGIN                                                                                                                  
               Update booking set BookingStatusId=@JobStatusId,ArrivalDateTime=getdate()                                                                                        
                         
                     where  id=@jobId and driverid=@driverid                                                                                      
                                
            END                                                                                                                            
                                                
    else  if(@JobStatusId=7)    -- POB                                                                                                                   
     BEGIN                                                                                                                          
                                                
     declare @journeytypeId int                                                      
                declare @PickupZoneId int                                                      
                                                                      
                                          
               Update booking set BookingStatusId=@JobStatusId,POBDateTime=getdate(),@DestZoneId=dropoffzoneid ,@journeytypeId=journeytypeid, @PickupZoneId=zoneid  where  id=@jobId and driverid=@driverid                                                                              
                              if(@journeytypeId=3)                                             
                         begin                                                          
                                                                                   
                            if(@PickupZoneId is not NULL)                                  
       begin                               
       if exists(select * from gen_zones where id=@PickupZoneId and (DisableDriverRank is null or DisableDriverRank=0))                                
        begin                                
                                                       
          Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@PickupZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@PickupZoneId,previouszone='',newzonename='' where driverid= @driverId                    
                                             
                              end                                
         else                                
      
         begin                                
          select @PickupZoneId=BackupZone1Id from Gen_Zone_Backups where zoneid=@PickupZoneId                                
                                
               if(@PickupZoneId is not null and @PickupZoneId>0)                                
          Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@PickupZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@PickupZoneId,previouszone='',newzonename='' where driverid= @driverId              
           
         end                                
                       
       end                                
                                                                                   
                         end                                                          
                         else                                                          
                         begin                                                          
                                                                          
                          if(@DestZoneId is not NULL)                                 
        begin                                
       if exists(select * from gen_zones where id=@DestZoneId and (DisableDriverRank is null or DisableDriverRank=0))                                
                  begin                                
                      --  select @DestZoneId=BackupZone1Id from Gen_Zone_Backups where zoneid=@DestZoneId                                
                                
              -- if(@DestZoneId is not null and @DestZoneId>0)                                
            Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@DestZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@DestZoneId,previouszone='',newzonename='' where driverid=@driverId                 
                                          
                end                                 
          else                                
                                
   begin                                
                    select @DestZoneId=BackupZone1Id from Gen_Zone_Backups where zoneid=@DestZoneId                                
                                
               if(@DestZoneId is not null and @DestZoneId>0)                             
            Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@DestZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@DestZoneId,previouszone='',newzonename='' where driverid=@driverId                 
                  
          end                                
                                 
       end                                          
                         end                                
                                  
                                           
                                                
 --                                       if(@DestZoneId is not NULL)                                             
                                                
  --  Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@DestZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@DestZoneId,PrevZoneId=@DestZoneId,previouszone='',newzonename='' where driverid=@driverId
                                         
    END                                                                                    
                                   
                                                
   else  if(@JobStatusId=8)  -- STC                                                                                        
                                    
            BEGIN                                                                                        
                               
               Update booking set BookingStatusId=@JobStatusId,STCDateTime=getdate()             
                         
                     where  id=@jobId and driverid=@driverid                                                                                 
            END                                                                                        
                                  
            else  if(@JobStatusId=2)  --  DISPATCHED OR COMPLETED                                                                                        
                     
            BEGIN                                                                                        
                              
               Update booking set BookingStatusId=@JobStatusId,ClearedDateTime=getdate()                                    
                         
                     where  id=@jobId and driverid=@driverid                                                                                        
                                   
            END                                                                                        
                                
             else  if(@JobStatusId=3 or @JobStatusId=13 ) -- Cancel                                                                                        
                                                
            BEGIN                                                                                        
                                                
               Update booking set BookingStatusId=@JobStatusId                                   
                 
                                                
                     where  id=@jobId                                                    
                                                
                                                
                                                
            END                                                                                        
                                                
            else  if(@JobStatusId=10) -- No Show                                                                                        
                                                
                                                
                                               
     BEGIN                                                                                        
                                                
                      
               Update booking set BookingStatusId=@JobStatusId,ArrivalDateTime=getdate(),AutoDespatch=0,IsBidding=0                                                         
                                                
                                                
                     where  id=@jobId and driverid=@driverid                                                                                        
                                                
                                                
            END                                                                                        
                                                                                 
    else  if(@JobStatusId=1) -- Waiting                                 
                                                
                                                
            BEGIN                                                                                        
                 
               Update booking set BookingStatusId=@JobStatusId,ArrivalDateTime=NULL ,IsConfirmedDriver=0,AutoDespatch=0,IsBidding=0   where  id=@jobId                                                                                       
                                                
                                                
                                 
                     --     ,DriverId=NULL                                                                                        

                   --  where  id=@jobId                                                                        
 
                         INSERT INTO dbo.Fleet_Driver_RejectJobs (DriverId,BookingId,RejectedDateTime,BookingStatusId)                                                                                                    
               values (@DriverId,@JobId,getdate(),10)                                                  
 
            END                                                                                        
  
         else  if(@JobStatusId=12) -- Not Accepted                                                                                        
                                                 
                                     
begin                                                               
                                          
    Update booking set BookingStatusId=@JobStatusId                                                             
                                     
                     where  id=@jobId                                                                                        
                                                 
    END                                                                                        
                                         
       END";

                        (new TaxiDataContext()).ExecuteCommand(alterProcedureScript);
                        (new TaxiDataContext()).stp_UpdateJob(jobId, driverId, Enums.BOOKINGSTATUS.ONROUTE, Enums.Driver_WORKINGSTATUS.ONROUTE, Instance.objPolicy.SinBinTimer.ToInt());
                    }

                    General.BroadCastMessage("**action>>" + values[1].ToStr() + ">>" + values[2].ToStr() + ">>" + Enums.BOOKINGSTATUS.ONROUTE);
                }
            }
            catch (Exception ex)
            {
                Clients.Caller.fojJobStarted(ex.Message);
            }
        }

        public void preJobActionButton(string mesg)
        {
            try
            {
                byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                int valCnt = values.Count();
                string response = "true";

                if (new TaxiDataContext().Bookings.Count(c => c.Id == values[1].ToLong() && c.DriverId == values[2].ToInt()) == 0)
                    response = "false";

                //send message back to PDA
                if (mesg.EndsWith("=17"))
                    Clients.Caller.preJobAccepted(response);
                else
                    Clients.Caller.preJobRejected(response);

                //Byte[] byteResponse = Encoding.UTF8.GetBytes(response);
                //tcpClient.NoDelay = true;
                //tcpClient.SendTimeout = 5000;

                //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);

                long jobId = values[1].ToLong();
                int driverId = values[2].ToInt();
                int jobStatusId = values[3].ToInt();

                if (response == "true")
                {
                    new TaxiDataContext().stp_UpdateFutureJob(jobId, driverId, jobStatusId, null);

                    General.BroadCastMessage("**prejob action>>" + values[1].ToStr() + ">>" + values[2].ToStr() + ">>" + values[3].ToInt());

                    if (Instance.listofJobs.Count(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong()) > 0)
                    {
                        Instance.listofJobs.RemoveAll(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong());
                        //Instance.listofJobs.LastOrDefault(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong() && c.MessageTypeId==eMessageTypes.JOB).IsAccepted=true;
                    }
                }
            }
            catch (Exception ex)
            {
                if (mesg.EndsWith("=17"))
                    Clients.Caller.preJobAccepted(ex.Message);
                else
                    Clients.Caller.preJobRejected(ex.Message);
            }
        }

        public void preJobStart(string mesg)
        {
            try
            {
                byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                int valCnt = values.Count();

                long jobId = values[1].ToLong();
                int driverId = values[2].ToInt();

                string IsAvalable = "false";

                if ((new TaxiDataContext()).Bookings.Count(c => c.Id == jobId && c.DriverId == driverId && c.BookingStatusId == Enums.BOOKINGSTATUS.PENDING_START) > 0)
                    IsAvalable = "true";

                //send message back to PDA
                Clients.Caller.preJobStarted(IsAvalable);

                //Byte[] byteResponse = Encoding.UTF8.GetBytes(IsAvalable);
                //tcpClient.NoDelay = true;
                //tcpClient.SendTimeout = 5000;
                //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);

                int jobstatusId = values[2].ToInt();

                if (IsAvalable == "true")
                {
                    //new TaxiDataContext().stp_UpdateJobStatus(values[1].ToLong(), Enums.BOOKINGSTATUS.ONROUTE);
                    General.BroadCastMessage("**action>>" + values[1].ToStr() + ">>" + values[2].ToStr() + ">>" + Enums.BOOKINGSTATUS.ONROUTE);

                    if (dataValue.Contains("iphone") || ((new TaxiDataContext()).Fleet_Driver_PDASettings.Count(c => c.DriverId == driverId && c.CurrentPdaVersion < 19m) > 0))
                    {
                        (new TaxiDataContext()).stp_UpdateFutureJob(jobId, driverId, Enums.BOOKINGSTATUS.ONROUTE, null);
                    }
                }
            }
            catch (Exception ex)
            {
                Clients.Caller.preJobStarted(ex.Message);
            }
        }

        public void requestFlagDown(string mesg)
        {
            byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

            string dataValue = mesg;
            dataValue = dataValue.Trim();

            string[] values = dataValue.Split(new char[] { '=' });

            using (TaxiDataContext db = new TaxiDataContext())
            {
                try
                {

                    db.DeferredLoadingEnabled = false;

                    string vehicleName = values[7].ToStr().ToLower().Trim();
                    var res = db.stp_InsertOnRoadJob(values[1].ToStr(), values[2].ToStr(), values[3].ToDecimal(), values[4].ToStr()
                                    , values[5].ToStr(), values[6].ToInt(), vehicleName);

                    long jobId = 0;

                    if (res != null)
                        jobId = res.FirstOrDefault().jobid.ToLong();

                    //  //string respo=jobId.ToStr()+",1,0.5,2";
                    string isMeter = "0";



                    string fareJson = string.Empty;

                    string respo = jobId.ToStr();
                    if (Instance.objPolicy.FareMeterType == null)
                    {
                        respo += ",0,0,1";
                    }
                    else
                    {
                        //respo += "," + Instance.objPolicy.FareMeterRoundedCalc.ToDecimal() + "," + meterType;


                        int? vehicleTypeId = 0;




                        //vehicleTypeId = db.Fleet_VehicleTypes.Where(c => c.VehicleType.ToLower() == vehicleName).Select(c => c.Id).FirstOrDefault();
                        //if (vehicleTypeId == null)
                        //    vehicleTypeId = 0;

                         InitializeMeterList();
                        bool enableFareMeter = Global.listofMeter.FirstOrDefault(c => c.VehicleTypeId == vehicleTypeId).DefaultIfEmpty().HasMeter.ToBool();
                        enableFareMeter = false;
                        if (enableFareMeter && Global.listofMeter != null && Global.listofMeter.Count > 0)
                        {


                            if (enableFareMeter)
                            {
                                isMeter = "1";




                                var obj = Global.listofMeter.FirstOrDefault(c => c.VehicleTypeId == vehicleTypeId).DefaultIfEmpty();


                                FareMeterSettings fareJsonArr = null;

                                if (Instance.objPolicy.PDANewWeekMessageByDay.ToStr().StartsWith("{"))
                                    fareJsonArr = new JavaScriptSerializer().Deserialize<FareMeterSettings>(Instance.objPolicy.PDANewWeekMessageByDay.ToStr());
                                else
                                    fareJsonArr = new FareMeterSettings(true);

                                int? fareId = db.ExecuteQuery<int?>("exec stp_GetFareId {0},{1},{2},{3},{4}", vehicleTypeId, 0, 0.00m, DateTime.Now, 1).FirstOrDefault();


                                var jobTariff = (from f in db.Fares
                                                 join c in db.Fare_OtherCharges on f.Id equals c.FareId
                                                 where f.Id == fareId
                                                 select new
                                                 {
                                                     f.StartRate,
                                                     f.StartRateValidMiles,
                                                     f.FromDateTime,
                                                     f.TillDateTime,


                                                     c.FromMile,
                                                     c.ToMile,
                                                     c.Rate

                                                 }).ToList();


                                fareJsonArr.meterTarrif = new List<MeterTarrif>();

                                decimal roundJourneyMile = Instance.objPolicy.RoundJourneyMiles.ToDecimal();
                                foreach (var item in jobTariff)
                                {
                                    fareJsonArr.meterTarrif.Add(new MeterTarrif
                                    {
                                        StartRate = item.StartRate,
                                        StartRateValidMiles = item.StartRateValidMiles,
                                        FromMile = item.FromMile,
                                        TillMile = item.ToMile,
                                        Rate = item.Rate,

                                        AutoStartWaiting = obj.AutoStartWaiting == true ? 1 : 0,
                                        AutoStartWaitingBelowSpeed = obj.AutoStartWaitingBelowSpeed,
                                        AutoStartWaitingBelowSpeedSeconds = obj.AutoStartWaitingBelowSpeedSeconds,

                                        AutoStopWaitingOnSpeed = obj.AutoStopWaitingOnSpeed.ToInt(),
                                        DrvWaitingChargesPerMin = obj.DrvWaitingChargesPerMin,
                                        FullRoundFares = Instance.objPolicy.RoundMileageFares.ToBool() ? 1 : 0,
                                        RoundUpTo = Instance.objPolicy.RoundUpTo.ToDecimal(),
                                        WaitingSecondsToDivide = obj.AccWaitingChargesPerMin.ToInt(),
                                        RoundJourneyMiles = roundJourneyMile
                                    });

                                }

                                if (fareJsonArr.meterTarrif.Count > 0)
                                {

                                    fareJsonArr.meterTarrif.Insert(0, new MeterTarrif
                                    {

                                        StartRate = fareJsonArr.meterTarrif[0].StartRate,
                                        StartRateValidMiles = fareJsonArr.meterTarrif[0].StartRateValidMiles,
                                        FromMile = 0,
                                        TillMile = fareJsonArr.meterTarrif[0].StartRateValidMiles,
                                        Rate = fareJsonArr.meterTarrif[0].StartRate,

                                        AutoStartWaiting = obj.AutoStartWaiting == true ? 1 : 0,
                                        AutoStartWaitingBelowSpeed = obj.AutoStartWaitingBelowSpeed,
                                        AutoStartWaitingBelowSpeedSeconds = obj.AutoStartWaitingBelowSpeedSeconds,

                                        AutoStopWaitingOnSpeed = obj.AutoStopWaitingOnSpeed.ToInt(),
                                        DrvWaitingChargesPerMin = obj.DrvWaitingChargesPerMin,
                                        FullRoundFares = Instance.objPolicy.RoundMileageFares.ToBool() ? 1 : 0,
                                        RoundUpTo = Instance.objPolicy.RoundUpTo.ToDecimal(),
                                        WaitingSecondsToDivide = obj.AccWaitingChargesPerMin.ToInt(),
                                        RoundJourneyMiles = roundJourneyMile

                                    });
                                }


                                fareJson = ",jsonstring|" + new JavaScriptSerializer().Serialize(fareJsonArr).Replace(",", "|");
                                //
                            }
                        }



                        respo += "," + isMeter;

                        int meterType = Instance.objPolicy.FareMeterType.ToInt();

                        respo += "," + Instance.objPolicy.FareMeterRoundedCalc.ToDecimal() + "," + meterType;

                        if (fareJson.ToStr().Length > 0)
                            respo += fareJson;


                        try
                        {
                            File.AppendAllText(physicalPath + "\\requestFlagDown.txt", DateTime.Now.ToStr() + " response: " + respo + Environment.NewLine);
                        }
                        catch
                        {

                        }












                        //respo += ",1,0.5,2";
                    }

                    //send message back to PDA
                    Clients.Caller.flagDown(respo);

                    //Byte[] byteResponse = Encoding.UTF8.GetBytes(respo);
                    //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);

                    General.BroadCastMessage("**action>>" + jobId + ">>" + values[6].ToStr() + ">>" + Enums.BOOKINGSTATUS.POB);
                }
                catch (Exception ex)
                {
                    Clients.Caller.flagDown(ex.Message);
                }
            }
        }

        public void requestAuthorizationLogout(string mesg)
        {
            try
            {
                byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                Clients.Caller.authorizationLogout("true");

                //Byte[] byteResponse = Encoding.UTF8.GetBytes("true");
                //tcpClient.NoDelay = true;
                //tcpClient.SendTimeout = 5000;

                //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);

                General.BroadCastMessage("**logout auth>>" + values[1].ToStr() + ">>" + values[2].ToStr());
            }
            catch (Exception ex)
            {
                Clients.Caller.authorizationLogout(ex.Message);
            }
        }

        public void requestNoPickup(string mesg)
        {
            try
            {
                byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                //send message back to PDA

                if (mesg.Contains("jsonstring|"))
                {
                    long jobId = values[1].ToLong();
                    DateTime? arrivalDateTime = null;
                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        arrivalDateTime = db.Bookings.Where(c => c.Id == jobId).Select(c => c.ArrivalDateTime).FirstOrDefault();
                        //

                    }


                    if (arrivalDateTime != null)
                    {
                        double timeString = DateTime.Now.Subtract(arrivalDateTime.Value).TotalMinutes;


                        int restrictionMins = Global.NoPickupRestrictionMins.ToInt();

                        if (timeString < restrictionMins)
                        {
                            timeString = restrictionMins - timeString;
                            Clients.Caller.noPickupAuth("false:You can press No Pickup after " + timeString.ToInt() + "min");
                            return;
                        }
                        else
                        {
                            Clients.Caller.noPickupAuth("true");
                        }
                    }
                    else
                    {
                        Clients.Caller.noPickupAuth("true");

                    }

                }
                else
                {
                    Clients.Caller.noPickupAuth("true");
                }




                General.BroadCastMessage("**auth>>" + values[1].ToStr() + ">>" + values[2].ToStr() + ">>" + values[3].ToInt() + ">>" + values[4].ToInt());

                if (Instance.listofJobs.Count(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong()) > 0)
                {
                    Instance.listofJobs.RemoveAll(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong());
                    //Instance.listofJobs.LastOrDefault(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong() && c.MessageTypeId==eMessageTypes.JOB).IsAccepted=true;
                }
            }
            catch (Exception ex)
            {
                Clients.Caller.noPickupAuth(ex.Message);
            }
        }

        public void requestRecoverJob(string mesg)
        {
            try
            {
                byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                //send message back to PDA
                Clients.Caller.recoverJob("true");

                //Byte[] byteResponse = Encoding.UTF8.GetBytes("true");
                //tcpClient.NoDelay = true;
                //tcpClient.SendTimeout = 5000;

                //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);

                General.BroadCastMessage("**auth>>" + values[1].ToStr() + ">>" + values[2].ToStr() + ">>" + values[3].ToInt() + ">>" + values[4].ToInt());

                if (Instance.listofJobs.Count(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong()) > 0)
                {
                    Instance.listofJobs.RemoveAll(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong());
                    //Instance.listofJobs.LastOrDefault(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong() && c.MessageTypeId==eMessageTypes.JOB).IsAccepted=true;
                }
            }
            catch (Exception ex)
            {
                Clients.Caller.recoverJob(ex.Message);
            }
        }

        public void requestRejectJobAuth(string mesg)
        {
            try
            {
                byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                //send message back to PDA
                Clients.Caller.RejectJobAuth("true");

                //Byte[] byteResponse = Encoding.UTF8.GetBytes("true");
                //tcpClient.NoDelay = true;
                //tcpClient.SendTimeout = 5000;

                //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);

                General.BroadCastMessage("**auth>>" + values[1].ToStr() + ">>" + values[2].ToStr() + ">>" + values[3].ToInt() + ">>" + values[4].ToInt());

                if (Instance.listofJobs.Count(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong()) > 0)
                {
                    Instance.listofJobs.RemoveAll(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong());
                    //Instance.listofJobs.LastOrDefault(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong() && c.MessageTypeId==eMessageTypes.JOB).IsAccepted=true;
                }
            }
            catch (Exception ex)
            {
                Clients.Caller.RejectJobAuth(ex.Message);
            }
        }

        public void requestChangeDestination(string mesg)
        {
            try
            {
                byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                string plotName = "unknown";

                int? zoneId = null;

                GetZone(values[5].ToStr().ToUpper().Trim(), ref zoneId, ref plotName);

                // if(zoneId!=null)
                //{
                new TaxiDataContext().stp_UpdateJobAddress(values[1].ToLong(), values[2].ToInt(), values[3].ToStr().Trim(), zoneId, values[5].ToStr().ToUpper(), values[4].ToStr().ToUpper());
                //}           

                //send message back to PDA
                Clients.Caller.changeDestination(plotName);

                //Byte[] byteResponse = Encoding.UTF8.GetBytes(plotName);
                //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);

                General.BroadCastMessage("**changeaddress>>" + values[2].ToInt() + ">>" + values[1].ToLong() + ">>" + values[3].ToStr() + ">>" + values[5].ToStr());
            }
            catch (Exception ex)
            {
                Clients.Caller.changeDestination(ex.Message);
            }
        }

        public void requestChangePlot(string mesg)
        {
            try
            {
                try
                {


                    File.AppendAllText(physicalPath + "\\requestChangePlot.txt", DateTime.Now + Environment.NewLine);
                }
                catch
                { }



                string[] arr = Instance.listOfZone.OrderBy(c=>c.OrderNo).Select(args => args.Id + "," + args.Area).ToArray<string>();

                //send message back to PDA
                Clients.Caller.changePlot(string.Join(">>", arr));

                //Byte[] byteResponse = Encoding.UTF8.GetBytes(string.Join(">>", arr));
                //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);
            }
            catch (Exception ex)
            {
                try
                {

                    Clients.Caller.changePlot("exceptionoccurred");
                    File.AppendAllText(physicalPath + "\\requestChangePlot_exception.txt", DateTime.Now + Environment.NewLine);
                }
                catch
                { }

            }
        }

        public void requestNavigation(string mesg)
        {
            try
            {
                byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                string address = values[1].ToStr();
                string response = string.Empty;

                address = address.Replace("+", " ").Trim();

                stp_getCoordinatesByAddressResult result = null;
                using (TaxiDataContext db = new TaxiDataContext())
                {
                    result = db.stp_getCoordinatesByAddress(address, General.GetPostCodeMatch(address.ToStr().ToUpper().Trim())).FirstOrDefault();
                }

                if (result != null && result.Latitude != null && result.Latitude > 0)
                {
                    response = "{ \"Address\" :\"" + address +
                            "\", \"Latitude\":\"" + result.Latitude +
                            "\", \"Longitude\":\"" + result.Longtiude + "\"  }";
                }
                else
                {
                    response = "{ \"Address\" :\"" + address +
                            "\", \"Latitude\":\"" + "0" +
                            "\", \"Longitude\":\"" + "0" + "\"  }";
                }

                //try
                //{

                //    File.AppendAllText(Application.StartupPath+"\\navigate.txt",DateTime.Now.ToStr()+" response" +response+" datavalue"+dataValue+Environment.NewLine);
                //}
                //catch
                //{

                //}

                //send message back to PDA
                Clients.Caller.navigation(response);

                //Byte[] byteResponse = Encoding.UTF8.GetBytes(response);
                //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);
            }
            catch (Exception ex)
            {
                Clients.Caller.navigation(ex.Message);
            }
        }



        //public void requestSelectAsDirected(string mesg)
        //{
        //    try
        //    {




        //        string dataValue = mesg;
        //        dataValue = dataValue.Trim();

        //        string[] values = dataValue.Split(new char[] { '=' });

        //        int? driverId = values[1].ToIntorNull();
        //        long? jobId = values[2].ToLongorNull();

        //        int? DropOffZoneId = values[3].ToIntorNull();
        //        string DropOffZoneName = values[4].ToStr();


        //        new TaxiDataContext().stp_UpdateJobAddress(jobId, driverId, null, DropOffZoneId, DropOffZoneName, null);


        //        if (dataValue.Contains("jsonstring|"))
        //        {

        //            try
        //            {
        //                ClsChangePlot obj = new JavaScriptSerializer().Deserialize<ClsChangePlot>(values[5].Replace("jsonstring|", "").Trim());


        //                using (TaxiDataContext db = new TaxiDataContext())
        //                {

        //                    ClsGen_SysPolicy_SurchargeRates objSurcharge = db.ExecuteQuery<ClsGen_SysPolicy_SurchargeRates>("select Id,Parking,Waiting from Gen_SysPolicy_SurchargeRates where syspolicyid is not null and (ApplyOutofTown is not null and ApplyOutofTown=1) and zoneid=" + DropOffZoneId).FirstOrDefault();


        //                    if (objSurcharge != null)
        //                    {

        //                        if (objSurcharge.Parking.ToDecimal() > 0)
        //                        {
        //                            obj.ParkingCharges = objSurcharge.Parking.ToStr();

        //                            obj.IsUpdateParkingCharge = 1;
        //                        }

        //                        if (objSurcharge.Waiting.ToDecimal() > 0)
        //                        {
        //                            obj.ExtraDropCharges = objSurcharge.Waiting.ToStr();

        //                            obj.IsUpdateExtraCharges = 1;
        //                        }

        //                    }

        //                }




        //                Clients.Caller.selectAsDirected("success:" + new JavaScriptSerializer().Serialize(obj));
        //            }
        //            catch
        //            {
        //                Clients.Caller.selectAsDirected("true");

        //            }
        //        }

        //        else
        //        {
        //            //send message back to PDA
        //            Clients.Caller.selectAsDirected("true");
        //        }


        //        General.BroadCastMessage("**dropoffzone>>" + driverId + ">>" + jobId + ">>" + DropOffZoneId + ">>" + DropOffZoneName);

        //        try
        //        {

        //            //
        //            File.AppendAllText(physicalPath + "\\requestselectasdirected.txt", DateTime.Now.ToStr() + " , datavalue=" + dataValue + Environment.NewLine);

        //        }

        //        catch
        //        {

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        try
        //        {
        //            Clients.Caller.selectAsDirected("exceptionoccurred");

        //            File.AppendAllText(physicalPath + "\\requestselectasdirected_exception.txt", DateTime.Now.ToStr() + " , datavalue=" + mesg+",exception="+ex.Message + Environment.NewLine);

        //        }

        //        catch
        //        {

        //        }

        //    }
        //}



        public void requestSelectAsDirected(string mesg)
        {
            try
            {


                try
                {

                    File.AppendAllText(physicalPath + "\\" + "requestSelectAsDirected.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + mesg + Environment.NewLine);
                }
                catch
                {


                }

                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                int? driverId = values[1].ToIntorNull();
                long? jobId = values[2].ToLongorNull();

                int? DropOffZoneId = values[3].ToIntorNull();
                string DropOffZoneName = values[4].ToStr();


                new TaxiDataContext().stp_UpdateJobAddress(jobId, driverId, null, DropOffZoneId, DropOffZoneName, null);

                //
                if (dataValue.Contains("jsonstring|"))
                {

                    try
                    {
                        ClsChangePlot obj = new JavaScriptSerializer().Deserialize<ClsChangePlot>(values[5].Replace("jsonstring|", "").Trim());

                        //  string pickupDateTime = obj.PickupDateTime;
                        DateTime pickupDateAndTime = DateTime.Now;

                        if (obj.PickupDateTime.ToStr().Trim().Length > 0)
                        {
                            string pickupDateTime = obj.PickupDateTime.ToStr().Trim().Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Trim();
                            //    DateTime.TryParse("24/09/2021", out pickupDateAndTime);
                            DateTime.TryParseExact(pickupDateTime, "dd/M/yyyy HH:mm",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out pickupDateAndTime);
                        }
                        else
                        {
                            ////
                            pickupDateAndTime = DateTime.Now;
                        }

                        using (TaxiDataContext db = new TaxiDataContext())
                        {
                            //
                            string pickupPlot = obj.Pickup.ToStr().Trim();

                            if (pickupPlot.Contains("\u003c\u003c\u003c"))
                            {
                                pickupPlot = pickupPlot.Substring(pickupPlot.LastIndexOf("\u003c") + 1);


                            }
                            //
                            int? pickupPlotId = null;

                            try
                            {
                                if (pickupPlot.Length > 0)
                                {
                                    pickupPlotId = Instance.listOfZone.FirstOrDefault(c => c.Area == pickupPlot).Id;
                                }
                            }
                            catch
                            {

                            }


                            if (pickupPlotId == null)
                                pickupPlotId = 0;

                            if (obj.ShowParkingCharges.ToStr() == "1")
                            {
                                decimal? surchargeParking = null;
                                string query = string.Empty;

                                try
                                {
                                     query = "select max(Parking) from Gen_SysPolicy_SurchargeRates where syspolicyid is not null and (ApplicableFromDateTime<='" + string.Format("{0:yyyy/MM/dd HH:mm:ss}", pickupDateAndTime) + "' and ApplicableToDateTime>='" + string.Format("{0:yyyy/MM/dd HH:mm:ss}", pickupDateAndTime) + "')  and      zoneid in(" + pickupPlotId.ToStr() + "," + DropOffZoneId.ToStr() + ") and enablesurcharge=1";


                                    surchargeParking = db.ExecuteQuery<decimal?>(query).FirstOrDefault();
                                    File.AppendAllText(physicalPath + "\\" + "requestSelectAsDirectedquery.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + query + Environment.NewLine);


                                }
                                catch(Exception ex)
                                {

                                    try
                                    {
                                        File.AppendAllText(physicalPath + "\\" + "requestSelectAsDirectedquery_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + query +",exce[tion:"+ex.Message+ Environment.NewLine);

                                    }
                                    catch
                                    {

                                    }

                                }
                              


                                if (surchargeParking != null)
                                {


                                    obj.ParkingCharges = surchargeParking.ToDecimal().ToStr();

                                    obj.IsUpdateParkingCharge = 1;

                                    //
                                }
                                else
                                {
                                    obj.ParkingCharges = "0.00";

                                    obj.IsUpdateParkingCharge = 1;

                                }

                            }

                            if (obj.ShowExtraCharges.ToStr() == "1")
                            {
                                int outoftownPlots = db.ExecuteQuery<int>("select count(*) from gen_zones where id in(" + pickupPlotId.ToStr() + "," + DropOffZoneId.ToStr() + ")  and (blockdropoff is not null and blockdropoff=1)").FirstOrDefault();
                                decimal? surchargeExtra = null;


                                if (outoftownPlots == 2 || pickupPlotId == DropOffZoneId)
                                {

                                    surchargeExtra = db.ExecuteQuery<decimal?>("select max(Waiting) from Gen_SysPolicy_SurchargeRates where syspolicyid is not null and (ApplicableFromDateTime<='" + string.Format("{0:yyyy/MM/dd HH:mm:ss}", pickupDateAndTime) + "' and ApplicableToDateTime>='" + string.Format("{0:yyyy/MM/dd HH:mm:ss}", pickupDateAndTime) + "') and (ApplyOutofTown is not null and ApplyOutofTown=1) and zoneid in(" + pickupPlotId.ToStr() + "," + DropOffZoneId.ToStr() + ")  and enablesurcharge=1").FirstOrDefault();
                                }

                                if (surchargeExtra != null)
                                {


                                    obj.ExtraDropCharges = surchargeExtra.ToDecimal().ToStr();

                                    obj.IsUpdateExtraCharges = 1;

                                }
                                else
                                {
                                    obj.ExtraDropCharges = "0.00";

                                    obj.IsUpdateExtraCharges = 1;

                                }


                            }





                        }


                        //
                        //
                        Clients.Caller.selectAsDirected("success:" + new JavaScriptSerializer().Serialize(obj));
                    }
                    catch (Exception ex)
                    {
                        Clients.Caller.selectAsDirected("true");
                        try
                        {
                            Clients.Caller.selectAsDirected("exceptionoccurred");

                            File.AppendAllText(physicalPath + "\\requestselectasdirected_exceptionsurcharge.txt", DateTime.Now.ToStr() + " , datavalue=" + mesg + ",exception=" + ex.Message + Environment.NewLine);

                        }

                        catch
                        {

                        }



                    }
                }

                else
                {
                    //send message back to PDA
                    Clients.Caller.selectAsDirected("true");
                }


                General.BroadCastMessage("**dropoffzone>>" + driverId + ">>" + jobId + ">>" + DropOffZoneId + ">>" + DropOffZoneName);


            }
            catch (Exception ex)
            {
                try
                {
                    Clients.Caller.selectAsDirected("exceptionoccurred");

                    File.AppendAllText(physicalPath + "\\requestselectasdirected_exception.txt", DateTime.Now.ToStr() + " , datavalue=" + mesg + ",exception=" + ex.Message + Environment.NewLine);

                }

                catch
                {

                }

            }
        }


        public void requestDrivers(string mesg)
        {
            try
            {
                string[] arr = new TaxiDataContext().Fleet_Drivers.Where(c => c.HasPDA == true && c.IsActive == true).Select(args => args.Id + "," + args.DriverNo + "," + args.DriverName + "," + args.Fleet_VehicleType.VehicleType.ToUpper()).ToArray<string>();

                //send message back to PDA
                Clients.Caller.drivers(string.Join(">>", arr));

                //Byte[] byteResponse = Encoding.UTF8.GetBytes(string.Join(">>", arr));
                //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);                
            }
            catch (Exception ex)
            {
                Clients.Caller.drivers(ex.Message);
            }
        }

        public void requestVehicles(string mesg)
        {
            try
            {
                string[] arr = new TaxiDataContext().Fleet_Masters.Select(args => args.VehicleID).ToArray<string>();

                //send message back to PDA
                Clients.Caller.vehicles(string.Join(">>", arr));

                //Byte[] byteResponse = Encoding.UTF8.GetBytes(string.Join(">>", arr));
                //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);
            }
            catch (Exception ex)
            {
                Clients.Caller.vehicles(ex.Message);
            }
        }


        private string GetDriverPay(int driverId, Fleet_Driver objDriver,string version="")
        {
            ClsDriverPay objAction = new ClsDriverPay();
            //     objAction.ShowCCButton = true;
            //
            using (TaxiDataContext db = new TaxiDataContext())
            {
                string url = "http://eurosofttech-api.co.uk/CardStramDriverApp?Data=";
                //
                db.DeferredLoadingEnabled = false;
                // var objDriver = db.Fleet_Drivers.Where(c => c.Id == driverId).Select(c => new {c.DriverTypeId,c.RentLimit, c.DriverNo,c.DriverName,c.MobileNo,c.Address,c.Email }).FirstOrDefault();
                decimal BalanceLimit = objDriver.RentLimit.ToDecimal();


                if (objDriver.DriverTypeId == 1)
                {

                    var objTrans = db.DriverRents.Where(c => c.DriverId == driverId)
                    .Select(args => new { args.Id, args.TransNo, args.TransDate, args.Balance, args.ToDate }).OrderByDescending(c => c.Id).FirstOrDefault();

                    if (objTrans != null && objTrans.Balance.ToDecimal() > 0 && objTrans.Balance.ToDecimal() >= BalanceLimit)
                    {

                        DateTime? tillDate = null;

                        if (objTrans.ToDate != null)
                            tillDate = objTrans.ToDate;

                        if (Instance.objPolicy.DriverSuspensionDateTime != null)
                        {

                            tillDate = tillDate.Value.AddHours((Instance.objPolicy.DriverSuspensionDateTime.Value.Hour + 24));

                        }


                        if (objTrans.ToDate != null && tillDate != null && tillDate < DateTime.Now)
                        {

                            objAction.amount = objTrans.Balance.ToStr();
                            objAction.statementno = objTrans.TransNo.ToStr();
                            objAction.statementid = objTrans.Id.ToStr();
                            if (version.Length > 0 && version.ToDecimal() >= 41.64m && version.ToDecimal() < 45)
                            {
                                objAction.Gateway = "rms";
                            }
                            else
                                objAction.Gateway = "sumup";

                            objAction.tokendetails = "";
                            objAction.affiliatekey = "";
                            objAction.url = url + GetRMSJson(objTrans.Id, objAction.statementno.ToStr(), driverId, objDriver.DriverNo, objDriver.DriverName, objDriver.Email.ToStr(), objDriver.MobileNo, objDriver.Address.ToStr(), objTrans.Balance.ToDecimal());
                            objAction.ShowCCButton = true;
                            objAction.driverid = driverId.ToStr();
                            objAction.message = "Your Balance is due " + Environment.NewLine + objAction.amount;

                        }
                    }
                }
                else
                {
                    var objTrans = db.Fleet_DriverCommisions.Where(c => c.DriverId == driverId)
                        .Select(args => new { args.Id, args.TransNo, args.TransDate, args.Balance, args.ToDate }).OrderByDescending(c => c.Id).FirstOrDefault();

                    if (objTrans != null && objTrans.Balance.ToDecimal() > 0 && objTrans.Balance.ToDecimal() >= BalanceLimit)
                    {
                        //


                        DateTime? tillDate = null;

                        if (objTrans.ToDate != null)
                            tillDate = objTrans.ToDate;

                        if (Instance.objPolicy.DriverSuspensionDateTime != null)
                        {

                            tillDate = tillDate.Value.AddHours(Instance.objPolicy.DriverSuspensionDateTime.Value.Hour);

                        }

                        if (objTrans.ToDate != null && tillDate != null && tillDate < DateTime.Now)
                        {

                            objAction.amount = objTrans.Balance.ToStr();
                            objAction.statementno = objTrans.TransNo.ToStr();
                            objAction.statementid = objTrans.Id.ToStr();
                            if (version.Length > 0 && version.ToDecimal() >= 41.64m && version.ToDecimal() < 45)
                            {
                                objAction.Gateway = "rms";
                            }
                            else
                                objAction.Gateway = "sumup";
                            objAction.tokendetails = "";
                            objAction.affiliatekey = "";
                            objAction.url = url + GetRMSJson(objTrans.Id, objAction.statementno.ToStr(), driverId, objDriver.DriverNo, objDriver.DriverName, objDriver.Email.ToStr(), objDriver.MobileNo, objDriver.Address.ToStr(), objTrans.Balance.ToDecimal());
                            objAction.ShowCCButton = true;
                            //  objAction.url = url + GetRMSJson(objTrans.Id, driverId, objTrans.Balance.ToDecimal()) + "&isDriverApp&IsCustomerApp";
                            objAction.driverid = driverId.ToStr();
                            objAction.message = "Your Balance is due " + Environment.NewLine + objAction.amount;
                        }
                    }

                }




                try
                {

                    File.AppendAllText(physicalPath + "\\log_getdriverpay.txt", DateTime.Now.ToStr() + " , url=" + objAction.url + Environment.NewLine);

                }

                catch
                {

                }

            }


            if (objAction.statementid.ToStr().Trim().Length > 0)
                return new JavaScriptSerializer().Serialize(objAction);
            else
                return "";
        }


        private string GetRMSJson(long statementId, string statementNo, int driverId, string driverNo, string driverName, string driverEmail, string driverMobileNo, string driverAddress, decimal amount)
        {
            string response = string.Empty;

            if (Global.driverPayLiveDetails.ToStr() == "1")
            {
                using (TaxiDataContext db = new TaxiDataContext())
                {
                    ClsCCDetails arr = null;

                    try
                    {
                        arr = db.ExecuteQuery<ClsCCDetails>("select Id,CCDetails,IsDefault from  fleet_driver_ccdetails where IsDefault=1 and driverid=" + driverId).FirstOrDefault();
                    }
                    catch
                    {

                    }
                   
                    
                    
                    //

                    CardStreamSettings cardStreamSettings = new CardStreamSettings();
                    cardStreamSettings.merchantId = "128322";//MERCHANT_ID;
                    cardStreamSettings.SharedKey = "TGKAJRbGHezGq7bA";
                    cardStreamSettings.action = "SALE";
                    cardStreamSettings.transType = 1;
                    cardStreamSettings.uniqueIdentifier = statementNo;
                    //
                    cardStreamSettings.currencyCode = 826;
                    cardStreamSettings.amount = amount;//amount.multiply(BigDecimal.valueOf(100)).toBigInteger().toString(); // VISA
                    cardStreamSettings.orderRef = statementNo + "|" + driverNo;
                   
                    cardStreamSettings.customerName = driverName; // VISA
                    cardStreamSettings.customerEmail = driverEmail;
                    cardStreamSettings.customerPhone = driverMobileNo;



                    cardStreamSettings.cardNumber = ""; // VISA
                    cardStreamSettings.cardExpiryMM = "";
                    cardStreamSettings.cardExpiryYY = "";
                    cardStreamSettings.cardCVV = "";
                    cardStreamSettings.customerAddress = "";
                    cardStreamSettings.customerPostcode = "";

                    if (arr != null)
                    {
                        try
                        {
                            string decrypt = Cryptography.Decrypt(arr.CCDetails, driverId.ToStr(), true);

                            ClsCCDetails objCls = new JavaScriptSerializer().Deserialize<ClsCCDetails>(decrypt);


                            cardStreamSettings.cardNumber = objCls.cardNumber.ToStr(); // VISA
                            cardStreamSettings.cardExpiryMM = objCls.cardExpiryMM.ToStr();
                            cardStreamSettings.cardExpiryYY = objCls.cardExpiryYY.ToStr();
                            //  cardStreamSettings.cardCVV = objCls;
                            cardStreamSettings.customerAddress = objCls.customerAddress.ToStr();
                            cardStreamSettings.customerPostcode = objCls.customerPostcode.ToStr().ToUpper();
                        }
                        catch
                        {

                        }
                    }
                  
                   
                    cardStreamSettings.countryCode = 826;
                   
                    cardStreamSettings.FetchString = "LDgm7JF0nesl9KEa1jx+BNRagY6UIfNloltpoonCJBjQttzWsI2LnHCaFxLhLRHzK6IQHhrVOpokMFPugWHPyOYpvcSIvEuMKoJEzQvDZEKN/3ThUdzfhfryTuuG9PqMxZe9nQ7ITZD14TvZgDgRW+zwKwKwMEXZ";
                    cardStreamSettings.TableType = "Driver";
                    cardStreamSettings.StatementId = statementId;
                    cardStreamSettings.driverId = driverId;
                     response = new JavaScriptSerializer().Serialize(cardStreamSettings);

                }


            }
            else
            {





                CardStreamSettings cardStreamSettings = new CardStreamSettings();
                cardStreamSettings.merchantId = "101600";//MERCHANT_ID;
                cardStreamSettings.action = "SALE";
                cardStreamSettings.transType = 1;
                cardStreamSettings.uniqueIdentifier = statementNo;
                //
                cardStreamSettings.currencyCode = 826;
                cardStreamSettings.amount = amount;//amount.multiply(BigDecimal.valueOf(100)).toBigInteger().toString(); // VISA
                cardStreamSettings.orderRef = statementNo + "|" + driverNo;
                cardStreamSettings.cardNumber = "4543059999999982"; // VISA
                cardStreamSettings.cardExpiryMM = "11";
                cardStreamSettings.cardExpiryYY = "21";
                cardStreamSettings.cardCVV = "110";
                cardStreamSettings.customerName = "Test"; // VISA
                cardStreamSettings.customerEmail = driverEmail;
                cardStreamSettings.customerPhone = driverMobileNo;


                driverAddress = driverAddress.ToStr().Trim().ToUpper();
                string postCode = General.GetPostCodeMatch(driverAddress);


                if (postCode.Length > 0)
                    driverAddress = driverAddress.Replace(postCode, "");

                cardStreamSettings.customerAddress = "6 Roseby Avenue";
                cardStreamSettings.countryCode = 826;
                cardStreamSettings.customerPostcode = "M63X 7TH";
                cardStreamSettings.FetchString = "LDgm7JF0nesl9KEa1jx+BNRagY6UIfNloltpoonCJBjQttzWsI2LnHCaFxLhLRHzK6IQHhrVOpokMFPugWHPyOYpvcSIvEuMKoJEzQvDZEKN/3ThUdzfhfryTuuG9PqMxZe9nQ7ITZD14TvZgDgRW+zwKwKwMEXZ";
                cardStreamSettings.TableType = "Driver";
                cardStreamSettings.StatementId = statementId;
                cardStreamSettings.driverId = driverId;
                 response = new JavaScriptSerializer().Serialize(cardStreamSettings);

            }

            response = Cryptography.Encrypt(response, "tcloudX@@!", true);
            return response;
        }


        public void requestDriverPay(string dataValue)
        {
            try
            {
                string json = "";
                //
                if (dataValue.EndsWith("="))
                {
                    string[] values = dataValue.Split(new char[] { '=' });

                    json = values[0].ToStr().Trim();

                }
                else
                    json = dataValue;


                ClsDriverPay objAction = new JavaScriptSerializer().Deserialize<ClsDriverPay>(json);
                //
               
                string response = string.Empty;


                int driverId = 0;

                driverId = objAction.driverid.ToInt();



                //if (pdaversion.ToDecimal() >= 41.55m && pdaversion.ToDecimal() <= 45)
                //{
                //

                //}
                //

                using (TaxiDataContext db = new TaxiDataContext())
                {

                    db.DeferredLoadingEnabled = false;


                    var objDriver = db.Fleet_Drivers.FirstOrDefault(c => c.Id == driverId);
                    objDriver.RentLimit = 0.00m;

                    response = GetDriverPay(driverId, objDriver,objAction.ver.ToStr());

                    if (response.Length > 0)
                    {



                        Clients.Caller.driverPay(response);

                    }
                    else
                    {
                        objAction.amount = "0.00";
                        objAction.Gateway = "rms";
                        objAction.ShowCCButton = true;
                        response = new JavaScriptSerializer().Serialize(objAction);
                        Clients.Caller.driverPay(response);
                    }


                    //int drivertypeid = db.Fleet_Drivers.Where(c => c.Id == driverId).Select(c => c.DriverTypeId).FirstOrDefault().ToInt(); ;
                    //if (drivertypeid == 1)
                    //{

                    //    var objTrans = db.DriverRents.Where(c => c.DriverId == driverId)
                    //    .Select(args => new { args.Id, args.TransNo, args.TransDate, args.Balance }).OrderByDescending(c => c.Id).FirstOrDefault();

                    //    if (objTrans != null && objTrans.Balance.ToDecimal() > 0)
                    //    {

                    //        objAction.amount = objTrans.Balance.ToStr();
                    //        objAction.statementno = objTrans.TransNo.ToStr();
                    //        objAction.statementid = objTrans.Id.ToStr();
                    //        objAction.Gateway = "sumup";
                    //        objAction.tokendetails = "";
                    //        objAction.affiliatekey = "";

                    //    }
                    //}
                    //else
                    //{
                    //    var objTrans = db.Fleet_DriverCommisions.Where(c => c.DriverId == driverId)
                    //        .Select(args => new { args.Id, args.TransNo, args.TransDate, args.Balance }).OrderByDescending(c => c.Id).FirstOrDefault();

                    //    if (objTrans != null && objTrans.Balance.ToDecimal() > 0)
                    //    {

                    //        objAction.amount = objTrans.Balance.ToStr();
                    //        objAction.statementno = objTrans.TransNo.ToStr();
                    //        objAction.statementid = objTrans.Id.ToStr();
                    //        objAction.Gateway = "sumup";
                    //        objAction.tokendetails = "";
                    //        objAction.affiliatekey = "736ba26a-ef22-476b-96d4-8d17a48b1c0e";

                    //    }
                    //}

                }


                //string response = new JavaScriptSerializer().Serialize(objAction);

                //Clients.Caller.driverPay(response);


                try
                {

                    File.AppendAllText(physicalPath + "\\requestDriverPay.txt", DateTime.Now.ToStr() + " , datavalue=" + dataValue + ",response:" + response + Environment.NewLine);

                }

                catch
                {

                }
            }
            catch (Exception ex)
            {
                Clients.Caller.driverPay("Exception Occured");

                try
                {

                    File.AppendAllText(physicalPath + "\\exception_requestdriverpay.txt", DateTime.Now.ToStr() + " , datavalue=" + dataValue + Environment.NewLine);

                }

                catch
                {

                }
            }
        }



        public void updateDriverPayment(string dataValue)
        {
            try
            {
                string json = "";

                if (dataValue.EndsWith("="))
                {
                    string[] values = dataValue.Split(new char[] { '=' });

                    json = values[0].ToStr().Trim();

                }
                else
                    json = dataValue;

                ClsDriverPay objAction = new JavaScriptSerializer().Deserialize<ClsDriverPay>(json);


                int driverId = 0;

                driverId = objAction.driverid.ToInt();

                long statementId = 0;
                statementId = objAction.statementid.ToLong();
                using (TaxiDataContext db = new TaxiDataContext())
                {

                    if (db.Fleet_Drivers.Where(c => c.Id == driverId && c.DriverTypeId == 1).Count() > 0)
                    {
                        var objDriverCommision = db.
                            DriverRents.FirstOrDefault(c => c.Id == statementId);


                        objDriverCommision.Balance = 0.00m;
                        Fleet_DriverRentExpense objDriverCommissionExpense = new Fleet_DriverRentExpense();

                        objDriverCommissionExpense.AddBy = "Driver(" + objAction.driverno.ToStr() + ")";


                        objDriverCommissionExpense.Credit = objAction.amount.ToDecimal();
                        objDriverCommissionExpense.Amount = objAction.amount.ToDecimal();

                        objDriverCommissionExpense.Date = DateTime.Now;
                        objDriverCommissionExpense.Description = "Paid : " + objAction.amount + " , TransactionID : " + objAction.transId.ToStr();
                        objDriverCommissionExpense.IsPaid = true;
                        if (objDriverCommision.Fleet_DriverRentExpenses.Count == 0)
                        {
                            objDriverCommision.Fleet_DriverRentExpenses.Add(objDriverCommissionExpense);
                        }
                        else
                        {
                            objDriverCommision.Fleet_DriverRentExpenses.Add(objDriverCommissionExpense);
                        }

                    }
                    else
                    {
                        var objDriverCommision = db.Fleet_DriverCommisions.FirstOrDefault(c => c.Id == statementId);


                        objDriverCommision.Balance = 0.00m;
                        Fleet_DriverCommissionExpense objDriverCommissionExpense = new Fleet_DriverCommissionExpense();

                        objDriverCommissionExpense.AddBy = "Driver(" + objAction.driverno.ToStr() + ")";


                        objDriverCommissionExpense.Credit = objAction.amount.ToDecimal();
                        objDriverCommissionExpense.Amount = objAction.amount.ToDecimal();

                        objDriverCommissionExpense.Date = DateTime.Now;
                        objDriverCommissionExpense.Description = "Paid : " + objAction.amount + " , TransactionID : " + objAction.transId.ToStr();
                        objDriverCommissionExpense.IsPaid = true;
                        if (objDriverCommision.Fleet_DriverCommissionExpenses.Count == 0)
                        {
                            objDriverCommision.Fleet_DriverCommissionExpenses.Add(objDriverCommissionExpense);
                        }
                        else
                        {
                            objDriverCommision.Fleet_DriverCommissionExpenses.Add(objDriverCommissionExpense);
                        }

                    }

                    db.SubmitChanges();

                    objAction.message = "success";


                }


                string response = new JavaScriptSerializer().Serialize(objAction);

                Clients.Caller.DriverPayment(response);


                try
                {

                    File.AppendAllText(physicalPath + "\\log_updatedriverpayment.txt", DateTime.Now.ToStr() + " , datavalue=" + dataValue + Environment.NewLine);

                }

                catch
                {

                }
            }
            catch (Exception ex)
            {
                Clients.Caller.DriverPayment("Exception Occured");

                try
                {

                    File.AppendAllText(physicalPath + "\\exception_updatedriverpayment.txt", DateTime.Now.ToStr() + " , datavalue=" + dataValue + Environment.NewLine);

                }

                catch
                {

                }
            }
        }



        public void requestPaymentHistory(string dataValue)
        {
            try
            {
                string json = "";

                if (dataValue.EndsWith("="))
                {
                    string[] values = dataValue.Split(new char[] { '=' });

                    json = values[0].ToStr().Trim();

                }
                else
                    json = dataValue;

                ClsDriverPay objAction = new JavaScriptSerializer().Deserialize<ClsDriverPay>(json);


                int driverId = 0;

                driverId = objAction.driverid.ToInt();
                string response = string.Empty;
                using (TaxiDataContext db = new TaxiDataContext())
                {

                    db.DeferredLoadingEnabled = false;

                    var objTrans = db.Fleet_DriverCommissionExpenses.Where(c => c.AddBy.StartsWith("Driver") && c.Fleet_DriverCommision.DriverId == driverId)
                        .Select(args => new { amount = args.Amount, statementno = args.Fleet_DriverCommision.TransNo, transTime = string.Format("{0:dd/MM/yyyy HH:mm}", args.Date), transId = args.Description, args.Date })
                        .OrderByDescending(c => c.Date).Take(10).ToList();

                    //if (objTrans != null)
                    //{

                    //    objAction.amount = objTrans.am.ToStr();
                    //    objAction.statementno = objTrans.TransNo.ToStr();
                    //    objAction.statementid = objTrans.Id.ToStr();
                    //    objAction.Gateway = "sumup";
                    //    objAction.tokendetails = "";
                    //    objAction.affiliatekey = "77dd50ea-55ec-47fe-a1c0-85904a45f0b3";

                    //}
                    response = new JavaScriptSerializer().Serialize(objTrans);

                }




                Clients.Caller.paymentHistory(response);


                try
                {

                    File.AppendAllText(physicalPath + "\\log_requestPaymentHistory.txt", DateTime.Now.ToStr() + " , datavalue=" + dataValue + Environment.NewLine);

                }

                catch
                {

                }
            }
            catch (Exception ex)
            {
                Clients.Caller.paymentHistory("Exception Occured");

                try
                {

                    File.AppendAllText(physicalPath + "\\exception_requestPaymentHistory.txt", DateTime.Now.ToStr() + " , datavalue=" + dataValue + Environment.NewLine);

                }

                catch
                {

                }
            }
        }



        public void requestPaymentDetailSummary(string mesg)
        {
            try
            {

                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\requestPaymentDetailSummary.txt", DateTime.Now.ToStr() + ": request :" + mesg + Environment.NewLine);
                }
                catch (Exception ex)
                {



                }

                byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                long jobId = values[1].ToLong();
                int driverId = values[2].ToInt();


                JobAction objAction = new JavaScriptSerializer().Deserialize<JobAction>(values[4].Trim());

                Taxi_Model.Booking objBooking = null;
                string response = string.Empty;

                using (TaxiDataContext db = new TaxiDataContext())
                {
                    try
                    {
                        objBooking = db.Bookings.FirstOrDefault(c => c.Id == jobId);

                        if (objPaymentColumns == null)
                        {
                            objPaymentColumns = db.Gen_PaymentColumnSettings.FirstOrDefault();
                        }

                        response = "failed:Job is not available";

                        if (objBooking != null)
                        {
                            if (objPaymentColumns == null)
                                objPaymentColumns = new Gen_PaymentColumnSetting() { ChargesType = Enums.PAYMENT_CHARGESTYPE.CHARGESTYPE1, ShowFares = false, ShowTip = false, ShowSurchargesRates = true, ShowTotal = true };

                            bool ShowFares = objPaymentColumns.ShowFares.ToBool();
                            bool ShowParking = objPaymentColumns.ShowParking.ToBool();
                            bool ShowWaiting = objPaymentColumns.ShowWaiting.ToBool();

                            bool ShowSurcharge = objPaymentColumns.ShowSurchargesRates.ToBool();
                            bool ShowTip = objPaymentColumns.ShowTip.ToBool();
                            bool ShowTotal = objPaymentColumns.ShowTotal.ToBool();

                            bool EditFares = objPaymentColumns.EditFares.ToBool();
                            bool EditParking = objPaymentColumns.EditParking.ToBool();
                            bool EditWaiting = objPaymentColumns.EditWaiting.ToBool();


                            SetChargesLimit();

                            decimal surchargeValue = Instance.objPolicy.CreditCardExtraCharges.ToDecimal();
                            decimal amount = objBooking.FareRate.ToDecimal();
                            string surchargeType = Instance.objPolicy.CreditCardChargesType.ToInt() == 1 ? "Amount" : "Percent";
                            //string surchargeType = "Percent";

                            if (Instance.objPolicy.CreditCardExtraCharges.ToDecimal() > 0)
                            {
                                ShowSurcharge = true;

                                if (Instance.objPolicy.CreditCardChargesType.ToInt() == 1)
                                {
                                    if (chargesLimit != null && chargesLimit.Count() >= 3 && amount > chargesLimit[0].ToDecimal())
                                    {
                                        if (chargesLimit[1].ToInt() == 1)
                                        {
                                            surchargeValue = chargesLimit[2].ToDecimal();
                                            surchargeType = "Amount";
                                        }
                                        else if (chargesLimit[1].ToInt() == 2)
                                        {
                                            //numSurchargePercent.Enabled = true;
                                            surchargeValue = chargesLimit[2].ToDecimal();
                                            surchargeType = "Percent";
                                        }
                                    }
                                    else
                                    {
                                        surchargeValue = Instance.objPolicy.CreditCardExtraCharges.ToDecimal();
                                        surchargeType = "Amount";
                                    }
                                }
                                else if (Instance.objPolicy.CreditCardChargesType.ToInt() == 2)
                                {
                                    if (chargesLimit != null && chargesLimit.Count() >= 3 && amount > chargesLimit[0].ToDecimal())
                                    {
                                        if (chargesLimit[1].ToInt() == 1)
                                        {
                                            surchargeValue = chargesLimit[2].ToDecimal();
                                            surchargeType = "Amount";
                                        }
                                        else if (chargesLimit[1].ToInt() == 2)
                                        {
                                            //numSurchargePercent.Enabled = true;
                                            surchargeValue = chargesLimit[2].ToDecimal();
                                            surchargeType = "Percent";
                                        }
                                    }
                                    else
                                    {
                                        //numSurchargePercent.Enabled = true;
                                        surchargeValue = Instance.objPolicy.CreditCardExtraCharges.ToDecimal();
                                        surchargeType = "Percent";
                                    }
                                }
                            }
                            ////
                            if (Instance.objPolicy.SendBookingCompletionEmail.ToBool())
                            {
                                response = "{ \"JobId\" :\"" + jobId.ToStr() +
                                                    "\", \"ShowFares\":\"" + "false" +
                                                    "\", \"ShowSurcharge\":\"" + "false" + "\"," +
                                                    "\"ShowTip\":\"" + "false" + "\"" +
                                                    ",\"ShowTotal\":\"" + "false" + "\",\"Fares\":\"" + objBooking.FareRate + " " + "\",\"Surcharge\":\"" + surchargeValue + "\""
                                                    + " }";
                            }
                            else
                            {
                                decimal price = 0.00m;
                                //
                                decimal parking = objBooking.CongtionCharges.ToDecimal();
                                decimal waiting = objBooking.MeetAndGreetCharges.ToDecimal();

                                if (objPaymentColumns.ChargesType.ToInt() == 0 || objPaymentColumns.ChargesType.ToInt() == Enums.PAYMENT_CHARGESTYPE.CHARGESTYPE1 ||
                                    objPaymentColumns.ChargesType.ToInt() == Enums.PAYMENT_CHARGESTYPE.CHARGESTYPE2)
                                {
                                    // Show Fares
                                    // Show Surcharges
                                    // Show TOTAL
                                    // Hide Tip

                                    if (objPaymentColumns.EditFares.ToBool())
                                    {
                                        price = objBooking.FareRate.ToDecimal();
                                        if (objBooking.CompanyId != null && objBooking.Gen_Company.SysGenId.ToInt() != 2)
                                        {
                                            price = objBooking.CompanyPrice.ToDecimal();
                                            parking = objBooking.ParkingCharges.ToDecimal();
                                            waiting = objBooking.WaitingCharges.ToDecimal();
                                        }
                                    }
                                    else
                                    {
                                        price = objBooking.FareRate.ToDecimal();
                                        if (objBooking.CompanyId != null && objBooking.Gen_Company.SysGenId.ToInt() != 2)
                                        {
                                            price = objBooking.CompanyPrice.ToDecimal();
                                            parking = objBooking.ParkingCharges.ToDecimal();
                                            waiting = objBooking.WaitingCharges.ToDecimal();
                                        }
                                    }
                                }
                                //else if (objPaymentColumns.ChargesType.ToInt() == Enums.PAYMENT_CHARGESTYPE.CHARGESTYPE2)
                                //{
                                //    price = objBooking.FareRate.ToDecimal();
                                //}
                                else if (objPaymentColumns.ChargesType.ToInt() == Enums.PAYMENT_CHARGESTYPE.CHARGESTYPE3)
                                {
                                    // For Shaftbury
                                    // Show Fares
                                    // Show Surcharges
                                    // Show TOTAL
                                    // Show Tip (optional Hide or Show)

                                    price = objBooking.FareRate.ToDecimal() + objBooking.MeetAndGreetCharges.ToDecimal() + objBooking.CongtionCharges.ToDecimal();
                                }
                                else if (objPaymentColumns.ChargesType.ToInt() == Enums.PAYMENT_CHARGESTYPE.CHARGESTYPE4)
                                {
                                    // For PinkApple
                                    // Hide Fares
                                    // Hide Surcharges
                                    // Hide TOTAL
                                    // Hide Tip

                                    price = objBooking.FareRate.ToDecimal();
                                }



                                if (objAction.IsMeter.ToStr() == "1")
                                {
                                    price = objAction.Fares.ToDecimal();
                                    parking = objAction.ParkingCharges.ToDecimal();
                                    waiting = objAction.WaitingCharges.ToDecimal();


                                }
                                else
                                {
                                    waiting = objAction.WaitingCharges.ToDecimal();

                                    price = price + objBooking.ServiceCharges.ToDecimal();
                                }


                                decimal extracharges = objBooking.ExtraDropCharges.ToDecimal();
                                string showExtraCharges = "true";
                                if (objBooking.CustomerCreditCardDetails.ToStr().Trim().Length == 0)
                                {
                                    extracharges = 0.00m;
                                    showExtraCharges = "false";

                                }

                                response = "{ \"JobId\" :\"" + jobId.ToStr() +
                                                    "\", \"ShowFares\":\"" + ShowFares +
                                                    "\", \"EditFares\":\"" + EditFares +
                                                    "\", \"ShowWaiting\":\"" + ShowWaiting.ToStr() +
                                                    "\", \"ShowParking\":\"" + ShowParking.ToStr() +
                                                     "\", \"ShowBookingFee\":\"" + "false" +
                                                      "\", \"ShowExtraDropCharges\":\"" + "true" +

                                                    "\", \"EditParking\":\"" + EditParking +
                                                    "\", \"EditWaiting\":\"" + EditWaiting +

                                                     "\", \"BookingFee\":\"" + objAction.BookingFee.ToDecimal() +
                                                      "\", \"ExtraDropCharges\":\"" + objAction.ExtraDropCharges.ToDecimal() +

                                                    "\", \"ShowSurcharge\":\"" + ShowSurcharge + "\"," +
                                                    "\"SurchargeType\":\"" + surchargeType + "\"," +
                                                    "\"ShowTip\":\"" + ShowTip + "\"" +
                                                    ",\"ShowTotal\":\"" + ShowTotal + "\",\"Fares\":\"" + price + " " + "\",\"Parking\":\"" + parking + " " + "\",\"Waiting\":\"" + waiting + " " + "\",\"Surcharge\":\"" + surchargeValue + "\"";
                                //    + " }";

                                var objPay = db.Gen_SysPolicy_PaymentDetails.FirstOrDefault(c => c.EnableMobileIntegration == true);




                                string paymentGateway = string.Empty;

                                if (objPay != null)
                                {

                                    string gatewayName = string.Empty;



                                    int gatewayId = objPay.PaymentGatewayId.ToInt();

                                    if (gatewayId == 8)
                                        gatewayName = "paypalhere";
                                    else if (gatewayId == 9)
                                        gatewayName = "sumup";



                                    if (objBooking.CustomerCreditCardDetails.ToStr().Trim().Length > 0)
                                    {
                                        gatewayName = "direct=" + gatewayName;

                                    }



                                    paymentGateway = ",\"Gateway\":\"" + gatewayName + "\"" + ",\"UserName\":\"" + objPay.MerchantID + "\"" + ",\"Password\":\"" + objPay.MerchantPassword + "\",\"affiliatekey\":\"" + objPay.PaypalID.ToStr() + "\",\"tokendetails\":\"" + objBooking.CustomerCreditCardDetails.ToStr().Trim() + "\"";

                                    // paymentGateway = ",\"Gateway\":\"" + gatewayName + "\"" + ",\"UserName\":\"" + objPay.MerchantID + "\"" + ",\"Password\":\"" + objPay.MerchantPassword + "\",\"affiliatekey\":\"" + objPay.PaypalID.ToStr() + "\"";


                                }
                                else
                                {

                                    string gatewayName = string.Empty;
                                    if (objBooking.CustomerCreditCardDetails.ToStr().Trim().Length > 0)
                                    {
                                        gatewayName = "direct";

                                    }
                                    paymentGateway = ",\"Gateway\":\"" + gatewayName + "\"" + ",\"UserName\":\"" + "" + "\"" + ",\"Password\":\"" + "" + "\",\"affiliatekey\":\"" + "" + "\",\"tokendetails\":\"" + objBooking.CustomerCreditCardDetails.ToStr().Trim() + "\"";



                                }

                                //    paymentGateway = ",\"Gateway\":\"" + "sumup,paypalhere" + "\"" + ",\"UserName\":\"" + "" + "\"" + ",\"Password\":\"" + "" + "\",\"affiliatekey\":\"" + "77dd50ea-55ec-47fe-a1c0-85904a45f0b3" + "\",\"tokendetails\":\"" + "" + "\"";


                                response += paymentGateway + " }";






                            }
                        }
                    }
                    catch
                    {
                        response = "failed:Problem on getting details from server";
                    }
                }

                //send message back to PDA
                Clients.Caller.paymentDetailSummary(response);
                //Byte[] byteResponse = Encoding.UTF8.GetBytes(response);
                //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);
            }
            catch (Exception ex)
            {
                Clients.Caller.paymentSummary("exceptionoccured");
            }
        }

        public void requestmakepayment(string mesg)
        {




            string response = "failed:paymentdetails not found";

            try
            {
                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\requestmakepayment.txt", DateTime.Now.ToStr() + ": request :" + mesg + Environment.NewLine);
                }
                catch (Exception ex)
                {



                }

                byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                bool IsDirectBookingPayment = Instance.objPolicy.SendBookingCompletionEmail.ToBool();


                JavaScriptSerializer s = new JavaScriptSerializer();
                ClsPaymentInformation objPaymentDetails = s.Deserialize<ClsPaymentInformation>(values[1].ToStr());



                if (objPaymentDetails.AuthCode.ToStr().Trim().Length > 0)
                {
                    response = "success:" + objPaymentDetails.AuthCode.ToStr();

                }
                else
                {

                    Taxi_Model.Booking objBooking = General.GetObject<Taxi_Model.Booking>(c => c.Id == objPaymentDetails.BookingId.ToLong());

                    decimal Price = objBooking.FareRate.ToDecimal();
                    decimal extraPickup = 0.00m;
                    decimal extraDropOff = 0.00m;
                    decimal CreditCardSurchargeRate = 0.00m;
                    decimal ServiceCharges = objBooking.ServiceCharges.ToDecimal();
                    decimal Tip = 0.00m;
                    int? CompanyId = objBooking.CompanyId.ToIntorNull();
                    decimal actualPrice = objBooking.FareRate.ToDecimal();

                    if (objPaymentColumns == null)
                    {

                        if (objPaymentColumns == null)
                        {
                            objPaymentColumns = new TaxiDataContext().Gen_PaymentColumnSettings.FirstOrDefault();
                        }
                    }

                    if (objPaymentColumns.ChargesType.ToInt() == 0 || objPaymentColumns.ChargesType.ToInt() == Enums.PAYMENT_CHARGESTYPE.CHARGESTYPE1
                        || objPaymentColumns.ChargesType.ToInt() == Enums.PAYMENT_CHARGESTYPE.CHARGESTYPE2)
                    {

                        if (objPaymentColumns.EditFares.ToBool() || objPaymentColumns.EditParking.ToBool() || objPaymentColumns.EditWaiting.ToBool())
                        {
                            try
                            {
                                Price = objPaymentDetails.NetFares.ToDecimal() + objPaymentDetails.Parking.ToDecimal()
                                    + objPaymentDetails.Waiting.ToDecimal() + objPaymentDetails.ExtraDropCharges.ToDecimal() + objPaymentDetails.BookingFee.ToDecimal();
                                actualPrice = objPaymentDetails.NetFares.ToDecimal();
                            }
                            catch
                            {

                                Price = objPaymentDetails.NetFares.ToDecimal();
                                actualPrice = objPaymentDetails.NetFares.ToDecimal();
                            }
                        }
                        else
                        {
                            Price = objBooking.FareRate.ToDecimal() + objBooking.MeetAndGreetCharges.ToDecimal() + objBooking.CongtionCharges.ToDecimal();
                            actualPrice = objBooking.FareRate.ToDecimal();
                            if (objBooking.CompanyId != null && objBooking.Gen_Company.SysGenId.ToInt() != 2)
                            {
                                Price = objBooking.CompanyPrice.ToDecimal() + objBooking.WaitingCharges.ToDecimal() + objBooking.ParkingCharges.ToDecimal();
                                actualPrice = objBooking.CompanyPrice.ToDecimal();
                            }
                        }
                    }
                    else if (objPaymentColumns.ChargesType.ToInt() == Enums.PAYMENT_CHARGESTYPE.CHARGESTYPE2)
                    {
                        if (objPaymentColumns.EditFares.ToBool() || objPaymentColumns.EditParking.ToBool() || objPaymentColumns.EditWaiting.ToBool())
                        {
                            try
                            {
                                Price = objPaymentDetails.NetFares.ToDecimal() + objPaymentDetails.Parking.ToDecimal() + objPaymentDetails.Waiting.ToDecimal();
                                actualPrice = objPaymentDetails.NetFares.ToDecimal();
                            }
                            catch
                            {

                                Price = objPaymentDetails.NetFares.ToDecimal();
                                actualPrice = objPaymentDetails.NetFares.ToDecimal();
                            }
                        }
                        else
                        {

                            Price = objBooking.FareRate.ToDecimal();

                        }

                        actualPrice = Price;
                    }
                    else if (objPaymentColumns.ChargesType.ToInt() == Enums.PAYMENT_CHARGESTYPE.CHARGESTYPE3)
                    {

                        if (objPaymentColumns.EditFares.ToBool())
                        {
                            Price = objPaymentDetails.NetFares.ToDecimal();

                        }
                        else
                        {


                            Price = objBooking.FareRate.ToDecimal() + objBooking.MeetAndGreetCharges.ToDecimal() + objBooking.CongtionCharges.ToDecimal();
                        }

                        actualPrice = Price;
                    }

                    else if (objPaymentColumns.ChargesType.ToInt() == Enums.PAYMENT_CHARGESTYPE.CHARGESTYPE4)
                    {
                        Price = objBooking.FareRate.ToDecimal();
                        if (objBooking.FromLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT && objBooking.FromLocId != null)
                        {
                            extraPickup = General.GetObject<Gen_SysPolicy_AirportPickupCharge>(c => c.AirportId == objBooking.FromLocId).DefaultIfEmpty().Charges.ToDecimal();
                        }

                        if (objBooking.ToLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT && objBooking.ToLocId != null)
                        {
                            extraDropOff = General.GetObject<Gen_SysPolicy_AirportDropOffCharge>(c => c.AirportId == objBooking.ToLocId).DefaultIfEmpty().Charges.ToDecimal();
                        }

                        actualPrice = Price;
                    }


                    if (Instance.objPolicy.CreditCardExtraCharges.ToDecimal() > 0)
                    {

                        if (Instance.objPolicy.CreditCardExtraCharges.ToDecimal() > 0)
                        {

                            SetChargesLimit();

                            if (Instance.objPolicy.CreditCardChargesType.ToInt() == 1)
                            {


                                if (chargesLimit != null && chargesLimit.Count() >= 3 && Price > chargesLimit[0].ToDecimal())
                                {


                                    if (chargesLimit[1].ToInt() == 1)
                                    {


                                        CreditCardSurchargeRate = chargesLimit[2].ToDecimal();
                                        //   surchargeType = "Amount";
                                    }
                                    else if (chargesLimit[1].ToInt() == 2)
                                    {
                                        //     numSurchargePercent.Enabled = true;

                                        CreditCardSurchargeRate = (Price * chargesLimit[2].ToDecimal()) / 100;
                                        //   surchargeType = "Percent";

                                    }
                                }
                                else
                                {

                                    CreditCardSurchargeRate = Instance.objPolicy.CreditCardExtraCharges.ToDecimal();
                                    //surchargeType = "Amount";

                                }




                            }
                            else if (Instance.objPolicy.CreditCardChargesType.ToInt() == 2)
                            {
                                if (chargesLimit != null && chargesLimit.Count() >= 3 && Price > chargesLimit[0].ToDecimal())
                                {


                                    if (chargesLimit[1].ToInt() == 1)
                                    {
                                        CreditCardSurchargeRate = chargesLimit[2].ToDecimal();
                                        // surchargeType = "Amount";
                                    }
                                    else if (chargesLimit[1].ToInt() == 2)
                                    {
                                        //    numSurchargePercent.Enabled = true;
                                        CreditCardSurchargeRate = (Price * chargesLimit[2].ToDecimal()) / 100;

                                        //  surchargeType = "Percent";
                                    }
                                }
                                else
                                {

                                    CreditCardSurchargeRate = (Price * Instance.objPolicy.CreditCardExtraCharges.ToDecimal()) / 100;

                                    //   numSurchargePercent.Enabled = true;
                                    //  CreditCardSurchargeRate = (Price * chargesLimit[2].ToDecimal()) / 100;
                                    // surchargeType = "Percent";
                                }
                            }

                        }



                        // CreditCardSurchargeRate = (Price * objPolicy.CreditCardExtraCharges.ToDecimal()) / 100;

                    }


                    var objBookingPayment = General.GetObject<Booking_Payment>(c => c.BookingId == objPaymentDetails.BookingId.ToLong() && c.AuthCode != null && c.AuthCode != "");

                    if (objBookingPayment == null || objBookingPayment.AuthCode.ToStr().Trim().Length == 0)
                    {


                        Gen_SysPolicy_PaymentDetail obj = null;

                        if (objPaymentDetails.TokenDetails.ToStr().Trim().Length > 0)
                        {

                            if (objPaymentDetails.TokenDetails.ToStr().Trim().StartsWith("pi_"))
                            {

                                obj = General.GetObject<Gen_SysPolicy_PaymentDetail>(c => c.SysPolicyId != 0 && (c.PaymentGatewayId == 7));
                            }
                            else
                                obj = General.GetObject<Gen_SysPolicy_PaymentDetail>(c => c.SysPolicyId != 0 && (c.PaymentGatewayId == 6));

                        }
                        else
                        {
                            obj = General.GetObject<Gen_SysPolicy_PaymentDetail>(c => c.SysPolicyId != 0 && (c.EnableMobileIntegration != null && c.EnableMobileIntegration == true));

                        }

                        if (obj != null)
                        {
                            objPaymentDetails.BookingNo = objBooking.BookingNo.ToStr().Trim();

                            objPaymentDetails.Price = Price;
                            objPaymentDetails.ExtraPickup = extraPickup;
                            objPaymentDetails.ExtraDropOff = extraDropOff;

                            objPaymentDetails.SurchargeAmount = CreditCardSurchargeRate;
                            objPaymentDetails.Tip = Tip.ToStr();
                            objPaymentDetails.ServiceCharges = ServiceCharges;

                            objPaymentDetails.Total = Math.Round((Price + objPaymentDetails.SurchargeAmount.ToDecimal()), 2).ToStr();

                            objPaymentDetails.PaymentGatewayID = obj.PaymentGatewayId.ToStr();

                            response = MakePayment(obj, objPaymentDetails);

                            //if (Debugger.IsAttached)
                            //    response = "success:AuthCode:11555151";

                            if (response.StartsWith("success:"))
                            {
                                response = response.ToLower().Replace("authcode:", "").Trim();

                                //objPaymentDetails.TransactionID = response.Replace("success:", "").Trim();


                                //using (TaxiDataContext db = new TaxiDataContext())
                                //{

                                //    string transId = objPaymentDetails.TransactionID.ToStr().Trim().ToLower().Contains("authcode:") ? objPaymentDetails.TransactionID.ToStr().Trim().ToLower().Replace("authcode:", "").Trim() : objPaymentDetails.TransactionID.ToStr().Trim(); ;



                                //    db.stp_MakePayment(objPaymentDetails.Name.ToStr(), objPaymentDetails.CardNumber.ToStr(), objPaymentDetails.StartDate.ToDateorNull(),
                                //              objPaymentDetails.ExpiryDate.ToDateorNull(), objPaymentDetails.CV2.ToStr()
                                //        , objPaymentDetails.Address.ToStr(), objPaymentDetails.City.ToStr(), objPaymentDetails.PostCode.ToStr(), objPaymentDetails.Email.ToStr()
                                //        , transId, objPaymentDetails.PaymentGatewayID.ToInt(),
                                //         actualPrice, objPaymentDetails.Parking.ToDecimal(), objPaymentDetails.Waiting.ToDecimal(),
                                //           objPolicy.CreditCardExtraCharges.ToDecimal(), objPaymentDetails.SurchargeAmount.ToDecimal(), objPaymentDetails.Tip.ToDecimal(),
                                //           objPaymentDetails.Total.ToDecimal(), "paid", objPaymentDetails.BookingId.ToLong(), objPaymentDetails.DriverId.ToInt(),
                                //           objPaymentDetails.DriverNo.ToStr(), objPaymentColumns.EditFares.ToBool(), objPaymentColumns.EditWaiting.ToBool(), objPaymentColumns.EditWaiting.ToBool(), CompanyId);
                                //}
                            }
                        }
                    }
                    else
                    {
                        response = "success:" + objBookingPayment.AuthCode.ToStr();
                    }
                }

            }
            catch (Exception ex)
            {
                response = "failed:" + ex.Message;
            }


            if (response.ToStr().Length == 0)
                response = "failed:Failed to Process your Transaction";


            try
            {
                Clients.Caller.makePayment(response);
            }
            catch
            {

            }
            //Byte[] byteResponse = Encoding.UTF8.GetBytes(response);
            //    tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);

            //    General.BroadCastMessage(("**payment driverapp>>" + values[1].ToLong() + ">>" + values[2].ToStr() + ">>" + values[3].ToStr()));




        }



        public void requestPaymentSummary(string mesg)
        {
            try
            {
                byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                long jobId = values[1].ToLong();
                int driverId = values[2].ToInt();

                Taxi_Model.Booking objBooking = null;
                string response = string.Empty;

                using (TaxiDataContext db = new TaxiDataContext())
                {
                    try
                    {
                        objBooking = db.Bookings.FirstOrDefault(c => c.Id == jobId);

                        if (objPaymentColumns == null)
                        {
                            objPaymentColumns = db.Gen_PaymentColumnSettings.FirstOrDefault();
                        }

                        response = "failed:Job is not available";

                        if (objBooking != null)
                        {
                            if (objPaymentColumns == null)
                                objPaymentColumns = new Gen_PaymentColumnSetting() { ChargesType = Enums.PAYMENT_CHARGESTYPE.CHARGESTYPE1, ShowFares = false, ShowTip = false, ShowSurchargesRates = true, ShowTotal = true };

                            bool ShowFares = objPaymentColumns.ShowFares.ToBool();
                            bool ShowParking = objPaymentColumns.ShowParking.ToBool();
                            bool ShowWaiting = objPaymentColumns.ShowWaiting.ToBool();

                            bool ShowSurcharge = objPaymentColumns.ShowSurchargesRates.ToBool();
                            bool ShowTip = objPaymentColumns.ShowTip.ToBool();
                            bool ShowTotal = objPaymentColumns.ShowTotal.ToBool();

                            bool EditFares = objPaymentColumns.EditFares.ToBool();
                            bool EditParking = objPaymentColumns.EditParking.ToBool();
                            bool EditWaiting = objPaymentColumns.EditWaiting.ToBool();


                            SetChargesLimit();

                            decimal surchargeValue = Instance.objPolicy.CreditCardExtraCharges.ToDecimal();
                            decimal amount = objBooking.FareRate.ToDecimal();
                            string surchargeType = Instance.objPolicy.CreditCardChargesType.ToInt() == 1 ? "Amount" : "Percent";
                            //string surchargeType = "Percent";

                            if (Instance.objPolicy.CreditCardExtraCharges.ToDecimal() > 0)
                            {
                                ShowSurcharge = true;

                                if (Instance.objPolicy.CreditCardChargesType.ToInt() == 1)
                                {
                                    if (chargesLimit != null && chargesLimit.Count() >= 3 && amount > chargesLimit[0].ToDecimal())
                                    {
                                        if (chargesLimit[1].ToInt() == 1)
                                        {
                                            surchargeValue = chargesLimit[2].ToDecimal();
                                            surchargeType = "Amount";
                                        }
                                        else if (chargesLimit[1].ToInt() == 2)
                                        {
                                            //numSurchargePercent.Enabled = true;
                                            surchargeValue = chargesLimit[2].ToDecimal();
                                            surchargeType = "Percent";
                                        }
                                    }
                                    else
                                    {
                                        surchargeValue = Instance.objPolicy.CreditCardExtraCharges.ToDecimal();
                                        surchargeType = "Amount";
                                    }
                                }
                                else if (Instance.objPolicy.CreditCardChargesType.ToInt() == 2)
                                {
                                    if (chargesLimit != null && chargesLimit.Count() >= 3 && amount > chargesLimit[0].ToDecimal())
                                    {
                                        if (chargesLimit[1].ToInt() == 1)
                                        {
                                            surchargeValue = chargesLimit[2].ToDecimal();
                                            surchargeType = "Amount";
                                        }
                                        else if (chargesLimit[1].ToInt() == 2)
                                        {
                                            //numSurchargePercent.Enabled = true;
                                            surchargeValue = chargesLimit[2].ToDecimal();
                                            surchargeType = "Percent";
                                        }
                                    }
                                    else
                                    {
                                        //numSurchargePercent.Enabled = true;
                                        surchargeValue = Instance.objPolicy.CreditCardExtraCharges.ToDecimal();
                                        surchargeType = "Percent";
                                    }
                                }
                            }

                            if (Instance.objPolicy.SendBookingCompletionEmail.ToBool())
                            {
                                response = "{ \"JobId\" :\"" + jobId.ToStr() +
                                                    "\", \"ShowFares\":\"" + "false" +
                                                    "\", \"ShowSurcharge\":\"" + "false" + "\"," +
                                                    "\"ShowTip\":\"" + "false" + "\"" +
                                                    ",\"ShowTotal\":\"" + "false" + "\",\"Fares\":\"" + objBooking.FareRate + " " + "\",\"Surcharge\":\"" + surchargeValue + "\""
                                                    + " }";
                            }
                            else
                            {
                                decimal price = 0.00m;

                                decimal parking = objBooking.CongtionCharges.ToDecimal();
                                decimal waiting = objBooking.MeetAndGreetCharges.ToDecimal();

                                if (objPaymentColumns.ChargesType.ToInt() == 0 || objPaymentColumns.ChargesType.ToInt() == Enums.PAYMENT_CHARGESTYPE.CHARGESTYPE1 ||
                                    objPaymentColumns.ChargesType.ToInt() == Enums.PAYMENT_CHARGESTYPE.CHARGESTYPE2)
                                {
                                    // Show Fares
                                    // Show Surcharges
                                    // Show TOTAL
                                    // Hide Tip

                                    if (objPaymentColumns.EditFares.ToBool())
                                    {
                                        price = objBooking.FareRate.ToDecimal();
                                        if (objBooking.CompanyId != null && objBooking.Gen_Company.SysGenId.ToInt() != 2)
                                        {
                                            price = objBooking.CompanyPrice.ToDecimal();
                                            parking = objBooking.ParkingCharges.ToDecimal();
                                            waiting = objBooking.WaitingCharges.ToDecimal();
                                        }
                                    }
                                    else
                                    {
                                        price = objBooking.FareRate.ToDecimal() + objBooking.MeetAndGreetCharges.ToDecimal() + objBooking.CongtionCharges.ToDecimal();
                                        if (objBooking.CompanyId != null && objBooking.Gen_Company.SysGenId.ToInt() != 2)
                                        {
                                            price = objBooking.CompanyPrice.ToDecimal() + objBooking.WaitingCharges.ToDecimal() + objBooking.ParkingCharges.ToDecimal();
                                            parking = objBooking.ParkingCharges.ToDecimal();
                                            waiting = objBooking.WaitingCharges.ToDecimal();
                                        }
                                    }
                                }
                                //else if (objPaymentColumns.ChargesType.ToInt() == Enums.PAYMENT_CHARGESTYPE.CHARGESTYPE2)
                                //{
                                //    price = objBooking.FareRate.ToDecimal();
                                //}
                                else if (objPaymentColumns.ChargesType.ToInt() == Enums.PAYMENT_CHARGESTYPE.CHARGESTYPE3)
                                {
                                    // For Shaftbury
                                    // Show Fares
                                    // Show Surcharges
                                    // Show TOTAL
                                    // Show Tip (optional Hide or Show)

                                    price = objBooking.FareRate.ToDecimal() + objBooking.MeetAndGreetCharges.ToDecimal() + objBooking.CongtionCharges.ToDecimal();
                                }
                                else if (objPaymentColumns.ChargesType.ToInt() == Enums.PAYMENT_CHARGESTYPE.CHARGESTYPE4)
                                {
                                    // For PinkApple
                                    // Hide Fares
                                    // Hide Surcharges
                                    // Hide TOTAL
                                    // Hide Tip

                                    price = objBooking.FareRate.ToDecimal();
                                }

                                response = "{ \"JobId\" :\"" + jobId.ToStr() +
                                                    "\", \"ShowFares\":\"" + ShowFares +
                                                    "\", \"EditFares\":\"" + EditFares +
                                                    "\", \"ShowWaiting\":\"" + ShowWaiting.ToStr() +
                                                    "\", \"ShowParking\":\"" + ShowParking.ToStr() +
                                                    "\", \"EditParking\":\"" + EditParking +
                                                    "\", \"EditWaiting\":\"" + EditWaiting +
                                                    "\", \"ShowSurcharge\":\"" + ShowSurcharge + "\"," +
                                                    "\"SurchargeType\":\"" + surchargeType + "\"," +
                                                    "\"ShowTip\":\"" + ShowTip + "\"" +
                                                    ",\"ShowTotal\":\"" + ShowTotal + "\",\"Fares\":\"" + price + " " + "\",\"Parking\":\"" + parking + " " + "\",\"Waiting\":\"" + waiting + " " + "\",\"Surcharge\":\"" + surchargeValue + "\""
                                                    + " }";
                            }
                        }
                    }
                    catch
                    {
                        response = "failed:Problem on getting details from server";
                    }
                }

                //send message back to PDA
                Clients.Caller.paymentSummary(response);
                //Byte[] byteResponse = Encoding.UTF8.GetBytes(response);
                //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);
            }
            catch (Exception ex)
            {
                Clients.Caller.paymentSummary(ex.Message);
            }
        }

        //private void requestPDA(string mesg)
        //{
        //    try
        //    {
        //        byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

        //        string dataValue = mesg;
        //        dataValue = dataValue.Trim();

        //        string[] values = dataValue.Split(new char[] { '=' });

        //        if (values[4].ToInt() == eMessageTypes.MESSAGING)
        //        {
        //            if (values[1].ToStr().Contains(","))
        //            {
        //                DateTime dt = DateTime.Now.AddSeconds(-45);
        //                foreach (string dId in values[1].Split(','))
        //                {
        //                    Instance.listofJobs.Add(new clsPDA
        //                    {
        //                        DriverId = dId.ToInt(),
        //                        JobId = values[2].ToLong(),
        //                        MessageDateTime = dt,
        //                        JobMessage = values[3].ToStr().Trim(),
        //                        MessageTypeId = values[4].ToInt()
        //                    });
        //                }
        //            }
        //            else
        //            {
        //                try
        //                {
        //                    Instance.listofJobs.Add(new clsPDA
        //                    {
        //                        DriverId = values[1].ToInt(),
        //                        JobId = values[2].ToLong(),
        //                        MessageDateTime = DateTime.Now.AddSeconds(-45),
        //                        JobMessage = values[3].ToStr().Trim(),
        //                        MessageTypeId = values[4].ToInt()
        //                    });
        //                }
        //                catch (Exception ex)
        //                {
        //                    try
        //                    {
        //                        Thread.Sleep(100);

        //                        Instance.listofJobs.Add(new clsPDA
        //                        {
        //                            DriverId = values[1].ToInt(),
        //                            JobId = values[2].ToLong(),
        //                            MessageDateTime = DateTime.Now.AddSeconds(-45),
        //                            JobMessage = values[3].ToStr().Trim(),
        //                            MessageTypeId = values[4].ToInt()
        //                        });

        //                        File.AppendAllText(physicalPath + "\\exception_servertoclient.txt", DateTime.Now.ToStr() + " FIXED: " + ex.Message + Environment.NewLine);
        //                    }
        //                    catch (Exception ex2)
        //                    {
        //                        File.AppendAllText(physicalPath + "\\exception_servertoclient.txt", DateTime.Now.ToStr() + " NOTFIXED: " + ex2.Message + Environment.NewLine);
        //                    }
        //                }
        //            }

        //            List<string> listOfConnections = new List<string>();
        //            listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
        //            Clients.Clients(listOfConnections).sendMessage(Instance.listofJobs[0].JobMessage.ToStr());

        //        }
        //        else if (values[4].ToInt() == eMessageTypes.JOB)
        //        {
        //            try
        //            {
        //                if (Instance.listofJobs.Count(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong() && c.MessageTypeId == eMessageTypes.RECALLJOB) > 0)
        //                    Instance.listofJobs.RemoveAll(c => c.JobId != 0 && c.JobId != values[1].ToLong() && c.MessageTypeId == eMessageTypes.RECALLJOB);

        //                if (Instance.listofJobs.Count(c => c.JobId == values[1].ToLong() && c.DriverId == values[2].ToInt()) > 0)
        //                    Instance.listofJobs.RemoveAll(c => c.JobId == values[1].ToLong() && c.DriverId == values[2].ToInt());

        //                Instance.listofJobs.Add(new clsPDA
        //                {
        //                    JobId = values[1].ToLong(),
        //                    DriverId = values[2].ToInt(),
        //                    MessageDateTime = DateTime.Now,
        //                    JobMessage = values[3].ToStr().Trim(),
        //                    MessageTypeId = values[4].ToInt(),
        //                    DriverNo = values[5].ToStr()
        //                });
        //            }
        //            catch (Exception ex)
        //            {
        //                try
        //                {
        //                    Thread.Sleep(100);

        //                    if (Instance.listofJobs.Count(c => c.JobId == values[1].ToLong() && c.DriverId == values[2].ToInt()) > 0)
        //                        Instance.listofJobs.RemoveAll(c => c.JobId == values[1].ToLong() && c.DriverId == values[2].ToInt());

        //                    Instance.listofJobs.Add(new clsPDA
        //                    {
        //                        JobId = values[1].ToLong(),
        //                        DriverId = values[2].ToInt(),
        //                        MessageDateTime = DateTime.Now,
        //                        JobMessage = values[3].ToStr().Trim(),
        //                        MessageTypeId = values[4].ToInt(),
        //                        DriverNo = values[5].ToStr()
        //                    });

        //                    File.AppendAllText(physicalPath + "\\exception_servertoclient.txt", DateTime.Now.ToStr() + " FIXED: " + ex.Message + Environment.NewLine);
        //                }
        //                catch (Exception ex2)
        //                {
        //                    try
        //                    {
        //                        File.AppendAllText(physicalPath + "\\exception_servertoclient.txt", DateTime.Now.ToStr() + " NOTFIXED: " + ex2.Message + Environment.NewLine);
        //                    }
        //                    catch
        //                    {

        //                    }
        //                }
        //            }

        //            List<string> listOfConnections = new List<string>();
        //            listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
        //            Clients.Clients(listOfConnections).despatchBooking(Instance.listofJobs[0].JobMessage.ToStr());
        //        }
        //        else if (values[4].ToInt() == eMessageTypes.CLEAREDJOB)
        //        {
        //            Instance.listofJobs.Add(new clsPDA
        //            {
        //                DriverId = values[1].ToInt(),
        //                JobId = values[2].ToLong(),
        //                MessageDateTime = DateTime.Now.AddSeconds(-30),
        //                JobMessage = values[3].ToStr().Trim(),
        //                MessageTypeId = values[4].ToInt()
        //            });

        //            List<string> listOfConnections = new List<string>();
        //            listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
        //            Clients.Clients(listOfConnections).forceClearJob(Instance.listofJobs[0].JobMessage.ToStr());
        //        }
        //        else if (values[4].ToInt() == eMessageTypes.RECALLJOB)
        //        {
        //            Instance.listofJobs.Add(new clsPDA
        //            {
        //                DriverId = values[1].ToInt(),
        //                JobId = values[2].ToLong(),
        //                MessageDateTime = DateTime.Now.AddSeconds(-30),
        //                JobMessage = values[3].ToStr().Trim(),
        //                MessageTypeId = values[4].ToInt()
        //            });

        //            List<string> listOfConnections = new List<string>();
        //            listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
        //            Clients.Clients(listOfConnections).forceRecoverJob(Instance.listofJobs[0].JobMessage.ToStr());
        //        }
        //        else if (values[4].ToInt() == eMessageTypes.UPDATEJOB)
        //        {
        //            Instance.listofJobs.Add(new clsPDA
        //            {
        //                DriverId = values[1].ToInt(),
        //                JobId = values[2].ToLong(),
        //                MessageDateTime = DateTime.Now.AddSeconds(-15),
        //                JobMessage = values[3].ToStr(),
        //                MessageTypeId = values[4].ToInt()
        //            });

        //            List<string> listOfConnections = new List<string>();
        //            listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
        //            Clients.Clients(listOfConnections).updateJob(Instance.listofJobs[0].JobMessage.ToStr());
        //        }
        //        else if (values[4].ToInt() == eMessageTypes.AUTHORIZATION)
        //        {
        //            Instance.listofJobs.Add(new clsPDA
        //            {
        //                DriverId = values[1].ToInt(),
        //                JobId = values[2].ToLong(),
        //                MessageDateTime = DateTime.Now.AddSeconds(-30),
        //                JobMessage = values[3].ToStr().Replace(">>", "=").Trim(),
        //                MessageTypeId = values[4].ToInt()
        //            });

        //            List<string> listOfConnections = new List<string>();
        //            listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
        //            Clients.Clients(listOfConnections).sendMessage(Instance.listofJobs[0].JobMessage.ToStr());
        //        }
        //        else if (values[4].ToInt() == eMessageTypes.BIDALERT)
        //        {
        //            Instance.listofJobs.Add(new clsPDA
        //            {
        //                DriverId = values[1].ToInt(),
        //                JobId = values[2].ToLong(),
        //                MessageDateTime = DateTime.Now.AddSeconds(-15),
        //                JobMessage = values[3].ToStr(),
        //                MessageTypeId = values[4].ToInt()
        //            });

        //            List<string> listOfConnections = new List<string>();
        //            listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
        //            Clients.Clients(listOfConnections).sendMessage(Instance.listofJobs[0].JobMessage.ToStr());
        //        }
        //        else if (values[4].ToInt() == eMessageTypes.BIDPRICEALERT)
        //        {
        //            string[] driverIds = values[1].ToStr().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

        //            Instance.listofJobs.AddRange((from a in driverIds
        //                                 select new clsPDA
        //                                 {
        //                                     DriverId = a.ToInt(),
        //                                     JobId = values[2].ToLong(),
        //                                     MessageDateTime = DateTime.Now.AddSeconds(-30),
        //                                     JobMessage = values[3].ToStr(),
        //                                     MessageTypeId = values[4].ToInt()

        //                                 }));

        //            List<string> listOfConnections = new List<string>();
        //            listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
        //            Clients.Clients(listOfConnections).sendMessage(Instance.listofJobs[0].JobMessage.ToStr());
        //        }
        //        else if (values[4].ToInt() == eMessageTypes.UPDATEPLOT)
        //        {
        //            Instance.listofJobs.Add(new clsPDA
        //            {
        //                DriverId = values[1].ToInt(),
        //                JobId = values[2].ToLong(),
        //                MessageDateTime = DateTime.Now.AddSeconds(-15),
        //                JobMessage = values[3].ToStr(),
        //                MessageTypeId = values[4].ToInt()
        //            });

        //            List<string> listOfConnections = new List<string>();
        //            listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
        //            Clients.Clients(listOfConnections).sendMessage(Instance.listofJobs[0].JobMessage.ToStr());
        //        }
        //        else if (values[4].ToInt() == eMessageTypes.LOGOUTAUTHORIZATION)
        //        {
        //            Instance.listofJobs.Add(new clsPDA
        //            {
        //                DriverId = values[1].ToInt(),
        //                JobId = 0,
        //                MessageDateTime = DateTime.Now.AddSeconds(-45),
        //                JobMessage = values[3].ToStr(),
        //                MessageTypeId = values[4].ToInt()
        //            });

        //            List<string> listOfConnections = new List<string>();
        //            listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
        //            Clients.Clients(listOfConnections).sendMessage(Instance.listofJobs[0].JobMessage.ToStr());
        //        }
        //        else if (values[4].ToInt() == eMessageTypes.FORCE_ACTION_BUTTON)
        //        {
        //            Instance.listofJobs.Add(new clsPDA
        //            {
        //                DriverId = values[1].ToInt(),
        //                JobId = values[2].ToLong(),
        //                MessageDateTime = DateTime.Now.AddSeconds(-45),
        //                JobMessage = values[3].ToStr(),
        //                MessageTypeId = values[4].ToInt()
        //            });

        //            List<string> listOfConnections = new List<string>();
        //            listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
        //            Clients.Clients(listOfConnections).sendMessage(Instance.listofJobs[0].JobMessage.ToStr());
        //        }
        //        else if (values[4].ToInt() == eMessageTypes.UPDATE_SETTINGS)
        //        {
        //            Instance.listofJobs.Add(new clsPDA
        //            {
        //                DriverId = values[2].ToInt(),
        //                JobId = 0,
        //                MessageDateTime = DateTime.Now.AddSeconds(-50),
        //                JobMessage = values[3].ToStr().Trim(),
        //                MessageTypeId = values[4].ToInt()
        //            });

        //            List<string> listOfConnections = new List<string>();
        //            listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
        //            Clients.Clients(listOfConnections).sendMessage(Instance.listofJobs[0].JobMessage.ToStr());
        //        }

        //        //send acknowledgement message to desktop
        //        //Clients.Caller.cMessageToDesktop("ok");
        //    }
        //    catch (Exception ex)
        //    {
        //        File.AppendAllText(physicalPath + "\\exception_servertoclient.txt", DateTime.Now.ToStr() + " FIXED: " + ex.Message + Environment.NewLine);
        //        Clients.Caller.cMessageToDesktop("exceptionOccured" + ex.Message);
        //    }
        //}

        private void requestPDA(string mesg)
        {
            try
            {

                try
                {
                    File.AppendAllText(physicalPath + "\\requestPDA.txt", DateTime.Now.ToStr() + ":msg:" + mesg + Environment.NewLine);
                }
                catch
                {

                }

                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });
                int ddId = 0;
                if (values[4].ToInt() == eMessageTypes.MESSAGING)
                {
                    if (values[1].ToStr().Contains(","))
                    {
                        DateTime dt = DateTime.Now.AddSeconds(-45);
                        foreach (string dId in values[1].Split(','))
                        {

                            string recordId = Guid.NewGuid().ToString();

                            Instance.listofJobs.Add(new clsPDA
                            {
                                DriverId = dId.ToInt(),
                                JobId = values[2].ToLong(),
                                MessageDateTime = dt,
                                JobMessage = values[3].ToStr().Trim(),
                                MessageTypeId = values[4].ToInt(),
                                Id = recordId
                            });

                            SocketIO.SendToSocket(dId.ToStr(), values[3].ToStr(), "sendMessage", "", recordId);
                        }
                    }
                    else
                    {
                        try
                        {
                            string recordId = Guid.NewGuid().ToString();

                            Instance.listofJobs.Add(new clsPDA
                            {
                                DriverId = values[1].ToInt(),
                                JobId = values[2].ToLong(),
                                MessageDateTime = DateTime.Now.AddSeconds(-45),
                                JobMessage = values[3].ToStr().Trim(),
                                MessageTypeId = values[4].ToInt(),
                                Id = recordId
                            });


                            try
                            {
                                SocketIO.SendToSocket(values[1].ToStr(), values[3].ToStr(), "sendMessage", "", recordId);

                                File.AppendAllText(AppContext.BaseDirectory + "\\requestpda.txt", DateTime.Now.ToStr() + ": recordid :" + recordId + Environment.NewLine);
                            }
                            catch (Exception ex)
                            {



                            }

                            try
                            {

                                File.AppendAllText(AppContext.BaseDirectory + "\\requestpda.txt", DateTime.Now.ToStr() + ": listofjob.count :" + Instance.listofJobs.Count + " :  ConnectionID = " + Instance.ReturnDriverConnections(values[1].ToInt()).FirstOrDefault() + Environment.NewLine);
                            }
                            catch (Exception ex)
                            {



                            }

                            ddId = values[1].ToInt();

                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                Thread.Sleep(100);

                                Instance.listofJobs.Add(new clsPDA
                                {
                                    DriverId = values[1].ToInt(),
                                    JobId = values[2].ToLong(),
                                    MessageDateTime = DateTime.Now.AddSeconds(-45),
                                    JobMessage = values[3].ToStr().Trim(),
                                    MessageTypeId = values[4].ToInt()
                                });

                                File.AppendAllText(physicalPath + "\\exception_servertoclient.txt", DateTime.Now.ToStr() + " FIXED: " + ex.Message + Environment.NewLine);
                            }
                            catch (Exception ex2)
                            {
                                File.AppendAllText(physicalPath + "\\exception_servertoclient.txt", DateTime.Now.ToStr() + " NOTFIXED: " + ex2.Message + Environment.NewLine);
                            }
                        }
                    }

                    //if (ddId > 0)
                    //{

                    //    clsPDA objc = Instance.listofJobs.LastOrDefault(c => c.DriverId == ddId);

                    //    if (objc != null)
                    //    {
                    //        List<string> listOfConnections = new List<string>();
                    //        listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(objc.DriverId));
                    //        Clients.Clients(listOfConnections).sendMessage(objc.JobMessage.ToStr());


                    //        Instance.listofJobs.Remove(objc);


                    //    }
                    //}
                    //else
                    // {
                    //     List<string> listOfConnections = new List<string>();
                    //     listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
                    //     Clients.Clients(listOfConnections).sendMessage(Instance.listofJobs[0].JobMessage.ToStr());


                    // }

                }
                else if (values[4].ToInt() == eMessageTypes.JOB)
                {
                    int drvId = values[2].ToInt();
                    try
                    {
                        try
                        {
                            if (Instance.listofJobs.Count(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong() && c.MessageTypeId == eMessageTypes.RECALLJOB) > 0)
                                Instance.listofJobs.RemoveAll(c => c.JobId != 0 && c.JobId != values[1].ToLong() && c.MessageTypeId == eMessageTypes.RECALLJOB);

                            if (Instance.listofJobs.Count(c => c.JobId == values[1].ToLong() && c.DriverId == values[2].ToInt()) > 0)
                                Instance.listofJobs.RemoveAll(c => c.JobId == values[1].ToLong() && c.DriverId == values[2].ToInt());
                        }
                        catch
                        {

                        }


                        string recordId = Guid.NewGuid().ToString();

                        Instance.listofJobs.Add(new clsPDA
                        {
                            JobId = values[1].ToLong(),
                            DriverId = values[2].ToInt(),
                            MessageDateTime = DateTime.Now,
                            JobMessage = values[3].ToStr().Trim(),
                            MessageTypeId = values[4].ToInt(),
                            DriverNo = values[5].ToStr(),
                            Id = recordId
                        });


                        SocketIO.SendToSocket(values[2].ToStr(), values[3].ToStr(), "despatchBooking", "", recordId);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            Thread.Sleep(10);

                            if (Instance.listofJobs.Count(c => c.JobId == values[1].ToLong() && c.DriverId == values[2].ToInt()) > 0)
                                Instance.listofJobs.RemoveAll(c => c.JobId == values[1].ToLong() && c.DriverId == values[2].ToInt());

                            string recordId = Guid.NewGuid().ToString();

                            Instance.listofJobs.Add(new clsPDA
                            {
                                JobId = values[1].ToLong(),
                                DriverId = values[2].ToInt(),
                                MessageDateTime = DateTime.Now,
                                JobMessage = values[3].ToStr().Trim(),
                                MessageTypeId = values[4].ToInt(),
                                DriverNo = values[5].ToStr(),
                                Id = recordId
                            });

                            SocketIO.SendToSocket(values[2].ToStr(), values[3].ToStr(), "despatchBooking", "", recordId);
                            File.AppendAllText(physicalPath + "\\requestpda_despatchjob_exceptionhandle.txt", DateTime.Now.ToStr() + " FIXED: " + ex.Message + Environment.NewLine);
                        }
                        catch (Exception ex2)
                        {
                            try
                            {
                                File.AppendAllText(physicalPath + "\\requestpda_despatchjob_exceptionhandlefailed.txt", DateTime.Now.ToStr() + " NOTFIXED: " + ex2.Message + Environment.NewLine);
                            }
                            catch
                            {

                            }
                        }
                    }

                    //List<string> listOfConnections = new List<string>();
                    ////  listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
                    //// Clients.Clients(listOfConnections).despatchBooking(Instance.listofJobs[0].JobMessage.ToStr());
                    //listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs.LastOrDefault(c => c.DriverId == drvId).DriverId));
                    //Clients.Clients(listOfConnections).despatchBooking(Instance.listofJobs.LastOrDefault(c => c.DriverId == drvId).JobMessage.ToStr());
                }
                else if (values[4].ToInt() == eMessageTypes.CLEAREDJOB)
                {

                    string recordId = Guid.NewGuid().ToString();
                    try
                    {
                        Instance.listofJobs.Add(new clsPDA
                        {
                            DriverId = values[1].ToInt(),
                            JobId = values[2].ToLong(),
                            MessageDateTime = DateTime.Now.AddSeconds(-30),
                            JobMessage = values[3].ToStr().Trim(),
                            MessageTypeId = values[4].ToInt(),
                            Id = recordId
                        });


                        File.AppendAllText(physicalPath + "\\forceclearjob.txt", DateTime.Now.ToStr() + ":driverid:" + values[1].ToInt() + ",jobid:" + values[2].ToLong() + Environment.NewLine);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            Thread.Sleep(100);

                            Instance.listofJobs.Add(new clsPDA
                            {
                                DriverId = values[1].ToInt(),
                                JobId = values[2].ToLong(),
                                MessageDateTime = DateTime.Now.AddSeconds(-30),
                                JobMessage = values[3].ToStr().Trim(),
                                MessageTypeId = values[4].ToInt(),
                                Id = recordId
                            });

                            File.AppendAllText(physicalPath + "\\exception_servertoclient.txt", DateTime.Now.ToStr() + "force clear job FIXED: " + ex.Message + Environment.NewLine);
                        }
                        catch (Exception ex2)
                        {
                            File.AppendAllText(physicalPath + "\\exception_servertoclient.txt", DateTime.Now.ToStr() + "force clear job NOTFIXED: " + ex2.Message + Environment.NewLine);
                        }
                    }

                    SocketIO.SendToSocket(values[1].ToStr(), values[3].ToStr(), "forceClearJob", recordId);
                    //List<string> listOfConnections = new List<string>();
                    //listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
                    //Clients.Clients(listOfConnections).forceClearJob(Instance.listofJobs[0].JobMessage.ToStr());
                }
                else if (values[4].ToInt() == eMessageTypes.RECALLJOB)
                {

                    string recordId = Guid.NewGuid().ToString();
                    try
                    {

                        Instance.listofJobs.Add(new clsPDA
                        {
                            DriverId = values[1].ToInt(),
                            JobId = values[2].ToLong(),
                            MessageDateTime = DateTime.Now.AddSeconds(-30),
                            JobMessage = values[3].ToStr().Trim(),
                            MessageTypeId = values[4].ToInt(),
                            Id = recordId
                        });

                        try
                        {

                            if (values.Count() > 5 && values[5] == "1")
                            {

                                BroadCastPostionChanged(values[1].ToInt());
                            }
                        }
                        catch
                        {

                        }

                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            Thread.Sleep(100);

                            Instance.listofJobs.Add(new clsPDA
                            {
                                DriverId = values[1].ToInt(),
                                JobId = values[2].ToLong(),
                                MessageDateTime = DateTime.Now.AddSeconds(-30),
                                JobMessage = values[3].ToStr().Trim(),
                                MessageTypeId = values[4].ToInt(),
                                Id = recordId
                            });


                            File.AppendAllText(physicalPath + "\\exception_servertoclient.txt", DateTime.Now.ToStr() + "recover job FIXED: " + ex.Message + Environment.NewLine);
                        }
                        catch (Exception ex2)
                        {
                            File.AppendAllText(physicalPath + "\\exception_servertoclient.txt", DateTime.Now.ToStr() + "recover job NOTFIXED: " + ex2.Message + Environment.NewLine);
                        }
                    }


                    try
                    {
                        CallSupplierApi.UpdateStatus(values[2].ToLong(), Enums.BOOKINGSTATUS.NOSHOW.ToInt());
                    }
                    catch
                    { }
                    //List<string> listOfConnections = new List<string>();
                    //listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
                    //Clients.Clients(listOfConnections).forceRecoverJob(Instance.listofJobs[0].JobMessage.ToStr());

                    SocketIO.SendToSocket(values[1].ToStr(), values[3].ToStr(), "forceRecoverJob", recordId);
                }
                else if (values[4].ToInt() == eMessageTypes.UPDATEJOB)
                {
                    string recordId = Guid.NewGuid().ToString();
                    int drvId = values[1].ToInt();

                    try
                    {
                        Instance.listofJobs.Add(new clsPDA
                        {
                            DriverId = values[1].ToInt(),
                            JobId = values[2].ToLong(),
                            MessageDateTime = DateTime.Now.AddSeconds(-15),
                            JobMessage = values[3].ToStr(),
                            MessageTypeId = values[4].ToInt(),
                            Id = recordId
                        });
                    }
                    catch
                    {
                        try
                        {
                            Thread.Sleep(200);

                            Instance.listofJobs.Add(new clsPDA
                            {
                                DriverId = values[1].ToInt(),
                                JobId = values[2].ToLong(),
                                MessageDateTime = DateTime.Now.AddSeconds(-15),
                                JobMessage = values[3].ToStr(),
                                MessageTypeId = values[4].ToInt(),
                                Id = recordId
                            });
                        }
                        catch
                        {

                        }

                    }

                    //List<string> listOfConnections = new List<string>();
                    //listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs.LastOrDefault(c => c.DriverId == drvId).DriverId));
                    //Clients.Clients(listOfConnections).updateJob(Instance.listofJobs.LastOrDefault(c => c.DriverId == drvId).JobMessage.ToStr());


                    SocketIO.SendToSocket(values[1].ToStr(), values[3].ToStr(), "updateJob", recordId);
                }
                else if (values[4].ToInt() == eMessageTypes.AUTHORIZATION)
                {
                    try
                    {
                        Instance.listofJobs.Add(new clsPDA
                        {
                            DriverId = values[1].ToInt(),
                            JobId = values[2].ToLong(),
                            MessageDateTime = DateTime.Now.AddSeconds(-30),
                            JobMessage = values[3].ToStr().Replace(">>", "=").Trim(),
                            MessageTypeId = values[4].ToInt()
                        });


                        //List<string> listOfConnections = new List<string>();
                        //listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
                        //Clients.Clients(listOfConnections).sendMessage(Instance.listofJobs[0].JobMessage.ToStr());
                        File.AppendAllText(physicalPath + "\\authorixation.txt", DateTime.Now.ToStr() + " mesg: " + mesg + Environment.NewLine);
                    }
                    catch
                    {

                    }
                    SocketIO.SendToSocket(values[1].ToStr(), values[3].ToStr().Replace(">>", "=").Trim(), "authStatus");
                }
                else if (values[4].ToInt() == eMessageTypes.BIDALERT)
                {
                    Instance.listofJobs.Add(new clsPDA
                    {
                        DriverId = values[1].ToInt(),
                        JobId = values[2].ToLong(),
                        MessageDateTime = DateTime.Now.AddSeconds(-15),
                        JobMessage = values[3].ToStr(),
                        MessageTypeId = values[4].ToInt()
                    });

                    //List<string> listOfConnections = new List<string>();
                    //listOfConnections = Instance.ReturnDriverConnections(values[1].ToInt());
                    //Clients.Clients(listOfConnections).bidAlert(values[3].ToStr());
                    ////



                    SocketIO.SendToSocket(values[1].ToStr(), values[3].ToStr(), "bidAlert");
                    //try
                    //{
                    //    File.AppendAllText(physicalPath + "\\bidalert.txt", DateTime.Now.ToStr() + Environment.NewLine);
                    //}
                    //catch
                    //{

                    //}
                }
                else if (values[4].ToInt() == eMessageTypes.BIDPRICEALERT)
                {
                    string[] driverIds = values[1].ToStr().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    Instance.listofJobs.AddRange((from a in driverIds
                                                  select new clsPDA
                                                  {
                                                      DriverId = a.ToInt(),
                                                      JobId = values[2].ToLong(),
                                                      MessageDateTime = DateTime.Now.AddSeconds(-30),
                                                      JobMessage = values[3].ToStr(),
                                                      MessageTypeId = values[4].ToInt()

                                                  }));

                    List<string> listOfConnections = new List<string>();
                    listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
                    Clients.Clients(listOfConnections).sendMessage(Instance.listofJobs[0].JobMessage.ToStr());
                }
                else if (values[4].ToInt() == eMessageTypes.UPDATEPLOT)
                {
                    Instance.listofJobs.Add(new clsPDA
                    {
                        DriverId = values[1].ToInt(),
                        JobId = values[2].ToLong(),
                        MessageDateTime = DateTime.Now.AddSeconds(-15),
                        JobMessage = values[3].ToStr(),
                        MessageTypeId = values[4].ToInt()
                    });

                    //List<string> listOfConnections = new List<string>();
                    //listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
                    //Clients.Clients(listOfConnections).sendMessage(Instance.listofJobs[0].JobMessage.ToStr());

                    SocketIO.SendToSocket(values[1].ToStr(), values[3].ToStr(), "sendMessage");
                }
                else if (values[4].ToInt() == eMessageTypes.LOGOUTAUTHORIZATION)
                {
                    try
                    {
                        Instance.listofJobs.Add(new clsPDA
                        {
                            DriverId = values[1].ToInt(),
                            JobId = 0,
                            MessageDateTime = DateTime.Now.AddSeconds(-35),
                            JobMessage = values[3].ToStr(),
                            MessageTypeId = values[4].ToInt()
                        });

                    }
                    catch (Exception ex)
                    {

                        try
                        {
                            File.AppendAllText(physicalPath + "\\logoutauth_exception.txt", DateTime.Now.ToStr() + ", mesg:" + mesg + ",exception:" + ex.Message + Environment.NewLine);
                        }
                        catch
                        {

                        }
                    }

                    try
                    {
                        if (values[3].ToStr().ToLower().Contains("yes"))
                        {

                            using (TaxiDataContext db = new TaxiDataContext())
                            {
                                db.stp_LoginLogoutDriver(values[1].ToInt(), false, null);
                            }
                        }
                    }
                    catch
                    {


                    }

                    SocketIO.SendToSocket(values[1].ToStr(), values[3].ToStr(), "logoutAuthStatus");
                }
                else if (values[4].ToInt() == eMessageTypes.FORCE_ACTION_BUTTON)
                {
                    Instance.listofJobs.Add(new clsPDA
                    {
                        DriverId = values[1].ToInt(),
                        JobId = values[2].ToLong(),
                        MessageDateTime = DateTime.Now.AddSeconds(-45),
                        JobMessage = values[3].ToStr(),
                        MessageTypeId = values[4].ToInt()
                    });

                    //List<string> listOfConnections = new List<string>();
                    //listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
                    //Clients.Clients(listOfConnections).sendMessage(Instance.listofJobs[0].JobMessage.ToStr());

                    if (values[3].ToStr().ToStr().Contains("<<Arrive Job>>"))
                    {
                        SocketIO.SendToSocket(values[1].ToStr(), values[3].ToStr(), "forceArriveJob");

                    }
                    else if (values[3].ToStr().ToStr().Contains("<<POB Job>>"))
                    {
                        SocketIO.SendToSocket(values[1].ToStr(), values[3].ToStr(), "forcePobJob");

                    }
                }
                else if (values[4].ToInt() == eMessageTypes.UPDATE_SETTINGS)
                {
                    Instance.listofJobs.Add(new clsPDA
                    {
                        DriverId = values[2].ToInt(),
                        JobId = 0,
                        MessageDateTime = DateTime.Now.AddSeconds(-50),
                        JobMessage = values[3].ToStr().Trim(),
                        MessageTypeId = values[4].ToInt()
                    });




                    //List<string> listOfConnections = new List<string>();
                    //listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
                    //Clients.Clients(listOfConnections).sendMessage(Instance.listofJobs[0].JobMessage.ToStr());

                    SocketIO.SendToSocket(values[2].ToStr(), values[3].ToStr(), "updateSetting");

                    try
                    {

                        File.AppendAllText(AppContext.BaseDirectory + "\\updateSetting.txt", DateTime.Now.ToStr() + ": msg :" + values[3].ToStr() + Environment.NewLine);
                    }
                    catch (Exception ex)
                    {



                    }
                }

                else if (mesg.Contains("auth status>>yes>>") || mesg.Contains("auth status>>no>>"))
                {


                    File.AppendAllText(physicalPath + "\\authstatus.txt", DateTime.Now.ToStr() + " mesg: " + mesg + Environment.NewLine);
                    authStatus(mesg, "0");


                    Instance.listofJobs.Add(new clsPDA
                    {
                        DriverId = values[1].ToInt(),
                        JobId = values[2].ToLong(),
                        MessageDateTime = DateTime.Now.AddSeconds(-30),
                        JobMessage = values[3].Split(new String[] { ">>" }, StringSplitOptions.RemoveEmptyEntries)[1].ToStr().Trim(),
                        MessageTypeId = values[4].ToInt()
                    });


                }

                //send acknowledgement message to desktop
                //Clients.Caller.cMessageToDesktop("ok");
            }
            catch (Exception ex)
            {
                // File.AppendAllText(physicalPath + "\\exception_servertoclient.txt", DateTime.Now.ToStr() + " FIXED: " + ex.Message + Environment.NewLine);
                // Clients.Caller.cMessageToDesktop("exceptionOccured" + ex.Message);
            }
        }



        private void logoutAuthStatus(string mesg, string driverID)
        {
            try
            {
                byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                List<string> listOfConnections = new List<string>();
                listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(driverID));
                Clients.Clients(listOfConnections).logoutAuthStatus(values[3]);

                //send acknowledgement message to desktop
                Clients.Caller.cMessageToDesktop("ok");
            }
            catch (Exception ex)
            {
                Clients.Caller.cMessageToDesktop("exceptionOccured" + ex.Message);
            }
        }

        private void authStatus(string mesg, string driverID)
        {
            try
            {
                byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

                string dataValue = mesg.Replace("auth status>>", "");
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                List<string> listOfConnections = new List<string>();
                listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(driverID));
                Clients.Clients(listOfConnections).authStatus(values[3]);

                //send acknowledgement message to desktop
                Clients.Caller.cMessageToDesktop("ok");

                try
                {
                    if (Instance.objPolicy.EnableBookingOtherCharges.ToBool() && values.Count() > 5 && mesg.EndsWith("=13") && values[5].ToStr() == "13")
                    {
                        int driverId = Convert.ToInt32(driverID);

                        BroadCastPostionChanged(driverId);
                    }
                }
                catch
                {

                }


                try
                {
                    if (values.Count() > 5 && values[5] == "13")
                    {


                        CallSupplierApi.UpdateStatus(values[2].ToLong(), 2);
                    }
                    else
                    {
                        CallSupplierApi.UpdateStatus(values[2].ToLong(), Enums.BOOKINGSTATUS.NOSHOW.ToInt());
                    }
                }
                catch
                {

                }
            }
            catch (Exception ex)
            {
                Clients.Caller.cMessageToDesktop("exceptionOccured" + ex.Message);
            }
        }

        public void requestTemplates(string mesg)
        {
            try
            {
                string resp = string.Empty;
                using (TaxiDataContext db = new TaxiDataContext())
                {
                    string[] arr = db.Fleet_DriverTemplets.Select(args => args.Templets).OrderBy(c => c)
                               .ToArray<string>();

                    resp = new JavaScriptSerializer().Serialize(arr);
                }

                //send message back to PDA
                Clients.Caller.templates(resp);

                //Byte[] byteResponse = Encoding.UTF8.GetBytes(resp);
                //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);
            }
            catch (Exception ex)
            {
                Clients.Caller.templates(ex.Message);
            }
        }

        public void onRoadJob(string mesg)
        {
            try
            {
                byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                var res = (new TaxiDataContext()).stp_InsertOnRoadJob(values[1].ToStr(), values[2].ToStr(), values[3].ToDecimal(), values[4].ToStr()
                                , values[5].ToStr(), values[6].ToInt(), "");

                long jobId = 0;

                if (res != null)
                    jobId = res.FirstOrDefault().jobid.ToLong();

                //send message back to PDA
                Clients.Caller.onRoadJob(jobId.ToStr());

                //Byte[] byteResponse = Encoding.UTF8.GetBytes(jobId.ToStr());
                //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);

                General.BroadCastMessage("**action>>" + jobId + ">>" + values[6].ToStr() + ">>" + Enums.BOOKINGSTATUS.POB);
            }
            catch (Exception ex)
            {
                Clients.Caller.onRoadJob(ex.Message);
            }
        }

        public void updateFlagDown(string mesg)
        {
            byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

            string dataValue = mesg;
            dataValue = dataValue.Trim();

            string[] values = dataValue.Split(new char[] { '=' });

            try
            {
                long jobId = values[1].ToLong();
                int driverId = values[2].ToInt();
                string custName = values[3].ToStr().Trim();
                string mobNo = values[4].ToStr().Trim();
                string pickup = values[5].ToStr().Trim();
                string destination = values[6].ToStr().Trim();
                int totalPax = values[7].ToInt();

                if (jobId > 0)
                {
                    if (pickup.ToStr().Trim().Contains("<<<"))
                        pickup = destination.Substring(0, pickup.IndexOf("<<<")).Trim();

                    if (destination.ToStr().Trim().Contains("<<<"))
                        destination = destination.Substring(0, destination.IndexOf("<<<")).Trim();

                    pickup = pickup.ToStr().ToUpper();
                    destination = destination.ToStr().ToUpper();

                    bool IsUpdated = false;

                    if (pickup.Length > 0 && destination.Length > 0)
                    {
                        IsUpdated = true;

                        using (TaxiDataContext db = new TaxiDataContext())
                        {
                            db.stp_RunProcedure("update booking set  customername='" + custName + "',CustomerMobileNo='" + mobNo + "',fromaddress='" + pickup + "',toAddress='" + destination + "',NoofPassengers=" + totalPax + " where Id=" + jobId);
                        }
                    }

                    if (custName.ToStr().Trim().Length == 0)
                        custName = "UNKNOWN";

                    if (pickup.Length > 0)
                    {
                        IsUpdated = true;

                        using (TaxiDataContext db = new TaxiDataContext())
                        {
                            db.stp_RunProcedure("update booking set  customername='" + custName + "',CustomerMobileNo='" + mobNo + "',fromaddress='" + pickup + "',NoofPassengers=" + totalPax + " where Id=" + jobId);
                        }
                    }

                    if (destination.Length > 0)
                    {
                        IsUpdated = true;
                        using (TaxiDataContext db = new TaxiDataContext())
                        {
                            db.stp_RunProcedure("update booking set  customername='" + custName + "',CustomerMobileNo='" + mobNo + "',toAddress='" + destination + "',NoofPassengers=" + totalPax + " where Id=" + jobId);
                        }
                    }

                    if (destination.Length == 0 && pickup.Length == 0 && IsUpdated == false)
                    {
                        using (TaxiDataContext db = new TaxiDataContext())
                        {
                            db.stp_RunProcedure("update booking set  NoofPassengers=" + totalPax + " where Id=" + jobId);
                        }
                    }

                    //send message back to PDA
                    Clients.Caller.updateFlagDown("true");

                    //Byte[] byteResponse = Encoding.UTF8.GetBytes("true");
                    //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);
                }
            }
            catch (Exception ex)
            {
                Clients.Caller.updateFlagDown(ex.Message);
            }
        }

        public void requestMeter(string mesg)
        {
            try
            {
                byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                JobMeter objAction = new JavaScriptSerializer().Deserialize<JobMeter>(values[1].ToStr());

                string response = string.Empty;
                decimal rtnFares = 0.00m;

                decimal miles = objAction.Miles.ToDecimal();
                string IsWaiting = objAction.IsWaiting.ToStr();
                decimal waitingCharges = objAction.WaitingCharges.ToDecimal();
                int waitingTime = objAction.WaitingTime.ToInt();
                string vehicleType = objAction.VehicleType.ToStr();
                //decimal waitingSpeed = objAction.WaitingSpeed.ToDecimal();
                int SpeedSecs = objAction.SpeedSecs.ToInt();

                DateTime pickupDate;

                if (objAction.PickupDateTime.ToStr().Trim().Length > 0)
                {
                    string pickupDateTime = string.Format("{0:HH:mm}", objAction.PickupDateTime.ToStr().Trim().Replace("  ", " ").Trim());
                    DateTime.TryParse(pickupDateTime, out pickupDate);
                }
                else
                {
                    pickupDate = DateTime.Now;
                }

                var objFareMeter = Global.listofMeter.FirstOrDefault(c => c.VehicleType.ToLower() == vehicleType.ToLower()).DefaultIfEmpty();

                //DateTime dateValue = new DateTime(1900, 1, 1, 0, 0, 0);
                //pickupDate = string.Format("{0:dd/MM/yyyy HH:mm}", dateValue.ToDate() + pickupDate.TimeOfDay).ToDateTime();

                if (miles > 0 && Instance.objPolicy.RoundJourneyMiles.ToDecimal() > 0)
                {
                    miles = Math.Ceiling(miles / Instance.objPolicy.RoundJourneyMiles.ToDecimal()) * Instance.objPolicy.RoundJourneyMiles.ToDecimal();
                }

                if (objAction.Speed.ToDecimal() > 0 || objAction.Fares.ToDecimal() == 0 || objAction.IsWaiting == "1")
                {
                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        try
                        {
                            var objFare = db.stp_CalculateMeterFares(objFareMeter.VehicleTypeId, objAction.CompanyId.ToInt(), miles, pickupDate, objAction.SubCompanyId.ToInt()).FirstOrDefault();

                            if (objFare != null)
                            {
                                rtnFares = objFare.totalFares.ToDecimal();

                                if (Instance.objPolicy.RoundMileageFares.ToBool() == false)
                                {
                                    decimal roundUp = Instance.objPolicy.RoundUpTo.ToDecimal();
                                    if (roundUp > 0)
                                    {
                                        if (objFare.Result.ToStr().IsNumeric() && objFare.CompanyFareExist.ToBool())
                                        {
                                            rtnFares = rtnFares.ToDecimal();
                                        }
                                        else
                                        {
                                            rtnFares = (decimal)Math.Ceiling(rtnFares / roundUp) * roundUp;
                                        }
                                    }
                                }
                                else
                                {
                                    string ff = string.Format("{0:f2}", rtnFares);
                                    if (ff == string.Empty)
                                        ff = "0";

                                    rtnFares = ff.ToDecimal();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                File.AppendAllText(physicalPath + "\\" + "exception_meterstringcatch.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ":" + dataValue + "," + ex.Message + Environment.NewLine);
                            }
                            catch
                            {

                            }
                        }
                    }
                }
                else
                {
                    rtnFares = objAction.Fares.ToDecimal();
                }

                if (objAction.IsWaiting == "1")
                {
                    if (objFareMeter.AutoStopWaitingOnSpeed.ToDecimal() > 0 && objAction.Speed.ToDecimal() >= objFareMeter.AutoStopWaitingOnSpeed.ToDecimal())
                    {
                        IsWaiting = "0";
                    }
                    else
                    {
                        if (waitingTime > 0)
                        {
                            if (objFareMeter.AccWaitingChargesPerMin == null || objFareMeter.AccWaitingChargesPerMin.ToInt() == 0)
                            {
                                decimal waitingMins = waitingTime / 60;
                                waitingMins = Math.Ceiling(waitingMins);
                                waitingCharges = waitingMins * objFareMeter.DrvWaitingChargesPerMin.ToDecimal();
                            }
                            else
                            {
                                decimal waitingMins = Math.Floor((waitingTime / objFareMeter.AccWaitingChargesPerMin.ToDecimal()));
                                //waitingMins = Math.Ceiling(waitingMins);
                                waitingCharges = waitingMins * objFareMeter.DrvWaitingChargesPerMin.ToDecimal();
                            }
                        }
                    }
                }
                else
                {
                    if (objFareMeter.AutoStartWaiting.ToBool() && SpeedSecs >= objFareMeter.AutoStartWaitingBelowSpeedSeconds.ToInt())
                    {
                        //
                        IsWaiting = "1";
                    }
                }

                objAction.WaitingSpeed = objFareMeter.AutoStartWaitingBelowSpeed.ToDecimal();

                if (rtnFares < objAction.Fares.ToDecimal() && rtnFares < (objAction.Fares.ToDecimal() - 2))
                    rtnFares = objAction.Fares.ToDecimal();

                objAction.Fares = rtnFares;
                objAction.IsWaiting = IsWaiting;
                objAction.WaitingTime = waitingTime;
                objAction.WaitingCharges = waitingCharges;

                string res = new JavaScriptSerializer().Serialize(objAction);

                response = "meter=" + res;

                //send message back to PDA
                Clients.Caller.meter(response);

                //Byte[] byteResponse2 = Encoding.UTF8.GetBytes(response);
                //tcpClient.GetStream().Write(byteResponse2, 0, byteResponse2.Length);

                //tcpClient.Close();
                //clientStream.Close();
                //clientStream.Dispose();

                GC.Collect();

                try
                {
                    File.AppendAllText(physicalPath + "\\" + objAction.JobID.ToStr() + ".txt", "logon:" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", miles:" + objAction.Miles + ", fares:" + objAction.Fares + ",waitingtime:" + objAction.WaitingTime.ToInt() + ",speed:" + objAction.Speed.ToStr() + ",lat lng:" + objAction.lg.ToStr() + "," + objAction.lt.ToStr() + Environment.NewLine);
                }
                catch
                {

                }
            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(physicalPath + "\\" + "exception_faremeter.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", data:" + mesg + ":" + ex.Message + Environment.NewLine);
                    //RestartProgram();
                    Clients.Caller.meter(ex.Message);
                }
                catch (Exception e)
                {
                    Clients.Caller.meter(e.Message);
                }
            }

            return;
        }

        public void requestMeterType(string mesg)
        {
            try
            {
                byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

                string dataValue = mesg;
                dataValue = dataValue.Trim();

                try
                {
                    File.AppendAllText(physicalPath + "\\requestMeterType.txt", DateTime.Now.ToStr() + " request: " + mesg + Environment.NewLine);
                }
                catch
                {

                }

                string[] values = dataValue.Split(new char[] { '=' });

                ChooseMeterType objAction = new JavaScriptSerializer().Deserialize<ChooseMeterType>(values[1].ToStr());

                string response = string.Empty;

                string vehicle = objAction.drivervehicle.ToStr().ToLower().Trim();


                InitializeMeterList();
                int pax = Global.listofMeter.FirstOrDefault(c => c.VehicleType.ToLower() == vehicle).DefaultIfEmpty().NoofPassengers.ToInt();

                List<string> listofvehicles = Global.listofMeter.Where(c => c.VehicleType.ToLower() != vehicle && c.NoofPassengers <= pax)
                    .Select(c => c.VehicleType.ToUpper()).ToList<string>();

                listofvehicles.Insert(0, objAction.drivervehicle.ToStr().Trim().ToUpper());

                objAction.vehiclelist = listofvehicles.ToArray<string>();

                //if (objAction.vehiclelist.Count() == 0)
                //{
                //    objAction.vehiclelist = new string[1];
                //    objAction.vehiclelist[0] = objAction.drivervehicle.ToStr().Trim();
                //}

                string res = new JavaScriptSerializer().Serialize(objAction);

               
                response = res;

                //send message back to PDA
                Clients.Caller.meterType(response);
               
                GC.Collect();
            }
            catch (Exception ex)
            {
                Clients.Caller.meterType(ex.Message);

                //Byte[] byteResponse2 = Encoding.UTF8.GetBytes("failed");
                //tcpClient.GetStream().Write(byteResponse2, 0, byteResponse2.Length);

                //tcpClient.Close();
                //clientStream.Close();
                //clientStream.Dispose();
                //tcpClient = null;
                GC.Collect();
            }

            return;
        }

        public void requestViadetails(string mesg)
        {
            try
            {
                byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                JobDetails objAction = new JavaScriptSerializer().Deserialize<JobDetails>(values[1].ToStr());

                string name = string.Empty;
                string mobileno = string.Empty;

                long jobId = objAction.JobId.ToLong();
                int driverId = objAction.driverId.ToInt();

                string via = objAction.Address.ToStr().Trim().ToLower();
                //string response = string.Empty;
                try
                {
                    if (via.Length > 0)
                    {
                        using (TaxiDataContext db = new TaxiDataContext())
                        {
                            var obj = db.Booking_ViaLocations.FirstOrDefault(c => c.BookingId == jobId && c.ViaLocValue.Trim().ToLower() == via);

                            if (obj != null)
                            {
                                name = obj.ViaLocTypeLabel.ToStr().Trim();
                                mobileno = obj.ViaLocTypeValue.ToStr().Trim();
                            }
                        }
                    }
                }
                catch
                {

                }

                objAction.Name = name;
                objAction.MobileNo = mobileno;

                string res = new JavaScriptSerializer().Serialize(objAction);

                //send message back to PDA
                Clients.Caller.viadetails(res);

                //Byte[] byteResponse2 = Encoding.UTF8.GetBytes(res);
                //tcpClient.GetStream().Write(byteResponse2, 0, byteResponse2.Length);

                //tcpClient.Close();
                //clientStream.Close();
                //clientStream.Dispose();
                return;
            }
            catch (Exception ex)
            {
                Clients.Caller.viadetails(ex.Message);
            }
        }

        public void requestAsDirected()
        {
            try
            {
                try
                {


                    File.AppendAllText(physicalPath + "\\requestAsDirected.txt", DateTime.Now + Environment.NewLine);
                }
                catch
                { }
                //string[] arr = new TaxiDataContext().ExecuteQuery<clsZones>("select Id,ZoneName from Gen_Zones. where (isprice is null or isprice=0) order by orderno)")
                //    .ToList()                 

                //   .Select(args => args.Id + "," + args.Area).ToArray<string>();


                string[] arr = Instance.listOfZone.OrderBy(c=>c.Area)

                   .Select(args => args.Id + "," + args.Area).ToArray<string>();


                //send message back to PDA
                Clients.Caller.directed(string.Join(">>", arr));

                //Byte[] byteResponse = Encoding.UTF8.GetBytes(string.Join(">>", arr));
                //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);
            }
            catch (Exception ex)
            {
                try
                {
                    Clients.Caller.directed("exceptionoccurred");

                    File.AppendAllText(physicalPath + "\\requestAsDirected_exception.txt", DateTime.Now + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                { }
            }
        }

        public void requestChangeAddress(string mesg)
        {
            try
            {
                byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                string plotName = "unknown";

                int? zoneId = null;

                GetZone(values[5].ToStr().ToUpper().Trim(), ref zoneId, ref plotName);

                // if(zoneId!=null)
                //{
                new TaxiDataContext().stp_UpdateJobAddress(values[1].ToLong(), values[2].ToInt(), values[3].ToStr().Trim(), zoneId, values[5].ToStr().ToUpper(), values[4].ToStr().ToUpper());
                //}           

                //send message back to PDA
                Clients.Caller.changeAddress(plotName);

                //Byte[] byteResponse = Encoding.UTF8.GetBytes(plotName);
                //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);

                General.BroadCastMessage("**changeaddress>>" + values[2].ToInt() + ">>" + values[1].ToLong() + ">>" + values[3].ToStr() + ">>" + values[5].ToStr());
            }
            catch (Exception ex)
            {
                Clients.Caller.changeAddress(ex.Message);
            }
        }

        //public void driverBid(string mesg)
        //{
        //    try
        //    {
        //        byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

        //        string dataValue = mesg;
        //        dataValue = dataValue.Trim();

        //        string[] values = dataValue.Split(new char[] { '=' });

        //        string response = string.Empty;

        //        int zoneId = values[1].ToInt();
        //        //long jobId = values[1].ToLong();
        //        int driverId = values[2].ToInt();

        //        long jobId = 0;
        //        string zoneName = string.Empty;
        //        if (values.Count() >= 5)
        //        {
        //            zoneName = values[4].ToStr().Trim().ToUpper();
        //        }

        //        try
        //        {

        //            if (Instance.objPolicy == null)
        //                Instance.objPolicy = new TaxiDataContext().Gen_SysPolicy_Configurations.FirstOrDefault(c => c.SysPolicyId == 1);

        //            //if (Instance.objPolicy.EnableBidding.ToBool() == false)
        //            //{
        //            //    response = "Failed:";
        //            //}
        //            //else
        //            //{
        //            using (TaxiDataContext db = new TaxiDataContext())
        //            {
        //                db.CommandTimeout = 8;

        //                Taxi_Model.Booking objBooking = null;

        //                DateTime fromPickupDate = DateTime.Now.AddMinutes(-60);
        //                DateTime tillPickupDate = DateTime.Now.AddMinutes(120);

        //                if (zoneId > 0)
        //                {
        //                    var objDriver = db.Fleet_Drivers.FirstOrDefault(c => c.Id == driverId);
        //                    if (objDriver.Fleet_VehicleType.AttributeValues.ToStr().Trim().Length > 0)
        //                    {
        //                        objBooking = db.Bookings.Where(c => (c.PickupDateTime > fromPickupDate && c.PickupDateTime <= tillPickupDate) && (c.IsBidding != null && c.IsBidding == true)
        //                            && (c.BookingStatusId == Enums.BOOKINGSTATUS.BID)
        //                            && (c.ZoneId != null && c.ZoneId == zoneId)
        //                            && (c.Fleet_VehicleType.AttributeValues.Contains("," + objDriver.VehicleTypeId.ToStr() + ","))
        //                            ).OrderBy(c => c.PickupDateTime).FirstOrDefault();
        //                    }
        //                    else
        //                    {
        //                        objBooking = db.Bookings.Where(c => (c.PickupDateTime > fromPickupDate && c.PickupDateTime <= tillPickupDate) && (c.IsBidding != null && c.IsBidding == true)
        //                            && (c.BookingStatusId == Enums.BOOKINGSTATUS.BID)
        //                            && (c.ZoneId != null && c.ZoneId == zoneId)
        //                            ).OrderBy(c => c.PickupDateTime).FirstOrDefault();
        //                    }
        //                }
        //                else if (zoneName.ToStr().Length > 0)
        //                {
        //                    //zoneName=zoneName.Split(new char[]{

        //                    objBooking = db.Bookings.OrderBy(c => c.PickupDateTime).FirstOrDefault(c => (c.PickupDateTime > fromPickupDate && c.PickupDateTime <= tillPickupDate) && (c.IsBidding != null && c.IsBidding == true)
        //                                        && (c.BookingStatusId == Enums.BOOKINGSTATUS.BID)
        //                                            && (c.ZoneId == null && c.FromPostCode != null && c.FromPostCode.Contains(" ") && c.FromPostCode.Substring(0, c.FromPostCode.IndexOf(" ")).Trim() == zoneName));
        //                }

        //                if (objBooking != null)
        //                {
        //                    jobId = objBooking.Id;

        //                    string journey = "O/W";
        //                    if (objBooking.JourneyTypeId.ToInt() == 2)
        //                    {
        //                        journey = "Return";
        //                    }
        //                    else if (objBooking.JourneyTypeId.ToInt() == 3)
        //                    {
        //                        journey = "W/R";
        //                    }

        //                    string IsExtra = (objBooking.CompanyId != null || objBooking.FromLocTypeId == Enums.LOCATION_TYPES.AIRPORT || objBooking.ToLocTypeId == Enums.LOCATION_TYPES.AIRPORT) ? "1" : "0";
        //                    int i = 0;
        //                    string viaP = "";

        //                    if (objBooking.Booking_ViaLocations.Count > 0)
        //                    {
        //                        viaP = "(" + (++i).ToStr() + ")" + string.Join(Environment.NewLine + "(" + (++i).ToStr() + ")", objBooking.Booking_ViaLocations.Select(c => c.ViaLocValue.ToStr()).ToArray<string>());
        //                    }

        //                    string specialRequirements = objBooking.SpecialRequirements.ToStr();
        //                    if (objBooking.SecondaryPaymentTypeId != null && objBooking.CashFares.ToDecimal() > 0)
        //                    {
        //                        specialRequirements += " , Additional Cash Payment : " + objBooking.CashFares.ToDecimal();
        //                    }

        //                    decimal pdafares = objBooking.GetType().GetProperty(Instance.objPolicy.PDAFaresPropertyName.ToStr().Trim()).GetValue(objBooking, null).ToDecimal();

        //                    //pdafares = objBooking.TotalCharges.ToDecimal();

        //                    string msg = string.Empty;

        //                    string mobileNo = objBooking.CustomerMobileNo.ToStr();
        //                    string telNo = objBooking.CustomerPhoneNo.ToStr();

        //                    //Fleet_Driver ObjDriver = General.GetObject<Fleet_Driver>(c => c.Id == driverId);

        //                    //decimal drvPdaVersion = ObjDriver.Fleet_Driver_PDASettings.Count > 0 ? ObjDriver.Fleet_Driver_PDASettings[0].CurrentPdaVersion.ToDecimal() : 9.80m;

        //                    decimal drvPdaVersion = 20.00m;

        //                    if (string.IsNullOrEmpty(mobileNo) && !string.IsNullOrEmpty(telNo))
        //                    {
        //                        mobileNo = telNo;
        //                    }
        //                    else if (!string.IsNullOrEmpty(mobileNo) && !string.IsNullOrEmpty(telNo))
        //                    {
        //                        mobileNo += "/" + telNo;
        //                    }

        //                    if (drvPdaVersion >= 11 && Instance.objPolicy.PDAJobAlertOnly.ToBool() == false)
        //                    {
        //                        string showFaresValue = objBooking.Gen_PaymentType.ShowFaresOnPDA.ToStr().Trim();

        //                        string showFares = ",\"ShowFares\":\"" + showFaresValue + "\"";
        //                        string showSummary = ",\"ShowSummary\":\"" + showFaresValue + "\"";
        //                        //string showSummary = string.Empty;

        //                        string agentDetails = string.Empty;
        //                        string parkingandWaiting = string.Empty;
        //                        if (objBooking.CompanyId != null)
        //                        {
        //                            agentDetails = ",\"AgentFees\":\"" + String.Format("{0:0.00}", objBooking.AgentCommission) + "\"";
        //                            parkingandWaiting = ",\"Parking\":\"" + string.Format("{0:0.00}", objBooking.ParkingCharges) + "\",\"Waiting\":\"" + String.Format("{0:0.00}", objBooking.WaitingCharges) + "\"";
        //                        }
        //                        else
        //                        {
        //                            parkingandWaiting = ",\"Parking\":\"" + string.Format("{0:0.00}", objBooking.CongtionCharges) + "\",\"Waiting\":\"" + String.Format("{0:0.00}", objBooking.MeetAndGreetCharges) + "\"";
        //                            //
        //                        }

        //                        string fromAddress = objBooking.FromAddress.ToStr().Trim();
        //                        string toAddress = objBooking.ToAddress.ToStr().Trim();

        //                        if (objBooking.FromLocTypeId.ToInt() == Enums.LOCATION_TYPES.POSTCODE || objBooking.FromLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT)
        //                        {
        //                            fromAddress = objBooking.FromStreet.ToStr() + " " + objBooking.FromAddress.ToStr();
        //                        }

        //                        if (objBooking.ToLocTypeId.ToInt() == Enums.LOCATION_TYPES.POSTCODE || objBooking.ToLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT)
        //                        {
        //                            toAddress = objBooking.ToStreet.ToStr() + " " + objBooking.ToAddress.ToStr();
        //                        }

        //                        string paymentType = objBooking.Gen_PaymentType.PaymentCategoryId == null ? objBooking.Gen_PaymentType.DefaultIfEmpty().PaymentType.ToStr()
        //                                                        : objBooking.Gen_PaymentType.Gen_PaymentCategory.CategoryName.ToStr();

        //                        string companyName = string.Empty;

        //                        if (drvPdaVersion < 11 && objBooking.CompanyId != null && objBooking.Gen_Company.DefaultIfEmpty().AccountTypeId.ToInt() != Enums.ACCOUNT_TYPE.CASH)
        //                            companyName = objBooking.Gen_Company.DefaultIfEmpty().CompanyName;
        //                        else
        //                            companyName = objBooking.Gen_Company.DefaultIfEmpty().CompanyName.ToStr();

        //                        string pickUpPlot = "";
        //                        string dropOffPlot = "";
        //                        if (drvPdaVersion > 9 && drvPdaVersion != 13.4m)
        //                        {
        //                            pickUpPlot = objBooking.ZoneId != null ? "<<<" + objBooking.Gen_Zone1.DefaultIfEmpty().ZoneName.ToStr() : "";
        //                            dropOffPlot = objBooking.DropOffZoneId != null ? "<<<" + objBooking.Gen_Zone.DefaultIfEmpty().ZoneName.ToStr() : "";
        //                        }

        //                        string fromdoorno = objBooking.FromDoorNo.ToStr().Trim();

        //                        if (drvPdaVersion == 23.50m)
        //                        {
        //                            if (fromdoorno.Length > 0 && fromdoorno.WordCount() > 2 && fromdoorno.Contains(" "))
        //                            {
        //                                try
        //                                {
        //                                    fromdoorno = fromdoorno.Replace(" ", "-");
        //                                }
        //                                catch
        //                                {

        //                                }
        //                            }

        //                            if (fromAddress.ToStr().Trim().Contains("-"))
        //                            {
        //                                fromAddress = fromAddress.Replace("-", "  ");
        //                            }
        //                        }

        //                        response = "JobId:" + "{ \"JobId\" :\"" + objBooking.Id.ToStr() +
        //                        "\", \"Pickup\":\"" + (!string.IsNullOrEmpty(objBooking.FromDoorNo) ? fromdoorno + "-" + fromAddress + pickUpPlot : fromAddress + pickUpPlot) +
        //                        "\", \"Destination\":\"" + (!string.IsNullOrEmpty(objBooking.ToDoorNo) ? objBooking.ToDoorNo + "-" + toAddress + dropOffPlot : toAddress + dropOffPlot) + "\"," +
        //                        "\"PickupDateTime\":\"" + string.Format("{0:dd/MM/yyyy   HH:mm}", objBooking.PickupDateTime) + "\"" +
        //                        ",\"Cust\":\"" + objBooking.CustomerName + "\",\"Mob\":\"" + mobileNo + " " + "\",\"Fare\":\"" + string.Format("{0:0.00}", pdafares) + "\",\"Vehicle\":\"" + objBooking.Fleet_VehicleType.VehicleType + "\",\"Account\":\"" + companyName + " " + "\"" +
        //                        ",\"Lug\":\"" + objBooking.NoofLuggages.ToInt() + "\",\"Passengers\":\"" + objBooking.NoofPassengers.ToInt() + "\",\"Journey\":\"" + journey + "\",\"Payment\":\"" + paymentType + "\",\"Special\":\"" + specialRequirements + " " + "\",\"Extra\":\"" + IsExtra + "\",\"Via\":\"" + viaP + " " + "\"" +
        //                    parkingandWaiting + ",\"DriverFares\":\"" + String.Format("{0:0.00}", objBooking.FareRate) + "\"" +
        //                    agentDetails +
        //                        ",\"Did\":\"" + driverId + "\",\"BabySeats\":\"" + objBooking.BabySeats.ToStr() + "\"" + showFares + showSummary + " }";

        //                        //msg=  FOJJob + startJobPrefix + objBooking.Id +
        //                        //   ":Pickup:" + (!string.IsNullOrEmpty(objBooking.FromDoorNo) ? objBooking.FromDoorNo + "-" + fromAddress + pickUpPlot : fromAddress + pickUpPlot) +
        //                        //   ":Destination:" + (!string.IsNullOrEmpty(objBooking.ToDoorNo) ? objBooking.ToDoorNo + "-" + toAddress + dropOffPlot : toAddress + dropOffPlot) +
        //                        //     ":PickupDateTime:" + string.Format("{0:dd/MM/yyyy   HH:mm}", objBooking.PickupDateTime) +
        //                        //          ":Cust:" + objBooking.CustomerName + ":Mob:" + mobileNo + " " + ":Fare:" + pdafares
        //                        //         + ":Vehicle:" + objBooking.Fleet_VehicleType.VehicleType + ":Account:" + companyName + " " +
        //                        //         ":Lug:" + objBooking.NoofLuggages.ToInt() + ":Passengers:" + objBooking.NoofPassengers.ToInt() + ":Journey:" + journey +
        //                        //         ":Payment:" + paymentType + ":Special:" + specialRequirements + " "
        //                        //         + ":Extra:" + IsExtra + ":Via:" + viaP + " " + ":Did:" + ObjDriver.Id;

        //                        if (response.Contains("\r\n"))
        //                        {
        //                            response = response.Replace("\r\n", " ").Trim();
        //                        }
        //                        else
        //                        {
        //                            if (response.Contains("\n"))
        //                            {
        //                                response = response.Replace("\n", " ").Trim();
        //                            }
        //                        }

        //                        if (response.Contains("&"))
        //                        {
        //                            response = response.Replace("&", "And");
        //                        }

        //                        if (response.Contains(">"))
        //                            response = response.Replace(">", " ");

        //                        if (response.Contains("="))
        //                            response = response.Replace("=", " ");
        //                    }
        //                    else
        //                    {
        //                        response = "JobId:" + objBooking.Id +
        //                            ":Pickup:" + (!string.IsNullOrEmpty(objBooking.FromDoorNo) ? objBooking.FromDoorNo + "-" + objBooking.FromAddress : objBooking.FromAddress) +

        //                            ":Destination:" + (!string.IsNullOrEmpty(objBooking.ToDoorNo) ? objBooking.ToDoorNo + "-" + objBooking.ToAddress : objBooking.ToAddress) +
        //                                ":PickupDateTime:" + string.Format("{0:dd/MM/yyyy      HH:mm}", objBooking.PickupDateTime) +
        //                                    ":Cust:" + objBooking.CustomerName + ":Mob:" + objBooking.CustomerMobileNo.ToStr() + " " + ":Fare:" + objBooking.FareRate
        //                                    + ":Vehicle:" + objBooking.Fleet_VehicleType.VehicleType + ":Account:" + objBooking.Gen_Company.DefaultIfEmpty().CompanyName + " " +
        //                                    ":Lug:" + objBooking.NoofLuggages.ToInt() + ":Passengers:" + objBooking.NoofPassengers.ToInt() + ":Journey:" + journey +
        //                                    ":Payment:" + objBooking.Gen_PaymentType.DefaultIfEmpty().PaymentType.ToStr() + ":Special:" + objBooking.SpecialRequirements.ToStr() + " "
        //                                    + ":Extra:" + IsExtra + ":Via:" + viaP + " " + ":Did:" + driverId;
        //                    }

        //                    if (Instance.objPolicy.BiddingType.ToInt() == Enums.BIDDING_TYPES.FASTEST_FINGER
        //                        || Instance.objPolicy.BiddingType.ToInt() == Enums.BIDDING_TYPES.NEAREST_DRIVER
        //                        || Instance.objPolicy.BiddingType.ToInt() == Enums.BIDDING_TYPES.LONGEST_WAITING_QUEUE)
        //                    {
        //                        DateTime elapedTime = DateTime.Now.AddSeconds(Instance.objPolicy.BiddingElapsedTime.ToInt());

        //                        if (listofDrvBidding == null)
        //                            listofDrvBidding = new List<ClsDriverBid>();

        //                        ClsDriverBid obj = new ClsDriverBid();
        //                        obj.JobMessage = response;
        //                        obj.DriverId = driverId;
        //                        obj.JobZoneId = zoneId;
        //                        obj.JobId = jobId;
        //                        obj.BiddingDateTime = DateTime.Now;
        //                        obj.BiddingType = Instance.objPolicy.BiddingType.ToInt();

        //                        var objFirstBidJob = listofDrvBidding.FirstOrDefault(c => c.JobId == jobId);

        //                        if (objFirstBidJob != null)
        //                        {
        //                            elapedTime = objFirstBidJob.ElapsedTime;
        //                        }

        //                        obj.ElapsedTime = elapedTime;

        //                        //if (this.Instance.objPolicy.BiddingType.ToInt() == Enums.BIDDING_TYPES.FASTEST_FINGER && this.Instance.objPolicy.BiddingElapsedTime.ToInt() == 1)
        //                        //{
        //                        //    try
        //                        //    {
        //                        //        //if (Instance.listofJobs.Count(c => c.JobId == jobId && c.MessageTypeId == eMessageTypes.JOB && c.DriverId != driverId) == 0)
        //                        //        //{
        //                        //            Instance.listofJobs.Add(new clsPDA { DriverId = driverId, JobId = jobId, JobMessage = obj.JobMessage, DriverNo = "", MessageTypeId = eMessageTypes.ONBIDDESPATCH, MessageDateTime = DateTime.Now });
        //                        //        //}
        //                        //        //else
        //                        //        //{
        //                        //        //    IsBid = false;
        //                        //        //}
        //                        //    }
        //                        //    catch
        //                        //    {
        //                        //        Thread.Sleep(500);
        //                        //        Instance.listofJobs.Add(new clsPDA { DriverId = driverId, JobId = jobId, JobMessage = obj.JobMessage, DriverNo = "", MessageTypeId = eMessageTypes.ONBIDDESPATCH, MessageDateTime = DateTime.Now });

        //                        //    }
        //                        //}
        //                        //else
        //                        //{
        //                        listofDrvBidding.Add(obj);
        //                        //      }

        //                        try
        //                        {
        //                            if (Instance.listofJobs.Count(c => c.DriverId == driverId && c.MessageTypeId == eMessageTypes.BIDALERT) > 0)
        //                            {
        //                                Instance.listofJobs.RemoveAll(c => c.DriverId == driverId && c.MessageTypeId == eMessageTypes.BIDALERT);
        //                            }
        //                        }
        //                        catch
        //                        {

        //                        }

        //                        response = "You Bidding Request has been sent successfully!:";

        //                        General.BroadCastMessage("**driver bid>>" + jobId + ">>" + driverId);
        //                    }
        //                }
        //                else
        //                {
        //                    response = "failed:";
        //                }
        //            }
        //            //   }
        //        }
        //        catch (Exception ex)
        //        {
        //            response = "failed:";
        //        }

        //        //send message back to PDA
        //        Clients.Caller.driverBid(response);

        //        //Byte[] byteResponse = Encoding.UTF8.GetBytes(response);
        //        //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);

        //        if (response != "failed:")
        //        {
        //            int? bidStatusId = 2;

        //            if (response == "You Bidding Request has been sent successfully!:")
        //                bidStatusId = 4;

        //            General.SP_SaveBid(jobId, driverId, 0, bidStatusId);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Clients.Caller.driverBid(ex.Message);
        //    }
        //}


        public void requestFareMeter(string mesg)
        {
            byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

            string dataValue = mesg;
            dataValue = dataValue.Trim();

            string[] values = dataValue.Split(new char[] { '=' });

            try
            {
                JobMeter objAction = new JavaScriptSerializer().Deserialize<JobMeter>(values[1].ToStr());

                string response = string.Empty;
                decimal rtnFares = 0.00m;

                decimal miles = objAction.Miles.ToDecimal();
                string IsWaiting = objAction.IsWaiting.ToStr();
                decimal waitingCharges = objAction.WaitingCharges.ToDecimal();
                int waitingTime = objAction.WaitingTime.ToInt();
                string vehicleType = objAction.VehicleType.ToStr();
                //decimal waitingSpeed = objAction.WaitingSpeed.ToDecimal();
                int SpeedSecs = objAction.SpeedSecs.ToInt();

                DateTime pickupDate;

                if (objAction.PickupDateTime.ToStr().Trim().Length > 0)
                {
                    string pickupDateTime = string.Format("{0:HH:mm}", objAction.PickupDateTime.ToStr().Trim().Replace("  ", " ").Trim());
                    DateTime.TryParse(pickupDateTime, out pickupDate);
                }
                else
                {
                    pickupDate = DateTime.Now;
                }

                var objFareMeter = Global.listofMeter.FirstOrDefault(c => c.VehicleType.ToLower() == vehicleType.ToLower()).DefaultIfEmpty();

                //DateTime dateValue = new DateTime(1900, 1, 1, 0, 0, 0);
                //pickupDate = string.Format("{0:dd/MM/yyyy HH:mm}", dateValue.ToDate() + pickupDate.TimeOfDay).ToDateTime();

                if (miles > 0 && Instance.objPolicy.RoundJourneyMiles.ToDecimal() > 0)
                {
                    miles = Math.Ceiling(miles / Instance.objPolicy.RoundJourneyMiles.ToDecimal()) * Instance.objPolicy.RoundJourneyMiles.ToDecimal();
                }

                if (objAction.Speed.ToDecimal() > 0 || objAction.Fares.ToDecimal() == 0 || objAction.IsWaiting == "1")
                {
                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        try
                        {
                            var objFare = db.stp_CalculateMeterFares(objFareMeter.VehicleTypeId, objAction.CompanyId.ToInt(), miles, pickupDate, objAction.SubCompanyId.ToInt()).FirstOrDefault();

                            if (objFare != null)
                            {
                                rtnFares = objFare.totalFares.ToDecimal();

                                if (Instance.objPolicy.RoundMileageFares.ToBool() == false)
                                {
                                    decimal roundUp = Instance.objPolicy.RoundUpTo.ToDecimal();
                                    if (roundUp > 0)
                                    {
                                        if (objFare.Result.ToStr().IsNumeric() && objFare.CompanyFareExist.ToBool())
                                        {
                                            rtnFares = rtnFares.ToDecimal();
                                        }
                                        else
                                        {
                                            rtnFares = (decimal)Math.Ceiling(rtnFares / roundUp) * roundUp;
                                        }
                                    }
                                }
                                else
                                {
                                    string ff = string.Format("{0:f2}", rtnFares);
                                    if (ff == string.Empty)
                                        ff = "0";

                                    rtnFares = ff.ToDecimal();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                File.AppendAllText(physicalPath + "\\" + "exception_meterstringcatch.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ":" + dataValue + "," + ex.Message + Environment.NewLine);
                            }
                            catch
                            {

                            }
                        }
                    }
                }
                else
                {
                    rtnFares = objAction.Fares.ToDecimal();
                }

                if (objAction.IsWaiting == "1")
                {
                    if (objFareMeter.AutoStopWaitingOnSpeed.ToDecimal() > 0 && objAction.Speed.ToDecimal() >= objFareMeter.AutoStopWaitingOnSpeed.ToDecimal())
                    {
                        IsWaiting = "0";
                    }
                    else
                    {
                        if (waitingTime > 0)
                        {
                            if (objFareMeter.AccWaitingChargesPerMin == null || objFareMeter.AccWaitingChargesPerMin.ToInt() == 0)
                            {
                                decimal waitingMins = waitingTime / 60;
                                waitingMins = Math.Ceiling(waitingMins);
                                waitingCharges = waitingMins * objFareMeter.DrvWaitingChargesPerMin.ToDecimal();
                            }
                            else
                            {
                                decimal waitingMins = Math.Floor((waitingTime / objFareMeter.AccWaitingChargesPerMin.ToDecimal()));
                                //waitingMins = Math.Ceiling(waitingMins);
                                waitingCharges = waitingMins * objFareMeter.DrvWaitingChargesPerMin.ToDecimal();
                            }
                        }
                    }
                }
                else
                {
                    if (objFareMeter.AutoStartWaiting.ToBool() && SpeedSecs >= objFareMeter.AutoStartWaitingBelowSpeedSeconds.ToInt())
                    {
                        IsWaiting = "1";
                    }
                }

                objAction.WaitingSpeed = objFareMeter.AutoStartWaitingBelowSpeed.ToDecimal();

                if (rtnFares < objAction.Fares.ToDecimal() && rtnFares < (objAction.Fares.ToDecimal() - 2))
                    rtnFares = objAction.Fares.ToDecimal();

                objAction.Fares = rtnFares;
                objAction.IsWaiting = IsWaiting;
                objAction.WaitingTime = waitingTime;
                objAction.WaitingCharges = waitingCharges;

                string res = new JavaScriptSerializer().Serialize(objAction);

                response = "meter=" + res;

                //send message back to PDA
                Clients.Caller.fareMeter(response);

                //Byte[] byteResponse2 = Encoding.UTF8.GetBytes(response);
                //tcpClient.GetStream().Write(byteResponse2, 0, byteResponse2.Length);

                //tcpClient.Close();
                //clientStream.Close();
                //clientStream.Dispose();

                try
                {
                    File.AppendAllText(physicalPath + "\\" + objAction.JobID.ToStr() + ".txt", "logon:" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", miles:" + objAction.Miles + ", fares:" + objAction.Fares + ",waitingtime:" + objAction.WaitingTime.ToInt() + ",speed:" + objAction.Speed.ToStr() + ",lat lng:" + objAction.lg.ToStr() + "," + objAction.lt.ToStr() + Environment.NewLine);
                }
                catch
                {

                }
            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(physicalPath + "\\" + "exception_faremeter.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", data:" + dataValue + ":" + ex.Message + Environment.NewLine);
                    Clients.Caller.fareMeter(ex.Message);
                }
                catch (Exception e)
                {
                    Clients.Caller.fareMeter(e.Message);
                }
                //
            }
        }



        public static string RemoveUK(ref string address)
        {
            if (address.ToUpper().EndsWith(", UK"))
            {
                address = address.Remove(address.ToUpper().LastIndexOf(", UK"));
            }

            return address;
        }

        public static string GetPostCodeMatch(string value)
        {
            string postCode = "";

            if (value.ToStr().Contains(","))
            {
                value = value.Replace(",", "").Trim();
            }

            if (value.ToStr().Contains(" "))
            {
                value = value.Replace(" ", " ").Trim();
            }

            RemoveUK(ref value);

            //string ukAddress = @"[[:alnum:]][a-zA-Z0-9_\.\#\' \-]{2,60}$";
            string ukAddress = @"^(GIR 0AA)|((([A-PR-UWYZ][0-9][0-9]?)|(([A-PR-UWYZ][A-HK-Y][0-9][0-9]?)|(([A-PR-UWYZ][0-9][A-HJKSTUW])|([A-PR-UWYZ][A-HK-Y][0-9][ABEHMNPRVWXY])))) [0-9][A-BD-HJLNP-UW-Z]{2})$";
            //string ukAddress = @"^(GIR 0AA|[A-PR-UWYZ]([0-9]{1,2}| ([A-HK-Y][0-9]|[A-HK-Y][0-9]([0-9]| [ABEHMNPRV-Y]))|[0-9][A-HJKPS-UW]) [0-9][ABD-HJLNP-UW-Z]{2})$";

            Regex reg = new Regex(ukAddress);
            Match em = reg.Match(value);

            if (em != null)
                postCode = em.Value;

            if (em.Value == "")
            {
                ukAddress = @"[A-Z]{1,2}[0-9R][0-9A-Z]?";
                reg = new Regex(ukAddress);
                MatchCollection mat = reg.Matches(value);

                foreach (Match item in mat)
                {
                    if (item.Value.ToStr().IsAlpha() == false)
                        postCode += item.Value.ToStr() + " ";
                }

                // postCode = em.Value;
            }

            return postCode.Trim();
        }

        public static byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            if (imageIn == null) return null;
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }

        private void RemoveLogoutDriver(int driverId)
        {
            //using (TaxiDataContext db = new TaxiDataContext())
            //{                
            //    var objDriver = db.Fleet_Drivers.Where(c => c.Id == driverId).FirstOrDefault();
            //    if (objDriver != null)
            //    {
            //      //  General.BroadCastMessage("**logout driver=" + driverId + "=" + objDriver.DriverNo);
            //        //new BroadcasterData().BroadCastToAll("**logout driver=" + driverId + "=" + objDriver.DriverNo);

            //        //GMapMarkerCustom marker = (GMapMarkerCustom)gMapControl1.Overlays[0].Markers.FirstOrDefault(c => c.Tag.ToStr() == objDriver.DriverNo);
            //        //if (marker != null)
            //        //{
            //        //    gMapControl1.Overlays[0].Markers.Remove(marker);
            //        //}
            //    }
            //}
        }

        private void RemoveLogoutDriverNo(string driverNo)
        {
            try
            {
                if (!string.IsNullOrEmpty(driverNo))
                {
                    General.BroadCastMessage("**logout driver=" + driverNo + "=" + driverNo);
                    //new BroadcasterData().BroadCastToAll("**logout driver=" + driverNo + "=" + driverNo);

                    //if (listofLogoutDrivers == null)
                    //    listofLogoutDrivers = new ArrayList();

                    //listofLogoutDrivers.Add(driverNo);
                }
            }
            catch (Exception ex)
            {

            }
        }

     

        private void GetZone(string address, ref int? ZoneId, ref string zoneName)
        {
            if (string.IsNullOrEmpty(GetPostCodeMatch(address)))
                return;

            if (address.Contains(", UK"))
                address = address.Remove(address.LastIndexOf(", UK"));

            string postCode = GetPostCode(address);

            try
            {
                Gen_Coordinate objCoord = General.GetObject<Gen_Coordinate>(c => c.PostCode == postCode);

                if (objCoord != null)
                {
                    double latitude = 0, longitude = 0;

                    latitude = Convert.ToDouble(objCoord.Latitude);
                    longitude = Convert.ToDouble(objCoord.Longitude);

                    var plot = (from a in General.GetQueryable<Gen_Zone>(c => (latitude >= c.MinLatitude && latitude <= c.MaxLatitude)
                                                       && (longitude <= c.MaxLongitude && longitude >= c.MinLongitude))
                                select new
                                {
                                    a.Id,
                                    a.ZoneName,
                                    DiffMaxLong = a.MaxLongitude - longitude,
                                    DiffMinLong = longitude - a.MinLongitude,
                                    DiffMaxLat = a.MaxLatitude - latitude,
                                    DiffMinLat = latitude - a.MinLatitude
                                    //  ZoneName=  a.ZoneName
                                });

                    var objZ = (from a in plot
                                select new
                                {
                                    LngSum = (a.DiffMinLong + a.DiffMaxLong) + (a.DiffMaxLat + a.DiffMinLat),
                                    a.ZoneName,
                                    a.Id
                                }).OrderBy(c => c.LngSum).FirstOrDefault();

                    if (objZ != null)
                    {
                        ZoneId = objZ.Id;
                        zoneName = objZ.ZoneName.ToStr();
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public static string GetPostCode(string value)
        {
            string postCode = "";

            if (value.ToStr().Contains(","))
            {
                value = value.Replace(",", "").Trim();
            }

            if (value.ToStr().Contains(" "))
            {
                value = value.Replace(" ", " ").Trim();
            }

            //string ukAddress = @"[[:alnum:]][a-zA-Z0-9_\.\#\' \-]{2,60}$";
            string ukAddress = @"^(GIR 0AA)|((([A-PR-UWYZ][0-9][0-9]?)|(([A-PR-UWYZ][A-HK-Y][0-9][0-9]?)|(([A-PR-UWYZ][0-9][A-HJKSTUW])|([A-PR-UWYZ][A-HK-Y][0-9][ABEHMNPRVWXY])))) [0-9][A-BD-HJLNP-UW-Z]{2})$";
            //string ukAddress = @"^(GIR 0AA|[A-PR-UWYZ]([0-9]{1,2}| ([A-HK-Y][0-9]|[A-HK-Y][0-9]([0-9]| [ABEHMNPRV-Y]))|[0-9][A-HJKPS-UW]) [0-9][ABD-HJLNP-UW-Z]{2})$";

            Regex reg = new Regex(ukAddress);
            Match em = reg.Match(value);

            if (em != null)
                postCode = em.Value;

            if (em.Value == "")
            {
                ukAddress = @"[A-Z]{1,2}[0-9R][0-9A-Z]?";
                reg = new Regex(ukAddress);
                em = reg.Match(value);

                postCode = em.Value;
            }

            return postCode;
        }



        private bool CancelCurrentBookingFromPDA(long jobId, int driverId)
        {
            try
            {
                bool rtn = true;

                using (TaxiDataContext db = new TaxiDataContext())
                {
                    string alterProcedureScript = @"
                        ALTER PROCEDURE [dbo].[stp_UpdateJob]                                                                                        
                                                
(                                                                                        
                                               
  @jobId as bigint,                                                                                        
                                                
  @DriverId as int,                                                                                        
                                                
  @JobStatusId as int,                                                                                        
                                                
  @DriverWorkStatusId int,                                                                                        
                                                
  @SinBinTimer int                                                                                        
                                                
)                                                                                        
                                                
                                                
AS                                                                                        
                                               
SET NOCOUNT ON                                                                                        
                                                
Begin                                                                                        
                                                
      Declare @DestZoneId int                                                                                        
                                                
      Declare @DriverCurrentJobId bigint                                                                                        
                                                
      DECLARE @BookingTypeId int                                                                                        
                                                
      declare @sinbinMins int                                                                                       
                                                
                                                
 if( (@JobStatusId=10 or @JobStatusId=11 or @JobStatusId=12))                                                                                       
    select @sinbinMins=SinBinMinutes from Gen_SysPolicy_SinBinSettings where sinbintypeid=@JobStatusId                                                                                        
                                                
                                                
         if(@JobStatusId=10 or @JobStatusId=11 or @JobStatusId=12 or @JobStatusId=13 or @jobstatusid=2)                                                                                        
                                                
                                                
            set @DriverCurrentJobId=NULL                                                                                        
                                                
                                                
        else                                                                                        
                                                
                                                
       set @DriverCurrentJobId=@JobId                                                                                        
                                                
       if(@JobStatusId=5 or @JobStatusId=11)                                                                                        
                
       begin               
                                                
                                                
        declare @fleetMasterId int               
  --set @fleetMasterId=null                                                          
                     declare @currstatusid int                             
                                                
            select @fleetMasterId=fleetMasterId,@currstatusid=DriverWorkStatusId from fleet_DriverQueueList where driverId=@driverId and status=1                                                                                    
                                                
                                   
            Declare @AcceptedDateTime DateTime                                  
                                                
                                                
            if(@JobStatusId=5)                                          
                                                
              SET  @AccepteddateTime=getdate()                                                           
                                                 
              DECLARE @pickup varchar(200)                                                              
                                                
              DECLARE @Destination varchar(200)                                                             
                                                
              if(@JobStatusId=5)                               
                                                
              begin                                                           
                                                
                                                
    Update booking set BookingStatusId=@JobStatusId,FleetMasterId=@fleetMasterId,AcceptedDateTime=@AcceptedDateTime ,DriverId=@DriverId                                                                                        
                                                
                                                
    where id=@jobId                                                                                        
                                                
                            
                             if(@currstatusid=7)                              
       Update fleet_driver_location set plotdate=getdate(),ZoneId=null,PrevZoneId=null,NewZoneName='',PreviousZone='', PickupPoint='',SinBinTillOn=null ,Destination='',disableautoplotting=0,LastActiveZoneName='' where driverid=@driverId                        
                             
                  else                            
                        Update fleet_driver_location set plotdate=getdate(),PickupPoint='',Destination='',disableautoplotting=0,LastActiveZoneName='' where driverid=@driverId                                                                   
                                   
               end                                                                    
                                                
               else                                                                                  
                                                
               begin                                                                                  
                                                
      Update booking set BookingStatusId=@JobStatusId,FleetMasterId=@fleetMasterId,AcceptedDateTime=@AcceptedDateTime,IsBidding=1,AutoDespatch=1                                                                                          
                   ,@pickup=FromAddress,@Destination=ToAddress ,IsConfirmedDriver=0                                                                                  
  
     where id=@jobId and DriverId=@DriverId                                                                          
                  
                            
         Update fleet_driver_location set plotdate=getdate(),PickupPoint='',Destination='',disableautoplotting=0 where driverid=@driverId                                                                   
                                   
               end                                                                                  
                    
                                                
            --  Update fleet_driver_location set plotdate=getdate(),PickupPoint='',Destination='',disableautoplotting=0 where driverid=@driverId                                      
                                                
                                                
    if(@JobStatusId=5)                                                                                      
                     
             begin                                                                           
                                                
                                                
      declare @drvNo varchar(100)                          
                                                
                                                
                                                
     select @drvNo=DriverNo from Fleet_Driver where id=@driverId                     
                                           
     insert into Booking_Log values(@jobid,'','','Job accepted by Driver ('+ISNULL(@drvNo,'')+')',getdate(),NULL)                       
                             
             end                                                                                                         
       END                                                                                        
                                                
                                                
       ELSE                                                                         
                                                
       begin                                                                                        
                                       
                                                
      if(@JobStatusId=6)  -- Arrive                                                                                        
                                           
            BEGIN                                                                                                                  
               Update booking set BookingStatusId=@JobStatusId,ArrivalDateTime=getdate()                                                                                        
                         
                     where  id=@jobId and driverid=@driverid                                                                                      
                                
            END                                                                                                                            
                                                
    else  if(@JobStatusId=7)    -- POB                                                                                                                   
     BEGIN                                                                                                                          
                                                
     declare @journeytypeId int                                                      
                declare @PickupZoneId int                                                      
                                                                      
                                          
               Update booking set BookingStatusId=@JobStatusId,POBDateTime=getdate(),@DestZoneId=dropoffzoneid ,@journeytypeId=journeytypeid, @PickupZoneId=zoneid  where  id=@jobId and driverid=@driverid                                                                              
                              if(@journeytypeId=3)                                             
                         begin                                                          
                                                                                   
                            if(@PickupZoneId is not NULL)                                  
       begin                               
       if exists(select * from gen_zones where id=@PickupZoneId and (DisableDriverRank is null or DisableDriverRank=0))                                
        begin                                
                                                       
          Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@PickupZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@PickupZoneId,previouszone='',newzonename='' where driverid= @driverId                    
                                             
                              end                                
         else                                
      
         begin                                
          select @PickupZoneId=BackupZone1Id from Gen_Zone_Backups where zoneid=@PickupZoneId                                
                                
               if(@PickupZoneId is not null and @PickupZoneId>0)                                
          Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@PickupZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@PickupZoneId,previouszone='',newzonename='' where driverid= @driverId              
           
         end                                
                       
       end                                
                                                                                   
                         end                                                          
                         else                                                          
                         begin                                                          
                                                                          
                          if(@DestZoneId is not NULL)                                 
        begin                                
       if exists(select * from gen_zones where id=@DestZoneId and (DisableDriverRank is null or DisableDriverRank=0))                                
                  begin                                
                      --  select @DestZoneId=BackupZone1Id from Gen_Zone_Backups where zoneid=@DestZoneId                                
                                
              -- if(@DestZoneId is not null and @DestZoneId>0)                                
            Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@DestZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@DestZoneId,previouszone='',newzonename='' where driverid=@driverId                 
                                          
                end                                 
          else                                
                                
   begin                                
                    select @DestZoneId=BackupZone1Id from Gen_Zone_Backups where zoneid=@DestZoneId                                
                                
               if(@DestZoneId is not null and @DestZoneId>0)                             
            Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@DestZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@DestZoneId,previouszone='',newzonename='' where driverid=@driverId                 
                  
          end                                
                                 
       end                                          
                         end                                
                                  
                                           
                                                
 --                                       if(@DestZoneId is not NULL)                                             
                                                
  --  Update fleet_driver_location set plotdate=(case when zoneid is not null and zoneid!=@DestZoneId then getdate() else plotdate END),disableautoplotting=1,ZoneId=@DestZoneId,PrevZoneId=@DestZoneId,previouszone='',newzonename='' where driverid=@driverId
                                         
    END                                                                                    
                                   
                                                
   else  if(@JobStatusId=8)  -- STC                                                                                        
                                    
            BEGIN                                                                                        
                               
               Update booking set BookingStatusId=@JobStatusId,STCDateTime=getdate()             
                         
                     where  id=@jobId and driverid=@driverid                                                                                 
            END                                                                                        
                                  
            else  if(@JobStatusId=2)  --  DISPATCHED OR COMPLETED                                                                                        
                     
            BEGIN                                                                                        
                              
               Update booking set BookingStatusId=@JobStatusId,ClearedDateTime=getdate()                                    
                         
                     where  id=@jobId and driverid=@driverid                                                                                        
                                   
            END                                                                                        
                                
             else  if(@JobStatusId=3 or @JobStatusId=13 ) -- Cancel                                                                                        
                                                
            BEGIN                                                                                        
                                                
               Update booking set BookingStatusId=@JobStatusId                                   
                 
                                                
                     where  id=@jobId                                                    
                                                
                                                
                                                
            END                                                                                        
                                                
            else  if(@JobStatusId=10) -- No Show                                                                                        
                                                
                                                
                                               
     BEGIN                                                                                        
                                                
                      
               Update booking set BookingStatusId=@JobStatusId,ArrivalDateTime=getdate(),AutoDespatch=0,IsBidding=0                                                         
                                                
                                                
                     where  id=@jobId and driverid=@driverid                                                                                        
                                                
                                                
            END                                                                                        
                                                                                 
    else  if(@JobStatusId=1) -- Waiting                                 
                                                
                                                
            BEGIN                                                                                        
                 
               Update booking set BookingStatusId=@JobStatusId,ArrivalDateTime=NULL ,IsConfirmedDriver=0,AutoDespatch=0,IsBidding=0   where  id=@jobId                                                                                       
                                                
                                                
                                 
                     --     ,DriverId=NULL                                                                                        

                   --  where  id=@jobId                                                                        
 
                         INSERT INTO dbo.Fleet_Driver_RejectJobs (DriverId,BookingId,RejectedDateTime,BookingStatusId)                                                                                                    
               values (@DriverId,@JobId,getdate(),10)                                                  
 
            END                                                                                        
  
         else  if(@JobStatusId=12) -- Not Accepted                                                                                        
                                                 
                                     
begin                                                               
                                          
    Update booking set BookingStatusId=@JobStatusId                                                             
                                     
                     where  id=@jobId                                                                                        
                                                 
    END                                                                                        
                                         
       END";

                    db.ExecuteCommand(alterProcedureScript);
                    db.stp_UpdateJob(jobId, driverId, Enums.BOOKINGSTATUS.CANCELLED, Enums.Driver_WORKINGSTATUS.AVAILABLE, Instance.objPolicy.SinBinTimer.ToInt());
                }

                //Instance.listofJobs.Add(new clsPDA
                //{
                //    DriverId = driverId,
                //    JobId = jobId,
                //    MessageDateTime = DateTime.Now.AddSeconds(-30),
                //    JobMessage = "Cancelled Job>>" + jobId,
                //    MessageTypeId = 2
                //});

                string recordId = Guid.NewGuid().ToString();
                try
                {
                    Instance.listofJobs.Add(new clsPDA
                    {
                        DriverId = driverId,
                        JobId = jobId,
                        MessageDateTime = DateTime.Now.AddSeconds(-30),
                        JobMessage = "Cancelled Job>>" + jobId + ">>Job is cancelled by Customer",
                        MessageTypeId = 2,
                         Id=recordId
                    });
                }
                catch
                {

                }

                try
                {

                    SocketIO.SendToSocket(driverId.ToStr(), "Cancelled Job>>" + jobId + ">>Job is cancelled by Customer", "forceRecoverJob", recordId);

                    File.AppendAllText(physicalPath + "\\CancelCurrentBookingFromPDA.txt", DateTime.Now.ToStr() + " ,jobid:" + jobId + ", driverid:" + driverId + Environment.NewLine);
                }
                catch
                {
                    Thread.Sleep(500);
                    try
                    {
                        Instance.listofJobs.Add(new clsPDA
                        {
                            DriverId = driverId,
                            JobId = jobId,
                            MessageDateTime = DateTime.Now.AddSeconds(-30),
                            JobMessage = "Cancelled Job>>" + jobId + ">>Job is cancelled by Customer",
                            MessageTypeId = 2,
                            Id = recordId
                        });
                    }
                    catch
                    {

                    }
                    SocketIO.SendToSocket(driverId.ToStr(), "Cancelled Job >> " + jobId + " >> Job is cancelled by Customer", "forceRecoverJob",recordId);

                }

                if (Instance.objPolicy.DespatchOfflineJobs.ToBool())
                {
                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        db.stp_SaveOfflineMessage(jobId, driverId, "", "Customer", "Cancelled Job>>" + jobId + "=2");
                    }
                }

                return rtn;
            }
            catch (Exception ex)
            {
                return false;
                //ENUtils.ShowMessage(ex.Message);
            }
        }

        private bool ReCallFOJBookingFromPDA(long jobId, int driverId)
        {
            bool rtn = true;

            string recordId = Guid.NewGuid().ToString();
            try
            {
              
                Instance.listofJobs.Add(new clsPDA
                {
                    DriverId = driverId,
                    JobId = jobId,
                    MessageDateTime = DateTime.Now.AddSeconds(-30),
                    JobMessage = "Cancelled Foj Job>>" + jobId,
                    MessageTypeId = 2,
                     Id=recordId
                });



                //


            }
            catch (Exception ex)
            {
                try
                {
                    Instance.listofJobs.Add(new clsPDA
                    {
                        DriverId = driverId,
                        JobId = jobId,
                        MessageDateTime = DateTime.Now.AddSeconds(-30),
                        JobMessage = "Cancelled Foj Job>>" + jobId,
                        MessageTypeId = 2,
                        Id = recordId
                    });
                }
                catch
                {

                }

                //ENUtils.ShowMessage(ex.Message);
            }

            //try
            //{
            //    Instance.listofJobs.Add(new clsPDA
            //    {
            //        DriverId = driverId,
            //        JobId = jobId,
            //        MessageDateTime = DateTime.Now.AddSeconds(-30),
            //        JobMessage = "Cancelled Job>>" + jobId + ">>Job is cancelled by Customer",
            //        MessageTypeId = 2
            //    });
            //}
            //catch
            //{

            //}

            try
            {

                SocketIO.SendToSocket(driverId.ToStr(), "Cancelled Foj Job>>" + jobId, "forceRecoverJob",recordId);

                File.AppendAllText(physicalPath + "\\ReCallFOJBookingFromPDA.txt", DateTime.Now.ToStr() + " ,jobid:" + jobId + ", driverid:" + driverId + Environment.NewLine);
            }
            catch
            {
               

            }


            return rtn;
        }

        //private bool ReCallFOJBookingFromPDA(long jobId, int driverId)
        //{
        //    bool rtn = true;

        //    try
        //    {
        //        //(new TaxiDataContext()).stp_UpdateJobStatus(jobId, Enums.BOOKINGSTATUS.WAITING);
        //        Instance.listofJobs.Add(new clsPDA
        //        {
        //            DriverId = driverId,
        //            JobId = jobId,
        //            MessageDateTime = DateTime.Now.AddSeconds(-30),
        //            JobMessage = "Cancelled Foj Job>>" + jobId,
        //            MessageTypeId = 2
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        //ENUtils.ShowMessage(ex.Message);
        //    }

        //    return rtn;
        //}

        private string MakePayment(Gen_SysPolicy_PaymentDetail obj, ClsPaymentInformation objCard)
        {
            string rtn = string.Empty;

            try
            {


                if (obj.PaymentGatewayId.ToInt() == Enums.PAYMENT_GATEWAY.ATLANTE_CONNECTPAY)
                {
                    string URL = "https://connectpayadmin.co.uk/services/smartpos2/mpos_xpospay.ashx?";
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
                    var request = (HttpWebRequest)WebRequest.Create(URL);

                    //  int amt = numTotalCharges.Value * 100;
                    decimal amt = Math.Round((objCard.Total.ToDecimal() * 100), 0);
                    var postData = "uid=" + obj.MerchantID.ToStr().Trim() + "&pwd=" + obj.MerchantPassword.ToStr().Trim() + "&accountid=" + obj.ApplicationId.ToStr().Trim() + "&profileid=" + obj.PaypalID.ToStr().Trim()
                    + "&sessionid=" + (objCard.BookingNo.ToStr().Trim() + "/" + objCard.DriverNo)   //"//seeionid = jobid + "payment";
                    + "&amount=" + amt
                    + "&cardno=" + objCard.CardNumber.ToStr()
                    + "&expmonth=" + objCard.ExpiryMonth.ToStr()
                    + "&expyear=" + objCard.ExpiryYear.ToStr()
                    //  + "&startmonth="+dtpStartDate.Value.Value.Month.ToStr()+"&startyear="+dtpStartDate.Value.Value.Year.ToStr()

                    + "&issueno="
                    + "&cvv=" + objCard.CV2.ToStr().Trim()
                    + "&avshouseno=&avspostcode=";
                    var data = Encoding.UTF8.GetBytes(postData);

                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = data.Length;


                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                    var response = (HttpWebResponse)request.GetResponse();

                    var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                    //  lblStatus.Text = responseString.ToString();

                    var result = HttpUtility.ParseQueryString(responseString);


                    if (result != null)
                    {


                        if (result["status"] == "1")
                        {

                            //if (obj.ResponseLog.ToBool())
                            //{
                            //    rtn = "{\"success\":\"true\",\"authcode\":\"" + result["authcode"].ToStr().Trim() + "\",\"message\":\"Your Payment has been processed successfully!" + "\",\"responselog\":\"" + HttpUtility.UrlDecode(result["response"].ToStr().Trim()) + "\"}";


                            //}
                            //else
                            //{
                            rtn = "success:" + result["authcode"].ToStr().Trim();
                            //     rtn = "{\"success\":\"true\",\"authcode\":\"" + result["authcode"].ToStr().Trim() + "\",\"message\":\"Your Payment has been processed successfully!\"}";


                            //  }

                        }

                        else
                        {
                            //if (obj.ResponseLog.ToBool())
                            //{
                            //    rtn = "{\"success\":\"false\",\"authcode\":\"\",\"message\":\"" + HttpUtility.UrlDecode(result["errorcode"].ToStr().Trim()) + ", " + HttpUtility.UrlDecode(result["response"].ToStr().Trim()) + "\",\"responselog\":\"" + HttpUtility.UrlDecode(result["response"].ToStr().Trim()) + "\"}";

                            //}
                            //else
                            //{
                            rtn = "failed:" + HttpUtility.UrlDecode(result["errorcode"].ToStr().Trim());
                            //   rtn = "{\"success\":\"false\",\"authcode\":\"\",\"message\":\"" + HttpUtility.UrlDecode(result["errorcode"].ToStr().Trim()) + ", " + HttpUtility.UrlDecode(result["response"].ToStr().Trim()) + "\"}";
                            //
                        }

                    }





                    //

                    //lblStatus.Text = responseString.ToString();

                    //if (!string.IsNullOrEmpty(responseString))
                    //{
                    //    string[] dataArr = responseString.Split(new char[] { '&' });

                    //    if (dataArr.Count() > 4)
                    //    {
                    //        string val = dataArr[2].ToString();

                    //        string[] arr = val.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

                    //        if (arr.Count() == 2)
                    //        {
                    //            rtn = "success:" + val.Replace("=", ":").ToStr().Trim();
                    //        }

                    //        else if (responseString.Contains("errorcode") && dataArr[4].ToString().StartsWith("errorcode"))
                    //        {
                    //            arr = dataArr[4].Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

                    //            if (arr.Count() == 2)
                    //            {
                    //                rtn = "failed:" + responseString.ToStr();
                    //            }
                    //        }
                    //    }
                    //    else
                    //    {
                    //        rtn = "failed:" + responseString.ToStr();
                    //    }
                    //}
                }
                else if (obj.PaymentGatewayId.ToInt() == 6)
                {

                    
                        rtn = General.ProcessJudoPayment(obj, objCard);




                        try
                        {


                            File.AppendAllText(physicalPath + "\\processjudoreceipt.txt", DateTime.Now.ToStr() + ": request" + objCard.BookingId.ToStr() + ",token:" + objCard.TokenDetails.ToStr() + ",response:" + rtn.ToStr() + Environment.NewLine);
                        }
                        catch
                        {


                        }

                  
              




                    if (rtn.Contains("success"))
                    {
                        try
                        {
                            if (Directory.Exists(physicalPath + "\\Transactions") == false)
                            {
                                Directory.CreateDirectory(physicalPath + "\\Transactions");

                            }






                            File.AppendAllText(physicalPath + "\\Transactions\\" + objCard.BookingId + ".txt", rtn.Replace("success:", "").Trim() + ":" + Math.Round(objCard.Total.ToDecimal(), 2));


                        }
                        catch
                        {

                        }

                    }
                }


                else if (obj.PaymentGatewayId.ToInt() == 7) // stripe
                {

                    string json = string.Empty;
                    try
                    {
                        Stripe3DS st = new Stripe3DS();
                        int amount = Math.Round((objCard.Total.ToDecimal() * 100), 0).ToInt();


                        st.Amount = amount.ToInt();
                        // double increasedAmount = amount + Convert.ToDouble(((Convert.ToDouble(amount) * 20) / 100));
                        st.Description = "Booking Ref : " + objCard.BookingNo.ToStr();
                        st.Currency = "GBP";
                        st.APIkey = obj.ApplicationId.ToStr();
                        st.APISecret = obj.PaypalID.ToStr();
                        st.BookingId = objCard.BookingId.ToStr();
                        st.MobileNo = "";
                        st.Email = "";
                        st.paymentIntentId = objCard.TokenDetails.ToStr();
                        st.status = objCard.PaymentStatus.ToStr();

                        string response = string.Empty;
                        json = Newtonsoft.Json.JsonConvert.SerializeObject(st);


                        using (var client = new System.Net.Http.HttpClient())
                        {
                            var BASE_URL = "https://api-eurosofttech.co.uk/StripePayment-api/";
                            client.BaseAddress = new Uri(BASE_URL);
                            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));
                            // var stringContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(updatebookingstatustoSupplier), Encoding.UTF8, "application/json");
                            var postTask = client.PostAsync(BASE_URL + "PaymentProcess?data=" + json, null).Result;
                            response = postTask.Content.ReadAsStringAsync().Result;
                            //  var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
                            st = new JavaScriptSerializer().Deserialize<Stripe3DS>(response);

                        }

                        //    using (WebClient webClient = new WebClient())
                        //    {

                        //        webClient.Proxy = null;



                        //        webClient.BaseAddress = "https://api-eurosofttech.co.uk/StripePayment-api";
                        //        var url = "/PaymentProcess?data=";
                        //        webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                        //        var postTask = webClient.post.PostAsync("https://api-eurosofttech.co.uk/StripePayment-api" + "PaymentProcess?data=" +json, null).Result;
                        //        var readTask = postTask.Content.ReadAsStringAsync().Result;

                        //      st = new JavaScriptSerializer().Deserialize<Stripe3DS>(readTask);


                        //    // response = webClient.UploadString(url, json);
                        //    // st =new JavaScriptSerializer().Deserialize<Stripe3DS>(response);
                        //}


                        if (st.IsSuccess)
                        {
                            rtn = "success:" + st.paymentIntentId.ToStr();
                        }
                        else
                            rtn = "failed:" + st.Message.ToStr();

                        try
                        {


                            File.AppendAllText(physicalPath + "\\stripepayment.txt", DateTime.Now.ToStr() + ": json" + json + "bookingno:" + objCard.BookingNo.ToStr() + ",response:" + response.ToStr() + Environment.NewLine);
                        }
                        catch
                        {


                        }

                    }
                    catch (Exception ex)
                    {
                        try
                        {


                            File.AppendAllText(physicalPath + "\\stripepayment_exception.txt", DateTime.Now.ToStr() + ": token=" + json + "  ,bookingno:" + objCard.BookingNo.ToStr() + ",exception:" + ex.Message + Environment.NewLine);
                        }
                        catch
                        {


                        }
                    }


                }
            }
            catch (Exception ex)
            {
                rtn = "failed:" + ex.ToString();
            }

            return rtn;
        }

        //private void button1_Click(object sender, EventArgs e)
        //{
        //    AddLog();
        //}

        void objBroadcaster_AutoRefreshMessage(string message)
        {
            try
            {
                if (message.StartsWith("request pda="))
                {
                    string[] values = message.Split('=');

                    if (values[4].ToInt() == eMessageTypes.JOB)
                    {
                        //Instance.listofJobs.RemoveAll(c => c.JobId != 0 && c.JobId != values[1].ToLong() && c.DriverId == values[2].ToInt());

                        if (Instance.listofJobs.Count(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong() && c.MessageTypeId == eMessageTypes.RECALLJOB) > 0)
                            Instance.listofJobs.RemoveAll(c => c.JobId != 0 && c.JobId != values[1].ToLong() && c.MessageTypeId == eMessageTypes.RECALLJOB);

                        if (Instance.listofJobs.Count(c => c.JobId == values[1].ToLong() && c.DriverId == values[2].ToInt()) > 0)
                            Instance.listofJobs.RemoveAll(c => c.JobId == values[1].ToLong() && c.DriverId == values[2].ToInt());

                        Instance.listofJobs.Add(new clsPDA
                        {
                            JobId = values[1].ToLong(),
                            DriverId = values[2].ToInt(),
                            MessageDateTime = DateTime.Now,
                            JobMessage = values[3].ToStr().Trim(),
                            MessageTypeId = values[4].ToInt(),
                            DriverNo = values[5].ToStr()
                        });
                    }
                    else if (values[4].ToInt() == eMessageTypes.CLEAREDJOB || values[4].ToInt() == eMessageTypes.RECALLJOB)
                    {
                        Instance.listofJobs.Add(new clsPDA
                        {
                            DriverId = values[1].ToInt(),
                            JobId = values[2].ToLong(),
                            MessageDateTime = DateTime.Now.AddSeconds(-30),
                            JobMessage = values[3].ToStr().Trim(),
                            MessageTypeId = values[4].ToInt()
                        });
                    }
                    else if (values[4].ToInt() == eMessageTypes.MESSAGING)
                    {
                        if (values[1].ToStr().Contains(","))
                        {
                            DateTime dt = DateTime.Now.AddSeconds(-45);
                            foreach (string dId in values[1].Split(','))
                            {
                                Instance.listofJobs.Add(new clsPDA
                                {
                                    DriverId = dId.ToInt(),
                                    JobId = values[2].ToLong(),
                                    MessageDateTime = dt,
                                    JobMessage = values[3].ToStr().Trim(),
                                    MessageTypeId = values[4].ToInt()
                                });
                            }
                        }
                        else
                        {
                            Instance.listofJobs.Add(new clsPDA
                            {
                                DriverId = values[1].ToInt(),
                                JobId = values[2].ToLong(),
                                MessageDateTime = DateTime.Now.AddSeconds(-45),
                                JobMessage = values[3].ToStr().Trim(),
                                MessageTypeId = values[4].ToInt()
                            });
                        }
                    }
                    else if (values[4].ToInt() == eMessageTypes.AUTHORIZATION)
                    {
                        Instance.listofJobs.Add(new clsPDA
                        {
                            DriverId = values[1].ToInt(),
                            JobId = values[2].ToLong(),
                            MessageDateTime = DateTime.Now.AddSeconds(-30),
                            JobMessage = values[3].ToStr().Replace(">>", "=").Trim(),
                            MessageTypeId = values[4].ToInt()
                        });
                    }
                    else if (values[4].ToInt() == eMessageTypes.BIDALERT)
                    {
                        Instance.listofJobs.Add(new clsPDA
                        {
                            DriverId = values[1].ToInt(),
                            JobId = values[2].ToLong(),
                            MessageDateTime = DateTime.Now.AddSeconds(-15),
                            JobMessage = values[3].ToStr(),
                            MessageTypeId = values[4].ToInt()
                        });
                    }
                    else if (values[4].ToInt() == eMessageTypes.BIDPRICEALERT)
                    {
                        string[] driverIds = values[1].ToStr().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                        Instance.listofJobs.AddRange((from a in driverIds
                                                      select new clsPDA
                                                      {
                                                          DriverId = a.ToInt(),
                                                          JobId = values[2].ToLong(),
                                                          MessageDateTime = DateTime.Now.AddSeconds(-30),
                                                          JobMessage = values[3].ToStr(),
                                                          MessageTypeId = values[4].ToInt()
                                                      }));
                    }
                    else if (values[4].ToInt() == eMessageTypes.UPDATEPLOT || values[4].ToInt() == eMessageTypes.UPDATEJOB)
                    {
                        Instance.listofJobs.Add(new clsPDA
                        {
                            DriverId = values[1].ToInt(),
                            JobId = values[2].ToLong(),
                            MessageDateTime = DateTime.Now.AddSeconds(-15),
                            JobMessage = values[3].ToStr(),
                            MessageTypeId = values[4].ToInt()
                        });
                    }
                    else if (values[4].ToInt() == eMessageTypes.LOGOUTAUTHORIZATION)
                    {
                        Instance.listofJobs.Add(new clsPDA
                        {
                            DriverId = values[1].ToInt(),
                            JobId = 0,
                            MessageDateTime = DateTime.Now.AddSeconds(-45),
                            JobMessage = values[3].ToStr(),
                            MessageTypeId = values[4].ToInt()
                        });
                    }
                    else if (values[4].ToInt() == eMessageTypes.FORCE_ACTION_BUTTON)
                    {
                        Instance.listofJobs.Add(new clsPDA
                        {
                            DriverId = values[1].ToInt(),
                            JobId = values[2].ToLong(),
                            MessageDateTime = DateTime.Now.AddSeconds(-45),
                            JobMessage = values[3].ToStr(),
                            MessageTypeId = values[4].ToInt()
                        });
                    }
                    else if (values[4].ToInt() == eMessageTypes.UPDATE_SETTINGS)
                    {
                        Instance.listofJobs.Add(new clsPDA
                        {
                            DriverId = values[2].ToInt(),
                            JobId = 0,
                            MessageDateTime = DateTime.Now.AddSeconds(-50),
                            JobMessage = values[3].ToStr().Trim(),
                            MessageTypeId = values[4].ToInt()
                        });
                    }
                }
            }
            catch
            {

            }
        }

   
        public static string GetPostCodeMatchWithBase(string value, bool isBase)
        {
            string postCode = "";
            try
            {

                if (value.ToStr().Contains(","))
                {
                    value = value.Replace(",", "").Trim();
                }

                if (value.ToStr().Contains(" "))
                {
                    value = value.Replace(" ", " ").Trim();
                }


                RemoveUK(ref value);


                string ukAddress = @"^(GIR 0AA)|((([A-PR-UWYZ][0-9][0-9]?)|(([A-PR-UWYZ][A-HK-Y][0-9][0-9]?)|(([A-PR-UWYZ][0-9][A-HJKSTUW])|([A-PR-UWYZ][A-HK-Y][0-9][ABEHMNPRVWXY])))) [0-9][A-BD-HJLNP-UW-Z]{2})$";


                if (isBase)
                {

                    ukAddress = @"([A-Z][A-Z][0-9] [0-9]?)|([A-Z][A-Z][0-9][0-9] [0-9]?)|([A-Z][0-9][0-9] [0-9]?)|([A-Z][0-9] [0-9]?)|([A-Z][A-Z][0-9][A-Z] [0-9]?)|([A-Z][0-9][A-Z] [0-9]?)";

                }

                Regex reg = new Regex(ukAddress);
                Match em = reg.Match(value);

                if (em != null)
                    postCode = em.Value;

                if (em.Value == "")
                {

                    reg = new Regex(ukAddress);
                    MatchCollection mat = reg.Matches(value);


                    foreach (Match item in mat)
                    {
                        if (item.Value.ToStr().IsAlpha() == false)
                            postCode += item.Value.ToStr() + " ";

                    }

                    // postCode = em.Value;

                }

            }
            catch
            {

            }

            return postCode.Trim();

        }

        public static string CheckIfSpecialPostCode(string postcode)
        {
            try
            {

                if (((postcode.StartsWith("EC") || postcode.StartsWith("WC") || postcode.StartsWith("SE1") ||
                          postcode.StartsWith("SW1")) && postcode.Length == 4) || postcode.StartsWith("W1") && postcode.Length == 3)
                {

                    if (char.IsLetter(postcode[postcode.Length - 1]))
                    {
                        postcode = postcode.Remove(postcode.Length - 1);
                    }
                }
            }
            catch
            {


            }

            return postcode;

        }

        private string GetHalfPostCodeMatch(string value)
        {
            string postCode = "";





            string ukAddress = @"^(GIR 0AA)|((([A-PR-UWYZ][0-9][0-9]?)|(([A-PR-UWYZ][A-HK-Y][0-9][0-9]?)|(([A-PR-UWYZ][0-9][A-HJKSTUW])|([A-PR-UWYZ][A-HK-Y][0-9][ABEHMNPRVWXY])))) [0-9][A-BD-HJLNP-UW-Z]{2})$";


            Regex reg = new Regex(ukAddress);
            Match em = reg.Match(value);

            if (em != null)
                postCode = em.Value;

            if (em.Value == "")
            {
                ukAddress = @"[A-Z]{1,2}[0-9R][0-9A-Z]?";
                reg = new Regex(ukAddress);
                MatchCollection mat = reg.Matches(value);


                foreach (Match item in mat)
                {
                    if (item.Value.ToString().IsAlpha() == false)
                        postCode += item.Value.ToString() + " ";

                }

            }


            if (postCode.WordCount() == 2 && postCode.Contains(" "))
                postCode = postCode.Split(' ')[0];



            if (!string.IsNullOrEmpty(postCode.Trim()))
            {
                postCode = CheckIfSpecialPostCode(postCode.Trim());

            }

            return postCode.Trim();

        }





        public void requestOfficeBase(string mesg)
        {
            try
            {
                byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                string postcode = GetPostCodeMatch(Instance.objPolicy.BaseAddress.ToStr());

                var obj = General.GetObject<Gen_Coordinate>(c => c.PostCode == postcode);

                if (obj != null)
                {
                    postcode = postcode + ">>>" + obj.Latitude + ">>>" + obj.Longitude;
                }

                Clients.Caller.officeBase(postcode);
            }
            catch (Exception ex)
            {
                Clients.Caller.officeBase(ex.Message);
            }
        }



        public void requestAccountCharges(string mesg)
        {
            try
            {
                byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });


                long jobId = values[1].ToLong();
                int driverId = values[2].ToInt();

                string response = "nocharges";


                try
                {

                    if (Global.enableAccountCharges == "1")
                    {

                        using (TaxiDataContext db = new TaxiDataContext())
                        {

                            int? companyId = db.Bookings.Where(c => c.Id == jobId).Select(c => c.CompanyId).FirstOrDefault();


                            if (companyId != null)
                            {

                                response = "charges=" + string.Join("=", db.Gen_Company_ExtraCharges.Where(c => c.CompanyId == companyId)
                                                            .OrderBy(c => c.Charges)
                                                               .Select(c => c.Gen_Charge.ChargesName).ToArray<string>());

                            }

                        }
                    }
                    else
                        response = "";
                }
                catch
                {


                }



                if (response.ToStr().Length == 0 || response.ToStr() == "charges=")
                    response = "nocharges";
                else
                {
                    response = response.Replace("Parking Charges", "parking");
                    response = response.Replace("Waiting Time", "waiting");
                    response = response.Replace("Extra Charges", "extracharges");
                    response = response.Replace("Fares", "fares");
                    response = response.Replace("Passenger", "passenger");
                    response = response.Replace("Signature", "signature");

                    response = response.ToLower();
                }


                //send message back to PDA
                Clients.Caller.AccountCharges(response);



                try
                {



                    File.AppendAllText(physicalPath + "\\requestaccountcharges.txt", DateTime.Now + " , response" + response + ", message" + mesg + " , EnabledAccountCharges=" + Global.enableAccountCharges + Environment.NewLine);


                }
                catch
                {


                }

            }
            catch (Exception ex)
            {
                try
                {
                    Clients.Caller.AccountCharges("nocharges");


                    File.AppendAllText(physicalPath + "\\requestaccountcharges_exception.txt", DateTime.Now + " ," + ex.Message + ", message" + mesg);


                }
                catch
                {


                }


            }
        }


        public void requestMakeSignature(string mesg)
        {
            string jStatus = string.Empty;

            try
            {

                //  Newtonsoft.Json.Linq.JArray arr = (Newtonsoft.Json.Linq.JArray)mesg;

                string base64Decoded = "";
                byte[] inputBuffer = null;


                string[] values = null;

                string dataValue = string.Empty;
                if (mesg.ToStr().StartsWith("jaction") == false)
                {
                    inputBuffer = System.Convert.FromBase64String(mesg);
                    base64Decoded = System.Text.ASCIIEncoding.ASCII.GetString(inputBuffer);


                    dataValue = base64Decoded;
                    dataValue = dataValue.Trim();
                    values = dataValue.Split(new char[] { '=' });
                }
                else
                {
                    dataValue = mesg;
                    values = dataValue.Split(new char[] { '=' });
                }

                string rrr = "false";

                JobAction objAction = null;

                if (values != null)
                {
                    objAction = new JavaScriptSerializer().Deserialize<JobAction>(values[1].ToStr());

                }
                else
                {

                    objAction = new JavaScriptSerializer().Deserialize<JobAction>(mesg);
                }

                jStatus = objAction.JStatus.ToStr().ToLower();


                if (objAction.JStatus.ToStr().ToLower() == "accountcharges")
                {
                    try
                    {



                        byte[] arr2 = null;

                        if (values != null && values.Count() > 2 && base64Decoded.ToStr().Trim().Length > 0)
                        {

                            try
                            {

                                // NEED TO UNCOMMENT
                                int idx = dataValue.IndexOf("}=") + 2;
                                int len = values[2].ToInt();
                                idx += (values[2].Length) + 1;
                                using (MemoryStream st = new MemoryStream(inputBuffer, idx, len))
                                {
                                    // lastCourierImg = Image.FromStream(st);
                                    arr2 = imageToByteArray(Image.FromStream(st));


                                    st.Close();
                                    //   st.Dispose();
                                }

                            }
                            catch
                            {


                            }
                        }



                        if (objAction.Fares.ToDecimal() == 0)
                        {
                            objAction.Fares = null;
                        }


                        if (arr2 == null)
                        {
                            arr2 = new byte[1];

                        }

                        using (TaxiDataContext db = new TaxiDataContext())
                        {

                            db.stp_UpdateAccountJobCharges(objAction.JobId.ToLong(), objAction.DrvId.ToInt(), objAction.ParkingCharges.ToDecimal(),
                                   objAction.WaitingCharges.ToDecimal(), objAction.ExtraDropCharges.ToDecimal(), objAction.Miles, objAction.Fares, objAction.Passengers.ToStr().Trim(), arr2);

                        }

                        rrr = "true";
                        Clients.Caller.makeSignature(rrr);

                    }
                    catch (Exception ex)
                    {

                        try
                        {
                            File.AppendAllText(physicalPath + "\\exceptionelse_accountsignature.txt", DateTime.Now + " ," + ex.Message + ", message" + mesg);

                        }
                        catch
                        {

                        }


                    }

                }


            }
            catch (Exception ex)
            {


                Clients.Caller.makeSignature("Exception Occurred");

                try
                {
                    File.AppendAllText(physicalPath + "\\exception_accountsignature.txt", DateTime.Now + " ," + ex.Message + ", message" + mesg);

                }
                catch
                {

                }

            }
        }
        protected string ToTinyURLS(string txt)
        {
            Regex regx = new Regex("http://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,]*)?", RegexOptions.IgnoreCase);

            MatchCollection mactches = regx.Matches(txt);

            foreach (Match match in mactches)
            {
                string tURL = MakeTinyUrl(match.Value);
                txt = txt.Replace(match.Value, tURL);
            }

            return txt;
        }

        public static string MakeTinyUrl(string Url)
        {
            string text;
            try
            {
                if (Url.Length <= 12)
                {
                    return Url;
                }
                if (!Url.ToLower().StartsWith("http") && !Url.ToLower().StartsWith("ftp"))
                {
                    Url = "http://" + Url;
                }

                WebRequest request = HttpWebRequest.Create("http://tinyurl.com/api-create.php?url=" + Url);
                request.Proxy = null;
                WebResponse response = request.GetResponse();
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {



                    text = reader.ReadToEnd();
                }
                return text;
                //
            }
            catch (Exception)
            {
                return Url;
            }
        }






        #region AutoDispatch


     



       


     

    


        #endregion


    

        #region SMS    
     


        public string RestartSMSService(string message)
        {
            //  Global.r

            return Global.RestartSMS(message);

        }
        #endregion


















        #region bidregion





        public void requestBidding(string msg)
        {
           // byte[] inputBuffer = Encoding.UTF8.GetBytes(msg);

            string dataValue = msg;
            dataValue = dataValue.Trim();

            string[] values = dataValue.Split(new char[] { '=' });

            int driverId = values[1].ToInt();
            string[] arr = null;

            string response = string.Empty;

            try
            {

                //try
                //{
                //    File.AppendAllText(physicalPath + "\\requestBidding.txt", DateTime.Now.ToStr() + "request : " + dataValue +  Environment.NewLine);

                //}
                //catch
                //{

                //    //
                //}

                if (Instance.objPolicy.EnableBiddingForChauffers.ToBool())
                {

                    arr = new TaxiDataContext().stp_getbiddingjobsfulldetails().Where(c => c.driverid == driverId)
                        .OrderBy(c => c.PickupDateTime)




                      .Select(args => (args.zoneid + "<<" + (args.zonename.Length > 0 ? args.zonename : " - ") + "<<" + "0" + "<<" + args.JobId + "<<" + args.PickupDateTime + "<<" + args.FromAddress + "<<" + args.ToAddress + "<<" + args.FareRate + "<<" + args.VehicleType))

                        .ToArray<string>();



                    //
                    string firstRow = "";

                    if (arr != null && arr.Count() > 0)
                    {
                        firstRow = arr[0] + "<<" + "showzonename=0" + "<<" + "showpickupdatetime=1" + "<<" + "showpickup=1" + "<<" + "showdropoff=1" + "<<" + "showfares=1" + "<<" + "showvehicle=1";

                        arr[0] = firstRow;

                    }



                }
                else
                {
                    ClsBidAction objBid = null;
                    if (dataValue.Contains("jsonstring|"))
                    {
                        objBid = new JavaScriptSerializer().Deserialize<ClsBidAction>(values[2].Replace("jsonstring|", ""));

                    }


                    if (objBid != null && objBid.Status == "8")
                    {
                        //
                       //
                       //
                        arr = (from a in new TaxiDataContext().ExecuteQuery<stp_getbiddingSTCjobsResult>("exec stp_getbiddingSTCjobs {0}", driverId)

                               select new

                               {
                                   Distance = a.biddingradius > 0 && a.JobLatitude != null && a.JobLatitude != 0 ? new LatLng(Convert.ToDouble(a.DestLat), Convert.ToDouble(a.DestLon))
                                            .DistanceMiles(new LatLng(Convert.ToDouble(a.JobLatitude), Convert.ToDouble(a.JobLongitude))) : 0,
                                   a.zonename,
                                   a.biddingradius,
                                   a.zoneid

                               })
                               .Where(c => c.Distance <= c.biddingradius)
                                .GroupBy(args => new
                                {
                                    args.zonename,
                                    args.zoneid

                                })


                          
                             .Select(args => new
                             {
                                 args.Key.zoneid,
                                 args.Key.zonename,
                                 Jobs = args.Count()


                             })


                               .Select(args => (args.zoneid + "<<" + args.zonename + "<<" + args.Jobs)).ToArray<string>();



                        //var listtest =  (from a in new TaxiDataContext().ExecuteQuery<stp_getbiddingSTCjobsResult>("exec stp_getbiddingSTCjobs {0}", driverId)

                        //                  select new

                        //                  {
                        //                      Distance = a.biddingradius > 0 && a.JobLatitude != null && a.JobLatitude != 0 ? new LatLng(Convert.ToDouble(a.DestLat), Convert.ToDouble(a.DestLon))
                        //                               .DistanceMiles(new LatLng(Convert.ToDouble(a.JobLatitude), Convert.ToDouble(a.JobLongitude))) : 0,
                        //                      a.zonename,
                        //                      a.biddingradius,
                        //                      a.zoneid,
                        //                       a.DestLat,
                        //                       a.DestLon

                        //                  });
                        //foreach (var item in listtest)
                        //{
                        //    File.AppendAllText(physicalPath + "\\radius.txt","Distance:"+ item.Distance + ", zone:" + item.zonename + ",radius:" 
                        //        + item.biddingradius 
                        //        + ",destlat"+ Convert.ToDouble(item.DestLat) + ",destlong" + Convert.ToDouble(item.DestLon)


                        //        + Environment.NewLine);
                        //}
//
                    }
                    else
                    {

                        //arr = (from a in new TaxiDataContext().stp_getbiddingjobs().Where(c => c.driverid == driverId && c.zonename != "")

                        //       select new

                        //       {
                        //           Distance = a.biddingradius > 0 && a.JobLatitude != null && a.JobLatitude != 0 ? new LatLng(a.latitude, a.longitude).DistanceMiles(new LatLng(Convert.ToDouble(a.JobLatitude), Convert.ToDouble(a.JobLongitude))) : 0,
                        //           a.zonename,
                        //           a.biddingradius,
                        //           a.zoneid

                        //       }).Where(c => c.Distance <= c.biddingradius)
                        //        .GroupBy(args => new
                        //        {
                        //            args.zonename,
                        //            args.zoneid

                        //        })



                        //     .Select(args => new
                        //     {
                        //         args.Key.zoneid,
                        //         args.Key.zonename,
                        //         Jobs = args.Count()


                        //     })


                        //       .Select(args => (args.zoneid + "<<" + args.zonename + "<<" + args.Jobs)).ToArray<string>();



                        arr = (from a in new TaxiDataContext().ExecuteQuery<stp_getbiddingAvailablejobsResult>("exec stp_getbiddingjobs {0}", driverId)
                               .Where(c=> c.zonename != "")

                               select new

                               {
                                   Distance = a.biddingradius > 0 && a.JobLatitude != null && a.JobLatitude != 0 ? new LatLng(a.latitude, a.longitude).DistanceMiles(new LatLng(Convert.ToDouble(a.JobLatitude), Convert.ToDouble(a.JobLongitude))) : 0,
                                   a.zonename,
                                   a.biddingradius,
                                   a.zoneid

                               }).Where(c => c.Distance <= c.biddingradius)
                                    .GroupBy(args => new
                                    {
                                        args.zonename,
                                        args.zoneid

                                    })



                                 .Select(args => new
                                 {
                                     args.Key.zoneid,
                                     args.Key.zonename,
                                     Jobs = args.Count()


                                 })


                                   .Select(args => (args.zoneid + "<<" + args.zonename + "<<" + args.Jobs)).ToArray<string>();

                    }
                }

                if (arr != null)
                {
                    response = string.Join(">>", arr);


                }

                Clients.Caller.biddingList(response);

            }
            catch (Exception ex)
            {

                try
                {
                    File.AppendAllText(physicalPath + "\\requestBidding_exception.txt", DateTime.Now.ToStr() + "request : " + dataValue + " , exception : " + ex.Message + Environment.NewLine);

                }
                catch
                {


                }
            }


        }






        public void requestDriverBid(string msg)
        {



            string dataValue = msg;
            dataValue = dataValue.Trim();

            string[] values = dataValue.Split(new char[] { '=' });
            string response = string.Empty;

            int zoneId = values[1].ToInt();

            int driverId = values[2].ToInt();

            long jobId = 0;
            string zoneName = string.Empty;




            ClsBidAction objBid = null;


            try
            {
                if (values.Count() >= 5)
                {
                    zoneName = values[4].ToStr().Trim().ToUpper();
                }

                if (dataValue.Contains("jsonstring|"))
                {
                    objBid = new JavaScriptSerializer().Deserialize<ClsBidAction>(values[5].Replace("jsonstring|", ""));

                }


                if (Instance.objPolicy == null)
                    Instance.objPolicy = General.GetObject<Gen_SysPolicy_Configuration>(c => c.SysPolicyId == 1);



                using (TaxiDataContext db = new TaxiDataContext())
                {
                    db.CommandTimeout = 8;


                    if (objBid != null)
                    {
                        if (objBid.AllocatedJobId > 0 && objBid.Status == "8")
                        {

                            if (db.Bookings.Where(c => c.Id == objBid.AllocatedJobId &&
                                                   (c.DriverId == driverId && c.IsConfirmedDriver == true)
                                                   && c.BookingStatusId == Enums.BOOKINGSTATUS.WAITING
                                                    ).Count() > 0)
                            {

                                objBid.Message = "failed:You already have a job allocated.";
                                Clients.Caller.driverBid(new JavaScriptSerializer().Serialize(objBid));
                                return;
                            }
                            else
                            {
                                if (objBid.AllocatedJobId > 0 && Global.listofDrvBidding != null &&
                                   Global.listofDrvBidding.Where(c => c.DriverId == driverId && c.JobId != jobId && c.ElapsedTime > DateTime.Now).Count() > 0)
                                {
                                    var lastElapsedTime = Global.listofDrvBidding
                                        .Where(c => c.DriverId == driverId && c.JobId != jobId && c.ElapsedTime > DateTime.Now)
                                        .Select(c => c.ElapsedTime)
                                        .OrderByDescending(c => c).FirstOrDefault();

                                    var secs = (lastElapsedTime.Value.Subtract(DateTime.Now).TotalSeconds.ToInt()) + 5;

                                    objBid.Message = "failed:You have already bidded on other job." +
                                                Environment.NewLine + "Now you can bid on other job for next " + secs + " seconds";
                                    Clients.Caller.driverBid(new JavaScriptSerializer().Serialize(objBid));
                                    return;


                                }


                            }
                        }
                        else
                        {

                            if (objBid.Status == "7")
                            {
                                objBid.Message = "failed:You cannot bid on POB status";
                                Clients.Caller.driverBid(new JavaScriptSerializer().Serialize(objBid));
                                return;

                            }
                        }


                    }

                    Taxi_Model.Booking objBooking = null;



                    DateTime fromPickupDate = DateTime.Now.AddMinutes(-60);
                    DateTime tillPickupDate = DateTime.Now.AddMinutes(120);


                    if (zoneId > 0)
                    {

                        //
                        long BidjobId = 0;

                        //if (objBid != null && objBid.Status == "8")
                        //{
                        //    BidjobId = db.ExecuteQuery<long>("exec stp_getdriverbidSTCjob {0},{1},{2}", driverId, zoneId, zoneName).FirstOrDefault();


                        //   if(BidjobId==0)
                        //    {
                        //        objBid.Message = "failed:You have far away from this job";
                        //        Clients.Caller.driverBid(new JavaScriptSerializer().Serialize(objBid));
                        //        return;

                        //    }
                        //}
                        //else
                        //{
                        //    BidjobId= db.stp_getdriverbidjob(driverId, zoneId, zoneName).FirstOrDefault().DefaultIfEmpty().Id;
                        //   }

                        if (objBid != null && objBid.JobId > 0)
                        {
                            //  BidjobId = objBid.JobId;
                            BidjobId = db.ExecuteQuery<long>("exec stp_getdriverSinglebidjob {0},{1},{2},{3}", driverId, zoneId, zoneName, objBid.JobId).FirstOrDefault();

                        }
                        else
                            BidjobId = db.stp_getdriverbidjob(driverId, zoneId, zoneName).FirstOrDefault().DefaultIfEmpty().Id;

                        objBooking = db.Bookings.FirstOrDefault(c => c.Id == BidjobId);


                        //if (objBid!=null && objBid.Status =="7")
                        //{

                        //    string query = "update booking set ReAutoDespatchTime=getdate(), bookingstatusid=1,IsConfirmedDriver=1,driverId=" + driverId + " where id=" + BidjobId+ ";";
                        //    query += "INSERT INTO BOOKING_LOG (bookingid,[user],beforeupdate,afterupdate,updatedate)values(" + BidjobId + ",'" + "AutoDespatch" + "','" + "" + "','Auto Allocate STC Driver (" +objBid.DrvNo.ToStr() + ")',getdate());";
                        //    query += "Update fleet_driverqueuelist set isidle=1 where status=1 and driverid=" + driverId;
                        //    db.stp_RunProcedure(query);


                        //    response = "success:You have allocated";
                        //    Clients.Caller.driverBid(response);
                        //    return;

                        //}

                    }
                    else if (zoneName.ToStr().Length > 0)
                    {



                        objBooking = General.GetQueryable<Taxi_Model.Booking>(c => (c.PickupDateTime > fromPickupDate && c.PickupDateTime <= tillPickupDate) && (c.IsBidding != null && c.IsBidding == true)
                        && (c.BookingStatusId == Enums.BOOKINGSTATUS.BID)
                        && (c.ZoneId == null && c.FromPostCode != null && c.FromPostCode.Contains(" ") && c.FromPostCode.Substring(0, c.FromPostCode.IndexOf(" ")).Trim() == zoneName)
                        )
                                                 .OrderBy(c => c.PickupDateTime).FirstOrDefault();

                    }

                    //
                    if (objBooking != null)
                    {

                        jobId = objBooking.Id;



                        string journey = "O/W";
                        if (objBooking.JourneyTypeId.ToInt() == 2)
                        {
                            journey = "Return";
                        }
                        else if (objBooking.JourneyTypeId.ToInt() == 3)
                        {
                            journey = "W/R";
                        }


                        string IsExtra = (objBooking.CompanyId != null || objBooking.FromLocTypeId == Enums.LOCATION_TYPES.AIRPORT || objBooking.ToLocTypeId == Enums.LOCATION_TYPES.AIRPORT) ? "1" : "0";
                        int i = 0;
                        string viaP = "";

                        if (objBooking.Booking_ViaLocations.Count > 0)
                        {
                            viaP = "(" + (++i).ToStr() + ")" + string.Join(Environment.NewLine + "(" + (++i).ToStr() + ")", objBooking.Booking_ViaLocations.Select(c => c.ViaLocValue.ToStr()).ToArray<string>());
                        }


                        string specialRequirements = objBooking.SpecialRequirements.ToStr();
                        if (objBooking.SecondaryPaymentTypeId != null && objBooking.CashFares.ToDecimal() > 0)
                        {

                            specialRequirements += " , Additional Cash Payment : " + objBooking.CashFares.ToDecimal();
                        }

                        decimal pdafares = objBooking.GetType().GetProperty(Instance.objPolicy.PDAFaresPropertyName.ToStr().Trim()).GetValue(objBooking, null).ToDecimal();

                        string showFaresValue = objBooking.IsQuotedPrice.ToBool() == true ? "1" : objBooking.Gen_PaymentType.ShowFaresOnPDA.ToStr().Trim();

                        //if (Instance.objPolicy.PDAFaresPropertyName.ToStr().Trim() == "FareRate")
                        //{
                        //    pdafares = pdafares + objBooking.ServiceCharges.ToDecimal() + objBooking.CongtionCharges.ToDecimal() + objBooking.MeetAndGreetCharges.ToDecimal() + objBooking.ExtraDropCharges.ToDecimal();


                        //}
                        //  pdafares = objBooking.TotalCharges.ToDecimal();


                        pdafares = objBooking.FareRate.ToDecimal() + objBooking.MeetAndGreetCharges.ToDecimal() + objBooking.CongtionCharges.ToDecimal()
                            + objBooking.AgentCommission.ToDecimal()
                        //+ objBooking.CashRate.ToDecimal() + objBooking.CashFares.ToDecimal() +
                        + objBooking.ExtraDropCharges.ToDecimal() + objBooking.ServiceCharges.ToDecimal();


                        if (showFaresValue.ToStr() == "1" && objBooking.CompanyId != null && (objBooking.PaymentTypeId.ToInt() == 2 || objBooking.PaymentTypeId.ToInt() == 6))
                        {
                            pdafares = objBooking.CompanyPrice.ToDecimal() + objBooking.MeetAndGreetCharges.ToDecimal() + objBooking.CongtionCharges.ToDecimal()
                                 + objBooking.AgentCommission.ToDecimal()
                             //+ objBooking.CashRate.ToDecimal() + objBooking.CashFares.ToDecimal() +
                             + objBooking.ExtraDropCharges.ToDecimal() + objBooking.ServiceCharges.ToDecimal();
                        }

                        msg = string.Empty;

                        string mobileNo = objBooking.CustomerMobileNo.ToStr();
                        string telNo = objBooking.CustomerPhoneNo.ToStr();

                        //  Fleet_Driver ObjDriver = General.GetObject<Fleet_Driver>(c => c.Id == driverId);

                        //  decimal drvPdaVersion = ObjDriver.Fleet_Driver_PDASettings.Count > 0 ? ObjDriver.Fleet_Driver_PDASettings[0].CurrentPdaVersion.ToDecimal() : 9.80m;

                        decimal drvPdaVersion = 20.00m;

                        if (string.IsNullOrEmpty(mobileNo) && !string.IsNullOrEmpty(telNo))
                        {
                            mobileNo = telNo;
                        }
                        else if (!string.IsNullOrEmpty(mobileNo) && !string.IsNullOrEmpty(telNo))
                        {
                            mobileNo += "/" + telNo;
                        }

                        if (drvPdaVersion >= 11 && Instance.objPolicy.PDAJobAlertOnly.ToBool() == false)
                        {
                            //  string showFaresValue = objBooking.Gen_PaymentType.ShowFaresOnPDA.ToStr().Trim();




                            string showFares = ",\"ShowFares\":\"" + showFaresValue + "\"";
                            string showSummary = ",\"ShowSummary\":\"" + showFaresValue + "\"";
                            //   string showSummary = string.Empty;

                            string agentDetails = string.Empty;
                            string parkingandWaiting = string.Empty;
                            if (objBooking.CompanyId != null)
                            {
                                agentDetails = ",\"AgentFees\":\"" + String.Format("{0:0.00}", objBooking.AgentCommission + objBooking.ServiceCharges.ToDecimal()) + "\"";
                                parkingandWaiting = ",\"Parking\":\"" + string.Format("{0:0.00}", objBooking.ParkingCharges) + "\",\"Waiting\":\"" + String.Format("{0:0.00}", objBooking.WaitingCharges) + "\"";

                            }
                            else
                            {
                                agentDetails = ",\"AgentFees\":\"" + String.Format("{0:0.00}", objBooking.ServiceCharges.ToDecimal()) + "\"";

                                parkingandWaiting = ",\"Parking\":\"" + string.Format("{0:0.00}", objBooking.CongtionCharges) + "\",\"Waiting\":\"" + String.Format("{0:0.00}", objBooking.MeetAndGreetCharges) + "\"";
                                //

                            }



                            string fromAddress = objBooking.FromAddress.ToStr().Trim();
                            string toAddress = objBooking.ToAddress.ToStr().Trim();

                            if (objBooking.FromLocTypeId.ToInt() == Enums.LOCATION_TYPES.POSTCODE || objBooking.FromLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT)
                            {
                                fromAddress = objBooking.FromStreet.ToStr() + " " + objBooking.FromAddress.ToStr();

                            }

                            if (objBooking.ToLocTypeId.ToInt() == Enums.LOCATION_TYPES.POSTCODE || objBooking.ToLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT)
                            {
                                toAddress = objBooking.ToStreet.ToStr() + " " + objBooking.ToAddress.ToStr();
                            }

                            string paymentType = objBooking.Gen_PaymentType.PaymentCategoryId == null ? objBooking.Gen_PaymentType.DefaultIfEmpty().PaymentType.ToStr()
                                                          : objBooking.Gen_PaymentType.Gen_PaymentCategory.CategoryName.ToStr();



                            string companyName = string.Empty;

                            if (drvPdaVersion < 11 && objBooking.CompanyId != null && objBooking.Gen_Company.DefaultIfEmpty().AccountTypeId.ToInt() != Enums.ACCOUNT_TYPE.CASH)
                                companyName = objBooking.Gen_Company.DefaultIfEmpty().CompanyName;
                            else
                                companyName = objBooking.Gen_Company.DefaultIfEmpty().CompanyName.ToStr();



                            string pickUpPlot = "";
                            string dropOffPlot = "";
                            if (drvPdaVersion > 9 && drvPdaVersion != 13.4m)
                            {
                                pickUpPlot = objBooking.ZoneId != null ? "<<<" + objBooking.Gen_Zone1.DefaultIfEmpty().ZoneName.ToStr() : "";
                                dropOffPlot = objBooking.DropOffZoneId != null ? "<<<" + objBooking.Gen_Zone.DefaultIfEmpty().ZoneName.ToStr() : "";
                            }


                            string fromdoorno = objBooking.FromDoorNo.ToStr().Trim();

                            if (drvPdaVersion == 23.50m)
                            {

                                if (fromdoorno.Length > 0 && fromdoorno.WordCount() > 2 && fromdoorno.Contains(" "))
                                {

                                    try
                                    {

                                        fromdoorno = fromdoorno.Replace(" ", "-");
                                    }
                                    catch
                                    {


                                    }
                                }

                                if (fromAddress.ToStr().Trim().Contains("-"))
                                {
                                    fromAddress = fromAddress.Replace("-", "  ");

                                }
                            }





                            string pickupDateTime = string.Format("{0:dd/MM/yyyy   HH:mm}", objBooking.PickupDateTime);




                            //try
                            //{
                            //    if (objBooking.BookingDate.Value.AddMinutes(10) > objBooking.PickupDateTime.Value)
                            //    {

                            //        pickupDateTime = pickupDateTime + "<<ASAP";
                            //    }
                            //}
                            //catch
                            //{

                            //}

                            string appendString = "";


                            try
                            {
                                appendString = ",\"ShowOnlyPlot\":\"" + "0" + "\"" +
                                 ",\"ExtraCharges\":\"" + objBooking.ExtraDropCharges.ToDecimal() + "\"" +
                                  ",\"BookingFee\":\"" + 0.00 + "\"" +
                                  ",\"BgColor\":\"" + "" + "\"";

                                //if (objBooking.BookingDate.Value.AddMinutes(10) > objBooking.PickupDateTime.Value)
                                //{

                                //    appendString += ",\"priority\":\"" + "ASAP" + "\"";
                                //    //
                                //}
                            }
                            catch
                            {

                            }


                            response = "JobId:" + "{ \"JobId\" :\"" + objBooking.Id.ToStr() +
                         "\", \"Pickup\":\"" + (!string.IsNullOrEmpty(objBooking.FromDoorNo) ? fromdoorno + "-" + fromAddress + pickUpPlot : fromAddress + pickUpPlot) +
                         "\", \"Destination\":\"" + (!string.IsNullOrEmpty(objBooking.ToDoorNo) ? objBooking.ToDoorNo + "-" + toAddress + dropOffPlot : toAddress + dropOffPlot) + "\"," +
                         "\"PickupDateTime\":\"" + pickupDateTime + "\"" +
                         ",\"Cust\":\"" + objBooking.CustomerName + "\",\"Mob\":\"" + mobileNo + " " + "\",\"Fare\":\"" + string.Format("{0:0.00}", pdafares) + "\",\"Vehicle\":\"" + objBooking.Fleet_VehicleType.VehicleType + "\",\"Account\":\"" + companyName + " " + "\"" +
                           ",\"Lug\":\"" + objBooking.NoofLuggages.ToInt() + "\",\"Passengers\":\"" + objBooking.NoofPassengers.ToInt() + "\",\"Journey\":\"" + journey + "\",\"Payment\":\"" + paymentType + "\",\"Special\":\"" + specialRequirements + " " + "\",\"Extra\":\"" + IsExtra + "\",\"Via\":\"" + viaP + " " + "\"" +
                        parkingandWaiting + ",\"DriverFares\":\"" + String.Format("{0:0.00}", objBooking.FareRate) + "\"" +
                        agentDetails +
                           ",\"Did\":\"" + driverId + "\",\"BabySeats\":\"" + objBooking.BabySeats.ToStr() + "\"" + showFares + showSummary + appendString + " }";


                            if (response.Contains("\r\n"))
                            {
                                response = response.Replace("\r\n", " ").Trim();
                            }
                            else
                            {
                                if (response.Contains("\n"))
                                {
                                    response = response.Replace("\n", " ").Trim();

                                }

                            }

                            if (response.Contains("&"))
                            {
                                response = response.Replace("&", "And");
                            }

                            if (response.Contains(">"))
                                response = response.Replace(">", " ");


                            if (response.Contains("="))
                                response = response.Replace("=", " ");

                        }
                        else
                        {



                            response = "JobId:" + objBooking.Id +
                                ":Pickup:" + (!string.IsNullOrEmpty(objBooking.FromDoorNo) ? objBooking.FromDoorNo + "-" + objBooking.FromAddress : objBooking.FromAddress) +

                                ":Destination:" + (!string.IsNullOrEmpty(objBooking.ToDoorNo) ? objBooking.ToDoorNo + "-" + objBooking.ToAddress : objBooking.ToAddress) +
                                  ":PickupDateTime:" + string.Format("{0:dd/MM/yyyy      HH:mm}", objBooking.PickupDateTime) +
                                       ":Cust:" + objBooking.CustomerName + ":Mob:" + objBooking.CustomerMobileNo.ToStr() + " " + ":Fare:" + objBooking.FareRate
                                      + ":Vehicle:" + objBooking.Fleet_VehicleType.VehicleType + ":Account:" + objBooking.Gen_Company.DefaultIfEmpty().CompanyName + " " +
                                      ":Lug:" + objBooking.NoofLuggages.ToInt() + ":Passengers:" + objBooking.NoofPassengers.ToInt() + ":Journey:" + journey +
                                      ":Payment:" + objBooking.Gen_PaymentType.DefaultIfEmpty().PaymentType.ToStr() + ":Special:" + objBooking.SpecialRequirements.ToStr() + " "
                                      + ":Extra:" + IsExtra + ":Via:" + viaP + " " + ":Did:" + driverId;
                        }


                        if (Instance.objPolicy.BiddingType.ToInt() == Enums.BIDDING_TYPES.FASTEST_FINGER
                            || Instance.objPolicy.BiddingType.ToInt() == Enums.BIDDING_TYPES.NEAREST_DRIVER
                            || Instance.objPolicy.BiddingType.ToInt() == Enums.BIDDING_TYPES.LONGEST_WAITING_QUEUE)
                        {


                            DateTime elapedTime = DateTime.Now.AddSeconds(Instance.objPolicy.BiddingElapsedTime.ToInt());


                            if (Instance.objPolicy != null && Instance.objPolicy.BiddingType.ToInt() == Enums.BIDDING_TYPES.FASTEST_FINGER)
                            {
                                elapedTime = DateTime.Now;


                            }

                            if (Global.listofDrvBidding == null)
                                Global.listofDrvBidding = new List<ClsDriverBid>();


                            //ClsDriverBid objFirstBidJob = null;


                            //if(listofDrvBidding!=null)
                            //    listofDrvBidding.FirstOrDefault(c => c.JobId == jobId && c.DriverId == driverId);

                            //if (objFirstBidJob != null)
                            //{
                            //    objFirstBidJob.ElapsedTime = objFirstBidJob.ElapsedTime.ToDateTime();

                            //}

                            var objFirstBidJob = Global.listofDrvBidding.FirstOrDefault(c => c.JobId == jobId);

                            if (objFirstBidJob != null)
                            {
                                elapedTime = objFirstBidJob.ElapsedTime.ToDateTime();

                            }


                            if (objFirstBidJob != null && objFirstBidJob.DriverId == driverId)
                            {
                                objFirstBidJob.ElapsedTime = elapedTime;

                            }
                            else
                            {

                                //
                                ClsDriverBid obj = new ClsDriverBid();
                                obj.JobMessage = response;
                                obj.DriverId = driverId;
                                obj.JobZoneId = zoneId;
                                obj.JobZoneName = zoneName.ToStr();
                                obj.JobId = jobId;
                                obj.BiddingDateTime = DateTime.Now;
                                obj.BiddingType = Instance.objPolicy.BiddingType.ToInt();
                                //  obj.FromPostCode = objBooking.FromPostCode.ToStr().Trim().ToUpper();
                                //  obj.FromAddress = objBooking.FromAddress.ToStr().Trim().ToUpper();

                                obj.ElapsedTime = elapedTime;


                                if (objBid != null)
                                    obj.DriverNo = objBid.DrvNo.ToStr();

                                Global.listofDrvBidding.Add(obj);
                            }


                            try
                            {
                                if (Instance.listofJobs.Count(c => c.DriverId == driverId && c.MessageTypeId == eMessageTypes.BIDALERT) > 0)
                                {
                                    Instance.listofJobs.RemoveAll(c => c.DriverId == driverId && c.MessageTypeId == eMessageTypes.BIDALERT);


                                }
                            }
                            catch
                            {


                            }


                            response = "You Bidding Request has been sent successfully!:";



                            General.BroadCastMessage("**driver bid>>" + jobId + ">>" + driverId);

                        }


                    }
                    else
                    {
                        response = "failed:";
                    }

                }

                //   }


            }
            catch (Exception ex)
            {
                response = "failed:";


                try
                {
                    File.AppendAllText(physicalPath + "\\requestdriverBid_exception.txt", DateTime.Now.ToStr() + ex.Message + Environment.NewLine);

                }
                catch
                {


                }


            }


            if (response != "failed:" && objBid != null)
            {
                objBid.Message = response;

                if (objBid.Status == "8")
                {

                    objBid.AllocatedJobId = jobId;
                }

                var res = new JavaScriptSerializer().Serialize(objBid);
                Clients.Caller.driverBid(res);
            }
            else
            {

                Clients.Caller.driverBid(response);
            }

            if (response != "failed:")
            {
                int? bidStatusId = 2;

                if (response == "You Bidding Request has been sent successfully!:")
                    bidStatusId = 4;



                General.SP_SaveBid(jobId, driverId, 0.00m, bidStatusId, "", "");

            }

        }




        public void requestdriverJobBid(string msg)
        {



            //string dataValue = msg;
            //dataValue = dataValue.Trim();

            //string[] values = dataValue.Split(new char[] { '=' });
            //string response = string.Empty;

            //int zoneId = values[1].ToInt();

            //int driverId = values[2].ToInt();

            //long jobId = 0;
            //string zoneName = string.Empty;




            string dataValue = msg;
            dataValue = dataValue.Trim();
            ClsDriverBid objAction = new JavaScriptSerializer().Deserialize<ClsDriverBid>(dataValue);



            string response = string.Empty;

            int zoneId = objAction.JobZoneId.ToInt();

            int driverId = objAction.DriverId.ToInt();
            long jobId = objAction.JobId.ToLong();

            string zoneName = objAction.JobZoneName.ToStr();
            //   decimal drvPrice = objAction.DriverPrice.ToDecimal();




            ClsBidAction objBid = null;


            try
            {


                //if (dataValue.Contains("jsonstring|"))
                //{
                //    objBid = new JavaScriptSerializer().Deserialize<ClsBidAction>(values[5].Replace("jsonstring|", ""));

                //}


                if (Instance.objPolicy == null)
                    Instance.objPolicy = General.GetObject<Gen_SysPolicy_Configuration>(c => c.SysPolicyId == 1);



                using (TaxiDataContext db = new TaxiDataContext())
                {
                    db.CommandTimeout = 8;


                    //if (objBid != null)
                    //{
                    //    if (objBid.AllocatedJobId > 0 && objBid.Status == "8")
                    //    {

                    //        if (db.Bookings.Where(c => c.Id == objBid.AllocatedJobId &&
                    //                               (c.DriverId == driverId && c.IsConfirmedDriver == true)
                    //                               && c.BookingStatusId == Enums.BOOKINGSTATUS.WAITING
                    //                                ).Count() > 0)
                    //        {

                    //            objBid.Message = "failed:You already have a job allocated.";
                    //            Clients.Caller.driverBid(new JavaScriptSerializer().Serialize(objBid));
                    //            return;
                    //        }
                    //        else
                    //        {
                    //            if (objBid.AllocatedJobId > 0 && listofDrvBidding != null &&
                    //                listofDrvBidding.Where(c => c.DriverId == driverId && c.JobId != jobId && c.ElapsedTime > DateTime.Now).Count() > 0)
                    //            {
                    //                var lastElapsedTime = listofDrvBidding
                    //                    .Where(c => c.DriverId == driverId && c.JobId != jobId && c.ElapsedTime > DateTime.Now)
                    //                    .Select(c => c.ElapsedTime)
                    //                    .OrderByDescending(c => c).FirstOrDefault();

                    //                var secs = (lastElapsedTime.Value.Subtract(DateTime.Now).TotalSeconds.ToInt()) + 5;

                    //                objBid.Message = "failed:You have already bidded on other job." +
                    //                            Environment.NewLine + "Now you can bid on other job for next " + secs + " seconds";
                    //                Clients.Caller.driverBid(new JavaScriptSerializer().Serialize(objBid));
                    //                return;


                    //            }


                    //        }
                    //    }
                    //    else
                    //    {

                    //        if (objBid.Status == "7")
                    //        {
                    //            objBid.Message = "failed:You cannot bid on POB status";
                    //            Clients.Caller.driverBid(new JavaScriptSerializer().Serialize(objBid));
                    //            return;

                    //        }
                    //    }


                    //}

                    Taxi_Model.Booking objBooking = null;



                    DateTime fromPickupDate = DateTime.Now.AddMinutes(-60);
                    DateTime tillPickupDate = DateTime.Now.AddMinutes(120);


                    if (zoneId > 0)
                    {


                        // long BidjobId = 0;


                        //BidjobId = db.stp_getdriverbidjob(driverId, zoneId, zoneName).FirstOrDefault().DefaultIfEmpty().Id;


                        objBooking = db.Bookings.FirstOrDefault(c => c.Id == jobId);

                        //

                    }


                    //
                    if (objBooking != null && objBooking.BookingStatusId.ToInt() == Enums.BOOKINGSTATUS.BID && objBooking.IsBidding.ToBool() && objBooking.ZoneId != null)
                    {

                        jobId = objBooking.Id;

                        string journey = "O/W";
                        if (objBooking.JourneyTypeId.ToInt() == 2)
                        {
                            journey = "Return";
                        }
                        else if (objBooking.JourneyTypeId.ToInt() == 3)
                        {
                            journey = "W/R";
                        }


                        string IsExtra = (objBooking.CompanyId != null || objBooking.FromLocTypeId == Enums.LOCATION_TYPES.AIRPORT || objBooking.ToLocTypeId == Enums.LOCATION_TYPES.AIRPORT) ? "1" : "0";
                        int i = 0;
                        string viaP = "";

                        if (objBooking.Booking_ViaLocations.Count > 0)
                        {
                            viaP = "(" + (++i).ToStr() + ")" + string.Join(Environment.NewLine + "(" + (++i).ToStr() + ")", objBooking.Booking_ViaLocations.Select(c => c.ViaLocValue.ToStr()).ToArray<string>());
                        }


                        string specialRequirements = objBooking.SpecialRequirements.ToStr();
                        if (objBooking.SecondaryPaymentTypeId != null && objBooking.CashFares.ToDecimal() > 0)
                        {

                            specialRequirements += " , Additional Cash Payment : " + objBooking.CashFares.ToDecimal();
                        }

                        decimal pdafares = objBooking.GetType().GetProperty(Instance.objPolicy.PDAFaresPropertyName.ToStr().Trim()).GetValue(objBooking, null).ToDecimal();

                        //  pdafares = objBooking.TotalCharges.ToDecimal();

                        string showFaresValue = objBooking.IsQuotedPrice.ToBool() == true ? "1" : objBooking.Gen_PaymentType.ShowFaresOnPDA.ToStr().Trim();

                        //if (Instance.objPolicy.PDAFaresPropertyName.ToStr().Trim() == "FareRate")
                        //{
                        //    pdafares = pdafares + objBooking.ServiceCharges.ToDecimal() + objBooking.CongtionCharges.ToDecimal() + objBooking.MeetAndGreetCharges.ToDecimal() + objBooking.ExtraDropCharges.ToDecimal();


                        //}
                        //  pdafares = objBooking.TotalCharges.ToDecimal();


                        pdafares = objBooking.FareRate.ToDecimal() + objBooking.MeetAndGreetCharges.ToDecimal() + objBooking.CongtionCharges.ToDecimal()
                            + objBooking.AgentCommission.ToDecimal()
                        //+ objBooking.CashRate.ToDecimal() + objBooking.CashFares.ToDecimal() +
                        + objBooking.ExtraDropCharges.ToDecimal() + objBooking.ServiceCharges.ToDecimal();


                        if (showFaresValue.ToStr() == "1" && objBooking.CompanyId != null && (objBooking.PaymentTypeId.ToInt() == 2 || objBooking.PaymentTypeId.ToInt() == 6))
                        {
                            pdafares = objBooking.CompanyPrice.ToDecimal() + objBooking.MeetAndGreetCharges.ToDecimal() + objBooking.CongtionCharges.ToDecimal()
                                 + objBooking.AgentCommission.ToDecimal()
                             //+ objBooking.CashRate.ToDecimal() + objBooking.CashFares.ToDecimal() +
                             + objBooking.ExtraDropCharges.ToDecimal() + objBooking.ServiceCharges.ToDecimal();
                        }

                        msg = string.Empty;

                        string mobileNo = objBooking.CustomerMobileNo.ToStr();
                        string telNo = objBooking.CustomerPhoneNo.ToStr();

                        //  Fleet_Driver ObjDriver = General.GetObject<Fleet_Driver>(c => c.Id == driverId);

                        //  decimal drvPdaVersion = ObjDriver.Fleet_Driver_PDASettings.Count > 0 ? ObjDriver.Fleet_Driver_PDASettings[0].CurrentPdaVersion.ToDecimal() : 9.80m;

                        decimal drvPdaVersion = 20.00m;

                        if (string.IsNullOrEmpty(mobileNo) && !string.IsNullOrEmpty(telNo))
                        {
                            mobileNo = telNo;
                        }
                        else if (!string.IsNullOrEmpty(mobileNo) && !string.IsNullOrEmpty(telNo))
                        {
                            mobileNo += "/" + telNo;
                        }

                        if (drvPdaVersion >= 11 && Instance.objPolicy.PDAJobAlertOnly.ToBool() == false)
                        {
                            //  string showFaresValue = objBooking.Gen_PaymentType.ShowFaresOnPDA.ToStr().Trim();

                            //    string showFaresValue = objBooking.IsQuotedPrice.ToBool() == true ? "1" : objBooking.Gen_PaymentType.ShowFaresOnPDA.ToStr().Trim();



                            string showFares = ",\"ShowFares\":\"" + showFaresValue + "\"";
                            string showSummary = ",\"ShowSummary\":\"" + showFaresValue + "\"";
                            //   string showSummary = string.Empty;

                            string agentDetails = string.Empty;
                            string parkingandWaiting = string.Empty;
                            if (objBooking.CompanyId != null)
                            {
                                agentDetails = ",\"AgentFees\":\"" + String.Format("{0:0.00}", objBooking.AgentCommission + objBooking.ServiceCharges.ToDecimal()) + "\"";
                                parkingandWaiting = ",\"Parking\":\"" + string.Format("{0:0.00}", objBooking.ParkingCharges) + "\",\"Waiting\":\"" + String.Format("{0:0.00}", objBooking.WaitingCharges) + "\"";

                            }
                            else
                            {
                                agentDetails = ",\"AgentFees\":\"" + String.Format("{0:0.00}", objBooking.ServiceCharges.ToDecimal()) + "\"";
                                parkingandWaiting = ",\"Parking\":\"" + string.Format("{0:0.00}", objBooking.CongtionCharges) + "\",\"Waiting\":\"" + String.Format("{0:0.00}", objBooking.MeetAndGreetCharges) + "\"";
                                //

                            }



                            string fromAddress = objBooking.FromAddress.ToStr().Trim();
                            string toAddress = objBooking.ToAddress.ToStr().Trim();

                            if (objBooking.FromLocTypeId.ToInt() == Enums.LOCATION_TYPES.POSTCODE || objBooking.FromLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT)
                            {
                                fromAddress = objBooking.FromStreet.ToStr() + " " + objBooking.FromAddress.ToStr();

                            }

                            if (objBooking.ToLocTypeId.ToInt() == Enums.LOCATION_TYPES.POSTCODE || objBooking.ToLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT)
                            {
                                toAddress = objBooking.ToStreet.ToStr() + " " + objBooking.ToAddress.ToStr();
                            }

                            string paymentType = objBooking.Gen_PaymentType.PaymentCategoryId == null ? objBooking.Gen_PaymentType.DefaultIfEmpty().PaymentType.ToStr()
                                                          : objBooking.Gen_PaymentType.Gen_PaymentCategory.CategoryName.ToStr();



                            string companyName = string.Empty;

                            if (drvPdaVersion < 11 && objBooking.CompanyId != null && objBooking.Gen_Company.DefaultIfEmpty().AccountTypeId.ToInt() != Enums.ACCOUNT_TYPE.CASH)
                                companyName = objBooking.Gen_Company.DefaultIfEmpty().CompanyName;
                            else
                                companyName = objBooking.Gen_Company.DefaultIfEmpty().CompanyName.ToStr();



                            string pickUpPlot = "";
                            string dropOffPlot = "";
                            if (drvPdaVersion > 9 && drvPdaVersion != 13.4m)
                            {
                                pickUpPlot = objBooking.ZoneId != null ? "<<<" + objBooking.Gen_Zone1.DefaultIfEmpty().ZoneName.ToStr() : "";
                                dropOffPlot = objBooking.DropOffZoneId != null ? "<<<" + objBooking.Gen_Zone.DefaultIfEmpty().ZoneName.ToStr() : "";
                            }


                            string fromdoorno = objBooking.FromDoorNo.ToStr().Trim();

                            if (drvPdaVersion == 23.50m)
                            {

                                if (fromdoorno.Length > 0 && fromdoorno.WordCount() > 2 && fromdoorno.Contains(" "))
                                {

                                    try
                                    {

                                        fromdoorno = fromdoorno.Replace(" ", "-");
                                    }
                                    catch
                                    {


                                    }
                                }

                                if (fromAddress.ToStr().Trim().Contains("-"))
                                {
                                    fromAddress = fromAddress.Replace("-", "  ");

                                }
                            }





                            string pickupDateTime = string.Format("{0:dd/MM/yyyy   HH:mm}", objBooking.PickupDateTime);




                            //try
                            //{
                            //    if (objBooking.BookingDate.Value.AddMinutes(10) > objBooking.PickupDateTime.Value)
                            //    {

                            //        pickupDateTime = pickupDateTime + "<<ASAP";
                            //    }
                            //}
                            //catch
                            //{

                            //}

                            string appendString = "";


                            try
                            {
                                appendString = ",\"ShowOnlyPlot\":\"" + "0" + "\"" +
                                 ",\"ExtraCharges\":\"" + objBooking.ExtraDropCharges.ToDecimal() + "\"" +
                                  ",\"BookingFee\":\"" + 0.00 + "\"" +
                                  ",\"BgColor\":\"" + "" + "\"";

                                //if (objBooking.BookingDate.Value.AddMinutes(10) > objBooking.PickupDateTime.Value)
                                //{

                                //    appendString += ",\"priority\":\"" + "ASAP" + "\"";
                                //    //
                                //}
                            }
                            catch
                            {

                            }


                            response = "JobId:" + "{ \"JobId\" :\"" + objBooking.Id.ToStr() +
                         "\", \"Pickup\":\"" + (!string.IsNullOrEmpty(objBooking.FromDoorNo) ? fromdoorno + "-" + fromAddress + pickUpPlot : fromAddress + pickUpPlot) +
                         "\", \"Destination\":\"" + (!string.IsNullOrEmpty(objBooking.ToDoorNo) ? objBooking.ToDoorNo + "-" + toAddress + dropOffPlot : toAddress + dropOffPlot) + "\"," +
                         "\"PickupDateTime\":\"" + pickupDateTime + "\"" +
                         ",\"Cust\":\"" + objBooking.CustomerName + "\",\"Mob\":\"" + mobileNo + " " + "\",\"Fare\":\"" + string.Format("{0:0.00}", pdafares) + "\",\"Vehicle\":\"" + objBooking.Fleet_VehicleType.VehicleType + "\",\"Account\":\"" + companyName + " " + "\"" +
                           ",\"Lug\":\"" + objBooking.NoofLuggages.ToInt() + "\",\"Passengers\":\"" + objBooking.NoofPassengers.ToInt() + "\",\"Journey\":\"" + journey + "\",\"Payment\":\"" + paymentType + "\",\"Special\":\"" + specialRequirements + " " + "\",\"Extra\":\"" + IsExtra + "\",\"Via\":\"" + viaP + " " + "\"" +
                        parkingandWaiting + ",\"DriverFares\":\"" + String.Format("{0:0.00}", objBooking.FareRate) + "\"" +
                        agentDetails +
                           ",\"Did\":\"" + driverId + "\",\"BabySeats\":\"" + objBooking.BabySeats.ToStr() + "\"" + showFares + showSummary + appendString + " }";


                            if (response.Contains("\r\n"))
                            {
                                response = response.Replace("\r\n", " ").Trim();
                            }
                            else
                            {
                                if (response.Contains("\n"))
                                {
                                    response = response.Replace("\n", " ").Trim();

                                }

                            }

                            if (response.Contains("&"))
                            {
                                response = response.Replace("&", "And");
                            }

                            if (response.Contains(">"))
                                response = response.Replace(">", " ");


                            if (response.Contains("="))
                                response = response.Replace("=", " ");

                        }
                        else
                        {



                            response = "JobId:" + objBooking.Id +
                                ":Pickup:" + (!string.IsNullOrEmpty(objBooking.FromDoorNo) ? objBooking.FromDoorNo + "-" + objBooking.FromAddress : objBooking.FromAddress) +

                                ":Destination:" + (!string.IsNullOrEmpty(objBooking.ToDoorNo) ? objBooking.ToDoorNo + "-" + objBooking.ToAddress : objBooking.ToAddress) +
                                  ":PickupDateTime:" + string.Format("{0:dd/MM/yyyy      HH:mm}", objBooking.PickupDateTime) +
                                       ":Cust:" + objBooking.CustomerName + ":Mob:" + objBooking.CustomerMobileNo.ToStr() + " " + ":Fare:" + objBooking.FareRate
                                      + ":Vehicle:" + objBooking.Fleet_VehicleType.VehicleType + ":Account:" + objBooking.Gen_Company.DefaultIfEmpty().CompanyName + " " +
                                      ":Lug:" + objBooking.NoofLuggages.ToInt() + ":Passengers:" + objBooking.NoofPassengers.ToInt() + ":Journey:" + journey +
                                      ":Payment:" + objBooking.Gen_PaymentType.DefaultIfEmpty().PaymentType.ToStr() + ":Special:" + objBooking.SpecialRequirements.ToStr() + " "
                                      + ":Extra:" + IsExtra + ":Via:" + viaP + " " + ":Did:" + driverId;
                        }


                        if (Instance.objPolicy.BiddingType.ToInt() == Enums.BIDDING_TYPES.FASTEST_FINGER
                            || Instance.objPolicy.BiddingType.ToInt() == Enums.BIDDING_TYPES.NEAREST_DRIVER
                            || Instance.objPolicy.BiddingType.ToInt() == Enums.BIDDING_TYPES.LONGEST_WAITING_QUEUE)
                        {


                            DateTime elapedTime = DateTime.Now.AddSeconds(Instance.objPolicy.BiddingElapsedTime.ToInt());


                            if (Instance.objPolicy != null && Instance.objPolicy.BiddingType.ToInt() == Enums.BIDDING_TYPES.FASTEST_FINGER)
                            {
                                elapedTime = DateTime.Now;


                            }

                            if (Global.listofDrvBidding == null)
                                Global.listofDrvBidding = new List<ClsDriverBid>();


                            //ClsDriverBid objFirstBidJob = null;


                            //if(listofDrvBidding!=null)
                            //    listofDrvBidding.FirstOrDefault(c => c.JobId == jobId && c.DriverId == driverId);

                            //if (objFirstBidJob != null)
                            //{
                            //    objFirstBidJob.ElapsedTime = objFirstBidJob.ElapsedTime.ToDateTime();

                            //}

                            var objFirstBidJob = Global.listofDrvBidding.FirstOrDefault(c => c.JobId == jobId);

                            if (objFirstBidJob != null)
                            {
                                elapedTime = objFirstBidJob.ElapsedTime.ToDateTime();

                            }


                            if (objFirstBidJob != null && objFirstBidJob.DriverId == driverId)
                            {
                                objFirstBidJob.ElapsedTime = elapedTime;

                            }
                            else
                            {

                                //
                                ClsDriverBid obj = new ClsDriverBid();
                                obj.JobMessage = response;
                                obj.DriverId = driverId;
                                obj.JobZoneId = zoneId;
                                obj.JobZoneName = zoneName.ToStr();
                                obj.JobId = jobId;
                                obj.BiddingDateTime = DateTime.Now;
                                obj.BiddingType = Instance.objPolicy.BiddingType.ToInt();
                                //  obj.FromPostCode = objBooking.FromPostCode.ToStr().Trim().ToUpper();
                                //  obj.FromAddress = objBooking.FromAddress.ToStr().Trim().ToUpper();

                                obj.ElapsedTime = elapedTime;


                                if (objBid != null)
                                    obj.DriverNo = objBid.DrvNo.ToStr();

                                Global.listofDrvBidding.Add(obj);
                            }


                            try
                            {
                                if (Instance.listofJobs.Count(c => c.DriverId == driverId && c.MessageTypeId == eMessageTypes.BIDALERT) > 0)
                                {
                                    Instance.listofJobs.RemoveAll(c => c.DriverId == driverId && c.MessageTypeId == eMessageTypes.BIDALERT);


                                }
                            }
                            catch
                            {


                            }


                            response = "You Bidding Request has been sent successfully!:";



                            General.BroadCastMessage("**driver bid>>" + jobId + ">>" + driverId);

                        }


                    }
                    else
                    {
                        response = "failed:";
                    }

                }

                //   }


            }
            catch (Exception ex)
            {
                response = "failed:";


                try
                {
                    File.AppendAllText(physicalPath + "\\requestdriverBid_exception.txt", DateTime.Now.ToStr() + ex.Message + Environment.NewLine);

                }
                catch
                {


                }


            }


            try
            {
                if (response != "failed:" && objBid != null)
                {
                    objBid.Message = response;

                    if (objBid.Status == "8")
                    {

                        objBid.AllocatedJobId = jobId;
                    }

                    var res = new JavaScriptSerializer().Serialize(objBid);
                    Clients.Caller.driverJobBid(res);
                }
                else
                {

                    Clients.Caller.driverJobBid(response);
                }

                if (response != "failed:")
                {
                    int? bidStatusId = 2;

                    if (response == "You Bidding Request has been sent successfully!:")
                        bidStatusId = 4;



                    General.SP_SaveBid(jobId, driverId, 0.00m, bidStatusId, "", "");

                }

            }
            catch
            {
                response = "failed:";
                Clients.Caller.driverJobBid(response);
            }

        }








        #endregion


    }


}