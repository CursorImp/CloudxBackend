using System;
using System.Net;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Data.Linq;
using System.Collections.Generic;
using DotNetCoords;
using System.IO;
using System.Xml.Serialization;
using Taxi_Model;
using Utils;

/// <summary>
/// Summary description for GetDistance
/// </summary>
public static class GetDistance
{
    public class LocationDetails
    {
        public string Pickup { get; set; }
        public string PickupPostCode { get; set; }
        public Coords? PickupPointLatLng { get; set; }
        public string DestinationPostCode { get; set; }
        public string Destination { get; set; }
        public Coords? DestinationPointLatLng { get; set; }

        public string DisplayEstimateTime { get; set; }
        public TimeSpan EstimateTime { get; set; }
        public Int32 EstimateTimeInMinutes { get; set; }
        public string DisplayDistance { get; set; }
        public decimal Distance { get; set; }

        public List<string> ViaPointsPostCode { get; set; }
        public List<string> ViaPoints { get; set; }
        public List<Coords> ViaPointsLatLng { get; set; }


    }


    // Handy structure for Long/Lat information
    public struct Coords
    {
        public double Longitude;
        public double Latitude;
    }

    // Unit calculations
    public enum Units
    {
        Miles,
        Kilometres
    }

    // Will return a null if the Google API is unable to find either post code, or the country constraint fails
    public static double? BetweenTwoPostCodes(string postcodeA, string postcodeB, string countryCodeWithin, Units units)
    {
        var ll1 = PostCodeToLongLat(postcodeA, countryCodeWithin);
        if (!ll1.HasValue) return null;
        var ll2 = PostCodeToLongLat(postcodeB, countryCodeWithin);
        if (!ll2.HasValue) return null;
        return ll1.Value.DistanceTo(ll2.Value, units);
    }

    // Overload for UK post codes
    public static double? BetweenTwoUKPostCodes(string postcodeA, string postcodeB)
    {
        return BetweenTwoPostCodes(postcodeA, postcodeB, "GB", Units.Miles);
    }

