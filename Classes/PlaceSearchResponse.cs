using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace SignalRHub
{
    [XmlRoot(ElementName = "location")]
    public class Location
    {
        [XmlElement(ElementName = "lat")]
        public string Lat { get; set; }
        [XmlElement(ElementName = "lng")]
        public string Lng { get; set; }
    }

    [XmlRoot(ElementName = "southwest")]
    public class Southwest
    {
        [XmlElement(ElementName = "lat")]
        public string Lat { get; set; }
        [XmlElement(ElementName = "lng")]
        public string Lng { get; set; }
    }

    [XmlRoot(ElementName = "northeast")]
    public class Northeast
    {
        [XmlElement(ElementName = "lat")]
        public string Lat { get; set; }
        [XmlElement(ElementName = "lng")]
        public string Lng { get; set; }
    }

    [XmlRoot(ElementName = "viewport")]
    public class Viewport
    {
        [XmlElement(ElementName = "southwest")]
        public Southwest Southwest { get; set; }
        [XmlElement(ElementName = "northeast")]
        public Northeast Northeast { get; set; }
    }

    [XmlRoot(ElementName = "geometry")]
    public class Geometry
    {
        [XmlElement(ElementName = "location")]
        public Location Location { get; set; }
        [XmlElement(ElementName = "viewport")]
        public Viewport Viewport { get; set; }
    }

    [XmlRoot(ElementName = "opening_hours")]
    public class Opening_hours
    {
        [XmlElement(ElementName = "open_now")]
        public string Open_now { get; set; }
    }

    [XmlRoot(ElementName = "photo")]
    public class Photo
    {
        [XmlElement(ElementName = "photo_reference")]
        public string Photo_reference { get; set; }
        [XmlElement(ElementName = "width")]
        public string Width { get; set; }
        [XmlElement(ElementName = "height")]
        public string Height { get; set; }
        [XmlElement(ElementName = "html_attribution")]
        public string Html_attribution { get; set; }
    }

    [XmlRoot(ElementName = "result")]
    public class Result
    {
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }
        [XmlElement(ElementName = "type")]
        public List<string> Type { get; set; }
        [XmlElement(ElementName = "vicinity")]
        public string Vicinity { get; set; }
        [XmlElement(ElementName = "formatted_address")]
        public string Formatted_address { get; set; }
        [XmlElement(ElementName = "geometry")]
        public Geometry Geometry { get; set; }
        [XmlElement(ElementName = "rating")]
        public string Rating { get; set; }
        [XmlElement(ElementName = "icon")]
        public string Icon { get; set; }
        [XmlElement(ElementName = "reference")]
        public string Reference { get; set; }
        [XmlElement(ElementName = "id")]
        public string Id { get; set; }
        [XmlElement(ElementName = "opening_hours")]
        public Opening_hours Opening_hours { get; set; }
        [XmlElement(ElementName = "photo")]
        public Photo Photo { get; set; }
        [XmlElement(ElementName = "place_id")]
        public string Place_id { get; set; }
        [XmlElement(ElementName = "scope")]
        public string Scope { get; set; }
        
        public double? Distance { get; set; }
    }

    [XmlRoot(ElementName = "PlaceSearchResponse")]
    public class PlaceSearchResponse
    {
        [XmlElement(ElementName = "status")]
        public string Status { get; set; }
        [XmlElement(ElementName = "result")]
        public List<Result> Result { get; set; }
    }
   
}
