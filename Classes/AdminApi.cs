using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using Taxi_Model;

namespace SignalRHub.Classes
{
    #region Shuttle
    public class GroupJob_Order
    {
        public int Bookingid;
        public int OrderId;
        public int GroupJobId;
    }
    public class ShuttleTrip
    {
        public long TripId { get; set; }
        public string TripNo { get; set; }
        public bool followSequence { get; set; }
        public int DriverId { get; set; }
        public List<ShuttleJobs> jobs { get; set; }
        public int TripStatusId { get; set; }
        public string Message { get; set; }
    }
    public class ShuttleJobs
    {
        public string PickupDateTime { get; set; }
        public string Cust { get; set; }
        public int Passengers { get; set; }
        public string Pickup { get; set; }
        public string Destination { get; set; }
        public string BookingNo { get; set; }
        public string Special { get; set; }
        public string Payment { get; set; }
        public string Journey { get; set; }
        public decimal Fares { get; set; }
        public string Account { get; set; }
        public string Vehicle { get; set; }
        public string Lug { get; set; }
        public string BookingType { get; set; }
        public string JobId { get; set; }
        public string SubCompanyId { get; set; }
        public int Did { get; set; }
        public bool ShowFares { get; set; }
        public bool HideAccountName { get; set; }
        public int JStatus { get; set; }

    }
    public class ShuttleBookings
    {
        public int? BookingId { get; set; }
        public int? OrderId { get; set; }
    }
    #endregion Shuttle
    public class ClsIVRCaller
    {
        public class IVRInfo
        {
            public string ClientId;
            public string ClientName;
            public string ClientConn;
            public string IVRNumbers;
            public string CloudUrl;
            public bool ReleaseMode;
            public bool CalculateFares;
            public string Reason;
        }

        public static IVRInfo GetIVRInfo(IVRInfo obj)
        {
            try
            {
                string Urls = "http://eurlic.co.uk/license/api/Cab/GetIVRInfo";
                var baseAddress = new Uri(Urls);
                var json = string.Empty;
                json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(Urls);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.Proxy = null;
                httpWebRequest.Headers.Add("Authorization", "Basic " + "Y2FidHJlYXN1cmU6Y2FidHJlYXN1cmU5ODcwIUAj");
                //   string usernamePassword = Base64Encode("cabtreasure:cabtreasure9870!@#");
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    obj = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<IVRInfo>(result);
                }

            }
            catch (Exception ex)
            {

            }

            return obj;

        }