    //Uses the Google API to resolve a post code(within the specified country)
    public static Coords? PostCodeToLongLat(string postcode, string countryCodeWithin)
    {
        if (postcode == null || postcode == string.Empty)
            return null;
        // Download the XML response from Google
        //if (postcode.StartsWith("FLAT") || (postcode.Contains(" ") && postcode.Split(' ')[0][0].ToStr().IsNumeric()))
        //{

        //}
        //else
        //    return null;

        Coords? coord = null;


        try
        {

          //  FLAT 8 OLEASTOR COURT  STONELEIGH ROAD ILFORD IG5 0JX
          if(postcode.ToStr().StartsWith("FLAT "))
            {
                postcode = postcode.Replace("FLAT ", "").Trim();


                if(postcode.ToStr().Trim().Length>0 &&  (postcode.Split(' ')[0].IsNumeric() || (postcode.Split(' ')[0].IsAlpha() && postcode.Split(' ')[0].Length==1) ) )
                    {

                    postcode = postcode.Substring(postcode.IndexOf(' ')).ToStr().Trim();
                }
            }

            using (TaxiDataContext db = new TaxiDataContext())
            {
                try
                {

                    var loc = db.Gen_Locations.FirstOrDefault(c => c.FullLocationName == postcode && c.Latitude != null && c.Latitude != 0);
                    if(loc!=null && loc.Latitude!=null && loc.Latitude>0)
                    {

                        coord = new Coords
                        {
                            Longitude =Convert.ToDouble(loc.Longitude),
                            Latitude = Convert.ToDouble(loc.Latitude)
                        };

                    }
                   
                }
                catch
                {


                }

            }



            if (coord != null)
                return coord;
            // https://geocoder.ls.hereapi.com/6.2/geocode.json?apiKey=avsHjHri-tP5Su5wV7xyPBWwmdqOtEKK2Atn0xgDnrM&searchtext=12%20%20HIGHCLERE%20ASCOT%20SL5%200AA

            using (WebClient client = new WebClient())
            {
                client.Proxy = null;
                //Taxi_AppMain.General.GetKey();
               string googleKey = SignalRHub.General.GetKey();
                var encodedPostCode = HttpUtility.UrlEncode(postcode);
                //   var url = string.Format("http://maps.google.com/maps/geo?q={0}&output=xml", encodedPostCode);
                var url = "https://maps.googleapis.com/maps/api/geocode/xml?address=" + encodedPostCode + googleKey + "&sensor=true&region=GB";
                var xml = client.DownloadString(url);
                XDocument doc = XDocument.Parse(xml);

                //   [0].FirstChild.InnerText

                var nodes = doc.Descendants(XName.Get("location")).Nodes();

                //var nodes = doc.ChildNodes[1].ChildNodes[1].ChildNodes[7].ChildNodes[3].ChildNodes;

                double latitude = Convert.ToDouble(nodes.FirstOrDefault().ToString().Replace("<lat>", "").Trim().Replace("</lat>", "").Trim().ToString().Trim());
                double longitude = Convert.ToDouble(nodes.LastOrDefault().ToString().Replace("<lng>", "").Trim().Replace("</lng>", "").Trim().ToString().Trim());

                //  var latNode = (xelement.Elements<XElement>(XName.Get("lat"))).Nodes();
                //double latitude =Convert.ToDouble( from a in nodes.Nodes()
                //                  where  (a as XElement).Name == "lat"
                //                  select (a as XElement).Value);


                //double longitude = Convert.ToDouble(from a in doc.Element(XName.Get("location")).Nodes()
                //                                    where (a as XElement).Name == "lng"
                //                                    select (a as XElement).Value);





                ////latitude = Convert.ToDouble(nodes[0].FirstChild.InnerText);
                ////longitude = Convert.ToDouble(nodes[0].LastChild.InnerText);

                coord = new Coords
                {
                    Longitude = longitude,
                    Latitude = latitude
                };



            }
        }
        catch
        {


        }


        return coord;




    }
    public static Coords? PostCodeToLongLat2(string postcode, string countryCodeWithin)
    {
        if (postcode == null || postcode == string.Empty)
            return null;
        // Download the XML response from Google


        if (postcode.StartsWith("FLAT") || (postcode.Contains(" ") && postcode.Split(' ')[0].IsNumeric() ))
    {

        }
        else
            return null;
        Coords? coord = null;


        try
        {
            // https://geocoder.ls.hereapi.com/6.2/geocode.json?apiKey=avsHjHri-tP5Su5wV7xyPBWwmdqOtEKK2Atn0xgDnrM&searchtext=12%20%20HIGHCLERE%20ASCOT%20SL5%200AA
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            using (WebClient client = new WebClient())
            {
                client.Proxy = null;

                var encodedPostCode = HttpUtility.UrlEncode(postcode);

                var url = "https://geocoder.ls.hereapi.com/6.2/geocode.json?apiKey=avsHjHri-tP5Su5wV7xyPBWwmdqOtEKK2Atn0xgDnrM&searchtext=" + encodedPostCode;

                var xml = client.DownloadString(url);
               SignalRHub.HereMapApi.Root here = Newtonsoft.Json.JsonConvert.DeserializeObject< SignalRHub.HereMapApi.Root>(xml);

                //   [0].FirstChild.InnerText

              
                //var nodes = doc.ChildNodes[1].ChildNodes[1].ChildNodes[7].ChildNodes[3].ChildNodes;

                double latitude = here.Response.View[0].Result[0].Location.DisplayPosition.Latitude;
                double longitude = here.Response.View[0].Result[0].Location.DisplayPosition.Longitude;


                coord = new Coords
                {
                    Longitude = longitude,
                    Latitude = latitude
                };



            }
        }
        catch(Exception ex)
        {


        }


        return coord;




    }


