using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Taxi_Model;
using Utils;
using CabTreasureJobPoolGateway;

using System.Net;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using CabTreasureJobPoolGateway.Models;

namespace SignalRHub
{

    public partial class PoolBooking
    {

        public long? PoolJobId;
        public long? JobId;
        public string JobProviderDefaultClientId;
        public string JobProviderClientName;
        public string JobAcceptorDefaultClientId;
        public string JobAcceptorClientName;
        public decimal JobPrice;
        public string JobStatus;
        public string PickupDatetime;
        public string PickupLocation;
        public string BookingJson;
        public string JobAcceptorConnectionString;
        public EventType EventType;
        public int SubCompanyId;
        public int VehicleTypeId;
        public string JobProviderClientCompanyNumber;
        public string VehicleName;
        public string UserName;

    }

    public class JobPoolResponse
    {
        public bool HasError;
        public string Message;
        public string Data;
    }

    public class RequestResponse<T>
    {
        private T _data;
        private bool _hasError = true;
        private string _message = "Some things went wrong.";

        public RequestResponse()
        {
        }
        public RequestResponse(ref T data)
        {
            _data = data;
        }

        public bool HasError
        {
            get { return _hasError; }
            set { _hasError = value; }
        }

        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        public T Data
        {
            get { return _data; }
            set { _data = value; }
        }
    }


    public class JobPoolBLL
    {

        private JobPoolGateway jobpoolGateway;

        public JobPoolBLL(string defaultClientId, string clientName)
        {
            try
            {
                ////
                jobpoolGateway = new JobPoolGateway(defaultClientId, 0, clientName);
                jobpoolGateway.SignRDetailEvent += new EventHandler<CabTreasureJobPoolGateway.Models.EventDetails>(jobpoolGateway_SignRDetailEvent);
                jobpoolGateway.SignRDriverStatusEventDetails += new EventHandler<CabTreasureJobPoolGateway.Models.DriverStatusEventDetails>(JobpoolGateway_SignRDriverStatusEventDetails);
                LogUtility.LogInfo("JobPoolSuccess", "JobPoolSuccess", "JobPoolSuccess");
                // //
                ////
            }
            catch (Exception exe)
            {
                LogUtility.LogException(exe, "JobPoolBLL", "JobPoolBLLError");
            }
        }

        Gen_SysPolicy_Configuration objPolicyConfiguration = null;
        List<SMSTag> listofsmstags = null;

        public static void UpdatePoolJob(long onlineBookingId, int bookingTypeId, long jobId, int driverId, int jobstatusId, string eventName)
        {
            try
            {
                if (jobId == 0)
                    return;

                if (bookingTypeId == 0)
                {
                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        db.CommandTimeout = 5;

                        var objBook = db.Bookings.Where(c => c.Id == jobId).Select(args => new { args.OnlineBookingId, args.BookingTypeId }).FirstOrDefault();
                        if (objBook.OnlineBookingId != null)
                        {

                            onlineBookingId = objBook.OnlineBookingId.ToLong();
                            bookingTypeId = objBook.BookingTypeId.ToInt();

                        }

                    }
                }

                if (onlineBookingId != 0)
                {
                    if (bookingTypeId.ToInt() == 100)
                    {

                        new System.Threading.Thread(delegate ()
                        {
                            try
                            {

                                General.UpdateThirdPartyJob(null, bookingTypeId.ToInt(), jobId, driverId, onlineBookingId.ToLong(), jobstatusId, eventName, HubProcessor.Instance.objPolicy.DefaultClientId.ToStr(), "");
                            }
                            catch (Exception ex)
                            {
                            }
                        }).Start();

                    }
                }

            }
            catch
            {

            }

        }


