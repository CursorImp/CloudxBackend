using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRHub.Classes
{
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
        public bool? IsOutsideArea { get; set; }
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
        public bool? IsOutsideArea { get; set; }
    }


    public class ZoneMasterDetailList
    {
        public List<ZonesMaster> ZoneMaster { get; set; }
        public List<Gen_Zone_PolyVertices> ZoneDetail { get; set; }
    }

}