    public static List<Coords?> GetAddressesToLongLatArray(string postcode, string countryCodeWithin)
    {
        // Download the XML response from Google

        Coords? coord = null;


        try
        {

            using (WebClient client = new WebClient())
            {
                client.Proxy = null;

                var encodedPostCode = HttpUtility.UrlEncode(postcode);
                //   var url = string.Format("http://maps.google.com/maps/geo?q={0}&output=xml", encodedPostCode);
                var url = "https://maps.googleapis.com/maps/api/geocode/xml?address=" + encodedPostCode + "&key=AIzaSyBPnwLqOrUNJOClIihSkeFOANC7wSHP00Q&sensor=true";
                var xml = client.DownloadString(url);
                XDocument doc = XDocument.Parse(xml);

                //   [0].FirstChild.InnerText

                var nodes = doc.Descendants(XName.Get("location")).Nodes();

                //var nodes = doc.ChildNodes[1].ChildNodes[1].ChildNodes[7].ChildNodes[3].ChildNodes;

                double latitude = Convert.ToDouble(nodes.FirstOrDefault().ToString().Replace("<lat>", "").Trim().Replace("</lat>", "").Trim().ToString().Trim());
                double longitude = Convert.ToDouble(nodes.LastOrDefault().ToString().Replace("<lng>", "").Trim().Replace("</lng>", "").Trim().ToString().Trim());

                //  var latNode = (xelement.Elements<XElement>(XName.Get("lat"))).Nodes();
                //double latitude =Convert.ToDouble( from a in nodes.Nodes()
                //                  where  (a as XElement).Name == "lat"
                //                  select (a as XElement).Value);


                //double longitude = Convert.ToDouble(from a in doc.Element(XName.Get("location")).Nodes()
                //                                    where (a as XElement).Name == "lng"
                //                                    select (a as XElement).Value);





                ////latitude = Convert.ToDouble(nodes[0].FirstChild.InnerText);
                ////longitude = Convert.ToDouble(nodes[0].LastChild.InnerText);

                coord = new Coords
                {
                    Longitude = longitude,
                    Latitude = latitude
                };



            }
        }
        catch
        {


        }


        return null;




    }


    public static double DistanceTo(this Coords from, Coords to, Units units)
    {
        // Haversine Formula...
        var dLat1InRad = from.Latitude * (Math.PI / 180.0);
        var dLong1InRad = from.Longitude * (Math.PI / 180.0);
        var dLat2InRad = to.Latitude * (Math.PI / 180.0);
        var dLong2InRad = to.Longitude * (Math.PI / 180.0);

        var dLongitude = dLong2InRad - dLong1InRad;
        var dLatitude = dLat2InRad - dLat1InRad;

        // Intermediate result a.
        var a = Math.Pow(Math.Sin(dLatitude / 2.0), 2.0) +
                Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) *
                Math.Pow(Math.Sin(dLongitude / 2.0), 2.0);

