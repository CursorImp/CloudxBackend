using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Taxi_Model;

namespace SignalRHub.WebApiClasses
{
    #region google search model
    public class Prediction
    {
        public string description { get; set; }
        public List<MatchedSubstring> matched_substrings { get; set; }
        public string place_id { get; set; }
        public string reference { get; set; }
        public StructuredFormatting structured_formatting { get; set; }
        public List<Term> terms { get; set; }
        public List<string> types { get; set; }
    }
    public class GoogleAutoCompleteRoot
    {
        public List<Prediction> predictions { get; set; }
        public string status { get; set; }
    }

    public class MatchedSubstring
    {
        public int length { get; set; }
        public int offset { get; set; }
    }
    public class StructuredFormatting
    {
        public string main_text { get; set; }
        public List<MainTextMatchedSubstring> main_text_matched_substrings { get; set; }
        public string secondary_text { get; set; }
    }
    public class Term
    {
        public int offset { get; set; }
        public string value { get; set; }
    }
    public class MainTextMatchedSubstring
    {
        public int length { get; set; }
        public int offset { get; set; }
    }
    #endregion
    public class ClsQuotationBooking
    {
        public long BookingId { get; set; }
        public string BookingNo { get; set; }

        public int? BookingStatusId { get; set; }
        public int? BookingTypeId;

        public string BookingType;
        public string UserName { get; set; }
        public bool IsQuotation { get; set; }

    }
    public class AppSetting
    {
        public long Id { get; set; }
        public string SetKey { get; set; }
        public string SetVal { get; set; }
        public string description { get; set; }
        public bool IsLogin { get;  set; }
    }
    public class ClsOnlineBooking
    {
        public long Id { get; set; }
        public string BookingNo { get; set; }

        public DateTime? BookingDate;
        public string BookingDateString;
        public DateTime? PickupDateTime;
        public string PickupDateString;
        public string PickupTimeString;
        public string CustomerName;
        public string CustomerMobileNo;
        public string CustomerPhoneNo;
        public string CustomerEmail;
        public string FromAddress;
        public string FromDoorNo;
        public string FromStreet;
        public string ToAddress;
        public string ToDoorNo;
        public string ToStreet;
        public int? BookingStatusId { get; set; }
        public int? BookingTypeId;
        public string CompanyName;
        public string VehicleType;
        public string ViaString;
        public string PaymentType;
        public decimal? FareRate;
        public decimal? Parking;
        public decimal? Waiting;
        public decimal? Extra;
        public decimal? CompanyPrice;
        public string SpecialRequirements;
        public string FlightNumber;
        public string PaymentComments;
        public string BookingType;
        public string OrderNo { get; set; }
        public string UserName { get; set; }
    }
    public class Zones
    {
    }
    public class Gen_Zones
    {
        public int? Id { get; set; }
        public string ZoneName { get; set; }
        public DateTime? AddOn { get; set; }
        public int? AddBy { get; set; }
        public DateTime? EditOn { get; set; }
        public int? EditBy { get; set; }
        public string PostCode { get; set; }
        public int? OrderNo { get; set; }
        public string ShapeCategory { get; set; }
        public string ShapeType { get; set; }
        public int? Linewidth { get; set; }
        public int? Lineforecolor { get; set; }
        public double? MinLatitude { get; set; }
        public double? MaxLatitude { get; set; }
        public double? MinLongitude { get; set; }
        public double? MaxLongitude { get; set; }
        public string ShortName { get; set; }
        public bool? EnableAutoDespatch { get; set; }
        public bool? EnableBidding { get; set; }
        public int? BiddingRadius { get; set; }
        public int? ZoneTypeId { get; set; }
        public int? PlotKind { get; set; }
        public bool? DisableDriverRank { get; set; }
        public DateTime? JobDueTime { get; set; }
        public List<Gen_Zone_PolyVertices> Gen_Zone_PolyVertices { get; set; }
    }
    public class Gen_Zone_PolyVertices
    {
        public long Id { get; set; }
        public string PostCode { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public string Latitude1 { get; set; }
        public string Longitude1 { get; set; }
        public int ZoneId { get; set; }
        public double? Diameter { get; set; }
        public string Diameter1 { get; set; }
    }
    public class ZonesMaster
    {
        public int? Id { get; set; }
        public string ZoneName { get; set; }
        public DateTime? AddOn { get; set; }
        public int? AddBy { get; set; }
        public DateTime? EditOn { get; set; }
        public int? EditBy { get; set; }
        public string PostCode { get; set; }
        public int? OrderNo { get; set; }
        public string ShapeCategory { get; set; }
        public string ShapeType { get; set; }
        public int? Linewidth { get; set; }
        public int? Lineforecolor { get; set; }
        public double? MinLatitude { get; set; }
        public double? MaxLatitude { get; set; }
        public double? MinLongitude { get; set; }
        public double? MaxLongitude { get; set; }
        public string ShortName { get; set; }
        public bool? EnableAutoDespatch { get; set; }
        public bool? EnableBidding { get; set; }
        public int? BiddingRadius { get; set; }
        public int? ZoneTypeId { get; set; }
        public int? PlotKind { get; set; }
        public bool? DisableDriverRank { get; set; }
        public DateTime? JobDueTime { get; set; }
    }


    public class ZoneMasterDetailList
    {
        public List<ZonesMaster> ZoneMaster { get; set; }
        public List<Gen_Zone_PolyVertices> ZoneDetail { get; set; }
    }


    public class CustomerHistoryModel
    {

        public string CustomerName { get; set; }

        public string Address { get; set; }
        public string Email { get; set; }

        public string DoorNo { get; set; }

        public int? Cancelled { get; set; }
        public int? Used { get; set; }
        public int? NoFares { get; set; }

        public bool? IsBlackListed { get; set; }
        public string BlackListReason { get; set; }
        public int? SubCompanyId { get; set; }
        public string SubCompanyName { get; set; }
        public string ExcludedDriverIds { get; set; }

        public int? AccountId { get; set; }

        public bool? IsAccount { get; set; }

        public string Notes { get; set; }

        public List<stp_getcustomerhistoryResultEx> HistoryList { get; set; }
        public List<WaitingCurrentHistoryList> WaitingList { get; set; }
        public List<WaitingCurrentHistoryList> OnRoute { get; set; }
    }