        private void JobpoolGateway_SignRDriverStatusEventDetails(object sender, CabTreasureJobPoolGateway.Models.DriverStatusEventDetails e)
        {
            try
            {
                string msg = Newtonsoft.Json.JsonConvert.SerializeObject(e);
                LogUtility.LogInfo(msg, "SignRDriverStatusEventDetails", "SignRDriverStatusEventDetails");
            }
            catch
            {
            }
            try
            {



                using (TaxiDataContext db = new TaxiDataContext())
                {
                    var objJob = db.Bookings.FirstOrDefault(c => c.Id == e.BookingID);
                    if (objJob != null)
                    {

                        var objDriver = Newtonsoft.Json.JsonConvert.DeserializeObject<CabTreasureJobPoolGateway.Models.ClsDriverDetails>(e.DriverDetails);
                        //
                        if (objPolicyConfiguration == null)
                            objPolicyConfiguration = General.GetObject<Gen_SysPolicy_Configuration>(c => c.Id != 0);


                        if (e.BookingStatusID == 5)
                        {
                            objJob.AcceptedDateTime = DateTime.Now;

                            if (objPolicyConfiguration.EnablePassengerText.ToBool())
                            {





                                if (objJob != null && objJob.BookingTypeId.ToInt() != 100 && objJob.JobCode.ToStr().Trim().Length == 0)
                                {
                                    // onlineBookingId = job.OnlineBookingId.ToLong();
                                    if (objJob.CustomerMobileNo.ToStr().Trim() != string.Empty && objJob.DisablePassengerSMS.ToBool() == false)
                                    {

                                        new Thread(delegate ()
                                        {
                                            try

                                            {

                                                SendSMS(objJob.CustomerMobileNo.ToStr().Trim(), GetMessage(objPolicyConfiguration.DespatchTextForCustomer.ToStr(), objJob, objJob.Id, objDriver), objJob.SMSType.ToInt());
                                            }
                                            catch
                                            {

                                            }
                                        }).Start();
                                        //

                                    }


                                }
                            }


                        }
                        else if (e.BookingStatusID == 6)
                        {
                            objJob.ArrivalDateTime = DateTime.Now;




                            if (objPolicyConfiguration.EnableArrivalBookingText.ToBool())
                            {


                                if (objJob != null && objJob.BookingTypeId.ToInt() != 100 && objJob.JobCode.ToStr().Trim().Length == 0)
                                {

                                    if (!string.IsNullOrEmpty(objJob.CustomerMobileNo))
                                    {


                                        string arrivalText = string.Empty;

                                        if (objJob.FromLocTypeId.ToInt() == 1)
                                        {
                                            arrivalText = objPolicyConfiguration.ArrivalAirportBookingText.ToStr().Trim();
                                        }
                                        else
                                        {
                                            arrivalText = objPolicyConfiguration.ArrivalBookingText.ToStr().Trim();
                                        }

                                        if (!string.IsNullOrEmpty(arrivalText))
                                        {

                                            new Thread(delegate ()
                                            {
                                                try
                                                {
                                                    SendSMS(objJob.CustomerMobileNo.ToStr().Trim(), GetMessage(arrivalText, objJob, objJob.Id, objDriver), objJob.SMSType.ToInt());
                                                }
                                                catch
                                                { }
                                            }).Start();
                                        }

                                    }
                                    else if (!string.IsNullOrEmpty(objJob.CustomerPhoneNo.ToStr().Trim()))
                                    {



                                        string arrivalText = string.Empty;

                                        if (objJob.FromLocTypeId.ToInt() == 1)
                                        {
                                            arrivalText = objPolicyConfiguration.ArrivalAirportBookingText.ToStr().Trim();
                                        }
                                        else
                                        {
                                            arrivalText = objPolicyConfiguration.ArrivalBookingText.ToStr().Trim();
                                        }

                                        if (!string.IsNullOrEmpty(arrivalText))
                                        {

                                            new Thread(delegate ()
                                            {
                                                SendSMS(objJob.CustomerPhoneNo.ToStr().Trim(), GetMessage(arrivalText, objJob, objJob.Id, objDriver), objJob.SMSType.ToInt());
                                            }).Start();
                                        }






                                    }
                                }
                            }
                        }
                        else if (e.BookingStatusID == 7)
                            objJob.POBDateTime = DateTime.Now;
                        else if (e.BookingStatusID == 8)
                            objJob.STCDateTime = DateTime.Now;
                        else if (e.BookingStatusID == 2)
                        {
                            objJob.ClearedDateTime = DateTime.Now;
                            objJob.BookingStatusId = 27;



                        }

                        if (e.BookingStatusID.ToInt() == 5)
                        {
                            //    a.On origin company - it should show below details
                            //i.Driver Name
                            //ii.Driver Badge No
                            //iii.Vehicle Badge No
                            //iv.Vehicle Reg No
                            //v.Subcompany Company Number
                            //vi.Time Accepted Booking
                            //vii.Sent back details(if exists)
                            //                                b.On the company who accpeted
                            //i.Origin Sub Company
                            //ii.Time job transferred to JP
                            //iii.Time job accepted
                            string driverInfo = string.Empty;

                            driverInfo += "Badge No :" + objDriver.driverBadge.ToStr();
                            driverInfo += Environment.NewLine + "Vehicle Badge No :" + objDriver.vehicleBadge.ToStr();
                            driverInfo += Environment.NewLine + "Vehicle Reg No :" + objDriver.vehicleNo.ToStr();

                            driverInfo += Environment.NewLine + "Vehicle :" + objDriver.vehicleColor.ToStr() + "," + objDriver.vehicleMake.ToStr() + "-" + objDriver.vehicleModel.ToStr();
                            objJob.NotesString = driverInfo.ToStr();

                            objJob.Booking_Logs.Add(new Booking_Log { User = "JOB POOL", BeforeUpdate = "", UpdateDate = DateTime.Now, AfterUpdate = driverInfo.ToStr() });
                        }

                        db.SubmitChanges();
                        BroadCast("**internalmessage>>" + "request updatepooljobstatus>>" + objJob.Id + ">>" + e.BookingStatusID + ">>" + objJob.NotesString);
                    }
                }




            }
            catch (Exception exe)
            {
                LogUtility.LogException(exe, "SignRDriverStatusEventDetails", "SignRDriverStatusEventDetailsError");
            }
        }