        // Intermediate result c (great circle distance in Radians).
        var c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));

        // Unit of measurement
        var radius = 6371;
        if (units == Units.Miles) radius = 3959;

        return radius * c;
    }




    public static string PlaceKey;

    public static SignalRHub.PlaceSearchResponse SearchPlaces(string keyword, Coords? coords, double radiusInMiles)
    {
        if (keyword == null || keyword == string.Empty)
            return null;

        var RadiusInMeter = radiusInMiles * 1609.344;

        string location = string.Empty;
        if (coords != null)
        {
            location = "&location=" + Convert.ToString(coords.Value.Latitude) + "," + Convert.ToString(coords.Value.Longitude);
        }
        else
        {

        }



        if (string.IsNullOrEmpty(PlaceKey))
        {
            using (TaxiDataContext db = new TaxiDataContext())
            {

                PlaceKey = db.ExecuteQuery<string>("select APIKey from mapkeys where maptype='places'").FirstOrDefault().ToStr().Trim();


                if (PlaceKey.Length == 0)
                    PlaceKey = "&key=AIzaSyDQRv7o4pxOeXApK6oKGp7U2FEHIttW5KA";
                else
                    PlaceKey = "&key=" + PlaceKey;
            }
        }



        var url = "https://maps.googleapis.com/maps/api/place/textsearch/xml?query=" + HttpUtility.UrlEncode(keyword) + location + "&radius=" + Convert.ToString(RadiusInMeter) + PlaceKey + "&sensor=true&region=GB";
        SignalRHub.PlaceSearchResponse response = new SignalRHub.PlaceSearchResponse();

        using (WebClient client = new WebClient())
        {
            client.Proxy = null;
            var xml = client.DownloadString(url);

            response = DeserializeXMLToObject<SignalRHub.PlaceSearchResponse>(xml);

            if (response != null && response.Status == "OK" && coords != null)
            {
                if (response.Result.Count > 3)
                {

                    response.Result = response.Result
                        //.Where(c=>c.Name.ToUpper().Contains(keyword))
                        .Select(m =>
                        new SignalRHub.Result()
                        {
                            Name = m.Name,
                            Type = m.Type,
                            Vicinity = m.Vicinity,
                            Formatted_address = m.Formatted_address,
                            Geometry = m.Geometry,
                            Rating = m.Rating,
                            Icon = m.Icon,
                            Reference = m.Reference,
                            Id = m.Id,
                            Opening_hours = m.Opening_hours,
                            Photo = m.Photo,
                            Place_id = m.Place_id,
                            Scope = m.Scope,
                            Distance = new LatLng(coords.Value.Latitude, coords.Value.Longitude).DistanceMiles(new LatLng(Convert.ToDouble(m.Geometry.Location.Lat), Convert.ToDouble(m.Geometry.Location.Lng))) //new LatLng(Convert.ToDouble(m.Geometry.Location.Lat), Convert.ToDouble(m.Geometry.Location.Lng)).DistanceMiles(new LatLng(coords.Value.Latitude, coords.Value.Longitude))
                        }).OrderBy(m => m.Distance).ThenByDescending(m => m.Rating).Where(c => c.Distance <= radiusInMiles).Distinct().ToList();
                }

            }
        }

        return response;
    }

    private static T DeserializeXMLToObject<T>(string Xml)
    {
        T returnObject = default(T);
        if (string.IsNullOrEmpty(Xml)) return default(T);

        try
        {

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (TextReader reader = new StringReader(Xml))
            {
                returnObject = (T)serializer.Deserialize(reader);
            }

        }
        catch (Exception ex)
        {
            throw ex;
        }
        return returnObject;
    }

    private static T DeserializeXMLFileToObject<T>(string XmlFilename)
    {
        T returnObject = default(T);
        if (string.IsNullOrEmpty(XmlFilename)) return default(T);

        try
        {
            StreamReader xmlStream = new StreamReader(XmlFilename);
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            returnObject = (T)serializer.Deserialize(xmlStream);
        }
        catch (Exception ex)
        {
            throw ex;
        }
        return returnObject;
    }
    public static string GetLocationDetailsByMapHere(Coords origin, Coords destination, string keys, List<Coords> via = null)
    {
        LocationDetails locDetails = new LocationDetails();
        try
        {
            if ((origin.Latitude == 0 && origin.Longitude == 0) || (destination.Latitude == 0 && destination.Longitude == 0))
            { return null; }


            string MapHereURL = @"https://route.api.here.com/routing/7.2/calculateroute.json?app_id={0}&app_code={1}&mode=shortest;car;&metricSystem=imperial{2}";

            int i = 0;
            string waypointTemp = "&waypoint{0}=geo!{1},{2}";
            string waypoint = string.Format(waypointTemp, i, origin.Latitude, origin.Longitude);

            if (via != null)
            {

                for (int j = 0; j < via.Count; j++)
                {
                    if (via[j].Latitude != 0 && via[j].Longitude != 0)
                    {
                        i++;
                        waypoint += string.Format(waypointTemp, i, via[j].Latitude, via[j].Longitude);
                    }
                }

            }

            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string APP_ID = keys.Split(new char[] { ',' })[0];
            //System.Configuration.ConfigurationManager.AppSettings["MAP_APP_ID"] != null ? System.Configuration.ConfigurationManager.AppSettings["MAP_APP_ID"].ToStr() : "3AFVxo9lo4YV4NVnqgz1";
            string APP_CODE = keys.Split(new char[] { ',' })[1];

            i++;
            waypoint += string.Format(waypointTemp, i, destination.Latitude, destination.Longitude);

            MapHereURL = string.Format(MapHereURL, APP_ID, APP_CODE, waypoint);



            WebRequest request = HttpWebRequest.Create(MapHereURL);

            System.Net.WebRequest.DefaultWebProxy = null;
            request.Proxy = null;
            WebResponse result = request.GetResponse();
            StreamReader reader = new StreamReader(result.GetResponseStream());
            string urlText = reader.ReadToEnd();


            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;

            SignalRHub.Root obj = serializer.Deserialize<SignalRHub.Root>(urlText);



            long dist = 0;

            var itemx = obj.Response.Route.OrderBy(c => c.Summary.Distance).FirstOrDefault();
            dist += itemx.Summary.TravelTime;

            locDetails.DisplayEstimateTime = (dist / 60).ToInt().ToStr() + " min(s)";

            //     locDetails.DisplayEstimateTime = Math.Round(Convert.ToDecimal((Convert.ToDouble(dist) / 1609.344)), 1);




            //using (XmlTextReader reader = new XmlTextReader(MapHereURL))
            //{

            //    reader.WhitespaceHandling = WhitespaceHandling.Significant;
            //    using (System.Data.DataSet ds = new System.Data.DataSet())
            //    {
            //        ds.ReadXml(reader);
            //        if (ds.Tables["Waypoint"] != null)
            //        {
            //            locDetails.Pickup = Convert.ToString(ds.Tables["Waypoint"].Rows[0]["Label"]);
            //            locDetails.Destination = Convert.ToString(ds.Tables["Waypoint"].Rows[ds.Tables["Waypoint"].Rows.Count - 1]["Label"]);
            //            if (ds.Tables["Waypoint"].Rows.Count > 2)
            //            {
            //                locDetails.ViaPoints = new List<string>();

            //                for (int j = 1; j < ds.Tables["Waypoint"].Rows.Count - 1; j++)
            //                {
            //                    locDetails.ViaPoints.Add(Convert.ToString(ds.Tables["Waypoint"].Rows[j]["Label"]));
            //                }
            //            }
            //        }

            //        if (ds.Tables["MappedPosition"] != null)
            //        {

            //            for (int j = 0; j < ds.Tables["MappedPosition"].Rows.Count; j++)
            //            {
            //                if (!string.IsNullOrEmpty(Convert.ToString(ds.Tables["MappedPosition"].Rows[j]["Waypoint_Id"])))
            //                {
            //                    if (j == 0)
            //                    {
            //                        locDetails.PickupPointLatLng = new Coords() { Latitude = Convert.ToDouble(ds.Tables["MappedPosition"].Rows[j]["Latitude"]), Longitude = Convert.ToDouble(ds.Tables["MappedPosition"].Rows[j]["Longitude"]) };
            //                    }
            //                    else if (ds.Tables["Waypoint"].Rows.Count > 2 && !string.IsNullOrEmpty(Convert.ToString(ds.Tables["MappedPosition"].Rows[j + 1]["Waypoint_Id"])))
            //                    {
            //                        if (locDetails.ViaPointsLatLng == null)
            //                        {
            //                            locDetails.ViaPointsLatLng = new List<Coords>();
            //                        }
            //                        var PointLatLng = new Coords() { Latitude = Convert.ToDouble(ds.Tables["MappedPosition"].Rows[j]["Latitude"]), Longitude = Convert.ToDouble(ds.Tables["MappedPosition"].Rows[j]["Longitude"]) };
            //                        locDetails.ViaPointsLatLng.Add(PointLatLng);
            //                    }
            //                }
            //                else
            //                {
            //                    locDetails.DestinationPointLatLng = new Coords() { Latitude = Convert.ToDouble(ds.Tables["MappedPosition"].Rows[j - 1]["Latitude"]), Longitude = Convert.ToDouble(ds.Tables["MappedPosition"].Rows[j - 1]["Longitude"]) };
            //                    break;
            //                }
            //            }
            //        }

            //        if (ds.Tables["Summary"] != null)
            //        {
            //            locDetails.Distance = Math.Round(Convert.ToDecimal(ds.Tables["Summary"].Rows[0]["Distance"]) * 0.00062137119m, 1);
            //            locDetails.DisplayDistance = locDetails.Distance + " Miles";
            //            locDetails.EstimateTime = TimeSpan.FromSeconds(Convert.ToInt32(ds.Tables["Summary"].Rows[0]["TravelTime"]));
            //            locDetails.EstimateTimeInMinutes = Convert.ToInt32(ds.Tables["Summary"].Rows[0]["TravelTime"]) / 60;

            //            if (locDetails.EstimateTime.Hours > 1)
            //            {
            //                locDetails.DisplayEstimateTime = string.Format("{0} Hours {1} Mins", locDetails.EstimateTime.Hours, locDetails.EstimateTime.Minutes);
            //            }
            //            else if (locDetails.EstimateTime.Hours == 1)
            //            {
            //                locDetails.DisplayEstimateTime = string.Format("{0} Hour {1} Mins", locDetails.EstimateTime.Hours, locDetails.EstimateTime.Minutes);
            //            }
            //            else
            //            {
            //                locDetails.DisplayEstimateTime = string.Format("{0} Mins", locDetails.EstimateTime.Minutes);
            //            }
            //        }
            //    }
            //}

            return locDetails.DisplayEstimateTime;

        }
        catch (Exception ex)
        {

        }
        return null;
    }




    //public static string GetLocationDetailsByMapHere(Coords origin, Coords destination,string keys, List<Coords> via = null)
    //{
    //    LocationDetails locDetails = new LocationDetails();
    //    try
    //    {
    //        if ((origin.Latitude == 0 && origin.Longitude == 0) || (destination.Latitude == 0 && destination.Longitude == 0))
    //        { return null; }

    //        string MapHereURL = @"https://route.api.here.com/routing/7.2/calculateroute.xml?app_id={0}&app_code={1}&mode=shortest;car;&metricSystem=imperial{2}";

    //        int i = 0;
    //        string waypointTemp = "&waypoint{0}=geo!{1},{2}";
    //        string waypoint = string.Format(waypointTemp, i, origin.Latitude, origin.Longitude);

    //        if (via != null)
    //        {

    //            for (int j = 0; j < via.Count; j++)
    //            {
    //                if (via[j].Latitude != 0 && via[j].Longitude != 0)
    //                {
    //                    i++;
    //                    waypoint += string.Format(waypointTemp, i, via[j].Latitude, via[j].Longitude);
    //                }
    //            }

    //        }

    //        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
    //        string APP_ID = keys.Split(new char[] { ',' })[0];
    //        //System.Configuration.ConfigurationManager.AppSettings["MAP_APP_ID"] != null ? System.Configuration.ConfigurationManager.AppSettings["MAP_APP_ID"].ToStr() : "3AFVxo9lo4YV4NVnqgz1";
    //        string APP_CODE = keys.Split(new char[] { ',' })[1];

    //        i++;
    //        waypoint += string.Format(waypointTemp, i, destination.Latitude, destination.Longitude);

    //        MapHereURL = string.Format(MapHereURL, APP_ID, APP_CODE, waypoint);

    //        using (XmlTextReader reader = new XmlTextReader(MapHereURL))
    //        {
    //            reader.WhitespaceHandling = WhitespaceHandling.Significant;
    //            using (System.Data.DataSet ds = new System.Data.DataSet())
    //            {
    //                ds.ReadXml(reader);
    //                if (ds.Tables["Waypoint"] != null)
    //                {
    //                    locDetails.Pickup = Convert.ToString(ds.Tables["Waypoint"].Rows[0]["Label"]);
    //                    locDetails.Destination = Convert.ToString(ds.Tables["Waypoint"].Rows[ds.Tables["Waypoint"].Rows.Count - 1]["Label"]);
    //                    if (ds.Tables["Waypoint"].Rows.Count > 2)
    //                    {
    //                        locDetails.ViaPoints = new List<string>();

    //                        for (int j = 1; j < ds.Tables["Waypoint"].Rows.Count - 1; j++)
    //                        {
    //                            locDetails.ViaPoints.Add(Convert.ToString(ds.Tables["Waypoint"].Rows[j]["Label"]));
    //                        }
    //                    }
    //                }

    //                if (ds.Tables["MappedPosition"] != null)
    //                {

    //                    for (int j = 0; j < ds.Tables["MappedPosition"].Rows.Count; j++)
    //                    {
    //                        if (!string.IsNullOrEmpty(Convert.ToString(ds.Tables["MappedPosition"].Rows[j]["Waypoint_Id"])))
    //                        {
    //                            if (j == 0)
    //                            {
    //                                locDetails.PickupPointLatLng = new Coords() { Latitude = Convert.ToDouble(ds.Tables["MappedPosition"].Rows[j]["Latitude"]), Longitude = Convert.ToDouble(ds.Tables["MappedPosition"].Rows[j]["Longitude"]) };
    //                            }
    //                            else if (ds.Tables["Waypoint"].Rows.Count > 2 && !string.IsNullOrEmpty(Convert.ToString(ds.Tables["MappedPosition"].Rows[j + 1]["Waypoint_Id"])))
    //                            {
    //                                if (locDetails.ViaPointsLatLng == null)
    //                                {
    //                                    locDetails.ViaPointsLatLng = new List<Coords>();
    //                                }
    //                                var PointLatLng = new Coords() { Latitude = Convert.ToDouble(ds.Tables["MappedPosition"].Rows[j]["Latitude"]), Longitude = Convert.ToDouble(ds.Tables["MappedPosition"].Rows[j]["Longitude"]) };
    //                                locDetails.ViaPointsLatLng.Add(PointLatLng);
    //                            }
    //                        }
    //                        else
    //                        {
    //                            locDetails.DestinationPointLatLng = new Coords() { Latitude = Convert.ToDouble(ds.Tables["MappedPosition"].Rows[j - 1]["Latitude"]), Longitude = Convert.ToDouble(ds.Tables["MappedPosition"].Rows[j - 1]["Longitude"]) };
    //                            break;
    //                        }
    //                    }
    //                }

    //                if (ds.Tables["Summary"] != null)
    //                {
    //                    locDetails.Distance = Math.Round(Convert.ToDecimal(ds.Tables["Summary"].Rows[0]["Distance"]) * 0.00062137119m, 1);
    //                    locDetails.DisplayDistance = locDetails.Distance + " Miles";
    //                    locDetails.EstimateTime = TimeSpan.FromSeconds(Convert.ToInt32(ds.Tables["Summary"].Rows[0]["TravelTime"]));
    //                    locDetails.EstimateTimeInMinutes = Convert.ToInt32(ds.Tables["Summary"].Rows[0]["TravelTime"]) / 60;

    //                    if (locDetails.EstimateTime.Hours > 1)
    //                    {
    //                        locDetails.DisplayEstimateTime = string.Format("{0} Hours {1} Mins", locDetails.EstimateTime.Hours, locDetails.EstimateTime.Minutes);
    //                    }
    //                    else if (locDetails.EstimateTime.Hours == 1)
    //                    {
    //                        locDetails.DisplayEstimateTime = string.Format("{0} Hour {1} Mins", locDetails.EstimateTime.Hours, locDetails.EstimateTime.Minutes);
    //                    }
    //                    else
    //                    {
    //                        locDetails.DisplayEstimateTime = string.Format("{0} Mins", locDetails.EstimateTime.Minutes);
    //                    }
    //                }
    //            }
    //        }

    //        return locDetails.DisplayEstimateTime;

    //    }
    //    catch (Exception ex)
    //    {

    //    }
    //    return null;
    //}

}


