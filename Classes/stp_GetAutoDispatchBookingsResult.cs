using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Web;

namespace SignalRHub
{
    public class stp_GetAutoDispatchBookingsResultEx
    {
        public stp_GetAutoDispatchBookingsResultEx()
        { }

        [Column(Storage = "_NoofPassengers", DbType = "Int")]
        public int? NoofPassengers { get; set; }
        [Column(Storage = "_EnableZoneAutoDispatch", DbType = "Bit")]
        public bool? EnableZoneAutoDispatch { get; set; }
        [Column(Storage = "_EnableZoneBidding", DbType = "Bit")]
        public bool? EnableZoneBidding { get; set; }
        [Column(Storage = "_BiddingRadius", DbType = "Int")]
        public int? BiddingRadius { get; set; }
        [Column(Storage = "_FareRate", DbType = "Decimal(18,2)")]
        public decimal? FareRate { get; set; }
        [Column(Storage = "_Longitude", DbType = "Float")]
        public double? Longitude { get; set; }
        [Column(Storage = "_Latitude", DbType = "Float")]
        public double? Latitude { get; set; }
        [Column(Storage = "_VehicleAttributes", DbType = "VarChar(MAX)")]
        public string VehicleAttributes { get; set; }
        [Column(Storage = "_OnHoldDateTime", DbType = "DateTime")]
        public DateTime? OnHoldDateTime { get; set; }
        [Column(Storage = "_IsConfirmedDriver", DbType = "Bit")]
        public bool? IsConfirmedDriver { get; set; }
        [Column(Storage = "_FromAddress", DbType = "VarChar(200)")]
        public string FromAddress { get; set; }
        [Column(Storage = "_FromPostCode", DbType = "VarChar(50)")]
        public string FromPostCode { get; set; }
        [Column(Storage = "_FromLocTypeId", DbType = "Int")]
        public int? FromLocTypeId { get; set; }
        [Column(Storage = "_ZoneName", DbType = "VarChar(50)")]
        public string ZoneName { get; set; }
        [Column(Storage = "_CompanyId", DbType = "Int")]
        public int? CompanyId { get; set; }
        [Column(Storage = "_JobAttributes", DbType = "VarChar(50)")]
        public string JobAttributes { get; set; }
        [Column(Storage = "_VehicleTypeId", DbType = "Int")]
        public int? VehicleTypeId { get; set; }
        [Column(Storage = "_DropOffZoneId", DbType = "Int")]
        public int? DropOffZoneId { get; set; }
        [Column(Storage = "_ZoneId", DbType = "Int")]
        public int? ZoneId { get; set; }
        [Column(Storage = "_DriverId", DbType = "Int")]
        public int? DriverId { get; set; }
        [Column(Storage = "_IsBidding", DbType = "Bit")]
        public bool? IsBidding { get; set; }
        [Column(Storage = "_AutoDespatch", DbType = "Bit")]
        public bool? AutoDespatch { get; set; }
        [Column(Storage = "_Lead", DbType = "DateTime")]
        public DateTime? Lead { get; set; }
        [Column(Storage = "_PickupDateTime", DbType = "DateTime")]
        public DateTime? PickupDateTime { get; set; }
        [Column(Storage = "_BookingDate", DbType = "DateTime")]
        public DateTime? BookingDate { get; set; }
        [Column(Storage = "_DespatchDateTime", DbType = "DateTime")]
        public DateTime? DespatchDateTime { get; set; }
        [Column(Storage = "_bookingstatusId", DbType = "Int")]
        public int? bookingstatusId { get; set; }
        [Column(Storage = "_JobId", DbType = "BigInt NOT NULL")]
        public long JobId { get; set; }
        [Column(Storage = "_ExcludedDriverIds", DbType = "VarChar(MAX)")]
        public string ExcludedDriverIds { get; set; }
        [Column(Storage = "_DeadMileage", DbType = "Decimal(18,2)")]
        public decimal? DeadMileage { get; set; }



        [Column(Storage = "_Priority", DbType = "Decimal(18,2)")]
        public decimal? Priority { get; set; }



        [Column(Storage = "_BookingTypeId", DbType = "int")]
        public int? BookingTypeId { get; set; }
        [Column(Storage = "_SubcompanyId", DbType = "int")]
        public int? SubcompanyId { get; set; }
    }
}