using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Utils;

namespace SignalRHub
{
    public class ClsHereMapRequestResponse
    {
        public bool HasError { get; set; }
        public string Message { get; set; }
      

        public string Coordinates;
        public decimal Distance;
    }

    public class ClsHereMap
    {
        public string StartingPoint { get; set; }
        public string EndingPoint { get; set; }
        public string ViaString { get; set; }
        public ClsHereMapRequestResponse GenerateRoute(bool requiredPolyline)
        {
            ClsHereMapRequestResponse response = new ClsHereMapRequestResponse();

            try
            {


                //Dictionary<string, string> waypoints = new Dictionary<string, string>();
                //waypoints.Add("waypoint" + 0, StartingPoint);

                //int Count = 1;
                //if (ViaString.ToStr().Trim().Length > 0)
                //{
                //    string[] via = ViaString.Split('|');
                //    foreach (string cordinates in via)
                //    {
                //        waypoints.Add("waypoint" + Count, cordinates);
                //        Count++;
                //    }

                //}

                //waypoints.Add("waypoint" + Count, EndingPoint);


                //string waypointinfo = "";
                //foreach (KeyValuePair<string, string> entry in waypoints)
                //{
                //    waypointinfo += "&" + entry.Key + "=geo!" + entry.Value;
                //}
                Dictionary<string, string> waypoints = new Dictionary<string, string>();
                waypoints.Add("waypoint" + 0, StartingPoint);

                int Count = 1;
                if (ViaString.ToStr().Trim().Length > 0)
                {
                    string[] via = ViaString.Split('|');
                    foreach (string cordinates in via)
                    {
                        waypoints.Add("waypoint" + Count, cordinates);
                        Count++;
                    }

                }

                waypoints.Add("waypoint" + Count, EndingPoint);


                string waypointinfo = "";
                foreach (KeyValuePair<string, string> entry in waypoints)
                {
                    waypointinfo += "&" + entry.Key + "=geo!" + entry.Value;
                }


                ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

                //   string URL2 = "https://route.cit.api.here.com/routing/7.2/calculateroute.json?"+ waypointinfo + "&mode=shortest%3Bcar%3Btraffic%3Adisabled&app_id=3AFVxo9lo4YV4NVnqgz1%20&%20app_code=uCIGBo3LGjk4d02fxXGtvw&representation=linkPaging&returnElevation=true&maneuverAttributes=position%2Clength%2CtravelTime%2CstartAngle&routeAttributes=waypoints%2Csummary";
                //string URL2 = "http://route.cit.api.here.com/routing/7.2/calculateroute.json?" + waypointinfo + "&mode=shortest;car;traffic:disabled&app_id={0}&app_code={1}&representation=linkPaging&returnElevation=true&maneuverAttributes=position,length,travelTime,startAngle&routeAttributes=waypoints,summary&alternatives=3";

                string URL2 = "http://route.cit.api.here.com/routing/7.2/calculateroute.json?" + waypointinfo + "&mode=shortest;car;traffic:disabled&app_id=3AFVxo9lo4YV4NVnqgz1&app_code=uCIGBo3LGjk4d02fxXGtvw&routeAttributes=waypoints,summary&alternatives=3";
                WebRequest request = HttpWebRequest.Create(URL2);

                System.Net.WebRequest.DefaultWebProxy = null;
                request.Proxy = null;
                WebResponse result = request.GetResponse();
                StreamReader reader = new StreamReader(result.GetResponseStream());
                string urlText = reader.ReadToEnd();


                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializer.MaxJsonLength = int.MaxValue;

                Root obj = serializer.Deserialize<Root>(urlText);


                if (requiredPolyline)
                {

                    string Latlan = string.Empty;
                    foreach (var item in obj.Response.Route[0].Leg[0].Link)
                    {

                        foreach (var item2 in item.Shape)
                        {
                            var arr = item2.Split(',');


                            Latlan += arr[0] + "," + arr[1] + "|";

                        }

                    }

                    response.Coordinates = Latlan.Remove(Latlan.Length - 1, 1);
                    //   General.objShortest3 = response.Coordinates;


                }

                long dist = 0;

                //foreach (var item in obj.Response.Route.OrderBy(c=>c.Summary.TrafficTime).FirstOrDefault())
                //{
                var itemx = obj.Response.Route.OrderBy(c => c.Summary.Distance).FirstOrDefault();
                dist += itemx.Summary.Distance;

                // }

                response.Distance = Math.Round(Convert.ToDecimal((Convert.ToDouble(dist) / 1609.344)), 1);


                response.HasError = false;
                response.Message = "Success";

                //try
                //{

                //    File.AppendAllText(Environment.CurrentDirectory + "\\generateroute.txt", DateTime.Now + " :" + urlText + Environment.NewLine);

                //}
                //catch
                //{


                //}

                return response;
            }
            catch (Exception ex)
            {

                try
                {

                    File.AppendAllText(Environment.CurrentDirectory + "\\generateroute_exception.txt", DateTime.Now + " :" + ex.Message + Environment.NewLine);

                }
                catch
                {


                }
                response.HasError = true;
                response.Message = ex.Message;
                return response;
            }

        }


