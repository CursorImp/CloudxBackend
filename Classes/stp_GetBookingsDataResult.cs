using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Linq.Mapping;

namespace SignalRHub
{
     public class stp_GetBookingsDataResult
    {
       

        [Column(Storage = "_UpdateBy", DbType = "VarChar(50)")]
        public string UpdateBy { get; set; }
        [Column(Storage = "_SpecialReq", DbType = "VarChar(MAX)")]
        public string SpecialReq { get; set; }
        [Column(Storage = "_StatusId", DbType = "Int")]
        public int? StatusId { get; set; }
        [Column(Storage = "_Status", DbType = "VarChar(50) NOT NULL", CanBeNull = false)]
        public string Status { get; set; }
        [Column(Storage = "_StatusColor", DbType = "VarChar(30)")]
        public string StatusColor { get; set; }
        [Column(Storage = "_Driver", DbType = "VarChar(83)")]
        public string Driver { get; set; }
        [Column(Storage = "_DriverId", DbType = "Int")]
        public int? DriverId { get; set; }
        [Column(Storage = "_IsAutoDespatch", DbType = "Bit")]
        public bool? IsAutoDespatch { get; set; }
        [Column(Storage = "_BookingTypeId", DbType = "Int")]
        public int? BookingTypeId { get; set; }
        [Column(Storage = "_HasNotes", DbType = "Int")]
        public int? HasNotes { get; set; }
        [Column(Storage = "_HasNotesImg", DbType = "VarChar(1) NOT NULL", CanBeNull = false)]
        public string HasNotesImg { get; set; }
        [Column(Storage = "_SubCompanyBgColor", DbType = "Int")]
        public int? SubCompanyBgColor { get; set; }
        [Column(Storage = "_BookingBackgroundColor", DbType = "Int")]
        public int? BookingBackgroundColor { get; set; }
        [Column(Storage = "_GroupId", DbType = "VarChar(10)")]
        public string GroupId { get; set; }
        [Column(Storage = "_FromLocId", DbType = "Int")]
        public int? FromLocId { get; set; }
        [Column(Storage = "_PrePickupDate", DbType = "Date")]
        public DateTime? PrePickupDate { get; set; }
        [Column(Storage = "_BabySeats", DbType = "VarChar(200)")]
        public string BabySeats { get; set; }
        [Column(Storage = "_IsConfirmedDriver", DbType = "Bit")]
        public bool? IsConfirmedDriver { get; set; }
        [Column(Storage = "_MilesFromBase", DbType = "Decimal(18,2)")]
        public decimal? MilesFromBase { get; set; }
        [Column(Storage = "_IsBidding", DbType = "Bit")]
        public bool? IsBidding { get; set; }
        [Column(Storage = "_DeadMileage", DbType = "Decimal(18,2)")]
        public decimal? DeadMileage { get; set; }
        [Column(Storage = "_DespatchDateTime", DbType = "DateTime")]
        public DateTime? DespatchDateTime { get; set; }
        [Column(Storage = "_JourneyTypeId", DbType = "Int")]
        public int? JourneyTypeId { get; set; }
        [Column(Storage = "_Due", DbType = "DateTime")]
        public DateTime? Due { get; set; }
        [Column(Storage = "_BackgroundColor", DbType = "VarChar(30)")]
        public string BackgroundColor { get; set; }
        [Column(Storage = "_Vehicle", DbType = "VarChar(100)")]
        public string Vehicle { get; set; }
        [Column(Storage = "_NoofLuggages", DbType = "Int")]
        public int? NoofLuggages { get; set; }
        [Column(Storage = "_PReference", DbType = "VarChar(MAX)")]
        public string PReference { get; set; }
        [Column(Storage = "_TextColor", DbType = "VarChar(30)")]
        public string TextColor { get; set; }
        [Column(Storage = "_Id", DbType = "BigInt NOT NULL")]
        public long Id { get; set; }
        [Column(Storage = "_Lead", DbType = "DateTime")]
        public DateTime? Lead { get; set; }
        [Column(Storage = "_Plot", DbType = "VarChar(20)")]
        public string Plot { get; set; }
        [Column(Storage = "_PlotHour", DbType = "DateTime")]
        public DateTime? PlotHour { get; set; }
        [Column(Storage = "_RefNumber", DbType = "VarChar(50)")]
        public string RefNumber { get; set; }
        [Column(Storage = "_BookingDateTime", DbType = "DateTime")]
        public DateTime? BookingDateTime { get; set; }
        [Column(Storage = "_PickupDateTemp", DbType = "DateTime")]
        public DateTime? PickupDateTemp { get; set; }
        [Column(Storage = "_PickUpDate", DbType = "VarChar(8000)")]
        public string PickUpDate { get; set; }
        [Column(Storage = "_Time", DbType = "VarChar(5)")]
        public string Time { get; set; }
        [Column(Storage = "_Passenger", DbType = "VarChar(100)")]
        public string Passenger { get; set; }
        [Column(Storage = "_MobileNo", DbType = "VarChar(50)")]
        public string MobileNo { get; set; }
        [Column(Storage = "_TelephoneNo", DbType = "VarChar(50)")]
        public string TelephoneNo { get; set; }
        [Column(Name = "[From]", Storage = "_From", DbType = "VarChar(502)")]
        public string From { get; set; }
        [Column(Storage = "_Pickup", DbType = "VarChar(200)")]
        public string Pickup { get; set; }
        [Column(Storage = "_FromPostCode", DbType = "VarChar(50)")]
        public string FromPostCode { get; set; }
        [Column(Name = "[To]", Storage = "_To", DbType = "VarChar(200)")]
        public string To { get; set; }
        [Column(Storage = "_GoingTo", DbType = "VarChar(200)")]
        public string GoingTo { get; set; }
        [Column(Storage = "_ToPostCode", DbType = "VarChar(50)")]
        public string ToPostCode { get; set; }
        [Column(Storage = "_Fare", DbType = "Decimal(18,2)")]
        public decimal? Fare { get; set; }
        [Column(Storage = "_Pax", DbType = "Int")]
        public int? Pax { get; set; }
        [Column(Storage = "_PaymentMethod", DbType = "VarChar(50)")]
        public string PaymentMethod { get; set; }
        [Column(Storage = "_FromLocTypeId", DbType = "Int")]
        public int? FromLocTypeId { get; set; }
        [Column(Storage = "_ToLocTypeId", DbType = "Int")]
        public int? ToLocTypeId { get; set; }
        [Column(Storage = "_BackgroundColor1", DbType = "VarChar(50)")]
        public string BackgroundColor1 { get; set; }
        [Column(Storage = "_TextColor1", DbType = "VarChar(20)")]
        public string TextColor1 { get; set; }
        [Column(Storage = "_Account", DbType = "VarChar(100)")]
        public string Account { get; set; }
        [Column(Storage = "_Vias", DbType = "Int NOT NULL")]
        public int Vias { get; set; }
        [Column(Storage = "_Attributes", DbType = "VarChar(200)")]
        public string Attributes { get; set; }
        [Column(Storage = "_VehicleDetails", DbType = "VarChar(500)")]
        public string VehicleDetails { get; set; }


        [Column(Storage = "_VehicleID", DbType = "VarChar(500)")]
        public string VehicleID { get; set; }
        public int? CompanyId { get; set; }
        public int? VehicleTypeId { get; set; }
        public long? EscortId { get; set; }
        public bool? HasEscort { get; set; }
        public bool? IsHideJobFromDrivers { get; set; }
        public string BookingTypeName { get; set; }
        public string OrderNo { get; set; }
    }
}