        //public void BidThisJob(long jobID)
        //{
        //    try
        //    {

        //        using (TaxiDataContext db = new TaxiDataContext())
        //        {
        //            db.DeferredLoadingEnabled = false;

        //            var objBooking = db.Bookings.FirstOrDefault(m => m.Id == jobID);


        //            if (objBooking.CompanyId != null)
        //                objBooking.CompanyCreditCardDetails = db.Gen_Companies.FirstOrDefault(c => c.Id == objBooking.CompanyId).DefaultIfEmpty().CompanyName.ToStr();

        //            objBooking.CustomerCreditCardDetails = db.Fleet_VehicleTypes.FirstOrDefault(c => c.Id == objBooking.VehicleTypeId).DefaultIfEmpty().VehicleType.ToStr();

        //            var bookingInformation = Newtonsoft.Json.JsonConvert.SerializeObject(objBooking);

        //            var response = jobpoolGateway.BidThisJob(bookingInformation);
        //            if (response != null && response.HasError == false)
        //            {
        //                db.stp_RunProcedure("update Booking set BookingStatusId=25,FlightDepartureDate=getdate() where Id=" + jobID);



        //                new BroadcasterData().BroadCastToAll(RefreshTypes.REFRESH_ACTIVEBOOKINGS_DASHBOARD);
        //            }
        //            else if (response != null && response.HasError == true)
        //            {
        //                //ENUtils.ShowMessage(response.Message);
        //            }
        //            else
        //            {
        //                //ENUtils.ShowMessage("Job Transfer failed.");
        //            }

        //        }
        //    }
        //    catch (Exception exe)
        //    {
        //        LogUtility.LogException(exe, "BidThisJob", "JobPoolBLLError");
        //        //ENUtils.ShowMessage(exe.Message);
        //    }
        //}

        #region Events

