using DotNetCoords;
using SignalRHub.WebApiClasses;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Taxi_BLL;
using Taxi_Model;


using Utils;
using System.Text;
using System.Threading;
using System.Configuration;
using System.Dynamic;
using System.Xml.Linq;
using System.Data.SqlClient;
using static SignalRHub.DriverAppController;
using System.Threading.Tasks;
using CabTreasureWebApi.Models.HereForwardGeocode;
using System.Collections;

namespace SignalRHub.Controllers
{

    public class WebApiController : Controller
    {

        public static void CallGetDashboardData()
        {
            SocketIO.SendToSocket("", "GetDashboardData", "GetDashboardData", "WebApp");

            //
        }


        public static void CallGetDashboardDriversData()
        {
            SocketIO.SendToSocket("", "GetDashboardDriversData", "GetDashboardDriversData", "WebApp");


        }


        public static void BroadcastToWebControllers(string message)
        {
            SocketIO.SendToSocket("", message, "ReceiveData", "WebApp");


        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("UpdateEscort")]
        public JsonResult UpdateEscort(WebApiClasses.RequestWebApi obj)
        {
            try
            {
                General.WriteLog("UpdateEscort", "json: " + new JavaScriptSerializer().Serialize(obj));
            }
            catch
            {

            }

            ResponseWebApi response = new ResponseWebApi();

            try
            {
                using (TaxiDataContext db = new TaxiDataContext())
                {
                    long bookingId = obj.bookingInfo.Id.ToLong();
                    // Get escort id as nullable int
                    int? escortId = obj.bookingInfo.EscortId.ToIntorNull();

                    // Inline SQL update (EscortId is a foreign key, can be null)
                    string sql = escortId == null || escortId == 0
                        ? $"UPDATE Booking SET EscortId = null WHERE Id = {bookingId}"
                        : $"UPDATE Booking SET EscortId = {escortId} WHERE Id = {bookingId}";

                    db.ExecuteCommand(sql);

                    response.HasError = false;
                    response.Message = "Escort updated successfully";
                    response.Data = bookingId;
                }
            }
            catch (Exception ex)
            {
                response.HasError = true;
                response.Message = ex.Message;

                General.WriteLog("UpdateEscort", "Exception: " + ex.Message);
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }


        #region Phase#1




        public JsonResult LoginUser(WebApiClasses.RequestWebApi obj)
        {
            try
            {
                //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\LoginUser.txt", DateTime.Now + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
                General.WriteLog("LoginUser", "json: " + new JavaScriptSerializer().Serialize(obj));
            }
            catch
            {

            }
            //

            ResponseWebApi response = new ResponseWebApi();
            bool IsAdmin = false;
            try
            {
                using (TaxiDataContext db = new TaxiDataContext())
                {


                    if (General.VerifyLicense(db.Gen_SysPolicy_Configurations.Select(c => c.DefaultClientId).FirstOrDefault()) == false)
                    {

                        response.HasError = true;
                        response.Message = "Your License is expired.";
                    }
                    else
                    {

                        var objUser = db.UM_Users.Where(c => c.IsActive == true && c.UserName.ToLower() == obj.UserName.ToLower().Trim() && c.Passwrd.ToLower() == obj.Password.ToLower().Trim())
                             .Select(args => new { args.Id, args.SubcompanyId, args.ShowAllBookings, args.ShowAllDrivers, args.SecurityGroupId, args.Email, args.ShowBookingFilter }).FirstOrDefault();



                        if (objUser != null)
                        {
                            //List<SignalRHub.Classes.AppSetting> AppSettings = new List<SignalRHub.Classes.AppSetting>();


                            Global.EnsureRequiredAppSettings();
                            var AppSettings = db.ExecuteQuery<AppSetting>(@"SELECT SetKey, SetVal, description FROM AppSettings").ToList();

                            var ShowETASetting = AppSettings.FirstOrDefault(a => a.SetKey == "ShowETA");
                            var ShowCompleteJobSetting = AppSettings.FirstOrDefault(a => a.SetKey == "ShowCompleteJob");
                            var EnableBookingChargesSetting = AppSettings.FirstOrDefault(a => a.SetKey == "EnableBookingCharges");
                            var BookingPaymentSetting = AppSettings.FirstOrDefault(a => a.SetKey == "BookingPayment");
                            var ShowMapBydefaultOndashboard = AppSettings.FirstOrDefault(a => a.SetKey == "ShowMapBydefaultOndashboard");

                            //bool showCommandLine = false;
                            // showCommandLine= db.UM_SecurityGroup_Permissions.Where(c => c.SecurityGroupId == objUser.SecurityGroupId && c.UM_FormFunction.UM_Function.FunctionName == "SHOW COMMAND LINE").Count() > 0;
                            //showCommandLine = true;

                            if (objUser.SecurityGroupId == 2)
                            {
                                IsAdmin = false;
                            }
                            else
                            {
                                IsAdmin = true;
                            }
                            var EnablePartialCloudX = "false";
                            try
                            {
                                EnablePartialCloudX = AppSettings.FirstOrDefault(a => a.SetKey == "EnablePartialCloudX").SetVal.ToStr();
                            }
                            catch
                            {
                            }

                            if (EnablePartialCloudX == "true")
                            {
                                IsAdmin = false;
                            }
                            int sessionId = db.stp_ControlerLogins(objUser.Id, null, null, Environment.MachineName).FirstOrDefault().Id.ToInt();
                            string baseAddress = string.Empty;
                            string companyName = string.Empty;
                            try
                            {
                                var aa = db.Gen_SubCompanies.Where(c => c.Id == objUser.SubcompanyId).Select(c => new { c.Address, c.CompanyName }).FirstOrDefault();

                                baseAddress = aa.Address.ToStr();
                                companyName = aa.CompanyName.ToStr();
                            }
                            catch
                            {

                            }


                            if (HubProcessor.Instance.objPolicy == null)
                                HubProcessor.Instance.objPolicy = General.GetObject<Gen_SysPolicy_Configuration>(c => c.SysPolicyId == 1);




                            var airports = db.Gen_Locations.Where(c => c.LocationTypeId == Enums.LOCATION_TYPES.AIRPORT && c.PostCode != "").Select(args =>
                            new { args.PostCode, BackgroundColor = args.BackgroundColor == null ? -5374161 : args.BackgroundColor, TextColor = (args.TextColor == null) ? -16777216 : args.TextColor }).Distinct().ToList();
                            bool ShowMultiBooking = true;


                            var sysSettings = new Dictionary<string, object>();
                            foreach (var setting in AppSettings)
                            {
                                var key = setting.SetKey.Replace(" ", "").Replace("-", "").Replace(".", "");
                                sysSettings[key] = setting.SetVal;
                            }


                            // Add extra values
                            sysSettings["DefaultVehicleTypeId"] = HubProcessor.Instance.objPolicy.DefaultVehicleTypeId;
                            sysSettings["ApplyAccBgColorOnRow"] = HubProcessor.Instance.objPolicy.ApplyAccBgColorOnRow;
                            sysSettings["DisablePopupNotifications"] = HubProcessor.Instance.objPolicy.DisablePopupNotifications;
                            sysSettings["EnableFOJ"] = HubProcessor.Instance.objPolicy.EnableFOJ;
                            sysSettings["EnableQuotation"] = HubProcessor.Instance.objPolicy.EnableQuotation;
                            sysSettings["TrackDriverType"] = HubProcessor.Instance.objPolicy.TrackDriverType;
                            sysSettings["AutoCalculateFares"] = HubProcessor.Instance.objPolicy.AutoCalculateFares;
                            sysSettings["BaseAddress"] = baseAddress;
                            sysSettings["airports"] = airports;
                            sysSettings["EnableAutoDespatch"] = HubProcessor.Instance.objPolicy.EnableAutoDespatch;
                            sysSettings["EnableBidding"] = HubProcessor.Instance.objPolicy.EnableBidding;
                            sysSettings["AutoModeType"] = HubProcessor.Instance.objPolicy.AutoDespatchDriverCategoryPriority;
                            sysSettings["TransferBooking"] = IsAdmin;

                            var EnableFilterSubCompanyId = "false";
                            try
                            {
                                EnableFilterSubCompanyId = AppSettings.FirstOrDefault(a => a.SetKey == "EnableFilterSubCompanyId").SetVal.ToStr();
                            }
                            catch
                            {
                            }
                            if (EnableFilterSubCompanyId == "true")
                            {


                                var FilterSubCompanyId = 0;
                                var TransferBooking = false;
                                var ShowAllBookings = false;
                                var ShowBookingFilter = false;
                                if (objUser.SecurityGroupId == 1)
                                {
                                    FilterSubCompanyId = objUser.SubcompanyId.ToInt();
                                    ShowAllBookings = false;
                                    ShowBookingFilter = false;
                                    TransferBooking = false;
                                }
                                else
                                {
                                    ShowAllBookings = objUser.ShowAllBookings.ToBool();
                                    ShowBookingFilter = objUser.ShowBookingFilter.ToBool();
                                    TransferBooking = ShowAllBookings;
                                }

                                sysSettings["FilterSubCompanyId"] = FilterSubCompanyId;
                                sysSettings["ShowAllBookings"] = ShowAllBookings;
                                sysSettings["ShowBookingFilter"] = ShowBookingFilter;
                                sysSettings["TransferBooking"] = TransferBooking;
                            }
                            var rights = db.UM_SecurityGroup_Permissions.Where(c => c.SecurityGroupId == objUser.SecurityGroupId);

                            var ListofUserRights = (from a in rights
                                                    select new
                                                    {
                                                        formFunctionId = a.FormFunctionId,
                                                        formId = a.UM_FormFunction.FormId,
                                                        formName = a.UM_FormFunction.UM_Form.FormName,
                                                        formTitle = a.UM_FormFunction.UM_Form.FormTitle,
                                                        functionId = a.UM_FormFunction.UM_Function.FunctionName,
                                                        moduleId = a.UM_FormFunction.UM_Form.ModuleId,
                                                        moduleName = a.UM_FormFunction.UM_Form.UM_Module.ModuleName,
                                                        formType = a.UM_FormFunction.UM_Form.FormType

                                                    }).ToList();

                            var bookingcolumns = db.UM_Form_UserDefinedSettings.Where(c => c.FormId == 20 && c.IsVisible == true)
                                 .Select(args => new { args.GridColumnName, args.HeaderText, args.IsVisible, args.GridColWidth, args.FormTab, args.GridColMoveTo })

                                .OrderBy(c => c.GridColMoveTo).ToList();



                            //

                            response.Data = new
                            {
                                CompanyName = companyName,
                                SocketUrl = ConfigurationManager.AppSettings["socketurl"],
                                objUser.Email,
                                SessionId = sessionId,
                                objUser.Id,
                                objUser.SubcompanyId,
                                BookingColumns = bookingcolumns,
                                SysSettings = sysSettings,
                                IsAdmin = IsAdmin,
                                ShowETA = ShowETASetting.SetVal,
                                ShowCompleteJob = ShowCompleteJobSetting.SetVal,
                                ListofUserRights = ListofUserRights,
                                EnableBookingCharges = EnableBookingChargesSetting.SetVal,
                                BookingPayment = BookingPaymentSetting.SetVal,
                                ShowMapBydefaultOndashboard = ShowMapBydefaultOndashboard.SetVal

                            };




                        }
                        else
                        {
                            response.HasError = true;
                            response.Message = "Invalid Login Details";
                        }



                    }

                }
            }
            catch (Exception ex)
            {
                try
                {
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\LoginUser.txt", DateTime.Now + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
                    General.WriteLog("LoginUser", "Exception: " + ex.Message);

                }
                catch
                {

                }
            }


            return Json(response, JsonRequestBehavior.AllowGet);

        }


        //[System.Web.Http.HttpGet]
        //[System.Web.Http.HttpPost]
        //[System.Web.Http.Route("LoginUser")]
        //public JsonResult LoginUser(WebApiClasses.RequestWebApi obj)
        //{
        //    try
        //    {
        //        System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\LoginUser.txt", DateTime.Now + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);

        //    }
        //    catch
        //    {

        //    }
        //    //

        //    ResponseWebApi response = new ResponseWebApi();
        //    bool IsAdmin = false;
        //    try
        //    {
        //        using (TaxiDataContext db = new TaxiDataContext())
        //        {
        //            var objUser = db.UM_Users.Where(c => c.IsActive == true && c.UserName.ToLower() == obj.UserName.ToLower().Trim() && c.Passwrd.ToLower() == obj.Password.ToLower().Trim())
        //                 .Select(args => new { args.Id, args.SubcompanyId, args.ShowAllBookings, args.ShowAllDrivers, args.SecurityGroupId, args.Email }).FirstOrDefault();



        //            if (objUser != null)
        //            {

        //                bool showCommandLine = false;
        //                // showCommandLine= db.UM_SecurityGroup_Permissions.Where(c => c.SecurityGroupId == objUser.SecurityGroupId && c.UM_FormFunction.UM_Function.FunctionName == "SHOW COMMAND LINE").Count() > 0;
        //                showCommandLine = true;
        //                if (objUser.SecurityGroupId == 2)
        //                {
        //                    IsAdmin = false;
        //                }
        //                else
        //                {
        //                    IsAdmin = false;
        //                }

        //                int sessionId = db.stp_ControlerLogins(objUser.Id, null, null, Environment.MachineName).FirstOrDefault().Id.ToInt();
        //                string baseAddress = string.Empty;
        //                string companyName = string.Empty;
        //                try
        //                {
        //                    var aa = db.Gen_SubCompanies.Where(c => c.Id == objUser.SubcompanyId).Select(c => new { c.Address, c.CompanyName }).FirstOrDefault();

        //                    baseAddress = aa.Address.ToStr();
        //                    companyName = aa.CompanyName.ToStr();
        //                }
        //                catch
        //                {

        //                }


        //                if (HubProcessor.Instance.objPolicy == null)
        //                   HubProcessor.Instance.objPolicy = General.GetObject<Gen_SysPolicy_Configuration>(c => c.SysPolicyId == 1);




        //                var airports = db.Gen_Locations.Where(c => c.LocationTypeId == Enums.LOCATION_TYPES.AIRPORT && c.PostCode != "").Select(args =>
        //                new { args.PostCode, BackgroundColor = args.BackgroundColor == null ? -5374161 : args.BackgroundColor, TextColor = (args.TextColor == null) ? -16777216 : args.TextColor }).Distinct().ToList();

        //                var objSettings = new
        //                {
        //                    HubProcessor.Instance.objPolicy.DefaultVehicleTypeId,
        //                    HubProcessor.Instance.objPolicy.ApplyAccBgColorOnRow
        //                    ,
        //                    HubProcessor.Instance.objPolicy.DisablePopupNotifications,
        //                    HubProcessor.Instance.objPolicy.EnableFOJ,
        //                    HubProcessor.Instance.objPolicy.EnableQuotation
        //                    ,
        //                    HubProcessor.Instance.objPolicy.TrackDriverType,
        //                    HubProcessor.Instance.objPolicy.AutoCalculateFares,
        //                    BaseAddress = baseAddress,
        //                    showCommandLine,
        //                    HubProcessor.Instance.objPolicy.EnableAutoDespatch,
        //                    HubProcessor.Instance.objPolicy.EnableBidding,
        //                    AutoModeType = HubProcessor.Instance.objPolicy.AutoDespatchDriverCategoryPriority,
        //                    airports,
        //                    BookingSortBy = "Lead",
        //                    Currency = "£",

        //                };



        //                var bookingcolumns = db.UM_Form_UserDefinedSettings.Where(c => c.FormId == 20 && c.IsVisible == true)
        //                     .Select(args => new { args.GridColumnName, args.HeaderText, args.IsVisible, args.GridColWidth, args.FormTab, args.GridColMoveTo })

        //                    .OrderBy(c => c.GridColMoveTo).ToList();



        //                //

        //                response.Data = new { CompanyName = companyName, SocketUrl = ConfigurationManager.AppSettings["socketurl"], objUser.Email, SessionId = sessionId, objUser.Id, objUser.SubcompanyId, BookingColumns = bookingcolumns, SysSettings = objSettings, IsAdmin = IsAdmin,ShowETA=true,ShowCompleteJob=true };




        //            }
        //            else
        //            {
        //                response.HasError = true;
        //                response.Message = "Invalid Login Details";
        //            }

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        try
        //        {
        //            System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\LoginUser.txt", DateTime.Now + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);

        //        }
        //        catch
        //        {

        //        }
        //    }


        //    return Json(response, JsonRequestBehavior.AllowGet);

        //}


        public ClsDashboardModel SelectDashboardDrivers(TaxiDataContext db, ref ClsDashboardModel datab, int subCompanyId = 0)
        {

            ClsDashboardModel data = datab;



            if (data == null)
                data = new ClsDashboardModel();
            try
            {

                //data.listofdrivers = db.stp_GetDashboardDrivers(0).ToList();
                data.listofdrivers = db.ExecuteQuery<DashboardDriver>("exec stp_GetDashboardDrivers {0}", subCompanyId).ToList();

                data.objDriverCount = new DriverCounts();


                data.objDriverCount.totalDrivers = data.listofdrivers.Count;
                data.objDriverCount.totalAvailable = data.listofdrivers.Count(c => c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.AVAILABLE);
                data.objDriverCount.totalBreak = data.listofdrivers.Count(c => c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.ONBREAK);
                data.objDriverCount.totalBusy = data.listofdrivers.Count(c => c.driverworkstatusid != Enums.Driver_WORKINGSTATUS.AVAILABLE && c.driverworkstatusid != Enums.Driver_WORKINGSTATUS.ONBREAK);
                data.objDriverCount.totalOnArrived = data.listofdrivers.Count(c => c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.ARRIVED);
                data.objDriverCount.totalOnRoute = data.listofdrivers.Count(c => c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.ONROUTE);
                data.objDriverCount.totalPOB = data.listofdrivers.Count(c => c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.NOTAVAILABLE);
                data.objDriverCount.totalSTC = data.listofdrivers.Count(c => c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.SOONTOCLEAR);
                data.objDriverCount.totalWaiting = data.listofdrivers.Count(c => c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.AVAILABLE || c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.ONBREAK || c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.FOJ || c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.SINBIN);
            }
            catch
            {

            }

            return data;
        }

        private ClsDashboardModel SelectDashboardBookingsList(TaxiDataContext db, ref ClsDashboardModel datab, int subCompanyId = 0)
        {

            ClsDashboardModel data = datab;

            if (data == null)
                data = new ClsDashboardModel();
            try
            {




                DateTime? dt = DateTime.Now.ToDateorNull();
                DateTime? lastSevenDays = dt.Value.AddDays(-7);
                DateTime recentDays = dt.Value.AddDays(-1);
                DateTime dtNow = DateTime.Now;
                DateTime prebookingdays = dt.Value.AddDays(HubProcessor.Instance.objPolicy.HourControllerReport.ToInt()).ToDate();





                data.listofbookings = db.ExecuteQuery<stp_GetBookingsDataResult>("exec stp_getbookingsdata {0},{1},{2},{3}", recentDays, prebookingdays, subCompanyId, HubProcessor.Instance.objPolicy.DaysInTodayBooking.ToInt()).ToList();
                //      data.listofdrivers = db.stp_GetDashboardDrivers(1).ToList();


                data.listoftodaybookings = data.listofbookings.Where(a =>
                (a.PickupDateTemp >= recentDays && a.PickupDateTemp.Value.Date <= dt.Value.AddDays(0))
                            &&

                            (a.StatusId == Enums.BOOKINGSTATUS.WAITING || a.StatusId == Enums.BOOKINGSTATUS.PENDING || a.StatusId == Enums.BOOKINGSTATUS.NOTACCEPTED || a.StatusId == Enums.BOOKINGSTATUS.REJECTED
                               || a.StatusId == Enums.BOOKINGSTATUS.NOSHOW || a.StatusId == Enums.BOOKINGSTATUS.ONHOLD || a.StatusId == Enums.BOOKINGSTATUS.BID
                                || a.StatusId == Enums.BOOKINGSTATUS.PENDING_START || a.StatusId == Enums.BOOKINGSTATUS.FOJ))
                                .OrderBy(c => c.Lead).ToList();



                data.listofprebookings = data.listofbookings.Where(a => a.PickupDateTemp.Value.Date > dt
                                      && (a.StatusId == Enums.BOOKINGSTATUS.WAITING || a.StatusId == Enums.BOOKINGSTATUS.PENDING
                                      || a.StatusId == Enums.BOOKINGSTATUS.ONHOLD || a.StatusId == Enums.BOOKINGSTATUS.PENDING_START
                                      || a.StatusId == Enums.BOOKINGSTATUS.REJECTED || a.StatusId == Enums.BOOKINGSTATUS.NOTACCEPTED
                                      || a.StatusId == Enums.BOOKINGSTATUS.NOSHOW))
                                            .OrderBy(c => c.PickupDateTemp).ToList();

                //data.listofincompletebookings = data.listofbookings.Where(a => a.PickupDateTemp.Value.Date < dt
                //                     && (a.StatusId != Enums.BOOKINGSTATUS.DISPATCHED || a.StatusId == Enums.BOOKINGSTATUS.CANCELLED))
                //                           .OrderBy(c => c.PickupDateTemp).ToList();


                data.listofrecentbookings = data.listofbookings.Where(c => c.StatusId == Enums.BOOKINGSTATUS.ONROUTE || c.StatusId == Enums.BOOKINGSTATUS.ARRIVED
                                                          || c.StatusId == Enums.BOOKINGSTATUS.POB || c.StatusId == Enums.BOOKINGSTATUS.STC
                                                          || c.StatusId == Enums.BOOKINGSTATUS.FOJ).OrderBy(c => c.PickupDateTemp).ToList();

                List<clsBookingscount> objCnt = null;
                DateTime from = DateTime.Now.ToDate();
                DateTime till = DateTime.Now.AddDays(1).ToDate();

                objCnt = db.ExecuteQuery<clsBookingscount>
                    ("exec stp_getbookingsdatacountbystatus {0},{1},{2},{3}", from, till, subCompanyId, 0).ToList();

                data.objBookingCount = new BookingCounts();

                data.objDriverCount = new DriverCounts();


                if (objCnt != null)
                {

                    data.objBookingCount.totalToday = data.listoftodaybookings.Count;
                    data.objBookingCount.totalPre = data.listofprebookings.Count;
                    data.objBookingCount.totalRecent = data.listofrecentbookings.Count;
                    data.objBookingCount.totalCancelled = objCnt.Where(c => c.bookingstatusid == Enums.BOOKINGSTATUS.CANCELLED).FirstOrDefault().DefaultIfEmpty().count;
                    data.objBookingCount.totalNoPickup = objCnt.Where(c => c.bookingstatusid == Enums.BOOKINGSTATUS.NOPICKUP).FirstOrDefault().DefaultIfEmpty().count; ;
                    data.objBookingCount.totalCompleted = objCnt.Where(c => c.bookingstatusid == Enums.BOOKINGSTATUS.DISPATCHED).FirstOrDefault().DefaultIfEmpty().count;
                    data.objBookingCount.totalOnline = objCnt.Where(c => c.bookingstatusid == Enums.BOOKINGSTATUS.WAITING_WEBBOOKING).FirstOrDefault().DefaultIfEmpty().count;
                    var data1 = db.ExecuteQuery<ClsBookingListData>("exec stp_GetBookingsListData {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}", false, true, false, false, false, false, false, false, false, "", (DateTime?)DateTime.Today, 0, "", 1).ToList();
                    data.objBookingCount.totalInCompleted = data1.Where(a =>
                        a.PickupDate.HasValue &&
                        a.PickupDate.Value.Date < dt.Value.Date &&
                        (a.StatusId == Enums.BOOKINGSTATUS.WAITING)
                    ).Count();

                    data.objBookingCount.totalQuotation = (from a in db.Bookings
                                                           join bt in db.BookingTypes on a.BookingTypeId equals bt.Id
                                                           join b in db.Gen_PaymentTypes on a.PaymentTypeId equals b.Id
                                                           join c in db.Gen_Companies on a.CompanyId equals c.Id into table2
                                                           from c in table2.DefaultIfEmpty()
                                                           join v in db.Fleet_VehicleTypes on a.VehicleTypeId equals v.Id
                                                           where a.PickupDateTime >= recentDays
                                                                 && a.IsQuotation == true
                                                                 && a.BookingStatusId == Enums.BOOKINGSTATUS.WAITING
                                                           select a).Count();
                    //data.objBookingCount.totalInCompleted = db.ExecuteQuery<ClsBookingListData>("exec stp_GetIncompleteBookingListData {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}", false, true, false, false, false, false, false, false, false, lastSevenDays, dt, 0, "", 100).Count();
                }

            }
            catch
            {


            }


            return data;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetDashboardData")]
        public JsonResult GetDashboardData(WebApiClasses.RequestWebApi obj)
        {
            //

            try
            {
                //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\GetDashboardData.txt", DateTime.Now + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
                General.WriteLog("GetDashboardData", "json: " + new JavaScriptSerializer().Serialize(obj));

            }
            catch
            {

            }

            if (HubProcessor.Instance.objPolicy == null)
                Global.LoadDataList(true);

            ResponseWebApi response = new ResponseWebApi();

            try
            {
                ClsDashboardModel data = new ClsDashboardModel();

                using (TaxiDataContext db = new TaxiDataContext())
                {



                    //DateTime? dt = DateTime.Now.ToDateorNull();
                    //DateTime recentDays = dt.Value.AddDays(-1);
                    //DateTime dtNow = DateTime.Now;
                    //DateTime prebookingdays = dt.Value.AddDays(HubProcessor.Instance.objPolicy.HourControllerReport.ToInt()).ToDate();



                    //data.listofbookings = db.ExecuteQuery<stp_GetBookingsDataResult>("exec stp_getbookingsdata {0},{1},{2},{3}", recentDays, prebookingdays, 0, HubProcessor.Instance.objPolicy.DaysInTodayBooking.ToInt()).ToList();



                    //data.listoftodaybookings = data.listofbookings.Where(a =>
                    //(a.PickupDateTemp >= recentDays && a.PickupDateTemp.Value.Date <= dt.Value.AddDays(0))
                    //            &&

                    //            (a.StatusId == Enums.BOOKINGSTATUS.WAITING || a.StatusId == Enums.BOOKINGSTATUS.PENDING || a.StatusId == Enums.BOOKINGSTATUS.NOTACCEPTED || a.StatusId == Enums.BOOKINGSTATUS.REJECTED
                    //               || a.StatusId == Enums.BOOKINGSTATUS.NOSHOW || a.StatusId == Enums.BOOKINGSTATUS.ONHOLD || a.StatusId == Enums.BOOKINGSTATUS.BID
                    //                || a.StatusId == Enums.BOOKINGSTATUS.PENDING_START || a.StatusId == Enums.BOOKINGSTATUS.FOJ))
                    //                .OrderBy(c => c.Lead).ToList();



                    //data.listofprebookings = data.listofbookings.Where(a => a.PickupDateTemp.Value.Date > dt
                    //                      && (a.StatusId == Enums.BOOKINGSTATUS.WAITING || a.StatusId == Enums.BOOKINGSTATUS.PENDING
                    //                      || a.StatusId == Enums.BOOKINGSTATUS.ONHOLD || a.StatusId == Enums.BOOKINGSTATUS.PENDING_START
                    //                      || a.StatusId == Enums.BOOKINGSTATUS.REJECTED || a.StatusId == Enums.BOOKINGSTATUS.NOTACCEPTED
                    //                      || a.StatusId == Enums.BOOKINGSTATUS.NOSHOW))
                    //                            .OrderBy(c => c.PickupDateTemp).ToList();



                    //data.listofrecentbookings = data.listofbookings.Where(c => c.StatusId == Enums.BOOKINGSTATUS.ONROUTE || c.StatusId == Enums.BOOKINGSTATUS.ARRIVED
                    //                                          || c.StatusId == Enums.BOOKINGSTATUS.POB || c.StatusId == Enums.BOOKINGSTATUS.STC
                    //                                          || c.StatusId == Enums.BOOKINGSTATUS.FOJ
                    //                                          || c.StatusId == Enums.BOOKINGSTATUS.PENDING_START).OrderBy(c => c.PickupDateTemp).ToList();






                    //List<clsBookingscount> objCnt = null;
                    //DateTime from = DateTime.Now.ToDate();
                    //DateTime till = DateTime.Now.AddDays(1).ToDate();

                    //objCnt = db.ExecuteQuery<clsBookingscount>
                    //    ("exec stp_getbookingsdatacountbystatus {0},{1},{2},{3}", from, till, 0, 0).ToList();

                    //data.objBookingCount = new BookingCounts();





                    //if (objCnt != null)
                    //{

                    //    data.objBookingCount.totalToday = data.listoftodaybookings.Count;
                    //    data.objBookingCount.totalPre = data.listofprebookings.Count;
                    //    data.objBookingCount.totalRecent = data.listofrecentbookings.Count;
                    //    data.objBookingCount.totalCancelled = objCnt.Where(c => c.bookingstatusid == Enums.BOOKINGSTATUS.CANCELLED).FirstOrDefault().DefaultIfEmpty().count;
                    //    data.objBookingCount.totalNoPickup = objCnt.Where(c => c.bookingstatusid == Enums.BOOKINGSTATUS.NOPICKUP).FirstOrDefault().DefaultIfEmpty().count; ;
                    //    data.objBookingCount.totalCompleted = objCnt.Where(c => c.bookingstatusid == Enums.BOOKINGSTATUS.DISPATCHED).FirstOrDefault().DefaultIfEmpty().count;
                    //}





                    data = SelectDashboardBookingsList(db, ref data, obj.objUserInfo != null ? obj.objUserInfo.SubcompanyId.ToInt() : 0);



                    SelectDashboardDrivers(db, ref data, obj.objUserInfo != null ? obj.objUserInfo.SubcompanyId.ToInt() : 0);


                    //data.listofdrivers = db.stp_GetDashboardDrivers(0).ToList();

                    //data.objDriverCount = new DriverCounts();


                    //data.objDriverCount.totalDrivers = data.listofdrivers.Count;
                    //data.objDriverCount.totalAvailable = data.listofdrivers.Count(c => c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.AVAILABLE);
                    //data.objDriverCount.totalBreak = data.listofdrivers.Count(c => c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.ONBREAK);
                    //data.objDriverCount.totalBusy = data.listofdrivers.Count(c => c.driverworkstatusid!= Enums.Driver_WORKINGSTATUS.AVAILABLE && c.driverworkstatusid != Enums.Driver_WORKINGSTATUS.ONBREAK);
                    //data.objDriverCount.totalOnArrived = data.listofdrivers.Count(c => c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.ARRIVED);
                    //data.objDriverCount.totalOnRoute = data.listofdrivers.Count(c => c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.ONROUTE);
                    //data.objDriverCount.totalPOB = data.listofdrivers.Count(c => c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.NOTAVAILABLE);
                    //data.objDriverCount.totalSTC = data.listofdrivers.Count(c => c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.SOONTOCLEAR);
                    //data.objDriverCount.totalWaiting = data.listofdrivers.Count(c => c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.AVAILABLE || c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.ONBREAK || c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.FOJ || c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.SINBIN);

                    //
                    response.Data = data;

                }




            }
            catch (Exception ex)
            {

                try
                {
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\GetDashboardData_exception.txt", DateTime.Now + ",json:" + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message + Environment.NewLine);
                    General.WriteLog("GetDashboardData_exception", "json: " + new JavaScriptSerializer().Serialize(obj) + ", Exception: " + ex.Message);
                }
                catch
                {

                }

                response.HasError = true;
                response.Message = ex.Message;
            }



            return new CustomJsonResult { Data = response };
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetZonesList")]
        public JsonResult GetZonesList()
        {


            ResponseWebApi response = new ResponseWebApi();

            try
            {
                General OBJ = new General();





                List<Gen_Zones> data = null;


                //

                try
                {
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\GetZonesList.txt", DateTime.Now + ",json:" + new JavaScriptSerializer().Serialize(response.Data) + Environment.NewLine);
                    General.WriteLog("GetZonesList", "json: " + new JavaScriptSerializer().Serialize(response.Data));
                }
                catch
                {

                }




                List<Gen_Zones> lst = new List<Gen_Zones>();
                ZoneMasterDetailList obj = new ZoneMasterDetailList();
                List<ZonesMaster> zonesMaster = new List<ZonesMaster>();
                List<Gen_Zone_PolyVertices> gen_Zone_PolyVertices = new List<Gen_Zone_PolyVertices>();
                using (TaxiDataContext db = new TaxiDataContext())
                {
                    zonesMaster = db.ExecuteQuery<ZonesMaster>(@"SELECT [Id],[ZoneName],[AddOn],[AddBy],[EditOn],[EditBy],[PostCode],[OrderNo],[ShapeCategory],[ShapeType],[Linewidth],[Lineforecolor],[MinLatitude],[MaxLatitude]    
                        ,[MinLongitude],[MaxLongitude],[ShortName],[EnableAutoDespatch],[EnableBidding],[BiddingRadius],[ZoneTypeId],[PlotKind],[DisableDriverRank],[JobDueTime]
                        FROM[Gen_Zones]").ToList();

                    gen_Zone_PolyVertices = db.ExecuteQuery<Gen_Zone_PolyVertices>(@"SELECT[Id] ,[PostCode],[Latitude],[Longitude],[ZoneId],[Diameter] FROM[Gen_Zone_PolyVertices]").ToList();

                }




                obj.ZoneMaster = zonesMaster;
                obj.ZoneDetail = gen_Zone_PolyVertices;

                foreach (var item in obj.ZoneMaster)
                {
                    Gen_Zones dataX = new Gen_Zones();

                    dataX.Id = item.Id;
                    dataX.ZoneName = item.ZoneName;
                    dataX.AddOn = item.AddOn;
                    dataX.AddBy = item.AddBy;
                    dataX.EditOn = item.EditOn;
                    dataX.EditBy = item.EditBy;
                    dataX.PostCode = item.PostCode;
                    dataX.OrderNo = item.OrderNo;
                    dataX.ShapeCategory = item.ShapeCategory;
                    dataX.ShapeType = item.ShapeType;
                    dataX.Linewidth = item.Linewidth;
                    dataX.Lineforecolor = item.Lineforecolor;
                    dataX.MinLatitude = item.MinLatitude;
                    dataX.MaxLatitude = item.MaxLatitude;
                    dataX.MinLongitude = item.MinLongitude;
                    dataX.MaxLongitude = item.MaxLongitude;
                    dataX.ShortName = item.ShortName;
                    dataX.EnableAutoDespatch = item.EnableAutoDespatch;
                    dataX.EnableBidding = item.EnableBidding;
                    dataX.BiddingRadius = item.BiddingRadius;
                    dataX.ZoneTypeId = item.ZoneTypeId;
                    dataX.PlotKind = item.PlotKind;
                    dataX.DisableDriverRank = item.DisableDriverRank;
                    dataX.JobDueTime = item.JobDueTime;

                    dataX.Gen_Zone_PolyVertices = obj.ZoneDetail.Where(a => a.ZoneId == dataX.Id).ToList();
                    lst.Add(dataX);
                }


                response.Data = lst;


            }
            catch (Exception ex)
            {

                try
                {
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\GetZonesList_exception.txt", DateTime.Now + ",exception:" + ex.Message + Environment.NewLine);
                    General.WriteLog("GetZonesList_exception", "Exception: " + ex.Message);
                }
                catch
                {

                }

                response.HasError = true;
                response.Message = ex.Message;
            }



            return new CustomJsonResult { Data = response };
        }




        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetMapReport")]
        public JsonResult GetMapReport(WebApiClasses.RequestWebApi obj)
        {


            ResponseWebApi response = new ResponseWebApi();

            try
            {








                //

                try
                {
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\GetMapReport.txt", DateTime.Now + ",json:" + new JavaScriptSerializer().Serialize(response.Data) + Environment.NewLine);
                    General.WriteLog("GetMapReport", "json: " + new JavaScriptSerializer().Serialize(obj));
                }
                catch
                {

                }


                long jobId = obj.bookingInfo.Id;

                ClsMapReport cls = new ClsMapReport();

                List<ZonesMaster> zonesMaster = new List<ZonesMaster>();
                List<Gen_Zone_PolyVertices> gen_Zone_PolyVertices = new List<Gen_Zone_PolyVertices>();
                using (TaxiDataContext db = new TaxiDataContext())
                {

                    db.DeferredLoadingEnabled = false;
                    cls.routePath = db.Booking_RoutePaths.Where(c => c.BookingId == jobId).ToList();



                    var obj2 = db.Bookings.FirstOrDefault(c => c.Id == jobId);

                    cls.bookingInfo = new PartialBookingInfo();
                    foreach (var item in obj2.GetType().GetProperties())
                    {
                        try
                        {
                            if (cls.bookingInfo.GetType().GetProperty(item.Name) != null)
                                cls.bookingInfo.GetType().GetProperty(item.Name).SetValue(cls.bookingInfo, item.GetValue(obj2));
                        }
                        catch (Exception ex)
                        {

                        }

                    }

                    if (cls.bookingInfo.DriverId != null)
                        cls.bookingInfo.DriverNo = db.Fleet_Drivers.Where(c => c.Id == cls.bookingInfo.DriverId).Select(c => c.DriverNo).FirstOrDefault();


                    if (cls.bookingInfo.AcceptedDateTime != null)
                    {

                        cls.bookingInfo.AcceptedCoordinates = cls.routePath.FirstOrDefault(c => c.UpdateDate == cls.bookingInfo.AcceptedDateTime || c.UpdateDate.Value.AddSeconds(30) > cls.bookingInfo.AcceptedDateTime);

                        if (cls.bookingInfo.AcceptedCoordinates != null)
                            cls.bookingInfo.AcceptedCoordinates.UpdateDate = cls.bookingInfo.AcceptedDateTime;
                    }

                    if (cls.bookingInfo.ArrivalDateTime != null)
                    {

                        cls.bookingInfo.ArrivedCoordinates = cls.routePath.FirstOrDefault(c => c.UpdateDate == cls.bookingInfo.ArrivalDateTime || c.UpdateDate.Value.AddSeconds(30) > cls.bookingInfo.ArrivalDateTime);

                        if (cls.bookingInfo.ArrivedCoordinates != null)
                            cls.bookingInfo.ArrivedCoordinates.UpdateDate = cls.bookingInfo.ArrivalDateTime;
                    }


                    if (cls.bookingInfo.POBDateTime != null)
                    {

                        cls.bookingInfo.POBCoordinates = cls.routePath.FirstOrDefault(c => c.UpdateDate == cls.bookingInfo.POBDateTime || c.UpdateDate.Value.AddSeconds(30) > cls.bookingInfo.POBDateTime);

                        if (cls.bookingInfo.POBCoordinates != null)
                            cls.bookingInfo.POBCoordinates.UpdateDate = cls.bookingInfo.POBDateTime;
                    }


                    if (cls.bookingInfo.STCDateTime != null)
                    {

                        cls.bookingInfo.STCCoordinates = cls.routePath.FirstOrDefault(c => c.UpdateDate == cls.bookingInfo.STCDateTime || c.UpdateDate.Value.AddSeconds(30) > cls.bookingInfo.STCDateTime);

                        if (cls.bookingInfo.STCCoordinates != null)
                            cls.bookingInfo.STCCoordinates.UpdateDate = cls.bookingInfo.STCDateTime;
                    }


                    if (cls.bookingInfo.ClearedDateTime != null)
                    {

                        cls.bookingInfo.ClearCoordinates = cls.routePath.FirstOrDefault(c => c.UpdateDate == cls.bookingInfo.ClearedDateTime || c.UpdateDate.Value.AddSeconds(30) > cls.bookingInfo.ClearedDateTime);

                        if (cls.bookingInfo.ClearCoordinates != null)
                            cls.bookingInfo.ClearCoordinates.UpdateDate = cls.bookingInfo.ClearedDateTime;
                    }

                }



                response.Data = cls;


            }
            catch (Exception ex)
            {

                try
                {
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\GetMapReport_exception.txt", DateTime.Now + ",exception:" + ex.Message + Environment.NewLine);
                    General.WriteLog("GetMapReport_exception", "Exception: " + ex.Message);
                }
                catch
                {

                }

                response.HasError = true;
                response.Message = ex.Message;
            }



            return new CustomJsonResult { Data = response };
        }
        //  return Json(response, JsonRequestBehavior.AllowGet);




        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetDashboardDriversData")]
        public JsonResult GetDashboardDriversData(WebApiClasses.RequestWebApi obj)
        {
            //

            ResponseWebApi response = new ResponseWebApi();

            try
            {
                using (TaxiDataContext db = new TaxiDataContext())
                {

                    //


                    ClsDashboardModel data = new ClsDashboardModel();

                    // data.listofdrivers = db.stp_GetDashboardDrivers(0).ToList();


                    //data.objDriverCount = new DriverCounts();
                    //data.objDriverCount.totalDrivers = data.listofdrivers.Count;
                    //data.objDriverCount.totalAvailable = data.listofdrivers.Count(c => c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.AVAILABLE);
                    //data.objDriverCount.totalBreak = data.listofdrivers.Count(c => c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.ONBREAK);
                    //data.objDriverCount.totalBusy = data.listofdrivers.Count(c => c.driverworkstatusid != Enums.Driver_WORKINGSTATUS.AVAILABLE && c.driverworkstatusid != Enums.Driver_WORKINGSTATUS.ONBREAK);
                    //data.objDriverCount.totalOnArrived = data.listofdrivers.Count(c => c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.ARRIVED);
                    //data.objDriverCount.totalOnRoute = data.listofdrivers.Count(c => c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.ONROUTE);
                    //data.objDriverCount.totalPOB = data.listofdrivers.Count(c => c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.NOTAVAILABLE);
                    //data.objDriverCount.totalSTC = data.listofdrivers.Count(c => c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.SOONTOCLEAR);
                    //data.objDriverCount.totalWaiting = data.listofdrivers.Count(c => c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.AVAILABLE || c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.ONBREAK || c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.FOJ || c.driverworkstatusid == Enums.Driver_WORKINGSTATUS.SINBIN);


                    response.Data = SelectDashboardDrivers(db, ref data, obj.objUserInfo != null ? obj.objUserInfo.SubcompanyId.ToInt() : 0);




                }

            }
            catch (Exception ex)
            {

                response.HasError = true;
                response.Message = ex.Message;
            }



            return new CustomJsonResult { Data = response };
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetDashboardDriversPlotData")]
        public JsonResult GetDashboardDriversPlotData(WebApiClasses.RequestWebApi obj)
        {
            //

            ResponseWebApi response = new ResponseWebApi();

            try
            {
                using (TaxiDataContext db = new TaxiDataContext())
                {




                    ClsDashboardModel data = new ClsDashboardModel();

                    //data.listofdrivers = db.stp_GetDashboardDrivers(0).ToList();

                    //data.objDriverCount = new DriverCounts();
                    //data.objDriverCount.totalDrivers = data.listofdrivers.Count;


                    //response.Data = data;
                    response.Data = SelectDashboardDrivers(db, ref data, obj.objUserInfo != null ? obj.objUserInfo.SubcompanyId.ToInt() : 0);




                }

            }
            catch (Exception ex)
            {

                response.HasError = true;
                response.Message = ex.Message;
            }



            return new CustomJsonResult { Data = response };
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetDashboardBookingData")]
        public JsonResult GetDashboardBookingData(WebApiClasses.RequestWebApi obj)
        {
            //

            ResponseWebApi response = new ResponseWebApi();

            try
            {
                using (TaxiDataContext db = new TaxiDataContext())
                {



                    //DateTime? dt = DateTime.Now.ToDateorNull();
                    //DateTime recentDays = dt.Value.AddDays(-1);
                    //DateTime dtNow = DateTime.Now;
                    //DateTime prebookingdays = dt.Value.AddDays(HubProcessor.Instance.objPolicy.HourControllerReport.ToInt()).ToDate();



                    ClsDashboardModel data = new ClsDashboardModel();

                    //data.listofbookings = db.ExecuteQuery<stp_GetBookingsDataResult>("exec stp_getbookingsdata {0},{1},{2},{3}", recentDays, prebookingdays, 0, HubProcessor.Instance.objPolicy.DaysInTodayBooking.ToInt()).ToList();
                    ////      data.listofdrivers = db.stp_GetDashboardDrivers(1).ToList();


                    //data.listoftodaybookings = data.listofbookings.Where(a =>
                    //(a.PickupDateTemp >= recentDays && a.PickupDateTemp.Value.Date <= dt.Value.AddDays(0))
                    //            &&

                    //            (a.StatusId == Enums.BOOKINGSTATUS.WAITING || a.StatusId == Enums.BOOKINGSTATUS.PENDING || a.StatusId == Enums.BOOKINGSTATUS.NOTACCEPTED || a.StatusId == Enums.BOOKINGSTATUS.REJECTED
                    //               || a.StatusId == Enums.BOOKINGSTATUS.NOSHOW || a.StatusId == Enums.BOOKINGSTATUS.ONHOLD || a.StatusId == Enums.BOOKINGSTATUS.BID
                    //                || a.StatusId == Enums.BOOKINGSTATUS.PENDING_START || a.StatusId == Enums.BOOKINGSTATUS.FOJ))
                    //                .OrderBy(c => c.Lead).ToList();



                    //data.listofprebookings = data.listofbookings.Where(a => a.PickupDateTemp.Value.Date > dt
                    //                      && (a.StatusId == Enums.BOOKINGSTATUS.WAITING || a.StatusId == Enums.BOOKINGSTATUS.PENDING
                    //                      || a.StatusId == Enums.BOOKINGSTATUS.ONHOLD || a.StatusId == Enums.BOOKINGSTATUS.PENDING_START
                    //                      || a.StatusId == Enums.BOOKINGSTATUS.REJECTED || a.StatusId == Enums.BOOKINGSTATUS.NOTACCEPTED
                    //                      || a.StatusId == Enums.BOOKINGSTATUS.NOSHOW))
                    //                            .OrderBy(c => c.PickupDateTemp).ToList();



                    //data.listofrecentbookings = data.listofbookings.Where(c => c.StatusId == Enums.BOOKINGSTATUS.ONROUTE || c.StatusId == Enums.BOOKINGSTATUS.ARRIVED
                    //                                          || c.StatusId == Enums.BOOKINGSTATUS.POB || c.StatusId == Enums.BOOKINGSTATUS.STC
                    //                                          || c.StatusId == Enums.BOOKINGSTATUS.FOJ
                    //                                          || c.StatusId == Enums.BOOKINGSTATUS.PENDING_START).OrderBy(c => c.PickupDateTemp).ToList();






                    //List<clsBookingscount> objCnt = null;
                    //DateTime from = DateTime.Now.ToDate();
                    //DateTime till = DateTime.Now.AddDays(1).ToDate();

                    //objCnt = db.ExecuteQuery<clsBookingscount>
                    //    ("exec stp_getbookingsdatacountbystatus {0},{1},{2},{3}", from, till, 0, 0).ToList();

                    //data.objBookingCount = new BookingCounts();

                    //data.objDriverCount = new DriverCounts();


                    //if (objCnt != null)
                    //{

                    //    data.objBookingCount.totalToday = data.listoftodaybookings.Count;
                    //    data.objBookingCount.totalPre = data.listofprebookings.Count;
                    //    data.objBookingCount.totalRecent = data.listofrecentbookings.Count;
                    //    data.objBookingCount.totalCancelled = objCnt.Where(c => c.bookingstatusid == Enums.BOOKINGSTATUS.CANCELLED).FirstOrDefault().DefaultIfEmpty().count;
                    //    data.objBookingCount.totalNoPickup = objCnt.Where(c => c.bookingstatusid == Enums.BOOKINGSTATUS.NOPICKUP).FirstOrDefault().DefaultIfEmpty().count; ;
                    //    data.objBookingCount.totalCompleted = objCnt.Where(c => c.bookingstatusid == Enums.BOOKINGSTATUS.DISPATCHED).FirstOrDefault().DefaultIfEmpty().count;
                    //}

                    response.Data = SelectDashboardBookingsList(db, ref data, obj.objUserInfo != null ? obj.objUserInfo.SubcompanyId.ToInt() : 0);




                }

            }
            catch
            {

                response.HasError = true;
                response.Message = "exception occured";
            }

            //   return Json(response, JsonRequestBehavior.AllowGet);
            return new CustomJsonResult { Data = response };
        }




        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetBookingsByStatus")]
        public JsonResult GetBookingsByStatus(WebApiClasses.RequestWebApi obj)
        {
            //

            ResponseWebApi response = new ResponseWebApi();

            try
            {
                DateTime from = DateTime.Now.ToDate();
                DateTime till = DateTime.Now.AddDays(1).ToDate();




                //using (TaxiDataContext db = new TaxiDataContext("Data Source=88.198.21.250,58527;Initial Catalog=AscotCars;User ID=asc321;Password=asc321!;Trusted_Connection=False;"))
                //{
                using (TaxiDataContext db = new TaxiDataContext())
                {

                    if (obj.bookingInfo.BookingStatusId == 1)
                    {
                        DateTime? dt = DateTime.Now.ToDateorNull();
                        //    DateTime? lastSevenDays = dt.Value.AddDays(-7);
                        //    var data = db.ExecuteQuery<ClsBookingListData>("exec stp_GetIncompleteBookingListData {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}", false, true, false, false, false, false, false, false, false, lastSevenDays, dt, 0, "", obj.bookingInfo.BookingStatusId.ToInt()).ToList();
                        //    response.Data = data.Where(a =>
                        //    a.PickupDate.HasValue &&
                        //    a.PickupDate.Value.Date >= lastSevenDays.Value.Date &&
                        //    a.PickupDate.Value.Date < dt.Value.Date &&
                        //    (a.StatusId != Enums.BOOKINGSTATUS.DISPATCHED ||
                        //     a.StatusId == Enums.BOOKINGSTATUS.CANCELLED)
                        //).ToList();
                        var data = db.ExecuteQuery<ClsBookingListData>("exec stp_GetBookingsListData {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}", false, true, false, false, false, false, false, false, false, "", (DateTime?)DateTime.Today, obj.bookingInfo.SubcompanyId.ToInt(), "", obj.bookingInfo.BookingStatusId.ToInt()).ToList();
                        response.Data = data.Where(a =>
                            a.PickupDate.HasValue &&
                            a.PickupDate.Value.Date < dt.Value.Date &&
                            (a.StatusId == Enums.BOOKINGSTATUS.WAITING)
                        ).ToList();


                    }
                    else
                    {
                        response.Data = db.ExecuteQuery<ClsBookingListData>("exec stp_GetBookingsListData {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}", false, true, false, false, false, false, false, false, false, from, till, obj.bookingInfo.SubcompanyId.ToInt(), "", obj.bookingInfo.BookingStatusId.ToInt()).ToList();

                    }



                }




            }
            catch (Exception ex)
            {

                response.HasError = true;
                response.Message = ex.Message;
            }



            return new CustomJsonResult { Data = response };
        }


        //[System.Web.Http.HttpGet]
        //[System.Web.Http.HttpPost]
        //[System.Web.Http.Route("GetAvailableDriversDropdown")]
        //public JsonResult GetAvailableDriversDropdown(WebApiClasses.RequestWebApi obj)
        //{
        //    //

        //    ResponseWebApi response = new ResponseWebApi();

        //    using (TaxiDataContext db = new TaxiDataContext())
        //    {


        //        var list = (from a in db.Fleet_DriverQueueLists
        //                    where a.DriverId != null && a.Fleet_Driver.IsActive == true && (obj.AllocateAnyDriver == true
        //                    || (a.Status == true && (a.DriverWorkStatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE || a.DriverWorkStatusId == Enums.Driver_WORKINGSTATUS.ONBREAK))
        //                    )

        //                    orderby a.QueueDateTime
        //                    select new
        //                    {
        //                        Id = a.DriverId,
        //                        DriverName = (a.Fleet_Driver.DriverNo + " - " + a.Fleet_Driver.DriverName + " [" + a.Fleet_Driver.Fleet_VehicleType.VehicleType + "]")
        //                        //         ,
        //                        //   a.IsManualLogin
        //                         ,
        //                        a.Fleet_Driver.SubcompanyId
        //                        ,
        //                        a.Fleet_Driver.VehicleTypeId
        //                    }).Distinct().ToList();

        //        response.Data = list;

        //    }


        //    return Json(response, JsonRequestBehavior.AllowGet);

        //}

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetAvailableDriversDropdown")]
        public JsonResult GetAvailableDriversDropdown(WebApiClasses.RequestWebApi obj)
        {
            //

            ResponseWebApi response = new ResponseWebApi();

            using (TaxiDataContext db = new TaxiDataContext())
            {


                List<object> list;

                if (obj.AllocateAnyDriver == true)
                {
                    // Case 1: Load all active drivers
                    list = db.Fleet_Drivers
                        .Where(d => d.IsActive == true)
                        .Select(d => new
                        {
                            Id = d.Id,
                            DriverName = d.DriverNo + " - " + d.DriverName +
                                         " [" + d.Fleet_VehicleType.VehicleType + "]",
                            d.SubcompanyId,
                            d.VehicleTypeId
                        })
                        .ToList<object>();
                }
                else
                {
                    // Case 2: Load from queue list with conditions
                    list = (from a in db.Fleet_DriverQueueLists
                            where a.Fleet_Driver.IsActive == true
                               && a.DriverId != null
                               && a.Status == true
                               && (a.DriverWorkStatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE
                                   || a.DriverWorkStatusId == Enums.Driver_WORKINGSTATUS.ONBREAK)
                            orderby a.QueueDateTime
                            select new
                            {
                                Id = a.DriverId,
                                DriverName = a.Fleet_Driver.DriverNo + " - " + a.Fleet_Driver.DriverName +
                                             " [" + a.Fleet_Driver.Fleet_VehicleType.VehicleType + "]",
                                a.Fleet_Driver.SubcompanyId,
                                a.Fleet_Driver.VehicleTypeId
                            })
                            .Distinct()
                            .ToList<object>();
                }


                response.Data = list;

                //var list = (from a in db.Fleet_DriverQueueLists
                //            where a.DriverId != null && a.Fleet_Driver.IsActive == true && (obj.AllocateAnyDriver == true
                //            || (a.Status == true && (a.DriverWorkStatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE || a.DriverWorkStatusId == Enums.Driver_WORKINGSTATUS.ONBREAK))
                //            )

                //            orderby a.QueueDateTime
                //            select new
                //            {
                //                Id = a.DriverId,
                //                DriverName = (a.Fleet_Driver.DriverNo + " - " + a.Fleet_Driver.DriverName + " [" + a.Fleet_Driver.Fleet_VehicleType.VehicleType + "]")
                //                //         ,
                //                //   a.IsManualLogin
                //                 ,
                //                a.Fleet_Driver.SubcompanyId
                //                ,
                //                a.Fleet_Driver.VehicleTypeId
                //            }).Distinct().ToList();

                //response.Data = list;

            }


            return Json(response, JsonRequestBehavior.AllowGet);

        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetBusyDriversDropdown")]
        public JsonResult GetBusyDriversDropdown(WebApiClasses.RequestWebApi obj)
        {
            //

            ResponseWebApi response = new ResponseWebApi();

            using (TaxiDataContext db = new TaxiDataContext())
            {


                var list = (from a in db.Fleet_DriverQueueLists
                            where a.DriverId != null && a.Status == true && a.Fleet_Driver.IsActive == true
                            //&& (a.Fleet_Driver.SubcompanyId == AppVars.DefaultDriverSubCompanyId || AppVars.DefaultDriverSubCompanyId == 0) && (a.ZoneId == null || a.ZoneId != null && a.Gen_Zone.ZoneName != "SIN BIN")
 && (a.DriverWorkStatusId != Enums.Driver_WORKINGSTATUS.AVAILABLE && a.DriverWorkStatusId != Enums.Driver_WORKINGSTATUS.ONBREAK)
                            //&& (a.IsManualLogin == null || a.IsManualLogin == false)


                            orderby a.QueueDateTime
                            select new
                            {
                                Id = a.DriverId,
                                DriverName = (a.Fleet_Driver.DriverNo + " - " + a.Fleet_Driver.DriverName + " [" + a.Fleet_Driver.Fleet_VehicleType.VehicleType + "]")
                                //         ,
                                //   a.IsManualLogin
                            }).Distinct().ToList();

                response.Data = list;

            }


            return Json(response, JsonRequestBehavior.AllowGet);

        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetAllDriversDropdown")]
        public JsonResult GetAllDriversDropdown(WebApiClasses.RequestWebApi obj)
        {
            //

            ResponseWebApi response = new ResponseWebApi();

            using (TaxiDataContext db = new TaxiDataContext())
            {


                var list = (from a in db.Fleet_Drivers
                            where a.IsActive == true

                            select new
                            {
                                Id = a.Id,
                                DriverName = (a.DriverNo + " - " + a.DriverName + " [" + a.Fleet_VehicleType.VehicleType + "]")

                            }).Distinct().ToList();

                response.Data = list;

            }


            return Json(response, JsonRequestBehavior.AllowGet);

        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("UpdateFareInBooking")]
        public JsonResult UpdateFareInBooking(bookingFare obj)
        {
            ResponseWebApi response = new ResponseWebApi();

            try
            {
                using (TaxiDataContext db = new TaxiDataContext())
                {


                    var documentexpiry = db.Fleet_Driver_Documents
               .Where(x => x.DriverId == obj.DriverId.ToInt())
               .ToList();

                    bool isExpired = documentexpiry.Any(x => x.ExpiryDate < DateTime.Now);
                    if (!isExpired)
                    {
                        var result = db.ExecuteQuery<string>(
                   "EXEC stp_UpdateBookingFare @Id = {0}, @FareRate = {1}, @ReturnFareRate = {2}, @TotalCharges = {3}",
                   obj.Id,
                   obj.FareRate,
                   obj.ReturnFareRate,
                   obj.TotalCharges

               ).ToList();
                        response.Message = result.ToString();
                    }
                    response.HasError = false;






                }
            }
            catch (Exception ex)
            {
                response.HasError = true;
                response.Message = "Exception occurred: " + ex.Message;
            }

            return Json(response);
        }
        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetDriverDetailsForDeduction")]
        public JsonResult GetDriverDetailsForDeduction(WebApiClasses.RequestWebApi obj)
        {
            try
            {

                General.WriteLog("GetDriverDetailsForDeduction", "json: " + new JavaScriptSerializer().Serialize(obj));
                //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetDriverDetailsForDeduction.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
            }
            catch
            {

            }
            ResponseWebApi response = new ResponseWebApi();
            try
            {
                using (TaxiDataContext db = new TaxiDataContext())
                {
                    if (obj?.bookingInfo?.DriverId > 0)
                    {

                        var tempDriver = db.Fleet_Drivers
                            .Where(x => x.Id == obj.bookingInfo.DriverId)
                            .Select(x => new
                            {
                                x.Id,
                                x.DriverTypeId,
                                x.DriverCommissionPerBooking,
                                x.DriverMonthlyRent
                            })
                            .FirstOrDefault();

                        if (tempDriver != null)
                        {
                            // Manually create detached Fleet_Driver
                            var driver = new Fleet_Driver
                            {
                                Id = tempDriver.Id,
                                DriverTypeId = tempDriver.DriverTypeId,
                                DriverCommissionPerBooking = tempDriver.DriverCommissionPerBooking,
                                DriverMonthlyRent = tempDriver.DriverMonthlyRent
                            };

                            obj.bookingInfo.Driver = driver;
                        }
                    }
                    response.Data = obj.bookingInfo;
                }
            }
            catch (Exception ex)
            {
                try
                {
                    response.HasError = true;
                    response.Message = ex.Message;

                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetDriverDetailsForDeduction_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message + Environment.NewLine);
                    General.WriteLog("GetDriverDetailsForDeduction_exception", "json:" + new JavaScriptSerializer().Serialize(obj) + ",Exception: " + ex.Message);
                }
                catch
                {

                }
            }
            return new CustomJsonResult { Data = response };

        }
        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetBookingDetails")]
        public JsonResult GetBookingDetails(WebApiClasses.RequestWebApi obj)
        {
            //

            try
            {


                //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetBookingDetails.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
                General.WriteLog("GetBookingDetails", "json: " + new JavaScriptSerializer().Serialize(obj));
            }
            catch
            {

            }
            ResponseWebApi response = new ResponseWebApi();

            try
            {
                using (TaxiDataContext db = new TaxiDataContext())
                {
                    try
                    {
                        string query = "SELECT IsFare FROM Booking WHERE ID =" + obj.bookingInfo.Id;
                        var data = db.ExecuteQuery<BookingInfo>(query).FirstOrDefault();
                        obj.bookingInfo.IsFare = data.IsFare;
                    }
                    catch
                    {

                    }

                    try
                    {
                        string query = "SELECT CAST(ISNULL(AllocatedDriver,0) AS BIT) FROM Booking WHERE ID =" + obj.bookingInfo.Id;
                        obj.bookingInfo.AllocatedDriver = db.ExecuteQuery<bool>(query).FirstOrDefault().ToBool();
                    }
                    catch
                    {

                    }



                    var obj2 = db.Bookings.FirstOrDefault(c => c.Id == obj.bookingInfo.Id);
                    try
                    {

                        var master = db.Bookings.FirstOrDefault(x => x.MasterJobId == obj2.Id);
                        string query = "SELECT DriverId, VehicleTypeId FROM Booking WHERE ID =" + master.Id;
                        var data = db.ExecuteQuery<BookingInfo>(query).FirstOrDefault();
                        obj.bookingInfo.DriverIdReturn = data.DriverId;
                        obj.bookingInfo.VehicleTypeIdReturn = data.VehicleTypeId;
                    }
                    catch
                    {

                    }

                    if (obj?.bookingInfo?.DriverId > 0)
                    {

                        var tempDriver = db.Fleet_Drivers
                            .Where(x => x.Id == obj.bookingInfo.DriverId)
                            .Select(x => new
                            {
                                x.Id,
                                x.DriverTypeId,
                                x.DriverCommissionPerBooking,
                                x.DriverMonthlyRent
                            })
                            .FirstOrDefault();

                        if (tempDriver != null)
                        {
                            // Manually create detached Fleet_Driver
                            var driver = new Fleet_Driver
                            {
                                Id = tempDriver.Id,
                                DriverTypeId = tempDriver.DriverTypeId,
                                DriverCommissionPerBooking = tempDriver.DriverCommissionPerBooking,
                                DriverMonthlyRent = tempDriver.DriverMonthlyRent
                            };

                            obj.bookingInfo.Driver = driver;
                        }
                    }

                    try
                    {
                        if (obj2.Customer != null && !string.IsNullOrEmpty(obj2.Customer.LikesAndDislikes))
                            obj.bookingInfo.PermanentNotes = obj2.Customer.LikesAndDislikes;
                    }
                    catch
                    {
                    }
                    try
                    {
                        foreach (var item in obj2.GetType().GetProperties())
                        {
                            try
                            {
                                if (obj.bookingInfo.GetType().GetProperty(item.Name) != null)
                                    obj.bookingInfo.GetType().GetProperty(item.Name).SetValue(obj.bookingInfo, item.GetValue(obj2));


                                obj.bookingInfo.Userlog = "Job booked by : " + obj.bookingInfo.AddLog.ToStr() + " on " + string.Format("{0:dd/MM/yyyy HH:mm}", obj.bookingInfo.AddOn.ToDateTime());


                                //}

                                if (!string.IsNullOrEmpty(obj.bookingInfo.EditLog))
                                {
                                    obj.bookingInfo.Userlog += " , Edit by : " + obj.bookingInfo.EditLog.ToStr() + " on " + string.Format("{0:dd/MM/yyyy HH:mm}", obj.bookingInfo.EditOn.ToDateTime());
                                }


                                if (!string.IsNullOrEmpty(obj.bookingInfo.Despatchby))
                                {
                                    obj.bookingInfo.Userlog += " , Despatched by : " + obj.bookingInfo.Despatchby.ToStr() + " on " + string.Format("{0:dd/MM/yyyy HH:mm}", obj.bookingInfo.DespatchDateTime.ToDateTime());
                                }
                            }
                            catch (Exception ex)
                            {

                            }

                        }
                    }
                    catch
                    {

                    }

                    obj.bookingInfo.Booking_ViaLocations = new List<ClsBooking_ViaLocation>();


                    // if(obj2.ViaString.ToStr().Trim().Length>0)
                    //{

                    //try
                    //{
                    //    //
                    //    foreach (var item in db.Booking_ViaLocations.Where(c => c.BookingId == obj.bookingInfo.Id))
                    //    {
                    //        obj.bookingInfo.Booking_ViaLocations.Add(new ClsBooking_ViaLocation { Id = item.Id, BookingId = obj.bookingInfo.Id, ViaLocTypeId = Enums.LOCATION_TYPES.ADDRESS, ViaLocValue = item.ViaLocValue });
                    //    }
                    //}
                    //catch
                    //{

                    //}


                    try
                    {
                        //
                        foreach (var item in db.Booking_ViaLocations.Where(c => c.BookingId == obj.bookingInfo.Id))
                        {
                            obj.bookingInfo.Booking_ViaLocations.Add(new ClsBooking_ViaLocation { ViaLocId = item.ViaLocId, ViaLocTypeValue = item.ViaLocTypeValue, ViaLocTypeLabel = item.ViaLocTypeLabel, Id = item.Id, BookingId = obj.bookingInfo.Id, ViaLocTypeId = Enums.LOCATION_TYPES.ADDRESS, ViaLocValue = item.ViaLocValue });
                        }
                    }
                    catch
                    {

                    }
                    if (obj?.bookingInfo?.CompanyId > 0)
                    {
                        var tempCompany = db.Gen_Companies.Where(x => x.Id == obj.bookingInfo.CompanyId).Select(x => new { x.HasBookedBy, x.HasOrderNo, x.HasDepartment, x.HasEscort, x.PasswordEnable }).FirstOrDefault();

                        if (tempCompany != null)
                        {
                            // Manually create detached Fleet_Driver
                            var company = new Gen_Company
                            {
                                HasBookedBy = tempCompany.HasBookedBy == null ? false : tempCompany.HasBookedBy,
                                HasOrderNo = tempCompany.HasOrderNo == null ? false : tempCompany.HasOrderNo,
                                HasDepartment = tempCompany.HasDepartment == null ? false : tempCompany.HasDepartment,
                                HasEscort = tempCompany.HasEscort == null ? false : tempCompany.HasEscort,
                                PasswordEnable = tempCompany.PasswordEnable == null ? false : tempCompany.PasswordEnable,
                            };

                            obj.bookingInfo.Company = company;
                        }
                    }
                    if (obj.bookingInfo.ZoneId != null)
                    {
                        obj.bookingInfo.PickupZoneName = db.Gen_Zones.Where(c => c.Id == obj.bookingInfo.ZoneId).Select(c => c.ZoneName).FirstOrDefault();
                    }

                    if (obj.bookingInfo.DropOffZoneId != null)
                    {
                        obj.bookingInfo.DestinationZoneName = db.Gen_Zones.Where(c => c.Id == obj.bookingInfo.DropOffZoneId).Select(c => c.ZoneName).FirstOrDefault();
                    }
                    //      }


                    //   obj.bookingInfo.Booking_ViaLocations.Add(new Booking_ViaLocation { BookingId=obj.bookingInfo.Id, ViaLocValue="HA2 0DU", ViaLocTypeId=7 });
                    try
                    {
                        if (obj2.PaymentTypeId.ToInt() == Enums.PAYMENT_TYPES.CREDIT_CARD_PAID && db.Booking_Payments.Count(c => c.BookingId == obj2.Id) > 0)
                        {

                            obj.bookingInfo.AuthCode = db.Booking_Payments.Where(c => c.BookingId == obj2.Id).Select(c => c.AuthCode).FirstOrDefault().ToStr();
                            Booking_Payment BookingPayment = db.Booking_Payments.Where(c => c.BookingId == obj2.Id).FirstOrDefault();
                            if (BookingPayment != null)
                            {
                                obj.bookingInfo.PaymentGatewayID = BookingPayment.PaymentGatewayId.ToInt();
                                obj.bookingInfo.AuthCode = BookingPayment.AuthCode.ToStr();
                                obj.bookingInfo.IsRefund = false;

                                Taxi_Model.Booking objBooking = General.GetObject<Taxi_Model.Booking>(c => c.Id == obj2.Id);
                                Booking_Log RefundLog = null;
                                if (objBooking.Booking_Logs != null)
                                {
                                    RefundLog = objBooking.Booking_Logs.Where(c => c.AfterUpdate.ToLower().Contains("refund") && c.AfterUpdate.ToLower().Contains("payment")).FirstOrDefault();
                                    if (RefundLog != null)
                                    {
                                        obj.bookingInfo.IsRefund = true;
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {

                    }
                    int callRefNo = 0;
                    try
                    {
                        callRefNo = int.TryParse(obj.bookingInfo.CallRefNo?.Trim(), out var temp) ? temp : 0;
                    }
                    catch
                    {
                    }
                    var userName = db.CallerIdVOIP_Configurations.FirstOrDefault().UserName.ToStr();
                    string VoipUrl = System.Configuration.ConfigurationManager.AppSettings["VoipUrl"];
                    var callerData = (from a in db.CallHistories
                                      join b in db.Gen_SubCompanies on a.CalledToNumber equals b.ConnectionString into table2
                                      from b in table2.DefaultIfEmpty()
                                      where
                                       (a.PhoneNumber.Trim() == obj.bookingInfo.CustomerMobileNo || a.PhoneNumber.Trim() == obj.bookingInfo.CustomerPhoneNo)
                                       && a.Id == callRefNo
                                      orderby a.CallDateTime descending
                                      select new
                                      {
                                          Sno = a.Sno,
                                          Name = a.Name,
                                          PhoneNumber = a.PhoneNumber,
                                          CallDateTime = string.Format("{0:dd/MM/yyyy hh:mm}", a.CallDateTime),
                                          AnsweredDateTime = string.Format("{0:dd/MM/yyyy hh:mm}", a.AnsweredDateTime),
                                          Line = a.Line,
                                          STN = a.STN,
                                          Duration = a.CallDuration,
                                          IsMissed = (a.IsAccepted != null && a.IsAccepted == true) ? "1" : "0",
                                          Company = b != null && b.CompanyName != "" ? b.CompanyName : a.CalledToNumber,
                                          //RecordingUrl = a.CallDuration.Contains(".") ? VoipUrl + "/" + userName + "/inbound/" + a.CallDuration + "_" + (a.PhoneNumber.StartsWith("0") ? a.PhoneNumber.Substring(1) : a.PhoneNumber) : ""
                                          RecordingUrl = a.CallDuration.Contains(".") ? VoipUrl + "/" + userName + "/inbound/" + a.CallDuration + "_" + (a.PhoneNumber.StartsWith("0") ? "44" + a.PhoneNumber.Substring(1) : a.PhoneNumber) : ""
                                      }).FirstOrDefault();
                    Booking_DriverCommsiion booking_DriverCommsiion = new Booking_DriverCommsiion();
                    try
                    {
                        booking_DriverCommsiion = db.ExecuteQuery<Booking_DriverCommsiion>($"Select ISNULL(IsFixedNoOfHours,0) IsFixNoOfHours,ISNULL(IsFixedDriverCommission,0) IsFixedDriverCommission,DriverCommissionTypeId,DriverCommissionValue,DriverHours from Booking WHERE Id={obj.bookingInfo.Id}").FirstOrDefault();
                        if (booking_DriverCommsiion != null)
                        {
                            obj.bookingInfo.IsFixNoOfHours = booking_DriverCommsiion.IsFixNoOfHours.ToBool();
                            obj.bookingInfo.IsFixedDriverCommission = booking_DriverCommsiion.IsFixedDriverCommission.ToBool();
                            obj.bookingInfo.DriverCommissionTypeId = booking_DriverCommsiion.DriverCommissionTypeId.ToInt();
                            obj.bookingInfo.DriverCommissionValue = booking_DriverCommsiion.DriverCommissionValue.ToDecimal();
                            obj.bookingInfo.DriverHours = booking_DriverCommsiion.DriverHours.ToDecimal();
                        }
                    }
                    catch
                    {
                    }
                    if (callerData != null)
                    {
                        obj.bookingInfo.RecordingUrl = callerData.RecordingUrl;
                    }
                    if (obj2.DeadMileage > 0)
                    {
                        obj.bookingInfo.DeadMileage = obj2.DeadMileage;
                    }
                    else
                    {
                        obj.bookingInfo.DeadMileage = 0;
                    }
                    response.Data = obj.bookingInfo;




                }
            }
            catch (Exception ex)
            {
                try
                {
                    response.HasError = true;
                    response.Message = ex.Message;

                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetBookingDetails_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message + Environment.NewLine);

                    General.WriteLog("GetBookingDetails_exception", "json:" + new JavaScriptSerializer().Serialize(obj) + ",Exception: " + ex.Message);
                }
                catch
                {

                }
            }


            //   return Json(response, JsonRequestBehavior.AllowGet);
            return new CustomJsonResult { Data = response };
        }





        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetBookingDropDownData")]
        public JsonResult GetBookingDropDownData(WebApiClasses.RequestWebApi obj)
        {
            //

            ResponseWebApi response = new ResponseWebApi();
            try
            {

                DataSet ds = new DataSet();
                using (System.Data.SqlClient.SqlConnection sqlconn = new System.Data.SqlClient.SqlConnection(Cryptography.Decrypt(System.Configuration.ConfigurationManager.AppSettings["ConnectionString"].ToStr(), "tcloudX@@!", true)))
                {

                    sqlconn.Open();

                    using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand())
                    {

                        cmd.Connection = sqlconn;

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "stp_fillbookingcombos";

                        using (System.Data.SqlClient.SqlDataAdapter da = new System.Data.SqlClient.SqlDataAdapter(cmd))
                        {

                            da.Fill(ds);
                        }

                    }

                    var LocationTypes = ds.Tables[0].AsEnumerable()
                       .Select(datarow => new
                       {
                           Id = datarow.Field<int>("Id"),
                           LocationType = datarow.Field<string>("LocationType")
                       });


                    var SubCompanies = ds.Tables[1].AsEnumerable()
                      .Select(datarow => new
                      {
                          Id = datarow.Field<int>("Id"),
                          CompanyName = datarow.Field<string>("CompanyName")
                      });


                    var BookingTypes = ds.Tables[3].AsEnumerable()
                      .Select(datarow => new
                      {
                          Id = datarow.Field<int>("Id"),
                          BookingTypeName = datarow.Field<string>("BookingTypeName")
                      });


                    var VehicleTypes = ds.Tables[4].AsEnumerable()
                      .Select(datarow => new
                      {
                          Id = datarow.Field<int>("Id"),
                          VehicleType = datarow.Field<string>("VehicleType"),
                          IsActive = datarow.Field<bool?>("IsActive"),
                      });


                    var PaymentTypes = ds.Tables[5].AsEnumerable()
                      .Select(datarow => new
                      {
                          Id = datarow.Field<int>("Id"),
                          PaymentType = datarow.Field<string>("PaymentType")
                      });


                    var AccountNames = ds.Tables[6].AsEnumerable()
                 .Select(datarow => new
                 {
                     Id = datarow.Field<int>("Id"),
                     CompanyName = datarow.Field<string>("CompanyName"),
                     // HasVat = datarow.Field<bool>("HasVat")
                 });




                    response.Data = new { LocationTypes = LocationTypes, SubCompanies = SubCompanies, VehicleTypes = VehicleTypes, BookingTypes = BookingTypes, PaymentTypes = PaymentTypes, AccountNames = AccountNames };

                    //      response.Data = list;
                }

                //using (TaxiDataContext db = new TaxiDataContext())
                //{







                //    DataTableCollection tables = db.ExecuteQuery<DataTableCollection>("exec stp_fillbookingcombos").FirstOrDefault();



                //    response.Data = tables;




                //}
            }
            catch (Exception ex)
            {
                response.HasError = true;
                response.Message = ex.Message;
            }


            return Json(response, JsonRequestBehavior.AllowGet);

        }






        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("SaveBooking")]
        public JsonResult SaveBooking(WebApiClasses.RequestWebApi obj)
        {
            //


            BookingBO objMaster = new BookingBO();

            ResponseWebApi response = new ResponseWebApi();
            try
            {

                //
                try
                {
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "SaveBooking.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
                    General.WriteLog("SaveBooking", "json: " + new JavaScriptSerializer().Serialize(obj));
                }
                catch
                {

                }

                bool IsAddMode = false;
                using (TaxiDataContext db = new TaxiDataContext())
                {
                    int journeyTypeId = 0;
                    long? AdvanceBookingId = null;
                    DateTime? startDate = obj.bookingInfo.PickupDateTime;
                    DateTime? endDateTime = obj.bookingInfo.PickupDateTime;
                    List<string> Days = null;
                    int weeks = 0;
                    try
                    {


                        if (obj.bookingInfo.AdvanceBookingId != null && obj.bookingInfo.AdvanceBookingId != 0 && obj.bookingInfo.ExtendMulti == true)
                        {
                            journeyTypeId = obj.bookingInfo.JourneyTypeId.ToInt();
                            var multi = obj.bookingInfo.objMulti;
                            startDate = obj.bookingInfo.objMulti.StartDate;
                            endDateTime = obj.bookingInfo.objMulti.EndDate;
                            string Query = "SELECT * FROM  BOOKING WHERE AdvanceBookingId = {0} and journeytypeid = {1}";

                            var data = db.ExecuteQuery<BookingInfo>(Query, obj.bookingInfo.AdvanceBookingId, obj.bookingInfo.JourneyTypeId).FirstOrDefault();

                            obj.bookingInfo = data;
                            obj.bookingInfo.JourneyTypeId = journeyTypeId;
                            obj.bookingInfo.objMulti = multi;
                            obj.bookingInfo.ExtendMulti = true;
                            obj.bookingInfo.Id = 0;
                            obj.bookingInfo.BookingStatusId = 1;
                        }

                        if (!string.IsNullOrEmpty(obj.bookingInfo.CustomerMobileNo))
                        {
                            var blacklistedCustomer = db.Customers.Where(x => x.BlackList == true && (x.MobileNo == obj.bookingInfo.CustomerMobileNo || x.TelephoneNo == obj.bookingInfo.CustomerMobileNo)).Select(x => x.Id).FirstOrDefault().ToInt();
                            if (blacklistedCustomer > 0)
                            {
                                response.HasError = true;
                                response.Message = "Customer is black listed. Cannot save booking.";
                                return Json(response, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    catch
                    {
                    }

                    //
                    if (obj.bookingInfo.objMulti != null)
                    {

                        startDate = obj.bookingInfo.objMulti.StartDate;
                        endDateTime = obj.bookingInfo.objMulti.EndDate;
                        obj.bookingInfo.objMulti.Days = obj.bookingInfo.objMulti.Days.ToStr().Trim().Replace("MON", "MONDAY").Replace("TUE", "TUESDAY").Replace("WED", "WEDNESDAY").Replace("THURS", "THURSDAY").Replace("FRI", "FRIDAY").Replace("SAT", "SATURDAY").Replace("SUN", "SUNDAY");

                        Days = obj.bookingInfo.objMulti.Days.ToStr().Trim().Split(',').ToList();

                        Days.RemoveAll(c => c == "");

                        weeks = obj.bookingInfo.objMulti.Noofweeks.ToInt();


                        AdvanceBookingBO objAdvBO = new AdvanceBookingBO();
                        if (obj.bookingInfo.AdvanceBookingId != null && obj.bookingInfo.AdvanceBookingId != 0 && obj.bookingInfo.ExtendMulti == true)
                        {
                            objAdvBO.GetByPrimaryKey(obj.bookingInfo.AdvanceBookingId);

                            objAdvBO.Edit();

                            objAdvBO.Current.EditOn = DateTime.Now;
                            objAdvBO.Current.CustomerName = obj.bookingInfo.CustomerName.ToStr();
                            objAdvBO.Current.CustomerTelephoneNo = obj.bookingInfo.CustomerPhoneNo.ToStr();
                            objAdvBO.Current.CustomerMobileNo = obj.bookingInfo.CustomerMobileNo.ToStr();
                            objAdvBO.Current.CustomerEmail = obj.bookingInfo.CustomerEmail.ToStr();
                            objAdvBO.Current.FromAddress = obj.bookingInfo.FromAddress.ToStr();
                            objAdvBO.Current.ToAddress = obj.bookingInfo.ToAddress.ToStr();
                            objAdvBO.Current.PickupDateTime = endDateTime;

                            objAdvBO.Save();

                            AdvanceBookingId = obj.bookingInfo.AdvanceBookingId;
                        }
                        //if (savedAdvanceBookingId == 0)
                        //{

                        else
                        {
                            objAdvBO.New();
                            objAdvBO.Current.AdvanceBookingNo = obj.bookingInfo.SubcompanyId.ToInt().ToStr();
                            objAdvBO.Current.CustomerName = obj.bookingInfo.CustomerName.ToStr();
                            objAdvBO.Current.CustomerTelephoneNo = obj.bookingInfo.CustomerPhoneNo.ToStr();
                            objAdvBO.Current.CustomerMobileNo = obj.bookingInfo.CustomerMobileNo.ToStr();
                            objAdvBO.Current.CustomerEmail = obj.bookingInfo.CustomerEmail.ToStr();
                            objAdvBO.Current.FromAddress = obj.bookingInfo.FromAddress.ToStr();
                            objAdvBO.Current.ToAddress = obj.bookingInfo.ToAddress.ToStr();



                            objAdvBO.Current.AddOn = DateTime.Now;
                            objAdvBO.Current.PickupDateTime = endDateTime;

                            objAdvBO.Save();

                            AdvanceBookingId = objAdvBO.Current.Id;
                        }
                        //  objAdvBO.Current.AddLog = AppVars.LoginObj.UserName.ToStr();
                        //   objAdvBO.Current.AddBy = AppVars.LoginObj.LuserId.ToIntorNull();
                        //  }
                        //else
                        //{



                        //    objAdvBO.GetByPrimaryKey(savedAdvanceBookingId);

                        //    objAdvBO.Edit();




                        //}



                    }
                    //

                    if (startDate == null && endDateTime == null)
                    {

                        startDate = DateTime.Now;
                        endDateTime = startDate;
                    }

                    while (startDate.HasValue && endDateTime.HasValue && startDate.Value.Date <= endDateTime.Value.Date)
                    {


                        string dayName = System.Threading.Thread.CurrentThread.CurrentUICulture.DateTimeFormat.GetDayName(startDate.Value.DayOfWeek);

                        if (Days != null && Days.Count(c => c.Contains(dayName.ToUpper())) == 0)
                        {
                            startDate = startDate.Value.AddDays(1);
                            continue;
                        }



                        if (obj.bookingInfo.PickupDateTime == null)
                            obj.bookingInfo.PickupDateTime = DateTime.Now;

                        startDate = startDate.ToDate() + obj.bookingInfo.PickupDateTime.Value.TimeOfDay;

                        //if (obj.bookingInfo.Id == 0)
                        //{
                        //    IsAddMode = true;
                        //    objMaster.New();

                        //    objMaster.Current.BookingDate = DateTime.Now;
                        //}
                        //else
                        //    objMaster.GetByPrimaryKey(obj.bookingInfo.Id);
                        if (obj.bookingInfo.Id == 0)
                        {
                            IsAddMode = true;
                            objMaster.New();

                            objMaster.Current.BookingDate = DateTime.Now;

                            objMaster.Current.AddLog = obj.UserName.ToStr();
                            objMaster.Current.AddOn = DateTime.Now;
                            //
                        }
                        else
                        {
                            objMaster.GetByPrimaryKey(obj.bookingInfo.Id);


                            objMaster.Current.EditLog = obj.UserName.ToStr();
                            objMaster.Current.EditOn = DateTime.Now;

                        }
                        //


                        //
                        foreach (var item in obj.bookingInfo.GetType().GetProperties())
                        {
                            try
                            {

                                if (item.Name == "CustomerCreditCardDetails")
                                    continue;

                                if (item.Name == "AddBy")
                                    continue;

                                if (item.Name == "AddOn")
                                    continue;

                                if (item.Name == "AddLog")
                                    continue;


                                if (item.Name == "BookingDate")
                                    continue;


                                if (item.Name == "EditBy")
                                    continue;

                                if (item.Name == "EditOn")
                                    continue;

                                if (item.Name == "EditLog")
                                    continue;


                                if (item.Name == "AcceptedDateTime")
                                    continue;

                                if (item.Name == "ArrivalDateTime")
                                    continue;

                                if (item.Name == "POBDateTime")
                                    continue;

                                if (item.Name == "STCDateTime")
                                    continue;

                                if (item.Name == "ClearedDateTime")
                                    continue;

                                //    obj.bookingInfo.

                                if (item.Name == "BookingReturn")
                                    continue;


                                if (objMaster.Current.Id > 0)
                                {
                                    if (item.Name == "CustomerId")
                                        continue;
                                    if (item.Name == "CallRefNo")
                                        continue;

                                    if (item.Name == "BookingStatusId")
                                        continue;

                                    if (item.Name == "BookingNo")
                                        continue;

                                    if (item.Name == "MasterJobId")
                                        continue;



                                    if (item.Name == "AdvanceBookingId")
                                        continue;



                                    if (item.Name == "DriverId")
                                    {
                                        if (objMaster.Current.BookingStatusId != Enums.BOOKINGSTATUS.WAITING && obj.bookingInfo.DriverId == null && obj.editbookingInfo != null && objMaster.Current.DriverId != null)
                                            continue;
                                    }
                                }


                                if (objMaster.Current.GetType().GetProperty(item.Name) != null && item.Name != "Booking_ViaLocations")
                                    objMaster.Current.GetType().GetProperty(item.Name).SetValue(objMaster.Current, item.GetValue(obj.bookingInfo));
                            }
                            catch (Exception ex)
                            {

                            }

                        }


                        if (objMaster.Current.CompanyId != null)
                            objMaster.Current.IsCompanyWise = true;
                        else
                            objMaster.Current.IsCompanyWise = false;


                        if (objMaster.Current.VehicleTypeId == null)
                            objMaster.Current.VehicleTypeId = HubProcessor.Instance.objPolicy.DefaultVehicleTypeId;


                        if (objMaster.Current.SubcompanyId == null)
                            objMaster.Current.SubcompanyId = 1;


                        if (objMaster.Current.PaymentTypeId == null)
                            objMaster.Current.PaymentTypeId = Enums.PAYMENT_TYPES.CASH;



                        if (objMaster.Current.IsQuotation == null)
                            objMaster.Current.IsQuotation = false;

                        if (objMaster.Current.PickupDateTime == null)
                            objMaster.Current.PickupDateTime = DateTime.Now;


                        if (AdvanceBookingId != null)
                            objMaster.Current.PickupDateTime = startDate;

                        if (objMaster.Current.JourneyTypeId == null)
                            objMaster.Current.JourneyTypeId = Enums.JOURNEY_TYPES.ONEWAY;



                        if (objMaster.Current.FromLocTypeId == null)
                            objMaster.Current.FromLocTypeId = Enums.LOCATION_TYPES.ADDRESS;


                        if (objMaster.Current.ToLocTypeId == null)
                            objMaster.Current.ToLocTypeId = Enums.LOCATION_TYPES.ADDRESS;


                        if (objMaster.Current.BookingTypeId == null)
                            objMaster.Current.BookingTypeId = Enums.BOOKING_TYPES.LOCAL;



                        if (objMaster.Current.ZoneId == null)
                            objMaster.Current.ZoneId = General.GetZoneId(objMaster.Current.FromAddress.ToStr().ToUpper().Trim());

                        if (objMaster.Current.DropOffZoneId == null)
                            objMaster.Current.DropOffZoneId = General.GetZoneId(objMaster.Current.ToAddress.ToStr().ToUpper().Trim());



                        if (objMaster.Current.ZoneId.ToInt() == 0)
                            objMaster.Current.ZoneId = null;




                        if (objMaster.Current.DropOffZoneId.ToInt() == 0)
                            objMaster.Current.DropOffZoneId = null;



                        objMaster.Current.AdvanceBookingId = AdvanceBookingId;

                        try
                        {


                            if (objMaster.Current.Booking_ViaLocations != null)
                                objMaster.Current.Booking_ViaLocations.Clear();


                            foreach (var item in obj.bookingInfo.Booking_ViaLocations)
                            {

                                objMaster.Current.Booking_ViaLocations.Add(new Booking_ViaLocation { BookingId = obj.bookingInfo.Id, ViaLocTypeId = Enums.LOCATION_TYPES.ADDRESS, ViaLocValue = item.ViaLocValue });
                            }
                            //foreach (var item in obj.bookingInfo.Booking_ViaLocations)
                            //{

                            //    objMaster.Current.Booking_ViaLocations.Add(new Booking_ViaLocation { ViaLocTypeLabel = item.ViaLocTypeLabel, ViaLocTypeValue = item.ViaLocTypeValue, BookingId = obj.bookingInfo.Id, ViaLocTypeId = Enums.LOCATION_TYPES.ADDRESS, ViaLocValue = item.ViaLocValue });
                            //}

                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "SaveBooking_viaexception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",error:" + ex.Message + Environment.NewLine);
                                General.WriteLog("SaveBooking_viaexception", "Exception: " + ex.Message);
                            }
                            catch
                            {

                            }

                        }



                        objMaster.ReturnSpecialRequirement = obj.bookingInfo.ReturnSpecialRequirements.ToStr().Trim();





                        if (objMaster.Current.JourneyTypeId.ToInt() == Enums.JOURNEY_TYPES.RETURN && AdvanceBookingId != null)
                        {
                            objMaster.Current.ReturnPickupDateTime = startDate.ToDate() + objMaster.Current.ReturnPickupDateTime.Value.TimeOfDay;


                        }


                        //if (objMaster.Current.Id > 0 && obj.editbookingInfo != null)
                        //{
                        //    try
                        //    {
                        //        System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "EditBooking.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",old pickup:" + string.Format("{0:dd/MM/yyyy HH:mm}", obj.editbookingInfo.PickupDateTime) + ",new pickup:" + string.Format("{0:dd/MM/yyyy HH:mm}", obj.bookingInfo.PickupDateTime) + Environment.NewLine);
                        //    }
                        //    catch
                        //    {

                        //    }

                        //    if (obj.editbookingInfo.PickupDateTime != objMaster.Current.PickupDateTime)
                        //    {


                        //        objMaster.Current.Booking_Logs.Add(new Booking_Log { BookingId = objMaster.Current.Id, User = obj.UserName.ToStr(), BeforeUpdate = "Pickup Date/Time : " + string.Format("{0:dd/MM/yyyy HH:mm}", obj.editbookingInfo.PickupDateTime), AfterUpdate = "Pickup Date/Time : " + string.Format("{0:dd/MM/yyyy HH:mm}", objMaster.Current.PickupDateTime), UpdateDate = DateTime.Now });

                        //    }

                        //}

                        try
                        {
                            if (IsAddMode == true && Global.ShowAllocatedInFutureList == "1" && objMaster.Current.DriverId.ToInt() > 0)
                            {
                                General.requestPDA("request pda=" + objMaster.Current.DriverId + "=" + 0 + "=" + "Message>>" + "You have received a Future Jobs" + ">>" + String.Format("{0:MM/dd/yyyy HH:mm:ss}", DateTime.Now) + "=4");
                            }
                        }
                        catch
                        {
                        }

                        if (objMaster.Current.Id > 0 && obj.editbookingInfo != null)
                        {
                            try
                            {
                                //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "EditBooking.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",old pickup:" + string.Format("{0:dd/MM/yyyy HH:mm}", obj.editbookingInfo.PickupDateTime) + ",new pickup:" + string.Format("{0:dd/MM/yyyy HH:mm}", obj.bookingInfo.PickupDateTime) + Environment.NewLine);
                                General.WriteLog("EditBooking", "old pickup: " + obj.editbookingInfo.PickupDateTime + ",new pickup:" + obj.bookingInfo.PickupDateTime);
                            }
                            catch
                            {

                            }

                            try
                            {
                                try
                                {
                                    string Query = "SELECT * FROM  BOOKING WHERE ID = {0}";

                                    var data = db.ExecuteQuery<BookingInfo>(Query, objMaster.Current.Id).FirstOrDefault();
                                    if (obj.bookingInfo.IsFare != data.IsFare)
                                    {
                                        objMaster.Current.Booking_Logs.Add(new Booking_Log { BookingId = objMaster.Current.Id, User = obj.UserName.ToStr(), BeforeUpdate = "Manual Fare " + (data.IsFare == true ? "Enabled" : "Disabled"), AfterUpdate = "Manual Fare " + (obj.bookingInfo.IsFare == true ? "Enabled" : "Disabled"), UpdateDate = DateTime.Now });

                                    }
                                }
                                catch
                                {

                                }


                                if (obj.editbookingInfo.PickupDateTime != objMaster.Current.PickupDateTime)
                                {


                                    objMaster.Current.Booking_Logs.Add(new Booking_Log { BookingId = objMaster.Current.Id, User = obj.UserName.ToStr(), BeforeUpdate = "Pickup Date/Time : " + string.Format("{0:dd/MM/yyyy HH:mm}", obj.editbookingInfo.PickupDateTime), AfterUpdate = "Pickup Date/Time : " + string.Format("{0:dd/MM/yyyy HH:mm}", objMaster.Current.PickupDateTime), UpdateDate = DateTime.Now });

                                }


                                if (obj.editbookingInfo.FromAddress.ToStr().ToUpper() != objMaster.Current.FromAddress.ToStr().ToUpper())
                                {


                                    objMaster.Current.Booking_Logs.Add(new Booking_Log { BookingId = objMaster.Current.Id, User = obj.UserName.ToStr(), BeforeUpdate = "Pickup:" + obj.editbookingInfo.FromAddress.ToStr(), AfterUpdate = "Pickup:" + objMaster.Current.FromAddress, UpdateDate = DateTime.Now });

                                }


                                if (obj.editbookingInfo.ToAddress.ToStr().ToUpper() != objMaster.Current.ToAddress.ToStr().ToUpper())
                                {


                                    objMaster.Current.Booking_Logs.Add(new Booking_Log { BookingId = objMaster.Current.Id, User = obj.UserName.ToStr(), BeforeUpdate = "Destination:" + obj.editbookingInfo.ToAddress.ToStr(), AfterUpdate = "Destination:" + objMaster.Current.ToAddress, UpdateDate = DateTime.Now });

                                }
                                if (obj.editbookingInfo.FareRate.ToDecimal() != objMaster.Current.FareRate.ToDecimal())
                                {


                                    objMaster.Current.Booking_Logs.Add(new Booking_Log { BookingId = objMaster.Current.Id, User = obj.UserName.ToStr(), BeforeUpdate = "Fares:" + obj.editbookingInfo.FareRate.ToStr(), AfterUpdate = "Fares:" + objMaster.Current.FareRate, UpdateDate = DateTime.Now });

                                }
                                if (obj.editbookingInfo.CustomerName.ToStr().ToUpper() != objMaster.Current.CustomerName.ToStr().ToUpper())
                                {


                                    objMaster.Current.Booking_Logs.Add(new Booking_Log { BookingId = objMaster.Current.Id, User = obj.UserName.ToStr(), BeforeUpdate = "Customer Name:" + obj.editbookingInfo.CustomerName.ToStr(), AfterUpdate = "Customer Name:" + objMaster.Current.CustomerName, UpdateDate = DateTime.Now });

                                }
                                if (obj.editbookingInfo.CustomerMobileNo.ToStr().ToUpper() != objMaster.Current.CustomerMobileNo.ToStr().ToUpper())
                                {


                                    objMaster.Current.Booking_Logs.Add(new Booking_Log { BookingId = objMaster.Current.Id, User = obj.UserName.ToStr(), BeforeUpdate = "Customer Mobile No:" + obj.editbookingInfo.CustomerMobileNo.ToStr(), AfterUpdate = "Customer Mobile No:" + objMaster.Current.CustomerMobileNo, UpdateDate = DateTime.Now });

                                }
                                if (obj.editbookingInfo.DriverId.ToInt() != objMaster.Current.DriverId.ToInt())
                                {

                                    if (obj.editbookingInfo.DriverId == null && objMaster.Current.DriverId != null)
                                    {
                                        objMaster.Current.Booking_Logs.Add(new Booking_Log { BookingId = objMaster.Current.Id, User = obj.UserName.ToStr(), BeforeUpdate = "Driver:", AfterUpdate = "Driver:" + db.Fleet_Drivers.Where(c => c.Id == objMaster.Current.Id).Select(c => c.DriverNo).FirstOrDefault(), UpdateDate = DateTime.Now });
                                    }
                                    else if (obj.editbookingInfo.DriverId != null && objMaster.Current.DriverId != null)
                                    {
                                        objMaster.Current.Booking_Logs.Add(new Booking_Log { BookingId = objMaster.Current.Id, User = obj.UserName.ToStr(), BeforeUpdate = "Driver:" + db.Fleet_Drivers.Where(c => c.Id == obj.editbookingInfo.DriverId).Select(c => c.DriverNo).FirstOrDefault(), AfterUpdate = "Driver:" + db.Fleet_Drivers.Where(c => c.Id == objMaster.Current.DriverId).Select(c => c.DriverNo).FirstOrDefault(), UpdateDate = DateTime.Now });
                                    }
                                }
                                if (obj.editbookingInfo.PaymentTypeId.ToInt() != objMaster.Current.PaymentTypeId.ToInt())
                                {
                                    objMaster.Current.Booking_Logs.Add(new Booking_Log { BookingId = objMaster.Current.Id, User = obj.UserName.ToStr(), BeforeUpdate = "Payment Type:" + db.Gen_PaymentTypes.Where(c => c.Id == obj.editbookingInfo.PaymentTypeId).Select(c => c.PaymentType).FirstOrDefault(), AfterUpdate = "Payment Type:" + db.Gen_PaymentTypes.Where(c => c.Id == objMaster.Current.PaymentTypeId).Select(c => c.PaymentType).FirstOrDefault(), UpdateDate = DateTime.Now });

                                }


                            }
                            catch (Exception ex)
                            {
                                try
                                {
                                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "EditBookingLog_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine + ",Exception:" + ex.Message + Environment.NewLine);
                                    General.WriteLog("EditBookingLog_exception", "json: " + new JavaScriptSerializer().Serialize(obj) + ", Exception: " + ex.Message);
                                }
                                catch
                                {

                                }
                            }

                        }


                        if (objMaster.Current.AttributeValues.ToStr().Trim() == "''" || objMaster.Current.AttributeValues.ToStr().Trim() == ", ,")
                            objMaster.Current.AttributeValues = null;

                        if (Global.EnableManualLeadTime == "true")
                        {
                            if (obj.bookingInfo.LeadTime > 0)
                            {
                                objMaster.Current.AutoDespatchTime = objMaster.Current.PickupDateTime.Value.AddMinutes(-obj.bookingInfo.LeadTime.ToInt()).ToDateTime();
                                objMaster.Current.DeadMileage = obj.bookingInfo.LeadTime;
                            }
                            else
                            {
                                objMaster.Current.AutoDespatchTime = null;
                                objMaster.Current.DeadMileage = 0;
                            }
                        }
                        objMaster.ReturnCustomerPrice = objMaster.Current.ServiceCharges.ToDecimal();
                        objMaster.Save();
                        try
                        {
                            if (obj.editbookingInfo != null && obj.editbookingInfo.CustomerId != null && obj.bookingInfo.PermanentNotes != null)
                            {
                                var CustomerId = obj.editbookingInfo.CustomerId.ToInt();
                                Customer cus = new Customer();
                                objMaster.Current.Customer = cus;
                                objMaster.Current.Customer.Id = CustomerId;
                            }
                            if (obj.bookingInfo.PermanentNotes != null)
                            {
                                db.ExecuteQuery<int>("update Customer set LikesAndDislikes='" + obj.bookingInfo.PermanentNotes + "'where Id=" + objMaster.Current.Customer.Id);
                            }
                        }
                        catch
                        {
                        }
                        try
                        {
                            if (!string.IsNullOrEmpty(obj.bookingInfo.OrderNo) && obj.bookingInfo.CompanyId > 0)
                            {
                                // Check if the order number already exists for this company
                                bool exists = db.Gen_Company_OrderNumbers
                                    .Any(x => x.CompanyId == obj.bookingInfo.CompanyId && x.OrderNo == obj.bookingInfo.OrderNo);

                                if (!exists)
                                {
                                    // Create and add new order number entry
                                    var newOrderNumber = new Gen_Company_OrderNumber
                                    {
                                        OrderNo = obj.bookingInfo.OrderNo,
                                        CompanyId = obj.bookingInfo.CompanyId.Value
                                    };

                                    db.Gen_Company_OrderNumbers.InsertOnSubmit(newOrderNumber);
                                    db.SubmitChanges();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                        try
                        {
                            string query1 = "delete from Booking_ViaLocations where BookingId={0}";
                            db.ExecuteCommand(query1, objMaster.Current.Id);
                            foreach (var item in obj.bookingInfo.Booking_ViaLocations)
                            {
                                string queryR = "INSERT INTO Booking_ViaLocations (ViaLocTypeLabel, ViaLocTypeValue, BookingId, ViaLocTypeId, ViaLocValue,ViaLocId) VALUES ({0}, {1}, {2}, {3}, {4},NULLIF({5},0))";
                                db.ExecuteCommand(queryR, item.ViaLocTypeLabel != null ? item.ViaLocTypeLabel : "", item.ViaLocTypeValue != null ? item.ViaLocTypeValue : "", objMaster.Current.Id, Enums.LOCATION_TYPES.ADDRESS, item.ViaLocValue, item.ViaLocId > 0 ? item.ViaLocId : 0);


                            }
                        }
                        catch (Exception ex)
                        {

                        }

                        try
                        {
                            string query1 = "Update booking set IsFare= {0} where Id={1}";
                            db.ExecuteCommand(query1, obj.bookingInfo.IsFare, objMaster.Current.Id);
                            var master = db.Bookings.FirstOrDefault(x => x.MasterJobId == objMaster.Current.Id);
                            if (master != null)
                            {
                                string query2 = "Update booking set IsFare= {0} where Id={1}";
                                db.ExecuteCommand(query2, obj.bookingInfo.IsFare, master.Id);
                            }
                        }
                        catch
                        {
                        }
                        try
                        {
                            var master = db.Bookings.FirstOrDefault(x => x.MasterJobId == objMaster.Current.Id);
                            if (master != null)
                            {

                                try
                                {
                                    var EnableOtherReturnFields = db.ExecuteQuery<string>("Select SetVal From AppSettings where setkey='EnableOtherReturnFields'").FirstOrDefault().ToStr();

                                    if (EnableOtherReturnFields == "true")
                                    {
                                        string query2 = "Update booking set DriverId=NULLIF({0},0), VehicleTypeId=NULLIF({1},0) where Id={2}";
                                        db.ExecuteCommand(query2, obj.bookingInfo.DriverIdReturn > 0 ? obj.bookingInfo.DriverIdReturn : 0, obj.bookingInfo.VehicleTypeIdReturn > 0 ? obj.bookingInfo.VehicleTypeIdReturn : 0, master.Id);
                                    }


                                }
                                catch
                                {

                                }



                                string query1 = "delete from Booking_ViaLocations where BookingId={0}";
                                db.ExecuteCommand(query1, master.Id);
                                foreach (var item in obj.bookingInfo.Booking_ViaLocations)
                                {

                                    string queryR = "INSERT INTO Booking_ViaLocations (ViaLocTypeLabel, ViaLocTypeValue, BookingId, ViaLocTypeId, ViaLocValue,ViaLocId) VALUES ({0}, {1}, {2}, {3}, {4}, NULLIF({5},0))";
                                    db.ExecuteCommand(queryR, item.ViaLocTypeLabel != null ? item.ViaLocTypeLabel : "", item.ViaLocTypeValue != null ? item.ViaLocTypeValue : "", master.Id, Enums.LOCATION_TYPES.ADDRESS, item.ViaLocValue, item.ViaLocId > 0 ? item.ViaLocId : 0);

                                }
                            }
                        }

                        catch (Exception ex)
                        {

                        }
                        var EnableSendConfirmationEmail = "false";
                        try
                        {
                            EnableSendConfirmationEmail = db.ExecuteQuery<string>("Select SetVal from AppSettings where SetKey ='EnableSendConfirmationEmail'").FirstOrDefault().ToStr();
                        }
                        catch
                        {
                        }
                        if (EnableSendConfirmationEmail == "true")
                        {
                            try
                            {
                                if (objMaster.Current.Id > 0)
                                {
                                    var subCompany = db.ExecuteQuery<Gen_SubcompanyFields>($"select Id,EmailAddress,SmtpEmailAddress,SmtpInvoiceEmailAddress,SmtpDriverEmailAddress,CAST(ISNULL(UseDifferentEmailForInvoices,0) AS BIT) UseDifferentEmailForInvoices,SmtpInvoiceUserName from Gen_SubCompany WHERE Id={objMaster.Current.SubcompanyId}").FirstOrDefault();

                                    var emailPayload = new RequestWebApi
                                    {
                                        emailInfo = new EmailInfo
                                        {
                                            From = subCompany.EmailAddress,
                                            To = obj.bookingInfo.CustomerEmail,
                                            Subject = "BOOKING CONFIRMATION - " + objMaster.Current.BookingDate + " BOOKING ID " + objMaster.Current.BookingNo,
                                            BookingId = objMaster.Current.Id
                                        }
                                    };
                                    var controller = new WebApiController();
                                    controller.SendConfirmationEmail(emailPayload);


                                }


                            }

                            catch (Exception ex)
                            {

                            }
                        }

                        try
                        {
                            if (objMaster.Current.Id > 0 && objMaster.Current.DriverId != null && !(new List<int> { 1, 2, 3 }.Contains(objMaster.Current.BookingStatusId.ToInt())))
                            {
                                General.UpdateJobToDriverPDA(objMaster.Current.Id);



                            }
                        }
                        catch
                        {

                        }

                        startDate = startDate.Value.AddDays(1);
                    }

                    response.Data = GetBookingJSON(objMaster);

                    try
                    {
                        string driverCommissionQuery = $"Update Booking SET DriverCommissionTypeId={obj.bookingInfo.DriverCommissionTypeId}, DriverCommissionValue={obj.bookingInfo.DriverCommissionValue.ToDecimal()}, DriverHours={obj.bookingInfo.DriverHours.ToDecimal()}, IsFixedNoOfHours={(obj.bookingInfo.IsFixNoOfHours.ToBool() ? "1" : "0")}, IsFixedDriverCommission={(obj.bookingInfo.IsFixedDriverCommission.ToBool() ? "1" : "0")} WHERE Id={objMaster.Current.Id}";
                        db.ExecuteQuery<int>(driverCommissionQuery);
                        if (objMaster.Current.Id > 0 && objMaster.Current.DriverId > 0)
                        {
                            db.ExecuteQuery<int>($"exec sp_calculateDriverCommission {objMaster.Current.Id},{objMaster.Current.DriverId}");
                        }
                    }
                    catch
                    {
                    }

                    try
                    {
                        string allocatedDriverQuery = $"Update Booking SET AllocatedDriver={(obj.bookingInfo.AllocatedDriver.ToBool() ? "1" : "0")} WHERE Id={objMaster.Current.Id}";
                        db.ExecuteQuery<int>(allocatedDriverQuery);
                    }
                    catch
                    {
                    }

                    General.MessageToPDA("request broadcast=" + RefreshTypes.REFRESH_REQUIRED_DASHBOARD + "=" + objMaster.Current.Id);


                    try
                    {


                        //try
                        //{
                        //    var driverids = db.Fleet_DriverQueueLists.Where(c => c.Status == true && (c.DriverWorkStatusId==Enums.Driver_WORKINGSTATUS.AVAILABLE || c.DriverWorkStatusId==Enums.Driver_WORKINGSTATUS.ONBREAK))
                        //                                                    .Select(c => c.DriverId).ToList().ToArray<int?>();


                        //    try
                        //    {
                        //        System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "sendtobidding.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",driverids:" + driverids.Count() + Environment.NewLine);
                        //    }
                        //    catch
                        //    {

                        //    }


                        //    PutJobOnBidding(driverids, objMaster.Current.Id, "");
                        //}
                        //catch(Exception ex)
                        //{
                        //    try
                        //    {
                        //        System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "sendtobidding_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",driverids:" + ex.Message + Environment.NewLine);
                        //    }
                        //    catch
                        //    {

                        //    }

                        //}

                        Global.InitializeSMSTags();

                        DateTime? pickupdateTime = objMaster.Current.PickupDateTime;
                        if (HubProcessor.Instance.objPolicy.EnableAdvanceBookingSMSConfirmation.ToBool() && Global.listofSMSTags != null
                            && IsAddMode && pickupdateTime != null && objMaster.Current.IsQuotation.ToBool() == false
                            && (objMaster.Current.CompanyId == null || objMaster.Current.Gen_Company.DefaultIfEmpty().DisableAdvanceText.ToBool() == false))
                        {

                            // string msg=string.Empty;
                            string msg = HubProcessor.Instance.objPolicy.AdvanceBookingSMSText.ToStr().Trim();

                            string advancemsg = msg;



                            //using (TaxiDataContext db = new TaxiDataContext())
                            //{
                            //    msg = db.Gen_SysPolicy_Configurations.FirstOrDefault().AdvanceBookingSMSText.ToStr();
                            //    AppVars.listofSMSTags = db.SMSTags.ToList();
                            //      advancemsg=msg;
                            //}





                            string mobileNo = objMaster.Current.CustomerMobileNo.ToStr().Trim();

                            if (mobileNo.Length > 0)
                            {
                                string pickupSpan = string.Format("{0:HH:mm}", pickupdateTime);

                                TimeSpan picktime = TimeSpan.Parse(pickupSpan);

                                string nowP = string.Format("{0:HH:mm}", DateTime.Now);
                                TimeSpan nowSpantime = TimeSpan.Parse(nowP);

                                int afterMins = HubProcessor.Instance.objPolicy.AdvanceBookingSMSConfirmationMins.ToInt();
                                double minDifference = pickupdateTime.Value.Subtract(DateTime.Now).TotalMinutes;

                                if (afterMins == 0 || minDifference >= afterMins)
                                {
                                    object propertyValue = string.Empty;

                                    foreach (var tag in Global.listofSMSTags.Where(c => msg.Contains(c.TagMemberValue)))
                                    {
                                        switch (tag.TagObjectName)
                                        {
                                            case "booking":

                                                if (tag.TagPropertyValue.Contains('.'))
                                                {

                                                    string[] val = tag.TagPropertyValue.Split(new char[] { '.' });

                                                    object parentObj = objMaster.Current.GetType().GetProperty(val[0]).GetValue(objMaster.Current, null);

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
                                                    propertyValue = objMaster.Current.GetType().GetProperty(tag.TagPropertyValue).GetValue(objMaster.Current, null);
                                                }


                                                if (string.IsNullOrEmpty(propertyValue.ToStr()) && !string.IsNullOrEmpty(tag.TagPropertyValue2))
                                                {
                                                    propertyValue = objMaster.Current.GetType().GetProperty(tag.TagPropertyValue2).GetValue(objMaster.Current, null);
                                                }
                                                break;


                                            case "Booking_ViaLocations":
                                                if (tag.TagPropertyValue == "ViaLocValue")
                                                {


                                                    string[] VilLocs = null;
                                                    int cnt = 1;
                                                    VilLocs = objMaster.Current.Booking_ViaLocations.Select(c => cnt++.ToStr() + ". " + c.ViaLocValue).ToArray();
                                                    if (VilLocs.Count() > 0)
                                                    {

                                                        string Locations = "VIA POINT(s) : \n" + string.Join("\n", VilLocs);
                                                        propertyValue = Locations;
                                                    }
                                                    else
                                                        propertyValue = string.Empty;

                                                }
                                                break;




                                            default:


                                                propertyValue = objMaster.Current.Gen_SubCompany.GetType().GetProperty(tag.TagPropertyValue).GetValue(objMaster.Current.Gen_SubCompany, null);


                                                break;



                                        }


                                        msg = msg.Replace(tag.TagMemberValue,
                                            tag.TagPropertyValuePrefix.ToStr() + string.Format(tag.TagDataFormat, propertyValue) + tag.TagPropertyValueSuffix.ToStr());

                                    }


                                    msg.Replace("\n\n", "\n");

                                    string refMsg = "";


                                    HubProcessor.Instance.listofSMS.Add("request dispatchsms = " + mobileNo.Trim() + " = " + msg);

                                    try
                                    {

                                        if (objMaster.Current.JourneyTypeId.ToInt() == Enums.JOURNEY_TYPES.RETURN && objMaster.Current.BookingReturns.Count > 0)
                                        {
                                            msg = advancemsg;

                                            Booking objReturn = objMaster.Current.BookingReturns[0];

                                            foreach (var tag in Global.listofSMSTags.Where(c => msg.Contains(c.TagMemberValue)))
                                            {
                                                switch (tag.TagObjectName)
                                                {
                                                    case "booking":

                                                        if (tag.TagPropertyValue.Contains('.'))
                                                        {

                                                            string[] val = tag.TagPropertyValue.Split(new char[] { '.' });

                                                            object parentObj = objReturn.GetType().GetProperty(val[0]).GetValue(objReturn, null);

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
                                                            propertyValue = objReturn.GetType().GetProperty(tag.TagPropertyValue).GetValue(objReturn, null);
                                                        }


                                                        if (string.IsNullOrEmpty(propertyValue.ToStr()) && !string.IsNullOrEmpty(tag.TagPropertyValue2))
                                                        {
                                                            propertyValue = objReturn.GetType().GetProperty(tag.TagPropertyValue2).GetValue(objReturn, null);
                                                        }
                                                        break;


                                                    case "Booking_ViaLocations":
                                                        if (tag.TagPropertyValue == "ViaLocValue")
                                                        {


                                                            string[] VilLocs = null;
                                                            int cnt = 1;
                                                            VilLocs = objReturn.Booking_ViaLocations.Select(c => cnt++.ToStr() + ". " + c.ViaLocValue).ToArray();
                                                            if (VilLocs.Count() > 0)
                                                            {

                                                                string Locations = "VIA POINT(s) : \n" + string.Join("\n", VilLocs);
                                                                propertyValue = Locations;
                                                            }
                                                            else
                                                                propertyValue = string.Empty;

                                                        }
                                                        break;


                                                    default:


                                                        propertyValue = objMaster.Current.Gen_SubCompany.GetType().GetProperty(tag.TagPropertyValue).GetValue(objMaster.Current.Gen_SubCompany, null);


                                                        break;



                                                }


                                                msg = msg.Replace(tag.TagMemberValue,
                                                    tag.TagPropertyValuePrefix.ToStr() + string.Format(tag.TagDataFormat, propertyValue) + tag.TagPropertyValueSuffix.ToStr());

                                            }

                                            msg.Replace("\n\n", "\n");


                                            HubProcessor.Instance.listofSMS.Add("request dispatchsms = " + mobileNo.Trim() + " = " + msg);



                                        }
                                    }
                                    catch
                                    {


                                    }







                                }
                            }
                        }
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
                    response.HasError = true;

                    if (objMaster.Errors.Count == 0)
                    {
                        response.Message = ex.Message;
                        //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "SaveBooking_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message + Environment.NewLine);
                        General.WriteLog("SaveBooking_exception", "json:" + new JavaScriptSerializer().Serialize(obj) + "Exception: " + ex.Message);

                    }
                    else
                    {
                        response.Message = objMaster.ShowErrors();
                        //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "SaveBooking_validation.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message + Environment.NewLine);
                        General.WriteLog("SaveBooking_validation", "json: " + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message);
                    }
                }
                catch
                {

                }
            }


            return Json(response, JsonRequestBehavior.AllowGet);

        }


        private void PutJobOnBidding(int?[] driverIds, long jobId, string plot)
        {

            Thread smsThread = new Thread(delegate ()
            {

                SendBidMessage(driverIds, jobId, plot);

            });

            smsThread.Priority = ThreadPriority.Highest;

            smsThread.Start();


        }

        private void SendBidMessage(int?[] driverIds, long jobId, string plot)
        {

            try
            {

                try
                {
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "SendBidMessage.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",driverids:" + driverIds.Count() + Environment.NewLine);
                    General.WriteLog("SendBidMessage", "driverids:" + driverIds.Count());
                }
                catch
                {

                }

                foreach (var item in driverIds)
                {



                    General.requestPDA("request pda=" + item + "=" + jobId + "=" + "Bid Alert>>" + plot + "=6");




                }




            }
            catch (Exception ex)
            {


            }
        }




        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("CancelBooking")]
        public JsonResult CancelBooking(WebApiClasses.RequestWebApi obj)
        {
            //




            ResponseWebApi response = new ResponseWebApi();
            try
            {


                try
                {
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "CancelBooking.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
                    General.WriteLog("CancelBooking", "json: " + new JavaScriptSerializer().Serialize(obj));
                }
                catch
                {

                }
                //
                //   BookingBO objMaster = new BookingBO();
                //    objMaster.GetByPrimaryKey(obj.bookingInfo.Id);

                using (TaxiDataContext db = new TaxiDataContext())
                {
                    Booking objMaster = db.Bookings.FirstOrDefault(c => c.Id == obj.bookingInfo.Id);

                    string reason = obj.bookingInfo.CancelReason.ToStr().Trim();
                    if (string.IsNullOrEmpty(reason))
                    {
                        reason = "XXX";


                    }




                    //     if (objMaster == null || objMaster.Current == null)
                    //    objMaster.GetByPrimaryKey(obj.bookingInfo.Id);




                    bool cancelReturnJob = false;
                    long? returnBookingId = null;

                    if (objMaster.JourneyTypeId.ToInt() == Enums.JOURNEY_TYPES.RETURN)
                    {



                        if (obj.bookingInfo.BookingReturn != null && obj.bookingInfo.BookingReturn.Id > 0)
                        {
                            cancelReturnJob = true;

                        }


                    }


                    //  this._onlineBookingId = objMaster.Current.OnlineBookingId.ToLong();




                    db.stp_CancelBookingWithUserLog(objMaster.Id, reason, obj.bookingInfo.JobCancelledBy, obj.bookingInfo.AddBy);

                    //   jobIds = objMaster.Current.Id.ToStr();
                    if (cancelReturnJob && objMaster.BookingReturns.Count > 0)
                    {
                        returnBookingId = objMaster.BookingReturns[0].Id;
                        db.stp_CancelBookingWithUserLog(returnBookingId, reason, obj.bookingInfo.JobCancelledBy, obj.bookingInfo.AddBy);
                        //    jobIds += "," + returnBookingId.ToStr();
                    }








                    try
                    {
                        int driverId = objMaster.DriverId.ToInt();



                        if (objMaster.BookingStatusId.ToInt() != Enums.BOOKINGSTATUS.WAITING && driverId > 0)
                        {




                            if (db.Fleet_DriverQueueLists.Where(c => c.DriverId == driverId && c.CurrentJobId == obj.bookingInfo.Id).Count() > 0)
                            {

                                General.CancelCurrentBookingFromPDA(obj.bookingInfo.Id, driverId);
                                //   General.SendMessageToPDA("request broadcast=" + RefreshTypes.REFRESH_ACTIVEBOOKINGS_DASHBOARD + "=" + objMaster.Current.Id + "=syncdrivers");

                            }
                            //else
                            //{
                            //    if (objMaster.Current.BookingStatusId.ToInt() == Enums.BOOKINGSTATUS.PENDING_START)
                            //    {

                            //        if (AppVars.objPolicyConfiguration.ShowPendingJobsOnRecentTab.ToBool())
                            //            General.ReCallPreBooking(BookingId, driverId, true);
                            //        else
                            //            General.ReCallPreBooking(BookingId, driverId);

                            //    }
                            //    else
                            //    {

                            //        ReCallFOJBookingFromPDA(BookingId, driverId);

                            //    }


                            //    General.SendMessageToPDA("request broadcast=" + RefreshTypes.REFRESH_ACTIVEBOOKINGS_DASHBOARD + "=" + objMaster.Current.Id + "=syncdrivers");


                            //}

                        }


                        if (objMaster.BookingTypeId.ToInt() == Enums.BOOKING_TYPES.THIRDPARTY)
                        {
                            var json = new { JobId = objMaster.Id, Status = "cancelled", BookingStatusId = objMaster.BookingStatusId.ToInt(), UserName = "", DriverId = driverId, Reason = reason };

                            General.UpdateSupplierStatus(new JavaScriptSerializer().Serialize(json));
                        }



                        if (objMaster != null && objMaster.CustomerMobileNo.ToStr().Length >= 9 && HubProcessor.Instance.objPolicy.SMSCancelJob.ToStr().Trim().Length > 0)
                        {
                            string msg = General.GetMessage(HubProcessor.Instance.objPolicy.SMSCancelJob.ToStr().Trim(), objMaster, objMaster.Id);

                            General.AddSMS(objMaster.CustomerMobileNo.ToStr().Trim(), msg, 1);


                        }

                        //if (objMaster.Current.BookingTypeId.ToInt() == Enums.BOOKING_TYPES.THIRDPARTY)
                        //{
                        //    General.UpdateSupplierStatus(objMaster.Current.Id, objMaster.Current.DriverId.ToInt(), objMaster.Current.BookingStatusId.ToInt(), "cancelled", reason);
                        //}



                        //if (chkCancellationSMS.Checked && objMaster.Current != null && objMaster.Current.CustomerMobileNo.ToStr().Length >= 9 && AppVars.objPolicyConfiguration.SMSCancelJob.ToStr().Trim().Length > 0)
                        //{
                        //    string rtnMsg = string.Empty;
                        //    EuroSMS sms = new EuroSMS();
                        //    sms.Message = GetMessage(AppVars.objPolicyConfiguration.SMSCancelJob.ToStr(), objMaster.Current, objMaster.Current.Id);
                        //    sms.ToNumber = objMaster.Current.CustomerMobileNo.ToStr().Trim();
                        //    sms.Send(ref rtnMsg);
                        //}

                    }
                    catch
                    {



                    }

                    //  SendCancelEmail();


                    //if (objMaster != null && objMaster.Current != null)
                    //{
                    //    if (objMaster.Current.BookingStatusId == 21 || objMaster.Current.BookingStatusId == 25 || objMaster.Current.BookingStatusId == 27)
                    //    {

                    //        new Thread(delegate ()
                    //        {
                    //            try
                    //            {

                    //                JobPool.CancelJob(AppVars.objPolicyConfiguration.DefaultClientId, objMaster.Current.Id, AppVars.LoginObj.UserName.ToStr());

                    //            }
                    //            catch
                    //            {



                    //            }
                    //        }).Start();
                    //    }
                    //}


                    //this.Close();
                    CallGetDashboardData();

                    response.Data = obj.bookingInfo.Id;

                }
            }
            catch (Exception ex)
            {
                try
                {
                    response.HasError = true;
                    response.Message = ex.Message;

                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "CancelBooking_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message + Environment.NewLine);
                    General.WriteLog("CancelBooking_exception", "json: " + new JavaScriptSerializer().Serialize(obj) + ", Exception: " + ex.Message);
                }
                catch
                {

                }
            }


            return Json(response, JsonRequestBehavior.AllowGet);

        }






        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("DispatchBooking")]
        public JsonResult DispatchBooking(WebApiClasses.RequestWebApi obj)
        {
            //




            ResponseWebApi response = new ResponseWebApi();
            try
            {

                //
                try
                {
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "DispatchBooking.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
                    General.WriteLog("DispatchBooking", "json: " + new JavaScriptSerializer().Serialize(obj));
                }
                catch
                {

                }


                string msg = string.Empty;
                using (TaxiDataContext db = new TaxiDataContext())
                {
                    string despatchBy = obj.bookingInfo.Despatchby.ToStr();
                    var pickupDateTime = db.Bookings.Where(c => c.Id == obj.bookingInfo.Id).Select(x => x.PickupDateTime).FirstOrDefault();
                    if (pickupDateTime != null && pickupDateTime?.Date != DateTime.Now.Date && obj.bookingInfo.BookingTypeId.ToInt() != 4 && obj.bookingInfo.BookingTypeId.ToInt() != 3)
                    {
                        //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "DispatchBooking_warning.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",pickup date" + pickupDateTime?.Date.ToStr() + "bookingid: " + obj.bookingInfo.Id.ToStr() + " type: " + obj.bookingInfo.BookingTypeId.ToStr() + Environment.NewLine);
                        try
                        {
                            General.WriteLog("DispatchBooking_warning", "pickup date" + pickupDateTime?.Date.ToStr() + "bookingid: " + obj.bookingInfo.Id.ToStr() + " type: " + obj.bookingInfo.BookingTypeId.ToStr());
                        }

                        catch
                        {
                        }

                        response.HasError = true;
                        response.Message = "Only today bookings can be dispatch.";
                    }
                    else
                    {
                        if (obj.DriverIds != null && obj.DriverIds.Length > 0)
                        {
                            string[] DriverIds = obj.DriverIds;
                            foreach (var item in DriverIds)
                            {
                                General.OnDespatching(HubProcessor.Instance.objPolicy, db.Bookings.FirstOrDefault(c => c.Id == obj.bookingInfo.Id), db.Fleet_Drivers.FirstOrDefault(c => c.Id == int.Parse(item)), obj.bookingInfo.BookingTypeId.ToInt(), true, despatchBy);
                            }
                        }
                        else
                        {

                            bool isExcluded = IsDriverExcludedForBooking(obj.bookingInfo.Id, obj.bookingInfo.DriverId.ToInt());

                            if (isExcluded)
                            {
                                response.HasError = true;
                                response.Message = "This driver is Excluded for this customer";
                            }
                            else
                            {

                                if (obj.bookingInfo.DriverId.ToInt() == 0 && obj.bookingInfo.BookingTypeId.ToInt() != 4)
                                {
                                    response.HasError = true;
                                    response.Message = "Please select a driver";

                                }
                                else
                                {
                                    if (obj.bookingInfo.BookingTypeId.ToInt() == 4)
                                    {

                                        int? driverId = db.Bookings.Where(c => c.Id == obj.bookingInfo.Id).Select(c => c.DriverId).FirstOrDefault().ToIntorNull();

                                        if (driverId == null && obj.bookingInfo.DriverId.ToInt() == 0)
                                        {
                                            response.HasError = true;
                                            response.Message = "Please select a driver";
                                            //
                                        }
                                        else
                                            msg = General.AllocateDriver(db, HubProcessor.Instance.objPolicy, db.Bookings.FirstOrDefault(c => c.Id == obj.bookingInfo.Id), db.Fleet_Drivers.FirstOrDefault(c => c.Id == obj.bookingInfo.DriverId), obj.bookingInfo.BookingTypeId.ToInt());





                                    }
                                    else
                                    {
                                        try
                                        {
                                            if (db.Bookings.Where(x => x.Id == obj.bookingInfo.Id.ToLong() && x.DriverId != obj.bookingInfo.DriverId.ToInt()).Select(x => x.BookingStatusId).FirstOrDefault().ToInt() == 4) //4 = PENDING ACCEPT
                                            {
                                                msg = "Job is already dispatched to other driver.";
                                                try
                                                {
                                                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "DispatchBooking_alreadyacceptedthisdriver.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",jobid:" + obj.bookingInfo.Id + ",driverid:" + obj.bookingInfo.DriverId.ToInt() + Environment.NewLine);
                                                    General.WriteLog("DispatchBooking", "jobid: " + obj.bookingInfo.Id + ", driverid: " + obj.bookingInfo.DriverId.ToInt() );
                                                }
                                                catch
                                                {
                                                }
                                            }
                                            //else if (db.Bookings.Where(x => x.DriverId == obj.bookingInfo.DriverId.ToInt() && x.Id != obj.bookingInfo.Id.ToLong() && x.BookingStatusId == 4).Select(x => x.Id).ToList().Count > 0) //4 = PENDING ACCEPT
                                            //{
                                            //    msg = "Other job is already dispatched to this driver.";
                                            //    try
                                            //    {
                                            //        System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "DispatchBooking_alreadyacceptedthisdriver.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",jobid:" + obj.bookingInfo.Id + ",driverid:" + obj.bookingInfo.DriverId.ToInt() + Environment.NewLine);
                                            //    }
                                            //    catch
                                            //    {
                                            //    }
                                            //}
                                            else if (db.ExecuteQuery<int>("select count(*) from fleet_driverqueuelist (nolock) where driverid=" + obj.bookingInfo.DriverId.ToInt() + " and status=1 and currentjobid=" + obj.bookingInfo.Id.ToLong()).FirstOrDefault() > 0)
                                            {
                                                msg = "Job is already accepted by this driver";
                                                try
                                                {


                                                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "DispatchBooking_alreadyacceptedthisdriver.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",jobid:" + obj.bookingInfo.Id + ",driverid:" + obj.bookingInfo.DriverId.ToInt() + Environment.NewLine);
                                                    General.WriteLog("DispatchBooking_alreadyacceptedthisdriver", "jobid:" + obj.bookingInfo.Id + ",driverid:" + obj.bookingInfo.DriverId.ToInt());
                                                }
                                                catch
                                                {

                                                }


                                            }
                                            else if (db.ExecuteQuery<int>("select count(*) from fleet_driverqueuelist (nolock) where driverid!=" + obj.bookingInfo.DriverId.ToInt() + " and status=1 and currentjobid=" + obj.bookingInfo.Id.ToLong()).FirstOrDefault() > 0)
                                            {

                                                msg = "Job is already accepted by other driver";
                                                try
                                                {


                                                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "DispatchBooking_alreadyacceptedotherdriver.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",jobid:" + obj.bookingInfo.Id + ",driverid:" + obj.bookingInfo.DriverId.ToInt() + Environment.NewLine);
                                                    General.WriteLog("DispatchBooking_alreadyacceptedotherdriver", "jobid:" + obj.bookingInfo.Id + ",driverid:" + obj.bookingInfo.DriverId.ToInt());
                                                }
                                                catch
                                                {

                                                }

                                            }
                                        }
                                        catch
                                        {

                                        }


                                        if (msg.ToStr().Trim().Length == 0)
                                            General.OnDespatching(HubProcessor.Instance.objPolicy, db.Bookings.FirstOrDefault(c => c.Id == obj.bookingInfo.Id), db.Fleet_Drivers.FirstOrDefault(c => c.Id == obj.bookingInfo.DriverId), obj.bookingInfo.BookingTypeId.ToInt(), false, despatchBy);
                                    }

                                    if (msg.ToStr().Length > 0)
                                    {
                                        response.HasError = true;
                                        response.Message = msg;
                                    }
                                }
                            }
                        }
                    }


                }
                //using (TaxiDataContext db = new TaxiDataContext())
                //{
                //    General.OnDespatching(HubProcessor.Instance.objPolicy, db.Bookings.FirstOrDefault(c => c.Id == obj.bookingInfo.Id), db.Fleet_Drivers.FirstOrDefault(c => c.Id == obj.bookingInfo.DriverId), obj.bookingInfo.BookingTypeId.ToInt());

                //}


                response.Data = obj.bookingInfo.Id;
            }
            catch (Exception ex)
            {
                try
                {
                    response.HasError = true;
                    response.Message = ex.Message;

                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "DispatchBooking_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message + Environment.NewLine);
                    General.WriteLog("DispatchBooking_exception", "json: " + new JavaScriptSerializer().Serialize(obj) + ", Exception: " + ex.Message);
                }
                catch
                {

                }
            }


            return Json(response, JsonRequestBehavior.AllowGet);

        }

        private bool IsDriverExcludedForBooking(long bookingId, int? driverId)
        {
            using (TaxiDataContext db = new TaxiDataContext())
            {
                try
                {
                    var booking = db.Bookings.FirstOrDefault(x => x.Id == bookingId);
                    if (driverId > 0)
                    {
                        var drivers = db.ExecuteQuery<ExcludedFleetDriver>(
                            "exec stp_GetCustomerAndAccountExcludedDrivers @MobileNo={0}",
                            booking.CustomerMobileNo
                        ).ToList();

                        bool isExcluded = false;

                        if (booking != null)
                        {
                            isExcluded = drivers.Any(d => d.Id == driverId);
                        }
                        return isExcluded;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }

        }


        #endregion











        #region Phase#2


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetTrackingDriversData")]
        public JsonResult GetTrackingDriversData(WebApiClasses.RequestWebApi obj)
        {
            //

            ResponseWebApi response = new ResponseWebApi();

            try
            {

                ClsDashboardModel data = new ClsDashboardModel();
                //using (TaxiDataContext db = new TaxiDataContext("Data Source=88.198.21.250,58527;Initial Catalog=AscotCars;User ID=asc321;Password=asc321!;Trusted_Connection=False;"))
                //{
                using (TaxiDataContext db = new TaxiDataContext())
                {


                    response.Data = db.stp_GetLoginDriverPlotsUpdated().ToList();




                }



                //using (TaxiDataContext db = new TaxiDataContext("Data Source=88.198.21.250,58527;Initial Catalog=AscotCars;User ID=asc321;Password=asc321!;Trusted_Connection=False;"))
                //{






                //      data.listofdrivers = db.stp_GetDashboardDrivers(1).ToList();

                //    response.Data = data;




                //}

            }
            catch
            {

                response.HasError = true;
                response.Message = "exception occured";
            }



            return new CustomJsonResult { Data = response };
        }





        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetCallerIdHistory")]
        public JsonResult GetCallerIdHistory(WebApiClasses.RequestWebApi obj)
        {
            ResponseWebApi response = new ResponseWebApi();
            try
            {
                CustomerHistoryModel model = new CustomerHistoryModel();
                using (TaxiDataContext db = new TaxiDataContext())
                {
                    string phoneNumber = obj.callerInfo.PhoneNumber.ToStr();
                    var historyList = new List<stp_getcustomerhistoryResultEx>();
                    var pendingBookings = new List<WaitingCurrentHistoryList>();
                    var completedBookings = new List<WaitingCurrentHistoryList>();

                    using (SqlConnection conn = new SqlConnection(db.Connection.ConnectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("stp_getcustomerhistory_Update", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                            conn.Open();
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                // First result set: History
                                while (reader.Read())
                                {
                                    historyList.Add(new stp_getcustomerhistoryResultEx
                                    {
                                        FromTypeId = reader["FromTypeId"] as int?,
                                        FromId = reader["FromId"] as int?,
                                        From = reader["From"] as string,
                                        ViaString = reader["ViaString"] as string,
                                        To = reader["To"] as string,
                                        ToId = reader["ToId"] as int?,
                                        ToTypeId = reader["ToTypeId"] as int?,
                                        Fare = reader["Fare"] as decimal?,
                                        Companyid = reader["Companyid"] as int?,
                                        VehicleTypeId = Convert.ToInt32(reader["VehicleTypeId"]),
                                        FromDoorNo = reader["FromDoorNo"] as string,
                                        ToDoorNo = reader["ToDoorNo"] as string,
                                        PickupDate = reader["PickupDate"] as DateTime?,
                                        CancelledJobCount = reader["CancelledJobCount"] as int?,
                                        NoPickupJobCount = reader["NoPickupJobCount"] as int?,
                                        Completed = reader["Completed"] as int?
                                    });
                                }
                                // Second result set: Waiting bookings
                                if (reader.NextResult())
                                {
                                    while (reader.Read())
                                    {
                                        pendingBookings.Add(new WaitingCurrentHistoryList
                                        {
                                            BookingNo = reader["BookingNo"] as string,
                                            PickupDateTime = Convert.ToDateTime(reader["PickupDateTime"]),
                                            FromAddress = reader["FromAddress"] as string,
                                            ToAddress = reader["ToAddress"] as string
                                        });
                                    }
                                }
                                // Third result set: OnRoute bookings
                                if (reader.NextResult())
                                {

                                    while (reader.Read())
                                    {
                                        completedBookings.Add(new WaitingCurrentHistoryList
                                        {
                                            BookingNo = reader["BookingNo"] as string,
                                            PickupDateTime = Convert.ToDateTime(reader["PickupDateTime"]),
                                            FromAddress = reader["FromAddress"] as string,
                                            ToAddress = reader["ToAddress"] as string
                                        });
                                    }
                                }
                            }
                        }
                    }
                    model.HistoryList = historyList;
                    model.WaitingList = pendingBookings;
                    model.OnRoute = completedBookings;

                    if (model.HistoryList != null && model.HistoryList.Count > 0)
                    {
                        model.Cancelled = model.HistoryList.Sum(c => c.CancelledJobCount ?? 0);
                        model.Used = model.HistoryList.Sum(c => c.Completed ?? 0);
                        model.NoFares = model.HistoryList.Sum(c => c.NoPickupJobCount ?? 0);
                    }
                    try
                    {
                        db.CommandTimeout = 4;
                        var objCustomer = db.stp_GetCallerInfo(phoneNumber, "").FirstOrDefault();
                        if (objCustomer != null)
                        {
                            model.CustomerName = objCustomer.Name.ToStr();
                            model.Address = objCustomer.Address1.ToStr().Trim();
                            model.DoorNo = objCustomer.DoorNo.ToStr().Trim();
                            model.Email = objCustomer.Email.ToStr().Trim();
                            model.Notes = objCustomer.Notes.ToStr().Trim();
                            model.IsBlackListed = objCustomer.BlackList.ToBool();
                            model.BlackListReason = objCustomer.BlackListResion.ToStr().Trim();
                            model.IsAccount = objCustomer.IsAccount.ToBool();
                            model.AccountId = objCustomer.AccountId;
                            model.ExcludedDriverIds = objCustomer.ExcludedDriverIds.ToStr().Trim();
                            model.SubCompanyId = objCustomer.SubCompanyId.ToInt();
                            model.SubCompanyName = objCustomer.SubCompanyName.ToStr();

                            if (model.ExcludedDriverIds.ToStr().Trim().Length > 0 && model.ExcludedDriverIds.ToStr().Trim().Contains(",") == false)
                                model.ExcludedDriverIds = "," + model.ExcludedDriverIds + ",";

                        }
                    }
                    catch
                    {

                    }

                    response.Data = model;
                }

            }
            catch (Exception ex)
            {
                response.HasError = true;
                response.Message = ex.Message;
            }
            return new CustomJsonResult { Data = response };
        }




        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetAddressDetails")]
        public JsonResult GetAddressDetails(WebApiClasses.RequestWebApi obj)
        {
            int loctypeId = 0;
            //
            try
            {
                //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetAddressDetails.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + obj.addressInfo.Address.ToStr().ToUpper().Trim() + Environment.NewLine);
                General.WriteLog("GetAddressDetails", "json: " + obj.addressInfo.Address.ToStr().ToUpper().Trim());
            }
            catch
            {
            }
            ResponseWebApi response = new ResponseWebApi();

            try
            {
                using (TaxiDataContext db = new TaxiDataContext())
                {
                    string searchValue = obj.addressInfo.Address.ToStr().ToUpper().Trim();
                    try
                    {
                        var objLoc = db.Gen_Locations.FirstOrDefault(c => c.Address.ToUpper() == searchValue || c.FullLocationName.ToUpper() == searchValue);
                        if (obj.addressInfo.locTypeId > 0)
                        {
                            loctypeId = obj.addressInfo.locTypeId;
                        }
                        else
                        {
                            loctypeId = Enums.LOCATION_TYPES.ADDRESS;
                        }
                        //   double? latitude = null;
                        //  double? longitude = null;
                        double? latitude = obj.addressInfo.Latitude;
                        double? longitude = obj.addressInfo.Longitude;

                        if (objLoc != null)
                        {
                            if (objLoc.LocationTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT)
                                loctypeId = objLoc.LocationTypeId.ToInt();

                            if (objLoc.Latitude != null && objLoc.Latitude != 0)
                            {
                                obj.addressInfo.Latitude = objLoc.Latitude;
                                obj.addressInfo.Longitude = objLoc.Longitude;
                                latitude = objLoc.Latitude;
                                longitude = objLoc.Longitude;
                            }
                        }

                        obj.addressInfo.locTypeId = loctypeId;



                        if (latitude == null || latitude == 0)
                        {
                            //var loc1 = General.GetLocationCoordByDisplayPosition(searchValue);
                            var loc = db.stp_getCoordinatesByAddress(searchValue, General.GetPostCodeMatch(searchValue)).FirstOrDefault();

                            if (loc != null && loc.Latitude != 0)
                            {
                                obj.addressInfo.Latitude = loc.Latitude;
                                obj.addressInfo.Longitude = loc.Longtiude;

                                latitude = loc.Latitude;
                                longitude = loc.Longtiude;
                            }
                        }

                        obj.addressInfo.zoneId = General.GetZoneId(searchValue);//General.GetZoneId(latitude + "," + longitude);
                        // GetZoneIDTime zone = General.GetZoneIdAndTime(latitude + "," + longitude);

                        //    obj.addressInfo.zoneId = General.GetZoneId(latitude + "," + longitude);
                        //  obj.addressInfo.leadZoneDueTime = zone.leadZoneDueTime;

                        if (obj.addressInfo.zoneId.ToInt() > 0)
                        {
                            var ZoneInfo = db.Gen_Zones.Where(c => c.Id == obj.addressInfo.zoneId).Select(c => new { ZoneName = c.ZoneName, JobDueTime = c.JobDueTime }).FirstOrDefault();
                            obj.addressInfo.zoneName = ZoneInfo.ZoneName;
                            if (Global.EnableManualLeadTime == "true")
                            {
                                if (ZoneInfo.JobDueTime != null)
                                {
                                    int hour = ZoneInfo.JobDueTime.Value.Hour;
                                    int min = ZoneInfo.JobDueTime.Value.Minute;

                                    obj.addressInfo.JobDueTime = ((hour * 60) + min);
                                }
                            }
                        }

                        //if (obj.addressInfo.AddressType.ToInt() == 1 && latitude != null && latitude != 0)
                        if (latitude != null && latitude != 0)
                        {
                            try
                            {

                                var ListofAvailDrvs = (from a in db.GetTable<Fleet_DriverQueueList>().Where(c => c.Status == true &&
                                            (c.DriverWorkStatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE))
                                                       join b in db.GetTable<Fleet_Driver_Location>().Where(c => c.Latitude != 0)

                                                       on a.DriverId equals b.DriverId

                                                       join d in db.GetTable<Fleet_Driver>() on a.DriverId equals d.Id

                                                       select new
                                                       {
                                                           DriverId = a.DriverId,
                                                           DriverNo = d.DriverNo,
                                                           DriverLocation = b.LocationName,
                                                           Latitude = b.Latitude,
                                                           Longitude = b.Longitude,
                                                           // NoofPassengers = a.Fleet_Driver.Fleet_VehicleType.NoofPassengers
                                                       }).ToList();

                                var nearestDrivers = ListofAvailDrvs.Select(args => new
                                {
                                    args.DriverId,

                                    MilesAwayFromPickup = new DotNetCoords.LatLng(args.Latitude, args.Longitude).DistanceMiles(new DotNetCoords.LatLng(Convert.ToDouble(latitude), Convert.ToDouble(longitude))),
                                    args.DriverNo,
                                    Latitude = args.Latitude,
                                    Longitude = args.Longitude,
                                    Location = args.DriverLocation

                                }).OrderBy(args => args.MilesAwayFromPickup)
                                .Take(3).ToList();
                                //List<RouteCoords> coords;
                                for (int i = 0; i < nearestDrivers.Count; i++)
                                {
                                    string time = string.Empty;

                                    time = General.GetETATime(nearestDrivers[i].Latitude + "," + nearestDrivers[i].Longitude, Convert.ToDouble(latitude) + "," + Convert.ToDouble(longitude), "").ToStr();

                                    //obj.addressInfo.drvlatlong.Add(nearestDrivers[i].Latitude + "," + nearestDrivers[i].Longitude);

                                    if (obj.addressInfo.NearestDrivers == null)
                                        obj.addressInfo.NearestDrivers = new List<string>();

                                    obj.addressInfo.NearestDrivers.Add("Drv " + nearestDrivers[i].DriverNo + " - " + time + "_LatLng" + nearestDrivers[i].Latitude + "," + nearestDrivers[i].Longitude);
                                }
                            }
                            catch (Exception ex)
                            {
                                try
                                {
                                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetAddressDetails_exceptionnearestdriver.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj.addressInfo) + ",exception:" + ex.Message + Environment.NewLine);
                                    General.WriteLog("GetAddressDetails_exceptionnearestdriver", "json:" + new JavaScriptSerializer().Serialize(obj.addressInfo) + ", Exception: " + ex.Message);
                                }
                                catch
                                {
                                }
                            }
                        }
                        response.Data = obj.addressInfo;
                        //
                    }
                    catch (Exception ex)
                    {
                        //   AddExcepLog("POIWORKER_DOWORK : " + ex.Message);
                        //     Console.WriteLine("Start work catch: " + searchValue);

                    }
                }
                try
                {
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetAddressDetails_response.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj.addressInfo) + Environment.NewLine);
                    General.WriteLog("GetAddressDetails_response", "json:" + new JavaScriptSerializer().Serialize(obj.addressInfo));
                }
                catch
                {
                }
            }
            catch
            {
                response.HasError = true;
                response.Message = "exception occured";
            }
            return new CustomJsonResult { Data = response };
        }

        //[System.Web.Http.HttpGet]
        //[System.Web.Http.HttpPost]
        //[System.Web.Http.Route("GetAddressDetails")]
        //public JsonResult GetAddressDetails(WebApiClasses.RequestWebApi obj)
        //{
        //    //

        //    ResponseWebApi response = new ResponseWebApi();

        //    try
        //    {



        //        using (TaxiDataContext db = new TaxiDataContext())
        //        {

        //            string searchValue = obj.addressInfo.Address.ToStr().ToUpper().Trim();
        //            try
        //            {





        //                int loctypeId = db.Gen_Locations.FirstOrDefault(c => c.Address.ToUpper() == searchValue || c.FullLocationName.ToUpper() == searchValue).DefaultIfEmpty().LocationTypeId.ToInt();


        //                if (loctypeId == 0)
        //                    loctypeId = Enums.LOCATION_TYPES.ADDRESS;

        //                if (loctypeId != 1 && loctypeId != 7)
        //                    loctypeId = Enums.LOCATION_TYPES.ADDRESS;

        //                obj.addressInfo.locTypeId = loctypeId;
        //                obj.addressInfo.zoneId = General.GetZoneId(searchValue);

        //                if (obj.addressInfo.zoneId.ToInt() > 0)
        //                {
        //                    obj.addressInfo.zoneName = db.Gen_Zones.Where(c => c.Id == obj.addressInfo.zoneId).Select(c => c.ZoneName).FirstOrDefault();

        //                }
        //                var coord = db.stp_getCoordinatesByAddress(searchValue, General.GetPostCodeMatch(searchValue)).FirstOrDefault();


        //                if (coord != null)
        //                {
        //                    obj.addressInfo.Latitude = coord.Latitude;
        //                    obj.addressInfo.Longitude = coord.Longtiude;

        //                }


        //                response.Data = obj.addressInfo;

        //                //



        //            }
        //            catch (Exception ex)
        //            {
        //                //   AddExcepLog("POIWORKER_DOWORK : " + ex.Message);
        //                //     Console.WriteLine("Start work catch: " + searchValue);

        //            }








        //        }





        //    }
        //    catch
        //    {

        //        response.HasError = true;
        //        response.Message = "exception occured";
        //    }



        //    return new CustomJsonResult { Data = response };
        //}


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("CalculateRoute")]
        public JsonResult CalculateRoute(WebApiClasses.RequestWebApi obj)
        {
            //

            ResponseWebApi response = new ResponseWebApi();

            try
            {




                using (TaxiDataContext db = new TaxiDataContext())
                {
                    string pickup = obj.routeInfo.pickupAddress.Address.ToStr().ToUpper().Trim();
                    string destination = obj.routeInfo.destinationAddress.Address.ToStr().ToUpper().Trim();
                    if (HubProcessor.Instance.objPolicy.BookingInterval.ToInt() == 1)
                    {
                        var hereMapResponse = General.getHeremapJourneyLocationCordinates(pickup, destination);
                        if (hereMapResponse != null)
                        {
                            //pickup
                            obj.routeInfo.pickupAddress.Latitude = hereMapResponse.pickup?.DisplayPosition?.Latitude;
                            obj.routeInfo.pickupAddress.Longitude = hereMapResponse.pickup?.DisplayPosition?.Longitude;
                            //destination
                            obj.routeInfo.destinationAddress.Latitude = hereMapResponse.dropOff?.DisplayPosition?.Latitude;
                            obj.routeInfo.destinationAddress.Longitude = hereMapResponse.dropOff?.DisplayPosition?.Longitude;
                        }
                    }


                    if (obj.routeInfo.pickupAddress.Latitude == null)
                    {
                        var coord = db.stp_getCoordinatesByAddress(pickup, General.GetPostCodeMatch(pickup)).FirstOrDefault();


                        if (coord != null)
                        {
                            obj.routeInfo.pickupAddress.Latitude = coord.Latitude;
                            obj.routeInfo.pickupAddress.Longitude = coord.Longtiude;

                        }
                    }




                    //if (obj.routeInfo.viaAddresses!=null && obj.routeInfo.viaAddresses[0].Latitude!=null)
                    //{



                    //    var coord = db.stp_getCoordinatesByAddress(pickup, General.GetPostCodeMatch(pickup)).FirstOrDefault();


                    //    if (coord != null)
                    //    {
                    //        obj.routeInfo.pickupAddress.Latitude = coord.Latitude;
                    //        obj.routeInfo.pickupAddress.Longitude = coord.Longtiude;

                    //    }
                    //}


                    if (obj.routeInfo.destinationAddress.Latitude == null)
                    {
                        var coord = db.stp_getCoordinatesByAddress(destination, General.GetPostCodeMatch(destination)).FirstOrDefault();


                        if (coord != null)
                        {

                            obj.routeInfo.destinationAddress.Latitude = coord.Latitude;
                            obj.routeInfo.destinationAddress.Longitude = coord.Longtiude;

                        }
                    }

                    if (HubProcessor.Instance.objPolicy.BookingInterval.ToInt() == 1)
                    {
                        string newPickup = pickup;
                        string newDest = destination;
                        if (!string.IsNullOrEmpty(obj.routeInfo.pickupAddress.Latitude.ToStr()))
                        {
                            newPickup = "";
                        }
                        if (!string.IsNullOrEmpty(obj.routeInfo.destinationAddress.Latitude.ToStr()))
                        {
                            newDest = "";
                        }
                        var hereMapResponse = General.getHeremapJourneyLocationCordinates(newPickup, newDest);
                        if (hereMapResponse != null)
                        {
                            if (newPickup != "")
                            {


                                //pickup
                                obj.routeInfo.pickupAddress.Latitude = hereMapResponse.pickup?.DisplayPosition?.Latitude;
                                obj.routeInfo.pickupAddress.Longitude = hereMapResponse.pickup?.DisplayPosition?.Longitude;
                            }
                            //destination
                            if (newDest != "")
                            {

                                obj.routeInfo.destinationAddress.Latitude = hereMapResponse.dropOff?.DisplayPosition?.Latitude;
                                obj.routeInfo.destinationAddress.Longitude = hereMapResponse.dropOff?.DisplayPosition?.Longitude;
                            }
                        }
                    }

                    try
                    {
                        string vias = "";
                        if (obj.routeInfo.viaAddresses != null)
                        {

                            foreach (var item in obj.routeInfo.viaAddresses)
                            {
                                var coord = db.stp_getCoordinatesByAddress(item.Address.ToStr().Trim(), General.GetPostCodeMatch(item.Address.ToStr().Trim())).FirstOrDefault();
                                vias += coord.Latitude + "," + coord.Longtiude + "|";
                            }
                            vias = vias.Remove(vias.Length - 1, 1);
                        }

                        string KEY = "avsHjHri-tP5Su5wV7xyPBWwmdqOtEKK2Atn0xgDnrM";


                        if (HubProcessor.Instance.objPolicy.MapType.ToInt() == 1)
                            KEY = db.ExecuteQuery<string>("select APIKey from mapkeys where maptype='google'").FirstOrDefault().ToStr().Trim();

                        string routeType = "short";
                        if (db.ExecuteQuery<string>("Select SetVal from AppSettings WHERE SetKey ='EnableFastestRoute'").FirstOrDefault().ToStr().Trim() == "1")
                        {
                            routeType = "fastest";
                        }
                        bool HasDeadMileage = false;
                        if (db.ExecuteQuery<string>("Select SetVal from AppSettings WHERE SetKey ='HasDeadMileage'").FirstOrDefault().ToStr().Trim() == "true")
                        {
                            HasDeadMileage = true;
                        }
                        var objX = new
                        {
                            originLat = Convert.ToDouble(obj.routeInfo.pickupAddress.Latitude),
                            originLng = Convert.ToDouble(obj.routeInfo.pickupAddress.Longitude),
                            destLat = Convert.ToDouble(obj.routeInfo.destinationAddress.Latitude),
                            destLng = Convert.ToDouble(obj.routeInfo.destinationAddress.Longitude),
                            defaultclientid = HubProcessor.Instance.objPolicy.DefaultClientId.ToStr(),
                            keys = KEY,
                            MapType = HubProcessor.Instance.objPolicy.MapType.ToInt(),
                            sourceType = "hubapi",
                            routeType = routeType,
                            vias = vias,
                            HasDeadMileage
                            //vias = obj.routeInfo.viaAddresses.Select(args => new {Via=args.Latitude +","+args.Longitude })
                        };


                        string json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(objX);
                        //string API = "http://localhost/GENERAL_WEBAPI/Home/GetRouteDetails" + "?json=" + json;
                        string API = "https://www.treasureonlineapi.co.uk/CabTreasureWebApi/Home/GetRouteDetails" + "?json=" + json;


                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(API);
                        request.ContentType = "application/json; charset=utf-8";
                        request.Accept = "application/json";
                        request.Method = WebRequestMethods.Http.Post;
                        request.Proxy = null;
                        request.ContentLength = 0;



                        using (WebResponse responsea = request.GetResponse())
                        {

                            using (StreamReader sr = new StreamReader(responsea.GetResponseStream()))
                            {
                                response.Data = sr.ReadToEnd();
                            }
                        }



                        RouteCoordinates route = Newtonsoft.Json.JsonConvert.DeserializeObject<RouteCoordinates>(response.Data.ToStr());

                        obj.routeInfo.Distance = route.Distance;
                        obj.routeInfo.HasDeadMileage = route.HasDeadMileage;
                        obj.routeInfo.legs = route.legs;
                        if (obj.routeInfo.AutoCalculateFares.ToBool() && pickup.ToStr().Trim().Length > 0 && destination.ToStr().Trim().Length > 0)
                        {
                            //if (obj.routeInfo.Noofhours > 0)
                            //{
                            //    if (obj.routeInfo.VehicleTypeId == -1)
                            //    {
                            //        route.fareModel = CalculateFaresByFixedHoursAllVehicle(obj);
                            //    }
                            //    else
                            //    {
                            //        route.fareModel = CalculateFaresByFixedHours(obj);
                            //    }
                            //}
                            //else
                            //{
                            //    if (obj.routeInfo.VehicleTypeId == -1)
                            //    {
                            //        route.fareModel = CalculateFaresAllVehicle(obj);
                            //    }
                            //    else
                            //    {
                            //        route.fareModel = CalculateFares(obj);
                            //    }
                            //}
                            route.fareModel = CalculateFares(obj);
                        }
                        if (obj.routeInfo.DriverId > 0)
                        {
                            var tempDriver = db.Fleet_Drivers
                                .Where(x => x.Id == obj.routeInfo.DriverId)
                                .Select(x => new
                                {
                                    x.Id,
                                    x.DriverTypeId,
                                    x.DriverCommissionPerBooking,
                                    x.DriverMonthlyRent
                                })
                                .FirstOrDefault();

                            if (tempDriver != null)
                            {
                                // Manually create detached Fleet_Driver
                                var driver = new Fleet_Driver
                                {
                                    Id = tempDriver.Id,
                                    DriverTypeId = tempDriver.DriverTypeId,
                                    DriverCommissionPerBooking = tempDriver.DriverCommissionPerBooking,
                                    DriverMonthlyRent = tempDriver.DriverMonthlyRent
                                };

                                route.Driver = driver;
                            }
                        }

                        if (obj.routeInfo.ReturnDriverId > 0)
                        {
                            var tempDriver = db.Fleet_Drivers
                                .Where(x => x.Id == obj.routeInfo.ReturnDriverId)
                                .Select(x => new
                                {
                                    x.Id,
                                    x.DriverTypeId,
                                    x.DriverCommissionPerBooking,
                                    x.DriverMonthlyRent
                                })
                                .FirstOrDefault();

                            if (tempDriver != null)
                            {
                                // Manually create detached Fleet_Driver
                                var driver = new Fleet_Driver
                                {
                                    Id = tempDriver.Id,
                                    DriverTypeId = tempDriver.DriverTypeId,
                                    DriverCommissionPerBooking = tempDriver.DriverCommissionPerBooking,
                                    DriverMonthlyRent = tempDriver.DriverMonthlyRent
                                };

                                route.ReturnDriver = driver;
                            }
                        }



                        response.Data = route;
                    }
                    catch (Exception ex)
                    {

                    }









                }


                return new CustomJsonResult { Data = response };


            }
            catch
            {

                response.HasError = true;
                response.Message = "exception occured";
            }



            return new CustomJsonResult { Data = response };
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("MoveDriver")]
        public JsonResult MoveDriver(WebApiClasses.RequestWebApi obj)
        {
            ResponseWebApi response = new ResponseWebApi();
            List<string> listofErrors = new List<string>();
            string DirectionToMove = obj.DirectionToMove;
            int MoveToDriverId = obj.MoveToDriverId;

            if (obj.bookingInfo.DriverId > 0 && MoveToDriverId > 0)
            {
                try
                {
                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        db.stp_SwapDriverRank(obj.bookingInfo.DriverId, MoveToDriverId, DirectionToMove.ToLower());
                    }
                    General.CallGetDashboardDriversData();
                }
                catch (Exception ex)
                {
                    try
                    {
                        response.HasError = true;
                        response.Message = ex.Message;

                        //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "MoveDriver_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message + Environment.NewLine);                        
                        General.WriteLog("MoveDriver_exception", "json:" + new JavaScriptSerializer().Serialize(obj) + ", Exception: " + ex.Message);
                    }
                    catch
                    {

                    }
                }
            }

            return new CustomJsonResult { Data = response };
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("CalculateFares")]
        public JsonResult CalculateFares(WebApiClasses.RequestWebApi obj)
        {
            //

            try
            {


                //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "CalculateFares.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
                General.WriteLog("CalculateFares", "json: " + new JavaScriptSerializer().Serialize(obj));
            }
            catch
            {

            }

            ResponseWebApi response = new ResponseWebApi();

            try
            {




                using (TaxiDataContext db = new TaxiDataContext())
                {
                    //

                    try
                    {


                        //
                        //
                        BookingInformation info = new BookingInformation();
                        info.FromAddress = obj.routeInfo.pickupAddress.Address.ToStr().Trim();


                        //var objCoor = db.stp_getCoordinatesByAddress(booking.FromAddress.ToStr().ToUpper(), GetPostCodeMatch(booking.FromAddress.ToStr().ToUpper())).FirstOrDefault();

                        //if (objCoor != null && objCoor.Latitude != null)
                        //    info.fromLatLng = objCoor.Latitude + "," + objCoor.Longtiude;

                        //objCoor = db.stp_getCoordinatesByAddress(booking.ToAddress.ToStr().ToUpper(), GetPostCodeMatch(booking.ToAddress.ToStr().ToUpper())).FirstOrDefault();

                        //if (objCoor != null && objCoor.Latitude != null)
                        //    info.toLatLng = objCoor.Latitude + "," + objCoor.Longtiude;
                        //
                        if (obj.routeInfo.pickupAddress.locTypeId == 0)
                        {
                            string postcode = General.GetPostCodeMatch(obj.routeInfo.pickupAddress.ToStr().Trim().ToUpper());
                            if (db.Gen_Locations.Where(c => c.LocationTypeId == 1 && c.PostCode == postcode).Count() > 0)
                            {
                                obj.routeInfo.pickupAddress.locTypeId = 1;
                            }

                        }

                        if (obj.routeInfo.destinationAddress.locTypeId == 0)
                        {
                            string postcode = General.GetPostCodeMatch(obj.routeInfo.destinationAddress.ToStr().Trim().ToUpper());
                            if (db.Gen_Locations.Where(c => c.LocationTypeId == 1 && c.PostCode == postcode).Count() > 0)
                            {
                                obj.routeInfo.destinationAddress.locTypeId = 1;
                            }

                        }

                        info.MapType = HubProcessor.Instance.objPolicy.MapType.ToInt();
                        info.ToAddress = obj.routeInfo.destinationAddress.Address.ToStr().Trim();
                        info.FromType = obj.routeInfo.pickupAddress.locTypeId == 1 ? "airport" : "address";
                        info.ToType = obj.routeInfo.destinationAddress.locTypeId == 1 ? "airport" : "address";
                        info.CompanyId = obj.routeInfo.CompanyId.ToInt();
                        //info.PickupDateTime = string.Format("{0:dd/MM/yyyy HH:mm}", DateTime.Now);

                        //if (info.PickupDateTime != null)
                        //    info.PickupDateTime = string.Format("{0:dd/MM/yyyy HH:mm}", obj.routeInfo.PickupDateTime);
                        //  info.RouteCoordinates = "-1";
                        // info.VehicleTypeId = obj.routeInfo.VehicleTypeId.ToInt();

                        //
                        try
                        {
                            info.VehicleTypeId = -1;
                            info.Vehicle = db.Fleet_VehicleTypes.Where(c => c.Id == obj.routeInfo.VehicleTypeId).Select(c => c.VehicleType).FirstOrDefault().ToStr();
                        }
                        catch
                        {
                            info.VehicleTypeId = obj.routeInfo.VehicleTypeId.ToInt();
                        }
                        try
                        {
                            info.ReturnVehicleTypeId = -1;
                            info.ReturnVehicle = db.Fleet_VehicleTypes.Where(c => c.Id == obj.routeInfo.ReturnVehicleTypeId).Select(c => c.VehicleType).FirstOrDefault().ToStr();
                        }
                        catch
                        {
                            info.ReturnVehicleTypeId = obj.routeInfo.ReturnVehicleTypeId.ToInt();
                        }



                        //  info.Via = booking.Via;
                        //


                        if (obj.routeInfo.PickupDateTime != null)
                            info.PickupDateTime = string.Format("{0:dd-MMM-yyyy HH:mm}", obj.routeInfo.PickupDateTime);
                        else
                            info.PickupDateTime = string.Format("{0:dd/MM/yyyy HH:mm}", DateTime.Now);



                        if (obj.routeInfo.returnPickupDateTime != null)
                            info.returnPickupDateTime = string.Format("{0:dd-MMM-yyyy HH:mm}", obj.routeInfo.returnPickupDateTime);
                        else
                            info.returnPickupDateTime = string.Format("{0:dd/MM/yyyy HH:mm}", DateTime.Now);

                        if (obj.routeInfo.viaAddresses != null)
                        {

                            info.Via = (from a in obj.routeInfo.viaAddresses select new ViaAddresses { Viaaddress = a.Address.ToStr().Trim(), Viatype = "address" }).ToArray();

                        }
                        //  info.Via = booking.Via;
                        //
                        info.Mileage = obj.routeInfo.Distance;
                        info.Miles = info.Mileage.ToStr();

                        info.Duration = obj.routeInfo.Duration;
                        info.Noofhours = obj.routeInfo.Noofhours;
                        string paymentType = "cash";
                        if (obj.routeInfo.PaymentTypeId.ToInt() > 0)
                        {
                            paymentType = db.Gen_PaymentTypes.Where(x => x.Id == obj.routeInfo.PaymentTypeId).Select(c => c.PaymentType).FirstOrDefault().ToStr();
                        }
                        info.PaymentType = paymentType.Trim();

                        if (info.MapType == 1)
                            info.MapKey = db.ExecuteQuery<string>("select APIKey from mapkeys where maptype='google'").FirstOrDefault().ToStr().Trim();
                        else
                            info.MapKey = db.ExecuteQuery<string>("select APIKey from mapkeys where maptype='here'").FirstOrDefault().ToStr().Trim();

                        ////
                        if (obj.routeInfo.SubCompanyId.ToInt() > 0)
                        {
                            info.SubCompanyId = obj.routeInfo.SubCompanyId;
                        }

                        //    info.Vehicle = db.Fleet_VehicleTypes.Where(a => a.Id == info.VehicleTypeId).Select(a => a.VehicleType).FirstOrDefault().ToStr();
                        //needtouncomment
                        //try
                        //{
                        //    AppAPISer.AppAPISoapClient c = new AppAPISer.AppAPISoapClient();
                        //    string data = c.GetAllFaresFromDispatch(HubProcessor.Instance.objPolicy.DefaultClientId.ToStr(), "LOCAL", new JavaScriptSerializer().Serialize(info), HubProcessor.Instance.objPolicy.DefaultClientId.ToStr() + "4321orue");
                        //    data = data.Replace("\\", "");
                        //    int startIndex = data.IndexOf("[{") + 1;

                        //    //
                        //    data = data.Substring(startIndex);
                        //    int lastIndex = data.IndexOf("}]") + 1;
                        //    data = data.Substring(0, lastIndex);

                        //    response.Data = Newtonsoft.Json.JsonConvert.DeserializeObject<ClsDispatchFares>(data);
                        //}
                        //catch
                        //{

                        //GETALLFARESFROMDISPATCHNEW


                        var url = "https://www.treasureonlineapi.co.uk/CabTreasureWebApi/Home/GETALLFARESFROMDISPATCHNEW";
                        var requestData = new
                        {

                            defaultclientId = HubProcessor.Instance.objPolicy.DefaultClientId.ToStr(),
                            bookingInformation = info
                        };
                        var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Method = "POST";
                        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                        {
                            string json = new JavaScriptSerializer().Serialize(requestData);
                            streamWriter.Write(json);
                            streamWriter.Flush();
                            streamWriter.Close();
                        }
                        String result = "";
                        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            result = streamReader.ReadToEnd();



                        }
                        try
                        {


                            //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GETALLFARESFROMDISPATCHNEW.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(result) + Environment.NewLine);
                            General.WriteLog("GETALLFARESFROMDISPATCHNEW", "json: " + new JavaScriptSerializer().Serialize(result));
                        }
                        catch
                        {

                        }


                        result = result.Replace("\\", "");
                        int startIndex = result.IndexOf("[{") + 1;

                        //
                        result = result.Substring(startIndex);
                        int lastIndex = result.IndexOf("}]") + 1;
                        result = result.Substring(0, lastIndex);

                        var res = Newtonsoft.Json.JsonConvert.DeserializeObject<ClsDispatchFares>(result);
                        //try
                        //{
                        //    var FareSett = CalculateFareSetting(obj, res);
                        //    res.Fare = FareSett.fareVal;
                        //    res.ReturnFare = FareSett.returnFares.ToDecimal() > 0 ? FareSett.returnFares.ToDecimal() : 0;
                        //}
                        //catch
                        //{
                        //}

                        try
                        {
                            var EnableCongestionCharges = "false";
                            try
                            {
                                EnableCongestionCharges = db.ExecuteQuery<string>("Select SetVal from AppSettings where SetKey ='EnableCongestionCharges'").FirstOrDefault().ToStr();
                            }
                            catch
                            {
                            }
                            if (EnableCongestionCharges == "true")
                            {
                                var CongestionCharges = GetCongestionCharges(obj.routeInfo.legs, info.PickupDateTime, obj.routeInfo.SubCompanyId);
                                Newtonsoft.Json.Linq.JObject jsonObj = Newtonsoft.Json.Linq.JObject.Parse(CongestionCharges);
                                decimal congestion = jsonObj["Data"]["FareRate"].ToObject<decimal>();
                                res.Congestion = congestion;
                                if (res.Congestion > 0)
                                {
                                    res.Parking = 0;
                                }
                            }
                        }
                        catch
                        {
                        }

                        if (res.ReturnFare.ToDecimal() > 0 && res.ReturnFare < res.Fare)
                            res.ReturnFare = res.Fare.ToDecimal();

                        response.Data = res;
                        //try
                        //{


                        //    System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GETALLFARESFROMDISPATCHNEWdeserial.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + result + Environment.NewLine);
                        //}
                        //catch
                        //{

                        //}

                        //  }

                        try
                        {


                            //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "CalculateFares_response.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(response.Data) + Environment.NewLine);
                            General.WriteLog("CalculateFares_response", "json: " + new JavaScriptSerializer().Serialize(response.Data));
                        }
                        catch
                        {

                        }



                        //if (info.MapType == 1)
                        //    info.MapKey = db.ExecuteQuery<string>("select APIKey from mapkeys where maptype='google'").FirstOrDefault().ToStr().Trim();
                        //else
                        //    info.MapKey = db.ExecuteQuery<string>("select APIKey from mapkeys where maptype='here'").FirstOrDefault().ToStr().Trim();

                        //////


                        //info.Vehicle = db.Fleet_VehicleTypes.Where(a => a.Id == info.VehicleTypeId).Select(a => a.VehicleType).FirstOrDefault().ToStr();
                        ////needtouncomment
                        //AppAPISer.AppAPISoapClient c = new AppAPISer.AppAPISoapClient();
                        //string data = c.GetAllFaresFromDispatch(HubProcessor.Instance.objPolicy.DefaultClientId.ToStr(), "LOCAL", new JavaScriptSerializer().Serialize(info), HubProcessor.Instance.objPolicy.DefaultClientId.ToStr() + "4321orue");
                        //data = data.Replace("\\", "");
                        //int startIndex = data.IndexOf("[{") + 1;

                        ////
                        //data = data.Substring(startIndex);
                        //int lastIndex = data.IndexOf("}]") + 1;
                        //data = data.Substring(0, lastIndex);

                        //response.Data = Newtonsoft.Json.JsonConvert.DeserializeObject<ClsDispatchFares>(data);


                        //try
                        //{


                        //    System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "CalculateFares_response.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(response.Data) + Environment.NewLine);
                        //}
                        //catch
                        //{

                        //}

                    }

                    catch (Exception ex)
                    {
                        response.Message = ex.Message;
                        response.HasError = true;

                    }








                }


                return new CustomJsonResult { Data = response };

                //
            }
            catch
            {

                response.HasError = true;
                response.Message = "exception occured";
            }



            return new CustomJsonResult { Data = response };
        }

        public string GetCongestionCharges(List<RouteLeg> route, string PickupDateTime, int? SubCompanyId)
        {
            var url = "https://www.treasureonlineapi.co.uk/CabTreasureWebApi/Home/GetCongestionCharges";
            List<RouteCoords> allCoords = new List<RouteCoords>();
            foreach (var leg in route)
            {
                if (leg.coords != null)
                {
                    allCoords.AddRange(leg.coords);
                }
            }
            string result = "";
            int batchSize = 100; // Adjust based on server limit
            List<List<RouteCoords>> batches = allCoords.Select((coord, index) => new { coord, index }).GroupBy(x => x.index / batchSize).Select(g => g.Select(x => x.coord).ToList()).ToList();
            foreach (var batch in batches)
            {
                var requestData = new
                {
                    Coords = batch,
                    defaultclientId = HubProcessor.Instance.objPolicy.DefaultClientId.ToStr(),
                    PickupDateTime = PickupDateTime,
                    SubCompanyId = SubCompanyId
                };
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = new JavaScriptSerializer().Serialize(requestData);
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                    Newtonsoft.Json.Linq.JObject jsonObj = Newtonsoft.Json.Linq.JObject.Parse(result);
                    decimal congestion = jsonObj["Data"]["FareRate"].ToObject<decimal>();
                    if (congestion > 0)
                    {
                        break;
                    }
                }
                try
                {
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetCongestionCharges.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(result) + Environment.NewLine);
                    General.WriteLog("GetCongestionCharges", "json: " + new JavaScriptSerializer().Serialize(result));
                }
                catch
                {
                }
            }
            return result;
        }

        public FareSettings CalculateFareSetting(WebApiClasses.RequestWebApi obj, ClsDispatchFares res)
        {
            FareSettings set = new FareSettings();

            decimal AddedAmount = 0.00m;
            decimal returnAddedAmount = 0.00m;
            string op = string.Empty;
            int actualVehicleTypeId = obj.routeInfo.VehicleTypeId.ToInt();
            decimal fareVal = res.Fare.ToDecimal();
            decimal returnFares = res.ReturnFare.ToDecimal();
            decimal companyPrice = res.CompanyPrice.ToDecimal();
            Gen_SysPolicy_FaresSetting objFare = General.GetObject<Gen_SysPolicy_FaresSetting>(c => c.SysPolicyId != null && c.VehicleTypeId == actualVehicleTypeId);

            if (objFare != null)
            {
                op = objFare.Operator.ToStr();

                Gen_SysPolicy_Configuration confg = General.GetObject<Gen_SysPolicy_Configuration>(c => c.SysPolicyId != null);


                if (objFare.IsAmountWise.ToBool() && objFare.Percentage.ToInt() > 0 && objFare.VehicleTypeName.ToStr().Trim().Length > 0 && objFare.VehicleTypeName.ToStr().Trim().IsNumeric() && objFare.VehicleTypeName.ToStr().Trim().ToInt() > 0)
                {

                    int ValueAddedType = objFare.VehicleTypeName.ToStr().Trim().ToInt();

                    decimal AddedPercentage = 0.00m;
                    decimal returnAddedPercentage = 0.00m;

                    decimal onlyAmount = objFare.Amount.ToDecimal();

                    AddedPercentage = ((fareVal + confg.ViaPointExtraCharges.ToDecimal()) * objFare.Percentage.ToDecimal()) / 100;
                    returnAddedPercentage = ((returnFares + confg.ViaPointExtraCharges.ToDecimal()) * objFare.Percentage.ToDecimal()) / 100;



                    AddedAmount = objFare.Amount.ToDecimal() + confg.ViaPointExtraCharges.ToDecimal();
                    returnAddedAmount = objFare.Amount.ToDecimal() + confg.ViaPointExtraCharges.ToDecimal();



                    if (ValueAddedType == 1)
                    {
                        if (AddedPercentage < onlyAmount)
                        {
                            AddedPercentage = 0;
                            returnAddedAmount = 0;

                        }
                        else
                        {
                            AddedAmount = AddedPercentage + confg.ViaPointExtraCharges.ToDecimal();
                            returnAddedAmount = returnAddedPercentage + confg.ViaPointExtraCharges.ToDecimal();

                        }

                    }
                    else if (ValueAddedType == 2)
                    {
                        if (AddedPercentage > onlyAmount)
                        {

                            AddedPercentage = 0;
                            returnAddedAmount = 0;
                            if (obj.routeInfo.JourneyTypeId != 2 ||
                                (obj.routeInfo.JourneyTypeId == 2 && obj.routeInfo.VehicleTypeId.ToInt() > 0 && actualVehicleTypeId == obj.routeInfo.VehicleTypeId.ToInt()))
                                confg.ViaPointExtraCharges = 0;




                        }
                        else
                        {
                            AddedAmount = AddedPercentage + confg.ViaPointExtraCharges.ToDecimal();
                            returnAddedAmount = returnAddedPercentage + confg.ViaPointExtraCharges.ToDecimal();

                        }


                    }


                    if (obj.routeInfo.JourneyTypeId != 2 ||
                            (obj.routeInfo.JourneyTypeId == 2 && obj.routeInfo.VehicleTypeId.ToInt() > 0 && actualVehicleTypeId == obj.routeInfo.VehicleTypeId.ToInt()))
                        confg.ViaPointExtraCharges = 0;
                    //  }


                    if (obj.routeInfo.JourneyTypeId == 2 && obj.routeInfo.VehicleTypeId.ToInt() > 0 && actualVehicleTypeId != obj.routeInfo.VehicleTypeId.ToInt())
                    {


                        objFare = General.GetObject<Gen_SysPolicy_FaresSetting>(c => c.SysPolicyId != null && c.VehicleTypeId == obj.routeInfo.VehicleTypeId.ToInt());

                        if (objFare != null)
                        {


                            returnAddedPercentage = ((returnFares + confg.ViaPointExtraCharges.ToDecimal()) * objFare.Percentage.ToDecimal()) / 100;


                            returnAddedAmount = objFare.Amount.ToDecimal() + confg.ViaPointExtraCharges.ToDecimal();

                            if (ValueAddedType == 1)
                            {
                                if (returnAddedPercentage < returnAddedAmount)
                                {
                                    returnAddedPercentage = 0;
                                    returnAddedAmount = 0;


                                    confg.ViaPointExtraCharges = 0;

                                }
                                else
                                {

                                    returnAddedAmount = returnAddedPercentage;

                                }

                            }
                            else if (ValueAddedType == 2)
                            {
                                if (returnAddedPercentage > returnAddedAmount)
                                {

                                    returnAddedAmount = returnAddedPercentage;



                                }
                                else
                                {

                                    returnAddedPercentage = 0;
                                    returnAddedAmount = 0;

                                    confg.ViaPointExtraCharges = 0;
                                }


                            }





                        }
                        else
                            returnAddedAmount = 0.00m;
                    }

                }
                else
                {


                    if (objFare.IsAmountWise == false)
                    {

                        AddedAmount = ((fareVal + confg.ViaPointExtraCharges.ToDecimal()) * objFare.Percentage.ToDecimal()) / 100;
                        returnAddedAmount = ((returnFares + confg.ViaPointExtraCharges.ToDecimal()) * objFare.Percentage.ToDecimal()) / 100;
                    }
                    else
                    {
                        AddedAmount = objFare.Amount.ToDecimal() + confg.ViaPointExtraCharges.ToDecimal();
                        returnAddedAmount = objFare.Amount.ToDecimal() + confg.ViaPointExtraCharges.ToDecimal();

                        if (obj.routeInfo.JourneyTypeId != 2 ||
                                (obj.routeInfo.JourneyTypeId == 2 && obj.routeInfo.VehicleTypeId.ToInt() > 0 && actualVehicleTypeId == obj.routeInfo.VehicleTypeId.ToInt()))
                            confg.ViaPointExtraCharges = 0;
                    }


                    if (obj.routeInfo.JourneyTypeId == 2 && obj.routeInfo.VehicleTypeId.ToInt() > 0 && actualVehicleTypeId != obj.routeInfo.VehicleTypeId.ToInt())
                    {


                        objFare = General.GetObject<Gen_SysPolicy_FaresSetting>(c => c.SysPolicyId != null && c.VehicleTypeId == obj.routeInfo.VehicleTypeId.ToInt());

                        if (objFare != null)
                        {
                            if (objFare.IsAmountWise == false)
                            {

                                returnAddedAmount = ((returnFares + confg.ViaPointExtraCharges.ToDecimal()) * objFare.Percentage.ToDecimal()) / 100;
                            }
                            else
                            {

                                returnAddedAmount = objFare.Amount.ToDecimal() + confg.ViaPointExtraCharges.ToDecimal();
                                confg.ViaPointExtraCharges = 0;
                            }

                        }
                        else
                            returnAddedAmount = 0.00m;
                    }

                }


                //cls.ExtraViaCharges = 0.00m;

                switch (op)
                {
                    case "+":

                        if (AddedAmount > 0)
                            fareVal = (decimal)Math.Ceiling((fareVal + AddedAmount) / 0.1m) * 0.1m;

                        if (returnAddedAmount > 0)
                            returnFares = (decimal)Math.Ceiling((returnFares + returnAddedAmount) / 0.1m) * 0.1m;

                        if (companyPrice > 0 && companyPrice == fareVal && AddedAmount > 0)
                            companyPrice = (decimal)Math.Ceiling((companyPrice + AddedAmount) / 0.1m) * 0.1m;
                        break;

                    case "-":
                        //fareVal = fareVal - AddedAmount;
                        //returnFares = returnFares + returnAddedAmount;
                        if (AddedAmount > 0)
                            fareVal = (decimal)Math.Ceiling((fareVal - AddedAmount) / 0.1m) * 0.1m;

                        if (returnAddedAmount > 0)
                            returnFares = (decimal)Math.Ceiling((returnFares - returnAddedAmount) / 0.1m) * 0.1m;
                        break;

                    default:
                        if (companyPrice > 0 && companyPrice == fareVal && AddedAmount > 0)
                            companyPrice = (decimal)Math.Ceiling((companyPrice + AddedAmount) / 0.1m) * 0.1m;

                        if (AddedAmount > 0)
                            fareVal = (decimal)Math.Ceiling((fareVal + AddedAmount) / 0.1m) * 0.1m;

                        if (returnAddedAmount > 0)
                            returnFares = (decimal)Math.Ceiling((returnFares + returnAddedAmount) / 0.1m) * 0.1m;
                        break;


                        //   rtnFare = (decimal)Math.Ceiling(rtnFare / 0.5m) * 0.5m;

                }


                decimal roundUp = confg.RoundUpTo.ToDecimal();
                if (roundUp > 0)
                {
                    //fareVal = (decimal)Math.Ceiling(fareVal / roundUp) * roundUp;
                    //returnFares = (decimal)Math.Ceiling(returnFares / roundUp) * roundUp;

                    //if (ENABLECMACBOOKINGCALCULATION == true)
                    //{
                    //    if (objMaster.Current != null)
                    //    {
                    //        companyPrice = objMaster.Current.CompanyPrice.ToDecimal();
                    //    }
                    //    else
                    //    {
                    //        if (companyPrice > 0 && companyPrice == fareVal)
                    //        {
                    //            companyPrice = (decimal)Math.Ceiling(companyPrice / roundUp) * roundUp;


                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    if (companyPrice > 0 && companyPrice == fareVal)
                    //    {
                    //        companyPrice = (decimal)Math.Ceiling(companyPrice / roundUp) * roundUp;


                    //    }
                    //}
                    fareVal = (decimal)Math.Ceiling(fareVal / roundUp) * roundUp;
                    returnFares = (decimal)Math.Ceiling(returnFares / roundUp) * roundUp;

                    if (companyPrice > 0 && companyPrice == fareVal)
                    {
                        companyPrice = (decimal)Math.Ceiling(companyPrice / roundUp) * roundUp;


                    }

                }

            }

            set = new FareSettings
            {
                fareVal = fareVal,
                returnFares = returnFares,
                companyPrice = companyPrice


            };
            return set;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetAddressData")]
        public JsonResult GetAddressData(WebApiClasses.RequestWebApi obj)
        {
            //

            ResponseWebApi response = new ResponseWebApi();

            try
            {//



                using (TaxiDataContext db = new TaxiDataContext())
                {

                    string searchValue = obj.addressInfo.searchText.ToStr().Trim().ToUpper();
                    try
                    {

                        if (HubProcessor.Instance.objPolicy.BookingInterval.ToInt() == 1)
                        {
                            var locationList = GetGoogleAndOtherAddressData(obj);
                            var locationName = locationList.Select(x => x.AddressLine).ToList();
                            response.Data = locationList;
                        }
                        else
                        {
                            //
                            string postCode = General.GetPostCodeMatchOpt(searchValue);

                            string doorNo = string.Empty;
                            string place = string.Empty;




                            if (postCode.Length == 0 && searchValue.Trim().Contains(" ") && searchValue.Trim().Contains(".") == false && searchValue.Trim().Contains("#") == false
                              && searchValue[0].ToStr().IsAlpha() && searchValue.Split(new char[] { ' ' }).Any(c => c.IsAlpha() == false))
                            //    && (searchValue.Trim().Substring(0, searchValue.Trim().IndexOf(' ')).ToStr().IsAlpha() == false || searchValue.Trim().Substring(searchValue.Trim().IndexOf(' ') + 1)[0].ToStr().IsAlpha()))
                            {
                                var arrData = searchValue.Split(new char[] { ' ' });



                                if (arrData.Count() == 2)
                                {
                                    postCode = General.GetPostCodeMatchOpt(arrData.FirstOrDefault(c => c.IsAlpha() == false));

                                }
                                else if (arrData.Count() > 2)
                                {

                                    if (arrData[1][0].ToStr().IsNumeric())
                                        postCode = General.GetPostCodeMatchOpt((arrData.FirstOrDefault(c => c.IsAlpha() == false) + " " + arrData[1]).Trim());
                                    else if (arrData[1].ToStr().IsAlpha() == false && arrData[2].ToStr().IsAlpha() == false)
                                        postCode = General.GetPostCodeMatchOpt((arrData.FirstOrDefault(c => c.IsAlpha() == false) + " " + arrData[2]).Trim());
                                    else
                                        postCode = General.GetPostCodeMatchOpt(arrData.FirstOrDefault(c => c.IsAlpha() == false));
                                }


                            }

                            if (!string.IsNullOrEmpty(postCode) && postCode.IsAlpha() == true)
                                postCode = string.Empty;

                            string street = searchValue;

                            if (postCode.Length > 0)
                            {
                                street = street.Replace(postCode, "").Trim();
                            }


                            if (!string.IsNullOrEmpty(street) && !string.IsNullOrEmpty(postCode) && street.IsAlpha() == false && street.Length < 4 && searchValue.IndexOf(postCode) < searchValue.IndexOf(street))
                            {
                                street = "";
                                postCode = searchValue;
                            }


                            if (street.Length > 0)
                            {

                                if (char.IsNumber(street[0]))
                                {

                                    for (int i = 0; i <= 3; i++)
                                    {

                                        try
                                        {
                                            if (char.IsNumber(street[i]) || (doorNo.Length > 0 && doorNo.Length == i && char.IsLetter(street[i])))
                                                doorNo += street[i];
                                            else
                                                break;
                                        }
                                        catch
                                        {


                                        }
                                    }
                                }
                            }


                            if (street.Contains("#"))
                            {
                                street = street.Replace("#", "").Trim();
                                place = "p=";
                            }

                            if (doorNo.Length > 0 && place.Length == 0)
                            {
                                street = street.Replace(doorNo, "").Trim();


                            }


                            if (postCode.Length == 0 && street.Length < 3 && street != "//")
                            {
                                //   e.Cancel = true;
                                //   return;

                            }


                            if (street.Length > 1 || postCode.Length > 0)
                            {
                                if (postCode.Length > 0)
                                {
                                    if (doorNo.Length > 0 && postCode == General.GetPostCodeMatch(postCode))
                                    {
                                        doorNo = string.Empty;
                                    }

                                }

                                if (postCode.Length >= 5 && postCode.Contains(" ") == false)
                                {


                                    //string resultPostCode = AppVars.listOfAddress.FirstOrDefault(a => a.PostalCode.Strip(' ') == postCode).DefaultIfEmpty().PostalCode.ToStr().Trim();


                                    //if (resultPostCode.Length >= 5 && resultPostCode.Contains(" "))
                                    //{
                                    //    postCode = resultPostCode;

                                    //}

                                }


                                //if (POIWorker == null || POIWorker.CancellationPending || ((sender as BackgroundWorker) == null || (sender as BackgroundWorker).CancellationPending))
                                //{
                                //    e.Cancel = true;
                                //    return;
                                //}






                                //if (text.Contains(" ") && text.Length < 13 && text.WordCount() == 2 && text.Remove(text.IndexOf(' ')).Trim().Length <= 3 && text.Strip(' ').IsAlpha()==false
                                //    && (AppVars.keyLocations.Contains(text.Split(new char[] { ' ' })[0])))
                                //{
                                //  aTxt.ListBoxElement.Items.Clear();
                                if (searchValue.Contains(" ") && searchValue.Length < 20 && searchValue.WordCount() == 2 && searchValue.Contains(".") == false && searchValue.Strip(' ').IsAlpha() == false)
                                {

                                    string[] arr = searchValue.Split(new char[] { ' ' });

                                    if (arr.Count() == 2)
                                    {
                                        if (arr[0].IsAlpha())
                                        {
                                            string pcode = General.GetPostCodeMatch(arr[1].ToStr().ToUpper());

                                            if (pcode.ToStr().Length > 0)
                                            {
                                                response.Data = (from a in db.Gen_Locations.Where(c => (c.Gen_LocationType.ShortCutKey == arr[0]) && c.PostCode.StartsWith(pcode))
                                                                 select (a.PostCode != string.Empty ? a.LocationName + ", " + a.PostCode : a.LocationName)
                                                  ).ToArray<string>();

                                                if (response.Data != null && (response.Data as string[]).Count() == 0)
                                                    response.Data = null;

                                            }
                                        }
                                    }

                                }




                                if (response.Data == null)
                                {

                                    if (doorNo.Length > 0 && street.Strip(' ').IsAlpha() == false)
                                    {
                                        postCode = General.GetPostCodeMatch(street);
                                        if (postCode.Length > 0)
                                        {

                                            street = street.Replace(postCode, "").Trim();
                                        }
                                    }
                                    else if (postCode.Length > 0 && street.Length == 0 && postCode.Count(c => c == ' ') > 1)
                                    {
                                        string originalPostCode = postCode;
                                        postCode = postCode.Substring(0, postCode.LastIndexOf(' '));

                                        doorNo = originalPostCode.Replace(postCode, "").ToStr().Trim();
                                    }
                                    else if (street.Length > 3 && street.Contains(' ') && street.IsAlpha() == false && doorNo.Length == 0)
                                    {


                                        for (int i = 0; i < street.Length; i++)
                                        {
                                            if (Char.IsDigit(street[i]))
                                            {
                                                if (i > 0 && street[i - 1] == ' ')
                                                {

                                                    doorNo += street[i];
                                                }
                                                else if (i == 0)
                                                {
                                                    doorNo += street[i];
                                                }
                                                else if (doorNo.Length > 0)
                                                {
                                                    doorNo += street[i];

                                                }


                                            }

                                        }


                                        if (doorNo.Length > 0)
                                            street = street.Replace(doorNo, "").Trim();
                                    }
                                    else if (postCode.Length > 0 && postCode.Contains(" ") == false && street.Length == 0 && doorNo.Length == 0 && place.Length == 0)
                                    {
                                        //    IF LENGTH IS 5
                                        //THEN
                                        //E11AA=> IF 3RD CHARACTER IS NUMERIC THEN E1 1AA


                                        //IF LENGTH IS 6
                                        //THEN
                                        //HA20DU=> IF 4TH CHARACTER IS NUMERIC THEN HA2 0DU

                                        //IF LENGTH IS 7
                                        //THEN 
                                        //WC1A1AB=> IF 5TH CHARACTER IS NUMERIC THEN WC1A 1AB



                                        if (postCode.Length == 5)
                                        {
                                            if (postCode[2].ToStr().IsNumeric())
                                            {
                                                postCode = postCode.Insert(2, " ");

                                            }

                                        }
                                        else if (postCode.Length == 6)
                                        {
                                            if (postCode[3].ToStr().IsNumeric())
                                            {
                                                postCode = postCode.Insert(3, " ");

                                            }

                                        }
                                        else if (postCode.Length == 7)
                                        {
                                            if (postCode[4].ToStr().IsNumeric())
                                            {
                                                postCode = postCode.Insert(4, " ");

                                            }

                                        }
                                    }





                                    if (street == "//" && obj.addressInfo.customerinfo.ToStr().Length > 0)
                                    {
                                        try
                                        {

                                            response.Data = db.ExecuteQuery<string>("exec stp_getcustomerhistoryaddresses {0}", obj.addressInfo.customerinfo.ToStr()).ToArray<string>();
                                        }
                                        catch
                                        {


                                        }
                                    }
                                    else
                                    {






                                        var finalList = db.ExecuteQuery<stp_GetByRoadLevelDataResult>("exec stp_GetByRoadLevelData {0},{1},{2},{3}", postCode, doorNo, street, place)
                                            .Select(c => c.AddressLine1).Where(c => c != null).ToArray<string>();



                                        var localization = db.stp_GetLocalizationDetails().Where(c => c.MasterId == null).Select(c => c.PostCode).ToArray<string>();

                                        var finalList2 = (from a in localization
                                                          from b in finalList
                                                          where b.Contains(a) && (b.Substring(b.IndexOf(a), a.Length) == a && (b.IndexOf(a) - 1) >= 0 && b[b.IndexOf(a) - 1] == ' ' && GeneralBLL.GetHalfPostCodeMatch(b) == a)

                                                          select b).ToArray<string>();


                                        if (finalList2.Count() > 0)
                                        {



                                            finalList = finalList2.Union(finalList).ToArray<string>();


                                            //finalList2 = (from a in finalList2
                                            //                 where General.GetPostCodeMatch(a).Length == 0
                                            //                 select a).ToArray<string>();


                                            //    finalList = finalList2.Union(finalList).ToArray<string>();

                                        }

                                        response.Data = finalList;
                                        //else
                                        //{
                                        //    finalList = resValue;

                                        //    var finalList2 = (from a in resValue
                                        //                      where General.GetPostCodeMatch(a).Length == 0
                                        //                      select a).ToArray<string>();


                                        //    finalList = finalList2.Union(finalList).ToArray<string>();
                                        //}


                                        //    }

                                        //}
                                    }
                                    //   }


                                    //    }
                                }








                            }
                        }

                        //



                    }
                    catch (Exception ex)
                    {

                        try
                        {
                            //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "getaddressdata1_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + ex.Message + Environment.NewLine);
                            General.WriteLog("getaddressdata1_exception", "Exception: " + ex.Message);
                        }
                        catch
                        {

                        }

                        //   AddExcepLog("POIWORKER_DOWORK : " + ex.Message);
                        //     Console.WriteLine("Start work catch: " + searchValue);

                    }








                }





            }
            catch
            {

                response.HasError = true;
                response.Message = "exception occured";
            }



            return new CustomJsonResult { Data = response };
        }


        public List<LocationList> GetGoogleAndOtherAddressData(WebApiClasses.RequestWebApi obj)
        {
            ResponseWebApi response = new ResponseWebApi();
            List<LocationList> str = new List<LocationList>();
            using (TaxiDataContext db = new TaxiDataContext())
            {
                string street = obj.addressInfo.searchText.ToStr();
                string query = @"select  AddressLine=LTRIM(RTRIM(REPLACE((  LocationName + ' '+REPLACE((REPLACE(Address, ISNULL(postcode,''),'')),LOCATIONNAME,'') + ' '+ISNULL(PostCode,'')),'  ',' ')))
                                                ,CONVERT(varchar, LocationTypeId) LocationTypeId from Gen_Locations where LocationTypeId !=8 And LocationName like '" + street + "'  +'%'  or Address like '" + street + "'  +'%' ";
                //response.Data = db.stp_GetByRoadLevelData(postCode, doorNo, street, place).Select(c => c.AddressLine1).Where(c => c != null).ToArray<string>();
                List<LocationList> lst = db.ExecuteQuery<LocationList>(query).ToList();
                response.Data = lst;
            }
            if (HubProcessor.Instance.objPolicy.BookingInterval.ToInt() == 1)
            {
                str = General.GetGoogleAddressData(obj.addressInfo.searchText.ToStr());
            }
            List<LocationList> castedList = (List<LocationList>)response.Data;
            if (castedList == null)
            {
                castedList = new List<LocationList>();
            }
            castedList.AddRange(str);
            return castedList;
        }
        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("CalculateFaresAllVehicle")]
        public JsonResult CalculateFaresAllVehicle(WebApiClasses.RequestWebApi obj)
        {
            //

            try
            {


                //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "CalculateFares.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
                General.WriteLog("CalculateFares", "json: " + new JavaScriptSerializer().Serialize(obj));
            }
            catch
            {

            }

            ResponseWebApi response = new ResponseWebApi();

            try
            {




                using (TaxiDataContext db = new TaxiDataContext())
                {
                    //

                    try
                    {


                        //
                        //
                        BookingInformation info = new BookingInformation();
                        info.FromAddress = obj.routeInfo.pickupAddress.Address.ToStr();
                        if (obj.routeInfo.pickupAddress.locTypeId == 0)
                        {
                            string postcode = General.GetPostCodeMatch(obj.routeInfo.pickupAddress.ToStr().Trim().ToUpper());
                            if (db.Gen_Locations.Where(c => c.LocationTypeId == 1 && c.PostCode == postcode).Count() > 0)
                            {
                                obj.routeInfo.pickupAddress.locTypeId = 1;
                            }

                        }

                        if (obj.routeInfo.destinationAddress.locTypeId == 0)
                        {
                            string postcode = General.GetPostCodeMatch(obj.routeInfo.destinationAddress.ToStr().Trim().ToUpper());
                            if (db.Gen_Locations.Where(c => c.LocationTypeId == 1 && c.PostCode == postcode).Count() > 0)
                            {
                                obj.routeInfo.destinationAddress.locTypeId = 1;
                            }

                        }

                        info.MapType = HubProcessor.Instance.objPolicy.MapType.ToInt();
                        info.ToAddress = obj.routeInfo.destinationAddress.Address.ToStr();
                        info.FromType = obj.routeInfo.pickupAddress.locTypeId == 1 ? "airport" : "address";
                        info.ToType = obj.routeInfo.destinationAddress.locTypeId == 1 ? "airport" : "address";
                        info.CompanyId = obj.routeInfo.CompanyId.ToInt();
                        //info.PickupDateTime = string.Format("{0:dd/MM/yyyy HH:mm}", DateTime.Now);

                        //if (info.PickupDateTime != null)
                        //    info.PickupDateTime = string.Format("{0:dd/MM/yyyy HH:mm}", obj.routeInfo.PickupDateTime);
                        //  info.RouteCoordinates = "-1";
                        // info.VehicleTypeId = obj.routeInfo.VehicleTypeId.ToInt();

                        //
                        try
                        {
                            info.VehicleTypeId = -1;
                            info.Vehicle = "";
                        }
                        catch
                        {
                            info.VehicleTypeId = obj.routeInfo.VehicleTypeId.ToInt();
                        }
                        //  info.Via = booking.Via;
                        //


                        if (obj.routeInfo.PickupDateTime != null)
                            info.PickupDateTime = string.Format("{0:dd-MMM-yyyy HH:mm}", obj.routeInfo.PickupDateTime);
                        else
                            info.PickupDateTime = string.Format("{0:dd/MM/yyyy HH:mm}", DateTime.Now);



                        if (obj.routeInfo.returnPickupDateTime != null)
                            info.returnPickupDateTime = string.Format("{0:dd-MMM-yyyy HH:mm}", obj.routeInfo.returnPickupDateTime);
                        else
                            info.returnPickupDateTime = string.Format("{0:dd/MM/yyyy HH:mm}", DateTime.Now);

                        if (obj.routeInfo.viaAddresses != null)
                        {

                            info.Via = (from a in obj.routeInfo.viaAddresses select new ViaAddresses { Viaaddress = a.Address, Viatype = "address" }).ToArray();

                        }
                        //  info.Via = booking.Via;
                        //
                        info.Mileage = obj.routeInfo.Distance;
                        info.Miles = info.Mileage.ToStr();

                        info.Duration = obj.routeInfo.Duration;
                        info.Noofhours = obj.routeInfo.Noofhours;

                        if (info.MapType == 1)
                            info.MapKey = db.ExecuteQuery<string>("select APIKey from mapkeys where maptype='google'").FirstOrDefault().ToStr().Trim();
                        else
                            info.MapKey = db.ExecuteQuery<string>("select APIKey from mapkeys where maptype='here'").FirstOrDefault().ToStr().Trim();


                        var url = "https://www.treasureonlineapi.co.uk/CabTreasureWebApi/Home/GETALLFARESFROMDISPATCHNEW";
                        var requestData = new
                        {

                            defaultclientId = HubProcessor.Instance.objPolicy.DefaultClientId.ToStr(),
                            bookingInformation = info
                        };
                        var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Method = "POST";
                        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                        {
                            string json = new JavaScriptSerializer().Serialize(requestData);
                            streamWriter.Write(json);
                            streamWriter.Flush();
                            streamWriter.Close();
                        }
                        try
                        {


                            //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GETALLFARESFROMDISPATCHNEW_request.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(requestData) + Environment.NewLine);
                            General.WriteLog("GETALLFARESFROMDISPATCHNEW_request", "json: " + new JavaScriptSerializer().Serialize(requestData));
                        }
                        catch
                        {

                        }
                        String result = "";
                        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            result = streamReader.ReadToEnd();



                        }
                        try
                        {


                            //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GETALLFARESFROMDISPATCHNEW.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(result) + Environment.NewLine);
                            General.WriteLog("GETALLFARESFROMDISPATCHNEW", "json: " + new JavaScriptSerializer().Serialize(result));
                        }
                        catch
                        {

                        }


                        result = result.Replace("\\", "");
                        //int startIndex = result.IndexOf("[{") + 1;

                        ////
                        //result = result.Substring(startIndex);
                        //int lastIndex = result.IndexOf("}]") + 1;
                        //result = result.Substring(0, lastIndex);

                        var res = Newtonsoft.Json.JsonConvert.DeserializeObject<ClsDispatchFaresLst>(result);

                        if (res != null && res.Data != null && res.Data.ClsDispatchFareslist != null)
                        {
                            foreach (var fare in res.Data.ClsDispatchFareslist)
                            {
                                try
                                {
                                    if (info.Noofhours == 0)
                                    {
                                        long FareId = 0;
                                        if (obj.routeInfo.FareCalculationSetting > 1)
                                        {
                                            FareId = db.ExecuteQuery<int>("select Id from HourlyFare where VehicleTypeId = " + fare.ID).FirstOrDefault();
                                            if (obj.routeInfo.CompanyId.ToInt() > 0)
                                            {
                                                var newFareId = db.ExecuteQuery<int>("select Id from HourlyFare where VehicleTypeId = " + fare.ID + " AND CompanyId = " + obj.routeInfo.CompanyId).FirstOrDefault();
                                                if (newFareId > 0)
                                                {
                                                    FareId = newFareId;
                                                }
                                            }

                                            if (FareId > 0)
                                            {
                                                var startRate = 0.00m;
                                                var perMinRate = db.ExecuteQuery<decimal>("select Rate from HourlyFare_OtherCharges where fareid = " + FareId + " and " + info.Duration + " between FromMins and ToMins").FirstOrDefault();
                                                var objStartRate = db.ExecuteQuery<SignalRHub.Classes.HourlyFare>("select StartRate,StartRateValidMins from HourlyFare where Id = " + FareId).FirstOrDefault();
                                                if (objStartRate != null)
                                                {
                                                    startRate = objStartRate.StartRate.ToDecimal();
                                                    if (info.Duration > objStartRate.StartRateValidMins.ToDecimal())
                                                    {
                                                        info.Duration -= objStartRate.StartRateValidMins.ToDecimal().ToInt();
                                                    }
                                                }
                                                if (obj.routeInfo.FareCalculationSetting == 3)
                                                {
                                                    fare.Fare += startRate + (info.Duration * perMinRate);
                                                }
                                                else
                                                {
                                                    fare.Fare = startRate + (info.Duration * perMinRate);
                                                }

                                                fare.ReturnFare = 0.0m;
                                                fare.CompanyPrice = null;
                                                if (obj.routeInfo.CompanyId.ToInt() > 0)
                                                {
                                                    fare.CompanyPrice = fare.Fare;
                                                }
                                            }
                                        }
                                    }
                                }
                                catch
                                {


                                }

                                if (fare.ReturnFare.ToDecimal() > 0 && fare.ReturnFare < fare.Fare)
                                    fare.ReturnFare = fare.Fare.ToDecimal();
                            }
                        }
                        response.Data = res;
                        //try
                        //{


                        //    System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GETALLFARESFROMDISPATCHNEWdeserial.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + result + Environment.NewLine);
                        //}
                        //catch
                        //{

                        //}

                        //  }

                        try
                        {


                            //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "CalculateFares_response.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(response.Data) + Environment.NewLine);
                            General.WriteLog("CalculateFares_response", "json: " + new JavaScriptSerializer().Serialize(response.Data));
                        }
                        catch
                        {

                        }

                    }

                    catch (Exception ex)
                    {
                        response.Message = ex.Message;
                        response.HasError = true;

                    }








                }


                return new CustomJsonResult { Data = response };

                //
            }
            catch
            {

                response.HasError = true;
                response.Message = "exception occured";
            }



            return new CustomJsonResult { Data = response };
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("CalculateFaresByFixedHours")]
        public JsonResult CalculateFaresByFixedHours(WebApiClasses.RequestWebApi obj)
        {
            //

            try
            {


                //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "CalculateFaresByFixedHours.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
                General.WriteLog("CalculateFaresByFixedHours", "json: " + new JavaScriptSerializer().Serialize(obj));
            }
            catch
            {

            }

            ResponseWebApi response = new ResponseWebApi();

            try
            {
                using (TaxiDataContext db = new TaxiDataContext())
                {
                    try
                    {
                        var res = new ClsDispatchFares();
                        res.Fare = 0.00m;
                        try
                        {
                            long FareId = 0;
                            if (obj.routeInfo.Noofhours > 0)
                            {
                                FareId = db.ExecuteQuery<int>("select Id from HourlyTariffFare where VehicleTypeId = " + obj.routeInfo.VehicleTypeId).FirstOrDefault();
                                if (obj.routeInfo.CompanyId.ToInt() > 0)
                                {
                                    var newFareId = db.ExecuteQuery<int>("select Id from HourlyTariffFare where VehicleTypeId = " + obj.routeInfo.VehicleTypeId + " AND CompanyId = " + obj.routeInfo.CompanyId).FirstOrDefault();
                                    if (newFareId > 0)
                                    {
                                        FareId = newFareId;
                                    }
                                }
                                if (FareId > 0)
                                {
                                    res.Fare = db.ExecuteQuery<decimal>("select Rate from HourlyTariffFare_OtherCharges where fareid = " + FareId + " and hours =" + obj.routeInfo.Noofhours).FirstOrDefault();
                                    res.ReturnFare = res.Fare;
                                    res.CompanyPrice = null;
                                }
                            }
                        }
                        catch
                        {


                        }

                        response.Data = res;
                        try
                        {
                            //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "CalculateFaresByFixedHours_response.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(response.Data) + Environment.NewLine);
                            General.WriteLog("CalculateFaresByFixedHours_response", "json: " + new JavaScriptSerializer().Serialize(response.Data));
                        }
                        catch
                        {

                        }

                    }

                    catch (Exception ex)
                    {
                        response.Message = ex.Message;
                        response.HasError = true;
                    }
                }
                return new CustomJsonResult { Data = response };

                //
            }
            catch
            {

                response.HasError = true;
                response.Message = "exception occured";
            }
            return new CustomJsonResult { Data = response };
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("CalculateFaresByFixedHoursAllVehicle")]
        public JsonResult CalculateFaresByFixedHoursAllVehicle(WebApiClasses.RequestWebApi obj)
        {
            //

            try
            {


                //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "CalculateFaresByFixedHours.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
                General.WriteLog("CalculateFaresByFixedHours", "json: " + new JavaScriptSerializer().Serialize(obj));
            }
            catch
            {

            }

            ResponseWebApi response = new ResponseWebApi();

            try
            {
                using (TaxiDataContext db = new TaxiDataContext())
                {
                    try
                    {
                        var resList = new ClsDispatchFaresLst(); // Initialize the response object

                        // Initialize the ClsDispatchFareslist inside the ClsDispatchFaresData class
                        resList.Data = new ClsDispatchFaresData();
                        resList.Data.ClsDispatchFareslist = new List<ClsDispatchFares>(); // Initialize the list

                        var vehicleList = db.Fleet_VehicleTypes.ToList(); // Get the list of vehicles from the database

                        if (vehicleList != null && vehicleList.Count > 0)
                        {
                            int counter = 0;
                            foreach (var item in vehicleList)
                            {
                                obj.routeInfo.VehicleTypeId = item.Id;  // Set the VehicleTypeId for the routeInfo

                                // Initialize a new ClsDispatchFares object
                                var fareItem = new ClsDispatchFares
                                {
                                    ID = item.Id,
                                    Name = item.VehicleType,
                                    Logo = item.Photo != null ? Convert.ToBase64String(item.Photo.ToArray()) : "",
                                    NoOfPassengers = item.NoofPassengers,
                                    NoOfLuggages = item.NoofLuggages,
                                    HandLuggages = item.NoofHandLuggages,
                                    StartRate = item.StartRate,
                                    SortOrder = item.OrderNo,
                                    Fare = 0.00m  // Initialize Fare with a default value
                                };

                                try
                                {
                                    long FareId = 0;

                                    // Check if routeInfo has a valid value for Noofhours
                                    if (obj.routeInfo.Noofhours > 0)
                                    {
                                        // Get the FareId for the HourlyTariffFare
                                        FareId = db.ExecuteQuery<int>("select Id from HourlyTariffFare where VehicleTypeId = " + obj.routeInfo.VehicleTypeId)
                                                   .FirstOrDefault();

                                        // If a CompanyId is provided, check for a specific fare for that company
                                        if (obj.routeInfo.CompanyId.ToInt() > 0)
                                        {
                                            var newFareId = db.ExecuteQuery<int>("select Id from HourlyTariffFare where VehicleTypeId = " + obj.routeInfo.VehicleTypeId + " AND CompanyId = " + obj.routeInfo.CompanyId)
                                                              .FirstOrDefault();

                                            if (newFareId > 0)
                                            {
                                                FareId = newFareId;
                                            }
                                        }

                                        // If a valid FareId is found, retrieve the fare for the specified hours
                                        if (FareId > 0)
                                        {
                                            fareItem.Fare = db.ExecuteQuery<decimal>("select Rate from HourlyTariffFare_OtherCharges where fareid = " + FareId + " and hours = " + obj.routeInfo.Noofhours)
                                                             .FirstOrDefault();

                                            fareItem.ReturnFare = fareItem.Fare; // Assuming return fare is the same as the main fare
                                            fareItem.CompanyPrice = null;        // Set CompanyPrice to null as per the logic
                                        }
                                    }

                                    // Optionally, populate other fields of 'fareItem' here if needed
                                    // For example: fareItem.Name = item.Name; fareItem.NoOfPassengers = item.PassengerCount;

                                    // Add the populated fareItem to the list of ClsDispatchFares
                                    resList.Data.ClsDispatchFareslist.Add(fareItem);
                                }
                                catch (Exception ex)
                                {
                                }

                                counter++;
                            }
                        }

                        response.Data = resList;

                        try
                        {
                            // Optionally log the response data to a file
                            //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "CalculateFaresByFixedHours_response.txt",
                            //DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(response.Data) + Environment.NewLine);
                            General.WriteLog("CalculateFaresByFixedHours_response", "json: " + new JavaScriptSerializer().Serialize(response.Data));
                        }
                        catch
                        {
                        }

                    }
                    catch (Exception ex)
                    {
                        response.Message = ex.Message;
                        response.HasError = true;
                    }
                }
                return new CustomJsonResult { Data = response };

                //
            }
            catch
            {

                response.HasError = true;
                response.Message = "exception occured";
            }
            return new CustomJsonResult { Data = response };
        }




        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetDriverConverstation")]
        public JsonResult GetDriverConverstation(WebApiClasses.RequestWebApi obj)
        {
            //

            ResponseWebApi response = new ResponseWebApi();

            try
            {


                //using (TaxiDataContext db = new TaxiDataContext("Data Source=88.198.21.250,58527;Initial Catalog=AscotCars;User ID=asc321;Password=asc321!;Trusted_Connection=False;"))
                //{
                using (TaxiDataContext db = new TaxiDataContext())
                {

                    int DriverId = obj.driverInfo.driverId;

                    int val = obj.driverInfo.dateFilter;



                    DateTime filterDate = DateTime.Now.AddDays(-val).ToDate();
                    string dateFormat = val == 0 ? "{0:HH:mm}" : "{0:dd/MM HH:mm}";




                    response.Data = (from a in db.Messages

                                     where a.MessageCreatedOn.Value.Date >= filterDate.Date &&

                                ((a.SenderId != null && a.SenderId == DriverId && a.SendFrom == "pda") || (a.ReceiverId != null && a.ReceiverId == DriverId))
                                     orderby a.MessageCreatedOn
                                     orderby a.Id
                                     select new
                                     {
                                         a.MessageBody,
                                         a.MessageCreatedOn,
                                         SenderName = a.SenderName

                                     }
                             ).ToList();













                }



                //using (TaxiDataContext db = new TaxiDataContext("Data Source=88.198.21.250,58527;Initial Catalog=AscotCars;User ID=asc321;Password=asc321!;Trusted_Connection=False;"))
                //{






                //      data.listofdrivers = db.stp_GetDashboardDrivers(1).ToList();

                //    response.Data = data;




                //}

            }
            catch
            {

                response.HasError = true;
                response.Message = "exception occured";
            }



            return new CustomJsonResult { Data = response };
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("SendMessageToDriver")]
        public JsonResult SendMessageToDriver(WebApiClasses.RequestWebApi obj)
        {
            //

            ResponseWebApi response = new ResponseWebApi();

            try
            {
                //
                int receiverId = obj.driverInfo.driverId;
                int senderId = obj.driverInfo.userId;
                string senderName = obj.driverInfo.userName.ToStr();
                string receiverName = obj.driverInfo.driverNo.ToStr();
                string messageBody = obj.driverInfo.messageBody.ToStr();
                using (Taxi_Model.TaxiDataContext db = new TaxiDataContext())
                {
                    if (receiverId != 0)
                    {
                        db.stp_SendMessage(senderId, receiverId, senderName, receiverName, messageBody, "");
                    }




                    //if (messageBody.Contains("\r\n"))
                    //{
                    //    messageBody = messageBody.Replace("\r\n", " ").Trim();
                    //}

                    if (messageBody.Contains("&"))
                    {
                        messageBody = messageBody.Replace("&", "And");
                    }

                    if (messageBody.Contains(">"))
                        messageBody = messageBody.Replace(">", " ");


                    if (messageBody.Contains("="))
                        messageBody = messageBody.Replace("=", " ");











                    General.requestPDA("request pda=" + receiverId + "=" + 0 + "=" + "Message>>" + messageBody + ">>" + String.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Now) + "=4");



                    response.Data = "success";



                }

            }
            catch (Exception ex)
            {

                response.HasError = true;
                response.Message = ex.Message;
            }



            return new CustomJsonResult { Data = response };
        }



        #endregion



        #region Phase#3


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetTrackDriver")]
        public JsonResult GetTrackDriver(WebApiClasses.RequestWebApi obj)
        {
            //

            ResponseWebApi response = new ResponseWebApi();

            try
            {

                ClsDashboardModel data = new ClsDashboardModel();
                //using (TaxiDataContext db = new TaxiDataContext("Data Source=88.198.21.250,58527;Initial Catalog=AscotCars;User ID=asc321;Password=asc321!;Trusted_Connection=False;"))
                //{
                using (TaxiDataContext db = new TaxiDataContext())
                {

                    string query = "select Id=ISNULL(z.id,0),ZoneName=ISNULL(z.ZoneName,l.newzonename) ,l.driverid,d.driverno " +
                                ", l.plotdate,l.latitude,l.longitude,l.speed,locationname = ISNULL(z.ZoneName, l.newzonename) " +
                                ",q.driverworkstatusid,s.workstatus,s.backgroundcolor,fm.Plateno,l.EstimatedTimeLeft,UpdateDate = l.UpdateDate ,WaitSinceOn = isnull(q.WaitSinceOn, getdate())" +
                                " from gen_zones z " +
                                " right JOIN Fleet_Driver_Location l on z.id = l.zoneid " +
                                " INNER JOIN FLEET_DRIVERQUEUELIST q on q.driverid = L.driverid and q.status = 1 " +
                                " inner join fleet_driver d on d.id = l.driverid and d.haspda = 1 " +
                                "  inner join fleet_driverworkingstatus s on s.id = q.driverworkstatusid" +
                                " left join fleet_master fm on q.fleetmasterid = fm.id" +
                                " where l.driverid = " + obj.bookingInfo.DriverId.ToInt();






                    var resp = db.ExecuteQuery<clsTrackDriver>(query).FirstOrDefault();


                    if (obj.bookingInfo.Id > 0)
                    {


                        if (obj.bookingInfo.FromAddress.ToStr().Trim().Length == 0)
                        {
                            var objBookingInfo = db.Bookings.Where(c => c.Id == obj.bookingInfo.Id).Select(args => new { args.FromAddress, args.ToAddress }).FirstOrDefault();


                            if (objBookingInfo != null)
                            {
                                obj.bookingInfo.FromAddress = objBookingInfo.FromAddress.ToStr();
                                obj.bookingInfo.ToAddress = objBookingInfo.ToAddress.ToStr();

                            }
                        }

                        var coord = db.stp_getCoordinatesByAddress(obj.bookingInfo.FromAddress.ToStr().Trim().ToUpper(), General.GetPostCodeMatch(obj.bookingInfo.FromAddress.ToStr().Trim().ToUpper())).FirstOrDefault();

                        if (coord != null)

                        {
                            resp.PickupAddress = new AddressInfo();
                            resp.PickupAddress.Address = obj.bookingInfo.FromAddress.ToStr();
                            resp.PickupAddress.Latitude = coord.Latitude;
                            resp.PickupAddress.Longitude = coord.Longtiude;

                        }

                        coord = db.stp_getCoordinatesByAddress(obj.bookingInfo.ToAddress.ToStr().Trim().ToUpper(), General.GetPostCodeMatch(obj.bookingInfo.ToAddress.ToStr().Trim().ToUpper())).FirstOrDefault();

                        if (coord != null)

                        {
                            resp.destinationAddress = new AddressInfo();
                            resp.destinationAddress.Address = obj.bookingInfo.ToAddress.ToStr();
                            resp.destinationAddress.Latitude = coord.Latitude;
                            resp.destinationAddress.Longitude = coord.Longtiude;

                        }
                    }

                    response.Data = resp;





                }



                //using (TaxiDataContext db = new TaxiDataContext("Data Source=88.198.21.250,58527;Initial Catalog=AscotCars;User ID=asc321;Password=asc321!;Trusted_Connection=False;"))
                //{






                //      data.listofdrivers = db.stp_GetDashboardDrivers(1).ToList();

                //    response.Data = data;




                //}

            }
            catch (Exception ex)
            {

                response.HasError = true;
                response.Message = ex.Message;
            }



            return new CustomJsonResult { Data = response };
        }
        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetTrackEscort")]
        public JsonResult GetTrackEscort(WebApiClasses.RequestWebApi obj)
        {
            //

            ResponseWebApi response = new ResponseWebApi();

            try
            {

                ClsDashboardModel data = new ClsDashboardModel();
                //using (TaxiDataContext db = new TaxiDataContext("Data Source=88.198.21.250,58527;Initial Catalog=AscotCars;User ID=asc321;Password=asc321!;Trusted_Connection=False;"))
                //{
                using (TaxiDataContext db = new TaxiDataContext())
                {

                    string query = @"
                    SELECT TOP 1
                        Id = ISNULL(z.Id, 0),
                        ZoneName = ISNULL(z.ZoneName, l.NewZoneName),
                        e.Id AS EscortId,
                        e.EscortNo,
                        b.DriverId,
                        l.PlotDate,
                        l.Latitude,
                        l.Longitude,
                        l.Speed,
                        LocationName = ISNULL(z.ZoneName, l.NewZoneName),
                        dq.DriverWorkStatusId,
                        ws.WorkStatus,
                        ws.BackgroundColor,
                        fm.PlateNo,
                        l.EstimatedTimeLeft,
                        UpdateDate = l.UpdateDate,
                        WaitSinceOn = ISNULL(dq.WaitSinceOn, GETDATE())
                    FROM Gen_Escort_Location l
                    INNER JOIN Gen_Escort e ON e.Id = l.EscortId
                    INNER JOIN Booking b ON b.Id = " + obj.bookingInfo.Id.ToInt() + @"
                    INNER JOIN Fleet_DriverQueueList dq ON dq.DriverId = b.DriverId AND dq.Status = 1
                    INNER JOIN Fleet_DriverWorkingStatus ws ON ws.Id = dq.DriverWorkStatusId
                    LEFT JOIN Gen_Zones z ON z.Id = l.ZoneId
                    LEFT JOIN Fleet_Master fm ON dq.FleetMasterId = fm.Id
                    WHERE l.EscortId = " + obj.bookingInfo.EscortId.ToInt() + @"
                    ORDER BY l.Id DESC";

                    var resp = db.ExecuteQuery<clsTrackEscort>(query).FirstOrDefault();


                    if (obj.bookingInfo.Id > 0)
                    {


                        if (obj.bookingInfo.FromAddress.ToStr().Trim().Length == 0)
                        {
                            var objBookingInfo = db.Bookings.Where(c => c.Id == obj.bookingInfo.Id).Select(args => new { args.FromAddress, args.ToAddress }).FirstOrDefault();


                            if (objBookingInfo != null)
                            {
                                obj.bookingInfo.FromAddress = objBookingInfo.FromAddress.ToStr();
                                obj.bookingInfo.ToAddress = objBookingInfo.ToAddress.ToStr();

                            }
                        }

                        var coord = db.stp_getCoordinatesByAddress(obj.bookingInfo.FromAddress.ToStr().Trim().ToUpper(), General.GetPostCodeMatch(obj.bookingInfo.FromAddress.ToStr().Trim().ToUpper())).FirstOrDefault();

                        if (coord != null)

                        {
                            resp.PickupAddress = new AddressInfo();
                            resp.PickupAddress.Address = obj.bookingInfo.FromAddress.ToStr();
                            resp.PickupAddress.Latitude = coord.Latitude;
                            resp.PickupAddress.Longitude = coord.Longtiude;

                        }

                        coord = db.stp_getCoordinatesByAddress(obj.bookingInfo.ToAddress.ToStr().Trim().ToUpper(), General.GetPostCodeMatch(obj.bookingInfo.ToAddress.ToStr().Trim().ToUpper())).FirstOrDefault();

                        if (coord != null)

                        {
                            resp.destinationAddress = new AddressInfo();
                            resp.destinationAddress.Address = obj.bookingInfo.ToAddress.ToStr();
                            resp.destinationAddress.Latitude = coord.Latitude;
                            resp.destinationAddress.Longitude = coord.Longtiude;

                        }
                    }

                    response.Data = resp;





                }





            }
            catch (Exception ex)
            {

                response.HasError = true;
                response.Message = ex.Message;
            }



            return new CustomJsonResult { Data = response };
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("UpdateBookingStatus")]
        public JsonResult UpdateBookingStatus(WebApiClasses.RequestWebApi obj)
        {
            //




            ResponseWebApi response = new ResponseWebApi();
            try
            {

                //
                try
                {
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "UpdateBookingStatus.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
                    General.WriteLog("UpdateBookingStatus", "json: " + new JavaScriptSerializer().Serialize(obj));
                }
                catch
                {

                }
                using (TaxiDataContext db = new TaxiDataContext())
                {

                    int driverId = obj.bookingInfo.DriverId.ToInt();

                    long Id = db.Fleet_DriverQueueLists.Where(a => a.DriverId == driverId && a.Status == true && a.CurrentJobId != null).Select(c => c.Id).FirstOrDefault().ToLong();


                    if (obj.bookingInfo.BookingStatusId.ToInt() == Enums.BOOKINGSTATUS.NOPICKUP)
                    {
                        //if (HubProcessor.Instance.objPolicy.EnableBookingOtherCharges.ToBool()

                        //{

                        //    FreezeTopRank(jobId, driverId, bookingStatusId.ToIntorNull(), driverStatusId.ToIntorNull());
                        //    rankChanged = 1;
                        //}
                        //else
                        //{

                        db.stp_UpdateJob(obj.bookingInfo.Id, driverId, Enums.BOOKINGSTATUS.NOPICKUP, Enums.Driver_WORKINGSTATUS.AVAILABLE, HubProcessor.Instance.objPolicy.SinBinTimer.ToInt());
                        //   }

                        General.CancelledJobFromController(driverId, obj.bookingInfo.Id);
                    }
                    else if (obj.bookingInfo.BookingStatusId.ToInt() == Enums.BOOKINGSTATUS.DISPATCHED)
                    {

                        var c = new
                        {
                            QueueId = Id,
                            ClearBy = obj.bookingInfo.EditLog.ToStr()


                        };

                        new SupplierController().manualclearjob(new JavaScriptSerializer().Serialize(c));
                    }
                    else if (obj.bookingInfo.BookingStatusId.ToInt() == Enums.BOOKINGSTATUS.NOSHOW)
                    {
                        //if (HubProcessor.Instance.objPolicy.EnableBookingOtherCharges.ToBool()

                        //{

                        //    FreezeTopRank(jobId, driverId, bookingStatusId.ToIntorNull(), driverStatusId.ToIntorNull());
                        //    rankChanged = 1;
                        //}
                        //else
                        //{

                        db.stp_UpdateJob(obj.bookingInfo.Id, driverId, Enums.BOOKINGSTATUS.WAITING, Enums.Driver_WORKINGSTATUS.AVAILABLE, HubProcessor.Instance.objPolicy.SinBinTimer.ToInt());
                        //   }


                        try
                        {
                            string username = obj.UserName.ToStr().Trim();

                            if (username.Length == 0)
                                username = "system";
                            //s
                            //  db.ExecuteQuery<int>("exec stp_BookingLog {0},{1},{2},{3} ", obj.bookingInfo.Id, obj.UserName.ToStr(), "Job is Recovered by Controller", DateTime.Now.GetUtcTimeZone());
                            db.stp_BookingLog(obj.bookingInfo.Id, username, "Job is Recovered by Controller");
                        }
                        catch
                        {


                        }

                        General.CancelledJobFromController(driverId, obj.bookingInfo.Id);


                    }
                    else if (obj.bookingInfo.BookingStatusId.ToInt() == Enums.BOOKINGSTATUS.CANCELLED)
                    {

                        CancelBooking(obj);
                    }
                    else if (obj.bookingInfo.BookingStatusId.ToInt() == Enums.BOOKINGSTATUS.FOJ)
                    {
                        //if (objBooking.BookingStatusId.ToInt() != Enums.BOOKINGSTATUS.WAITING && driverId > 0)
                        //{
                        //    if (db.Fleet_DriverQueueLists.Where(c => c.DriverId == driverId && c.CurrentJobId == objBooking.Id).Count() > 0)
                        //    {
                        //        //new Thread(delegate ()
                        //        //{
                        //            CancelBooking(obj);
                        //      //  }).Start();
                        //    }
                        //    else
                        //    {
                        //    ReCallFOJBookingFromPDA(bookingId, driverId);
                        //    }
                        // }


                        db.stp_UpdateJobStatus(obj.bookingInfo.Id, Enums.BOOKINGSTATUS.WAITING);

                        HubProcessor.Instance.listofJobs.Add(new clsPDA
                        {
                            DriverId = driverId,
                            JobId = obj.bookingInfo.Id,
                            MessageDateTime = DateTime.Now.AddSeconds(-30),
                            JobMessage = "Cancelled Foj Job>>" + obj.bookingInfo.Id,
                            MessageTypeId = 2
                        });
                        try
                        {
                            db.stp_BookingLog(obj.bookingInfo.Id, obj.UserName.ToStr(), "Job is Recovered by Controller");
                        }
                        catch
                        {


                        }
                    }
                    else if (obj.bookingInfo.BookingStatusId.ToInt() == Enums.BOOKINGSTATUS.PENDING_START)
                    {

                        db.stp_RecoverPreJob(obj.bookingInfo.Id, Enums.BOOKINGSTATUS.WAITING, driverId, "", obj.UserName.ToStr());

                        HubProcessor.Instance.listofJobs.Add(new clsPDA
                        {
                            DriverId = driverId,
                            JobId = obj.bookingInfo.Id,
                            MessageDateTime = DateTime.Now.AddSeconds(-30),
                            JobMessage = "Cancelled Pre Job>>" + obj.bookingInfo.Id,
                            MessageTypeId = 2
                        });
                        try
                        {
                            db.stp_BookingLog(obj.bookingInfo.Id, obj.UserName.ToStr(), "Job is Recovered by Controller");
                        }
                        catch
                        {


                        }
                        try
                        {
                            db.stp_BookingLog(obj.bookingInfo.Id, obj.UserName.ToStr(), "Job is Recovered by Controller");
                        }
                        catch
                        {


                        }
                    }
                }
                response.Data = obj.bookingInfo.Id;
                if (obj.bookingInfo.BookingStatusId.ToInt() != Enums.BOOKINGSTATUS.CANCELLED)
                {
                    CallGetDashboardData();
                }
            }
            catch (Exception ex)
            {
                try
                {
                    response.HasError = true;
                    response.Message = ex.Message;

                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "DispatchBooking_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message + Environment.NewLine);
                    General.WriteLog("DispatchBooking_exception", "json:" + new JavaScriptSerializer().Serialize(obj) + ", Exception: " + ex.Message);
                }
                catch
                {

                }
            }


            return Json(response, JsonRequestBehavior.AllowGet);

        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("UpdateHideJobStatus")]
        public JsonResult UpdateHideJobStatus(WebApiClasses.RequestWebApi obj)
        {
            //




            ResponseWebApi response = new ResponseWebApi();
            try
            {

                //
                try
                {
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "UpdateBookingStatus.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
                    General.WriteLog("UpdateBookingStatus", "json: " + new JavaScriptSerializer().Serialize(obj));
                }
                catch
                {

                }
                using (TaxiDataContext db = new TaxiDataContext())
                {
                    // Assuming obj.bookingInfo.Id has the Job/Booking ID
                    // and obj.bookingInfo.IsHideJobFromDrivers has the value (0 or 1)

                    long bookingId = obj.bookingInfo.Id;
                    bool? hideJob = obj.bookingInfo.IsHideJobFromDrivers;

                    // Inline SQL
                    string query = $"UPDATE booking SET IsHideJobFromDrivers = {(hideJob == true ? 1 : 0)} WHERE Id = {bookingId}";

                    db.ExecuteCommand(query); // Execute the inline query
                    db.stp_BookingLog(bookingId, obj.UserName.ToStr().Trim().Length > 0 ? obj.UserName.ToStr() : "controller", hideJob == true ? "Hide Job from driver" : "Show Job to Driver");
                }
                response.Data = obj.bookingInfo.Id;
                CallGetDashboardData();

            }
            catch (Exception ex)
            {

            }


            return Json(response, JsonRequestBehavior.AllowGet);

        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("LogoutDriver")]
        public JsonResult LogoutDriver(WebApiClasses.RequestWebApi obj)
        {
            //




            ResponseWebApi response = new ResponseWebApi();
            try
            {

                //
                try
                {
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "LogoutDriver.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
                    General.WriteLog("LogoutDriver", "json: " + new JavaScriptSerializer().Serialize(obj));
                }
                catch
                {

                }
                using (TaxiDataContext db = new TaxiDataContext())
                {


                    //
                    int driverId = obj.bookingInfo.DriverId.ToInt();

                    long Id = db.Fleet_DriverQueueLists.FirstOrDefault(c => c.Status == true && c.DriverId == driverId
                      && (c.DriverWorkStatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE || c.DriverWorkStatusId == Enums.Driver_WORKINGSTATUS.ONBREAK)).DefaultIfEmpty().Id;


                    if (Id > 0)
                    {


                        string driverNo = db.Fleet_Drivers.Where(c => c.Id == driverId).Select(c => c.DriverNo).FirstOrDefault().ToStr();

                        db.ExecuteQuery<int>("update fleet_driverqueuelist set logoutdatetime=getdate() ,status =0 where driverid=" + driverId + " and status=1");
                        //        General.AddUserLog("Driver {" + objMaster.Current.Fleet_Driver.DefaultIfEmpty().DriverNo.ToStr() + "} is forcefully logout by Controller", 3);
                        General.requestPDA("request force logout=" + driverNo + "=" + driverId);

                        CallGetDashboardDriversData();
                        response.Data = "Success";
                    }
                    else
                    {

                    }
                }


            }
            catch (Exception ex)
            {
                try
                {
                    response.HasError = true;
                    response.Message = ex.Message;

                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "DispatchBooking_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message + Environment.NewLine);
                    General.WriteLog("DispatchBooking_exception", "json:" + new JavaScriptSerializer().Serialize(obj) + ", Exception: " + ex.Message);
                }
                catch
                {

                }
            }


            return Json(response, JsonRequestBehavior.AllowGet);

        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetShiftJobs")]
        public JsonResult GetShiftJobs(WebApiClasses.RequestWebApi obj)
        {
            //




            ResponseWebApi response = new ResponseWebApi();
            try
            {

                //
                try
                {
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetShiftJobs.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
                    General.WriteLog("GetShiftJobs", "json: " + new JavaScriptSerializer().Serialize(obj));
                }
                catch
                {

                }
                using (TaxiDataContext db = new TaxiDataContext())
                {

                    int driverId = obj.bookingInfo.DriverId.ToInt();

                    var queue = db.Fleet_DriverQueueLists.Where(a => a.DriverId == driverId && a.Status == true).Select(c => new { c.Id, c.LoginDateTime })
                        .FirstOrDefault();




                    if (queue != null)
                    {
                        DateTime loginDateTime = queue.LoginDateTime.ToDateTime();

                        if (HubProcessor.Instance.objPolicy.EarningLoginHours.ToInt() > 0)
                        {

                            DateTime? newloginDateTime = loginDateTime.AddMinutes(-HubProcessor.Instance.objPolicy.EarningLoginHours.ToInt());

                            //  newloginDateTime = db.Fleet_DriverQueueLists.FirstOrDefault(c => c.LogoutDateTime.Value >= loginDateTime).DefaultIfEmpty().LoginDateTime.ToDateTimeorNull();


                            if (newloginDateTime != null)
                                loginDateTime = newloginDateTime.ToDateTime();

                        }

                        response.Data = db.Bookings.Where(c => c.PickupDateTime >= loginDateTime
                                                      && (c.DriverId == driverId || c.ReturnDriverId == driverId)
                                                    && c.BookingStatusId == Enums.BOOKINGSTATUS.DISPATCHED)
                                                    .Select(args => new { args.Id, args.BookingNo, PickupDate = string.Format("{0:dd/MM/yyyy HH:mm}", args.PickupDateTime), args.PickupDateTime, Pickup = args.FromAddress, Destination = args.ToAddress, Fares = args.FareRate + args.MeetAndGreetCharges + args.ExtraDropCharges })
                                                   .OrderByDescending(c => c.PickupDateTime).ToList();
                    }


                }


            }
            catch (Exception ex)
            {
                try
                {
                    response.HasError = true;
                    response.Message = ex.Message;

                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetShiftJobs_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message + Environment.NewLine);
                    General.WriteLog("GetShiftJobs_exception", "json:" + new JavaScriptSerializer().Serialize(obj) + ", Exception: " + ex.Message);
                }
                catch
                {

                }
            }


            return new CustomJsonResult { Data = response };

        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetCallHistory")]
        public JsonResult GetCallHistory(WebApiClasses.RequestWebApi obj)
        {
            //
            ResponseWebApi response = new ResponseWebApi();
            try
            {
                //
                try
                {
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetCallHistory.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
                    General.WriteLog("GetCallHistory", "json: " + new JavaScriptSerializer().Serialize(obj));
                }
                catch
                {
                }
                using (TaxiDataContext db = new TaxiDataContext())
                {
                    DateTime? fromDate = null;
                    DateTime? tillDate = null;
                    if (obj.callerInfo != null)
                    {
                        fromDate = obj.callerInfo.FromDate;
                        tillDate = obj.callerInfo.TillDate;
                    }
                    if (fromDate == null)
                    {
                        fromDate = DateTime.Now.ToDate();
                        tillDate = DateTime.Now.AddDays(1).ToDate();
                    }
                    string name = string.Empty;
                    string line = string.Empty;
                    string phone = string.Empty;
                    if (obj.callerInfo != null)
                    {
                        name = obj.callerInfo.Name.ToStr().Trim().ToLower();
                        line = obj.callerInfo.Extension.ToStr().Trim();
                        phone = obj.callerInfo.PhoneNumber.ToStr().Trim();
                    }
                    string VoipUrl = System.Configuration.ConfigurationManager.AppSettings["VoipUrl"];
                    var userName = db.CallerIdVOIP_Configurations.FirstOrDefault().UserName.ToStr();
                    response.Data = (from a in db.CallHistories
                                     join b in db.Gen_SubCompanies on a.CalledToNumber equals b.ConnectionString into table2
                                     from b in table2.DefaultIfEmpty()
                                     where (fromDate == null || a.CallDateTime.Value.Date >= fromDate)
                                      && (tillDate == null || a.CallDateTime.Value.Date <= tillDate)
                                     && (name == string.Empty || a.Name.Trim().ToLower() == name)
                                     //   && (MissedCalls == false || (a.STN == null || a.STN == ""))
                                     && (phone == string.Empty || a.PhoneNumber.Trim() == phone)
                                      && (line == string.Empty || (a.Line != null && a.Line.Trim() == line))
                                     //       && (stn == string.Empty || (a.STN != null && a.STN.Trim() == stn))
                                     //    && (IsAccepted == true || (a.IsAccepted != null && a.IsAccepted == true))
                                     //        && (SubCompanyNo == string.Empty || (a.CalledToNumber != null && a.CalledToNumber.Trim() == SubCompanyNo))
                                     orderby a.CallDateTime descending
                                     select new
                                     {
                                         Sno = a.Sno,
                                         Id = a.Id,
                                         Name = a.Name,
                                         PhoneNumber = a.PhoneNumber,
                                         CallDateTime = string.Format("{0:dd/MM/yyyy HH:mm:ss}", a.CallDateTime),
                                         Line = a.Line,
                                         STN = a.STN,
                                         Duration = a.CallDuration,
                                         IsMissed = (a.IsAccepted != null && a.IsAccepted == true) ? "1" : "0",
                                         Company = b != null && b.CompanyName != "" ? b.CompanyName : a.CalledToNumber,
                                         //  RecordingUrl = a.CallDuration.Contains(".") ? VoipUrl + "/" + userName + "/inbound/" + a.CallDuration + "_" + a.PhoneNumber : ""
                                         RecordingUrl = a.CallDuration.Contains(".") ? VoipUrl + "/" + userName + "/inbound/" + a.CallDuration + "_" + (a.PhoneNumber.StartsWith("0") ? "44" + a.PhoneNumber.Substring(1) : a.PhoneNumber) : ""
                                     }).ToList();
                }
            }
            catch (Exception ex)
            {
                try
                {
                    response.HasError = true;
                    response.Message = ex.Message;
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetCallHistory_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message + Environment.NewLine);
                    General.WriteLog("GetCallHistory_exception", "json:" + new JavaScriptSerializer().Serialize(obj) + ", Exception: " + ex.Message);
                }
                catch
                {
                }
            }
            return new CustomJsonResult { Data = response };
        }


        #endregion



        #region Phase#4



        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetAdvancedSearchDropDownData")]
        public JsonResult GetAdvancedSearchDropDownData(WebApiClasses.RequestWebApi obj)
        {
            //

            ResponseWebApi response = new ResponseWebApi();
            try
            {

                DataSet ds = new DataSet();
                using (System.Data.SqlClient.SqlConnection sqlconn = new System.Data.SqlClient.SqlConnection(Cryptography.Decrypt(System.Configuration.ConfigurationManager.AppSettings["ConnectionString"].ToStr(), "tcloudX@@!", true)))
                {

                    sqlconn.Open();

                    using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand())
                    {

                        cmd.Connection = sqlconn;

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "stp_fillsearchcombos";

                        using (System.Data.SqlClient.SqlDataAdapter da = new System.Data.SqlClient.SqlDataAdapter(cmd))
                        {

                            da.Fill(ds);
                        }

                    }


                    var BookingTypes = ds.Tables[0].AsEnumerable()
                     .Select(datarow => new
                     {
                         Id = datarow.Field<int>("Id"),
                         Name = datarow.Field<string>("Name")
                     });


                    var VehicleTypes = ds.Tables[1].AsEnumerable()
                     .Select(datarow => new
                     {
                         Id = datarow.Field<int>("Id"),
                         Name = datarow.Field<string>("Name")
                     });



                    var PaymentTypes = ds.Tables[2].AsEnumerable()
                     .Select(datarow => new
                     {
                         Id = datarow.Field<int>("Id"),
                         Name = datarow.Field<string>("Name")
                     });




                    var CompanyNames = ds.Tables[3].AsEnumerable()
                     .Select(datarow => new
                     {
                         Id = datarow.Field<int>("Id"),
                         Name = datarow.Field<string>("Name")
                     });




                    var BookingStatuses = ds.Tables[4].AsEnumerable()
                     .Select(datarow => new
                     {
                         Id = datarow.Field<int>("Id"),
                         Name = datarow.Field<string>("Name")
                     });




                    var DriversList = ds.Tables[5].AsEnumerable()
                     .Select(datarow => new
                     {
                         Id = datarow.Field<int>("Id"),
                         Name = datarow.Field<string>("Name")
                     });


                    var CustomersList = ds.Tables[6].AsEnumerable()
                     .Select(datarow => new
                     {
                         Id = datarow.Field<string>("Name"),
                         Name = datarow.Field<string>("Name")
                     });



                    response.Data = new { BookingTypes, VehicleTypes, PaymentTypes, CompanyNames, BookingStatuses, DriversList, CustomersList };


                }


            }
            catch (Exception ex)
            {
                response.HasError = true;
                response.Message = ex.Message;
            }


            return Json(response, JsonRequestBehavior.AllowGet);

        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetAdvancedSearch")]
        public JsonResult GetAdvancedSearch(WebApiClasses.RequestWebApi obj)
        {
            //




            ResponseWebApi response = new ResponseWebApi();
            try
            {

                //
                try
                {
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetAdvancedSearch.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
                    General.WriteLog("GetAdvancedSearch", "json: " + new JavaScriptSerializer().Serialize(obj));
                }
                catch
                {

                }
                using (TaxiDataContext db = new TaxiDataContext())
                {

                    // int driverId = obj.bookingInfo.DriverId.ToInt();

                    if (obj.searchInfo == null)
                        obj.searchInfo = new stp_searchbookings();

                    DateTime? startDate = obj.searchInfo.FromDate;
                    DateTime? endDate = obj.searchInfo.TillDate;

                    if (endDate != null && endDate.Value.Minute == 0 && endDate.Value.Hour == 0)
                    {
                        endDate = endDate.Value.ToDate() + new TimeSpan(23, 59, 59);

                    }


                    if (startDate == null)
                        startDate = DateTime.Now.ToDate();


                    if (endDate == null)
                        endDate = DateTime.Now.ToDate() + new TimeSpan(23, 59, 59);

                    string email = obj.searchInfo.Email.ToStr().Trim();
                    string orderNo = obj.searchInfo.OrderNo.ToStr().Trim();
                    string refNo = obj.searchInfo.RefNumber.ToStr().Trim();
                    string phoneNo = obj.searchInfo.PhoneNo.ToStr().Trim();
                    string mobNo = obj.searchInfo.MobileNo.ToStr().Trim();
                    string customerName = obj.searchInfo.Passenger.ToStr().Trim();
                    string pickUp = obj.searchInfo.From.ToStr().Trim().ToLower();
                    string via = obj.searchInfo.Via.ToStr().Trim().ToLower();
                    string destination = obj.searchInfo.To.ToStr().Trim().ToLower();
                    string paymentRef = obj.searchInfo.PaymentRef.ToStr().Trim().ToLower();

                    int companyId = obj.searchInfo.CompanyId.ToInt();
                    int paymentTypeId = obj.searchInfo.PaymentTypeId.ToInt();
                    int statusId = obj.searchInfo.BookingStatusId.ToInt();
                    int BookingTypeId = obj.searchInfo.BookingTypeId.ToInt();
                    int vehicleTypeId = obj.searchInfo.VehicleTypeId.ToInt();
                    int driverId = obj.searchInfo.DriverId.ToInt();
                    int companyVehId = 0;
                    bool withQuotation = false;

                    int searchDateTypeId = obj.searchInfo.searchDateType.ToInt();
                    string tokenNo = "";

                    bool withRecording = false;

                    long ID = 0;

                    if (refNo.Length > 0)
                    {

                        startDate = null;
                        endDate = null;

                        if (refNo.ToStr().Trim().IsNumeric())
                            ID = refNo.ToStr().Trim().ToLong();
                        else
                        {
                            try
                            {

                                string dummyID = string.Empty;

                                for (int i = 0; i <= refNo.Length; i++)
                                {
                                    if (char.IsNumber(refNo[i]))
                                        dummyID += refNo[i];
                                    else
                                        continue;
                                }

                                if (dummyID.ToStr().Trim().IsNumeric())
                                    ID = dummyID.ToStr().Trim().ToLong();
                            }
                            catch
                            {

                            }
                        }
                    }

                    if (startDate == null)
                        startDate = new DateTime(1900, 1, 1);

                    if (endDate == null)
                        endDate = new DateTime(1900, 1, 1);





                    searchDateTypeId = 1;

                    var query = db.ExecuteQuery<stp_searchbookings>("exec stp_searchbookings {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23} ",
                                                 searchDateTypeId,
                                                 startDate,
                                                 endDate,
                                                 companyId,
                                                 companyVehId,
                                                 vehicleTypeId,
                                                 paymentTypeId,
                                                 statusId,
                                                 BookingTypeId,
                                                 customerName,
                                                 phoneNo,
                                                 mobNo,
                                                 pickUp,
                                                 via,
                                                 destination,
                                                 refNo,
                                                 orderNo,
                                                 paymentRef,
                                                 withQuotation,
                                                 withRecording,
                                                0, //subcompanyid
                                                 driverId,
                                                 ID,
                                                 email
                                                 ).ToList();




                    response.Data = query;


                }
            }
            catch (Exception ex)
            {
                try
                {
                    response.HasError = true;
                    response.Message = ex.Message;

                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetAdvancedSearch_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message + Environment.NewLine);
                    General.WriteLog("GetAdvancedSearch_exception", "json:" + new JavaScriptSerializer().Serialize(obj) + ", Exception: " + ex.Message);
                }
                catch
                {

                }
            }


            return new CustomJsonResult { Data = response };

        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetAccountDetails")]
        public JsonResult GetAccountDetails(WebApiClasses.RequestWebApi obj)
        {
            //

            ResponseWebApi response = new ResponseWebApi();
            try
            {

                DataSet ds = new DataSet();
                using (TaxiDataContext db = new TaxiDataContext())
                {
                    int companyId = obj.bookingInfo.CompanyId.ToInt();

                    var objCompany = db.Gen_Companies.Where(c => c.Id == companyId).Select(args => new { args.HasOrderNo, args.HasBookedBy, args.HasEscort, args.PasswordEnable }).FirstOrDefault();


                    if (objCompany != null)
                    {
                        var depts = db.Gen_Company_Departments.Where(c => c.CompanyId == companyId).Select(args => new { args.Id, Name = args.DepartmentName }).ToList();

                        var bookedbys = db.Gen_Company_BookedBies.Where(c => c.CompanyId == companyId).Select(args => new { args.Id, Name = args.BookedBy }).ToList();


                        var orderNos = db.Gen_Company_OrderNumbers.Where(c => c.CompanyId == companyId).Select(args => new { args.Id, Name = args.OrderNo }).ToList();


                        var escorts = db.Gen_Escorts.Select(args => new { args.Id, Name = args.EscortName }).ToList();




                        bool HasDepartment = depts.Count > 0 ? true : false;
                        bool HasOrderNo = objCompany.HasOrderNo.ToBool();
                        bool HasEscort = objCompany.HasEscort.ToBool();
                        bool HasBookedBy = objCompany.HasBookedBy.ToBool();
                        bool HasPassword = objCompany.PasswordEnable.ToBool();




                        response.Data = new { depts, bookedbys, orderNos, escorts, HasDepartment, HasOrderNo, HasBookedBy, HasEscort, HasPassword };

                    }
                    else
                    {
                        response.Data = new { HasOrderNo = default(bool?) };


                    }
                }


            }
            catch (Exception ex)
            {
                response.HasError = true;
                response.Message = ex.Message;
            }


            return Json(response, JsonRequestBehavior.AllowGet);

        }

        #endregion



        #region Phase#5
        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("UpdateAdvanceBooking")]
        public JsonResult UpdateAdvanceBooking(WebApiClasses.RequestWebApi obj)
        {
            //


            BookingBO objMaster = new BookingBO();

            ResponseWebApi response = new ResponseWebApi();
            try
            {


                try
                {
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "UpdateAdvanceBooking.txt", DateTime.Now + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
                    General.WriteLog("UpdateAdvanceBooking", "json: " + new JavaScriptSerializer().Serialize(obj));
                }
                catch
                {

                }


                using (TaxiDataContext db = new TaxiDataContext())
                {


                    long? AdvanceBookingId = obj.advancebookingInfo.AdvanceBookingId.ToLong();





                    AdvanceBookingBO objAdvBO = new AdvanceBookingBO();
                    if (AdvanceBookingId == 0)
                    {


                        objAdvBO.New();


                        objAdvBO.Current.CustomerName = obj.advancebookingInfo.CustomerName.ToStr();
                        objAdvBO.Current.CustomerTelephoneNo = obj.advancebookingInfo.CustomerPhoneNo.ToStr();
                        objAdvBO.Current.CustomerMobileNo = obj.advancebookingInfo.CustomerMobileNo.ToStr();
                        objAdvBO.Current.CustomerEmail = obj.advancebookingInfo.CustomerEmail.ToStr();
                        objAdvBO.Current.FromAddress = obj.advancebookingInfo.FromAddress.ToStr();
                        objAdvBO.Current.ToAddress = obj.advancebookingInfo.ToAddress.ToStr();



                        objAdvBO.Current.AddOn = DateTime.Now;
                    }
                    else
                    {

                        objAdvBO.GetByPrimaryKey(AdvanceBookingId);

                        objAdvBO.Edit();

                        objAdvBO.Current.EditOn = DateTime.Now;

                        objAdvBO.Current.CustomerName = obj.advancebookingInfo.CustomerName.ToStr();
                        objAdvBO.Current.CustomerTelephoneNo = obj.advancebookingInfo.CustomerPhoneNo.ToStr();
                        objAdvBO.Current.CustomerMobileNo = obj.advancebookingInfo.CustomerMobileNo.ToStr();
                        objAdvBO.Current.CustomerEmail = obj.advancebookingInfo.CustomerEmail.ToStr();
                        objAdvBO.Current.FromAddress = obj.advancebookingInfo.FromAddress.ToStr();
                        objAdvBO.Current.ToAddress = obj.advancebookingInfo.ToAddress.ToStr();
                    }
                    //  objAdvBO.Current.AddLog = AppVars.LoginObj.UserName.ToStr();
                    //   objAdvBO.Current.AddBy = AppVars.LoginObj.LuserId.ToIntorNull();
                    //  }
                    //else
                    //{

                    //

                    //    objAdvBO.GetByPrimaryKey(savedAdvanceBookingId);

                    //    objAdvBO.Edit();




                    //}

                    //objAdvBO.Current.PickupDateTime = obj.advancebookingInfo.PickupDateTime;

                    objAdvBO.Save();

                    // AdvanceBookingId = objAdvBO.Current.Id;



                    if (obj.advancebookingInfo.Ids != null && obj.advancebookingInfo.Ids.Any())
                    {
                        var bookingsToUpdate = db.Bookings
                            .Where(c => obj.advancebookingInfo.Ids.Contains(c.Id)
                                        && c.BookingStatusId == Enums.BOOKINGSTATUS.WAITING
                                        && c.MasterJobId == null)
                            .ToList();

                        foreach (var booking in bookingsToUpdate)
                        {
                            booking.FromAddress = obj.advancebookingInfo.FromAddress.ToStr();
                            booking.ToAddress = obj.advancebookingInfo.ToAddress.ToStr();

                            booking.PickupDateTime = booking.PickupDateTime.ToDate() + obj.advancebookingInfo.PickupDateTime.Value.TimeOfDay;

                            booking.CustomerName = obj.advancebookingInfo.CustomerName.ToStr();
                            booking.CustomerEmail = obj.advancebookingInfo.CustomerEmail.ToStr();
                            booking.CustomerMobileNo = obj.advancebookingInfo.CustomerMobileNo.ToStr();
                            booking.CustomerPhoneNo = obj.advancebookingInfo.CustomerPhoneNo.ToStr();

                            booking.FareRate = obj.advancebookingInfo.FareRate.ToDecimal();
                            booking.CompanyPrice = obj.advancebookingInfo.CompanyPrice.ToDecimal();

                            //try
                            //{
                            //    if (obj.advancebookingInfo.BookingReturn != null)
                            //        booking.ReturnFareRate = obj.advancebookingInfo.BookingReturn.FareRate.ToDecimal();
                            //}
                            //catch
                            //{
                            //    // log if needed
                            //}

                            booking.IsCompanyWise = obj.advancebookingInfo.CompanyId != null;
                            booking.CompanyId = obj.advancebookingInfo.CompanyId.ToIntorNull();

                            booking.VehicleTypeId = obj.advancebookingInfo.VehicleTypeId.ToIntorNull();
                            booking.OrderNo = obj.advancebookingInfo.OrderNo.ToStr();
                            booking.DepartmentId = obj.advancebookingInfo.DepartmentId.ToIntorNull();
                            booking.PaymentTypeId = obj.advancebookingInfo.PaymentTypeId.ToIntorNull();
                            booking.DriverId = obj.advancebookingInfo.DriverId.ToIntorNull();

                            booking.IsConfirmedDriver = obj.advancebookingInfo.DriverId != null;


                            string query1 = "delete from Booking_ViaLocations where BookingId={0}";
                            db.ExecuteCommand(query1, booking.Id);
                            if (obj.advancebookingInfo.Booking_ViaLocations != null)
                            {
                                foreach (var item in obj.advancebookingInfo.Booking_ViaLocations)
                                {
                                    string queryR = "INSERT INTO Booking_ViaLocations (ViaLocTypeLabel, ViaLocTypeValue, BookingId, ViaLocTypeId, ViaLocValue,ViaLocId) VALUES ({0}, {1}, {2}, {3}, {4},NULLIF({5},0))";
                                    db.ExecuteCommand(queryR, item.ViaLocTypeLabel != null ? item.ViaLocTypeLabel : "", item.ViaLocTypeValue != null ? item.ViaLocTypeValue : "", booking.Id, Enums.LOCATION_TYPES.ADDRESS, item.ViaLocValue, item.ViaLocId > 0 ? item.ViaLocId : 0);
                                }
                            }
                        }
                    }

                    if (obj.advancebookingInfo.BookingReturn != null
    && obj.advancebookingInfo.BookingReturn.Ids != null
    && obj.advancebookingInfo.BookingReturn.Ids.Any())
                    {
                        var bookingsToUpdate = db.Bookings
                            .Where(c => obj.advancebookingInfo.BookingReturn.Ids.Contains(c.Id)
                                        && c.BookingStatusId == Enums.BOOKINGSTATUS.WAITING && c.MasterJobId != null)
                            .ToList();

                        foreach (var booking in bookingsToUpdate)
                        {
                            booking.FromAddress = obj.advancebookingInfo.BookingReturn.FromAddress.ToStr();
                            booking.ToAddress = obj.advancebookingInfo.BookingReturn.ToAddress.ToStr();


                            booking.PickupDateTime = booking.PickupDateTime.ToDate() + obj.advancebookingInfo.BookingReturn.ReturnPickupDateTime.Value.TimeOfDay;
                            //   booking.PickupDateTime = obj.advancebookingInfo.BookingReturn.ReturnPickupDateTime;

                            booking.CustomerName = obj.advancebookingInfo.BookingReturn.CustomerName.ToStr();

                            booking.CustomerEmail = obj.advancebookingInfo.BookingReturn.CustomerEmail.ToStr();

                            booking.CustomerMobileNo = obj.advancebookingInfo.BookingReturn.CustomerMobileNo.ToStr();

                            booking.CustomerPhoneNo = obj.advancebookingInfo.BookingReturn.CustomerPhoneNo.ToStr();


                            booking.FareRate = obj.advancebookingInfo.BookingReturn.FareRate.ToDecimal();

                            booking.CompanyPrice = obj.advancebookingInfo.BookingReturn.CompanyPrice.ToDecimal();


                            if (obj.advancebookingInfo.BookingReturn.CompanyId == null)
                            {

                                booking.IsCompanyWise = false;

                            }
                            else
                            {
                                booking.IsCompanyWise = true;
                            }
                            booking.CompanyId = obj.advancebookingInfo.BookingReturn.CompanyId.ToIntorNull();



                            booking.VehicleTypeId = obj.advancebookingInfo.BookingReturn.VehicleTypeId.ToIntorNull();
                            booking.OrderNo = obj.advancebookingInfo.BookingReturn.OrderNo.ToStr();

                            booking.DepartmentId = obj.advancebookingInfo.BookingReturn.DepartmentId.ToIntorNull();
                            booking.PaymentTypeId = obj.advancebookingInfo.BookingReturn.PaymentTypeId.ToIntorNull();

                            if (obj.advancebookingInfo.BookingReturn != null)
                            {
                                booking.DriverId = obj.advancebookingInfo.BookingReturn.DriverId.ToIntorNull();
                                //   if (obj.advancebookingInfo.BookingReturn.DriverId != null)
                                booking.IsConfirmedDriver = obj.advancebookingInfo.BookingReturn.DriverId != null;
                            }

                            string query1 = "delete from Booking_ViaLocations where BookingId={0}";
                            db.ExecuteCommand(query1, booking.Id);
                            if (obj.advancebookingInfo.BookingReturn.Booking_ViaLocations != null)
                            {
                                foreach (var item in obj.advancebookingInfo.BookingReturn.Booking_ViaLocations)
                                {
                                    string queryR = "INSERT INTO Booking_ViaLocations (ViaLocTypeLabel, ViaLocTypeValue, BookingId, ViaLocTypeId, ViaLocValue,ViaLocId) VALUES ({0}, {1}, {2}, {3}, {4},NULLIF({5},0))";
                                    db.ExecuteCommand(queryR, item.ViaLocTypeLabel != null ? item.ViaLocTypeLabel : "", item.ViaLocTypeValue != null ? item.ViaLocTypeValue : "", booking.Id, Enums.LOCATION_TYPES.ADDRESS, item.ViaLocValue, item.ViaLocId > 0 ? item.ViaLocId : 0);
                                }
                            }
                        }
                    }




                    db.SubmitChanges();

                    response.Data = "";


                    //General.MessageToPDA("request broadcast=" + DispatchHub.RefreshTypes.REFRESH_REQUIRED_DASHBOARD + "=" + objMaster.Current.Id);




                }
            }
            catch (Exception ex)
            {
                try
                {
                    response.HasError = true;

                    if (objMaster.Errors.Count == 0)
                    {
                        response.Message = ex.Message;
                        //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "UpdateAdvanceBooking_exception.txt", DateTime.Now + ",json:" + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message + Environment.NewLine);
                        General.WriteLog("UpdateAdvanceBooking_exception", "json:" + new JavaScriptSerializer().Serialize(obj) + ", Exception: " + ex.Message);

                    }
                    else
                    {
                        response.Message = objMaster.ShowErrors();
                        //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "UpdateAdvanceBooking_validation.txt", DateTime.Now + ",json:" + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message + Environment.NewLine);
                        General.WriteLog("UpdateAdvanceBooking_validation", "json:" + new JavaScriptSerializer().Serialize(obj) + ", Exception: " + ex.Message);
                    }
                }
                catch
                {

                }
            }


            return Json(response, JsonRequestBehavior.AllowGet);

        }

        //[System.Web.Http.HttpGet]
        //[System.Web.Http.HttpPost]
        //[System.Web.Http.Route("UpdateAdvanceBooking")]
        //public JsonResult UpdateAdvanceBooking(WebApiClasses.RequestWebApi obj)
        //{
        //    //


        //    BookingBO objMaster = new BookingBO();

        //    ResponseWebApi response = new ResponseWebApi();
        //    try
        //    {


        //        try
        //        {
        //            System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "UpdateAdvanceBooking.txt", DateTime.Now + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
        //        }
        //        catch
        //        {

        //        }


        //        using (TaxiDataContext db = new TaxiDataContext())
        //        {


        //            long? AdvanceBookingId = obj.advancebookingInfo.AdvanceBookingId.ToLong();





        //            AdvanceBookingBO objAdvBO = new AdvanceBookingBO();
        //            if (AdvanceBookingId == 0)
        //            {


        //                objAdvBO.New();


        //                objAdvBO.Current.CustomerName = obj.advancebookingInfo.CustomerName.ToStr();
        //                objAdvBO.Current.CustomerTelephoneNo = obj.advancebookingInfo.CustomerPhoneNo.ToStr();
        //                objAdvBO.Current.CustomerMobileNo = obj.advancebookingInfo.CustomerMobileNo.ToStr();
        //                objAdvBO.Current.CustomerEmail = obj.advancebookingInfo.CustomerEmail.ToStr();
        //                objAdvBO.Current.FromAddress = obj.advancebookingInfo.FromAddress.ToStr();
        //                objAdvBO.Current.ToAddress = obj.advancebookingInfo.ToAddress.ToStr();



        //                objAdvBO.Current.AddOn = DateTime.Now;
        //            }
        //            else
        //            {

        //                objAdvBO.GetByPrimaryKey(AdvanceBookingId);

        //                objAdvBO.Edit();

        //                objAdvBO.Current.EditOn = DateTime.Now;

        //                objAdvBO.Current.CustomerName = obj.advancebookingInfo.CustomerName.ToStr();
        //                objAdvBO.Current.CustomerTelephoneNo = obj.advancebookingInfo.CustomerPhoneNo.ToStr();
        //                objAdvBO.Current.CustomerMobileNo = obj.advancebookingInfo.CustomerMobileNo.ToStr();
        //                objAdvBO.Current.CustomerEmail = obj.advancebookingInfo.CustomerEmail.ToStr();
        //                objAdvBO.Current.FromAddress = obj.advancebookingInfo.FromAddress.ToStr();
        //                objAdvBO.Current.ToAddress = obj.advancebookingInfo.ToAddress.ToStr();
        //            }
        //            //  objAdvBO.Current.AddLog = AppVars.LoginObj.UserName.ToStr();
        //            //   objAdvBO.Current.AddBy = AppVars.LoginObj.LuserId.ToIntorNull();
        //            //  }
        //            //else
        //            //{

        //            //

        //            //    objAdvBO.GetByPrimaryKey(savedAdvanceBookingId);

        //            //    objAdvBO.Edit();

        //            //


        //            //}

        //            //objAdvBO.Current.PickupDateTime = obj.advancebookingInfo.PickupDateTime;

        //            objAdvBO.Save();

        //            // AdvanceBookingId = objAdvBO.Current.Id;










        //            foreach (var booking in db.Bookings.Where(c => c.AdvanceBookingId == AdvanceBookingId && c.BookingStatusId == Enums.BOOKINGSTATUS.WAITING && c.MasterJobId == null))
        //            {
        //                //foreach (var item in obj.advancebookingInfo.GetType().GetProperties())
        //                //{
        //                //    try
        //                //    {



        //                //        if (item.Name == "Id")
        //                //            continue;

        //                //        if (item.Name == "MasterJobId")
        //                //            continue;

        //                //        if (item.Name == "BookingReturn")
        //                //            continue;

        //                //        if (item.Name == "AdvanceBookingId")
        //                //            continue;

        //                //        if (item.Name == "JourneyTypeId")
        //                //            continue;


        //                //        booking.GetType().GetProperty(item.Name).SetValue(booking, item.GetValue(obj.advancebookingInfo));


        //                //    }
        //                //    catch (Exception ex)
        //                //    {

        //                //    }

        //                //}

        //                booking.FromAddress = obj.advancebookingInfo.FromAddress.ToStr();
        //                booking.ToAddress = obj.advancebookingInfo.ToAddress.ToStr();

        //                booking.PickupDateTime = obj.advancebookingInfo.PickupDateTime;

        //                booking.CustomerName = obj.advancebookingInfo.CustomerName.ToStr();

        //                booking.CustomerEmail = obj.advancebookingInfo.CustomerEmail.ToStr();

        //                booking.CustomerMobileNo = obj.advancebookingInfo.CustomerMobileNo.ToStr();

        //                booking.CustomerPhoneNo = obj.advancebookingInfo.CustomerPhoneNo.ToStr();
        //                if (obj.advancebookingInfo.CompanyId == null)
        //                {

        //                    booking.IsCommissionWise = false;

        //                }
        //                else
        //                {
        //                    booking.IsCommissionWise = true;
        //                }
        //                booking.CompanyId = obj.advancebookingInfo.CompanyId.ToIntorNull();



        //                booking.VehicleTypeId = obj.advancebookingInfo.VehicleTypeId.ToIntorNull();
        //                booking.OrderNo = obj.advancebookingInfo.OrderNo.ToStr();

        //                booking.DepartmentId = obj.advancebookingInfo.DepartmentId.ToIntorNull();
        //                booking.PaymentTypeId = obj.advancebookingInfo.PaymentTypeId.ToIntorNull();

        //            }
        //            //

        //            foreach (var booking in db.Bookings.Where(c => c.AdvanceBookingId == AdvanceBookingId && c.BookingStatusId == Enums.BOOKINGSTATUS.WAITING && c.MasterJobId != null))
        //            {


        //                booking.FromAddress = obj.advancebookingInfo.BookingReturn.FromAddress.ToStr();
        //                booking.ToAddress = obj.advancebookingInfo.BookingReturn.ToAddress.ToStr();

        //                booking.PickupDateTime = obj.advancebookingInfo.BookingReturn.ReturnPickupDateTime;

        //                booking.CustomerName = obj.advancebookingInfo.BookingReturn.CustomerName.ToStr();

        //                booking.CustomerEmail = obj.advancebookingInfo.BookingReturn.CustomerEmail.ToStr();

        //                booking.CustomerMobileNo = obj.advancebookingInfo.BookingReturn.CustomerMobileNo.ToStr();

        //                booking.CustomerPhoneNo = obj.advancebookingInfo.BookingReturn.CustomerPhoneNo.ToStr();
        //                if (obj.advancebookingInfo.BookingReturn.CompanyId == null)
        //                {

        //                    booking.IsCommissionWise = false;

        //                }
        //                else
        //                {
        //                    booking.IsCommissionWise = true;
        //                }
        //                booking.CompanyId = obj.advancebookingInfo.BookingReturn.CompanyId.ToIntorNull();



        //                booking.VehicleTypeId = obj.advancebookingInfo.BookingReturn.VehicleTypeId.ToIntorNull();
        //                booking.OrderNo = obj.advancebookingInfo.BookingReturn.OrderNo.ToStr();

        //                booking.DepartmentId = obj.advancebookingInfo.BookingReturn.DepartmentId.ToIntorNull();
        //                booking.PaymentTypeId = obj.advancebookingInfo.BookingReturn.PaymentTypeId.ToIntorNull();

        //                //foreach (var item in obj.advancebookingInfo.BookingReturn.GetType().GetProperties())
        //                //{
        //                //    try
        //                //    {
        //                //        //


        //                //        if (item.Name == "Id")
        //                //            continue;

        //                //        if (item.Name == "MasterJobId")
        //                //            continue;

        //                //        if (item.Name == "AdvanceBookingId")
        //                //            continue;



        //                //        if (item.Name == "BookingStatusId")
        //                //            continue;


        //                //        if (item.Name == "JourneyTypeId")
        //                //            continue;

        //                //        if (item.Name == "booking.Booking1")
        //                //            continue;
        //                //        if (item.Name == "Booking1")
        //                //            continue;

        //                //        //

        //                //        booking.GetType().GetProperty(item.Name).SetValue(booking, item.GetValue(obj.advancebookingInfo.BookingReturn));
        //                //        //




        //                //    }
        //                //    catch (Exception ex)
        //                //    {

        //                //    }

        //                //}

        //            }

        //            db.SubmitChanges();

        //            response.Data = "";


        //            //General.MessageToPDA("request broadcast=" + DispatchHub.RefreshTypes.REFRESH_REQUIRED_DASHBOARD + "=" + objMaster.Current.Id);

        //        //


        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        try
        //        {
        //            response.HasError = true;

        //            if (objMaster.Errors.Count == 0)
        //            {
        //                response.Message = ex.Message;
        //                System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "UpdateAdvanceBooking_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message + Environment.NewLine);


        //            }
        //            else
        //            {
        //                response.Message = objMaster.ShowErrors();
        //                System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "UpdateAdvanceBooking_validation.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message + Environment.NewLine);

        //            }
        //        }
        //        catch
        //        {

        //        }
        //    }


        //    return Json(response, JsonRequestBehavior.AllowGet);

        //}


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("UpdateAutoDispatchAction")]
        public JsonResult UpdateAutoDispatchAction(WebApiClasses.RequestWebApi obj)
        {
            //

            try
            {

                General.WriteLog("UpdateAutoDispatchAction", "json: " + new JavaScriptSerializer().Serialize(obj));
                //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "UpdateAutoDispatchAction.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
            }
            catch
            {

            }
            ResponseWebApi response = new ResponseWebApi();

            try
            {



                using (TaxiDataContext db = new TaxiDataContext())
                {




                    bool EnableAutoDespatchMode = obj.autoDispatchInfo.EnableAuto.ToBool();


                    try
                    {

                        if (EnableAutoDespatchMode)
                        {

                            db.stp_RunProcedure("update Gen_SysPolicy_Configurations set EnableAutoDespatch=1");


                        }
                        else
                        {

                            db.stp_RunProcedure("update Gen_SysPolicy_Configurations set EnableAutoDespatch=0");




                        }

                        HubProcessor.Instance.objPolicy.EnableAutoDespatch = EnableAutoDespatchMode;







                        General.BroadcastToWebControllers("**autodespatchmode>>" + EnableAutoDespatchMode + ">>" + Environment.MachineName.ToLower());







                    }
                    catch (Exception ex)
                    {
                        //  MessageBox.Show(ex.Message);
                        //   IsPerformingAutoDespatchActivity = false;

                        try
                        {

                            //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "UpdateAutoDispatchAction_exception1.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message + Environment.NewLine);
                            General.WriteLog("UpdateAutoDispatchAction_exception1", "json:" + new JavaScriptSerializer().Serialize(obj) + ", Exception: " + ex.Message);
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
                    response.HasError = true;
                    response.Message = ex.Message;

                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "UpdateAutoDispatchAction_exception2.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message + Environment.NewLine);
                    General.WriteLog("UpdateAutoDispatchAction_exception2", "json:" + new JavaScriptSerializer().Serialize(obj) + ", Exception: " + ex.Message);
                }
                catch
                {

                }
            }


            //   return Json(response, JsonRequestBehavior.AllowGet);
            return new CustomJsonResult { Data = response };
        }






        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetAdvanceBookingList")]
        public JsonResult GetAdvanceBookingList(WebApiClasses.RequestWebApi obj)
        {
            //

            ResponseWebApi response = new ResponseWebApi();

            using (TaxiDataContext db = new TaxiDataContext())
            {


                db.DeferredLoadingEnabled = false;

                int subCompanyId = obj.bookingInfo.SubcompanyId.ToInt();

                try
                {
                    var query = (from a in db.AdvanceBookings
                                 .Where(c => (c.AdvBookingTypeId == null || c.AdvBookingTypeId == 2 || c.AdvBookingTypeId == 3 || c.AdvBookingTypeId == 4))
                                     //&& (AppVars.DefaultBookingSubCompanyId == 0 || (c.AdvanceBookingNo != null && c.AdvanceBookingNo == subCompanyId)))
                                     //   join b in db.Gen_Companies on a.CompanyId equals b.Id into table2
                                     // from b in table2.DefaultIfEmpty()
                                 select new
                                 {
                                     Id = a.Id,
                                     AddOn = a.AddOn,
                                     RefNumber = a.AdvanceBookingNo,
                                     BookingDate = a.AddOn,

                                     Passenger = a.CustomerName,
                                     ContactNo = a.CustomerTelephoneNo != null && a.CustomerTelephoneNo != "" ? a.CustomerMobileNo + " - " + a.CustomerTelephoneNo : a.CustomerMobileNo,
                                     From = a.FromAddress,
                                     To = a.ToAddress,
                                     a.AdvBookingTypeId,
                                     EndDate = a.PickupDateTime
                                 }).ToList();

                    response.Data = query;

                }
                catch (Exception ex)
                {
                    response.Message = ex.Message;
                    response.HasError = true;
                }

                //    grdLister.MasterTemplate.BeginUpdate();
                //  grdLister.DataSource = query;
            }



            return new CustomJsonResult { Data = response };






        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetSingleAdvanceBookingDetails")]
        public JsonResult GetSingleAdvanceBookingDetails(WebApiClasses.RequestWebApi obj)
        {
            try
            {
                //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetAdvanceBookingDetails.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
                General.WriteLog("GetAdvanceBookingDetails", "json: " + new JavaScriptSerializer().Serialize(obj));
            }
            catch
            {

            }

            ResponseWebApi response = new ResponseWebApi();
            using (TaxiDataContext db = new TaxiDataContext())
            {
                db.DeferredLoadingEnabled = false;

                long Id = obj.bookingInfo == null ? 0 : obj.bookingInfo.Id.ToLong();
                try
                {
                    if (Id > 0)
                    {
                        var list = db.Bookings.Where(c => c.Id == Id).ToList();

                        var objFirstBooking = list.FirstOrDefault(c => c.BookingStatusId == Enums.BOOKINGSTATUS.WAITING);


                        if (objFirstBooking == null)
                            objFirstBooking = list.FirstOrDefault();

                        if (objFirstBooking != null)
                        {
                            foreach (var item in objFirstBooking.GetType().GetProperties())
                            {
                                try
                                {
                                    if (obj.bookingInfo.GetType().GetProperty(item.Name) != null)
                                        obj.bookingInfo.GetType().GetProperty(item.Name).SetValue(obj.bookingInfo, item.GetValue(objFirstBooking));
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }

                        obj.bookingInfo.PickupDateTimeStr = obj.bookingInfo.PickupDateTime.Value.ToString("yyyy-MM-dd HH:mm:ss tt");

                        var lists = (from a in list
                                     join status in db.BookingStatus
                                         on a.BookingStatusId equals status.Id
                                     select new
                                     {
                                         Id = a.Id,
                                         ReturnFareRate = a.ReturnFareRate,
                                         Booking_ViaLocations = db.Booking_ViaLocations.Where(x => x.BookingId == a.Id).ToList(), //a.Booking_ViaLocations,
                                         PickupDateTime = a.PickupDateTime,
                                         FromAddress = a.FromAddress,
                                         ToAddress = a.ToAddress,
                                         FareRate = a.FareRate,
                                         MasterJobId = a.MasterJobId,
                                         BookingStatus = status.StatusName,
                                         JourneyTypeId = a.JourneyTypeId,

                                     }).ToList();

                        var data = new { bookingInfo = lists.ToList(), booking = obj.bookingInfo };

                        response.Data = data;
                    }
                    else
                    {
                        response.HasError = true;
                        response.Message = "Required : BookingID";
                    }
                }
                catch (Exception ex)
                {
                    response.Message = ex.Message;
                    response.HasError = true;
                }
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetAdvanceBookingDetails")]
        public JsonResult GetAdvanceBookingDetails(WebApiClasses.RequestWebApi obj)
        {
            //
            try
            {
                //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetAdvanceBookingDetails.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
                General.WriteLog("GetAdvanceBookingDetails", "json: " + new JavaScriptSerializer().Serialize(obj));
            }
            catch
            {

            }

            ResponseWebApi response = new ResponseWebApi();

            using (TaxiDataContext db = new TaxiDataContext())
            {


                db.DeferredLoadingEnabled = false;

                long Id = obj.bookingInfo == null ? 0 : obj.bookingInfo.AdvanceBookingId.ToLong();

                try
                {
                    if (Id > 0)
                    {
                        BookingInfo objReturn = null;





                        var list = db.Bookings.Where(c => c.AdvanceBookingId == Id).ToList();

                        var objFirstBooking = list.FirstOrDefault(c => c.BookingStatusId == Enums.BOOKINGSTATUS.WAITING);


                        if (objFirstBooking == null)
                            objFirstBooking = list.FirstOrDefault(c => c.MasterJobId == null);








                        if (objFirstBooking != null)
                        {


                            foreach (var item in objFirstBooking.GetType().GetProperties())
                            {
                                try
                                {
                                    if (obj.bookingInfo.GetType().GetProperty(item.Name) != null)
                                        obj.bookingInfo.GetType().GetProperty(item.Name).SetValue(obj.bookingInfo, item.GetValue(objFirstBooking));
                                }
                                catch (Exception ex)
                                {

                                }

                            }






                            obj.bookingInfo.Booking_ViaLocations = new List<ClsBooking_ViaLocation>();




                            try
                            {
                                //
                                foreach (var item in objFirstBooking.Booking_ViaLocations)
                                {
                                    obj.bookingInfo.Booking_ViaLocations.Add(new ClsBooking_ViaLocation { Id = item.Id, BookingId = obj.bookingInfo.Id, ViaLocTypeId = Enums.LOCATION_TYPES.ADDRESS, ViaLocValue = item.ViaLocValue });
                                }
                            }
                            catch
                            {

                            }







                            var objReturnBooking = list.FirstOrDefault(c => c.BookingStatusId == Enums.BOOKINGSTATUS.WAITING && c.MasterJobId != null);


                            if (objReturnBooking == null)
                                objReturnBooking = list.FirstOrDefault(c => c.MasterJobId != null);




                            if (objReturnBooking != null)
                            {

                                objReturn = new BookingInfo();
                                foreach (var item in objReturnBooking.GetType().GetProperties())
                                {
                                    try
                                    {
                                        if (objReturn.GetType().GetProperty(item.Name) != null)
                                            objReturn.GetType().GetProperty(item.Name).SetValue(objReturn, item.GetValue(objReturnBooking));
                                    }
                                    catch (Exception ex)
                                    {

                                    }

                                }





                                objReturn.Booking_ViaLocations = new List<ClsBooking_ViaLocation>();




                                try
                                {
                                    //
                                    foreach (var item in objReturnBooking.Booking_ViaLocations.Where(c => c.BookingId == objReturn.Id))
                                    {
                                        objReturn.Booking_ViaLocations.Add(new ClsBooking_ViaLocation { Id = item.Id, BookingId = objReturn.Id, ViaLocTypeId = Enums.LOCATION_TYPES.ADDRESS, ViaLocValue = item.ViaLocValue });
                                    }
                                }
                                catch
                                {

                                }

                            }

                            List<BookingInfo> BookingsList = new List<BookingInfo>();

                            foreach (var itemData in list)
                            {
                                BookingInfo otherBooking = new BookingInfo();
                                foreach (var item in itemData.GetType().GetProperties())
                                {

                                    try
                                    {
                                        if (otherBooking.GetType().GetProperty(item.Name) != null)
                                            otherBooking.GetType().GetProperty(item.Name).SetValue(otherBooking, item.GetValue(itemData));


                                        BookingsList.Add(otherBooking);
                                    }
                                    catch (Exception ex)
                                    {

                                    }

                                }

                            }

                        }
                        var OneWayBookingList = list.Where(c => c.MasterJobId == null).OrderBy(c => c.PickupDateTime).ToList();
                        var lists = (from a in OneWayBookingList
                                     join status in db.BookingStatus
                                         on a.BookingStatusId equals status.Id
                                     select new
                                     {
                                         Id = a.Id,
                                         ReturnFareRate = a.ReturnFareRate,
                                         Booking_ViaLocations = db.Booking_ViaLocations.Where(x => x.BookingId == a.Id).ToList(), //a.Booking_ViaLocations,
                                         PickupDateTime = a.PickupDateTime,
                                         FromAddress = a.FromAddress,
                                         ToAddress = a.ToAddress,
                                         FareRate = a.FareRate,
                                         MasterJobId = a.MasterJobId,
                                         BookingStatus = status.StatusName,
                                         JourneyTypeId = a.JourneyTypeId,

                                     }).ToList();
                        var ReturnBookingList = list.Where(c => c.MasterJobId != null).OrderBy(c => c.PickupDateTime).ToList();
                        var Returnlists = (from a in ReturnBookingList
                                           join status in db.BookingStatus
                                         on a.BookingStatusId equals status.Id
                                           select new
                                           {
                                               Id = a.Id,
                                               Booking_ViaLocations = db.Booking_ViaLocations.Where(x => x.BookingId == a.Id).ToList(), //a.Booking_ViaLocations,
                                               PickupDateTime = a.PickupDateTime,
                                               FromAddress = a.FromAddress,
                                               ToAddress = a.ToAddress,
                                               FareRate = a.FareRate,
                                               MasterJobId = a.MasterJobId,
                                               BookingStatus = status.StatusName,
                                               JourneyTypeId = a.JourneyTypeId,

                                           }).ToList();

                        //      }

                        // oneway=> where bookingstatusid=1 and masterjobid is null
                        // return=> where bookingstatusid=1 and masterjobid is not null
                        var data = new { OneWayBookingInfo = obj.bookingInfo, ReturnBookingInfo = objReturn, OneWayBookingList = lists.Where(c => c.MasterJobId == null).OrderBy(c => c.PickupDateTime).ToList(), ReturnBookingList = Returnlists };




                        response.Data = data;

                    }
                    else
                    {
                        response.HasError = true;
                        response.Message = "Required : Advance BookingID";
                    }



                }
                catch (Exception ex)
                {
                    response.Message = ex.Message;
                    response.HasError = true;
                }

                //    grdLister.MasterTemplate.BeginUpdate();
                //  grdLister.DataSource = query;
            }



            return new CustomJsonResult { Data = response };







        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("CancelAdvanceBooking")]
        public JsonResult CancelAdvanceBooking(WebApiClasses.RequestWebApi obj)
        {
            //
            try
            {
                //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "CancelAdvanceBooking.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
                General.WriteLog("CancelAdvanceBooking", "json: " + new JavaScriptSerializer().Serialize(obj));
            }
            catch
            {

            }

            ResponseWebApi response = new ResponseWebApi();

            using (TaxiDataContext db = new TaxiDataContext())
            {


                db.DeferredLoadingEnabled = false;

                int Id = obj.bookingInfo == null ? 0 : obj.bookingInfo.AdvanceBookingId.ToInt();

                try
                {
                    if (Id > 0)
                    {

                        string FinalQuery = "update Booking set BookingStatusId=3 where advancebookingid=" + Id + " and bookingstatusid not in(2)";
                        db.ExecuteQuery<int>(FinalQuery);

                        db.ExecuteQuery<int>("update advancebooking set AdvBookingTypeId=4 where id=" + Id);
                        db.ExecuteQuery<int>(FinalQuery);
                    }
                    else
                    {
                        response.HasError = true;
                        response.Message = "Required : Advance BookingID";
                    }



                }
                catch (Exception ex)
                {
                    response.Message = ex.Message;
                    response.HasError = true;
                }

                //    grdLister.MasterTemplate.BeginUpdate();
                //  grdLister.DataSource = query;
            }



            return Json(response, JsonRequestBehavior.AllowGet);







        }
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("CancelBulkAdvanceBooking")]
        public JsonResult CancelBulkAdvanceBooking(WebApiClasses.RequestWebApi obj)
        {
            try
            {
                //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "CancelBulkAdvanceBooking.txt",
                //DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);

                General.WriteLog("CancelBulkAdvanceBooking", "json: " + new JavaScriptSerializer().Serialize(obj));
            }
            catch { }

            ResponseWebApi response = new ResponseWebApi();

            using (TaxiDataContext db = new TaxiDataContext())
            {
                db.DeferredLoadingEnabled = false;

                try
                {
                    if (obj.AdvanceBookingIds != null && obj.AdvanceBookingIds.Any())
                    {
                        foreach (var id in obj.AdvanceBookingIds)
                        {
                            string FinalQuery = $"UPDATE Booking SET BookingStatusId = 3 WHERE Id = {id} AND BookingStatusId NOT IN (2)";
                            db.ExecuteQuery<int>(FinalQuery);

                            // If return booking handling is enabled
                            if (obj.HasReturnBooking == true)
                            {
                                // Find return bookings linked to this booking
                                var ReturnBooking = db.Bookings.FirstOrDefault(x => x.MasterJobId == id);
                                if (ReturnBooking != null && ReturnBooking.Id > 0)
                                {
                                    string ReturnQuery = $"UPDATE Booking SET BookingStatusId = 3 WHERE Id = {ReturnBooking.Id} AND BookingStatusId NOT IN (2)";
                                    db.ExecuteQuery<int>(ReturnQuery);
                                }

                            }
                        }

                    }
                    else
                    {
                        response.HasError = true;
                        response.Message = "No booking IDs provided.";
                    }
                }
                catch (Exception ex)
                {
                    response.Message = ex.Message;
                    response.HasError = true;
                }
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }




        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetAuditTrial")]
        public JsonResult GetAuditTrial(WebApiClasses.RequestWebApi obj)
        {
            //




            ResponseWebApi response = new ResponseWebApi();
            try
            {

                //
                try
                {
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetAuditTrial.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
                    General.WriteLog("GetAuditTrial", "json: " + new JavaScriptSerializer().Serialize(obj));
                }
                catch
                {

                }


                string bookingNo = obj.bookingInfo.BookingNo.ToStr().Trim();

                clsAuditTrial cls = new clsAuditTrial();
                if (bookingNo.ToStr().Trim().Length > 0)
                {
                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        //
                        db.DeferredLoadingEnabled = false;


                        cls.bookingLog = db.Vu_BookingLogs.Where<Vu_BookingLog>(c => c.BookingNo == bookingNo).OrderByDescending(b => b.PickupDateTime).ToList();
                        cls.bookingUpdates = db.vw_BookingUpdates.Where(c => c.BookingNo == bookingNo).OrderByDescending(b => b.BookingId).ToList();



                        cls.objSubCompany = db.Gen_SubCompanies.FirstOrDefault(c => c.Id == cls.bookingLog[0].SubCompanyId);


                        cls.objSubCompany.CompanyLogo = null;
                        cls.objSubCompany.CompanyFooterLogo = null;

                        var list = db.Fleet_Driver_Documents.Where(c => c.DriverId == cls.bookingLog[0].DriverId && (c.DocumentId == Enums.DRIVER_DOCUMENTS.PCODriver || c.DocumentId == Enums.DRIVER_DOCUMENTS.PCOVehicle))
                            .Select(c => new { c.BadgeNumber, c.DocumentId }).ToList();

                        foreach (var item in list)
                        {
                            if (item.DocumentId.ToInt() == Enums.DRIVER_DOCUMENTS.PCODriver)
                            {
                                cls.phcdriver = item.BadgeNumber.ToStr();

                                if (cls.phcdriver.ToStr().Trim().Length == 0)
                                    cls.phcdriver = " ";
                            }

                            if (item.DocumentId.ToInt() == Enums.DRIVER_DOCUMENTS.PCOVehicle)
                            {
                                cls.phcVehicle = item.BadgeNumber.ToStr();


                                if (cls.phcVehicle.ToStr().Trim().Length == 0)
                                    cls.phcVehicle = " ";
                            }
                        }

                        var vehicleDAta = db.Fleet_Drivers.Where(c => c.Id == cls.bookingLog[0].DriverId).Select(args => new { args.VehicleMake, args.VehicleModel, args.VehicleNo }).FirstOrDefault();
                        if (vehicleDAta != null)
                        {

                            long jobId = 0;

                            int fleetMasterId = 0;

                            try
                            {
                                jobId = cls.bookingLog[0].Id;
                                fleetMasterId = db.Bookings.Where(c => c.Id == jobId).Select(c => c.FleetMasterId).FirstOrDefault().ToInt();

                                if (fleetMasterId > 0)
                                {
                                    cls.vehicleDetails = db.Fleet_Masters.Where(c => c.Id == fleetMasterId).Select(c => "Vehicle Details : " + c.VehicleMake + " - " + c.VehicleModel + " - " + c.VehicleNo).FirstOrDefault();


                                }
                                else
                                    cls.vehicleDetails = "Vehicle Details : " + vehicleDAta.VehicleMake.ToStr() + " - " + vehicleDAta.VehicleModel.ToStr() + " - " + vehicleDAta.VehicleNo.ToStr();
                            }
                            catch
                            {
                                cls.vehicleDetails = "Vehicle Details : " + vehicleDAta.VehicleMake.ToStr() + " - " + vehicleDAta.VehicleModel.ToStr() + " - " + vehicleDAta.VehicleNo.ToStr();

                            }
                        }
                    }

                    response.Data = cls;
                }
                else
                {

                    response.HasError = true;
                    response.Message = "Required : Booking Ref";

                }


            }
            catch (Exception ex)
            {
                try
                {
                    response.HasError = true;
                    response.Message = ex.Message;

                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetShiftJobs_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message + Environment.NewLine);
                    General.WriteLog("GetShiftJobs_exception", "json:" + new JavaScriptSerializer().Serialize(obj) + ", Exception: " + ex.Message);
                }
                catch
                {

                }
            }


            return new CustomJsonResult { Data = response };

        }

        //[System.Web.Http.HttpGet]
        //[System.Web.Http.HttpPost]
        //[System.Web.Http.Route("GetAuditTrial")]
        //public JsonResult GetAuditTrial(WebApiClasses.RequestWebApi obj)
        //{
        //    //




        //    ResponseWebApi response = new ResponseWebApi();
        //    try
        //    {

        //        //
        //        try
        //        {
        //            System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetAuditTrial.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
        //        }
        //        catch
        //        {

        //        }


        //        string bookingNo = obj.bookingInfo.BookingNo.ToStr().Trim();

        //        clsAuditTrial cls = new clsAuditTrial();
        //        if (bookingNo.ToStr().Trim().Length > 0)
        //        {
        //            using (TaxiDataContext db = new TaxiDataContext())
        //            {
        //                //
        //                db.DeferredLoadingEnabled = false;


        //                cls.bookingLog = db.Vu_BookingLogs.Where<Vu_BookingLog>(c => c.BookingNo == bookingNo).OrderByDescending(b => b.PickupDateTime).ToList();
        //                cls.bookingUpdates = db.vw_BookingUpdates.Where(c => c.BookingNo == bookingNo).OrderByDescending(b => b.BookingId).ToList();



        //                cls.objSubCompany = db.Gen_SubCompanies.FirstOrDefault(c => c.Id == cls.bookingLog[0].SubCompanyId);


        //                cls.objSubCompany.CompanyLogo = null;

        //                var list = db.Fleet_Driver_Documents.Where(c => c.DriverId == cls.bookingLog[0].DriverId && (c.DocumentId == Enums.DRIVER_DOCUMENTS.PCODriver || c.DocumentId == Enums.DRIVER_DOCUMENTS.PCOVehicle))
        //                    .Select(c => new { c.BadgeNumber, c.DocumentId }).ToList();

        //                foreach (var item in list)
        //                {
        //                    if (item.DocumentId.ToInt() == Enums.DRIVER_DOCUMENTS.PCODriver)
        //                    {
        //                        cls.phcdriver = item.BadgeNumber.ToStr();

        //                        if (cls.phcdriver.ToStr().Trim().Length == 0)
        //                            cls.phcdriver = " ";
        //                    }

        //                    if (item.DocumentId.ToInt() == Enums.DRIVER_DOCUMENTS.PCOVehicle)
        //                    {
        //                        cls.phcVehicle = item.BadgeNumber.ToStr();


        //                        if (cls.phcVehicle.ToStr().Trim().Length == 0)
        //                            cls.phcVehicle = " ";
        //                    }
        //                }

        //                var vehicleDAta = db.Fleet_Drivers.Where(c => c.Id == cls.bookingLog[0].DriverId).Select(args => new { args.VehicleMake, args.VehicleModel, args.VehicleNo }).FirstOrDefault();
        //                if (vehicleDAta != null)
        //                {

        //                    long jobId = 0;

        //                    int fleetMasterId = 0;

        //                    try
        //                    {
        //                        jobId = cls.bookingLog[0].Id;
        //                        fleetMasterId = db.Bookings.Where(c => c.Id == jobId).Select(c => c.FleetMasterId).FirstOrDefault().ToInt();

        //                        if (fleetMasterId > 0)
        //                        {
        //                            cls.vehicleDetails = db.Fleet_Masters.Where(c => c.Id == fleetMasterId).Select(c => "Vehicle Details : " + c.VehicleMake + " - " + c.VehicleModel + " - " + c.VehicleNo).FirstOrDefault();


        //                        }
        //                        else
        //                            cls.vehicleDetails = "Vehicle Details : " + vehicleDAta.VehicleMake.ToStr() + " - " + vehicleDAta.VehicleModel.ToStr() + " - " + vehicleDAta.VehicleNo.ToStr();
        //                    }
        //                    catch
        //                    {
        //                        cls.vehicleDetails = "Vehicle Details : " + vehicleDAta.VehicleMake.ToStr() + " - " + vehicleDAta.VehicleModel.ToStr() + " - " + vehicleDAta.VehicleNo.ToStr();

        //                    }
        //                }
        //            }

        //            response.Data = cls;
        //        }
        //        else
        //        {

        //            response.HasError = true;
        //            response.Message = "Required : Booking Ref";

        //        }


        //    }
        //    catch (Exception ex)
        //    {
        //        try
        //        {
        //            response.HasError = true;
        //            response.Message = ex.Message;

        //            System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetShiftJobs_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message + Environment.NewLine);
        //        }
        //        catch
        //        {

        //        }
        //    }


        //    return new CustomJsonResult { Data = response };

        //}


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetDriverToolTip")]
        public JsonResult GetDriverToolTip(WebApiClasses.RequestWebApi objX)
        {
            //




            ResponseWebApi response = new ResponseWebApi();
            try
            {

                //
                try
                {
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetDriverToolTip.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(objX) + Environment.NewLine);
                    General.WriteLog("GetDriverToolTip", "json: " + new JavaScriptSerializer().Serialize(objX));
                }
                catch
                {

                }


                int driverId = objX.bookingInfo.DriverId.ToInt();

                clsAuditTrial cls = new clsAuditTrial();

                if (driverId > 0)
                {

                    DateTime? loginDateTime = DateTime.Now;
                    string plot = string.Empty;

                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        loginDateTime = db.Fleet_DriverQueueLists
                        .Where(x => x.DriverId == driverId)
                        .OrderByDescending(x => x.LoginDateTime)
                        .Select(x => x.LoginDateTime)
                        .FirstOrDefault();
                        var obj = db.stp_GetDriverToolTipData(driverId, 0, loginDateTime).FirstOrDefault();


                        string vehicleNo = obj.CompanyVehicle.ToStr();



                        if ((obj.CurrentJobId != null && (obj.DriverWorkStatusId.ToInt() != Enums.Driver_WORKINGSTATUS.AVAILABLE && obj.DriverWorkStatusId.ToInt() != Enums.Driver_WORKINGSTATUS.ONBREAK)))
                        {


                            loginDateTime = obj.LoginDateTime;



                            DateTime newLoginDateTime = loginDateTime.ToDateTime();

                            if (HubProcessor.Instance.objPolicy.EarningLoginHours.ToInt() > 0)
                            {

                                newLoginDateTime = loginDateTime.Value.AddMinutes(-HubProcessor.Instance.objPolicy.EarningLoginHours.ToInt());

                                //   loginDateTime = General.GetObject<Fleet_DriverQueueList>(c => c.LogoutDateTime.Value > newLoginDateTime).DefaultIfEmpty().LoginDateTime;
                            }


                            if (loginDateTime == null)
                                loginDateTime = obj.LoginDateTime;



                            string waitSince = string.Empty;

                            if (obj.DriverWorkStatusId.ToInt() == Enums.Driver_WORKINGSTATUS.AVAILABLE && obj.WaitSinceOn != null)
                            {
                                try
                                {
                                    waitSince = GetMinsInWords(DateTime.Now.Subtract(obj.WaitSinceOn.Value).TotalMinutes);


                                    waitSince = Environment.NewLine + "Wait Since : " + waitSince;
                                }
                                catch
                                {


                                }

                            }






                            int totalMins = DateTime.Now.Subtract(loginDateTime.Value).TotalMinutes.ToInt();
                            decimal AvgEarning = 0.00m;


                            decimal totalEarning = obj.TotalEarning.ToDecimal();
                            string earn = string.Format("{0:c}", totalEarning).Substring(1);
                            string lastGPSContact = string.Empty;


                            if (obj.DriverId != null)
                            {
                                lastGPSContact = Environment.NewLine + "Last GPS Contact Time : " + string.Format("{0:dd-MMM HH:mm}", obj.LastGpsContact.ToDateTime());




                            }





                            if (totalMins > 0)
                            {
                                totalMins = Math.Ceiling((totalMins.ToDecimal() * 60) / 3600).ToInt();
                                AvgEarning = Math.Round((totalEarning / totalMins), 2);



                            }


                            string journeyType = "";


                            if (obj.JourneyTypeId.ToInt() == Enums.JOURNEY_TYPES.ONEWAY)
                                journeyType = "(One Way Journey)";
                            else if (obj.JourneyTypeId.ToInt() == Enums.JOURNEY_TYPES.RETURN)
                                journeyType = "(Return Journey)";
                            else if (obj.JourneyTypeId.ToInt() == Enums.JOURNEY_TYPES.WAITANDRETURN)
                                journeyType = "(Wait and Return Journey)";



                            lastGPSContact = " , Last GPS Contact : " + string.Format("{0:dd-MMM HH:mm}", obj.LastGpsContact.ToDateTime());



                            response.Data = "Driver : " + obj.DriverNo.ToStr() + " - " + obj.DriverName.ToStr()
                                  + Environment.NewLine



                                                 + "Jobs Done : " + obj.TotalJobs.ToInt()
                                             + Environment.NewLine + "Total Earned : £ " + earn
                                              + Environment.NewLine + "Avg earning per hour  : £ " + AvgEarning



                                           + Environment.NewLine + "Status : " + obj.Status.ToStr() + lastGPSContact +
                                            (vehicleNo != string.Empty ? Environment.NewLine + "Company Vehicle : " + vehicleNo : "")

                                          + (obj.CurrentJobId != null && obj.DriverWorkStatusId.ToInt() != Enums.Driver_WORKINGSTATUS.AVAILABLE ? Environment.NewLine + Environment.NewLine + "On Job:" + journeyType + Environment.NewLine + " Pickup : "
                                                              + obj.FromAddress + Environment.NewLine + " Destination : " + obj.ToAddress : "")
                                          + (obj.CurrentJobId != null && obj.DriverWorkStatusId.ToInt() != Enums.Driver_WORKINGSTATUS.AVAILABLE && obj.DropOffZoneId != null ? Environment.NewLine + "DropOff Plot : " + obj.DropOffZoneName : "")
                                            + (obj.CurrentJobId != null && obj.DriverWorkStatusId.ToInt() != Enums.Driver_WORKINGSTATUS.AVAILABLE ? Environment.NewLine + " Pickup Date/Time : " + string.Format("{0:dd/MM/yyyy HH:mm}", obj.PickupDateTime) : "")

                                          + Environment.NewLine + "Vehicle : " + obj.VehicleType.ToStr() + " - " + obj.VehicleNo.ToStr() + " - " + obj.VehicleColor.ToStr() + " - " + obj.VehicleMake.ToStr();





                        }
                        else
                        {


                            loginDateTime = obj.LoginDateTime;



                            DateTime newLoginDateTime = loginDateTime.ToDateTime();

                            if (HubProcessor.Instance.objPolicy.EarningLoginHours.ToInt() > 0)
                            {

                                newLoginDateTime = loginDateTime.Value.AddMinutes(-HubProcessor.Instance.objPolicy.EarningLoginHours.ToInt());


                            }


                            if (loginDateTime == null)
                                loginDateTime = obj.LoginDateTime;



                            string waitSince = string.Empty;

                            if (obj.DriverWorkStatusId.ToInt() == Enums.Driver_WORKINGSTATUS.AVAILABLE && obj.WaitSinceOn != null)
                            {
                                try
                                {
                                    waitSince = GetMinsInWords(DateTime.Now.Subtract(obj.WaitSinceOn.Value).TotalMinutes);


                                    waitSince = Environment.NewLine + "Wait Since : " + waitSince;
                                }
                                catch
                                {


                                }

                            }






                            int totalMins = DateTime.Now.Subtract(loginDateTime.Value).TotalMinutes.ToInt();
                            decimal AvgEarning = 0.00m;


                            decimal totalEarning = obj.TotalEarning.ToDecimal();
                            string earn = string.Format("{0:c}", totalEarning).Substring(1);
                            string lastGPSContact = string.Empty;
                            string sinbinTill = string.Empty;

                            if (obj.DriverId != null)
                            {
                                lastGPSContact = Environment.NewLine + "Last GPS Contact Time : " + string.Format("{0:dd-MMM HH:mm}", obj.LastGpsContact.ToDateTime());


                                if (obj.DriverWorkStatusId.ToInt() == Enums.Driver_WORKINGSTATUS.SINBIN)
                                {
                                    sinbinTill = " - till : " + string.Format("{0:HH:mm:ss}", obj.SinBinTillOn.ToDateTime());
                                }

                            }





                            if (totalMins > 0)
                            {
                                totalMins = Math.Ceiling((totalMins.ToDecimal() * 60) / 3600).ToInt();
                                AvgEarning = Math.Round((totalEarning / totalMins), 2);



                            }

                            response.Data = "Driver : " + obj.DriverNo.ToStr() + " - " + obj.DriverName.ToStr() + Environment.NewLine
                                             + plot


                                                + "Jobs Done : " + obj.TotalJobs.ToInt()
                                            + Environment.NewLine + "Total Earned : £ " + earn
                                             + Environment.NewLine + "Avg earning per hour  : £ " + AvgEarning
                                            + Environment.NewLine + "Status : " + obj.Status.ToStr() + sinbinTill
                                            + lastGPSContact
                                            + (vehicleNo != string.Empty ? Environment.NewLine + "Company Vehicle : " + vehicleNo : "")
                                            + Environment.NewLine + "Login Since : " + loginDateTime + " (" + obj.LoginFrom.ToStr() + ")"

                                            + Environment.NewLine + "Vehicle : " + obj.VehicleType.ToStr() + " - " + obj.VehicleNo.ToStr() + " - " + obj.VehicleColor.ToStr() + " - " + obj.VehicleMake.ToStr()

                                            + waitSince;
                        }

                    }
                }
                else
                {

                    response.HasError = true;
                    response.Message = "Required : DriverID";

                }


            }
            catch (Exception ex)
            {
                try
                {
                    response.HasError = true;
                    response.Message = ex.Message;

                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetDriverToolTip_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(objX) + ",exception:" + ex.Message + Environment.NewLine);
                    General.WriteLog("GetDriverToolTip_exception", "json:" + new JavaScriptSerializer().Serialize(objX) + ", Exception: " + ex.Message);
                }
                catch
                {

                }
            }


            return new CustomJsonResult { Data = response };

        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetDriverDetails")]
        public JsonResult GetDriverDetails(WebApiClasses.RequestWebApi obj)
        {
            //

            try
            {


                System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetDriverDetails.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
            }
            catch
            {

            }
            ResponseWebApi response = new ResponseWebApi();

            try
            {
                using (TaxiDataContext db = new TaxiDataContext())
                {


                    var obj2 = db.Fleet_Drivers.FirstOrDefault(c => c.Id == obj.bookingInfo.DriverId);





                    string PDAVersion = string.Empty;
                    if (obj2.HasPDA.ToBool())
                    {

                        PDAVersion = obj2.Fleet_Driver_PDASettings.Count > 0 ? obj2.Fleet_Driver_PDASettings.FirstOrDefault().CurrentPdaVersion.ToStr() : null;
                        string LastUpdated = obj2.Fleet_Driver_PDASettings.Count > 0 ? string.Format("{0:dd/MM/yyyy HH:mm}", obj2.Fleet_Driver_PDASettings.FirstOrDefault().LastVersionUpdatedOn) : null;
                        PDAVersion = PDAVersion + " ( Updated On : " + LastUpdated + " )";
                    }



                    string status = string.Empty;
                    if (db.Fleet_DriverQueueLists.Where(c => c.DriverId == obj2.Id && c.Status == true).Count() > 0)
                    {
                        status = "ONLINE";
                        //txtStatus.ForeColor = Color.Green;

                    }
                    else
                    {
                        status = "OFFLINE";
                        //txtStatus.ForeColor = Color.Gray;

                    }

                    var data = new
                    {
                        obj2.DriverNo,
                        obj2.Surname,
                        obj2.DriverName,
                        obj2.MobileNo,
                        obj2.PDARent
                    ,
                        Vehicle = obj2.Fleet_VehicleType.DefaultIfEmpty().VehicleType.ToStr() + " - " + obj2.VehicleColor.ToStr() + " - " + obj2.VehicleNo.ToStr()
                    ,
                        MakeModel = obj2.VehicleMake.ToStr().Trim() + "/" + obj2.VehicleModel.ToStr().Trim(),
                        RentPaid = obj2.PDALoginBlocked.ToBool(),
                        InitialBalance = obj2.InitialBalance,
                        PDAVersion = PDAVersion,
                        Status = status
                    };
                    response.Data = data;




                }
            }
            catch (Exception ex)
            {
                try
                {
                    response.HasError = true;
                    response.Message = ex.Message;

                    System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetDriverDetails_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
            }


            //   return Json(response, JsonRequestBehavior.AllowGet);
            return new CustomJsonResult { Data = response };
        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetBookingConfirmationDetails")]
        public JsonResult GetBookingConfirmationDetails(WebApiClasses.RequestWebApi obj)
        {
            //

            try
            {
                General.WriteLog("GetBookingConfirmationDetails", "json: " + new JavaScriptSerializer().Serialize(obj));
                //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetBookingConfirmationDetails.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
            }
            catch
            {

            }
            ResponseWebApi response = new ResponseWebApi();

            try
            {
                using (TaxiDataContext db = new TaxiDataContext())
                {


                    var obj2 = db.Bookings.FirstOrDefault(c => c.Id == obj.bookingInfo.Id);


                    if (obj2 != null)
                    {
                        string subject = string.Empty;
                        if (obj2.IsQuotation.ToBool())
                        {

                            //if (BookingActionType.ToStr().Trim().ToUpper() == "AMENDMENT")
                            //{
                            //    subject = "AMENDMENT OF QUOTATION -" + objBook.BookingNo.ToStr() + " - " + string.Format("{0:dd MMMM yyyy}", objBook.PickupDateTime)
                            //     + ", TIME " + string.Format("{0:HH.mm}", objBook.PickupDateTime);
                            //}


                            //else
                            //{
                            subject = "BOOKING QUOTATION - " + string.Format("{0:dd MMMM yyyy}", obj2.PickupDateTime)
                                 + ", TIME " + string.Format("{0:HH.mm}", obj2.PickupDateTime) + " - BOOKING ID "
                                 + obj2.BookingNo.ToStr();
                            //   }

                        }
                        else
                        {

                            //if (BookingActionType.ToStr().Trim().ToUpper() == "AMENDMENT")
                            //{
                            //    subject = "AMENDMENT OF BOOKING REFERENCE:" + objBook.BookingNo.ToStr() + " - " + string.Format("{0:dd MMMM yyyy}", objBook.PickupDateTime)
                            //     + ", TIME " + string.Format("{0:HH.mm}", objBook.PickupDateTime);
                            //}


                            //else
                            //{
                            subject = "BOOKING CONFIRMATION -  " + string.Format("{0:dd MMMM yyyy}", obj2.PickupDateTime)
                                 + ", TIME " + string.Format("{0:HH.mm}", obj2.PickupDateTime) + " - BOOKING ID "
                                 + obj2.BookingNo.ToStr();
                            //   }

                        }


                        var data = new EmailInfo { SubCompanyId = obj2.SubcompanyId.ToInt(), From = db.Gen_SubCompanies.Where(c => c.Id == obj2.SubcompanyId).Select(c => c.SmtpUserName).FirstOrDefault(), Subject = subject, BookingId = obj2.Id, To = obj2.CustomerEmail, IsAccountJob = obj2.CompanyId != null ? true : false, PaymentTypeId = obj2.PaymentTypeId.ToInt() };
                        try
                        {
                            var EnableThirdPartyEmailSetting = false;
                            if (db.ExecuteQuery<string>("select SetVal from AppSettings where setkey= 'EnableThirdPartyEmailSetting'").FirstOrDefault().ToStr() == "true")
                            {
                                EnableThirdPartyEmailSetting = true;
                            }
                            var subCompany = db.ExecuteQuery<Gen_SubcompanyFields>($"select Id,EmailAddress,SmtpEmailAddress,SmtpInvoiceEmailAddress,SmtpDriverEmailAddress,CAST(ISNULL(UseDifferentEmailForInvoices,0) AS BIT) UseDifferentEmailForInvoices,SmtpInvoiceUserName from Gen_SubCompany WHERE Id={obj2.SubcompanyId}").FirstOrDefault();
                            if (subCompany != null && EnableThirdPartyEmailSetting)
                            {
                                data.From = (EnableThirdPartyEmailSetting ? subCompany.SmtpEmailAddress : subCompany.EmailAddress);
                            }
                        }
                        catch
                        {
                        }


                        response.Data = data;

                    }


                }
            }
            catch (Exception ex)
            {
                try
                {
                    response.HasError = true;
                    response.Message = ex.Message;

                    General.WriteLog("GetBookingConfirmationDetails_exception", "json:" + new JavaScriptSerializer().Serialize(obj) +  ", Exception: " + ex.Message);
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetBookingConfirmationDetails_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
            }


            //   return Json(response, JsonRequestBehavior.AllowGet);
            return new CustomJsonResult { Data = response };
        }
        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetBookingDetailsForSMS")]
        public JsonResult GetBookingDetailsForSMS(WebApiClasses.RequestWebApi obj)
        {
            //

            try
            {
                General.WriteLog("GetBookingDetailsForSMS", "json: " + new JavaScriptSerializer().Serialize(obj));
                //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetBookingDetailsForSMS.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
            }
            catch
            {

            }
            ResponseWebApi response = new ResponseWebApi();

            try
            {
                using (TaxiDataContext db = new TaxiDataContext())
                {


                    var obj2 = db.Bookings.FirstOrDefault(c => c.Id == obj.bookingInfo.Id);


                    if (obj2 != null)
                    {





                        var data = new EmailInfo { SubCompanyId = obj2.SubcompanyId.ToInt(), From = db.Gen_SubCompanies.Where(c => c.Id == obj2.SubcompanyId).Select(c => c.SmtpUserName).FirstOrDefault(), BookingId = obj2.Id, CustomerName = obj2.CustomerName, To = obj2.CustomerMobileNo, IsAccountJob = obj2.CompanyId != null ? true : false, PaymentTypeId = obj2.PaymentTypeId.ToInt() };


                        response.Data = data;

                    }


                }
            }
            catch (Exception ex)
            {
                try
                {
                    response.HasError = true;
                    response.Message = ex.Message;

                    General.WriteLog("GetBookingConfirmationDetails_exception", "json:" + new JavaScriptSerializer().Serialize(obj) + ", Exception: " + ex.Message);
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetBookingConfirmationDetails_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
            }


            //   return Json(response, JsonRequestBehavior.AllowGet);
            return new CustomJsonResult { Data = response };
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("SendConfirmationEmail")]
        public JsonResult SendConfirmationEmail(WebApiClasses.RequestWebApi obj)
        {
            //
            string emailTo = string.Empty;
            decimal price = 0.00m;
            decimal returnPrice = 0.00m;
            try
            {

                General.WriteLog("SendConfirmationEmail", "json: " + new JavaScriptSerializer().Serialize(obj));
                //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "SendConfirmationEmail.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
            }
            catch
            {

            }
            ResponseWebApi response = new ResponseWebApi();

            try
            {
                using (TaxiDataContext db = new TaxiDataContext())
                {


                    var obj2 = db.Bookings.FirstOrDefault(c => c.Id == obj.emailInfo.BookingId);

                    obj.emailInfo.toEmailType = 0;
                    if (obj2 != null)
                    {
                        var objSubCompany = db.Gen_SubCompanies.Where(c => c.Id == obj2.SubcompanyId).FirstOrDefault();
                        //var objSubCompany = db.Gen_SubCompanies.Where(c => c.Id == obj2.SubcompanyId).Select(args => new { args.SmtpHost, args.SmtpUserName, args.SmtpPassword, args.SmtpPort, args.SmtpHasSSL, args.EmailCC }).FirstOrDefault();
                        if (obj.emailInfo.toEmailType == 0)
                        {
                            emailTo = obj2.CustomerName.ToStr();
                            price = obj2.FareRate.ToDecimal() + obj2.MeetAndGreetCharges.ToDecimal() + obj2.CongtionCharges.ToDecimal() + obj2.ExtraDropCharges.ToDecimal() + obj2.ServiceCharges.ToDecimal();
                            returnPrice = obj2.BookingReturns.Count > 0 ? obj2.BookingReturns[0].FareRate.ToDecimal() + obj2.BookingReturns[0].CongtionCharges.ToDecimal() + obj2.BookingReturns[0].ExtraDropCharges.ToDecimal() + obj2.BookingReturns[0].MeetAndGreetCharges.ToDecimal() + obj2.BookingReturns[0].ServiceCharges.ToDecimal() : 0.00m;

                        }
                        StringBuilder StrBld = new StringBuilder();


                        StrBld.Append("<table width='100%' border='0' cellspacing='0' cellpadding='0' style='border: #d4e0ee 1px solid; background-color:White;font-family: verdana, arial;font-size: 11px;font-weight: normal;color: #000;text-decoration: none;'>");
                        //  StrBld.Append("<tr><td style='text-align: left; padding: 10px 20px 10px 20px; font-size: 16px;font-weight: bold; color: #ef0000; border-bottom: #d4e0ee 1px solid; border-right: #d4e0ee 1px solid;'>Booking Confirmation</td>");

                        string fullCompanyName = obj2.Gen_SubCompany.CompanyName;
                        StrBld.Append("Dear: " + emailTo + "<td width='20%' style='padding: 10px 5px 10px 5px; font-size: 16px; font-weight: bold;border-right: #d4e0ee 1px solid;'>REF NO:</td><td width='30%' style='padding: 10px 5px 10px 5px; font-size: 16px; font-weight: bold;border-right: #d4e0ee 1px solid; color: #008000;'>" + obj2.BookingNo.ToStr() + "</td>");

                        StrBld.Append("</tr>");
                        StrBld.Append("<tr style='background-color: #eff3f9;'><td colspan='2' style='padding: 5px; text-decoration: underline;border-top: #d4e0ee 1px solid; border-bottom: #d4e0ee 1px solid; border-right: #d4e0ee 1px solid; font-size: 12px;'>");
                        StrBld.Append("Traveller Information</td><td colspan='2' style='padding: 5px; text-decoration: underline;border-top: #d4e0ee 1px solid; border-bottom: #d4e0ee 1px solid; font-size: 12px;'>Carrier Details</td></tr><tr><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid; width: 15%'>");
                        StrBld.Append("Passenger:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid; width: 25%'>" + obj2.CustomerName.ToStr() + "</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid; width: 15%'>");
                        StrBld.Append("Passenger No:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;width: 45%'>" + obj2.NoofPassengers.ToInt() + "</td></tr><tr><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>");
                        StrBld.Append("Mobile:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>" + obj2.CustomerMobileNo.ToStr() + "</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>");
                        StrBld.Append("   </td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;'>" + " " + "</td></tr><tr><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>");
                        StrBld.Append("Phone:</td><td style='padding: 5px; bold; border-bottom: #d4e0ee 1px solid;border-bottom: #d4e0ee 1px solid; border-right: #d4e0ee 1px solid;'>" + obj2.CustomerPhoneNo.ToStr() + "</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>");
                        StrBld.Append("Check-in Luggage:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;'>" + obj2.NoofLuggages.ToInt() + "</td></tr><tr><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>");
                        StrBld.Append("Email:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>" + obj2.CustomerEmail.ToStr() + "</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>");
                        StrBld.Append("Vehicle:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;'>" + obj2.Fleet_VehicleType.VehicleType.ToStr() + "</td></tr>");
                        StrBld.Append("<tr><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>Pickup Date/Time:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>" + string.Format("{0:dd-MMM-yyyy HH:mm}", obj2.PickupDateTime) + "</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>");
                        StrBld.Append("Special Ins:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;'>" + obj2.SpecialRequirements.ToStr() + "</td></tr>");
                        StrBld.Append("<tr><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>Account:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>" + obj2.Gen_Company.DefaultIfEmpty().CompanyName.ToStr() + "</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>");
                        StrBld.Append("Order No</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;'>" + obj2.OrderNo.ToStr() + "</td></tr><tr><td colspan='4' style='border-bottom: #d4e0ee 1px solid;'>&nbsp;</td></tr><tr valign='top'><td colspan='2'><table width='100%' border='0' cellspacing='0' cellpadding='0' style='border: #d4e0ee 1px solid;background-color: White; font-family: verdana, arial; font-size: 11px; font-weight: normal;color: #000; text-decoration: none;'><tr style='background-color: #eff3f9;'><td style='padding: 5px; text-decoration: underline;border-bottom: #d4e0ee 1px solid; border-right: #d4e0ee 1px solid; font-size: 12px;' colspan='2'>");
                        StrBld.Append("Pick-up Information</td></tr><tr><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>From:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>" + obj2.FromAddress + "</td></tr>");
                        if (obj2.FromLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT)
                        {
                            StrBld.Append("<tr><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>Flight Number:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>" + obj2.FromDoorNo.ToStr() + "</td></tr>");
                            StrBld.Append("<tr><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>Coming From:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>" + obj2.FromStreet.ToStr() + "</td></tr>");

                        }
                        else if (obj2.FromLocTypeId.ToInt() == Enums.LOCATION_TYPES.POSTCODE)
                        {
                            StrBld.Append("<tr><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>From Door #:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>" + obj2.FromDoorNo.ToStr() + "</td></tr>");
                            StrBld.Append("<tr><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>From Street:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>" + obj2.FromStreet.ToStr() + "</td></tr>");
                        }
                        else
                        {
                            StrBld.Append("<tr><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>Door #:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>" + obj2.FromDoorNo.ToStr() + "</td></tr>");
                        }
                        StrBld.Append("</table></td><td colspan='2'><table width='100%' border='0' cellspacing='0' cellpadding='0' style='border: #d4e0ee 1px solid;background-color: White; font-family: verdana, arial; font-size: 11px; font-weight: normal;color: #000; text-decoration: none;'><tr style='background-color: #eff3f9;'><td style='padding: 5px; text-decoration: underline;border-bottom: #d4e0ee 1px solid; font-size: 12px;' colspan='2'>");
                        StrBld.Append("Drop-off Information</td></tr>");


                        StrBld.Append("<tr><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>To:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;'>" + obj2.ToAddress + "</td></tr><tr><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>");

                        if (obj2.ToLocTypeId.ToInt() == Enums.LOCATION_TYPES.POSTCODE)
                        {

                            StrBld.Append("To Door No:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;'>" + obj2.ToDoorNo + "</td></tr><tr><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>");
                            StrBld.Append("To Street:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;'>" + obj2.ToStreet.ToStr() + "</td></tr>");
                        }
                        else
                        {
                            string toDoorNo = obj2.ToDoorNo.ToStr().Trim();

                            if (obj2.ToLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT && obj2.JourneyTypeId.ToInt() == Enums.JOURNEY_TYPES.RETURN)
                                toDoorNo = string.Empty;
                            else if (toDoorNo.Length > 0)
                                toDoorNo = toDoorNo + "-";

                            StrBld.Append("To Door No:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;'>" + toDoorNo + "</td></tr>");


                            //  StrBld.Append("To Door No:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;'>" + objBooking.ToDoorNo + "</td></tr>");


                        }

                        StrBld.Append("</table></td></tr>");


                        if (obj2.Booking_ViaLocations.Count > 0)
                        {

                            StrBld.Append("<tr><td colspan='4'><table width='100%' border='0' cellspacing='0' cellpadding='0' style='border: #d4e0ee 1px solid;background-color: White; font-family: verdana, arial; font-size: 11px; font-weight: normal;color: #000; text-decoration: none;'><tr style='background-color: #eff3f9;'><td style='padding: 5px; text-decoration: underline; border-bottom: #d4e0ee 1px solid;font-size: 12px; width: 50%' align='center'>From</td><td style='padding: 5px; text-decoration: underline; border-bottom: #d4e0ee 1px solid;font-size: 12px; width: 50%' align='center'>To</td></tr>");

                            int cnt = obj2.Booking_ViaLocations.Count;


                            for (int i = 0; i < cnt; i++)
                            {
                                if (i == 0)
                                {
                                    StrBld.Append("<tr>");
                                    StrBld.Append("<td style='padding: 5px; border-bottom: #d4e0ee 1px solid; border-right: #d4e0ee 1px solid;'>" + obj2.FromAddress.ToStr() + "</td>");

                                    StrBld.Append("<td style='padding: 5px; border-bottom: #d4e0ee 1px solid;'>" + obj2.Booking_ViaLocations[i].ViaLocValue.ToStr() + "</td>");
                                    StrBld.Append("</tr>");

                                }
                                else
                                {
                                    if (i < cnt)
                                    {

                                        StrBld.Append("<tr>");
                                        StrBld.Append("<td style='padding: 5px; border-bottom: #d4e0ee 1px solid; border-right: #d4e0ee 1px solid;'>" + obj2.Booking_ViaLocations[i - 1].ViaLocValue.ToStr() + "</td>");


                                        StrBld.Append("<td style='padding: 5px; border-bottom: #d4e0ee 1px solid;'>" + obj2.Booking_ViaLocations[i].ViaLocValue.ToStr() + "</td>");
                                        StrBld.Append("</tr>");
                                    }
                                }


                                if (i + 1 == cnt)
                                {
                                    StrBld.Append("<tr>");
                                    StrBld.Append("<td style='padding: 5px; border-bottom: #d4e0ee 1px solid; border-right: #d4e0ee 1px solid;'>" + obj2.Booking_ViaLocations[i].ViaLocValue.ToStr() + "</td>");

                                    StrBld.Append("<td style='padding: 5px; border-bottom: #d4e0ee 1px solid;'>" + obj2.ToAddress.ToStr() + "</td>");
                                    StrBld.Append("</tr>");

                                }


                            }





                            StrBld.Append("</table></td></tr>");
                        }


                        StrBld.Append("<tr><td colspan='4'>&nbsp;</td></tr>");
                        StrBld.Append("<tr><td colspan='4' style='border-bottom: #d4e0ee 1px solid;'>&nbsp;</td></tr><tr><td colspan='4' style='padding: 10px 5px 10px 5px; font-size: 18px; border-bottom: #d4e0ee 1px solid;background-color: #eff3f9;'>");


                        //if (objBooking.IsQuotation.ToBool() || (chkHideFares == null && hideFares == false) || (chkHideFares != null && chkHideFares.Checked == false))
                        if (obj2.IsQuotation.ToBool())
                        {
                            StrBld.Append("GBP Cost: <span style='color: #008000;'>£ " + string.Format("{0:f2}", (price)) + "</span>");
                        }
                        StrBld.Append(" <span style='color: #008000;'>" + obj2.Gen_PaymentType.DefaultIfEmpty().PaymentType.ToStr() + "</span></td></tr>");

                        if (obj2.BookingReturns.Count > 0)
                        {

                            Booking objReturns = obj2.BookingReturns[0];

                            StrBld.Append("<tr style='background-color: #eff3f9;'><td colspan='2' style='padding: 5px; text-decoration: underline;border-top: #d4e0ee 1px solid; border-bottom: #d4e0ee 1px solid; border-right: #d4e0ee 1px solid; font-size: 12px;'>");

                            StrBld.Append("Return Booking Details</td><td colspan='2' style='padding: 5px; text-decoration: underline;border-top: #d4e0ee 1px solid; border-bottom: #d4e0ee 1px solid; font-size: 12px;'>REF NO:" + objReturns.BookingNo.ToStr() + "</td></tr><tr><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid; width: 15%'>");
                            StrBld.Append("Passenger:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid; width: 25%'>" + objReturns.CustomerName.ToStr() + "</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid; width: 15%'>");
                            StrBld.Append("Passenger No:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;width: 45%'>" + objReturns.NoofPassengers.ToInt() + "</td></tr><tr><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>");
                            StrBld.Append("Mobile:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>" + objReturns.CustomerMobileNo.ToStr() + "</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>");
                            StrBld.Append("   </td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;'>" + " " + "</td></tr><tr><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>");


                            StrBld.Append("Phone:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>" + objReturns.CustomerPhoneNo.ToStr() + "</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>");
                            // StrBld.Append("Phone:</td><td style='padding: 5px; bold; border-bottom: #d4e0ee 1px solid;border-bottom: #d4e0ee 1px solid; border-right: #d4e0ee 1px solid;'>" + objReturns.CustomerPhoneNo.ToStr() + "</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>");
                            StrBld.Append("Check-in Luggage:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;'>" + objReturns.NoofLuggages.ToInt() + "</td></tr><tr><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>");
                            StrBld.Append("Email:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>" + objReturns.CustomerEmail.ToStr() + "</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>");
                            StrBld.Append("Vehicle:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;'>" + obj2.Fleet_VehicleType.VehicleType.ToStr() + "</td></tr>");
                            StrBld.Append("<tr><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>Pickup Date/Time:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>" + string.Format("{0:dd-MMM-yyyy HH:mm}", objReturns.PickupDateTime) + "</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>");

                            StrBld.Append("Special Ins:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;'>" + objReturns.SpecialRequirements.ToStr() + "</td></tr>");

                            //  StrBld.Append("<tr><td colspan='4' style='border-bottom: #d4e0ee 1px solid;'>&nbsp;</td></tr><tr valign='top'><td colspan='2'><table width='100%' border='0' cellspacing='0' cellpadding='0' style='border: #d4e0ee 1px solid;background-color: White; font-family: verdana, arial; font-size: 11px; font-weight: normal;color: #000; text-decoration: none;'><tr style='background-color: #eff3f9;'><td style='padding: 5px; text-decoration: underline;border-bottom: #d4e0ee 1px solid; border-right: #d4e0ee 1px solid; font-size: 12px;' colspan='2'>");



                            StrBld.Append("<tr><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>Account:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>" + objReturns.Gen_Company.DefaultIfEmpty().CompanyName.ToStr() + "</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>");

                            StrBld.Append("Order No</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;'>" + objReturns.OrderNo.ToStr() + "</td></tr><tr><td colspan='4' style='border-bottom: #d4e0ee 1px solid;'>&nbsp;</td></tr><tr valign='top'><td colspan='2'><table width='100%' border='0' cellspacing='0' cellpadding='0' style='border: #d4e0ee 1px solid;background-color: White; font-family: verdana, arial; font-size: 11px; font-weight: normal;color: #000; text-decoration: none;'><tr style='background-color: #eff3f9;'><td style='padding: 5px; text-decoration: underline;border-bottom: #d4e0ee 1px solid; border-right: #d4e0ee 1px solid; font-size: 12px;' colspan='2'>");



                            StrBld.Append("Pick-up Information</td></tr><tr><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>From:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>" + objReturns.FromAddress + "</td></tr>");

                            if (objReturns.FromLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT)
                            {
                                StrBld.Append("<tr><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>Flight Number:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>" + objReturns.FromDoorNo.ToStr() + "</td></tr>");
                                StrBld.Append("<tr><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>Coming From:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>" + objReturns.FromStreet.ToStr() + "</td></tr>");

                            }
                            else if (objReturns.FromLocTypeId.ToInt() == Enums.LOCATION_TYPES.POSTCODE)
                            {
                                StrBld.Append("<tr><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>From Door #:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>" + objReturns.FromDoorNo.ToStr() + "</td></tr>");
                                StrBld.Append("<tr><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>From Street:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>" + objReturns.FromStreet.ToStr() + "</td></tr>");


                            }
                            else
                            {


                                StrBld.Append("<tr><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>Door #:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>" + objReturns.FromDoorNo.ToStr() + "</td></tr>");


                            }
                            StrBld.Append("</table></td><td colspan='2'><table width='100%' border='0' cellspacing='0' cellpadding='0' style='border: #d4e0ee 1px solid;background-color: White; font-family: verdana, arial; font-size: 11px; font-weight: normal;color: #000; text-decoration: none;'><tr style='background-color: #eff3f9;'><td style='padding: 5px; text-decoration: underline;border-bottom: #d4e0ee 1px solid; font-size: 12px;' colspan='2'>");
                            StrBld.Append("Drop-off Information</td></tr>");


                            StrBld.Append("<tr><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>To:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;'>" + objReturns.ToAddress + "</td></tr><tr><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>");

                            if (objReturns.ToLocTypeId.ToInt() == Enums.LOCATION_TYPES.POSTCODE)
                            {

                                StrBld.Append("To Door No:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;'>" + objReturns.ToDoorNo + "</td></tr><tr><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;border-right: #d4e0ee 1px solid;'>");
                                StrBld.Append("To Street:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;'>" + objReturns.ToStreet.ToStr() + "</td></tr>");
                            }
                            else
                            {

                                StrBld.Append("To Door No:</td><td style='padding: 5px; border-bottom: #d4e0ee 1px solid;'>" + objReturns.ToDoorNo + "</td></tr>");


                            }

                            StrBld.Append("</table></td></tr>");


                            if (objReturns.Booking_ViaLocations.Count > 0)
                            {

                                StrBld.Append("<tr><td colspan='4'><table width='100%' border='0' cellspacing='0' cellpadding='0' style='border: #d4e0ee 1px solid;background-color: White; font-family: verdana, arial; font-size: 11px; font-weight: normal;color: #000; text-decoration: none;'><tr style='background-color: #eff3f9;'><td style='padding: 5px; text-decoration: underline; border-bottom: #d4e0ee 1px solid;font-size: 12px; width: 50%' align='center'>From</td><td style='padding: 5px; text-decoration: underline; border-bottom: #d4e0ee 1px solid;font-size: 12px; width: 50%' align='center'>To</td></tr>");

                                int cnt = objReturns.Booking_ViaLocations.Count;


                                for (int i = 0; i < cnt; i++)
                                {
                                    if (i == 0)
                                    {
                                        StrBld.Append("<tr>");
                                        StrBld.Append("<td style='padding: 5px; border-bottom: #d4e0ee 1px solid; border-right: #d4e0ee 1px solid;'>" + objReturns.FromAddress.ToStr() + "</td>");

                                        StrBld.Append("<td style='padding: 5px; border-bottom: #d4e0ee 1px solid;'>" + objReturns.Booking_ViaLocations[i].ViaLocValue.ToStr() + "</td>");
                                        StrBld.Append("</tr>");

                                    }
                                    else
                                    {
                                        if (i < cnt)
                                        {

                                            StrBld.Append("<tr>");
                                            StrBld.Append("<td style='padding: 5px; border-bottom: #d4e0ee 1px solid; border-right: #d4e0ee 1px solid;'>" + objReturns.Booking_ViaLocations[i - 1].ViaLocValue.ToStr() + "</td>");


                                            StrBld.Append("<td style='padding: 5px; border-bottom: #d4e0ee 1px solid;'>" + objReturns.Booking_ViaLocations[i].ViaLocValue.ToStr() + "</td>");
                                            StrBld.Append("</tr>");
                                        }
                                    }


                                    if (i + 1 == cnt)
                                    {
                                        StrBld.Append("<tr>");
                                        StrBld.Append("<td style='padding: 5px; border-bottom: #d4e0ee 1px solid; border-right: #d4e0ee 1px solid;'>" + objReturns.Booking_ViaLocations[i].ViaLocValue.ToStr() + "</td>");

                                        StrBld.Append("<td style='padding: 5px; border-bottom: #d4e0ee 1px solid;'>" + objReturns.ToAddress.ToStr() + "</td>");
                                        StrBld.Append("</tr>");

                                    }

                                }



                                StrBld.Append("</table></td></tr>");
                            }


                            StrBld.Append("<tr><td colspan='4'>&nbsp;</td></tr>");


                            //                 <tr><td style='padding: 10px 5px 10px 5px; font-size: 14px; border: #d4e0ee 1px solid;background-color: White; text-decoration: underline; font-weight: bold;'>Meeting Point:</td><td style='padding: 10px 5px 10px 5px; font-size: 11px; border: #d4e0ee 1px solid;background-color: #eff3f9;' colspan='3'>The driver will meet you with a name board displaying the Passenger name at ARRIVALS <span style='color: Green'>05 Minutes</span> after your flight lands (as per your request). You will have a further <span style='color: Green'>35 minutes</span> of Free waiting time, meaning a total Free waiting time allowance of <span style='color: Red'>40 Minutes</span> from the time of landing which also include car park. Please Note thereafter waiting time is chargeable at the rate of <span style='color: Red'>GBP £20p</span> per minute.</td></tr>
                            StrBld.Append("<tr><td colspan='4' style='border-bottom: #d4e0ee 1px solid;'>&nbsp;</td></tr><tr><td colspan='4' style='padding: 10px 5px 10px 5px; font-size: 18px; border-bottom: #d4e0ee 1px solid;background-color: #eff3f9;'>");



                            //if (objReturns.IsQuotation.ToBool() || (chkHideFares == null && hideFares == false) || (chkHideFares != null && chkHideFares.Checked == false))
                            if (objReturns.IsQuotation.ToBool())
                            {
                                StrBld.Append("GBP Cost: <span style='color: #008000;'>£ " + string.Format("{0:f2}", (returnPrice)) + "</span>");

                            }

                            StrBld.Append(" <span style='color: #008000;'>" + objReturns.Gen_PaymentType.DefaultIfEmpty().PaymentType.ToStr() + "</span></td></tr>");




                        }
                        string footer = string.Empty;
                        //try
                        //{

                        //    footer = db.ExecuteQuery<string>("select Footer from EmailSettings").FirstOrDefault();

                        //}
                        //catch
                        //{

                        //}

                        //if (footer.ToStr().Trim().Length > 0)
                        //    if (footer.ToStr().Trim().Length > 0)
                        //        StrBld.Append("<tr><td colspan='4' style='border-bottom: #d4e0ee 1px solid;color: #6b97c2;'><p style=\"color:#6b97c2\"><strong>" + footer + "</strong></p></td></tr>");

                        // }

                        //if (footer.ToStr().Trim().Length > 0)
                        //    StrBld.Append("<tr><td colspan='4' style='border-bottom: #d4e0ee 1px solid;'>&nbsp;</td></tr><tr><td colspan='4' style='padding: 10px 5px 10px 5px; font-weight: bold; font-size: 17px;text-align: center; border-bottom: #d4e0ee 1px solid; line-height:10px'><p style=\"color:#6b97c2\"><strong>" + footer + "</strong></p></td></tr>");

                        if (obj2.IsQuotation.ToBool())
                        {
                            StrBld.Append("<tr><td colspan='4' style='border-bottom: #d4e0ee 1px solid;'>&nbsp;</td></tr><tr><td colspan='4' style='padding: 10px 5px 10px 5px; font-weight: bold; font-size: 17px;text-align: center; border-bottom: #d4e0ee 1px solid; line-height:10px'><p style=\"color:#6b97c2\"><strong>Please check the quotation carefully…</strong></p></td></tr>");

                        }
                        else
                            StrBld.Append("<tr><td colspan='4' style='border-bottom: #d4e0ee 1px solid;'>&nbsp;</td></tr><tr><td colspan='4' style='padding: 10px 5px 10px 5px; font-weight: bold; font-size: 17px;text-align: center; border-bottom: #d4e0ee 1px solid; line-height:10px'><p style=\"color:#6b97c2\"><strong>Please check the confirmation carefully…</strong></p></td></tr>");



                        StrBld.Append("<tr><td colspan='4' style='text-align: center; padding: 5px 0px 5px 0px; font-size: 18px;font-weight: normal; color: #6b97c2;' ><p align=\"Center\">We welcome all comments on the services that we provide.</p></td></tr>");





                        StrBld.Append("</table>");

                        //try
                        //{
                        //    System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "SendConfirmationEmailTable.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + Environment.NewLine + StrBld + Environment.NewLine);

                        //}
                        //catch (Exception)
                        //{

                        //}

                        var obja = new
                        {
                            //   imageName = file,
                            //   file = base64Text,
                            subject = obj.emailInfo.Subject,
                            messageBody = StrBld.ToStr(),
                            fromEmail = obj.emailInfo.From,
                            toEmail = obj.emailInfo.To,
                            CCEmail = objSubCompany.EmailCC.ToStr(),
                            smtpHost = objSubCompany.SmtpHost,
                            smtpPwd = objSubCompany.SmtpPassword,
                            defaultclientid = HubProcessor.Instance.objPolicy.DefaultClientId.ToStr(),
                            sourceType = "dispatch-cloudx"
                        };
                        objSubCompany.UseDifferentEmailForInvoices = false;

                        List<System.Net.Mail.Attachment> attachments = new List<System.Net.Mail.Attachment>();
                        SignalRHub.Classes.ClsEmail.Send(obja.subject, obja.messageBody, obja.fromEmail, obja.toEmail, attachments, objSubCompany, "");

                        response.Data = "Booking confirmation email sent successfully.";

                        //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        //HttpClient httpClient = new HttpClient();
                        //var stringContent = new StringContent
                        //(Newtonsoft.Json.JsonConvert.SerializeObject(obja), Encoding.UTF8, "application/json");

                        //HttpResponseMessage res = httpClient.PostAsync("https://cabtreasureappapi.co.uk/CabTreasureWebApi/Home/SendTPEmail", stringContent).Result;
                        //httpClient.Dispose();
                        //string sd = res.Content.ReadAsStringAsync().Result;



                        //if (sd.ToStr().Contains("\"success\"") == false)
                        //{
                        //    ResponseData dt = null;
                        //    try
                        //    {
                        //        dt = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseData>(sd);

                        //    }
                        //    catch
                        //    {

                        //    }

                        //    if (dt != null)
                        //    {

                        //        response.HasError = true;
                        //        response.Message = dt.Message;

                        //    }
                        //    else
                        //    {
                        //        response.HasError = true;
                        //        //  response.Message =


                        //    }
                        //}
                        //else
                        //{
                        //    try
                        //    {
                        //        using (TaxiDataContext dbContext = new TaxiDataContext())
                        //        {
                        //            dbContext.ExecuteCommand("exec insertInSendEmail {0}, {1}, {2}, {3}",
                        //                obja.subject,
                        //                obja.messageBody,
                        //                string.IsNullOrWhiteSpace(obj.UserName) ? obj.objUserInfo?.UserName : obj.UserName,
                        //                obja.toEmail);
                        //        }
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //    }
                        //    response.Data = sd;
                        //}





                    }


                }
            }
            catch (Exception ex)
            {
                try
                {
                    response.HasError = true;
                    response.Message = ex.Message;

                    General.WriteLog("SendConfirmationEmail_exception", "json:" + new JavaScriptSerializer().Serialize(obj) + ", Exception: " + ex.Message);
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "SendConfirmationEmail_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
            }


            //   return Json(response, JsonRequestBehavior.AllowGet);
            return new CustomJsonResult { Data = response };
        }


        //[System.Web.Http.HttpGet]
        //[System.Web.Http.HttpPost]
        //[System.Web.Http.Route("GetBookingHistory")]
        //public JsonResult GetBookingHistory(WebApiClasses.RequestWebApi obj)
        //{
        //    //

        //    try
        //    {


        //        System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetBookingHistory.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
        //    }
        //    catch
        //    {

        //    }
        //    ResponseWebApi response = new ResponseWebApi();

        //    try
        //    {

        //        string mobNo = obj.bookingInfo.CustomerMobileNo.ToStr().Trim();
        //        string telNo = obj.bookingInfo.CustomerPhoneNo.ToStr().Trim();
        //        string customerName = obj.bookingInfo.CustomerName.ToStr().Trim();

        //        using (TaxiDataContext db = new TaxiDataContext())
        //        {



        //            var list = (from a in db.Bookings
        //                        where

        //                         (a.CustomerMobileNo != null && a.CustomerPhoneNo != null)
        //                        && (customerName == string.Empty || a.CustomerName.Trim().ToLower().StartsWith(customerName))
        //                        &&

        //                        (
        //                        ((a.CustomerPhoneNo.Trim() == telNo || telNo == string.Empty) && (a.CustomerMobileNo.Trim() == mobNo || mobNo == string.Empty))
        //                   || ((a.CustomerPhoneNo.Trim() == mobNo || mobNo == string.Empty) && (a.CustomerMobileNo.Trim() == telNo || telNo == string.Empty))

        //                   )
        //                        //
        //                        select new
        //                        {
        //                            Id = a.Id,

        //                            PickupDate = a.PickupDateTime,
        //                            FromTypeId = a.FromLocTypeId,
        //                            FromId = a.FromLocId,
        //                            From = a.FromDoorNo != string.Empty ? a.FromDoorNo + " - " + a.FromAddress : a.FromAddress,
        //                            ToId = a.ToLocId,
        //                            ToTypeId = a.ToLocTypeId,
        //                            Via = a.ViaString,
        //                            To = a.ToDoorNo != string.Empty ? a.ToDoorNo + " - " + a.ToAddress : a.ToAddress,
        //                            Fare = a.FareRate,
        //                            Fees = a.ServiceCharges,
        //                            CustFare = a.CustomerPrice,
        //                            Customer = a.CustomerName,
        //                            MobileNo = a.CustomerMobileNo,
        //                            TelNo = a.CustomerPhoneNo,
        //                            Account = a.Gen_Company.CompanyName,
        //                            CompanyFares = a.CompanyPrice,
        //                            BookingTypeId = a.BookingTypeId,
        //                            RefNo = a.BookingNo,
        //                            Vechile = a.Fleet_VehicleType.VehicleType,
        //                            Email = a.CustomerEmail,
        //                            Drv = a.DriverId != null ? a.Fleet_Driver.DriverNo : "",
        //                            AccountId = a.CompanyId,
        //                            BookingBackgroundColor = a.BookingType.BackgroundColor,
        //                            PermanentNotes = a.NoOfChilds,
        //                            SpecialReq = a.SpecialRequirements,
        //                        }).OrderByDescending(c => c.PickupDate).Take(100).ToList();
        //            response.Data = list;

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        try
        //        {
        //            response.HasError = true;
        //            response.Message = ex.Message;

        //            System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetBookingHistory_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message + Environment.NewLine);
        //        }
        //        catch
        //        {

        //        }
        //    }


        //    //   return Json(response, JsonRequestBehavior.AllowGet);
        //    return new CustomJsonResult { Data = response };
        //}

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetBookingHistory")]
        public JsonResult GetBookingHistory(WebApiClasses.RequestWebApi obj)
        {
            //

            try
            {

                General.WriteLog("GetBookingHistory", "json: " + new JavaScriptSerializer().Serialize(obj));
                //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetBookingHistory.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
            }
            catch
            {

            }
            ResponseWebApi response = new ResponseWebApi();

            try
            {

                string mobNo = obj.bookingInfo.CustomerMobileNo.ToStr().Trim();
                string telNo = obj.bookingInfo.CustomerPhoneNo.ToStr().Trim();
                string customerName = obj.bookingInfo.CustomerName.ToStr().Trim();

                using (TaxiDataContext db = new TaxiDataContext())
                {


                    var query = db.Bookings
     .Where(a => a.CustomerMobileNo != null && a.CustomerPhoneNo != null)
     .AsQueryable();

                    // Filter by customer name if provided
                    if (!string.IsNullOrEmpty(customerName))
                    {
                        var lowerName = customerName.ToLower();
                        query = query.Where(a => a.CustomerName.Trim().ToLower().Contains(lowerName));
                    }

                    // Filter by phone/mobile number
                    if (!string.IsNullOrEmpty(telNo) || !string.IsNullOrEmpty(mobNo))
                    {
                        query = query.Where(a =>
                            (
                                (string.IsNullOrEmpty(telNo) || a.CustomerPhoneNo.Trim() == telNo) &&
                                (string.IsNullOrEmpty(mobNo) || a.CustomerMobileNo.Trim() == mobNo)
                            )
                            ||
                            (
                                (string.IsNullOrEmpty(mobNo) || a.CustomerPhoneNo.Trim() == mobNo) &&
                                (string.IsNullOrEmpty(telNo) || a.CustomerMobileNo.Trim() == telNo)
                            )
                        );
                    }

                    if (string.IsNullOrEmpty(mobNo) && string.IsNullOrEmpty(telNo) && string.IsNullOrEmpty(customerName))
                    {
                        response.Data = new List<object>();
                    }
                    else
                    {
                        var list = query
                        .Select(a => new
                        {
                            Id = a.Id,
                            PickupDate = a.PickupDateTime,
                            FromTypeId = a.FromLocTypeId,
                            FromId = a.FromLocId,
                            From = a.FromDoorNo != string.Empty ? a.FromDoorNo + " - " + a.FromAddress : a.FromAddress,
                            ToId = a.ToLocId,
                            ToTypeId = a.ToLocTypeId,
                            Via = a.ViaString,
                            To = a.ToDoorNo != string.Empty ? a.ToDoorNo + " - " + a.ToAddress : a.ToAddress,
                            Fare = a.FareRate,
                            Fees = a.ServiceCharges,
                            CustFare = a.CustomerPrice,
                            Customer = a.CustomerName,
                            MobileNo = a.CustomerMobileNo,
                            TelNo = a.CustomerPhoneNo,
                            Account = a.Gen_Company.CompanyName,
                            CompanyFares = a.CompanyPrice,
                            BookingTypeId = a.BookingTypeId,
                            RefNo = a.BookingNo,
                            Vechile = a.Fleet_VehicleType.VehicleType,
                            Email = a.CustomerEmail,
                            Drv = a.DriverId != null ? a.Fleet_Driver.DriverNo : "",
                            AccountId = a.CompanyId,
                            BookingBackgroundColor = a.BookingType.BackgroundColor,
                            PermanentNotes = a.NoOfChilds,
                            SpecialReq = a.SpecialRequirements,
                            LikesAndDisLikes = a.Customer != null ? a.Customer.LikesAndDislikes : ""
                        })
                        .OrderByDescending(c => c.PickupDate)
                        .Take(100)
                        .ToList();
                        response.Data = list;
                    }

                }
            }
            catch (Exception ex)
            {
                try
                {
                    response.HasError = true;
                    response.Message = ex.Message;

                    General.WriteLog("GetBookingHistory_exception", "json:" + new JavaScriptSerializer().Serialize(obj) + ", Exception: " + ex.Message);
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetBookingHistory_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
            }


            //   return Json(response, JsonRequestBehavior.AllowGet);
            return new CustomJsonResult { Data = response };
        }




        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("UpdateBookingAction")]
        public JsonResult UpdateBookingAction(WebApiClasses.RequestWebApi obj)
        {
            //

            try
            {

                General.WriteLog("UpdateBookingAction", "json: " + new JavaScriptSerializer().Serialize(obj));
                //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "UpdateBookingAction.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
            }
            catch
            {

            }
            ResponseWebApi response = new ResponseWebApi();

            try
            {

                long jobId = obj.bookingInfo.Id;


                using (TaxiDataContext db = new TaxiDataContext())
                {

                    //
                    if (obj.bookingInfo.AutoDespatch.ToBool() && obj.bookingInfo.IsBidding.ToBool())
                    {

                        //var objBooking = db.Bookings.FirstOrDefault(c => c.Id == jobId);

                        //if (objBooking != null)
                        //{
                        //    objBooking.AutoDespatch = true;
                        //    objBooking.IsBidding = true;
                        //    db.SubmitChanges();

                        //
                        db.ExecuteQuery<int>("update booking set AutoDespatch=1,IsBidding=1 where id=" + jobId);

                        //  new BroadcasterData().BroadCastToAll(RefreshTypes.REFRESH_ACTIVEBOOKINGS_DASHBOARD);
                        General.BroadCastMessage("refresh holdandrelease" + " >> " + jobId.ToStr() + ">>release");

                        // db.ExecuteQuery<int>("exec stp_BookingLog {0},{1},{2},{3} ", jobId, obj.UserName.ToStr().Trim().Length > 0 ? obj.UserName.ToStr() : "controller", "RELEASE JOB", DateTime.Now.GetUtcTimeZone());

                        db.stp_BookingLog(jobId, obj.UserName.ToStr().Trim().Length > 0 ? obj.UserName.ToStr() : "controller", "RELEASE JOB");
                        //   }


                    }
                    else
                    {
                        int statusId = db.Bookings.Where(c => c.Id == jobId).Select(c => c.BookingStatusId).FirstOrDefault().ToInt();

                        if (
                                (statusId == Enums.BOOKINGSTATUS.ONROUTE || statusId == Enums.BOOKINGSTATUS.ARRIVED
                                || statusId == Enums.BOOKINGSTATUS.POB || statusId == Enums.BOOKINGSTATUS.STC || statusId == Enums.BOOKINGSTATUS.FOJ || statusId == Enums.BOOKINGSTATUS.PENDING_START))
                        {

                            response.HasError = true;
                            response.Message = "Job is already Accepted by driver";

                            // MessageBox.Show("Job is already Accepted by driver");
                        }
                        else
                        {

                            db.stp_UpdateJobStatus(jobId, Enums.BOOKINGSTATUS.ONHOLD);

                            General.BroadCastMessage("refresh holdandrelease" + ">>" + jobId.ToStr() + ">>hold");
                            // db.ExecuteQuery<int>("exec stp_BookingLog {0},{1},{2},{3} ", jobId, obj.UserName.ToStr().Trim().Length > 0 ? obj.UserName.ToStr() : "controller", "HOLD JOB", DateTime.Now.GetUtcTimeZone());
                            db.stp_BookingLog(jobId, obj.UserName.ToStr().Trim().Length > 0 ? obj.UserName.ToStr() : "controller", "HOLD JOB");


                        }


                    }
















                }
            }
            catch (Exception ex)
            {
                try
                {
                    response.HasError = true;
                    response.Message = ex.Message;

                    General.WriteLog("UpdateBookingAction_exception", "json:" + new JavaScriptSerializer().Serialize(obj) + ", Exception: " + ex.Message);
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "UpdateBookingAction_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
            }


            //   return Json(response, JsonRequestBehavior.AllowGet);
            return new CustomJsonResult { Data = response };
        }

        //[System.Web.Http.HttpGet]
        //[System.Web.Http.HttpPost]
        //[System.Web.Http.Route("UpdateBookingAction")]
        //public JsonResult UpdateBookingAction(WebApiClasses.RequestWebApi obj)
        //{
        //    //

        //    try
        //    {


        //        System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "UpdateBookingAction.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
        //    }
        //    catch
        //    {

        //    }
        //    ResponseWebApi response = new ResponseWebApi();

        //    try
        //    {

        //        long jobId = obj.bookingInfo.Id;


        //        using (TaxiDataContext db = new TaxiDataContext())
        //        {

        //            string username = "Controller";

        //            if (obj.UserName.ToStr().Trim().Length > 0)
        //                username = obj.UserName.ToStr().Trim();

        //            if (obj.bookingInfo.AutoDespatch.ToBool() && obj.bookingInfo.IsBidding.ToBool())
        //            {

        //                var objBooking = db.Bookings.FirstOrDefault(c => c.Id == jobId);

        //                if (objBooking != null)
        //                {
        //                    objBooking.AutoDespatch = true;
        //                    objBooking.IsBidding = true;
        //                    db.SubmitChanges();

        //                    //  new BroadcasterData().BroadCastToAll(RefreshTypes.REFRESH_ACTIVEBOOKINGS_DASHBOARD);
        //                    General.BroadCastMessage("refresh holdandrelease" + " >> " + jobId.ToStr() + ">>release");


        //                    try
        //                    {
        //                        db.ExecuteQuery<int>("exec stp_BookingLog {0},{1},{2},{3}", jobId, username, "RELEASE JOB", DateTime.Now);
        //                    }
        //                    catch
        //                    {

        //                    }
        //                    // db.stp_BookingLog(jobId, username, "RELEASE JOB");


        //                }


        //            }
        //            else
        //            {
        //                int statusId = db.Bookings.Where(c => c.Id == jobId).Select(c => c.BookingStatusId).FirstOrDefault().ToInt();

        //                if (
        //                        (statusId == Enums.BOOKINGSTATUS.ONROUTE || statusId == Enums.BOOKINGSTATUS.ARRIVED
        //                        || statusId == Enums.BOOKINGSTATUS.POB || statusId == Enums.BOOKINGSTATUS.STC || statusId == Enums.BOOKINGSTATUS.FOJ || statusId == Enums.BOOKINGSTATUS.PENDING_START))
        //                {

        //                    response.HasError = true;
        //                    response.Message = "Job is already Accepted by driver";

        //                    // MessageBox.Show("Job is already Accepted by driver");
        //                }
        //                else
        //                {

        //                    db.stp_UpdateJobStatus(jobId, Enums.BOOKINGSTATUS.ONHOLD);

        //                    General.BroadCastMessage("refresh holdandrelease" + ">>" + jobId.ToStr() + ">>hold");
        //                    //  db.stp_BookingLog(jobId, username, "HOLD JOB");

        //                    try
        //                    {
        //                        db.ExecuteQuery<int>("exec stp_BookingLog {0},{1},{2},{3}", jobId, username, "HOLD JOB", DateTime.Now);
        //                    }
        //                    catch
        //                    {

        //                    }
        //                }


        //            }



        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        try
        //        {
        //            response.HasError = true;
        //            response.Message = ex.Message;

        //            System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "UpdateBookingAction_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message + Environment.NewLine);
        //        }
        //        catch
        //        {

        //        }
        //    }


        //    //   return Json(response, JsonRequestBehavior.AllowGet);
        //    return new CustomJsonResult { Data = response };
        //}



        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("AuthAction")]
        public JsonResult AuthAction(WebApiClasses.RequestWebApi obj)
        {
            //

            try
            {

                General.WriteLog("AuthAction", "json: " + new JavaScriptSerializer().Serialize(obj));
                //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "AuthAction.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
            }
            catch
            {

            }
            ResponseWebApi response = new ResponseWebApi();

            try
            {

                long jobId = obj.authInfo.BookingId.ToLong();
                int driverId = obj.authInfo.DriverId.ToInt();
                string action = obj.authInfo.AuthStatus.ToStr();

                using (TaxiDataContext db = new TaxiDataContext())
                {

                    //   **logout auth >> " + values[1].ToStr() + " >> " + values[2].ToStr()
                    //

                    if (obj.authInfo.AuthType.ToInt() == 1)
                    {
                        string currentTime = string.Format("{0:dd/MM/yyyy hh:mm:ss}", DateTime.Now);


                        if (obj.authInfo.AuthStatus.ToStr().ToLower() == "yes")
                        {
                            General.requestPDA("request pda=" + driverId.ToStr() + "=" + obj.authInfo.DriverNo.ToStr() + "=logout auth status>>yes>>" + driverId.ToStr() + ">>" + currentTime + "=10");



                            //  General.SendMessageToPDA("request force logout=" + values[2].ToStr());

                            //   System.Threading.Thread.Sleep(3000);
                            //    General.BroadCastMessage("**broadcast close logout auth");

                            //   General.BroadCastMessage("**broadcast close logout auth>>" + Environment.MachineName + ">>" + "**logout auth>>" + driverId.ToStr() + ">>" + obj.authInfo.DriverNo.ToStr());








                        }
                        else
                        {
                            //  string[] values = (sender as RadButtonElement).Tag.ToStr().Split(new string[] { ">>" }, StringSplitOptions.None);



                            General.requestPDA("request pda=" + driverId.ToStr() + "=" + obj.authInfo.DriverNo.ToStr() + "=logout auth status>>no>>" + driverId.ToStr() + ">>" + currentTime + "=10");

                            General.requestPDA("request broadcast=" + "**broadcast close logout auth>>" + Environment.MachineName + ">>" + driverId.ToStr());
                        }

                    }
                    else if (obj.authInfo.AuthType.ToInt() == 2)
                    {
                        //   string[] values = msg.Split(new string[] { ">>" }, StringSplitOptions.None);




                        int JobStatusId = obj.authInfo.BookingStatusId.ToInt();
                        int DriverStatusId = obj.authInfo.DriverStatusId.ToInt();




                        if (obj.authInfo.AuthStatus.ToStr().ToLower() == "yes")
                        {
                            General.MessageToPDAByDriverId("request pda=" + driverId + "=" + jobId + "=auth status>>yes>>" + jobId + "=5=" + JobStatusId.ToStr(), driverId.ToStr());





                            if (JobStatusId == Enums.BOOKINGSTATUS.NOPICKUP)
                            {
                                DriverStatusId = Enums.Driver_WORKINGSTATUS.AVAILABLE;
                            }
                            else if (JobStatusId == Enums.BOOKINGSTATUS.REJECTED)
                            {
                                DriverStatusId = Enums.Driver_WORKINGSTATUS.AVAILABLE;
                            }
                            else if (JobStatusId == Enums.BOOKINGSTATUS.NOSHOW)
                            {
                                DriverStatusId = Enums.Driver_WORKINGSTATUS.AVAILABLE;
                            }




                            if (JobStatusId == Enums.BOOKINGSTATUS.NOPICKUP && HubProcessor.Instance.objPolicy.EnableBookingOtherCharges.ToBool())
                            {


                                db.ExecuteQuery<int>("exec stp_UpdateJobEnd {0},{1},{2},{3},{4},{5},{6}", jobId, driverId, JobStatusId.ToIntorNull(), DriverStatusId.ToIntorNull(), -1, "", "-1");

                            }
                            else
                            {
                                try
                                {
                                    db.ExecuteQuery<int>("exec stp_UpdateJob {0},{1},{2},{3},{4},{5}", jobId, driverId, JobStatusId.ToIntorNull(), DriverStatusId.ToIntorNull(), -1, true);
                                }
                                catch
                                {
                                    db.stp_UpdateJob(jobId, driverId, JobStatusId.ToIntorNull(), DriverStatusId.ToIntorNull(), -1);
                                }


                            }


                            //   AppVars.frmMDI.RefreshActiveDashBoard();

                            //  General.CallGetDashboardData();
                            //     General.BroadCastMessage("**broadcast close auth job");
                            General.BroadCastMessage("**broadcast close auth job>>" + Environment.MachineName + ">>" + obj.authInfo.Message.ToStr().Replace(">>", "<<") + ">>allow");
                            try
                            {
                                if (JobStatusId == Enums.BOOKINGSTATUS.NOSHOW)
                                {
                                    var objDriver = new List<int>();
                                    var EnableSentPDAMsgOnNoPickupToOther = "0";

                                    objDriver = db.Fleet_Drivers.Where(c => c.Id != driverId.ToInt()).Select(c => c.Id).ToList();
                                    EnableSentPDAMsgOnNoPickupToOther = db.ExecuteQuery<string>("Select SetVal from AppSettings where SetKey = 'EnableSentPDAMsgOnNoPickupToOther'").FirstOrDefault();

                                    if (EnableSentPDAMsgOnNoPickupToOther == "1" && objDriver != null && objDriver.Count > 0)
                                    {
                                        foreach (int itemId in objDriver)
                                        {
                                            General.requestPDA("request pda=" + itemId + "=" + 0 + "=" + "Message>>Driver Priority - No Show>>" + String.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Now) + "=4");
                                        }
                                    }
                                }
                            }
                            catch
                            {
                            }
                            //if (JobStatusId == Enums.BOOKINGSTATUS.NOPICKUP)
                            //{
                            //    if (AppVars.objPolicyConfiguration.SMSNoPickup.ToStr().Trim().Length > 0)
                            //    {
                            //        using (TaxiDataContext db = new TaxiDataContext())
                            //        {


                            //            db.CommandTimeout = 3;
                            //            var objBook = db.Bookings.FirstOrDefault(c => c.Id == _JobId);

                            //            if (objBook != null && objBook.CustomerMobileNo.ToStr().Trim().Length > 0)
                            //            {
                            //                SendSMS(objBook.CustomerMobileNo.ToStr().Trim(), GetMessage(AppVars.objPolicyConfiguration.SMSNoPickup.ToStr().Trim(), objBook, objBook.Id), objBook.SMSType.ToInt());
                            //            }
                            //        }
                            //    }


                            //    UpdateNoPickupAndCancelledCountFromDb();
                            //}


                            try
                            {
                                if (JobStatusId == Enums.BOOKINGSTATUS.NOPICKUP)
                                {
                                    decimal cancellationfee = 0;
                                    if (!string.IsNullOrEmpty(Global.CancellationFee))
                                    {
                                        try
                                        {
                                            cancellationfee = Global.CancellationFee.ToDecimal();
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    if (cancellationfee > 0)
                                    {
                                        var objBooking = db.Bookings.Where(c => c.Id == jobId)
                                           .Select(args => new { args.Id, args.BookingNo, args.DriverId, args.BookingStatusId, args.CustomerName, args.FromAddress, args.ToAddress, args.PickupDateTime, args.PaymentTypeId, args.CustomerCreditCardDetails, args.Gen_SubCompany, args.CompanyCreditCardDetails, args.FareRate }).FirstOrDefault();

                                        if (objBooking != null)
                                        {
                                            if (objBooking.PaymentTypeId.ToInt() == Enums.PAYMENT_TYPES.CREDIT_CARD && objBooking.CustomerCreditCardDetails.ToStr().Trim().Length > 0)
                                            {
                                                double amount = Convert.ToDouble(cancellationfee);
                                                if (Global.CancellationFeeType == "2")
                                                {
                                                    amount = Convert.ToDouble(objBooking.FareRate.ToDecimal() * (cancellationfee / 100));
                                                }

                                                if (amount > 0)
                                                {
                                                    string DefaultCurrency = System.Configuration.ConfigurationManager.AppSettings["DefaultCurrency"];
                                                    string DefaultClientLocation = System.Configuration.ConfigurationManager.AppSettings["DefaultClientLocation"];
                                                    string DefaultCurrencySign = System.Configuration.ConfigurationManager.AppSettings["DefaultCurrencySign"];

                                                    Classes.KonnectSupplier.PaymentCaptureResponse resp = new Classes.KonnectSupplier.PaymentCaptureResponse();

                                                    Classes.KonnectSupplier.PaymentCaptureDto st = new Classes.KonnectSupplier.PaymentCaptureDto();
                                                    Gen_SysPolicy_PaymentDetail GatwayObj = General.GetObject<Gen_SysPolicy_PaymentDetail>(c => c.PaymentGatewayId == 15);
                                                    st.bookingId = objBooking.Id;
                                                    st.description = objBooking?.Gen_SubCompany?.CompanyName + " | " + objBooking?.BookingNo.ToStr() + " | " + "Fares : " + amount + " " + DefaultCurrency;
                                                    st.bookingRef = objBooking?.BookingNo.ToStr();
                                                    st.countryId = GatwayObj.ApplicationId.ToInt();
                                                    st.connectedAccountId = GatwayObj.PaypalID.ToStr();
                                                    st.applicationFee = 0;
                                                    st.otherCharges = 0;
                                                    st.amount = Convert.ToInt64(amount * 100);
                                                    st.displayAmount = amount.ToDecimal();
                                                    st.currency = DefaultCurrency;
                                                    st.defaultClientId = HubProcessor.Instance.objPolicy.DefaultClientId.ToStr();
                                                    st.location = DefaultClientLocation;
                                                    st.companyName = objBooking?.Gen_SubCompany?.CompanyName ?? "";
                                                    st.paymentIntentId = objBooking.CustomerCreditCardDetails.ToStr();
                                                    st.status = objBooking.CompanyCreditCardDetails;
                                                    var DataObj = Newtonsoft.Json.JsonConvert.SerializeObject(st);
                                                    string StripeAPIBaseURL = System.Configuration.ConfigurationManager.AppSettings["StripeAPIBaseURL"];
                                                    using (var client = new HttpClient())
                                                    {
                                                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                                                        client.BaseAddress = new Uri(StripeAPIBaseURL);
                                                        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                                                        var content = new StringContent(DataObj, Encoding.UTF8, "application/json");
                                                        var postTask = client.PostAsync(StripeAPIBaseURL + "/v1/CapturePayment/IncrementAuthorization", content).Result;
                                                        string PayResp = postTask.Content.ReadAsStringAsync().Result;
                                                        resp = new JavaScriptSerializer().Deserialize<Classes.KonnectSupplier.PaymentCaptureResponse>(PayResp);
                                                        if (resp != null && resp.isSuccess)
                                                        {
                                                            string msg = "NoPickup fee charged  " + DefaultCurrencySign + string.Format("{0:f2}", amount) + " | TRANSACTION - " + resp.paymentId.ToStr() + " | " + string.Format("{0:dd/MM/yyyy HH:mm}", DateTime.Now);
                                                            db.stp_BookingLog(objBooking.Id.ToLong(), "System", msg);
                                                        }
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

                        }
                        else
                        {


                            General.MessageToPDAByDriverId("request pda=" + driverId + "=" + jobId + "=auth status>>no>>" + jobId + "=5", driverId.ToStr());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    response.HasError = true;
                    response.Message = ex.Message;

                    General.WriteLog("AuthAction_exception", "json:" + new JavaScriptSerializer().Serialize(obj) + ", Exception: " + ex.Message);
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "AuthAction_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
            }


            //   return Json(response, JsonRequestBehavior.AllowGet);
            return new CustomJsonResult { Data = response };
        }
        private string GetMinsInWords(double mins)
        {


            double hr = mins / 60;



            if (hr < 1)
                return mins.ToInt().ToStr() + " mins";
            else
            {
                if (hr.ToStr().Contains("."))
                {

                    var t = TimeSpan.FromMinutes(mins);


                    if (t.TotalHours.ToStr().Contains("."))
                    {
                        return t.TotalHours.ToStr().Substring(0, t.TotalHours.ToStr().IndexOf(".")).ToInt() + "h" + " " + t.Minutes + "m";
                    }
                    else
                        return t.TotalHours.ToInt() + "h" + " " + t.Minutes + "m";
                }
                else
                {
                    if (hr.ToInt() < 2)
                        return "1 hour";
                    else
                        return hr.ToInt() + "hours";


                }
            }





        }




        public Classes.FlightData flightDataobj = null;
        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetFlightInformation")]
        public JsonResult GetFlightInformation(WebApiClasses.RequestWebApi obj)
        {
            //
            ResponseWebApi response = new ResponseWebApi();
            try
            {
                DateTime? ScheduleDateTime = obj.ScheduleDateTime;
                DateTime? DelayedDateTime = obj.DelayedDateTime;
                string ArrivalTerminal = obj.ArrivalTerminal;
                string ArrivingFrom = obj.ArrivingFrom;
                string FlightNum = obj.FlightNo;
                string FlightDate = obj.InputDateTime;
                string Status = obj.Status;
                string DateTime = obj.DateTime;
                string Message = obj.Message;
                int? AllowanceMins = obj.AllowanceMins;
                string FlightInformation = obj.FlightInformation;
                string InputDateTime = obj.InputDateTime;
                var json = "";
                Classes.FlightData fdata = new Classes.FlightData();
                // fdata.FlightNo = FlightNum;
                //fdata.InputDateTime = string.Format("{0:dd/MM/yyyy}", FlightDate);
                fdata.ScheduleDateTime = ScheduleDateTime;
                fdata.ScheduleDateTime = ScheduleDateTime;
                fdata.DelayedDateTime = DelayedDateTime;
                fdata.ArrivalTerminal = ArrivalTerminal;
                fdata.ArrivingFrom = ArrivingFrom;
                fdata.FlightNo = FlightNum;
                fdata.InputDateTime = InputDateTime;
                fdata.Status = Status;
                fdata.DateTime = DateTime;
                fdata.Message = Message;
                fdata.AllowanceMins = AllowanceMins;
                fdata.FlightInformation = FlightInformation;
                fdata.InputDateTime = InputDateTime;
                json = Newtonsoft.Json.JsonConvert.SerializeObject(fdata);
                string API = "http://116.202.117.250/WindsorCars/api/supplier/requestflightdetails" + "?mesg=" + json;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(API);
                request.ContentType = "application/json; charset=utf-8";
                request.Accept = "application/json";
                request.Method = WebRequestMethods.Http.Get;
                request.Proxy = null;
                WebResponse responsea = request.GetResponse();
                StreamReader sr = new StreamReader(responsea.GetResponseStream());
                json = sr.ReadToEnd();
                flightDataobj = null;
                if (json.Contains("{"))
                {
                    try
                    {
                        flightDataobj = Newtonsoft.Json.JsonConvert.DeserializeObject<Classes.FlightData>(json);
                        flightDataobj.StrScheduleDateTime = flightDataobj.ScheduleDateTime.ToString();
                        flightDataobj.StrDelayedDateTime = flightDataobj.DelayedDateTime.ToString();
                    }
                    catch
                    {
                    }
                    //if (data.ScheduleDateTime.Year < 2000)
                    //{
                    //    //MessageBox.Show("Flight details not found");
                    //    data = null;
                    //}
                    //else
                    {
                        try
                        {
                            // File.AppendAllText(Application.StartupPath + "\\Logs\\FlightTracker\\" + FlightNum + ".txt", json);
                        }
                        catch
                        {
                        }
                        response.Data = flightDataobj;
                    }
                    ;
                }
            }
            catch (Exception ex)
            {
                response.HasError = true;
                response.Message = ex.Message;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }


        #endregion




        private stp_GetBookingsDataResult GetBookingJSON(BookingBO objMaster)
        {

            try
            {
                //

                string status = "";

                if (objMaster.Current.BookingStatusId == Enums.BOOKINGSTATUS.WAITING)
                    status = "Waiting";
                else if (objMaster.Current.BookingStatusId == Enums.BOOKINGSTATUS.DISPATCHED)
                    status = "Completed";
                else if (objMaster.Current.BookingStatusId == Enums.BOOKINGSTATUS.ONROUTE)
                    status = "OnRoute";
                else if (objMaster.Current.BookingStatusId == Enums.BOOKINGSTATUS.ARRIVED)
                    status = "Arrived";
                else if (objMaster.Current.BookingStatusId == Enums.BOOKINGSTATUS.POB)
                    status = "POB";
                else if (objMaster.Current.BookingStatusId == Enums.BOOKINGSTATUS.STC)
                    status = "STC";
                else if (objMaster.Current.BookingStatusId == Enums.BOOKINGSTATUS.NOPICKUP)
                    status = "NoPickup";
                else if (objMaster.Current.BookingStatusId == Enums.BOOKINGSTATUS.CANCELLED)
                    status = "Cancelled";
                else if (objMaster.Current.BookingStatusId == Enums.BOOKINGSTATUS.FOJ)
                    status = "FOJ";
                else if (objMaster.Current.BookingStatusId == Enums.BOOKINGSTATUS.PENDING)
                    status = "Pending";
                else if (objMaster.Current.BookingStatusId == Enums.BOOKINGSTATUS.PENDING_START)
                    status = "Pending Start";
                else if (objMaster.Current.BookingStatusId == Enums.BOOKINGSTATUS.BID)
                    status = "BID";
                else if (objMaster.Current.BookingStatusId == Enums.BOOKINGSTATUS.NOSHOW)
                    status = "Recover";
                else if (objMaster.Current.BookingStatusId == Enums.BOOKINGSTATUS.NOTACCEPTED)
                    status = "Not Accepted";
                else if (objMaster.Current.BookingStatusId == Enums.BOOKINGSTATUS.REJECTED)
                    status = "Rejected";



                DateTime? oneWayJobDue = null;

                if (objMaster.Current.ZoneId != null)
                {
                    try
                    {
                        oneWayJobDue = objMaster.Current.Gen_Zone.JobDueTime.ToDateTime();
                    }
                    catch
                    {

                    }


                    //if (chkLead.Checked)
                    //{
                    //    oneWayJobDue = objMaster.Current.PickupDateTime.Value.AddMinutes(-numLead.Value.ToInt());

                    //}
                    //else
                    //{

                    if (oneWayJobDue != null)
                    {
                        oneWayJobDue = objMaster.Current.PickupDateTime.Value.AddMinutes(-((oneWayJobDue.Value.Hour * 60) + oneWayJobDue.Value.Minute)).ToDateTime();

                    }
                    else
                        oneWayJobDue = objMaster.Current.PickupDateTime;
                    //   }
                }
                else
                {
                    //if (chkLead.Checked)
                    //{
                    //    oneWayJobDue = objMaster.Current.PickupDateTime.Value.AddMinutes(-numLead.Value.ToInt());

                    //}
                    //else
                    //{
                    oneWayJobDue = objMaster.Current.PickupDateTime.Value.AddMinutes(-HubProcessor.Instance.objPolicy.BookingExpiryNoticeInMins.ToInt());
                    //    }


                }







                string pickupNotes = "";
                if (objMaster.Current.FromLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT && objMaster.Current.FromDoorNo.ToStr().Trim().Length > 0)
                {

                    pickupNotes = objMaster.Current.FromDoorNo.ToStr().Trim() + "-";


                    //if (objMaster.Current.FromStreet.ToStr().Trim().Length > 0 && AppVars.listUserRights.Count(c => c.functionId == "HIDE FLIGHT OTHER DETAILS ON DASHBOARD") == 0)
                    //{
                    //    pickupNotes += objMaster.Current.FromStreet.ToStr().Trim() + " ";

                    //}

                }



                string specialReq = objMaster.Current.SpecialRequirements.ToStr().Replace("=", "-");

                string accountbgcolor = "";
                string accounttextcolor = "";

                //if (objMaster.Current.CompanyId != null && ddlCompany.SelectedItem != null)
                //{
                //    if (ddlCompany.SelectedItem.ToStr().ToLower().Contains("backgroundcolor"))
                //    {
                //        try
                //        {
                //            string[] arr = (ddlCompany.SelectedItem.ToStr().Substring(ddlCompany.SelectedItem.ToStr().IndexOf("BackgroundColor = ")).Split(','));
                //            if (arr.Count() == 2)
                //            {
                //                accountbgcolor = arr[0].Replace("BackgroundColor =", "").ToStr().Trim();
                //                accounttextcolor = arr[1].Replace("TextColor =", "").ToStr().Trim();

                //                if (accounttextcolor.ToStr().EndsWith("}"))
                //                {
                //                    accounttextcolor = accounttextcolor.Replace("}", "").ToStr().Trim();
                //                }
                //            }
                //        }
                //        catch
                //        {

                //        }


                //    }
                //}


                string vehicleDetails = string.Empty;
                //if (objMaster.Current.DriverId != null)
                //{
                //    if (ddlDriver.SelectedItem.DataBoundItem.ToStr().ToLower().Contains("vehicledetails"))
                //    {
                //        try
                //        {
                //            string[] arr = (ddlDriver.SelectedItem.DataBoundItem.ToStr().Substring(ddlDriver.SelectedItem.DataBoundItem.ToStr().ToLower().IndexOf("vehicledetails = ")).Split(','));
                //            if (arr.Count() == 1)
                //            {
                //                vehicleDetails = arr[0].ToLower().Replace("vehicledetails =", "").ToStr().Trim();

                //            }
                //        }
                //        catch
                //        {

                //        }


                //    }


                //}


                int hasNotes = 0;
                //if ((objMaster.Current.BookingStatusId == Enums.BOOKINGSTATUS.BID && btn_notes.Text.Contains("(0)") == false))
                //{
                //    hasNotes = 1;
                //}
                //else if (ddlPickupPlot.Text.ToUpper().Contains("CONGESTION") || ddlDropOffPlot.Text.ToUpper().Contains("CONGESTION"))
                //{

                //    hasNotes = -1;
                //}

                // (objMaster.Current.BookingStatusId == Enums.BOOKINGSTATUS.BID && btn_notes.Text.Contains("(0)") == false) ? 1 : 0


                var obj = new stp_GetBookingsDataResult()
                {
                    Id = objMaster.Current.Id,

                    Account = objMaster.Current.CompanyId != null ? objMaster.Current.Gen_Company.CompanyName : "",


                    Vehicle = objMaster.Current.Fleet_VehicleType.VehicleType,
                    From = pickupNotes + objMaster.Current.FromAddress,
                    To = objMaster.Current.ToAddress,
                    Fare = Math.Round(objMaster.Current.FareRate.ToDecimal() + objMaster.Current.ServiceCharges.ToDecimal(), 2),

                    Plot = objMaster.Current.Gen_Zone.DefaultIfEmpty().ZoneName,
                    BabySeats = "",

                    BookingTypeId = objMaster.Current.BookingTypeId,


                    FromLocTypeId = objMaster.Current.FromLocTypeId,
                    JourneyTypeId = objMaster.Current.JourneyTypeId,
                    Passenger = objMaster.Current.CustomerName,
                    ToLocTypeId = objMaster.Current.ToLocTypeId,

                    DeadMileage = objMaster.Current.DeadMileage,
                    IsAutoDespatch = objMaster.Current.AutoDespatch,
                    IsBidding = objMaster.Current.IsBidding,
                    FromPostCode = objMaster.Current.FromPostCode,
                    PReference = objMaster.Current.PaymentComments,
                    Vias = objMaster.Current.Booking_ViaLocations.Count,
                    RefNumber = objMaster.Current.BookingNo,
                    SpecialReq = specialReq,

                    PaymentMethod = objMaster.Current.Gen_PaymentType.DefaultIfEmpty().PaymentType,

                    Pax = objMaster.Current.NoofPassengers,
                    BackgroundColor1 = accountbgcolor,
                    TextColor1 = accounttextcolor,
                    BackgroundColor = objMaster.Current.Fleet_VehicleType.BackgroundColor.ToStr(),
                    TextColor = objMaster.Current.Fleet_VehicleType.TextColor.ToStr(),
                    SubCompanyBgColor = objMaster.Current.Gen_SubCompany.BackgroundColor.ToIntorNull(),
                    BookingBackgroundColor = objMaster.Current.BookingType.BackgroundColor.ToIntorNull(),
                    UpdateBy = objMaster.Current.AddLog,
                    StatusColor = "-1",
                    MobileNo = objMaster.Current.CustomerMobileNo,

                    BookingDateTime = objMaster.Current.BookingDate,
                    Due = objMaster.Current.PickupDateTime,
                    Time = string.Format("{0:HH:mm}", objMaster.Current.PickupDateTime),
                    PickUpDate = string.Format("{0:dd-MMM}", objMaster.Current.PickupDateTime).ToUpper(),
                    PrePickupDate = objMaster.Current.PickupDateTime.Value.Date,
                    PickupDateTemp = objMaster.Current.PickupDateTime,
                    Lead = oneWayJobDue,
                    DespatchDateTime = objMaster.Current.DespatchDateTime,

                    Driver = objMaster.Current.DriverId != null ? objMaster.Current.Fleet_Driver.DriverNo : "",
                    DriverId = objMaster.Current.DriverId,
                    IsConfirmedDriver = objMaster.Current.IsConfirmedDriver,
                    StatusId = objMaster.Current.BookingStatusId,
                    Status = status,

                    TelephoneNo = objMaster.Current.CustomerPhoneNo,
                    HasNotesImg = "",
                    //  HasNotes = (objMaster.Current.BookingStatusId == Enums.BOOKINGSTATUS.BID && btn_notes.Text.Contains("(0)") == false) ? 1 : 0,
                    HasNotes = hasNotes,
                    MilesFromBase = objMaster.Current.ExtraMile,
                    NoofLuggages = objMaster.Current.IsQuotedPrice.ToBool() ? 1 : 0,
                    Attributes = objMaster.Current.AttributeValues.ToStr().Replace(",", " "),
                    VehicleDetails = vehicleDetails,
                    ToPostCode = objMaster.Current.ToPostCode,
                    GroupId = objMaster.Current.GroupJobId != null ? objMaster.Current.BookingGroup.GroupName.ToStr() : ""


                };




                return obj;

            }
            catch (Exception ex)
            {
                try
                {

                    //    File.AppendAllText(Application.StartupPath + "\\exception_getbookingjson.txt", DateTime.Now + " :" + ex.Message + Environment.NewLine);

                }
                catch
                {


                }
                return null;


            }


        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("ForceCompleteJob")]
        public JsonResult ForceCompleteJob(WebApiClasses.RequestWebApi obj)
        {
            //
            ResponseWebApi response = new ResponseWebApi();
            List<string> listofErrors = new List<string>();
            bool IsDespatched = OnDespatching(ref listofErrors);
            if (IsDespatched)
            {
                try
                {
                    (new TaxiDataContext()).stp_DespatchedJob(obj.JobId, obj.DriverId, false, false, false, false, obj.UserName, Enums.BOOKINGSTATUS.DISPATCHED);
                    //if (!this.IsAutoDespatchActivity)
                    //{
                    //    new BroadcasterData().BroadCastToAll(RefreshTypes.REFRESH_ACTIVEBOOKINGS_DASHBOARD);
                    //}
                }
                catch (Exception ex)
                {
                    response.Message = ex.Message;
                    response.HasError = true;
                }
            }
            return new CustomJsonResult { Data = response };
        }
        public bool OnDespatching(ref List<string> listofErrors)
        {
            bool rtn = false;
            string smsError1 = string.Empty;
            string smsError2 = string.Empty;
            try
            {
                rtn = true;
            }
            catch (Exception ex)
            {
                //  IsSuccess1 = false;
                listofErrors.Add(ex.Message);
                rtn = false;
            }
            return rtn;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetNearestDrivers")]
        public JsonResult GetNearestDrivers(WebApiClasses.RequestWebApi obj)
        {
            ResponseWebApi response = new ResponseWebApi();

            try
            {
                //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetNearestDrivers.txt",
                //DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + obj.addressInfo.Address.ToStr().ToUpper().Trim() + Environment.NewLine);
                General.WriteLog("GetNearestDrivers", "json: " + obj.addressInfo.Address.ToStr().ToUpper().Trim());
            }
            catch { }

            try
            {
                using (TaxiDataContext db = new TaxiDataContext())
                {
                    string searchValue = obj.addressInfo.Address.ToStr().ToUpper().Trim();
                    int loctypeId = Enums.LOCATION_TYPES.ADDRESS;
                    double? latitude = obj.addressInfo.Latitude;
                    double? longitude = obj.addressInfo.Longitude;

                    // Try to find location in Gen_Locations
                    var objLoc = db.Gen_Locations.FirstOrDefault(c =>
                        c.Address.ToUpper() == searchValue || c.FullLocationName.ToUpper() == searchValue);

                    if (objLoc != null)
                    {
                        if (objLoc.LocationTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT)
                            loctypeId = objLoc.LocationTypeId.ToInt();

                        if (objLoc.Latitude != null && objLoc.Latitude != 0)
                        {
                            obj.addressInfo.Latitude = objLoc.Latitude;
                            obj.addressInfo.Longitude = objLoc.Longitude;
                            latitude = objLoc.Latitude;
                            longitude = objLoc.Longitude;
                        }
                    }

                    obj.addressInfo.locTypeId = loctypeId;

                    // If still no coordinates, try lookup
                    if (latitude == null || latitude == 0)
                    {
                        var loc = db.stp_getCoordinatesByAddress(searchValue, General.GetPostCodeMatch(searchValue)).FirstOrDefault();
                        if (loc != null && loc.Latitude != 0)
                        {
                            obj.addressInfo.Latitude = loc.Latitude;
                            obj.addressInfo.Longitude = loc.Longtiude;
                            latitude = loc.Latitude;
                            longitude = loc.Longtiude;
                        }
                    }

                    // Assign zone info
                    obj.addressInfo.zoneId = General.GetZoneId(searchValue);
                    if (obj.addressInfo.zoneId.ToInt() > 0)
                    {
                        obj.addressInfo.zoneName = db.Gen_Zones
                            .Where(c => c.Id == obj.addressInfo.zoneId)
                            .Select(c => c.ZoneName)
                            .FirstOrDefault();
                    }

                    // Get nearest drivers (core logic)
                    if (latitude != null && latitude != 0)
                    {
                        try
                        {
                            var ListofAvailDrvs = (from a in db.GetTable<Fleet_DriverQueueList>()
                                                   where a.Status == true && a.DriverWorkStatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE
                                                   join b in db.GetTable<Fleet_Driver_Location>() on a.DriverId equals b.DriverId
                                                   join d in db.GetTable<Fleet_Driver>() on a.DriverId equals d.Id
                                                   where b.Latitude != 0
                                                   select new
                                                   {
                                                       a.DriverId,
                                                       d.DriverNo,
                                                       b.LocationName,
                                                       b.Latitude,
                                                       b.Longitude
                                                   }).ToList();

                            var nearestDrivers = ListofAvailDrvs
                                .Select(args => new
                                {
                                    args.DriverId,
                                    args.DriverNo,
                                    MilesAwayFromPickup = new DotNetCoords.LatLng(args.Latitude, args.Longitude)
                                        .DistanceMiles(new DotNetCoords.LatLng(Convert.ToDouble(latitude), Convert.ToDouble(longitude))),
                                    args.Latitude,
                                    args.Longitude,
                                    args.LocationName
                                })
                                .OrderBy(args => args.MilesAwayFromPickup)
                                .Take(3)
                                .ToList();

                            // Build NearestDrivers list
                            foreach (var drv in nearestDrivers)
                            {
                                string eta = General.GetETATime(drv.Latitude + "," + drv.Longitude,
                                    Convert.ToDouble(latitude) + "," + Convert.ToDouble(longitude), "").ToStr();

                                if (obj.addressInfo.NearestDrivers == null)
                                    obj.addressInfo.NearestDrivers = new List<string>();

                                obj.addressInfo.NearestDrivers.Add(
                                    "Drv " + drv.DriverNo + " - " + eta + "_LatLng" + drv.Latitude + "," + drv.Longitude + "_Id" + drv.DriverId
                                );
                            }
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetNearestDrivers_exception.txt",
                                //    DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj.addressInfo) +
                                //    ",exception:" + ex.Message + Environment.NewLine);
                                General.WriteLog("GetNearestDrivers_exception", "json:" + new JavaScriptSerializer().Serialize(obj.addressInfo) + ", Exception: " + ex.Message);
                            }
                            catch { }
                        }
                    }

                    response.Data = obj.addressInfo;
                }

                try
                {
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetNearestDrivers_response.txt",
                    //    DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj.addressInfo) + Environment.NewLine);
                    General.WriteLog("GetNearestDrivers_response", "json: " + new JavaScriptSerializer().Serialize(obj.addressInfo));
                }
                catch { }
            }
            catch (Exception ex)
            {
                response.HasError = true;
                response.Message = "exception occured: " + ex.Message;
            }

            return new CustomJsonResult { Data = response };
        }
        //public JsonResult GetNearestDrivers(WebApiClasses.RequestWebApi obj)
        //{
        //    //

        //    try
        //    {


        //        System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetNearestDrivers.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + obj.addressInfo.Address.ToStr().ToUpper().Trim() + Environment.NewLine);
        //    }
        //    catch
        //    {

        //    }

        //    ResponseWebApi response = new ResponseWebApi();

        //    try
        //    {



        //        using (TaxiDataContext db = new TaxiDataContext())
        //        {

        //            string searchValue = obj.addressInfo.Address.ToStr().ToUpper().Trim();
        //            try
        //            {
        //                var objLoc = db.Gen_Locations.FirstOrDefault(c => c.Address.ToUpper() == searchValue || c.FullLocationName.ToUpper() == searchValue);
        //                int loctypeId = Enums.LOCATION_TYPES.ADDRESS;
        //                //   double? latitude = null;
        //                //  double? longitude = null;
        //                double? latitude = obj.addressInfo.Latitude;
        //                double? longitude = obj.addressInfo.Longitude;

        //                if (objLoc != null)
        //                {
        //                    if (objLoc.LocationTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT)
        //                        loctypeId = objLoc.LocationTypeId.ToInt();



        //                    if (objLoc.Latitude != null && objLoc.Latitude != 0)
        //                    {
        //                        obj.addressInfo.Latitude = objLoc.Latitude;
        //                        obj.addressInfo.Longitude = objLoc.Longitude;
        //                        latitude = objLoc.Latitude;
        //                        longitude = objLoc.Longitude;
        //                    }
        //                }

        //                obj.addressInfo.locTypeId = loctypeId;



        //                if (latitude == null || latitude == 0)
        //                {
        //                    var loc = General.GetLocationCoordByDisplayPosition(searchValue);
        //                    //var loc1 = db.stp_getCoordinatesByAddress(searchValue, General.GetPostCodeMatch(searchValue)).FirstOrDefault();

        //                    if (loc != null && loc.Latitude != 0)
        //                    {

        //                        obj.addressInfo.Latitude = loc.Latitude;
        //                        //obj.addressInfo.Longitude = loc.Longtiude;
        //                        obj.addressInfo.Longitude = loc.Longitude;

        //                        latitude = loc.Latitude;
        //                        //longitude = loc.Longtiude;
        //                        longitude = loc.Longitude;
        //                    }

        //                }


        //                obj.addressInfo.zoneId = General.GetZoneId(searchValue);

        //                if (obj.addressInfo.zoneId.ToInt() > 0)
        //                {
        //                    obj.addressInfo.zoneName = db.Gen_Zones.Where(c => c.Id == obj.addressInfo.zoneId).Select(c => c.ZoneName).FirstOrDefault();

        //                }

        //                if (latitude != null && latitude != 0)
        //                {
        //                    try
        //                    {

        //                        var ListofAvailDrvs = (from a in db.GetTable<Fleet_DriverQueueList>().Where(c => c.Status == true &&
        //                                    (c.DriverWorkStatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE))
        //                                               join b in db.GetTable<Fleet_Driver_Location>().Where(c => c.Latitude != 0)

        //                                               on a.DriverId equals b.DriverId

        //                                               join d in db.GetTable<Fleet_Driver>() on a.DriverId equals d.Id

        //                                               select new
        //                                               {
        //                                                   DriverId = a.DriverId,
        //                                                   DriverNo = d.DriverNo,
        //                                                   DriverLocation = b.LocationName,
        //                                                   Latitude = b.Latitude,
        //                                                   Longitude = b.Longitude,
        //                                               }).ToList();
        //                        var nearestDrivers = ListofAvailDrvs.Select(args => new
        //                        {
        //                            args.DriverId,

        //                            MilesAwayFromPickup = new DotNetCoords.LatLng(args.Latitude, args.Longitude).DistanceMiles(new DotNetCoords.LatLng(Convert.ToDouble(latitude), Convert.ToDouble(longitude))),
        //                            args.DriverNo,
        //                            Latitude = args.Latitude,
        //                            Longitude = args.Longitude,
        //                            Location = args.DriverLocation

        //                        }).OrderBy(args => args.MilesAwayFromPickup)
        //                        .Take(3).ToList();

        //                        for (int i = 0; i < nearestDrivers.Count; i++)
        //                        {
        //                            string time = string.Empty;

        //                            time = General.GetETATime(nearestDrivers[i].Latitude + "," + nearestDrivers[i].Longitude, Convert.ToDouble(latitude) + "," + Convert.ToDouble(longitude), "").ToStr();

        //                            if (obj.addressInfo.NearestDrivers == null)
        //                                obj.addressInfo.NearestDrivers = new List<string>();

        //                            obj.addressInfo.NearestDrivers.Add("Drv " + nearestDrivers[i].DriverNo + " - " + time + "_LatLng" + nearestDrivers[i].Latitude + "," + nearestDrivers[i].Longitude + "_Id" + nearestDrivers[i].DriverId);

        //                        }
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        try
        //                        {
        //                            System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetAddressDetails_exceptionnearestdriver.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj.addressInfo) + ",exception:" + ex.Message + Environment.NewLine);
        //                        }
        //                        catch
        //                        {

        //                        }
        //                    }
        //                }
        //                response.Data = obj.addressInfo;
        //                //
        //            }
        //            catch (Exception ex)
        //            {

        //            }
        //        }
        //        try
        //        {
        //            System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetAddressDetails_response.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj.addressInfo) + Environment.NewLine);
        //        }
        //        catch
        //        {

        //        }
        //    }
        //    catch
        //    {
        //        response.HasError = true;
        //        response.Message = "exception occured";
        //    }
        //    return new CustomJsonResult { Data = response };
        //}

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetQuotationBookingList")]
        public JsonResult GetQuotationBookingList()
        {
            List<WebApiClasses.ClsOnlineBooking> list = null;
            ResponseWebApi response = new ResponseWebApi();
            try
            {

                //
                using (TaxiDataContext db = new TaxiDataContext())
                {
                    DateTime? dt = DateTime.Now.ToDateorNull();
                    DateTime recentDays = dt.Value.AddDays(-1);

                    //
                    list = (from a in db.Bookings
                            join bt in db.BookingTypes on a.BookingTypeId equals bt.Id
                            join b in db.Gen_PaymentTypes on a.PaymentTypeId equals b.Id
                            join c in db.Gen_Companies on a.CompanyId equals c.Id into table2
                            join v in db.Fleet_VehicleTypes on a.VehicleTypeId equals v.Id
                            from c in table2.DefaultIfEmpty()
                            where (a.PickupDateTime >= recentDays)
                            && a.IsQuotation == true && a.BookingStatusId == Enums.BOOKINGSTATUS.WAITING

                            select new WebApiClasses.ClsOnlineBooking
                            {
                                Id = a.Id,
                                BookingNo = a.BookingNo,
                                BookingDate = a.BookingDate,
                                //BookingDateString = a.BookingDate.HasValue ? "" : a.BookingDate.Value.ToString("dd-MMM-yyyy"),
                                PickupDateTime = a.PickupDateTime,
                                //PickupDateString = a.PickupDateTime.HasValue? "" : a.PickupDateTime.Value.ToString("dd-MMM-yyyy"),
                                //PickupTimeString = a.PickupDateTime.HasValue? "" : a.PickupDateTime.Value.ToString("HH:mm"),
                                CustomerName = a.CustomerName,
                                CustomerEmail = a.CustomerEmail,
                                CustomerMobileNo = a.CustomerMobileNo,
                                CustomerPhoneNo = a.CustomerPhoneNo,
                                CompanyPrice = a.CompanyPrice,
                                Extra = a.ExtraDropCharges,
                                FareRate = a.FareRate,
                                Parking = a.CongtionCharges,
                                Waiting = a.MeetAndGreetCharges,

                                FromAddress = a.FromAddress,
                                FromDoorNo = a.FromDoorNo,
                                FromStreet = a.FromStreet,
                                ToAddress = a.ToAddress,
                                ToDoorNo = a.ToDoorNo,

                                ToStreet = a.ToStreet,
                                BookingStatusId = a.BookingStatusId,
                                BookingTypeId = a.BookingTypeId,
                                CompanyName = c.CompanyName,
                                VehicleType = v.VehicleType,
                                ViaString = a.ViaString,
                                PaymentType = b.PaymentType,
                                SpecialRequirements = a.SpecialRequirements,
                                FlightNumber = a.FromFlightNo,
                                PaymentComments = a.PaymentComments,
                                BookingType = bt.BookingTypeName,
                                OrderNo = a.OrderNo,
                                UserName = a.AddLog

                            }).ToList();
                }
                response.Data = list;
            }
            catch (Exception e)
            {
                response.Message = "Some Error Occured while retrieving list";
                response.HasError = true;
            }
            return new CustomJsonResult { Data = response };
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("UpdateQuotationBookingStatus")]
        public JsonResult UpdateQuotationBookingStatus(WebApiClasses.ClsQuotationBooking obj)
        {
            ResponseWebApi response = new ResponseWebApi();
            try
            {
                General.WriteLog("UpdateQuotationBookingStatus", "json: " + new JavaScriptSerializer().Serialize(obj));
                //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\UpdateQuotationBookingStatus.txt", DateTime.Now + ",json: " + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
            }
            catch
            {
            }
            try
            {

                using (TaxiDataContext db = new TaxiDataContext())
                {
                    string Username = obj.UserName == "" ? "Controller" : obj?.UserName;
                    if (!obj.IsQuotation)
                    {
                        db.ExecuteQuery<int>("exec stp_UpdateQuotationJobStatus {0},{1},{2},{3},{4},{5}", obj.BookingId, Enums.BOOKINGSTATUS.WAITING, obj.IsQuotation, "Quotation", "Quotation Booking Confirmed", Username);
                        response.Message = "Quotation Booking Confirmed";
                    }
                    else
                    {

                        db.ExecuteQuery<int>("exec stp_UpdateQuotationJobStatus {0},{1},{2},{3},{4},{5}", obj.BookingId, Enums.BOOKINGSTATUS.CANCELLED, 0, "Quotation", "Quotation Booking Cancelled", Username);
                        response.Message = "Quotation Booking Cancelled";
                    }
                    response.HasError = false;
                    response.Message = "Booking Updated";
                    General.BroadCastMessage("**refresh required dashboard");
                }


            }
            catch (Exception ex)
            {
                try
                {
                    General.WriteLog("UpdateQuotationBookingStatus_Exception", "json:" + new JavaScriptSerializer().Serialize(obj) + ", Exception: " + ex.Message);
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\UpdateQuotationBookingStatus_Exception.txt", DateTime.Now + ",json: " + ex.Message + Environment.NewLine);
                }
                catch
                {
                }
                response.HasError = true;
                response.Message = ex.Message;
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetOnlineAddressData")]
        public JsonResult GetOnlineAddressData(WebApiClasses.RequestWebApi obj)
        {
            //

            try
            {

                General.WriteLog("GetOnlineAddressData", "json: " + new JavaScriptSerializer().Serialize(obj.addressInfo.Address.ToStr().ToUpper().Trim()));
                //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetOnlineAddressData.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + obj.addressInfo.Address.ToStr().ToUpper().Trim() + Environment.NewLine);
            }
            catch
            {

            }

            ResponseWebApi response = new ResponseWebApi();

            try
            {
                string postCode = obj.addressInfo.searchText.ToStr().ToUpper().Trim();
                double radius = 2500;
                PlaceSearchResponse SearchPlaces = new PlaceSearchResponse();
                using (TaxiDataContext db = new TaxiDataContext())
                {

                    try
                    {
                        string baseaddress = db.Gen_SysPolicy_Configurations.Select(x => x.BaseAddress).FirstOrDefault().ToStr();
                        stp_getCoordinatesByAddressResult baseCoordinates = null;
                        baseCoordinates = db.stp_getCoordinatesByAddress(baseaddress, General.GetPostCodeMatch(baseaddress)).FirstOrDefault();

                        if (baseCoordinates != null && baseCoordinates.Latitude.HasValue && baseCoordinates.Longtiude.HasValue)
                        {
                            SearchPlaces = GetDistance.SearchPlaces(postCode, new GetDistance.Coords() { Latitude = baseCoordinates.Latitude.Value, Longitude = baseCoordinates.Longtiude.Value }, radius);

                            if (SearchPlaces != null && SearchPlaces.Status == "OK")
                            {
                                if (SearchPlaces.Result.Count > 0)
                                {
                                    response.Data = SearchPlaces.Result;

                                    //  Run inserts in background thread
                                    Task.Run(() =>
                                    {
                                        using (TaxiDataContext dbs = new TaxiDataContext())
                                        {
                                            foreach (var p in SearchPlaces.Result)
                                            {
                                                var formattedAddress = p.Formatted_address?.Trim();
                                                var postcode = General.GetPostCodeMatch(formattedAddress);
                                                double? lat = p.Geometry?.Location != null ? Convert.ToDouble(p.Geometry.Location.Lat) : (double?)null;
                                                double? lng = p.Geometry?.Location != null ? Convert.ToDouble(p.Geometry.Location.Lng) : (double?)null;

                                                bool exists = dbs.Gen_Locations.Any(x =>
                                                    x.Address == formattedAddress ||
                                                    x.FullLocationName == (p.Name + " " + formattedAddress) ||
                                                    (x.Latitude == lat && x.Longitude == lng) ||
                                                    (x.PostCode == postcode && x.LocationName == p.Name)
                                                );

                                                if (exists)
                                                    continue;

                                                var entity = new Gen_Location();

                                                entity.LocationName = p.Name;
                                                entity.Address = formattedAddress;
                                                entity.PostCode = postcode;

                                                if (lat != null && lng != null)
                                                {
                                                    entity.Latitude = lat;
                                                    entity.Longitude = lng;
                                                }

                                                entity.FullLocationName = p.Name + " " + formattedAddress;
                                                entity.AddOn = DateTime.Now;
                                                entity.AddBy = obj.UserId;

                                                dbs.Gen_Locations.InsertOnSubmit(entity);
                                            }

                                            dbs.SubmitChanges();
                                        }
                                    });

                                 
                                }
                            }

                        }
                        //
                    }
                    catch (Exception ex)
                    {

                    }
                }
                try
                {
                    General.WriteLog("GetOnlineAddressData", "json: " + new JavaScriptSerializer().Serialize(obj.addressInfo));
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetOnlineAddressData.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj.addressInfo) + Environment.NewLine);
                }
                catch
                {

                }
            }
            catch
            {
                response.HasError = true;
                response.Message = "exception occured";
            }
            return new CustomJsonResult { Data = response };
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("SendSMSJobDetails")]
        public JsonResult SendSMSJobDetails(WebApiClasses.RequestWebApi obj)
        {
            try
            {
                General.WriteLog("SendSMSJobDetails", "json: " + new JavaScriptSerializer().Serialize(obj));
                //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "SendSMSJobDetails.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
            }
            catch
            {
            }
            ResponseWebApi response = new ResponseWebApi();
            try
            {
                HubProcessor.Instance.listofSMS.Add("request dispatchsms = " + obj.smsInfo.MobileNo.Trim() + " = " + obj.smsInfo.Message);
                response.HasError = false;
            }
            catch (Exception ex)
            {
                try
                {
                    response.HasError = true;
                    response.Message = ex.Message;
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "SendSMSJobDetails_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message + Environment.NewLine);
                    General.WriteLog("SendSMSJobDetails_exception", "json:" + new JavaScriptSerializer().Serialize(obj) + ", Exception: " + ex.Message);
                }
                catch
                {
                }
            }
            //   return Json(response, JsonRequestBehavior.AllowGet);
            return new CustomJsonResult { Data = response };
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("DispatchAllocatedPreBooking")]
        public JsonResult DispatchAllocatedPreBooking(WebApiClasses.RequestWebApi obj)
        {
            //




            ResponseWebApi response = new ResponseWebApi();
            try
            {

                //
                try
                {
                    General.WriteLog("DispatchAllocatedPreBooking", "json: " + new JavaScriptSerializer().Serialize(obj));
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "DispatchAllocatedPreBooking.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
                }
                catch
                {

                }
                using (TaxiDataContext db = new TaxiDataContext())
                {
                    string despatchBy = obj.bookingInfo.Despatchby.ToStr();
                    string msg = "<jobcount> allocated pre jobs dispatched successfully.";
                    bool HasError = false;
                    int dispatchCounter = 0;
                    try
                    {
                        var preBookingList = db.ExecuteQuery<BookingInfo>($"Select * from Booking where PickupDateTime >= '{obj.bookingInfo.FromDate.ToDateTime().ToString("yyyy-MM-dd HH:mm")}' and PickupDateTime <= '{obj.bookingInfo.ToDate.ToDateTime().ToString("yyyy-MM-dd HH:mm")}' and ISNULL(DriverId,0) > 0 and BookingStatusId IN (1,4,14)").ToList();
                        if (preBookingList != null && preBookingList.Count > 0)
                        {
                            foreach (var preBooking in preBookingList)
                            {
                                obj.bookingInfo.DriverId = preBooking.DriverId;
                                obj.bookingInfo.Id = preBooking.Id;
                                if (db.ExecuteQuery<int>("select count(*) from fleet_driverqueuelist (nolock) where driverid=" + obj.bookingInfo.DriverId.ToInt() + " and status=1 and currentjobid=" + obj.bookingInfo.Id.ToLong()).FirstOrDefault() > 0)
                                {
                                    msg = "Job is already accepted by this driver";
                                    try
                                    {

                                        General.WriteLog("DispatchBooking_alreadyacceptedthisdriver", "jobid: " + obj.bookingInfo.Id + ",driverid:" + obj.bookingInfo.DriverId.ToInt());
                                        //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "DispatchBooking_alreadyacceptedthisdriver.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",jobid:" + obj.bookingInfo.Id + ",driverid:" + obj.bookingInfo.DriverId.ToInt() + Environment.NewLine);
                                    }
                                    catch
                                    {

                                    }


                                }
                                else if (db.ExecuteQuery<int>("select count(*) from fleet_driverqueuelist (nolock) where driverid!=" + obj.bookingInfo.DriverId.ToInt() + " and status=1 and currentjobid=" + obj.bookingInfo.Id.ToLong()).FirstOrDefault() > 0)
                                {

                                    msg = "Job is already accepted by other driver";
                                    try
                                    {

                                        General.WriteLog("DispatchBooking_alreadyacceptedotherdriver", "jobid: " + obj.bookingInfo.Id + ", driverid: " + obj.bookingInfo.DriverId.ToInt());
                                        //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "DispatchBooking_alreadyacceptedotherdriver.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",jobid:" + obj.bookingInfo.Id + ",driverid:" + obj.bookingInfo.DriverId.ToInt() + Environment.NewLine);
                                    }
                                    catch
                                    {

                                    }

                                }
                                else
                                {
                                    try
                                    {
                                        General.OnDespatching(HubProcessor.Instance.objPolicy, db.Bookings.FirstOrDefault(c => c.Id == obj.bookingInfo.Id), db.Fleet_Drivers.FirstOrDefault(c => c.Id == obj.bookingInfo.DriverId), obj.bookingInfo.BookingTypeId.ToInt(), false, despatchBy);
                                        dispatchCounter += 1;
                                    }
                                    catch
                                    {
                                    }


                                }
                            }
                        }
                        else
                        {
                            msg = "No allocated pre job found.";
                            HasError = true;
                        }
                    }
                    catch
                    {

                    }


                    response.HasError = HasError;
                    response.Message = msg.Replace("<jobcount>", dispatchCounter.ToStr());
                }

                response.Data = "DispatchAllocatedPreBooking Executed";
            }
            catch (Exception ex)
            {
                try
                {
                    response.HasError = true;
                    response.Message = ex.Message;

                    General.WriteLog("DispatchAllocatedPreBooking_exception", "json:" + new JavaScriptSerializer().Serialize(obj) + ", Exception: " + ex.Message);
                    //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "DispatchAllocatedPreBooking_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
            }


            return Json(response, JsonRequestBehavior.AllowGet);

        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetAllMessages")]
        public JsonResult GetAllMessages(WebApiClasses.RequestWebApi obj)
        {
            try
            {
                // Log incoming request
                //System.IO.File.AppendAllText(
                //    AppContext.BaseDirectory + "\\GetAllMessages.txt",
                //    DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") +
                //    ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine
                //);
                General.WriteLog("GetAllMessages", "json: " + new JavaScriptSerializer().Serialize(obj));
            }
            catch
            {
                // ignore logging failure
            }

            ResponseWebApi response = new ResponseWebApi();

            try
            {
                using (TaxiDataContext db = new TaxiDataContext())
                {
                    // The MessageType (Inbox/Outbox) will be passed via obj.MessageType
                    string messageType = obj.MessageType ?? "";


                    if (messageType == "Inbox")
                    {
                        var data = (from a in General.GetQueryable<Taxi_Model.Message>(c => c.MessageType != null && c.MessageType == "Inbox")

                                    select new
                                    {
                                        Id = a.Id,
                                        SenderId = a.SenderId,
                                        Name = a.SenderName,
                                        Message = a.MessageBody,
                                        Time = a.MessageCreatedOn,
                                        SendFrom = a.SendFrom,
                                        MessageType = a.MessageType

                                    }).OrderByDescending(c => c.Id).Take(1000).ToList();
                        response.Data = data;
                    }
                    else
                    {
                        var data = (from a in General.GetQueryable<Taxi_Model.Message>(c => c.SendFrom != null && c.SendFrom == "pda")

                                    select new
                                    {
                                        Id = a.Id,
                                        SenderId = a.SenderId,
                                        Name = a.SenderName,
                                        Message = a.MessageBody,
                                        Time = a.MessageCreatedOn,
                                        SendFrom = a.SendFrom,
                                        MessageType = a.MessageType


                                    }).OrderByDescending(c => c.Id).Take(1000).ToList();
                        response.Data = data;
                    }




                    response.HasError = false;
                    response.Message = "Success";
                }
            }
            catch (Exception ex)
            {
                response.HasError = true;
                response.Message = ex.Message;

                try
                {
                    //System.IO.File.AppendAllText(
                    //    AppContext.BaseDirectory + "\\GetAllMessages_exception.txt",
                    //    DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") +
                    //    ",json:" + new JavaScriptSerializer().Serialize(obj) +
                    //    ",exception:" + ex.Message + Environment.NewLine
                    //);
                    General.WriteLog("GetAllMessages_exception", "json:" + new JavaScriptSerializer().Serialize(obj) + ", Exception: " + ex.Message);
                }
                catch
                {
                    // ignore logging failure
                }
            }

            return new CustomJsonResult { Data = response };
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetAllSentMessages")]
        public JsonResult GetAllSentMessages(WebApiClasses.RequestWebApi obj)
        {
            try
            {
                // Log incoming request
                //System.IO.File.AppendAllText(
                //    AppContext.BaseDirectory + "\\GetAllMessages.txt",
                //    DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") +
                //    ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine
                //);
                General.WriteLog("GetAllMessages", "json: " + new JavaScriptSerializer().Serialize(obj));
            }
            catch
            {
                // ignore logging failure
            }

            ResponseWebApi response = new ResponseWebApi();

            try
            {
                DateTime filterDate = DateTime.Now.AddDays(-90).Date;

                using (TaxiDataContext db = new TaxiDataContext())
                {
                    var data = db.SentSMs
                        .Where(c => c.SentOn >= filterDate)
                        .Select(c => new
                        {
                            Id = c.Id,
                            To = c.SentTo,
                            Message = c.SMSBody,
                            SentOn = c.SentOn,
                            By = c.SentBy
                        })
                        .OrderByDescending(c => c.SentOn)
                        .Take(1000)
                        .ToList();

                    response.Data = data;
                    response.HasError = false;
                    response.Message = "Success";
                }
            }
            catch (Exception ex)
            {
                response.HasError = true;
                response.Message = ex.Message;

                try
                {
                    //System.IO.File.AppendAllText(
                    //    AppContext.BaseDirectory + "\\GetAllMessages_exception.txt",
                    //    DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") +
                    //    ",json:" + new JavaScriptSerializer().Serialize(obj) +
                    //    ",exception:" + ex.Message + Environment.NewLine
                    //);
                    General.WriteLog("GetAllMessages_exception", "json:" + new JavaScriptSerializer().Serialize(obj) + ", Exception: " + ex.Message);
                }
                catch
                {
                    // ignore logging failure
                }
            }

            return new CustomJsonResult { Data = response };
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetAllControllerMessages")]
        public JsonResult GetAllControllerMessages(WebApiClasses.RequestWebApi obj)
        {
            // Initialize response object
            ResponseWebApi response = new ResponseWebApi();

            try
            {
                // Optional logging
                try
                {
                    //System.IO.File.AppendAllText(
                    //    AppContext.BaseDirectory + "\\GetAllControllerMessages.txt",
                    //    DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") +
                    //    ",json:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine
                    //);
                    General.WriteLog("GetAllControllerMessages", "json: " + new JavaScriptSerializer().Serialize(obj));
                }
                catch { }

                // Validate input
                if (obj == null || string.IsNullOrEmpty(obj.Type))
                {
                    response.HasError = true;
                    response.Message = "Invalid request";
                    return new CustomJsonResult { Data = response };
                }

                using (TaxiDataContext db = new TaxiDataContext())
                {
                    List<object> messages = new List<object>();

                    if (obj.Type.Equals("sent", StringComparison.OrdinalIgnoreCase))
                    {
                        // Sent messages for logged-in user
                        messages = (from a in db.InternalMessagings
                                    where a.SenderName.ToLower() == obj.UserName.ToLower() // Use obj.UserName instead of AppVars
                                    orderby a.AddOn descending
                                    select new
                                    {
                                        Id = a.Id,
                                        Recipient = a.ReceiveTo != null ? db.UM_Users.FirstOrDefault(u => u.Id == a.ReceiveTo).UserName : "",
                                        MessageText = a.MessageText,
                                        DateTime = a.AddOn
                                    }).ToList<object>();
                    }
                    else
                    {
                        // Inbox messages for logged-in user
                        int? userId = obj.UserId; // make sure RequestWebApi has UserId
                        messages = (from a in db.InternalMessagings
                                    join b in db.UM_Users on a.SenderName equals b.UserName
                                    where a.ReceiveTo == userId
                                    orderby a.AddOn descending
                                    select new
                                    {
                                        Id = a.Id,
                                        Sender = a.SenderName,
                                        MessageText = a.MessageText,
                                        DateTime = a.AddOn
                                    }).ToList<object>();
                    }

                    response.Data = messages;
                    response.HasError = false;
                    response.Message = "Success";
                }
            }
            catch (Exception ex)
            {
                response.HasError = true;
                response.Message = ex.Message;

                try
                {
                    //System.IO.File.AppendAllText(
                    //    AppContext.BaseDirectory + "\\GetAllControllerMessages_exception.txt",
                    //    DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") +
                    //    ",json:" + new JavaScriptSerializer().Serialize(obj) +
                    //    ",exception:" + ex.Message + Environment.NewLine
                    //);
                    General.WriteLog("GetAllControllerMessages_exception", "json:" + new JavaScriptSerializer().Serialize(obj) + ", Exception: " + ex.Message);
                }
                catch { }
            }

            return new CustomJsonResult { Data = response };
        }



    }
}