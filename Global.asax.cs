using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Taxi_BLL;
using Taxi_Model;
using Utils;
using AsterNET;



using System.Configuration;
using System.IO;

using HMSMS = SMSGateway.HypermediaGateway;
using DSSMS = SMSGateway.DinstarSMSGateway;
using AsterNET.Manager.Event;
using AsterNET.Manager;

using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Http;
using DotNetCoords;
using System.Threading;
using System.Data;
using System.Xml;

namespace SignalRHub
{
    public class Global : System.Web.HttpApplication
    {
        public static List<ClsFareMeter> listofMeter = null;
        public static string DefaultCurrencySign = "£";
        public static string googleKey = string.Empty;
        public static string Region = "";
        public static List<MeterTarrif> listMeterTariff = null;
        public static List<SMSTag> listofSMSTags = null;
        private static CallerIdVOIP_Configuration objAsterik = null;
        private ManagerConnection manager = null;
        private string physicalPath = AppContext.BaseDirectory;
        private enum GatewayType { HypermediaSMSGateway = 1, DinstarSMSGateway = 2 }
        private static GatewayType SelectedGateway = GatewayType.DinstarSMSGateway;
        private string DefaultClientId { get; set; }
        private DinstarSettings DSSMS_Settings = null;
        private HypermediaSettings HMSMS_Settings = null;

        public static HMSMS.SmsGateway HMSMS_smsgateway = null;
        public static DSSMS.SmsGateway DSSMS_smsgateway = null;


        private static System.Timers.Timer callerIDTimer = null;

        public static string enableEmaiLReceipt = "0";
        public static string centerPoint = "";
        public static string MAPBOXKEY = "";
        public static string HEREKEY = "";
        public static string smsInbox = "0";
        public static string enableRingBack = "0";
        public static string enableClearJobText = "0";
        public static string enableAccountCharges = "0";
        public static string enableCallOffice = "0";
        public static string enableReceiptOnCompleteJob = "0";
        public static string customerOfficeNumber = "";
        public static string driverPayLiveDetails = "0";
        public static string EnableBidOnPlots = "0";
        public static string DriverPay = "0";
        public static string NoPickupRestrictionMins = "0";
        public static string CallerId_EnableHotDesk = "0";
        public  int CallerID_FromExt = 200;
        public  int CallerID_TillExt = 250;
        public static string enableChangePlotUpdateDestination = "0";

        public static string EnableWaitingAfterArrive = "0";

        public static string AutoSTC = "0";

        public static AutoDispatchSetting AutoDispatchSetting = null;
        public static List<clsSTCReminder> listofSTCReminder = new List<clsSTCReminder>();

        public static string EnableViaAction = "0";
        public static string EnablaDriverDocuments = "";

        public static void RemoveJobFromBidList(long jobId)
        {

            try
            {
                ////
                listofDrvBidding.RemoveAll(c => c.JobId == jobId);


            }
            catch
            {

            }
        }

        public static void InitializeSMSTags()
        {
            ////  ////
            if (listofSMSTags == null)
                listofSMSTags = General.GetQueryable<SMSTag>(null).ToList();
            //
        }

        private HubProcessor Instance
        {
            get { return HubProcessor.Instance; }
        }

        public static void AddSTCReminder(long jobId, int driverId)
        {

            try
            {
                if (AutoSTC == "1")
                {
                    Global.listofSTCReminder.Add(new clsSTCReminder { JobId = jobId, DriverId = driverId });

                    //try
                    //{
                    //    File.AppendAllText(AppContext.BaseDirectory + "\\AddSTCReminder.txt", DateTime.Now.ToStr() + ": jobid:" + jobId  +",driverid:"+driverId+ Environment.NewLine);
                    //}
                    //catch
                    //{


                    //}

                }
                //else
                //{
                //    try
                //    {
                //        File.AppendAllText(AppContext.BaseDirectory + "\\AddSTCReminderelse.txt", DateTime.Now.ToStr() + ": jobid:" + jobId + ",driverid:" + driverId + Environment.NewLine);
                //    }
                //    catch
                //    {


                //    }
                //}
            }
            catch
            {

            }

        }

        public static void RemoveSTCReminder(long jobId, int driverId)
        {

            try
            {

                Global.listofSTCReminder.RemoveAll(c => c.JobId == jobId && c.DriverId == driverId);
            }
            catch
            {

            }

        }


