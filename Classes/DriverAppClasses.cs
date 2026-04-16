using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRHub
{
    public class Trip
    {
        public long TripId;
        public string TripNo;
        public bool followSequence;
        public int DriverId;
        public List<Jobs> jobs;
        public int TripStatusId;
        public string Message;
    }
    public class Jobs
    {
        public string PickupDateTime;
        public string Cust;
        public int Passengers;
        public string Pickup;
        public string Destination;
        public string BookingNo;
        public string Special;
        public string Payment;
        public string Journey;
        public decimal Fares;
        public string Account;
        public string Vehicle;
        public string Lug;
        public string BookingType;
        public string JobId;
        public string SubCompanyId;
        public int Did;
        public bool ShowFares;
        public bool HideAccountName;
        public int JStatus;

    }


    public class AppJobStatus
    {

        public static int ACCEPT = 4;
        public static int ONROUTE = 5;
        public static int ARRIVED = 6;
        public static int POB = 7;
        public static int STC = 8;
        public static int NOPICKUP = 13;
        public static int NOSHOW = 10;
        public static int CLEAR = 2;
        public static int REJECT = 11;



    }
    public class DriverProfile
    {
        public string Image;
        public string DriverNo;
        public string DriverName;
        public decimal Rating;

        public string Address;
        public int WorkingSince;
        public string WorkingSinceUnit;
        public int Bookings;

        public string Mobile;
        public string VehicleNo;
        public string VehicleMake;
        public string VehicleColor;
        public string VehicleModel;
        public string VehicleType;
        public int VehicleTypeId;


    }
    public class clsupdateversion
    {

        public decimal currVer;
        public bool isUpdate;
        public int priority;
    }


    public class ClsCCDetails
    {

        public string customerName { get; set; }
        public string cardNumber { get; set; }
        public string cardExpiryMM { get; set; }
        public string cardExpiryYY { get; set; }

        public string customerAddress { get; set; }

        public string customerPostcode { get; set; }

        public bool? IsDefault { get; set; }


        public int RecordId { get; set; }

        public long Id { get; set; }

        public string CCDetails { get; set; }


        public string url { get; set; }
        public string message { get; set; }
    }

  

   

    public class sendPaymentLinkReq
    {
        public string version { get; set; }
        public string JobId { get; set; }
        public string mobNo { get; set; }
        public decimal? TotalFares { get; set; }
        public string DrvId { get; set; }
        public string DrvNo { get; set; }

    }


  

    public class BookingDetail
    {
        public long jobId;
        public int metertype;
        public FareMeterSettings objMeter;


    }

    public class DriverDetail
    {

        public string JobId;
        public string DrvId;
        public string JStatus;
        public string DrvNo;
        public string Password;

        public string version;
        public string VehicleNo;
        public string DeviceId;

        public string EnableOnlineStatus;
        public string DStatus;
        public string EnableDriverChecklist;
    }

    public class MakePaymentRequestViewModel
    {
        public int driverId { get; set; }
        public int jobId { get; set; }
        public String selectedGateway { get; set; }
        public decimal Fare { get; set; }
    }
    public class UpdateCustomPaymentRequestModel
    {
        public string JobId { get; set; }
        public string driverId { get; set; }
        public string version { get; set; }
        public List<MakePaymentCustomList> customList { get; set; }
    }
    public class MakePaymentResponseViewModel
    {
        public int ResponseType { get; set; }
        public bool IsTransactionSuccess { get; set; }
        public string TransactionMessage { get; set; }
        public string TransactionId { get; set; }
        public string TransactionUrl { get; set; }
        public string KonnectAccId { get; set; }
        public List<dynamic> Gateways { get; set; }
        public string isKonnectPayEnable { get; set; }
        public List<MakePaymentCustomList> customList { get; set; }
    }

    public class MakePaymentCustomList
    {
        public string label { get; set; }
        public string value { get; set; }
        public string fieldname { get; set; }
        public string ismultiline { get; set; }
        public bool isvisible { get; set; }
        public bool isedit { get; set; }
    }

    public class ClsPlotDriverDetails
    {

        public int TotalAvailableDrivers;
        public string VehicleType;

    }


    public class ClsPlotFullDetails
    {

        public List<ClsPlotDetails> plotDetails;
        public List<ClsPlotDriverDetails> driverDetails;

    }
    public class CheckListSubmission
    {
        public int Id { get; set; }
        public int DriverId { get; set; }
        public int VehicleId { get; set; }
        public DateTime SubmittedDate { get; set; }
        public bool IsCompleted { get; set; }
    }
    public class DriverChecklist
    {
        public int CheckListHeaderID { get; set; }
        public int CheckListDetailID { get; set; }
        public int DriverId { get; set; }
        public string VehicleNo { get; set; }
        public string ImagePath { get; set; }
        public string comment { get; set; }
        public int isSuccess { get; set; }
        public string strDate { get; set; }
        public string imgUrl { get; set; }
    }
    public class ResponseData
    {
        public bool IsSuccess;
        public string Message;
        public string Data;
    }


    public class SendDataRequest
    {
        public string LatLong;
        public string MeterString;
        public string PlotBidding;
        public string GpsError;
    }




    public class BookingSummary
    {
        public string label;
        public decimal value;
        public string fieldname;
        public bool isvisible;
        public bool isedit;
        public string DisableChangePayment;



    }

    public class ChargesSummary
    {
        public string label;
        public string value;
        public string fieldname;
        public bool isvisible;
        public bool isedit;



    }

    public class SearchAddressInfo
    {

        public string JobId;
        public string DrvId;


        public string DrvNo;

        public string version;
        public string keyword;
        public double? Latitude;
        public double? Longitude;
    }

    public partial class stp_GetByRoadLevelDataForAppsResult
    {

        private string _Address;

        private System.Nullable<double> _Latitude;

        private System.Nullable<double> _Longitude;


        public string LocationType;

        public stp_GetByRoadLevelDataForAppsResult()
        {
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Address", DbType = "VarChar(200)")]
        public string Address
        {
            get
            {
                return this._Address;
            }
            set
            {
                if ((this._Address != value))
                {
                    this._Address = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Latitude", DbType = "Float")]
        public System.Nullable<double> Latitude
        {
            get
            {
                return this._Latitude;
            }
            set
            {
                if ((this._Latitude != value))
                {
                    this._Latitude = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Longitude", DbType = "Float")]
        public System.Nullable<double> Longitude
        {
            get
            {
                return this._Longitude;
            }
            set
            {
                if ((this._Longitude != value))
                {
                    this._Longitude = value;
                }
            }
        }
    }

}