    public class WaitingCurrentHistoryList
    {
        public string BookingNo { get; set; }
        public DateTime PickupDateTime { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
    }

    public class clsAuditTrial
    {
        public List<Vu_BookingLog> bookingLog { get; set; }
        public List<vw_BookingUpdate> bookingUpdates { get; set; }

        public Gen_SubCompany objSubCompany { get; set; }

        public string phcdriver { get; set; }
        public string phcVehicle { get; set; }
        public string vehicleDetails { get; set; }

    }

    public class stp_getcustomerhistoryResultEx
    {
        public stp_getcustomerhistoryResultEx() { }



        public DateTime? PickupDate { get; set; }

        public string ToDoorNo { get; set; }

        public string FromDoorNo { get; set; }

        public int VehicleTypeId { get; set; }

        public int? Companyid { get; set; }



        public decimal? Fare { get; set; }

        public int? ToId { get; set; }

        public string To { get; set; }

        public string ViaString { get; set; }

        public string From { get; set; }

        public int? FromId { get; set; }

        public int? FromTypeId { get; set; }

        public int? ToTypeId { get; set; }

        public int? CancelledJobCount { get; set; }
        public int? Completed { get; set; }
        public int? NoPickupJobCount { get; set; }


    }

    public class stp_searchbookings
    {
        public long Id { get; set; }
        public string RefNumber { get; set; }
        public DateTime? BookingDate { get; set; }
        public DateTime? PickupDate { get; set; }
        public string Passenger { get; set; }

        public string Acc { get; set; }
        public string OrderNo { get; set; }

        public string From { get; set; }            //From = a.FromLocTypeId == Enums.LOCATION_TYPES.ADDRESS || a.FromLocTypeId == Enums.LOCATION_TYPES.BASE ? a.FromAddress : a.FromLocTypeId == Enums.LOCATION_TYPES.POSTCODE ? a.FromPostCode : a.Gen_Location.LocationName,
        public string To { get; set; }             //To = a.ToLocTypeId == Enums.LOCATION_TYPES.ADDRESS || a.ToLocTypeId == Enums.LOCATION_TYPES.BASE ? a.ToAddress : a.ToLocTypeId == Enums.LOCATION_TYPES.POSTCODE ? a.ToPostCode : a.Gen_Location1.LocationName,

        public decimal? Fare { get; set; }
        public string Driver { get; set; }
        public string Vehicle { get; set; }
        public string MobileNo { get; set; }
        public string Status { get; set; }
        public string StatusTextColor { get; set; }
        public string PaymentRef { get; set; }
        public string PaymentType { get; set; }

        public int searchDateType { get; set; }
        public string Email { get; set; }
        public string Via { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? TillDate { get; set; }

        public string PhoneNo { get; set; }


        public int? BookingStatusId { get; set; }
        public int? VehicleTypeId { get; set; }
        public int? PaymentTypeId { get; set; }

        public int? DriverId { get; set; }
        public int? CompanyId { get; set; }
        public int? BookingTypeId { get; set; }
    }

    public class ClsBooking_ViaLocation
    {
        public ClsBooking_ViaLocation() { }

        public long Id { get; set; }
        public long BookingId { get; set; }
        public int? ViaLocTypeId { get; set; }
        public string ViaLocTypeLabel { get; set; }
        public string ViaLocTypeValue { get; set; }
        public int? ViaLocId { get; set; }
        public string ViaLocValue { get; set; }
        public string ViaLocLabel { get; set; }

    }

    public class MultiInfo
    {
        public int Noofweeks { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Days { get; set; }

    }





    public class ClsMapReport
    {
        public PartialBookingInfo bookingInfo;
        public List<Booking_RoutePath> routePath;


    }

    public class PartialBookingInfo
    {
        public PartialBookingInfo()
        {

        }



        public List<ClsBooking_ViaLocation> Booking_ViaLocations { get; set; }






        public decimal? FareRate { get; set; }

        public string SpecialRequirements { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }

        public int? BookingStatusId { get; set; }

        public DateTime? PickupDateTime { get; set; }
        public long Id { get; set; }

        public int? VehicleTypeId { get; set; }
        public int? DriverId { get; set; }

        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhoneNo { get; set; }
        public string CustomerMobileNo { get; set; }
        public int? JourneyTypeId { get; set; }
        public string BookingNo { get; set; }


        public DateTime? AcceptedDateTime { get; set; }
        public DateTime? POBDateTime { get; set; }
        public DateTime? STCDateTime { get; set; }
        public DateTime? ClearedDateTime { get; set; }


        public decimal? TotalTravelledMiles { get; set; }
        public string DriverNo { get; set; }

        public DateTime? ArrivalDateTime { get; set; }

        public Booking_RoutePath AcceptedCoordinates { get; set; }
        public Booking_RoutePath ArrivedCoordinates { get; set; }
        public Booking_RoutePath POBCoordinates { get; set; }
        public Booking_RoutePath STCCoordinates { get; set; }
        public Booking_RoutePath ClearCoordinates { get; set; }



    }



    public class BookingInfo
    {
        public BookingInfo()
        {

        }
        public string PickupZoneName { get; set; }
        public string DestinationZoneName { get; set; }
        public string ReturnSpecialRequirements { get; set; }

        public MultiInfo objMulti { get; set; }
        public string AuthCode { get; set; }
        public int PaymentGatewayID { get; set; }
        public bool IsRefund { get; set; }
        public List<ClsBooking_ViaLocation> Booking_ViaLocations { get; set; }


        public bool? EnableFareMeter { get; set; }

        public DateTime? PriceBiddingExpiryDate { get; set; }

        public int? DriverWaitingMins { get; set; }

        public string JobCancelledBy { get; set; }

        public DateTime? JobCancelledOn { get; set; }

        public string CallRefNo { get; set; }

        public bool? IsReverse { get; set; }

        public string ViaString { get; set; }

        public string NotesString { get; set; }

        public long? TransferJobId { get; set; }

        public decimal? TransferJobCommission { get; set; }

        public int? PartyId { get; set; }

        public long? OnlineBookingId { get; set; }

        public decimal? ServiceCharges { get; set; }