        private void jobpoolGateway_SignRDetailEvent(object sender, CabTreasureJobPoolGateway.Models.EventDetails e)
        {
            string msg = string.Empty;
            try
            {

                //File.AppendAllText(Application.StartupPath + "\\jobpoolGateway_SignRDetailEvent.txt", DateTime.Now + "args : " + e.ToStr() + Environment.NewLine);



                msg = Newtonsoft.Json.JsonConvert.SerializeObject(e);
                LogUtility.LogInfo(msg, "SignRDetailEvent", "SignRDetailEvent");
            }
            catch
            {
            }

            try
            {

                if (e.EventType == CabTreasureJobPoolGateway.Models.EventType.UpdateBid)
                {
                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        string query = "insert into Booking_Biddings(JobId,BidRate,BiddingDateTime,BiddingStatusId,bidBy,bidbyclientid) values(" + e.JobId + "," + e.JobPrice + ",getdate()," + e.JobStatus + ",'" + e.JobAcceptorClientName + "','" + e.JobAcceptorDefaultClientId + "');";

                        //  query = query + "insert into booking_log(BookingId,[User],AfterUpdate,UpdateDate)values(" + e.JobId + ",'" + "JOB POOL"+ "','Job is Accepted by Company : " + e.JobAcceptorLicenseNo.ToStr() + "',getdate())";
                        //
                        db.stp_RunProcedure(query);
                    }
                }
                else if (e.EventType == CabTreasureJobPoolGateway.Models.EventType.AssignJob)
                {
                    using (TaxiDataContext db = new TaxiDataContext())
                    {

                        var bo = Newtonsoft.Json.JsonConvert.DeserializeObject<Booking>(e.BookingJson);



                        try
                        {

                            //File.AppendAllText(Application.StartupPath + "\\jobpoolGateway_SignRDetailEvent.txt", DateTime.Now + "args : " + e.ToStr() + Environment.NewLine);



                            LogUtility.LogInfo(e.BookingJson.ToStr(), "AssignJob", "AssignJob");
                        }
                        catch
                        {
                        }

                        //int comId = db.Gen_Companies.FirstOrDefault(c => c.CompanyName.ToLower() == bo.CompanyCreditCardDetails.ToStr().ToLower()).DefaultIfEmpty().Id;
                        int vehicleId = db.Fleet_VehicleTypes.Where(c => c.VehicleType == bo.BoundType).Select(c => c.Id).FirstOrDefault().DefaultIfEmpty();
                        /*
                        if (comId > 0)
                        {
                            bo.CompanyId = comId;
                        }
                        */



                        bo.CompanyId = null;
                        bo.AdvanceBookingId = null;
                        bo.MasterJobId = null;
                        bo.ZoneId = null;
                        bo.DropOffZoneId = null;
                        bo.DriverId = null;
                        bo.ReturnDriverId = null;
                        bo.VehicleTypeId = null;
                        bo.FleetMasterId = null;
                        bo.OnlineBookingId = e.JobId;
                        bo.Id = 0;
                        bo.BookingTypeId = 100;
                        bo.BookingDate = DateTime.Now;
                        bo.CustomerId = null;
                        // = e.JobProviderClientName.ToStr();
                        bo.SubcompanyId = 1;
                        bo.BookingNo = "JP-" + e.PoolJobId;
                        bo.CompanyCreditCardDetails = e.JobProviderLicenseNo.ToStr();


                        bo.ZoneId = null;
                        bo.DropOffZoneId = null;


                        if (bo.BoundType.Length > 30)
                        {
                            bo.BoundType = bo.BoundType.Substring(0, 28);


                        }

                        if (bo.FromLocTypeId.ToInt() == 1)
                        {
                            if (bo.FromLocId.ToInt() > 0)
                            {
                                if (db.Gen_Locations.Count(c => c.Id == bo.FromLocId) == 0)
                                {
                                    bo.FromLocId = null;

                                    bo.FromLocTypeId = 7;
                                }
                            }
                        }

                        if (bo.ToLocTypeId.ToInt() == 1)
                        {
                            if (bo.ToLocId.ToInt() > 0)
                            {
                                if (db.Gen_Locations.Count(c => c.Id == bo.ToLocId) == 0)
                                {
                                    bo.ToLocId = null;

                                    bo.ToLocTypeId = 7;
                                }
                            }
                        }


                        if (bo.FromLocTypeId.ToInt() != 1)
                        {
                            bo.FromLocTypeId = 7;
                            bo.FromLocId = null;
                        }

                        if (bo.ToLocTypeId.ToInt() != 1)
                        {
                            bo.ToLocTypeId = 7;
                            bo.ToLocId = null;
                        }



                        if (vehicleId > 0)
                        {
                            bo.VehicleTypeId = vehicleId;
                        }
                        else
                        {
                            bo.VehicleTypeId = HubProcessor.Instance.objPolicy.DefaultVehicleTypeId;

                            bo.SpecialRequirements = "Vehicle Type:+" + bo.BoundType + " , " + bo.SpecialRequirements.ToStr();


                        }
                        //
                        //    bo.Booking_Logs.Add(new Booking_Log { User = "JOB POOL", UpdateDate = DateTime.Now, BeforeUpdate = "", AfterUpdate = "Job Received from Company :" + e.JobProviderLicenseNo.ToStr() });

                        db.Bookings.InsertOnSubmit(bo);
                        db.SubmitChanges();
                    }



                    e.BookingJson = JobPoolAPICaller.JobPoolAPIProxy.GetJobCount(HubProcessor.Instance.objPolicy.DefaultClientId.ToStr());


                    JobPoolResponse objResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<JobPoolResponse>(e.BookingJson);
                    if (objResponse.HasError == false)
                        e.BookingJson = objResponse.Data.ToStr();
                    else
                        e.BookingJson = "0";

                    msg = Newtonsoft.Json.JsonConvert.SerializeObject(e);


                    BroadCast("**internalmessage>>" + "request jobpool>>" + e.JobId.ToLong() + ">>" + msg);
                    //  BroadCast("refresh active booking dashboard");

                    //  ShowMessage("Job Assigned", "Job has been Assigned to " + e.JobAcceptorClientName, ToolTipIcon.Info);
                }
                else if (e.EventType == CabTreasureJobPoolGateway.Models.EventType.UpdateJob)
                {
                    using (TaxiDataContext db = new TaxiDataContext())
                    {

                        var objBooking = db.Bookings.Where(c => c.OnlineBookingId == e.JobId).FirstOrDefault();

                        var bo = Newtonsoft.Json.JsonConvert.DeserializeObject<Booking>(e.BookingJson);

                        //int comId = db.Gen_Companies.FirstOrDefault(c => c.CompanyName.ToLower() == bo.CompanyCreditCardDetails.ToStr().ToLower()).DefaultIfEmpty().Id;
                        int vehicleId = db.Fleet_VehicleTypes.FirstOrDefault(c => c.VehicleType == bo.BoundType).DefaultIfEmpty().Id;
                        //


                        //bo.CompanyId = null;
                        //bo.AdvanceBookingId = null;
                        //bo.MasterJobId = null;
                        //bo.ZoneId = null;
                        //bo.DropOffZoneId = null;
                        //bo.DriverId = null;
                        //bo.ReturnDriverId = null;
                        //bo.VehicleTypeId = null;
                        //bo.FleetMasterId = null;
                        //bo.OnlineBookingId = e.JobId;

                        //bo.BookingTypeId = 100;
                        //bo.BookingDate = DateTime.Now;
                        //bo.CustomerId = null;
                        //bo.BookingNo = "JP-" + e.PoolJobId;
                        //bo.SubcompanyId = AppVariables.objSubCompany.Id;

                        objBooking.FromAddress = bo.FromAddress.ToStr();
                        objBooking.ToAddress = bo.ToAddress.ToStr();
                        objBooking.FromLocTypeId = bo.FromLocTypeId;
                        objBooking.ToLocTypeId = bo.ToLocTypeId;
                        objBooking.PickupDateTime = bo.PickupDateTime;
                        objBooking.SpecialRequirements = bo.SpecialRequirements.ToStr();
                        objBooking.CustomerName = bo.CustomerName.ToStr();
                        objBooking.CustomerEmail = bo.CustomerEmail.ToStr();
                        objBooking.JourneyTypeId = bo.JourneyTypeId;
                        objBooking.CustomerMobileNo = bo.CustomerMobileNo;
                        objBooking.CustomerPhoneNo = bo.CustomerPhoneNo;
                        objBooking.FareRate = bo.FareRate.ToDecimal();
                        objBooking.FromDoorNo = bo.FromDoorNo.ToStr();
                        objBooking.ViaString = bo.ViaString.ToStr();
                        objBooking.TotalCharges = bo.TotalCharges.ToDecimal();
                        objBooking.FromStreet = bo.FromStreet.ToStr();
                        objBooking.ToDoorNo = bo.ToDoorNo.ToStr();
                        objBooking.ToStreet = bo.ToStreet.ToStr();
                        objBooking.MeetAndGreetCharges = bo.MeetAndGreetCharges.ToDecimal();
                        objBooking.CongtionCharges = bo.CongtionCharges.ToDecimal();
                        objBooking.ExtraDropCharges = bo.ExtraDropCharges.ToDecimal();

                        objBooking.ZoneId = null;
                        objBooking.DropOffZoneId = null;


                        if (objBooking.FromLocTypeId.ToInt() == 1)
                        {
                            if (objBooking.FromLocId.ToInt() > 0)
                            {
                                if (db.Gen_Locations.Count(c => c.Id == bo.FromLocId) == 0)
                                {
                                    objBooking.FromLocId = null;

                                    objBooking.FromLocTypeId = 7;
                                }
                            }
                        }

                        if (objBooking.ToLocTypeId.ToInt() == 1)
                        {
                            if (objBooking.ToLocId.ToInt() > 0)
                            {
                                if (db.Gen_Locations.Count(c => c.Id == bo.ToLocId) == 0)
                                {
                                    objBooking.ToLocId = null;

                                    objBooking.ToLocTypeId = 7;
                                }
                            }
                        }


                        if (objBooking.FromLocTypeId.ToInt() != 1)
                        {
                            objBooking.FromLocTypeId = 7;
                            objBooking.FromLocId = null;
                        }

                        if (objBooking.ToLocTypeId.ToInt() != 1)
                        {
                            objBooking.ToLocTypeId = 7;
                            objBooking.ToLocId = null;
                        }


                        if (vehicleId > 0)
                        {
                            objBooking.VehicleTypeId = vehicleId;
                        }
                        else
                        {
                            objBooking.SpecialRequirements = "Vehicle Type:+" + bo.BoundType + " , " + bo.SpecialRequirements.ToStr();

                        }


                        bo.Booking_Logs.Add(new Booking_Log { User = "JOB POOL", UpdateDate = DateTime.Now, AfterUpdate = "Job is Updated by Company :" + e.JobProviderLicenseNo.ToStr() });



                        try
                        {
                            db.SubmitChanges();

                            BroadCast("**internalmessage>>" + "request updateJob" + ">>" + objBooking.Id + ">>" + "Job " + objBooking.BookingNo + " is updated>>" + objBooking.CustomerName.ToStr() + ">>" + objBooking.FromAddress.ToStr() + ">>" + string.Format("{0:dd/MM/yyyy HH:mm}", objBooking.PickupDateTime) + ">>" + "This booking is updated from Company :" + e.JobProviderLicenseNo.ToStr() + ">>1");

                            LogUtility.LogInfo("updatejob", "SignRDetailEvent", "SignRDetailEvent");
                        }
                        catch (Exception ex)
                        {
                            LogUtility.LogInfo(ex.Message + ",json:" + e.BookingJson.ToStr(), "updatejob_error", "updatejob_error");

                        }

                    }

                    //Taxi_Model.Booking objBooking = General.GetObject<Taxi_Model.Booking>(c => c.Id == values[1].ToLong());
                    //
                    //if (objBooking != null)
                    //{
                    //    General.BroadCastMessage("**internalmessage>>" + "request updateJob" + ">>" + objBooking.Id + ">>" + "Job " + objBooking.BookingNo + " is updated by Customer" + ">>" + objBooking.CustomerName.ToStr() + ">>" + values[3].ToStr() + ">>" + string.Format("{0:dd/MM/yyyy HH:mm}", objBooking.PickupDateTime) + ">>" + "This booking is updated from " + values[2].ToStr());

                    //    Byte[] byteResponse = Encoding.UTF8.GetBytes("true");
                    //    tcpClient.GetStream().Write(byteResponse, 0, byteResponse.Length);

                    //    long bookingId = objBooking.Id;

                    //    if (values.Count() <= 3)
                    //    {
                    //        using (TaxiDataContext db = new TaxiDataContext())
                    //        {

                    //            db.stp_CancelBooking(bookingId, "Job is cancelled by Customer (From App)", "Customer");
                    //        }
                    //    }
                    //}



                    //  ShowMessage("Job Assigned", "Job has been Assigned to " + e.JobAcceptorClientName, ToolTipIcon.Info);
                }
                else if (e.EventType == CabTreasureJobPoolGateway.Models.EventType.CancelJob)
                {
                    using (TaxiDataContext db = new TaxiDataContext())
                    {

                        var objBooking = db.Bookings.Where(c => c.OnlineBookingId == e.JobId).FirstOrDefault();
                        BroadCast("**internalmessage>>" + "request canceljob" + ">>" + objBooking.Id + ">>" + "Job " + objBooking.BookingNo + " is cancelled>>" + objBooking.CustomerName.ToStr() + ">>" + objBooking.FromAddress.ToStr() + ">>" + objBooking.ToAddress.ToStr() + ">>" + string.Format("{0:dd/MM/yyyy HH:mm}", objBooking.PickupDateTime) + ">>" + "This booking is cancelled from Company :" + e.JobProviderLicenseNo.ToStr() + ">>1");



                        long bookingId = objBooking.Id;
                        db.stp_CancelBooking(bookingId, "Job is cancelled by Company :" + e.JobProviderLicenseNo.ToStr(), "Job Pool");

                    }


                }


                else if (e.EventType == CabTreasureJobPoolGateway.Models.EventType.TransferredJob)
                {
                    using (var db = new TaxiDataContext())
                    {
                        var objJob = db.Bookings.FirstOrDefault(m => m.Id == e.JobId);
                        objJob.JobOfferDateTime = DateTime.Now;
                        objJob.CompanyCreditCardDetails = e.JobAcceptorLicenseNo.ToStr();
                        objJob.CustomerCreditCardDetails = e.JobAcceptorConnectionString;
                        objJob.BookingStatusId = 21;
                        objJob.Booking_Logs.Add(new Booking_Log { BeforeUpdate = "", AfterUpdate = "Job is accepted by Company : " + e.JobAcceptorLicenseNo.ToStr(), UpdateDate = DateTime.Now, BookingId = objJob.Id, User = "JOB POOL", });
                        db.SubmitChanges();
                    }
                    //
                    BroadCast("**internalmessage>>" + "request updatepooljobstatus>>accpeted>>" + e.JobAcceptorClientName + ">>Job is Accepted by " + e.JobAcceptorLicenseNo);

                    // ShowMessage("Job Transfered", "Job has been accepted By " + e.JobAcceptorClientName, ToolTipIcon.Info);

                }
                else if (e.EventType == CabTreasureJobPoolGateway.Models.EventType.OfferedJob)
                {
                    // ShowMessage("Job Offered", "New Job Received on Pool." , ToolTipIcon.Info);
                    //  ShowMessage("Job Offered", "New Job Received on Pool." + Environment.NewLine + "Pickup : " + e.PickupLocation.ToStr(), ToolTipIcon.Info);

                    BroadCast("**internalmessage>>" + "request jobpool>>" + e.JobId + ">>" + msg);
                }
                else if (e.EventType == CabTreasureJobPoolGateway.Models.EventType.SendBackToOriginator)
                {
                    // ShowMessage("Job Offered", "New Job Received on Pool." , ToolTipIcon.Info);
                    //  ShowMessage("Job Offered", "New Job Received on Pool." + Environment.NewLine + "Pickup : " + e.PickupLocation.ToStr(), ToolTipIcon.Info);
                    string query2 = "insert into booking_log(bookingid,[user],beforeupdate,afterupdate,updatedate)values(" + e.JobId + ",'" + "JOB POOL" + "','','" + "Job Send Back to Pool by Company :" + e.JobAcceptorLicenseNo.ToStr() + "',getdate())";

                    using (TaxiDataContext db = new TaxiDataContext())
                    {

                        db.stp_RunProcedure("update Booking set BookingStatusId=25,NotesString='',CompanyCreditCardDetails='' where Id=" + e.JobId.ToLong() + ";" + query2);

                    }
                    BroadCast("**internalmessage>>" + "request jobsendbackpool>>" + e.JobId + ">>" + "Job has been send back to Job Pool>>1");
                    LogUtility.LogInfo("sendback jobid:" + e.JobId.ToStr(), "SignRDetailEvent", "SignRDetailEvent");


                }
            }
            catch (Exception exe)
            {
                LogUtility.LogException(exe, "SignRDetailEvent", "SignRDetailEventError");
            }
        }


        private void BroadCast(string message)
        {

            try
            {
                List<string> listOfConnections = new List<string>();
                listOfConnections = HubProcessor.Instance.ReturnDesktopConnections();
                HubProcessor.Instance.Clients.Clients(listOfConnections).cMessageToDesktop(message);
            }

            catch
            {

            }

        }
        private string GetMessage(string message, Booking objBooking, long jobId, CabTreasureJobPoolGateway.Models.ClsDriverDetails objDriver)
        {
            try
            {


                if (listofsmstags == null)
                    listofsmstags = new TaxiDataContext().SMSTags.ToList();

                string msg = message;

                object propertyValue = string.Empty;
                foreach (var tag in listofsmstags.Where(c => msg.Contains(c.TagMemberValue)))
                {


                    switch (tag.TagObjectName)
                    {
                        case "booking":


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


                            if (tag.TagPropertyValue.ToStr().Contains("VehicleNo"))
                            {
                                propertyValue = objDriver.vehicleNo.ToStr();

                            }
                            else if (tag.TagPropertyValue.ToStr().Contains("VehicleModel"))
                            {
                                propertyValue = objDriver.vehicleModel.ToStr();

                            }
                            else if (tag.TagPropertyValue.ToStr().Contains("VehicleColor"))
                            {
                                propertyValue = objDriver.vehicleColor.ToStr();

                            }
                            else if (tag.TagPropertyValue.ToStr().Contains("DriverName"))
                            {
                                propertyValue = objDriver.driverName.ToStr();

                            }
                            else if (tag.TagPropertyValue.ToStr().Contains("VehicleMake"))
                            {
                                propertyValue = objDriver.vehicleMake.ToStr();

                            }
                            else if (tag.TagPropertyValue.ToStr().Contains("DriverNo"))
                            {
                                propertyValue = objDriver.driverNo.ToStr();

                            }
                            else if (tag.TagPropertyValue.ToStr().Contains("MobileNo"))
                            {
                                propertyValue = objDriver.mobileNo.ToStr();

                            }

                            //VehicleNo
                            //VehicleModel
                            //VehicleColor
                            //DriverName
                            //VehicleMake
                            //DriverNo
                            //MobileNo



                            //propertyValue = objBooking.Fleet_Driver.DefaultIfEmpty().GetType().GetProperty(tag.TagPropertyValue).GetValue(objBooking.Fleet_Driver.DefaultIfEmpty(), null);


                            break;


                        case "Fleet_Driver_Image":






                            if (!string.IsNullOrEmpty(tag.TagPropertyValue.ToStr()) && !string.IsNullOrEmpty(tag.TagPropertyValue2))
                            {
                                if (objDriver.photoLinkId.ToStr().Trim().Length > 0)
                                {
                                    string linkId = objDriver.photoLinkId.ToStr().Trim();

                                    if (linkId.ToStr().Length == 0)
                                        propertyValue = " ";
                                    else
                                    {
                                        //
                                        // propertyValue = "http://tradrv.co.uk/drv.aspx?ref=" + objBooking.BookingNo.ToStr() + ":" + linkId;
                                        if (tag.TagMemberValue.ToStr().Trim().ToLower() == "<trackdrv>")
                                        {
                                            //  string encrypt = Cryptography.Encrypt(objDriver.BookingNo.ToStr() + ":" + linkId + ":" + objDriver.ListenerDetails.ToStr().Trim() + ":" + objDriver.BookingId.ToStr(), "softeuroconnskey", true);
                                            //  propertyValue = "http://tradrv.co.uk/tck.aspx?q=" + encrypt;

                                            propertyValue = objDriver.ListenerDetails.ToStr();
                                            //
                                        }
                                        else
                                        {

                                            propertyValue = "http://tradrv.co.uk/drv.aspx?ref=" + objDriver.BookingNo.ToStr() + ":" + linkId;
                                        }
                                    }
                                }
                                else
                                    propertyValue = " ";


                                //      propertyValue = objBooking.Fleet_Driver.DefaultIfEmpty().GetType().GetProperty(tag.TagPropertyValue2).GetValue(objBooking.Fleet_Driver.DefaultIfEmpty(), null);
                            }
                            break;


                        case "Fleet_Driver_Documents":



                            if (!string.IsNullOrEmpty(tag.TagPropertyValue.ToStr()) && !string.IsNullOrEmpty(tag.TagPropertyValue2))
                            {

                                if (tag.TagPropertyValue.Contains("PHC Vehicle"))
                                {
                                    propertyValue = objDriver.vehicleBadge.ToStr();


                                }
                                else if (tag.TagPropertyValue.Contains("PHC Driver"))
                                {
                                    propertyValue = objDriver.driverBadge.ToStr();


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


                return msg.Replace("\n\n", "\n");
            }
            catch (Exception ex)
            {


                // ENUtils.ShowMessage(ex.Message);
                return "";
            }
        }

        public static string ToTinyURLS(string txt)
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


        private void SendSMS(string mobileNo, string Message, int smsType)
        {

            try
            {

                string rtnMsg = string.Empty;

                string mobNo = mobileNo;

                if (Message.Length > 450)
                {
                    Message = Message.Substring(0, 447);
                    Message += "...";
                }


                Global.SendSMS(mobileNo, Message);

            }
            catch
            {

            }

        }



        #endregion

    }
}