        public static IVRInfo SaveIVRInfo(IVRInfo obj)
        {

            try
            {
                try
                {
                    string Urls = "http://eurlic.co.uk/license/api/Cab/SaveIVRInfo";
                    var baseAddress = new Uri(Urls);
                    var json = string.Empty;
                    json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);



                    var httpWebRequest = (HttpWebRequest)WebRequest.Create(Urls);
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "POST";
                    httpWebRequest.Proxy = null;
                    httpWebRequest.Headers.Add("Authorization", "Basic " + "Y2FidHJlYXN1cmU6Y2FidHJlYXN1cmU5ODcwIUAj");
                    //   string usernamePassword = Base64Encode("cabtreasure:cabtreasure9870!@#");
                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        streamWriter.Write(json);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }

                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        obj = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<IVRInfo>(result);
                    }

                }
                catch (Exception ex)
                {


                }


            }
            catch (Exception ex)
            {

            }

            return obj;

        }


    }
    public class clsAvail
    {
        public int? DriverId { get; set; }
        public DateTime? EndingDate { get; set; }
    }

    public class CommissionPay
    {
        public long Id { get; set; }
        public long CommissionId { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public decimal CurrentBalance { get; set; }
        public string Description { get; set; }
        public string AddBy { get; set; }
        public int UserId { get; set; }
        public bool IsCredit { get; set; }
    }
    public class ExportInvoiceCSV
    {
        public string InvoiceNo { get; set; }
        public DateTime? PickupDateTime { get; set; }
        public string OrderNo { get; set; }
        public string BookingNo { get; set; }
        public string CustomerName { get; set; }
        public string DepartmentName { get; set; }
        public string BookedBy { get; set; }
        public string FromAddress { get; set; }
        public string Via1 { get; set; }
        public string ViaString { get; set; }
        public string Via2 { get; set; }
        public string Via3 { get; set; }
        public string ToAddress { get; set; }
        public string VehicleType { get; set; }
        public decimal? Fare { get; set; }
        public decimal? AccMiles { get; set; }
        public decimal? WaitMins { get; set; }
        public decimal? CompanyPrice { get; set; }
        public decimal? WaitingCharges { get; set; }
        public decimal? ParkingCharges { get; set; }
        public decimal? ExtraDropCharges { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? VATAmount { get; set; }
        public decimal? TipAmount { get; set; }
        public decimal? BookingFee { get; set; }
        public decimal? ServiceCharges { get; set; }
        public decimal? TotalCharges { get; set; }
    }
    public class Gen_SubCompany_Details
    {
        //public int Id { get; set; }
        public int? SubCompanyId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? Radius { get; set; }
        public string Region { get; set; }
        public string TimeZoneId { get; set; }
        public string DistanceUnit { get; set; }
    }
    public class AdminApi
    {
        public decimal? SubCompanyBookingFees { get; set; }

        public int pageNumber { get; set; }
        public int pageSize { get; set; }
        public string searchTerm { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public int? UserId { get; set; }
        public bool loginWise { get; set; }
        public string DateType { get; set; }
        public string ReportType { get; set; }
        public DateTime? Todate { get; set; }
        public DateTime? Fromdate { get; set; }
        public string DayName { get; set; }
        public Gen_SubCompany SubCompany { get; set; }
        public Fleet_Driver_CompanyVehicle companyVehicle { get; set; }
        public Fleet_VehicleType fleetVehicleType { get; set; }
        public Fleet_Driver fleetDriver { get; set; }
        public WebApiClasses.BookingInfo bookingInfo { get; set; }
        public Gen_Company Company { get; set; }
        public UM_User user { get; set; }
        public Fare fare { get; set; }
        public Gen_Location location { get; set; }
        public string RowValue { get; set; }
        public string HeaderValue { get; set; }
        public string CellSetValue { get; set; }
        public List<Fare_OtherChargesLst> fare_OtherChargesLsts { get; set; }
        public string FromTimestr { get; set; }
        public string ToTimestr { get; set; }
        public string FromDatestr { get; set; }
        public string ToDatestr { get; set; }
        public string Ext { get; set; }
        public int? DriverId { get; set; }
        public long Id { get; set; }
        public int? SubCompanyId { get; set; }
        public bool optTodays { get; set; }

        public bool? optAllTag { get; set; }
        public int? DriverTypeId { get; set; }
        public string PickupPoint { get; set; }
        public string Destination { get; set; }
        public decimal fares { get; set; }
        public decimal parking { get; set; }
        public decimal waiting { get; set; }
        public decimal extraDrop { get; set; }
        public decimal bookingFees { get; set; }
        public decimal AgentFees { get; set; }
        public List<GetCalcDriverStatement> GetCalcDriverList { get; set; }
        public decimal OldBalance { get; set; }
        public decimal numDrvRent { get; set; }
        public decimal numpdaRent { get; set; }
        public decimal numCarRent { get; set; }
        public decimal numCarInsuranceRent { get; set; }
        public decimal numPrimeCompanyRent { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal numAccountExpenses { get; set; }
        public decimal numPDARentPerWeek { get; set; }
        public string CommsionTotal { get; set; }
        public decimal numCollectionDeliveryAmount { get; set; }
        public decimal spnAccountExpenses { get; set; }
        public string TransNo { get; set; }
        public List<GetCalcDriverCommStatement> GetCalcDriverCommList { get; set; }
        public List<Fleet_DriverCommissionExpense> DriverCommissionExpenseList { get; set; }
        public Fleet_DriverCommision driverCommission { get; set; }
        public int? AccountBookingDays { get; set; }
        public bool IsFareAndWaitingWiseComm { get; set; }
        public int? PickType { get; set; }
        public string toEmail { get; set; }
        public string fromEmail { get; set; }
        public string emailSubject { get; set; }
        public string emailBody { get; set; }
        public string exportType { get; set; }
        public string base64File { get; set; }
        public string fileName { get; set; }
        public List<SysPolicyConfg> SecurityGeneral { get; set; }
        public List<callerId> CallerId { get; set; }
        public List<sMS> SMS { get; set; }
        public bool OptDefault { get; set; }
        public bool OptAsc { get; set; }
        public bool OptDesc { get; set; }

        public int? CompanyId { get; set; }
        public int InvoiceId { get; set; }
        public List<invoice_Payment> invoice_Payments { get; set; }
        public decimal? InvoiceCurrentBalance { get; set; }
        public bool OptAll { get; set; }
        public bool OptCompleted { get; set; }
        public bool OptCanceled { get; set; }
        public int? InvoiceType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? TillDate { get; set; }
        public int? GroupId { get; set; }
        public bool? checkAllAccounts { get; set; }
        public Invoice invoice { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public bool chkAllFromDate { get; set; }
        public DateTime titleDate { get; set; }
        public bool DepartmentWise { get; set; }
        public int? DepartmentId { get; set; }
        public string OrderNo { get; set; }
        public Customer customer { get; set; }
        public Gen_Location Gen_Location { get; set; }
        public List<Gen_SysPolicy_AirportPickupCharges2> GetSysPAirportPickupChargesList { get; set; }
        public List<Gen_SysPolicy_AirportDropOffCharges2> GetSysPAirportDropOffChargesList { get; set; }
        public List<Gen_Location> AirportColorCodes { get; set; }
        public int ModeId { get; set; }
        public List<BookingType> BookingType { get; set; }
        public int hourRange { get; set; }
        public List<LocalizationDetail> LocalizationDetail { get; set; }
        public List<Gen_Syspolicy_DriverDocumentList> DriverDocumentList { get; set; }
        public string LocationId { get; set; }
        public string AddressName { get; set; }
        public string AddressLine1 { get; set; }
        public List<Gen_LocationType> LocationTypes { get; set; }
        public List<Localization> Localization { get; set; }
        public List<SurchargeRate> SurchargeRatesSetting { get; set; }
        public List<UM_SecurityGroup_Permission> AuthoritiesList { get; set; }
        public int SysPolicyId { get; set; }
        public List<ClsAutoDespatch> OtherData { get; set; }
        public bool chkTopstdJobPlot { get; set; }
        public bool chkTopstdJobBackPlot { get; set; }
        public bool chkNearestDriver { get; set; }
        public int NearestDriverRadious { get; set; }
        public bool chkAutoAllocateSTC { get; set; }
        public bool chkbinding { get; set; }
        public DriverRent driverRent { get; set; }

        public string AttributeName { get; set; }
        public string ShortName { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public int? MaxQty { get; set; }
        public decimal? ChargesPerQty { get; set; }
        public int? AttributeCategoryId { get; set; }

        public List<ClsUpdate> list { get; set; }
        public Gen_SysPolicy_PDASetting objPdaSettings { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double PosLatitude { get; set; }
        public double PosLongitude { get; set; }
        public double Speed { get; set; }
        public bool chkETA { get; set; }
        public string etaKey { get; set; }
        public string WorkStatus { get; set; }
        public int ExpiredPDAJobHours { get; set; }
        public string JobIds { get; set; }
        public int JobId { get; set; }

        public List<FareMeterSetting> FareMeterSettings { get; set; }
        public List<BookingFeesRange> BookingFeesRange { get; set; }
        public int FareMeterType { get; set; }
        public decimal FareRoundedValue { get; set; }
        public bool EnablePeakOffPeakWiseFares { get; set; }
        public bool ChangePlotAsDirected { get; set; }
        public bool ShowBookingFees { get; set; }
        public bool ShowExtraCharges { get; set; }
        public bool ShowParkinCharges { get; set; }
        public bool RemoveExtraCharges { get; set; }
        public decimal ExtraChargesPerQty { get; set; }
        public int[] DriverIds { get; set; }
        public string Message { get; set; }
        public string UserName { get; set; }
        public List<Gen_SysPolicy_FaresSetting> Gen_SysPolicy_FaresSettings { get; set; }
        public int? formId { get; set; }
        public string[] columnName { get; set; }
        public int[] columnWidth { get; set; }
        public int[] columnOrder { get; set; }
        public string[] columnVisible { get; set; }



        //




        public string MapType { get; set; }
        public string MapTypeDesc { get; set; }
        public string MapKey { get; set; }
        public DateTime? DateCriteria { get; set; }
        
        public DateTime? TripDate { get; set; }
        public bool? chkFollowSequence { get; set; }
        public int? NoOfPax { get; set; }
        public List<ShuttleBookings> ShuttleBookings { get; set; }
        public long tripId { get; set; }
        public int tripUpdateStatusId { get; set; }
        public int tripStatusId { get; set; }



        public CommissionPay CommissionPay { get; set; }


        public string CompanyName { get; set; }
        public List<IVRInfo> IVRInfo { get; set; }
        public string HubURL { get; set; }
        #region odysse
        public DateTime? PHCVehicleExpiryDate { get; set; }
        public string PHCVehicleExpiryPath { get; set; }
        public string UFMOTDoc_CVP_Text { get; set; }
        public string UFRTEDoc_CVP_Text { get; set; }
        public string UFIEDoc_CVP_Text { get; set; }
        public string UFVehicleLogDoc_CVP_Text { get; set; }
        public string PHCVehicleExpiryDate_CVP_Text { get; set; }
        #endregion
        #region viken lux
        public double Radius { get; set; }
        public string Region { get; set; }
        public string TimeZone { get; set; }
        public string DistanceUnit { get; set; }
        public int? MinPassenger_Normal { get; set; }
        public int? MinPassenger_Airport { get; set; }
        public decimal? Charges_Normal { get; set; }
        public decimal? Charges_Airport { get; set; }
        public List<CompanyDepartment> DeptNotes { get; set; }


        //

        public HourlyFare HourlyFare { get; set; }
        public HourlyFare HourlyTariffFare { get; set; }
        public List<HourlyFare_OtherCharges> HourlyFare_OtherCharges { get; set; }
        public List<HourlyTariffFare_OtherCharges> HourlyTariffFare_OtherCharges { get; set; }


        public string MobileNo { get; set; }
        public bool? HasVatInclusive { get; set; }
        public decimal? DriverHourlyRate { get; set; }
        public List<FleetDriverHourlyTariffRate> Fleet_Driver_HourlyTariffRate { get; set; }
        #endregion
    }
    #region viken lux 
    public class FleetDriverHourlyTariffRate
    {
        public int Id { get; set; }
        public int DriverId { get; set; }
        public int VehicleTypeId { get; set; }
        public decimal FromMinute { get; set; }
        public decimal ToMinute { get; set; }
        public decimal HTRate { get; set; }
        public bool IsActive { get; set; }
    }
    public class CompanyDepartment
    {
        public long Id { get; set; }
        public string DepartmentName { get; set; }
        public int CompanyId { get; set; }
        public string ComapanyFromAddress { get; set; }
        public string ComapnyToAddress { get; set; }
        public string invoicenote { get; set; }
    }


    public class HourlyFare
    {
        public int Id { get; set; }
        public int VehicleTypeId { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? AddOn { get; set; }
        public int? AddBy { get; set; }
        public DateTime? EditOn { get; set; }
        public int? EditBy { get; set; }
        public bool IsVehicleWise { get; set; }
        public bool IsCompanyWise { get; set; }
        public int CompanyId { get; set; }
        public int SubCompanyId { get; set; }
        public decimal PerMinJourneyCharges { get; set; }
        public bool IsDayWise { get; set; }
        public string DayValue { get; set; }
        public decimal StartRate { get; set; }
        public decimal StartRateValidMins { get; set; }
        public string FromDayName { get; set; }
        public string TillDayName { get; set; }
        public string SpecialDayName { get; set; }
        public DateTime? FromSpecialDate { get; set; }
        public DateTime? TillSpecialDate { get; set; }
        public DateTime? FromDateTime { get; set; }
        public DateTime? TillDateTime { get; set; }
        public decimal WaitingCharges { get; set; }
        public int WaitingChargesPerSeconds { get; set; }
        public int WaitingSecondsFree { get; set; }

    }
    public class HourlyTariffFare
    {
        public int Id { get; set; }
        public int VehicleTypeId { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? AddOn { get; set; }
        public int? AddBy { get; set; }
        public DateTime? EditOn { get; set; }
        public int? EditBy { get; set; }
        public bool IsVehicleWise { get; set; }
        public bool IsCompanyWise { get; set; }
        public int CompanyId { get; set; }
        public int SubCompanyId { get; set; }
        public bool IsDayWise { get; set; }
        public string DayValue { get; set; }
        public string FromDayName { get; set; }
        public string TillDayName { get; set; }
        public string SpecialDayName { get; set; }
        public DateTime? FromSpecialDate { get; set; }
        public DateTime? TillSpecialDate { get; set; }
        public DateTime? FromDateTime { get; set; }
        public DateTime? TillDateTime { get; set; }
        public decimal WaitingCharges { get; set; }
        public int WaitingChargesPerSeconds { get; set; }
        public int WaitingSecondsFree { get; set; }
    }
    public class HourlyFare_OtherCharges
    {
        public long? Id { get; set; }
        public int FareId { get; set; }
        public int FromMins { get; set; }
        public int ToMins { get; set; }
        public decimal Rate { get; set; }
        public decimal PeakTimeRate { get; set; }
        public decimal OffPeakTimeRate { get; set; }
        public decimal NightTimeRate { get; set; }
        public DateTime? FromStartTime { get; set; }
        public DateTime? TillStartTime { get; set; }
        public DateTime? FromEndTime { get; set; }
        public DateTime? TillEndTime { get; set; }
        public decimal CompanyRate { get; set; }
    }
    public class HourlyTariffFare_OtherCharges
    {
        public long? Id { get; set; }
        public int FareId { get; set; }
        public int FromMins { get; set; }
        public int Hours { get; set; }
        public decimal Rate { get; set; }
        public decimal CompanyRate { get; set; }
    }
    public class Gen_Company_Contacts
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string ContactName { get; set; }
        public string Email { get; set; }
        public string TelephoneNo { get; set; }
        public string MobileNo { get; set; }
        public string CustomerId { get; set; }
        public string PaymentMethodId { get; set; }
        public string PaymentStatus { get; set; }
        public bool? IsSuccess { get; set; }
        public string lastfour { get; set; }
        public string Expiry { get; set; }
        public string CardType { get; set; }
    }
    public class PromotionReport
    {
        public long Id { get; set; }
        public string PromotionCode { get; set; }
        public string PromotionTitle { get; set; }
        public string PromotionMessage { get; set; }
        public DateTime? PromotionStartDateTime { get; set; }
        public DateTime? PromotionEndDateTime { get; set; }
        public int PromotionTypeID { get; set; }
        public decimal? Charges { get; set; }
        public bool IsActive { get; set; }
        public int? SubCompanyId { get; set; }

    }
    public class stp_BookingPromotionReports
    {
        public string PromotionCode { get; set; }
        public string BookingNo { get; set; }
        public DateTime? PickupDateTime { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string FromAddress { get; set; }
        public string ViaCount { get; set; }
        public string ToAddress { get; set; }
        public decimal? OriginalFare { get; set; }
        public decimal? DiscountedFare { get; set; }

    }
    public class Promotion
    {
        public long Id { get; set; }
        public int? SubCompanyId { get; set; }
        public bool IsActive { get; set; }
        public string PromotionCode { get; set; }
        public string PromotionTitle { get; set; }
        public string PromotionMessage { get; set; }
        public DateTime PromotionStartDateTime { get; set; }
        public DateTime PromotionEndDateTime { get; set; }
        public int? PromotionTypeID { get; set; }
        public decimal? Charges { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifyedDateTime { get; set; }
        public string ModifyedBy { get; set; }
        public int? CustomerId { get; set; }
        public int? Totaljourney { get; set; }
        public bool? IsAllCustomer { get; set; }
        public int? NoOfUses { get; set; }
        public int? NoofCustomer { get; set; }
        public int? DiscountTypeId { get; set; }
        public long? Value { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? IsPeriod { get; set; }
        public decimal? MinimumFare { get; set; }
        public decimal? MaximumDiscount { get; set; }
        public bool? IsDiscountToDriver { get; set; }
    }
    public class Promotion_Type
    {
        public int? Id { get; set; }
        public string PromotionType { get; set; }
    }
    public class Discount_Type
    {
        public int? Id { get; set; }
        public string DiscountType { get; set; }
    }
    public class PromotonCustomer
    {
        public long? RecordId { get; set; }
        public int? CustomerId { get; set; }
        public string Name { get; set; }
        public string MobileNo { get; set; }
        public string Email { get; set; }
    }
    public class VehicleTypeExtraDetails
    {
        public int? MinPassenger { get; set; }
        public int? MinPassengerAirport { get; set; }
        public decimal? ChargesNormal { get; set; }
        public decimal? ChargesAirport { get; set; }
        public int Id { get; set; }
        public string VehicleType { get; set; }
        public decimal? StartRate { get; set; }
        public decimal? IncrementRate { get; set; }
        public int? NoofPassengers { get; set; }
        public int? NoofLuggages { get; set; }
        public int? NoofHandLuggages { get; set; }
        public string BackgroundColor { get; set; }
        public string TextColor { get; set; }
        public decimal? StartRateValidMiles { get; set; }
        public string AttributeValues { get; set; }
    }
    public class SMSLOG
    {
        public long Id { get; set; }
        public string SMSBody { get; set; }
        public DateTime? SentOn { get; set; }
        public string SentBy { get; set; }
        public string SentTo { get; set; }
        public string Status { get; set; }
        public string SmsRequest { get; set; }
        public string SentOnstr { get; set; }
    }
    #endregion
    public class Fleet_Master_Update
    {
        public int Id { get; set; }
        public string VehicleID { get; set; }
        public string PlateNo { get; set; }
        public string VehicleNo { get; set; }
        public string Vehicle { get; set; }
        public string Owner { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Mot { get; set; }
        public string RoadTax { get; set; }
        public string Insurance { get; set; }
        public string Plate { get; set; }
        public int VehicleTypeId { get; set; }
        public int FuelTypeId { get; set; }
        public string VehicleColor { get; set; }
        public string LogBookNo { get; set; }
        public string MOTExpiryPath { get; set; }
        public string InsuranceExpiryPath { get; set; }
        public string RoadTaxExpPath { get; set; }
        public string LogBookPath { get; set; }
        public string PHCVehicleExpiryDate { get; set; }
        public string PHCVehicleExpiryPath { get; set; }
        public bool InActive { get; set; }
        public string Manufacture { get; set; }
    }
    public class IVRInfo
    {
        public string IVRNumbers { get; set; }
        public bool ReleaseMode { get; set; }
        public bool CalculateFares { get; set; }
    }
    public class PaymentGatewaySetting
    {
        public PaymentGatewaySetting()
        {
            PaymentDetail = new List<PaymentGatewayDetail>();
        }

        public PaymentDetailPda PdaSetting { get; set; }
        public List<PaymentGatewayDetail> PaymentDetail { get; set; }
    }
    public class PaymentGatewayList
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
    public class PaymentDetailPda
    {
        public bool ShowFares { get; set; }
        public bool ShowParking { get; set; }
        public bool ShowWaiting { get; set; }
        public bool EditFares { get; set; }
        public bool EditParking { get; set; }
        public bool EditWaiting { get; set; }
    }
    public class PaymentGatewayDetail
    {
        public int Id { get; set; }
        public int SysPolicyId { get; set; }
        public int? PaymentGatewayId { get; set; }
        public string MerchantID { get; set; }
        public string MerchantPassword { get; set; }
        public string ApiCertificate { get; set; }
        public string PaypalID { get; set; }
        public string ApplicationId { get; set; }
        public bool? EnableMobileIntegration { get; set; }
        public string ApiUsername { get; set; }
        public string ApiPassword { get; set; }
        public string ApiSignature { get; set; }
        public string PrivateKeyPassword { get; set; }
        public string IPNListenerUrl { get; set; }
        public string Name { get; set; }
    }
    public class ActionBookingRequest
    {
        public int JobId { get; set; }
        public string BookingStatus { get; set; }
    }
    public class FareSettingNew
    {
        public int? Id { get; set; }
        public string VehicleType { get; set; }
        public bool? IsAmountWise { get; set; }
        public int? Percentage { get; set; }
        public decimal? Amount { get; set; }
        public int? FareSettingId { get; set; }
        public string Operator { get; set; }
    }
    public class ParameterValues
    {
        public object ApplyPromotion { get; set; }
        public string JudoId { get; set; }
        public string JudoSecret { get; set; }
        public string JudoToken { get; set; }
        public object PSPID { get; set; }
        public object APIUserId { get; set; }
        public object APIPassword { get; set; }
        public object InstId { get; set; }
        public string Gateway { get; set; }
        public string PaymentTypes { get; set; }
        public string CurrencySymbol { get; set; }
        public string enableSignup { get; set; }
        public string isVia { get; set; }
        public string showFares { get; set; }
        public int maxMilesLimit { get; set; }
        public string shoppingEnabled { get; set; }
        public string shoppingNotice { get; set; }
        public int restrictTime { get; set; }
        public string restrictTimeMessage { get; set; }
        public int autorefreshvehicles { get; set; }
        public string verificationCodeType { get; set; }
        public string TermsAndConditions { get; set; }
        public string EmergencyNo { get; set; }
        public string ShareJourney { get; set; }
        public string syncBookings { get; set; }
        public string ZeroFareText { get; set; }
        public string LostProperty { get; set; }
        public string EnableReceipt { get; set; }
        public string EnablePromotion { get; set; }
        public string showPromoList { get; set; }
        public string sendReceiptOnClear { get; set; }
        public int FareRangePercent { get; set; }
        public string RegisterCardMessage { get; set; }
        public string HideAvailableDrivers { get; set; }
        public string authorizeBooking { get; set; }
        public string restrictVehicle { get; set; }
        public string restrictMessage { get; set; }
        public bool EmailConfirmation { get; set; }
        public bool IsHoldMode { get; set; }
        public bool FixedFarePriority { get; set; }
        public int DriverAvailablePickupMins { get; set; }
        public string DriverAvailPickupMsg { get; set; }
        public string VersionUpgrade { get; set; }
        public string EnableLostItemInquiry { get; set; }
        public string EnableTip { get; set; }
        public string enableOutsideUK { get; set; }
        public string EnableComplain { get; set; }
        public string DistanceUnit { get; set; }
        public bool HideCurrentLocation { get; set; }
        public int? AppBooking { get; set; }
        public decimal? InstantBookNow { get; set; }
        public decimal? InstantBookLater { get; set; }
    }

    public class stp_GetDriverEarningResult_Template3
    {
        public int DriverId { get; set; }
        public string DriverNo { get; set; }
        public string Name { get; set; }
        public decimal? Account { get; set; }
        public decimal? BookingFee { get; set; }
        public decimal? Account1 { get; set; }
        public decimal? Cash { get; set; }
        public decimal? Cash1 { get; set; }
        public decimal? Commission { get; set; }
        public int? Earning { get; set; }
        public int? Decline { get; set; }
        public int? JobsDone { get; set; }
        public int? Noshow { get; set; }
        public decimal? Total { get; set; }
        public int? TotalDays { get; set; }
        public int? TotalHrs { get; set; }
        public int? Avgday { get; set; }
        public int? Avghour { get; set; }
        public int? AvgJob { get; set; }
        public int? LoginHour { get; set; }
        public int? LoginDateTime { get; set; }
        public decimal? Expenses { get; set; }
        public decimal? Parking { get; set; }
        public decimal? Waiting { get; set; }
        public decimal? ExtraDropCharges { get; set; }
        public decimal? PDARent { get; set; }
        public decimal? DriverCommissionPerBooking { get; set; }
        public int Break { get; set; }
        public int BreakTime { get; set; }
    }

    public class ClsUpdate
    {

        public int? DriverId { get; set; }
        public decimal? PDAVersion { get; set; }


    }
    public class MeterTariff
    {
        public string RemoveExtraCharges;
        public string ExtraChargesPerQty;
        public string ShowExtraCharges;
        public string ShowBookingFees;
        public List<BookingFeesRange> BookingFeesRange;
        public string ShowParkingCharges;
        public string ChangePlotOnAsDirected;
    }
    public class FareMeterSetting
    {
        public int Id { get; set; }
        public int SysPolicyId { get; set; }
        public int VehicleTypeId { get; set; }
        public bool HasMeter { get; set; }
        public bool AutoStartWaiting { get; set; }
        public decimal AutoStartWaitingBelowSpeed { get; set; }
        public int AutoStartWaitingBelowSpeedSeconds { get; set; }
        public decimal AutoStopWaitingOnSpeed { get; set; }
        public decimal DrvWaitingChargesPerMin { get; set; }
        public decimal AccWaitingChargesPerMin { get; set; }
        public decimal FreeWaitingSeconds { get; set; }
    }
    public class CreateAllDriverCommissionRequest
    {
        public DateTime? RentFromDateTime { get; set; }
        public DateTime? RentToDateTime { get; set; }
        public int? SubCompanyId { get; set; }
        //public string FromTime { get; set; }
        //public string ToTime { get; set; }
        //public int? DriverId { get; set; }
        //public int? PaymentTypeId { get; set; }
        //public int? TransferredSubCompanyId { get; set; }
        //public long? CompanyVehicleId { get; set; }
        //public int? reportType { get; set; }
        //public string OrderNo { get; set; }
        //public int? CompanyId { get; set; }
        //public int? SubCompanyId { get; set; }
        //public int? optSortAsc { get; set; }
    }
    public class Fare_OtherChargesLst
    {
        public int Id { get; set; }
        public int FareId { get; set; }
        public decimal? FromMile { get; set; }
        public decimal? ToMile { get; set; }
        public decimal? Rate { get; set; }
        public decimal? CompanyRate { get; set; }
    }
    public class JobListReportRequest
    {
        public string PaymentType { get; set; }
        public string BookedBy { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string FromTime { get; set; }
        public string ToTime { get; set; }
        public int? DriverId { get; set; }
        public int? PaymentTypeId { get; set; }
        public int? TransferredSubCompanyId { get; set; }
        public long? CompanyVehicleId { get; set; }
        public int? reportType { get; set; }
        public string OrderNo { get; set; }
        public int? CompanyId { get; set; }
        public int? SubCompanyId { get; set; }
        public int? optSortAsc { get; set; }
    }
    public class stp_GetBookingBaseResultEx
    {
        public int? SubCompanyId { get; set; }
        public decimal? TotalTravelledMiles { get; set; }
        public DateTime? ClearedDateTime { get; set; }
        public DateTime? STCDateTime { get; set; }
        public DateTime? POBDateTime { get; set; }
        public DateTime? AcceptedDateTime { get; set; }
        public DateTime? ArrivalDateTime { get; set; }
        public string AuthCode { get; set; }
        public string DriverCommissionType { get; set; }
        public decimal? DriverCommission { get; set; }
        public bool? IsCommissionWise { get; set; }
        public string PupilNo { get; set; }
        public string OrderNo { get; set; }
        public int? NoofHandLuggages { get; set; }
        public int? NoofLuggages { get; set; }
        public int? NoofPassengers { get; set; }
        public string ToStreet { get; set; }
        public string FromStreet { get; set; }
        public string SpecialRequirements { get; set; }
        public string Despatchby { get; set; }
        public string ReturnDriverFullName { get; set; }
        public int? AgentCommissionPercent { get; set; }
        public bool? JobTakenByCompany { get; set; }
        public int? FleetMasterId { get; set; }
        public string SurchargeAmount { get; set; }
        public decimal? ReceiptTotal { get; set; }
        public string CreditCardNumber { get; set; }
        public string CardNumber { get; set; }
        public string CompanyVatNumber { get; set; }
        public string PaymentComments { get; set; }
        public string JourneyType { get; set; }
        public decimal? AccountTotalCharges { get; set; }
        public decimal? DriverTotalCharges { get; set; }
        public string NotesString { get; set; }
        public string CustomerEmail { get; set; }
        public string Via1 { get; set; }
        public int? CompanyGroupId { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string InvoiceNo { get; set; }
        public long? MasterJobId { get; set; }
        public decimal? CustomerPrice { get; set; }
        public decimal? CompanyPrice { get; set; }
        public decimal? AgentCommission { get; set; }
        public int? ReturnDriverId { get; set; }
        public string DriverAddress { get; set; }
        public string DriverName { get; set; }
        public int? ToLocType { get; set; }
        public int? FromLocType { get; set; }
        public string CostCenterName { get; set; }
        public int? CostCenterId { get; set; }
        public string DepartmentName { get; set; }
        public long? DepartmentId { get; set; }
        public string VehicleType { get; set; }
        public int? VehicleTypeId { get; set; }
        public int? BookingTypeId { get; set; }
        public bool? IsAgent { get; set; }
        public string CompanyAddress { get; set; }
        public int? AccountTypeId { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public int? CompanyId { get; set; }
        public string BookedBy { get; set; }
        public DateTime? BookingDate { get; set; }
        public string BookingNo { get; set; }
        public long Id { get; set; }
        public string CustomerName { get; set; }
        public string CustomerMobileNo { get; set; }
        public string CustomerPhoneNo { get; set; }
        public DateTime? PickupDateTime { get; set; }
        public string DriverFullName { get; set; }
        public string DriverNo { get; set; }
        public int? DriverId { get; set; }
        public int? PaymentTypeId { get; set; }
        public string PaymentType { get; set; }
        public string StatusName { get; set; }
        public int? BookingStatusId { get; set; }
        public decimal TotalCharges { get; set; }
        public decimal CongtionCharges { get; set; }
        public string FleetMasterVehicleNo { get; set; }
        public decimal MeetAndGreetCharges { get; set; }
        public decimal WaitingCharges { get; set; }
        public decimal? ParkingCharges { get; set; }
        public decimal? FareRate { get; set; }
        public string ToDoorNo { get; set; }
        public string FromDoorNo { get; set; }
        public string ToAddress { get; set; }
        public string FromAddress { get; set; }
        public decimal? ReturnFareRate { get; set; }
        public DateTime? ReturnPickupDateTime { get; set; }
        public decimal? ExtraDropCharges { get; set; }
        public decimal ServiceCharges { get; set; }
        public int? DriverWaitingMins { get; set; }
    }
    public class UserDefinedSetting
    {
        public int Id { get; set; }
        public int? FormId { get; set; }
        public string GridColumnName { get; set; }
        public bool? IsVisible { get; set; }
        public int? GridColWidth { get; set; }
        public int? GridColMoveTo { get; set; }
        public string FormTab { get; set; }
        public bool? DisplaySettings { get; set; }
        public string HeaderText { get; set; }
    }
    public class ResponseAdminApi
    {
        public bool HasError { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
    public class AdminGraphCount
    {
        public string StatusName { get; set; }
        public int Monday { get; set; }
        public int Tuesday { get; set; }
        public int Wednesday { get; set; }
        public int Thursday { get; set; }
        public int Friday { get; set; }
        public int Saturday { get; set; }
        public int Sunday { get; set; }
    }
    public class stp_GetDriversList
    {
        public int Id { get; set; }
        public string No { get; set; }
        public string Name { get; set; }
        public string VehicleNo { get; set; }
        public string VehicleType { get; set; }
        public string DrvBadge { get; set; }
        public string VehBadge { get; set; }
        public string NI { get; set; }
        public DateTime? MOTExpiry { get; set; }
        public DateTime? MOT2Expiry { get; set; }
        public DateTime? PCOVehicleExpiry { get; set; }
        public DateTime? InsuranceExpiry { get; set; }
        public DateTime? PCODriverExpiry { get; set; }
        public DateTime? LicenseExpiry { get; set; }
        public DateTime? RoadTaxExpiry { get; set; }
        public string MobileNo { get; set; }
        public DateTime? EndDate { get; set; }
        public int? DriverTypeId { get; set; }

        public string MOTExpirystr { get; set; }
        public string MOT2Expirystr { get; set; }
        public string PCOVehicleExpirystr { get; set; }
        public string InsuranceExpirystr { get; set; }
        public string PCODriverExpirystr { get; set; }
        public string LicenseExpirystr { get; set; }
        public string RoadTaxExpirystr { get; set; }
        public string EndDatestr { get; set; }
        public DateTime? TFLCheckExpiryDate { get; set; }
        public DateTime? RightToWorkExpiryDate { get; set; }

        public string TFLCheckExpirystr { get; set; }
        public string RightToWorkExpirystr { get; set; }
        public string Surname { get; set; }
    }
    public class stp_GetDriverRecord
    {
        public int Id { get; set; }
        public string No { get; set; }
        public string Name { get; set; }
        public string VehicleNo { get; set; }
        public string VehicleType { get; set; }
        public string DrvBadge { get; set; }
        public string VehBadge { get; set; }
        public string NI { get; set; }
        public DateTime? MOTExpiry { get; set; }
        public DateTime? MOT2Expiry { get; set; }
        public DateTime? PCOVehicleExpiry { get; set; }
        public DateTime? InsuranceExpiry { get; set; }
        public DateTime? PCODriverExpiry { get; set; }
        public DateTime? LicenseExpiry { get; set; }
        public DateTime? RoadTaxExpiry { get; set; }
        public string MobileNo { get; set; }
        public DateTime? EndDate { get; set; }
        public int? DriverTypeId { get; set; }
        public bool? IsActive { get; set; }
        public string DriverNo { get; set; }
        public string LoginPassword { get; set; }
        public string LoginId { get; set; }
        public bool? PDALoginBlocked { get; set; }
        public string DriverName { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string TelephoneNo { get; set; }
        public string Nationality { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public int? VehicleTypeId { get; set; }
        public decimal? InitialBalance { get; set; }
        public decimal? RentLimit { get; set; }
        public string VehicleOwner { get; set; }
        public string VehicleModel { get; set; }
        public string VehicleMake { get; set; }
        public string VehicleColor { get; set; }
        public int? SubcompanyId { get; set; }
        public int? DriverCategory { get; set; }
        public bool? HasPDA { get; set; }
        public bool? EnableBidding { get; set; }
        public string City { get; set; }
        public string PostCode { get; set; }
        public decimal? MaxCommission { get; set; }
        public string NICNo { get; set; }
        public byte[] photo { get; set; }
        public decimal? DriverMonthlyRent { get; set; }
        public decimal? DriverCommissionPerBooking { get; set; }
        public decimal? PDARent { get; set; }
        public decimal? VAT { get; set; }
        public DateTime? LicenseExpiryDate { get; set; }
        public bool? UseCompanyVehicle { get; set; }
        public decimal? CarRent { get; set; }
        public decimal? CarInsuranceRent { get; set; }
        public decimal? PrimeCompanyRent { get; set; }
        public DateTime? InsuranceExpiryDate { get; set; }
        public DateTime? MOTExpiryDate { get; set; }
        public DateTime? PCODriverExpiryDate { get; set; }
        public DateTime? MOT2ExpiryDate { get; set; }
        public DateTime? DrivingLicenseExpiryDate { get; set; }
        public DateTime? RoadTaxiExpiryDate { get; set; }

        public string MOTExpirystr { get; set; }
        public string MOT2Expirystr { get; set; }
        public string PCOVehicleExpirystr { get; set; }
        public string InsuranceExpirystr { get; set; }
        public string PCODriverExpirystr { get; set; }
        public string LicenseExpirystr { get; set; }
        public string DrivingLicenseExpirystr { get; set; }

        public string RoadTaxExpirystr { get; set; }
        public string RoadTaxiExpirystr { get; set; }
        public string EndDatestr { get; set; }
        public string LicenseExpiryDatestr { get; set; }
        public string DateOfBirthstr { get; set; }
    }
    public class stp_GetDriverRecord1
    {
        public string Comments { get; set; }
        public string DataAllowance { get; set; }
        public string IMEINumber { get; set; }
        public string DeviceMake { get; set; }
        public string DeviceModel { get; set; }
        public string NetworkAPN { get; set; }
        public string SIMNetworkName { get; set; }
        public string SIMNumber { get; set; }
        public string DeviceDeposits { get; set; }
        public DateTime? DeviceDateGiven { get; set; }
    }

    public class stp_GetDriverRecord2
    {
        public long ID { get; set; }
        public int MASTERID { get; set; }
        public string FILENAME { get; set; }
        public string FULLPATH { get; set; }
        public string BADGENUMBER { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public string ExpiryDatestr { get; set; }

        public int DocumentId { get; set; }

        public string DocumentName { get; set; }

    }

    public class stp_GetDriverRecord3
    {
        public int ID { get; set; }
        public int MASTERID { get; set; }
        public DateTime? BECAMEAVAIL { get; set; }
        public DateTime? ENDINGDATE { get; set; }
        public string BECAMEAVAILstr { get; set; }
        public string ENDINGDATEstr { get; set; }
    }
    public class stp_GetDriverRecord4
    {
        public int ID { get; set; }
        public int MASTERID { get; set; }
        public decimal? FROMPRICE { get; set; }
        public decimal? TOPRICE { get; set; }
        public decimal? COMMISSIONPERCENT { get; set; }
    }
    public class stp_GetDriverRecord5
    {
        public int Id { get; set; }
        public string Notes { get; set; }
        public int DriverId { get; set; }
        public DateTime? Time { get; set; }
        public DateTime? AddOnTime { get; set; }
        public DateTime? AddOn { get; set; }
        public string AddBy { get; set; }
        public string TimeStr { get; set; }
        public string AddOnTimeStr { get; set; }
        public string AddOnStr { get; set; }

    }

    public class stp_GetDriverRecord6
    {
        public int DriverId { get; set; }
        public int DriverRents_Count { get; set; }
    }
    public class stp_GetDriverRecord7
    {
        public int DriverId { get; set; }
        public DateTime? LicenseExpiry { get; set; }
        public DateTime? PCOVehicleExpiry { get; set; }
        public DateTime? PCODriverExpiryDate { get; set; }
        public DateTime? MOTExpiryDate { get; set; }
        public DateTime? MOT2ExpiryDate { get; set; }
        public DateTime? RoadTaxiExpiryDate { get; set; }
        public DateTime? DrivingLicenseExpiryDate { get; set; }
        public string DriverNo { get; set; }
        public decimal? MaxCommission { get; set; }
        public int VehicleTypeId { get; set; }
        public string VehicleNo { get; set; }
        public string LicenseExpirystr { get; set; }
        public string PCOVehicleExpirystr { get; set; }
        public string PCODriverExpirystr { get; set; }
        public string MOTExpirystr { get; set; }
        public string MOT2Expirystr { get; set; }
        public string RoadTaxiExpirystr { get; set; }
        public string DrivingLicenseExpirystr { get; set; }

    }

    public class stp_GetDriverStatementCombo
    {
        public int Id { get; set; }
        public string DriverName { get; set; }
        public string NICNo { get; set; }
        public string VehicleDetails { get; set; }
        public int? DriverTypeId { get; set; }
        public decimal DriverCommissionPerBooking { get; set; }
        public decimal DriverAccCommissionPerBooking { get; set; }
        public decimal PDARent { get; set; }
        //public decimal CarRent { get; set; }
        //public decimal CarInsuranceRent { get; set; }
        //public decimal PrimeCompanyRent { get; set; }
        public decimal VAT { get; set; }
        public bool IsPrimeCompanyDriver { get; set; }
        public bool UseCompanyVehicle { get; set; }
        public int SubcompanyId { get; set; }
        //public long Id_DrvRent { get; set; }
        //public string TransNo_DrvRent { get; set; }
        //public int DriverId_DrvRent { get; set; }
        //public decimal DriverRent { get; set; }
        public decimal? Balance { get; set; }
        public decimal? OldBalance { get; set; }
        //public decimal OldBalance_Rent { get; set; }
        //public decimal OldBalance_Commission { get; set; }
        public decimal InitialBalance { get; set; }

        public long? Id_DrvCommission { get; set; }
        public string TransNo_DrvCommission { get; set; }

        //public int DriverId_DrvCommission { get; set; }

        public decimal MaxCommission { get; set; }
        public decimal DriverMonthlyRent { get; set; }
        public decimal TotalpdaRent { get; set; }
        public decimal TotalCarRent { get; set; }
        public decimal TotalCarInsuranceRent { get; set; }
        public decimal TotalPrimeCompanyRent { get; set; }
        public decimal AccountTotal { get; set; }
        public decimal agentFeesTotal { get; set; }
        public decimal extraTotal { get; set; }
        public decimal parkingTotal { get; set; }
        public decimal? CurrentBalance { get; set; }
        public decimal? DriverRent1 { get; set; }
        public decimal? CommissionTotal { get; set; }
    }
    public class GetFleetDriverCommission
    {
        public long Id { get; set; }
        public string TransNo { get; set; }
        public DateTime? TransDate { get; set; }
        public int? DriverId { get; set; }
        public decimal? JobsTotal { get; set; }
        public DateTime? AddOn { get; set; }
        public int? AddBy { get; set; }
        public DateTime? EditOn { get; set; }
        public int? EditBy { get; set; }
        public string AddLog { get; set; }
        public string EditLog { get; set; }
        public int? BookingTypeId { get; set; }
        public decimal? DriverCommision { get; set; }
        public decimal? Balance { get; set; }
        public decimal? OldBalance { get; set; }
        public decimal? CommisionPay { get; set; }
        public string Remarks { get; set; }
        public decimal? Fuel { get; set; }
        public decimal? Extra { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string TransFor { get; set; }
        public bool? IsCreditOrDebit { get; set; }
        public decimal? PDARent { get; set; }
        public int? CommisionPayReasonId { get; set; }
        public decimal? AccJobsTotal { get; set; }
        public decimal? CommissionTotal { get; set; }
        public decimal? DriverOwed { get; set; }
        public decimal? AgentFeesTotal { get; set; }
        public decimal? AccountExpenses { get; set; }
        public decimal? CollectionDeliveryCharges { get; set; }
        public int? AccountBookingDays { get; set; }
        public int? TotalWeeks { get; set; }
        public decimal? TotalServiceCharges { get; set; }
        public bool? WeekOff { get; set; }
        public int? TransactionType { get; set; }
        public decimal? OldAgentCommission { get; set; }
        public bool? IsPaid { get; set; }
        public decimal? Adjustments { get; set; }
        public bool? IsWeeklyPaid { get; set; }
        public decimal? OldCollectionCharges { get; set; }
        public decimal? OldAdjustments { get; set; }
        public decimal? MaxCommission { get; set; }
        public decimal? VAT { get; set; }
    }
    public class DriverCommisExpenses
    {
        public long Id { get; set; }
        public long? CommissionId { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public DateTime? Date { get; set; }
        public string Description { get; set; }
        public decimal? Amount { get; set; }
        public bool? IsPaid { get; set; }
        public string AddBy { get; set; }
    }

    public class BookingDriverCommLister
    {
        public long Id { get; set; }
        // public long InvoiceId { get; set; }
        public DateTime? BookingDate { get; set; }
        public DateTime? PickupDate { get; set; }
        public string BookingDateStr { get; set; }
        public string PickupDateStr { get; set; }
        public string RefNo { get; set; }
        public string Vehicle { get; set; }
        public string OrderNo { get; set; }
        public string PupilNo { get; set; }
        public string Passenger { get; set; }
        public string PickupPoint { get; set; }
        public string Destination { get; set; }
        public decimal? Charges { get; set; }
        public int? CompanyId { get; set; }
        public string Account { get; set; }
        public decimal? Parking { get; set; }
        public decimal? Waiting { get; set; }
        public decimal? ExtraDrop { get; set; }
        public decimal? MeetAndGreet { get; set; }
        public decimal? Congtion { get; set; }
        public string Description { get; set; }
        public decimal? Total { get; set; }
        public string BookedBy { get; set; }
        public decimal? Fare { get; set; }
        public int? AccountType { get; set; }
        public bool? IsCommissionWise { get; set; }
        public string DriverCommissionType { get; set; }
        public decimal? DriverCommission { get; set; }
        public decimal? AgentCommission { get; set; }
        public decimal? DropOffCharge { get; set; }
        public decimal? PickupCharge { get; set; }
        public int? PaymentTypeId { get; set; }
        public string Payment { get; set; }
        public decimal? ServiceCharges { get; set; }
        public string PickupPlot { get; set; }
        public string DropOffPlot { get; set; }
        public decimal? Promotion { get; set; }
        public int? BookingStatusId { get; set; }
        public string StatusName { get; set; }
    }

    public class stp_DriverCommissionLastStatement
    {
        public int? DriverId;
        public string DriverNo;
        public string DriverName;
        public long? Id;
        public decimal? JobsTotal;
        public decimal? Balance;
        public decimal? OldBalance;
        public DateTime? TransDate;
        public string TransNo;
        public DateTime? FromDate;
        public DateTime? ToDate;
    }
    public class stp_getdrivercommlist2
    {
        public long? Id { get; set; }
        public string DriverNo { get; set; }
        public string DriverName { get; set; }
        public string MobileNo { get; set; }
        public DateTime? TransDate { get; set; }
        public long? TotalRentCount { get; set; }
    }
    public class DriverCommissionSendEmailRequest
    {
        public string TransNo { get; set; }
        public int DriverId { get; set; }
        public string CommissionId { get; set; }
        public string CustomerEmail { get; set; }
        public bool UseDifferentEmailForInvoices { get; set; }
        public int? SubcompanyId { get; set; }
    }
    public class EmailInfo2
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public string MessageBody { get; set; }
        //public bool HideFares { get; set; }
        public string TransNo { get; set; }
        public int SubCompanyId { get; set; }
        //public bool IsAccountJob { get; set; }
        //public int PaymentTypeId { get; set; }
        public int DriverId { get; set; }
        public string CommissionId { get; set; }
    }
    public class BookingDriverRentLister
    {
        public long Id { get; set; }
        // public long InvoiceId { get; set; }
        public DateTime? BookingDate { get; set; }
        public DateTime? PickupDate { get; set; }
        public string BookingDateStr { get; set; }
        public string PickupDateStr { get; set; }
        public string RefNo { get; set; }
        public string Vehicle { get; set; }
        public string OrderNo { get; set; }
        public string PupilNo { get; set; }
        public string Passenger { get; set; }
        public string PickupPoint { get; set; }
        public string Destination { get; set; }
        public decimal? Charges { get; set; }
        public int? CompanyId { get; set; }
        public string Account { get; set; }
        public decimal? Parking { get; set; }
        public decimal? Waiting { get; set; }
        public decimal? ExtraDrop { get; set; }
        public decimal? MeetAndGreet { get; set; }
        public decimal? Congtion { get; set; }
        public string Description { get; set; }
        public decimal? Total { get; set; }
        public string BookedBy { get; set; }
        public decimal? Fare { get; set; }
        public int? AccountType { get; set; }
        public bool? IsCommissionWise { get; set; }
        public string DriverCommissionType { get; set; }
        public decimal? DriverCommission { get; set; }
        public decimal? AgentCommission { get; set; }
        public int? PaymentTypeId { get; set; }
        public decimal? DropOffCharge { get; set; }
        public decimal? PickupCharge { get; set; }
        public decimal? ExtrCharges { get; set; }
        public decimal? BookingFee { get; set; }
        public int? BookingStatusId { get; set; }
        public string StatusName { get; set; }
        public string PaymentType { get; set; }
    }
    public class GetCalcDriverStatement
    {
        public long Id { get; set; }
        // public long InvoiceId { get; set; }
        public DateTime? BookingDate { get; set; }
        public DateTime? PickupDate { get; set; }
        public string BookingDateStr { get; set; }
        public string PickupDateStr { get; set; }
        public string RefNo { get; set; }
        public string Vehicle { get; set; }
        public string OrderNo { get; set; }
        public string PupilNo { get; set; }
        public string Passenger { get; set; }
        public string PickupPoint { get; set; }
        public string Destination { get; set; }
        public decimal? Charges { get; set; }
        public int? CompanyId { get; set; }
        public string Account { get; set; }
        public decimal? Parking { get; set; }
        public decimal? Waiting { get; set; }
        public decimal? ExtraDrop { get; set; }
        public decimal? MeetAndGreet { get; set; }
        public decimal? Congtion { get; set; }
        public string Description { get; set; }
        public decimal? Total { get; set; }
        public string BookedBy { get; set; }
        public decimal? Fare { get; set; }
        public int? AccountType { get; set; }
        public bool? IsCommissionWise { get; set; }
        public string DriverCommissionType { get; set; }
        public decimal? DriverCommission { get; set; }
        public decimal? AgentCommission { get; set; }
        public int? PaymentTypeId { get; set; }
        public decimal? DropOffCharge { get; set; }
        public decimal? PickupCharge { get; set; }
        public decimal? ExtrCharges { get; set; }
        public decimal? BookingFee { get; set; }
        public int? BookingStatusId { get; set; }
        public string StatusName { get; set; }
        public string PaymentType { get; set; }
    }
    public class GetCalcDriverCommStatement
    {
        public long Id { get; set; }
        // public long InvoiceId { get; set; }
        public DateTime? BookingDate { get; set; }
        public DateTime? PickupDate { get; set; }
        public string BookingDateStr { get; set; }
        public string PickupDateStr { get; set; }
        public string RefNo { get; set; }
        public string Vehicle { get; set; }
        public string OrderNo { get; set; }
        public string PupilNo { get; set; }
        public string Passenger { get; set; }
        public string PickupPoint { get; set; }
        public string Destination { get; set; }
        public decimal? Charges { get; set; }
        public int? CompanyId { get; set; }
        public string Account { get; set; }
        public decimal? Parking { get; set; }
        public decimal? Waiting { get; set; }
        public decimal? ExtraDrop { get; set; }
        public decimal? MeetAndGreet { get; set; }
        public decimal? Congtion { get; set; }
        public string Description { get; set; }
        public decimal? Total { get; set; }
        public string BookedBy { get; set; }
        public decimal? Fare { get; set; }
        public int? AccountType { get; set; }
        public bool? IsCommissionWise { get; set; }
        public string DriverCommissionType { get; set; }
        public decimal? DriverCommission { get; set; }
        public decimal? AgentCommission { get; set; }
        public decimal? DropOffCharge { get; set; }
        public decimal? PickupCharge { get; set; }
        public int? PaymentTypeId { get; set; }
        public string Payment { get; set; }
        public decimal? ServiceCharges { get; set; }
        public string PickupPlot { get; set; }
        public string DropOffPlot { get; set; }
        public decimal? Promotion { get; set; }
        public int? BookingStatusId { get; set; }
        public string StatusName { get; set; }
    }

    public class stp_AddAllDriverCommissionList
    {
        public int Id { get; set; }
        public string Driver { get; set; }
        public decimal PDARent { get; set; }
        public decimal? DriverCommissionPerBooking { get; set; }
        public string Email { get; set; }
        public decimal OldBalance { get; set; }
        public decimal InitialBalance { get; set; }
        public decimal JobsTotal { get; set; }
        public decimal RentDue { get; set; }
        public long CommissionId { get; set; }
        public decimal? CollectionCharges { get; set; }
        public decimal VAT { get; set; }
        public int? SubcompanyId { get; set; }
    }
    public class AllDriverCommissionGetBookingRequest
    {
        public DateTime? RentFromDateTime { get; set; }
        public DateTime? RentToDateTime { get; set; }
        public List<stp_AddAllDriverCommissionList> AddAllDriverCommission { get; set; }
        //public int Id { get; set; }
        public long[] Id { get; set; }
        public int PickBookingOnInvoicingType { get; set; }
        public int PaymentTypeId { get; set; }
    }
    public class AllDriverCommissionGetDisplayCommissionRequest
    {
        public DateTime? RentFromDateTime { get; set; }
        public DateTime? RentToDateTime { get; set; }
        public List<stp_AddAllDriverCommissionList> AddAllDriverCommission { get; set; }
        //public int Id { get; set; }
        public int[] Id { get; set; }
        public int PickBookingOnInvoicingType { get; set; }
        public int PaymentTypeId { get; set; }
        public int subCompanyId { get; set; }
        public int NoCommissionFromAccount { get; set; }
        public int chkApplyBookingFees { get; set; }
    }
    public class addDriverCommission
    {
        public long id { get; set; }
        public long Booking { get; set; }
    }

    public class stp_getDriverListForGeneratingCommResult2
    {
        public long Id;
        public int? Driverid;
        public decimal? TotalFare;
        public int? CompanyId;
        public int AccountTypeId;
        public decimal? DriverCommissionAmount;
        public string DriverCommissionType;
        public bool? IsCommissionWise;
        public decimal? AgentCommission;
        public int DropOffCharge;
        public int PickupCharge;
        public decimal Parking;
        public decimal BookingFee;
        public decimal ExtraDropCharges;
        public string PickupPlot;
        public string DropOffPlot;
        public int? PaymentTypeId;
        public decimal? Promotion;
        public decimal? Waiting;
        public decimal? FareRate;
    }
    public class stp_AddAllDriverCommission
    {
        public int Id { get; set; }
        public string Driver { get; set; }
        public decimal PDARent { get; set; }
        public decimal? DriverCommissionPerBooking { get; set; }
        public string Email { get; set; }
        public decimal OldBalance { get; set; }
        public decimal InitialBalance { get; set; }
        public decimal JobsTotal { get; set; }
        public decimal RentDue { get; set; }
        public decimal current { get; set; }
        public long CommissionId { get; set; }
        public decimal? CollectionCharges { get; set; }
        public decimal VAT { get; set; }
        public int? SubcompanyId { get; set; }
    }

    public class DisplayCommissionDataList
    {
        public string Email { get; set; }
        public int Id { get; set; }
        public int DriverNo { get; set; }
        public decimal? DriverCommission { get; set; }
        public decimal? DriverCommissionPerBooking { get; set; }
        public decimal? DriverPDARent { get; set; }
        public decimal? CommissionPay { get; set; }
        public decimal? OldBalance { get; set; }
        public decimal? InitialBalance { get; set; }
        public int? SubCompanyId { get; set; }
        public decimal? CurrBalance { get; set; }
        public decimal? TotalPDARent { get; set; }
        public decimal? BookingFees { get; set; }
        public decimal? AccountsTotal { get; set; }
        public decimal? AccountExpense { get; set; }
        public decimal? Owed { get; set; }
        public int? CommissionId { get; set; }
        public decimal? CashTotal { get; set; }
        public decimal? JobsTotal { get; set; }
        public decimal? AgentFees { get; set; }
        public string DriverEmail { get; set; }
        public decimal? PaidPromotion { get; set; }
        public decimal? Promotion { get; set; }
        public decimal? CollectionAndDelivery { get; set; }
        public decimal? TotalCollectionAndDelivery { get; set; }
        public bool? Holiday { get; set; }
        public decimal? VAT { get; set; }
        public int? Bookings { get; set; }
    }

    public class SaveAllDriverCommissionRequest
    {
        public DateTime? RentFromDateTime { get; set; }
        public DateTime? RentToDateTime { get; set; }
        public int? SubCompanyId { get; set; }
        public string Email { get; set; }
        public int Id { get; set; }
        public int? DriverNo { get; set; }
        public decimal? DriverCommission { get; set; }
        public decimal DriverCommissionPerBooking { get; set; }
        public decimal DriverPDARent { get; set; }
        public decimal CommissionPay { get; set; }
        public decimal OldBalance { get; set; }
        public decimal InitialBalance { get; set; }
        public decimal? CurrBalance { get; set; }
        public decimal? TotalPDARent { get; set; }
        public decimal? BookingFees { get; set; }
        public decimal? AccountsTotal { get; set; }
        public decimal? AccountExpense { get; set; }
        public decimal? Owed { get; set; }
        public int? CommissionId { get; set; }
        public decimal? CashTotal { get; set; }
        public decimal? JobsTotal { get; set; }
        public int? AgentFees { get; set; }
        public string DriverEmail { get; set; }
        public int? PaidPromotion { get; set; }
        public int? Promotion { get; set; }
        public int? CollectionAndDelivery { get; set; }
        public int? TotalCollectionAndDelivery { get; set; }
        public bool Holiday { get; set; }
        public int VAT { get; set; }
        public int? Bookings { get; set; }
    }

    public class ClsRowState
    {
        //public GridViewRowInfo row;
        public int subCompanyId;
        public decimal currBlance;
        public long transId;
        public decimal accJobsTotal;
        public decimal rentDue;
        public int DriverID;
    }
    public class SysPolicyConfg
    {
        // HourControllerReport
        public int HourControllerReport { get; set; }
        // BookingExpiryNoticeInMins
        public int BookingExpiryNoticeInMins { get; set; }
        // DeadMileage
        public decimal DeadMileage { get; set; }
        // ApplyStartRateWithInMiles
        public decimal ApplyStartRateWithInMiles { get; set; }
        // RoundUpTo
        public decimal RoundUpTo { get; set; }
        // RoundJourneyMiles
        public decimal RoundJourneyMiles { get; set; }
        // GridRowSize
        public int GridRowSize { get; set; }
        // DiscountForReturnedJourneyPercent
        public int DiscountForReturnedJourneyPercent { get; set; }
        public int DiscountForWRJourneyPercent { get; set; }
        public int ListingPagingSize { get; set; }
        public string AccJobsShowNotificationDay { get; set; }
        public int AdvanceBookingSMSConfirmationMins { get; set; }
        // WaitAndReturnDiscountType
        public int WaitAndReturnDiscountType { get; set; }
        // DaysInTodayBooking
        public int DaysInTodayBooking { get; set; }
        // EnableApplyStartRateWithInMiles
        public bool EnableApplyStartRateWithInMiles { get; set; }
        // RoundMileageFares
        public bool RoundMileageFares { get; set; }
        // AutoShowBookingNearestDrv
        public bool AutoShowBookingNearestDrv { get; set; }
        // AutoCalculateFares
        public bool AutoCalculateFares { get; set; }
        // EnableReplaceNoToZoneSuggesstion
        public bool EnableReplaceNoToZoneSuggesstion { get; set; }
        // EnableAdvanceBookingSMSConfirmation
        public bool EnableAdvanceBookingSMSConfirmation { get; set; }
        // DeadMileageType
        public int DeadMileageType { get; set; }
        // EnableQuotation
        public bool EnableQuotation { get; set; }
        // FareMeterType
        public int FareMeterType { get; set; }
        // EnableZoneWiseFares
        public bool EnableZoneWiseFares { get; set; }
        // PreferredMileageFares
        public bool PreferredMileageFares { get; set; }
        // EnablePassengerText
        public bool EnablePassengerText { get; set; }
        // EnableArrivalBookingText
        public bool EnableArrivalBookingText { get; set; }
        // BookingAlertExpiryNoticeInMins
        public int BookingAlertExpiryNoticeInMins { get; set; }
        // ApplyAccBgColorOnRow
        public bool ApplyAccBgColorOnRow { get; set; }
        // DisablePopupNotifications
        public bool DisablePopupNotifications { get; set; }
        public bool EnablePeakOffPeakFares { get; set; }
        public int RecentBookingDays { get; set; }
    }
    public class callerId
    {
        public bool IsVOIP { get; set; }
        public int VOIPCLIType { get; set; }
        public string Port { get; set; }
        public string AccountId { get; set; }
        public string Host { get; set; }
        public string SIP_BTProxy { get; set; }
        public string CallRecordingToken { get; set; }
        public string txtIVRNumbers { get; set; }
        public string Password { get; set; }
    }
    public class sMS
    {
        public string ClickSMSUserName { get; set; }
        public string ClickSMSPassword { get; set; }
        public string ClickSMSSenderName { get; set; }
        public string ModemSMSPortName { get; set; }
        public string ClickSMSApiKey { get; set; }
        public string DespatchTextForDriver { get; set; }
        public string DespatchTextForCustomer { get; set; }
        public string ConfirmationSMSText { get; set; }
        public string AdvanceBookingSMSText { get; set; }
        public string ArrivalBookingText { get; set; }
        public string ArrivalAirportBookingText { get; set; }
        public string DespatchTextForPDA { get; set; }
        public string SMSNoPickup { get; set; }
        public string SMSCancelJob { get; set; }
        public string WebBookingText { get; set; }

    }
    public class stp_GetPendingInvoicesEx
    {
        public long? CreditNoteId { get; set; }
        public decimal? CreditNoteTotal { get; set; }
        public decimal? DiscountAmount { get; set; }
        public bool? Vat { get; set; }
        public string ContactName { get; set; }
        public string CompanyName { get; set; }
        public string CompanyCode { get; set; }
        public int? InvoicePaymentTypeID { get; set; }
        public decimal? CreditNoteAmount { get; set; }
        public int? CompanyId { get; set; }
        public decimal? OldBalance { get; set; }
        public decimal? CurrentBalance { get; set; }
        public decimal? TotalInvoiceAmount { get; set; }
        public decimal? InvoiceTotal { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string StrDueDate { get; set; }
        public string StrInvoiceDate { get; set; }
        public string InvoiceNo { get; set; }
        public long Id { get; set; }
        public decimal? PaidAmount { get; set; }
        public string TelephoneNo { get; set; }
        public decimal? NetTotal { get; set; }
        public int? AdminFees { get; set; }
        public string AdminFeeType { get; set; }
        public bool? VatOnlyOnAdminFees { get; set; }
        public decimal? DiscountPercentage { get; set; }

    }
    public class BookingLister
    {
        public long Id { get; set; }
        // public long InvoiceId { get; set; }
        public DateTime? BookingDate { get; set; }
        public DateTime? PickupDate { get; set; }
        public string RefNo { get; set; }
        public string Vehicle { get; set; }
        public string OrderNo { get; set; }
        public string PupilNo { get; set; }
        public string Passenger { get; set; }
        public string PickupPoint { get; set; }
        public string Destination { get; set; }
        public decimal? Charges { get; set; }
        public int? CompanyId { get; set; }
        public string CompanyName { get; set; }
        public decimal? Parking { get; set; }
        public decimal? Waiting { get; set; }
        public decimal? ExtraDrop { get; set; }
        public decimal? MeetAndGreet { get; set; }
        public decimal? Congtion { get; set; }
        public string Description { get; set; }
        public decimal? Total { get; set; }
        public string BookedBy { get; set; }
        public decimal? Fare { get; set; }
        public int? AccountType { get; set; }
        public int? PaymentTypeId { get; set; }
        public int? BookingStatusId { get; set; }
        public string PaymentType { get; set; }
        public decimal? BookingFee { get; set; }
        public decimal? WaitingTime { get; set; }
        public string ViaString { get; set; }
        public decimal? EscortPrice { get; set; }
        public string Status { get; set; }
    }
    public class AccountInvoicehistory
    {
        public string InvoiceId { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public int CompanyId { get; set; }
    }
    public class stp_GetInvoiceBookingsResultEx
    {
        public long Id { get; set; }

        public string InvoiceNo { get; set; }
        public long? BookingId { get; set; }
        public long InvoiceId { get; set; }
        public int? InvoicePaymentTypeId { get; set; }
        public DateTime? PickupDateTime { get; set; }
        public string OrderNo { get; set; }
        public string PupilNo { get; set; }
        public int? VehicleTypeId { get; set; }
        public string BookingNo { get; set; }
        public decimal? CompanyPrice { get; set; }
        public decimal? ParkingCharges { get; set; }
        public decimal? WaitingCharges { get; set; }
        public decimal? ExtraDropCharges { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public decimal? TotalCharges { get; set; }
        public string CustomerName { get; set; }
        public int? PaymentTypeId { get; set; }
        public int? BookingStatusId { get; set; }
        public string VehicleType { get; set; }
        public string DepartmentName { get; set; }
        public decimal? TipAmount { get; set; }
        public decimal? BookingFee { get; set; }
        public string BookingBookedBy { get; set; }
        public string ViaString { get; set; }
        public int? CompanyId { get; set; }
    }

    public class Gen_SysPolicy_AirportPickupCharges2
    {
        public int Id { get; set; }
        public int SysPolicyId { get; set; }
        public int AirportId { get; set; }
        public decimal? Charges { get; set; }
        public int? VehicleTypeId { get; set; }
    }
    public class Gen_SysPolicy_Configurations2
    {
        public int? SysPolicyId { get; set; }
        public decimal? AirportPickupCharges { get; set; }
        public decimal? CreditCardChargesType { get; set; }
        public decimal? CreditCardExtraCharges { get; set; }
        public bool? HasMultipleAirportPickupCharges { get; set; }
        public decimal? CashCallCharges { get; set; }
        public decimal? DriverMonthlyRent { get; set; }
        public bool RentForProcessedJobs { get; set; }
        public DateTime? RentFromDateTime { get; set; }
        public DateTime? RentToDateTime { get; set; }
        public DateTime? DriverSuspensionDateTime { get; set; }
        public decimal? DriverCommissionPerBooking { get; set; }
        public bool? PickCommissionFromCharges { get; set; }
        public bool? PickCommissionFromChargesAndWaiting { get; set; }
        public bool? NoCommissionFromAccount { get; set; }
        public bool? HasAirportDropOffCharges { get; set; }
    }
    public class Gen_SysPolicy_AirportDropOffCharges2
    {
        public int Id { get; set; }
        public int SysPolicyId { get; set; }
        public int AirportId { get; set; }
        public decimal? Charges { get; set; }
    }

    public class Gen_Charges2
    {
        public int Id { get; set; }
        public string ChargesName { get; set; }
        public bool IsVisible { get; set; }
    }
    public class Gen_SysPolicy_CommissionPriceRange2
    {
        public int Id { get; set; }
        public decimal? FromPrice { get; set; }
        public decimal? ToPrice { get; set; }
        public decimal? CommissionValue { get; set; }
        public int SysPolicyId { get; set; }

    }
    public class ModeList
    {

        public int Id { get; set; }

        public string Modes { get; set; }

    }

    public class ClsBookingTypes
    {
        public int Id;
        public string BookingTypeName;

    }
    public class ClsAutoDespatch
    {
        public int BookingType { get; set; }
        public decimal? Radius { get; set; }
        public string OtherData { get; set; }
    }
    public class stp_DriverStatsData
    {
        public int? DriverId { get; set; }
        public string DriverNo { get; set; }
        public string DriverName { get; set; }
        public string LoginTime { get; set; }
        public int? JobsDone { get; set; }
        public decimal? Earned { get; set; }
        public string WaitingSince { get; set; }
    }
    public class NewCallHistory
    {
        public string CalledToNumber { get; set; }
        public DateTime? AnsweredDateTime { get; set; }
        public int Id { get; set; }
        public DateTime? CallDateTime { get; set; }
        public DateTime? FromTime { get; set; }
        public DateTime? ToTime { get; set; }


    }
    public class GraphData
    {
        public string DespatchBy { get; set; }
        public int TotalDespatch { get; set; }

        public string ControllerName { get; set; }
        public int TotalCallRecive { get; set; }

        public int AccountBookings { get; set; }
        public int CashBookings { get; set; }
        public string EarningDriverName { get; set; }

        public decimal DriverTotalEarning { get; set; }

    }
    public class LocalizationDetail
    {
        public int DetailId { get; set; }
        public string PostCode { get; set; }
    }
    public class Address
    {
        public long Id { get; set; }
        public string AddressName { get; set; }
        public string AddressLine1 { get; set; }

    }

    public class ManagePlotsZoneRequest
    {
        public List<ManagePlotsZone2> ManagePlotsZoneList { get; set; }

    }
    public class ManagePlotsZone2
    {
        public int Id { get; set; }
        public DateTime? FlashingHour { get; set; }
        public DateTime? JobDueTime { get; set; }
        public bool EnableAutoDespatch { get; set; }
        public bool EnableBidding { get; set; }
        public int BiddingRadius { get; set; }
        public List<BackupZoneDataList> Gen_Zone_BackupsList { get; set; }
        public string PlotEntranceMessage { get; set; }
        public int PlotLimit { get; set; }

        public string PlotLimitExceedMessage { get; set; }
        public bool BlockDropOff { get; set; }

        public int? BackupPlot1 { get; set; }
        public int? BackupPlot2 { get; set; }
        public bool? BackupPlot1Tag { get; set; }
        public bool? BackupPlot2Tag { get; set; }


    }
    public class BackupZoneDataList
    {
        public int Id { get; set; }
        public int ZoneId { get; set; }
        public int? BackupZone1Id { get; set; }
        public int? BackupZone2Id { get; set; }
        public int? BackupZone3Id { get; set; }
        public int? BackupZone4Id { get; set; }
        public int? BackupZone5Id { get; set; }
        public int? BackupZone6Id { get; set; }
        public int? BackupZone7Id { get; set; }
        public int? BackupZone8Id { get; set; }
        public int? BackupZone9Id { get; set; }
        public int? BackupZone10Id { get; set; }
        public int? BackupZone11Id { get; set; }
        public int? BackupZone12Id { get; set; }
        public int? BackupZone13Id { get; set; }
        public int? BackupZone14Id { get; set; }
        public int? BackupZone15Id { get; set; }
        public int? BackupZone16Id { get; set; }
        public int? BackupZone17Id { get; set; }
        public int? BackupZone18Id { get; set; }
        public int? BackupZone19Id { get; set; }
        public int? BackupZone20Id { get; set; }
        public bool? BackupZone1Priority { get; set; }
    }

    public class ZonesPlot
    {
        public int? Id { get; set; }
        public int? OrderNo { get; set; }
        public string ZoneName { get; set; }
        public string ShortName { get; set; }
        public DateTime? FlashingHour { get; set; }
        public bool? EnableAutoDespatch { get; set; }
        public bool? EnableBidding { get; set; }
        public int? BiddingRadius { get; set; }
        //a.Gen_Zone_Backups,
        public int? PlotLimit { get; set; }
        public string PlotEntranceMessage { get; set; }
        public string PlotLimitExceedMessage { get; set; }
        public DateTime? JobDueTime { get; set; }
        public bool? BlockDropOff { get; set; }
    }
    public class Localization
    {
        public long PostCodeId { get; set; }
        public string PostCode { get; set; }

    }

    public class OnlineBookingSettings
    {
        public int? Id { get; set; }
        public bool? BlockWebBooking { get; set; }
        public bool? BlockAppBooking { get; set; }
        public string CallConnectHostName { get; set; }
        public string CallConnectUserName { get; set; }
        public string CallConnectPassword { get; set; }
        public int? CallConnectPort { get; set; }
        public bool? IsCallConnectActive { get; set; }
        public string Description { get; set; }

    }
    public class OnlineBookingSettings_Details
    {
        public int? Id { get; set; }
        public int? OnlineBookingSettingId { get; set; }
        public DateTime? FromDateTime { get; set; }
        public DateTime? TillDateTime { get; set; }
        public int? BookingTypeId { get; set; }
        public string Description { get; set; }
        public bool? DateWise { get; set; }
    }
    public class BookingRecallRequest
    {
        public int bookingStatusId { get; set; }
        public int driverId { get; set; }
        public int id { get; set; }
        public string UserName { get; set; }
    }
    public class SurchargeRate
    {
        public string ZoneName { get; set; }
        public int GenZoneID { get; set; }
        public long? Id { get; set; }
        public string PostCode { get; set; }
        public decimal? Percentage { get; set; }
        public int? SysPolicyId { get; set; }
        public bool? IsAmountWise { get; set; }
        public decimal? Amount { get; set; }
        public int? ZoneId { get; set; }
        public bool? ZoneWiseSurcharge { get; set; }
        public bool? EnableSurcharge { get; set; }
        public int? ApplicableFromDay { get; set; }
        public int? ApplicableToDay { get; set; }
        public DateTime? ApplicableFromDateTime { get; set; }
        public DateTime? ApplicableToDateTime { get; set; }
        public int? CriteriaBy { get; set; }
        public string Holidays { get; set; }
        public decimal? Parking { get; set; }
        public decimal? Waiting { get; set; }
        public bool? ApplyOutofTown { get; set; }
    }
    public class BookingInvoice
    {
        public long? Id { get; set; }
        public decimal? fare { get; set; }
        public decimal? parking { get; set; }
        public decimal? waiting { get; set; }
        public decimal? extraDrop { get; set; }
        public decimal? bookingFee { get; set; }
        public decimal? TotalCharges { get; set; }
        public int? invoicepaymentId { get; set; }
        public string orderNo { get; set; }
        public string Destination { get; set; }
        public string PickupPoint { get; set; }
        public string Passenger { get; set; }
        public int? waitingMins { get; set; }
        public decimal escortprice { get; set; }
        public int? vehicleTypeId { get; set; }

    }
    public class MySettings
    {
        public string RemoveExtraCharges { get; set; }
        public string ExtraChargesPerQty { get; set; }
        public string ShowExtraCharges { get; set; }
        public string ShowBookingFees { get; set; }
        public List<BookingFeesRange> BookingFeesRange { get; set; }
        public string ShowParkingCharges { get; set; }
        public string ChangePlotOnAsDirected { get; set; }
    }
    public class BookingFeesRange
    {
        public double From { get; set; }
        public double To { get; set; }
        public double Charges { get; set; }
    }
}