        public bool? ApplyServiceCharges { get; set; }

        public string JobCode { get; set; }

        public decimal? ExtraPickup { get; set; }

        public decimal? ExtraDropOff { get; set; }

        public decimal? TipAmount { get; set; }

        public string CompanyCreditCardDetails { get; set; }

        public string CustomerCreditCardDetails { get; set; }

        public string OnHoldReason { get; set; }

        public DateTime? OnHoldDateTime { get; set; }

        public long? GroupJobId { get; set; }

        public string RoomNo { get; set; }

        public string FaresPostedFrom { get; set; }

        public long? AdvanceBookingId { get; set; }

        public string BoundType { get; set; }

        public string BookedBy { get; set; }

        public string BabySeats { get; set; }

        public string FromOther { get; set; }

        public string ToOther { get; set; }

        public decimal? JourneyTimeInMins { get; set; }

        public string EscortName { get; set; }

        public long? EscortId { get; set; }

        public decimal? EscortPrice { get; set; }

        public string PaymentComments { get; set; }

        public bool? DisableDriverCommissionTick { get; set; }

        public int? SecondaryPaymentTypeId { get; set; }

        public decimal? CashFares { get; set; }

        public bool? IsConfirmedDriver { get; set; }

        public decimal? DeadMileage { get; set; }

        public int? OnHoldWaitingMins { get; set; }

        public bool? IsProcessed { get; set; }

        public int? NoOfChilds { get; set; }

        public bool? IsQuotedPrice { get; set; }

        public string AttributeValues { get; set; }

        public Booking BookingReturn { get; set; }

        public int? SMSType { get; set; }

        public string ExcludedDriverIds { get; set; }

        public DateTime? ReAutoDespatchTime { get; set; }


        public bool? JobTakenByCompany { get; set; }

        public decimal? AgentCommission { get; set; }
        public decimal? FareRate { get; set; }
        public int? PaymentTypeId { get; set; }
        public string SpecialRequirements { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public string FromPostCode { get; set; }
        public string ToPostCode { get; set; }
        public string FromDoorNo { get; set; }
        public string ToDoorNo { get; set; }
        public string FromStreet { get; set; }
        public string ToStreet { get; set; }
        public string FromFlightNo { get; set; }
        public string FromComing { get; set; }
        public int? BookingStatusId { get; set; }
        public string DistanceString { get; set; }
        public bool? AutoDespatch { get; set; }
        public DateTime? AutoDespatchTime { get; set; }
        public DateTime? AddOn { get; set; }
        public int? AddBy { get; set; }
        public int? CompanyId { get; set; }
        public string AddLog { get; set; }
        public bool? IsCompanyWise { get; set; }
        public DateTime? PickupDateTime { get; set; }
        public long Id { get; set; }
        public int? FromLocTypeId { get; set; }
        public int? ToLocTypeId { get; set; }
        public int? FromLocId { get; set; }
        public int? ToLocId { get; set; }
        public int? VehicleTypeId { get; set; }
        public int? DriverId { get; set; }
        public int? ReturnDriverId { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhoneNo { get; set; }
        public string CustomerMobileNo { get; set; }
        public int? JourneyTypeId { get; set; }
        public string BookingNo { get; set; }
        public DateTime? BookingDate { get; set; }
        public int? NoofPassengers { get; set; }
        public int? NoofLuggages { get; set; }
        public int? NoofHandLuggages { get; set; }
        public DateTime? ReturnPickupDateTime { get; set; }
        public int? EditBy { get; set; }
        public DateTime? EditOn { get; set; }
        public int? CostCenterId { get; set; }
        public decimal? CashRate { get; set; }
        public decimal? AccountRate { get; set; }
        public decimal? WaitingMins { get; set; }
        public decimal? ExtraMile { get; set; }
        public DateTime? AcceptedDateTime { get; set; }
        public DateTime? POBDateTime { get; set; }
        public DateTime? STCDateTime { get; set; }
        public DateTime? ClearedDateTime { get; set; }
        public string EditLog { get; set; }
        public string CancelReason { get; set; }
        public decimal? CompanyPrice { get; set; }
        public int? InvoicePaymentTypeId { get; set; }
        public int? FleetMasterId { get; set; }
        public string Despatchby { get; set; }
        public int? ZoneId { get; set; }
        public int? DropOffZoneId { get; set; }
        public int? ReturnFromLocId { get; set; }
        public int? SubcompanyId { get; set; }
        public decimal? TotalTravelledMiles { get; set; }
        public bool? IsQuotation { get; set; }
        public bool? IsBidding { get; set; }
        public string DriverCommissionType { get; set; }
        public string OrderNo { get; set; }
        public string PupilNo { get; set; }
        public decimal? ParkingCharges { get; set; }
        public decimal? WaitingCharges { get; set; }
        public decimal? ExtraDropCharges { get; set; }
        public decimal? MeetAndGreetCharges { get; set; }
        public decimal? CongtionCharges { get; set; }
        public decimal? TotalCharges { get; set; }
        public long? DepartmentId { get; set; }
        public decimal? ReturnFareRate { get; set; }
        public DateTime? ArrivalDateTime { get; set; }
        public long? MasterJobId { get; set; }
        public bool? DisablePassengerSMS { get; set; }
        public bool? DisableDriverSMS { get; set; }
        public bool? IsCommissionWise { get; set; }
        public decimal? DriverCommission { get; set; }
        public DateTime? DespatchDateTime { get; set; }
        public DateTime? JobOfferDateTime { get; set; }
        public int? BookingTypeId { get; set; }
        public decimal? CustomerPrice { get; set; }
        public int? AgentCommissionPercent { get; set; }

        public string Userlog { get; set; }
        public string RecordingUrl { get; set; }
        public bool? IsFixNoOfHours { get; set; }
        public bool? IsFixedDriverCommission { get; set; }
        public int? DriverCommissionTypeId { get; set; }
        public decimal? DriverCommissionValue { get; set; }
        public decimal? DriverHours { get; set; }
    }

    public class Booking_DriverCommsiion
    {
        public bool? IsFixNoOfHours { get; set; }
        public bool? IsFixedDriverCommission { get; set; }
        public int? DriverCommissionTypeId { get; set; }
        public decimal? DriverCommissionValue { get; set; }
        public decimal? DriverHours { get; set; }
    }

    public class ClsLic
    {
        private string _DefaultClientID;

        public string DefaultClientID
        {
            get { return _DefaultClientID; }
            set { _DefaultClientID = value; }
        }



        private string _CabTrackUrl;
        private string _AppServiceUrl;

        public string CabTrackUrl
        {
            get { return _CabTrackUrl; }
            set { _CabTrackUrl = value; }
        }


        public string AppServiceUrl
        {
            get { return _AppServiceUrl; }
            set { _AppServiceUrl = value; }
        }


        private bool _IsValid;

        public bool IsValid
        {
            get { return _IsValid; }
            set { _IsValid = value; }
        }

        private string _OnlineDataString;

        public string OnlineDataString
        {
            get { return _OnlineDataString; }
            set { _OnlineDataString = value; }
        }
        private string _ExpiryDateTime;

        public string ExpiryDateTime
        {
            get { return _ExpiryDateTime; }
            set { _ExpiryDateTime = value; }
        }
        private string _OtherInformation1;

        public string OtherInformation1
        {
            get { return _OtherInformation1; }
            set { _OtherInformation1 = value; }
        }
        private string _OtherInformation2;

        public string OtherInformation2
        {
            get { return _OtherInformation2; }
            set { _OtherInformation2 = value; }
        }
        private string _OtherInformation3;

        public string OtherInformation3
        {
            get { return _OtherInformation3; }
            set { _OtherInformation3 = value; }
        }
        private string _Reason;

        public string Reason
        {
            get { return _Reason; }
            set { _Reason = value; }
        }

    }


    public class CustomJsonResult : JsonResult
    {
        private const string _dateFormat = "yyyy-MM-dd HH:mm:ss";

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            HttpResponseBase response = context.HttpContext.Response;

            if (!String.IsNullOrEmpty(ContentType))
            {
                response.ContentType = ContentType;
            }
            else
            {
                response.ContentType = "application/json";
            }
            if (ContentEncoding != null)
            {
                response.ContentEncoding = ContentEncoding;
            }
            if (Data != null)
            {
                // Using Json.NET serializer
                var isoConvert = new IsoDateTimeConverter();
                isoConvert.DateTimeFormat = _dateFormat;
                response.Write(JsonConvert.SerializeObject(Data, isoConvert));
            }
        }
    }