        //public ClsHereMapRequestResponse GenerateRoute(bool requiredPolyline)
        //{
        //    ClsHereMapRequestResponse response = new ClsHereMapRequestResponse();

        //    try
        //    {


        //        Dictionary<string, string> waypoints = new Dictionary<string, string>();
        //        waypoints.Add("waypoint" + 0, StartingPoint);

        //        int Count = 1;
        //        if (ViaString.ToStr().Trim().Length > 0)
        //        {
        //            string[] via = ViaString.Split('|');
        //            foreach (string cordinates in via)
        //            {
        //                waypoints.Add("waypoint" + Count, cordinates);
        //                Count++;
        //            }

        //        }

        //        waypoints.Add("waypoint" + Count, EndingPoint);


        //        string waypointinfo = "";
        //        foreach (KeyValuePair<string, string> entry in waypoints)
        //        {
        //            waypointinfo += "&" + entry.Key + "=geo!" + entry.Value;
        //        }
        //        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        //        // ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //     //   string URL2 = "https://route.cit.api.here.com/routing/7.2/calculateroute.json?"+ waypointinfo + "&mode=shortest%3Bcar%3Btraffic%3Adisabled&app_id=3AFVxo9lo4YV4NVnqgz1%20&%20app_code=uCIGBo3LGjk4d02fxXGtvw&representation=linkPaging&returnElevation=true&maneuverAttributes=position%2Clength%2CtravelTime%2CstartAngle&routeAttributes=waypoints%2Csummary";

        //        string URL2 = "http://route.cit.api.here.com/routing/7.2/calculateroute.json?" + waypointinfo + "&mode=fastest;car;traffic:disabled&app_id=3AFVxo9lo4YV4NVnqgz1&app_code=uCIGBo3LGjk4d02fxXGtvw&representation=linkPaging&returnElevation=true&maneuverAttributes=position,length,travelTime,startAngle&routeAttributes=waypoints,summary&alternatives=3";
        //        WebRequest request = HttpWebRequest.Create(URL2);

        //        System.Net.WebRequest.DefaultWebProxy = null;
        //        request.Proxy = null;
        //        WebResponse result = request.GetResponse();
        //        StreamReader reader = new StreamReader(result.GetResponseStream());
        //        string urlText = reader.ReadToEnd();


        //        Root obj = new JavaScriptSerializer().Deserialize<Root>(urlText);


        //            if (requiredPolyline)
        //            {

        //                string Latlan = string.Empty;
        //                foreach (var item in obj.Response.Route[0].Leg[0].Link)
        //                {