        public void RecordDisplay(string line, string name, string phone, string stn, string calledNumber, string uniqueId = null)
        {
            try
            {
                if (phone.StartsWith("9"))
                    phone = phone.Substring(phone.Length > 1 ? 1 : phone.Length);

                if (phone.StartsWith("00"))
                {
                    phone = phone.Substring(phone.Length > 1 ? 1 : phone.Length);
                }

                if (phone.Length < 8 || line.ToStr().Trim() == "<unknown>")
                    return;

                if (line.ToStr().Trim().Length > 6)
                {
                    try
                    {
                        //log
                        //     //Calllog.InfoFormat("XCaller : {0} , Status : {1} , Ext :{2}  ,Time : {3}", phone.ToStr(), "Answer", line.ToStr().Trim(), string.Format("{0:HH:mm}", DateTime.Now));
                    }
                    catch
                    {

                    }
                //    //
                    return;
                }

                //AppVars.openedPhoneNo = phone;
                string BlackListReason = string.Empty;

                // Console.WriteLine(string.Format("Caller No : {0} ", phone));
                // New

                Customer objCustomer = null;
                try
                {
                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        db.CommandTimeout = 4;
                        //
                        objCustomer = db.Customers.Where(x => x.TelephoneNo == phone || x.MobileNo == phone).FirstOrDefault();
                    }
                }
                catch (Exception ex)
                {
                    //log
                    //Errorlog.Error(ex.Message, ex);
                }

                if (objCustomer != null)
                {
                    if (objCustomer.Name.ToString().ToLower().StartsWith("driver "))
                        return;
                    name = objCustomer.Name.ToString();

                    // If Customer is Black Listed
                    if (Convert.ToBoolean(objCustomer.BlackList))
                    {
                        BlackListReason = objCustomer.BlackListResion.ToString().Trim();
                    }
                }

                DateTime callDate = DateTime.Now;

                try
                {
                    //log
                    //   //Calllog.InfoFormat("Caller : {0} , Status : {1} , Ext :{2}  ,Time : {3}", phone.ToStr(), "Answer", line.ToStr().Trim(), string.Format("{0:HH:mm}", callDate));
                }
                catch
                {

                }

                // //BroadCaster
                var msg = "**cti_incomingcall>>" + phone.ToStr() + ">>" + line.ToStr().Trim() + ">>answer>>" + "ANS" + ">>" + "vpn" + ">>" + calledNumber + ">>" + uniqueId;

                //send message to all desktop users
                //List<string> listOfConnections = new List<string>();
                //listOfConnections = Instance.ReturnDesktopConnections();
                //Instance.Clients.Clients(listOfConnections).cMessageToDesktop(msg);


                General.BroadCastMessage(msg);

                //
                //new BroadCaster().BroadCastToAll("**cti_incomingcall>>" + phone.ToStr() + ">>" + line.ToStr().Trim() + ">>answer>>" + "ANS" + ">>" + "vpn" + ">>" + calledNumber + ">>" + uniqueId);
                //
                //if (true) // (EnabledMissedCallLogs)
                //{//
                UpdateLog(name, phone, callDate, uniqueId, line, line, "", calledNumber);
                //}
                //else
                //{
                //   CreateLog(name, phone, callDate, uniqueId, line, calledNumber);
                //}
                //

                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\log_calleridanswered.txt", DateTime.Now.ToStr() + ": " + phone.ToStr()  +",ext:"+ line.ToStr().Trim()+ Environment.NewLine);
                }
                catch
                {
                    //
                }
            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\exception_CallerIDRecordDisplay.txt", DateTime.Now.ToStr() + ": " + ex.Message + "|" + ex.InnerException.StackTrace + "|" + ex.InnerException.Message + Environment.NewLine);
                }
                catch
                {


                }

            }
        }



     



        public static string UniqueCallID = "";



        public void CreateLog(string name, string phoneNumber, DateTime date, string duration, string line, string calledNumber)
        {
            try
            {
                using (TaxiDataContext db = new TaxiDataContext())
                {
                    db.CommandTimeout = 5;
                    db.stp_AddCallLog(name, phoneNumber, date, duration, line, 1, calledNumber);
                }
            }
            catch (Exception ex)
            {
                //  // File.AppendAllText(AppContext.BaseDirectory + "\\exception_CallerIDCreateLog.txt", DateTime.Now.ToStr() + ": " + ex.Message + "|" + ex.InnerException.StackTrace + "|" + ex.InnerException.Message + Environment.NewLine);
            }
        }

        public void UpdateLog(string name, string phoneNumber, DateTime date, string duration, string line, string stn, string callType, string calledNumber)
        {

            using (TaxiDataContext db = new TaxiDataContext())
            {
                try
                {
                    db.CommandTimeout = 5;
                    //
                    var obj = db.GetTable<CallHistory>().Where(c => c.PhoneNumber == phoneNumber).OrderByDescending(c => c.Id).FirstOrDefault();

                    if (obj != null)
                    {
                        if (!string.IsNullOrEmpty(line.ToStr()))
                        {
                            obj.Line = line.ToStr();
                        }
                        //
                        obj.Line = line;
                        //
                        if (!string.IsNullOrEmpty(stn.ToStr()))
                        {
                            obj.STN = stn.ToStr();
                        }

                        obj.CallDuration = duration;
                        obj.CalledToNumber = calledNumber;

                        obj.IsAccepted = false;
                        obj.AnsweredDateTime = DateTime.Now;




                        if (name.ToStr().Trim().Length > 0)
                        {
                            obj.Name = name;
                        }

                        db.SubmitChanges();
                    }

                    try
                    {
                        File.AppendAllText(AppContext.BaseDirectory + "\\CallerIDUpdateLog.txt", DateTime.Now.ToStr() + ": " + phoneNumber + "|" + line + Environment.NewLine);
                    }
                    catch
                    {

                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        File.AppendAllText(AppContext.BaseDirectory + "\\exception_CallerIDUpdateLog.txt", DateTime.Now.ToStr() + ": " + ex.Message + "|" + ex.InnerException.StackTrace + "|" + ex.InnerException.Message + Environment.NewLine);
                    }
                    catch
                    {

                    }

                }
            }
        }


        private class DinstarSettings
        {
            public string ServerBaseURL { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
            public string DefaultClientId { get; set; }
            public bool CanReceiveSMS { get; set; }
            public int[] SendingMsgPort { get; set; }
            public int[] ReceivingMsgPort { get; set; }
        }
        private class HypermediaSettings
        {
            public string ServerIPAddress { get; set; }
            public int Port { get; set; }
            public string Password { get; set; }
            public string DefaultClientId { get; set; }
            public bool CanReceiveSMS { get; set; }
        }


        //private void General.BroadCastMessage(string message)
        //{
        //    try
        //    {
        //        //send message to all desktop users
        //        List<string> listOfConnections = new List<string>();
        //        listOfConnections = Instance.ReturnDesktopConnections();
        //        Instance.Clients.Clients(listOfConnections).cMessageToDesktop(message);
        //    }
        //    catch
        //    {

        //    }
        //}

        protected void Application_Start(object sender, EventArgs e)
        {
            GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(10);
            initializeSettings();


            try
            {
                // ////
                GlobalHost.Configuration.KeepAlive = TimeSpan.FromSeconds(3);
                //
                //   // ////////
                File.AppendAllText(physicalPath + "\\log_applicationstart.txt", DateTime.Now.ToStr() + " Defaultclientid:" + DefaultClientId.ToStr() + ",ringback:" + Global.enableRingBack + ",accountcharges:" + Global.enableAccountCharges + ",cleartext:" + Global.enableClearJobText + ",calloffice:" + Global.enableCallOffice + Environment.NewLine);



                ReloadMeterList();

                AreaRegistration.RegisterAllAreas();
                GlobalConfiguration.Configure(RouteConfig.Register);
                FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
                RouteConfig.RegisterRoutes(RouteTable.Routes);

                IsSendingSMS = false;
                setTimer();
            }
            catch(Exception ex)
            {
                File.AppendAllText(physicalPath + "\\applicationstart_exception.txt", DateTime.Now.ToStr() + " Defaultclientid:" + DefaultClientId.ToStr()+ ",exception:"+ex.Message+ Environment.NewLine);

            }
        }


        private void setTimer()
        {
            if (Instance.smsTimer == null)
            {
                //Create a timer with a 5 seconds interval
                Instance.smsTimer = new System.Timers.Timer(5000);

                //Hook up the Elapsed event for the timer 
                Instance.smsTimer.Elapsed += sendSMS;

                Instance.smsTimer.AutoReset = true;
                Instance.smsTimer.Enabled = true;

                Instance.listofSMS = new List<string>();
            }


            if (Instance.autoDispatchTimer == null)
            {
                //Create a timer with a 5 seconds interval
                Instance.autoDispatchTimer = new System.Timers.Timer(5000);

                //Hook up the Elapsed event for the timer 
                Instance.autoDispatchTimer.Elapsed += AutoDispatchActivity;

                Instance.autoDispatchTimer.AutoReset = true;
                Instance.autoDispatchTimer.Enabled = true;


            }

        }


        public static bool IsSendingSMS = false;

        private void sendSMS(Object source, System.Timers.ElapsedEventArgs e)
        {
            try
            {

                if (IsSendingSMS)
                {
                    try
                    {
                        File.AppendAllText(physicalPath + "\\log_sendingsmsinprogress.txt", DateTime.Now.ToStr() + Environment.NewLine);
                    }
                    catch
                    {
                    }
                }

                if (Instance.listofSMS != null && Instance.listofSMS.Count > 0 && IsSendingSMS == false)
                {
                    IsSendingSMS = true;
                    //
                    //  Instance.listofSMS
                    //
                    string itemSMS = Instance.listofSMS.FirstOrDefault();


                    string[] values = itemSMS.Split(new char[] { '=' });

                    string val = itemSMS.Replace(values[0] + "=" + values[1] + "=", "");

                    //  string val = Instance.listofSMS[Instance.listofSMS.Count - 1].Replace(values[0] + "=" + values[1] + "=", "");

                    string mmsg = val;

                    if (mmsg.ToStr().Contains("^"))
                        mmsg = mmsg.Replace("^", "=");


                    //Send sms to all PDAs



                    //
                    try
                    {
                        Global.SendSMS(values[1].Trim(), mmsg);

                        File.AppendAllText(physicalPath + "\\smslogs.txt", DateTime.Now.ToStr() + " ::: " + mmsg + ", number : " + values[1] + Environment.NewLine);


                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            File.AppendAllText(physicalPath + "\\log_catchremovesms.txt", DateTime.Now.ToStr() + " ::: " + ex.Message + Environment.NewLine);
                        }
                        catch
                        {
                            //

                        }

                    }


                    try
                    {

                        Instance.listofSMS.Remove(itemSMS);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            File.AppendAllText(physicalPath + "\\log_catchremovesms2.txt", DateTime.Now.ToStr() + " ::: " + ex.Message + Environment.NewLine);


                            if (ex.Message.ToStr().Contains("Count must be positive and count"))
                            {
                                try
                                {
                                    Instance.listofSMS.RemoveAll(c => c == null);

                                    Instance.listofSMS = new List<string>();
                                }
                                catch (Exception ex2)
                                {
                                    try
                                    {
                                        Instance.listofSMS = new List<string>();

                                        File.AppendAllText(physicalPath + "\\log_catchremovesmsexception.txt", DateTime.Now.ToStr() + " ::: " + ex2.Message + Environment.NewLine);


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
                    //

                    //  Instance.listofSMS.Remove(itemSMS);
                    IsSendingSMS = false;
                }
                //
                try
                {

                    Instance.listofJobs.RemoveAll(c => DateTime.Now.Subtract(c.MessageDateTime).TotalSeconds > 300);


                }
                catch
                {
                    try
                    {
                        Instance.listofJobs.RemoveAll(c => c == null);


                    }
                    catch
                    {
                        try
                        {
                            Instance.listofJobs = new List<clsPDA>();


                        }
                        catch
                        {




                        }



                    }

                }



            }
            catch (Exception ex)
            {
                IsSendingSMS = false;
                try
                {
                    File.AppendAllText(physicalPath + "\\exception_SendSMS.txt", DateTime.Now.ToStr() + ": " + ex.Message + "|" + ex.InnerException.DefaultIfEmpty().StackTrace + "|" + ex.InnerException.DefaultIfEmpty().Message + Environment.NewLine);



                    try
                    {
                        Instance.listofSMS.RemoveAll(c => c == null);


                    }
                    catch
                    {
                        try
                        {
                            Instance.listofSMS = new List<string>();


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
        }


        public static void ReloadMeterList()
        {


            try
            {
                //
                if (listofMeter != null)
                    listofMeter.Clear();
                //

                var list = General.GetQueryable<Gen_SysPolicy_FareMeterSetting>(c => c.SysPolicyId != null).ToList();


                listofMeter = new List<ClsFareMeter>();

                foreach (var item in list)
                {

                    listofMeter.Add(new ClsFareMeter
                    {
                        //
                        VehicleId = item.VehicleTypeId,
                        VehicleType = item.Fleet_VehicleType.VehicleType,
                        VehicleTypeId = item.VehicleTypeId,
                        AccWaitingChargesPerMin = item.AccWaitingChargesPerMin,
                        AutoStartWaiting = item.AutoStartWaiting,
                        AutoStartWaitingBelowSpeed = item.AutoStartWaitingBelowSpeed,
                        AutoStartWaitingBelowSpeedSeconds = item.AutoStartWaitingBelowSpeedSeconds,
                        AutoStopWaitingOnSpeed = item.AutoStopWaitingOnSpeed,
                        DrvWaitingChargesPerMin = item.DrvWaitingChargesPerMin,
                        HasMeter = item.HasMeter,
                        NoofPassengers = item.Fleet_VehicleType.NoofPassengers

                    });


                }


                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\meterlist.txt", DateTime.Now.ToStr() + ": Listcount" + Global.listofMeter.Count + Environment.NewLine);
                }
                catch
                {


                }

            }
            catch (Exception ex)
            { }



        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        public static void LoadDataList(bool forceRefresh = false)
        {
            try
            {
                if (HubProcessor.Instance.objPolicy == null || forceRefresh)
                    HubProcessor.Instance.objPolicy = General.GetObject<Gen_SysPolicy_Configuration>(c => c.SysPolicyId == 1);

                if (HubProcessor.Instance.listofPolyVertices == null || forceRefresh)
                {
                    HubProcessor.Instance.listofPolyVertices = General.GetQueryable<Gen_Zone_PolyVertice>(c => c.ZoneId != 0).ToList();
                }

                if (HubProcessor.Instance.listOfZone == null || forceRefresh)
                {
                    HubProcessor.Instance.listOfZone = General.GetQueryable<Gen_Zone>(c => c.ShapeCategory != null)
                                .OrderBy(c => c.OrderNo).AsEnumerable().Select(args => new clsZones
                                {

                                    Id = args.Id,
                                    Area = args.ZoneName,
                                    PostCode = args.PostCode,
                                    ZoneType = "Fixed",
                                    IsBaseZone = args.IsBase,
                                    MinLat = args.MinLatitude,
                                    MaxLat = args.MaxLatitude,
                                    MinLng = args.MinLongitude,
                                    MaxLng = args.MaxLongitude,
                                    PlotLimit = args.PlotLimit,
                                    PlotEntranceMessage = args.PlotEntranceMessage,
                                    PlotOverLimitMessage = args.PlotLimitExceedMessage,
                                    shapeType = args.ShapeType,
                                    radius = args.ShapeType == "circle" ? Convert.ToDouble(args.Gen_Zone_PolyVertices[0].Diameter) : 0,
                                    DisableRank = args.DisableDriverRank,
                                    PlotKind = args.PlotKind,
                                    OrderNo = args.OrderNo
                                }).ToList();
                }


                if (Global.listofMeter == null || forceRefresh)
                {

                    ReloadMeterList();
                    //
                }

                //

                try
                {
                    if (forceRefresh)
                        File.AppendAllText(AppContext.BaseDirectory + "\\loaddatalist.txt", DateTime.Now.ToStr() + "," + "forcerefresh :" + forceRefresh.ToStr() + Environment.NewLine);

                }
                catch
                {

                }
            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\loaddatalist_exception.txt", DateTime.Now.ToStr() + "," + "forcerefresh :" + forceRefresh.ToStr() + " , exception : " + ex.Message.ToStr() + Environment.NewLine);
                    //

                }
                catch
                {

                }

            }

        }



        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }




        public static int? CallerIdType = null;
        private void initializeSettings()
        {
            try
            {
                if (objAsterik == null)
                {
                    objAsterik = General.GetObject<CallerIdVOIP_Configuration>(c => c.Port != null && c.Port != "");
                    //
                    if (objAsterik != null)
                    {
                        if (manager == null)
                        {
                            manager = new ManagerConnection(objAsterik.Host.ToStr(), objAsterik.Port.ToInt(), objAsterik.UserName.ToStr(), objAsterik.Password.ToStr());

                            if (CallerIdType == null)
                            {
                                using (TaxiDataContext db = new TaxiDataContext())
                                {

                                    CallerIdType = db.CallerIdType_Configurations.Select(c => c.VOIPCLIType).FirstOrDefault().DefaultIfEmpty();
                                }
                            }


                            try
                            {

                                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["CallerId_EnableHotDesk"]))
                                {
                                    CallerId_EnableHotDesk = ConfigurationManager.AppSettings["CallerId_EnableHotDesk"].ToStr();

                                }


                                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["CallerID_FromExt"]))
                                {
                                    CallerID_FromExt = ConfigurationManager.AppSettings["CallerID_FromExt"].ToInt();

                                }


                                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["CallerID_TillExt"]))
                                {
                                    CallerID_TillExt = ConfigurationManager.AppSettings["CallerID_TillExt"].ToInt();

                                }
                            }
                            catch
                            {

                            }


                            if (CallerIdType == 2)
                            {
                                if(Global.CallerId_EnableHotDesk=="1")
                                      manager.NewState += new NewStateEventHandler(Manager_NewStateHotDesk);
                                else
                                    manager.NewState += new NewStateEventHandler(Manager_NewState);
                            }
                            else if (CallerIdType == 4)
                            {
                                manager.NewState += new NewStateEventHandler(ManagerEmerald_NewState);
                                manager.NewChannel += ManagerEmerald_NewChannel;
                            }

                            try
                            {
                                manager.Login();

                                try
                                {
                                    File.AppendAllText(physicalPath + "\\log_calleridstart.txt", DateTime.Now.ToStr() + Environment.NewLine);
                                }
                                catch
                                {
                                    //
                                }
                                // // ////  ////
                            }
                            catch (Exception ex)
                            {
                                manager.Logoff();
                                try
                                {
                                    File.AppendAllText(physicalPath + "\\log_exceptioncalleridstart.txt", DateTime.Now.ToStr() + "," + ex.Message + Environment.NewLine);
                                }
                                catch
                                {

                                }
                                //  //ShowNotification("VOIP", "Asterik Voip Connection Failed : " + ex.Message, null);
                            }
                        }

                        try
                        {
                            if (callerIDTimer == null)
                            {
                                //Create a timer with a 5 seconds interval
                                callerIDTimer = new System.Timers.Timer(60000);

                                ////Hook up the Elapsed event for the timer 
                                callerIDTimer.Elapsed += callerID_Tick;

                                callerIDTimer.AutoReset = true;
                                callerIDTimer.Enabled = true;


                            }
                        }
                        catch
                        {


                        }

                    }
                }


                if (Instance.objPolicy == null)
                    Instance.objPolicy = General.GetObject<Gen_SysPolicy_Configuration>(c => c.SysPolicyId == 1);

                //
                // DefaultClientId = new TaxiDataContext().Gen_SysPolicy_Configurations.Where(m => m.SysPolicyId == 1).Select(c => c.DefaultClientId).FirstOrDefault();

                DefaultClientId = Instance.objPolicy.DefaultClientId.ToStr();
                Int16 _selectgateway = 0;

                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["SelectedGateway"]))
                {
                    if (Int16.TryParse(ConfigurationManager.AppSettings["SelectedGateway"], out _selectgateway) == true)
                    {
                        if (_selectgateway == 1)
                        {
                            SelectedGateway = GatewayType.HypermediaSMSGateway;
                        }
                        else if (_selectgateway == 2)
                        {
                            SelectedGateway = GatewayType.DinstarSMSGateway;
                        }
                    }
                }

                if (SelectedGateway == GatewayType.HypermediaSMSGateway)
                {
                    HMSMS_Settings = new HypermediaSettings()
                    {
                        ServerIPAddress = ConfigurationManager.AppSettings["HM_ServerIPAddress"],
                        Port = Convert.ToInt32(ConfigurationManager.AppSettings["HM_Port"]),
                        Password = ConfigurationManager.AppSettings["HM_Password"],
                        CanReceiveSMS = true,
                        DefaultClientId = DefaultClientId
                    };

                    if (ConfigurationManager.AppSettings["CanReceiveSMS"] == "0" || Convert.ToString(ConfigurationManager.AppSettings["CanReceiveSMS"]).ToLower() == "no")
                    {
                        HMSMS_Settings.CanReceiveSMS = false;
                    }
                    else if (ConfigurationManager.AppSettings["CanReceiveSMS"] == "1" || Convert.ToString(ConfigurationManager.AppSettings["CanReceiveSMS"]).ToLower() == "yes")
                    {
                        HMSMS_Settings.CanReceiveSMS = true;
                    }

                    System.Net.IPHostEntry IPHostInfo = null;

                    try
                    {
                        IPHostInfo = System.Net.Dns.GetHostEntry(System.Net.IPAddress.Parse(HMSMS_Settings.ServerIPAddress));
                    }
                    catch
                    {
                        IPHostInfo = System.Net.Dns.GetHostEntry(HMSMS_Settings.ServerIPAddress);
                    }

                    HMSMS_smsgateway = new HMSMS.SmsGateway(HMSMS_Settings.Port, IPHostInfo, HMSMS_Settings.DefaultClientId, HMSMS_Settings.Password, HMSMS_Settings.CanReceiveSMS);

                    // HMSMS_smsgateway.OnPostMessageOutResultUpdate += SMSGatway_OnPostMessageOutResultUpdate;
                    // HMSMS_smsgateway.OnPostEventLog += Smsgateway_OnPostEventLog;
                }
                else if (SelectedGateway == GatewayType.DinstarSMSGateway)
                {

                    // , new int[] { 0 }, new int[] { 0 });

                    DSSMS_Settings = new DinstarSettings()
                    {
                        ServerBaseURL = ConfigurationManager.AppSettings["DS_ServerBaseURL"],
                        UserName = ConfigurationManager.AppSettings["DS_UserName"],
                        Password = ConfigurationManager.AppSettings["DS_Password"],
                        CanReceiveSMS = true,
                        DefaultClientId = DefaultClientId,
                        SendingMsgPort = new int[] { 0 },
                        ReceivingMsgPort = new int[] { 0 }
                    };

                    if (ConfigurationManager.AppSettings["CanReceiveSMS"] == "0" || Convert.ToString(ConfigurationManager.AppSettings["CanReceiveSMS"]).ToLower() == "no")
                    {
                        DSSMS_Settings.CanReceiveSMS = false;
                    }
                    else if (ConfigurationManager.AppSettings["CanReceiveSMS"] == "1" || Convert.ToString(ConfigurationManager.AppSettings["CanReceiveSMS"]).ToLower() == "yes")
                    {
                        DSSMS_Settings.CanReceiveSMS = true;
                    }

                    if (ConfigurationManager.AppSettings["DS_SendingMsgPort"] != null && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["DS_SendingMsgPort"]))
                    {
                        DSSMS_Settings.SendingMsgPort = ConfigurationManager.AppSettings["DS_SendingMsgPort"].Split(',').Select(m => Convert.ToInt32(m)).ToArray();
                    }

                    if (ConfigurationManager.AppSettings["DS_ReceivingMsgPort"] != null && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["DS_ReceivingMsgPort"]))
                    {
                        DSSMS_Settings.ReceivingMsgPort = ConfigurationManager.AppSettings["DS_ReceivingMsgPort"].Split(',').Select(m => Convert.ToInt32(m)).ToArray();
                    }

                    Gen_SysPolicy_SMSConfiguration objSMSConfig = null;

                    try
                    {
                        using (TaxiDataContext db = new TaxiDataContext())
                        {

                            objSMSConfig = db.Gen_SysPolicy_SMSConfigurations.FirstOrDefault();



                        }
                    }
                    catch
                    {

                    }


                    if (objSMSConfig != null && objSMSConfig.ClickSMSApiKey.ToStr().Trim().StartsWith("http") && objSMSConfig.ClickSMSUserName.ToStr().Trim().Length > 0
                          && objSMSConfig.ClickSMSPassword.ToStr().Trim().Length > 0 && objSMSConfig.ClickSMSSenderName.ToStr().Trim().Length > 0)
                    {


                        DSSMS_Settings = new DinstarSettings()
                        {
                            ServerBaseURL = objSMSConfig.ClickSMSApiKey.ToStr().Trim(),
                            UserName = objSMSConfig.ClickSMSUserName.ToStr().Trim(),
                            Password = objSMSConfig.ClickSMSPassword.ToStr().Trim(),
                            CanReceiveSMS = true,
                            DefaultClientId = DefaultClientId,
                            SendingMsgPort = new int[] { 0 },
                            ReceivingMsgPort = new int[] { 0 }
                        };



                        DSSMS_Settings.SendingMsgPort = objSMSConfig.ClickSMSSenderName.ToStr().Trim().Split(',').Select(m => Convert.ToInt32(m)).ToArray();
                        DSSMS_Settings.ReceivingMsgPort = objSMSConfig.ModemSMSPortName.ToStr().Trim().Split(',').Select(m => Convert.ToInt32(m)).ToArray();

                        try
                        {
                            File.AppendAllText(physicalPath + "\\SMSServiceStart.txt", DateTime.Now.ToStr() + " : Start by Db Credentials" + Environment.NewLine);
                        }
                        catch
                        {
                      //  //    //
                        }
                    }
                    else
                    {
                        try
                        {
                            File.AppendAllText(physicalPath + "\\SMSServiceStart.txt", DateTime.Now.ToStr() + " : Start by WebConfig Credentials" + Environment.NewLine);
                        }
                        catch
                        {
                            //
                        }


                    }


                    try
                    {
                        if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["SMSInbox"]))
                        {
                            smsInbox = ConfigurationManager.AppSettings["SMSInbox"].ToStr();

                        }
                    }
                    catch
                    {

                    }

                    DSSMS_smsgateway = new DSSMS.SmsGateway(DSSMS_Settings.ServerBaseURL, DSSMS_Settings.UserName, DSSMS_Settings.Password, DSSMS_Settings.CanReceiveSMS, DSSMS_Settings.SendingMsgPort, DSSMS_Settings.ReceivingMsgPort);

                    if (smsInbox == "1")
                        DSSMS_smsgateway.OnPostMessageIn += DSSMS_smsgateway_OnPostMessageIn;
                }

                //

                // InitializeDefaultSettings

                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["enableRingBack"]))
                {
                    enableRingBack = ConfigurationManager.AppSettings["enableRingBack"].ToStr();

                }

                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["enableAccountCharges"]))
                {
                    enableAccountCharges = ConfigurationManager.AppSettings["enableAccountCharges"].ToStr();

                }

                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["enableClearJobText"]))
                {
                    enableClearJobText = ConfigurationManager.AppSettings["enableClearJobText"].ToStr();

                }

                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["enableCallOffice"]))
                {
                    enableCallOffice = ConfigurationManager.AppSettings["enableCallOffice"].ToStr();

                }

                //
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["customOfficeNumber"]))
                {
                    customerOfficeNumber = ConfigurationManager.AppSettings["customOfficeNumber"].ToStr();

                }

                //
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["driverPayLiveDetails"]))
                {
                    driverPayLiveDetails = ConfigurationManager.AppSettings["driverPayLiveDetails"].ToStr();

                }

                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["EnableBidOnPlots"]))
                {
                    EnableBidOnPlots = ConfigurationManager.AppSettings["EnableBidOnPlots"].ToStr();

                }


                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DriverPay"]))
                {
                    DriverPay = ConfigurationManager.AppSettings["DriverPay"].ToStr();

                }



                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["NoPickupRestrictionMins"]))
                {
                    NoPickupRestrictionMins = ConfigurationManager.AppSettings["NoPickupRestrictionMins"].ToStr();

                }

                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["enableChangePlotUpdateDestination"]))
                {
                    enableChangePlotUpdateDestination = ConfigurationManager.AppSettings["enableChangePlotUpdateDestination"].ToStr();

                }


                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["EnableWaitingAfterArrive"]))
                {
                    EnableWaitingAfterArrive = ConfigurationManager.AppSettings["EnableWaitingAfterArrive"].ToStr();

                }



                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["AutoSTC"]))
                {
                    AutoSTC = ConfigurationManager.AppSettings["AutoSTC"].ToStr();

                }

                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["enableReceiptOnCompleteJob"]))
                {
                    enableReceiptOnCompleteJob = ConfigurationManager.AppSettings["enableReceiptOnCompleteJob"].ToStr();

                }


                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["enableEmaiLReceipt"]))
                {
                    enableEmaiLReceipt = ConfigurationManager.AppSettings["enableEmaiLReceipt"].ToStr();

                }


                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["EnablaDriverDocuments"]))
                {
                    EnablaDriverDocuments = ConfigurationManager.AppSettings["EnablaDriverDocuments"].ToStr();

                }



                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["EnableViaAction"]))
                {
                    EnableViaAction = ConfigurationManager.AppSettings["EnableViaAction"].ToStr();

                }


            }
            catch (Exception ex)
            {
                File.AppendAllText(physicalPath + "\\exception_globalasxx.txt", DateTime.Now.ToStr() + ex.Message + Environment.NewLine);
            }
        }

        public static void DSSMS_smsgateway_OnPostMessageIn(object sender, SMSGateway.MessageInInfo e)
        {
            try
            {
                File.AppendAllText(AppContext.BaseDirectory + "\\smsinbox.txt", DateTime.Now.ToStr() + ": From:" + e.From.ToStr() + ",Message:" + e.Message.ToStr() + Environment.NewLine);


                try
                {








                    string phone = e.From.Replace("+44", "0").Replace("+92", "0");
                    string text = e.Message.ToStr().Trim();



                    string name = string.Empty;
                    using (TaxiDataContext db = new TaxiDataContext())
                    {


                        var objCustomer = db.Customers.Where(c => c.TelephoneNo == phone || c.MobileNo == phone)
                              .Select(args => new { args.Id, args.Name, args.MobileNo }).OrderByDescending(C => C.Id).FirstOrDefault();


                        if (objCustomer != null)
                        {

                            name = objCustomer.Name;

                            (new TaxiDataContext()).stp_SaveInboxMessage(objCustomer.Id, 0, objCustomer.Name + "(" + objCustomer.MobileNo.ToStr() + ")"
                                , "", text, "Inbox", "Customer", DateTime.Now);
                        }
                        else
                        {

                            try
                            {
                                ClsDriverInfo drv = db.ExecuteQuery<ClsDriverInfo>("select Id,DriverNo,DriverName,MobileNo from fleet_driver where mobileNo='" + phone + "'").FirstOrDefault();

                                // Fleet_Driver drv = db.Fleet_Drivers.FirstOrDefault(c => c.MobileNo == phone);
                                if (drv != null)
                                {
                                    name = drv.DriverNo + " - " + drv.DriverName;

                                    (new TaxiDataContext()).stp_SaveInboxMessage(drv.Id, 0, (drv.DriverNo.ToStr() + "(" + drv.MobileNo.ToStr() + ")")
                                             , "", text.ToStr(), "Inbox", "Customer", DateTime.Now);
                                }
                                else
                                {
                                    name = phone.ToStr();
                                    (new TaxiDataContext()).stp_SaveInboxMessage(null, 0, "(" + phone.ToStr() + ")", "", text.ToStr(), "Inbox", "Unknown", DateTime.Now);

                                }
                                File.AppendAllText(AppContext.BaseDirectory + "\\DriverSMSReceived.txt", string.Format("{0:dd/MM/yy hh:mm:ss}", DateTime.Now) + " : number" + phone + ",message:" + text + Environment.NewLine);

                                //
                            }
                            catch (Exception ex)
                            {
                                try
                                {
                                    File.AppendAllText(AppContext.BaseDirectory + "\\DriverSMSReceivedException.txt", string.Format("{0:dd/MM/yy hh:mm:ss}", DateTime.Now) + " : number" + phone + ",message:" + text + ",exception:" + ex.Message + Environment.NewLine);
                                }
                                catch
                                {

                                }
                            }
                        }

                    }


                    List<string> listOfConnections = new List<string>();
                    listOfConnections = HubProcessor.Instance.ReturnDesktopConnections();
                    HubProcessor.Instance.Clients.Clients(listOfConnections).cMessageToDesktop("**internalmessage>>received text>>" + name + ">>" + "<html><b><span><color=Blue>" + text + "</span></b></html>");













                }
                catch (Exception ex)
                {
                    try
                    {
                        File.AppendAllText(AppContext.BaseDirectory + "\\smsinbox_exception.txt", string.Format("{0:dd/MM/yy hh:mm:ss}", DateTime.Now) + " : exception" + ex.Message + ",number:" + e.From.ToStr() + Environment.NewLine);
                    }
                    catch
                    {

                    }

                }

            }
            catch
            {
                //
            }
        }




        private void Manager_NewStateHotDesk(object sender, NewStateEvent e)
        {
            try
            {



                //try
                //{
                //    File.AppendAllText(AppContext.BaseDirectory + "\\log_calleridEvents.txt", DateTime.Now.ToStr() + ": " + e.ToString() + Environment.NewLine);
                //}
                //catch
                //{
                //    //
                //}
                //
                string number = e.CallerIdNum;
                string desc = e.ChannelStateDesc;

                int Ext = 0;
                string Exten = string.Empty;
                if (desc.ToString().ToUpper() == "RING" && !string.IsNullOrEmpty(e.CallerIdNum) && e.CallerIdNum.ToStr().Length > 7)
                {
                    try
                    {


                      
                        string connectedLineNum = string.Empty;
                        string callerName = string.Empty;
                        try
                        {

                            number = e.CallerIdNum;
                            // connectedLineNum = e.AccountCode.Trim();
                            e.Attributes.TryGetValue("exten", out connectedLineNum);


                            if (connectedLineNum.IsNumeric())
                            {
                                if (connectedLineNum.StartsWith("44"))
                                {
                                    connectedLineNum = connectedLineNum.Remove(0, 2);

                                    connectedLineNum = connectedLineNum.Insert(0, "0");
                                }
                            }
                            else
                                connectedLineNum = string.Empty;

                        }
                        catch
                        {

                        }

                        if (!string.IsNullOrEmpty(number) && number.ToStr().Length > 7)
                        {
                            try
                            {
                                using (TaxiDataContext db = new TaxiDataContext())
                                {
                                    db.CommandTimeout = 4;
                                    callerName = db.stp_GetCallerInfo(number, "").FirstOrDefault().DefaultIfEmpty().Name;
                                }
                            }
                            catch
                            {
                            }

                            //


                            try
                            {
                                if (connectedLineNum.IsNumeric())
                                {
                                    if (connectedLineNum.StartsWith("44"))
                                    {
                                        connectedLineNum = connectedLineNum.Remove(0, 2);
                                        connectedLineNum = connectedLineNum.Insert(0, "0");
                                    }
                                }
                            }
                            catch
                            {
                            }

                            ////
                            CreateLog(callerName, number.ToStr(), DateTime.Now, "00:00:00", "", connectedLineNum);
                            

                        }
                    }
                    catch
                    {
                    }
                }
                if (desc.ToString().ToUpper() == "UP")
                {

                  
                  
                    string connectedLineNum = string.Empty;

                    try
                    {


                        connectedLineNum = e.AccountCode.Trim();



                        if (connectedLineNum.IsNumeric())
                        {
                            if (connectedLineNum.StartsWith("44"))
                            {
                                connectedLineNum = connectedLineNum.Remove(0, 2);

                                connectedLineNum = connectedLineNum.Insert(0, "0");
                            }
                        }
                        else
                            connectedLineNum = string.Empty;

                    }
                    catch
                    {

                    }

                    //
                    e.Attributes.TryGetValue("connectedlinenum", out number);
                    e.Attributes.TryGetValue("exten", out Exten);
                    string extension = e.CallerIdName.ToString().ToLower().Replace("ext", "").Trim();
                    bool IsValidExt = int.TryParse(extension, out Ext);
                    if (IsValidExt)
                    {
                        if (Exten != Ext.ToString() && Exten != "650")
                        {
                            return;
                        }




                    }
                    else
                    {

                        if (number.ToStr().Length < 6 && e.Channel.ToStr().Contains("Local/"))
                        {
                            try
                            {
                                number = e.CallerIdNum.ToStr();
                                extension = e.Channel.ToStr().Replace("Local/", "");
                                extension = extension.Remove(extension.IndexOf("@"));

                                if (extension.IsNumeric() && extension.ToInt() >= CallerID_FromExt && extension.ToInt() <= CallerID_TillExt)
                                {
                                    
                                    number = e.CallerIdNum.ToStr();


                                }
                                else
                                    number = "999";


                                if (extension == "<unknown>")
                                {
                                    number = "999";
                                }
                            }
                            catch
                            {


                            }
                        }

                        if (UniqueCallID == "" || UniqueCallID != e.UniqueId)
                        {
                            UniqueCallID = e.UniqueId;


                            if (extension.IsNumeric() && extension.ToInt() >= CallerID_FromExt && extension.ToInt() <= CallerID_TillExt)
                            {
                                //
                                string unId = "";
                                try
                                {
                                    e.Attributes.TryGetValue("linkedid", out unId);
                                }
                                catch
                                {
                                    //
                                }
                                RecordDisplay(extension, "", number, extension, connectedLineNum, unId);
                            }
                            else
                            {

                                if (Exten.ToStr().IsNumeric() && (Exten.ToInt() == 750 || Exten.ToInt() == 751))
                                {
                                    try
                                    {
                                        string newNumber = e.CallerIdNum.ToStr().Trim();

                                        try
                                        {
                                            File.AppendAllText(AppContext.BaseDirectory + "\\log_calleridIVRABOP.txt", DateTime.Now.ToStr() + ": " + "number:" + newNumber + ",exten:" + Exten.ToStr() + ",extension:" + extension.ToStr() + ",uniqueId:" + "" + Environment.NewLine);


                                        }
                                        catch
                                        {
                                            //
                                        }

                                        string name = string.Empty;
                                        Customer objCustomer = null;

                                        try
                                        {
                                            using (TaxiDataContext db = new TaxiDataContext())
                                            {
                                                db.CommandTimeout = 4;
                                                //
                                                objCustomer = db.Customers.Where(x => x.TelephoneNo == newNumber || x.MobileNo == newNumber).FirstOrDefault();
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }

                                        if (objCustomer != null)
                                        {
                                            if (objCustomer.Name.ToString().ToLower().StartsWith("driver "))
                                                return;
                                            name = objCustomer.Name.ToString();

                                            //
                                        }

                                        if (Exten.ToStr() == "751")
                                            Exten = "IVR";
                                        else
                                        if (Exten.ToStr() == "750")
                                            Exten = "ABOP";

                                        UpdateLog(name, newNumber, DateTime.Now, "00:00:00", Exten.ToStr(), Exten.ToStr(), "", connectedLineNum);
                                    }
                                    catch
                                    {

                                    }
                                }

                            }


                        }

                    }

                } // end up
            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\exception_callerid.txt", DateTime.Now.ToStr() + ": " + e.ToString() + Environment.NewLine);
                }
                catch
                {
                    //
                }

            }
        }
        private void Manager_NewState(object sender, NewStateEvent e)
        {
            try
            {



                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\log_calleridEvents.txt", DateTime.Now.ToStr() + ": " + e.ToString() + Environment.NewLine);
                }
                catch
                {
                    //
                }

                string number = e.CallerIdNum;
                string desc = e.ChannelStateDesc;

                int Ext = 0;
                string Exten = string.Empty;
                if (desc.ToString().ToUpper() == "RING" && !string.IsNullOrEmpty(e.CallerIdNum) && e.CallerIdNum.ToStr().Length > 7)
                {
                    try
                    {
                        //try
                        //{
                        //    File.AppendAllText(AppContext.BaseDirectory + "\\log_calleridring.txt", DateTime.Now.ToStr() + ": " + e.ToString() + Environment.NewLine);
                        //}
                        //catch
                        //{
                        //    //
                        //}

                        //

                        //  callerNumber = e.CallerIdNum;
                        //  callerName = e.CallerIdName;
                        //  uniqueId = e.UniqueId;
                        //  e.Attributes.TryGetValue("linkedid", out uniqueId);
                        string connectedLineNum = string.Empty;
                        string callerName = string.Empty;
                        try
                        {

                            number = e.CallerIdNum;
                            // connectedLineNum = e.AccountCode.Trim();
                            e.Attributes.TryGetValue("exten", out connectedLineNum);


                            if (connectedLineNum.IsNumeric())
                            {
                                if (connectedLineNum.StartsWith("44"))
                                {
                                    connectedLineNum = connectedLineNum.Remove(0, 2);

                                    connectedLineNum = connectedLineNum.Insert(0, "0");
                                }
                            }
                            else
                                connectedLineNum = string.Empty;

                        }
                        catch
                        {

                        }






                        if (!string.IsNullOrEmpty(number) && number.ToStr().Length > 7)
                        {
                            try
                            {
                                using (TaxiDataContext db = new TaxiDataContext())
                                {
                                    db.CommandTimeout = 4;
                                    callerName = db.stp_GetCallerInfo(number, "").FirstOrDefault().DefaultIfEmpty().Name;
                                }
                            }
                            catch
                            {
                            }

                            //
                            string item = string.Empty;

                            try
                            {

                               
                                if (!string.IsNullOrEmpty(callerName))
                                {
                                    item = callerName + " - " + number + "-" + string.Format("{0:HH:mm}", DateTime.Now);
                                }
                                else
                                {
                                    item = number + "-" + string.Format("{0:HH:mm}", DateTime.Now);
                                }

                                if (connectedLineNum.IsNumeric())
                                {
                                    if (connectedLineNum.StartsWith("44"))
                                    {
                                        connectedLineNum = connectedLineNum.Remove(0, 2);
                                        connectedLineNum = connectedLineNum.Insert(0, "0");
                                    }
                                }
                            }
                            catch
                            {
                            }

                            ////
                            ///
                            General.BroadCastMessage("**cti_remoteincomingcall>>" + number.ToStr() + ">>" + "XXX" + ">>ring>>" + item);
                            CreateLog(callerName, number.ToStr(), DateTime.Now, "00:00:00", "", connectedLineNum);
                            try
                            {
                                File.AppendAllText(AppContext.BaseDirectory + "\\log_calleridring2.txt", DateTime.Now.ToStr() + ": " + item + Environment.NewLine);
                            }
                            catch
                            {
                                //
                            }

                        }
                    }
                    catch
                    {
                    }
                }
                else if (desc.ToString().ToUpper() == "UP")
                {

                    //  Calllog.Info(e);

                    string connectedLineNum = string.Empty;

                    try
                    {
                        //

                        connectedLineNum = e.AccountCode.Trim();



                        if (connectedLineNum.IsNumeric())
                        {
                            if (connectedLineNum.StartsWith("44"))
                            {
                                connectedLineNum = connectedLineNum.Remove(0, 2);

                                connectedLineNum = connectedLineNum.Insert(0, "0");
                            }
                        }
                        else
                            connectedLineNum = string.Empty;

                    }
                    catch
                    {

                    }

                    //
                    number = e.Connectedlinenum.ToStr().Trim();
                    //   e.Attributes.TryGetValue("connectedlinenum", out number);
                    e.Attributes.TryGetValue("exten", out Exten);
                    string extension = e.CallerIdName.ToStr().ToLower().Replace("ext", "").Trim();
                    bool IsValidExt = int.TryParse(extension, out Ext);
                    //


                    //try
                    //{
                    //    File.AppendAllText(AppContext.BaseDirectory + "\\log_callerid.txt", DateTime.Now.ToStr() + ": " + e.ToString() + Environment.NewLine);

                    //    File.AppendAllText(AppContext.BaseDirectory + "\\log_calleridANS.txt", DateTime.Now.ToStr() + ": Ext" + extension + ",number:" + number + Environment.NewLine);

                    //}
                    //catch
                    //{
                    //    //
                    //}

                    //if (IsValidExt)
                    //{
                    //    if (Exten != Ext.ToString() && Exten != "650")
                    //    {
                    //        return;
                    //    }


                    //}
                    //else
                    //{

                    //

                    //if (UniqueCallID == "" || UniqueCallID != e.UniqueId)
                    //{
                    UniqueCallID = e.UniqueId;
                    //  Calllog.Info(e);
                    if (extension.IsNumeric() && extension.ToInt() >= CallerID_FromExt && extension.ToInt() <= CallerID_TillExt)
                    {
                        string unId = "";
                        try
                        {
                            e.Attributes.TryGetValue("linkedid", out unId);
                        }
                        catch
                        {

                        }
                        //
                        RecordDisplay(extension, "", number, extension, connectedLineNum, unId.ToStr());

                        // RecordDisplay(extension, "", number, extension, connectedLineNum, e.UniqueId.ToStr());
                    }
                    //   }

                    // }

                }
            }
            catch (Exception ex)
            {
                try
                {
                    //  Calllog.Info(e);
                    //  Calllog.Error(ex.Message);
                }
                catch
                {


                }

            }
        }

        private void ManagerEmerald_NewChannel(object sender, NewChannelEvent e)
        {
            try
            {
                File.AppendAllText(AppContext.BaseDirectory + "\\ManagerEmerald_NewChannel.txt", DateTime.Now.ToStr() + ": " + e.ToString() + Environment.NewLine);
            }
            catch
            {
                //
            }


            string desc = e.ChannelStateDesc;

            string connectedLineNum = string.Empty;
            string callerName = string.Empty;
            string callerNumber = string.Empty;
            string uniqueId = string.Empty;
            //
            if ((desc.ToString().ToUpper() == "RING") && !string.IsNullOrEmpty(e.CallerIdNum) && e.CallerIdNum.ToStr().Length > 7)
            {
                try
                {
                    //
                    e.Attributes.TryGetValue("exten", out connectedLineNum);
                    callerNumber = e.CallerIdNum;
                    callerName = e.CallerIdName;
                    uniqueId = e.UniqueId;
                    //  e.Attributes.TryGetValue("linkedid", out uniqueId);



                    if (!string.IsNullOrEmpty(connectedLineNum) && connectedLineNum.ToStr().Length > 7)
                    {
                        try
                        {
                            using (TaxiDataContext db = new TaxiDataContext())
                            {
                                db.CommandTimeout = 4;
                                callerName = db.stp_GetCallerInfo(callerNumber, "").FirstOrDefault().DefaultIfEmpty().Name;
                            }
                        }
                        catch
                        {
                        }

                        string item = string.Empty;
                        if (!string.IsNullOrEmpty(callerName))
                        {
                            item = callerName + " - " + callerNumber + "-" + string.Format("{0:HH:mm}", DateTime.Now);
                        }
                        else
                        {
                            item = callerNumber + "-" + string.Format("{0:HH:mm}", DateTime.Now);
                        }


                        try
                        {
                            if (connectedLineNum.IsNumeric())
                            {
                                if (connectedLineNum.StartsWith("44"))
                                {
                                    connectedLineNum = connectedLineNum.Remove(0, 2);
                                    connectedLineNum = connectedLineNum.Insert(0, "0");
                                }
                            }
                        }
                        catch
                        {
                        }

                        //
                      General.BroadCastMessage("**cti_remoteincomingcall>>" + callerNumber.ToStr() + ">>" + "XXX" + ">>ring>>" + item);

                        CreateLog(callerName, callerNumber.ToStr(), DateTime.Now, "00:00:00", "", connectedLineNum);

                    }
                }
                catch
                {
                }
            }

        }



        public void ManagerEmerald_NewState(object sender, NewStateEvent e)
        {
            try
            {
                //  Calllog.Info(e);

                string desc = e.ChannelStateDesc;

                string connectedLineNum = string.Empty;
                string callerName = string.Empty;
                string callerNumber = string.Empty;
                string uniqueId = string.Empty;
                //
                if (desc.ToString().ToUpper() == "RING" && !string.IsNullOrEmpty(e.CallerIdNum) && e.CallerIdNum.ToStr().Length > 7)
                {
                    try
                    {
                        //
                        e.Attributes.TryGetValue("exten", out connectedLineNum);
                        callerNumber = e.CallerIdNum;
                        callerName = e.CallerIdName;
                        uniqueId = e.UniqueId;
                        //  e.Attributes.TryGetValue("linkedid", out uniqueId);

                        if (!string.IsNullOrEmpty(connectedLineNum) && connectedLineNum.ToStr().Length > 7)
                        {
                            try
                            {
                                using (TaxiDataContext db = new TaxiDataContext())
                                {
                                    db.CommandTimeout = 4;
                                    callerName = db.stp_GetCallerInfo(callerNumber, "").FirstOrDefault().DefaultIfEmpty().Name;
                                }
                            }
                            catch
                            {
                            }

                            string item = string.Empty;
                            if (!string.IsNullOrEmpty(callerName))
                            {
                                item = callerName + " - " + callerNumber + "-" + string.Format("{0:HH:mm}", DateTime.Now);
                            }
                            else
                            {
                                item = callerNumber + "-" + string.Format("{0:HH:mm}", DateTime.Now);
                            }


                            try
                            {
                                if (connectedLineNum.IsNumeric())
                                {
                                    if (connectedLineNum.StartsWith("44"))
                                    {
                                        connectedLineNum = connectedLineNum.Remove(0, 2);
                                        connectedLineNum = connectedLineNum.Insert(0, "0");
                                    }
                                }
                            }
                            catch
                            {
                            }

                            //
                            General.BroadCastMessage("**cti_remoteincomingcall>>" + callerNumber.ToStr() + ">>" + "XXX" + ">>ring>>" + item);

                            CreateLog(callerName, callerNumber.ToStr(), DateTime.Now, "00:00:00", "", connectedLineNum);

                        }
                    }
                    catch
                    {
                    }
                }
                else if (desc.ToString().ToUpper() == "UP" && !string.IsNullOrEmpty(e.CallerIdNum) && e.CallerIdNum.ToStr().Length < 6)
                {
                    e.Attributes.TryGetValue("exten", out connectedLineNum);
                    e.Attributes.TryGetValue("connectedlinenum", out callerNumber);
                    callerNumber = e.Connectedlinenum;
                    //callerNumber = e.AccountCode;
                    callerName = e.CallerIdName;



                    try
                    {
                         e.Attributes.TryGetValue("linkedid", out uniqueId);

                     //   uniqueId = e.UniqueId;
                    }
                    catch
                    {
                        //
                    }
                    // uniqueId = e.UniqueId;
                    string exten = e.CallerIdNum;
                    //
                    try
                    {
                        if (connectedLineNum.IsNumeric())
                        {
                            if (connectedLineNum.StartsWith("44"))
                            {
                                connectedLineNum = connectedLineNum.Remove(0, 2);
                                connectedLineNum = connectedLineNum.Insert(0, "0");
                            }
                        }


                    }
                    catch
                    {
                    }

                    if (exten.ToStr().Length < 6 && connectedLineNum.ToStr().Length > 7)
                    {
                        RecordDisplayEmerald(exten, "", callerNumber, exten, connectedLineNum, uniqueId);
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(AppContext.BaseDirectory + "\\exception_ManagerEmerald_NewState.txt", DateTime.Now.ToStr() + ": " + ex.Message + "|" + ex.InnerException.StackTrace + "|" + ex.InnerException.Message + Environment.NewLine);
            }
        }

        public void RecordDisplayEmerald(string line, string name, string phone, string stn, string calledNumber, string uniqueId = null)
        {
            try
            {
                if (phone.StartsWith("9"))
                    phone = phone.Substring(phone.Length > 1 ? 1 : phone.Length);

                if (phone.StartsWith("00"))
                {
                    phone = phone.Substring(phone.Length > 1 ? 1 : phone.Length);
                }

                if (phone.Length < 8 || line.ToStr().Trim() == "<unknown>")
                    return;

                if (line.ToStr().Trim().Length > 6)
                {
                    try
                    {
                        //log
                        //     //Calllog.InfoFormat("XCaller : {0} , Status : {1} , Ext :{2}  ,Time : {3}", phone.ToStr(), "Answer", line.ToStr().Trim(), string.Format("{0:HH:mm}", DateTime.Now));
                    }
                    catch
                    {

                    }

                    return;
                }

                //AppVars.openedPhoneNo = phone;
                string BlackListReason = string.Empty;

                // Console.WriteLine(string.Format("Caller No : {0} ", phone));
                // New

                Customer objCustomer = null;
                try
                {
                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        db.CommandTimeout = 4;

                        objCustomer = db.Customers.Where(x => x.TelephoneNo == phone || x.MobileNo == phone).FirstOrDefault();
                    }
                }
                catch (Exception ex)
                {
                    //log
                    //Errorlog.Error(ex.Message, ex);
                }

                if (objCustomer != null)
                {
                    if (objCustomer.Name.ToString().ToLower().StartsWith("driver "))
                        return;
                    name = objCustomer.Name.ToString();

                    // If Customer is Black Listed
                    if (Convert.ToBoolean(objCustomer.BlackList))
                    {
                        BlackListReason = objCustomer.BlackListResion.ToString().Trim();
                    }
                }

                DateTime callDate = DateTime.Now;

                try
                {
                    //log
                    //   //Calllog.InfoFormat("Caller : {0} , Status : {1} , Ext :{2}  ,Time : {3}", phone.ToStr(), "Answer", line.ToStr().Trim(), string.Format("{0:HH:mm}", callDate));
                }
                catch
                {

                }

                // //BroadCaster
                var msg = "**cti_incomingcall>>" + phone.ToStr() + ">>" + line.ToStr().Trim() + ">>answer>>" + "ANS" + ">>" + "vpn" + ">>" + calledNumber + ">>" + uniqueId;

                //send message to all desktop users
                //List<string> listOfConnections = new List<string>();
                //listOfConnections = Instance.ReturnDesktopConnections();
                //Instance.Clients.Clients(listOfConnections).cMessageToDesktop(msg);


                General.BroadCastMessage(msg);
                //fe
                //new BroadCaster().BroadCastToAll("**cti_incomingcall>>" + phone.ToStr() + ">>" + line.ToStr().Trim() + ">>answer>>" + "ANS" + ">>" + "vpn" + ">>" + calledNumber + ">>" + uniqueId);
                //
                //if (true) // (EnabledMissedCallLogs)
                //{
                UpdateLog(name, phone, callDate, uniqueId, line, line, "", calledNumber);
                //}
                //else
                //{
                //   CreateLog(name, phone, callDate, uniqueId, line, calledNumber);
                //}                
            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\exception_CallerIDRecordDisplay.txt", DateTime.Now.ToStr() + ": " + ex.Message + "|" + ex.InnerException.StackTrace + "|" + ex.InnerException.Message + Environment.NewLine);
                }
                catch
                {


                }

            }
        }


        private void callerID_Tick(Object source, System.Timers.ElapsedEventArgs e)
        {
            try
            {

                //

                if (manager.IsConnected() == false)
                {
                    try
                    {
                        manager.Logoff();

                        manager.Login();
                        //

                        File.AppendAllText(AppContext.BaseDirectory + "\\callerID_Tick_autoreconnect.txt", DateTime.Now.ToStr() + Environment.NewLine);
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

                    File.AppendAllText(AppContext.BaseDirectory + "\\callerID_Tick_exception_autoreconnect.txt", DateTime.Now.ToStr() + "," + ex.Message + Environment.NewLine);

                    if (objAsterik == null)
                    {
                        objAsterik = General.GetObject<CallerIdVOIP_Configuration>(c => c.Port != null);

                    }

                    manager = new ManagerConnection(objAsterik.Host.ToStr(), objAsterik.Port.ToInt(), objAsterik.UserName.ToStr(), objAsterik.Password.ToStr());

                    if (CallerIdType == 2)
                    {
                        if (Global.CallerId_EnableHotDesk == "1")
                            manager.NewState += new NewStateEventHandler(Manager_NewStateHotDesk);
                        else
                            manager.NewState += new NewStateEventHandler(Manager_NewState);

                       
                    }
                    else if (CallerIdType == 4)
                    {
                        manager.NewState += new NewStateEventHandler(ManagerEmerald_NewState);
                        manager.NewChannel += ManagerEmerald_NewChannel;
                    }


                    try
                    {

                        manager.Login();

                        try
                        {
                            File.AppendAllText(physicalPath + "\\logrestart_calleridstart.txt", DateTime.Now.ToStr() + Environment.NewLine);




                        }
                        catch
                        {

                        }

                    }
                    catch (Exception ex2)
                    {

                        try
                        {
                            File.AppendAllText(physicalPath + "\\logrestart_exception_calleridstart.txt", DateTime.Now.ToStr() + "," + ex2.Message + Environment.NewLine);
                        }
                        catch
                        {

                        }
                        //ShowNotification("VOIP", "Asterik Voip Connection Failed : " + ex.Message, null);
                    }
                }
                catch
                {

                }
                //ShowNotification("VOIP", "Asterik Voip Connection Failed : " + ex.Message, null);
            }



        }













        public static void SendSMS(string number, string message)
        {
            if (number.Length >= 10 && (number.ToStr().StartsWith("+447") || number.ToStr().StartsWith("07")))
            {

                if (SelectedGateway == GatewayType.HypermediaSMSGateway)
                    HMSMS_smsgateway.Send(number, message);
                else
                    DSSMS_smsgateway.Send(number, message);

            }
        }







        public static string RestartSMS(string message)
        {
            string resp = string.Empty;
            try
            {
                //
                //DSSMS_smsgateway=new DSSMS.SmsGateway()
                if (message.ToStr().Trim() == "restart")
                {
                    Gen_SysPolicy_SMSConfiguration objSMSConfig = null;

                    try
                    {
                        using (TaxiDataContext db = new TaxiDataContext())
                        {

                            objSMSConfig = db.Gen_SysPolicy_SMSConfigurations.FirstOrDefault();



                        }
                    }
                    catch
                    {

                    }


                    if (objSMSConfig != null && objSMSConfig.ClickSMSApiKey.ToStr().Trim().StartsWith("http") && objSMSConfig.ClickSMSUserName.ToStr().Trim().Length > 0
                          && objSMSConfig.ClickSMSPassword.ToStr().Trim().Length > 0 && objSMSConfig.ClickSMSSenderName.ToStr().Trim().Length > 0)
                    {


                        DinstarSettings DSSMS_Settings = new DinstarSettings()
                        {
                            ServerBaseURL = objSMSConfig.ClickSMSApiKey.ToStr().Trim(),
                            UserName = objSMSConfig.ClickSMSUserName.ToStr().Trim(),
                            Password = objSMSConfig.ClickSMSPassword.ToStr().Trim(),
                            CanReceiveSMS = true,
                            DefaultClientId = "",
                            SendingMsgPort = new int[] { 0 },
                            ReceivingMsgPort = new int[] { 0 }
                        };



                        DSSMS_Settings.SendingMsgPort = objSMSConfig.ClickSMSSenderName.ToStr().Trim().Split(',').Select(m => Convert.ToInt32(m)).ToArray();

                        if (objSMSConfig.ModemSMSPortName.ToStr().Trim().Length > 0)
                            DSSMS_Settings.ReceivingMsgPort = objSMSConfig.ModemSMSPortName.ToStr().Trim().Split(',').Select(m => Convert.ToInt32(m)).ToArray();


                        DSSMS_smsgateway = new DSSMS.SmsGateway(DSSMS_Settings.ServerBaseURL, DSSMS_Settings.UserName, DSSMS_Settings.Password, DSSMS_Settings.CanReceiveSMS, DSSMS_Settings.SendingMsgPort, DSSMS_Settings.ReceivingMsgPort);

                        if(smsInbox=="1")
                        DSSMS_smsgateway.OnPostMessageIn += DSSMS_smsgateway_OnPostMessageIn;
                        resp = "SMS Service Restarted Successfully!";
                        try
                        {
                            File.AppendAllText(AppContext.BaseDirectory + "\\SMSServiceStart.txt", DateTime.Now.ToStr() + " : Restart by Db Credentials" + Environment.NewLine);
                        }
                        catch
                        {
                            //
                        }
                    }


                    //

                }
            }
            catch (Exception ex)
            {
                resp = "Something went wrong!" + Environment.NewLine + ex.Message;

            }

            return resp;
        }




        public static void AddOfflineJob(int driverId, string driverno, string resp)
        {
            try
            {

                HubProcessor.Instance.listofJobs.Add(new clsPDA
                {
                    JobId = 0,
                    DriverId = driverId,
                    MessageDateTime = DateTime.Now,
                    JobMessage = resp,
                    MessageTypeId = eMessageTypes.JOB,
                    DriverNo = driverno
                });

                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\AddOfflineJob.txt", DateTime.Now.ToStr() + "," + resp + Environment.NewLine);
                }
                catch
                {


                }
            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\AddOfflineJob_exception.txt", DateTime.Now.ToStr() + "," + ex.Message + Environment.NewLine);
                }
                catch
                {


                }

            }
        }




     




        #region AutoDispatch

        private void AutoDispatchActivity(Object source, System.Timers.ElapsedEventArgs e)
        {

            try
            {
                if (IsPerformingAutoDespatchActivity == false)
                {
                    if (Instance.objPolicy.EnableAutoDespatch.ToBool())
                    {


                        new Thread(delegate ()
                        {
                            try
                            {
                                using (TaxiDataContext db = new TaxiDataContext())
                                {
                                    if (IsPerformingAutoDespatchActivity == false)
                                    {
                                        db.CommandTimeout = 5;
                                        // var bookings = db.stp_GetAutoDispatchBookings().ToList();
                                        var bookings = db.ExecuteQuery<stp_GetAutoDispatchBookingsResultEx>("exec stp_GetAutoDispatchBookings").ToList();
                                        if (bookings.Count > 0)
                                        {
                                            PerformAutoDespatchActivity(bookings);

                                            // ThreadPool.QueueUserWorkItem(PerformAutoDespatchActivity, bookings);

                                        }
                                    }
                                    //else
                                    //{
                                    //    try
                                    //    {
                                    //        File.AppendAllText(physicalPath + "\\isperformingauto.txt", DateTime.Now.ToStr()  + Environment.NewLine);
                                    //        //
                                    //    }
                                    //    catch
                                    //    {


                                    //    }

                                    //}

                                }
                            }
                            catch (Exception ex)
                            {
                                try
                                {
                                    File.AppendAllText(physicalPath + "\\autodespatchcatchlog.txt", DateTime.Now.ToStr() + ex.Message + Environment.NewLine);

                                   
                                    //
                                }
                                catch
                                {


                                }
                            }

                        }).Start();

                    }
                    if (Instance.objPolicy.EnableBidding.ToBool())
                    {
                        new Thread(delegate ()
                        {

                            try
                            {
                                if (Instance.objPolicy.EnableBiddingForChauffers.ToBool())
                                {

                                    CheckChaufferBiddingJobs();

                                    try
                                    {
                                        File.AppendAllText(AppContext.BaseDirectory + "\\CHECKCHAUFFERBIDDING.txt", DateTime.Now.ToStr() + Environment.NewLine);
                                    }
                                    catch
                                    {

                                    }

                                }
                                else
                                    CheckBiddingJobs();
                            }
                            catch
                            {

                            }


                        }).Start();

                    }

                }
            }
            catch (Exception EX)
            {


            }





        }
        public static List<ClsAutoDespatchPlot> listofSuccessAutoDespatch = null;
        public static List<Gen_PricePlot> listofPricePlots = null;
        private static bool IsPerformingAutoDespatchActivity = false;
        int jobTimeOutInterval = 0;

        private string DistanceMatrixKey = string.Empty;
        private string DistanceMatrixServerKey = string.Empty;
        private List<clsDriverRadius> listofDrvRadius = new List<clsDriverRadius>();
        public class clsDriverRadius
        {
            public double latitude1;
            public double latitude2;
            public double longitude1;
            public double longitude2;
            public double Miles;

        }


        private double GetNearestDriverRadiusOnline(double? Latitude, double? Longitude, double? destLatitude, double? destLongitude)
        {
            double miles = 0.00;
            try
            {

                if (string.IsNullOrEmpty(DistanceMatrixKey))
                {
                    using (TaxiDataContext db = new TaxiDataContext())
                    {

                        DistanceMatrixKey = db.ExecuteQuery<string>("select APIKey from mapkeys where maptype='distancematrix'").FirstOrDefault().ToStr().Trim();


                        if (DistanceMatrixKey.Length == 0)
                            DistanceMatrixKey = " ";
                        else
                            DistanceMatrixKey = "&key=" + DistanceMatrixKey;
                    }
                }


                if (listofDrvRadius.Count(c => c.latitude1 == Latitude && c.longitude1 == Longitude && c.latitude2 == destLatitude && c.longitude2 == destLongitude) > 0)
                {
                    miles = listofDrvRadius.FirstOrDefault(c => c.latitude1 == Latitude && c.longitude1 == Longitude && c.latitude2 == destLatitude && c.longitude2 == destLongitude).Miles;
                }
                else
                {
                    if (DistanceMatrixKey.ToStr().Trim().Length > 0)
                    {

                        using (System.Data.DataSet ds = new System.Data.DataSet())
                        {
                            ds.ReadXml(new XmlTextReader("https://maps.googleapis.com/maps/api/distancematrix/xml?origins=" + Latitude + "," + Longitude + "&destinations=" + destLatitude + "," + destLongitude + DistanceMatrixKey + "&units=imperial&mode=driving&sensor=false"));
                            DataTable dt = ds.Tables["distance"];

                            if (dt != null && dt.Rows.Count > 0)
                            {
                                miles = Convert.ToDouble(dt.Rows[0]["text"].ToStr().Replace(" mi", "").Trim());


                                if (listofDrvRadius.Count(c => c.latitude1 == Latitude && c.longitude1 == Longitude && c.latitude2 == destLatitude && c.longitude2 == destLongitude) == 0)
                                {
                                    listofDrvRadius.Add(new clsDriverRadius { latitude1 = Convert.ToDouble(Latitude), longitude1 = Convert.ToDouble(Longitude), latitude2 = Convert.ToDouble(destLatitude), longitude2 = Convert.ToDouble(destLongitude), Miles = miles });

                                }
                            }

                        }

                    }



                    if (miles == 0)
                    {
                        if (string.IsNullOrEmpty(DistanceMatrixServerKey))
                        {
                            using (TaxiDataContext db = new TaxiDataContext())
                            {

                                DistanceMatrixServerKey = db.ExecuteQuery<string>("select APIKey from mapkeys where maptype='distancematrixserverkey'").FirstOrDefault().ToStr().Trim();


                                if (DistanceMatrixServerKey.Length == 0)
                                    DistanceMatrixServerKey = " ";
                                else
                                    DistanceMatrixServerKey = "&key=" + DistanceMatrixServerKey;
                            }
                        }


                        if (DistanceMatrixServerKey.ToStr().Trim().Length > 0)
                        {


                            using (System.Data.DataSet ds = new System.Data.DataSet())
                            {
                                ds.ReadXml(new XmlTextReader("https://maps.googleapis.com/maps/api/distancematrix/xml?origins=" + Latitude + "," + Longitude + "&destinations=" + destLatitude + "," + destLongitude + DistanceMatrixServerKey + "&units=imperial&mode=driving&sensor=false"));
                                DataTable dt = ds.Tables["distance"];

                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    miles = Convert.ToDouble(dt.Rows[0]["text"].ToStr().Replace(" mi", "").Trim());


                                    if (listofDrvRadius.Count(c => c.latitude1 == Latitude && c.longitude1 == Longitude && c.latitude2 == destLatitude && c.longitude2 == destLongitude) == 0)
                                    {
                                        listofDrvRadius.Add(new clsDriverRadius { latitude1 = Convert.ToDouble(Latitude), longitude1 = Convert.ToDouble(Longitude), latitude2 = Convert.ToDouble(destLatitude), longitude2 = Convert.ToDouble(destLongitude), Miles = miles });

                                    }
                                }

                            }
                        }


                    }



                }
            }
            catch (Exception ex)
            {
                //try
                //{
                //    File.AppendAllText(Application.StartupPath + "\\distancematrix.txt", DateTime.Now.ToStr() + ":" + ex.Message);


                //}
                //catch
                //{


                //}

            }

            return miles;
        }





        private void PerformAutoDespatchActivity(List<stp_GetAutoDispatchBookingsResultEx> bookings)
        {

            try
            {


                if (IsPerformingAutoDespatchActivity)
                {
                    try
                    {
                        File.AppendAllText("autodespatchlog.txt", DateTime.Now.ToStr() + ": performing autodespatch" + Environment.NewLine);

                    }
                    catch
                    {


                    }

                    return;
                }


                IsPerformingAutoDespatchActivity = true;

                bool IsUpdated = false;
                string AutoRefreshVar = string.Empty;

                int autoDespatchType = Instance.objPolicy.AutoDespatchType.ToInt();

                if (autoDespatchType == 0)
                    return;





                if (jobTimeOutInterval == 0)
                {

                    if (Instance.objPolicy.PDAJobOfferRequestTimeout.ToInt() >= 20)
                    {
                        jobTimeOutInterval = Instance.objPolicy.PDAJobOfferRequestTimeout.ToInt();

                    }
                    else
                        jobTimeOutInterval = 115;

                }

                using (TaxiDataContext db = new TaxiDataContext())
                {





                    //if (Instance.objPolicy.AutoDespatchPriorityForAccJobs.ToBool())
                    //{




                    //    var list0 = bookings.Where(c => c.FromLocTypeId == Enums.LOCATION_TYPES.AIRPORT).OrderBy(c => c.BookingDate).ToList();



                    //    if (bookings.Where(c => c.FromLocTypeId != Enums.LOCATION_TYPES.AIRPORT).Count() == 2)
                    //    {


                    //        var list2 = bookings.Where(c => c.FromLocTypeId != Enums.LOCATION_TYPES.AIRPORT).OrderBy(c => c.BookingDate).ToList();
                    //        bookings = list0.Union(list2).ToList();

                    //    }
                    //    else
                    //    {

                    //        var list2 = bookings.Where(c => c.FromLocTypeId != Enums.LOCATION_TYPES.AIRPORT).OrderBy(c => c.BookingDate).ToList();
                    //        bookings = list0.Union(list2).ToList();
                    //    }
                    //}
                    //else
                    //{

                    //  var list0 = bookings.Where(c => c.FromLocTypeId == Enums.LOCATION_TYPES.AIRPORT).OrderBy(c => c.BookingDate).ToList();


                    // var list1 = bookings.Where(c => c.FromLocTypeId != Enums.LOCATION_TYPES.AIRPORT).OrderBy(c => c.PickupDateTime).ToList();


                    //bookings = list0.Union(list1).ToList();


                    bookings = bookings.OrderBy(c => c.PickupDateTime).ToList();

                    //  }

                    //
                    int bookingCount = bookings.Count;
                    if (bookingCount > 0)
                    {
                        double fojRadius = Convert.ToDouble(Instance.objPolicy.AutoDespatchFOJRadius);
                        double nearestDrvWithinRadius = Convert.ToDouble(Instance.objPolicy.AutoDespatchNearestDrvRadius);
                        int longestWaitingMins = Instance.objPolicy.AutoDespatchLongestWaitingMins.ToInt();


                        if (listofSuccessAutoDespatch == null)
                            listofSuccessAutoDespatch = new List<ClsAutoDespatchPlot>();
                        else
                            listofSuccessAutoDespatch.Clear();



                        var listofDrvs = (from a in db.GetTable<Fleet_DriverQueueList>().Where(c => c.Status == true &&
                                                        (c.DriverWorkStatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE || (c.DriverWorkStatusId == Enums.Driver_WORKINGSTATUS.SOONTOCLEAR))
                                          )
                                          join b in db.GetTable<Fleet_Driver_Location>().Where(c => c.Latitude != 0
                                              //     && c.UpdateDate >= minUpdateDate
                                              )
                                                  on a.DriverId equals b.DriverId
                                          select new
                                          {
                                              DriverId = a.DriverId,
                                              DriverInfo = a.Fleet_Driver,
                                              DriverLocation = b.LocationName,
                                              ZoneId = b.ZoneId,
                                              PlotDateTime = b.PlotDate,
                                              StatusId = a.DriverWorkStatusId,
                                              Latitude = b.Latitude,
                                              Longitude = b.Longitude,
                                              EstTime = b.EstimatedTimeLeft,
                                              WaitSince = a.WaitSinceOn,
                                              JobClearingZoneId = default(int?)
                                              ,
                                              a.IsIdle
                                          }).ToList();

                        if (listofDrvs.Count > 0)
                        {

                            if (listofPricePlots == null)
                            {
                                listofPricePlots = db.Gen_PricePlots.ToList();

                            }




                            List<string> listofErrors = new List<string>();

                            int nearestRadius = Instance.objPolicy.AutoDespatchElapsedTime.ToInt();


                            if (autoDespatchType == 5 && Instance.objPolicy.AutoDespatchPriorityForAccJobs.ToBool()) // AutoDespatch Rule X2 :- Top Standing in Plot Queue with nearest driver
                            {
                                bookings = (from b in bookings
                                            orderby b.Priority descending, b.BookingDate ascending
                                            select b).ToList();


                            }

                            foreach (var job in bookings)
                            {




                                if (job.ZoneId == null && autoDespatchType == Enums.AUTODESPATCH_TYPES.TOP_STANDING_QUEUE) // AutoDespatch Rule 1 :- Top Standing in Plot Queue
                                    continue;


                                if (job.ZoneId != null && job.EnableZoneAutoDispatch.ToBool() == false)
                                    continue;






                                if (job.DespatchDateTime != null && job.DespatchDateTime.Value.AddSeconds(jobTimeOutInterval) > DateTime.Now && job.bookingstatusId.ToInt() == Enums.BOOKINGSTATUS.PENDING)
                                    continue;



                                try
                                {
                                    if (job.bookingstatusId == Enums.BOOKINGSTATUS.BID && listofDrvBidding != null && listofDrvBidding.Count(c => c.JobId == job.JobId && c.BiddingDateTime.Value.AddSeconds(20) > DateTime.Now) > 0)
                                    {
                                        try
                                        {
                                            File.AppendAllText(physicalPath + "\\DriversBiddingonthisjob.txt", DateTime.Now.ToStr() + ": JOBID" + job.JobId.ToStr() + Environment.NewLine);

                                        }
                                        catch
                                        {


                                        }
                                        continue;


                                    }


                                }
                                catch (Exception ex)
                                {
                                    try
                                    {
                                        File.AppendAllText(physicalPath + "\\AutoAlready_Exception.txt", DateTime.Now.ToStr() + ": JOBID" + job.JobId.ToStr() + ",exception:" + ex.Message + Environment.NewLine);

                                    }
                                    catch
                                    {


                                    }
                                }



                                var pendingDrvs = db.Bookings.Where(c => c.DriverId != null && c.DespatchDateTime != null
                                                                 &&
                                                                 (
                                                                         (
                                                                               (c.BookingStatusId == Enums.BOOKINGSTATUS.PENDING || c.BookingStatusId == Enums.BOOKINGSTATUS.NOTACCEPTED)
                                                                              && c.DespatchDateTime.Value.AddSeconds(jobTimeOutInterval) > DateTime.Now
                                                                         )

                                                                    ||
                                                                         (
                                                                               (c.BookingStatusId == Enums.BOOKINGSTATUS.FOJ && c.DespatchDateTime.Value.AddDays(1) > DateTime.Now)
                                                                         )


                                                                )
                                                                )
                                                                .Select(c => c.DriverId).Distinct().ToArray<int?>();


                                if (pendingDrvs.Count() > 0)
                                {
                                    foreach (var item in pendingDrvs)
                                    {
                                        listofDrvs.RemoveAll(c => c.DriverId == item);
                                    }

                                }
                                else
                                {
                                    listofDrvs.RemoveAll(c => listofSuccessAutoDespatch.Find(a => a.DriverId == c.DriverId) != null);

                                }



                                if (job.bookingstatusId.ToInt() == Enums.BOOKINGSTATUS.PENDING && job.DriverId != null && job.DespatchDateTime.Value.AddSeconds(jobTimeOutInterval) < DateTime.Now)
                                {

                                    if (listofDrvs.Count > 1 && listofDrvs.Count(c => c.DriverId == job.DriverId) > 0)
                                    {

                                        int rtn = 0;
                                        using (TaxiDataContext dbX = new TaxiDataContext())
                                        {
                                            try
                                            {

                                                if (dbX.Fleet_Driver_RejectJobs.Count(c => c.BookingId == job.JobId && c.DriverId == job.DriverId && c.BookingStatusId == Enums.BOOKINGSTATUS.NOTACCEPTED) >= 1)
                                                {

                                                    rtn = dbX.ExecuteQuery<int>("stp_UpdateJobStatusWithDriver {0}, {1}, {2}", job.JobId, 101, job.DriverId).FirstOrDefault();
                                                }
                                                else
                                                {
                                                    rtn = dbX.ExecuteQuery<int>("stp_UpdateJobStatusWithDriver {0}, {1}, {2}", job.JobId, 102, job.DriverId).FirstOrDefault();

                                                }
                                            }
                                            catch
                                            {


                                            }
                                        }


                                        if (rtn == 1)
                                        {
                                            General.BroadCastMessage("refresh active dashboard");



                                        }

                                        if (rtn != -1)
                                            continue;
                                    }
                                }



                                int? RejectedJobDriverId = null;





                                var listofJobAvailableDrvs = listofDrvs.Where(c => c.DriverId == 0).ToList();




                                string vehAttributes = job.VehicleAttributes.ToStr().Trim();

                                if (vehAttributes.Length > 0)
                                {




                                    string jobAttributes = job.JobAttributes.ToStr().Trim();
                                    if (jobAttributes.Length > 0)
                                    {



                                        // Get Vehicle Attributes Driver
                                        listofJobAvailableDrvs = listofDrvs.Where(c => vehAttributes.Contains("," + c.DriverInfo.VehicleTypeId.ToStr() + ",")).ToList();


                                        // Exclude Drivers
                                        if (listofJobAvailableDrvs.Count > 0)
                                        {

                                            if (job.ExcludedDriverIds.ToStr().Trim().Length > 0)
                                            {
                                                foreach (var objExcDriverId in job.ExcludedDriverIds.ToStr().Trim().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                                                {

                                                    listofJobAvailableDrvs.RemoveAll(c => c.DriverId == objExcDriverId.ToInt());
                                                }
                                            }
                                        }




                                        // Get Matching Attributes Drivers with JOB
                                        if (listofJobAvailableDrvs.Count > 0)
                                        {
                                            foreach (var objAttr in jobAttributes.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                                            {

                                                listofJobAvailableDrvs.RemoveAll(c => c.DriverInfo.AttributeValues.ToStr().Contains("," + objAttr + ",") == false);
                                            }
                                        }


                                    }
                                    else
                                    {


                                        listofJobAvailableDrvs = listofDrvs.Where(c => vehAttributes.Contains("," + c.DriverInfo.VehicleTypeId.ToStr() + ",")).ToList();


                                        // Exclude Drivers
                                        if (listofJobAvailableDrvs.Count > 0)
                                        {

                                            if (job.ExcludedDriverIds.ToStr().Trim().Length > 0)
                                            {
                                                foreach (var objExcDriverId in job.ExcludedDriverIds.ToStr().Trim().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                                                {

                                                    listofJobAvailableDrvs.RemoveAll(c => c.DriverId == objExcDriverId.ToInt());
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {

                                    // DRIVER ATTRIBUTES  WHC,YYY
                                    // JOB ATTRIBUTES ,TST,WHC,

                                    string jobAttributes = job.JobAttributes.ToStr().Trim();
                                    if (jobAttributes.Length > 0)
                                    {
                                        listofJobAvailableDrvs = listofDrvs.Where(c => jobAttributes.Contains("," + c.DriverInfo.AttributeValues.ToStr() + ",")).ToList();
                                    }
                                    else
                                    {

                                        listofJobAvailableDrvs = listofDrvs.Where(c => c.DriverInfo.Fleet_VehicleType.NoofPassengers >= job.NoofPassengers).ToList();
                                    }
                                }



                                if (((job.bookingstatusId == Enums.BOOKINGSTATUS.WAITING || job.bookingstatusId == Enums.BOOKINGSTATUS.BID) && job.DriverId != null) || job.bookingstatusId == Enums.BOOKINGSTATUS.NOSHOW || job.bookingstatusId == Enums.BOOKINGSTATUS.NOTACCEPTED || job.bookingstatusId == Enums.BOOKINGSTATUS.REJECTED)
                                {
                                    int rejectRetry = Instance.objPolicy.NoResponseRetry.ToInt();

                                    if (rejectRetry > 0)
                                    {

                                        int driverRejectCount = General.GetQueryable<Fleet_Driver_RejectJob>(c => c.BookingId == job.JobId && c.DriverId == job.DriverId).Count();

                                        if (driverRejectCount > rejectRetry)
                                        {
                                            RejectedJobDriverId = job.DriverId;


                                            if (RejectedJobDriverId != null)
                                            {
                                                listofJobAvailableDrvs.RemoveAll(c => c.DriverId == RejectedJobDriverId);
                                            }
                                        }



                                    }
                                    else
                                    {
                                        using (TaxiDataContext db2 = new TaxiDataContext())
                                        {


                                            int driverRejectCount = db2.Fleet_Driver_RejectJobs.Where(c => c.BookingId == job.JobId).Count();


                                            RejectedJobDriverId = job.DriverId;
                                            if (driverRejectCount > 0 && RejectedJobDriverId != null)
                                            {
                                                foreach (var item in db2.Fleet_Driver_RejectJobs.Where(c => c.BookingId == job.JobId).Select(c => c.DriverId))
                                                {
                                                    listofJobAvailableDrvs.RemoveAll(c => c.DriverId == item);

                                                }


                                            }
                                        }

                                    }

                                }


                                //if (listofJobAvailableDrvs.Count == 0)
                                //{
                                //    //if(job.bookingstatusId.ToInt()!=Enums.BOOKINGSTATUS.BID)
                                //    //{
                                //    //    SendJobOnBid(job);
                                //    //    IsUpdated = true;
                                //    //    continue;
                                //    //}
                                //    //else
                                //    continue;


                                //}




                                // Remove pending and notaccepted driver of other jobs in this job
                                foreach (var book in bookings.Where(c => c.JobId != job.JobId && c.DriverId != null && (c.bookingstatusId == Enums.BOOKINGSTATUS.PENDING)))
                                {
                                    listofJobAvailableDrvs.RemoveAll(c => c.DriverId == book.DriverId);


                                }







                                // Check for Pre-Allocated Driver First

                                if (Instance.objPolicy.AutoDespatchPriorityForAllocatedDrv.ToBool() && job.AutoDespatch.ToBool() && (job.bookingstatusId.ToInt() == Enums.BOOKINGSTATUS.WAITING || job.bookingstatusId.ToInt() == Enums.BOOKINGSTATUS.PENDING || job.bookingstatusId.ToInt() == Enums.BOOKINGSTATUS.NOTACCEPTED)
                                    && job.DriverId != null && job.IsConfirmedDriver.ToBool())
                                {

                                    var objTopDrvInQueueAllocated = (from a in listofJobAvailableDrvs
                                                                     where a.DriverId == job.DriverId

                                                                     select new
                                                                     {
                                                                         a,
                                                                         //  Distance = new LatLng(a.Latitude, a.Longitude).DistanceMiles(new LatLng(Convert.ToDouble(objJobCoord.Latitude), Convert.ToDouble(objJobCoord.Longitude))),
                                                                         a.WaitSince
                                                                     }).FirstOrDefault();


                                    if (objTopDrvInQueueAllocated != null)
                                    {





                                        if (objTopDrvInQueueAllocated.a.StatusId.ToInt() == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                        {
                                            OnSuccessAutoDespatchJob(job, objTopDrvInQueueAllocated.a.DriverInfo, ref listofErrors, "Job Auto Despatched to Allocated Drv '" + objTopDrvInQueueAllocated.a.DriverInfo.DriverNo + "'");
                                            listofSuccessAutoDespatch.Add(new ClsAutoDespatchPlot { DriverId = objTopDrvInQueueAllocated.a.DriverId, JobId = job.JobId });
                                            IsUpdated = true;

                                            if (bookingCount == 1)
                                                AutoRefreshVar = RefreshTypes.REFRESH_DESPATCHJOB + ">>" + job.JobId + ">>" + objTopDrvInQueueAllocated.a.DriverInfo.DriverNo.ToStr() + ">>" + Enums.BOOKINGSTATUS.PENDING + ">>pda";


                                            continue;
                                        }
                                        else if (objTopDrvInQueueAllocated.a.StatusId.ToInt() == Enums.Driver_WORKINGSTATUS.SOONTOCLEAR)
                                        {


                                            listofJobAvailableDrvs = listofJobAvailableDrvs.Where(c => c.ZoneId != null).ToList();



                                            var objTopDrvInQueue = listofJobAvailableDrvs.Where(c => (job.ZoneId != null && c.ZoneId == job.ZoneId)
                                                                                            && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                                                       .OrderBy(c => c.PlotDateTime).FirstOrDefault();




                                            // If Top Standing Available Driver exist
                                            if (objTopDrvInQueue != null && job.AutoDespatch.ToBool())
                                            {

                                                long rtnId = 0;
                                                using (TaxiDataContext dbX = new TaxiDataContext())
                                                {
                                                    rtnId = db.stp_IsJobAvailableForDriver(objTopDrvInQueue.DriverId, job.JobId).FirstOrDefault().DefaultIfEmpty().Id;

                                                }


                                                if (rtnId > 0)
                                                {
                                                    listofErrors.Clear();

                                                    OnSuccessAutoDespatchJob(job, objTopDrvInQueue.DriverInfo, ref listofErrors, "Top Standing Drv '" + objTopDrvInQueue.DriverInfo.DriverNo.ToStr() + "' of Job Plot");
                                                    listofSuccessAutoDespatch.Add(new ClsAutoDespatchPlot { DriverId = objTopDrvInQueue.DriverId, JobId = job.JobId });
                                                    IsUpdated = true;



                                                    if (objTopDrvInQueueAllocated.a.DriverId.ToInt() == job.DriverId.ToInt()
                                                        && objTopDrvInQueue.DriverId.ToInt() != objTopDrvInQueueAllocated.a.DriverId.ToInt())
                                                    {
                                                        Instance.listofJobs.Add(new clsPDA
                                                        {
                                                            JobId = job.JobId,
                                                            DriverId = job.DriverId.ToInt(),
                                                            MessageDateTime = DateTime.Now,
                                                            JobMessage = "failed:Allocated job has been recovered.",
                                                            MessageTypeId = eMessageTypes.STC_ALLOCATED,

                                                            DriverNo = ""
                                                        });



                                                    }
                                                }





                                            }

                                            continue;
                                        }


                                        //  continue;



                                    }
                                    else
                                    {




                                        if (listofJobAvailableDrvs.Count(c => c.DriverId == job.DriverId && c.StatusId == Enums.Driver_WORKINGSTATUS.SOONTOCLEAR) > 0
                                            && listofJobAvailableDrvs.Count(c => (job.ZoneId != null && c.ZoneId == job.ZoneId) && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE) > 0)
                                        {







                                        }
                                        else
                                            continue;
                                    }
                                }



                                //
                                //
                                // Remove allocated driver of other jobs in this job
                                foreach (var book in bookings.Where(c => c.JobId != job.JobId && c.DriverId != null && ((c.bookingstatusId == Enums.BOOKINGSTATUS.WAITING && c.IsConfirmedDriver == true) || c.bookingstatusId == Enums.BOOKINGSTATUS.ONHOLD)))
                                {
                                    listofJobAvailableDrvs.RemoveAll(c => c.DriverId == book.DriverId);

                                }
                                //


                                //need to uncomment
                                //if (job.bookingstatusId.ToInt() == Enums.BOOKINGSTATUS.BID)
                                //{

                                //    if (job.PriceBiddingExpiryDate == null)
                                //        continue;

                                //    if (job.PriceBiddingExpiryDate != null && job.PriceBiddingExpiryDate.Value.AddSeconds(10) > DateTime.Now)
                                //    {
                                //        continue;
                                //    }
                                //    else
                                //    {
                                //        try
                                //        {
                                //            if (job.PriceBiddingExpiryDate != null && job.PriceBiddingExpiryDate.Value.AddSeconds(10) <= DateTime.Now
                                //                && (job.Booking_Biddings.Count == 0 || job.Booking_Biddings.OrderByDescending(c => c.BiddingDateTime).FirstOrDefault(c => c.BiddingDateTime.Value.AddSeconds(10) < DateTime.Now) != null))
                                //            {

                                //                if (job.Booking_Biddings.Count > 0
                                //                    && job.Booking_Biddings.OrderByDescending(c => c.BiddingDateTime).FirstOrDefault(c => c.BiddingDateTime.Value.AddSeconds(10) > DateTime.Now) != null)
                                //                {
                                //                    continue;

                                //                }


                                //            }
                                //        }
                                //        catch
                                //        {


                                //        }
                                //    }
                                //}



                                // add price plot rule

                                try
                                {
                                    if (listofPricePlots != null && listofPricePlots.Count > 0)
                                    {
                                        int priceplotId = listofPricePlots.FirstOrDefault(c => (c.FromPrice.ToDecimal() <= job.FareRate.ToDecimal() && c.TillPrice.ToDecimal() >= job.FareRate.ToDecimal())
                                                 || (c.FromPrice.ToDecimal() <= job.FareRate.ToDecimal() && c.TillPrice == null)).DefaultIfEmpty().Id;

                                        if (priceplotId > 0)
                                        {
                                            int topstandingpriceplotid = 0;
                                            using (TaxiDataContext dbX = new TaxiDataContext())
                                            {
                                                topstandingpriceplotid = dbX.stp_GetPricePlotDrivers(0)
                                                                        .Where(c => c.PricePlotId == priceplotId && c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                   .OrderBy(c => c.QueueDateTime).FirstOrDefault().DefaultIfEmpty().DriverId.ToInt();




                                                //TOWN,SCARLETSS WELL, CASTLE STREET, COOKSLAND, WALKERLINES, WESTHEATH, DUNMERE
                                                if (topstandingpriceplotid > 0 && listofJobAvailableDrvs.Count(c => c.DriverId == topstandingpriceplotid) > 0)
                                                {
                                                    var objqueue = listofJobAvailableDrvs.FirstOrDefault(c => c.DriverId == topstandingpriceplotid
                                                                      && (c.ZoneId == job.ZoneId || c.ZoneId == 10 || c.ZoneId == 11 || c.ZoneId == 9 || c.ZoneId == 8 || c.ZoneId == 12 || c.ZoneId == 13 || c.ZoneId == 14)
                                                                      && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE);

                                                    // If Top Standing Available Driver exist
                                                    if (job.AutoDespatch.ToBool())
                                                    {


                                                        if (objqueue != null)
                                                        {

                                                            long rtnId = 0;

                                                            rtnId = db.stp_IsJobAvailableForDriver(objqueue.DriverId, job.JobId).FirstOrDefault().DefaultIfEmpty().Id;

                                                            if (rtnId > 0)
                                                            {
                                                                listofErrors.Clear();


                                                                OnSuccessAutoDespatchJob(job, objqueue.DriverInfo, ref listofErrors, "Top Standing Drv '" + objqueue.DriverInfo.DriverNo.ToStr() + "' of Price Plot");
                                                                listofSuccessAutoDespatch.Add(new ClsAutoDespatchPlot { DriverId = objqueue.DriverId, JobId = job.JobId });
                                                                IsUpdated = true;
                                                                continue;
                                                            }
                                                        }
                                                        else
                                                        {

                                                            if (Instance.objPolicy.LeastEstimedTimeToClear.ToInt() > 0 && Instance.objPolicy.EnableFOJ.ToBool() && job.ZoneId != null)
                                                            {

                                                                topstandingpriceplotid = dbX.stp_GetPricePlotDrivers(0)
                                                                                   .Where(c => c.PricePlotId == priceplotId && c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.SOONTOCLEAR)
                                                              .OrderBy(c => c.QueueDateTime).FirstOrDefault().DefaultIfEmpty().DriverId.ToInt();



                                                                try
                                                                {
                                                                    objqueue = listofJobAvailableDrvs.FirstOrDefault(c => c.DriverId == topstandingpriceplotid
                                                                                                                       && c.StatusId == Enums.Driver_WORKINGSTATUS.SOONTOCLEAR
                                                                                                                       && (c.ZoneId == job.ZoneId || c.ZoneId == 10 || c.ZoneId == 11 || c.ZoneId == 9 || c.ZoneId == 8 || c.ZoneId == 12 || c.ZoneId == 13 || c.ZoneId == 14)
                                                                                                                        );


                                                                    if (objqueue != null && job.AutoDespatch.ToBool())
                                                                    {
                                                                        long rtnId = 0;

                                                                        rtnId = db.stp_IsJobAvailableForDriver(objqueue.DriverId, job.JobId).FirstOrDefault().DefaultIfEmpty().Id;


                                                                        if (rtnId > 0)
                                                                        {

                                                                            listofErrors.Clear();
                                                                            OnSuccessAutoDespatchJobWithFOJ(job, objqueue.DriverInfo, ref listofErrors, "FOJ Job Auto Despatched to Top Standing Price Plot Driver '" + objqueue.DriverInfo.DriverNo.ToStr() + "' ", true);
                                                                            listofSuccessAutoDespatch.Add(new ClsAutoDespatchPlot { DriverId = objqueue.DriverId, JobId = job.JobId });

                                                                            IsUpdated = true;
                                                                        }
                                                                        continue;
                                                                    }
                                                                }
                                                                catch
                                                                {


                                                                }
                                                            }


                                                        }
                                                    }
                                                }
                                                else
                                                {

                                                    if (Instance.objPolicy.LeastEstimedTimeToClear.ToInt() > 0 && Instance.objPolicy.EnableFOJ.ToBool() && job.ZoneId != null)
                                                    {

                                                        topstandingpriceplotid = dbX.stp_GetPricePlotDrivers(0)
                                                                           .Where(c => c.PricePlotId == priceplotId && c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.SOONTOCLEAR)
                                                      .OrderBy(c => c.QueueDateTime).FirstOrDefault().DefaultIfEmpty().DriverId.ToInt();



                                                        try
                                                        {



                                                            var objqueue = (from a in listofJobAvailableDrvs.Where(c => c.DriverId == topstandingpriceplotid
                                                                                                                && c.StatusId == Enums.Driver_WORKINGSTATUS.SOONTOCLEAR
                                                                                                               && (c.ZoneId == job.ZoneId || c.ZoneId == 10 || c.ZoneId == 11 || c.ZoneId == 9 || c.ZoneId == 8 || c.ZoneId == 12 || c.ZoneId == 13 || c.ZoneId == 14)
                                                                                                                     )
                                                                            select new
                                                                            {
                                                                                a,
                                                                                Distance = 0.00m

                                                                            }).OrderBy(c => c.Distance).FirstOrDefault();

                                                            if (objqueue != null && job.AutoDespatch.ToBool())
                                                            {
                                                                long rtnId = 0;

                                                                rtnId = db.stp_IsJobAvailableForDriver(objqueue.a.DriverId, job.JobId).FirstOrDefault().DefaultIfEmpty().Id;



                                                                if (rtnId > 0)
                                                                {

                                                                    listofErrors.Clear();
                                                                    OnSuccessAutoDespatchJobWithFOJ(job, objqueue.a.DriverInfo, ref listofErrors, "FOJ Job Auto Despatched to Top Standing Price Plot Driver '" + objqueue.a.DriverInfo.DriverNo.ToStr() + "' ", true);
                                                                    listofSuccessAutoDespatch.Add(new ClsAutoDespatchPlot { DriverId = objqueue.a.DriverId, JobId = job.JobId });

                                                                    IsUpdated = true;
                                                                }
                                                                continue;
                                                            }
                                                        }
                                                        catch
                                                        {


                                                        }
                                                    }




                                                }
                                            }
                                        }


                                    }
                                }
                                catch
                                {


                                }
                                //


                                if (listofJobAvailableDrvs.Count(c => c.IsIdle != null && c.IsIdle == true && c.StatusId == Enums.Driver_WORKINGSTATUS.SOONTOCLEAR) > 0)

                                {
                                    // File.AppendAllText(physicalPath + "\\stcquerystart.txt", DateTime.Now + Environment.NewLine);

                                    using (TaxiDataContext db2 = new TaxiDataContext())
                                    {
                                        var stcList = listofJobAvailableDrvs
                                                                         .Where(c => c.IsIdle != null && c.IsIdle == true && c.StatusId == Enums.Driver_WORKINGSTATUS.SOONTOCLEAR).ToList();



                                        foreach (var itemSTC in stcList)
                                        {
                                            if (
                                                                             db2.Bookings.Where(b =>
                                                                               b.IsConfirmedDriver == true && b.DriverId == itemSTC.DriverId
                                                                               && b.ReAutoDespatchTime != null && b.BookingStatusId == Enums.BOOKINGSTATUS.WAITING)
                                                                               .Count() > 0)
                                            {

                                                listofJobAvailableDrvs.RemoveAll(c => c.DriverId == itemSTC.DriverId);
                                            }


                                        }

                                    }

                                    //  File.AppendAllText(physicalPath + "\\stcqueryend.txt", DateTime.Now + Environment.NewLine);

                                }

                                if (autoDespatchType == Enums.AUTODESPATCH_TYPES.TOP_STANDING_QUEUE) // AutoDespatch Rule 1 :- Top Standing in Plot Queue
                                {
                                    listofJobAvailableDrvs = listofJobAvailableDrvs.Where(c => c.ZoneId != null).ToList();



                                    var objTopDrvInQueue = listofJobAvailableDrvs.Where(c => (job.ZoneId != null && c.ZoneId == job.ZoneId) && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                                               .OrderBy(c => c.PlotDateTime).FirstOrDefault();




                                    // If Top Standing Available Driver exist
                                    if (objTopDrvInQueue != null && job.AutoDespatch.ToBool())
                                    {
                                        long rtnId = 0;
                                        using (TaxiDataContext dbX = new TaxiDataContext())
                                        {
                                            rtnId = db.stp_IsJobAvailableForDriver(objTopDrvInQueue.DriverId, job.JobId).FirstOrDefault().DefaultIfEmpty().Id;

                                        }


                                        if (rtnId > 0)
                                        {
                                            listofErrors.Clear();

                                            OnSuccessAutoDespatchJob(job, objTopDrvInQueue.DriverInfo, ref listofErrors, "Top Standing Drv '" + objTopDrvInQueue.DriverInfo.DriverNo.ToStr() + "' of Job Plot");
                                            listofSuccessAutoDespatch.Add(new ClsAutoDespatchPlot { DriverId = objTopDrvInQueue.DriverId, JobId = job.JobId });
                                            IsUpdated = true;
                                        }

                                    }
                                    else
                                    {
                                        // Top Standing Driver not exist check for STC Driver
                                        //if (fojRadius > 0)
                                        //{


                                        Gen_Coordinate objCoord = null;

                                        if (job.FromPostCode.ToStr().Trim().Length > 0)
                                        {
                                            objCoord = db.Gen_Coordinates.FirstOrDefault(c => c.PostCode == job.FromPostCode.ToStr().Trim().ToUpper());
                                        }


                                        var STCDriver = (from a in listofJobAvailableDrvs.Where(c => c.ZoneId == job.ZoneId &&
                                                                    c.StatusId == Enums.Driver_WORKINGSTATUS.SOONTOCLEAR && (c.IsIdle == null || c.IsIdle == false)

                                                                )
                                                         select new
                                                         {
                                                             a,
                                                             Distance = objCoord != null ? new LatLng(a.Latitude, a.Longitude).DistanceMiles(new LatLng(Convert.ToDouble(objCoord.Latitude), Convert.ToDouble(objCoord.Longitude))) : 0.0

                                                         })
                                                            //.Where(c => c.Distance < fojRadius)
                                                            .OrderBy(c => c.Distance).FirstOrDefault();





                                        if (STCDriver != null && job.AutoDespatch.ToBool())
                                        {

                                            try
                                            {
                                                using (TaxiDataContext db2 = new TaxiDataContext())
                                                {

                                                    db2.CommandTimeout = 5;

                                                    string query = "update booking set ReAutoDespatchTime=getdate(), bookingstatusid=1,IsConfirmedDriver=1,driverId=" + STCDriver.a.DriverId + " where id=" + job.JobId + ";";
                                                    query += "INSERT INTO BOOKING_LOG (bookingid,[user],beforeupdate,afterupdate,updatedate)values(" + job.JobId + ",'" + "AutoDespatch" + "','" + "" + "','Auto Allocate STC Driver (" + STCDriver.a.DriverInfo.DriverNo.ToStr() + ")',getdate());";
                                                    query += "Update fleet_driverqueuelist set isidle=1 where status=1 and driverid=" + STCDriver.a.DriverId;
                                                    db2.stp_RunProcedure(query);
                                                }
                                                IsUpdated = true;

                                                long autoJobId = job.JobId;
                                                int autoDriverId = STCDriver.a.DriverId.ToInt();
                                                bool autoIsIdle = STCDriver.a.IsIdle.ToBool();

                                                Instance.listofJobs.Add(new clsPDA
                                                {
                                                    JobId = autoJobId,
                                                    DriverId = autoDriverId,
                                                    MessageDateTime = DateTime.Now,
                                                    JobMessage = "success:You have allocated for Job [" + job.ZoneName.ToStr() + "]",
                                                    MessageTypeId = eMessageTypes.STC_ALLOCATED,

                                                    DriverNo = STCDriver.a.DriverInfo.DriverNo
                                                });

                                                listofJobAvailableDrvs.RemoveAll(C => C.DriverId == STCDriver.a.DriverId);
                                                listofDrvs.RemoveAll(C => C.DriverId == STCDriver.a.DriverId);

                                                //testauto
                                                File.AppendAllText(physicalPath + "\\autoallocatestc.txt", DateTime.Now.ToStr() + " jobId : " + autoJobId + ", driverId : " + autoDriverId + ", isidle : " + autoIsIdle + Environment.NewLine);


                                            }
                                            catch (Exception ex)
                                            {
                                                try
                                                {
                                                    File.AppendAllText(physicalPath + "\\autoallocatestc_exception.txt", DateTime.Now.ToStr() + " jobId : " + job.JobId + ", driverId : " + STCDriver.a.DriverId.ToInt() + ", isidle : " + STCDriver.a.IsIdle.ToBool() + Environment.NewLine);

                                                    listofJobAvailableDrvs.RemoveAll(C => C.DriverId == STCDriver.a.DriverId);
                                                    listofDrvs.RemoveAll(C => C.DriverId == STCDriver.a.DriverId);

                                                    File.AppendAllText(physicalPath + "\\autoallocatestcstep2.txt", DateTime.Now.ToStr() + " jobId : " + job.JobId + ", driverId : " + STCDriver.a.DriverId.ToInt() + ", isidle : " + STCDriver.a.IsIdle.ToBool() + Environment.NewLine);

                                                }
                                                catch
                                                {

                                                }


                                            }
                                            continue;
                                        }
                                        //   }



                                        // Check Driver in Backup Plots (Sub Rule 1)
                                        string reason = string.Empty;
                                        if (job.ZoneId != null && job.AutoDespatch.ToBool())
                                        {

                                            //   var backupZones = zonesList.FirstOrDefault(c => c.Id == job.ZoneId).Gen_Zone_Backups;
                                            var backupZones = db.Gen_Zones.FirstOrDefault(c => c.Id == job.ZoneId).Gen_Zone_Backups.DefaultIfEmpty();


                                            if (backupZones != null)
                                            {

                                                if (backupZones.BackupZone1Id != null && backupZones.BackupZone1Priority.ToBool())
                                                {

                                                    objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone1Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                     .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                    if (objTopDrvInQueue != null)
                                                        reason = "Top Standing Drv of Job 1st Backup Plot";
                                                }

                                                if (objTopDrvInQueue == null && backupZones.BackupZone2Id != null)
                                                {


                                                    objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone2Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                      .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                    if (objTopDrvInQueue != null)
                                                        reason = "Top Standing Drv of Job 2nd Backup Plot";
                                                }


                                                if (objTopDrvInQueue == null && backupZones.BackupZone3Id != null)
                                                {


                                                    objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone3Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                      .OrderBy(c => c.PlotDateTime).FirstOrDefault();


                                                    if (objTopDrvInQueue != null)
                                                        reason = "Top Standing Drv of Job 3rd Backup Plot";
                                                }

                                                if (objTopDrvInQueue == null && backupZones.BackupZone4Id != null)
                                                {


                                                    objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone4Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                      .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                    if (objTopDrvInQueue != null)
                                                        reason = "Top Standing Drv of Job 4th Backup Plot";
                                                }


                                                if (objTopDrvInQueue == null && backupZones.BackupZone5Id != null)
                                                {


                                                    objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone5Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                      .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                    if (objTopDrvInQueue != null)
                                                        reason = "Top Standing Drv of Job 5th Backup Plot";
                                                }


                                                if (objTopDrvInQueue == null && backupZones.BackupZone6Id != null)
                                                {


                                                    objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone6Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                      .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                    if (objTopDrvInQueue != null)
                                                        reason = "Top Standing Drv of Job 6th Backup Plot";
                                                }

                                                if (objTopDrvInQueue == null && backupZones.BackupZone7Id != null)
                                                {


                                                    objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone7Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                      .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                    if (objTopDrvInQueue != null)
                                                        reason = "Top Standing Drv of Job 7th Backup Plot";
                                                }


                                                if (objTopDrvInQueue == null && backupZones.BackupZone8Id != null)
                                                {


                                                    objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone8Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                      .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                    if (objTopDrvInQueue != null)
                                                        reason = "Top Standing Drv of Job 8th Backup Plot";
                                                }


                                                if (objTopDrvInQueue == null && backupZones.BackupZone9Id != null)
                                                {


                                                    objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone9Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                      .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                    if (objTopDrvInQueue != null)
                                                        reason = "Top Standing Drv of Job 9th Backup Plot";
                                                }


                                                if (objTopDrvInQueue == null && backupZones.BackupZone10Id != null)
                                                {


                                                    objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone10Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                      .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                    if (objTopDrvInQueue != null)
                                                        reason = "Top Standing Drv of Job 10th Backup Plot";
                                                }


                                                if (objTopDrvInQueue == null && backupZones.BackupZone11Id != null)
                                                {


                                                    objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone11Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                      .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                    if (objTopDrvInQueue != null)
                                                        reason = "Top Standing Drv of Job 11th Backup Plot";
                                                }


                                                if (objTopDrvInQueue == null && backupZones.BackupZone12Id != null)
                                                {


                                                    objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone12Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                      .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                    if (objTopDrvInQueue != null)
                                                        reason = "Top Standing Drv of Job 12th Backup Plot";
                                                }


                                                if (objTopDrvInQueue == null && backupZones.BackupZone13Id != null)
                                                {


                                                    objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone13Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                      .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                    if (objTopDrvInQueue != null)
                                                        reason = "Top Standing Drv of Job 13th Backup Plot";
                                                }


                                                if (objTopDrvInQueue == null && backupZones.BackupZone14Id != null)
                                                {


                                                    objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone14Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                      .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                    if (objTopDrvInQueue != null)
                                                        reason = "Top Standing Drv of Job 14th Backup Plot";
                                                }


                                                if (objTopDrvInQueue == null && backupZones.BackupZone15Id != null)
                                                {


                                                    objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone15Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                      .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                    if (objTopDrvInQueue != null)
                                                        reason = "Top Standing Drv of Job 15th Backup Plot";
                                                }


                                                if (objTopDrvInQueue == null && backupZones.BackupZone16Id != null)
                                                {


                                                    objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone16Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                      .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                    if (objTopDrvInQueue != null)
                                                        reason = "Top Standing Drv of Job 16th Backup Plot";
                                                }


                                                if (objTopDrvInQueue == null && backupZones.BackupZone17Id != null)
                                                {


                                                    objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone17Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                      .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                    if (objTopDrvInQueue != null)
                                                        reason = "Top Standing Drv of Job 17th Backup Plot";
                                                }





                                                if (objTopDrvInQueue == null && backupZones.BackupZone18Id != null)
                                                {


                                                    objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone18Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                      .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                    if (objTopDrvInQueue != null)
                                                        reason = "Top Standing Drv of Job 18th Backup Plot";
                                                }

                                                if (objTopDrvInQueue == null && backupZones.BackupZone19Id != null)
                                                {


                                                    objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone19Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                      .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                    if (objTopDrvInQueue != null)
                                                        reason = "Top Standing Drv of Job 19th Backup Plot";
                                                }


                                                if (objTopDrvInQueue == null && backupZones.BackupZone20Id != null)
                                                {


                                                    objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone20Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                      .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                    if (objTopDrvInQueue != null)
                                                        reason = "Top Standing Drv of Job 20th Backup Plot";
                                                }



                                            }

                                        }






                                        if (objTopDrvInQueue != null && job.AutoDespatch.ToBool())
                                        {


                                            long rtnId = 0;
                                            using (TaxiDataContext dbX = new TaxiDataContext())
                                            {
                                                rtnId = db.stp_IsJobAvailableForDriver(objTopDrvInQueue.DriverId, job.JobId).FirstOrDefault().DefaultIfEmpty().Id;

                                            }

                                            if (rtnId > 0)
                                            {

                                                OnSuccessAutoDespatchJob(job, objTopDrvInQueue.DriverInfo, ref listofErrors, reason);
                                                listofSuccessAutoDespatch.Add(new ClsAutoDespatchPlot { DriverId = objTopDrvInQueue.DriverId, JobId = job.JobId });
                                                IsUpdated = true;
                                            }

                                        }
                                        else
                                        {
                                            // Put Bidding Sub Rule 3



                                            if (job.IsBidding.ToBool() && job.bookingstatusId.ToInt() != Enums.BOOKINGSTATUS.BID && job.EnableZoneBidding.ToBool())


                                            {

                                                int biddingRadius = job.ZoneId == null ? 1000 : job.BiddingRadius.ToInt();
                                                if (biddingRadius <= 0)
                                                    biddingRadius = 10000;


                                                if (Instance.objPolicy.EnableBidding.ToBool())
                                                {






                                                    var objNearestDrvForBidding = (from a in listofJobAvailableDrvs.Where(c =>

                                                                                                    (
                                                                                                     c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE

                                                                                                  )
                                                                                                  )


                                                                                   select new
                                                                                   {
                                                                                       a,
                                                                                       Distance = new LatLng(a.Latitude, a.Longitude).DistanceMiles(new LatLng(Convert.ToDouble(job.Latitude)
                                                                                           , Convert.ToDouble(job.Longitude)))
                                                                                   }).Where(c => c.Distance <= biddingRadius);




                                                    int[] driverIds = objNearestDrvForBidding.Select(c => c.a.DriverId.ToInt()).ToArray<int>();




                                                    if (driverIds.Count() > 0)
                                                    {


                                                        if (job.ZoneId != null)
                                                        {
                                                            PutJobOnBidding(driverIds, job.JobId, job.ZoneId.ToStr() + ">>" + job.ZoneName.ToStr());

                                                        }
                                                        else
                                                        {
                                                            PutJobOnBidding(driverIds, job.JobId, " >> ");



                                                        }
                                                    }
                                                    IsUpdated = true;
                                                    using (TaxiDataContext db3 = new TaxiDataContext())
                                                        db3.stp_UpdateJobStatus(job.JobId, Enums.BOOKINGSTATUS.BID);



                                                }
                                            }

                                            else // No Driver is Availble for a Job
                                            {


                                                if (job.bookingstatusId.ToInt() == Enums.BOOKINGSTATUS.BID)
                                                {

                                                    using (TaxiDataContext db4 = new TaxiDataContext())
                                                    {
                                                        try
                                                        {
                                                            db4.ExecuteQuery<int>("update booking set pricebiddingexpirydate='" + string.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now) + "' where id=" + job.JobId);


                                                        }
                                                        catch
                                                        {


                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }



                                else if (autoDespatchType == Enums.AUTODESPATCH_TYPES.TOP_STANDING_QUEUE_NEAREST_DRIVER) // AutoDespatch Rule 2 :- Top Standing in Plot Queue with nearest driver
                                {
                                    try
                                    {



                                        var objTopDrvInQueue = listofJobAvailableDrvs.Where(c => (job.ZoneId != null && c.ZoneId == job.ZoneId) && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                             .OrderBy(c => c.PlotDateTime)
                                                                             .FirstOrDefault();



                                        // If Top Standing Available Driver exist
                                        if (objTopDrvInQueue != null && job.AutoDespatch.ToBool())
                                        {
                                            long rtnId = 0;
                                            using (TaxiDataContext dbX = new TaxiDataContext())
                                            {
                                                rtnId = db.stp_IsJobAvailableForDriver(objTopDrvInQueue.DriverId, job.JobId).FirstOrDefault().DefaultIfEmpty().Id;

                                            }


                                            if (rtnId > 0)
                                            {


                                                listofErrors.Clear();
                                                OnSuccessAutoDespatchJob(job, objTopDrvInQueue.DriverInfo, ref listofErrors, "Job Auto Despatched to the Top Standing Drv '" + objTopDrvInQueue.DriverInfo.DriverNo.ToStr() + "' ");
                                                listofSuccessAutoDespatch.Add(new ClsAutoDespatchPlot { DriverId = objTopDrvInQueue.DriverId, JobId = job.JobId });

                                                IsUpdated = true;
                                            }

                                        }
                                        else
                                        {

                                            // Top Standing Driver not exist
                                            //
                                            try
                                            {
                                                if (bookings.Count > 1)
                                                {
                                                    foreach (var bookingItem in bookings.Where(c => c.JobId != job.JobId))
                                                    {
                                                        if (listofJobAvailableDrvs.Count(c => c.ZoneId == bookingItem.ZoneId && c.StatusId != Enums.Driver_WORKINGSTATUS.SOONTOCLEAR) > 0)
                                                        {
                                                            listofJobAvailableDrvs.RemoveAll(c => c.ZoneId == bookingItem.ZoneId && c.StatusId != Enums.Driver_WORKINGSTATUS.SOONTOCLEAR);
                                                        }
                                                    }
                                                }
                                            }
                                            catch
                                            {

                                                //
                                            }



                                            //
                                            Gen_Coordinate objCoord = null;

                                            if (job.FromPostCode.ToStr().Trim().Length > 0)
                                            {
                                                objCoord = db.Gen_Coordinates.FirstOrDefault(c => c.PostCode == job.FromPostCode.ToStr().Trim().ToUpper());
                                            }


                                            var STCDriver = (from a in listofJobAvailableDrvs.Where(c => c.ZoneId == job.ZoneId &&
                                                                         c.StatusId == Enums.Driver_WORKINGSTATUS.SOONTOCLEAR && (c.IsIdle == null || c.IsIdle == false)

                                                                     )
                                                             select new
                                                             {
                                                                 a,
                                                                 Distance = objCoord != null ? new LatLng(a.Latitude, a.Longitude).DistanceMiles(new LatLng(Convert.ToDouble(objCoord.Latitude), Convert.ToDouble(objCoord.Longitude))) : 0.0

                                                             })
                                                                 //.Where(c => c.Distance < fojRadius)
                                                                 .OrderBy(c => c.Distance).FirstOrDefault();





                                            if (STCDriver != null && job.AutoDespatch.ToBool())
                                            {

                                                try
                                                {
                                                    using (TaxiDataContext db2 = new TaxiDataContext())
                                                    {

                                                        db2.CommandTimeout = 5;

                                                        string query = "update booking set ReAutoDespatchTime=getdate(), bookingstatusid=1,IsConfirmedDriver=1,driverId=" + STCDriver.a.DriverId + " where id=" + job.JobId + ";";
                                                        query += "INSERT INTO BOOKING_LOG (bookingid,[user],beforeupdate,afterupdate,updatedate)values(" + job.JobId + ",'" + "AutoDespatch" + "','" + "" + "','Auto Allocate STC Driver (" + STCDriver.a.DriverInfo.DriverNo.ToStr() + ")',getdate());";
                                                        query += "Update fleet_driverqueuelist set isidle=1 where status=1 and driverid=" + STCDriver.a.DriverId;
                                                        db2.stp_RunProcedure(query);
                                                    }
                                                    IsUpdated = true;

                                                    long autoJobId = job.JobId;
                                                    int autoDriverId = STCDriver.a.DriverId.ToInt();
                                                    bool autoIsIdle = STCDriver.a.IsIdle.ToBool();

                                                    Instance.listofJobs.Add(new clsPDA
                                                    {
                                                        JobId = autoJobId,
                                                        DriverId = autoDriverId,
                                                        MessageDateTime = DateTime.Now,
                                                        JobMessage = "success:You have allocated for Job [" + job.ZoneName.ToStr() + "]",
                                                        MessageTypeId = eMessageTypes.STC_ALLOCATED,

                                                        DriverNo = STCDriver.a.DriverInfo.DriverNo
                                                    });

                                                    listofJobAvailableDrvs.RemoveAll(C => C.DriverId == STCDriver.a.DriverId);
                                                    listofDrvs.RemoveAll(C => C.DriverId == STCDriver.a.DriverId);

                                                    //testauto
                                                    File.AppendAllText(physicalPath + "\\autoallocatestc.txt", DateTime.Now.ToStr() + " jobId : " + autoJobId + ", driverId : " + autoDriverId + ", isidle : " + autoIsIdle + Environment.NewLine);


                                                }
                                                catch (Exception ex)
                                                {
                                                    try
                                                    {
                                                        File.AppendAllText(physicalPath + "\\autoallocatestc_exception.txt", DateTime.Now.ToStr() + " jobId : " + job.JobId + ", driverId : " + STCDriver.a.DriverId.ToInt() + ", isidle : " + STCDriver.a.IsIdle.ToBool() + Environment.NewLine);

                                                        listofJobAvailableDrvs.RemoveAll(C => C.DriverId == STCDriver.a.DriverId);
                                                        listofDrvs.RemoveAll(C => C.DriverId == STCDriver.a.DriverId);

                                                        File.AppendAllText(physicalPath + "\\autoallocatestcstep2.txt", DateTime.Now.ToStr() + " jobId : " + job.JobId + ", driverId : " + STCDriver.a.DriverId.ToInt() + ", isidle : " + STCDriver.a.IsIdle.ToBool() + Environment.NewLine);

                                                    }
                                                    catch
                                                    {

                                                    }


                                                }
                                                continue;
                                            }



                                            // Check Driver in Backup Plots (Sub Rule 1)
                                            // var backupZones = zonesList.FirstOrDefault(c => c.Id == job.ZoneId).Gen_Zone_Backups;
                                            string reason = string.Empty;
                                            if (job.ZoneId != null && job.AutoDespatch.ToBool())
                                            {

                                                var backupZones = db.Gen_Zones.FirstOrDefault(c => c.Id == job.ZoneId).DefaultIfEmpty().Gen_Zone_Backups.DefaultIfEmpty();


                                                if (backupZones != null)
                                                {

                                                    if (backupZones.BackupZone1Id != null && backupZones.BackupZone1Priority.ToBool())
                                                    {

                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone1Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                         .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv '" + objTopDrvInQueue.DriverInfo.DriverNo.ToStr() + "' of Job 1st Backup Plot";
                                                    }

                                                    if (objTopDrvInQueue == null && backupZones.BackupZone2Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone2Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv '" + objTopDrvInQueue.DriverInfo.DriverNo.ToStr() + "' of Job 2nd Backup Plot";
                                                    }


                                                    if (objTopDrvInQueue == null && backupZones.BackupZone3Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone3Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv '" + objTopDrvInQueue.DriverInfo.DriverNo.ToStr() + "' of Job 3rd Backup Plot";
                                                    }

                                                    if (objTopDrvInQueue == null && backupZones.BackupZone4Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone4Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv '" + objTopDrvInQueue.DriverInfo.DriverNo.ToStr() + "' of Job 4th Backup Plot";
                                                    }



                                                    if (objTopDrvInQueue == null && backupZones.BackupZone5Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone5Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv of Job 5th Backup Plot";
                                                    }


                                                    if (objTopDrvInQueue == null && backupZones.BackupZone6Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone6Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv of Job 6th Backup Plot";
                                                    }

                                                    if (objTopDrvInQueue == null && backupZones.BackupZone7Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone7Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv of Job 7th Backup Plot";
                                                    }


                                                    if (objTopDrvInQueue == null && backupZones.BackupZone8Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone8Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv of Job 8th Backup Plot";
                                                    }


                                                    if (objTopDrvInQueue == null && backupZones.BackupZone9Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone9Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv of Job 9th Backup Plot";
                                                    }


                                                    if (objTopDrvInQueue == null && backupZones.BackupZone10Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone10Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv of Job 10th Backup Plot";
                                                    }


                                                    if (objTopDrvInQueue == null && backupZones.BackupZone11Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone11Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv of Job 11th Backup Plot";
                                                    }


                                                    if (objTopDrvInQueue == null && backupZones.BackupZone12Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone12Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv of Job 12th Backup Plot";
                                                    }


                                                    if (objTopDrvInQueue == null && backupZones.BackupZone13Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone13Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv of Job 13th Backup Plot";
                                                    }


                                                    if (objTopDrvInQueue == null && backupZones.BackupZone14Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone14Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv of Job 14th Backup Plot";
                                                    }


                                                    if (objTopDrvInQueue == null && backupZones.BackupZone15Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone15Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv of Job 15th Backup Plot";
                                                    }


                                                    if (objTopDrvInQueue == null && backupZones.BackupZone16Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone16Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv of Job 16th Backup Plot";
                                                    }


                                                    if (objTopDrvInQueue == null && backupZones.BackupZone17Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone17Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv of Job 17th Backup Plot";
                                                    }





                                                    if (objTopDrvInQueue == null && backupZones.BackupZone18Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone18Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv of Job 18th Backup Plot";
                                                    }

                                                    if (objTopDrvInQueue == null && backupZones.BackupZone19Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone19Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv of Job 19th Backup Plot";
                                                    }


                                                    if (objTopDrvInQueue == null && backupZones.BackupZone20Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone20Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv of Job 20th Backup Plot";
                                                    }




                                                }
                                            }


                                            if (objTopDrvInQueue != null && job.AutoDespatch.ToBool())
                                            {

                                                long rtnId = 0;
                                                using (TaxiDataContext dbX = new TaxiDataContext())
                                                {
                                                    rtnId = db.stp_IsJobAvailableForDriver(objTopDrvInQueue.DriverId, job.JobId).FirstOrDefault().DefaultIfEmpty().Id;

                                                }

                                                if (rtnId > 0)
                                                {

                                                    OnSuccessAutoDespatchJob(job, objTopDrvInQueue.DriverInfo, ref listofErrors, reason);
                                                    listofSuccessAutoDespatch.Add(new ClsAutoDespatchPlot { DriverId = objTopDrvInQueue.DriverId, JobId = job.JobId });

                                                    IsUpdated = true;
                                                }
                                            }
                                            else
                                            {

                                                string postcodefull = string.Empty;
                                                Gen_Coordinate objJobCoord = null;
                                                try
                                                {
                                                    postcodefull = General.GetPostCodeMatch(job.FromAddress.ToStr().ToUpper().Trim());
                                                }
                                                catch
                                                {
                                                }


                                                if (job.FromPostCode.ToStr().Trim().Length == 0 || job.FromPostCode.ToStr().Trim().Contains(" ") == false)
                                                {


                                                    string postcode = string.Empty;
                                                    try
                                                    {
                                                        postcode = General.GetPostCodeMatch(job.FromAddress.ToStr().ToUpper().Trim());
                                                    }
                                                    catch
                                                    {


                                                    }


                                                    if (postcode.ToStr().Trim().Length > 0)
                                                    {
                                                        if (job.Latitude != null)
                                                        {
                                                            objJobCoord = new Gen_Coordinate();
                                                            objJobCoord.Latitude = job.Latitude;
                                                            objJobCoord.Longitude = job.Longitude;


                                                        }

                                                    }
                                                }

                                                if (objJobCoord != null || (postcodefull.ToStr().Trim().Length > 0 && postcodefull.ToStr().Trim().Contains(" ")))
                                                {

                                                    if (objJobCoord == null)
                                                    {
                                                        if (job.Latitude != null)
                                                        {
                                                            objJobCoord = new Gen_Coordinate();
                                                            objJobCoord.Latitude = job.Latitude;
                                                            objJobCoord.Longitude = job.Longitude;


                                                        }

                                                        // objJobCoord = listofCoordinates.FirstOrDefault(c => c.PostCode == job.FromPostCode);


                                                    }



                                                    if (objJobCoord == null)
                                                    {

                                                        try
                                                        {

                                                            using (TaxiDataContext dbC = new TaxiDataContext())
                                                            {

                                                                var objClsCoord = dbC.stp_GetCoordinatesByRoadLevelData(null, postcodefull).FirstOrDefault();
                                                                //var objClsCoord = dbC.ExecuteQuery(typeof(ClsCoord), "exec stp_GetCoordFromPAF 'HA2 0DU'", job.FromPostCode.ToStr().Trim().ToUpper());

                                                                if (objClsCoord != null)
                                                                {
                                                                    objJobCoord = new Gen_Coordinate();
                                                                    objJobCoord.PostCode = postcodefull.ToStr().Trim().ToUpper();
                                                                    objJobCoord.Latitude = objClsCoord.Latitude;
                                                                    objJobCoord.Longitude = objClsCoord.Longitude;

                                                                }
                                                            }
                                                        }
                                                        catch
                                                        {


                                                        }

                                                    }
                                                }

                                                if (objJobCoord != null)
                                                {



                                                    var otherNearestDriver = (from a in listofJobAvailableDrvs.Where(c => c.StatusId == 50)
                                                                              select new
                                                                              {

                                                                                  a,
                                                                                  Distance = default(double),
                                                                              }).FirstOrDefault();




                                                    if (nearestDrvWithinRadius > 0)
                                                    {


                                                        otherNearestDriver = (from a in listofJobAvailableDrvs.Where(c => c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)

                                                                              select new
                                                                              {
                                                                                  a,
                                                                                  Distance = new LatLng(a.Latitude, a.Longitude).DistanceMiles(new LatLng(Convert.ToDouble(objJobCoord.Latitude), Convert.ToDouble(objJobCoord.Longitude)))

                                                                              }).Where(c => c.Distance < nearestDrvWithinRadius).OrderBy(c => c.Distance).FirstOrDefault();







                                                        double distance = 0.00;
                                                        if (otherNearestDriver != null)
                                                        {
                                                            distance = otherNearestDriver.Distance;

                                                            if (otherNearestDriver.Distance > 0.2)
                                                            {

                                                                var dist = GetNearestDriverRadiusOnline(otherNearestDriver.a.Latitude, otherNearestDriver.a.Longitude, objJobCoord.Latitude, objJobCoord.Longitude);


                                                                if (dist > 0)
                                                                {
                                                                    if (dist > nearestDrvWithinRadius)
                                                                    {
                                                                        otherNearestDriver = null;
                                                                    }
                                                                    else
                                                                    {
                                                                        distance = dist;

                                                                    }
                                                                }
                                                            }

                                                        }


                                                        if (otherNearestDriver != null)
                                                        {
                                                            reason = "nearest Available Drv '" + otherNearestDriver.a.DriverInfo.DriverNo.ToStr() + "' "
                                                                      + Math.Round(distance, 2) + " miles away from Pickup";
                                                        }
                                                    }





                                                    //if (otherNearestDriver == null && Instance.objPolicy.EnableFOJ.ToBool())
                                                    //{



                                                    //    try
                                                    //    {




                                                    //        if (Instance.objPolicy.AutoDespatchFOJRadius.ToDecimal() > 0)
                                                    //        {

                                                    //            if (Instance.objPolicy.FOJLimit.ToInt() > 0)
                                                    //            {
                                                    //                int?[] FOJDrivers = db.stp_GetFOJDriversWithLimit(Instance.objPolicy.FOJLimit.ToInt()).Select(c => c.DriverId).ToArray<int?>();

                                                    //                foreach (var item in FOJDrivers)
                                                    //                {
                                                    //                    listofJobAvailableDrvs.RemoveAll(c => c.DriverId == item);
                                                    //                }
                                                    //            }

                                                    //            otherNearestDriver = (from a in listofJobAvailableDrvs.Where(c =>
                                                    //                                                                  (
                                                    //                                  c.StatusId == Enums.Driver_WORKINGSTATUS.SOONTOCLEAR)

                                                    //                                 )
                                                    //                                  select new
                                                    //                                  {
                                                    //                                      a,
                                                    //                                      Distance = new LatLng(a.Latitude, a.Longitude).DistanceMiles(new LatLng(Convert.ToDouble(objJobCoord.Latitude), Convert.ToDouble(objJobCoord.Longitude)))

                                                    //                                  }).Where(c => c.Distance < fojRadius).OrderBy(c => c.Distance).FirstOrDefault();






                                                    //            double distance = 0.00;
                                                    //            if (otherNearestDriver != null && otherNearestDriver.Distance > 0.2)
                                                    //            {
                                                    //                distance = otherNearestDriver.Distance;

                                                    //                var dist = GetNearestDriverRadiusOnline(otherNearestDriver.a.Latitude, otherNearestDriver.a.Longitude, objJobCoord.Latitude, objJobCoord.Longitude);

                                                    //                if (dist > 0)
                                                    //                {
                                                    //                    if (dist > fojRadius)
                                                    //                    {
                                                    //                        otherNearestDriver = null;
                                                    //                    }
                                                    //                    else
                                                    //                    {
                                                    //                        distance = dist;

                                                    //                    }
                                                    //                }
                                                    //            }

                                                    //            if (otherNearestDriver != null && job.AutoDespatch.ToBool())
                                                    //            {

                                                    //                listofErrors.Clear();
                                                    //                OnSuccessAutoDespatchJobWithFOJ(job, otherNearestDriver.a.DriverInfo, ref listofErrors, "FOJ Job Auto Despatched to nearest STC Drv  "
                                                    //                              + Math.Round(otherNearestDriver.Distance, 2) + " miles away from Pickup", true);
                                                    //                listofSuccessAutoDespatch.Add(new ClsAutoDespatchPlot { DriverId = otherNearestDriver.a.DriverId, JobId = job.JobId });

                                                    //                IsUpdated = true;
                                                    //                continue;
                                                    //            }
                                                    //        }
                                                    //        //  }
                                                    //    }
                                                    //    catch
                                                    //    {


                                                    //    }


                                                    //}


                                                    if (otherNearestDriver != null && job.AutoDespatch.ToBool())
                                                    {

                                                        long rtnId = 0;
                                                        using (TaxiDataContext dbX = new TaxiDataContext())
                                                        {
                                                            rtnId = db.stp_IsJobAvailableForDriver(otherNearestDriver.a.DriverId, job.JobId).FirstOrDefault().DefaultIfEmpty().Id;

                                                        }

                                                        if (rtnId > 0)
                                                        {


                                                            if (reason.ToStr().Trim().Length == 0)
                                                                reason = "Job Auto Despatched " + otherNearestDriver.a.DriverInfo.DriverNo.ToStr();

                                                            listofErrors.Clear();
                                                            OnSuccessAutoDespatchJobWithFOJ(job, otherNearestDriver.a.DriverInfo, ref listofErrors, reason, false);
                                                            listofSuccessAutoDespatch.Add(new ClsAutoDespatchPlot { DriverId = otherNearestDriver.a.DriverId, JobId = job.JobId });

                                                            IsUpdated = true;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // Put Bidding Sub Rule 3



                                                        if (job.IsBidding.ToBool() && job.bookingstatusId.ToInt() != Enums.BOOKINGSTATUS.BID && job.EnableZoneBidding.ToBool())


                                                        {

                                                            int biddingRadius = job.ZoneId == null ? 1000 : job.BiddingRadius.ToInt();
                                                            if (biddingRadius <= 0)
                                                                biddingRadius = 10000;


                                                            if (Instance.objPolicy.EnableBidding.ToBool())
                                                            {






                                                                var objNearestDrvForBidding = (from a in listofJobAvailableDrvs.Where(c =>

                                                                                                                (
                                                                                                                 c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE

                                                                                                              )
                                                                                                              )


                                                                                               select new
                                                                                               {
                                                                                                   a,
                                                                                                   Distance = new LatLng(a.Latitude, a.Longitude).DistanceMiles(new LatLng(Convert.ToDouble(objJobCoord.Latitude)
                                                                                                       , Convert.ToDouble(objJobCoord.Longitude)))
                                                                                               }).Where(c => c.Distance <= biddingRadius);




                                                                int[] driverIds = objNearestDrvForBidding.Select(c => c.a.DriverId.ToInt()).ToArray<int>();




                                                                if (driverIds.Count() > 0)
                                                                {


                                                                    if (job.ZoneId != null)
                                                                    {
                                                                        PutJobOnBidding(driverIds, job.JobId, job.ZoneId.ToStr() + ">>" + job.ZoneName.ToStr());

                                                                    }
                                                                    else
                                                                    {
                                                                        PutJobOnBidding(driverIds, job.JobId, " >> ");



                                                                    }
                                                                }
                                                                IsUpdated = true;

                                                                using (TaxiDataContext db2 = new TaxiDataContext())
                                                                {
                                                                    db2.stp_UpdateJobStatus(job.JobId, Enums.BOOKINGSTATUS.BID);
                                                                }


                                                            }
                                                        }

                                                        else // No Driver is Availble for a Job
                                                        {


                                                            if (job.bookingstatusId.ToInt() == Enums.BOOKINGSTATUS.BID)
                                                            {
                                                                using (TaxiDataContext db2 = new TaxiDataContext())
                                                                {
                                                                    try
                                                                    {
                                                                        db2.ExecuteQuery<int>("update booking set pricebiddingexpirydate='" + string.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now) + "' where id=" + job.JobId);
                                                                    }
                                                                    catch
                                                                    {


                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                                // end bid


                                            }


                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        //try
                                        //{

                                        //    File.AppendAllText(Application.StartupPath + "\\autodespcatchlog.txt", DateTime.Now.ToStr() + ": " + ex.Message + Environment.NewLine);
                                        //}
                                        //catch
                                        //{





                                        //}
                                    }
                                }

                                else if (autoDespatchType == Enums.AUTODESPATCH_TYPES.NEAREST_DRIVER) // AutoDespatch Rule 3 :- NEAREST_DRIVER
                                {
                                    try
                                    {



                                        listofJobAvailableDrvs = listofJobAvailableDrvs.Where(c => c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE).ToList();





                                        string reason = string.Empty;

                                        string postcodefull = string.Empty;
                                        Gen_Coordinate objJobCoord = null;
                                        try
                                        {
                                            postcodefull = General.GetPostCodeMatch(job.FromAddress.ToStr().ToUpper().Trim());
                                        }
                                        catch
                                        {
                                        }


                                        if (job.FromPostCode.ToStr().Trim().Length == 0 || job.FromPostCode.ToStr().Trim().Contains(" ") == false)
                                        {


                                            string postcode = string.Empty;
                                            try
                                            {
                                                postcode = General.GetPostCodeMatch(job.FromAddress.ToStr().ToUpper().Trim());
                                            }
                                            catch
                                            {


                                            }


                                            if (postcode.ToStr().Trim().Length > 0)
                                            {
                                                if (job.Latitude != null)
                                                {
                                                    objJobCoord = new Gen_Coordinate();
                                                    objJobCoord.Latitude = job.Latitude;
                                                    objJobCoord.Longitude = job.Longitude;


                                                }

                                            }
                                        }

                                        if (objJobCoord != null || (postcodefull.ToStr().Trim().Length > 0 && postcodefull.ToStr().Trim().Contains(" ")))
                                        {

                                            if (objJobCoord == null)
                                            {
                                                if (job.Latitude != null)
                                                {
                                                    objJobCoord = new Gen_Coordinate();
                                                    objJobCoord.Latitude = job.Latitude;
                                                    objJobCoord.Longitude = job.Longitude;


                                                }
                                            }



                                            if (objJobCoord == null)
                                            {

                                                try
                                                {

                                                    using (TaxiDataContext dbC = new TaxiDataContext())
                                                    {

                                                        var objClsCoord = dbC.stp_GetCoordinatesByRoadLevelData(null, postcodefull).FirstOrDefault();
                                                        //var objClsCoord = dbC.ExecuteQuery(typeof(ClsCoord), "exec stp_GetCoordFromPAF 'HA2 0DU'", job.FromPostCode.ToStr().Trim().ToUpper());

                                                        if (objClsCoord != null)
                                                        {
                                                            objJobCoord = new Gen_Coordinate();
                                                            objJobCoord.PostCode = postcodefull.ToStr().Trim().ToUpper();
                                                            objJobCoord.Latitude = objClsCoord.Latitude;
                                                            objJobCoord.Longitude = objClsCoord.Longitude;

                                                        }
                                                    }
                                                }
                                                catch
                                                {


                                                }

                                            }
                                        }




                                        if (objJobCoord == null && postcodefull.Length == 0)
                                        {

                                            try
                                            {

                                                string from = job.FromAddress.ToStr().ToUpper();
                                                using (TaxiDataContext dbC = new TaxiDataContext())
                                                {
                                                    dbC.DeferredLoadingEnabled = false;

                                                    var objClsCoord = dbC.Gen_Locations.Where(c => c.Latitude != null && c.FullLocationName != null
                                                    && c.FullLocationName.EndsWith(from))
                                                        .Select(args => new { args.Latitude, args.Longitude }).FirstOrDefault();

                                                    if (objClsCoord != null)
                                                    {
                                                        objJobCoord = new Gen_Coordinate();
                                                        //objJobCoord.PostCode = postcodefull.ToStr().Trim().ToUpper();
                                                        objJobCoord.Latitude = objClsCoord.Latitude;
                                                        objJobCoord.Longitude = objClsCoord.Longitude;

                                                    }
                                                }
                                            }
                                            catch
                                            {


                                            }

                                        }


                                        if (objJobCoord != null)
                                        {



                                            var otherNearestDriver = (from a in listofJobAvailableDrvs.Where(c => c.StatusId == 50)
                                                                      select new
                                                                      {

                                                                          a,
                                                                          Distance = default(double),
                                                                      }).FirstOrDefault();




                                            if (nearestDrvWithinRadius > 0)
                                            {


                                                otherNearestDriver = (from a in listofJobAvailableDrvs.Where(c => c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)

                                                                      select new
                                                                      {
                                                                          a,
                                                                          Distance = new LatLng(a.Latitude, a.Longitude).DistanceMiles(new LatLng(Convert.ToDouble(objJobCoord.Latitude), Convert.ToDouble(objJobCoord.Longitude)))

                                                                      }).Where(c => c.Distance < nearestDrvWithinRadius).OrderBy(c => c.Distance).FirstOrDefault();







                                                double distance = 0.00;
                                                if (otherNearestDriver != null)
                                                {
                                                    distance = otherNearestDriver.Distance;

                                                    if (otherNearestDriver.Distance > 0.2)
                                                    {

                                                        var dist = GetNearestDriverRadiusOnline(otherNearestDriver.a.Latitude, otherNearestDriver.a.Longitude, objJobCoord.Latitude, objJobCoord.Longitude);


                                                        if (dist > 0)
                                                        {
                                                            if (dist > nearestDrvWithinRadius)
                                                            {
                                                                otherNearestDriver = null;
                                                            }
                                                            else
                                                            {
                                                                distance = dist;

                                                            }
                                                        }
                                                    }

                                                }


                                                if (otherNearestDriver != null)
                                                {
                                                    reason = "nearest Available Drv '" + otherNearestDriver.a.DriverInfo.DriverNo.ToStr() + "' "
                                                              + Math.Round(distance, 2) + " miles away from Pickup";
                                                }
                                            }








                                            if (otherNearestDriver != null && job.AutoDespatch.ToBool())
                                            {

                                                long rtnId = 0;
                                                using (TaxiDataContext dbX = new TaxiDataContext())
                                                {
                                                    rtnId = db.stp_IsJobAvailableForDriver(otherNearestDriver.a.DriverId, job.JobId).FirstOrDefault().DefaultIfEmpty().Id;

                                                }

                                                if (rtnId > 0)
                                                {


                                                    if (reason.ToStr().Trim().Length == 0)
                                                        reason = "Job Auto Despatched " + otherNearestDriver.a.DriverInfo.DriverNo.ToStr();

                                                    listofErrors.Clear();
                                                    OnSuccessAutoDespatchJobWithFOJ(job, otherNearestDriver.a.DriverInfo, ref listofErrors, reason, false);
                                                    listofSuccessAutoDespatch.Add(new ClsAutoDespatchPlot { DriverId = otherNearestDriver.a.DriverId, JobId = job.JobId });

                                                    IsUpdated = true;
                                                }
                                            }
                                            else
                                            {
                                                // Put Bidding Sub Rule 3



                                                if (job.IsBidding.ToBool() && job.bookingstatusId.ToInt() != Enums.BOOKINGSTATUS.BID && job.EnableZoneBidding.ToBool())
                                                {

                                                    int biddingRadius = job.ZoneId == null ? 1000 : job.BiddingRadius.ToInt();
                                                    if (biddingRadius <= 0)
                                                        biddingRadius = 10000;


                                                    if (Instance.objPolicy.EnableBidding.ToBool())
                                                    {






                                                        var objNearestDrvForBidding = (from a in listofJobAvailableDrvs


                                                                                       select new
                                                                                       {
                                                                                           a,
                                                                                           Distance = new LatLng(a.Latitude, a.Longitude).DistanceMiles(new LatLng(Convert.ToDouble(objJobCoord.Latitude)
                                                                                               , Convert.ToDouble(objJobCoord.Longitude)))
                                                                                       }).Where(c => c.Distance <= biddingRadius);




                                                        int[] driverIds = objNearestDrvForBidding.Select(c => c.a.DriverId.ToInt()).ToArray<int>();




                                                        if (driverIds.Count() > 0)
                                                        {


                                                            if (job.ZoneId != null)
                                                            {
                                                                PutJobOnBidding(driverIds, job.JobId, job.ZoneId.ToStr() + ">>" + job.ZoneName.ToStr());

                                                            }
                                                            else
                                                            {
                                                                PutJobOnBidding(driverIds, job.JobId, " >> ");



                                                            }
                                                        }
                                                        IsUpdated = true;
                                                        using (TaxiDataContext db2 = new TaxiDataContext())
                                                        {
                                                            db2.stp_UpdateJobStatus(job.JobId, Enums.BOOKINGSTATUS.BID);

                                                        }

                                                    }
                                                }

                                                else // No Driver is Availble for a Job
                                                {


                                                    if (job.bookingstatusId.ToInt() == Enums.BOOKINGSTATUS.BID)
                                                    {
                                                        using (TaxiDataContext db2 = new TaxiDataContext())
                                                        {
                                                            try
                                                            {
                                                                db2.ExecuteQuery<int>("update booking set pricebiddingexpirydate='" + string.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now) + "' where id=" + job.JobId);
                                                            }
                                                            catch
                                                            {


                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        // end bid







                                    }
                                    catch (Exception ex)
                                    {
                                        //try
                                        //{

                                        //    File.AppendAllText(Application.StartupPath + "\\autodespcatchlog.txt", DateTime.Now.ToStr() + ": " + ex.Message + Environment.NewLine);
                                        //}
                                        //catch
                                        //{





                                        //}
                                    }
                                }

                                else if (autoDespatchType == 5) // AutoDespatch Rule X2 :- Top Standing in Plot Queue with nearest driver
                                {
                                    try
                                    {


                                        if (Global.AutoDispatchSetting == null)
                                        {
                                            int modeType = Instance.objPolicy.AutoDespatchDriverCategoryPriority.ToInt();
                                            Global.AutoDispatchSetting = db.ExecuteQuery<AutoDispatchSetting>("SELECT * FROM autodispatchsettings where autodispatchmodetype=" + modeType).FirstOrDefault();

                                        }









                                        var objTopDrvInQueue = listofJobAvailableDrvs.Where(c => (job.ZoneId != null && c.ZoneId == job.ZoneId) && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                             .OrderBy(c => c.PlotDateTime)
                                                                             .FirstOrDefault();




                                        // priortise nearest

                                        string postcodefull = string.Empty;
                                        Gen_Coordinate objJobCoord = null;

                                        string reason = string.Empty;

                                        if (Global.AutoDispatchSetting != null && Global.AutoDispatchSetting.OtherData.ToStr().Trim().Length > 0)
                                        {
                                            var objBookingTypeData = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<List<ClsAutoDespatchOtherData>>(Global.AutoDispatchSetting.OtherData.ToStr().Trim()).ToList();

                                            decimal newradius = 0.00m;
                                            if (objBookingTypeData != null)
                                            {
                                                var jobBookType = objBookingTypeData.FirstOrDefault(c => c.BookingType == job.BookingTypeId.ToInt() && c.Radius > 0);


                                                if (jobBookType != null)
                                                {
                                                    newradius = jobBookType.Radius.ToDecimal();

                                                }

                                            }




                                            if (newradius > 0)
                                            {
                                                try
                                                {
                                                    postcodefull = General.GetPostCodeMatch(job.FromAddress.ToStr().ToUpper().Trim());
                                                }
                                                catch
                                                {
                                                }


                                                if (job.FromPostCode.ToStr().Trim().Length == 0 || job.FromPostCode.ToStr().Trim().Contains(" ") == false)
                                                {


                                                    string postcode = string.Empty;
                                                    try
                                                    {
                                                        postcode = General.GetPostCodeMatch(job.FromAddress.ToStr().ToUpper().Trim());
                                                    }
                                                    catch
                                                    {


                                                    }


                                                    if (postcode.ToStr().Trim().Length > 0)
                                                    {
                                                        if (job.Latitude != null)
                                                        {
                                                            objJobCoord = new Gen_Coordinate();
                                                            objJobCoord.Latitude = job.Latitude;
                                                            objJobCoord.Longitude = job.Longitude;


                                                        }

                                                    }
                                                }

                                                if (objJobCoord != null || (postcodefull.ToStr().Trim().Length > 0 && postcodefull.ToStr().Trim().Contains(" ")))
                                                {

                                                    if (objJobCoord == null)
                                                    {
                                                        if (job.Latitude != null)
                                                        {
                                                            objJobCoord = new Gen_Coordinate();
                                                            objJobCoord.Latitude = job.Latitude;
                                                            objJobCoord.Longitude = job.Longitude;


                                                        }

                                                        // objJobCoord = listofCoordinates.FirstOrDefault(c => c.PostCode == job.FromPostCode);


                                                    }



                                                    if (objJobCoord == null)
                                                    {

                                                        try
                                                        {

                                                            using (TaxiDataContext dbC = new TaxiDataContext())
                                                            {

                                                                var objClsCoord = dbC.stp_GetCoordinatesByRoadLevelData(null, postcodefull).FirstOrDefault();
                                                                //var objClsCoord = dbC.ExecuteQuery(typeof(ClsCoord), "exec stp_GetCoordFromPAF 'HA2 0DU'", job.FromPostCode.ToStr().Trim().ToUpper());

                                                                if (objClsCoord != null)
                                                                {
                                                                    objJobCoord = new Gen_Coordinate();
                                                                    objJobCoord.PostCode = postcodefull.ToStr().Trim().ToUpper();
                                                                    objJobCoord.Latitude = objClsCoord.Latitude;
                                                                    objJobCoord.Longitude = objClsCoord.Longitude;

                                                                }
                                                            }
                                                        }
                                                        catch
                                                        {


                                                        }

                                                    }
                                                }




                                                if (objJobCoord != null)
                                                {
                                                    var otherNearestDriver = (from a in listofJobAvailableDrvs.Where(c => c.StatusId == 50)
                                                                              select new
                                                                              {

                                                                                  a,
                                                                                  Distance = default(double),
                                                                              }).FirstOrDefault();






                                                    double rad = Convert.ToDouble(newradius);

                                                    otherNearestDriver = (from a in listofJobAvailableDrvs.Where(c => c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)

                                                                          select new
                                                                          {
                                                                              a,
                                                                              Distance = new LatLng(a.Latitude, a.Longitude).DistanceMiles(new LatLng(Convert.ToDouble(objJobCoord.Latitude), Convert.ToDouble(objJobCoord.Longitude)))

                                                                          }).Where(c => c.Distance < rad).OrderBy(c => c.Distance).FirstOrDefault();







                                                    double distance = 0.00;
                                                    if (otherNearestDriver != null)
                                                    {
                                                        distance = otherNearestDriver.Distance;

                                                        if (otherNearestDriver.Distance > 0.2)
                                                        {

                                                            var dist = GetNearestDriverRadiusOnline(otherNearestDriver.a.Latitude, otherNearestDriver.a.Longitude, objJobCoord.Latitude, objJobCoord.Longitude);


                                                            if (dist > 0)
                                                            {
                                                                if (dist > rad)
                                                                {
                                                                    otherNearestDriver = null;
                                                                }
                                                                else
                                                                {
                                                                    distance = dist;

                                                                }
                                                            }
                                                        }

                                                    }


                                                    if (otherNearestDriver != null)
                                                    {
                                                        reason = "nearest Available Drv '" + otherNearestDriver.a.DriverInfo.DriverNo.ToStr() + "' "
                                                                  + Math.Round(distance, 2) + " miles away from Pickup";
                                                    }




                                                    if (otherNearestDriver != null && job.AutoDespatch.ToBool())
                                                    {

                                                        long rtnId = 0;
                                                        using (TaxiDataContext dbX = new TaxiDataContext())
                                                        {
                                                            rtnId = db.stp_IsJobAvailableForDriver(otherNearestDriver.a.DriverId, job.JobId).FirstOrDefault().DefaultIfEmpty().Id;

                                                        }

                                                        if (rtnId > 0)
                                                        {


                                                            if (reason.ToStr().Trim().Length == 0)
                                                                reason = "Job Auto Despatched " + otherNearestDriver.a.DriverInfo.DriverNo.ToStr();

                                                            listofErrors.Clear();
                                                            OnSuccessAutoDespatchJobWithFOJ(job, otherNearestDriver.a.DriverInfo, ref listofErrors, reason, false);
                                                            listofSuccessAutoDespatch.Add(new ClsAutoDespatchPlot { DriverId = otherNearestDriver.a.DriverId, JobId = job.JobId });

                                                            IsUpdated = true;


                                                            continue;
                                                        }
                                                    }
                                                }



                                            }

                                        }

                                        //





                                        if (Global.AutoDispatchSetting.TopStandingInQueue.ToBool() && Global.AutoDispatchSetting.AutoDispatchModeType.ToInt() == 2)
                                        {
                                            int jobZoneId = job.ZoneId.ToInt();
                                            try
                                            {
                                                int? quiteDriverId = db.ExecuteQuery<int?>("exec stp_GetQuitePlotDriverId {0}", jobZoneId).FirstOrDefault();


                                                if (quiteDriverId != null)
                                                {
                                                    objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.DriverId == quiteDriverId).FirstOrDefault();
                                                }
                                            }
                                            catch
                                            {

                                            }
                                        }




                                        if (Global.AutoDispatchSetting.TopStandingInQueue.ToBool() == false)
                                        {
                                            objTopDrvInQueue = null;
                                        }



                                        // If Top Standing Available Driver exist
                                        if (objTopDrvInQueue != null && job.AutoDespatch.ToBool())
                                        {
                                            long rtnId = 0;
                                            using (TaxiDataContext dbX = new TaxiDataContext())
                                            {
                                                rtnId = db.stp_IsJobAvailableForDriver(objTopDrvInQueue.DriverId, job.JobId).FirstOrDefault().DefaultIfEmpty().Id;

                                            }


                                            if (rtnId > 0)
                                            {


                                                listofErrors.Clear();
                                                OnSuccessAutoDespatchJob(job, objTopDrvInQueue.DriverInfo, ref listofErrors, "Job Auto Despatched to the Top Standing Drv '" + objTopDrvInQueue.DriverInfo.DriverNo.ToStr() + "' ");
                                                listofSuccessAutoDespatch.Add(new ClsAutoDespatchPlot { DriverId = objTopDrvInQueue.DriverId, JobId = job.JobId });

                                                IsUpdated = true;

                                                if (bookingCount == 1)
                                                    AutoRefreshVar = RefreshTypes.REFRESH_DESPATCHJOB + ">>" + job.JobId + ">>" + objTopDrvInQueue.DriverInfo.DriverNo.ToStr() + ">>" + Enums.BOOKINGSTATUS.PENDING + ">>pda";

                                            }

                                        }
                                        else
                                        {

                                            // Top Standing Driver not exist
                                            //
                                            try
                                            {
                                                if (bookings.Count > 1)
                                                {
                                                    foreach (var bookingItem in bookings.Where(c => c.JobId != job.JobId))
                                                    {
                                                        if (listofJobAvailableDrvs.Count(c => c.ZoneId == bookingItem.ZoneId && c.StatusId != Enums.Driver_WORKINGSTATUS.SOONTOCLEAR) > 0)
                                                        {
                                                            listofJobAvailableDrvs.RemoveAll(c => c.ZoneId == bookingItem.ZoneId && c.StatusId != Enums.Driver_WORKINGSTATUS.SOONTOCLEAR);
                                                        }
                                                    }
                                                }
                                            }
                                            catch
                                            {

                                                //
                                            }



                                            //
                                            Gen_Coordinate objCoord = null;

                                            if (objJobCoord != null)
                                            {
                                                objCoord = objJobCoord;
                                            }
                                            else
                                            {
                                                if (job.FromPostCode.ToStr().Trim().Length > 0)
                                                {
                                                    objCoord = db.Gen_Coordinates.FirstOrDefault(c => c.PostCode == job.FromPostCode.ToStr().Trim().ToUpper());
                                                }
                                            }


                                            var STCDriver = (from a in listofJobAvailableDrvs.Where(c => c.ZoneId == job.ZoneId &&
                                                                         c.StatusId == Enums.Driver_WORKINGSTATUS.SOONTOCLEAR && (c.IsIdle == null || c.IsIdle == false)

                                                                     )
                                                             select new
                                                             {
                                                                 a,
                                                                 Distance = objCoord != null ? new LatLng(a.Latitude, a.Longitude).DistanceMiles(new LatLng(Convert.ToDouble(objCoord.Latitude), Convert.ToDouble(objCoord.Longitude))) : 0.0

                                                             })
                                                                 //.Where(c => c.Distance < fojRadius)
                                                                 .OrderBy(c => c.Distance).FirstOrDefault();




                                            if (Global.AutoDispatchSetting.AutoAllocateSTC.ToBool() == false)
                                            {
                                                STCDriver = null;
                                            }



                                            if (STCDriver != null && job.AutoDespatch.ToBool())
                                            {

                                                try
                                                {
                                                    using (TaxiDataContext db2 = new TaxiDataContext())
                                                    {

                                                        db2.CommandTimeout = 5;

                                                        string query = "update booking set ReAutoDespatchTime=getdate(), bookingstatusid=1,IsConfirmedDriver=1,driverId=" + STCDriver.a.DriverId + " where id=" + job.JobId + ";";
                                                        query += "INSERT INTO BOOKING_LOG (bookingid,[user],beforeupdate,afterupdate,updatedate)values(" + job.JobId + ",'" + "AutoDespatch" + "','" + "" + "','Auto Allocate STC Driver (" + STCDriver.a.DriverInfo.DriverNo.ToStr() + ")',getdate());";
                                                        query += "Update fleet_driverqueuelist set isidle=1 where status=1 and driverid=" + STCDriver.a.DriverId;
                                                        db2.stp_RunProcedure(query);
                                                    }
                                                    IsUpdated = true;

                                                    long autoJobId = job.JobId;
                                                    int autoDriverId = STCDriver.a.DriverId.ToInt();
                                                    bool autoIsIdle = STCDriver.a.IsIdle.ToBool();

                                                    Instance.listofJobs.Add(new clsPDA
                                                    {
                                                        JobId = autoJobId,
                                                        DriverId = autoDriverId,
                                                        MessageDateTime = DateTime.Now,
                                                        JobMessage = "success:You have allocated for Job [" + job.ZoneName.ToStr() + "]",
                                                        MessageTypeId = eMessageTypes.STC_ALLOCATED,

                                                        DriverNo = STCDriver.a.DriverInfo.DriverNo
                                                    });

                                                    listofJobAvailableDrvs.RemoveAll(C => C.DriverId == STCDriver.a.DriverId);
                                                    listofDrvs.RemoveAll(C => C.DriverId == STCDriver.a.DriverId);

                                                    //testauto
                                                    File.AppendAllText(physicalPath + "\\autoallocatestc.txt", DateTime.Now.ToStr() + " jobId : " + autoJobId + ", driverId : " + autoDriverId + ", isidle : " + autoIsIdle + Environment.NewLine);


                                                }
                                                catch (Exception ex)
                                                {
                                                    try
                                                    {
                                                        File.AppendAllText(physicalPath + "\\autoallocatestc_exception.txt", DateTime.Now.ToStr() + " jobId : " + job.JobId + ", driverId : " + STCDriver.a.DriverId.ToInt() + ", isidle : " + STCDriver.a.IsIdle.ToBool() + Environment.NewLine);

                                                        listofJobAvailableDrvs.RemoveAll(C => C.DriverId == STCDriver.a.DriverId);
                                                        listofDrvs.RemoveAll(C => C.DriverId == STCDriver.a.DriverId);

                                                        File.AppendAllText(physicalPath + "\\autoallocatestcstep2.txt", DateTime.Now.ToStr() + " jobId : " + job.JobId + ", driverId : " + STCDriver.a.DriverId.ToInt() + ", isidle : " + STCDriver.a.IsIdle.ToBool() + Environment.NewLine);

                                                    }
                                                    catch
                                                    {

                                                    }


                                                }
                                                continue;
                                            }



                                            // Check Driver in Backup Plots (Sub Rule 1)
                                            // var backupZones = zonesList.FirstOrDefault(c => c.Id == job.ZoneId).Gen_Zone_Backups;

                                            if (job.ZoneId != null && job.AutoDespatch.ToBool() && Global.AutoDispatchSetting.TopStandingInQueueBackupPlot.ToBool())
                                            {

                                                var backupZones = db.Gen_Zones.FirstOrDefault(c => c.Id == job.ZoneId).DefaultIfEmpty().Gen_Zone_Backups.DefaultIfEmpty();


                                                if (backupZones != null)
                                                {

                                                    if (backupZones.BackupZone1Id != null && backupZones.BackupZone1Priority.ToBool())
                                                    {

                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone1Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                         .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv '" + objTopDrvInQueue.DriverInfo.DriverNo.ToStr() + "' of Job 1st Backup Plot";
                                                    }

                                                    if (objTopDrvInQueue == null && backupZones.BackupZone2Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone2Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv '" + objTopDrvInQueue.DriverInfo.DriverNo.ToStr() + "' of Job 2nd Backup Plot";
                                                    }


                                                    if (objTopDrvInQueue == null && backupZones.BackupZone3Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone3Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv '" + objTopDrvInQueue.DriverInfo.DriverNo.ToStr() + "' of Job 3rd Backup Plot";
                                                    }

                                                    if (objTopDrvInQueue == null && backupZones.BackupZone4Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone4Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv '" + objTopDrvInQueue.DriverInfo.DriverNo.ToStr() + "' of Job 4th Backup Plot";
                                                    }



                                                    if (objTopDrvInQueue == null && backupZones.BackupZone5Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone5Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv of Job 5th Backup Plot";
                                                    }


                                                    if (objTopDrvInQueue == null && backupZones.BackupZone6Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone6Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv of Job 6th Backup Plot";
                                                    }

                                                    if (objTopDrvInQueue == null && backupZones.BackupZone7Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone7Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv of Job 7th Backup Plot";
                                                    }


                                                    if (objTopDrvInQueue == null && backupZones.BackupZone8Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone8Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv of Job 8th Backup Plot";
                                                    }


                                                    if (objTopDrvInQueue == null && backupZones.BackupZone9Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone9Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv of Job 9th Backup Plot";
                                                    }


                                                    if (objTopDrvInQueue == null && backupZones.BackupZone10Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone10Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv of Job 10th Backup Plot";
                                                    }


                                                    if (objTopDrvInQueue == null && backupZones.BackupZone11Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone11Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv of Job 11th Backup Plot";
                                                    }


                                                    if (objTopDrvInQueue == null && backupZones.BackupZone12Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone12Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv of Job 12th Backup Plot";
                                                    }


                                                    if (objTopDrvInQueue == null && backupZones.BackupZone13Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone13Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv of Job 13th Backup Plot";
                                                    }


                                                    if (objTopDrvInQueue == null && backupZones.BackupZone14Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone14Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv of Job 14th Backup Plot";
                                                    }


                                                    if (objTopDrvInQueue == null && backupZones.BackupZone15Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone15Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv of Job 15th Backup Plot";
                                                    }


                                                    if (objTopDrvInQueue == null && backupZones.BackupZone16Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone16Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv of Job 16th Backup Plot";
                                                    }


                                                    if (objTopDrvInQueue == null && backupZones.BackupZone17Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone17Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv of Job 17th Backup Plot";
                                                    }





                                                    if (objTopDrvInQueue == null && backupZones.BackupZone18Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone18Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv of Job 18th Backup Plot";
                                                    }

                                                    if (objTopDrvInQueue == null && backupZones.BackupZone19Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone19Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv of Job 19th Backup Plot";
                                                    }


                                                    if (objTopDrvInQueue == null && backupZones.BackupZone20Id != null)
                                                    {


                                                        objTopDrvInQueue = listofJobAvailableDrvs.Where(c => c.ZoneId == backupZones.BackupZone20Id && c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                          .OrderBy(c => c.PlotDateTime).FirstOrDefault();

                                                        if (objTopDrvInQueue != null)
                                                            reason = "Top Standing Drv of Job 20th Backup Plot";
                                                    }




                                                }
                                            }


                                            if (objTopDrvInQueue != null && job.AutoDespatch.ToBool())
                                            {

                                                long rtnId = 0;
                                                using (TaxiDataContext dbX = new TaxiDataContext())
                                                {
                                                    rtnId = db.stp_IsJobAvailableForDriver(objTopDrvInQueue.DriverId, job.JobId).FirstOrDefault().DefaultIfEmpty().Id;

                                                }

                                                if (rtnId > 0)
                                                {

                                                    OnSuccessAutoDespatchJob(job, objTopDrvInQueue.DriverInfo, ref listofErrors, reason);
                                                    listofSuccessAutoDespatch.Add(new ClsAutoDespatchPlot { DriverId = objTopDrvInQueue.DriverId, JobId = job.JobId });

                                                    IsUpdated = true;

                                                    if (bookingCount == 1)
                                                        AutoRefreshVar = RefreshTypes.REFRESH_DESPATCHJOB + ">>" + job.JobId + ">>" + objTopDrvInQueue.DriverInfo.DriverNo.ToStr() + ">>" + Enums.BOOKINGSTATUS.PENDING + ">>pda";

                                                }
                                            }
                                            else
                                            {

                                                //   string postcodefull = string.Empty;
                                                //   Gen_Coordinate objJobCoord = null;


                                                if (objJobCoord == null)
                                                {
                                                    try
                                                    {
                                                        postcodefull = General.GetPostCodeMatch(job.FromAddress.ToStr().ToUpper().Trim());
                                                    }
                                                    catch
                                                    {
                                                    }


                                                    if (job.FromPostCode.ToStr().Trim().Length == 0 || job.FromPostCode.ToStr().Trim().Contains(" ") == false)
                                                    {


                                                        string postcode = string.Empty;
                                                        try
                                                        {
                                                            postcode = General.GetPostCodeMatch(job.FromAddress.ToStr().ToUpper().Trim());
                                                        }
                                                        catch
                                                        {


                                                        }


                                                        if (postcode.ToStr().Trim().Length > 0)
                                                        {
                                                            if (job.Latitude != null)
                                                            {
                                                                objJobCoord = new Gen_Coordinate();
                                                                objJobCoord.Latitude = job.Latitude;
                                                                objJobCoord.Longitude = job.Longitude;


                                                            }

                                                        }
                                                    }

                                                    if (objJobCoord != null || (postcodefull.ToStr().Trim().Length > 0 && postcodefull.ToStr().Trim().Contains(" ")))
                                                    {

                                                        if (objJobCoord == null)
                                                        {
                                                            if (job.Latitude != null)
                                                            {
                                                                objJobCoord = new Gen_Coordinate();
                                                                objJobCoord.Latitude = job.Latitude;
                                                                objJobCoord.Longitude = job.Longitude;


                                                            }

                                                            // objJobCoord = listofCoordinates.FirstOrDefault(c => c.PostCode == job.FromPostCode);


                                                        }



                                                        if (objJobCoord == null)
                                                        {

                                                            try
                                                            {

                                                                using (TaxiDataContext dbC = new TaxiDataContext())
                                                                {

                                                                    var objClsCoord = dbC.stp_GetCoordinatesByRoadLevelData(null, postcodefull).FirstOrDefault();
                                                                    //var objClsCoord = dbC.ExecuteQuery(typeof(ClsCoord), "exec stp_GetCoordFromPAF 'HA2 0DU'", job.FromPostCode.ToStr().Trim().ToUpper());

                                                                    if (objClsCoord != null)
                                                                    {
                                                                        objJobCoord = new Gen_Coordinate();
                                                                        objJobCoord.PostCode = postcodefull.ToStr().Trim().ToUpper();
                                                                        objJobCoord.Latitude = objClsCoord.Latitude;
                                                                        objJobCoord.Longitude = objClsCoord.Longitude;

                                                                    }
                                                                }
                                                            }
                                                            catch
                                                            {


                                                            }

                                                        }
                                                    }





                                                    if (objJobCoord == null)
                                                    {

                                                        try
                                                        {

                                                            using (TaxiDataContext dbC = new TaxiDataContext())
                                                            {

                                                                var objClsCoord = dbC.stp_getCoordinatesByAddress(job.FromAddress.ToStr().ToUpper().Trim(), postcodefull).FirstOrDefault();

                                                                if (objClsCoord != null && objClsCoord.Latitude != null && objClsCoord.Latitude != 0)
                                                                {
                                                                    objJobCoord = new Gen_Coordinate();
                                                                    objJobCoord.PostCode = postcodefull.ToStr().Trim().ToUpper();
                                                                    objJobCoord.Latitude = objClsCoord.Latitude;
                                                                    objJobCoord.Longitude = objClsCoord.Longtiude;

                                                                }
                                                            }
                                                        }
                                                        catch
                                                        {


                                                        }

                                                    }


                                                }

                                                if (objJobCoord != null)
                                                {



                                                    var otherNearestDriver = (from a in listofJobAvailableDrvs.Where(c => c.StatusId == 50)
                                                                              select new
                                                                              {

                                                                                  a,
                                                                                  Distance = default(double),
                                                                              }).FirstOrDefault();




                                                    if (Global.AutoDispatchSetting.NearestDriver.ToBool() && Global.AutoDispatchSetting.NearestDriverRadius.ToDecimal() > 0)
                                                    {

                                                        double rad = Convert.ToDouble(Global.AutoDispatchSetting.NearestDriverRadius);

                                                        otherNearestDriver = (from a in listofJobAvailableDrvs.Where(c => c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)

                                                                              select new
                                                                              {
                                                                                  a,
                                                                                  Distance = new LatLng(a.Latitude, a.Longitude).DistanceMiles(new LatLng(Convert.ToDouble(objJobCoord.Latitude), Convert.ToDouble(objJobCoord.Longitude)))

                                                                              }).Where(c => c.Distance < rad).OrderBy(c => c.Distance).FirstOrDefault();







                                                        double distance = 0.00;
                                                        if (otherNearestDriver != null)
                                                        {
                                                            distance = otherNearestDriver.Distance;

                                                            if (otherNearestDriver.Distance > 0.2)
                                                            {

                                                                var dist = GetNearestDriverRadiusOnline(otherNearestDriver.a.Latitude, otherNearestDriver.a.Longitude, objJobCoord.Latitude, objJobCoord.Longitude);


                                                                if (dist > 0)
                                                                {
                                                                    if (dist > rad)
                                                                    {
                                                                        otherNearestDriver = null;
                                                                    }
                                                                    else
                                                                    {
                                                                        distance = dist;

                                                                    }
                                                                }
                                                            }

                                                        }


                                                        if (otherNearestDriver != null)
                                                        {
                                                            reason = "nearest Available Drv '" + otherNearestDriver.a.DriverInfo.DriverNo.ToStr() + "' "
                                                                      + Math.Round(distance, 2) + " miles away from Pickup";
                                                        }
                                                    }



                                                    if (otherNearestDriver != null && job.AutoDespatch.ToBool())
                                                    {

                                                        long rtnId = 0;
                                                        using (TaxiDataContext dbX = new TaxiDataContext())
                                                        {
                                                            rtnId = db.stp_IsJobAvailableForDriver(otherNearestDriver.a.DriverId, job.JobId).FirstOrDefault().DefaultIfEmpty().Id;

                                                        }

                                                        if (rtnId > 0)
                                                        {


                                                            if (reason.ToStr().Trim().Length == 0)
                                                                reason = "Job Auto Despatched " + otherNearestDriver.a.DriverInfo.DriverNo.ToStr();

                                                            listofErrors.Clear();
                                                            OnSuccessAutoDespatchJobWithFOJ(job, otherNearestDriver.a.DriverInfo, ref listofErrors, reason, false);
                                                            listofSuccessAutoDespatch.Add(new ClsAutoDespatchPlot { DriverId = otherNearestDriver.a.DriverId, JobId = job.JobId });

                                                            IsUpdated = true;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // Put Bidding Sub Rule 3



                                                        if (job.IsBidding.ToBool() && job.bookingstatusId.ToInt() != Enums.BOOKINGSTATUS.BID && job.EnableZoneBidding.ToBool() && Global.AutoDispatchSetting.EnableBid.ToBool())


                                                        {

                                                            int biddingRadius = job.ZoneId == null ? 1000 : job.BiddingRadius.ToInt();
                                                            if (biddingRadius <= 0)
                                                                biddingRadius = 10000;


                                                            if (Instance.objPolicy.EnableBidding.ToBool())
                                                            {






                                                                var objNearestDrvForBidding = (from a in listofJobAvailableDrvs.Where(c =>

                                                                                                                (
                                                                                                                 c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE

                                                                                                              )
                                                                                                              )


                                                                                               select new
                                                                                               {
                                                                                                   a,
                                                                                                   Distance = new LatLng(a.Latitude, a.Longitude).DistanceMiles(new LatLng(Convert.ToDouble(objJobCoord.Latitude)
                                                                                                       , Convert.ToDouble(objJobCoord.Longitude)))
                                                                                               }).Where(c => c.Distance <= biddingRadius);




                                                                int[] driverIds = objNearestDrvForBidding.Select(c => c.a.DriverId.ToInt()).ToArray<int>();




                                                                if (driverIds.Count() > 0)
                                                                {


                                                                    if (job.ZoneId != null)
                                                                    {
                                                                        PutJobOnBidding(driverIds, job.JobId, job.ZoneId.ToStr() + ">>" + job.ZoneName.ToStr());

                                                                    }
                                                                    else
                                                                    {
                                                                        PutJobOnBidding(driverIds, job.JobId, " >> ");



                                                                    }
                                                                }
                                                                IsUpdated = true;
                                                                using (TaxiDataContext db2 = new TaxiDataContext())
                                                                {
                                                                    db2.stp_UpdateJobStatus(job.JobId, Enums.BOOKINGSTATUS.BID);


                                                                }
                                                            }
                                                        }

                                                        else // No Driver is Availble for a Job
                                                        {


                                                            if (job.bookingstatusId.ToInt() == Enums.BOOKINGSTATUS.BID)
                                                            {
                                                                using (TaxiDataContext db2 = new TaxiDataContext())
                                                                {
                                                                    try
                                                                    {
                                                                        db2.ExecuteQuery<int>("update booking set pricebiddingexpirydate='" + string.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now) + "' where id=" + job.JobId);
                                                                    }
                                                                    catch
                                                                    {


                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                                // end bid


                                            }


                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        //try
                                        //{

                                        //    File.AppendAllText(Application.StartupPath + "\\autodespcatchlog.txt", DateTime.Now.ToStr() + ": " + ex.Message + Environment.NewLine);
                                        //}
                                        //catch
                                        //{





                                        //}
                                    }
                                }


                            }
                        }
                        else
                        {

                            foreach (var job in bookings)
                            {
                                IsUpdated= SendJobOnBid(job);
                              
                            }
                        }
                    }
                }


                if (IsUpdated)
                {
                    //doneauto
                    try
                    {

                        File.AppendAllText(AppContext.BaseDirectory + "\\doneauto.txt", DateTime.Now.ToStr() + Environment.NewLine);


                        if (AutoRefreshVar.ToStr().Length > 0)
                        {
                            //List<string> listOfConnections = new List<string>();
                            //listOfConnections = Instance.ReturnDesktopConnections();

                            //Instance.Clients.Clients(listOfConnections).cMessageToDesktop(AutoRefreshVar);

                            General.BroadCastMessage(AutoRefreshVar);


                            File.AppendAllText(AppContext.BaseDirectory + "\\doneauto_Autorefreshvar.txt", DateTime.Now.ToStr() + " : value:" + AutoRefreshVar.ToStr() + Environment.NewLine);




                        }
                        else
                        {

                            string data = "jsonrefresh active booking dashboard";
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


                                data += "|>>>|" + Newtonsoft.Json.JsonConvert.SerializeObject(query);
                            }


                            General.BroadCastMessage(data);



                            //List<string> listOfConnections = new List<string>();
                            //listOfConnections = Instance.ReturnDesktopConnections();

                            //Instance.Clients.Clients(listOfConnections).cMessageToDesktop(data);
                        }
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            File.AppendAllText(AppContext.BaseDirectory + "\\doneauto_exception.txt", DateTime.Now.ToStr() + ",exception:" + ex.Message + Environment.NewLine);
                        }
                        catch
                        {

                        }

                    }
                }


                IsPerformingAutoDespatchActivity = false;
                //
            }

            catch (Exception ex)
            {

                IsPerformingAutoDespatchActivity = false;



            }

        }


        private void OnSuccessAutoDespatchJob(stp_GetAutoDispatchBookingsResultEx job, Fleet_Driver objDrvInfo, ref List<string> listofErrors, string despatchReason)
        {


            OnDespatching(Instance.objPolicy, General.GetObject<Booking>(c => c.Id == job.JobId), objDrvInfo);


        }


        private void OnSuccessAutoDespatchJobWithFOJ(stp_GetAutoDispatchBookingsResultEx job, Fleet_Driver objDrvInfo, ref List<string> listofErrors, string despatchReason, bool isfoj)
        {
            OnDespatching(Instance.objPolicy, General.GetObject<Booking>(c => c.Id == job.JobId), objDrvInfo);




        }


        private void OnSuccessAutoDespatchJob(stp_GetAutoDispatchBookingsResult job, Fleet_Driver objDrvInfo, ref List<string> listofErrors)
        {


            OnDespatching(Instance.objPolicy, General.GetObject<Booking>(c => c.Id == job.JobId), objDrvInfo);

        }

        private void OnSuccessAutoDespatchCompulsaryJob(stp_GetAutoDispatchBookingsResult job, Fleet_Driver objDrvInfo, ref List<string> listofErrors, string despatchReason)
        {


            OnDespatchingCompulsary(Instance.objPolicy, General.GetObject<Booking>(c => c.Id == job.JobId), objDrvInfo);

        }


        public void OnDespatching(Gen_SysPolicy_Configuration objPolicy, Booking objBooking, Fleet_Driver ObjDriver)
        {




            try
            {

                if (ObjDriver != null && objBooking != null)
                {



                    string customerMobileNo = objBooking.CustomerMobileNo.Trim();
                    // For testing Purpose
                    //  customerMobileNo = "03323755646"; 
                    //
                    string customerName = objBooking.CustomerName;

                    string via = string.Join(",", objBooking.Booking_ViaLocations.Select(c => c.ViaLocValue.ToStr()).ToArray<string>());

                    if (!string.IsNullOrEmpty(via.Trim()))
                        via = "Via: " + via;

                    //    string specialReq = objBooking.SpecialRequirements.ToStr().Trim();
                    //if (!string.IsNullOrEmpty(specialReq))
                    //    specialReq = "Special Req: " + specialReq;



                    bool enablePDA = Instance.objPolicy.EnablePDA.ToBool();

                    string custNo = !string.IsNullOrEmpty(objBooking.CustomerMobileNo) ? objBooking.CustomerMobileNo : objBooking.CustomerPhoneNo;



                    // Send To Driver









                    string paymentType = objBooking.Gen_PaymentType.PaymentCategoryId == null ? objBooking.Gen_PaymentType.DefaultIfEmpty().PaymentType.ToStr()
                            : objBooking.Gen_PaymentType.Gen_PaymentCategory.CategoryName.ToStr();

                    string strDeviceRegistrationId = ObjDriver.DeviceId.ToStr();
                    string journey = "O/W";

                    //if (objBooking.JourneyTypeId.ToInt() == 2)
                    //{
                    //    journey = "Return";

                    //}
                    if (objBooking.JourneyTypeId.ToInt() == 3)
                    {
                        journey = "W/R";
                    }


                    string IsExtra = (objBooking.CompanyId != null || objBooking.FromLocTypeId == Enums.LOCATION_TYPES.AIRPORT || objBooking.ToLocTypeId == Enums.LOCATION_TYPES.AIRPORT) ? "1" : "0";
                    int i = 1;
                    string viaP = "";



                    if (objBooking.Booking_ViaLocations.Count > 0)
                    {



                        viaP = string.Join(" * ", objBooking.Booking_ViaLocations.Select(c => "(" + i++.ToStr() + ")" + c.ViaLocValue.ToStr()).ToArray<string>());
                    }


                    string mobileNo = objBooking.CustomerMobileNo.ToStr();
                    string telNo = objBooking.CustomerPhoneNo.ToStr();

                    // decimal drvPdaVersion = ObjDriver.Fleet_Driver_PDASettings.Count > 0 ? ObjDriver.Fleet_Driver_PDASettings[0].CurrentPdaVersion.ToDecimal() : 9.40m;


                    if (string.IsNullOrEmpty(mobileNo) && !string.IsNullOrEmpty(telNo))
                    {
                        mobileNo = telNo;
                    }
                    else if (!string.IsNullOrEmpty(mobileNo) && !string.IsNullOrEmpty(telNo))
                    {
                        mobileNo += "/" + telNo;
                    }


                    string pickUpPlot = "";
                    string dropOffPlot = "";
                    string companyName = string.Empty;

                    //if (drvPdaVersion < 11 && objBooking.CompanyId != null && objBooking.Gen_Company.DefaultIfEmpty().AccountTypeId.ToInt() != Enums.ACCOUNT_TYPE.CASH)
                    //    companyName = objBooking.Gen_Company.DefaultIfEmpty().CompanyName;
                    //    else
                    companyName = objBooking.Gen_Company.DefaultIfEmpty().CompanyName.ToStr();


                    //error in 13.4 => if its a plot job, then pickup point is hiding in pda.
                    //if (drvPdaVersion >9 && drvPdaVersion!=13.4m)
                    //{





                    pickUpPlot = objBooking.ZoneId != null ? "<<<" + objBooking.Gen_Zone1.DefaultIfEmpty().ZoneName.ToStr() : "";
                    dropOffPlot = objBooking.DropOffZoneId != null ? "<<<" + objBooking.Gen_Zone.DefaultIfEmpty().ZoneName.ToStr() : "";
                    //  }


                    string FOJJob = string.Empty;



                    string startJobPrefix = "JobId:";
                    //if (Instance.objPolicy.PDAJobAlertOnly.ToBool() &&  ObjDriver.Fleet_Driver_PDASettings[0].CurrentPdaVersion.ToDecimal() >= 8.3m && ObjDriver.Fleet_Driver_PDASettings[0].ShowJobAsAlert.ToBool())
                    //{
                    //    startJobPrefix = "AlertJobId:";                                   

                    //}

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




                    //half card and cash
                    string specialRequirements = objBooking.SpecialRequirements.ToStr();
                    if (objBooking.SecondaryPaymentTypeId != null && objBooking.CashFares.ToDecimal() > 0)
                    {

                        specialRequirements += " , Additional Cash Payment : " + objBooking.CashFares.ToDecimal();
                    }

                    decimal pdafares = objBooking.GetType().GetProperty(Instance.objPolicy.PDAFaresPropertyName.ToStr().Trim()).GetValue(objBooking, null).ToDecimal();

                    //  pdafares = objBooking.TotalCharges.ToDecimal();


                    //if(Instance.objPolicy.PDAFaresPropertyName.ToStr().Trim()=="FareRate")
                    //{
                    //    pdafares = pdafares + objBooking.ServiceCharges.ToDecimal() + objBooking.CongtionCharges.ToDecimal() + objBooking.MeetAndGreetCharges.ToDecimal() + objBooking.ExtraDropCharges.ToDecimal();


                    //}

                    string msg = string.Empty;


                    // string showFaresValue = objBooking.Gen_PaymentType.ShowFaresOnPDA.ToStr().Trim();


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


                    string showFares = ",\"ShowFares\":\"" + showFaresValue + "\"";
                    string showSummary = ",\"ShowSummary\":\"" + showFaresValue + "\"";
                    //   string showSummary = string.Empty;




                    string agentDetails = string.Empty;
                    string parkingandWaiting = string.Empty;
                    if (objBooking.CompanyId != null)
                    {


                        if (Instance.objPolicy.PickCommissionDeductionFromJobsTotal.ToBool())
                        {

                            agentDetails = ",\"AgentFees\":\"" + String.Format("{0:0.00}", objBooking.AgentCommission + objBooking.ServiceCharges.ToDecimal()) + "\"";
                        }
                        else
                        {
                            agentDetails = ",\"AgentFees\":\"" + String.Format("{0:0.00}", objBooking.AgentCommission) + "\"";

                        }

                        parkingandWaiting = ",\"Parking\":\"" + string.Format("{0:0.00}", objBooking.ParkingCharges) + "\",\"Waiting\":\"" + String.Format("{0:0.00}", objBooking.WaitingCharges) + "\"";

                    }
                    else
                    {

                        if (Instance.objPolicy.PickCommissionDeductionFromJobsTotal.ToBool())
                        {


                            agentDetails = ",\"AgentFees\":\"" + String.Format("{0:0.00}", objBooking.ServiceCharges.ToDecimal()) + "\"";

                        }


                        parkingandWaiting = ",\"Parking\":\"" + string.Format("{0:0.00}", objBooking.CongtionCharges) + "\",\"Waiting\":\"" + String.Format("{0:0.00}", objBooking.MeetAndGreetCharges) + "\"";
                        //


                    }





                    string fromdoorno = objBooking.FromDoorNo.ToStr().Trim();
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


                    //if (drvPdaVersion == 23.50m && fromAddress.ToStr().Trim().Contains("-"))
                    //{
                    //    fromAddress = fromAddress.Replace("-", "  ");

                    //}

                    string appendString = "";


                    try
                    {
                        appendString = ",\"ShowOnlyPlot\":\"" + "0" + "\"" +
                                ",\"BookingType\":\"" + objBooking.BookingType.BookingTypeName.ToStr() + "\"" +
                         ",\"ExtraCharges\":\"" + objBooking.ExtraDropCharges.ToDecimal() + "\"" +
                          ",\"BookingFee\":\"" + 0.00 + "\"" +
                          ",\"BgColor\":\"" + "" + "\"";

                        //if (objBooking.BookingDate.Value.AddMinutes(10) > objBooking.PickupDateTime.Value)
                        //{

                        //    appendString += ",\"priority\":\"" + "ASAP" + "\"";
                        //}

                    }
                    catch
                    {

                    }

                    if (specialRequirements.ToStr().Contains("\""))
                        specialRequirements = specialRequirements.ToStr().Replace("\"", "-").Trim();

                    msg = FOJJob + startJobPrefix + "{ \"JobId\" :\"" + objBooking.Id.ToStr() +
                                   "\", \"Pickup\":\"" + (!string.IsNullOrEmpty(objBooking.FromDoorNo) ? fromdoorno + "-" + fromAddress + pickUpPlot : fromAddress + pickUpPlot) +
                                   "\", \"Destination\":\"" + (!string.IsNullOrEmpty(objBooking.ToDoorNo) ? objBooking.ToDoorNo + "-" + toAddress + dropOffPlot : toAddress + dropOffPlot) + "\"," +
                                   "\"PickupDateTime\":\"" + string.Format("{0:dd/MM/yyyy   HH:mm}", objBooking.PickupDateTime) + "\"" +
                                   ",\"Cust\":\"" + objBooking.CustomerName + "\",\"Mob\":\"" + mobileNo + " " + "\",\"Fare\":\"" + string.Format("{0:0.00}", pdafares) + "\",\"Vehicle\":\"" + objBooking.Fleet_VehicleType.VehicleType + "\",\"Account\":\"" + companyName + " " + "\"" +
                                     ",\"Lug\":\"" + objBooking.NoofLuggages.ToInt() + "\",\"Passengers\":\"" + objBooking.NoofPassengers.ToInt() + "\",\"Journey\":\"" + journey + "\",\"Payment\":\"" + paymentType + "\",\"Special\":\"" + specialRequirements + " " + "\",\"Extra\":\"" + IsExtra + "\",\"Via\":\"" + viaP + " " + "\"" +

                                     ",\"CompanyId\":\"" + objBooking.CompanyId.ToInt() + "\",\"SubCompanyId\":\"" + objBooking.SubcompanyId.ToInt() + "\",\"QuotedPrice\":\"" + (objBooking.IsQuotedPrice.ToBool() ? "1" : "0") + "\"" +


                                     parkingandWaiting + ",\"DriverFares\":\"" + String.Format("{0:0.00}", objBooking.FareRate) + "\"" +
                                  agentDetails +
                                     ",\"Did\":\"" + ObjDriver.Id + "\",\"BabySeats\":\"" + objBooking.BabySeats.ToStr() + "\"" + showFares + showSummary + appendString + " }";





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


                    //
                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        db.stp_DespatchedJobWithLogReason(objBooking.Id, ObjDriver.Id, ObjDriver.DriverNo.ToStr(), ObjDriver.HasPDA.ToBool(), true, false, true, "Admin", Enums.BOOKINGSTATUS.PENDING, false, "");
                    }

                    requestPDA("request pda=" + objBooking.Id + "=" + ObjDriver.Id + "=" + msg + "=1=" + ObjDriver.DriverNo);


                    try
                    {
                        ////
                        listofDrvBidding.RemoveAll(c => c.JobId == objBooking.Id);
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

                    File.AppendAllText(AppContext.BaseDirectory + "\\ondespatching_exception.txt", DateTime.Now.ToStr() + ex.Message + Environment.NewLine);
                }
                catch 
                {



                }

            }




        }


        private void requestPDA(string mesg)
        {
            try
            {
               //yte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

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
                            //Instance.listofJobs.Add(new clsPDA
                            //{
                            //    DriverId = dId.ToInt(),
                            //    JobId = values[2].ToLong(),
                            //    MessageDateTime = dt,
                            //    JobMessage = values[3].ToStr().Trim(),
                            //    MessageTypeId = values[4].ToInt()
                            //});


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
                            //Instance.listofJobs.Add(new clsPDA
                            //{
                            //    DriverId = values[1].ToInt(),
                            //    JobId = values[2].ToLong(),
                            //    MessageDateTime = DateTime.Now.AddSeconds(-45),
                            //    JobMessage = values[3].ToStr().Trim(),
                            //    MessageTypeId = values[4].ToInt()
                            //});

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

                            //try
                            //{

                            //    File.AppendAllText(AppContext.BaseDirectory + "\\requestpda.txt", DateTime.Now.ToStr() + ": listofjob.count :" + Instance.listofJobs.Count + " :  ConnectionID = " + Instance.ReturnDriverConnections(values[1].ToInt()).FirstOrDefault() + Environment.NewLine);
                            //}
                            //catch (Exception ex)
                            //{



                            //}

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
                    //        Instance.Clients.Clients(listOfConnections).sendMessage(objc.JobMessage.ToStr());


                    //        Instance.listofJobs.Remove(objc);


                    //    }
                    //}
                    //else
                    // {
                    //     List<string> listOfConnections = new List<string>();
                    //     listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
                    //     Instance.Clients.Clients(listOfConnections).sendMessage(Instance.listofJobs[0].JobMessage.ToStr());


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




                    //Instance.listofJobs.Add(new clsPDA
                    //{
                    //    JobId = values[1].ToLong(),
                    //    DriverId = values[2].ToInt(),
                    //    MessageDateTime = DateTime.Now,
                    //    JobMessage = values[3].ToStr().Trim(),
                    //    MessageTypeId = values[4].ToInt(),
                    //    DriverNo = values[5].ToStr()
                    //});

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

                            //Instance.listofJobs.Add(new clsPDA
                            //{
                            //    JobId = values[1].ToLong(),
                            //    DriverId = values[2].ToInt(),
                            //    MessageDateTime = DateTime.Now,
                            //    JobMessage = values[3].ToStr().Trim(),
                            //    MessageTypeId = values[4].ToInt(),
                            //    DriverNo = values[5].ToStr()
                            //});


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

                            File.AppendAllText(physicalPath + "\\exception_servertoclient.txt", DateTime.Now.ToStr() + " FIXED: " + ex.Message + Environment.NewLine);
                        }
                        catch (Exception ex2)
                        {
                            try
                            {
                                File.AppendAllText(physicalPath + "\\exception_servertoclient.txt", DateTime.Now.ToStr() + " NOTFIXED: " + ex2.Message + Environment.NewLine);
                            }
                            catch
                            {

                            }
                        }
                    }

                    List<string> listOfConnections = new List<string>();
                    //  listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
                    // Instance.Clients.Clients(listOfConnections).despatchBooking(Instance.listofJobs[0].JobMessage.ToStr());
                    listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs.LastOrDefault(c => c.DriverId == drvId).DriverId));
                    Instance.Clients.Clients(listOfConnections).despatchBooking(Instance.listofJobs.LastOrDefault(c => c.DriverId == drvId).JobMessage.ToStr());
                }
                else if (values[4].ToInt() == eMessageTypes.CLEAREDJOB)
                {
                    try
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
                                MessageTypeId = values[4].ToInt()
                            });

                            File.AppendAllText(physicalPath + "\\exception_servertoclient.txt", DateTime.Now.ToStr() + "force clear job FIXED: " + ex.Message + Environment.NewLine);
                        }
                        catch (Exception ex2)
                        {
                            File.AppendAllText(physicalPath + "\\exception_servertoclient.txt", DateTime.Now.ToStr() + "force clear job NOTFIXED: " + ex2.Message + Environment.NewLine);
                        }
                    }

                    //List<string> listOfConnections = new List<string>();
                    //listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
                    //Instance.Clients.Clients(listOfConnections).forceClearJob(Instance.listofJobs[0].JobMessage.ToStr());
                }
                else if (values[4].ToInt() == eMessageTypes.RECALLJOB)
                {

                    try
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
                                MessageTypeId = values[4].ToInt()
                            });


                            File.AppendAllText(physicalPath + "\\exception_servertoclient.txt", DateTime.Now.ToStr() + "recover job FIXED: " + ex.Message + Environment.NewLine);
                        }
                        catch (Exception ex2)
                        {
                            File.AppendAllText(physicalPath + "\\exception_servertoclient.txt", DateTime.Now.ToStr() + "recover job NOTFIXED: " + ex2.Message + Environment.NewLine);
                        }
                    }

                    //List<string> listOfConnections = new List<string>();
                    //listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
                    //Instance.Clients.Clients(listOfConnections).forceRecoverJob(Instance.listofJobs[0].JobMessage.ToStr());
                }
                else if (values[4].ToInt() == eMessageTypes.UPDATEJOB)
                {
                    int drvId = values[1].ToInt();

                    Instance.listofJobs.Add(new clsPDA
                    {
                        DriverId = values[1].ToInt(),
                        JobId = values[2].ToLong(),
                        MessageDateTime = DateTime.Now.AddSeconds(-15),
                        JobMessage = values[3].ToStr(),
                        MessageTypeId = values[4].ToInt()
                    });

                    SocketIO.SendToSocket(values[1].ToStr(), values[3].ToStr(), "updateJob");

                    //List<string> listOfConnections = new List<string>();
                    //listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs.LastOrDefault(c => c.DriverId == drvId).DriverId));
                    //Instance.Clients.Clients(listOfConnections).updateJob(Instance.listofJobs.LastOrDefault(c => c.DriverId == drvId).JobMessage.ToStr());
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

                    SocketIO.SendToSocket(values[1].ToStr(), values[3].ToStr().Replace(">>", "=").Trim(), "authStatus");
                    //List<string> listOfConnections = new List<string>();
                    //listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
                    //Instance.Clients.Clients(listOfConnections).sendMessage(Instance.listofJobs[0].JobMessage.ToStr());
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


                    SocketIO.SendToSocket(values[1].ToStr(), values[3].ToStr(), "bidAlert");
                    //List<string> listOfConnections = new List<string>();
                    //listOfConnections = Instance.ReturnDriverConnections(values[1].ToInt());
                    //Instance.Clients.Clients(listOfConnections).bidAlert(values[3].ToStr());
                    //

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
                    Instance.Clients.Clients(listOfConnections).sendMessage(Instance.listofJobs[0].JobMessage.ToStr());
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
                    //Instance.Clients.Clients(listOfConnections).sendMessage(Instance.listofJobs[0].JobMessage.ToStr());
                    SocketIO.SendToSocket(values[1].ToStr(), values[3].ToStr(), "sendMessage");
                }
                else if (values[4].ToInt() == eMessageTypes.LOGOUTAUTHORIZATION)
                {
                    Instance.listofJobs.Add(new clsPDA
                    {
                        DriverId = values[1].ToInt(),
                        JobId = 0,
                        MessageDateTime = DateTime.Now.AddSeconds(-35),
                        JobMessage = values[3].ToStr(),
                        MessageTypeId = values[4].ToInt()
                    });

                    SocketIO.SendToSocket(values[1].ToStr(), values[3].ToStr(), "logoutAuthStatus");
                    //List<string> listOfConnections = new List<string>();
                    //listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
                    //Instance.Clients.Clients(listOfConnections).sendMessage(Instance.listofJobs[0].JobMessage.ToStr());
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

                    List<string> listOfConnections = new List<string>();
                    listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
                    Instance.Clients.Clients(listOfConnections).sendMessage(Instance.listofJobs[0].JobMessage.ToStr());
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

                    List<string> listOfConnections = new List<string>();
                    listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
                    Instance.Clients.Clients(listOfConnections).sendMessage(Instance.listofJobs[0].JobMessage.ToStr());
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



        public void OnDespatchingCompulsary(Gen_SysPolicy_Configuration objPolicy, Booking objBooking, Fleet_Driver ObjDriver)
        {




            try
            {

                if (ObjDriver != null && objBooking != null)
                {



                    string customerMobileNo = objBooking.CustomerMobileNo.Trim();
                    // For testing Purpose
                    //  customerMobileNo = "03323755646"; 
                    //
                    string customerName = objBooking.CustomerName;

                    string via = string.Join(",", objBooking.Booking_ViaLocations.Select(c => c.ViaLocValue.ToStr()).ToArray<string>());

                    if (!string.IsNullOrEmpty(via.Trim()))
                        via = "Via: " + via;

                    //    string specialReq = objBooking.SpecialRequirements.ToStr().Trim();
                    //if (!string.IsNullOrEmpty(specialReq))
                    //    specialReq = "Special Req: " + specialReq;



                    bool enablePDA = Instance.objPolicy.EnablePDA.ToBool();

                    string custNo = !string.IsNullOrEmpty(objBooking.CustomerMobileNo) ? objBooking.CustomerMobileNo : objBooking.CustomerPhoneNo;



                    // Send To Driver









                    string paymentType = objBooking.Gen_PaymentType.PaymentCategoryId == null ? objBooking.Gen_PaymentType.DefaultIfEmpty().PaymentType.ToStr()
                            : objBooking.Gen_PaymentType.Gen_PaymentCategory.CategoryName.ToStr();

                    string strDeviceRegistrationId = ObjDriver.DeviceId.ToStr();
                    string journey = "O/W";

                    //if (objBooking.JourneyTypeId.ToInt() == 2)
                    //{
                    //    journey = "Return";

                    //}
                    if (objBooking.JourneyTypeId.ToInt() == 3)
                    {
                        journey = "W/R";
                    }


                    string IsExtra = (objBooking.CompanyId != null || objBooking.FromLocTypeId == Enums.LOCATION_TYPES.AIRPORT || objBooking.ToLocTypeId == Enums.LOCATION_TYPES.AIRPORT) ? "1" : "0";
                    int i = 1;
                    string viaP = "";



                    if (objBooking.Booking_ViaLocations.Count > 0)
                    {



                        viaP = string.Join(" * ", objBooking.Booking_ViaLocations.Select(c => "(" + i++.ToStr() + ")" + c.ViaLocValue.ToStr()).ToArray<string>());
                    }


                    string mobileNo = objBooking.CustomerMobileNo.ToStr();
                    string telNo = objBooking.CustomerPhoneNo.ToStr();

                    // decimal drvPdaVersion = ObjDriver.Fleet_Driver_PDASettings.Count > 0 ? ObjDriver.Fleet_Driver_PDASettings[0].CurrentPdaVersion.ToDecimal() : 9.40m;


                    if (string.IsNullOrEmpty(mobileNo) && !string.IsNullOrEmpty(telNo))
                    {
                        mobileNo = telNo;
                    }
                    else if (!string.IsNullOrEmpty(mobileNo) && !string.IsNullOrEmpty(telNo))
                    {
                        mobileNo += "/" + telNo;
                    }


                    string pickUpPlot = "";
                    string dropOffPlot = "";
                    string companyName = string.Empty;

                    //if (drvPdaVersion < 11 && objBooking.CompanyId != null && objBooking.Gen_Company.DefaultIfEmpty().AccountTypeId.ToInt() != Enums.ACCOUNT_TYPE.CASH)
                    //    companyName = objBooking.Gen_Company.DefaultIfEmpty().CompanyName;
                    //    else
                    companyName = objBooking.Gen_Company.DefaultIfEmpty().CompanyName.ToStr();


                    //error in 13.4 => if its a plot job, then pickup point is hiding in pda.
                    //if (drvPdaVersion >9 && drvPdaVersion!=13.4m)
                    //{





                    pickUpPlot = objBooking.ZoneId != null ? "<<<" + objBooking.Gen_Zone1.DefaultIfEmpty().ZoneName.ToStr() : "";
                    dropOffPlot = objBooking.DropOffZoneId != null ? "<<<" + objBooking.Gen_Zone.DefaultIfEmpty().ZoneName.ToStr() : "";
                    //  }


                    string FOJJob = string.Empty;



                    string startJobPrefix = "JobId:";
                    //if (Instance.objPolicy.PDAJobAlertOnly.ToBool() &&  ObjDriver.Fleet_Driver_PDASettings[0].CurrentPdaVersion.ToDecimal() >= 8.3m && ObjDriver.Fleet_Driver_PDASettings[0].ShowJobAsAlert.ToBool())
                    //{
                    //    startJobPrefix = "AlertJobId:";                                   

                    //}

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




                    //half card and cash
                    string specialRequirements = objBooking.SpecialRequirements.ToStr();
                    if (objBooking.SecondaryPaymentTypeId != null && objBooking.CashFares.ToDecimal() > 0)
                    {

                        specialRequirements += " , Additional Cash Payment : " + objBooking.CashFares.ToDecimal();
                    }

                    decimal pdafares = objBooking.GetType().GetProperty(Instance.objPolicy.PDAFaresPropertyName.ToStr().Trim()).GetValue(objBooking, null).ToDecimal();

                    //  pdafares = objBooking.TotalCharges.ToDecimal();

                    string msg = string.Empty;


                    // string showFaresValue = objBooking.Gen_PaymentType.ShowFaresOnPDA.ToStr().Trim();

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


                    string showFares = ",\"ShowFares\":\"" + showFaresValue + "\"";
                    string showSummary = ",\"ShowSummary\":\"" + showFaresValue + "\"";
                    //   string showSummary = string.Empty;




                    string agentDetails = string.Empty;
                    string parkingandWaiting = string.Empty;
                    if (objBooking.CompanyId != null)
                    {


                        if (Instance.objPolicy.PickCommissionDeductionFromJobsTotal.ToBool())
                        {

                            agentDetails = ",\"AgentFees\":\"" + String.Format("{0:0.00}", objBooking.AgentCommission + objBooking.ServiceCharges.ToDecimal()) + "\"";
                        }
                        else
                        {
                            agentDetails = ",\"AgentFees\":\"" + String.Format("{0:0.00}", objBooking.AgentCommission) + "\"";

                        }

                        parkingandWaiting = ",\"Parking\":\"" + string.Format("{0:0.00}", objBooking.ParkingCharges) + "\",\"Waiting\":\"" + String.Format("{0:0.00}", objBooking.WaitingCharges) + "\"";

                    }
                    else
                    {

                        if (Instance.objPolicy.PickCommissionDeductionFromJobsTotal.ToBool())
                        {


                            agentDetails = ",\"AgentFees\":\"" + String.Format("{0:0.00}", objBooking.ServiceCharges.ToDecimal()) + "\"";

                        }


                        parkingandWaiting = ",\"Parking\":\"" + string.Format("{0:0.00}", objBooking.CongtionCharges) + "\",\"Waiting\":\"" + String.Format("{0:0.00}", objBooking.MeetAndGreetCharges) + "\"";
                        //


                    }





                    string fromdoorno = objBooking.FromDoorNo.ToStr().Trim();
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


                    //if (drvPdaVersion == 23.50m && fromAddress.ToStr().Trim().Contains("-"))
                    //{
                    //    fromAddress = fromAddress.Replace("-", "  ");

                    //}

                    string appendString = "";


                    try
                    {
                        appendString = ",\"ShowOnlyPlot\":\"" + "0" + "\"" +
                         ",\"ExtraCharges\":\"" + objBooking.ExtraDropCharges.ToDecimal() + "\"" +
                          ",\"BookingFee\":\"" + 0.00 + "\"" +
                          ",\"BgColor\":\"" + "" + "\"";

                        if (objBooking.BookingDate.Value.AddMinutes(10) > objBooking.PickupDateTime.Value)
                        {

                            appendString += ",\"priority\":\"" + "ASAP" + "\"";
                        }


                        appendString += ",\"BgColor\":\"" + "#00574B" + "\"";

                    }
                    catch
                    {

                    }

                    if (specialRequirements.ToStr().Contains("\""))
                        specialRequirements = specialRequirements.ToStr().Replace("\"", "-").Trim();

                    msg = FOJJob + startJobPrefix + "{ \"JobId\" :\"" + objBooking.Id.ToStr() +
                                   "\", \"Pickup\":\"" + (!string.IsNullOrEmpty(objBooking.FromDoorNo) ? fromdoorno + "-" + fromAddress + pickUpPlot : fromAddress + pickUpPlot) +
                                   "\", \"Destination\":\"" + (!string.IsNullOrEmpty(objBooking.ToDoorNo) ? objBooking.ToDoorNo + "-" + toAddress + dropOffPlot : toAddress + dropOffPlot) + "\"," +
                                   "\"PickupDateTime\":\"" + string.Format("{0:dd/MM/yyyy   HH:mm}", objBooking.PickupDateTime) + "\"" +
                                   ",\"Cust\":\"" + objBooking.CustomerName + "\",\"Mob\":\"" + mobileNo + " " + "\",\"Fare\":\"" + string.Format("{0:0.00}", pdafares) + "\",\"Vehicle\":\"" + objBooking.Fleet_VehicleType.VehicleType + "\",\"Account\":\"" + companyName + " " + "\"" +
                                     ",\"Lug\":\"" + objBooking.NoofLuggages.ToInt() + "\",\"Passengers\":\"" + objBooking.NoofPassengers.ToInt() + "\",\"Journey\":\"" + journey + "\",\"Payment\":\"" + paymentType + "\",\"Special\":\"" + specialRequirements + " " + "\",\"Extra\":\"" + IsExtra + "\",\"Via\":\"" + viaP + " " + "\"" +

                                     ",\"CompanyId\":\"" + objBooking.CompanyId.ToInt() + "\",\"SubCompanyId\":\"" + objBooking.SubcompanyId.ToInt() + "\",\"QuotedPrice\":\"" + (objBooking.IsQuotedPrice.ToBool() ? "1" : "0") + "\"" +


                                     parkingandWaiting + ",\"DriverFares\":\"" + String.Format("{0:0.00}", objBooking.FareRate) + "\"" +
                                  agentDetails +
                                     ",\"Did\":\"" + ObjDriver.Id + "\",\"BabySeats\":\"" + objBooking.BabySeats.ToStr() + "\"" + showFares + showSummary + appendString + " }";





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


                    //

                    (new TaxiDataContext()).stp_DespatchedJobWithLogReason(objBooking.Id, ObjDriver.Id, ObjDriver.DriverNo.ToStr(), ObjDriver.HasPDA.ToBool(), true, false, true, "Admin", Enums.BOOKINGSTATUS.PENDING, false, "");


                    requestPDA("request pda=" + objBooking.Id + "=" + ObjDriver.Id + "=" + msg + "=1=" + ObjDriver.DriverNo);

                    //  General.SendMessageToPDA("request pda=" + objBooking.Id + "=" + ObjDriver.Id + "=" + msg + "=1=" + ObjDriver.DriverNo).Result.ToBool();



                }
            }
            catch (Exception ex)
            {

            }




        }

        private bool SendJobOnBid(stp_GetAutoDispatchBookingsResultEx job)
        {
            bool isupdated = false;
            try
            {
               
                    if (Global.AutoDispatchSetting == null)
                    {
                        int modeType = Instance.objPolicy.AutoDespatchDriverCategoryPriority.ToInt();
                    using(TaxiDataContext db=new TaxiDataContext())
                        Global.AutoDispatchSetting = db.ExecuteQuery<AutoDispatchSetting>("SELECT * FROM autodispatchsettings where autodispatchmodetype=" + modeType).FirstOrDefault();

                    }
              

                if (job.IsBidding.ToBool() && job.bookingstatusId.ToInt() != Enums.BOOKINGSTATUS.BID && job.EnableZoneBidding.ToBool() && Global.AutoDispatchSetting.EnableBid.ToBool())


                {




                    if (Instance.objPolicy.EnableBidding.ToBool())
                    {

                        using (TaxiDataContext db2 = new TaxiDataContext())
                        {
                            db2.stp_UpdateJobStatus(job.JobId, Enums.BOOKINGSTATUS.BID);

                            isupdated = true;
                        }
                    }
                }
            }
            catch
            {

            }
            return isupdated;
        }


        private void PutJobOnBidding(int[] driverIds, long jobId, string plot)
        {

            Thread smsThread = new Thread(delegate ()
            {

                SendBidMessage(driverIds, jobId, plot);

            });

            smsThread.Priority = ThreadPriority.Highest;

            smsThread.Start();


        }

        private void SendBidMessage(int[] driverIds, long jobId, string plot)
        {

            try
            {


                foreach (var item in driverIds)
                {



                    requestPDA("request pda=" + item + "=" + jobId + "=" + "Bid Alert>>" + plot + "=6");




                }




            }
            catch (Exception ex)
            {


            }
        }

        private void CheckChaufferBiddingJobs()
        {

            try
            {
                try
                {
                    File.AppendAllText(physicalPath + "\\CHECKCHAUFFERBIDDINGX.txt", DateTime.Now.ToStr() + Environment.NewLine);
                }
                catch
                {

                }
                if (listofDrvBidding != null && listofDrvBidding.Count > 0)
                {
                    try
                    {
                        listofDrvBidding.RemoveAll(c => c == null);

                    }
                    catch
                    {


                    }
                    //

                    if (listofDrvBidding.Count(c => DateTime.Now >= c.ElapsedTime) > 0)
                    {

                        var list = listofDrvBidding.Where(c => DateTime.Now >= c.ElapsedTime).ToList();

                        long[] jobIds = listofDrvBidding.Select(c => c.JobId).Distinct().ToArray<long>();

                        DateTime dt = DateTime.Now.AddMinutes(-60);

                        var availJobs = (from a in jobIds
                                         join b in General.GetQueryable<Taxi_Model.Booking>(c => c.PickupDateTime >= dt &&
                                           (c.BookingStatusId == Enums.BOOKINGSTATUS.BID || c.BookingStatusId == Enums.BOOKINGSTATUS.WAITING))
                                          on a equals b.Id
                                         select new
                                         {
                                             b.Id,
                                             b.FromPostCode,
                                             b.FromAddress,
                                             b.ZoneId

                                         }).ToList();





                        if (availJobs.Count > 0)
                        {



                            listofDrvBidding.RemoveAll(c => availJobs.Find(a => a.Id == c.JobId) != null);


                            string jobPostCode = string.Empty;
                            for (int i = 0; i < availJobs.Count; i++)
                            {
                                decimal minPrice = list.Where(c => c.JobId == availJobs[i].Id).Min(c => c.DriverPrice);


                                var listofDrvs = list.Where(c => c.JobId == availJobs[i].Id && c.DriverPrice == minPrice).ToList();

                                var fastestFingerDriver = (from a in listofDrvs
                                                           join b in General.GetQueryable<Fleet_DriverQueueList>(c => c.Status == true && c.DriverWorkStatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                           on a.DriverId equals b.DriverId
                                                           select a

                                                          ).OrderBy(c => c.BiddingDateTime).FirstOrDefault();


                                if (fastestFingerDriver != null)
                                {
                                    if (Instance.listofJobs.Count(c => c.JobId == fastestFingerDriver.JobId && c.DriverId == fastestFingerDriver.DriverId) > 0)
                                        Instance.listofJobs.RemoveAll(c => c.JobId == fastestFingerDriver.JobId && c.DriverId == fastestFingerDriver.DriverId);


                                    if (Instance.listofJobs.Count(c => c.JobId == fastestFingerDriver.JobId && c.MessageTypeId == eMessageTypes.ONBIDDESPATCH && c.DriverId != fastestFingerDriver.DriverId.ToInt()) == 0)
                                    {



                                        Instance.listofJobs.Add(new clsPDA
                                        {
                                            JobId = fastestFingerDriver.JobId,
                                            DriverId = fastestFingerDriver.DriverId.ToInt(),
                                            MessageDateTime = DateTime.Now,
                                            JobMessage = fastestFingerDriver.JobMessage,
                                            MessageTypeId = eMessageTypes.ONBIDDESPATCH,
                                            DriverNo = "",
                                            Price = minPrice
                                        });
                                    }

                                }





                            }



                        }
                        else
                        {

                            if (availJobs.Count == 0)
                            {
                                listofDrvBidding.RemoveAll(c => c.JobId != 0);

                            }
                            else
                                listofDrvBidding.RemoveAll(c => availJobs.Find(a => a.Id == c.JobId) != null);


                        }
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {


                    File.AppendAllText(physicalPath + "\\checkCHAUFFERbiddingjobscatch.txt", DateTime.Now.ToStr() + ":" + ex.Message + Environment.NewLine);
                }
                catch
                {


                }

            }

        }

        List<int?> listofRemovedBiddingDrivers;
        public static List<ClsDriverBid> listofDrvBidding = null;

        private void CheckBiddingJobs()
        {

            try
            {

                if (listofDrvBidding != null && listofDrvBidding.Count > 0)
                {
                    try
                    {
                        listofDrvBidding.RemoveAll(c => c == null);

                    }
                    catch
                    {


                    }
                    //try
                    //{
                    //    File.AppendAllText(physicalPath + "\\checkbiddingjobsStart.txt", DateTime.Now + Environment.NewLine);

                    //}
                    //catch
                    //{


                    //}

                    if (listofDrvBidding.Count(c => DateTime.Now >= c.ElapsedTime) > 0)
                    {

                        //try
                        //{
                        //    File.AppendAllText(physicalPath + "\\checkbiddingjobs.txt", DateTime.Now + Environment.NewLine);

                        //}
                        //catch
                        //{


                        //}


                        var list = listofDrvBidding.Where(c => DateTime.Now >= c.ElapsedTime).ToList();
                        //
                        long[] jobIds = list.Select(c => c.JobId).Distinct().ToArray<long>();

                        DateTime dt = DateTime.Now.AddMinutes(-60);
                        //
                        var availJobs = (from a in jobIds
                                         join b in General.GetQueryable<Taxi_Model.Booking>(c => c.PickupDateTime >= dt &&
                                             (c.BookingStatusId == Enums.BOOKINGSTATUS.BID))
                                          on a equals b.Id
                                         select new
                                         {
                                             b.Id,
                                             b.FromPostCode,
                                             b.FromAddress,
                                             b.ZoneId,
                                             b.ToAddress,
                                         }).ToList();


                        //var availJobs = (from a in jobIds
                        //                 join b in list
                        //                  on a equals b.JobId
                        //                 select new
                        //                 {
                        //                     Id= b.JobId,
                        //                     b.FromPostCode,
                        //                     b.FromAddress,
                        //                     ZoneId=  b.JobZoneId

                        //                 }).ToList();


                        if (availJobs.Count > 0)
                        {
                            listofDrvBidding.RemoveAll(c => availJobs.Find(a => a.Id == c.JobId) != null);


                            string jobPostCode = string.Empty;
                            for (int i = 0; i < availJobs.Count; i++)
                            {
                                jobPostCode = availJobs[i].FromPostCode.ToStr();


                                if (jobPostCode.Contains(" ") == false)
                                {
                                    try
                                    {
                                        jobPostCode = General.GetPostCodeMatch(availJobs[i].FromAddress.ToStr().ToUpper().Trim());

                                    }
                                    catch
                                    {


                                    }
                                }



                                Gen_Coordinate objJobCoord = null;
                                if (jobPostCode.ToStr().Trim().Length == 0)
                                {
                                    try
                                    {
                                        using (TaxiDataContext db = new TaxiDataContext())
                                        {
                                            db.CommandTimeout = 3;
                                            var coordJob = db.stp_getCoordinatesByAddress(availJobs[i].FromAddress.ToStr().ToUpper().Trim(), "").ToList().FirstOrDefault();


                                            if (coordJob != null)
                                            {
                                                objJobCoord = new Gen_Coordinate();
                                                objJobCoord.Latitude = coordJob.Latitude;
                                                objJobCoord.Longitude = coordJob.Longtiude;

                                            }
                                        }
                                    }
                                    catch
                                    {


                                    }
                                }
                                else
                                {

                                    objJobCoord = General.GetObject<Gen_Coordinate>(c => c.PostCode == jobPostCode);
                                }


                                if (objJobCoord == null)
                                {
                                    using (TaxiDataContext db = new TaxiDataContext())
                                    {
                                        db.CommandTimeout = 3;
                                        var coord = db.stp_GetCoordinatesByRoadLevelData(null, jobPostCode).FirstOrDefault();

                                        if (coord != null)
                                        {
                                            objJobCoord = new Gen_Coordinate();
                                            objJobCoord.Latitude = coord.Latitude;
                                            objJobCoord.Longitude = coord.Longitude;

                                        }
                                    }


                                }

                                if (objJobCoord != null || Instance.objPolicy.BiddingType.ToInt() == Enums.BIDDING_TYPES.FASTEST_FINGER || Instance.objPolicy.BiddingType.ToInt() == Enums.BIDDING_TYPES.NEAREST_DRIVER)
                                {

                                    var listofDrvs = list.Where(c => c.JobId == availJobs[i].Id).ToList();


                                    if (Instance.objPolicy.FOJLimit.ToInt() > 0)
                                    {

                                        using (TaxiDataContext db = new TaxiDataContext())
                                        {

                                            int?[] FOJDrivers = db.stp_GetFOJDriversWithLimit(Instance.objPolicy.FOJLimit.ToInt()).Select(c => c.DriverId).ToArray<int?>();

                                            foreach (var item in FOJDrivers)
                                            {
                                                listofDrvs.RemoveAll(c => c.DriverId == item);


                                            }
                                        }
                                    }


                                    if (Instance.objPolicy.BiddingType.ToInt() == Enums.BIDDING_TYPES.NEAREST_DRIVER)
                                    {





                                        using (TaxiDataContext db = new TaxiDataContext())
                                        {
                                            db.CommandTimeout = 6;


                                            //if (listofRemovedBiddingDrivers == null)
                                            //    listofRemovedBiddingDrivers = new List<int?>();

                                            //if (listofRemovedBiddingDrivers.Count > 0)
                                            //    listofRemovedBiddingDrivers.Clear();


                                            //foreach (var item in listofDrvs)
                                            //{


                                            //    var cnt = db.Bookings.Where(c => c.BookingStatusId == Enums.BOOKINGSTATUS.PENDING
                                            //                                                    && c.DriverId == item.DriverId
                                            //                                                    && c.Id != item.JobId && c.DespatchDateTime != null
                                            //                                                    && c.DespatchDateTime.Value.AddSeconds(120) >= DateTime.Now)
                                            //                                                 .Count();


                                            //    if (cnt > 0)
                                            //    {
                                            //        listofRemovedBiddingDrivers.Add(item.DriverId);
                                            //    }

                                            //}


                                            //if (listofRemovedBiddingDrivers.Count > 0)
                                            //{

                                            //    foreach (var item in listofRemovedBiddingDrivers)
                                            //    {
                                            //        listofDrvs.RemoveAll(c => c.DriverId == item);
                                            //    }

                                            //    listofRemovedBiddingDrivers.Clear();

                                            //}


                                            var listAvails = (from a in listofDrvs
                                                              join b in db.Fleet_Driver_Locations on a.DriverId equals b.DriverId
                                                              join c in db.Fleet_DriverQueueLists on b.DriverId equals c.DriverId
                                                              where b.Latitude != 0 && c.Status == true
                                                             && (c.DriverWorkStatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)


                                                              select new
                                                              {
                                                                  a,
                                                                  Distance = objJobCoord == null ? -1 : new LatLng(Convert.ToDouble(b.Latitude), Convert.ToDouble(b.Longitude))
                                                                  .DistanceMiles(new LatLng(Convert.ToDouble(objJobCoord.Latitude), Convert.ToDouble(objJobCoord.Longitude))),
                                                                  StatusId = c.DriverWorkStatusId,
                                                                  a.BiddingDateTime,
                                                                  JobZoneName = ""
                                                              }).ToList();



                                            //var listAvails = (from a in listofDrvs
                                            //                  join b in db.Fleet_Driver_Locations on a.DriverId equals b.DriverId
                                            //                  join c in db.Fleet_DriverQueueLists on b.DriverId equals c.DriverId
                                            //                  where b.Latitude != 0 && c.Status == true
                                            //                 && (c.DriverWorkStatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE
                                            //                   || c.DriverWorkStatusId == Enums.Driver_WORKINGSTATUS.SOONTOCLEAR
                                            //                    )
                                            //                  select new
                                            //                  {
                                            //                      a,
                                            //                      Distance = new LatLng(Convert.ToDouble(b.Latitude), Convert.ToDouble(b.Longitude))
                                            //                      .DistanceMiles(new LatLng(Convert.ToDouble(objJobCoord.Latitude), Convert.ToDouble(objJobCoord.Longitude))),
                                            //                      StatusId = c.DriverWorkStatusId,
                                            //                      JobZoneName = ""
                                            //                  }).ToList();


                                            //var listnotAvail = (from a in listofDrvs
                                            //                    join c in db.Fleet_DriverQueueLists on a.DriverId equals c.DriverId
                                            //                    where (
                                            //                         c.DriverWorkStatusId == Enums.Driver_WORKINGSTATUS.NOTAVAILABLE
                                            //                        || c.DriverWorkStatusId == Enums.Driver_WORKINGSTATUS.SOONTOCLEAR)
                                            //                   && c.CurrentJobId!=null
                                            //                     && c.Status == true

                                            //                    select new
                                            //                    {
                                            //                        a,
                                            //                        Distance =  GetDistanceInMiles(c.Booking.ToPostCode, a.JobId) ,
                                            //                        StatusId = c.DriverWorkStatusId
                                            //                    }).ToList();


                                            try
                                            {
                                                if (listAvails.Count > 1)
                                                {
                                                    foreach (var item in listAvails.Where(c => c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE).OrderBy(c => c.Distance))
                                                    {
                                                        File.AppendAllText(physicalPath + "\\Logs\\Bid\\" + item.a.JobId + ".txt", string.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Now) + " , Driver :"
                                                                                    + item.a.DriverNo.ToStr() + " , Distance : " + item.Distance + Environment.NewLine);


                                                    }
                                                }

                                            }
                                            catch
                                            {

                                            }

                                            var availBiddingDriver = listAvails.Where(c => c.StatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                    .ToList().OrderBy(c => c.Distance)
                                              .Select(args => new
                                              {
                                                  args.a,
                                                  Distance = args.Distance,
                                                  StatusId = args.StatusId,
                                                  args.a.JobZoneName
                                              }).FirstOrDefault();


                                            //if (availBiddingDriver == null)
                                            //{
                                            //    availBiddingDriver = listAvails.Where(c => c.StatusId == Enums.Driver_WORKINGSTATUS.SOONTOCLEAR)
                                            //                        .ToList().OrderBy(c => c.Distance)
                                            //  .Select(args => new
                                            //  {
                                            //      args.a,
                                            //      Distance = args.Distance,
                                            //      StatusId = args.StatusId,
                                            //      args.a.JobZoneName
                                            //  }).FirstOrDefault();



                                            //}


                                            if (availBiddingDriver != null)
                                            {
                                                try
                                                {

                                                    long LJobId = availBiddingDriver.a.JobId;
                                                    int LDriverId = availBiddingDriver.a.DriverId.ToInt();
                                                    if (Instance.listofJobs.Count(c => c.JobId > 0 && c.JobId != availBiddingDriver.a.JobId && c.DriverId == availBiddingDriver.a.DriverId) > 0)
                                                    {

                                                        try
                                                        {
                                                            File.AppendAllText(physicalPath + "\\removeavailbid.txt", "oldjobid:" + LJobId + ",availcnt:" + listAvails.Count + Environment.NewLine);

                                                        }
                                                        catch
                                                        {


                                                        }


                                                        //   listAvails.RemoveAll(c => c.a.JobId == LJobId && c.a.DriverId == LDriverId);



                                                        try
                                                        {
                                                            File.AppendAllText(physicalPath + "\\removeavailbid.txt", "oldjobid:" + LJobId + ",availcnt:" + listAvails.Count + Environment.NewLine);

                                                        }
                                                        catch
                                                        {


                                                        }


                                                        availBiddingDriver = listAvails

                                                       .OrderBy(c => c.Distance)
                                                       .Select(args => new
                                                       {
                                                           args.a,
                                                           Distance = args.Distance,
                                                           StatusId = args.StatusId,
                                                           args.JobZoneName

                                                       }).FirstOrDefault();

                                                        //try
                                                        //{
                                                        //    File.AppendAllText(Application.StartupPath + "\\onmultibid.txt", "oldjobid:" + LJobId + Environment.NewLine);

                                                        //}
                                                        //catch
                                                        //{


                                                        //}


                                                        try
                                                        {
                                                            if (availBiddingDriver != null)
                                                                File.AppendAllText(physicalPath + "\\biddone.txt", string.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Now) + "jobid:" + availBiddingDriver.a.JobId + ",driverid:" + availBiddingDriver.a.DriverId + Environment.NewLine);
                                                            else
                                                                File.AppendAllText(physicalPath + "\\biddoneobjblank.txt", string.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Now) + "jobid:" + LJobId + ",driverid:" + LDriverId + Environment.NewLine);
                                                        }
                                                        catch
                                                        {


                                                        }


                                                    }
                                                    else
                                                    {

                                                        try
                                                        {
                                                            File.AppendAllText(physicalPath + "\\nojobqueueexistofdriver.txt", string.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Now) + "jobid:" + LJobId + ",driverid:" + LDriverId + Environment.NewLine);

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
                                                        File.AppendAllText(physicalPath + "\\onmultibidexception.txt", string.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Now) + "jobid:" + availBiddingDriver.a.JobId + ",driverid:" + availBiddingDriver.a.DriverId + ",exception:" + ex.Message + Environment.NewLine);

                                                    }
                                                    catch
                                                    {


                                                    }

                                                }

                                            }

                                            //


                                            if (availBiddingDriver != null)
                                            {
                                                try
                                                {
                                                    if (Instance.listofJobs.Count(c => c.JobId == availBiddingDriver.a.JobId && c.DriverId == availBiddingDriver.a.DriverId) > 0)
                                                        Instance.listofJobs.RemoveAll(c => c.JobId == availBiddingDriver.a.JobId && c.DriverId == availBiddingDriver.a.DriverId);
                                                }
                                                catch
                                                {

                                                }




                                                //uncommented this
                                                if (Instance.listofJobs.Count(c => c.JobId == availBiddingDriver.a.JobId && c.MessageTypeId == eMessageTypes.ONBIDDESPATCH && c.DriverId != availBiddingDriver.a.DriverId.ToInt()) == 0)
                                                {

                                                    long rtnId = 0;
                                                    using (TaxiDataContext dbX = new TaxiDataContext())
                                                    {
                                                        rtnId = db.stp_IsJobAvailableForDriver(-1, availBiddingDriver.a.JobId).FirstOrDefault().DefaultIfEmpty().Id;


                                                    }

                                                    if (rtnId > 0)
                                                    {

                                                        try
                                                        {
                                                            File.AppendAllText(physicalPath + "\\beforeonBidDespatch.txt", DateTime.Now + ",DRIVERID:" + availBiddingDriver.a.DriverId.ToStr() + ",JOBID:" + availBiddingDriver.a.JobId.ToStr() + Environment.NewLine);

                                                        }
                                                        catch
                                                        {


                                                        }


                                                        //if (availBiddingDriver.StatusId.ToInt() == Enums.Driver_WORKINGSTATUS.SOONTOCLEAR)
                                                        //{

                                                        //    try
                                                        //    {
                                                        //        using (TaxiDataContext db2 = new TaxiDataContext())
                                                        //        {
                                                        //            db2.CommandTimeout = 5;

                                                        //            string query = "update booking set ReAutoDespatchTime=getdate(), bookingstatusid=1,IsConfirmedDriver=1,driverId=" + availBiddingDriver.a.DriverId + " where id=" + availBiddingDriver.a.JobId + ";";
                                                        //            query += "INSERT INTO BOOKING_LOG (bookingid,[user],beforeupdate,afterupdate,updatedate)values(" + availBiddingDriver.a.JobId + ",'" + "AutoDespatch" + "','" + "" + "','Auto Allocate STC Driver (" + availBiddingDriver.a.DriverNo.ToStr() + ")',getdate());";
                                                        //            query += "Update fleet_driverqueuelist set isidle=1 where status=1 and driverid=" + availBiddingDriver.a.DriverId;
                                                        //            db2.stp_RunProcedure(query);
                                                        //        }



                                                        //        Instance.listofJobs.Add(new clsPDA
                                                        //        {
                                                        //            JobId = availBiddingDriver.a.JobId,
                                                        //            DriverId = availBiddingDriver.a.DriverId.ToInt(),
                                                        //            MessageDateTime = DateTime.Now,
                                                        //            JobMessage = "success:You have a job allocated in " + availBiddingDriver.a.JobZoneName.ToStr(),
                                                        //            MessageTypeId = eMessageTypes.STC_ALLOCATED,

                                                        //            DriverNo = availBiddingDriver.a.DriverNo
                                                        //        });
                                                        //    }
                                                        //    catch
                                                        //    {

                                                        //    }
                                                        //}
                                                        //else
                                                        //{


                                                        try
                                                        {

                                                            Instance.listofJobs.Add(new clsPDA
                                                            {
                                                                JobId = availBiddingDriver.a.JobId,
                                                                DriverId = availBiddingDriver.a.DriverId.ToInt(),
                                                                MessageDateTime = DateTime.Now,
                                                                JobMessage = availBiddingDriver.a.JobMessage,
                                                                MessageTypeId = eMessageTypes.ONBIDDESPATCH,
                                                                DriverNo = ""
                                                            });
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            try
                                                            {
                                                                Thread.Sleep(100);
                                                                Instance.listofJobs.Add(new clsPDA
                                                                {
                                                                    JobId = availBiddingDriver.a.JobId,
                                                                    DriverId = availBiddingDriver.a.DriverId.ToInt(),
                                                                    MessageDateTime = DateTime.Now,
                                                                    JobMessage = availBiddingDriver.a.JobMessage,
                                                                    MessageTypeId = eMessageTypes.ONBIDDESPATCH,
                                                                    DriverNo = ""
                                                                });



                                                                try
                                                                {
                                                                    File.AppendAllText(physicalPath + "\\beforeonBidDespatch_exception.txt", DateTime.Now + ",exception:" + ex.Message + Environment.NewLine);

                                                                }
                                                                catch
                                                                {


                                                                }
                                                            }
                                                            catch
                                                            {



                                                            }




                                                        }
                                                        //   }



                                                    }
                                                    //else
                                                    //{
                                                    //    listofJobs.Add(new clsPDA
                                                    //    {
                                                    //        JobId = availBiddingDriver.a.JobId,
                                                    //        DriverId = availBiddingDriver.a.DriverId.ToInt(),
                                                    //        MessageDateTime = DateTime.Now,
                                                    //        JobMessage = "Message>>Your Bidding request is unsuccessfull!",
                                                    //        MessageTypeId = eMessageTypes.MESSAGING,
                                                    //        DriverNo = ""
                                                    //    });


                                                    //}
                                                }
                                                else
                                                {
                                                    try
                                                    {
                                                        File.AppendAllText(physicalPath + "\\BidJobDispatchedToOtherDriver.txt", DateTime.Now + ",DRIVERID:" + availBiddingDriver.a.DriverId.ToStr() + ",JOBID:" + availBiddingDriver.a.JobId.ToStr() + Environment.NewLine);

                                                    }
                                                    catch
                                                    {


                                                    }


                                                }

                                                //message to unsuccessfull driver
                                                //foreach (var item in listofDrvs.Where(c => c.DriverId != availBiddingDriver.a.DriverId))
                                                //{
                                                //    listofJobs.Add(new clsPDA
                                                //    {
                                                //        JobId = item.JobId,
                                                //        DriverId = item.DriverId.ToInt(),
                                                //        MessageDateTime = DateTime.Now,
                                                //        JobMessage = "Message>>Your Bidding request is unsuccessfull!",
                                                //        MessageTypeId = eMessageTypes.MESSAGING,
                                                //        DriverNo = ""
                                                //    });

                                                //}


                                            }
                                            else
                                            {

                                                try
                                                {
                                                    File.AppendAllText(physicalPath + "\\BidDriverAvailableButFailed.txt", DateTime.Now + ",JobId:" + availJobs[i].Id.ToStr() + ",no driver availalbe for bid" + Environment.NewLine);

                                                    File.AppendAllText(physicalPath + "\\BidDriverAvailableButFailedLog.txt", DateTime.Now + ",JobId:" + availJobs[i].Id.ToStr() + ",other job driver have" + Instance.listofJobs.FirstOrDefault(c => c.JobId > 0 && c.JobId != availBiddingDriver.a.JobId && c.DriverId == availBiddingDriver.a.DriverId).DefaultIfEmpty().JobId.ToStr() + Environment.NewLine);


                                                }
                                                catch
                                                {


                                                }

                                            }
                                        }


                                    }
                                    else if (Instance.objPolicy.BiddingType.ToInt() == Enums.BIDDING_TYPES.LONGEST_WAITING_QUEUE)
                                    {
                                        var longestWaitingDriver = (from a in listofDrvs
                                                                    join b in General.GetQueryable<Fleet_DriverQueueList>(c => c.Status == true && c.DriverWorkStatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                    on a.DriverId equals b.DriverId
                                                                    select new
                                                                    {
                                                                        a,
                                                                        b.WaitSinceOn

                                                                    }
                                                                  ).OrderByDescending(c => c.WaitSinceOn).FirstOrDefault();


                                        if (longestWaitingDriver != null)
                                        {
                                            if (Instance.listofJobs.Count(c => c.JobId == longestWaitingDriver.a.JobId && c.DriverId == longestWaitingDriver.a.DriverId) > 0)
                                                Instance.listofJobs.RemoveAll(c => c.JobId == longestWaitingDriver.a.JobId && c.DriverId == longestWaitingDriver.a.DriverId);



                                            if (Instance.listofJobs.Count(c => c.JobId == longestWaitingDriver.a.JobId && c.MessageTypeId == eMessageTypes.ONBIDDESPATCH && c.DriverId != longestWaitingDriver.a.DriverId.ToInt()) == 0)
                                            {
                                                Instance.listofJobs.Add(new clsPDA
                                                {
                                                    JobId = longestWaitingDriver.a.JobId,
                                                    DriverId = longestWaitingDriver.a.DriverId.ToInt(),
                                                    MessageDateTime = DateTime.Now,
                                                    JobMessage = longestWaitingDriver.a.JobMessage,
                                                    MessageTypeId = eMessageTypes.ONBIDDESPATCH,
                                                    DriverNo = ""
                                                });
                                            }

                                        }




                                    }
                                    else if (Instance.objPolicy.BiddingType.ToInt() == Enums.BIDDING_TYPES.FASTEST_FINGER)
                                    {
                                        var fastestFingerDriver = (from a in listofDrvs
                                                                   join b in General.GetQueryable<Fleet_DriverQueueList>(c => c.Status == true && c.DriverWorkStatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE)
                                                                   on a.DriverId equals b.DriverId
                                                                   select a

                                                                  ).OrderBy(c => c.BiddingDateTime).FirstOrDefault();


                                        if (fastestFingerDriver != null)
                                        {
                                            if (Instance.listofJobs.Count(c => c.JobId == fastestFingerDriver.JobId && c.DriverId == fastestFingerDriver.DriverId) > 0)
                                                Instance.listofJobs.RemoveAll(c => c.JobId == fastestFingerDriver.JobId && c.DriverId == fastestFingerDriver.DriverId);


                                            if (Instance.listofJobs.Count(c => c.JobId == fastestFingerDriver.JobId && c.MessageTypeId == eMessageTypes.ONBIDDESPATCH && c.DriverId != fastestFingerDriver.DriverId.ToInt()) == 0)
                                            {



                                                Instance.listofJobs.Add(new clsPDA
                                                {
                                                    JobId = fastestFingerDriver.JobId,
                                                    DriverId = fastestFingerDriver.DriverId.ToInt(),
                                                    MessageDateTime = DateTime.Now,
                                                    JobMessage = fastestFingerDriver.JobMessage,
                                                    MessageTypeId = eMessageTypes.ONBIDDESPATCH,
                                                    DriverNo = ""
                                                });
                                            }

                                        }


                                    }

                                }


                            }

                        }
                        else
                        {

                            if (availJobs.Count == 0)
                            {
                                listofDrvBidding.RemoveAll(c => c.JobId != 0);

                            }
                            else
                                listofDrvBidding.RemoveAll(c => availJobs.Find(a => a.Id == c.JobId) != null);


                        }
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {


                    File.AppendAllText(physicalPath + "\\checkbiddingjobs_exception.txt", DateTime.Now.ToStr() + ":" + ex.Message + Environment.NewLine);
                }
                catch
                {


                }

            }

        }

        #endregion
    }
}