    public class clsTrackDriver
    {
        public clsTrackDriver()
        {

        }

        public AddressInfo PickupAddress { get; set; }
        public AddressInfo destinationAddress { get; set; }

        public string EstimatedTimeLeft { get; set; }

        public string Plateno { get; set; }

        public string backgroundcolor { get; set; }

        public string workstatus { get; set; }

        public int? driverworkstatusid { get; set; }

        public string MapIcon { get; set; }

        public string locationname { get; set; }

        public double speed { get; set; }
        public double longitude { get; set; }
        public double latitude { get; set; }
        public DateTime? plotdate { get; set; }
        public string driverno { get; set; }
        public int driverid { get; set; }
        public string ZoneName { get; set; }
        public int Id { get; set; }
        public DateTime UpdateDate { get; set; }
        public DateTime WaitSinceOn { get; set; }
    }


    public class WebAccountData
    {
        public string MACAddress { get; set; }
        public string CompanyId { get; set; }

        public string VerifiedToken { get; set; }
        public string CallingMethod { get; set; }


    }





    public class ClsDashboardModel
    {
        public List<stp_GetDashboardDriversResult> listofdrivers;

        public List<stp_GetDashboardDriversResult> listofwaitingdrivers;
        public List<stp_GetDashboardDriversResult> listofonboarddrivers;
        public List<stp_GetDashboardDriversResult> listofplotdrivers;

        public List<stp_GetBookingsDataResult> listofbookings;
        public List<stp_GetBookingsDataResult> listoftodaybookings;
        public List<stp_GetBookingsDataResult> listofprebookings;
        public List<stp_GetBookingsDataResult> listofrecentbookings;
        public BookingCounts objBookingCount;
        public DriverCounts objDriverCount;

    }

    public class DriverCounts
    {

        public int totalDrivers;
        public int totalAvailable;
        public int totalBusy;


        public int totalWaiting;
        public int totalBreak;
        public int totalOnRoute;
        public int totalOnArrived;
        public int totalPOB;
        public int totalSTC;
    }


    public class BookingCounts
    {

        public int totalToday;
        public int totalPre;
        public int totalRecent;
        public int totalNoPickup;
        public int totalCancelled;
        public int totalCompleted;

    }

    public partial class ClsDispatchFares
    {



        private int? _ID;

        private string _Name;

        private string _Image;

        private System.Nullable<int> _NoOfPassengers;

        private System.Nullable<int> _NoOfLuggages;

        private System.Nullable<int> _HandLuggages;

        private System.Nullable<decimal> _StartRate;

        private System.Nullable<decimal> _IncrementRate;

        private System.Nullable<decimal> _StartRateValidMiles;

        private string _Logo;

        private System.Nullable<int> _SortOrder;

        private System.Nullable<decimal> _Fare;

        private System.Nullable<decimal> _ReturnFare;
        private System.Nullable<decimal> _WaitAndReturnFare;

        private string _IsQuoted;

        private System.Nullable<decimal> _ExtraCharges;
        private System.Nullable<decimal> _AgentFee;
        private System.Nullable<decimal> _AgentCharge;


        private System.Nullable<decimal> _Congestion;
        private System.Nullable<decimal> _Parking;
        private System.Nullable<decimal> _Waiting;

        private System.Nullable<decimal> _JourneyMiles;


        public decimal? JourneyMiles
        {
            get { return _JourneyMiles; }
            set { _JourneyMiles = value; }
        }

        private System.Nullable<decimal> _CompanyPrice;

        public System.Nullable<decimal> CompanyPrice
        {
            get { return _CompanyPrice; }
            set { _CompanyPrice = value; }
        }

        private System.Nullable<decimal> _ReturnCompanyPrice;

        public System.Nullable<decimal> ReturnCompanyPrice
        {
            get { return _ReturnCompanyPrice; }
            set { _ReturnCompanyPrice = value; }
        }

