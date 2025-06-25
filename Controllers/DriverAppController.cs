using DotNetCoords;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Taxi_BLL;
using Taxi_Model;
using Utils;

using System.Text;
using System.Security.Claims;
using System.Drawing;
using System.Net;
using System.Threading;
using System.Data;
using System.Xml;
using System.Configuration;
using SignalRHub.Classes;
using System.Net.Http;
using SignalRHub.Classes.KonnectSupplier;
using System.Threading.Tasks;

namespace SignalRHub
{
    public class ThirdPartyViewModelAdv
    {



        public string DispatchTripID { get; set; }
        public string SupplierTripID { get; set; }
        public string SupplierID { get; set; }

        public string FromUser { get; set; }
        public string ToUser { get; set; }
        public string MessageText { get; set; }
    }

    [System.Web.Http.AllowAnonymous]
    [System.Web.Http.RoutePrefix("api/DriverApp")]
    public class DriverAppController : ApiController
    {

        private bool LoginDrvOnExpiredDoc = false;
        string physicalPath = AppContext.BaseDirectory;
        string physicalPathSyslogfolder = AppContext.BaseDirectory + "\\logs\\systemlogs\\";

        int LicenseDays = 0;
        int InsuranceDays = 0;
        int PHCVehicleDays = 0;
        int MOT2Days = 0;
        int MOTDays = 0;
        int PHCDriverDays = 0;



        #region Shuttle Region
        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestshuttlefuturejoblist")] //already exist, make it change by returning ResponseData model as return type, and object as parameter

        public ResponseData requestshuttlefuturejoblist(string mesg)
        {
            try
            {

                File.AppendAllText(physicalPath + "\\" + "requestshuttlefuturejoblist.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", json: " + mesg.ToString() + Environment.NewLine);
            }
            catch
            {


            }

            ResponseData res = new ResponseData();
            try
            {
                JobActionEx objAction = new JavaScriptSerializer().Deserialize<JobActionEx>(mesg.ToStr());



                int driverId = int.Parse(objAction.DrvId); //values[0].ToInt();
                string driverNo = objAction.DrvNo; //values[1].ToStr();

                List<Trip> trips = new List<Trip>();

                using (TaxiDataContext db = new TaxiDataContext())
                {
                    db.CommandTimeout = 5;



                    var triplist = db.ExecuteQuery<Trip>("select TripId=Id,TripNo=GroupName,followSequence=IsFollowsequence,DriverId=driverid from bookinggroup where driverid=" + driverId + " and tripstatusid=17").ToList();



                    foreach (var objTrip in triplist)
                    {

                        Trip trip = new Trip();
                        trip.TripStatusId = 17;





                        trip.TripId = objTrip.TripId;
                        trip.TripNo = objTrip.TripNo.ToStr();
                        trip.followSequence = objTrip.followSequence.ToBool();
                        trip.DriverId = objTrip.DriverId.ToInt();



                        trip.jobs = new List<Jobs>();

                        foreach (var item in db.Bookings.Where(c => c.GroupJobId == objTrip.TripId).ToList())
                        {
                            Jobs job = new Jobs();
                            job.Pickup = item.FromAddress.ToStr();
                            job.Destination = item.ToAddress.ToStr();
                            job.PickupDateTime = string.Format("{0:dd/MM/yyyy HH:mm}", item.PickupDateTime);
                            job.ShowFares = true;
                            job.Passengers = item.NoofPassengers.ToInt();
                            job.BookingNo = item.BookingNo.ToStr();
                            job.JobId = item.Id.ToStr();
                            job.Payment = item.Gen_PaymentType.PaymentType.ToStr();
                            job.Did = item.DriverId.ToInt();
                            job.Cust = item.CustomerName.ToStr();

                            if (item.BookingStatusId == Enums.BOOKINGSTATUS.ONROUTE)
                                job.JStatus = AppJobStatus.ONROUTE;


                            else if (item.BookingStatusId == Enums.BOOKINGSTATUS.ARRIVED)
                                job.JStatus = AppJobStatus.ARRIVED;


                            else if (item.BookingStatusId == Enums.BOOKINGSTATUS.POB)
                                job.JStatus = AppJobStatus.POB;

                            else if (item.BookingStatusId == Enums.BOOKINGSTATUS.STC)
                                job.JStatus = AppJobStatus.STC;


                            //   else  if (item.BookingStatusId == Enums.BOOKINGSTATUS.)
                            else
                                job.JStatus = AppJobStatus.ACCEPT;



                            if (item.BookingStatusId != Enums.BOOKINGSTATUS.DISPATCHED && item.BookingStatusId != Enums.BOOKINGSTATUS.NOPICKUP && item.BookingStatusId != Enums.BOOKINGSTATUS.CANCELLED)
                            {

                                trip.jobs.Add(job);

                            }
                        }



                        trips.Add(trip);
                    }






                    string response = Newtonsoft.Json.JsonConvert.SerializeObject(trips);

                    try
                    {

                        File.AppendAllText(physicalPath + "\\" + "requestshuttlefuturejoblist_response.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",response:" + response + Environment.NewLine);
                    }
                    catch
                    {


                    }




                    res.Data = response;
                    res.IsSuccess = true;
                    res.Message = "";


                }
            }
            catch (Exception ex)
            {
                //return "exceptionoccurred";
                res.Data = null;
                res.IsSuccess = false;
                res.Message = ex.Message;
            }

            return res;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("tripOfferAction")]
        public ResponseData tripOfferAction(string mesg)
        {
            ResponseData res = new ResponseData();

            string jStatus = string.Empty;

            try
            {

                //
                try
                {
                    File.AppendAllText(physicalPath + "\\tripOfferAction.txt", DateTime.Now + " : msg" + mesg + Environment.NewLine);
                }
                catch
                {

                }

                JobActionEx objAction = new JavaScriptSerializer().Deserialize<JobActionEx>(mesg);
                long tripId = objAction.TripId.ToLong();



                using (TaxiDataContext db = new TaxiDataContext())
                {

                    if (objAction.JStatus.ToStr() == "4")
                    {
                        db.ExecuteQuery<int>("update BookingGroup set TripStatusId=2 where id=" + tripId);

                        foreach (var item in db.Bookings.Where(c => c.GroupJobId == tripId).Select(c => c.Id).ToList())
                        {
                            db.stp_UpdateJob(item, objAction.DrvId.ToInt(), Enums.BOOKINGSTATUS.ONROUTE, Enums.Driver_WORKINGSTATUS.ONROUTE, 0);
                        }

                    }
                    else if (objAction.JStatus.ToStr() == "11")
                    {
                        db.ExecuteQuery<int>("update BookingGroup set TripStatusId=6 where id=" + tripId);

                        foreach (var item in db.Bookings.Where(c => c.GroupJobId == tripId).Select(c => c.Id).ToList())
                        {

                            db.stp_UpdateJob(item, objAction.DrvId.ToInt(), Enums.BOOKINGSTATUS.REJECTED, Enums.Driver_WORKINGSTATUS.AVAILABLE, 0);
                        }
                    }
                    else if (objAction.JStatus.ToStr() == "11")
                    {
                        db.ExecuteQuery<int>("update BookingGroup set TripStatusId=7 where id=" + tripId);

                        foreach (var item in db.Bookings.Where(c => c.GroupJobId == tripId).Select(c => c.Id).ToList())
                        {

                            db.stp_UpdateJob(item, objAction.DrvId.ToInt(), Enums.BOOKINGSTATUS.NOTACCEPTED, Enums.Driver_WORKINGSTATUS.AVAILABLE, 0);
                        }
                    }
                    else if (objAction.JStatus.ToStr() == "17")
                    {
                        db.ExecuteQuery<int>("update BookingGroup set TripStatusId=17 where id=" + tripId);

                        foreach (var item in db.Bookings.Where(c => c.GroupJobId == tripId).Select(c => c.Id).ToList())
                        {

                            try
                            {
                                File.AppendAllText(physicalPath + "\\tripOfferAction_prebookings.txt", DateTime.Now + " : msg" + mesg + ",jobid:" + item + Environment.NewLine);
                            }
                            catch
                            {

                            }
                            db.stp_UpdateJobStatus(item, Enums.BOOKINGSTATUS.PENDING_START);
                        }
                    }


                }

                General.BroadCastMessage("**refresh required dashboard");

                res.Data = "";
                res.IsSuccess = true;
                res.Message = "";



            }
            catch (Exception ex)
            {
                try
                {
                    //                    
                    //if (jStatus == Enums.BOOKINGSTATUS.DISPATCHED.ToStr().ToLower())
                    //    Clients.Caller.jobCleared("exceptionoccurred");
                    //else
                    //    Clients.Caller.manualFares("exceptionoccurred");
                    ///------------------------------------------------------------------------
                    ///
                    res.Data = null;
                    res.IsSuccess = true;
                    res.Message = "exceptionoccurred";


                    File.AppendAllText(physicalPath + "\\tripOfferAction_exception.txt", DateTime.Now + ": datavalue=" + mesg + ",exception:" + ex.Message + Environment.NewLine);


                }
                catch
                {

                    //
                }

            }

            return res;
        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("preShuttleStart")]
        public ResponseData preShuttleStart(string mesg)
        {
            ResponseData res = new ResponseData();

            string jStatus = string.Empty;

            try
            {

                //
                try
                {
                    File.AppendAllText(physicalPath + "\\preShuttleStart.txt", DateTime.Now + " : msg" + mesg + Environment.NewLine);
                }
                catch
                {

                }


                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                int valCnt = values.Count();

                long tripId = values[1].ToLong();
                int driverId = values[2].ToInt();




                using (TaxiDataContext db = new TaxiDataContext())
                {


                    db.ExecuteQuery<int>("update BookingGroup set TripStatusId=2 where id=" + tripId);

                    foreach (var item in db.Bookings.Where(c => c.GroupJobId == tripId).Select(c => c.Id).ToList())
                    {

                        db.stp_UpdateJob(item, driverId.ToInt(), Enums.BOOKINGSTATUS.ONROUTE, Enums.Driver_WORKINGSTATUS.ONROUTE, 0);
                    }




                }

                General.BroadCastMessage("**refresh required dashboard");

                res.Data = "true";
                res.IsSuccess = true;
                res.Message = "";



            }
            catch (Exception ex)
            {
                try
                {
                    //                    
                    //if (jStatus == Enums.BOOKINGSTATUS.DISPATCHED.ToStr().ToLower())
                    //    Clients.Caller.jobCleared("exceptionoccurred");
                    //else
                    //    Clients.Caller.manualFares("exceptionoccurred");
                    ///------------------------------------------------------------------------
                    ///
                    res.Data = null;
                    res.IsSuccess = true;
                    res.Message = "exceptionoccurred";


                    File.AppendAllText(physicalPath + "\\tripOfferAction_exception.txt", DateTime.Now + ": datavalue=" + mesg + ",exception:" + ex.Message + Environment.NewLine);


                }
                catch
                {

                    //
                }

            }

            return res;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestBookingStatus")]
        public ResponseData requestBookingStatus(string mesg)
        {
            ResponseData res = new ResponseData();

            try
            {

                try
                {

                    File.AppendAllText(physicalPath + "\\requestBookingStatus.txt", DateTime.Now.ToStr() + " mesg:" + mesg + Environment.NewLine);
                }
                catch
                {

                }

                var jobaction = new JavaScriptSerializer().Deserialize<JobAction>(mesg);


                var data = new { IsJobAvailable = "", JobMessage = "" };
                long jobId = jobaction.JobId.ToLong();
                int driverId = jobaction.DrvId.ToInt();
                using (TaxiDataContext db = new TaxiDataContext())
                {
                    try
                    {
                        var obj = db.Bookings.Where(c => c.Id == jobId).Select(args => new { args.DriverId, args.BookingStatusId }).FirstOrDefault();

                        if (obj != null)
                        {
                            if (obj.DriverId == null || obj.DriverId != driverId)
                                data = new { IsJobAvailable = "0", JobMessage = "Job is recovered" };
                            else if (obj.DriverId == driverId && (obj.BookingStatusId == Enums.BOOKINGSTATUS.PENDING || obj.BookingStatusId == Enums.BOOKINGSTATUS.ONROUTE || obj.BookingStatusId == Enums.BOOKINGSTATUS.ARRIVED || obj.BookingStatusId == Enums.BOOKINGSTATUS.POB || obj.BookingStatusId == Enums.BOOKINGSTATUS.STC))
                            {
                                data = new { IsJobAvailable = "1", JobMessage = "" };
                            }
                            else if (obj.DriverId == driverId && (obj.BookingStatusId == Enums.BOOKINGSTATUS.CANCELLED || obj.BookingStatusId == Enums.BOOKINGSTATUS.NOPICKUP))
                            {
                                data = new { IsJobAvailable = "0", JobMessage = "Job is cancelled by controller" };
                            }
                            else
                                data = new { IsJobAvailable = "0", JobMessage = "Job is recovered" };
                        }
                    }
                    catch
                    {
                        data = new { IsJobAvailable = "1", JobMessage = "" };
                    }

                    res.IsSuccess = true;
                    res.Data = new JavaScriptSerializer().Serialize(data);
                    //
                    try
                    {

                        File.AppendAllText(physicalPath + "\\requestBookingStatusresponse.txt", DateTime.Now.ToStr() + " request:" + mesg + Environment.NewLine + " response:" + res.Data + Environment.NewLine);
                    }
                    catch
                    {

                    }
                }
            }
            catch
            {

                res.IsSuccess = false;
            }


            return res;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestTripAction")]
        public ResponseData requestTripAction(string mesg)
        {
            ResponseData res = new ResponseData();

            string jStatus = string.Empty;

            try
            {

                try
                {
                    File.AppendAllText(physicalPath + "\\requestTripAction.txt", DateTime.Now + " : msg" + mesg + Environment.NewLine);
                }
                catch
                {

                }


                JobActionEx objAction = new JavaScriptSerializer().Deserialize<JobActionEx>(mesg);

                int jstatus = objAction.JStatus.ToInt();


                using (TaxiDataContext db = new TaxiDataContext())
                {
                    if (jstatus == Enums.BOOKINGSTATUS.NOPICKUP)
                    {
                        actionButton("jaction=" + objAction.JobId.ToStr() + "=" + objAction.DrvId.ToStr() + "=" + objAction.JStatus.ToStr() + "=" + objAction.DStatus.ToStr() + "=" + objAction.Dropoff.ToStr() + "=" + objAction.version.ToStr() + "=" + objAction.PickupDateTime.ToStr());
                        //    (new TaxiDataContext()).stp_UpdateJobAndRoute(values[1].ToLong(), values[2].ToInt(), values[3].ToInt(), values[4].ToInt(), "", values[5].ToDecimal(), null);
                    }
                    else if (jstatus == Enums.BOOKINGSTATUS.ARRIVED)
                    {
                        requestarrive(mesg);


                    }
                    else if (jstatus == Enums.BOOKINGSTATUS.POB)
                    {
                        requestPOB(objAction.DrvId + "=" + objAction.JobId.ToStr() + "=" + objAction.DrvId + "=7=2=2=5=3=2=4");
                        //      (new TaxiDataContext()).stp_UpdateJob(values[1].ToLong(), values[2].ToInt(), values[3].ToInt(), values[4].ToInt(), HubProcessor.Instance.objPolicy.SinBinTimer.ToInt());

                        //
                    }
                    else if (jstatus == Enums.BOOKINGSTATUS.STC)
                    {
                        requestSTC(mesg);


                    }
                    else if (jstatus == Enums.BOOKINGSTATUS.DISPATCHED)
                    {

                        if (objAction.DStatus.ToStr() == "7")
                            objAction.DStatus = "2";
                        else if (objAction.DStatus.ToStr() == "8")
                            objAction.DStatus = "5";

                        requestClearJob("jaction=" + new JavaScriptSerializer().Serialize(objAction));

                        //
                        if (objAction.DStatus.ToStr() == "1")
                        {

                            db.ExecuteQuery<int>("update BookingGroup set TripStatusId=" + Enums.BOOKING_TRIPSTATUS.COMPLETED + " where id=" + objAction.TripId.ToLong());

                        }
                        //
                    }
                }

                //
                res.Data = "";
                res.IsSuccess = true;
                res.Message = "";



            }
            catch (Exception ex)
            {
                try
                {
                    //                    //
                    //if (jStatus == Enums.BOOKINGSTATUS.DISPATCHED.ToStr().ToLower())
                    //    Clients.Caller.jobCleared("exceptionoccurred");
                    //else
                    //    Clients.Caller.manualFares("exceptionoccurred");
                    ///------------------------------------------------------------------------
                    ///
                    res.Data = null;
                    res.IsSuccess = true;
                    res.Message = "exceptionoccurred";


                    File.AppendAllText(physicalPath + "\\requestTripAction_exception.txt", DateTime.Now + ": datavalue=" + mesg + ",exception:" + ex.Message + Environment.NewLine);


                }
                catch
                {

                    //
                }

            }

            return res;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("VerifyAuthCode")]
        public ResponseData VerifyAuthCode(string authcode)
        {


            try
            {
                //
                File.AppendAllText(AppContext.BaseDirectory + "\\VerifyAuthCode.txt", DateTime.Now.ToStr() + " request" + authcode + Environment.NewLine);

            }
            catch
            {

            }

            string message = authcode;



            ResponseData res = new ResponseData();



            //
            try
            {
                ///DriverDetail objDriverDetail = Newtonsoft.Json.JsonConvert.DeserializeObject<DriverDetail>(mesg);

                //update settings in json Format 
                int driverId = 0;

                using (TaxiDataContext db = new TaxiDataContext())
                {
                    try
                    {
                        if (message.ToStr().Trim().Length > 0)
                            driverId = db.ExecuteQuery<int>("select Id from fleet_driver where loginid='" + message + "' and isactive=1").FirstOrDefault();



                    }
                    catch { }




                }

                if (driverId == 0)
                {
                    res.IsSuccess = false;
                    res.Message = "Invalid Pin Code";
                }
                else
                {

                    res = requestDriverSettings("request driver settings=" + driverId + "=105.26");

                }




            }
            catch (Exception ex)
            {
                res.Data = null;
                res.IsSuccess = false;
                //  res.Message = ex.Message;


                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\requestDriverSettings_exception.txt", DateTime.Now.ToStr() + " request" + message + ",exception:" + ex.Message + Environment.NewLine);

                }
                catch
                {

                }
            }

            return res;
        }


        #endregion

        #region socket region

        private void LoadDataList(bool forceRefresh = false)
        {
            try
            {
                if (Instance.objPolicy == null || forceRefresh)
                    Instance.objPolicy = General.GetObject<Gen_SysPolicy_Configuration>(c => c.SysPolicyId == 1);

                if (Instance.listofPolyVertices == null || forceRefresh)
                {
                    Instance.listofPolyVertices = General.GetQueryable<Gen_Zone_PolyVertice>(c => c.ZoneId != 0).ToList();
                }

                if (Instance.listOfZone == null || forceRefresh)
                {
                    Instance.listOfZone = General.GetQueryable<Gen_Zone>(c => c.ShapeCategory != null)
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
                                    PlotKind = args.PlotKind
                                }).ToList();
                }


                if (Global.listofMeter == null || forceRefresh)
                {

                    ReloadMeterList();
                    //
                }

                ////

                try
                {
                    if (forceRefresh)
                        File.AppendAllText(physicalPath + "\\loaddatalist.txt", DateTime.Now.ToStr() + "," + "forcerefresh :" + forceRefresh.ToStr() + Environment.NewLine);

                }
                catch
                {

                }
            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(physicalPath + "\\loaddatalist_exception.txt", DateTime.Now.ToStr() + "," + "forcerefresh :" + forceRefresh.ToStr() + " , exception : " + ex.Message.ToStr() + Environment.NewLine);
                    //

                }
                catch
                {

                }

            }

        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("SendLocationDataApi")]
        public ResponseData SendLocationDataApi(string request)
        {
            ResponseData resp = new ResponseData();
            try
            {

                //    File.AppendAllText(physicalPath + "\\" + "SendLocationDataApiactual.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + request + Environment.NewLine);

                if (request.ToStr().Contains("\\u003d"))
                    request = request.Replace("\\u003d", "=");


                //try
                //{
                //    if (request.StartsWith("{\"Method\": \"SendLocationData\", \"Data\": { \"PlotBidding") && request.EndsWith(" } }"))
                //    {
                //        request = request.Remove(request.IndexOf(", \"GpsError"));

                //        request += "} }";


                //    }


                //    if (request.EndsWith("\"\"} }"))
                //    {
                //        request = request.Replace("\"\"} }", "\" } }");

                //    }
                //}
                //catch
                //{

                //}

                SocketRequest json = Newtonsoft.Json.JsonConvert.DeserializeObject<SocketRequest>(request);
                //  json.Data.IsSocket = true;

                if (json.Method == "requestPlotsBidding")
                {

                    string[] values = json.Data.ToStr().Split(new char[] { '=' });

                    ResponseData data = requestPlotsBidding(json.Data.ToStr());


                    //try
                    //{


                    //    if (values[0].ToStr() == "219")
                    //        File.AppendAllText(physicalPath + "\\" + "CallPlotBidding.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",request:" + json.Data.ToStr() + Environment.NewLine + "response:" + data.Data.ToStr() + Environment.NewLine);
                    //}
                    //catch
                    //{

                    //}

                    SocketIO.SendToSocket(values[0].ToStr(), data.Data, "plotsBiddings");
                }
                else if (json.Method == "forcereconnectsocket")
                {

                    //

                    try
                    {
                        File.AppendAllText(physicalPath + "\\" + "forcereconnectsocket.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + json.Data.ToStr() + Environment.NewLine);
                    }
                    catch
                    {

                    }

                }
                else
                {
                    //

                    SocketData json2 = Newtonsoft.Json.JsonConvert.DeserializeObject<SocketData>(request);


                    //try
                    //{
                    //    File.AppendAllText(physicalPath + "\\" + "SendLocationDataApi.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + request.ToStr() + Environment.NewLine);
                    //}
                    //catch
                    //{

                    //}


                    SendLocationData(json2.Data);
                }

                //

            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(physicalPath + "\\" + "SendLocationDataApi_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + request + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                { }
            }
            return resp;
        }
        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("Sendacknowledgement")]
        public ResponseData Sendacknowledgement(string request)
        {
            ResponseData resp = new ResponseData();
            try
            {






                SocketAck json = Newtonsoft.Json.JsonConvert.DeserializeObject<SocketAck>(request);


                if (HubProcessor.Instance.listofJobs.Count(C => C.Id == json.Id && C.DriverId == json.driverId.ToInt()) > 0)
                {


                    try
                    {
                        HubProcessor.Instance.listofJobs.RemoveAll(C => C.Id == json.Id && C.DriverId == json.driverId.ToInt());


                        File.AppendAllText(physicalPath + "\\" + "SendacknowledgementIDFound.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + request + Environment.NewLine);
                    }
                    catch
                    {

                    }



                }


                //  File.AppendAllText(physicalPath + "\\" + "Sendacknowledgement.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + request + Environment.NewLine);
            }
            catch (Exception ex)
            {
                //try
                //{
                //    File.AppendAllText(physicalPath + "\\" + "Sendacknowledgement_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + ex.Message + Environment.NewLine);
                //}
                //catch
                //{

                //}
            }
            return resp;
        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("OnConnect")]
        public ResponseData OnConnect(string request)
        {
            ResponseData resp = new ResponseData();
            try
            {
                //  SocketIO json = Newtonsoft.Json.JsonConvert.DeserializeObject<SocketIO>(request);

                try
                {
                    File.AppendAllText(physicalPath + "\\" + "OnConnect.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + request + Environment.NewLine);
                }
                catch
                {

                }
                int driverId = request.ToInt();

                if (HubProcessor.Instance.listofJobs.Count(c => c.DriverId == driverId && DateTime.Now.Subtract(c.MessageDateTime).TotalSeconds < 300) > 0)
                {
                    clsPDA objcls = HubProcessor.Instance.listofJobs.LastOrDefault(c => c.DriverId == driverId && DateTime.Now.Subtract(c.MessageDateTime).TotalSeconds < 300);

                    if (objcls != null)
                    {




                        try
                        {



                            //callsocket
                            if (objcls.MessageTypeId == eMessageTypes.JOB)
                            {
                                //List<string> listOfConnections = new List<string>();
                                //listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(objcls.DriverId));
                                //Clients.Clients(listOfConnections).despatchBooking(objcls.JobMessage.ToStr());

                                SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "despatchBooking");
                            }
                            else if (objcls.MessageTypeId == eMessageTypes.RECALLJOB)
                            {
                                //  List<string> listOfConnections = new List<string>();
                                //  listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(objcls.DriverId));
                                //   Clients.Clients(listOfConnections).forceRecoverJob(objcls.JobMessage.ToStr());


                                SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "forceRecoverJob");

                                try
                                {

                                    Instance.listofJobs.Remove(objcls);
                                }
                                catch
                                {

                                }
                            }

                            else if (objcls.MessageTypeId == eMessageTypes.UPDATEJOB)
                            {
                                // List<string> listOfConnections = new List<string>();
                                //   listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(objcls.DriverId));
                                //   Clients.Clients(listOfConnections).updateJob(objcls.JobMessage.ToStr());


                                SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "updateJob");
                            }
                            else if (objcls.MessageTypeId == eMessageTypes.UPDATE_SETTINGS)
                            {
                                // List<string> listOfConnections = new List<string>();
                                // listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(objcls.DriverId));
                                //   Clients.Clients(listOfConnections).updateSetting(objcls.JobMessage.ToStr());

                                SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "updateSetting");
                            }





                        }
                        catch (Exception ex)
                        {


                        }

                        //if (jobId != 0 && (objcls.MessageTypeId == eMessageTypes.JOB) && jobId == objcls.JobId)
                        //{

                        //    string pickup = string.Empty;
                        //    string destination = string.Empty;
                        //    try
                        //    {
                        //        if (objcls.JobMessage.ToStr().StartsWith("JobId:{ \"JobId\""))
                        //        {
                        //            ClsJobMessageParser objParser = new JavaScriptSerializer().Deserialize<ClsJobMessageParser>(objcls.JobMessage.ToStr().Substring(6));

                        //            if (objParser != null)
                        //            {
                        //                pickup = objParser.Pickup.ToStr();
                        //                destination = objParser.Destination.ToStr();
                        //            }
                        //        }
                        //        else
                        //        {
                        //            pickup = objcls.JobMessage.Substring(objcls.JobMessage.IndexOf(":Pickup:") + 8);
                        //            pickup = pickup.Remove(pickup.IndexOf(":Destination:"));

                        //            destination = objcls.JobMessage.Substring(objcls.JobMessage.IndexOf(":Destination:") + 13);
                        //            destination = destination.Remove(destination.IndexOf(":PickupDateTime:"));
                        //        }

                        //        General.BroadCastMessage("**job received>>" + objcls.DriverNo + ">>" + pickup + ">>" + destination);
                        //    }
                        //    catch
                        //    {

                        //    }

                        //    Instance.listofJobs.Remove(objcls);
                        //}

                        if (objcls.MessageTypeId == eMessageTypes.FORCE_ACTION_BUTTON)
                        {

                            //

                            //callsocket

                            //Send message to PDA  
                            if (objcls.JobMessage.ToStr().Contains("<<Arrive Job>>"))
                            {
                                SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "forceArriveJob");
                                // Clients.Caller.forceArriveJob(objcls.JobMessage.ToStr());
                            }
                            else if (objcls.JobMessage.ToStr().Contains("<<POB Job>>"))
                            {
                                SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "forcePobJob");
                                // Clients.Caller.forcePobJob(objcls.JobMessage.ToStr());
                            }

                            Instance.listofJobs.Remove(objcls);


                        }
                        else if (objcls.MessageTypeId == eMessageTypes.AUTHORIZATION)
                        {
                            //Send message to PDA
                            //callsocket
                            //  Clients.Caller.authStatus(objcls.JobMessage.ToStr());
                            SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "authStatus");


                            try
                            {
                                if (objcls.JobMessage.Contains("yes"))
                                {
                                    Instance.listofJobs.Remove(objcls);
                                }

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
                                //callsocket
                                //    SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "authStatus");
                                // Clients.Caller.authStatus(objcls.JobMessage.ToStr());

                                //callsocket
                                SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "logoutAuthStatus");
                                //   Clients.Caller.logoutAuthStatus(objcls.JobMessage.ToStr());
                                //
                            }
                            catch
                            {

                            }
                            //


                            if (DateTime.Now.Subtract(objcls.MessageDateTime).TotalMinutes > 1)
                            {
                                Instance.listofJobs.Remove(objcls);
                            }


                        }
                        else if (objcls.MessageTypeId == eMessageTypes.ONBIDDESPATCH)
                        {


                            try
                            {
                                File.AppendAllText(physicalPath + "\\" + "OnConnect_onbiddespatch.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + request + Environment.NewLine);
                            }
                            catch
                            {

                            }


                            int AvailCnter = 0;





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
                                    //  Clients.Caller.despatchBooking(objcls.JobMessage.ToStr());

                                    string recordId = Guid.NewGuid().ToString();
                                    //try
                                    //{

                                    //    Instance.listofJobs.Add(new clsPDA
                                    //    {
                                    //        JobId = objcls.JobId,
                                    //        DriverId = objcls.DriverId,
                                    //        MessageDateTime = DateTime.Now,
                                    //        JobMessage = objcls.JobMessage.ToStr(),
                                    //        MessageTypeId = eMessageTypes.JOB,
                                    //        DriverNo = objcls.DriverNo.ToStr(),
                                    //        Id = recordId
                                    //    });
                                    //}
                                    //catch
                                    //{

                                    //}

                                    SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "despatchBooking", "", recordId);



                                    General.SP_SaveBid(objcls.JobId, objcls.DriverId, objcls.Price, 2, "", "Job Despatched");



                                    //   objcls.MessageTypeId = eMessageTypes.JOB;

                                    General.BroadCastMessage("**onbid despatch>>" + objcls.JobId + ">>" + objcls.DriverId + ">>" + msg);
                                    //

                                    try
                                    {
                                        File.AppendAllText(physicalPath + "\\OnConnect_Onbiddespatchmethodsuccess.txt", DateTime.Now + ":" + ",jobid=" + objcls.JobId + ",driverid:" + objcls.DriverId +
                                            ",message:" + objcls.JobMessage + Environment.NewLine);
                                    }
                                    catch
                                    {

                                    }



                                    //try
                                    //{
                                    //    Instance.listofJobs.Remove(objcls);
                                    //}
                                    //catch
                                    //{ }
                                }
                                catch
                                {
                                    try
                                    {
                                        //Send message to PDA
                                        //  Clients.Caller.despatchBooking(objcls.JobMessage.ToStr());
                                        string recordId = Guid.NewGuid().ToString();
                                        try
                                        {

                                            Instance.listofJobs.Add(new clsPDA
                                            {
                                                JobId = objcls.JobId,
                                                DriverId = objcls.DriverId,
                                                MessageDateTime = DateTime.Now,
                                                JobMessage = objcls.JobMessage.ToStr(),
                                                MessageTypeId = eMessageTypes.JOB,
                                                DriverNo = objcls.DriverNo.ToStr(),
                                                Id = recordId
                                            });
                                        }
                                        catch
                                        {

                                        }

                                        SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "despatchBooking", "", recordId);

                                        General.SP_SaveBid(objcls.JobId, objcls.DriverId, objcls.Price, 2, "", "Job Despatched");


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

                            try
                            {
                                File.AppendAllText(physicalPath + "\\" + "OnConnect_stcallocated.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + request + Environment.NewLine);
                            }
                            catch
                            {

                            }
                            int AvailCnter = 0;

                            using (TaxiDataContext db = new TaxiDataContext())
                            {
                                try
                                {
                                    db.CommandTimeout = 6;

                                    AvailCnter = db.Fleet_DriverQueueLists.Count(c => c.Status == true && c.DriverId == objcls.DriverId && c.DriverWorkStatusId == Enums.Driver_WORKINGSTATUS.SOONTOCLEAR);

                                    if (AvailCnter > 0)
                                    {

                                        General.BroadCastMessage("**onbid allocate>>" + objcls.JobId + ">>" + objcls.DriverId + ">>" + objcls.DriverNo.ToStr() + " " + ">>" + objcls.JobMessage);
                                    }


                                }
                                catch (Exception ex)
                                {

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

                                    // Clients.Caller.notification(new JavaScriptSerializer().Serialize(not));
                                    //callsocket



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
                                // Clients.Caller.despatchBooking(jobMessage);
                                SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "despatchBooking");
                                //callsocket
                                Instance.listofJobs.Remove(objcls);



                            }
                            else if (objcls.MessageTypeId == eMessageTypes.RECALLJOB)
                            {

                                //callsocket
                                SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "forceRecoverJob");
                                //  Clients.Caller.forceRecoverJob(objcls.JobMessage.ToStr());
                                Instance.listofJobs.Remove(objcls);
                            }
                            else if (objcls.MessageTypeId == eMessageTypes.CLEAREDJOB)
                            {
                                //callsocket
                                SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "forceClearJob");
                                // Clients.Caller.forceClearJob(objcls.JobMessage.ToStr());
                                Instance.listofJobs.Remove(objcls);
                            }
                            else if (objcls.MessageTypeId == eMessageTypes.UPDATE_SETTINGS)
                            {
                                //callsocket
                                SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "updateSetting");
                                // Clients.Caller.updateSetting(objcls.JobMessage.ToStr());
                                Instance.listofJobs.Remove(objcls);

                            }
                            else if (objcls.MessageTypeId == eMessageTypes.UPDATEJOB)
                            {

                                //callsocket
                                SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "updateJob");
                                //   Clients.Caller.updateJob(objcls.JobMessage.ToStr());
                                Instance.listofJobs.Remove(objcls);
                            }

                            else if (objcls.MessageTypeId == eMessageTypes.BIDALERT)
                            {
                                //
                                Instance.listofJobs.Remove(objcls);
                            }
                            else if (objcls.MessageTypeId == eMessageTypes.BIDPRICEALERT)
                            {
                                Instance.listofJobs.Remove(objcls);
                            }
                            else if (objcls.MessageTypeId == eMessageTypes.MESSAGING)
                            {

                                if (objcls.JobMessage.ToStr().Trim().ToLower() == "force logout")
                                {
                                    //
                                    //callsocket
                                    SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "forceLogout");
                                    //  Clients.Caller.forceLogout(objcls.JobMessage.ToStr());
                                }
                                else
                                {
                                    SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "sendMessage");
                                    //   Clients.Caller.sendMessage(objcls.JobMessage.ToStr());
                                    //callsocket
                                }
                                Instance.listofJobs.Remove(objcls);
                            }
                            else if (objcls.MessageTypeId == eMessageTypes.UPDATEPLOT || objcls.MessageTypeId == eMessageTypes.UPDATEJOB)

                            {
                                long REjOBiD = objcls.JobId;
                                int driveriD = objcls.DriverId;
                                string sMsg = objcls.JobMessage.ToStr();

                                Instance.listofJobs.Remove(objcls);


                            }
                            else if (objcls.MessageTypeId == eMessageTypes.BIDALERT)
                            {
                                Instance.listofJobs.Remove(objcls);
                            }
                            else if (objcls.MessageTypeId == eMessageTypes.JOB)
                            {
                                Instance.listofJobs.Remove(objcls);
                            }

                            else if (objcls.MessageTypeId == eMessageTypes.UPDATE_SETTINGS)
                            {
                                Instance.listofJobs.Remove(objcls);
                            }
                            else if (objcls.MessageTypeId == eMessageTypes.TRIP)
                            {
                                SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "dispatchTrip");
                                //    Clients.Caller.dispatchTrip(objcls.JobMessage.ToStr());

                                try
                                {
                                    File.AppendAllText(physicalPath + "\\" + "Tripdispatch_onconnect.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + objcls.JobMessage.ToStr().ToString() + Environment.NewLine);
                                }
                                catch
                                {
                                    ////
                                }
                                Instance.listofJobs.Remove(objcls);
                            }
                            else
                            {

                                if (objcls.MessageTypeId == eMessageTypes.BIDALERT)
                                {


                                    //callsocket
                                    SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "bidAlert");
                                    //  Clients.Caller.bidAlert(objcls.JobMessage.ToStr());

                                }
                                else
                                {
                                    //callsocket
                                    SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "LatLongChanged");
                                    //  Clients.Caller.LatLongChanged(objcls.JobMessage.ToStr());

                                }
                                Instance.listofJobs.Remove(objcls);


                            }



                        }
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(physicalPath + "\\" + "OnConnect_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + request.ToStr() + ",exception:" + ex.Message.ToStr() + Environment.NewLine);
                }
                catch
                {

                }
            }
            return resp;
        }



        public void SendLocationData(SendDataRequest req)
        {
            try
            {
                try
                {
                    File.AppendAllText(physicalPath + "\\" + "SendDatamethodcalledSocket.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + new JavaScriptSerializer().Serialize(req) + Environment.NewLine);
                }
                catch
                {
                    //
                }
                //




                LoadDataList();

                //   SendDataRequest req = new JavaScriptSerializer().Deserialize<SendDataRequest>(json.ToString());

                string mesg = req.LatLong;



                //if (req.MeterString.ToStr().Trim().StartsWith("meter=>>>"))
                //    return;




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




                using (TaxiDataContext db = new TaxiDataContext())
                {
                    try
                    {



                        db.CommandTimeout = 3;


                        int oldzoneId = db.stp_SaveDriverLocationByZone(driverId, latitude, longitude, speed, jobId).FirstOrDefault().Column1.ToInt();
                        if (oldzoneId.ToInt() == -2 && jobId > 0 && Global.AutoSTC.ToStr() == "1")
                        {
                            try
                            {



                                if (Global.listofSTCReminder.Count(c => c.JobId == jobId && c.DriverId == driverId) > 0)
                                {
                                    oldzoneId = 0;

                                    try
                                    {
                                        File.AppendAllText(AppContext.BaseDirectory + "\\AddSTCReminder_step1.txt", DateTime.Now.ToStr() + ": jobid:" + jobId + ",driverid:" + driverId + Environment.NewLine);
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


                        if (oldzoneId == 0 || speed > 0)

                        {

                            //  LoadDataList();


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
                                            if (FindPoint(latitude, longitude, Instance.listofPolyVertices.Where(c => c.ZoneId == plotId).ToList()))
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

                                            //if (returnLoc.Length > 0)
                                            //{

                                            //    postcode = General.GetPostCodeMatchWithBase(returnLoc, true);

                                            //    if (!string.IsNullOrEmpty(returnLoc) && string.IsNullOrEmpty(postcode))
                                            //    {
                                            //        postcode = GetPostCodeMatch(returnLoc);

                                            //    }
                                            //}
                                        }
                                        catch
                                        {

                                        }
                                    }




                                    if (!string.IsNullOrEmpty(returnLoc))
                                    {

                                        if (Instance.objPolicy.AutoZonePlotType.ToStr() == "postcode")
                                        {
                                            newZoneName = General.GetHalfPostCodeMatch(returnLoc.ToStr().ToUpper());
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

                                                    SocketIO.SendToSocket(driverId.ToStr(), driverId.ToStr() + "=" + jobId + "=" + zoneId, "callstc");

                                                    //  Clients.Caller.callstc(driverId.ToStr() + "=" + jobId + "=" + zoneId);

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

                                        //
                                        if (hasChanges)
                                        {
                                            db.SubmitChanges();

                                            ////

                                            General.BroadCastMessage("**refresh plots");
                                            //   General.BroadCastMessage("**refresh jsonplots=" + Newtonsoft.Json.JsonConvert.SerializeObject(db.stp_GetDashboardDrivers(0)));

                                            //
                                            try
                                            {
                                                //callsocket
                                                // SocketIO.SendToSocket(driverId.ToStr(), "true", "requestZoneUpdates");
                                                //  Clients.Caller.requestZoneUpdates("true");

                                                //if (SendRankChangedNotification)
                                                //    BroadCastPostionChanged(driverId);
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
                    catch (Exception ex)
                    {
                        try
                        {

                            //   File.AppendAllText("excep_savedriverlocation.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt") + ":" + dataValue + " ," + ex.Message + ",retrycnt:" + retryDriverLocTimeout.ToStr() + Environment.NewLine);
                        }
                        catch
                        {

                        }


                    }
                }




                ResponseData data = requestPlotsBidding(req.PlotBidding.ToStr());


                if (data != null && data.Data.ToStr() != "")
                {
                    SocketIO.SendToSocket(values[5].ToStr(), data.Data, "plotsBiddings");

                    try
                    {
                        if (req.PlotBidding.ToStr().Length > 0)
                            File.AppendAllText(physicalPath + "\\" + "sendlocationdata_reqplotbidding.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + req.PlotBidding.ToStr() + ",response:" + data.Data + Environment.NewLine);
                    }
                    catch
                    {


                    }

                }
                if (Instance.listofJobs.Count(c => c.DriverId == driverId && DateTime.Now.Subtract(c.MessageDateTime).TotalSeconds < 300) > 0)
                {


                    clsPDA objcls = Instance.listofJobs.LastOrDefault(c => c.DriverId == driverId && DateTime.Now.Subtract(c.MessageDateTime).TotalSeconds < 300);

                    if (objcls != null)
                    {



                        //



                        //Send message to PDA


                        if (jobId == 0 && objcls.MessageTypeId == eMessageTypes.JOB)
                        {
                            //string jobMessage = objcls.JobMessage.ToStr();
                            //Clients.Caller.despatchBooking(jobMessage);

                            SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "despatchBooking", "", objcls.Id.ToStr());


                            try
                            {


                                //  File.AppendAllText(AppContext.BaseDirectory + "\\LATLNGjobapi.txt", DateTime.Now + ", DriverId :" + driverId + ",job :" + objcls.JobMessage.ToStr() + Environment.NewLine);
                                Instance.listofJobs.Remove(objcls);
                            }
                            catch
                            {

                            }
                            // List<string> listOfConnections = new List<string>();
                            // listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(objcls.DriverId));
                            // Clients.Clients(listOfConnections).despatchBooking(objcls.JobMessage.ToStr());
                        }
                        else if (objcls.MessageTypeId == eMessageTypes.ONBIDDESPATCH)
                        {
                            int AvailCnter = 0;
                            if (jobId > 0)
                            {
                                try
                                {
                                    File.AppendAllText(physicalPath + "\\onbiddespatchlatlongALREADYJOB.txt", DateTime.Now + ":" + ",jobid=" + objcls.JobId + ",driverid:" + objcls.DriverId + Environment.NewLine);
                                }
                                catch
                                {

                                }



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
                                            try
                                            {
                                                File.AppendAllText(physicalPath + "\\onbiddespatch_drivernotavailable.txt", DateTime.Now + ":" + ",jobid=" + objcls.JobId + Environment.NewLine);
                                            }
                                            catch
                                            {

                                            }

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        AvailCnter = 1;

                                        try
                                        {
                                            File.AppendAllText(physicalPath + "\\onbiddespatchALREADYJOB_catch.txt", DateTime.Now + ":" + ex.Message + ",jobid=" + objcls.JobId + Environment.NewLine);
                                        }
                                        catch
                                        {

                                        }
                                    }
                                }
                            }
                            else
                            {


                                using (TaxiDataContext db = new TaxiDataContext())
                                {

                                    db.CommandTimeout = 6;
                                    try
                                    {
                                        AvailCnter = db.Bookings.Count(c => c.Id == objcls.JobId && c.BookingStatusId == Enums.BOOKINGSTATUS.BID);


                                        if (AvailCnter == 0)
                                        {
                                            try
                                            {
                                                File.AppendAllText(physicalPath + "\\onbiddespatch_jobnotavailable.txt", DateTime.Now + ":" + ",jobid=" + objcls.JobId + Environment.NewLine);
                                            }
                                            catch
                                            {

                                            }

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        AvailCnter = 1;
                                        try
                                        {
                                            File.AppendAllText(physicalPath + "\\onbiddespatch_jobnotavailable_exception.txt", DateTime.Now + ":" + ",jobid=" + objcls.JobId + Environment.NewLine);
                                        }
                                        catch
                                        {

                                        }

                                    }
                                }

                            }
                            //if (jobId > 0)
                            //{
                            //    try
                            //    {
                            //        File.AppendAllText(physicalPath + "\\onbiddespatchlatlongALREADYJOB.txt", DateTime.Now + ":" + ",jobid=" + objcls.JobId + ",driverid:"+objcls.DriverId+ Environment.NewLine);
                            //    }
                            //    catch
                            //    {

                            //    }



                            //    //using (TaxiDataContext db = new TaxiDataContext())
                            //    //{
                            //    //    try
                            //    //    {
                            //    //        db.CommandTimeout = 6;

                            //    //        AvailCnter = db.Fleet_DriverQueueLists.Count(c => c.Status == true && c.DriverId == objcls.DriverId && c.DriverWorkStatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE);

                            //    //        if (AvailCnter > 0)
                            //    //        {
                            //    //            int valCnt = db.Bookings.Count(c => c.BookingStatusId == Enums.BOOKINGSTATUS.PENDING && c.DriverId == objcls.DriverId && c.PickupDateTime > DateTime.Now.AddDays(-1));

                            //    //            if (valCnt > 0)
                            //    //            {
                            //    //                AvailCnter = 0;
                            //    //            }
                            //    //        }

                            //    //        if (AvailCnter == 0)
                            //    //        {
                            //    //            //try
                            //    //            //{
                            //    //            //    File.AppendAllText(physicalPath + "\\onbiddespatchALREADYJOB.txt", DateTime.Now + ":" + ",jobid=" + objcls.JobId + Environment.NewLine);
                            //    //            //}
                            //    //            //catch
                            //    //            //{

                            //    //            //}

                            //    //        }
                            //    //    }
                            //    //    catch (Exception ex)
                            //    //    {
                            //    //        AvailCnter = 1;

                            //    //        //try
                            //    //        //{
                            //    //        //    File.AppendAllText(physicalPath + "\\onbiddespatchALREADYJOB_catch.txt", DateTime.Now + ":" + ex.Message + ",jobid=" + objcls.JobId + Environment.NewLine);
                            //    //        //}
                            //    //        //catch
                            //    //        //{

                            //    //        //}
                            //    //    }
                            //    //}
                            //}
                            //else
                            //    AvailCnter = 1;

                            if (jobId == 0 && AvailCnter > 0)
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
                                    //  Clients.Caller.despatchBooking(objcls.JobMessage.ToStr());

                                    string recordId = Guid.NewGuid().ToString();
                                    //try
                                    //{

                                    //    Instance.listofJobs.Add(new clsPDA
                                    //    {
                                    //        JobId = objcls.JobId,
                                    //        DriverId = objcls.DriverId,
                                    //        MessageDateTime = DateTime.Now,
                                    //        JobMessage = objcls.JobMessage.ToStr(),
                                    //        MessageTypeId = eMessageTypes.JOB,
                                    //        DriverNo = objcls.DriverNo.ToStr(),
                                    //        Id = recordId
                                    //    });
                                    //}
                                    //catch
                                    //{

                                    //}

                                    SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "despatchBooking", "", recordId);



                                    General.SP_SaveBid(objcls.JobId, objcls.DriverId, objcls.Price, 2, "", "Job Despatched");



                                    //   objcls.MessageTypeId = eMessageTypes.JOB;

                                    General.BroadCastMessage("**onbid despatch>>" + objcls.JobId + ">>" + objcls.DriverId + ">>" + msg);


                                    try
                                    {
                                        File.AppendAllText(physicalPath + "\\Onbiddespatchlatlongmethod.txt", DateTime.Now + ":" + ",jobid=" + objcls.JobId + ",driverid:" + objcls.DriverId +
                                            ",message:" + objcls.JobMessage + Environment.NewLine);
                                    }
                                    catch
                                    {

                                    }



                                    try
                                    {
                                        Instance.listofJobs.Remove(objcls);
                                    }
                                    catch
                                    {
                                        Thread.Sleep(200);
                                        Instance.listofJobs.Remove(objcls);

                                    }
                                }
                                catch
                                {
                                    try
                                    {

                                        try
                                        {
                                            File.AppendAllText(physicalPath + "\\OnbiddespatchmethodLatLong_exceptionSuccess.txt", DateTime.Now + ":" + ",jobid=" + objcls.JobId + ",driverid:" + objcls.DriverId +
                                                ",message:" + objcls.JobMessage + Environment.NewLine);
                                        }
                                        catch
                                        {

                                        }


                                        //Send message to PDA
                                        //  Clients.Caller.despatchBooking(objcls.JobMessage.ToStr());
                                        string recordId = Guid.NewGuid().ToString();
                                        try
                                        {

                                            Instance.listofJobs.Add(new clsPDA
                                            {
                                                JobId = objcls.JobId,
                                                DriverId = objcls.DriverId,
                                                MessageDateTime = DateTime.Now,
                                                JobMessage = objcls.JobMessage.ToStr(),
                                                MessageTypeId = eMessageTypes.JOB,
                                                DriverNo = objcls.DriverNo.ToStr(),
                                                Id = recordId
                                            });
                                        }
                                        catch
                                        {

                                        }

                                        SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "despatchBooking", "", recordId);

                                        General.SP_SaveBid(objcls.JobId, objcls.DriverId, objcls.Price, 2, "", "Job Despatched");


                                        General.BroadCastMessage("**onbid despatch>>" + objcls.JobId + ">>" + objcls.DriverId + ">>" + msg);

                                        Instance.listofJobs.Remove(objcls);
                                    }
                                    catch
                                    {
                                        try
                                        {
                                            General.BroadCastMessage("**onbid despatch>>" + objcls.JobId + ">>" + objcls.DriverId + ">>" + msg);
                                            Instance.listofJobs.Remove(objcls);
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


                                    try
                                    {
                                        File.AppendAllText(physicalPath + "\\DriverAlreadyHavedifferentjob.txt", DateTime.Now + ":" + ",jobid=" + jobId + Environment.NewLine);
                                    }
                                    catch
                                    {

                                    }

                                }
                                catch
                                {

                                }
                            }
                        }


                        else if (objcls.MessageTypeId == eMessageTypes.BIDALERT)
                        {


                            //callsocket
                            SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "bidAlert");
                            //  Clients.Caller.bidAlert(objcls.JobMessage.ToStr());

                            try
                            {
                                Instance.listofJobs.Remove(objcls);

                                File.AppendAllText(AppContext.BaseDirectory + "\\sendbidalertfromsocket.txt", DateTime.Now + ", DriverId :" + driverId + ",job :" + objcls.JobMessage.ToStr() + Environment.NewLine);
                            }
                            catch
                            {

                            }
                        }
                        else if (objcls.MessageTypeId == eMessageTypes.TRIP)
                        {
                            SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "dispatchTrip");
                            //    Clients.Caller.dispatchTrip(objcls.JobMessage.ToStr());



                            try
                            {
                                File.AppendAllText(physicalPath + "\\" + "Tripdispatch.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + objcls.JobMessage.ToStr().ToString() + Environment.NewLine);
                            }
                            catch
                            {
                                ////
                            }
                            Instance.listofJobs.Remove(objcls);
                        }
                        else if (objcls.MessageTypeId == eMessageTypes.RECALLJOB)
                        {
                            //List<string> listOfConnections = new List<string>();
                            //listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(objcls.DriverId));
                            //Clients.Clients(listOfConnections).forceRecoverJob(objcls.JobMessage.ToStr());
                            SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "forceRecoverJob");


                            try
                            {


                                File.AppendAllText(AppContext.BaseDirectory + "\\LATLNGrecoverjob.txt", DateTime.Now + ", DriverId :" + driverId + ",job :" + objcls.JobMessage.ToStr() + Environment.NewLine);

                            }
                            catch
                            {

                            }
                            //  Clients.Caller.forceRecoverJob(objcls.JobMessage.ToStr());
                            try
                            {
                                Instance.listofJobs.Remove(objcls);
                            }
                            catch
                            {

                            }
                        }
                        else if (objcls.MessageTypeId == eMessageTypes.UPDATEJOB)
                        {
                            // List<string> listOfConnections = new List<string>();
                            //   listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(objcls.DriverId));
                            //   Clients.Clients(listOfConnections).updateJob(objcls.JobMessage.ToStr());


                            SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "updateJob");

                            try
                            {


                                File.AppendAllText(AppContext.BaseDirectory + "\\LATLNGupdatejob.txt", DateTime.Now + ", DriverId :" + driverId + ",job :" + objcls.JobMessage.ToStr() + Environment.NewLine);

                            }
                            catch
                            {

                            }

                            try
                            {
                                Instance.listofJobs.Remove(objcls);
                            }
                            catch
                            {

                            }

                        }
                        else if (objcls.MessageTypeId == eMessageTypes.FORCE_ACTION_BUTTON)
                        {

                            //

                            //callsocket

                            //Send message to PDA  
                            if (objcls.JobMessage.ToStr().Contains("<<Arrive Job>>"))
                            {
                                SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "forceArriveJob");
                                // Clients.Caller.forceArriveJob(objcls.JobMessage.ToStr());
                            }
                            else if (objcls.JobMessage.ToStr().Contains("<<POB Job>>"))
                            {
                                SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "forcePobJob");
                                // Clients.Caller.forcePobJob(objcls.JobMessage.ToStr());
                            }

                            try
                            {
                                Instance.listofJobs.Remove(objcls);
                            }
                            catch
                            {

                            }



                        }
                        else if (objcls.MessageTypeId == eMessageTypes.AUTHORIZATION)
                        {
                            //Send message to PDA
                            //callsocket
                            //  Clients.Caller.authStatus(objcls.JobMessage.ToStr());
                            SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "authStatus");


                            try
                            {


                                File.AppendAllText(AppContext.BaseDirectory + "\\LATLNGauthorisation.txt", DateTime.Now + ", DriverId :" + driverId + ",job :" + objcls.JobMessage.ToStr() + Environment.NewLine);

                            }
                            catch
                            {

                            }

                            try
                            {
                                if (objcls.JobMessage.Contains("yes"))
                                {
                                    Instance.listofJobs.Remove(objcls);
                                }

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
                                //callsocket
                                //    SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "authStatus");
                                // Clients.Caller.authStatus(objcls.JobMessage.ToStr());

                                //callsocket
                                SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "logoutAuthStatus");
                                //   Clients.Caller.logoutAuthStatus(objcls.JobMessage.ToStr());
                                //
                            }
                            catch
                            {

                            }
                            //


                            if (DateTime.Now.Subtract(objcls.MessageDateTime).TotalMinutes > 1)
                            {
                                Instance.listofJobs.Remove(objcls);
                            }


                        }
                        else if (objcls.MessageTypeId == eMessageTypes.CLEAREDJOB)
                        {
                            //callsocket
                            SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "forceClearJob");
                            // Clients.Caller.forceClearJob(objcls.JobMessage.ToStr());



                            try
                            {


                                File.AppendAllText(AppContext.BaseDirectory + "\\LATLNGclearjob.txt", DateTime.Now + ", DriverId :" + driverId + ",job :" + objcls.JobMessage.ToStr() + Environment.NewLine);

                            }
                            catch
                            {

                            }



                            try
                            {
                                Instance.listofJobs.Remove(objcls);
                            }
                            catch
                            {

                            }
                        }
                        else if (objcls.MessageTypeId == eMessageTypes.MESSAGING)
                        {

                            if (objcls.JobMessage.ToStr().Trim().ToLower() == "force logout")
                            {
                                //
                                //callsocket
                                SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "forceLogout");
                                //  Clients.Caller.forceLogout(objcls.JobMessage.ToStr());
                            }
                            else
                            {
                                SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "sendMessage");
                                //   Clients.Caller.sendMessage(objcls.JobMessage.ToStr());
                                //callsocket
                            }

                            try
                            {


                                File.AppendAllText(AppContext.BaseDirectory + "\\LATLNGmessage.txt", DateTime.Now + ", DriverId :" + driverId + ",job :" + objcls.JobMessage.ToStr() + Environment.NewLine);

                            }
                            catch
                            {

                            }
                            //
                            try
                            {
                                Instance.listofJobs.Remove(objcls);
                            }
                            catch
                            {

                            }

                        }




                    }
                }




                //if (Instance.listofJobs.Count(c => c.DriverId == driverId && DateTime.Now.Subtract(c.MessageDateTime).TotalSeconds < 300) > 0)
                //{


                //    clsPDA objcls = Instance.listofJobs.LastOrDefault(c => c.DriverId == driverId && DateTime.Now.Subtract(c.MessageDateTime).TotalSeconds < 300);

                //    if (objcls != null)
                //    {







                //        //Send message to PDA


                //        if (jobId == 0 && objcls.MessageTypeId == eMessageTypes.JOB)
                //        {
                //            //string jobMessage = objcls.JobMessage.ToStr();
                //            //Clients.Caller.despatchBooking(jobMessage);

                //            SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "despatchBooking", "", objcls.Id.ToStr());


                //            try
                //            {


                //                File.AppendAllText(AppContext.BaseDirectory + "\\LATLNGjobapi.txt", DateTime.Now + ", DriverId :" + driverId + ",job :" + objcls.JobMessage.ToStr() + Environment.NewLine);
                //                Instance.listofJobs.Remove(objcls);
                //            }
                //            catch
                //            {

                //            }
                //            // List<string> listOfConnections = new List<string>();
                //            // listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(objcls.DriverId));
                //            // Clients.Clients(listOfConnections).despatchBooking(objcls.JobMessage.ToStr());
                //        }
                //        else if (jobId == 0 && objcls.MessageTypeId == eMessageTypes.ONBIDDESPATCH)
                //        {
                //            int AvailCnter = 0;
                //            if (jobId > 0)
                //            {




                //                using (TaxiDataContext db = new TaxiDataContext())
                //                {
                //                    try
                //                    {
                //                        db.CommandTimeout = 6;

                //                        AvailCnter = db.Fleet_DriverQueueLists.Count(c => c.Status == true && c.DriverId == objcls.DriverId && c.DriverWorkStatusId == Enums.Driver_WORKINGSTATUS.AVAILABLE);

                //                        if (AvailCnter > 0)
                //                        {
                //                            int valCnt = db.Bookings.Count(c => c.BookingStatusId == Enums.BOOKINGSTATUS.PENDING && c.DriverId == objcls.DriverId && c.PickupDateTime > DateTime.Now.AddDays(-1));

                //                            if (valCnt > 0)
                //                            {
                //                                AvailCnter = 0;
                //                            }
                //                        }

                //                        if (AvailCnter == 0)
                //                        {
                //                            //try
                //                            //{
                //                            //    File.AppendAllText(physicalPath + "\\onbiddespatchALREADYJOB.txt", DateTime.Now + ":" + ",jobid=" + objcls.JobId + Environment.NewLine);
                //                            //}
                //                            //catch
                //                            //{

                //                            //}

                //                        }
                //                    }
                //                    catch (Exception ex)
                //                    {
                //                        AvailCnter = 1;

                //                        //try
                //                        //{
                //                        //    File.AppendAllText(physicalPath + "\\onbiddespatchALREADYJOB_catch.txt", DateTime.Now + ":" + ex.Message + ",jobid=" + objcls.JobId + Environment.NewLine);
                //                        //}
                //                        //catch
                //                        //{

                //                        //}
                //                    }
                //                }
                //            }
                //            else
                //                AvailCnter = 1;

                //            if (AvailCnter > 0)
                //            {

                //                string msg = string.Empty;

                //                if (Instance.objPolicy.BiddingType.ToInt() == Enums.BIDDING_TYPES.NEAREST_DRIVER)
                //                    msg = "Bidding Job has been Despatched to Nearest driver";
                //                else if (Instance.objPolicy.BiddingType.ToInt() == Enums.BIDDING_TYPES.LONGEST_WAITING_QUEUE)
                //                    msg = "Job Despatch successfully to longest waiting driver";
                //                else if (Instance.objPolicy.BiddingType.ToInt() == Enums.BIDDING_TYPES.FASTEST_FINGER)
                //                    msg = "Job Received to Fastest Finger driver";

                //                try
                //                {
                //                    //Send message to PDA
                //                    //  Clients.Caller.despatchBooking(objcls.JobMessage.ToStr());

                //                    string recordId = Guid.NewGuid().ToString();
                //                    //try
                //                    //{

                //                    //    Instance.listofJobs.Add(new clsPDA
                //                    //    {
                //                    //        JobId = objcls.JobId,
                //                    //        DriverId = objcls.DriverId,
                //                    //        MessageDateTime = DateTime.Now,
                //                    //        JobMessage = objcls.JobMessage.ToStr(),
                //                    //        MessageTypeId = eMessageTypes.JOB,
                //                    //        DriverNo = objcls.DriverNo.ToStr(),
                //                    //        Id = recordId
                //                    //    });
                //                    //}
                //                    //catch
                //                    //{

                //                    //}

                //                    SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "despatchBooking", "", recordId);



                //                    General.SP_SaveBid(objcls.JobId, objcls.DriverId, objcls.Price, 2, "", "Job Despatched");



                //                    //   objcls.MessageTypeId = eMessageTypes.JOB;

                //                    General.BroadCastMessage("**onbid despatch>>" + objcls.JobId + ">>" + objcls.DriverId + ">>" + msg);


                //                    try
                //                    {
                //                        File.AppendAllText(physicalPath + "\\Onbiddespatchmethod.txt", DateTime.Now + ":" + ",jobid=" + objcls.JobId + ",driverid:" + objcls.DriverId +
                //                            ",message:" + objcls.JobMessage + Environment.NewLine);
                //                    }
                //                    catch
                //                    {

                //                    }



                //                    //try
                //                    //{
                //                    //    Instance.listofJobs.Remove(objcls);
                //                    //}
                //                    //catch
                //                    //{ }
                //                }
                //                catch
                //                {
                //                    try
                //                    {
                //                        //Send message to PDA
                //                        //  Clients.Caller.despatchBooking(objcls.JobMessage.ToStr());
                //                        string recordId = Guid.NewGuid().ToString();
                //                        try
                //                        {

                //                            Instance.listofJobs.Add(new clsPDA
                //                            {
                //                                JobId = objcls.JobId,
                //                                DriverId = objcls.DriverId,
                //                                MessageDateTime = DateTime.Now,
                //                                JobMessage = objcls.JobMessage.ToStr(),
                //                                MessageTypeId = eMessageTypes.JOB,
                //                                DriverNo = objcls.DriverNo.ToStr(),
                //                                Id = recordId
                //                            });
                //                        }
                //                        catch
                //                        {

                //                        }

                //                        SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "despatchBooking", "", recordId);

                //                        General.SP_SaveBid(objcls.JobId, objcls.DriverId, objcls.Price, 2, "", "Job Despatched");


                //                        General.BroadCastMessage("**onbid despatch>>" + objcls.JobId + ">>" + objcls.DriverId + ">>" + msg);

                //                        Instance.listofJobs.Remove(objcls);
                //                    }
                //                    catch
                //                    {
                //                        try
                //                        {
                //                            General.BroadCastMessage("**onbid despatch>>" + objcls.JobId + ">>" + objcls.DriverId + ">>" + msg);
                //                        }
                //                        catch
                //                        {

                //                        }
                //                    }
                //                }
                //            }
                //            else
                //            {
                //                try
                //                {
                //                    Instance.listofJobs.Remove(objcls);
                //                }
                //                catch
                //                {

                //                }
                //            }
                //        }
                //        else if (objcls.MessageTypeId == eMessageTypes.BIDALERT)
                //        {


                //            //callsocket
                //            SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "bidAlert");
                //            //  Clients.Caller.bidAlert(objcls.JobMessage.ToStr());

                //            try
                //            {
                //                Instance.listofJobs.Remove(objcls);

                //                File.AppendAllText(AppContext.BaseDirectory + "\\sendbidalertfromsocket.txt", DateTime.Now + ", DriverId :" + driverId + ",job :" + objcls.JobMessage.ToStr() + Environment.NewLine);
                //            }
                //            catch
                //            {

                //            }
                //        }
                //        else if (objcls.MessageTypeId == eMessageTypes.TRIP)
                //        {
                //            SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "dispatchTrip");
                //            //    Clients.Caller.dispatchTrip(objcls.JobMessage.ToStr());



                //            try
                //            {
                //                File.AppendAllText(physicalPath + "\\" + "Tripdispatch.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + objcls.JobMessage.ToStr().ToString() + Environment.NewLine);
                //            }
                //            catch
                //            {
                //                ////
                //            }
                //            Instance.listofJobs.Remove(objcls);
                //        }
                //        else if (objcls.MessageTypeId == eMessageTypes.RECALLJOB)
                //        {
                //            //List<string> listOfConnections = new List<string>();
                //            //listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(objcls.DriverId));
                //            //Clients.Clients(listOfConnections).forceRecoverJob(objcls.JobMessage.ToStr());
                //            SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "forceRecoverJob");


                //            try
                //            {


                //                File.AppendAllText(AppContext.BaseDirectory + "\\LATLNGrecoverjob.txt", DateTime.Now + ", DriverId :" + driverId + ",job :" + objcls.JobMessage.ToStr() + Environment.NewLine);

                //            }
                //            catch
                //            {

                //            }
                //            //  Clients.Caller.forceRecoverJob(objcls.JobMessage.ToStr());
                //            try
                //            {
                //                Instance.listofJobs.Remove(objcls);
                //            }
                //            catch
                //            {

                //            }
                //        }
                //        else if (objcls.MessageTypeId == eMessageTypes.UPDATEJOB)
                //        {
                //            // List<string> listOfConnections = new List<string>();
                //            //   listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(objcls.DriverId));
                //            //   Clients.Clients(listOfConnections).updateJob(objcls.JobMessage.ToStr());


                //            SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "updateJob");

                //            try
                //            {


                //                File.AppendAllText(AppContext.BaseDirectory + "\\LATLNGupdatejob.txt", DateTime.Now + ", DriverId :" + driverId + ",job :" + objcls.JobMessage.ToStr() + Environment.NewLine);

                //            }
                //            catch
                //            {

                //            }

                //            try
                //            {
                //                Instance.listofJobs.Remove(objcls);
                //            }
                //            catch
                //            {

                //            }

                //        }
                //        else if (objcls.MessageTypeId == eMessageTypes.FORCE_ACTION_BUTTON)
                //        {

                //            //

                //            //callsocket

                //            //Send message to PDA  
                //            if (objcls.JobMessage.ToStr().Contains("<<Arrive Job>>"))
                //            {
                //                SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "forceArriveJob");
                //                // Clients.Caller.forceArriveJob(objcls.JobMessage.ToStr());
                //            }
                //            else if (objcls.JobMessage.ToStr().Contains("<<POB Job>>"))
                //            {
                //                SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "forcePobJob");
                //                // Clients.Caller.forcePobJob(objcls.JobMessage.ToStr());
                //            }

                //            try
                //            {
                //                Instance.listofJobs.Remove(objcls);
                //            }
                //            catch
                //            {

                //            }



                //        }
                //        else if (objcls.MessageTypeId == eMessageTypes.AUTHORIZATION)
                //        {
                //            //Send message to PDA
                //            //callsocket
                //            //  Clients.Caller.authStatus(objcls.JobMessage.ToStr());
                //            SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "authStatus");


                //            try
                //            {


                //                File.AppendAllText(AppContext.BaseDirectory + "\\LATLNGauthorisation.txt", DateTime.Now + ", DriverId :" + driverId + ",job :" + objcls.JobMessage.ToStr() + Environment.NewLine);

                //            }
                //            catch
                //            {

                //            }

                //            try
                //            {
                //                if (objcls.JobMessage.Contains("yes"))
                //                {
                //                    Instance.listofJobs.Remove(objcls);
                //                }

                //                else if (objcls.JobMessage.Contains("no"))
                //                {
                //                    Instance.listofJobs.Remove(objcls);
                //                }
                //            }
                //            catch
                //            {

                //            }
                //        }
                //        else if (objcls.MessageTypeId == eMessageTypes.LOGOUTAUTHORIZATION)
                //        {
                //            //Send message to PDA
                //            try
                //            {
                //                //callsocket
                //                //    SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "authStatus");
                //                // Clients.Caller.authStatus(objcls.JobMessage.ToStr());

                //                //callsocket
                //                SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "logoutAuthStatus");
                //                //   Clients.Caller.logoutAuthStatus(objcls.JobMessage.ToStr());
                //                //
                //            }
                //            catch
                //            {

                //            }
                //            //


                //            if (DateTime.Now.Subtract(objcls.MessageDateTime).TotalMinutes > 1)
                //            {
                //                Instance.listofJobs.Remove(objcls);
                //            }


                //        }
                //        else if (objcls.MessageTypeId == eMessageTypes.CLEAREDJOB)
                //        {
                //            //callsocket
                //            SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "forceClearJob");
                //            // Clients.Caller.forceClearJob(objcls.JobMessage.ToStr());



                //            try
                //            {


                //                File.AppendAllText(AppContext.BaseDirectory + "\\LATLNGclearjob.txt", DateTime.Now + ", DriverId :" + driverId + ",job :" + objcls.JobMessage.ToStr() + Environment.NewLine);

                //            }
                //            catch
                //            {

                //            }



                //            try
                //            {
                //                Instance.listofJobs.Remove(objcls);
                //            }
                //            catch
                //            {

                //            }
                //        }
                //        else if (objcls.MessageTypeId == eMessageTypes.MESSAGING)
                //        {

                //            if (objcls.JobMessage.ToStr().Trim().ToLower() == "force logout")
                //            {
                //                //
                //                //callsocket
                //                SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "forceLogout");
                //                //  Clients.Caller.forceLogout(objcls.JobMessage.ToStr());
                //            }
                //            else
                //            {
                //                SocketIO.SendToSocket(objcls.DriverId.ToStr(), objcls.JobMessage.ToStr(), "sendMessage");
                //                //   Clients.Caller.sendMessage(objcls.JobMessage.ToStr());
                //                //callsocket
                //            }

                //            try
                //            {


                //                File.AppendAllText(AppContext.BaseDirectory + "\\LATLNGmessage.txt", DateTime.Now + ", DriverId :" + driverId + ",job :" + objcls.JobMessage.ToStr() + Environment.NewLine);

                //            }
                //            catch
                //            {

                //            }
                //            //
                //            try
                //            {
                //                Instance.listofJobs.Remove(objcls);
                //            }
                //            catch
                //            {

                //            }

                //        }

                //        //


                //    }
                //}




                string faremeterstring = req.MeterString.ToStr();

                try
                {




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


            }
            catch (Exception ex)
            {




                try
                {
                    //  Clients.Caller.exceptionOccured(ex.Message);
                    File.AppendAllText(physicalPath + "\\" + "exception_senddatasocket.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", latlong: " + req.LatLong.ToStr() + ",plotbidding:" + req.PlotBidding.ToStr() + "," + ex.Message + Environment.NewLine);
                }
                catch
                {


                }
            }
        }

        #endregion



        private static string RemoveUK(ref string address)
        {
            if (address.ToUpper().EndsWith(", UK"))
            {
                address = address.Remove(address.ToUpper().LastIndexOf(", UK"));
            }

            return address;
        }

        private static string GetPostCodeMatch(string value)
        {
            string postCode = "";


            RemoveUK(ref value);

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

        public static bool FindPoint(double pointLat, double pointLng, List<Gen_Zone_PolyVertice> PontosPolig)
        {
            //                             X               y               
            int sides = PontosPolig.Count();
            int j = sides - 1;
            bool pointStatus = false;

            if (sides == 1)
            {
                double radius = Convert.ToDouble(PontosPolig[0].Diameter) / 2;
                double lat = Convert.ToDouble(PontosPolig[0].Latitude);
                double lng = Convert.ToDouble(PontosPolig[0].Longitude);

                double dist = new LatLng(Convert.ToDouble(lat), Convert.ToDouble(lng)).DistanceMiles(new LatLng(pointLat, pointLng));

                if (dist <= radius)
                    pointStatus = true;
            }
            else
            {

                for (int i = 0; i < sides; i++)
                {
                    if (PontosPolig[i].Longitude < pointLng && PontosPolig[j].Longitude >= pointLng ||
                        PontosPolig[j].Longitude < pointLng && PontosPolig[i].Longitude >= pointLng)
                    {
                        if (PontosPolig[i].Latitude + (pointLng - PontosPolig[i].Longitude) /
                            (PontosPolig[j].Longitude - PontosPolig[i].Longitude) * (PontosPolig[j].Latitude - PontosPolig[i].Latitude) < pointLat)
                        {
                            pointStatus = !pointStatus;
                        }
                    }
                    j = i;
                }
            }

            return pointStatus;
        }

        public static double SqrRoot(double t)
        {
            double lb = 0, ub = t, temp = 0;
            int count = 50;

            while (count != 0)
            {
                temp = (lb + ub) / 2;

                if (temp * temp == t)
                {
                    return temp;
                }
                else if (temp * temp > t)
                {
                    ub = temp;
                }
                else
                {
                    lb = temp;
                }

                count--;
            }

            return temp;
        }


        private string GetDriverPay(int driverId, Fleet_Driver objDriver, string version = "")
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

                        if (HubProcessor.Instance.objPolicy.DriverSuspensionDateTime != null)
                        {

                            tillDate = tillDate.Value.AddHours((HubProcessor.Instance.objPolicy.DriverSuspensionDateTime.Value.Hour + 24));

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
                            objAction.ShowCCButton = false;
                            objAction.driverid = driverId.ToStr();
                            objAction.message = "Your Balance is due " + Environment.NewLine + objAction.amount;
                            //     objAction.HideButtons = true;

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

                        if (HubProcessor.Instance.objPolicy.DriverSuspensionDateTime != null)
                        {

                            tillDate = tillDate.Value.AddHours(HubProcessor.Instance.objPolicy.DriverSuspensionDateTime.Value.Hour);

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
                            objAction.ShowCCButton = false;
                            //  objAction.url = url + GetRMSJson(objTrans.Id, driverId, objTrans.Balance.ToDecimal()) + "&isDriverApp&IsCustomerApp";
                            objAction.driverid = driverId.ToStr();
                            objAction.message = "Your Balance is due " + Environment.NewLine + objAction.amount;
                            //     objAction.HideButtons = true;
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


            return response;
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

        public static byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            if (imageIn == null) return null;
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
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



        public string GetLocationName(double? latitude, double? longitude)
        {
            string locationName = string.Empty;
            try
            {

                //if (Global.googleKey.ToStr().Length == 0)
                //{

                //    using (TaxiDataContext db = new TaxiDataContext())
                //    {
                //        db.CommandTimeout = 5;
                //        Global.googleKey = "&key=" + db.ExecuteQuery<string>("select apikey from mapkeys where maptype='google'").FirstOrDefault();


                //    }
                //}
                // Starts Google Geocoding Webservice

                string url2 = string.Empty;
                DataTable dt = null;
                XmlTextReader reader = null;
                System.Data.DataSet ds = null;



                if (Global.googleKey.ToStr().Trim().Length > 0)
                {
                    try
                    {

                        url2 = "https://maps.googleapis.com/maps/api/geocode/xml?latlng=" + latitude + "," + longitude + Global.googleKey + "&sensor=false";

                        reader = new XmlTextReader(url2);
                        reader.WhitespaceHandling = WhitespaceHandling.Significant;
                        ds = new System.Data.DataSet();
                        ds.ReadXml(reader);

                        dt = ds.Tables["result"];

                        if (dt != null && dt.Rows.Count > 0)
                        {

                            DataRow row = dt.Rows.OfType<DataRow>().FirstOrDefault();
                            if (row != null)
                            {
                                locationName = row[1].ToStr().Trim();
                            }
                        }

                        ds.Dispose();

                        try
                        {

                            File.AppendAllText(physicalPath + "\\" + "googleLocation.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": Lat" + latitude + ",lng:" + longitude + ",location:" + locationName + Environment.NewLine);
                        }
                        catch
                        {


                        }


                    }
                    catch (Exception ex)
                    {
                        try
                        {

                            File.AppendAllText(physicalPath + "\\" + "exception_google.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ":" + ex.Message + Environment.NewLine);
                        }
                        catch
                        {


                        }
                    }
                }












            }
            catch (Exception ex)
            {


            }


            return locationName;
        }
        private void DispatchJobSMS(long jobId, int jobStatusId)
        {
            try
            {
                long onlineBookingId = 0;


                if (jobStatusId == Enums.BOOKINGSTATUS.ONROUTE)
                {
                    if (HubProcessor.Instance.objPolicy.EnablePassengerText.ToBool())
                    {





                        Booking job = General.GetObject<Booking>(c => c.Id == jobId);

                        if (job != null && job.JobCode.ToStr().Trim().Length == 0)
                        {
                            onlineBookingId = job.OnlineBookingId.ToLong();
                            if (job.CustomerMobileNo.ToStr().Trim() != string.Empty && job.DisablePassengerSMS.ToBool() == false)
                            {

                                if (HubProcessor.Instance.objPolicy.EnablePdaDespatchSms.ToBool() && HubProcessor.Instance.objPolicy.SendPdaDespatchSmsOnAcceptJob.ToBool())
                                {

                                    string driverMobileNo = General.GetObject<Fleet_Driver>(c => c.Id == job.DriverId).DefaultIfEmpty().MobileNo.ToStr().Trim();

                                    if (driverMobileNo.ToStr().Length > 0)
                                    {

                                        // ADDED ON 20/APRIL/2016 ON REQUEST OF COMMERCIAL CARS => DISABLE CUSTOMER TEXT FOR PARTICULAR ACCOUNT JOBS
                                        if (job.CompanyId != null && job.Gen_Company.DisableCustomerText.ToBool())
                                        {

                                            new Thread(delegate ()
                                            {
                                                AddSMS(driverMobileNo, GetMessage(HubProcessor.Instance.objPolicy.DespatchTextForDriver.ToStr(), job, jobId), job.SMSType.ToInt());

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
                                            AddSMS(job.CustomerMobileNo.ToStr().Trim(), GetMessage(HubProcessor.Instance.objPolicy.DespatchTextForCustomer.ToStr(), job, jobId), job.SMSType.ToInt());

                                        }).Start();
                                    }
                                }


                            }
                            else
                            {
                                if (Instance.objPolicy.EnablePdaDespatchSms.ToBool() && HubProcessor.Instance.objPolicy.SendPdaDespatchSmsOnAcceptJob.ToBool())
                                {

                                    string driverMobileNo = General.GetObject<Fleet_Driver>(c => c.Id == job.DriverId).DefaultIfEmpty().MobileNo.ToStr().Trim();

                                    if (driverMobileNo.ToStr().Length > 0)
                                    {
                                        new Thread(delegate ()
                                        {
                                            AddSMS(driverMobileNo, GetMessage(HubProcessor.Instance.objPolicy.DespatchTextForDriver.ToStr(), job, jobId), job.SMSType.ToInt());

                                        }).Start();
                                    }
                                }

                            }

                        }
                    }
                }
                else if (jobStatusId == Enums.BOOKINGSTATUS.ARRIVED)
                {

                    if (HubProcessor.Instance.objPolicy.EnableArrivalBookingText.ToBool())
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
                                        arrivalText = HubProcessor.Instance.objPolicy.ArrivalAirportBookingText.ToStr().Trim();
                                    }
                                    else
                                    {
                                        arrivalText = HubProcessor.Instance.objPolicy.ArrivalBookingText.ToStr().Trim();
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
                                        arrivalText = HubProcessor.Instance.objPolicy.ArrivalAirportBookingText.ToStr().Trim();
                                    }
                                    else
                                    {
                                        arrivalText = HubProcessor.Instance.objPolicy.ArrivalBookingText.ToStr().Trim();
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

                                    AddSMS(job.CustomerMobileNo.ToStr().Trim(), GetMessage(HubProcessor.Instance.objPolicy.DespatchTextForPDA.ToStr(), job, jobId), job.SMSType.ToInt());
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
        private HubProcessor Instance
        {
            get { return HubProcessor.Instance; }
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
                                                string encrypt = Cryptography.Encrypt(objBooking.BookingNo.ToStr() + ":" + linkId + ":" + Cryptography.Decrypt(ConfigurationManager.AppSettings["ConnectionString"], "tcloudX@@!", true).ToStr() + ":" + objBooking.Id, "softeuroconnskey", true);


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


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requeststc")]
        public ResponseData requestSTC(string mesg)
        {
            ResponseData resp = new ResponseData();
            JobActionEx obj = null;
            try
            {
                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\requestSTCAPI.txt", DateTime.Now.ToStr() + " request" + mesg + Environment.NewLine);
                }
                catch
                {

                }


                string dataValue = mesg;
                dataValue = dataValue.Trim();
                obj = new JavaScriptSerializer().Deserialize<JobActionEx>(mesg);
                obj.Message = "";
                RemoveUK(ref obj.Dropoff);

                long jobId = obj.JobId.ToLong();
                using (TaxiDataContext db = new TaxiDataContext())
                {


                    try
                    {


                        DateTime? POBDateTime = null;

                        if (HubProcessor.Instance.objPolicy.RestrictMilesOnSTC.ToDecimal() > 0
                            && obj.Dropoff.ToStr().Trim().Length > 0 && obj.DrvNo.ToStr().Trim().Length > 0
                            && obj.IsAuto.ToStr() != "1")
                        {
                            int RemoveRestriction = 0;

                            string dropOff = obj.Dropoff.ToStr();
                            if (dropOff.Contains("<<<"))
                            {
                                dropOff = dropOff.Remove(dropOff.IndexOf("<<<"));

                            }



                            db.CommandTimeout = 3;

                            string pickup = "";
                            string destination = GetPostCodeMatch(dropOff);

                            int journeyTypeId = 0;
                            int pickupZoneId = 0;
                            int dropOffZoneId = 0;
                            try
                            {



                                var objBooker = db.Bookings.Select(args => new
                                {
                                    args.Id,
                                    args.JourneyTypeId,
                                    args.FromAddress,
                                    args.ZoneId,
                                    args.ToAddress,
                                    args.DropOffZoneId,
                                    args.OnHoldWaitingMins,
                                    args.CashFares,
                                    args.POBDateTime,
                                    args.PickupDateTime
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
                                    POBDateTime = objBooker.POBDateTime;
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
                                distance = Math.Round(new DotNetCoords.LatLng(Convert.ToDouble(obj.Latitude), Convert.ToDouble(obj.Longitude)).DistanceMiles(new LatLng(Convert.ToDouble(coord.Latitude), Convert.ToDouble(coord.Longtiude))).ToDecimal(), 1);


                            }


                            if (coord == null)
                            {

                                //
                                bool isfound = true;


                                if (dropOffZoneId != 0)
                                {
                                    isfound = FindPoint(Convert.ToDouble(obj.Latitude), Convert.ToDouble(obj.Longitude), db.Gen_Zone_PolyVertices.Where(c => c.ZoneId == dropOffZoneId).ToList());

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




                            if (distance == -1 || (coord != null && distance > HubProcessor.Instance.objPolicy.RestrictMilesOnSTC.ToDecimal()))
                            {
                                if (RemoveRestriction == 1)
                                {

                                    //try
                                    //{

                                    //    File.AppendAllText(physicalPath + "//restrictionRemoved.txt", DateTime.Now + ": datavalue=" + dataValue + ",(stcjob)distance to : " + pickup + ",distance:" + distance + Environment.NewLine);
                                    //}
                                    //catch
                                    //{


                                    //}


                                }
                                else
                                {
                                    obj.Message = "You are far away from Destination";
                                    resp.Message = obj.Message;
                                    resp.IsSuccess = false;
                                    try
                                    {

                                        File.AppendAllText(AppContext.BaseDirectory + "//restrictionlog.txt", DateTime.Now + ": datavalue=" + dataValue + ",(stcjob)distance to : " + pickup + ",distance:" + distance + Environment.NewLine);
                                    }
                                    catch
                                    {


                                    }
                                    //        return resp;
                                }

                            }


                            //


                        }


                        try
                        {

                            if (resp.Message.ToStr().Trim().Length == 0)
                            {

                                db.stp_UpdateJob(jobId, obj.DrvId.ToInt(), Enums.BOOKINGSTATUS.STC, 5, 0);

                                General.BroadCastMessage("**action>>" + jobId.ToStr() + ">>" + obj.DrvId.ToStr() + ">>" + Enums.BOOKINGSTATUS.STC);



                                //      Global.RemoveSTCReminder(jobId, obj.DrvId.ToInt());


                                if (obj.IsAuto.ToStr() == "1")
                                {
                                    try
                                    {





                                        string query = "INSERT INTO BOOKING_LOG (bookingid,[user],beforeupdate,afterupdate,updatedate)values(" + jobId + ",'" + "Driver" + "','" + "" + "','" + "Driver " + obj.DrvNo.ToStr() + " Auto STC" + "',getdate());";
                                        db.ExecuteQuery<int>(query);

                                    }
                                    catch (Exception ex)
                                    {
                                        try
                                        {
                                            //
                                            //
                                            File.AppendAllText(AppContext.BaseDirectory + "\\autostcsavelog_exception.txt", DateTime.Now.ToStr() + ": request" + mesg + ",exception:" + ex.Message + Environment.NewLine);
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

                    catch (Exception ex)
                    {


                    }
                }

            }

            catch (Exception ex)
            {
                try
                {
                    obj.Message = ex.Message;
                    File.AppendAllText(AppContext.BaseDirectory + "\\requestSTCAPI_exception.txt", DateTime.Now.ToStr() + " request" + mesg + ",exception:" + ex.Message + Environment.NewLine);
                    resp.IsSuccess = false;
                    resp.Message = ex.Message;
                }
                catch
                {

                }
            }


            if (obj.Message.ToStr().Trim().Length == 0)
            {
                resp.IsSuccess = true;
                resp.Data = new JavaScriptSerializer().Serialize(obj);
                obj.Message = "";
            }
            return resp;
        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("startarrivewaiting")]
        public ResponseData startarrivewaiting(string mesg)
        {
            ResponseData r = new ResponseData();

            string respo = "true";
            JobActionEx objAction = null;
            try
            {
                try
                {
                    //
                    File.AppendAllText(AppContext.BaseDirectory + "\\requestarrivewaiting.txt", DateTime.Now.ToStr() + " request" + mesg + Environment.NewLine);
                }
                catch
                {

                }



                string dataValue = mesg;
                dataValue = dataValue.Trim();
                objAction = new JavaScriptSerializer().Deserialize<JobActionEx>(mesg);
                //   RemoveUK(ref objAction.Dropoff);

                long jobId = objAction.JobId.ToLong();



                DateTime pickupDate = DateTime.Now;
                //DateTime.Now;new TaxiDataContext().Bookings.Where(c => c.Id == jobId).Select(a => a.PickupDateTime).FirstOrDefault();

                //DateTime pickupdatetimeval=

                if (objAction.PickupDateTime.ToStr().Trim().Length > 0)
                {
                    try
                    {
                        string pickupDateTime = string.Format("{0:dd/MM/yyyy HH:mm}", objAction.PickupDateTime.ToStr().Trim().Replace("   ", " ").Trim());

                        pickupDateTime = string.Format("{0:dd/MM/yyyy HH:mm}", objAction.PickupDateTime.ToStr().Trim().Replace("  ", " ").Trim());

                        pickupDate = DateTime.Parse(pickupDateTime, CultureInfo.GetCultureInfo("en-gb"));


                    }
                    catch (Exception ex)
                    {

                        try
                        {

                            pickupDate = DateTime.Now;
                            File.AppendAllText(AppContext.BaseDirectory + "\\" + "exception_formatpickupdatetime_startarrivewaiting.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + mesg + "," + ex.Message + ", pickup : " + objAction.PickupDateTime.ToStr().Trim() + Environment.NewLine);
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


                if (pickupDate > DateTime.Now)
                {

                    r.IsSuccess = false;
                    r.Message = "You cannot press Start Waiting before pickup time";
                }
                else
                    r.IsSuccess = true;

            }

            catch (Exception ex)
            {
                try
                {
                    objAction.Message = ex.Message;
                    File.AppendAllText(AppContext.BaseDirectory + "\\requestarrive_exception2.txt", DateTime.Now.ToStr() + " request" + mesg + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
            }

            return r;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestDriverPDASettingUpdate")]
        public ResponseData requestDriverPDASettingUpdate(string mesg)
        {
            ResponseData res = new ResponseData();
            JobAction objData = new JavaScriptSerializer().Deserialize<JobAction>(mesg);
            string response = string.Empty;



            try
            {



                int driverId = objData.DrvId.ToInt();


                using (TaxiDataContext db = new TaxiDataContext())
                {
                    var obj = db.Fleet_Driver_PDASettings.FirstOrDefault(c => c.DriverId == driverId);


                    if (obj != null)
                    {
                        obj.NavigationApp = objData.NavType;


                    }
                    db.SubmitChanges();



                    //try
                    //{

                    //    File.AppendAllText(Application.StartupPath+"\\navigate.txt",DateTime.Now.ToStr()+" response" +response+" datavalue"+dataValue+Environment.NewLine);
                    //}
                    //catch
                    //{

                    //}


                }





                res.Data = "";
                res.IsSuccess = true;


                try
                {

                    File.AppendAllText(physicalPath + "\\requestDriverPDASettingUpdate.txt", DateTime.Now.ToStr() + "request:" + new JavaScriptSerializer().Serialize(mesg) + Environment.NewLine);
                }
                catch
                {

                }
            }
            catch (Exception ex)
            {
                try
                {
                    // res.Data = response;
                    res.IsSuccess = false;
                    res.Message = ex.Message;
                    //   Clients.Caller.callOffice("failed:Invalid Data");
                    File.AppendAllText(physicalPath + "\\requestDriverPDASettingUpdate_exception.txt", DateTime.Now.ToStr() + "request:" + new JavaScriptSerializer().Serialize(mesg) + " ,exception : " + ex.Message + Environment.NewLine);
                }
                catch
                {

                }

                //
            }

            return res;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestCallOffice")]
        public ResponseData requestCallOffice(string mesg)
        {
            ResponseData res = new ResponseData();
            string response = string.Empty;

            try
            {


                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                long jobId = values[0].ToLong();
                int subcompanyId = values[1].ToInt();

                using (TaxiDataContext db = new TaxiDataContext())
                {

                    if (db.CallerIdType_Configurations.Where(c => c.VOIPCLIType == 2).Count() > 0)
                    {
                        if (Global.customerOfficeNumber.ToStr().Trim().Length == 0)
                        {

                            if (subcompanyId == 0)
                            {
                                subcompanyId = db.Bookings.Where(c => c.Id == jobId).Select(c => c.SubcompanyId).FirstOrDefault().ToInt();
                            }

                            if (subcompanyId > 0)
                                response = db.Gen_SubCompanies.Where(c => c.Id == subcompanyId).Select(c => c.TelephoneNo).FirstOrDefault().ToStr().Trim();


                        }
                        else
                            response = Global.customerOfficeNumber.ToStr().Trim();

                        if (response.ToStr().Trim().Length > 0)
                            response = "success:" + response;
                        else
                            response = "failed:" + response;
                        // response = "failed:";


                        res.Data = response;
                        res.IsSuccess = true;


                        try
                        {

                            File.AppendAllText(physicalPath + "\\requestcalloffice_yestech.txt", DateTime.Now.ToStr() + "request:" + mesg + ", response" + response + " datavalue" + dataValue + Environment.NewLine);
                        }
                        catch
                        {

                        }

                    }
                    else
                    {
                        int driverId = values[2].ToInt();


                        string customer_number = string.Empty;
                        string driver_number = string.Empty;
                        string username = string.Empty;
                        string password = string.Empty;

                        //
                        customer_number = db.Bookings.Where(c => c.Id == jobId).Select(args => args.CustomerMobileNo).FirstOrDefault().ToStr();
                        driver_number = db.Fleet_Drivers.Where(c => c.Id == driverId).Select(args => args.MobileNo).FirstOrDefault().ToStr();
                        var objdata = db.CallerIdVOIP_Configurations.FirstOrDefault();

                        username = objdata.UserName.ToStr();
                        password = objdata.Password.ToStr().Replace("-321", "").Trim();



                        res.Data = response;
                        res.IsSuccess = false;
                        res.Message = "Request sent successfully!";

                        try
                        {




                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                            string APIBaseURL = "https://portal.emeraldvoip.com/config.php?client=" + username.ToStr() + "&password=" + password + "&customer_number=" + customer_number + "&exten=&hash=click2call&my_number=" + driver_number;
                            string resp = string.Empty;
                            using (WebClient wc = new WebClient())
                            {
                                wc.Proxy = null;
                                response = wc.DownloadString(new System.Uri(APIBaseURL));
                            }
                            mesg = APIBaseURL;



                            try
                            {

                                File.AppendAllText(physicalPath + "\\requestcalloffice_emerald.txt", DateTime.Now.ToStr() + "request:" + mesg + Environment.NewLine + ", response" + response + Environment.NewLine + " datavalue" + dataValue + Environment.NewLine);
                            }
                            catch
                            {

                            }

                        }
                        catch (Exception ex)
                        {
                            try
                            {

                                File.AppendAllText(physicalPath + "\\requestcallofficetest_exception.txt", DateTime.Now.ToStr() + "request:" + mesg + Environment.NewLine + ", response" + ex.Message + Environment.NewLine + " datavalue" + dataValue + Environment.NewLine);
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
                    res.Data = response;
                    res.IsSuccess = false;
                    res.Message = ex.Message;
                    //   Clients.Caller.callOffice("failed:Invalid Data");
                    File.AppendAllText(physicalPath + "\\requestcalloffice.txt", DateTime.Now.ToStr() + "request:" + mesg + ", response" + response + " ,exception : " + ex.Message + Environment.NewLine);
                }
                catch
                {

                }


            }

            return res;
        }



        public enum ResponseType { Direct = 1, Url = 2, MultipleGateways = 3 }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestMakePayment")]
        public ResponseData requestMakePayment(string mesg)
        {

            ResponseData res = new ResponseData();
            try
            {
                File.AppendAllText(AppContext.BaseDirectory + "\\" + "requestMakePaymentNew.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + mesg + Environment.NewLine);
            }
            catch
            {
            }


            List<Gen_SysPolicy_PaymentDetail> paymentGateway = null;

            using (TaxiDataContext db = new TaxiDataContext())
            {
                paymentGateway = db.Gen_SysPolicy_PaymentDetails.ToList();



                MakePaymentRequestViewModel input = new JavaScriptSerializer().Deserialize<MakePaymentRequestViewModel>(mesg);


                MakePaymentResponseViewModel makePaymentResponse = new MakePaymentResponseViewModel();

                makePaymentResponse.Gateways = new List<dynamic>();

                ///




                try
                {


                    foreach (var item in paymentGateway)
                    {
                        string gatewayName = item.PaymentGateway.Name;
                        if (gatewayName == "Sumup")
                        {
                            makePaymentResponse.Gateways.Add(new { GatewayName = "Sumup", Url = item.PaypalID });
                            makePaymentResponse.ResponseType = (int)ResponseType.MultipleGateways;
                        }
                        else if (gatewayName == "Paypal")
                        {
                            makePaymentResponse.Gateways.Add(new { GatewayName = "Paypal", Url = item.PaypalID });
                            makePaymentResponse.ResponseType = (int)ResponseType.MultipleGateways;
                        }
                        else if (gatewayName == "Judo" || gatewayName == "Stripe")
                        {



                            long jobId = input.jobId.ToLong();


                            JavaScriptSerializer s = new JavaScriptSerializer();




                            Taxi_Model.Booking objBooking = General.GetObject<Taxi_Model.Booking>(c => c.Id == jobId);

                            decimal Price = input.Fare;


                            var objBookingPayment = General.GetObject<Booking_Payment>(c => c.BookingId == jobId && c.AuthCode != null && c.AuthCode != "");

                            if (objBookingPayment == null || objBookingPayment.AuthCode.ToStr().Trim().Length == 0)
                            {


                                Gen_SysPolicy_PaymentDetail obj = null;

                                if (objBooking.CustomerCreditCardDetails.ToStr().Trim().Length > 0)
                                {
                                    if (objBooking.CustomerCreditCardDetails.ToStr().Trim().StartsWith("pm_"))
                                        obj = General.GetObject<Gen_SysPolicy_PaymentDetail>(c => c.SysPolicyId != 0 && (c.PaymentGatewayId == Enums.PAYMENT_GATEWAY.STRIPE));
                                    else
                                        obj = General.GetObject<Gen_SysPolicy_PaymentDetail>(c => c.SysPolicyId != 0 && (c.PaymentGatewayId == Enums.PAYMENT_GATEWAY.JUDO));


                                    makePaymentResponse.Gateways.Add(new { GatewayName = "Judo" });
                                    makePaymentResponse.ResponseType = (int)ResponseType.Direct;

                                }
                                else
                                {
                                    obj = General.GetObject<Gen_SysPolicy_PaymentDetail>(c => c.SysPolicyId != 0 && (c.EnableMobileIntegration != null && c.EnableMobileIntegration == true));

                                }

                                if (obj != null)
                                {

                                    ClsPaymentInformation objPaymentDetails = new ClsPaymentInformation();

                                    objPaymentDetails.BookingNo = objBooking.BookingNo.ToStr().Trim();

                                    objPaymentDetails.Price = Price;

                                    objPaymentDetails.Total = Price.ToStr();

                                    objPaymentDetails.PaymentGatewayID = obj.PaymentGatewayId.ToStr();
                                    objPaymentDetails.TokenDetails = objBooking.CustomerCreditCardDetails.ToStr();
                                    objPaymentDetails.BookingId = objBooking.Id.ToStr();


                                    try
                                    {
                                        if (objBooking.CustomerId != null && objBooking.CustomerCreditCardDetails.ToStr().Trim().StartsWith("pm_"))
                                            objPaymentDetails.StripeCustomerId = db.Customers.Where(c => c.Id == objBooking.CustomerId).Select(c => c.CreditCardDetails).FirstOrDefault().ToStr();
                                    }
                                    catch
                                    {

                                    }
                                    string response = MakePayment(obj, objPaymentDetails);




                                    if (response.StartsWith("success:"))
                                    {
                                        response = response.ToLower().Replace("authcode:", "").Trim();

                                        makePaymentResponse.TransactionId = response.Replace("success:", "").Trim();
                                        makePaymentResponse.IsTransactionSuccess = true;
                                        makePaymentResponse.TransactionMessage = "Payment Successfull";


                                    }
                                    else
                                    {
                                        makePaymentResponse.IsTransactionSuccess = false;
                                        makePaymentResponse.TransactionMessage = response.ToLower().Replace("failed:", "").Trim();

                                    }
                                }
                            }

                            else
                            {
                                makePaymentResponse.TransactionId = "success:" + objBookingPayment.AuthCode.ToStr();
                            }
                        }
                        ///
                        else if (gatewayName == "JudoURL")
                        {



                            makePaymentResponse.Gateways.Add(new { GatewayName = "Judo", Url = "https://eurosofttech-api.co.uk/3dspayment/judopayapi/collectpayment" });
                            makePaymentResponse.ResponseType = (int)ResponseType.Url;
                            makePaymentResponse.IsTransactionSuccess = true;
                            makePaymentResponse.TransactionId = "                                                                                           ";

                            makePaymentResponse.TransactionUrl = "";
                        }
                        else if (!string.IsNullOrEmpty(gatewayName) && (gatewayName.ToLower().Trim() == "konnectpay" || item.PaymentGatewayId == 15))
                        {
                            try
                            {
                                makePaymentResponse.KonnectAccId = db.Gen_SysPolicy_PaymentDetails.Where(c => c.PaymentGatewayId == 15).Select(c => c.PaypalID).FirstOrDefault();


                            }
                            catch { }


                            long jobId = input.jobId.ToLong();
                            bool IsAuthorize = false;
                            JavaScriptSerializer s = new JavaScriptSerializer();
                            Taxi_Model.Booking objBooking = General.GetObject<Taxi_Model.Booking>(c => c.Id == jobId);
                            decimal Price = input.Fare;
                            var objBookingPayment = General.GetObject<Booking_Payment>(c => c.BookingId == jobId && c.AuthCode != null && c.AuthCode != "");
                            try
                            {
                                File.AppendAllText(AppContext.BaseDirectory + "\\" + "requestMakePayment_KonenctPay.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " , Booking obj:" + new JavaScriptSerializer().Serialize(objBooking) + Environment.NewLine);
                            }
                            catch
                            {
                            }

                            if (objBookingPayment == null || objBookingPayment?.AuthCode.ToStr().Trim().Length == 0)
                            {
                                Gen_SysPolicy_PaymentDetail obj = null;

                                makePaymentResponse.isKonnectPayEnable = "1";

                                if (!string.IsNullOrEmpty(objBooking.CustomerCreditCardDetails) && (objBooking.CustomerCreditCardDetails.ToStr().Trim().StartsWith("pm_") || objBooking.CustomerCreditCardDetails.ToStr().Trim().StartsWith("cus_")) && !objBooking.CustomerCreditCardDetails.ToStr().Trim().Contains("secret_"))
                                {
                                    Customer objcustomer = General.GetObject<Customer>(c => c.MobileNo == objBooking.CustomerMobileNo && c.Id == objBooking.CustomerId);
                                    CustomerCardDetails CardDetails = null;
                                    CardDetails = GetCardDetailsKP(objcustomer?.Id, objBooking?.CustomerCreditCardDetails);
                                    obj = General.GetObject<Gen_SysPolicy_PaymentDetail>(c => c.PaymentGatewayId == 15);
                                    IsAuthorize = true;
                                    // makePaymentResponse.Gateways.Add(new { GatewayName = "konnectpay" });
                                    makePaymentResponse.ResponseType = (int)ResponseType.Direct;
                                    try
                                    {
                                        File.AppendAllText(AppContext.BaseDirectory + "\\" + "requestMakePayment_KonenctPay.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",card is registered for Booking  : " + mesg + " booking-details:" + new JavaScriptSerializer().Serialize(objBooking) + Environment.NewLine);
                                    }
                                    catch
                                    {
                                    }
                                    if (obj != null)
                                    {

                                        ClsPaymentInformation objPaymentDetails = new ClsPaymentInformation();

                                        objPaymentDetails.BookingNo = objBooking.BookingNo.ToStr().Trim();

                                        objPaymentDetails.Price = Price;

                                        objPaymentDetails.Total = Price.ToStr();

                                        objPaymentDetails.PaymentGatewayID = obj.PaymentGatewayId.ToStr();
                                        objPaymentDetails.TokenDetails = objBooking.CustomerCreditCardDetails.ToStr();
                                        objPaymentDetails.BookingId = objBooking.Id.ToStr();

                                        if (objcustomer != null)
                                        { objPaymentDetails.StripeCustomerId = objcustomer?.CreditCardDetails; }

                                        //Calling New Payment Method for stripe KonnectPay
                                        string response = MakePaymentCardTokenKonnectPay(obj, objPaymentDetails, objBooking);
                                        makePaymentResponse.ResponseType = (int)ResponseType.Direct;
                                        if (!string.IsNullOrEmpty(response) && response.StartsWith("success:"))
                                        {
                                            makePaymentResponse.TransactionId = response.Replace("success:", "").Trim();
                                            makePaymentResponse.IsTransactionSuccess = true;
                                            makePaymentResponse.ResponseType = (int)ResponseType.Direct;
                                            makePaymentResponse.TransactionMessage = "Payment Successfull";

                                            res.Data = Newtonsoft.Json.JsonConvert.SerializeObject(makePaymentResponse);
                                            res.IsSuccess = true;
                                            res.Message = makePaymentResponse.TransactionMessage;

                                        }
                                        else
                                        {
                                            makePaymentResponse.IsTransactionSuccess = false;
                                            makePaymentResponse.TransactionMessage = response?.ToLower().Replace("failed:", "").Trim();
                                            makePaymentResponse.TransactionId = "";


                                            dynamic ConnectGatewayDetails = GetKonnectPayGatewayDetails(obj, input.driverId);//Getting PAYMENT OPTIONS for KonnectPay 
                                            if (ConnectGatewayDetails != null) { makePaymentResponse.Gateways.AddRange(ConnectGatewayDetails); }
                                            makePaymentResponse.ResponseType = (int)ResponseType.MultipleGateways;
                                            res.Message = makePaymentResponse.TransactionMessage;
                                            res.Data = Newtonsoft.Json.JsonConvert.SerializeObject(makePaymentResponse);
                                            res.IsSuccess = true;

                                        }
                                        try
                                        {
                                            File.AppendAllText(AppContext.BaseDirectory + "\\" + "requestMakePayment_KonenctPay.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",KonnectPay Response : " + response + " returnObj:" + new JavaScriptSerializer().Serialize(makePaymentResponse) + Environment.NewLine);
                                        }
                                        catch
                                        {
                                        }
                                        return res;

                                    }

                                }

                                else
                                {
                                    if (objBooking.CustomerCreditCardDetails.ToStr().Trim().StartsWith("pi_") && objBooking.CustomerCreditCardDetails.ToStr().Trim().Contains("secret_"))
                                    {

                                        db.ExecuteQuery<int>("update booking set CustomerCreditCardDetails='',PaymentComments='' where id=" + jobId);

                                    }


                                    IsAuthorize = false;
                                    //setting up payment options for Konnect pay
                                    Gen_SysPolicy_PaymentDetail KPSettings = General.GetObject<Gen_SysPolicy_PaymentDetail>(c => c.SysPolicyId != 0 && (c.PaymentGatewayId == 15));
                                    dynamic ConnectGatewayDetails = GetKonnectPayGatewayDetails(KPSettings, input.driverId);//Getting PAYMENT OPTIONS for KonnectPay 
                                    if (ConnectGatewayDetails != null) { makePaymentResponse.Gateways.AddRange(ConnectGatewayDetails); }
                                    makePaymentResponse.ResponseType = (int)ResponseType.MultipleGateways;

                                }



                            }

                            else
                            {
                                // makePaymentResponse.ResponseType = (int)ResponseType.Direct;
                                makePaymentResponse.TransactionId = "success:" + objBookingPayment.AuthCode.ToStr();
                            }

                        }


                        if (paymentGateway != null && paymentGateway.Count() > 1 && (gatewayName.ToLower().Trim() != "konnectpay" && item.PaymentGatewayId != 15))
                        { makePaymentResponse.ResponseType = (int)ResponseType.MultipleGateways; }


                    }














                    res.Data = Newtonsoft.Json.JsonConvert.SerializeObject(makePaymentResponse);
                    res.IsSuccess = true;
                    res.Message = "";
                }
                catch (Exception ex)
                {
                    res.IsSuccess = false;
                    res.Message = "exceptionOccured " + ex.Message;
                    ///log.Error(ex.Message, ex);
                    ///
                    try
                    {
                        File.AppendAllText(AppContext.BaseDirectory + "\\requestMakePayment_exception.txt", DateTime.Now.ToStr() + ex.Message + Environment.NewLine);
                    }
                    catch
                    {
                    }
                }
                ///return Ok(resp);
                ///
            }
            return res;
        }



        private string MakePayment(Gen_SysPolicy_PaymentDetail obj, ClsPaymentInformation objCard)
        {
            string rtn = string.Empty;

            try
            {



                if (obj.PaymentGatewayId.ToInt() == Enums.PAYMENT_GATEWAY.JUDO)
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
                else if (obj.PaymentGatewayId.ToInt() == Enums.PAYMENT_GATEWAY.STRIPE) // stripe
                {

                    string json = string.Empty;
                    try
                    {

                        try
                        {


                            File.AppendAllText(physicalPath + "\\stripe.txt", DateTime.Now.ToStr() + ": request" + objCard.BookingId.ToStr() + ",token:" + objCard.TokenDetails.ToStr() + ",response:" + rtn.ToStr() + Environment.NewLine);
                        }
                        catch
                        {


                        }




                        if (objCard.TokenDetails.ToStr().StartsWith("pm_"))
                        {


                            try
                            {


                                File.AppendAllText(physicalPath + "\\stripe_pmintent.txt", DateTime.Now.ToStr() + ": request" + objCard.BookingId.ToStr() + ",token:" + objCard.TokenDetails.ToStr() + ",response:" + rtn.ToStr() + Environment.NewLine);
                            }
                            catch
                            {


                            }

                            try
                            {

                                string url = "http://api-eurosofttech.co.uk/WDStripe3DS/api/PaymentProcess-for-registercard-cloud";
                                //   string url = "https://www.api-eurosofttech.co.uk/stripeserverapi/createpaymentintent/";
                                string result = "";




                                var payload = new
                                {

                                    Amount = Math.Round((objCard.Total.ToDecimal() * 100), 0).ToInt(),
                                    PaymentMethod = objCard.TokenDetails.ToStr(),
                                    Currency = "GBP",
                                    APISecret = obj.PaypalID.ToStr(),
                                    CustomerId = objCard.StripeCustomerId.ToStr(),
                                    Description = objCard.BookingNo.ToStr() + "/" + objCard.BookingId + "/" + HubProcessor.Instance.objPolicy.DefaultClientId.ToStr()
                                };

                                Stripe3DS st = new Stripe3DS();
                                using (var client = new System.Net.Http.HttpClient())
                                {
                                    var dataString = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
                                    var BASE_URL = url;
                                    client.BaseAddress = new Uri(BASE_URL);
                                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));

                                    var postTask = client.PostAsync(BASE_URL + "?data=" + dataString, null).Result;
                                    result = postTask.Content.ReadAsStringAsync().Result;

                                    st = new JavaScriptSerializer().Deserialize<Stripe3DS>(result);


                                    if (st.IsSuccess)
                                    {
                                        rtn = "success:" + st.paymentIntentId.ToStr();
                                    }
                                    else
                                        rtn = "failed:" + st.Message.ToStr();


                                    try
                                    {


                                        File.AppendAllText(physicalPath + "\\stripepayment_intent.txt", DateTime.Now.ToStr() + ", URL :" + url + Environment.NewLine + ": json:" + dataString + Environment.NewLine + "bookingno:" + objCard.BookingNo.ToStr() + Environment.NewLine + ",response:" + result.ToStr() + Environment.NewLine);
                                    }
                                    catch
                                    {


                                    }
                                }





                            }
                            catch (Exception ex)
                            {

                                rtn = "failed:" + ex.Message.ToStr();

                                try
                                {


                                    File.AppendAllText(physicalPath + "\\stripe_pmintentexception.txt", DateTime.Now.ToStr() + ": request" + objCard.BookingId.ToStr() + ",token:" + objCard.TokenDetails.ToStr() + ",response:" + rtn.ToStr() + ",exception:" + ex.Message + Environment.NewLine);
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


                                File.AppendAllText(physicalPath + "\\stripepayment_directpayment.txt", DateTime.Now.ToStr() + ": json" + json + Environment.NewLine + "bookingno:" + objCard.BookingNo.ToStr() + Environment.NewLine + Environment.NewLine);
                            }
                            catch
                            {


                            }



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

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestPaymentDetailSummary")]//already exist, make it change by returning ResponseData model as return type, and object as parameter
        public ResponseData requestPaymentDetailSummary(string mesg)
        {
            ResponseData res = new ResponseData();
            try
            {

                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\requestPaymentDetailSummary.txt", DateTime.Now.ToStr() + ": request :" + mesg + Environment.NewLine);
                }
                catch (Exception ex)
                {



                }



                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                long jobId = values[1].ToLong();
                int driverId = values[2].ToInt();


                JobAction objAction = new JavaScriptSerializer().Deserialize<JobAction>(values[4].Trim());

                Taxi_Model.Booking objBooking = null;
                string response = string.Empty;
                Gen_PaymentColumnSetting objPaymentColumns = null;
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




                            decimal surchargeValue = Instance.objPolicy.CreditCardExtraCharges.ToDecimal();
                            decimal amount = objBooking.FareRate.ToDecimal();
                            string surchargeType = Instance.objPolicy.CreditCardChargesType.ToInt() == 1 ? "Amount" : "Percent";
                            //string surchargeType = "Percent";


                            ////


                            decimal price = objBooking.FareRate.ToDecimal();
                            //
                            decimal parking = objBooking.CongtionCharges.ToDecimal();
                            decimal waiting = objBooking.MeetAndGreetCharges.ToDecimal();



                            if (objBooking.CompanyId != null)
                            {
                                price = objBooking.CompanyPrice.ToDecimal();
                                parking = objBooking.ParkingCharges.ToDecimal();
                                waiting = objBooking.WaitingCharges.ToDecimal();
                            }








                            if (objAction.IsMeter.ToStr() == "1")
                            {
                                var enableFareMeterOnDriverPDA = db.Fleet_Driver_PDASettings.Where(c => c.DriverId == driverId).Select(x => x.EnableFareMeter).FirstOrDefault().ToBool();
                                if (enableFareMeterOnDriverPDA && objAction.QuotedPrice != "1")
                                {
                                    price = objAction.Fares.ToDecimal();
                                }
                                parking = objAction.ParkingCharges.ToDecimal();
                                waiting = objAction.WaitingCharges.ToDecimal();


                            }



                            List<BookingSummary> listofsummary = new List<BookingSummary>();
                            listofsummary.Add(new BookingSummary { fieldname = "Fares", isedit = EditFares.ToBool(), isvisible = objPaymentColumns.ShowFares.ToBool(), label = "Fares", value = price, DisableChangePayment = "1" });
                            listofsummary.Add(new BookingSummary { fieldname = "Parking", isedit = EditParking.ToBool(), isvisible = objPaymentColumns.ShowFares.ToBool(), label = "Parking", value = parking });
                            listofsummary.Add(new BookingSummary { fieldname = "Waiting", isedit = EditWaiting.ToBool(), isvisible = objPaymentColumns.ShowFares.ToBool(), label = "Waiting", value = waiting });
                            listofsummary.Add(new BookingSummary { fieldname = "ExtraDropCharges", isedit = true, isvisible = true, label = "Extras", value = objAction.ExtraDropCharges.ToDecimal() });
                            if (!string.IsNullOrEmpty(objAction.EnableDropOffAction) && objAction.EnableDropOffAction != "0")
                            {
                                if (objAction.PaymentType.ToLower() == "credit card")
                                {
                                    listofsummary.Add(new BookingSummary { fieldname = "Tip", isedit = true, isvisible = true, label = "Tip", value = 0 });
                                }

                            }
                            else
                            {
                                listofsummary.Add(new BookingSummary { fieldname = "Tip", isedit = true, isvisible = true, label = "Tip", value = 0 });
                            }

                            //      listofsummary.Add(new BookingSummary { fieldname = "Surcharge", isedit = false, isvisible = true, label = "Surcharge", value = 0.3m });


                            //     listofsummary.Add(new BookingSummary { fieldname = "Tip", isedit = true, isvisible = true, label = "Tip", value = 0 });
                            listofsummary.Add(new BookingSummary { fieldname = "BookingFee", isedit = false, isvisible = true, label = "BookingFee", value = objAction.BookingFee.ToDecimal() });

                            response = Newtonsoft.Json.JsonConvert.SerializeObject(listofsummary);
                            //response = "{ \"JobId\" :\"" + jobId.ToStr() +
                            //                        "\", \"ShowFares\":\"" + ShowFares +
                            //                        "\", \"EditFares\":\"" + EditFares +
                            //                        "\", \"ShowWaiting\":\"" + ShowWaiting.ToStr() +
                            //                        "\", \"ShowParking\":\"" + ShowParking.ToStr() +
                            //                         "\", \"ShowBookingFee\":\"" + "false" +
                            //                          "\", \"ShowExtraDropCharges\":\"" + "true" +

                            //                        "\", \"EditParking\":\"" + EditParking +
                            //                        "\", \"EditWaiting\":\"" + EditWaiting +

                            //                         "\", \"BookingFee\":\"" + objAction.BookingFee.ToDecimal() +
                            //                          "\", \"ExtraDropCharges\":\"" + objAction.ExtraDropCharges.ToDecimal() +

                            //                        "\", \"ShowSurcharge\":\"" + ShowSurcharge + "\"," +
                            //                        "\"SurchargeType\":\"" + surchargeType + "\"," +
                            //                        "\"ShowTip\":\"" + ShowTip + "\"" +
                            //                        ",\"ShowTotal\":\"" + ShowTotal + "\",\"Fares\":\"" + price + " " + "\",\"Parking\":\"" + parking + " " + "\",\"Waiting\":\"" + waiting + " " + "\",\"Surcharge\":\"" + surchargeValue + "\"";


                            //var objPay = db.Gen_SysPolicy_PaymentDetails.FirstOrDefault(c => c.EnableMobileIntegration == true);




                            //string paymentGateway = string.Empty;

                            //if (objPay != null)
                            //{

                            //    string gatewayName = string.Empty;



                            //    int gatewayId = objPay.PaymentGatewayId.ToInt();

                            //    if (gatewayId == 8)
                            //        gatewayName = "paypalhere";
                            //    else if (gatewayId == 9)
                            //        gatewayName = "sumup";



                            //    if (objBooking.CustomerCreditCardDetails.ToStr().Trim().Length > 0)
                            //    {
                            //        gatewayName = "direct=" + gatewayName;

                            //    }



                            //    paymentGateway = ",\"Gateway\":\"" + gatewayName + "\"" + ",\"UserName\":\"" + objPay.MerchantID + "\"" + ",\"Password\":\"" + objPay.MerchantPassword + "\",\"affiliatekey\":\"" + objPay.PaypalID.ToStr() + "\",\"tokendetails\":\"" + objBooking.CustomerCreditCardDetails.ToStr().Trim() + "\"";



                            //}
                            //else
                            //{

                            //    string gatewayName = string.Empty;
                            //    if (objBooking.CustomerCreditCardDetails.ToStr().Trim().Length > 0)
                            //    {
                            //        gatewayName = "direct";

                            //    }
                            //    paymentGateway = ",\"Gateway\":\"" + gatewayName + "\"" + ",\"UserName\":\"" + "" + "\"" + ",\"Password\":\"" + "" + "\",\"affiliatekey\":\"" + "" + "\",\"tokendetails\":\"" + objBooking.CustomerCreditCardDetails.ToStr().Trim() + "\"";



                            //}



                            //response += paymentGateway + " }";


                            res.Data = response;
                            res.IsSuccess = true;





                        }
                    }
                    catch
                    {
                        response = "failed:Problem on getting details from server";
                        res.Data = response;
                        res.IsSuccess = false;
                    }
                }


            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
                res.Data = "";
                res.IsSuccess = false;
            }

            return res;
        }

        #region KonnectPay

        private CustomerCardDetails GetCardDetailsKP(int? CustomerId, string CardToken)
        {
            CustomerCardDetails CCDetail = new CustomerCardDetails();

            try
            {
                KonnectCardDetails CardDetails = new KonnectCardDetails();
                using (TaxiDataContext db = new TaxiDataContext())
                {

                    CardDetails = db.ExecuteQuery<KonnectCardDetails>("select Id,CCDetails,PaymentMethodId,Lastfour,ExpiryMonth,ExpiryYear,Brand,FingerPrint from Customer_CCDetails where customerId=" + CustomerId.ToInt() + "and PaymentMethodId=" + CardToken.Trim()).LastOrDefault();

                }
                if (CardDetails != null && CardDetails.Lastfour > 0 && CardDetails.ExpiryMonth > 0 && CardDetails.ExpiryYear > 0)
                {
                    CCDetail.CustomerCardToken = CardDetails.PaymentMethodId;
                    CCDetail.Lastfour = CardDetails.Lastfour.ToStr();
                    CCDetail.expiry = CardDetails.ExpiryMonth + "/" + CardDetails.ExpiryYear;
                    CCDetail.cardType = CardDetails.Brand;

                }
            }
            catch
            {
                try
                {
                    ClsCCDetails CardDetails = new ClsCCDetails();
                    using (TaxiDataContext db = new TaxiDataContext())
                    {

                        CardDetails = db.ExecuteQuery<ClsCCDetails>("select Id,CCDetails,IsDefault from Customer_CCDetails where customerId=" + CustomerId.ToInt() + "and CCDetails like '%" + CardToken + "%'").FirstOrDefault();

                    }

                    if (CardDetails != null && !string.IsNullOrEmpty(CardDetails.CCDetails) && CardDetails.CCDetails.ToLower().Contains("konnectpaytoken"))
                    {
                        var CCDetails = CardDetails.CCDetails.Split('|');
                        string Token = CCDetails[0];
                        if (!string.IsNullOrEmpty(Token)) { CCDetail.CustomerCardToken = Token.Replace("KonnectPayToken:", "").Trim(); }
                        if (!string.IsNullOrEmpty(CCDetails[1]))
                        {
                            var CardInfo = CCDetails[1].Split(',');
                            if (CardInfo != null)
                            {
                                if (CardInfo[0] != null && CardInfo[0].ToLower().Contains("last four"))
                                {
                                    CCDetail.Lastfour = CardInfo[0].Replace("last four", "");
                                    CCDetail.Lastfour = CCDetail.Lastfour.Replace(":", "").Trim();
                                }
                                if (CardInfo[1] != null && CardInfo[1].ToLower().Contains("expiry"))
                                {
                                    CCDetail.expiry = CardInfo[1].Replace("expiry", "");
                                    CCDetail.expiry = CCDetail.expiry.Replace(":", "").Trim();


                                }
                                if (CardInfo[2] != null && CardInfo[2].ToLower().Contains("cardtype"))
                                {
                                    CCDetail.cardType = CardInfo[2].Replace("cardType", "");
                                    CCDetail.cardType = CCDetail.cardType.Replace(":", "").Trim();
                                }


                            }
                        }
                    }

                }
                catch
                {

                }
            }

            try
            {
                File.AppendAllText(AppContext.BaseDirectory + "\\GetCardDetailsKP.txt", DateTime.Now.ToStr() + ",response:" + new JavaScriptSerializer().Serialize(CCDetail) + Environment.NewLine);
            }
            catch
            {

            }
            return CCDetail;
        }

        private string MakePaymentCardTokenKonnectPay(Gen_SysPolicy_PaymentDetail obj, ClsPaymentInformation objCard, Booking objBooking)
        {
            string rtn = string.Empty;
            string DefaultCurrency = System.Configuration.ConfigurationManager.AppSettings["DefaultCurrency"];

            string DefaultClientLocation = System.Configuration.ConfigurationManager.AppSettings["DefaultClientLocation"];
            PaymentCaptureResponse resp = new PaymentCaptureResponse();
            string json = string.Empty;
            try
            {
                if (objCard.TokenDetails.ToStr().StartsWith("pm_") || objCard.TokenDetails.ToStr().StartsWith("cus_"))
                {

                    try
                    {
                        File.AppendAllText(physicalPath + "\\MakePaymentCardTokenKonnectPay.txt", DateTime.Now.ToStr() + ": request" + objCard.BookingId.ToStr() + ",token:" + objCard.TokenDetails.ToStr() + ",payment dto:" + new JavaScriptSerializer().Serialize(objCard) + ",gatway obj:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
                    }
                    catch { }

                    if (string.IsNullOrEmpty(obj?.ApplicationId) || string.IsNullOrEmpty(obj?.PaypalID))
                    {
                        rtn = "failed: gateway details not found!";
                        return rtn;
                    }

                    StripePaymentRequestDto st = new StripePaymentRequestDto();
                    int amount = Math.Round((Convert.ToDouble(objCard.Price) * 100), 0).ToInt();

                    st.isAuthorized = false;
                    st.bookingId = objBooking.Id;
                    st.description = objBooking?.Gen_SubCompany?.CompanyName + " | " + objBooking?.BookingNo.ToStr() + " | " + "Fares : " + objCard?.Price + " " + DefaultCurrency;
                    st.bookingRef = objBooking?.BookingNo.ToStr();
                    st.countryId = obj.ApplicationId.ToInt();
                    st.connectedAccountId = obj.PaypalID.ToStr();
                    st.applicationFee = 0;
                    st.otherCharges = 0;
                    st.key = "";
                    st.secret = "";
                    st.amount = Convert.ToInt64(objCard.Price * 100);// amount.ToInt();
                    st.ReturnAmount = 0;
                    st.displayAmount = objCard.Price.ToDecimal();
                    st.currency = DefaultCurrency;
                    st.defaultClientId = HubProcessor.Instance.objPolicy.DefaultClientId.ToStr();
                    st.location = DefaultClientLocation;
                    st.companyName = objBooking?.Gen_SubCompany?.CompanyName ?? "";
                    st.paymentMethodId = objCard.TokenDetails.ToStr();
                    st.customerId = objCard?.StripeCustomerId.ToStr();//"";
                    st.customerName = objBooking.CustomerName.ToStr();
                    st.email = objBooking.CustomerEmail.ToStr();
                    st.phoneNumber = objBooking.CustomerMobileNo.ToStr();
                    st.lastfour = objCard?.CardNumber ?? "";
                    st.expiry = objCard?.ExpiryDate ?? "";
                    st.cardtype = objCard?.CardType ?? "";
                    var DataObj = Newtonsoft.Json.JsonConvert.SerializeObject(st);
                    string StripeAPIBaseURL = System.Configuration.ConfigurationManager.AppSettings["StripeAPIBaseURL"];
                    using (var client = new HttpClient())
                    {
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        client.BaseAddress = new Uri(StripeAPIBaseURL);
                        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                        var content = new StringContent(DataObj, Encoding.UTF8, "application/json");
                        var postTask = client.PostAsync(StripeAPIBaseURL + "/v1/PaymentWithExistingCustomer", content).Result;
                        string PayResp = postTask.Content.ReadAsStringAsync().Result;
                        resp = new JavaScriptSerializer().Deserialize<PaymentCaptureResponse>(PayResp);

                    }

                    if (resp.isSuccess)
                    {

                        rtn = "success:" + resp.paymentintentId.ToStr();
                        try
                        {
                            if (!string.IsNullOrEmpty(resp.paymentintentId))
                            {
                                using (TaxiDataContext db = new TaxiDataContext())
                                {
                                    if ((db.ExecuteQuery<int>("select count(*) from booking_payment where bookingId=" + objBooking.Id).FirstOrDefault()) == 0)
                                    {
                                        db.ExecuteQuery<int>("insert into Booking_Payment(bookingid,PaymentGatewayId,AuthCode,Status,TotalAmount)VALUES(" + objBooking.Id + ",15,'" + resp.paymentintentId.ToStr() + "','Paid'," + objCard.Price + ")");
                                    }
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                    else
                    { rtn = "failed:" + resp.error.ToStr(); }

                    try
                    {
                        File.AppendAllText(physicalPath + "\\MakePaymentCardTokenKonnectPay.txt", DateTime.Now.ToStr() + ":  json:" + new JavaScriptSerializer().Serialize(st) + Environment.NewLine + "bookingno:" + objCard.BookingNo.ToStr() + Environment.NewLine + ",response:" + new JavaScriptSerializer().Serialize(resp) + Environment.NewLine);
                    }
                    catch
                    {


                    }

                }

            }
            catch (Exception ex)
            {
                rtn = "failed:" + ex.Message.ToStr();

                try
                {
                    File.AppendAllText(physicalPath + "\\stripeKP_Authpayment_exception.txt", DateTime.Now.ToStr() + ": request" + objCard.BookingId.ToStr() + ",token:" + objCard.TokenDetails.ToStr() + ",response:" + new JavaScriptSerializer().Serialize(resp) + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {


                }

            }



            return rtn;
        }

        /// <summary>
        /// This Method is to make payment for Konnect Pay Account
        /// </summary>
        private string MakePaymentKonnectPay(Gen_SysPolicy_PaymentDetail obj, ClsPaymentInformation objCard, Booking objBooking, bool IsAuthorize)
        {
            string rtn = string.Empty;
            string DefaultCurrency = System.Configuration.ConfigurationManager.AppSettings["DefaultCurrency"];

            string DefaultClientLocation = System.Configuration.ConfigurationManager.AppSettings["DefaultClientLocation"];
            PaymentCaptureResponse resp = new PaymentCaptureResponse();
            string json = string.Empty;
            try
            {
                if (objCard.TokenDetails.ToStr().StartsWith("pi_") && IsAuthorize)
                {

                    try
                    {
                        File.AppendAllText(physicalPath + "\\stripeKP_Authpayment.txt", DateTime.Now.ToStr() + ": request" + objCard.BookingId.ToStr() + ",token:" + objCard.TokenDetails.ToStr() + ",payment dto:" + new JavaScriptSerializer().Serialize(objCard) + ",gatway obj:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
                    }
                    catch { }

                    if (string.IsNullOrEmpty(obj?.ApplicationId) || string.IsNullOrEmpty(obj?.PaypalID))
                    {
                        rtn = "failed: gateway details not found!";
                        return rtn;
                    }

                    PaymentCaptureDto st = new PaymentCaptureDto();
                    int amount = Math.Round((Convert.ToDouble(objCard.Price) * 100), 0).ToInt();

                    st.bookingId = objBooking.Id;
                    st.description = objBooking?.Gen_SubCompany?.CompanyName + " | " + objBooking?.BookingNo.ToStr() + " | " + "Fares : " + objCard?.Price + " " + DefaultCurrency;
                    st.bookingRef = objBooking?.BookingNo.ToStr();
                    st.countryId = obj.ApplicationId.ToInt();
                    st.connectedAccountId = obj.PaypalID.ToStr();
                    st.applicationFee = 0;
                    st.otherCharges = 0;
                    st.key = "";
                    st.secret = "";
                    st.amount = Convert.ToInt64(objCard.Price * 100);// amount.ToInt();
                    st.displayAmount = objCard.Price.ToDecimal();
                    st.currency = DefaultCurrency;
                    st.defaultClientId = HubProcessor.Instance.objPolicy.DefaultClientId.ToStr();
                    st.location = DefaultClientLocation;
                    st.companyName = objBooking?.Gen_SubCompany?.CompanyName ?? "";
                    st.paymentIntentId = objBooking.CustomerCreditCardDetails.ToStr();
                    st.status = objBooking?.CompanyCreditCardDetails.ToStr();//"";
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
                        resp = new JavaScriptSerializer().Deserialize<PaymentCaptureResponse>(PayResp);

                    }

                    if (resp.isSuccess)
                    {
                        rtn = "success:" + resp.paymentId.ToStr();
                    }
                    else
                    { rtn = "failed:" + resp.error.ToStr(); }

                    try
                    {
                        File.AppendAllText(physicalPath + "\\stripeKP_Authpayment.txt", DateTime.Now.ToStr() + Environment.NewLine + ":API(CapturePayment/IncrementAuthorization)  json:" + new JavaScriptSerializer().Serialize(st) + Environment.NewLine + "bookingno:" + objCard.BookingNo.ToStr() + Environment.NewLine + ",response:" + new JavaScriptSerializer().Serialize(resp) + Environment.NewLine);
                    }
                    catch
                    {


                    }

                }

            }
            catch (Exception ex)
            {
                rtn = "failed:" + ex.Message.ToStr();

                try
                {
                    File.AppendAllText(physicalPath + "\\stripeKP_Authpayment_exception.txt", DateTime.Now.ToStr() + ": request" + objCard.BookingId.ToStr() + ",token:" + objCard.TokenDetails.ToStr() + ",response:" + new JavaScriptSerializer().Serialize(resp) + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {


                }

            }



            return rtn;
        }


        /// <summary>
        /// This Method is to render Payment Options for Konnect Pay
        /// </summary>
        private dynamic GetKonnectPayGatewayDetails(Gen_SysPolicy_PaymentDetail obj, long DriverID)
        {
            dynamic resp = null;
            try
            {
                var KPOptions = (dynamic)null;

                List<dynamic> ConnectGateWayType = new List<dynamic>();

                if (obj != null)
                {
                    StripeLocation LocationObj = new StripeLocation();
                    string TokenAPIURL = string.Empty;
                    string ClientLocationId = string.Empty;
                    string StripeError = string.Empty;
                    string StripeAccountId = obj.PaypalID;
                    string StripeCountryId = obj.ApplicationId;
                    string StripeAPIBaseURL = System.Configuration.ConfigurationManager.AppSettings["StripeAPIBaseURL"];



                    if (!string.IsNullOrEmpty(StripeAccountId) && !string.IsNullOrEmpty(StripeCountryId))
                    {
                        // get client Terminal list
                        StripeTerminalDTO StripeTerminals = GetTerminalList(StripeAccountId, StripeCountryId, StripeAPIBaseURL);

                        if (DriverID > 0 && StripeTerminals != null && StripeTerminals.isSuccess)
                        {
                            if (StripeTerminals?.terminals?.Count > 0)
                            {
                                ClientLocationId = StripeTerminals?.terminals[0].terminalLocationId;
                            }
                            else
                            {
                                StripeError = StripeTerminals.error;
                            }
                        }
                        else
                        {
                            StripeError = StripeTerminals?.error;
                        }

                        TokenAPIURL = StripeAPIBaseURL + "/v1/terminal/token/" + StripeAccountId.Trim() + "?CountryId=" + StripeCountryId;
                    }
                    string PaymentOptions = obj.MerchantPassword;
                    var KPOptionsList = PaymentOptions.Split(new[] { Environment.NewLine, "\n" }, StringSplitOptions.None);

                    if (KPOptionsList.ToStr().Length > 2) { KPOptions = KPOptionsList[2].Split(','); }
                    if (KPOptions != null)
                    {
                        foreach (string opt in KPOptions)
                        {

                            if (opt.ToLower().Contains("pay by link") || opt.ToLower().Contains("paybylink"))
                            {
                                ConnectGateWayType.Add(new { GatewayName = "KonnectPay", label = "Pay By Link", typeId = (int)GatewayTypesConnect.PayByLink, tokenRefreshURL = "", AccountId = StripeAccountId, LocationId = ClientLocationId, Url = "", Error = StripeError });
                            }
                            else if (opt.ToLower().Trim().Contains("terminal payment") || opt.ToLower().Trim().Contains("terminalpayment"))
                            {
                                ConnectGateWayType.Add(new { GatewayName = "KonnectPay", label = "Terminal Payment", typeId = (int)GatewayTypesConnect.CardTerminal, tokenRefreshURL = TokenAPIURL, AccountId = StripeAccountId, LocationId = ClientLocationId, Url = "", Error = StripeError });

                            }
                            else if (opt.ToLower().Trim().Contains("tap to pay") || opt.ToLower().Trim().Contains("taptopay"))
                            {
                                ConnectGateWayType.Add(new { GatewayName = "KonnectPay", label = "Tap to Pay", typeId = (int)GatewayTypesConnect.TapToPay, tokenRefreshURL = TokenAPIURL, AccountId = StripeAccountId, LocationId = ClientLocationId, Url = "", Error = StripeError });

                            }
                            else if (opt.ToLower().Trim().Contains("qr code") || opt.ToLower().Trim().Contains("qrcode"))
                            {
                                ConnectGateWayType.Add(new { GatewayName = "KonnectPay", label = "QR Code", typeId = (int)GatewayTypesConnect.QRCode, tokenRefreshURL = "", AccountId = StripeAccountId, LocationId = ClientLocationId, Url = "", Error = StripeError });

                            }
                        }
                    }
                    resp = ConnectGateWayType;

                    try
                    {
                        File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetKonnectPayGatewayDetails.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", json: " + new JavaScriptSerializer().Serialize(resp) + Environment.NewLine);
                    }
                    catch
                    {
                    }

                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(AppContext.BaseDirectory + "\\GetKonnectPayGatwayDetails_exception.txt", DateTime.Now.ToStr() + ex.Message + Environment.NewLine);
            }
            return resp;
        }

        /// <summary>
        /// This method is to get client Driver express accountid list Konnect pay
        /// </summary>
        private StripeTerminalDTO GetTerminalList(string StripeAccountId, string StripeCountryId, string StripeAPIBaseURL)
        {
            StripeTerminalDTO TerminalsList = new StripeTerminalDTO();
            try
            {
                if (!string.IsNullOrEmpty(StripeAPIBaseURL))
                {
                    using (var client = new System.Net.Http.HttpClient())
                    {
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));
                        string request = StripeAPIBaseURL + "/v1/terminal/location/" + StripeAccountId.Trim() + "?countryId=" + StripeCountryId;
                        var APICall = client.GetAsync(request).Result;
                        if (APICall.IsSuccessStatusCode) { }
                        var APIRespnse = APICall.Content.ReadAsStringAsync().Result;
                        if (APIRespnse != null)
                        {
                            TerminalsList = new JavaScriptSerializer().Deserialize<StripeTerminalDTO>(APIRespnse);

                        }
                    }

                }
            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\GetClientLocationId_exception.txt", DateTime.Now.ToStr() + ex.Message + Environment.NewLine);
                }
                catch
                {
                }
            }
            return TerminalsList;
        }
        /// <summary>
        /// This Method is for Creating Payment Link against JobId and send to customer
        /// </summary>
        /// <param name="mesg"></param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("CreatePaymentLinkKP")]
        public ResponseData CreatePaymentLinkKP(string mesg)
        {
            ResponseData resp = new ResponseData();

            string json = string.Empty;
            Gen_SysPolicy_PaymentDetail paymentGateway = null;
            try
            {
                File.AppendAllText(physicalPath + "\\CreatePaymentLinkKonnectPay.txt", DateTime.Now.ToStr() + ": request" + mesg.ToStr());
            }
            catch { }
            try
            {

                StripeKonnectPaymentRequestModel input = new JavaScriptSerializer().Deserialize<StripeKonnectPaymentRequestModel>(mesg);
                if (input.jobId <= 0)
                {
                    resp.IsSuccess = false;
                    resp.Message = "Invalid Job Id!";
                    return resp;
                }
                if (input.sendType == "1" && string.IsNullOrEmpty(input.email))
                {
                    resp.IsSuccess = false;
                    resp.Message = "Email is Required!";
                    return resp;

                }
                else if (input.sendType == "2" && string.IsNullOrEmpty(input.mobileno))
                {
                    resp.IsSuccess = false;
                    resp.Message = "Mobile Number is Required!";
                    return resp;

                }

                decimal Price = input.Fare;
                Taxi_Model.Booking objBooking = General.GetObject<Taxi_Model.Booking>(c => c.Id == input.jobId);

                using (TaxiDataContext db = new TaxiDataContext())
                {

                    paymentGateway = General.GetObject<Gen_SysPolicy_PaymentDetail>(c => c.PaymentGatewayId == 15);
                    if (objBooking != null && paymentGateway != null)
                    {

                        resp = CreatePaymentLinkKonnectPay(paymentGateway, objBooking, input.sendType, input.mobileno, input.email, Price);

                    }
                    else
                    {
                        resp.IsSuccess = false;
                        resp.Message = "Booking info or gateway details are missing!";
                    }
                }

            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\CreatePaymentLinkKonnectPay_exception.txt", DateTime.Now.ToStr() + ",response:" + new JavaScriptSerializer().Serialize(resp) + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
                resp.IsSuccess = false;
                resp.Message = ex.Message;

            }

            return resp;
        }
        /// <summary>
        /// This Method is to Create payment link for konnect payment 
        /// </summary>
        private ResponseData CreatePaymentLinkKonnectPay(Gen_SysPolicy_PaymentDetail obj, Booking objBooking, string sendtype, string mobileno, string email, decimal Price)
        {
            ResponseData resp = new ResponseData();
            string DefaultCurrency = System.Configuration.ConfigurationManager.AppSettings["DefaultCurrency"];
            string DefaultClientLocation = System.Configuration.ConfigurationManager.AppSettings["DefaultClientLocation"];
            string json = string.Empty;
            try
            {

                try
                {
                    File.AppendAllText(physicalPath + "\\CreatePaymentLinkKonnectPay.txt", DateTime.Now.ToStr() + ": request" + objBooking.Id.ToStr());
                }
                catch
                {

                }
                //Get customer card token if exist.
                //CustomerCardDetails CardDetails = null;
                //CardDetails = GetCardTokenKP(objBooking?.Customer?.Id);
                Customer objcustomer = General.GetObject<Customer>(c => c.MobileNo == objBooking.CustomerMobileNo);
                CustomerCardDetails CardDetails = null;
                CardDetails = GetCardTokenKP(objcustomer?.Id);

                string baseUrl = System.Configuration.ConfigurationManager.AppSettings["huburl"].ToStr();
                Classes.KonnectSupplier.StripePaymentRequestDto SpDTO = new Classes.KonnectSupplier.StripePaymentRequestDto();
                SpDTO.isAuthorized = false;
                SpDTO.key = "";
                SpDTO.secret = "";
                SpDTO.countryId = obj.ApplicationId.ToInt();
                SpDTO.connectedAccountId = obj.PaypalID.Trim();
                SpDTO.applicationFee = 0;
                SpDTO.otherCharges = 0;
                SpDTO.bookingId = objBooking?.Id ?? 0;
                SpDTO.bookingRef = objBooking?.BookingNo ?? "";
                SpDTO.amount = (Price * 100).ToInt();//(objBooking.TotalCharges * 100).ToInt();
                SpDTO.displayAmount = Price.ToDecimal();
                SpDTO.currency = DefaultCurrency;
                SpDTO.description = objBooking?.Gen_SubCompany?.CompanyName + " | " + objBooking?.BookingNo.ToStr() + " | " + "Fares : " + Price + " " + DefaultCurrency;
                if (SpDTO.description.ToStr().Length > 150)
                {
                    SpDTO.description = HubProcessor.Instance.objPolicy.DefaultClientId.ToStr() + " | " + objBooking?.BookingNo.ToStr() + " | " + "Fares : " + Price + " " + DefaultCurrency;
                }
                SpDTO.paymentMethodId = CardDetails?.CustomerCardToken ?? "";
                SpDTO.customerId = objcustomer?.CreditCardDetails; //objBooking?.Customer?.CreditCardDetails;
                SpDTO.customerName = objBooking?.CustomerName.ToStr().Trim();
                SpDTO.email = "";
                SpDTO.phoneNumber = "";
                if (sendtype == "1")
                {
                    SpDTO.email = email?.Trim();
                }
                else
                {
                    SpDTO.phoneNumber = mobileno?.Trim();
                }
                SpDTO.lastfour = "";
                SpDTO.expiry = "";
                SpDTO.cardtype = "";
                SpDTO.companyName = objBooking?.Gen_SubCompany?.CompanyName ?? "";
                SpDTO.defaultClientId = HubProcessor.Instance.objPolicy.DefaultClientId.ToStr();
                SpDTO.location = DefaultClientLocation;
                SpDTO.verificationWebhook = "";
                SpDTO.paymentUpdateWebhook = "";// baseUrl;
                SpDTO.ReturnAmount = objBooking.ReturnFareRate.HasValue ? objBooking.ReturnFareRate.Value : 0;
                SpDTO.UpdatePaymentURL = baseUrl;
                SpDTO.OperatorName = "";
                SpDTO.PreAuthUrl = "";
                SpDTO.PayByDispatch = "0";
                if (!string.IsNullOrEmpty(obj.ApplicationId) && !string.IsNullOrEmpty(obj.PaypalID))
                {
                    json = Newtonsoft.Json.JsonConvert.SerializeObject(SpDTO);
                    var result = new SupplierController().SendPayByLinkDriverAppKP(json);

                    if (!string.IsNullOrEmpty(result) && !result.ToStr().Contains("false"))
                    {
                        resp.Message = "Payment Link Sent to Customer!";
                        resp.IsSuccess = true;
                        resp.Data = "";

                    }
                    else
                    {
                        resp.IsSuccess = false;
                        resp.Data = "";
                        resp.Message = "Error in Sending Payment Link!";
                    }

                }
                try
                {
                    File.AppendAllText(physicalPath + "\\CreatePaymentLinkKonnectPay.txt", DateTime.Now.ToStr() + ": json" + new JavaScriptSerializer().Serialize(SpDTO) + "bookingno:" + objBooking.BookingNo.ToStr() + ",response:" + new JavaScriptSerializer().Serialize(resp) + Environment.NewLine);
                }
                catch
                {

                }


            }
            catch (Exception ex)
            {
                resp.IsSuccess = false;
                resp.Message = ex.Message;
                resp.Data = "";
                try
                {


                    File.AppendAllText(physicalPath + "\\CreatePaymentLinkKonnectPay_exception.txt", DateTime.Now.ToStr() + ": token=" + new JavaScriptSerializer().Serialize(json) + "  ,bookingno:" + objBooking.BookingNo.ToStr() + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {


                }
            }
            return resp;
        }




        /// <summary>
        /// This Method is to create Payment Intent for Terminal
        /// </summary>
        /// <param name="mesg"></param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("CreateTerminalPaymentIntentKP")]
        public StripeAPIResponse CreateTerminalPaymentIntentKP(string mesg)
        {
            StripeAPIResponse resp = new StripeAPIResponse();
            string StripeAPIBaseURL = System.Configuration.ConfigurationManager.AppSettings["StripeAPIBaseURL"];
            string json = string.Empty;
            Gen_SysPolicy_PaymentDetail paymentGateway = null;
            try
            {
                File.AppendAllText(physicalPath + "\\CreateTerminalPaymentIntentKP.txt", DateTime.Now.ToStr() + ": request" + mesg.ToStr());
            }
            catch { }
            try
            {

                StripeKonnectPaymentRequestModel input = new JavaScriptSerializer().Deserialize<StripeKonnectPaymentRequestModel>(mesg);
                if (input.jobId <= 0)
                {
                    resp.IsSuccess = false;
                    resp.Message = "Invalid Job Id!";
                    return resp;
                }

                decimal Fare = input.Fare;
                Taxi_Model.Booking objBooking = General.GetObject<Taxi_Model.Booking>(c => c.Id == input.jobId);

                using (TaxiDataContext db = new TaxiDataContext())
                {

                    paymentGateway = General.GetObject<Gen_SysPolicy_PaymentDetail>(c => c.PaymentGatewayId == 15);
                    if (objBooking != null && paymentGateway != null)
                    {
                        if (!string.IsNullOrEmpty(input.serialnumber) && input.serialnumber.ToLower().Contains("terminal"))
                        {
                            StripeTerminalDTO StripeTerminals = GetTerminalList(paymentGateway.PaypalID, paymentGateway.ApplicationId, StripeAPIBaseURL);
                            if (StripeTerminals == null || StripeTerminals?.terminals.Count <= 0)
                            {

                                resp.IsSuccess = false;
                                resp.Message = "No Terminal is found!";
                                return resp;
                            }
                        }

                        resp = CreateTerminalPaymentIntentKonnectPay(paymentGateway, objBooking, Fare, input.serialnumber);

                    }
                    else
                    {
                        resp.IsSuccess = false;
                        resp.Message = "Booking info or gateway details are missing!";
                    }
                }

            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\CreateTerminalPaymentIntentKP_exception.txt", DateTime.Now.ToStr() + ",response:" + new JavaScriptSerializer().Serialize(resp) + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
                resp.IsSuccess = false;
                resp.Message = ex.Message;

            }

            return resp;
        }
        private StripeAPIResponse CreateTerminalPaymentIntentKonnectPay(Gen_SysPolicy_PaymentDetail obj, Booking objBooking, decimal Fare, string terminalDetails)
        {
            StripeAPIResponse response = new StripeAPIResponse();
            string DefaultCurrency = System.Configuration.ConfigurationManager.AppSettings["DefaultCurrency"];
            string DefaultCurrencySign = Global.DefaultCurrencySign; // System.Configuration.ConfigurationManager.AppSettings["DefaultCurrencySign"];
            string DefaultClientLocation = System.Configuration.ConfigurationManager.AppSettings["DefaultClientLocation"];
            string json = string.Empty;

            try
            {
                try
                {
                    File.AppendAllText(physicalPath + "\\CreateTerminalPaymentIntentKP.txt", DateTime.Now.ToStr() + ": request" + objBooking.Id.ToStr() + ",Payment settings :" + new JavaScriptSerializer().Serialize(obj) + ", terminal Details:" + terminalDetails);
                }
                catch { }
                if (!string.IsNullOrEmpty(objBooking?.CustomerCreditCardDetails) && objBooking.CustomerCreditCardDetails.ToStr().Trim().StartsWith("pi_"))
                {
                    response.Message = "Payment Intent is created successfully!";
                    response.IsSuccess = true;
                    response.Data = objBooking.CustomerCreditCardDetails;
                    return response;

                }
                string companyName = objBooking?.Gen_SubCompany?.CompanyName.ToStr();
                if (companyName.Contains("'"))
                {
                    companyName = companyName.Replace("'", "");
                }

                string Desc = companyName.ToStr() + " | " + objBooking?.BookingNo.ToStr() + " | " + "Fares : " + Fare.ToDecimal() + " " + DefaultCurrency + " |" + terminalDetails;
                if (Desc.Length > 150)
                {
                    string deviceName = string.Empty;
                    if (!string.IsNullOrEmpty(terminalDetails) && terminalDetails.Contains(":"))
                    {
                        var device = terminalDetails.Split(':');
                        deviceName = device[0];
                    }
                    Desc = companyName + " | " + objBooking?.BookingNo.ToStr() + " | " + "Fares : " + Fare.ToDecimal() + " " + DefaultCurrency + " | " + deviceName;
                }

                string baseUrl = System.Configuration.ConfigurationManager.AppSettings["huburl"].ToStr();
                Classes.KonnectSupplier.TerminalPaymentIntentRequestDto SpDTO = new Classes.KonnectSupplier.TerminalPaymentIntentRequestDto();
                PaymentIntentResponse resp = new PaymentIntentResponse();

                SpDTO.key = "";
                SpDTO.secret = "";
                SpDTO.countryId = obj.ApplicationId.ToInt();
                SpDTO.connectedAccountId = obj.PaypalID.Trim();
                SpDTO.applicationFee = 0;
                SpDTO.otherCharges = 0;
                SpDTO.bookingId = objBooking?.Id ?? 0;
                SpDTO.bookingRef = objBooking?.BookingNo ?? "";
                SpDTO.amount = Convert.ToInt64(Fare * 100);
                SpDTO.displayAmount = Fare.ToDecimal();
                SpDTO.currency = DefaultCurrency;
                SpDTO.description = Desc;
                SpDTO.companyName = companyName ?? "";
                SpDTO.defaultClientId = HubProcessor.Instance.objPolicy.DefaultClientId.ToStr();
                SpDTO.location = DefaultClientLocation;
                var DataObj = Newtonsoft.Json.JsonConvert.SerializeObject(SpDTO);
                string StripeAPIBaseURL = System.Configuration.ConfigurationManager.AppSettings["StripeAPIBaseURL"];
                using (var client = new HttpClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    client.BaseAddress = new Uri(StripeAPIBaseURL);
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    var content = new StringContent(DataObj, Encoding.UTF8, "application/json");
                    var postTask = client.PostAsync(StripeAPIBaseURL + "/v1/terminal/CreatePaymentIntent", content).Result;
                    string PayResp = postTask.Content.ReadAsStringAsync().Result;
                    resp = new JavaScriptSerializer().Deserialize<PaymentIntentResponse>(PayResp);
                }

                if (resp != null && resp.isSuccess && !string.IsNullOrEmpty(resp.clientSecret))
                {
                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        string PaymentComment = "Authorised for " + DefaultCurrencySign + Fare;
                        db.ExecuteQuery<int>("update booking set PaymentComments='" + PaymentComment + "',CustomerCreditCardDetails='" + resp.clientSecret.ToStr() + "' where id=" + objBooking.Id.ToLong());

                        db.stp_BookingLog(objBooking.Id.ToLong(), "Customer", "Secure Terminal Transaction " + resp.clientSecret.ToStr() + " |  Fares | " + DefaultCurrencySign + Fare + " | " + terminalDetails);
                    }
                    response.Message = "Payment Intent is created successfully!";
                    response.IsSuccess = true;
                    response.Data = resp.clientSecret;
                }
                else
                {
                    response.IsSuccess = false;
                    response.Data = "";
                    response.Message = resp.error;
                }

                try
                {
                    File.AppendAllText(physicalPath + "\\CreateTerminalPaymentIntentKP.txt", DateTime.Now.ToStr() + ": json" + new JavaScriptSerializer().Serialize(SpDTO) + "bookingno:" + objBooking.BookingNo.ToStr() + ", response:" + new JavaScriptSerializer().Serialize(response) + Environment.NewLine);
                }
                catch { }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                response.Data = "";
                try
                {
                    File.AppendAllText(physicalPath + "\\CreateTerminalPaymentIntentKP_exception.txt", DateTime.Now.ToStr() + ": response =" + new JavaScriptSerializer().Serialize(response) + "  ,bookingno:" + objBooking.BookingNo.ToStr() + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch { }
            }

            return response;
        }

        /// <summary>
        /// This Method is to Get QRCODE for Konnect payment 
        /// </summary>
        /// <param name="mesg"></param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetQRCode")]
        public ResponseData GetQRCode(string mesg)
        {
            ResponseData resp = new ResponseData();

            string json = string.Empty;
            Gen_SysPolicy_PaymentDetail paymentGateway = null;
            string StripeAPIBaseURL = System.Configuration.ConfigurationManager.AppSettings["StripeAPIBaseURL"];
            try
            {
                File.AppendAllText(physicalPath + "\\GetQRCode.txt", DateTime.Now.ToStr() + ": request" + mesg.ToStr());
            }
            catch { }
            try
            {

                StripeKonnectPaymentRequestModel input = new JavaScriptSerializer().Deserialize<StripeKonnectPaymentRequestModel>(mesg);
                if (input.jobId <= 0)
                {
                    resp.IsSuccess = false;
                    resp.Message = "Invalid Job Id!";
                    return resp;
                }

                decimal Price = input.Fare;
                Taxi_Model.Booking objBooking = General.GetObject<Taxi_Model.Booking>(c => c.Id == input.jobId);

                using (TaxiDataContext db = new TaxiDataContext())
                {
                    paymentGateway = General.GetObject<Gen_SysPolicy_PaymentDetail>(c => c.PaymentGatewayId == 15);
                    if (objBooking != null && paymentGateway != null)
                    {

                        resp = GetPaymentLinkKonnectPay(paymentGateway, objBooking, Price, false);
                        if (resp != null && resp.IsSuccess && !string.IsNullOrEmpty(resp.Data))
                        {
                            //string BookingDesc  = resp.Message.Replace(" ", "%20");
                            // string desc = resp.Message.Replace(" ","");// StripeAPIBaseURL + "/QRcode?Data=" + resp.Data.Trim() + "&Description=" + resp.Message;
                            string QRcodeURL = $"{StripeAPIBaseURL}/QRcode?Data={resp.Data.Trim()}&Description={resp.Message}";
                            resp.IsSuccess = true;
                            resp.Data = QRcodeURL;
                            resp.Message = "Qr code Created Successfully!";


                        }
                        else
                        {
                            resp.IsSuccess = false;
                            resp.Message = "Error In creating QR CODE!";
                        }
                    }
                    else
                    {
                        resp.IsSuccess = false;
                        resp.Message = "Booking info or gateway details are missing!";
                    }
                }

            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\GetQRCode_exception.txt", DateTime.Now.ToStr() + ",response:" + new JavaScriptSerializer().Serialize(resp) + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
                resp.IsSuccess = false;
                resp.Message = ex.Message;

            }

            return resp;
        }
        /// <summary>
        /// This Method is to get payment link for konnect payment 
        /// </summary>
        private ResponseData GetPaymentLinkKonnectPay(Gen_SysPolicy_PaymentDetail obj, Booking objBooking, decimal Price, bool IsPayFromDriverApp)
        {
            ResponseData resp = new ResponseData();
            string DefaultCurrency = System.Configuration.ConfigurationManager.AppSettings["DefaultCurrency"];
            string DefaultClientLocation = System.Configuration.ConfigurationManager.AppSettings["DefaultClientLocation"];
            string json = string.Empty;
            try
            {

                try
                {
                    File.AppendAllText(physicalPath + "\\GetQRCode.txt", DateTime.Now.ToStr() + ": request" + objBooking.Id.ToStr());
                }
                catch
                {

                }

                //Get customer card token if exist.
                //CustomerCardDetails CardDetails = null;
                //CardDetails = GetCardTokenKP(objBooking?.Customer?.Id);
                Customer objcustomer = General.GetObject<Customer>(c => c.MobileNo == objBooking.CustomerMobileNo);
                CustomerCardDetails CardDetails = null;
                CardDetails = GetCardTokenKP(objcustomer?.Id);

                string baseUrl = System.Configuration.ConfigurationManager.AppSettings["huburl"].ToStr();
                Classes.KonnectSupplier.StripePaymentRequestDto SpDTO = new Classes.KonnectSupplier.StripePaymentRequestDto();
                SpDTO.isAuthorized = false;
                SpDTO.key = "";
                SpDTO.secret = "";
                SpDTO.countryId = obj.ApplicationId.ToInt();
                SpDTO.connectedAccountId = obj.PaypalID.Trim();
                SpDTO.applicationFee = 0;
                SpDTO.otherCharges = 0;
                SpDTO.bookingId = objBooking?.Id ?? 0;
                SpDTO.bookingRef = objBooking?.BookingNo ?? "";
                SpDTO.amount = (Price * 100).ToInt();
                SpDTO.displayAmount = Price.ToDecimal();
                SpDTO.currency = DefaultCurrency;
                SpDTO.description = objBooking?.Gen_SubCompany?.CompanyName + " | " + objBooking?.BookingNo.ToStr() + " | " + "Fares : " + Price.ToDecimal() + " " + DefaultCurrency;
                if (SpDTO.description.ToStr().Length > 150)
                {
                    SpDTO.description = HubProcessor.Instance.objPolicy.DefaultClientId.ToStr() + " | " + objBooking?.BookingNo.ToStr() + " | " + "Fares : " + Price.ToDecimal() + " " + DefaultCurrency;
                }
                SpDTO.paymentMethodId = CardDetails?.CustomerCardToken ?? "";
                SpDTO.customerId = objcustomer?.CreditCardDetails;//objBooking?.Customer?.CreditCardDetails;
                SpDTO.customerName = objBooking?.CustomerName?.Trim();
                SpDTO.email = objBooking?.CustomerEmail?.Trim();
                SpDTO.phoneNumber = objBooking?.CustomerPhoneNo?.Trim();

                SpDTO.lastfour = "";
                SpDTO.expiry = "";
                SpDTO.cardtype = "";
                SpDTO.companyName = objBooking?.Gen_SubCompany?.CompanyName ?? "";
                SpDTO.defaultClientId = HubProcessor.Instance.objPolicy.DefaultClientId.ToStr();
                SpDTO.location = DefaultClientLocation;
                SpDTO.verificationWebhook = baseUrl + "/api/Supplier/VerifyPaymentKOnnectPay";
                SpDTO.paymentUpdateWebhook = baseUrl + "/api/Supplier/UpdateDataFromPayByLinkDriverAppKOnnectPay";
                SpDTO.ReturnAmount = objBooking.ReturnFareRate.HasValue ? objBooking.ReturnFareRate.Value : 0;
                SpDTO.UpdatePaymentURL = baseUrl;
                SpDTO.OperatorName = "";
                SpDTO.PreAuthUrl = "";
                SpDTO.PayByDispatch = "1";
                if (IsPayFromDriverApp)
                {
                    SpDTO.paymentUpdateWebhook = baseUrl + "/api/Supplier/UpdateDataFromDriverAppKOnnectPay";
                }

                if (!string.IsNullOrEmpty(obj.ApplicationId) && !string.IsNullOrEmpty(obj.PaypalID))
                {
                    string data = new JavaScriptSerializer().Serialize(SpDTO);
                    string StripeAPIBaseURL = System.Configuration.ConfigurationManager.AppSettings["StripeAPIBaseURL"];
                    var PayLink = StripeAPIBaseURL + "/checkout?data=" + new SupplierController().EncodeBASE64(data);

                    if (PayLink.ToStr().Contains("tinyurl") == false)
                    {
                        PayLink = General.ToTinyURLS(PayLink.ToStr());
                    }

                    if (PayLink.ToStr().Length > 0)
                    {
                        resp.Message = SpDTO.description;
                        resp.IsSuccess = true;
                        resp.Data = PayLink;

                    }
                    else
                    {
                        resp.IsSuccess = false;
                        resp.Data = "";
                        resp.Message = "Error in Creating Payment Link!";
                    }

                }
                try
                {
                    File.AppendAllText(physicalPath + "\\GetQRCode.txt", DateTime.Now.ToStr() + ": json" + new JavaScriptSerializer().Serialize(SpDTO) + "bookingno:" + objBooking.BookingNo.ToStr() + ",response:" + new JavaScriptSerializer().Serialize(resp) + Environment.NewLine);
                }
                catch
                {

                }


            }
            catch (Exception ex)
            {
                resp.IsSuccess = false;
                resp.Message = ex.Message;
                resp.Data = "";
                try
                {


                    File.AppendAllText(physicalPath + "\\GetQRCode_exception.txt", DateTime.Now.ToStr() + ": token=" + new JavaScriptSerializer().Serialize(json) + "  ,bookingno:" + objBooking.BookingNo.ToStr() + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {


                }
            }







            return resp;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("CheckPaymentStatus")]
        public ResponseData CheckPaymentStatus(string mesg)
        {
            ResponseData resp = new ResponseData();
            CheckPaymentStatusResponse PayemntStatus = new CheckPaymentStatusResponse();
            string json = string.Empty;
            try
            {
                File.AppendAllText(physicalPath + "\\CheckPaymentStatus.txt", DateTime.Now.ToStr() + ": request" + mesg.ToStr());
            }
            catch { }
            try
            {

                CheckPaymentStatus input = new JavaScriptSerializer().Deserialize<CheckPaymentStatus>(mesg);
                if (input.jobId <= 0)
                {
                    resp.IsSuccess = false;
                    resp.Message = "Invalid Job Id!";
                    return resp;
                }

                Taxi_Model.Booking objBooking = null;
                Booking_Payment objBookingPayment = null;

                using (TaxiDataContext db = new TaxiDataContext())
                {
                    objBooking = db.Bookings.Where(c => c.Id == input.jobId).FirstOrDefault();
                    objBookingPayment = General.GetObject<Booking_Payment>(c => c.BookingId == input.jobId && c.AuthCode != null && c.AuthCode != "");

                }
                if (objBooking != null)
                {
                    if (string.IsNullOrEmpty(objBooking.CustomerCreditCardDetails) && string.IsNullOrEmpty(objBooking.PaymentComments))
                    {
                        PayemntStatus.paymentStatus = 0; // no process start for payment

                    }
                    else if (!string.IsNullOrEmpty(objBooking.CustomerCreditCardDetails) && !string.IsNullOrEmpty(objBooking.PaymentComments) && !objBooking.PaymentComments.Contains("failed"))
                    {
                        PayemntStatus.paymentStatus = 3;// payment process in progress

                    }
                    else if (!string.IsNullOrEmpty(objBooking.PaymentComments) && objBooking.PaymentComments.Contains("failed"))
                    {
                        PayemntStatus.paymentStatus = 2;// payment failed
                        PayemntStatus.TransactionId = "";
                        PayemntStatus.TransactionMessage = "Failed";

                    }
                    else if (objBookingPayment != null && !string.IsNullOrEmpty(objBooking.PaymentComments))
                    {
                        PayemntStatus.paymentStatus = 1;// payment success
                        PayemntStatus.TransactionId = objBookingPayment.AuthCode;
                        PayemntStatus.TransactionMessage = "Payment Successfull";

                    }
                    resp.IsSuccess = true;
                    resp.Data = new JavaScriptSerializer().Serialize(PayemntStatus);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\CheckPaymentStatus_exception.txt", DateTime.Now.ToStr() + ",response:" + new JavaScriptSerializer().Serialize(resp) + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
                resp.IsSuccess = false;
                resp.Message = ex.Message;

            }

            return resp;
        }


        public class PaymentIntentResponse
        {


            public dynamic response { get; set; }
            public bool isSuccess { get; set; }
            public string clientSecret { get; set; }
            public string error { get; set; }

        }
        public class StripeAPIResponse
        {
            public bool IsSuccess;
            public string Message;
            public dynamic Data;
        }

        public class CustomerCardDetails
        {
            public long Id { get; set; }
            public string CustomerCardToken { get; set; }
            public string Lastfour { get; set; }
            public string expiry { get; set; }
            public string cardType { get; set; }
            public bool IsDefault { get; set; }
        }
        private CustomerCardDetails GetCardTokenKP(int? CustomerId)
        {
            CustomerCardDetails CCDetail = new CustomerCardDetails();
            try
            {
                ClsCCDetails CardDetails = new ClsCCDetails();
                using (TaxiDataContext db = new TaxiDataContext())
                {
                    CardDetails = db.ExecuteQuery<ClsCCDetails>("select Id,CCDetails,IsDefault from Customer_CCDetails where customerId=" + CustomerId.ToInt()).LastOrDefault();

                }

                if (CardDetails != null && !string.IsNullOrEmpty(CardDetails.CCDetails) && CardDetails.CCDetails.ToLower().Contains("konnectpaytoken"))
                {
                    var CCDetails = CardDetails.CCDetails.Split('|');
                    string Token = CCDetails[0];
                    if (!string.IsNullOrEmpty(Token)) { CCDetail.CustomerCardToken = Token.Replace("KonnectPayToken:", "").Trim(); }
                    if (!string.IsNullOrEmpty(CCDetails[1]))
                    {
                        var CardInfo = CCDetails[1].Split(',');
                        if (CardInfo != null)
                        {
                            if (CardInfo[0] != null && CardInfo[0].ToLower().Contains("last four"))
                            {
                                CCDetail.Lastfour = CardInfo[0].Replace("last four", "");
                                CCDetail.Lastfour = CCDetail.Lastfour.Replace(":", "").Trim();
                            }
                            if (CardInfo[1] != null && CardInfo[1].ToLower().Contains("expiry"))
                            {
                                CCDetail.expiry = CardInfo[1].Replace("expiry", "");
                                CCDetail.expiry = CCDetail.expiry.Replace(":", "").Trim();


                            }
                            if (CardInfo[2] != null && CardInfo[2].ToLower().Contains("cardtype"))
                            {
                                CCDetail.cardType = CardInfo[2].Replace("cardType", "");
                                CCDetail.cardType = CCDetail.cardType.Replace(":", "").Trim();
                            }


                        }
                    }
                }

            }
            catch
            {

            }

            return CCDetail;
        }


        #endregion


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestFOJjoblist")]//already exist, make it change by returning ResponseData model as return type, and object as parameter
        //public string requestFOJJobList(string mesg)
        public ResponseData requestFOJJobList(string mesg)
        {
            try
            {

                File.AppendAllText(physicalPath + "\\" + "requestFOJJobList.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", json: " + mesg.ToString() + Environment.NewLine);
            }
            catch
            {


            }

            ResponseData res = new ResponseData();
            try
            {
                JobActionEx objAction = new JavaScriptSerializer().Deserialize<JobActionEx>(mesg.ToStr());




                int driverId = int.Parse(objAction.DrvId);  //values[0].ToInt();
                string driverNo = objAction.DrvNo;  //values[1].ToStr();

                using (TaxiDataContext db = new TaxiDataContext())
                {
                    db.CommandTimeout = 5;
                    var bookings = db.Bookings.Where(c => c.DriverId == driverId && c.BookingStatusId == Enums.BOOKINGSTATUS.FOJ) //Change to FOJ
                        .OrderBy(c => c.PickupDateTime).ToList();


                    StringBuilder s = new StringBuilder();
                    foreach (var objBooking in bookings)

                    {
                        if (objBooking != null)
                        {

                            string customerMobileNo = objBooking.CustomerMobileNo.ToStr().Trim();

                            string customerName = objBooking.CustomerName;

                            string via = string.Join(",", objBooking.Booking_ViaLocations.Select(c => c.ViaLocValue.ToStr()).ToArray<string>());

                            if (!string.IsNullOrEmpty(via.Trim()))
                                via = "Via: " + via;

                            string specialReq = objBooking.SpecialRequirements.ToStr().Trim();
                            if (!string.IsNullOrEmpty(specialReq))
                                specialReq = "Special Req: " + specialReq;




                            string custNo = !string.IsNullOrEmpty(objBooking.CustomerMobileNo) ? objBooking.CustomerMobileNo : objBooking.CustomerPhoneNo;



                            // Send To Driver






                            string paymentType = objBooking.Gen_PaymentType.PaymentCategoryId == null ? objBooking.Gen_PaymentType.DefaultIfEmpty().PaymentType.ToStr()
                                     : objBooking.Gen_PaymentType.Gen_PaymentCategory.CategoryName.ToStr();



                            //  string strDeviceRegistrationId = ObjDriver.DeviceId.ToStr();
                            string journey = "O/W";

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




                            if (objBooking.CompanyId != null)
                                companyName = objBooking.Gen_Company.DefaultIfEmpty().CompanyName.ToStr();





                            pickUpPlot = objBooking.ZoneId != null ? "<<<" + objBooking.Gen_Zone1.ZoneName.ToStr() : "";
                            dropOffPlot = objBooking.DropOffZoneId != null ? "<<<" + objBooking.Gen_Zone.ZoneName.ToStr() : "";




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




                            decimal pdafares = objBooking.GetType().GetProperty(HubProcessor.Instance.objPolicy.PDAFaresPropertyName.ToStr().Trim()).GetValue(objBooking, null).ToDecimal();


                            string msg = string.Empty;




                            string showFaresValue = objBooking.Gen_PaymentType.ShowFaresOnPDA.ToStr().Trim();

                            string showFares = ",\"ShowFares\":\"" + showFaresValue + "\"";
                            string showSummary = ",\"ShowSummary\":\"" + showFaresValue + "\"";

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


                            //if (objBooking.CompanyId != null && Global.enableBookingRefOnAccJob == "1")
                            //{
                            //    if (specialRequirements.Length == 0)
                            //        specialRequirements = "Booking Ref- " + objBooking.BookingNo.ToStr();
                            //    else
                            //        specialRequirements = "Booking Ref- " + objBooking.BookingNo.ToStr() + " , " + specialRequirements;
                            //}

                            if (specialRequirements.ToStr().Contains("\""))
                                specialRequirements = specialRequirements.ToStr().Replace("\"", "-").Trim();


                            string toDoorNo = objBooking.ToDoorNo.ToStr().Trim();

                            if (objBooking.ToLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT && objBooking.JourneyTypeId.ToInt() == Enums.JOURNEY_TYPES.RETURN)
                                toDoorNo = string.Empty;
                            else if (toDoorNo.Length > 0)
                                toDoorNo = toDoorNo + "-";


                            string summary = string.Empty;

                            List<ChargesSummary> listofSummary = new List<ChargesSummary>();

                            listofSummary.Add(new ChargesSummary { label = "Fares", value = string.Format("{0:0.00}", objBooking.FareRate.ToDecimal()) });

                            listofSummary.Add(new ChargesSummary { label = "Parking", value = string.Format("{0:0.00}", objBooking.CongtionCharges.ToDecimal()) });
                            listofSummary.Add(new ChargesSummary { label = "Waiting", value = string.Format("{0:0.00}", objBooking.MeetAndGreetCharges.ToDecimal()) });
                            listofSummary.Add(new ChargesSummary { label = "Extras", value = string.Format("{0:0.00}", objBooking.ExtraDropCharges.ToDecimal()) });
                            listofSummary.Add(new ChargesSummary { label = "Fee", value = string.Format("{0:0.00}", objBooking.AgentCommission.ToDecimal() + objBooking.CashRate.ToDecimal() + objBooking.ServiceCharges.ToDecimal()) });

                            summary = ",\"Summary\":" + Newtonsoft.Json.JsonConvert.SerializeObject(listofSummary);



                            msg = "{ \"JobId\" :\"" + objBooking.Id +
                                  "\", \"Pickup\":\"" + (!string.IsNullOrEmpty(objBooking.FromDoorNo) ? fromdoorno + "-" + fromAddress + pickUpPlot : fromAddress + pickUpPlot) +
                                  "\", \"Destination\":\"" + (toDoorNo + toAddress + dropOffPlot) + "\"," +
                                  "\"PickupDateTime\":\"" + string.Format("{0:dd/MM/yyyy   HH:mm}", objBooking.PickupDateTime) + "\"" +
                                  ",\"Cust\":\"" + objBooking.CustomerName + "\",\"Mob\":\"" + mobileNo + " " + "\",\"Fare\":\"" + pdafares + "\",\"Vehicle\":\"" + objBooking.Fleet_VehicleType.VehicleType + "\",\"Account\":\"" + companyName + " " + "\"" +
                                 ",\"Lug\":\"" + objBooking.NoofLuggages.ToInt() + "\",\"Passengers\":\"" + objBooking.NoofPassengers.ToInt() + "\",\"Journey\":\"" + journey + "\",\"Payment\":\"" + paymentType + "\",\"Special\":\"" + specialRequirements + " " + "\",\"Extra\":\"" + IsExtra + "\",\"Via\":\"" + viaP + " " + "\"" +

                               ",\"CompanyId\":\"" + objBooking.CompanyId.ToInt() + "\",\"SubCompanyId\":\"" + objBooking.SubcompanyId.ToInt() + "\",\"QuotedPrice\":\"" + (objBooking.IsQuotedPrice.ToBool() ? "1" : "0") + "\"" +


                                 parkingandWaiting + ",\"DriverFares\":\"" + String.Format("{0:0.00}", objBooking.FareRate) + "\"" +
                                   agentDetails +


                                 ",\"Did\":\"" + driverId + "\",\"BabySeats\":\"" + objBooking.BabySeats.ToStr() + "\"" + showFares + showSummary + summary + " }";














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








                        }

                    }



                    string resp = s.ToStr();

                    if (resp.ToStr().Trim().EndsWith(","))
                        resp = resp.Remove(resp.ToStr().LastIndexOf(","));

                    if (resp.ToStr().Trim().Length > 10)
                    {

                        resp = "[" + resp + "]";

                    }
                    //



                    //return resp;
                    res.Data = resp;
                    res.IsSuccess = true;
                    res.Message = "";

                    s = null;
                    GC.Collect();

                }
            }
            catch (Exception ex)
            {
                //return "exceptionoccurred";
                res.Data = null;
                res.IsSuccess = false;
                res.Message = "exceptionoccurred";
            }

            return res;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestfuturejoblist")] //already exist, make it change by returning ResponseData model as return type, and object as parameter
        //public ResponseData requestfuturejoblist(string mesg)
        public ResponseData requestfuturejoblist(string mesg)
        {
            try
            {

                File.AppendAllText(physicalPath + "\\" + "requestFOJJobList.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", json: " + mesg.ToString() + Environment.NewLine);
            }
            catch
            {


            }

            ResponseData res = new ResponseData();
            try
            {
                JobActionEx objAction = new JavaScriptSerializer().Deserialize<JobActionEx>(mesg.ToStr());

                //string dataValue = mesg;
                //dataValue = dataValue.Trim();

                //string[] values = dataValue.Split(new char[] { '=' });

                int driverId = int.Parse(objAction.DrvId); //values[0].ToInt();
                string driverNo = objAction.DrvNo; //values[1].ToStr();

                using (TaxiDataContext db = new TaxiDataContext())
                {
                    db.CommandTimeout = 5;
                    var bookings = db.Bookings.Where(c => c.DriverId == driverId && c.BookingStatusId == Enums.BOOKINGSTATUS.PENDING_START)
                        .OrderBy(c => c.PickupDateTime).ToList();


                    StringBuilder s = new StringBuilder();
                    foreach (var objBooking in bookings)

                    {
                        if (objBooking != null)
                        {

                            string customerMobileNo = objBooking.CustomerMobileNo.ToStr().Trim();

                            string customerName = objBooking.CustomerName;

                            string via = string.Join(",", objBooking.Booking_ViaLocations.Select(c => c.ViaLocValue.ToStr()).ToArray<string>());

                            if (!string.IsNullOrEmpty(via.Trim()))
                                via = "Via: " + via;

                            string specialReq = objBooking.SpecialRequirements.ToStr().Trim();
                            if (!string.IsNullOrEmpty(specialReq))
                                specialReq = "Special Req: " + specialReq;




                            string custNo = !string.IsNullOrEmpty(objBooking.CustomerMobileNo) ? objBooking.CustomerMobileNo : objBooking.CustomerPhoneNo;



                            // Send To Driver






                            string paymentType = objBooking.Gen_PaymentType.PaymentCategoryId == null ? objBooking.Gen_PaymentType.DefaultIfEmpty().PaymentType.ToStr()
                                     : objBooking.Gen_PaymentType.Gen_PaymentCategory.CategoryName.ToStr();



                            //  string strDeviceRegistrationId = ObjDriver.DeviceId.ToStr();
                            string journey = "O/W";

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




                            if (objBooking.CompanyId != null)
                                companyName = objBooking.Gen_Company.DefaultIfEmpty().CompanyName.ToStr();





                            pickUpPlot = objBooking.ZoneId != null ? "<<<" + objBooking.Gen_Zone1.ZoneName.ToStr() : "";
                            dropOffPlot = objBooking.DropOffZoneId != null ? "<<<" + objBooking.Gen_Zone.ZoneName.ToStr() : "";




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




                            decimal pdafares = objBooking.GetType().GetProperty(HubProcessor.Instance.objPolicy.PDAFaresPropertyName.ToStr().Trim()).GetValue(objBooking, null).ToDecimal();


                            string msg = string.Empty;




                            string showFaresValue = objBooking.Gen_PaymentType.ShowFaresOnPDA.ToStr().Trim();

                            string showFares = ",\"ShowFares\":\"" + showFaresValue + "\"";
                            string showSummary = ",\"ShowSummary\":\"" + showFaresValue + "\"";

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



                            //if (objBooking.CompanyId != null && Global.enableBookingRefOnAccJob == "1")
                            //{
                            //    if (specialRequirements.Length == 0)
                            //        specialRequirements = "Booking Ref- " + objBooking.BookingNo.ToStr();
                            //    else
                            //        specialRequirements = "Booking Ref- " + objBooking.BookingNo.ToStr() + " , " + specialRequirements;
                            //}

                            if (specialRequirements.ToStr().Contains("\""))
                                specialRequirements = specialRequirements.ToStr().Replace("\"", "-").Trim();


                            string toDoorNo = objBooking.ToDoorNo.ToStr().Trim();

                            if (objBooking.ToLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT && objBooking.JourneyTypeId.ToInt() == Enums.JOURNEY_TYPES.RETURN)
                                toDoorNo = string.Empty;
                            else if (toDoorNo.Length > 0)
                                toDoorNo = toDoorNo + "-";


                            string summary = string.Empty;

                            List<ChargesSummary> listofSummary = new List<ChargesSummary>();

                            listofSummary.Add(new ChargesSummary { label = "Fares", value = string.Format("{0:0.00}", objBooking.FareRate.ToDecimal()) });

                            listofSummary.Add(new ChargesSummary { label = "Parking", value = string.Format("{0:0.00}", objBooking.CongtionCharges.ToDecimal()) });
                            listofSummary.Add(new ChargesSummary { label = "Waiting", value = string.Format("{0:0.00}", objBooking.MeetAndGreetCharges.ToDecimal()) });
                            listofSummary.Add(new ChargesSummary { label = "Extras", value = string.Format("{0:0.00}", objBooking.ExtraDropCharges.ToDecimal()) });
                            listofSummary.Add(new ChargesSummary { label = "Fee", value = string.Format("{0:0.00}", objBooking.AgentCommission.ToDecimal() + objBooking.CashRate.ToDecimal() + objBooking.ServiceCharges.ToDecimal()) });

                            summary = ",\"Summary\":" + Newtonsoft.Json.JsonConvert.SerializeObject(listofSummary);

                            msg = "{ \"JobId\" :\"" + objBooking.Id +
                                  "\", \"Pickup\":\"" + (!string.IsNullOrEmpty(objBooking.FromDoorNo) ? fromdoorno + "-" + fromAddress + pickUpPlot : fromAddress + pickUpPlot) +
                                    "\", \"Destination\":\"" + (toDoorNo + toAddress + dropOffPlot) + "\"," +
                                  "\"PickupDateTime\":\"" + string.Format("{0:dd/MM/yyyy   HH:mm}", objBooking.PickupDateTime) + "\"" +
                                  ",\"Cust\":\"" + objBooking.CustomerName + "\",\"Mob\":\"" + mobileNo + " " + "\",\"Fare\":\"" + pdafares + "\",\"Vehicle\":\"" + objBooking.Fleet_VehicleType.VehicleType + "\",\"Account\":\"" + companyName + " " + "\"" +
                                 ",\"Lug\":\"" + objBooking.NoofLuggages.ToInt() + "\",\"Passengers\":\"" + objBooking.NoofPassengers.ToInt() + "\",\"Journey\":\"" + journey + "\",\"Payment\":\"" + paymentType + "\",\"Special\":\"" + specialRequirements + " " + "\",\"Extra\":\"" + IsExtra + "\",\"Via\":\"" + viaP + " " + "\"" +

                               ",\"CompanyId\":\"" + objBooking.CompanyId.ToInt() + "\",\"SubCompanyId\":\"" + objBooking.SubcompanyId.ToInt() + "\",\"QuotedPrice\":\"" + (objBooking.IsQuotedPrice.ToBool() ? "1" : "0") + "\"" +


                                 parkingandWaiting + ",\"DriverFares\":\"" + String.Format("{0:0.00}", objBooking.FareRate) + "\"" +
                                   agentDetails +


                                 ",\"Did\":\"" + driverId + "\",\"BabySeats\":\"" + objBooking.BabySeats.ToStr() + "\"" + showFares + showSummary + summary + " }";














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








                        }

                    }



                    string resp = s.ToStr();

                    if (resp.ToStr().Trim().EndsWith(","))
                        resp = resp.Remove(resp.ToStr().LastIndexOf(","));

                    if (resp.ToStr().Trim().Length > 10)
                    {

                        resp = "[" + resp + "]";

                    }
                    //



                    //return resp;
                    res.Data = resp;
                    res.IsSuccess = true;
                    res.Message = "";


                    s = null;
                    GC.Collect();

                }
            }
            catch (Exception ex)
            {
                //return "exceptionoccurred";
                res.Data = null;
                res.IsSuccess = false;
                res.Message = ex.Message;
            }

            return res;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestPlotsDetails")]
        public ResponseData requestPlotsDetails(string mesg)
        {
            ResponseData res = new ResponseData();
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
                            res.Data = "";
                            // Clients.Caller.plotDetails("");


                        }
                        else
                            res.Data = Newtonsoft.Json.JsonConvert.SerializeObject(objDetails);
                        //   Clients.Caller.plotDetails(objDetails);

                    }
                    else
                    {

                        if (list.Count == 0)
                            res.Data = "";
                        //   Clients.Caller.plotDetails("");
                        else
                            res.Data = Newtonsoft.Json.JsonConvert.SerializeObject(list);
                        // Clients.Caller.plotDetails(list);


                    }

                    res.IsSuccess = true;
                }
            }
            catch (Exception ex)
            {

                try
                {
                    res.Data = "";
                    res.IsSuccess = false;
                    res.Message = ex.Message;
                    //  Clients.Caller.plotDetails("");
                    File.AppendAllText(physicalPath + "\\" + "requestPlotsDetails.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + mesg + " ,exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {


                }
            }

            return res;

        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestPlotsBidding")]
        public ResponseData requestPlotsBidding(string mesg)
        {
            ResponseData data = new ResponseData();

            if (mesg.ToStr().Trim().Length == 0)
                return null;
            //
            List<ClsPlotBidding> list = new List<ClsPlotBidding>();
            string response = string.Empty;
            try
            {
                //
                //try
                //{
                //    File.AppendAllText(physicalPath + "\\requestPlotsBiddingApi.txt", DateTime.Now + ": datavalue=" + mesg + Environment.NewLine);

                //}
                //catch
                //{

                //}





                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });




                using (TaxiDataContext db = new TaxiDataContext())
                {
                    try
                    {
                        db.CommandTimeout = 4;

                        int driverId = values[0].ToInt();

                        string postedFrom = string.Empty;




                        var result = db.stp_GetAreaPlotsByVehicle(driverId, Instance.objPolicy.PlotsJobExpiryValue1, Instance.objPolicy.PlotsJobExpiryValue2).ToList();

                        //


                        int? statusId = db.Fleet_DriverQueueLists.Where(c => c.DriverId == driverId && c.Status == true).Select(c => c.DriverWorkStatusId).FirstOrDefault();


                        string statusName = "Available";

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

                            }
                            catch (Exception ex)
                            {
                                try
                                {
                                    rank = "-";
                                    File.AppendAllText(physicalPath + "\\stp_getdriverrank_exception.txt", DateTime.Now + ": driverid=" + driverId + ", zoneid:" + driverZoneId + ",Rank:" + rank + ",exception:" + ex.Message + Environment.NewLine);

                                }
                                catch
                                {

                                }

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

                            list.Add(new ClsPlotBidding { ZoneName = "-", Drivers = 0, J15 = 0, J30 = 0, BidDetails = 1, Rank = rank, DriverWorkStatus = statusName });

                        }


                        //

                        if (statusId == Enums.Driver_WORKINGSTATUS.AVAILABLE || statusId == Enums.Driver_WORKINGSTATUS.ONBREAK)
                        {


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
                        data.Data = response;
                        data.IsSuccess = true;




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

                        data.Data = ex.Message;
                        data.IsSuccess = false;
                    }
                }

                //send message back to PDA
                //    Clients.Caller.plotsBiddings(response);
                //

            }
            catch (Exception ex)
            {
                data.Data = ex.Message;
                data.IsSuccess = false;
                // Clients.Caller.plotsBiddings("exceptionoccurred");
            }


            //

            //try
            //{
            //    File.AppendAllText(physicalPath + "\\plotsBiddingapi.txt", DateTime.Now.ToStr() + ": " + data.Data + Environment.NewLine);
            //}
            //catch
            //{

            //}

            return data;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestUpdateVersion")]
        public ResponseData requestUpdateVersion(string mesg)
        {
            ResponseData data = new ResponseData();
            try
            {

                File.AppendAllText(AppContext.BaseDirectory + "\\requestupdateversion.txt", DateTime.Now.ToStr() + " request" + mesg + Environment.NewLine);

            }
            catch
            {

            }
            try
            {
                var obj = new JavaScriptSerializer().Deserialize<clsupdateversion>(mesg);



                if (obj.currVer < 100.40m)
                {
                    obj.isUpdate = true;
                    obj.priority = 1;


                }

                data.Data = new JavaScriptSerializer().Serialize(obj);
                data.IsSuccess = true;
            }
            catch
            {




            }

            return data;







        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestPlotsBiddingDetails")]
        public ResponseData requestPlotsBiddingDetails(string mesg)
        {
            ResponseData res = new ResponseData();
            List<ClsPlotDetails> list = new List<ClsPlotDetails>();
            try
            {

                try
                {

                    File.AppendAllText(physicalPath + "\\" + "requestPlotsBiddingDetails.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + mesg + Environment.NewLine);
                }
                catch
                {


                }

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


                    //

                    //.Select(args => (args.zoneid + "<<" + (args.zonename.Length > 0 ? args.zonename : " - ") + "<<" + "0" + "<<" + args.JobId + "<<" + args.PickupDateTime + "<<" + args.FromAddress + "<<" + args.ToAddress + "<<" + args.FareRate + "<<" + args.VehicleType))

                    //  .ToArray<string>();


                    if (obj.Version.ToStr().Trim().Length > 0)
                    {


                        if (obj.Version.ToStr().Trim().ToDecimal() >= 41.72m)
                        {

                            foreach (var item in arr)
                            {

                                list.Add(new ClsPlotDetails { PlotName = item.ZoneName, PlotId = item.ZoneId.ToInt(), JobId = item.JobId, PickupAddress = item.FromAddress, DropOff = item.ToAddress });
                                //
                                //
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

                //if (list.Count == 0)
                //    Clients.Caller.plotBiddingDetails("");
                //else
                //    Clients.Caller.plotBiddingDetails(list);
                ///-------------------------------------------------------------------------
                ///
                if (list.Count == 0)
                    res.Data = "";
                else
                    res.Data = Newtonsoft.Json.JsonConvert.SerializeObject(list);

                res.IsSuccess = true;
                res.Message = "success";



            }
            catch (Exception ex)
            {

                try
                {
                    ///Clients.Caller.plotDetails("");----------------------------------------------------
                    ///
                    res.Data = null;
                    res.IsSuccess = false;
                    res.Message = ex.Message;

                    File.AppendAllText(physicalPath + "\\" + "requestPlotsBiddingDetails_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + mesg + " ,exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {


                }
            }

            return res;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestChangePaymentType")]
        public ResponseData requestChangePaymentType(string mesg)
        {
            try
            {

                File.AppendAllText(physicalPath + "\\" + "requestChangePaymentType.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + mesg + Environment.NewLine);
            }
            catch
            {


            }

            ResponseData res = new ResponseData();

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

                            if (db.Gen_SysPolicy_PaymentDetails.Where(c => c.PaymentGatewayId == 9).Count() > 0)
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

            ///Clients.Caller.changePaymentType(resp);---------------------------------------------------------
            ///
            res.Data = resp;
            res.IsSuccess = true;
            res.Message = "";

            return res;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestChangeDestination")]
        public ResponseData requestChangeDestination(string mesg)
        {
            try
            {

                File.AppendAllText(physicalPath + "\\" + "requestChangeDestination.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + mesg + Environment.NewLine);
            }
            catch
            {


            }

            ResponseData res = new ResponseData();
            try
            {


                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                string plotName = "unknown";

                int? zoneId = null;

                GetZone(values[5].ToStr().ToUpper().Trim(), ref zoneId, ref plotName);

                // if(zoneId!=null)
                //{

                using (TaxiDataContext db = new TaxiDataContext())
                    db.stp_UpdateJobAddress(values[1].ToLong(), values[2].ToInt(), values[3].ToStr().Trim(), zoneId, values[5].ToStr().ToUpper(), values[4].ToStr().ToUpper());
                //}           

                //send message back to PDA
                ///Clients.Caller.changeDestination(plotName);------------------------------------------------------------
                ///
                res.Data = plotName;
                res.IsSuccess = true;
                res.Message = "";


                //Byte[] byteResponse = Encoding.UTF8.GetBytes(plotName);
                //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);

                General.BroadCastMessage("**changeaddress>>" + values[2].ToInt() + ">>" + values[1].ToLong() + ">>" + values[3].ToStr() + ">>" + values[5].ToStr());
            }
            catch (Exception ex)
            {
                ///Clients.Caller.changeDestination(ex.Message);-------------------------------------------------
                ///
                res.Data = null;
                res.IsSuccess = false;
                res.Message = ex.Message;
            }

            return res;
        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestDriverBid")]
        public ResponseData requestDriverBid(string mesg)
        {
            try
            {

                File.AppendAllText(physicalPath + "\\" + "requestconDriverBid.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + mesg + Environment.NewLine);
            }
            catch
            {


            }

            ResponseData res = new ResponseData();

            string dataValue = mesg;
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

                                //Clients.Caller.driverBid(new JavaScriptSerializer().Serialize(objBid));
                                //return;
                                ///--------------------------------------------------------------
                                ///
                                res.Data = new JavaScriptSerializer().Serialize(objBid);
                                res.IsSuccess = true;
                                res.Message = "";

                                return res;
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
                                    //Clients.Caller.driverBid(new JavaScriptSerializer().Serialize(objBid));
                                    //return;
                                    ///------------------------------------------------------------------------------
                                    ///
                                    res.Data = new JavaScriptSerializer().Serialize(objBid);
                                    res.IsSuccess = true;
                                    res.Message = "";

                                    return res;


                                }


                            }
                        }
                        else
                        {

                            if (objBid.Status == "7")
                            {
                                objBid.Message = "failed:You cannot bid on POB status";
                                //Clients.Caller.driverBid(new JavaScriptSerializer().Serialize(objBid));
                                //return;
                                ///----------------------------------------------------------------------------------------
                                ///
                                res.Data = new JavaScriptSerializer().Serialize(objBid);
                                res.IsSuccess = true;
                                res.Message = "";

                                return res;

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

                        //if (Instance.objPolicy.PDAFaresPropertyName.ToStr().Trim() == "FareRate")
                        //{
                        //    pdafares = pdafares + objBooking.ServiceCharges.ToDecimal() + objBooking.CongtionCharges.ToDecimal() + objBooking.MeetAndGreetCharges.ToDecimal() + objBooking.ExtraDropCharges.ToDecimal();


                        //}
                        //  pdafares = objBooking.TotalCharges.ToDecimal();

                        mesg = string.Empty;

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

                                if (objBooking.BookingDate.Value.AddMinutes(10) > objBooking.PickupDateTime.Value)
                                {
                                    //if (Global.enableASAPonPDA == "1")
                                    //{

                                    appendString += ",\"priority\":\"" + "ASAP" + "\"";
                                    //   }
                                }

                            }
                            catch
                            {

                            }

                            //

                            //if (objBooking.CompanyId != null && Global.enableBookingRefOnAccJob == "1")
                            //{
                            //    if (specialRequirements.Length == 0)
                            //        specialRequirements = "Booking Ref- " + objBooking.BookingNo.ToStr();
                            //    else
                            //        specialRequirements = "Booking Ref- " + objBooking.BookingNo.ToStr() + " , " + specialRequirements;
                            //}

                            if (specialRequirements.ToStr().Contains("\""))
                                specialRequirements = specialRequirements.ToStr().Replace("\"", "-").Trim();


                            string toDoorNo = objBooking.ToDoorNo.ToStr().Trim();

                            if (objBooking.ToLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT && objBooking.JourneyTypeId.ToInt() == Enums.JOURNEY_TYPES.RETURN)
                                toDoorNo = string.Empty;
                            else if (toDoorNo.Length > 0)
                                toDoorNo = toDoorNo + "-";







                            response = "JobId:" + "{ \"JobId\" :\"" + objBooking.Id.ToStr() +
                         "\", \"Pickup\":\"" + (!string.IsNullOrEmpty(objBooking.FromDoorNo) ? fromdoorno + "-" + fromAddress + pickUpPlot : fromAddress + pickUpPlot) +
                         "\", \"Destination\":\"" + (toDoorNo + toAddress + dropOffPlot) + "\"," +
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

                ///var res = new JavaScriptSerializer().Serialize(objBid);
                ///Clients.Caller.driverBid(res);------------------------------------------------------------------
                ///
                res.Data = new JavaScriptSerializer().Serialize(objBid);
                res.IsSuccess = true;
                res.Message = "";
            }
            else
            {

                ///Clients.Caller.driverBid(response);---------------------------------------------------------------
                ///
                res.Data = response;
                res.IsSuccess = true;
                res.Message = "";
            }

            if (response != "failed:")
            {
                int? bidStatusId = 2;

                if (response == "You Bidding Request has been sent successfully!:")
                    bidStatusId = 4;



                General.SP_SaveBid(jobId, driverId, 0.00m, bidStatusId, "", "");

            }

            return res;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestdriverJobBid")]
        public ResponseData requestdriverJobBid(string mesg)
        {
            try
            {

                File.AppendAllText(physicalPath + "\\" + "requestdriverJobBid.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + mesg + Environment.NewLine);
            }
            catch
            {


            }

            ResponseData res = new ResponseData();
            //string dataValue = msg;
            //dataValue = dataValue.Trim();

            //string[] values = dataValue.Split(new char[] { '=' });
            //string response = string.Empty;

            //int zoneId = values[1].ToInt();

            //int driverId = values[2].ToInt();

            //long jobId = 0;
            //string zoneName = string.Empty;




            string dataValue = mesg;
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

                        mesg = string.Empty;

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

                                if (objBooking.BookingDate.Value.AddMinutes(10) > objBooking.PickupDateTime.Value)
                                {
                                    //if (Global.enableASAPonPDA == "1")
                                    //{

                                    appendString += ",\"priority\":\"" + "ASAP" + "\"";
                                    // }
                                }

                            }
                            catch
                            {

                            }

                            //

                            //if (objBooking.CompanyId != null && Global.enableBookingRefOnAccJob == "1")
                            //{
                            //    if (specialRequirements.Length == 0)
                            //        specialRequirements = "Booking Ref- " + objBooking.BookingNo.ToStr();
                            //    else
                            //        specialRequirements = "Booking Ref- " + objBooking.BookingNo.ToStr() + " , " + specialRequirements;
                            //}

                            if (specialRequirements.ToStr().Contains("\""))
                                specialRequirements = specialRequirements.ToStr().Replace("\"", "-").Trim();


                            string toDoorNo = objBooking.ToDoorNo.ToStr().Trim();

                            if (objBooking.ToLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT && objBooking.JourneyTypeId.ToInt() == Enums.JOURNEY_TYPES.RETURN)
                                toDoorNo = string.Empty;
                            else if (toDoorNo.Length > 0)
                                toDoorNo = toDoorNo + "-";


                            response = "JobId:" + "{ \"JobId\" :\"" + objBooking.Id.ToStr() +
                         "\", \"Pickup\":\"" + (!string.IsNullOrEmpty(objBooking.FromDoorNo) ? fromdoorno + "-" + fromAddress + pickUpPlot : fromAddress + pickUpPlot) +
                         "\", \"Destination\":\"" + (toDoorNo + toAddress + dropOffPlot) + "\"," +
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

                    //var res = new JavaScriptSerializer().Serialize(objBid);
                    //Clients.Caller.driverJobBid(res);
                    ///-------------------------------------------------------------------------------
                    ///
                    res.Data = new JavaScriptSerializer().Serialize(objBid);
                    res.IsSuccess = true;
                    res.Message = "";

                }
                else
                {

                    ///Clients.Caller.driverJobBid(response);-----------------------------------------------------------------
                    ///
                    res.Data = response;
                    res.IsSuccess = true;
                    res.Message = "";
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
                ///Clients.Caller.driverJobBid(response);------------------------------------------------------------------
                ///
                res.Data = null;
                res.IsSuccess = false;
                res.Message = response;
            }

            return res;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("acceptfoj")]
        public string Acceptfoj(string mesg)
        {
            string IsAvalable = "false";
            //
            try
            {


                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });


                using (TaxiDataContext db = new TaxiDataContext())
                {
                    try
                    {

                        if (db.Bookings.Count(c => c.Id == values[1].ToLong() && c.DriverId == values[2].ToInt()) > 0)
                            IsAvalable = "true";



                        int jobstatusId = values[3].ToInt();

                        if (IsAvalable == "true")
                        {
                            db.stp_UpdateJobStatus(values[1].ToLong(), jobstatusId);

                            General.BroadCastMessage("**action>>" + values[1].ToStr() + ">>" + values[2].ToStr() + ">>" + values[3].ToInt());

                            if (HubProcessor.Instance.listofJobs.Count(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong()) > 0)
                            {
                                HubProcessor.Instance.listofJobs.RemoveAll(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong());
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

            }


            return IsAvalable;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestplots")]
        public string requestPlots(string mesg)
        {
            try
            {


                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                string driverPlot = "";
                string[] arr = null;

                using (TaxiDataContext db = new TaxiDataContext())
                {
                    try
                    {
                        db.CommandTimeout = 4;

                        string driverId = values[1].ToStr().Trim();




                        var result = db.stp_GetAreaPlotsByVehicle(driverId.ToInt()
                                , HubProcessor.Instance.objPolicy.PlotsJobExpiryValue1, HubProcessor.Instance.objPolicy.PlotsJobExpiryValue2);







                        arr = (from a in result
                               orderby a.Drivers descending, a.orderno
                               select (a.ZoneName + "," +
                                        a.ExpiryJobs1 + "," + a.ExpiryJobs2
                                   + "," + a.Drivers)).ToArray<string>();




                    }
                    catch (Exception ex)
                    {
                        //try
                        //{
                        //    File.AppendAllText(applic + "\\exception_plots.txt", DateTime.Now.ToStr() + ": " + ex.Message + Environment.NewLine);
                        //}
                        //catch
                        //{

                        //}
                    }
                }

                //send message back to PDA


                return (driverPlot + string.Join(">>", arr));

            }
            catch (Exception ex)
            {
                return "exceptionoccurred";
            }
        }











        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestdrivercclist")]
        public string requestDriverCCList(string mesg)
        {
            string result = "";
            try
            {


                string dataValue = mesg;
                dataValue = dataValue.Trim();

                ClsCCDetails obj = new JavaScriptSerializer().Deserialize<ClsCCDetails>(dataValue);



                using (TaxiDataContext db = new TaxiDataContext())
                {

                    db.CommandTimeout = 4;

                    int driverId = obj.RecordId;


                    //


                    var arr = db.ExecuteQuery<ClsCCDetails>("select Id,CCDetails,IsDefault from  fleet_driver_ccdetails where driverid=" + driverId).ToList();

                    List<ClsCCDetails> list = new List<ClsCCDetails>();

                    foreach (var item in arr)
                    {
                        string decrypt = Cryptography.Decrypt(item.CCDetails, driverId.ToStr(), true);

                        ClsCCDetails objCls = new JavaScriptSerializer().Deserialize<ClsCCDetails>(decrypt);
                        objCls.Id = item.Id;
                        objCls.IsDefault = item.IsDefault.ToBool();
                        list.Add(objCls);
                    }

                    result = new JavaScriptSerializer().Serialize(list);









                }

                //send message back to PDA


                return result;

            }
            catch (Exception ex)
            {
                return "exceptionoccurred";
            }
        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestadddrivercc")]
        public string requestAddDriverCC(string mesg)
        {
            string result = "";
            ClsCCDetails obj = null;
            try
            {


                string dataValue = mesg;
                dataValue = dataValue.Trim();

                obj = new JavaScriptSerializer().Deserialize<ClsCCDetails>(dataValue);



                using (TaxiDataContext db = new TaxiDataContext())
                {



                    int driverId = obj.RecordId;


                    //

                    int ii = obj.IsDefault.ToBool() ? 1 : 0;

                    string encrypt = Cryptography.Encrypt(dataValue, driverId.ToStr(), true);
                    string query = "insert into fleet_driver_ccdetails (driverId,CCDetails,AddOn,AddBy,IsDefault) values(" + driverId + ",'" + encrypt + "',getdate(),'Driver'," + ii + ")";

                    //
                    db.ExecuteQuery<int>(query);

                    decimal Id = db.ExecuteQuery<decimal>("select @@Identity").FirstOrDefault();
                    //   result = "success:" + Id.ToStr();

                    obj.Id = Id.ToLong();
                    obj.message = "Card Added Successfully";

                    if (obj.url.ToStr().Trim().Length > 0 && obj.IsDefault.ToBool())
                    {
                        try
                        {
                            string url = obj.url.ToStr().Trim();
                            string baseUrl = url.Remove(obj.url.IndexOf("?Data=") + 6);
                            url = url.Replace(baseUrl, "").Trim();
                            url = url.Replace(" ", "+");

                            //
                            url = Cryptography.Decrypt(url, "tcloudX@@!", true);
                            CardStreamSettings objCard = new JavaScriptSerializer().Deserialize<CardStreamSettings>(url);


                            objCard.cardNumber = obj.cardNumber;
                            objCard.cardExpiryMM = obj.cardExpiryMM;
                            objCard.cardExpiryYY = obj.cardExpiryYY;
                            objCard.customerName = obj.customerName;
                            objCard.customerPostcode = obj.customerPostcode;
                            objCard.customerAddress = obj.customerAddress;



                            /* else
                             {
                                 var item = db.ExecuteQuery<ClsCCDetails>("select Id,CCDetails,IsDefault from  fleet_driver_ccdetails where isdefault=1 and driverid=" + driverId).FirstOrDefault();

                                 List<ClsCCDetails> list = new List<ClsCCDetails>();


                                     string decrypt = Cryptography.Decrypt(item.CCDetails, driverId.ToStr(), true);



                                 CardStreamSettings objCard2 = new JavaScriptSerializer().Deserialize<CardStreamSettings>(decrypt);
                                 objCard.cardNumber = objCard2.cardNumber;
                                 objCard.cardExpiryMM = objCard2.cardExpiryMM;
                                 objCard.cardExpiryYY = objCard2.cardExpiryYY;
                                 objCard.customerName = objCard2.customerName;
                                 objCard.customerPostcode = objCard2.customerPostcode;
                                 objCard.customerAddress = objCard2.customerAddress;




                             }*/

                            url = new JavaScriptSerializer().Serialize(objCard);

                            url = Cryptography.Encrypt(url, "tcloudX@@!", true);
                            obj.url = baseUrl + url;
                        }
                        catch
                        {

                        }

                    }
                }

                result = new JavaScriptSerializer().Serialize(obj);
                //send message back to PDA


                return result;

            }
            catch (Exception ex)
            {
                obj.message = ex.Message;
                result = new JavaScriptSerializer().Serialize(obj);
                //send message back to PDA


                return result;
            }
        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestremovedrivercc")]

        public string requestRemoveDriverCC(string mesg)
        {
            string result = "";
            try
            {


                string dataValue = mesg;
                dataValue = dataValue.Trim();

                ClsCCDetails obj = new JavaScriptSerializer().Deserialize<ClsCCDetails>(dataValue);



                using (TaxiDataContext db = new TaxiDataContext())
                {



                    int driverId = obj.RecordId;


                    //

                    string query = "delete from fleet_driver_CCDetails where id=" + obj.Id;

                    db.ExecuteQuery<int>(query);


                    result = "success:Card removed successfully!";



                }

                //send message back to PDA


                return result;

            }
            catch (Exception ex)
            {
                result = "failed:" + ex.Message;
                return result;
            }
        }




        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestupdatedrivercc")]
        public string requestUpdateDriverCC(string mesg)
        {
            string result = "";
            ClsCCDetails obj = null;
            try
            {


                string dataValue = mesg;
                dataValue = dataValue.Trim();

                obj = new JavaScriptSerializer().Deserialize<ClsCCDetails>(dataValue);



                using (TaxiDataContext db = new TaxiDataContext())
                {



                    int driverId = obj.RecordId;


                    //
                    string encrypt = Cryptography.Encrypt(dataValue, driverId.ToStr(), true);



                    int ii = obj.IsDefault.ToBool() == true ? 1 : 0;

                    string query = "update fleet_driver_CCDetails set CCDetails='" + encrypt + "',IsDefault=" + ii + " where driverid=" + obj.RecordId + " and id=" + obj.Id;

                    db.ExecuteQuery<int>(query);

                    if (obj.IsDefault.ToBool())
                    {
                        db.ExecuteQuery<int>("update fleet_driver_CCDetails set isdefault=0 where id!=" + obj.Id);
                        ///

                    }
                    obj.message = "Card updated successfully!";


                    if (obj.url.ToStr().Trim().Length > 0 && obj.IsDefault.ToBool())
                    {
                        try
                        {
                            //
                            string url = obj.url.ToStr().Trim();
                            string baseUrl = url.Remove(obj.url.IndexOf("?Data=") + 6);
                            url = url.Replace(baseUrl, "").Trim();
                            url = url.Replace(" ", "+");


                            //   url = url.Remove(url.LastIndexOf("u0026"));
                            //   url = url.Remove(url.LastIndexOf("u0026"));

                            url = Cryptography.Decrypt(url, "tcloudX@@!", true);
                            CardStreamSettings objCard = new JavaScriptSerializer().Deserialize<CardStreamSettings>(url);


                            objCard.cardNumber = obj.cardNumber;
                            objCard.cardExpiryMM = obj.cardExpiryMM;
                            objCard.cardExpiryYY = obj.cardExpiryYY;
                            objCard.customerName = obj.customerName;
                            objCard.customerPostcode = obj.customerPostcode;
                            objCard.customerAddress = obj.customerAddress;





                            url = new JavaScriptSerializer().Serialize(objCard);

                            url = Cryptography.Encrypt(url, "tcloudX@@!", true);
                            obj.url = baseUrl + url;

                        }
                        catch
                        {

                        }
                    }

                    result = new JavaScriptSerializer().Serialize(obj);

                }

                //send message back to PDA


                return result;

            }
            catch (Exception ex)
            {
                obj.Id = 0;
                obj.message = ex.Message;
                //  result = "failed:" + ex.Message;
                result = new JavaScriptSerializer().Serialize(obj);
                return result;
            }
        }



        #region NEW DRIVER APP METHODS

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestAllDrivers")]
        public ResponseData requestAllDrivers()
        {
            ResponseData res = new ResponseData();
            try
            {
                using (TaxiDataContext db = new TaxiDataContext())
                {

                    var result = db.Fleet_Drivers.Where(c => c.HasPDA != null && c.HasPDA == true && c.IsActive == true).OrderBy(c => c.DriverNo)
                                    .Select(args => args.Id + "," + args.DriverNo + "," + args.DriverName + "," + args.Fleet_VehicleType.VehicleType.ToUpper()).ToArray<string>();

                    res.Data = new JavaScriptSerializer().Serialize(result);
                    res.IsSuccess = true;
                    res.Message = "";


                }

            }
            catch (Exception ex)
            {
                res.Data = null;
                res.IsSuccess = false;
                res.Message = ex.Message;
            }

            return res;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestDriverSettings")]
        public ResponseData requestDriverSettings(string mesg)
        {


            try
            {

                File.AppendAllText(AppContext.BaseDirectory + "\\requestDriverSettings.txt", DateTime.Now.ToStr() + " request" + mesg + Environment.NewLine);

            }
            catch
            {

            }

            string message = mesg;

            ResponseData res = new ResponseData();
            try
            {
                ///DriverDetail objDriverDetail = Newtonsoft.Json.JsonConvert.DeserializeObject<DriverDetail>(mesg);

                //update settings in json Format 
                string dataValue = message;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });
                int driverId = values[1].ToInt();///int.Parse(objDriverDetail.DrvId); ///

                using (TaxiDataContext db = new TaxiDataContext())
                {
                    //var obj = General.GetObject<Fleet_Driver_PDASetting>(c => c.DriverId == driverId.ToInt());                    
                    var obj = db.Fleet_Driver_PDASettings.Where(c => c.DriverId == driverId.ToInt()).FirstOrDefault();
                    //
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

                        pda.EnableOptMeter = "0"; // index 36
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

                        var EnableRecoverAuth = "0";
                        try
                        {
                            EnableRecoverAuth = db.ExecuteQuery<string>("Select SetVal From AppSettings where setkey='EnableRecoverAuth'").FirstOrDefault().ToStr();
                        }
                        catch
                        {
                            EnableRecoverAuth = "0";
                        }
                        //need to comment
                        pda.DisableJobAuth = ((obj.DisableRejectJobAuth.ToBool() ? ((EnableRecoverAuth == "1") ? "1" : "2") : "0"));


                        pda.showDestAfterPob = (obj.ShowDestinationAfterPOB.ToBool() ? "1" : "0");

                        //  pda.EnableBidOnPlots = Global.EnableBidOnPlots;
                        //  pda.DriverPay = Global.DriverPay;
                        pda.enableCallOffice = Global.enableCallOffice;
                        pda.isRingback = Global.enableRingBack;
                        //    pda.SyncBookingHistory = "1";
                        // new 598
                        pda.isDriverConnectEnabled = ((obj.EnableDriverConnect.ToBool() ? "1" : "0"));



                        pda.SyncFutureJobs = "1";
                        pda.EnableBidOnPlots = "1";
                        pda.SyncMessageTemplates = "1";

                        pda.SyncBookingHistory = "1";
                        pda.EnableWaitingAfterArrive = Global.EnableWaitingAfterArrive;
                        //
                        pda.EnableOnlineStatus = "1";

                        //  if (pda.EnableDriverPinLogin == "1")
                        //if (pda.EnableCompanyCars == "1")
                        //    pda.EnableDriverPinLogin = "1";
                        //else
                        pda.EnableDriverPinLogin = "0";
                        pda.EnableSocketIO = "1";
                        pda.EnableLocationAck = "1";
                        pda.CheckJobStatus = "1";

                        if (Global.EnablaDriverDocuments.ToStr() == "1")
                            pda.EnableDriverDocuments = "2";

                        pda.DisableEarning = "2";
                        pda.AcceptJobAdditional = Global.AcceptJobAdditional;
                        pda.EnableWaitingOnAddStop = Global.EnableWaitingOnAddStop;
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



                            var firstBooking = db.Bookings.Where(c => c.DriverId == driverId).Select(c => c.PickupDateTime).OrderBy(c => c).FirstOrDefault();
                            int workingSince = 0;
                            if (firstBooking != null)
                            {
                                workingSince = DateTime.Now.Date.Subtract(firstBooking.Value).TotalDays.ToInt();
                            }




                            int bookingscount = db.Bookings.Where(c => c.DriverId == driverId).Count();


                            var data = new DriverProfile
                            {


                                DriverName = pda.DrvName,
                                DriverNo = pda.DrvNo,
                                Bookings = bookingscount,
                                Rating = obj.Fleet_Driver.AvgRating == null ? 5.0m : obj.Fleet_Driver.AvgRating.ToDecimal(),
                                Address = obj.Fleet_Driver.Address,
                                Mobile = obj.Fleet_Driver.MobileNo,
                                WorkingSince = workingSince,
                                WorkingSinceUnit = "Days",
                                VehicleNo = obj.Fleet_Driver.VehicleNo,
                                VehicleColor = obj.Fleet_Driver.VehicleColor,
                                VehicleMake = obj.Fleet_Driver.VehicleMake,
                                VehicleModel = obj.Fleet_Driver.VehicleModel,
                                VehicleTypeId = obj.Fleet_Driver.VehicleTypeId.ToInt(),
                                VehicleType = obj.Fleet_Driver.Fleet_VehicleType.VehicleType,
                                Image = "http://tradrv.co.uk/DispatchDriverImages/" + Instance.objPolicy.DefaultClientId.ToStr().Replace("_", "").ToStr().Trim() + "_" + pda.DrvNo.ToStr() + ".jpg"


                            };

                            pda.objProfile = data;
                        }
                        catch
                        {


                        }




                        //}
                        //

                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(pda);



                        res.Data = "update settings<<<" + json;
                        res.IsSuccess = true;
                        res.Message = "";


                        GC.Collect();
                    }

                }
            }
            catch (Exception ex)
            {
                res.Data = null;
                res.IsSuccess = false;
                //  res.Message = ex.Message;


                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\requestDriverSettings_exception.txt", DateTime.Now.ToStr() + " request" + message + ",exception:" + ex.Message + Environment.NewLine);

                }
                catch
                {

                }
            }

            return res;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestShiftLogin")]
        public ResponseData requestShiftLogin(string mesg)
        {
            try
            {

                File.AppendAllText(AppContext.BaseDirectory + "\\requestShiftLoginx.txt", DateTime.Now.ToStr() + " request" + mesg + Environment.NewLine);

            }
            catch
            {

            }


            DriverDetail obj = Newtonsoft.Json.JsonConvert.DeserializeObject<DriverDetail>(mesg);

            ResponseData res = new ResponseData();

            try
            {
                //await Task.Run(() =>
                //{
                try
                {


                    int driverId = obj.DrvId.ToInt();
                    string driverno = obj.DrvNo.ToStr();
                    string password = obj.Password.ToStr();
                    decimal? pdaversion = obj.version.ToDecimal();
                    string vehicle = obj.VehicleNo.ToStr();
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

                                if (vehicle.Length > 0 && obj.DeviceId.ToStr().Trim().Length > 0)
                                {
                                    DateTime prevDate = DateTime.Now.AddDays(-1);

                                    Fleet_DriverQueueList objQueue = db.Fleet_DriverQueueLists.Where(c => c.DriverId == driverId).OrderByDescending(c => c.Id)
                                            .FirstOrDefault(c => c.Status == true && c.LoginDateTime.Value.Date >= prevDate.Date);

                                    if (objQueue != null && !string.IsNullOrEmpty(objDriver.DeviceId.ToStr().Trim())
                                      && obj.DeviceId.ToStr().Trim().Length > 0 && obj.DeviceId.ToStr().Trim() != objDriver.DeviceId.ToStr().Trim())
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
                                    if (objDriver.RentLimit.ToDecimal() > 0)
                                    {
                                        try
                                        {



                                            if ((pdaversion.ToDecimal() > 100.00m))
                                            {
                                                //
                                                string driverPay = GetDriverPay(driverId, objDriver, pdaversion.ToStr());

                                                if (driverPay.Length > 0)
                                                {
                                                    msg = msg.Replace("true", "balance:");
                                                    msg += driverPay;

                                                    //   Clients.Caller.shiftLogin(msg.ToStr());
                                                    //   return;

                                                    res.Data = msg;
                                                    //  res.Data = new JavaScriptSerializer().Serialize(msg.ToStr());
                                                    res.IsSuccess = true;
                                                    res.Message = msg;
                                                    //     return res;
                                                }
                                            }


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
                                //else
                                //{
                                //    //if (objDriver.RentLimit.ToDecimal() > 0 && Global.DriverPay.ToStr() == "1")
                                //    //{
                                //    //    try
                                //    //    {
                                //    //        string version = pdaversion.ToStr();









                                //    //        //string driverPay = GetDriverPay(driverId, objDriver, pdaversion.ToStr());

                                //    //        //if (driverPay.Length > 0)
                                //    //        //{
                                //    //        //    msg = msg.Replace("true", "balance:");
                                //    //        //    msg += driverPay;



                                //    //        //    res.Data = new JavaScriptSerializer().Serialize(msg.ToStr());
                                //    //        //    res.IsSuccess = true;
                                //    //        //    res.Message = "";

                                //    //        //}



                                //    //    }
                                //    //    catch (Exception ex)
                                //    //    {
                                //    //        try
                                //    //        {
                                //    //            File.AppendAllText(physicalPath + "\\RentlimitException.txt", DateTime.Now.ToStr() + ":" + ex.Message + Environment.NewLine);
                                //    //        }
                                //    //        catch
                                //    //        {

                                //    //        }
                                //    //    }
                                //    //} // END RENT LIMIT
                                //}

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

                                    if (objDriver.DrivingLicenseExpiryDate >= now.Date && objDriver.DrivingLicenseExpiryDate <= now.AddDays(Global.DocumentExpiryDays))
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

                                    if (objDriver.InsuranceExpiryDate >= now.Date && objDriver.InsuranceExpiryDate <= now.AddDays(Global.DocumentExpiryDays))
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

                                    if (objDriver.RoadTaxiExpiryDate >= now.Date && objDriver.RoadTaxiExpiryDate <= now.AddDays(Global.DocumentExpiryDays))
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

                                    if (objDriver.MOTExpiryDate >= now.Date && objDriver.MOTExpiryDate <= now.AddDays(Global.DocumentExpiryDays))
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

                                    if (objDriver.MOT2ExpiryDate >= now.Date && objDriver.MOT2ExpiryDate <= now.AddDays(Global.DocumentExpiryDays))
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

                                    if (objDriver.PCODriverExpiryDate >= now.Date && objDriver.PCODriverExpiryDate <= now.AddDays(Global.DocumentExpiryDays))
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

                                    if (objDriver.PCOVehicleExpiryDate >= now.Date && objDriver.PCOVehicleExpiryDate <= now.AddDays(Global.DocumentExpiryDays))
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

                                    HubProcessor.Instance.listofJobs.Add(new clsPDA
                                    {
                                        DriverId = splitArr[1].ToInt(),
                                        JobId = 0,
                                        MessageDateTime = DateTime.Now.AddSeconds(-50),
                                        JobMessage = splitArr[3].ToStr().Trim(),
                                        MessageTypeId = splitArr[4].ToInt()
                                    });
                                }


                                if (vehicle.Length > 0)
                                {
                                    string vehNo = vehicle;
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

                            if (msg == "true" && !string.IsNullOrEmpty(alertMsg) && HubProcessor.Instance.objPolicy.PDAVersion.ToDecimal() >= 1.7m)
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
                            //////Clients.Caller.shiftLogin(msg.ToStr());-----------------------------------------------------------------
                            ///
                            // res.Data = new JavaScriptSerializer().Serialize(msg.ToStr());
                            res.Data = msg.ToStr();
                            res.IsSuccess = true;
                            res.Message = "";

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



                                if (fleetMasterId == 0)
                                {
                                    db.stp_LoginLogoutDriver(driverId, true, pdaversion);
                                }
                                else
                                {
                                    db.stp_LoginLogoutDriverVeh(driverId, fleetMasterId, true, pdaversion);
                                }

                                //if (objDriver.VehicleNo.Length>0)
                                //{
                                //    db.stp_UpdateDriverDeviceId(driverId, values[5].ToStr().Trim());
                                //}

                                General.BroadCastMessage("**login>>Drv " + driverno + " is Login" + ">>" + driverno);

                                /*Uncomment later
                                if (Instance.objPolicy.AutoLogoutInActiveDrvMins.ToInt() > 0)
                                {
                                    new BroadcasterData().BroadCastToLocal("**refresh map");
                                    // RefreshMap();
                                }
                                */

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




                                }

                                // OFFLINE MESSAGE
                                if (HubProcessor.Instance.objPolicy.DespatchOfflineJobs.ToBool())




                                {

                                    try
                                    {
                                        HubProcessor.Instance.listofJobs.RemoveAll(c => c.DriverId == driverId && c.JobMessage.StartsWith("PreJobId:"));

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
                                            if (HubProcessor.Instance.listofJobs.Count(c => c.DriverId == driverId && c.JobId == jobId) > 0)
                                                HubProcessor.Instance.listofJobs.RemoveAll(c => c.DriverId == driverId && c.JobId == jobId);

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

                                                    decimal pdafares = objBooking.GetType().GetProperty(HubProcessor.Instance.objPolicy.PDAFaresPropertyName.ToStr().Trim()).GetValue(objBooking, null).ToDecimal();

                                                    //if (Instance.objPolicy.PDAFaresPropertyName.ToStr().Trim() == "FareRate")
                                                    //{
                                                    //    pdafares = pdafares + objBooking.ServiceCharges.ToDecimal() + objBooking.CongtionCharges.ToDecimal() + objBooking.MeetAndGreetCharges.ToDecimal() + objBooking.ExtraDropCharges.ToDecimal();


                                                    //}

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

                                                        if (objBooking.BookingDate.Value.AddMinutes(10) > objBooking.PickupDateTime.Value)
                                                        {
                                                            //if (Global.enableASAPonPDA == "1")
                                                            //{

                                                            appendString += ",\"priority\":\"" + "ASAP" + "\"";
                                                            //  }
                                                        }

                                                    }
                                                    catch
                                                    {

                                                    }

                                                    //

                                                    //if (objBooking.CompanyId != null && Global.enableBookingRefOnAccJob == "1")
                                                    //{
                                                    //    if (specialRequirements.Length == 0)
                                                    //        specialRequirements = "Booking Ref- " + objBooking.BookingNo.ToStr();
                                                    //    else
                                                    //        specialRequirements = "Booking Ref- " + objBooking.BookingNo.ToStr() + " , " + specialRequirements;
                                                    //}

                                                    if (specialRequirements.ToStr().Contains("\""))
                                                        specialRequirements = specialRequirements.ToStr().Replace("\"", "-").Trim();


                                                    string toDoorNo = objBooking.ToDoorNo.ToStr().Trim();

                                                    if (objBooking.ToLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT && objBooking.JourneyTypeId.ToInt() == Enums.JOURNEY_TYPES.RETURN)
                                                        toDoorNo = string.Empty;
                                                    else if (toDoorNo.Length > 0)
                                                        toDoorNo = toDoorNo + "-";


                                                    msg = "{ \"JobId\" :\"" + objBooking.Id.ToStr() +
                                                 "\", \"Pickup\":\"" + (!string.IsNullOrEmpty(objBooking.FromDoorNo) ? fromdoorno + "-" + fromAddress + pickUpPlot : fromAddress + pickUpPlot) +
                                                  "\", \"Destination\":\"" + (toDoorNo + toAddress + dropOffPlot) + "\"," +
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
                                            resp = "alertprejoblist:[" + resp + "]";

                                            General.requestPDA("request pda=" + driverId + "=" + 0 + "=" + "Message>>" + "You have received a Future Jobs" + ">>" + String.Format("{0:MM/dd/yyyy HH:mm:ss}", DateTime.Now) + "=4");

                                            //  Global.AddOfflineJob(driverId, driverno, resp);
                                            //HubProcessor.Instance.listofJobs.Add(new clsPDA
                                            //{
                                            //    JobId = 0,
                                            //    DriverId = driverId,
                                            //    MessageDateTime = DateTime.Now,
                                            //    JobMessage = resp,
                                            //    MessageTypeId = eMessageTypes.JOB,
                                            //    DriverNo = driverno
                                            //});

                                            if (jobIds.ToStr().Trim().EndsWith(","))
                                                jobIds = jobIds.Remove(jobIds.ToStr().LastIndexOf(","));


                                            string query = "update booking set bookingstatusid=17 where id in(" + jobIds + ") and bookingstatusid=4 and driverid=" + driverId;
                                            //
                                            db.ExecuteQuery<int>(query);
                                            db.ExecuteQuery<int>("exec stp_receivedofflinejob {0},{1},{2}", jobIds, driverId, query);

                                            //
                                            string data = "jsonrefresh required dashboard";

                                            DateTime? dt = DateTime.Now.ToDateorNull();
                                            DateTime recentDays = dt.Value.AddDays(-1);
                                            DateTime dtNow = DateTime.Now;
                                            DateTime prebookingdays = dt.Value.AddDays(Instance.objPolicy.HourControllerReport.ToInt()).ToDate();





                                            List<stp_GetBookingsDataResult> query2 = db.ExecuteQuery<stp_GetBookingsDataResult>("exec stp_getbookingsdata {0},{1},{2},{3}", recentDays, prebookingdays, 0, Instance.objPolicy.DaysInTodayBooking.ToInt()).ToList();

                                            data += "|>>>|" + Newtonsoft.Json.JsonConvert.SerializeObject(query2);
                                            //  List<string> listOfConnections = new List<string>();
                                            //  listOfConnections = Instance.ReturnDesktopConnections();

                                            //HubProcessor.Instance.Clients.Clients(listOfConnections).cMessageToDesktop(data);


                                            General.BroadCastMessage(data);
                                            General.CallGetDashboardData();
                                            File.AppendAllText(physicalPath + "\\" + "controllerofflinejobreceived.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", jobIds: " + jobIds + ",driverId:" + driverId + Environment.NewLine);

                                        }
                                        catch (Exception ex)
                                        {
                                            try
                                            {
                                                File.AppendAllText(physicalPath + "\\" + "controllerofflinejobreceived_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + ex.Message + ", jobIds: " + jobIds + ",driverId:" + driverId + Environment.NewLine);
                                            }
                                            catch
                                            {

                                            }

                                        }
                                        //


                                    }

                                    s = null;
                                    GC.Collect();

                                    //-----------------------------------------------------------------
                                    //Final Return
                                    //res.Data = new JavaScriptSerializer().Serialize(resp);
                                    //res.IsSuccess = true;
                                    //res.Message = "";


                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                //-----------------------------------------------------------------
                                //res.Data = null;
                                //res.IsSuccess = false;
                                //res.Message = ex.Message;


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
                    //Clients.Caller.shiftLogin("exceptionoccurred");-----------------------------------------------------------------
                    res.Data = null;
                    res.IsSuccess = false;
                    res.Message = "exceptionoccurred";

                    try
                    {
                        File.AppendAllText(physicalPath + "\\" + "requestshiftlogin_exception1.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + ex.Message + ", data: " + new JavaScriptSerializer().Serialize(mesg) + Environment.NewLine);
                    }
                    catch
                    {

                    }
                }
                // });



            }
            catch (Exception ex)
            {
                //////Clients.Caller.shiftLogin("exceptionoccurred");-----------------------------------------------------------------
                ///
                res.Data = null;
                res.IsSuccess = false;
                res.Message = "exceptionoccurred";

                try
                {
                    File.AppendAllText(physicalPath + "\\" + "requestshiftlogin_exception2.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + ex.Message + ", data: " + new JavaScriptSerializer().Serialize(mesg) + Environment.NewLine);
                }
                catch
                {
                    //
                }
            }

            return res;
        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("Login")]
        public ResponseData Login(string mesg)
        {
            DriverDetail obj2 = Newtonsoft.Json.JsonConvert.DeserializeObject<DriverDetail>(mesg);


            try
            {

                File.AppendAllText(AppContext.BaseDirectory + "\\Login.txt", DateTime.Now.ToStr() + " request" + mesg + Environment.NewLine);

            }
            catch
            {

            }


            ResponseData resp = new ResponseData();
            try
            {


                int driverId = obj2.DrvId.ToInt();
                string pwd = obj2.Password.ToStr();
                string ver = obj2.version.ToStr();

                //Entities db = new Entities();

                using (TaxiDataContext db = new TaxiDataContext())
                {


                    var driver = db.Fleet_Drivers.Where(e => e.Id == driverId && e.LoginPassword == pwd && e.IsActive == true)
                                                          .FirstOrDefault();

                    if (driver == null)
                    {
                        resp.IsSuccess = false;
                        resp.Message = "Invalid credentials";
                    }
                    else
                    {



                        var dq = db.Fleet_DriverQueueLists.Where(e => e.DriverId == driver.Id)
                                                               .OrderByDescending(e => e.Id)
                                                               .FirstOrDefault();

                        if (dq == null || dq.Status == false)
                        {
                            Fleet_DriverQueueList obj = new Fleet_DriverQueueList();

                            obj.DriverId = driver.Id;
                            obj.Status = true;
                            obj.LoginDateTime = DateTime.Now;
                            obj.DriverWorkStatusId = Enums.Driver_WORKINGSTATUS.AVAILABLE;

                        }
                        else
                        {
                            dq.DriverWorkStatusId = Enums.Driver_WORKINGSTATUS.AVAILABLE;
                        }

                        var firstBooking = db.Bookings.Where(c => c.DriverId == driver.Id).OrderBy(c => c.PickupDateTime).Select(c => c.PickupDateTime).FirstOrDefault();
                        int workingSince = 0;
                        if (firstBooking != null)
                        {
                            workingSince = DateTime.Now.Date.Subtract(firstBooking.Value).TotalDays.ToInt();
                        }





                        //string clientUrl = DispatchHelper.GetClientUrl(Request);

                        //var checkList = await db.Fleet_Driver_Checklist.Select(e => new
                        //{
                        //    e.Id,
                        //    e.Title
                        //}).ToListAsync();


                        int bookingscount = db.Bookings.Where(c => c.DriverId == driver.Id).Count();


                        var data = new DriverProfile
                        {
                            //Image = clientUrl + $"Uploads/Images/Drivers/{driver.Id}.jpg",
                            DriverName = driver.DriverName,
                            DriverNo = driver.DriverNo,
                            Bookings = bookingscount,
                            //  Rating = driver.AvgRating.ToDecimal(),
                            Rating = driver.AvgRating == null ? 5.0m : driver.AvgRating.ToDecimal(),
                            Address = driver.Address,
                            Mobile = driver.MobileNo,
                            WorkingSince = workingSince,
                            WorkingSinceUnit = "Days",
                            VehicleNo = driver.VehicleNo,
                            VehicleColor = driver.VehicleColor,
                            VehicleMake = driver.VehicleMake,
                            VehicleModel = driver.VehicleModel,
                            VehicleTypeId = driver.VehicleTypeId.ToInt(),
                            VehicleType = driver.Fleet_VehicleType.VehicleType,
                            Image = "http://tradrv.co.uk/DispatchDriverImages/" + Instance.objPolicy.DefaultClientId.ToStr().Replace("_", "").ToStr().Trim() + "_" + driver.DriverNo.ToStr() + ".jpg"
                            // CheckList = checkList
                        };

                        //   http://tradrv.co.uk/DispatchDriverImages/$tAt!on_cAr$_(dew$bury)_ltd_test.jpg


                        try
                        {

                            File.AppendAllText(AppContext.BaseDirectory + "\\Login2.txt", DateTime.Now.ToStr() + " request" + mesg + Environment.NewLine);

                        }
                        catch
                        {

                        }

                        resp.Data = new JavaScriptSerializer().Serialize(data);

                        // List<Claim> claims = new List<Claim>();
                        resp.Message = new Guid().ToStr();
                        resp.IsSuccess = true;
                        //await db.SaveChangesAsync();
                    }
                }

            }
            catch (Exception ex)
            {
                resp.Data = null;
                resp.IsSuccess = false;
                resp.Message = ex.Message;
            }

            return resp;
        }




        //14022022
        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestlogout")] //already exist, make it change by returning ResponseData model as return type
        public ResponseData requestLogout(string mesg)
        {
            //string rtn = "";
            ResponseData res = new ResponseData();
            try
            {
                string[] values = mesg.Split(new char[] { '=' });



                using (TaxiDataContext db = new TaxiDataContext())
                {
                    if (values.Count() >= 9)
                    {

                        db.stp_LogoutDriverPenalty(values[1].ToInt(), false, true);
                        General.BroadCastMessage("**logout>>" + values[1] + ">>" + values[7] + ">>Driver " + values[7] + " is Logout(OverBreak)");
                    }
                    else
                    {

                        db.stp_LoginLogoutDriver(values[1].ToInt(), false, null);


                        General.BroadCastMessage("**logout>>Driver " + values[2] + " is Logout");
                    }

                }

                ///rtn = "true";---------------------------------------------------------
                ///
                // res.Data = new JavaScriptSerializer().Serialize("true");
                res.Data = "true";
                res.IsSuccess = true;
                res.Message = "";
            }
            catch (Exception ex)
            {
                ///rtn = "exceptionoccurred";-------------------------------------------
                ///
                res.Data = null;
                res.IsSuccess = false;
                res.Message = "exceptionoccurred";
            }

            //return rtn;
            return res;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestjoblist")] //already exist, make it change by returning ResponseData model as return type
        public ResponseData requestJobList(string mesg)
        {
            JobObject obj = null;
            ResponseData res = new ResponseData();
            try
            {

                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\requestJobListAPI.txt", DateTime.Now.ToStr() + " request" + mesg + Environment.NewLine);
                }
                catch
                {

                }

                //
                obj = new JavaScriptSerializer().Deserialize<JobObject>(mesg);



                int driverId = obj.DrvId.ToInt();



                bool shiftJobs = true;
                //
                DateTime fromDate = DateTime.Now;
                DateTime tillDate = DateTime.Now;

                if (obj.FilterFrom.ToStr().Trim().Length > 0)
                {
                    if (obj.FilterFrom.ToStr().Contains(":"))
                    {
                        DateTime.TryParseExact(obj.FilterFrom.ToStr().Trim(), "yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDate);
                        // from = fromDate;
                    }
                    else
                        DateTime.TryParseExact(obj.FilterFrom.ToStr().Trim(), "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDate);

                    shiftJobs = false;
                }

                if (obj.FilterTo.ToStr().Trim().Length > 0)
                {
                    //
                    if (obj.FilterFrom.ToStr().Contains(":"))
                    {
                        DateTime.TryParseExact(obj.FilterTo.ToStr().Trim(), "yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out tillDate);

                    }
                    else
                    {

                        DateTime.TryParseExact(obj.FilterTo.ToStr().Trim(), "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out tillDate);
                        tillDate = tillDate.AddDays(1).ToDate();
                    }

                    // tillDate = tillDate.AddDays(1).ToDate();
                    //  till = tillDate;
                    //  till = till.Value.AddDays(1).ToDate();
                    shiftJobs = false;
                }

                if (tillDate.Subtract(fromDate).TotalDays > 30)
                {

                    obj.Message = "You cannot filter more than 30 days Records";

                    obj.JobSummary = new JobSummaryModel();

                    if (obj.JobSummary.TotalCash == null)
                    {

                        obj.JobSummary.TotalCash = 0.00m;
                        obj.JobSummary.TotalCashJobs = 0;
                        obj.JobSummary.TotalCharges = 0.00m;
                        obj.JobSummary.Fares = 0.00m;
                        obj.JobSummary.AgentFee = 0.00m;
                        obj.JobSummary.BookingFee = 0.00m;
                        obj.JobSummary.Congestion = 0.00m;
                        obj.JobSummary.Extra = 0.00m;
                        obj.JobSummary.Parking = 0.00m;
                        obj.JobSummary.Waiting = 0.00m;
                        obj.JobSummary.TotalAccount = 0.00m;
                        obj.JobSummary.TotalAccountJobs = 0;
                        obj.JobSummary.TotalCard = 0.00m;
                        obj.JobSummary.TotalCashJobs = 0;

                    }

                    obj.Joblist = new List<JobModel>();


                }

                if (obj.Message.ToStr().Trim().Length == 0)
                {
                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        ////



                        if (obj.FetchType == 1)
                        {
                            var objlist = db.ExecuteQuery<JobSummaryModel>("exec stp_getjobslistSummary {0},{1},{2},{3},{4},{5}", driverId, fromDate, tillDate, 0, 0, shiftJobs).FirstOrDefault();
                            //
                            obj.JobSummary = objlist;


                            if (obj.JobSummary != null)
                            {

                                if (obj.JobSummary.TotalCash == null)
                                {
                                    //
                                    obj.JobSummary.TotalCash = 0.00m;
                                    obj.JobSummary.TotalCashJobs = 0;
                                    obj.JobSummary.TotalCharges = 0.00m;
                                    obj.JobSummary.Fares = 0.00m;
                                    obj.JobSummary.AgentFee = 0.00m;
                                    obj.JobSummary.BookingFee = 0.00m;
                                    obj.JobSummary.Congestion = 0.00m;
                                    obj.JobSummary.Extra = 0.00m;
                                    obj.JobSummary.Parking = 0.00m;
                                    obj.JobSummary.Waiting = 0.00m;
                                    obj.JobSummary.TotalAccount = 0.00m;
                                    obj.JobSummary.TotalAccountJobs = 0;
                                    obj.JobSummary.TotalCard = 0.00m;
                                    obj.JobSummary.TotalCashJobs = 0;

                                }

                                try
                                {
                                    obj.TotalOnlineMin = db.ExecuteQuery<int>("exec stp_getdriveronlinemins {0},{1},{2}", driverId, fromDate, tillDate).FirstOrDefault().ToStr();
                                }
                                catch
                                {

                                }
                            }



                            res.Data = new JavaScriptSerializer().Serialize(obj);
                            res.IsSuccess = true;
                            res.Message = "";



                            try
                            {

                                File.AppendAllText(AppContext.BaseDirectory + "\\requestJobListsummary_response.txt", DateTime.Now.ToStr() + " request" + mesg + ",response:" + res.Data + Environment.NewLine);
                            }
                            catch
                            {

                            }

                        }
                        else
                        {
                            List<JobModel> list = null;
                            if (obj.JobId == 0)
                                list = db.ExecuteQuery<JobModel>("exec stp_getjobslist {0},{1},{2},{3},{4},{5}", driverId, fromDate, tillDate, 0, 0, shiftJobs).ToList();

                            else
                                list = db.ExecuteQuery<JobModel>("exec stp_getjobdata {0},{1}", driverId, obj.JobId).ToList();

                            foreach (JobModel job in list)
                            {
                                List<ChargesSummary> listofSummary = new List<ChargesSummary>();

                                listofSummary.Add(new ChargesSummary { label = "Fares", value = string.Format("{0:0.00}", job.Fares.ToDecimal()) });
                                listofSummary.Add(new ChargesSummary { label = "Parking", value = string.Format("{0:0.00}", job.Parking.ToDecimal()) });
                                listofSummary.Add(new ChargesSummary { label = "Waiting", value = string.Format("{0:0.00}", job.Waiting.ToDecimal()) });
                                listofSummary.Add(new ChargesSummary { label = "Extras", value = string.Format("{0:0.00}", job.Extra.ToDecimal()) });
                                listofSummary.Add(new ChargesSummary { label = "Fee", value = string.Format("{0:0.00}", job.AgentFee.ToDecimal() + job.BookingFee.ToDecimal()) });

                                job.Summary = listofSummary;
                            }

                            obj.Joblist = list;


                            res.Data = new JavaScriptSerializer().Serialize(obj);
                            res.IsSuccess = true;
                            res.Message = "";



                            try
                            {

                                File.AppendAllText(AppContext.BaseDirectory + "\\requestJobList_response.txt", DateTime.Now.ToStr() + " request" + mesg + ",response:" + res.Data + Environment.NewLine);
                            }
                            catch
                            {

                            }


                            //if (obj.Joblist != null && obj.Joblist.Count > 0)
                            //{

                            //    for (int x= 0; x<500; x++)
                            //    {
                            //        obj.Joblist.Add(obj.Joblist[0]);
                            //    }


                            //}
                        }
                    }


                }

            }
            catch (Exception ex)
            {
                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\requestJobList_exception.txt", DateTime.Now.ToStr() + " request" + mesg + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }

            }
            //return obj;
            return res;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestDriverStatus")]
        public ResponseData requestDriverStatus(string mesg)
        {


            try
            {

                File.AppendAllText(physicalPath + "\\requestDriverStatus.txt", DateTime.Now.ToStr() + " mesg:" + mesg + Environment.NewLine);
            }
            catch
            {

            }

            ResponseData res = new ResponseData();
            if (mesg.Contains("{") == false)    //--------------------For string request--------------------
            {

                try
                {


                    string dataValue = mesg;
                    dataValue = dataValue.Trim();

                    string[] values = dataValue.Split(new char[] { '=' });

                    string response = "true";

                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        if (values.Count() >= 4)
                        {

                            try
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
                            catch
                            {

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

                        //send message back to PDA//-----------------------------------------------------------

                        //res.Data = new JavaScriptSerializer().Serialize(response);
                        res.Data = response;
                        res.IsSuccess = true;
                        res.Message = "You cannot go OnBreak at this time";


                        if (response == "true")
                        {
                            //    if (issuccess == false)
                            //   {
                            db.stp_ChangeDriverStatus(values[1].ToInt(), values[2].ToInt());
                            //    }

                            General.BroadCastMessage("**changed driver status");

                            //-----------------------------------------------------------
                            res.Data = response;
                            res.IsSuccess = true;
                            res.Message = "**changed driver status";
                        }
                    }


                }
                catch (Exception ex)
                {
                    res.Data = null;
                    res.IsSuccess = false;
                    res.Message = ex.Message;
                }
            }

            else //--------------------For Json request--------------------
            {
                try
                {

                    DriverDetail req = Newtonsoft.Json.JsonConvert.DeserializeObject<DriverDetail>(mesg);

                    string response = "true";



                    if (req.DStatus.ToStr() == "3")
                    {
                        using (TaxiDataContext db = new TaxiDataContext())
                        {
                            try
                            {
                                db.CommandTimeout = 5;
                                int? statusId = db.Fleet_DriverQueueLists.FirstOrDefault(c => c.Status == true && c.DriverId == req.DrvId.ToInt() && c.DriverWorkStatusId == Enums.Driver_WORKINGSTATUS.SINBIN).DefaultIfEmpty().DriverWorkStatusId;

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
                    else if (req.JobId.ToStr().Trim().Length > 0 && req.JobId.ToStr().Trim().ToLong() > 0)
                    {

                        response = "false:You cannot go OnBreak at this time";
                    }

                    ////if (dataValue.Contains("=onrequest"))
                    ////{
                    ////    BreakDriver bd = new BreakDriver();
                    ////    bd.message = "You cannot go OnBreak at this time";
                    ////    bd.breakduration = "0";
                    ////    string json = Newtonsoft.Json.JsonConvert.SerializeObject(bd);
                    ////    response = response + ">>" + json;
                    ////}

                    //send message back to PDA//-----------------------------------------------------------

                    //res.Data = new JavaScriptSerializer().Serialize(response);
                    res.Data = response;
                    res.IsSuccess = false;
                    res.Message = "You cannot go OnBreak at this time";
                    //}

                    if (response == "true")
                    {
                        //    if (issuccess == false)
                        //   {
                        using (TaxiDataContext db = new TaxiDataContext())
                        {
                            db.stp_ChangeDriverStatus(req.DrvId.ToInt(), req.DStatus.ToInt());
                        }
                        //    }

                        General.BroadCastMessage("**changed driver status");

                        //-----------------------------------------------------------
                        res.Data = response;
                        res.IsSuccess = true;
                        res.Message = "Your break request has been submitted successfully!";
                    }


                }
                catch (Exception ex)
                {
                    res.Data = null;
                    res.IsSuccess = false;
                    res.Message = ex.Message;
                }


            }

            return res;
        }





        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("panicAlert")]
        public ResponseData panicAlert(string mesg)
        {

            try
            {

                File.AppendAllText(physicalPath + "\\panicAlert.txt", DateTime.Now.ToStr() + " mesg:" + mesg + Environment.NewLine);
            }
            catch
            {

            }

            ResponseData res = new ResponseData();
            string dataValue = mesg;
            dataValue = dataValue.Trim();

            string[] values = dataValue.Split(new char[] { '=' });

            try
            {
                bool IsPanic = values[2].ToBool();

                using (TaxiDataContext db = new TaxiDataContext())
                {
                    db.stp_PanicUnPanicDriver(values[1].ToInt(), IsPanic);
                }
                General.BroadCastMessage("**changed driver status");



                res.Data = "true";
                res.IsSuccess = true;
                res.Message = "**changed driver status";

            }
            catch (Exception ex)
            {


                res.Data = null;
                res.IsSuccess = false;
                res.Message = ex.Message;
            }

            return res;
        }




        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestRingback")]
        public ResponseData requestRingback(string mesg)
        {
            string message = mesg;
            ResponseData res = new ResponseData();

            try
            {
                string[] values = message.Split(new char[] { '=' });

                string bookingId = values[0].ToStr();
                string custName = values[1].ToStr();
                string mobNo = "";

                string driverId = values[3].ToStr();
                string driverNO = values[4].ToStr();


                string companyTelNo = string.Empty;

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




                string tokenNO = HubProcessor.Instance.objPolicy.CallRecordingToken.ToStr();




                if (mobNo.ToStr().Trim().Length > 0)
                {

                    RingbackVIPCallerRequest requestData = new RingbackVIPCallerRequest()
                    {
                        token = tokenNO,
                        destination = mobNo,
                        extension = "850",
                        callerId = "",
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


                    //   Clients.Caller.responseRingback(resp);


                    ///Clients.Caller.responseRingback(resp);----------------------------------------------------------
                    ///
                    res.Data = resp;
                    res.IsSuccess = true;
                    res.Message = "";


                    try
                    {
                        try
                        {
                            File.AppendAllText(AppContext.BaseDirectory + "ringbacklog.txt", "datastring:" + message + "RESULT : " + resp + " on time :" + DateTime.Now.ToStr());
                        }
                        catch
                        {


                        }

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
                    ///Clients.Caller.responseRingback("failed:RingBack No number found");-------------------------------------------------------------------
                    ///
                    res.Data = new JavaScriptSerializer().Serialize("failed:RingBack No number found");
                    res.IsSuccess = true;
                    res.Message = "";

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
                ///Clients.Caller.responseRingback("failed:RingBack Delivery Failed");------------------------------------------------------
                ///
                res.Data = new JavaScriptSerializer().Serialize("failed:RingBack Delivery Failed"); ;
                res.IsSuccess = false;
                res.Message = "";



                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "ringbacklog_exception.txt", "datastring:" + message + "RESULT : " + ex.Message + ",failed:RingBack Delivery Failed" + " on time :" + DateTime.Now.ToStr());

                }
                catch
                {


                }
            }

            return res;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestsendingmessage")] //already exist, make it change by returning ResponseData model as return type
        public ResponseData requestSendingMessage(string mesg)
        {
            ResponseData res = new ResponseData();
            //string rtn = "false";
            try
            {

                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                if (values.Count() >= 7)
                {
                    //send acknowledgement message to PDA
                    ///rtn = "true";----------------------------------------
                    ///
                    res.Data = "true";
                    res.IsSuccess = true;
                    res.Message = "";


                }

                using (TaxiDataContext db = new TaxiDataContext())
                {
                    db.stp_SendMessage(values[1].ToInt(), values[2].ToInt(), values[3].ToStr(), "", values[4].ToStr(), values[5].ToStr());
                }
                General.BroadCastMessage("**message>>" + values[1].ToStr() + ">>" + values[3].ToStr() + ">>" + values[4].ToStr() + ">>" + string.Format("{0:dd/MMM HH:mm:ss}", DateTime.Now));
            }
            catch (Exception ex)
            {
                ///rtn = "exceptionoccurred";-------------------------------------
                ///
                res.Data = null;
                res.IsSuccess = false;
                res.Message = "exceptionoccurred";
            }

            //return rtn;
            return res;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestOfficeBase")]
        public ResponseData requestOfficeBase(string mesg)
        {
            ResponseData res = new ResponseData();
            try
            {


                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                string postcode = GetPostCodeMatch(HubProcessor.Instance.objPolicy.BaseAddress.ToStr());

                var obj = General.GetObject<Gen_Coordinate>(c => c.PostCode == postcode);

                if (obj != null)
                {
                    postcode = postcode + ">>>" + obj.Latitude + ">>>" + obj.Longitude;
                }

                ///Clients.Caller.officeBase(postcode);---------------------------------------------
                ///
                res.Data = postcode;
                res.IsSuccess = true;
                res.Message = "";

            }
            catch (Exception ex)
            {
                ///Clients.Caller.officeBase(ex.Message);-----------------------------------------------------------
                ///
                res.Data = null;
                res.IsSuccess = false;
                res.Message = ex.Message;
            }

            return res;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestNavigation")]
        public ResponseData requestNavigation(string mesg)
        {
            ResponseData res = new ResponseData();
            try
            {


                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                string address = values[1].ToStr();
                string response = string.Empty;

                address = address.Replace("+", " ").Trim();

                try
                {

                    if (address.Contains("<<<"))
                        address = address.Remove(address.IndexOf("<<<"));
                    else if (address.Contains("\u003c\u003c\u003c"))
                    {
                        address = address.Remove(address.IndexOf("\u003c\u003c\u003c"));
                    }
                }
                catch
                {

                }
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
                ///Clients.Caller.navigation(response);-------------------------------------
                ///               

                res.Data = response;
                res.IsSuccess = true;
                res.Message = "";

            }
            catch (Exception ex)
            {
                ///Clients.Caller.navigation(ex.Message);----------------------------------------------------
                ///
                res.Data = null;
                res.IsSuccess = false;
                res.Message = ex.Message;
            }

            return res;
        }

        //[System.Web.Http.HttpGet]
        //[System.Web.Http.HttpPost]
        //[System.Web.Http.Route("requestNavigation")]
        //public ResponseData requestNavigation(string mesg)
        //{
        //    ResponseData res = new ResponseData();
        //    try
        //    {


        //        string dataValue = mesg;
        //        dataValue = dataValue.Trim();

        //        string[] values = dataValue.Split(new char[] { '=' });

        //        string address = values[1].ToStr();
        //        string response = string.Empty;

        //        address = address.Replace("+", " ").Trim();

        //        stp_getCoordinatesByAddressResult result = null;
        //        using (TaxiDataContext db = new TaxiDataContext())
        //        {
        //            result = db.stp_getCoordinatesByAddress(address, General.GetPostCodeMatch(address.ToStr().ToUpper().Trim())).FirstOrDefault();
        //        }

        //        if (result != null && result.Latitude != null && result.Latitude > 0)
        //        {
        //            response = "{ \"Address\" :\"" + address +
        //                    "\", \"Latitude\":\"" + result.Latitude +
        //                    "\", \"Longitude\":\"" + result.Longtiude + "\"  }";
        //        }
        //        else
        //        {
        //            response = "{ \"Address\" :\"" + address +
        //                    "\", \"Latitude\":\"" + "0" +
        //                    "\", \"Longitude\":\"" + "0" + "\"  }";
        //        }

        //        //try
        //        //{

        //        //    File.AppendAllText(Application.StartupPath+"\\navigate.txt",DateTime.Now.ToStr()+" response" +response+" datavalue"+dataValue+Environment.NewLine);
        //        //}
        //        //catch
        //        //{

        //        //}

        //        //send message back to PDA
        //        ///Clients.Caller.navigation(response);-------------------------------------
        //        ///               

        //        res.Data = response;
        //        res.IsSuccess = true;
        //        res.Message = "";

        //    }
        //    catch (Exception ex)
        //    {
        //        ///Clients.Caller.navigation(ex.Message);----------------------------------------------------
        //        ///
        //        res.Data = null;
        //        res.IsSuccess = false;
        //        res.Message = ex.Message;
        //    }

        //    return res;
        //}


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestJobLate")]
        public ResponseData requestJobLate(string mesg)
        {
            ResponseData res = new ResponseData();


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

                res.Data = dataValue;
                res.IsSuccess = true;
                res.Message = "";

            }
            catch (Exception ex)
            {
                //res.Data = null;
                //res.IsSuccess = false;
                //res.Message = ex.Message;
            }

            return res;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestMeterType")]
        public ResponseData requestMeterType(string mesg)
        {
            try
            {
                File.AppendAllText(physicalPath + "\\requestMeterType.txt", DateTime.Now.ToStr() + " request: " + mesg + Environment.NewLine);
            }
            catch
            {

            }

            ResponseData data = new ResponseData();
            try
            {


                string dataValue = mesg;
                dataValue = dataValue.Trim();



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

                data.Data = response;
                data.IsSuccess = true;

                //send message back to PDA
                // Clients.Caller.meterType(response);

                //GC.Collect();
            }
            catch (Exception ex)
            {
                data.Message = ex.Message;
                data.IsSuccess = false;



            }

            return data;
        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestFlagDown")]
        public ResponseData requestFlagDown(string mesg)
        {


            try
            {
                File.AppendAllText(physicalPath + "\\requestFlagDown.txt", DateTime.Now.ToStr() + " mesg: " + mesg + Environment.NewLine);
            }
            catch
            {

            }

            ResponseData resp = new ResponseData();
            try
            {
                byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                using (TaxiDataContext db = new TaxiDataContext())
                {
                    db.DeferredLoadingEnabled = false;

                    string pickup = values[1].ToStr();
                    //  int? zoneId = 0;
                    try
                    {


                        if (pickup.Length == 0 || pickup.ToLower() == "unknown")
                        {
                            try
                            {

                                File.AppendAllText(AppContext.BaseDirectory + "\\flagPickup.txt", DateTime.Now.ToStr() + ": msg :" + mesg + Environment.NewLine);
                            }
                            catch (Exception ex)
                            {



                            }
                            int driverId = values[6].ToInt();
                            var obj = db.Fleet_Driver_Locations.Where(c => c.DriverId == driverId).Select(args => new { args.Latitude, args.Longitude }).FirstOrDefault();

                            pickup = General.GetLocationName(obj.Latitude, obj.Longitude).ToStr().ToUpper();



                            //if(pickup.ToStr().Length>0)
                            //{

                            // zoneId=   General.GetZoneId(pickup.ToStr().ToUpper());


                            //}
                        }
                    }
                    catch
                    {

                    }

                    string vehicleName = values[7].ToStr().ToLower().Trim();
                    var res = db.stp_InsertOnRoadJob(pickup, values[2].ToStr(), values[3].ToDecimal(), values[4].ToStr()
                                    , values[5].ToStr(), values[6].ToInt(), vehicleName);

                    long jobId = 0;

                    if (res != null)
                        jobId = res.FirstOrDefault().jobid.ToLong();

                    //  //string respo=jobId.ToStr()+",1,0.5,2";
                    string isMeter = "0";

                    string fareJson = string.Empty;

                    string respo = jobId.ToStr();
                    if (HubProcessor.Instance.objPolicy.FareMeterType == null)
                    {
                        respo += ",0,0,1";
                    }
                    else
                    {
                        //respo += "," + Instance.objPolicy.FareMeterRoundedCalc.ToDecimal() + "," + meterType;


                        int? vehicleTypeId = 0;

                        vehicleTypeId = db.Fleet_VehicleTypes.Where(c => c.VehicleType.ToLower() == vehicleName).Select(c => c.Id).FirstOrDefault();
                        if (vehicleTypeId == null)
                            vehicleTypeId = 0;

                        //    int vehicleTypeId = General.GetObject<Taxi_Model.Fleet_VehicleType>(c => c.VehicleType.ToLower() == values[6].ToStr().ToLower().Trim()).DefaultIfEmpty().Id.ToInt();
                        InitializeMeterList();
                        bool enableFareMeter = Global.listofMeter.FirstOrDefault(c => c.VehicleTypeId == vehicleTypeId).DefaultIfEmpty().HasMeter.ToBool();

                        if (enableFareMeter && Global.listofMeter != null && Global.listofMeter.Count > 0)
                        {
                            if (enableFareMeter)
                            {
                                isMeter = "1";

                                var obj = Global.listofMeter.FirstOrDefault(c => c.VehicleTypeId == vehicleTypeId).DefaultIfEmpty();

                                FareMeterSettings fareJsonArr = null;

                                if (HubProcessor.Instance.objPolicy.PDANewWeekMessageByDay.ToStr().StartsWith("{"))
                                    fareJsonArr = new JavaScriptSerializer().Deserialize<FareMeterSettings>(HubProcessor.Instance.objPolicy.PDANewWeekMessageByDay.ToStr());
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

                                fareJsonArr.EnableDropOffAction = "3";
                                fareJsonArr.meterTarrif = new List<MeterTarrif>();

                                decimal roundJourneyMile = HubProcessor.Instance.objPolicy.RoundJourneyMiles.ToDecimal();
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
                                        FullRoundFares = HubProcessor.Instance.objPolicy.RoundMileageFares.ToBool() ? 1 : 0,
                                        RoundUpTo = HubProcessor.Instance.objPolicy.RoundUpTo.ToDecimal(),
                                        WaitingSecondsToDivide = obj.AccWaitingChargesPerMin.ToInt(),
                                        RoundJourneyMiles = roundJourneyMile,
                                        FreeWaitingMins = obj.FreeWaitingSeconds.ToInt()
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
                                        FullRoundFares = HubProcessor.Instance.objPolicy.RoundMileageFares.ToBool() ? 1 : 0,
                                        RoundUpTo = HubProcessor.Instance.objPolicy.RoundUpTo.ToDecimal(),
                                        WaitingSecondsToDivide = obj.AccWaitingChargesPerMin.ToInt(),
                                        RoundJourneyMiles = roundJourneyMile,
                                        FreeWaitingMins = obj.FreeWaitingSeconds.ToInt()
                                    });
                                }


                                fareJson = ",jsonstring|" + new JavaScriptSerializer().Serialize(fareJsonArr).Replace(",", "|");
                                //
                            }
                        }

                        //

                        respo += "," + isMeter;

                        int meterType = 3;

                        respo += "," + HubProcessor.Instance.objPolicy.FareMeterRoundedCalc.ToDecimal() + "," + meterType;

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
                    ///Clients.Caller.flagDown(respo);
                    ///-------------------------------------------------------------------------------------------

                    General.BroadCastMessage("**action>>" + jobId + ">>" + values[6].ToStr() + ">>" + Enums.BOOKINGSTATUS.POB);

                    resp.Data = respo;
                    resp.IsSuccess = true;
                    resp.Message = "**action>>" + jobId + ">>" + values[6].ToStr() + ">>" + Enums.BOOKINGSTATUS.POB;

                }

            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(physicalPath + "\\requestFlagDown_exception.txt", DateTime.Now.ToStr() + " mesg: " + mesg + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
                //Clients.Caller.flagDown(ex.Message);
                resp.Data = null;
                resp.IsSuccess = false;
                resp.Message = ex.Message;
            }

            return resp;
        }





        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestMakeSignature")]
        public ResponseData requestMakeSignature(JobAction mesg)
        {


            try
            {
                File.AppendAllText(physicalPath + "\\requestMakeSignatureapi.txt", DateTime.Now + " ," + ", message" + new JavaScriptSerializer().Serialize(mesg));

            }
            catch
            {

            }


            JobAction objAction = mesg;

            ResponseData res = new ResponseData();
            string jStatus = string.Empty;
            try
            {
                //  Newtonsoft.Json.Linq.JArray arr = (Newtonsoft.Json.Linq.JArray)mesg;

                string base64Decoded = "";
                byte[] inputBuffer = null;


                string[] values = null;

                string dataValue = string.Empty;



                //try
                //{
                //    if (mesg.ToStr().StartsWith("jaction") == false)
                //    {
                //        inputBuffer = System.Convert.FromBase64String(mesg);
                //        base64Decoded = System.Text.ASCIIEncoding.ASCII.GetString(inputBuffer);


                //        dataValue = base64Decoded;
                //        dataValue = dataValue.Trim();
                //        values = dataValue.Split(new char[] { '=' });
                //    }
                //    else
                //    {
                //        dataValue = mesg;
                //        values = dataValue.Split(new char[] { '=' });
                //    }
                //}
                //catch
                //{

                //}

                string rrr = "false";



                //if (values != null)
                //{
                //    objAction = new JavaScriptSerializer().Deserialize<JobAction>(values[1].ToStr());

                //}
                //else
                //{

                //   objAction = new JavaScriptSerializer().Deserialize<JobAction>(mesg);
                //   }

                jStatus = objAction.JStatus.ToStr().ToLower();


                if (objAction.JStatus.ToStr().ToLower() == "accountcharges")
                {
                    try
                    {



                        byte[] arr2 = null;

                        if (objAction.Signature.ToStr().Trim().Length > 0)
                        {
                            inputBuffer = System.Convert.FromBase64String(objAction.Signature);
                            base64Decoded = System.Text.ASCIIEncoding.ASCII.GetString(inputBuffer);
                        }
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
                                    //

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
                        ///Clients.Caller.makeSignature(rrr);----------------------------------
                        ///

                        res.Data = rrr;
                        res.IsSuccess = true;
                        res.Message = "";

                    }
                    catch (Exception ex)
                    {

                        try
                        {
                            File.AppendAllText(physicalPath + "\\exceptionelse_accountsignature.txt", DateTime.Now + " ," + ex.Message + ", message" + new JavaScriptSerializer().Serialize(mesg));

                        }
                        catch
                        {

                        }


                    }

                }

            }
            catch (Exception ex)
            {
                ///Clients.Caller.makeSignature("Exception Occurred");-------------------------------------------------------
                ///
                res.Data = null;
                res.IsSuccess = false;
                res.Message = "Exception Occurred";

                try
                {
                    File.AppendAllText(physicalPath + "\\exception_accountsignature.txt", DateTime.Now + " ," + ex.Message + ", message" + new JavaScriptSerializer().Serialize(mesg));

                }
                catch
                {

                }

            }

            return res;
        }




        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestAccountCharges")]
        public ResponseData requestAccountCharges(string mesg)
        {
            ResponseData res = new ResponseData();
            try
            {


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
                ///Clients.Caller.AccountCharges(response);--------------------------------------------------
                ///
                res.Data = response;
                res.IsSuccess = true;
                res.Message = "";



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
                    ///Clients.Caller.AccountCharges("nocharges");------------------------------------------------------------
                    ///
                    res.Data = null;
                    res.IsSuccess = false;
                    res.Message = "nocharges";


                    File.AppendAllText(physicalPath + "\\requestaccountcharges_exception.txt", DateTime.Now + " ," + ex.Message + ", message" + mesg);


                }
                catch
                {


                }


            }

            return res;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("preJobActionButton")]
        public ResponseData preJobActionButton(string mesg)
        {
            ResponseData res = new ResponseData();

            try
            {


                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                int valCnt = values.Count();
                string response = "true";


                using (TaxiDataContext db = new TaxiDataContext())
                {

                    try
                    {
                        if (db.Bookings.Count(c => c.Id == values[1].ToLong() && c.DriverId == values[2].ToInt()) == 0)
                            response = "false";



                        res.Data = response;
                        res.IsSuccess = true;
                        res.Message = "";



                        long jobId = values[1].ToLong();
                        int driverId = values[2].ToInt();
                        int jobStatusId = values[3].ToInt();

                        if (response == "true")
                        {
                            db.stp_UpdateFutureJob(jobId, driverId, jobStatusId, null);

                            General.BroadCastMessage("**prejob action>>" + values[1].ToStr() + ">>" + values[2].ToStr() + ">>" + values[3].ToInt());

                            if (HubProcessor.Instance.listofJobs.Count(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong()) > 0)
                            {
                                HubProcessor.Instance.listofJobs.RemoveAll(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong());
                                //Instance.listofJobs.LastOrDefault(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong() && c.MessageTypeId==eMessageTypes.JOB).IsAccepted=true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        res.Data = null;
                        res.IsSuccess = false;
                        res.Message = ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                ///--------------------------------------------------------------------
                ///
                //if (mesg.EndsWith("=17"))
                //    Clients.Caller.preJobAccepted(ex.Message);
                //else
                //    Clients.Caller.preJobRejected(ex.Message);

                res.Data = null;
                res.IsSuccess = false;
                res.Message = ex.Message;
            }

            return res;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("preJobStart")]
        public ResponseData preJobStart(string mesg)
        {
            ResponseData res = new ResponseData();
            try
            {


                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                int valCnt = values.Count();

                long jobId = values[1].ToLong();
                int driverId = values[2].ToInt();

                string IsAvalable = "false";

                using (TaxiDataContext db = new TaxiDataContext())
                {
                    if (db.Bookings.Count(c => c.Id == jobId && c.DriverId == driverId && c.BookingStatusId == Enums.BOOKINGSTATUS.PENDING_START) > 0)
                        IsAvalable = "true";

                    //send message back to PDA
                    ///Clients.Caller.preJobStarted(IsAvalable);------------------------------------------------
                    ///
                    res.Data = IsAvalable;
                    res.IsSuccess = true;
                    res.Message = "";

                    //Byte[] byteResponse = Encoding.UTF8.GetBytes(IsAvalable);
                    //tcpClient.NoDelay = true;
                    //tcpClient.SendTimeout = 5000;
                    //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);

                    int jobstatusId = values[2].ToInt();

                    if (IsAvalable == "true")
                    {
                        //new TaxiDataContext().stp_UpdateJobStatus(values[1].ToLong(), Enums.BOOKINGSTATUS.ONROUTE);
                        General.BroadCastMessage("**action>>" + values[1].ToStr() + ">>" + values[2].ToStr() + ">>" + Enums.BOOKINGSTATUS.ONROUTE);

                        if (dataValue.Contains("iphone") || db.Fleet_Driver_PDASettings.Count(c => c.DriverId == driverId && c.CurrentPdaVersion < 19m) > 0)
                        {
                            db.stp_UpdateFutureJob(jobId, driverId, Enums.BOOKINGSTATUS.ONROUTE, null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ///Clients.Caller.preJobStarted(ex.Message);-------------------------------------
                ///
                res.Data = null;
                res.IsSuccess = false;
                res.Message = ex.Message;
            }

            return res;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestAuthorizationLogout")]
        public ResponseData requestAuthorizationLogout(string mesg)
        {
            ResponseData res = new ResponseData();

            try
            {


                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });


                res.Data = "true";
                res.IsSuccess = true;
                res.Message = "";


                General.BroadCastMessage("**logout auth>>" + values[1].ToStr() + ">>" + values[2].ToStr());
            }
            catch (Exception ex)
            {

                res.Data = null;
                res.IsSuccess = false;
                res.Message = ex.Message;
            }

            return res;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestNoPickup")]
        public ResponseData requestNoPickup(string mesg)
        {


            try
            {



                File.AppendAllText(physicalPath + "\\requestNoPickup.txt", DateTime.Now + " , REQUEST" + mesg + Environment.NewLine);


            }
            catch
            {


            }

            ResponseData res = new ResponseData();

            try
            {
                //  byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

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
                        //   int restrictionMins = 5;

                        if (timeString < restrictionMins)
                        {
                            timeString = restrictionMins - timeString;
                            ///Clients.Caller.noPickupAuth("false:You can press No Pickup after " + timeString.ToInt() + "min");-------------------------------------------
                            ///
                            res.Data = "false:You can press No Pickup after " + timeString.ToInt() + "min";
                            res.IsSuccess = true;
                            res.Message = "";

                            ///
                            //  return res;



                        }
                        else
                        {

                            res.Data = "true";
                            res.IsSuccess = true;
                            res.Message = "";
                        }
                    }
                    else
                    {

                        res.Data = "true";
                        res.IsSuccess = true;
                        res.Message = "";

                    }

                }
                else
                {
                    ///Clients.Caller.noPickupAuth("true");-------------------------------------------------
                    ///
                    res.Data = "true";
                    res.IsSuccess = true;
                    res.Message = "";
                }





                if (res.Data == "true")
                {

                    int driverId = values[2].ToInt();
                    string driverNo = "-";
                    try
                    {
                        using (TaxiDataContext db = new TaxiDataContext())
                        {

                            driverNo = db.Fleet_Drivers.Where(c => c.Id == driverId).Select(c => c.DriverNo).FirstOrDefault().ToStr();

                        }
                    }
                    catch
                    {

                    }

                    General.BroadCastMessage("**auth>>" + values[1].ToStr() + ">>" + values[2].ToStr() + ">>" + values[3].ToInt() + ">>" + values[4].ToInt() + ">>" + driverNo);
                }
                if (HubProcessor.Instance.listofJobs.Count(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong()) > 0)
                {
                    HubProcessor.Instance.listofJobs.RemoveAll(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong());
                    //Instance.listofJobs.LastOrDefault(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong() && c.MessageTypeId==eMessageTypes.JOB).IsAccepted=true;
                }
            }
            catch (Exception ex)
            {
                ///Clients.Caller.noPickupAuth(ex.Message);--------------------------------------------------
                ///
                res.Data = null;
                res.IsSuccess = false;
                res.Message = ex.Message;

                try
                {



                    File.AppendAllText(physicalPath + "\\requestNoPickup_exception.txt", DateTime.Now + " , REQUEST" + mesg + ",exception:" + ex.Message + Environment.NewLine);


                }
                catch
                {


                }
            }

            return res;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestRecoverJob")]
        public ResponseData requestRecoverJob(string mesg)
        {
            ResponseData res = new ResponseData();
            try
            {


                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                //send message back to PDA

                res.Data = new JavaScriptSerializer().Serialize("true");
                res.IsSuccess = true;
                res.Message = "";



                int driverId = values[2].ToInt();
                string driverNo = "-";
                try
                {
                    using (TaxiDataContext db = new TaxiDataContext())
                    {

                        driverNo = db.Fleet_Drivers.Where(c => c.Id == driverId).Select(c => c.DriverNo).FirstOrDefault().ToStr();

                    }
                }
                catch
                {

                }
                General.BroadCastMessage("**auth>>" + values[1].ToStr() + ">>" + values[2].ToStr() + ">>" + values[3].ToInt() + ">>" + values[4].ToInt() + driverNo);

                if (HubProcessor.Instance.listofJobs.Count(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong()) > 0)
                {
                    HubProcessor.Instance.listofJobs.RemoveAll(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong());
                    //Instance.listofJobs.LastOrDefault(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong() && c.MessageTypeId==eMessageTypes.JOB).IsAccepted=true;
                }
            }
            catch (Exception ex)
            {
                ///Clients.Caller.recoverJob(ex.Message);------------------------------------------------------
                ///

                res.Data = null;
                res.IsSuccess = false;
                res.Message = ex.Message;
            }

            return res;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestRejectJobAuth")]
        public ResponseData requestRejectJobAuth(string mesg)
        {
            ResponseData res = new ResponseData();

            try
            {
                //  byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                //send message back to PDA

                res.Data = "true";
                res.IsSuccess = true;
                res.Message = "";


                int driverId = values[2].ToInt();
                string driverNo = "-";
                try
                {
                    using (TaxiDataContext db = new TaxiDataContext())
                    {

                        driverNo = db.Fleet_Drivers.Where(c => c.Id == driverId).Select(c => c.DriverNo).FirstOrDefault().ToStr();

                    }
                }
                catch
                {

                }

                bool isRestricted = false;
                try
                {
                    if (values[3].ToInt() == 10) //10 is RECOVER
                    {
                        DateTime? acceptedDateTime = null;
                        using (TaxiDataContext db = new TaxiDataContext())
                        {
                            acceptedDateTime = db.Bookings.Where(c => c.Id == values[1].ToLong()).Select(c => c.AcceptedDateTime).FirstOrDefault();
                        }

                        if (acceptedDateTime != null)
                        {
                            double timeString = DateTime.Now.Subtract(acceptedDateTime.Value).TotalSeconds;


                            int restrictionSecs = Global.NoRecoverRestrictionInSec.ToInt();
                            //   int restrictionMins = 5;

                            if (restrictionSecs > 0 && timeString < restrictionSecs)
                            {
                                timeString = restrictionSecs - timeString;
                                res.Data = "false";
                                res.IsSuccess = false;
                                res.Message = "You can press Recover after " + timeString.ToInt() + " seconds.";
                                isRestricted = true;
                            }
                            else
                            {
                                isRestricted = false;
                                res.Data = "true";
                                res.IsSuccess = true;
                                res.Message = "true";
                            }
                        }


                    }
                }
                catch
                {
                }

                if (!isRestricted)
                {
                    General.BroadCastMessage("**auth>>" + values[1].ToStr() + ">>" + values[2].ToStr() + ">>" + values[3].ToInt() + ">>" + values[4].ToInt() + ">>" + driverNo);

                    if (HubProcessor.Instance.listofJobs.Count(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong()) > 0)
                    {
                        HubProcessor.Instance.listofJobs.RemoveAll(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong());
                        //Instance.listofJobs.LastOrDefault(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong() && c.MessageTypeId==eMessageTypes.JOB).IsAccepted=true;
                    }
                }
            }
            catch (Exception ex)
            {


                res.Data = null;
                res.IsSuccess = false;
                res.Message = ex.Message;
            }

            return res;
        }





        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestChangePlot")]
        public ResponseData requestChangePlot(string mesg)
        {
            ResponseData res = new ResponseData();

            try
            {
                try
                {


                    File.AppendAllText(physicalPath + "\\requestChangePlot.txt", DateTime.Now + Environment.NewLine);
                }
                catch
                { }



                var arr = HubProcessor.Instance.listOfZone.OrderBy(c => c.OrderNo).Select(args => new { ZoneId = args.Id, ZoneName = args.Area }).ToList();



                res.Data = new JavaScriptSerializer().Serialize(arr);
                res.IsSuccess = true;
                res.Message = "";

                //Byte[] byteResponse = Encoding.UTF8.GetBytes(string.Join(">>", arr));
                //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);
            }
            catch (Exception ex)
            {
                try
                {

                    ///Clients.Caller.changePlot("exceptionoccurred");-----------------------------------------------------------------------
                    ///
                    res.Data = null;
                    res.IsSuccess = false;
                    res.Message = "exceptionoccurred";


                    File.AppendAllText(physicalPath + "\\requestChangePlot_exception.txt", DateTime.Now + Environment.NewLine);
                }
                catch
                { }

            }

            return res;
        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("fojActionButton")]
        public ResponseData fojActionButton(string mesg)
        {
            ResponseData res = new ResponseData();

            try
            {


                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                string IsAvalable = "false";

                using (TaxiDataContext db = new TaxiDataContext())
                {

                    try
                    {
                        if (db.Bookings.Count(c => c.Id == values[1].ToLong() && c.DriverId == values[2].ToInt()) > 0)
                            IsAvalable = "true";



                        res.Data = IsAvalable;
                        res.IsSuccess = true;
                        res.Message = "";


                        //Byte[] byteResponse = Encoding.UTF8.GetBytes(IsAvalable);
                        //tcpClient.NoDelay = true;
                        //tcpClient.SendTimeout = 5000;
                        //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);

                        int jobstatusId = values[3].ToInt();

                        if (IsAvalable == "true")
                        {
                            db.stp_UpdateJobStatus(values[1].ToLong(), jobstatusId);

                            General.BroadCastMessage("**action>>" + values[1].ToStr() + ">>" + values[2].ToStr() + ">>" + values[3].ToInt());

                            if (HubProcessor.Instance.listofJobs.Count(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong()) > 0)
                            {
                                HubProcessor.Instance.listofJobs.RemoveAll(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        res.Data = null;
                        res.IsSuccess = false;
                        res.Message = ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                //send message back to PDA
                ///-------------------------------------------------------------------------------
                ///
                //if (mesg.EndsWith("=16"))
                //    Clients.Caller.fojJobAccepted(ex.Message);
                //else
                //    Clients.Caller.fojJobRejected(ex.Message);

                res.Data = null;
                res.IsSuccess = false;
                res.Message = ex.Message;
            }

            return res;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("fojJobStart")]
        public ResponseData fojJobStart(string mesg)
        {
            ResponseData res = new ResponseData();

            try
            {


                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                int valCnt = values.Count();

                long jobId = values[1].ToLong();
                int driverId = values[2].ToInt();

                string IsAvalable = "false";

                using (TaxiDataContext db = new TaxiDataContext())
                {
                    try
                    {

                        if (db.Bookings.Count(c => c.Id == jobId && c.DriverId == driverId &&
                            (c.BookingStatusId == Enums.BOOKINGSTATUS.FOJ || c.BookingStatusId == Enums.BOOKINGSTATUS.DISPATCHED || c.BookingStatusId == Enums.BOOKINGSTATUS.CANCELLED)) > 0)
                            IsAvalable = "true";


                        res.Data = IsAvalable;
                        res.IsSuccess = true;
                        res.Message = "";




                        int jobstatusId = values[2].ToInt();

                        if (IsAvalable == "true")
                        {
                            if (dataValue.Contains("iphone") || db.Fleet_Driver_PDASettings.Count(c => c.DriverId == driverId &&
                                (c.CurrentPdaVersion < 19m || c.CurrentPdaVersion == 19.9m)) > 0)
                            {
                                db.stp_UpdateJobStatus(values[1].ToLong(), Enums.BOOKINGSTATUS.ONROUTE);

                                db.stp_UpdateJob(jobId, driverId, Enums.BOOKINGSTATUS.ONROUTE, Enums.Driver_WORKINGSTATUS.ONROUTE, HubProcessor.Instance.objPolicy.SinBinTimer.ToInt());
                            }

                            General.BroadCastMessage("**action>>" + values[1].ToStr() + ">>" + values[2].ToStr() + ">>" + Enums.BOOKINGSTATUS.ONROUTE);
                        }

                    }
                    catch (Exception ex)
                    {
                        res.Data = null;
                        res.IsSuccess = false;
                        res.Message = ex.Message;
                    }



                }
            }
            catch (Exception ex)
            {
                ///Clients.Caller.fojJobStarted(ex.Message);------------------------------------------------------
                ///
                res.Data = null;
                res.IsSuccess = false;
                res.Message = ex.Message;
            }

            return res;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestSelectAsDirected")]
        public ResponseData requestSelectAsDirected(string mesg)
        {
            ResponseData res = new ResponseData();

            try
            {


                try
                {

                    File.AppendAllText(physicalPath + "\\" + "requestSelectAsDirected.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + mesg + Environment.NewLine);
                }
                catch
                {


                }

                ClsChangePlot obj = new JavaScriptSerializer().Deserialize<ClsChangePlot>(mesg);





                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                int? driverId = obj.DrvId.ToInt();
                long? jobId = obj.JobId.ToLong();

                int? DropOffZoneId = obj.ZoneId.ToInt();
                string DropOffZoneName = obj.ZoneName.ToStr();



                using (TaxiDataContext db = new TaxiDataContext())
                    db.stp_UpdateJobAddress(jobId, driverId, null, DropOffZoneId, DropOffZoneName, null);

                //
                if (dataValue.Contains("jsonstring|"))
                {

                    try
                    {

                        //  string pickupDateTime = obj.PickupDateTime;
                        DateTime pickupDateAndTime = DateTime.Now;

                        if (obj.PickupDateTime.ToStr().Trim().Length > 0 && obj.PickupDateTime.ToStr() != "0")
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
                                    pickupPlotId = HubProcessor.Instance.listOfZone.FirstOrDefault(c => c.Area == pickupPlot).Id;
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
                                catch (Exception ex)
                                {

                                    try
                                    {
                                        File.AppendAllText(physicalPath + "\\" + "requestSelectAsDirectedquery_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + query + ",exce[tion:" + ex.Message + Environment.NewLine);

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
                        ///Clients.Caller.selectAsDirected("success:" + new JavaScriptSerializer().Serialize(obj));-------------------------------------------
                        ///
                        res.Data = new JavaScriptSerializer().Serialize("success:" + new JavaScriptSerializer().Serialize(obj));
                        res.IsSuccess = true;
                        res.Message = "";
                    }
                    catch (Exception ex)
                    {

                        res.Data = "true";
                        res.IsSuccess = true;
                        res.Message = "";

                        try
                        {
                            ///Clients.Caller.selectAsDirected("exceptionoccurred");-------------------------------------------------------------
                            ///
                            res.Data = null;
                            res.IsSuccess = true;
                            res.Message = "exceptionoccurred";

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



                    res.Data = "true";
                    res.IsSuccess = true;
                    res.Message = "";
                }


                General.BroadCastMessage("**dropoffzone>>" + driverId + ">>" + jobId + ">>" + DropOffZoneId + ">>" + DropOffZoneName);


            }
            catch (Exception ex)
            {
                try
                {
                    ///Clients.Caller.selectAsDirected("exceptionoccurred");-----------------------------------------------------------------------------
                    ///
                    res.Data = null;
                    res.IsSuccess = false;
                    res.Message = "exceptionoccurred";

                    File.AppendAllText(physicalPath + "\\requestselectasdirected_exception.txt", DateTime.Now.ToStr() + " , datavalue=" + mesg + ",exception=" + ex.Message + Environment.NewLine);

                }

                catch
                {

                }

            }

            return res;
        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestVehicles")]
        public ResponseData requestVehicles(string mesg)
        {
            ResponseData res = new ResponseData();
            try
            {

                try
                {


                    File.AppendAllText(physicalPath + "\\requestVehicles.txt", DateTime.Now + ",request:" + mesg + Environment.NewLine);
                }
                catch
                { }

                using (TaxiDataContext db = new TaxiDataContext())
                {
                    try
                    {

                        var lista = db.Fleet_Masters.Select(args => new { args.Id, args.VehicleID }).ToList();

                        string[] arr = lista.Select(args => args.VehicleID).ToArray<string>();


                        int driverId = mesg.Split('=')[1].ToInt();

                        var list = db.Fleet_Driver_CompanyVehicles.Where(c => c.DriverId == driverId).Select(c => c.FleetMasterId).ToList();


                        if (list.Count > 0)
                        {
                            arr = lista.Where(c => list.Count(a => a == c.Id) > 0).Select(args => args.VehicleID).ToArray<string>();


                        }

                        //send message back to PDA
                        ///Clients.Caller.vehicles(string.Join(">>", arr));----------------------------------------------
                        ///
                        res.Data = string.Join(">>", arr);//new JavaScriptSerializer().Serialize(string.Join(">>", arr));
                        res.IsSuccess = true;
                        res.Message = "";
                    }
                    catch (Exception ex)
                    {
                        res.Data = null;
                        res.IsSuccess = false;
                        res.Message = ex.Message;
                    }
                }
                //Byte[] byteResponse = Encoding.UTF8.GetBytes(string.Join(">>", arr));
                //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);

            }
            catch (Exception ex)
            {
                ///Clients.Caller.vehicles(ex.Message);-----------------------------------------------
                ///
                res.Data = null;
                res.IsSuccess = false;
                res.Message = ex.Message;
            }

            return res;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestAsDirected")]
        public ResponseData requestAsDirected(string mesg)
        {
            ResponseData res = new ResponseData();

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


                string[] arr = HubProcessor.Instance.listOfZone.OrderBy(c => c.Area)

                   .Select(args => args.Id + "," + args.Area).ToArray<string>();


                //send message back to PDA
                ///Clients.Caller.directed(string.Join(">>", arr));----------------------------------------------------------------------
                ///
                res.Data = new JavaScriptSerializer().Serialize(string.Join(">>", arr));
                res.IsSuccess = true;
                res.Message = "";

                //Byte[] byteResponse = Encoding.UTF8.GetBytes(string.Join(">>", arr));
                //tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);
            }
            catch (Exception ex)
            {
                try
                {
                    ///Clients.Caller.directed("exceptionoccurred");------------------------------------------
                    ///
                    res.Data = null;
                    res.IsSuccess = false;
                    res.Message = "exceptionoccurred";

                    File.AppendAllText(physicalPath + "\\requestAsDirected_exception.txt", DateTime.Now + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                { }
            }

            return res;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestAcceptJobAdditional")]
        public ResponseDataX requestAcceptJobAdditional(string mesg)
        {
            ResponseDataX resp = new ResponseDataX();
            try
            {
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\requestAcceptJobAdditional.txt", DateTime.Now.ToStr() + ",mesg=" + mesg + Environment.NewLine);

                }
                catch
                {


                }

                //   log.Debug("requestAcceptJobAdditional: " + jobId);
                JobActionEx objAction = new JavaScriptSerializer().Deserialize<JobActionEx>(mesg.ToStr());


                using (TaxiDataContext db = new TaxiDataContext())
                {
                    try
                    {
                        long jobId = objAction.JobId.ToLong();

                        var data = db.Bookings.Where(c => c.Id == jobId).Select(args => new { args.FromAddress, args.ToAddress, args.CompanyId, args.FareRate, args.ExtraDropCharges, args.MeetAndGreetCharges, args.CongtionCharges, args.AgentCommission, args.CashRate, args.CashFares, args.ServiceCharges }).FirstOrDefault();


                        objAction.Pickup = data.FromAddress.ToStr().ToUpper().Trim();
                        objAction.Dropoff = data.ToAddress.ToStr().ToUpper().Trim();

                        //var coord = db.stp_getCoordinatesByAddress(objAction.Pickup.ToStr(), GetPostCodeMatch(objAction.Pickup.ToStr().ToUpper().Trim())).FirstOrDefault();

                        //if (coord != null)
                        //{
                        //    objAction.Pickup = coord.Latitude + "," + coord.Longtiude;
                        //}


                        //coord = db.stp_getCoordinatesByAddress(objAction.Dropoff.ToStr(), GetPostCodeMatch(objAction.Dropoff.ToStr().ToUpper().Trim())).FirstOrDefault();

                        //if (coord != null)
                        //{
                        //    objAction.Dropoff = coord.Latitude + "," + coord.Longtiude;
                        //}


                        //int companyId = 0;

                        //companyId = data.CompanyId.ToInt();

                        //if (objAction.Via.ToStr().Trim().Length > 0)
                        //{
                        //    objAction.Via = string.Empty;
                        //    foreach (var item in db.Booking_ViaLocations.Where(c => c.BookingId == jobId).Select(c => c.ViaLocValue).ToList())
                        //    {
                        //        coord = db.stp_getCoordinatesByAddress(item.ToStr(), GetPostCodeMatch(item.ToStr().ToUpper().Trim())).FirstOrDefault();


                        //        if (coord != null)
                        //        {
                        //            objAction.Via += coord.Latitude + "," + coord.Longtiude + "|";
                        //        }

                        //    }

                        //    try
                        //    {
                        //        if (objAction.Via.ToStr().EndsWith("|"))
                        //            objAction.Via.ToStr().TrimEnd(new char[] { '|' });
                        //    }
                        //    catch
                        //    {

                        //    }

                        //}

                        string distance = "";
                        string showDetails = "1";
                        string eta = "";

                        //if (companyId == 597)
                        //{
                        //    showDetails = "1";
                        //    string ress = GetETADistanceWithDuration(objAction.Pickup, objAction.Dropoff.ToStr(), "", objAction.Via.ToStr());

                        //    if (ress.Contains(","))
                        //    {

                        //        distance = ress.Split(',')[0].ToStr() + " Miles";
                        //        eta = ress.Split(',')[1].ToStr() + " Min(s)";
                        //    }
                        //}
                        //

                        decimal totalFares = data.FareRate.ToDecimal() + data.ExtraDropCharges.ToDecimal() + data.MeetAndGreetCharges.ToDecimal() + data.CongtionCharges.ToDecimal() + data.AgentCommission.ToDecimal() + data.CashRate.ToDecimal() + data.CashFares.ToDecimal() + data.ServiceCharges.ToDecimal();


                        int driverId = objAction.DrvId.ToInt();
                        //var driverDetails = db.Fleet_Drivers.Where(c => c.Id == driverId).Select(c => new { c.DriverTypeId, c.DriverCommissionPerBooking }).FirstOrDefault();

                        //if (driverDetails.DriverTypeId.ToInt() == 2)
                        //{
                        //    var comm = (totalFares * driverDetails.DriverCommissionPerBooking.ToDecimal()) / 100;
                        //    totalFares = totalFares - comm;

                        //}
                        //

                        resp.Data = new
                        {
                            //TotalFares = booking.TotalCharges.Value.ToString("#.##"),
                            TotalFares = totalFares,
                            DriverCommission = -1,
                            ETA = eta,
                            Miles = distance,
                            ShowDetails = showDetails
                        };
                        resp.IsSuccess = true;
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            File.AppendAllText(AppContext.BaseDirectory + "\\requestAcceptJobAdditional_exception.txt", DateTime.Now.ToStr() + ",mesg=" + mesg + ",exception:" + ex.Message + Environment.NewLine);

                        }
                        catch
                        {


                        }
                        // log.Error(ex.Message, ex);
                        resp.Data = ex.Message;
                        resp.IsSuccess = false;
                    }
                }
            }
            catch
            {

            }
            return resp;
        }

        public static string GetETADistanceWithDuration(string origin, string destination, string key, string vias = "")
        {
            string res = "";
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var obj = new
                {
                    originLat = Convert.ToDouble(origin.Split(',')[0]),
                    originLng = Convert.ToDouble(origin.Split(',')[1]),
                    destLat = Convert.ToDouble(destination.Split(',')[0]),
                    destLng = Convert.ToDouble(destination.Split(',')[1]),
                    defaultclientid = HubProcessor.Instance.objPolicy.DefaultClientId.ToStr(),
                    keys = key,
                    MapType = HubProcessor.Instance.objPolicy.MapType.ToInt(),
                    sourceType = "dispatch",
                    routeType = "shortest",
                    vias
                };


                string json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(obj);
                string API = "https://www.treasureonlineapi.co.uk/CabTreasureWebApi/Home/GetETADistanceWithDuration" + "?json=" + json;

                try
                {

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
                            res = sr.ReadToEnd().ToStr();
                        }
                    }

                }
                catch (Exception ex)
                {
                    if (ex.Message.ToLower().Contains("ssl"))
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(API.Replace("https", "http"));
                        request.ContentType = "application/json; charset=utf-8";
                        request.Accept = "application/json";
                        request.Method = WebRequestMethods.Http.Post;
                        request.Proxy = null;
                        request.ContentLength = 0;



                        using (WebResponse responsea = request.GetResponse())
                        {

                            using (StreamReader sr = new StreamReader(responsea.GetResponseStream()))
                            {
                                res = sr.ReadToEnd().ToStr();
                            }
                        }
                    }


                }

            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\GetETADistance_exception.txt", DateTime.Now.ToStr() + ",origin" + origin + ",destination:" + destination + ex.Message + Environment.NewLine);

                }
                catch
                {


                }

            }
            return res;

        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("actionbutton")] //Already exist, make it change by returning ResponseData model as return type
        public ResponseData actionButton(string mesg)
        {
            ResponseData res = new ResponseData();

            string respo = "true";
            string dataValue = mesg;
            dataValue = dataValue.Trim();

            string[] values = dataValue.Split(new char[] { '=' });

            int valCnt = values.Count();

            try
            {

                int jobStatusId = values[3].ToInt();


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

                                        //try
                                        //{
                                        //    File.AppendAllText("settledoubledrvlog.txt", "datastring:" + dataValue + " on time :" + DateTime.Now.ToStr());
                                        //}
                                        //catch
                                        //{

                                        //}
                                    }
                                    else
                                    {
                                        db.stp_UpdateJob(values[1].ToLong(), values[2].ToInt(), values[3].ToInt(), values[4].ToInt(), HubProcessor.Instance.objPolicy.SinBinTimer.ToInt());
                                    }
                                }
                                else
                                {
                                    respo = "false:This Job is no longer available";

                                    //try
                                    //{
                                    //    File.AppendAllText("settledoubledrvlogcond2.txt", "datastring:" + dataValue + " on time :" + DateTime.Now.ToStr());
                                    //}
                                    //catch
                                    //{

                                    //}
                                }
                            }
                            else
                            {
                                db.stp_UpdateJob(values[1].ToLong(), values[2].ToInt(), values[3].ToInt(), values[4].ToInt(), HubProcessor.Instance.objPolicy.SinBinTimer.ToInt());
                            }

                            //send message back to PDA
                            //  Clients.Caller.jobAccepted(respo);
                            //   respo = "true";
                            if (respo == "true")
                            {
                                DispatchJobSMS(values[1].ToLong(), jobStatusId);

                                try
                                {

                                    Global.RemoveJobFromBidList(values[1].ToLong());

                                    if (Instance.listofJobs.Count(c => c.DriverId == driverId && c.JobId != values[1].ToLong() && c.MessageTypeId == eMessageTypes.JOB) > 0)
                                    {
                                        Instance.listofJobs.RemoveAll(c => c.DriverId == driverId && c.MessageTypeId == eMessageTypes.JOB && c.JobId != values[1].ToLong());

                                        try
                                        {

                                            File.AppendAllText(physicalPath + "\\actionbutton_otherjobinqueue.txt", DateTime.Now.ToStr() + " request" + dataValue + Environment.NewLine);
                                        }
                                        catch
                                        {
                                            //
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    try
                                    {

                                        File.AppendAllText(physicalPath + "\\actionbutton_otherjobinqueueexception.txt", DateTime.Now.ToStr() + " request" + dataValue + ",exception:" + ex.Message + Environment.NewLine);
                                    }
                                    catch
                                    {

                                    }


                                }
                            }
                        }
                    }


                    if (jobStatusId == Enums.BOOKINGSTATUS.DISPATCHED)
                    {

                        using (TaxiDataContext db = new TaxiDataContext())
                            db.stp_UpdateJobAndRoute(values[1].ToLong(), values[2].ToInt(), values[3].ToInt(), values[4].ToInt(), "", values[5].ToDecimal(), null);
                    }
                    else
                    {
                        bool isRestricted = false;
                        try
                        {
                            if (jobStatusId == Enums.BOOKINGSTATUS.NOPICKUP)
                            {
                                //send message back to PDA
                                // Clients.Caller.noPickup(respo);

                                DateTime? arrivalDateTime = null;
                                using (TaxiDataContext db = new TaxiDataContext())
                                {
                                    arrivalDateTime = db.Bookings.Where(c => c.Id == values[1].ToLong()).Select(c => c.ArrivalDateTime).FirstOrDefault();
                                }

                                if (arrivalDateTime != null)
                                {
                                    double timeString = DateTime.Now.Subtract(arrivalDateTime.Value).TotalMinutes;


                                    int restrictionMins = Global.NoPickupRestrictionMins.ToInt();
                                    //   int restrictionMins = 5;

                                    if (timeString < restrictionMins)
                                    {
                                        timeString = restrictionMins - timeString;
                                        ///Clients.Caller.noPickupAuth("false:You can press No Pickup after " + timeString.ToInt() + "min");-------------------------------------------
                                        ///
                                        respo = "false:You can press No Pickup after " + timeString.ToInt() + "min";
                                        isRestricted = true;
                                    }
                                    else
                                    {
                                        isRestricted = false;
                                        respo = "true";
                                    }
                                }


                            }
                        }
                        catch
                        {
                        }
                        if (!isRestricted && jobStatusId != Enums.BOOKINGSTATUS.ONROUTE && jobStatusId != Enums.BOOKINGSTATUS.ARRIVED && jobStatusId != Enums.BOOKINGSTATUS.STC)
                        {
                            using (TaxiDataContext db = new TaxiDataContext())
                            {
                                db.stp_UpdateJob(values[1].ToLong(), values[2].ToInt(), values[3].ToInt(), values[4].ToInt(), HubProcessor.Instance.objPolicy.SinBinTimer.ToInt());
                            }
                        }

                        if (jobStatusId == Enums.BOOKINGSTATUS.ARRIVED)
                        {


                            try
                            {


                                if (dataValue.ToStr().Contains("=jsonstring|") && values[7].ToStr().Contains("jsonstring|"))
                                {

                                    if (HubProcessor.Instance.objPolicy.RestrictMilesPOBAction.ToDecimal() > 0)

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

                                        if (coord != null && distance > HubProcessor.Instance.objPolicy.RestrictMilesPOBAction.ToDecimal())
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

                                                //try
                                                //{

                                                //    File.AppendAllText(physicalPath + "\\restrictionlog.txt", DateTime.Now + ": datavalue=" + dataValue + ",(arrivejob)distance to : " + pickup + ",distance:" + distance + Environment.NewLine);
                                                //}
                                                //catch
                                                //{


                                                //}
                                            }
                                            else
                                            {


                                            }

                                        }

                                    }
                                }


                                respo = "true";
                                //  Clients.Caller.jobArrived(respo);

                                if (respo == "true")
                                {
                                    using (TaxiDataContext db = new TaxiDataContext())
                                    {
                                        db.stp_UpdateJob(values[1].ToLong(), values[2].ToInt(), values[3].ToInt(), values[4].ToInt(), HubProcessor.Instance.objPolicy.SinBinTimer.ToInt());
                                    }
                                    //  DispatchJobSMS(values[1].ToLong(), jobStatusId);

                                }
                                // if(respo.StartsWith("failed")==false)

                            }
                            catch (Exception ex)
                            {
                                // Clients.Caller.exceptionOccured(ex.Message);
                                // Clients.Caller.jobArrived("exceptionoccurred");
                                //try
                                //{

                                //    File.AppendAllText(physicalPath + "\\arrived_exception.txt", DateTime.Now.ToStr() + " request" + dataValue + ",exception:" + ex.Message + Environment.NewLine);
                                //}
                                //catch
                                //{

                                //}
                            }
                        }
                        else if (jobStatusId == Enums.BOOKINGSTATUS.STC)
                        {



                            if (dataValue.ToStr().Contains("=jsonstring|") && values[7].ToStr().Contains("jsonstring|"))
                            {

                                if (HubProcessor.Instance.objPolicy.RestrictMilesOnSTC.ToDecimal() > 0)

                                {

                                    string json = values[7].ToStr().Replace("jsonstring|", "").Trim();
                                    JobAction objAction = new JavaScriptSerializer().Deserialize<JobAction>(json);




                                    if (HubProcessor.Instance.objPolicy.RestrictMilesOnSTC.ToDecimal() > 0 && objAction.Dropoff.ToStr().Trim().Length > 0 && objAction.DrvNo.ToStr().Trim().Length > 0)
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

                                                var objBooker = db.Bookings.Select(args => new
                                                {
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
                                                    isfound = FindPoint(Convert.ToDouble(objAction.Latitude), Convert.ToDouble(objAction.Longitude), db.Gen_Zone_PolyVertices.Where(c => c.ZoneId == dropOffZoneId).ToList());

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




                                            if (distance == -1 || (coord != null && distance > HubProcessor.Instance.objPolicy.RestrictMilesOnSTC.ToDecimal()))
                                            {
                                                if (RemoveRestriction == 1)
                                                {

                                                    //try
                                                    //{

                                                    //    File.AppendAllText(physicalPath + "//restrictionRemoved.txt", DateTime.Now + ": datavalue=" + dataValue + ",(stcjob)distance to : " + pickup + ",distance:" + distance + Environment.NewLine);
                                                    //}
                                                    //catch
                                                    //{


                                                    //}


                                                }
                                                else
                                                {
                                                    respo = "false:You are far away from Destination";

                                                    //try
                                                    //{

                                                    //    File.AppendAllText(physicalPath + "//restrictionlog.txt", DateTime.Now + ": datavalue=" + dataValue + ",(stcjob)distance to : " + pickup + ",distance:" + distance + Environment.NewLine);
                                                    //}
                                                    //catch
                                                    //{


                                                    //}
                                                }

                                            }






                                            //

                                        }
                                    }




                                }

                            }



                            //  Clients.Caller.jobStc(respo);
                            respo = "true";

                            if (respo == "true")
                            {
                                using (TaxiDataContext db = new TaxiDataContext())
                                {
                                    db.stp_UpdateJob(values[1].ToLong(), values[2].ToInt(), values[3].ToInt(), values[4].ToInt(), HubProcessor.Instance.objPolicy.SinBinTimer.ToInt());
                                }
                            }

                            //   }
                        }
                    }
                }
                else if (valCnt == 6)
                {
                    if (dataValue.Contains("ACK"))
                    {
                        //send message back to PDA

                        respo = "true";

                        using (TaxiDataContext db = new TaxiDataContext())
                            db.stp_UpdateJobAndRoute(values[1].ToLong(), values[2].ToInt(), values[3].ToInt(), values[4].ToInt(), "", 0, null);
                    }
                    else
                    {
                        //try
                        //{

                        //    File.AppendAllText(physicalPath + "\\called2.txt", DateTime.Now.ToStr() + " request" + dataValue + Environment.NewLine);
                        //}
                        //catch
                        //{

                        //}
                        using (TaxiDataContext db = new TaxiDataContext())
                            db.stp_UpdateJobAndRoute(values[1].ToLong(), values[2].ToInt(), values[3].ToInt(), values[4].ToInt(), "", values[5].ToDecimal(), null);
                    }
                }
                else
                {
                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        try
                        {
                            if (values[3].ToInt() == Enums.BOOKINGSTATUS.NOTACCEPTED && HubProcessor.Instance.objPolicy.EnableFOJ.ToBool())
                            {
                                db.stp_UpdateJobStatus(values[1].ToLong(), values[3].ToInt());
                            }
                            else
                            {
                                db.stp_UpdateJob(values[1].ToLong(), values[2].ToInt(), values[3].ToInt(), values[4].ToInt(), HubProcessor.Instance.objPolicy.SinBinTimer.ToInt());
                            }

                            //send message back to PDA

                            respo = "true";
                        }
                        catch
                        {


                        }
                    }
                }



                try
                {
                    if (respo == "true")
                    {
                        General.BroadCastMessage("**action>>" + values[1].ToStr() + ">>" + values[2].ToStr() + ">>" + values[3].ToInt());
                    }

                    if (HubProcessor.Instance.listofJobs.Count(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong()) > 0)
                    {
                        HubProcessor.Instance.listofJobs.RemoveAll(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong());
                    }

                    HubProcessor.Instance.listofJobs.RemoveAll(c => c.DriverId == values[2].ToInt() && c.JobId != 0 && (c.MessageTypeId == 9 || c.MessageTypeId == 1));
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
                    if (jobStatusId.ToInt() == Enums.BOOKINGSTATUS.NOPICKUP)
                    {
                        try
                        {
                            var objDriver = new List<int>();
                            var EnableSentPDAMsgOnNoPickupToOther = "0";
                            using (TaxiDataContext db = new TaxiDataContext())
                            {
                                objDriver = db.Fleet_Drivers.Where(c => c.Id != values[2].ToInt()).Select(c => c.Id).ToList();
                                EnableSentPDAMsgOnNoPickupToOther = db.ExecuteQuery<string>("Select SetVal from AppSettings where SetKey = 'EnableSentPDAMsgOnNoPickupToOther'").FirstOrDefault();
                            }

                            if (EnableSentPDAMsgOnNoPickupToOther == "1" && objDriver != null && objDriver.Count > 0)
                            {
                                foreach (int itemId in objDriver)
                                {
                                    General.requestPDA("request pda=" + itemId + "=" + 0 + "=" + "Message>>Driver Priority - No Show>>" + String.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Now) + "=4");
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
                    //   File.AppendAllText(physicalPath + "\\actionbutton_exception.txt", DateTime.Now + ": datavalue=" + dataValue + ",exception" + ex.Message);
                    HubProcessor.Instance.listofJobs.RemoveAll(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong());
                    // Clients.Caller.exceptionOccured(ex.Message);
                }
                catch (Exception e)
                {
                    //  Clients.Caller.exceptionOccured(e.Message);
                }
            }

            ///return respo;-------------------------------------------------
            ///
            res.Data = respo;
            res.IsSuccess = true;
            res.Message = "";

            return res;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestarrive")]
        public ResponseData requestarrive(string mesg)
        {
            ResponseData res = new ResponseData();

            string respo = "true";
            JobActionEx objAction = null;
            try
            {
                try
                {
                    //
                    File.AppendAllText(AppContext.BaseDirectory + "\\requestarrive.txt", DateTime.Now.ToStr() + " request" + mesg + Environment.NewLine);
                }
                catch
                {

                }


                string dataValue = mesg;
                dataValue = dataValue.Trim();
                objAction = new JavaScriptSerializer().Deserialize<JobActionEx>(mesg);
                RemoveUK(ref objAction.Dropoff);

                long jobId = objAction.JobId.ToLong();
                using (TaxiDataContext db = new TaxiDataContext())
                {

                    try
                    {


                        if (true)
                        {
                            //
                            if (HubProcessor.Instance.objPolicy.RestrictMilesPOBAction.ToDecimal() > 0)

                            {


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

                                        var oobj = db.stp_getCoordinatesByAddress(pickup, pickup).FirstOrDefault();


                                        if (oobj != null && oobj.Latitude != null)
                                        {
                                            coord = new Gen_Coordinate();
                                            coord.Latitude = oobj.Latitude;
                                            coord.Longitude = oobj.Longtiude;
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

                                if (coord != null && distance > HubProcessor.Instance.objPolicy.RestrictMilesPOBAction.ToDecimal())
                                {

                                    int RemoveRestriction = 0;
                                    try
                                    {




                                        int? objBooker = db.Bookings.Where(c => c.Id == jobId)
                                        .Select(args => args.OnHoldWaitingMins).FirstOrDefault();

                                        if (objBooker != null)
                                        {
                                            //   
                                            RemoveRestriction = objBooker.ToInt();
                                        }

                                    }
                                    catch (Exception ex)
                                    {


                                    }


                                    if (RemoveRestriction == 0)
                                    {
                                        objAction.Message = "You are far away from Pickup";

                                        respo = "false";
                                    }
                                    else
                                    {


                                    }

                                }

                            }
                        }



                        //  Clients.Caller.jobArrived(respo);

                        if (respo == "true")
                        {
                            //  using (TaxiDataContext db = new TaxiDataContext())

                            db.stp_UpdateJob(jobId, objAction.DrvId.ToInt(), 6, 6, HubProcessor.Instance.objPolicy.SinBinTimer.ToInt());

                            if (HubProcessor.Instance.objPolicy.EnableArrivalBookingText.ToBool())
                            {
                                try
                                {
                                    Booking job = General.GetObject<Booking>(c => c.Id == jobId);

                                    if (job != null && job.JobCode.ToStr().Trim().Length == 0)
                                    {
                                        // onlineBookingId = job.OnlineBookingId.ToLong();
                                        if (!string.IsNullOrEmpty(job.CustomerMobileNo))
                                        {
                                            // ADDED ON 20/APRIL/2016 ON REQUEST OF COMMERCIAL CARS => DISABLE ARRIVAL TEXT FOR PARTICULAR ACCOUNT JOBS
                                            if (job.CompanyId == null || job.Gen_Company.DisableArrivalText.ToBool() == false)
                                            {

                                                string arrivalText = string.Empty;

                                                if (job.FromLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT)
                                                {
                                                    arrivalText = HubProcessor.Instance.objPolicy.ArrivalAirportBookingText.ToStr().Trim();
                                                }
                                                else
                                                {
                                                    arrivalText = HubProcessor.Instance.objPolicy.ArrivalBookingText.ToStr().Trim();
                                                }

                                                if (!string.IsNullOrEmpty(arrivalText))
                                                {

                                                    new System.Threading.Thread(delegate ()
                                                    {
                                                        try
                                                        {
                                                            AddSMS(job.CustomerMobileNo.ToStr().Trim(), GetMessage(arrivalText, job, jobId), job.SMSType.ToInt());
                                                        }
                                                        catch
                                                        {

                                                        }
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
                                                    arrivalText = HubProcessor.Instance.objPolicy.ArrivalAirportBookingText.ToStr().Trim();
                                                }
                                                else
                                                {
                                                    arrivalText = HubProcessor.Instance.objPolicy.ArrivalBookingText.ToStr().Trim();
                                                }

                                                if (!string.IsNullOrEmpty(arrivalText))
                                                {

                                                    new System.Threading.Thread(delegate ()
                                                    {
                                                        try
                                                        {
                                                            AddSMS(job.CustomerPhoneNo.ToStr().Trim(), GetMessage(arrivalText, job, jobId), job.SMSType.ToInt());
                                                        }
                                                        catch { }
                                                    }).Start();
                                                }
                                            }



                                            // RingBackCall("Arrived Call to Customer", job.CustomerName.ToStr(), job.CustomerPhoneNo.ToStr().Trim());

                                        }
                                    }

                                }
                                catch
                                {

                                }


                            }
                            int? vehicleTypeId = 0;


                            Booking objBook = db.ExecuteQuery<Booking>("select Id,vehicletypeid,pickupdatetime,FromLocTypeId,FromAddress from booking where id=" + jobId).FirstOrDefault();

                            string query = string.Empty;
                            FareEx jobTariff = null;
                            try
                            {

                                vehicleTypeId = objBook.VehicleTypeId.ToInt();

                                int? fareId = null;

                                try
                                {
                                    fareId = db.ExecuteQuery<int?>("exec stp_GetFareId {0},{1},{2},{3},{4}", vehicleTypeId, 0, 0.00m, objBook.PickupDateTime, 1).FirstOrDefault();

                                    if (fareId.ToInt() > 0)
                                        query = "select StartRateValidMiles,FromDateTime,TillDateTime,FromSpecialDate,TillSpecialDate,DayValue,IsDayWise,WaitingCharges,WaitingChargesPerSeconds,WaitingSecondsFree from fare  where id=" + fareId;


                                    if (query.ToStr().Trim().Length > 0)
                                        jobTariff = db.ExecuteQuery<FareEx>(query).FirstOrDefault();

                                }
                                catch
                                {

                                }






                                objAction.objMeterTariff = new MeterTarrif();
                                if (jobTariff != null)
                                {
                                    objAction.objMeterTariff.FreeWaitingMins = jobTariff.WaitingSecondsFree.ToInt();
                                    objAction.objMeterTariff.WaitingSecondsToDivide = jobTariff.WaitingChargesPerSeconds.ToInt();
                                    objAction.objMeterTariff.DrvWaitingChargesPerMin = jobTariff.WaitingCharges.ToDecimal();


                                }



                                if (objAction.objMeterTariff.DrvWaitingChargesPerMin.ToDecimal() == 0)
                                {
                                    try
                                    {
                                        if (Global.listofMeter == null)
                                            Global.ReloadMeterList();

                                        var obj = Global.listofMeter.FirstOrDefault(c => c.VehicleTypeId == vehicleTypeId).DefaultIfEmpty();

                                        if (obj != null)
                                        {


                                            objAction.objMeterTariff.FreeWaitingMins = 0;
                                            objAction.objMeterTariff.WaitingSecondsToDivide = obj.AccWaitingChargesPerMin.ToInt();
                                            objAction.objMeterTariff.DrvWaitingChargesPerMin = obj.DrvWaitingChargesPerMin.ToDecimal();

                                        }
                                    }
                                    catch
                                    {

                                    }

                                }

                                AirportLocationData objAirportWaitingCharges = null;
                                if (objBook.FromLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT)
                                {
                                    string query2 = string.Empty;
                                    try
                                    {

                                        string locName = objBook.FromAddress.ToStr().Trim().Replace("  ", " ").Trim().Replace(",", "").Trim();
                                        query2 = "select l.LocationName, l.PostCode, e.AllowanceMins, e.WaitingSecondsFree, e.WaitingCharges, e.WaitingChargesPerSeconds from Gen_Syspolicy_LocationExpiry e inner join Gen_Locations l on e.LocationId = l.Id where replace( (replace(l.FullLocationName,'  ',' ')),',','')= '" + locName + "' and WaitingCharges is not null";


                                        objAirportWaitingCharges = db.ExecuteQuery<AirportLocationData>(query2).FirstOrDefault();

                                    }
                                    catch (Exception ex)
                                    {
                                        try
                                        {
                                            objAction.Message = ex.Message;
                                            File.AppendAllText(AppContext.BaseDirectory + "\\requestarrive_airportexception.txt", DateTime.Now.ToStr() + " request" + mesg + ",query=" + query2 + "exception:" + ex.Message + Environment.NewLine);
                                        }
                                        catch
                                        {

                                        }
                                    }

                                    if (objAirportWaitingCharges != null && objAirportWaitingCharges.WaitingCharges.ToDecimal() > 0)
                                    {
                                        try
                                        {



                                            objAction.objMeterTariff.FreeWaitingMins = objAirportWaitingCharges.WaitingSecondsFree.ToInt();
                                            objAction.objMeterTariff.WaitingSecondsToDivide = objAirportWaitingCharges.WaitingChargesPerSeconds.ToInt();
                                            objAction.objMeterTariff.DrvWaitingChargesPerMin = objAirportWaitingCharges.WaitingCharges.ToDecimal();


                                            try
                                            {

                                                File.AppendAllText(AppContext.BaseDirectory + "\\requestarrive_airportwaitingapplied.txt", DateTime.Now.ToStr() + " request" + mesg + ",query=" + query + Environment.NewLine);
                                            }
                                            catch
                                            {

                                            }



                                        }
                                        catch
                                        {

                                        }


                                    }

                                }

                                if (respo == "true")
                                {
                                    General.BroadCastMessage("**action>>" + objAction.JobId.ToStr() + ">>" + objAction.DrvId.ToStr() + ">>6");
                                }

                                respo = new JavaScriptSerializer().Serialize(objAction);


                            }
                            catch (Exception ex2)
                            {
                                try
                                {
                                    objAction.Message = ex2.Message;
                                    File.AppendAllText(AppContext.BaseDirectory + "\\requestarrive_innerexception.txt", DateTime.Now.ToStr() + " request" + mesg + ",query=" + query + "exception:" + ex2.Message + Environment.NewLine);
                                }
                                catch
                                {

                                }
                            }
                            //






                            //









                        }
                        else
                        {
                            //      respo = objAction.Message;

                        }
                        // if(respo.StartsWith("failed")==false)

                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            objAction.Message = ex.Message;
                            File.AppendAllText(AppContext.BaseDirectory + "\\requestarrive_exception1.txt", DateTime.Now.ToStr() + " request" + mesg + ",exception:" + ex.Message + Environment.NewLine);
                        }
                        catch
                        {

                        }
                    }

                }

                if (respo == "false")
                {

                    res.IsSuccess = false;
                    res.Message = objAction.Message;

                }
                else
                    res.IsSuccess = true;


            }

            catch (Exception ex)
            {
                res.IsSuccess = false;
                res.Message = ex.Message;
                try
                {
                    objAction.Message = ex.Message;
                    File.AppendAllText(AppContext.BaseDirectory + "\\requestarrive_exception2.txt", DateTime.Now.ToStr() + " request" + mesg + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
            }


            //if (respo.ToStr().ToLower().StartsWith("false") == false)
            //    respo = "true";


            res.Data = respo;

            return res;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestPOB")]
        public ResponseData requestPOB(string mesg)
        {
            ResponseData res = new ResponseData();

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

                    if (HubProcessor.Instance.objPolicy.FareMeterType == null)
                    {
                        respo += ",0,0,1";
                    }
                    else
                    {
                        //bool enableFareMeter = General.GetObject<Taxi_Model.Booking>(c => c.Id == values[1].ToLong()).DefaultIfEmpty().EnableFareMeter.ToBool();
                        //int vehicleTypeId = General.GetObject<Taxi_Model.Booking>(c => c.Id == values[1].ToLong()).DefaultIfEmpty().VehicleTypeId.ToInt();

                        int? vehicleTypeId = HubProcessor.Instance.objPolicy.DefaultVehicleTypeId.ToInt();

                        string isMeter = "0";



                        string fareJson = string.Empty;

                        using (TaxiDataContext db = new TaxiDataContext())
                        {
                            stp_GetBookingDetailsExResult objDetails = null;
                            long jobId = values[1].ToLong();
                            //objDetails = db.stp_GetBookingDetails(values[1].ToLong()).FirstOrDefault();
                            objDetails = db.ExecuteQuery<stp_GetBookingDetailsExResult>("exec stp_GetBookingDetailsEx {0}", jobId).FirstOrDefault();
                            if (objDetails != null)
                            {


                                vehicleTypeId = objDetails.VehicleTypeId.ToIntorNull();


                                InitializeMeterList();

                                if (Global.listofMeter != null && Global.listofMeter.Count > 0)
                                //if (1==1)
                                {

                                    //bool enableFareMeter = Global.listofMeter.FirstOrDefault(c => c.VehicleTypeId == vehicleTypeId).DefaultIfEmpty().HasMeter.ToBool();
                                    bool enableFareMeter = true;

                                    if (enableFareMeter)
                                    {
                                        //isMeter = "1";

                                        isMeter = Global.listofMeter != null && Global.listofMeter.Count > 0 && Global.listofMeter.FirstOrDefault(c => c.VehicleTypeId == vehicleTypeId).DefaultIfEmpty().HasMeter.ToBool() ? "1" : "0";

                                        isMeter = objDetails.DisableDriverSMS.ToBool() == true ? "0" : "1";

                                        var obj = Global.listofMeter.FirstOrDefault(c => c.VehicleTypeId == vehicleTypeId).DefaultIfEmpty();

                                        FareMeterSettings fareJsonArr = null;
                                        //  FareMeterSettings fareJsonArr = new JavaScriptSerializer().Deserialize<FareMeterSettings>(Instance.objPolicy.PDANewWeekMessageByDay.ToStr());

                                        if (HubProcessor.Instance.objPolicy.PDANewWeekMessageByDay.ToStr().StartsWith("{"))
                                            fareJsonArr = new JavaScriptSerializer().Deserialize<FareMeterSettings>(HubProcessor.Instance.objPolicy.PDANewWeekMessageByDay.ToStr());
                                        else
                                            fareJsonArr = new FareMeterSettings(true);

                                        int? fareId = db.ExecuteQuery<int?>("exec stp_GetFareId {0},{1},{2},{3},{4},{5}", vehicleTypeId, 0, 0.00m, objDetails.PickupDateTime, 1, objDetails.ZoneId.ToInt().ToStr()).FirstOrDefault();

                                        var jobTariff = (from f in db.Fares
                                                         join c in db.Fare_OtherCharges on f.Id equals c.FareId
                                                         where f.Id == fareId
                                                         select new
                                                         {
                                                             f.StartRate,
                                                             f.StartRateValidMiles,
                                                             f.FromDateTime,
                                                             f.TillDateTime,
                                                             f.FromSpecialDate,
                                                             f.TillSpecialDate,
                                                             f.DayValue,
                                                             f.IsDayWise,
                                                             c.FromMile,
                                                             c.ToMile,
                                                             c.Rate

                                                         }).ToList();


                                        fareJsonArr.meterTarrif = new List<MeterTarrif>();
                                        fareJsonArr.EnableDropOffAction = "3";
                                        //    Global.listMeterTariff = new List<MeterTarrif>();
                                        decimal roundJourneyMile = HubProcessor.Instance.objPolicy.RoundJourneyMiles.ToDecimal();
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
                                                FullRoundFares = HubProcessor.Instance.objPolicy.RoundMileageFares.ToBool() ? 1 : 0,
                                                RoundUpTo = HubProcessor.Instance.objPolicy.RoundUpTo.ToDecimal(),
                                                WaitingSecondsToDivide = obj.AccWaitingChargesPerMin.ToInt(),
                                                RoundJourneyMiles = roundJourneyMile,
                                                FreeWaitingMins = obj.FreeWaitingSeconds.ToInt()
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
                                                FullRoundFares = HubProcessor.Instance.objPolicy.RoundMileageFares.ToBool() ? 1 : 0,
                                                RoundUpTo = HubProcessor.Instance.objPolicy.RoundUpTo.ToDecimal(),
                                                WaitingSecondsToDivide = obj.AccWaitingChargesPerMin.ToInt(),
                                                RoundJourneyMiles = roundJourneyMile,
                                                FreeWaitingMins = obj.FreeWaitingSeconds.ToInt()
                                            });
                                        }

                                        //if (Global.EnableViaAction == "1")
                                        //{
                                        fareJsonArr.EnableViaAction = Global.EnableViaAction;
                                        //}
                                        try
                                        {
                                            fareJsonArr.EnablePauseMeter = Global.EnablePauseMeter;
                                        }
                                        catch
                                        {
                                        }
                                        fareJson = ",jsonstring|" + new JavaScriptSerializer().Serialize(fareJsonArr).Replace(",", "|");
                                        //
                                    }
                                    else
                                    {
                                        try
                                        {



                                            if (Global.EnableViaAction == "1")
                                            {

                                                FareMeterSettings fareJsonArr = new FareMeterSettings(true);
                                                fareJsonArr.EnableViaAction = "2";

                                                //if ((values[7].ToDecimal() >= 102.63m && values[7].ToDecimal() < 120))
                                                //    fareJsonArr.EnableViaAction = "3";

                                                fareJson = ",jsonstring|" + new JavaScriptSerializer().Serialize(fareJsonArr).Replace(",", "|");


                                            }

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
                                        if (Global.EnableViaAction.ToStr() == "1")
                                        {

                                            FareMeterSettings fareJsonArr = new FareMeterSettings(true);
                                            fareJsonArr.EnableViaAction = "2";
                                            //if ((values[7].ToDecimal() >= 102.63m && values[7].ToDecimal() < 120))
                                            //    fareJsonArr.EnableViaAction = "3";

                                            fareJson = ",jsonstring|" + new JavaScriptSerializer().Serialize(fareJsonArr).Replace(",", "|");


                                        }

                                    }
                                    catch
                                    {

                                    }
                                }

                            }
                        }

                        respo += "," + isMeter;

                        int meterType = 3;

                        respo += "," + HubProcessor.Instance.objPolicy.FareMeterRoundedCalc.ToDecimal() + "," + meterType;

                        if (fareJson.ToStr().Length > 0)
                            respo += fareJson;
                    }

                    //send message back to PDA
                    ///Clients.Caller.jobPob(respo);-------------------------------------------------------
                    ///
                    res.Data = respo;
                    res.IsSuccess = true;
                    res.Message = "";

                    //try
                    //{

                    //    File.AppendAllText(physicalPath + "\\pobresponse.txt", DateTime.Now.ToStr() + " rEsponSe" + respo + Environment.NewLine);
                    //}
                    //catch
                    //{

                    //}

                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        db.stp_UpdateJob(values[1].ToLong(), values[2].ToInt(), values[3].ToInt(), values[4].ToInt(), HubProcessor.Instance.objPolicy.SinBinTimer.ToInt());
                    }
                    try
                    {
                        General.BroadCastMessage("**action>>" + values[1].ToStr() + ">>" + values[2].ToStr() + ">>" + values[3].ToInt());

                        if (HubProcessor.Instance.listofJobs.Count(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong()) > 0)
                        {
                            HubProcessor.Instance.listofJobs.RemoveAll(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong());
                        }


                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            File.AppendAllText("exception_pobcollection.txt", DateTime.Now + ": datavalue=" + dataValue + ",exception" + ex.Message + Environment.NewLine);

                            if (HubProcessor.Instance.listofJobs.Count(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong()) > 0)
                            {
                                HubProcessor.Instance.listofJobs.RemoveAll(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong());
                            }
                        }
                        catch
                        {

                        }
                    }


                    try
                    {
                        Global.AddSTCReminder(values[1].ToLong(), values[2].ToInt());

                        General.UpdatePoolJob(0, 0, values[1].ToLong(), values[2].ToInt(), Enums.BOOKINGSTATUS.POB.ToInt(), "pob");
                        CallSupplierApi.UpdateStatus(values[1].ToLong(), 7);

                        GC.Collect();
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
                    try
                    {

                        File.AppendAllText(physicalPath + "\\pobexception.txt", DateTime.Now.ToStr() + " request" + dataValue + ",exception:" + ex.Message + Environment.NewLine);
                    }
                    catch
                    {

                    }

                    ///Clients.Caller.jobPob("exceptionoccurred");------------------------------------------------------------------
                    ///
                    res.Data = null;
                    res.IsSuccess = false;
                    res.Message = "exceptionoccurred";
                }
                catch
                {
                    ///Clients.Caller.jobPob("exceptionoccurred");---------------------------------------------------------
                    ///
                    res.Data = null;
                    res.IsSuccess = false;
                    res.Message = "exceptionoccurred";
                }
            }

            return res;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestClearJob")]
        public ResponseData requestClearJob(string mesg)
        {
            ResponseData res = new ResponseData();

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
                        string dropOff = General.GetLocationName(objAction.Latitude, objAction.Longitude);


                        objAction.Dropoff = dropOff;
                    }
                    else
                        objAction.Dropoff = string.Empty;

                    if (objAction.DropOffFareList != null && objAction.DropOffFareList.Count > 0)
                    {
                        foreach (var item in objAction.DropOffFareList)
                        {
                            if (item.fieldname.ToLower() == "fares")
                            {
                                objAction.Fares = item.value;
                            }
                            else if (item.fieldname.ToLower() == "parking")
                            {
                                objAction.ParkingCharges = item.value;
                            }
                            else if (item.fieldname.ToLower() == "waiting")
                            {
                                objAction.WaitingCharges = item.value;
                            }
                            else if (item.fieldname.ToLower() == "extradropcharges")
                            {
                                objAction.ExtraDropCharges = item.value;
                            }
                            else if (item.fieldname.ToLower() == "tip")
                            {
                                objAction.Tip = item.value;
                            }
                            else if (item.fieldname.ToLower() == "bookingfee")
                            {
                                objAction.BookingFee = item.value;
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(objAction.cardPaymentExtras))
                    {
                        var cardpaymentlist = new JavaScriptSerializer().Deserialize<List<BookingSummary>>(objAction.cardPaymentExtras.ToStr().Trim());
                        foreach (var item in cardpaymentlist)
                        {
                            if (item.fieldname.ToLower() == "fares")
                            {
                                objAction.Fares = item.value;
                            }
                            else if (item.fieldname.ToLower() == "parking")
                            {
                                objAction.ParkingCharges = item.value;
                            }
                            else if (item.fieldname.ToLower() == "waiting")
                            {
                                objAction.WaitingCharges = item.value;
                            }
                            else if (item.fieldname.ToLower() == "extradropcharges")
                            {
                                objAction.ExtraDropCharges = item.value;
                            }
                            else if (item.fieldname.ToLower() == "tip")
                            {
                                objAction.Tip = item.value;
                            }
                            else if (item.fieldname.ToLower() == "bookingfee")
                            {
                                objAction.BookingFee = item.value;
                            }
                        }
                    }


                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        if (objAction.IsMeter.ToStr().Trim() == "1" || (objAction.DropOffFareList != null && objAction.DropOffFareList.Count > 0))
                        {
                            int waitingTime = 0;

                            if (objAction.WaitingTime.ToStr().IsNumeric())
                                waitingTime = objAction.WaitingTime.ToInt();



                            db.ExecuteQuery<int?>("exec stp_UpdateAndClearJobFaresDetails {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}"
                                                                     , objAction.JobId.ToStr().ToLong(), objAction.DrvId.ToInt(), objAction.JStatus.ToInt(), objAction.DStatus.ToInt()
                                 , objAction.Dropoff.ToStr(), objAction.Miles, objAction.Fares.ToDecimal(), waitingTime, objAction.WaitingCharges, objAction.ParkingCharges.ToDecimal()
                                 , objAction.ExtraDropCharges.ToDecimal(), objAction.BookingFee.ToDecimal(), objAction.ExtrasDetail.ToStr());


                        }
                        else
                        {
                            db.stp_UpdateJobAndRoute(objAction.JobId.ToStr().ToLong(), objAction.DrvId.ToInt(), objAction.JStatus.ToInt(), objAction.DStatus.ToInt(), objAction.Dropoff.ToStr(), objAction.Miles, null);



                        }


                        string transId = objAction.TransId.ToStr().Trim();

                        var _bookingData = db.Bookings.FirstOrDefault(a => a.Id == objAction.JobId.ToLong());

                        if (transId.Length > 0)
                        {
                            decimal tipAmount = 0.00m;
                            if (objAction.DropOffFareList != null && objAction.DropOffFareList.Count > 0)
                            {
                                tipAmount = objAction.Tip.ToDecimal();
                                db.stp_BookingLog(objAction.JobId.ToLong(), "System", " Tip " + tipAmount + " Paid By Customer");
                            }
                            else
                            {
                                if (objAction.cardPaymentExtras.ToStr().Trim().Length > 0)
                                {

                                    try
                                    {

                                        try
                                        {
                                            File.AppendAllText(physicalPath + "\\requestClearJob_tipAmount.txt", DateTime.Now + " : msg" + mesg + Environment.NewLine);
                                        }
                                        catch
                                        {

                                        }

                                        var cardpaymentlist = new JavaScriptSerializer().Deserialize<List<BookingSummary>>(objAction.cardPaymentExtras.ToStr().Trim());


                                        tipAmount = cardpaymentlist.FirstOrDefault(c => c.label == "Tip").value;
                                        db.stp_BookingLog(objAction.JobId.ToLong(), "System", " Tip " + tipAmount + " Paid By Customer");



                                    }
                                    catch (Exception ex)
                                    {
                                        try
                                        {
                                            File.AppendAllText(physicalPath + "\\requestClearJob_tipAmount_exception.txt", DateTime.Now + " : msg" + mesg + ",exception:" + ex.Message + Environment.NewLine);
                                        }
                                        catch
                                        {

                                        }

                                    }
                                }
                            }


                            try
                            {



                                db.stp_MakePayment("XXX", "XXX", null,
                                      null, "123"
                                , "xxx", "xxx", "xxx", "xxx"
                                , transId, objAction.PaymentGatewayID,
                                 objAction.Fares.ToDecimal(), objAction.ParkingCharges.ToDecimal(), objAction.WaitingCharges.ToDecimal(),
                                   HubProcessor.Instance.objPolicy.CreditCardExtraCharges.ToDecimal(), 0.00m, 0.00m,
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
                        else if (string.IsNullOrEmpty(transId) && _bookingData != null && _bookingData.PaymentTypeId == 2)
                        {
                            db.ExecuteQuery<int>("update booking set paymenttypeid=1 where id=" + objAction.JobId.ToLong());
                            try
                            {
                                File.AppendAllText(physicalPath + "\\requestClearJob.txt", DateTime.Now + " : msg" + mesg + " : change type to cash " + Environment.NewLine);
                            }
                            catch
                            {

                            }

                        }

                    }

                    rrr = "true";

                    General.BroadCastMessage("**action>>" + objAction.JobId.ToStr() + ">>" + objAction.DrvId.ToStr() + ">>" + objAction.JStatus.ToInt());


                    try
                    {

                        if (HubProcessor.Instance.listofJobs.Count(c => c.DriverId == objAction.DrvId.ToInt() && c.JobId == objAction.JobId.ToLong()) > 0)
                        {
                            HubProcessor.Instance.listofJobs.RemoveAll(c => c.DriverId == objAction.DrvId.ToInt() && c.JobId == objAction.JobId.ToLong());
                        }
                    }
                    catch
                    {


                    }

                    if (respAccount.ToStr().Length > 0)
                        rrr = respAccount.ToStr();

                    ///Clients.Caller.jobCleared(rrr);--------------------------------------------
                    ///
                    res.Data = rrr;
                    res.IsSuccess = true;
                    res.Message = "";


                    try
                    {

                        if (objAction.JobId.ToLong() > 0 && HubProcessor.Instance.objPolicy.DespatchTextForPDA.ToStr().Trim().Length > 0)
                        {
                            DispatchJobSMS(objAction.JobId.ToLong(), Enums.BOOKINGSTATUS.DISPATCHED.ToInt());



                        }

                        if (Global.enableEmaiLReceipt.ToStr() == "1")
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

                    ///Clients.Caller.manualFares(rrr);----------------------------------------------
                    ///
                    res.Data = new JavaScriptSerializer().Serialize(rrr);
                    res.IsSuccess = true;
                    res.Message = "";
                }
            }
            catch (Exception ex)
            {
                try
                {
                    //                    
                    //if (jStatus == Enums.BOOKINGSTATUS.DISPATCHED.ToStr().ToLower())
                    //    Clients.Caller.jobCleared("exceptionoccurred");
                    //else
                    //    Clients.Caller.manualFares("exceptionoccurred");
                    ///------------------------------------------------------------------------
                    ///
                    res.Data = null;
                    res.IsSuccess = true;
                    res.Message = "exceptionoccurred";


                    File.AppendAllText(physicalPath + "\\requestclearjob_exception.txt", DateTime.Now + ": datavalue=" + mesg + ",exception:" + ex.Message + Environment.NewLine);


                }
                catch
                {

                    //
                }

            }

            return res;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestManualFares")]
        public ResponseData requestManualFares(string mesg)
        {
            ResponseData res = new ResponseData();

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
                        string dropOff = General.GetLocationName(objAction.Latitude, objAction.Longitude);


                        objAction.Dropoff = dropOff;
                    }
                    else
                        objAction.Dropoff = string.Empty;


                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        if (objAction.IsMeter.ToStr().Trim() == "1")
                        {
                            int waitingTime = 0;

                            if (objAction.WaitingTime.ToStr().IsNumeric())
                                waitingTime = objAction.WaitingTime.ToInt();



                            db.ExecuteQuery<int?>("exec stp_UpdateAndClearJobFaresDetails {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}"
                                                                     , objAction.JobId.ToStr().ToLong(), objAction.DrvId.ToInt(), objAction.JStatus.ToInt(), objAction.DStatus.ToInt()
                                 , objAction.Dropoff.ToStr(), objAction.Miles, objAction.Fares.ToDecimal(), waitingTime, objAction.WaitingCharges, objAction.ParkingCharges.ToDecimal()
                                 , objAction.ExtraDropCharges.ToDecimal(), objAction.BookingFee.ToDecimal(), objAction.ExtrasDetail.ToStr());








                        }
                        else
                        {
                            db.stp_UpdateJobAndRoute(objAction.JobId.ToStr().ToLong(), objAction.DrvId.ToInt(), objAction.JStatus.ToInt(), objAction.DStatus.ToInt(), objAction.Dropoff.ToStr(), objAction.Miles, null);


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
                                   HubProcessor.Instance.objPolicy.CreditCardExtraCharges.ToDecimal(), 0.00m, 0.00m,
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

                        if (HubProcessor.Instance.listofJobs.Count(c => c.DriverId == objAction.DrvId.ToInt() && c.JobId == objAction.JobId.ToLong()) > 0)
                        {
                            HubProcessor.Instance.listofJobs.RemoveAll(c => c.DriverId == objAction.DrvId.ToInt() && c.JobId == objAction.JobId.ToLong());
                        }
                    }
                    catch
                    {


                    }

                    if (respAccount.ToStr().Length > 0)
                        rrr = respAccount.ToStr();

                    ///Clients.Caller.jobCleared(rrr);--------------------------------------------
                    ///
                    res.Data = rrr;
                    res.IsSuccess = true;
                    res.Message = "";


                    try
                    {

                        if (objAction.JobId.ToLong() > 0 && HubProcessor.Instance.objPolicy.DespatchTextForPDA.ToStr().Trim().Length > 0 && Global.enableClearJobText == "1")
                        {
                            DispatchJobSMS(objAction.JobId.ToLong(), Enums.BOOKINGSTATUS.DISPATCHED.ToInt());



                        }

                        if (Global.enableEmaiLReceipt.ToStr() == "1")
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

                    ///Clients.Caller.manualFares(rrr);----------------------------------------------
                    ///
                    res.Data = rrr;///new JavaScriptSerializer().Serialize(rrr);
                    res.IsSuccess = true;
                    res.Message = "";
                }
            }
            catch (Exception ex)
            {
                try
                {
                    //                    
                    //if (jStatus == Enums.BOOKINGSTATUS.DISPATCHED.ToStr().ToLower())
                    //    Clients.Caller.jobCleared("exceptionoccurred");
                    //else
                    //    Clients.Caller.manualFares("exceptionoccurred");
                    ///------------------------------------------------------------------------
                    ///
                    res.Data = null;
                    res.IsSuccess = true;
                    res.Message = "exceptionoccurred";


                    File.AppendAllText(physicalPath + "\\requestclearjob_exception.txt", DateTime.Now + ": datavalue=" + mesg + ",exception:" + ex.Message + Environment.NewLine);


                }
                catch
                {

                    //
                }

            }

            return res;
        }


        //02032022
        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestbookingdetail")]
        public ResponseData requestbookingdetail(string mesg)
        {
            try
            {

                File.AppendAllText(physicalPath + "\\" + "requestbookingdetail.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + mesg + Environment.NewLine);
            }
            catch
            {


            }

            JobAction objJobAction = new JavaScriptSerializer().Deserialize<JobAction>(mesg);

            long l_JobId = long.Parse(objJobAction.JobId);

            ResponseData res = new ResponseData();


            try
            {
                using (TaxiDataContext db = new TaxiDataContext())
                {

                    //var result = db.Fleet_Drivers.Where(c => c.HasPDA != null && c.HasPDA == true && c.IsActive == true).OrderBy(c => c.DriverNo)
                    //                .Select(args => args.Id + "," + args.DriverNo + "," + args.DriverName + "," + args.Fleet_VehicleType.VehicleType.ToUpper()).ToArray<string>();

                    //var result = db.Bookings.Where(c=> c.Id == objJobAction.JobId).ToArray<string>();

                    var objBooking = db.Bookings.Where(c => c.Id == l_JobId).FirstOrDefault();

                    string FOJJob = string.Empty;
                    string startJobPrefix = "JobId:";

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
                    string fromAddress = objBooking.FromAddress.ToStr().Trim();
                    string pickUpPlot = "";
                    string toAddress = objBooking.ToAddress.ToStr().Trim();
                    string dropOffPlot = "";
                    string mobileNo = objBooking.CustomerMobileNo.ToStr();
                    decimal pdafares = objBooking.GetType().GetProperty(Instance.objPolicy.PDAFaresPropertyName.ToStr().Trim()).GetValue(objBooking, null).ToDecimal();
                    string journey = "O/W";

                    //if (objBooking.JourneyTypeId.ToInt() == 2)
                    //{
                    //    journey = "Return";

                    //}
                    if (objBooking.JourneyTypeId.ToInt() == 3)
                    {
                        journey = "W/R";
                    }

                    string companyName = string.Empty;

                    //if (drvPdaVersion < 11 && objBooking.CompanyId != null && objBooking.Gen_Company.DefaultIfEmpty().AccountTypeId.ToInt() != Enums.ACCOUNT_TYPE.CASH)
                    //    companyName = objBooking.Gen_Company.DefaultIfEmpty().CompanyName;
                    //    else
                    companyName = objBooking.Gen_Company.DefaultIfEmpty().CompanyName.ToStr();

                    string paymentType = objBooking.Gen_PaymentType.PaymentCategoryId == null ? objBooking.Gen_PaymentType.DefaultIfEmpty().PaymentType.ToStr()
                            : objBooking.Gen_PaymentType.Gen_PaymentCategory.CategoryName.ToStr();
                    string specialRequirements = objBooking.SpecialRequirements.ToStr();
                    if (objBooking.SecondaryPaymentTypeId != null && objBooking.CashFares.ToDecimal() > 0)
                    {

                        specialRequirements += " , Additional Cash Payment : " + objBooking.CashFares.ToDecimal();
                    }
                    string IsExtra = (objBooking.CompanyId != null || objBooking.FromLocTypeId == Enums.LOCATION_TYPES.AIRPORT || objBooking.ToLocTypeId == Enums.LOCATION_TYPES.AIRPORT) ? "1" : "0";

                    string viaP = "";
                    int i = 1;


                    if (objBooking.Booking_ViaLocations.Count > 0)
                    {



                        viaP = string.Join(" * ", objBooking.Booking_ViaLocations.Select(c => "(" + i++.ToStr() + ")" + c.ViaLocValue.ToStr()).ToArray<string>());
                    }

                    string parkingandWaiting = string.Empty;
                    string agentDetails = string.Empty;
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

                    string showFaresValue = objBooking.Gen_PaymentType.ShowFaresOnPDA.ToStr().Trim();
                    string showFares = ",\"ShowFares\":\"" + showFaresValue + "\"";
                    string showSummary = ",\"ShowSummary\":\"" + showFaresValue + "\"";

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

                    }
                    catch
                    {

                    }


                    //

                    //if (objBooking.CompanyId != null && Global.enableBookingRefOnAccJob == "1")
                    //{
                    //    if (specialRequirements.Length == 0)
                    //        specialRequirements = "Booking Ref- " + objBooking.BookingNo.ToStr();
                    //    else
                    //        specialRequirements = "Booking Ref- " + objBooking.BookingNo.ToStr() + " , " + specialRequirements;
                    //}

                    if (specialRequirements.ToStr().Contains("\""))
                        specialRequirements = specialRequirements.ToStr().Replace("\"", "-").Trim();


                    string toDoorNo = objBooking.ToDoorNo.ToStr().Trim();

                    if (objBooking.ToLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT && objBooking.JourneyTypeId.ToInt() == Enums.JOURNEY_TYPES.RETURN)
                        toDoorNo = string.Empty;
                    else if (toDoorNo.Length > 0)
                        toDoorNo = toDoorNo + "-";


                    //   "\", \"Destination\":\"" + (toDoorNo + toAddress + dropOffPlot) + "\"," +


                    //string msg = FOJJob + startJobPrefix + "{ \"JobId\" :\"" + objBooking.Id.ToStr() +
                    //               "\", \"Pickup\":\"" + (!string.IsNullOrEmpty(objBooking.FromDoorNo) ? fromdoorno + "-" + fromAddress + pickUpPlot : fromAddress + pickUpPlot) +
                    //               "\", \"Destination\":\"" + (!string.IsNullOrEmpty(objBooking.ToDoorNo) ? objBooking.ToDoorNo + "-" + toAddress + dropOffPlot : toAddress + dropOffPlot) + "\"," +
                    //               "\"PickupDateTime\":\"" + string.Format("{0:dd/MM/yyyy   HH:mm}", objBooking.PickupDateTime) + "\"" +
                    //               ",\"Cust\":\"" + objBooking.CustomerName + "\",\"Mob\":\"" + mobileNo + " " + "\",\"Fare\":\"" + string.Format("{0:0.00}", pdafares) + "\",\"Vehicle\":\"" + objBooking.Fleet_VehicleType.VehicleType + "\",\"Account\":\"" + companyName + " " + "\"" +
                    //                 ",\"Lug\":\"" + objBooking.NoofLuggages.ToInt() + "\",\"Passengers\":\"" + objBooking.NoofPassengers.ToInt() + "\",\"Journey\":\"" + journey + "\",\"Payment\":\"" + paymentType + "\",\"Special\":\"" + specialRequirements + " " + "\",\"Extra\":\"" + IsExtra + "\",\"Via\":\"" + viaP + " " + "\"" +

                    //                 ",\"CompanyId\":\"" + objBooking.CompanyId.ToInt() + "\",\"SubCompanyId\":\"" + objBooking.SubcompanyId.ToInt() + "\",\"QuotedPrice\":\"" + (objBooking.IsQuotedPrice.ToBool() ? "1" : "0") + "\"" +


                    //                 parkingandWaiting + ",\"DriverFares\":\"" + String.Format("{0:0.00}", objBooking.FareRate) + "\"" +
                    //              agentDetails +
                    //                 ",\"Did\":\"" + objJobAction.DrvId + "\",\"BabySeats\":\"" + objBooking.BabySeats.ToStr() + "\"" + showFares + showSummary + appendString + " }";

                    string msg = FOJJob + startJobPrefix + "{ \"JobId\" :\"" + objBooking.Id.ToStr() +
                                    "\", \"Pickup\":\"" + (!string.IsNullOrEmpty(objBooking.FromDoorNo) ? fromdoorno + "-" + fromAddress + pickUpPlot : fromAddress + pickUpPlot) +
                                   "\", \"Destination\":\"" + (toDoorNo + toAddress + dropOffPlot) + "\"," +
                                    "\"PickupDateTime\":\"" + string.Format("{0:dd/MM/yyyy   HH:mm}", objBooking.PickupDateTime) + "\"" +
                                    ",\"Cust\":\"" + objBooking.CustomerName + "\",\"Mob\":\"" + mobileNo + " " + "\",\"Fare\":\"" + string.Format("{0:0.00}", pdafares) + "\",\"Vehicle\":\"" + objBooking.Fleet_VehicleType.VehicleType + "\",\"Account\":\"" + companyName + " " + "\"" +
                                      ",\"Lug\":\"" + objBooking.NoofLuggages.ToInt() + "\",\"Passengers\":\"" + objBooking.NoofPassengers.ToInt() + "\",\"Journey\":\"" + journey + "\",\"Payment\":\"" + paymentType + "\",\"Special\":\"" + specialRequirements + " " + "\",\"Extra\":\"" + IsExtra + "\",\"Via\":\"" + viaP + " " + "\"" +

                                      ",\"CompanyId\":\"" + objBooking.CompanyId.ToInt() + "\",\"SubCompanyId\":\"" + objBooking.SubcompanyId.ToInt() + "\",\"QuotedPrice\":\"" + (objBooking.IsQuotedPrice.ToBool() ? "1" : "0") + "\"" +


                                      parkingandWaiting + ",\"DriverFares\":\"" + String.Format("{0:0.00}", objBooking.FareRate) + "\"" +
                                   agentDetails +
                                      ",\"Did\":\"" + objJobAction.DrvId + "\",\"BabySeats\":\"" + objBooking.BabySeats.ToStr() + "\"" + showFares + showSummary + appendString + " }";



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





                    res.Data = msg;
                    res.IsSuccess = true;
                    res.Message = "";


                }

            }
            catch (Exception ex)
            {

                try
                {
                    File.AppendAllText(physicalPath + "\\requestbookingdetail_exception.txt", DateTime.Now.ToStr() + ex.Message + Environment.NewLine);

                }
                catch
                {


                }

                res.Data = null;
                res.IsSuccess = false;
                res.Message = ex.Message;
            }

            return res;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestsendPaymentLink")]
        public ResponseData requestsendPaymentLink(string mesg)
        {
            ResponseData res = new ResponseData();

            //Logging
            try { File.AppendAllText(physicalPath + "\\" + "requestsendPaymentLink.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + mesg + Environment.NewLine); }
            catch { }

            try
            {
                sendPaymentLinkReq req = new JavaScriptSerializer().Deserialize<sendPaymentLinkReq>(mesg);
                string mobNo = req.mobNo;
                string strPaymentLink = "http://www.testPaymentLink.com";

                AddSMS(mobNo, strPaymentLink, 0);

                res.Data = "SMS Added to send successfully";
                res.IsSuccess = true;
                res.Message = "";
            }
            catch (Exception ex)
            {
                //Logging
                try { File.AppendAllText(physicalPath + "\\requestsendPaymentLink_exception.txt", DateTime.Now.ToStr() + ex.Message + Environment.NewLine); }
                catch { }

                res.Data = null;
                res.IsSuccess = false;
                res.Message = ex.Message;
            }

            return res;
        }

        //09032022
        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestDriverPinLogin")]
        public ResponseData requestDriverPinLogin(string mesg)
        {
            ResponseData res = new ResponseData();

            //Logging
            try { File.AppendAllText(physicalPath + "\\" + "requestDriverPinLogin.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + mesg + Environment.NewLine); }
            catch { }

            try
            {
                DriverDetail req = Newtonsoft.Json.JsonConvert.DeserializeObject<DriverDetail>(mesg);

                using (TaxiDataContext db = new TaxiDataContext())
                {
                    //req.Password as PIN, from paramenter
                    //e.LoginId as PIN, from table Fleet_Driver
                    var driver = db.Fleet_Drivers.Where(e => e.LoginId == req.Password && e.IsActive == true)
                                                      .FirstOrDefault();

                    if (driver == null)
                    {
                        res.Data = "";
                        res.IsSuccess = false;
                        res.Message = "Invalid PIN";

                        return res;
                    }

                    req.DrvId = driver.Id.ToString();
                    req.DrvNo = driver.DriverNo;
                    req.Password = driver.LoginPassword;

                    mesg = new JavaScriptSerializer().Serialize(req);
                }

                string strLoginOrShiftLogin, strrequestDriverSettingsLocal, strCombine;

                //if (req.EnableOnlineStatus == "1")
                //{
                //    //calling web method (HttpGet, HttpPost), not a local method
                //    strLoginOrShiftLogin = Login(mesg).Data;
                //}
                //else
                //{
                //calling web method (HttpGet, HttpPost), not a local method
                strLoginOrShiftLogin = requestShiftLogin(mesg).Data;
                //  }

                //calling web method (HttpGet, HttpPost), not a local method
                string strPara = "=" + req.DrvId.ToString();
                strrequestDriverSettingsLocal = requestDriverSettings(strPara).Data;

                if (strLoginOrShiftLogin.Contains("{") == false)    //check the result is not a Json                 
                    strCombine = "{\"loginResponse\":\"" + strLoginOrShiftLogin + "\", \"settingResponse\":" + strrequestDriverSettingsLocal.Replace("update settings<<<", "") + "}";
                else
                    strCombine = "{\"loginResponse\":" + strLoginOrShiftLogin + ", \"settingResponse\":" + strrequestDriverSettingsLocal.Replace("update settings<<<", "") + "}";


                res.Data = strCombine;
                res.IsSuccess = true;
                res.Message = "";
            }
            catch (Exception ex)
            {
                //Logging
                try { File.AppendAllText(physicalPath + "\\requestDriverPinLogin_exception.txt", DateTime.Now.ToStr() + ex.Message + Environment.NewLine); }
                catch { }

                res.Data = null;
                res.IsSuccess = false;
                res.Message = ex.Message;
            }

            return res;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestDocuments")]
        public ResponseData requestDocuments(string mesg)
        {
            ResponseData res = new ResponseData();

            string jStatus = string.Empty;

            try
            {

                try
                {
                    File.AppendAllText(physicalPath + "\\requestDocuments.txt", DateTime.Now + " : msg" + mesg + Environment.NewLine);
                }
                catch
                {

                }





                JobAction objAction = new JavaScriptSerializer().Deserialize<JobAction>(mesg);


                int driverId = objAction.DrvId.ToInt();


                string driverPin = "";

                //using (TaxiDataContext db = new TaxiDataContext())
                //{

                //    driverPin= db.Fleet_Drivers.Where(c => c.Id == driverId).Select(c => c.LoginId).FirstOrDefault();

                //}


                //if (driverPin.ToStr().Trim().Length == 0)
                //{

                //    res.IsSuccess = false;
                //    res.Message = "Required pin code";
                //}
                //else
                //{


                try
                {



                    string API = "https://www.treasureonlineapi.co.uk/CabTreasureWebApi/Home/GetDriverDocumentPortal" + "?json=" + HubProcessor.Instance.objPolicy.DefaultClientId.ToStr() + "," + driverPin.ToStr();


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
                            string respo = sr.ReadToEnd().ToStr();




                            if (respo.StartsWith("success"))
                            {

                                res.Data = respo.Replace("success:", "").Trim() + "&id=" + driverId;
                                res.IsSuccess = true;
                            }
                            else
                            {
                                res.Message = respo;
                                res.IsSuccess = false;
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    res.IsSuccess = false;
                    res.Message = ex.Message;
                    try
                    {
                        File.AppendAllText(physicalPath + "\\requestDocuments_exception1.txt", DateTime.Now + " : msg" + mesg + ",exception:" + ex.Message + Environment.NewLine);
                    }
                    catch
                    {

                    }
                }

                //      }

            }
            catch (Exception ex)
            {
                res.IsSuccess = false;
                res.Message = ex.Message;
                try
                {
                    File.AppendAllText(physicalPath + "\\requestDocuments_exception2.txt", DateTime.Now + " : msg" + mesg + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }

            }

            return res;
        }

        #endregion



        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestManualFaresAdv")]
        public ResponseData requestManualFaresAdv(string mesg)
        {
            ResponseData res = new ResponseData();

            string jStatus = string.Empty;

            try
            {

                try
                {

                    File.AppendAllText(physicalPath + "\\requestManualFaresAdv.txt", DateTime.Now + " : msg" + mesg + Environment.NewLine);
                }
                catch
                {

                }


                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                string rrr = "false";


                JobAction objAction = new JavaScriptSerializer().Deserialize<JobAction>(dataValue.ToStr());
                jStatus = objAction.JStatus.ToStr().ToLower();


                if (objAction.JStatus.ToStr().ToLower() == "jobcharges")
                {
                    try
                    {

                        if ((objAction.IsMeter.ToStr().Trim().Length == 0 || objAction.IsMeter.ToStr() == "0")
                            && objAction.QuotedPrice.ToStr() == "0")
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
                        }
                        rrr = "true";
                    }
                    catch (Exception ex)
                    {
                        rrr = "false";

                        File.AppendAllText(physicalPath + "\\log_manualfares.txt", DateTime.Now.ToStr() + ",DataValue:" + dataValue + ",exception:" + ex.Message);
                    }

                    ///Clients.Caller.manualFares(rrr);----------------------------------------------
                    ///
                    res.Data = rrr;///new JavaScriptSerializer().Serialize(rrr);
                    res.IsSuccess = true;
                    res.Message = "";
                }
            }
            catch (Exception ex)
            {
                try
                {
                    //                    
                    //if (jStatus == Enums.BOOKINGSTATUS.DISPATCHED.ToStr().ToLower())
                    //    Clients.Caller.jobCleared("exceptionoccurred");
                    //else
                    //    Clients.Caller.manualFares("exceptionoccurred");
                    ///------------------------------------------------------------------------
                    ///
                    res.Data = null;
                    res.IsSuccess = true;
                    res.Message = "exceptionoccurred";


                    File.AppendAllText(physicalPath + "\\requestclearjob_exception.txt", DateTime.Now + ": datavalue=" + mesg + ",exception:" + ex.Message + Environment.NewLine);


                }
                catch
                {

                    //
                }

            }

            return res;
        }
        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestPauseMeter")]
        public ResponseData requestPauseMeter(string mesg)
        {

            try
            {

                File.AppendAllText(physicalPath + "\\" + "requestPauseMeter.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", json: " + mesg.ToString() + Environment.NewLine);
            }
            catch
            {


            }

            ResponseData res = new ResponseData();

            try
            {


                string dataValue = mesg;
                dataValue = dataValue.Trim();


                PauseMeter objAction = new JavaScriptSerializer().Deserialize<PauseMeter>(dataValue);



                if (objAction.version.ToStr().Length == 0)
                {
                    res.Data = null;
                    res.IsSuccess = false;
                    res.Message = "Something went wrong";


                }
                else
                {

                    res.IsSuccess = true;
                    res.Message = "";

                    long jobId = objAction.JobId.ToLong();


                    string data = string.Empty;


                    if (objAction.isPause)
                    {
                        data = "Driver " + objAction.DrvNo.ToStr() + " pressed PAUSED Meter" + Environment.NewLine +
                               "Miles :" + objAction.Miles.ToDecimal() + Environment.NewLine +
                               "Fares   :" + objAction.Fares.ToDecimal() + Environment.NewLine +
                               "Waiting :" + objAction.WaitingCharges.ToDecimal() + Environment.NewLine;
                    }
                    try
                    {
                        data = "Driver " + objAction.DrvNo.ToStr() + " pressed RESUME Meter" + Environment.NewLine +
                              "Miles :" + objAction.Miles.ToDecimal() + Environment.NewLine +
                              "Fares   :" + objAction.Fares.ToDecimal() + Environment.NewLine +
                              "Waiting :" + objAction.WaitingCharges.ToDecimal() + Environment.NewLine;

                    }
                    catch
                    {

                    }


                    using (TaxiDataContext db = new TaxiDataContext())
                    {

                        db.stp_BookingLog(jobId, objAction.DrvNo.ToStr(), data);

                    }


                }

            }
            catch (Exception ex)
            {


                res.Data = null;
                res.IsSuccess = false;
                res.Message = ex.Message;
            }

            return res;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestViaAction")]
        public ResponseData requestViaAction(string mesg)
        {
            ResponseData res = new ResponseData();

            string respo = "true";
            JobActionEx objAction = null;
            try
            {
                try
                {
                    //
                    File.AppendAllText(AppContext.BaseDirectory + "\\requestViaAction.txt", DateTime.Now.ToStr() + " request" + mesg + Environment.NewLine);
                }
                catch
                {

                }


                string dataValue = mesg;
                dataValue = dataValue.Trim();
                objAction = new JavaScriptSerializer().Deserialize<JobActionEx>(mesg);
                RemoveUK(ref objAction.Dropoff);



                long jobId = objAction.JobId.ToLong();

                string via = objAction.Dropoff.ToStr();
                using (TaxiDataContext db = new TaxiDataContext())
                {

                    try
                    {

                        //
                        if (via.Contains(")"))
                            via = via.Substring(via.IndexOf(')') + 1);


                        if (respo == "true")
                        {
                            if (objAction.JStatus.ToStr() == "5")
                            {

                                db.ExecuteQuery<int>("update booking_viaLocations set iscurrentstop=1, onroutedatetime=getdate() where bookingid=" + jobId + " and vialocvalue='" + via + "'");


                                db.stp_BookingLog(jobId, "DRIVER", "Via (ONROUTE) : " + via);


                            }
                            else if (objAction.JStatus.ToStr() == "6")
                            {

                                db.ExecuteQuery<int>("update booking_viaLocations set iscurrentstop=1, arrivaldatetime=getdate() where bookingid=" + jobId + " and vialocvalue='" + via + "'");

                                db.stp_BookingLog(jobId, "DRIVER", "Via (ARRIVED) : " + via);

                            }
                            else if (objAction.JStatus.ToStr() == "7")
                            {

                                db.ExecuteQuery<int>("update booking_viaLocations set iscurrentstop=0, pobdatetime=getdate() where bookingid=" + jobId + " and vialocvalue='" + via + "'");
                                db.stp_BookingLog(jobId, "DRIVER", "Via (POB) : " + via);

                            }
                            if (objAction.JStatus.ToStr() == "13")
                            {
                                db.stp_BookingLog(jobId, "DRIVER", "Via (NOPICKUP) : " + via);
                            }

                            db.ExecuteQuery<int>("update booking_viaLocations set iscurrentstop=0 where bookingid=" + jobId + " and vialocvalue!='" + via + "'");




                        }
                        else
                        {
                            respo = objAction.Message;

                        }
                        res.IsSuccess = true;

                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            res.IsSuccess = false;
                            res.Message = ex.Message;
                            objAction.Message = ex.Message;
                            File.AppendAllText(AppContext.BaseDirectory + "\\requestarrive_exception1.txt", DateTime.Now.ToStr() + " request" + mesg + ",exception:" + ex.Message + Environment.NewLine);
                        }
                        catch
                        {

                        }
                    }

                }





            }

            catch (Exception ex)
            {
                res.IsSuccess = false;
                res.Message = ex.Message;
                try
                {
                    objAction.Message = ex.Message;
                    File.AppendAllText(AppContext.BaseDirectory + "\\requestarrive_exception2.txt", DateTime.Now.ToStr() + " request" + mesg + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
            }




            res.Data = respo;

            return res;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestViadetails")]
        public ResponseData requestViadetails(string mesg)
        {
            try
            {
                File.AppendAllText(physicalPath + "\\" + "requestViadetails.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + mesg + Environment.NewLine);
            }
            catch
            {
            }
            ResponseData res = new ResponseData();
            try
            {
                string dataValue = mesg.ToStr().Trim();
                JobDetails objAction = new JavaScriptSerializer().Deserialize<JobDetails>(dataValue.ToStr());
                string name = string.Empty;
                string mobileno = string.Empty;
                long jobId = objAction.JobId.ToLong();
                int driverId = objAction.driverId.ToInt();
                string via = objAction.Address.ToStr().Trim();
                string viaLocation = objAction.Address.ToStr().Trim();
                try
                {
                    if (via.Length > 0)
                    {
                        try
                        {
                            File.AppendAllText(physicalPath + "\\" + "requestViadetails.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", jobId:" + jobId + ", via: " + via + Environment.NewLine);
                        }
                        catch
                        {
                        }
                        using (TaxiDataContext db = new TaxiDataContext())
                        {
                            if (via.StartsWith("(") && via.IndexOf(")") > 0)
                            {
                                int closeBracketIndex = via.IndexOf(")");
                                viaLocation = via.Substring(closeBracketIndex + 1).Trim();
                            }
                            var obj = db.Booking_ViaLocations.FirstOrDefault(c => c.BookingId == jobId && c.ViaLocValue.Trim() == viaLocation);
                            try
                            {
                                File.AppendAllText(physicalPath + "\\" + "requestViadetails.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ,viaLocation:" + viaLocation + ", obj: " + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
                            }
                            catch
                            {
                            }
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
                res.Data = new JavaScriptSerializer().Serialize(objAction);
                res.IsSuccess = true;
                res.Message = "";
            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(physicalPath + "\\" + "requestViadetails_Exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + ex.Message + Environment.NewLine);
                }
                catch
                {
                }
                res.IsSuccess = false;
                res.Message = "";
            }
            return res;
        }
    }











}