using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CabTreasureWebApi.Models.HereForwardGeocode
{
    public class Access
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class Address
    {
        public string label { get; set; }
        public string countryCode { get; set; }
        public string countryName { get; set; }
        public string state { get; set; }
        public string countyCode { get; set; }
        public string county { get; set; }
        public string city { get; set; }
        public string district { get; set; }
        public string street { get; set; }
        public string postalCode { get; set; }
    }

    public class FieldScore
    {
        public double country { get; set; }
        public double postalCode { get; set; }
    }

    public class Item
    {
        public string title { get; set; }
        public string id { get; set; }
        public string resultType { get; set; }
        public Address address { get; set; }
        public Position position { get; set; }
        public List<Access> access { get; set; }
        public MapView mapView { get; set; }
        public Scoring scoring { get; set; }
    }

    public class MapView
    {
        public double west { get; set; }
        public double south { get; set; }
        public double east { get; set; }
        public double north { get; set; }
    }

    public class Position
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class HereForwardGeocoding
    {
        public List<Item> items { get; set; }
    }

    public class Scoring
    {
        public double queryScore { get; set; }
        public FieldScore fieldScore { get; set; }
    }
}