        private System.Nullable<decimal> _BookingFee;

        public decimal? BookingFee
        {
            get { return _BookingFee; }
            set { _BookingFee = value; }
        }


        public string IsQuoted
        {
            get { return _IsQuoted; }
            set { _IsQuoted = value; }
        }
        private string _DisplayMessage;

        public string DisplayMessage
        {
            get { return _DisplayMessage; }
            set { _DisplayMessage = value; }
        }



        private string _PromotionDetails;

        public string PromotionDetails
        {
            get { return _PromotionDetails; }
            set { _PromotionDetails = value; }
        }

        public ClsDispatchFares()
        {
        }




        public int? ID
        {
            get
            {
                return this._ID;
            }
            set
            {
                if ((this._ID != value))
                {
                    this._ID = value;
                }
            }
        }


        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                if ((this._Name != value))
                {
                    this._Name = value;
                }
            }
        }


        public string Image
        {
            get
            {
                return this._Image;
            }
            set
            {
                if ((this._Image != value))
                {
                    this._Image = value;
                }
            }
        }


        public System.Nullable<int> NoOfPassengers
        {
            get
            {
                return this._NoOfPassengers;
            }
            set
            {
                if ((this._NoOfPassengers != value))
                {
                    this._NoOfPassengers = value;
                }
            }
        }


        public System.Nullable<int> NoOfLuggages
        {
            get
            {
                return this._NoOfLuggages;
            }
            set
            {
                if ((this._NoOfLuggages != value))
                {
                    this._NoOfLuggages = value;
                }
            }
        }


        public System.Nullable<int> HandLuggages
        {
            get
            {
                return this._HandLuggages;
            }
            set
            {
                if ((this._HandLuggages != value))
                {
                    this._HandLuggages = value;
                }
            }
        }


        public System.Nullable<decimal> StartRate
        {
            get
            {
                return this._StartRate;
            }
            set
            {
                if ((this._StartRate != value))
                {
                    this._StartRate = value;
                }
            }
        }


        public System.Nullable<decimal> IncrementRate
        {
            get
            {
                return this._IncrementRate;
            }
            set
            {
                if ((this._IncrementRate != value))
                {
                    this._IncrementRate = value;
                }
            }
        }


        public System.Nullable<decimal> StartRateValidMiles
        {
            get
            {
                return this._StartRateValidMiles;
            }
            set
            {
                if ((this._StartRateValidMiles != value))
                {
                    this._StartRateValidMiles = value;
                }
            }
        }


        public string Logo
        {
            get
            {
                return this._Logo;
            }
            set
            {
                if ((this._Logo != value))
                {
                    this._Logo = value;
                }
            }
        }


        public System.Nullable<int> SortOrder
        {
            get
            {
                return this._SortOrder;
            }
            set
            {
                if ((this._SortOrder != value))
                {
                    this._SortOrder = value;
                }
            }
        }


        public System.Nullable<decimal> Fare
        {
            get
            {
                return this._Fare;
            }
            set
            {
                if ((this._Fare != value))
                {
                    this._Fare = value;
                }
            }
        }


        public System.Nullable<decimal> ReturnFare
        {
            get
            {
                return this._ReturnFare;
            }
            set
            {
                if ((this._ReturnFare != value))
                {
                    this._ReturnFare = value;
                }
            }
        }




        public System.Nullable<decimal> WaitAndReturnFare
        {
            get
            {
                return this._WaitAndReturnFare;
            }
            set
            {
                if ((this.WaitAndReturnFare != value))
                {
                    this._WaitAndReturnFare = value;
                }
            }
        }




        public System.Nullable<decimal> ExtraCharges
        {
            get
            {
                return this._ExtraCharges;
            }
            set
            {
                if ((this._ExtraCharges != value))
                {
                    this._ExtraCharges = value;
                }
            }
        }


        public System.Nullable<decimal> AgentFees
        {
            get
            {
                return this._AgentFee;
            }
            set
            {
                if ((this._AgentFee != value))
                {
                    this._AgentFee = value;
                }
            }
        }



        public System.Nullable<decimal> AgentCharge
        {
            get
            {
                return this._AgentCharge;
            }
            set
            {
                if ((this._AgentCharge != value))
                {
                    this._AgentCharge = value;
                }
            }
        }


        public System.Nullable<decimal> Congestion
        {
            get
            {
                return this._Congestion;
            }
            set
            {
                if ((this._Congestion != value))
                {
                    this._Congestion = value;
                }
            }
        }


        public System.Nullable<decimal> Parking
        {
            get
            {
                return this._Parking;
            }
            set
            {
                if ((this._Parking != value))
                {
                    this._Parking = value;
                }
            }
        }


        public System.Nullable<decimal> Waiting
        {
            get
            {
                return this._Waiting;
            }
            set
            {
                if ((this._Waiting != value))
                {
                    this._Waiting = value;
                }
            }
        }




    }


    public class EmailInfo
    {

        public string From { get; set; }
        public string To { get; set; }
        public string CustomerName { get; set; }
        public string Subject { get; set; }
        public string MessageBody { get; set; }
        public bool HideFares { get; set; }
        public long BookingId { get; set; }

        public int SubCompanyId { get; set; }

        public bool IsAccountJob { get; set; }
        public int PaymentTypeId { get; set; }
        public int toEmailType { get; set; }
    }
    public class ClsAutoDispatchInfo
    {
        public bool EnableAuto { get; set; }
        public bool EnableBidding { get; set; }
        public int AutoModeType { get; set; }
    }


    public class UserInfo
    {
        public int Id { get; set; }
        public int? SubcompanyId { get; set; }
        public string UserName { get; set; }
        public bool? ShowAllBookings { get; set; }
        public bool? ShowAllDrivers { get; set; }
        public int? SecurityGroupId { get; set; }
        public string Email { get; set; }
        //   args.Id, args.SubcompanyId, args.ShowAllBookings, args.ShowAllDrivers, args.SecurityGroupId, args.Email
    }

    public class RequestWebApi
    {
        public bool? AllocateAnyDriver { get; set; }
        public string MACAddress { get; set; }
        public int SendType { get; set; }

