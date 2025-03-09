using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using Taxi_BLL;
using Taxi_Model;
using Utils;

namespace SignalRHub
{
    public class CallSupplierApi
    {
        public static string UpdateStatus(long jobId, int jobStatusId)
        {
            try
            {


                if (jobStatusId == Enums.BOOKINGSTATUS.ONROUTE || jobStatusId == Enums.BOOKINGSTATUS.ARRIVED || jobStatusId == Enums.BOOKINGSTATUS.POB
                    || jobStatusId == Enums.BOOKINGSTATUS.NOPICKUP || jobStatusId == Enums.BOOKINGSTATUS.CANCELLED || jobStatusId == Enums.BOOKINGSTATUS.NOSHOW
                    || jobStatusId == Enums.BOOKINGSTATUS.DISPATCHED
                    || jobStatusId == -1)
                {

                    var objDriverDetails = new TaxiDataContext().ExecuteQuery<GettAPICall.stp_getbookingdriverdetails>("EXEC stp_getbookingdriverdetails {0},{1}", jobId, 0).FirstOrDefault();

                    //  GettAPICall.stp_getbookingdriverdetails objDriverDetails = new TaxiDataContext().stp_getbookingdriverdetails( jobId, 0).FirstOrDefault();
                    //

                    if (objDriverDetails != null && objDriverDetails.BookingTypeId.ToInt() == 11 && objDriverDetails.CompanyId != null)
                    {
                        //




                        GettAPICall.Apicalling api = new GettAPICall.Apicalling();
                        GettAPICall.UpdatebookingstatustoSupplier objUpdate = new GettAPICall.UpdatebookingstatustoSupplier();
                        objUpdate.SupplierID = objDriverDetails.SupplierId.ToStr();
                        objUpdate.jobidentifier = objDriverDetails.OrderNo.ToStr();
                        objUpdate.ctbookingid = objDriverDetails.BookingId.ToStr();
                        objUpdate.eventname = objDriverDetails.EventName.ToStr();


                        if (objUpdate.eventname.ToStr().Trim().Length == 0 && jobStatusId == Enums.BOOKINGSTATUS.NOSHOW)
                            objUpdate.eventname = "recovered";


                        try
                        {
                            System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\callsupplierapidata.txt", DateTime.Now.ToStr() + "request:" + jobId + ",status:" + jobStatusId + Environment.NewLine);
                        }
                        catch
                        {

                        }

                        if (objDriverDetails != null)
                        {
                            objUpdate.driver_location = new GettAPICall.driver_location();
                            objUpdate.driver_location.lat = Convert.ToDouble(objDriverDetails.Latitude);
                            objUpdate.driver_location.lng = Convert.ToDouble(objDriverDetails.Longitude);
                            objUpdate.vehicleDetail = new GettAPICall.VehicleDetail();
                            objUpdate.vehicleDetail.DriverName = objDriverDetails.DriverName;
                            objUpdate.vehicleDetail.VehicleColor = objDriverDetails.VehicleColor;
                            objUpdate.vehicleDetail.VehicleMake = objDriverDetails.VehicleMake;
                            objUpdate.vehicleDetail.VehicleModel = objDriverDetails.VehicleModel;
                            objUpdate.vehicleDetail.VehicleNumber = objDriverDetails.VehicleNo;
                            objUpdate.vehicleDetail.Phonenumber = objDriverDetails.PhoneNumber.ToStr();
                            objUpdate.vehicleDetail.BadgeNumber = objDriverDetails.PHCDriver.ToStr();
                            objUpdate.vehicleDetail.DriverID = objDriverDetails.DriverNo.ToStr();

                        }
                        string json = new JavaScriptSerializer().Serialize(objUpdate);
                        GettAPICall.Response<string> objResponse = api.BookingstatusUpdate(json);
                        try
                        {
                            System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\callsupplierapidata.txt", DateTime.Now.ToStr() + "request:" + json + "jobid:" + jobId + ",status:" + jobStatusId + Environment.NewLine);
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
                        System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\callsupplierapidata_else.txt", DateTime.Now.ToStr() + "jobid:" + jobId + ",status:" + jobStatusId + Environment.NewLine);
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
                    System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\callsupplierapi_exception.txt", DateTime.Now.ToStr() + "request:" + jobId + ",ststus:" + jobStatusId + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
            }
            return "";

        }
    }
}