        //                    foreach (var item2 in item.Shape)
        //                    {
        //                        var arr = item2.Split(',');


        //                            Latlan += arr[0] + "," + arr[1] + "|";

        //                    }                         

        //                }

        //                response.Coordinates = Latlan.Remove(Latlan.Length - 1, 1);
        //             //   General.objShortest3 = response.Coordinates;


        //            }

        //            long dist = 0;
        //        //foreach (var item in obj.Response.Route.OrderBy(c=>c.Summary.TrafficTime).FirstOrDefault())
        //        //{
        //        var itemx = obj.Response.Route.OrderBy(c => c.Summary.TravelTime).FirstOrDefault();
        //                dist += itemx.Summary.Distance;

        //           // }

        //            response.Distance = Math.Round(Convert.ToDecimal((Convert.ToDouble(dist) / 1609.344)), 1);


        //            response.HasError = false;
        //            response.Message = "Success";

        //        try
        //        {

        //            File.AppendAllText(Environment.CurrentDirectory + "\\generateroute.txt", DateTime.Now + " :" + urlText + Environment.NewLine);

        //        }
        //        catch
        //        {


        //        }

        //        return response;
        //    }
        //    catch (Exception ex)
        //    {

        //        try
        //        {

        //            File.AppendAllText(Environment.CurrentDirectory + "\\generateroute_exception.txt", DateTime.Now + " :" + ex.Message + Environment.NewLine);

        //        }
        //        catch
        //        {


        //        }
        //        response.HasError = true;
        //        response.Message = ex.Message;
        //        return response;
        //    }

        //}
    }




    public partial class Root
        {
            [JsonProperty("response")]
            public HereResponse Response { get; set; }
        }

        public partial class HereResponse
        {
        

            [JsonProperty("route")]
            public Route[] Route { get; set; }

         
        }

      

        public partial class Route
        {
        [JsonProperty("waypoint")]
        public Waypoint[] Waypoint { get; set; }

        [JsonProperty("mode")]
            public Mode Mode { get; set; }

            [JsonProperty("leg")]
            public Leg[] Leg { get; set; }


        [JsonProperty("summary")]
        public Summary Summary { get; set; }
    }


    public partial class Summary
    {
        [JsonProperty("distance")]
        public long Distance { get; set; }

        [JsonProperty("trafficTime")]
        public long TrafficTime { get; set; }

        [JsonProperty("baseTime")]
        public long BaseTime { get; set; }

      

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("travelTime")]
        public long TravelTime { get; set; }

        [JsonProperty("_type")]
        public string Type { get; set; }
    }
    public partial class Position
    {
        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }
    }

    public partial class Waypoint
    {
        [JsonProperty("linkId")]
        public string LinkId { get; set; }

        [JsonProperty("mappedPosition")]
        public Position MappedPosition { get; set; }

        [JsonProperty("originalPosition")]
        public Position OriginalPosition { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("spot")]
        public double Spot { get; set; }

        [JsonProperty("sideOfStreet")]
        public string SideOfStreet { get; set; }

        [JsonProperty("mappedRoadName")]
        public string MappedRoadName { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("shapeIndex")]
        public long ShapeIndex { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }
    }

    public partial class Leg
        {
            [JsonProperty("link")]
            public Link[] Link { get; set; }
        }

        public partial class Link
        {
            [JsonProperty("linkId")]
            public string LinkId { get; set; }

            [JsonProperty("shape")]
            public string[] Shape { get; set; }

         
        }

     

        public partial class Mode
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("transportModes")]
            public string[] TransportModes { get; set; }

            [JsonProperty("trafficMode")]
            public string TrafficMode { get; set; }

            [JsonProperty("feature")]
            public object[] Feature { get; set; }
        }

        public enum Flag { BuiltUpArea };

        public enum RoadNumber { B3022, B376, B470, Empty };

        public enum TypeEnum { PrivateTransportLinkType };

      

    
   

}