        public string VerifiedToken { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public UserInfo objUserInfo { get; set; }

        public BookingInfo editbookingInfo { get; set; }
        public BookingInfo bookingInfo { get; set; }

        public AddressInfo addressInfo { get; set; }

        public DriverInfo driverInfo { get; set; }


        public CallerInfo callerInfo { get; set; }


        public RouteInfo routeInfo { get; set; }
        public stp_searchbookings searchInfo { get; set; }


        public EmailInfo emailInfo { get; set; }

        public AuthInfo authInfo { get; set; }
        public ClsAutoDispatchInfo autoDispatchInfo { get; set; }


        public AdvanceBookingInfo advancebookingInfo { get; set; }
        public DateTime? ScheduleDateTime { get; set; }
        public DateTime? DelayedDateTime { get; set; }
        public string ArrivalTerminal { get; set; }
        public string ArrivingFrom { get; set; }
        public string FlightNo { get; set; }
        public string DefaultClientId { get; set; }
        public string APIKey { get; set; }
        public string Status { get; set; }
        public string DateTime { get; set; }
        public string Message { get; set; }
        public int? AllowanceMins { get; set; }
        public string FlightInformation { get; set; }
        public string InputDateTime { get; set; }
        public int? DriverId { get; set; }
        public int? JobId { get; set; }
        public string[] DriverIds { get; set; }

        public string DirectionToMove { get; set; }
        public int MoveToDriverId { get; set; }
        public SMSInfo smsInfo { get; set; }
    }

    public class SMSInfo
    {
        public string MobileNo { get; set; }
        public string Message { get; set; }
    }

    public class AdvanceBookingInfo
    {
        public AdvanceBookingInfo()
        {

        }

        public long? AdvanceBookingId { get; set; }


        public Booking BookingReturn { get; set; }


        public decimal? FareRate { get; set; }
        public decimal? CompanyPrice { get; set; }
        public int? PaymentTypeId { get; set; }
        //  public string SpecialRequirements { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        //   public string FromPostCode { get; set; }
        //   public string ToPostCode { get; set; }
        //   public string FromDoorNo { get; set; }
        //   public string ToDoorNo { get; set; }
        //   public string FromStreet { get; set; }
        //   public string ToStreet { get; set; }
        //   public string FromFlightNo { get; set; }
        //    public string FromComing { get; set; }
        //    public int? BookingStatusId { get; set; }
        //   public string DistanceString { get; set; }
        //   public bool? AutoDespatch { get; set; }
        //   public DateTime? AutoDespatchTime { get; set; }

        public int? CompanyId { get; set; }

        //    public bool? IsCompanyWise { get; set; }
        public DateTime? PickupDateTime { get; set; }
        public long Id { get; set; }
        //    public int? FromLocTypeId { get; set; }
        //   public int? ToLocTypeId { get; set; }
        //    public int? FromLocId { get; set; }
        //   public int? ToLocId { get; set; }
        public int? VehicleTypeId { get; set; }
        //    public int? DriverId { get; set; }
        //    public int? ReturnDriverId { get; set; }
        //    public int? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhoneNo { get; set; }
        public string CustomerMobileNo { get; set; }
        //    public int? JourneyTypeId { get; set; }
        //   public string BookingNo { get; set; }
        //   public DateTime? BookingDate { get; set; }


        public string OrderNo { get; set; }

        public long? DepartmentId { get; set; }

        public int? DriverId { get; set; }


    }

    public class AuthInfo
    {

        public long? BookingId { get; set; }
        public int? DriverId { get; set; }
        public string DriverNo { get; set; }

        public int? BookingStatusId { get; set; }
        public int? DriverStatusId { get; set; }

        public string AuthStatus { get; set; }
        public string Message { get; set; }
        /// <summary>
        /// 1 : For Driver Logout Auth
        /// 2 : For Job Auth  Nopickup/reject/recover
        /// </summary>
        public int AuthType { get; set; }

    }


    public class clsCallInfo
    {
        public DateTime? FromDate { get; set; }
        public DateTime? TillDate { get; set; }
        public string PhoneNo { get; set; }
        public string Extension { get; set; }
        public string Name { get; set; }

    }


    public class clsBookingscount
    {
        public int count;
        public int? bookingstatusid;

    }

    public class CallerInfo
    {

        public DateTime? FromDate { get; set; }
        public DateTime? TillDate { get; set; }

        public string Extension { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }


    }

    public class DriverInfo
    {
        public int driverId { get; set; }
        public int driverNo { get; set; }


        /// <summary>
        /// 0  : Today
        /// 1  : Yesterday
        /// -1 : All
        /// </summary>
        public int dateFilter { get; set; }
        public string messageBody { get; set; }
        public int userId { get; set; }
        public string userName { get; set; }
    }


    public class DisplayPosition
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
    public class Location
    {
        public string LocationId { get; set; }
        public string LocationType { get; set; }
        public DisplayPosition DisplayPosition { get; set; }
        public MapView MapView { get; set; }
        public GeoCodeAddress Address { get; set; }
    }
    public class MapView
    {
        public DisplayPosition TopLeft { get; set; }
        public DisplayPosition BottomRight { get; set; }
    }
    public class GeoCodeAddress
    {
        public string Label { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string County { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Street { get; set; }
        public string PostalCode { get; set; }
        public List<AdditionalData> AdditionalData { get; set; }
    }
    public class AdditionalData
    {
        public string value { get; set; }
        public string key { get; set; }
    }
    public class LocationList
    {
        public string AddressLine { get; set; }
        public string LocationTypeId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
    public class RootMapBox
    {
        public string type { get; set; }
        public List<string> query { get; set; }
        public List<FeatureMapBox> features { get; set; }
        public string attribution { get; set; }
    }
    public class FeatureMapBox
    {
        public string id { get; set; }
        public string type { get; set; }
        public List<string> place_type { get; set; }
        public double relevance { get; set; }
        public PropertiesMapBox properties { get; set; }
        public string text { get; set; }
        public string place_name { get; set; }
        public List<double> bbox { get; set; }
        public List<double> center { get; set; }
        public GeometryMapBox geometry { get; set; }
        public List<ContextMapBox> context { get; set; }
    }
    public class ContextMapBox
    {
        public string id { get; set; }
        public string mapbox_id { get; set; }
        public string wikidata { get; set; }
        public string text { get; set; }
        public string short_code { get; set; }
    }

    public class GeometryMapBox
    {
        public string type { get; set; }
        public List<double> coordinates { get; set; }
    }

    public class PropertiesMapBox
    {
        public string mapbox_id { get; set; }
        public string wikidata { get; set; }
        public string category { get; set; }
    }
    public class AddressInfo
    {

        public List<string> NearestDrivers { get; set; }
        public string searchText { get; set; }
        public string customerinfo { get; set; }


        public string Address { get; set; }
        public int locTypeId { get; set; }
        public int? zoneId { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string zoneName { get; set; }

    }


    public class RouteInfo
    {


        public AddressInfo pickupAddress { get; set; }
        public AddressInfo destinationAddress { get; set; }
        public List<AddressInfo> viaAddresses { get; set; }

        public int VehicleTypeId { get; set; }
        public DateTime? PickupDateTime { get; set; }
        public DateTime? returnPickupDateTime { get; set; }
        public int? CompanyId { get; set; }

        public bool? AutoCalculateFares { get; set; }
        public decimal Distance;
        public int Duration;
        public string unit;
        public string currency;
        public RouteCoordinates RouteCoordinates;
        public int Noofhours { get; set; }
        public int FareCalculationSetting { get; set; }
       

        public List<RouteLeg> legs;

        public int? JourneyTypeId { get; set; }
        public int? SubCompanyId { get; set; }
    }
    public class FareSettings
    {
        public decimal fareVal { get; set; }
        public decimal returnFares { get; set; }
        public decimal companyPrice { get; set; }
    }
    public class RouteCoords
    {
        public double? Latitude;
        public double? Longitude;

    }

    public class RouteLeg
    {
        public List<RouteCoords> coords;



    }

    public class RouteCoordinates
    {


        public List<RouteLeg> legs;
        public decimal Distance;
        public int duration;
        public object fareModel;
    }


    public class ResponseWebApi
    {
        public bool HasError { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }




    public class ClsBookingListData
    {

        private long _Id;

        private string _Token;

        private string _RefNumber;

        private System.Nullable<System.DateTime> _BookingDate;

        private System.Nullable<System.DateTime> _PickupDate;

        private string _Passenger;

        private string _MobileNo;

        private string _From;

        private string _FromPostCode;

        private string _To;

        private string _ToPostCode;

        private System.Nullable<decimal> _Fare;

        private string _PaymentMethod;

        private System.Nullable<decimal> _AccountFare;

        private System.Nullable<decimal> _CustomerFare;

        private string _Account;

        private string _Driver;

        private System.Nullable<int> _DriverId;

        private string _Vehicle;

        private string _Status;

        private string _StatusColor;

        private System.Nullable<int> _BookingTypeId;

        private string _VehicleBgColor;

        private string _VehicleTextColor;

        private string _BackgroundColor1;

        private string _TextColor1;

        private System.Nullable<int> _FromLocTypeId;

        private System.Nullable<int> _ToLocTypeId;

        private System.Nullable<int> _SubCompanyBgColor;

        private System.Nullable<int> _StatusId;

        private System.Nullable<int> _BookingBackgroundColor;

        private System.Nullable<int> _FromLocBgColor;

        private System.Nullable<int> _ToLocBgColor;

        private System.Nullable<int> _FromLocTextColor;

        private System.Nullable<int> _ToLocTextColor;

        private System.Nullable<bool> _IsAutoDespatch;

        private System.Nullable<bool> _IsBidding;

        public ClsBookingListData()
        {
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Id", DbType = "BigInt NOT NULL")]
        public long Id
        {
            get
            {
                return this._Id;
            }
            set
            {
                if ((this._Id != value))
                {
                    this._Id = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Token", DbType = "VarChar(10)")]
        public string Token
        {
            get
            {
                return this._Token;
            }
            set
            {
                if ((this._Token != value))
                {
                    this._Token = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_RefNumber", DbType = "VarChar(50)")]
        public string RefNumber
        {
            get
            {
                return this._RefNumber;
            }
            set
            {
                if ((this._RefNumber != value))
                {
                    this._RefNumber = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_BookingDate", DbType = "DateTime")]
        public System.Nullable<System.DateTime> BookingDate
        {
            get
            {
                return this._BookingDate;
            }
            set
            {
                if ((this._BookingDate != value))
                {
                    this._BookingDate = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_PickupDate", DbType = "DateTime")]
        public System.Nullable<System.DateTime> PickupDate
        {
            get
            {
                return this._PickupDate;
            }
            set
            {
                if ((this._PickupDate != value))
                {
                    this._PickupDate = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Passenger", DbType = "NVarChar(MAX)")]
        public string Passenger
        {
            get
            {
                return this._Passenger;
            }
            set
            {
                if ((this._Passenger != value))
                {
                    this._Passenger = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_MobileNo", DbType = "VarChar(50)")]
        public string MobileNo
        {
            get
            {
                return this._MobileNo;
            }
            set
            {
                if ((this._MobileNo != value))
                {
                    this._MobileNo = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Name = "[From]", Storage = "_From", DbType = "NVarChar(MAX)")]
        public string From
        {
            get
            {
                return this._From;
            }
            set
            {
                if ((this._From != value))
                {
                    this._From = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_FromPostCode", DbType = "VarChar(50)")]
        public string FromPostCode
        {
            get
            {
                return this._FromPostCode;
            }
            set
            {
                if ((this._FromPostCode != value))
                {
                    this._FromPostCode = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Name = "[To]", Storage = "_To", DbType = "NVarChar(MAX)")]
        public string To
        {
            get
            {
                return this._To;
            }
            set
            {
                if ((this._To != value))
                {
                    this._To = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_ToPostCode", DbType = "VarChar(50)")]
        public string ToPostCode
        {
            get
            {
                return this._ToPostCode;
            }
            set
            {
                if ((this._ToPostCode != value))
                {
                    this._ToPostCode = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Fare", DbType = "Decimal(24,2)")]
        public System.Nullable<decimal> Fare
        {
            get
            {
                return this._Fare;
            }
            set
            {
                if ((this._Fare != value))
                {
                    this._Fare = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_PaymentMethod", DbType = "VarChar(50)")]
        public string PaymentMethod
        {
            get
            {
                return this._PaymentMethod;
            }
            set
            {
                if ((this._PaymentMethod != value))
                {
                    this._PaymentMethod = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_AccountFare", DbType = "Decimal(18,2)")]
        public System.Nullable<decimal> AccountFare
        {
            get
            {
                return this._AccountFare;
            }
            set
            {
                if ((this._AccountFare != value))
                {
                    this._AccountFare = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_CustomerFare", DbType = "Decimal(18,2)")]
        public System.Nullable<decimal> CustomerFare
        {
            get
            {
                return this._CustomerFare;
            }
            set
            {
                if ((this._CustomerFare != value))
                {
                    this._CustomerFare = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Account", DbType = "VarChar(153)")]
        public string Account
        {
            get
            {
                return this._Account;
            }
            set
            {
                if ((this._Account != value))
                {
                    this._Account = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Driver", DbType = "VarChar(30)")]
        public string Driver
        {
            get
            {
                return this._Driver;
            }
            set
            {
                if ((this._Driver != value))
                {
                    this._Driver = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_DriverId", DbType = "Int")]
        public System.Nullable<int> DriverId
        {
            get
            {
                return this._DriverId;
            }
            set
            {
                if ((this._DriverId != value))
                {
                    this._DriverId = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Vehicle", DbType = "VarChar(100)")]
        public string Vehicle
        {
            get
            {
                return this._Vehicle;
            }
            set
            {
                if ((this._Vehicle != value))
                {
                    this._Vehicle = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Status", DbType = "VarChar(50) NOT NULL", CanBeNull = false)]
        public string Status
        {
            get
            {
                return this._Status;
            }
            set
            {
                if ((this._Status != value))
                {
                    this._Status = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_StatusColor", DbType = "VarChar(30)")]
        public string StatusColor
        {
            get
            {
                return this._StatusColor;
            }
            set
            {
                if ((this._StatusColor != value))
                {
                    this._StatusColor = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_BookingTypeId", DbType = "Int")]
        public System.Nullable<int> BookingTypeId
        {
            get
            {
                return this._BookingTypeId;
            }
            set
            {
                if ((this._BookingTypeId != value))
                {
                    this._BookingTypeId = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_VehicleBgColor", DbType = "VarChar(30)")]
        public string VehicleBgColor
        {
            get
            {
                return this._VehicleBgColor;
            }
            set
            {
                if ((this._VehicleBgColor != value))
                {
                    this._VehicleBgColor = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_VehicleTextColor", DbType = "VarChar(30)")]
        public string VehicleTextColor
        {
            get
            {
                return this._VehicleTextColor;
            }
            set
            {
                if ((this._VehicleTextColor != value))
                {
                    this._VehicleTextColor = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_BackgroundColor1", DbType = "VarChar(50)")]
        public string BackgroundColor1
        {
            get
            {
                return this._BackgroundColor1;
            }
            set
            {
                if ((this._BackgroundColor1 != value))
                {
                    this._BackgroundColor1 = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_TextColor1", DbType = "VarChar(20)")]
        public string TextColor1
        {
            get
            {
                return this._TextColor1;
            }
            set
            {
                if ((this._TextColor1 != value))
                {
                    this._TextColor1 = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_FromLocTypeId", DbType = "Int")]
        public System.Nullable<int> FromLocTypeId
        {
            get
            {
                return this._FromLocTypeId;
            }
            set
            {
                if ((this._FromLocTypeId != value))
                {
                    this._FromLocTypeId = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_ToLocTypeId", DbType = "Int")]
        public System.Nullable<int> ToLocTypeId
        {
            get
            {
                return this._ToLocTypeId;
            }
            set
            {
                if ((this._ToLocTypeId != value))
                {
                    this._ToLocTypeId = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_SubCompanyBgColor", DbType = "Int")]
        public System.Nullable<int> SubCompanyBgColor
        {
            get
            {
                return this._SubCompanyBgColor;
            }
            set
            {
                if ((this._SubCompanyBgColor != value))
                {
                    this._SubCompanyBgColor = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_StatusId", DbType = "Int")]
        public System.Nullable<int> StatusId
        {
            get
            {
                return this._StatusId;
            }
            set
            {
                if ((this._StatusId != value))
                {
                    this._StatusId = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_BookingBackgroundColor", DbType = "Int")]
        public System.Nullable<int> BookingBackgroundColor
        {
            get
            {
                return this._BookingBackgroundColor;
            }
            set
            {
                if ((this._BookingBackgroundColor != value))
                {
                    this._BookingBackgroundColor = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_FromLocBgColor", DbType = "Int")]
        public System.Nullable<int> FromLocBgColor
        {
            get
            {
                return this._FromLocBgColor;
            }
            set
            {
                if ((this._FromLocBgColor != value))
                {
                    this._FromLocBgColor = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_ToLocBgColor", DbType = "Int")]
        public System.Nullable<int> ToLocBgColor
        {
            get
            {
                return this._ToLocBgColor;
            }
            set
            {
                if ((this._ToLocBgColor != value))
                {
                    this._ToLocBgColor = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_FromLocTextColor", DbType = "Int")]
        public System.Nullable<int> FromLocTextColor
        {
            get
            {
                return this._FromLocTextColor;
            }
            set
            {
                if ((this._FromLocTextColor != value))
                {
                    this._FromLocTextColor = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_ToLocTextColor", DbType = "Int")]
        public System.Nullable<int> ToLocTextColor
        {
            get
            {
                return this._ToLocTextColor;
            }
            set
            {
                if ((this._ToLocTextColor != value))
                {
                    this._ToLocTextColor = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_IsAutoDespatch", DbType = "Bit")]
        public System.Nullable<bool> IsAutoDespatch
        {
            get
            {
                return this._IsAutoDespatch;
            }
            set
            {
                if ((this._IsAutoDespatch != value))
                {
                    this._IsAutoDespatch = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_IsBidding", DbType = "Bit")]
        public System.Nullable<bool> IsBidding
        {
            get
            {
                return this._IsBidding;
            }
            set
            {
                if ((this._IsBidding != value))
                {
                    this._IsBidding = value;
                }
            }
        }

    }


}