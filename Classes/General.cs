using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using DAL;
using Taxi_Model;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;
using Utils;
using Taxi_BLL;
using System.Xml;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Script.Serialization;
using SignalRHub.WebApiClasses;
using DotNetCoords;
using SignalRHub.Classes;
using System.Threading;

namespace SignalRHub
{
    public class JobDetails
    {
        public string JobId;
        public string driverId;
        public string DriverNo;
        public string Name;
        public string MobileNo;
        public string Address;

    }

    public class General
    {
        public static string remoteIps = string.Empty;



        public static void RecyclePool()
        {
            try
            {
                using (System.Net.WebClient webClient = new System.Net.WebClient())
                {
                    var result = webClient.DownloadString(System.Configuration.ConfigurationManager.AppSettings["ApplicationUrl"].ToStr());
                    //
                }
            }
            catch
            {

            }

        }


        public static bool VerifyLicense(string defaultClientId)
        {
            bool verify = false;


            try
            {
                ClsLic lic = new ClsLic();
                try
                {
                    string Urls = "http://eurlic.co.uk/license/api/Cab/VerifyLicense";
                    var baseAddress = new Uri(Urls);
                    var json = string.Empty;
                    json = "{\"DefaultClientID\":" + "\"" + defaultClientId + "\"" + "}";


                    var httpWebRequest = (HttpWebRequest)WebRequest.Create(Urls);
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "POST";
                    httpWebRequest.Proxy = null;
                    httpWebRequest.Headers.Add("Authorization", "Basic " + "Y2FidHJlYXN1cmU6Y2FidHJlYXN1cmU5ODcwIUAj");
                    //   string usernamePassword = Base64Encode("cabtreasure:cabtreasure9870!@#");
                    //string usernamePassword = Base64Encode("cabtreasure:cabtreasurecloud9870!@#");
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
                        lic = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<ClsLic>(result);



                    }

                }
                catch (Exception ex)
                {
                    lic.IsValid = false;
                    lic.Reason = ex.Message;


                }
                if (lic.IsValid)
                {
                    verify = true;


                }

                //if (Program.objLic.IsValid)
                //{
                //    verify = true;

                //    if (Program.objLic.ExpiryDateTime.ToStr().Trim().Length > 0)
                //        AppVars.LicenseExpiryDate = "License will Expire on " + string.Format("{0:dd/MMM/yyyy HH:mm}", Program.objLic.ExpiryDateTime.ToDateTimeorNull());

                //    string serialized = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(Program.objLic);
                //    try
                //    {

                //        File.WriteAllText(Application.StartupPath + "\\SysCon.dat", General.EncryptSysCon(serialized));
                //    }
                //    catch
                //    {


                //    }
                //}
                //else
                //{

                //    if (Program.objLic.Reason.ToStr().Trim().Length > 0)
                //    {


                //        if (Program.objLic.ExpiryDateTime.ToStr().Trim().Length > 0)
                //            AppVars.LicenseExpiryDate = "License will Expire on " + string.Format("{0:dd/MMM/yyyy HH:mm}", Program.objLic.ExpiryDateTime.ToDateTimeorNull());



                //    }
                //}
            }
            catch (Exception ex)
            {
                //  Program.objLic.IsValid = false;
                //  Program.objLic.Reason = ex.Message;


            }


            return verify;

        }





        public static WebApiClasses.DisplayPosition GetLocationCoordByDisplayPosition(string locationName, string keys = "")
        {

            WebApiClasses.DisplayPosition rtn = null;


            WebApiClasses.Location loc = GetLocationCoord(locationName, keys = "");

            if (loc != null)
                rtn = loc.DisplayPosition;





            return rtn;
        }



        public static WebApiClasses.Location GetLocationCoord(string locationName, string keys = "")
        {
            WebApiClasses.Location loc = null;
            string rtn = string.Empty;


            try
            {
                if (locationName.ToStr().Trim().Length > 2)
                {
                    string prefix = locationName.Substring(0, 2);

                    if (System.IO.File.Exists(AppContext.BaseDirectory + "\\Addresses\\" + prefix + ".txt"))
                    {
                        var data = System.IO.File.ReadAllLines(AppContext.BaseDirectory + "\\Addresses\\" + prefix + ".txt").Where(c => c.Contains(locationName)).FirstOrDefault();


                        if (data != null)
                        {
                            loc = new WebApiClasses.Location();
                            loc.DisplayPosition = new WebApiClasses.DisplayPosition();
                            var arr = data.Split('|');
                            loc.DisplayPosition.Latitude = Convert.ToDouble(arr[1]);
                            loc.DisplayPosition.Longitude = Convert.ToDouble(arr[2]);


                            try
                            {


                                System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetLocationCoordbyHerePickCache.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",json:" + locationName + Environment.NewLine);
                            }
                            catch
                            {

                            }

                        }
                        // //
                    }


                }
            }
            catch
            {

            }

            if (loc == null)
            {
                string key = keys;
                try
                {
                    //

                    if (HubProcessor.Instance.objPolicy.MapType.ToInt() == 4) //MAPBOX
                    {

                        var oo = GetMapBoxAddressData(locationName, true).FirstOrDefault();

                        if (oo != null && oo.Latitude != null)
                        {
                            loc = new WebApiClasses.Location();
                            loc.DisplayPosition = new DisplayPosition();
                            loc.DisplayPosition.Latitude = Convert.ToDouble(oo.Latitude);
                            loc.DisplayPosition.Longitude = Convert.ToDouble(oo.Longitude);
                        }

                    }
                    else
                    {

                        if (key.ToStr().Length == 0)
                            key = GetHEREKey();

                        if (key.Length > 0 && key.Contains(",") && key.Split(',').Count() > 2)
                            key = key.Split(',')[2].ToStr();



                        if (key.ToStr().Length == 0)
                            key = "avsHjHri-tP5Su5wV7xyPBWwmdqOtEKK2Atn0xgDnrM";

                        string url2 = "https://geocode.search.hereapi.com/v1/geocode?q=" + locationName + "&apiKey=" + key;



                        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

                        WebRequest request2 = HttpWebRequest.Create(url2);
                        request2.Proxy = null;

                        WebResponse response2 = request2.GetResponse();
                        using (StreamReader reader2 = new StreamReader(response2.GetResponseStream()))
                        {
                            //
                            System.Web.Script.Serialization.JavaScriptSerializer parser2 = new System.Web.Script.Serialization.JavaScriptSerializer();

                            string res = reader2.ReadToEnd();
                            CabTreasureWebApi.Models.HereForwardGeocode.HereForwardGeocoding objdata2 = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<CabTreasureWebApi.Models.HereForwardGeocode.HereForwardGeocoding>(res);

                            if (objdata2 != null && objdata2.items.Count > 0)
                            {
                                loc = new WebApiClasses.Location();
                                loc.DisplayPosition = new DisplayPosition();

                                if (objdata2.items[0].access != null && objdata2.items[0].access.Count > 0)
                                {
                                    loc.DisplayPosition.Latitude = objdata2.items[0].access[0].lat;
                                    loc.DisplayPosition.Longitude = objdata2.items[0].access[0].lng;


                                }
                                else
                                {
                                    loc.DisplayPosition.Latitude = objdata2.items[0].position.lat;
                                    loc.DisplayPosition.Longitude = objdata2.items[0].position.lng;
                                    //rtn = objdata2.items[0].position.lat + "," + objdata2.items[0].position.lng;
                                }










                            }



                            // System.IO.File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "\\GetForwardGeocodeByHere.txt", DateTime.Now.ToStr() + "url:" + url2 + " , location :" + rtn + Environment.NewLine);


                        }







                    }




                    try
                    {
                        if (locationName.ToStr().Trim().Length > 2 && loc != null && loc.DisplayPosition != null && loc.DisplayPosition.Latitude != 0)
                        {
                            string prefix = locationName.Substring(0, 2);
                            int cnt = 0;
                            if (System.IO.File.Exists(AppContext.BaseDirectory + "\\Addresses\\" + prefix + ".txt"))
                            {
                                cnt = System.IO.File.ReadAllLines(AppContext.BaseDirectory + "\\Addresses\\" + prefix + ".txt")
                                   .Count(c => c.Contains(locationName));

                            }
                            //
                            if (cnt == 0)
                                System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\Addresses\\" + prefix + ".txt", locationName + "|" + loc.DisplayPosition.Latitude + "|" + loc.DisplayPosition.Longitude + Environment.NewLine);


                        }
                    }
                    catch
                    {

                    }







                }
                catch (Exception ex3)
                {

                    System.IO.File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "\\GetForwardGeocodeByHere_exception.txt", DateTime.Now.ToStr() + "url:" + locationName + " , location :" + rtn + ",exception:" + ex3.Message + Environment.NewLine);


                }
            }
            return loc;
        }
        public static string GetMAPBOXKey()
        {


            if (Global.MAPBOXKEY.ToStr().Trim().Length > 0)
                return Global.MAPBOXKEY.ToStr().Trim();

            string keyVal = "";

            try
            {
                using (TaxiDataContext dbX = new TaxiDataContext())
                {
                    dbX.CommandTimeout = 5;
                    keyVal = dbX.ExecuteQuery<string>("select apikey from mapkeys where maptype='mapbox'").FirstOrDefault();
                    Global.MAPBOXKEY = keyVal;

                }
            }
            catch
            {

            }
            return keyVal;
        }


        public static string GetHEREKey()
        {


            if (Global.HEREKEY.ToStr().Trim().Length > 0)
                return Global.HEREKEY.ToStr().Trim();

            string keyVal = "";

            try
            {
                using (TaxiDataContext dbX = new TaxiDataContext())
                {
                    dbX.CommandTimeout = 5;
                    keyVal = dbX.ExecuteQuery<string>("select apikey from mapkeys where maptype='here'").FirstOrDefault();
                    Global.HEREKEY = keyVal;

                }
            }
            catch
            {

            }
            return keyVal;
        }
        public static List<LocationList> GetMapBoxAddressData(string locationName, bool searchcoordinates = false)
        {

            ResponseWebApi response = new ResponseWebApi();

            var list = new List<LocationList>();

            try
            {


                string apiKey = General.GetMAPBOXKey();
                string Address = locationName;
                int Limit = 10;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                using (HttpClient httpClient = new HttpClient())
                {

                    string placeSerachApiUrl = "https://api.mapbox.com/geocoding/v5/mapbox.places/" + Address + ".json?access_token=" + apiKey;

                    if (Global.centerPoint.ToStr().Trim().Length == 0)
                    {
                        using (TaxiDataContext db = new TaxiDataContext())
                        {
                            var Query = "SELECT SubCompanyId, Latitude, Longitude, Radius, Region from Gen_Subcompany_Details";
                            var SubcompanyDetails = db.ExecuteQuery<Classes.Gen_SubCompany_Details>(Query).FirstOrDefault();
                            Global.centerPoint = SubcompanyDetails.Longitude + "," + SubcompanyDetails.Latitude;

                        }

                    }
                    string suggestRequestUri = $"{placeSerachApiUrl}&limit={Limit}&proximity={Global.centerPoint}&country=ca";
                    // var suggestResponse = new System.Net.WebClient().DownloadString(suggestRequestUri);


                    string suggestResponseBody = new System.Net.WebClient().DownloadString(suggestRequestUri);

                    var suggestData = new RootMapBox();
                    suggestData.features = new List<FeatureMapBox>();
                    suggestData = Newtonsoft.Json.JsonConvert.DeserializeObject<RootMapBox>(suggestResponseBody);


                    if (suggestData.features != null)
                    {
                        foreach (var row in suggestData.features)
                        {

                            string CleanAddress = row.place_name.Replace(", Canada", "").Replace("#", "").Trim();


                            if (CleanAddress.ToLower().Trim() == locationName.ToLower().Trim())

                            {
                                LocationList locationList = new LocationList();
                                locationList.AddressLine = CleanAddress;
                                string PlactType = row.place_type[0].ToStr().ToLower();
                                if (row.center != null)
                                {
                                    locationList.Longitude = row.center[0];
                                    locationList.Latitude = row.center[1];
                                }
                                if (PlactType == "address")
                                {
                                    locationList.LocationTypeId = "0";

                                }
                                else
                                {
                                    if (row.properties.category.ToStr().ToLower() == "airport")
                                    {
                                        locationList.LocationTypeId = "1";
                                    }
                                    else
                                    {
                                        locationList.LocationTypeId = "0";
                                    }
                                }

                                list.Add(locationList);
                                break;

                            }


                        }


                        if (searchcoordinates && list.Count == 0)
                        {
                            try
                            {
                                //

                                try
                                {
                                    File.AppendAllText(AppContext.BaseDirectory + "\\getmapboxaddressdata_notmatch.txt", DateTime.Now + ",Location:" + locationName + ", response:" + suggestResponseBody + Environment.NewLine);
                                }
                                catch
                                {

                                }

                                if (locationName.ToStr().Contains(","))
                                {
                                    string newlocationname = locationName.Substring(0, locationName.IndexOf(',')).ToStr().Replace(",", "").Trim();

                                    File.AppendAllText(AppContext.BaseDirectory + "\\getmapboxaddressdata_notmatchnewlocationname.txt", DateTime.Now + ",Location:" + locationName + ",newlocation:" + newlocationname + ", response:" + suggestResponseBody + Environment.NewLine);


                                    placeSerachApiUrl = "https://api.mapbox.com/geocoding/v5/mapbox.places/" + newlocationname + ".json?access_token=" + apiKey;


                                    suggestRequestUri = $"{placeSerachApiUrl}&limit={Limit}&proximity={Global.centerPoint}&country=ca";


                                    suggestResponseBody = new System.Net.WebClient().DownloadString(suggestRequestUri);

                                    var suggestData2 = new RootMapBox();
                                    suggestData2.features = new List<FeatureMapBox>();
                                    suggestData2 = Newtonsoft.Json.JsonConvert.DeserializeObject<RootMapBox>(suggestResponseBody);


                                    foreach (var row in suggestData2.features)
                                    {

                                        string CleanAddress = row.place_name.Replace(", Canada", "").Replace("#", "").Trim();


                                        if (CleanAddress.ToLower().Trim() == locationName.ToLower().Trim())

                                        {
                                            //
                                            try
                                            {


                                                File.AppendAllText(AppContext.BaseDirectory + "\\getmapboxaddressdata_match2ndattempt.txt", DateTime.Now + ",Location:" + locationName + ", response:" + suggestResponseBody + Environment.NewLine);
                                            }
                                            catch
                                            {

                                            }


                                            LocationList locationList = new LocationList();
                                            locationList.AddressLine = CleanAddress;
                                            string PlactType = row.place_type[0].ToStr().ToLower();
                                            if (row.center != null)
                                            {
                                                locationList.Longitude = row.center[0];
                                                locationList.Latitude = row.center[1];
                                            }
                                            if (PlactType == "address")
                                            {
                                                locationList.LocationTypeId = "0";

                                            }
                                            else
                                            {
                                                if (row.properties.category.ToStr().ToLower() == "airport")
                                                {
                                                    locationList.LocationTypeId = "1";
                                                }
                                                else
                                                {
                                                    locationList.LocationTypeId = "0";
                                                }
                                            }

                                            list.Add(locationList);
                                            break;

                                        }

                                    }


                                }


                            }
                            catch
                            {

                            }
                        }


                        if (suggestData.features.Count > 0 && list.Count == 0)
                        {
                            foreach (var row in suggestData.features)
                            {

                                string CleanAddress = row.place_name.Replace(", Canada", "").Replace("#", "").Trim();

                                //

                                LocationList locationList = new LocationList();
                                locationList.AddressLine = CleanAddress;
                                string PlactType = row.place_type[0].ToStr().ToLower();
                                if (row.center != null)
                                {
                                    locationList.Longitude = row.center[0];
                                    locationList.Latitude = row.center[1];
                                }
                                if (PlactType == "address")
                                {
                                    locationList.LocationTypeId = "0";

                                }
                                else
                                {
                                    if (row.properties.category.ToStr().ToLower() == "airport")
                                    {
                                        locationList.LocationTypeId = "1";
                                    }
                                    else
                                    {
                                        locationList.LocationTypeId = "0";
                                    }
                                }

                                list.Add(locationList);




                            }
                        }

                    }

                    //if (suggestData.features != null)
                    //{
                    //    foreach (var row in suggestData.features)
                    //    {
                    //        LocationList locationList = new LocationList();
                    //        string CleanAddress = row.place_name.Replace(", United States", "");
                    //        locationList.AddressLine = CleanAddress;
                    //        string PlactType = row.place_type[0].ToStr().ToLower();
                    //        if (row.center != null)
                    //        {
                    //            locationList.Longitude = row.center[0];
                    //            locationList.Latitude = row.center[1];
                    //        }
                    //        if (PlactType == "address")
                    //        {
                    //            locationList.LocationTypeId = "0";

                    //        }
                    //        else
                    //        {
                    //            if (row.properties.category.ToStr().ToLower() == "airport")
                    //            {
                    //                locationList.LocationTypeId = "1";
                    //            }
                    //            else
                    //            {
                    //                locationList.LocationTypeId = "0";
                    //            }
                    //        }

                    //        list.Add(locationList);

                    //    }

                    //}
                }
            }
            catch (Exception ex)
            {

            }
            return list;

        }

        public static string GetMessage(string message, Booking objBooking, long jobId)
        {
            try
            {


                string msg = message;


                Global.InitializeSMSTags();




                object propertyValue = string.Empty;
                foreach (var tag in Global.listofSMSTags.Where(c => msg.Contains(c.TagMemberValue)))
                {


                    switch (tag.TagObjectName)
                    {
                        case "booking":

                            if (objBooking == null)
                                objBooking = General.GetObject<Booking>(c => c.Id == jobId);

                            if (tag.TagPropertyValue.Contains('.'))
                            {

                                string[] val = tag.TagPropertyValue.Split(new char[] { '.' });

                                object parentObj = objBooking.GetType().GetProperty(val[0]).GetValue(objBooking, null);

                                if (parentObj != null)
                                {
                                    propertyValue = parentObj.GetType().GetProperty(val[1]).GetValue(parentObj, null);
                                }
                                else
                                    propertyValue = string.Empty;


                                break;
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(tag.ConditionNotNull) && objBooking.GetType().GetProperty(tag.ConditionNotNull) != null)
                                {

                                    if (tag.ConditionNotNull.ToStr() == "BabySeats" && tag.TagPropertyValue.ToStr() == "BabySeats")
                                    {
                                        propertyValue = objBooking.GetType().GetProperty(tag.TagPropertyValue2).GetValue(objBooking, null);

                                        if (!string.IsNullOrEmpty(propertyValue.ToStr().Trim()) && propertyValue.ToStr().Contains("<<<"))
                                        {
                                            string[] arr = propertyValue.ToStr().Split(new string[] { "<<<" }, StringSplitOptions.None);

                                            propertyValue = "B Seat 1 : " + arr[0].ToStr() + Environment.NewLine + "B Seat 2 : " + arr[1].ToStr();

                                        }

                                    }
                                    else if (objBooking.GetType().GetProperty(tag.ConditionNotNull).GetValue(objBooking, null) != null)
                                    {
                                        propertyValue = tag.ConditionNotNullReplacedValue.ToStr();
                                    }

                                }
                                else
                                {

                                    if (tag.ExpressionValue.ToStr().Trim().Length > 0)
                                    {
                                        try
                                        {
                                            char[] splitArr = new char[] { ',' };
                                            char[] splitArr2 = new char[] { '|' };
                                            string[] val = tag.ExpressionValue.Split(splitArr);

                                            string replaceMessage = val[0].ToStr();
                                            int? expressionApplied = null;
                                            foreach (var item in val.Where(c => c.EndsWith("|replacemessage") == false))
                                            {
                                                var str = item.Split(splitArr2);

                                                if (objBooking.GetType().GetProperty(str[0]) != null)
                                                {
                                                    if (objBooking.GetType().GetProperty(str[0]).GetValue(objBooking, null).ToStr() == str[1])
                                                    {
                                                        if (expressionApplied == null)
                                                            expressionApplied = 1;
                                                    }
                                                    else
                                                        expressionApplied = null;

                                                }
                                            }

                                            if (expressionApplied != null && expressionApplied == 1)
                                            {
                                                var replacearr = replaceMessage.Split(splitArr2);

                                                msg = msg.Replace(replacearr[0], replacearr[1]);
                                            }
                                            else
                                            {
                                                propertyValue = objBooking.GetType().GetProperty(tag.TagPropertyValue).GetValue(objBooking, null);
                                            }
                                        }
                                        catch
                                        {
                                            propertyValue = objBooking.GetType().GetProperty(tag.TagPropertyValue).GetValue(objBooking, null);

                                        }

                                    }
                                    else
                                    {

                                        propertyValue = objBooking.GetType().GetProperty(tag.TagPropertyValue).GetValue(objBooking, null);
                                    }



                                }
                            }


                            if (string.IsNullOrEmpty(propertyValue.ToStr()) && !string.IsNullOrEmpty(tag.TagPropertyValue2))
                            {
                                propertyValue = objBooking.GetType().GetProperty(tag.TagPropertyValue2).GetValue(objBooking, null);
                            }
                            break;


                        case "Booking_ViaLocations":
                            if (tag.TagPropertyValue == "ViaLocValue")
                            {


                                string[] VilLocs = null;
                                int cnt = 1;
                                VilLocs = objBooking.Booking_ViaLocations.Select(c => cnt++.ToStr() + ". " + c.ViaLocValue).ToArray();
                                if (VilLocs.Count() > 0)
                                {

                                    string Locations = "VIA POINT(s) : \n" + string.Join("\n", VilLocs);
                                    propertyValue = Locations;
                                }
                                else
                                    propertyValue = string.Empty;

                            }
                            break;


                        case "driver":


                            if (tag.TagPropertyValue.Contains('.'))
                            {

                                string[] val = tag.TagPropertyValue.Split(new char[] { '.' });

                                object parentObj = objBooking.Fleet_Driver.DefaultIfEmpty().GetType().GetProperty(val[0]).GetValue(objBooking.Fleet_Driver.DefaultIfEmpty(), null);

                                if (parentObj != null)
                                {
                                    propertyValue = parentObj.GetType().GetProperty(val[1]).GetValue(parentObj, null);
                                }
                                else
                                    propertyValue = string.Empty;


                                break;
                            }

                            else
                            {
                                propertyValue = objBooking.Fleet_Driver.DefaultIfEmpty().GetType().GetProperty(tag.TagPropertyValue).GetValue(objBooking.Fleet_Driver.DefaultIfEmpty(), null);
                            }

                            if (string.IsNullOrEmpty(propertyValue.ToStr()) && !string.IsNullOrEmpty(tag.TagPropertyValue2))
                            {
                                //
                                propertyValue = objBooking.Fleet_Driver.DefaultIfEmpty().GetType().GetProperty(tag.TagPropertyValue2).GetValue(objBooking.Fleet_Driver.DefaultIfEmpty(), null);
                            }
                            break;


                        case "Fleet_Driver_Image":





                            //
                            try
                            {
                                if (!string.IsNullOrEmpty(tag.TagPropertyValue.ToStr()) && !string.IsNullOrEmpty(tag.TagPropertyValue2))
                                {
                                    if (objBooking.Fleet_Driver.DefaultIfEmpty().Fleet_Driver_Images.Count > 0)
                                    {
                                        string linkId = objBooking.Fleet_Driver.DefaultIfEmpty().Fleet_Driver_Images[0].PhotoLinkId.ToStr();

                                        if (linkId.ToStr().Length == 0)
                                            propertyValue = " ";
                                        else
                                        {
                                            // propertyValue = "http://tradrv.co.uk/drv.aspx?ref=" + objBooking.BookingNo.ToStr() + ":" + linkId;
                                            if (tag.TagMemberValue.ToStr().Trim().ToLower() == "<trackdrv>")
                                            {
                                                string encrypt = Cryptography.Encrypt(objBooking.BookingNo.ToStr() + ":" + linkId + ":" + Cryptography.Decrypt(System.Configuration.ConfigurationManager.AppSettings["ConnectionString"], "softeuroconnskey", true).ToStr() + ":" + objBooking.Id, "softeuroconnskey", true);


                                                propertyValue = "http://tradrv.co.uk/tck.aspx?q=" + encrypt;
                                                propertyValue = ToTinyURLS(propertyValue.ToStr());
                                            }
                                            else
                                            {

                                                propertyValue = "http://tradrv.co.uk/drv.aspx?ref=" + objBooking.BookingNo.ToStr() + ":" + linkId;
                                            }
                                        }
                                    }
                                    else
                                        propertyValue = " ";


                                    //      propertyValue = objBooking.Fleet_Driver.DefaultIfEmpty().GetType().GetProperty(tag.TagPropertyValue2).GetValue(objBooking.Fleet_Driver.DefaultIfEmpty(), null);
                                }
                            }
                            catch (Exception ex)
                            {
                                try
                                {

                                    File.AppendAllText(physicalPath + "\\log_trackdriver.txt", DateTime.Now.ToStr() + " : " + ex.Message + Environment.NewLine + Environment.NewLine);

                                }
                                catch
                                {

                                }


                            }
                            break;


                        case "Fleet_Driver_Documents":



                            if (!string.IsNullOrEmpty(tag.TagPropertyValue.ToStr()) && !string.IsNullOrEmpty(tag.TagPropertyValue2))
                            {

                                if (tag.TagPropertyValue.Contains("PHC Vehicle"))
                                {
                                    propertyValue = objBooking.Fleet_Driver.DefaultIfEmpty().Fleet_Driver_Documents.FirstOrDefault(c => c.DocumentId == Enums.DRIVER_DOCUMENTS.PCOVehicle)
                                                        .DefaultIfEmpty().BadgeNumber.ToStr();


                                }
                                else if (tag.TagPropertyValue.Contains("PHC Driver"))
                                {
                                    propertyValue = objBooking.Fleet_Driver.DefaultIfEmpty().Fleet_Driver_Documents.FirstOrDefault(c => c.DocumentId == Enums.DRIVER_DOCUMENTS.PCODriver)
                                                        .DefaultIfEmpty().BadgeNumber.ToStr();


                                }
                                else if (tag.TagPropertyValue.Contains("License"))
                                {
                                    propertyValue = objBooking.Fleet_Driver.DefaultIfEmpty().Fleet_Driver_Documents.FirstOrDefault(c => c.DocumentId == Enums.DRIVER_DOCUMENTS.LICENSE)
                                                        .DefaultIfEmpty().BadgeNumber.ToStr();


                                }
                                else if (tag.TagPropertyValue.Contains("Insurance"))
                                {
                                    propertyValue = objBooking.Fleet_Driver.DefaultIfEmpty().Fleet_Driver_Documents.FirstOrDefault(c => c.DocumentId == Enums.DRIVER_DOCUMENTS.Insurance)
                                                        .DefaultIfEmpty().BadgeNumber.ToStr();

                                }
                                else if (tag.TagPropertyValue.Contains("MOT"))
                                {
                                    propertyValue = objBooking.Fleet_Driver.DefaultIfEmpty().Fleet_Driver_Documents.FirstOrDefault(c => c.DocumentId == Enums.DRIVER_DOCUMENTS.MOT)
                                                        .DefaultIfEmpty().BadgeNumber.ToStr();

                                }



                            }
                            break;



                        default:
                            propertyValue = objBooking.Gen_SubCompany.GetType().GetProperty(tag.TagPropertyValue).GetValue(objBooking.Gen_SubCompany, null);
                            break;

                    }




                    msg = msg.Replace(tag.TagMemberValue,
                        tag.TagPropertyValuePrefix.ToStr() + string.Format(tag.TagDataFormat, propertyValue) + tag.TagPropertyValueSuffix.ToStr());




                }

                try
                {

                    if (msg.ToStr().Contains("="))
                        msg = msg.Replace("=", "^");


                }
                catch
                {

                }




                return msg.Replace("\n\n", "\n");



            }
            catch (Exception ex)
            {
                try
                {

                    File.AppendAllText(physicalPath + "\\log_trackdriver.txt", DateTime.Now.ToStr() + " : " + ex.Message + Environment.NewLine);

                }
                catch
                {

                }
                // ENUtils.ShowMessage(ex.Message);
                return "";
            }
        }


        public static void AddSMS(string mobileNo, string message, int smsType)
        {

            try
            {

                string mobNo = mobileNo;



                if (mobNo.ToStr().StartsWith("00") == false)
                {

                    int idx = -1;
                    if (mobNo.StartsWith("044") == true)
                    {
                        idx = mobNo.IndexOf("044");
                        mobNo = mobNo.Substring(idx + 3);
                        mobNo = mobNo.Insert(0, "+44");
                    }

                    if (mobNo.StartsWith("07"))
                    {
                        mobNo = mobNo.Substring(1);
                    }

                    if (mobNo.StartsWith("044") == false || mobNo.StartsWith("+44") == false)
                        mobNo = mobNo.Insert(0, "+44");
                }





                HubProcessor.Instance.listofSMS.Add("request dispatchsms = " + mobNo.Trim() + " = " + message);

            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\AddSMS_exception.txt", DateTime.Now.ToStr() + "request:" + message + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
            }
        }

        public static ClsLic objLic = new ClsLic();

        public static string UpdateSupplierStatus(string mesg)
        {
            //
            try
            {
                File.AppendAllText(AppContext.BaseDirectory + "\\UpdateSupplierStatus_gen.txt", DateTime.Now.ToStr() + "request:" + mesg + Environment.NewLine);
            }
            catch
            {

            }
            try
            {
                GettAPICall.ClsSupplierData obj = Newtonsoft.Json.JsonConvert.DeserializeObject<GettAPICall.ClsSupplierData>(mesg);


                if (obj.Status.ToStr().ToLower() == "cancelled")
                    obj.BookingStatusId = Enums.BOOKINGSTATUS.CANCELLED.ToInt();

                //


                return CallSupplierApi.UpdateStatus(obj.JobId, obj.BookingStatusId);


            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\UpdateSupplierStatus_gen_exception.txt", DateTime.Now.ToStr() + "request:" + mesg + Environment.NewLine);
                }
                catch
                {

                }
                return ex.Message;

            }

        }
        public static string GetHalfPostCodeMatch(string value)
        {
            string postCode = "";





            string ukAddress = @"^(GIR 0AA)|((([A-PR-UWYZ][0-9][0-9]?)|(([A-PR-UWYZ][A-HK-Y][0-9][0-9]?)|(([A-PR-UWYZ][0-9][A-HJKSTUW])|([A-PR-UWYZ][A-HK-Y][0-9][ABEHMNPRVWXY])))) [0-9][A-BD-HJLNP-UW-Z]{2})$";


            Regex reg = new Regex(ukAddress);
            Match em = reg.Match(value);

            if (em != null)
                postCode = em.Value;

            if (em.Value == "")
            {
                ukAddress = @"[A-Z]{1,2}[0-9R][0-9A-Z]?";
                reg = new Regex(ukAddress);
                MatchCollection mat = reg.Matches(value);


                foreach (Match item in mat)
                {
                    if (item.Value.ToString().IsAlpha() == false)
                        postCode += item.Value.ToString() + " ";

                }

            }


            if (postCode.WordCount() == 2 && postCode.Contains(" "))
                postCode = postCode.Split(' ')[0];



            if (!string.IsNullOrEmpty(postCode.Trim()))
            {
                postCode = CheckIfSpecialPostCode(postCode.Trim());

            }

            return postCode.Trim();

        }
        public static string GetETATime(string origin, string destination, string key)
        {
            string res = "";
            try
            {
                var obj = new
                {
                    originLat = Convert.ToDouble(origin.Split(',')[0]),
                    originLng = Convert.ToDouble(origin.Split(',')[1]),
                    destLat = Convert.ToDouble(destination.Split(',')[0]),
                    destLng = Convert.ToDouble(destination.Split(',')[1]),
                    //defaultclientid = AppVars.objPolicyConfiguration.DefaultClientId.ToStr(),
                    defaultclientid = "DemoNadeem",
                    keys = key,
                    //MapType = AppVars.objPolicyConfiguration.MapType.ToInt(),
                    MapType = 2,
                    sourceType = "dispatch"

                };


                string json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(obj);
                //string API = objLic.AppServiceUrl + "GetETA" + "?json=" + json;
                string API = "https://www.treasureonlineapi.co.uk/CabTreasureWebApi/Home/" + "GetETA" + "?json=" + json;


                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(API);
                request.ContentType = "application/json; charset=utf-8";
                request.Accept = "application/json";
                request.Method = WebRequestMethods.Http.Post;
                request.Proxy = null;
                request.ContentLength = 0;

                using (WebResponse responsea = request.GetResponse())
                {

                    using (StreamReader sr = new StreamReader(responsea.GetResponseStream()))
                    {
                        res = sr.ReadToEnd().ToStr();
                    }
                }


            }
            catch (Exception ex)
            {

            }
            return res;

        }
        public static string GetToken()
        {

            //var issuer = "https://cabtreasure.com/";  //normally this will be your site URL    

            //var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            //var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(DispatchHelper.GetApplicationSetting(AppSettingViewModel.JWT_Secret)));
            //var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            //Create a List of Claims, Keep claims name short
            //claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

            //Create Security Token object by giving required parameters
            //var token = new JwtSecurityToken(issuer, //Issure    
            //                issuer,  //Audience    
            //                claims,
            //                expires: DateTime.Now.AddMinutes(expiryMin),
            //                signingCredentials: credentials);
            //var jwt_token = new JwtSecurityTokenHandler().WriteToken(token);
            // return jwt_token;
            return DateTime.Now.ToString();
        }


        #region WebApi

        public static void CreateLog(string name, string phoneNumber, DateTime date, string duration, string line, string calledNumber)
        {
            try
            {
                using (TaxiDataContext db = new TaxiDataContext())
                {

                    try
                    {

                        name = db.stp_GetCallerInfo(phoneNumber, "").FirstOrDefault().DefaultIfEmpty().Name;

                    }
                    catch
                    {
                    }


                    db.CommandTimeout = 5;
                    db.stp_AddCallLog(name, phoneNumber, date, duration, line, 1, calledNumber);

                    try
                    {
                        //
                        File.AppendAllText(AppContext.BaseDirectory + "\\CallAnswered.txt", DateTime.Now.ToStr() + " number :" + phoneNumber + ",ext:" + line.ToStr() + Environment.NewLine);
                    }
                    catch
                    {

                    }


                }
            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\exception_CallerIDCreateLog.txt", DateTime.Now.ToStr() + ": " + ex.Message + "|" + ex.InnerException.StackTrace + "|" + ex.InnerException.Message + Environment.NewLine);
                }
                catch
                {

                }
            }
        }

        public static void BroadcastToWebControllers(string message)
        {
            SocketIO.SendToSocket("", message, "ReceiveData", "WebApp");


        }


        public static string AllocateDriver(TaxiDataContext db, Gen_SysPolicy_Configuration objPolicy, Booking objBooking, Fleet_Driver ObjDriver, int dispatchType = 0)
        {

            string message = string.Empty;
            int? driverId = null;

            try
            {

                try
                {
                    int defaultAllocationLimit = HubProcessor.Instance.objPolicy.AllocateDrvPreExistJobLimit.ToInt();
                     driverId = ObjDriver!=null ? ObjDriver.Id.ToIntorNull():null;
                    int? oldDriverId = null;
                    string oldDriverNo = string.Empty;


                    //    DateTime? pickupDateAndTime = ObjMaster.Current.PickupDateTime.ToDateTimeorNull();


                    bool isConfirmedDriver = true;

                    if (driverId == null)
                        isConfirmedDriver = false;
                    //else
                    //{
                    //    if (chkConfirmed.Visible == false)
                    //        chkConfirmed.Checked = true;


                    //}

                    try
                    {
                        System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "AllocateDriver.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",driverid:" + driverId + ",jobid:"+objBooking.Id+ Environment.NewLine);
                    }
                    catch
                    {

                    }



                    if (driverId != null)
                    {

                        //    var ObjDriver = General.GetObject<Fleet_Driver>(c => c.Id == driverId);
                        string allocateDrvNo = "";
                        if (ObjDriver != null)
                        {
                            allocateDrvNo = ObjDriver.DriverNo.ToStr().Trim();
                            if (ObjDriver.VehicleTypeId != null)
                            {
                                if (objBooking.AttributeValues.ToStr().Trim().Length > 0)
                                {

                                    string[] bookingAttrs = objBooking.AttributeValues.ToStr().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                    string drvAttributes = ObjDriver.AttributeValues.ToStr() + "," + ObjDriver.Fleet_VehicleType.AttributeValues;

                                    int totalAttr = bookingAttrs.Count();
                                    int matchCnt = 0;
                                    string unmatchedAttrValue = string.Empty;
                                    string[] drvAttrsArr = drvAttributes.ToStr().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                                    foreach (var item in bookingAttrs)
                                    {


                                        if (drvAttrsArr.Count(c => c.ToLower() == item.ToLower()) > 0)
                                        {
                                            matchCnt++;

                                        }
                                        else
                                        {

                                            unmatchedAttrValue += item + ",";
                                        }
                                    }

                                    if (matchCnt != totalAttr)
                                    {

                                        if (unmatchedAttrValue.EndsWith(","))
                                        {
                                            unmatchedAttrValue = unmatchedAttrValue.Substring(0, unmatchedAttrValue.LastIndexOf(","));

                                        }

                                        message = ("Driver : " + ObjDriver.DriverNo + " doesn't have attributes (" + unmatchedAttrValue + ")");
                                        return message;
                                    }
                                }

                                //if (AppVars.listUserRights.Count(c => c.functionId == "RESTRICT ON DESPATCH JOB TO INVALID VEHICLE DRIVER") > 0)
                                //{
                                string vehAttributes = objBooking.Fleet_VehicleType.DefaultIfEmpty().AttributeValues.ToStr().Trim();

                                if (vehAttributes.Length > 0)
                                {

                                    bool MatchedAttr = false;
                                    foreach (var item in vehAttributes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                    {
                                        if (ObjDriver.VehicleTypeId.ToInt() == item.ToInt())
                                        {
                                            MatchedAttr = true;
                                            break;

                                        }

                                    }



                                    if (MatchedAttr == false)
                                    {

                                        message = "This Job is for " + objBooking.Fleet_VehicleType.VehicleType.ToStr() + " Vehicle" + Environment.NewLine +
                                                                                    "and Driver no " + ObjDriver.DriverNo + " have " + ObjDriver.Fleet_VehicleType.VehicleType + ".";

                                        return message;

                                    }

                                }
                                else
                                {

                                    if (ObjDriver.Fleet_VehicleType.NoofPassengers.ToInt() < objBooking.Fleet_VehicleType.NoofPassengers.ToInt())
                                    {
                                        message = "This Job is for " + objBooking.Fleet_VehicleType.VehicleType.ToStr() + " Vehicle" + Environment.NewLine +
                                                                                      "and Driver no " + ObjDriver.DriverNo + " have " + ObjDriver.Fleet_VehicleType.VehicleType + ".";


                                        return message;
                                    }
                                }

                                //    }
                                //else
                                //{


                                //    if (ObjDriver.Fleet_VehicleType.NoofPassengers.ToInt() < ObjMaster.Current.Fleet_VehicleType.NoofPassengers.ToInt())
                                //    {
                                //        if (DialogResult.No == MessageBox.Show("This Job is for " + ObjMaster.Current.Fleet_VehicleType.VehicleType.ToStr() + " Vehicle" + Environment.NewLine +
                                //                                                  "and Driver no " + ObjDriver.DriverNo + " have " + ObjDriver.Fleet_VehicleType.VehicleType + "." + Environment.NewLine
                                //                                              + "Do you still want to Allocate this Job to that Driver " + ObjDriver.DriverNo + " ?", "Despatch", MessageBoxButtons.YesNo))
                                //        {
                                //            return;

                                //        }



                                //    }

                                //}

                            }
                        }

                        //try
                        //{
                        //    if ((driverId != null && objBooking.DriverId == null) || (driverId != null && objBooking.DriverId != null && driverId != objBooking.DriverId))
                        //    {

                        //        if (IsDriverDocumentExpired(driverId.ToInt(), ObjDriver))
                        //            return;

                        //    }
                        //}
                        //catch
                        //{

                        //}
                    }



                    //


                    if (objBooking != null)
                    {
                        if (driverId != null || (objBooking.DriverId != null && objBooking.BookingStatusId.ToInt() == Enums.BOOKINGSTATUS.WAITING))
                        {


                            //if ((driverId != null && ObjMaster.Current.DriverId == null) || (driverId!=null && ObjMaster.Current.DriverId!=null && driverId!=ObjMaster.Current.DriverId))
                            //{

                            //   if  (IsDriverDocumentExpired(driverId.ToInt()))
                            //       return;

                            //}

                            try
                            {
                                System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "AllocateDriver1.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",driverid:" + driverId + ",objbooking.driverid:" + objBooking.DriverId + Environment.NewLine);
                            }
                            catch
                            {

                            }


                            if (driverId == null && objBooking.DriverId != null)
                            {
                                oldDriverNo =db.Fleet_Drivers.Where(c=>c.Id==objBooking.DriverId).Select(c=>c.DriverNo).FirstOrDefault();


                            }

                            if (objBooking.DriverId != null)
                                oldDriverId = objBooking.DriverId;


                            //   ObjMaster.CheckDataValidation = false;

                            //   ObjMaster.Edit();

                            objBooking.DriverId = driverId;

                            try
                            {
                                System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "AllocateDriver2.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",driverid:" + driverId + ",objbooking.driverid:" + objBooking.DriverId + Environment.NewLine);
                            }
                            catch
                            {

                            }

                            objBooking.IsConfirmedDriver = driverId != null ? isConfirmedDriver : false;





                            if (objBooking.BookingStatusId.ToInt() == Enums.BOOKINGSTATUS.NOTACCEPTED)
                                objBooking.BookingStatusId = Enums.BOOKINGSTATUS.WAITING;



                            if (driverId == null || (oldDriverId != null && oldDriverId != driverId && objBooking.BookingStatusId != Enums.BOOKINGSTATUS.WAITING))
                            {
                                objBooking.BookingStatusId = Enums.BOOKINGSTATUS.WAITING;
                                //cancelJob = true;

                            }


                            //  jobId = ObjMaster.Current.Id;
                            //   allocatedJobId = jobId;

                            //  ObjMaster.CheckCustomerValidation = false;
                            //  ObjMaster.DisableUpdateReturnJob = true;

                            //     objBooking.Save();


                            //if (ObjMaster.Current.BookingTypeId.ToInt() == Enums.BOOKING_TYPES.THIRDPARTY && ObjMaster.Current.OnlineBookingId != null)
                            //{
                            //    General.UpdateSupplierStatus(jobId, ObjMaster.Current.DriverId.ToInt(), ObjMaster.Current.BookingStatusId.ToInt(), "allocated", "");
                            //}

                            //this.Close();


                            //if (IsOpenFrom == 1)
                            //{

                            //    RefreshTodayBookingsDashboard();

                            //}
                            //else
                            //{
                            //    RefreshTodayAndPreBookingsDashboard();
                            //    //  AppVars.frmMDI.RefreshTodayAnPreDashboard();

                            //}

                            string Msg = string.Empty;


                            if (driverId != null)
                            {
                                if (isConfirmedDriver)
                                    Msg = "Job is Allocated and confirmed to Driver (" + objBooking.Fleet_Driver.DriverNo.ToStr() + ")";
                                else
                                    Msg = "Job is Allocated  to Driver (" + objBooking.Fleet_Driver.DriverNo.ToStr() + ")";


                            }
                            else if (driverId == null && !string.IsNullOrEmpty(oldDriverNo))
                                Msg = "Job is De-Allocated from Driver (" + oldDriverNo + ")";


                            db.stp_BookingLog(objBooking.Id, "", Msg);

                            db.SubmitChanges();

                            //if (cancelJob)
                            //{

                            //    if (AppVars.objPolicyConfiguration.DespatchOfflineJobs.ToBool())
                            //    {
                            //        db.stp_DeleteDrvOfflineJob(ObjMaster.Current.Id, oldDriverId);
                            //    }
                            //}






                            //if (cancelJob)
                            //{

                            //    //For TCP Connection
                            //    if (AppVars.objPolicyConfiguration.IsListenAll.ToBool())
                            //    {
                            //        new Thread(delegate ()
                            //        {
                            //            General.SendMessageToPDA("request pda=" + oldDriverId + "=" + jobId + "=Cancelled Pre Job>>" + jobId + "=2");
                            //        }).Start();

                            //    }

                            //}





                        }
                        else
                        {

                            message = "Required: Driver";
                            //   ENUtils.ShowMessage("Required: Driver");
                        }



                    }

                }
                catch (Exception ex)
                {

                    try
                    {
                        System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "AllocateDriver_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",driverid:" + driverId + ",objbooking.driverid:" + objBooking.DriverId + ",exception:"+ex.Message+ Environment.NewLine);
                    }
                    catch
                    {

                    }
                }

            }
            catch (Exception ex)
            {

            }


            return message;

        }


        public static void OnDespatching(Gen_SysPolicy_Configuration objPolicy, Booking objBooking, Fleet_Driver ObjDriver, int dispatchType = 0, bool IsMultiDispatch = false)
        {




            try
            {

                if (ObjDriver != null && objBooking != null)
                {



                    string customerMobileNo = objBooking.CustomerMobileNo.ToStr().Trim();
                    // For testing Purpose
                    //  customerMobileNo = "03323755646"; 
                    //
                    string customerName = objBooking.CustomerName;

                    string via = string.Join(",", objBooking.Booking_ViaLocations.Select(c => c.ViaLocValue.ToStr()).ToArray<string>());

                    if (!string.IsNullOrEmpty(via.Trim()))
                        via = "Via: " + via;

                    //    string specialReq = objBooking.SpecialRequirements.ToStr().Trim();
                    //if (!string.IsNullOrEmpty(specialReq))
                    //    specialReq = "Special Req: " + specialReq;



                    bool enablePDA = HubProcessor.Instance.objPolicy.EnablePDA.ToBool();

                    string custNo = !string.IsNullOrEmpty(objBooking.CustomerMobileNo) ? objBooking.CustomerMobileNo : objBooking.CustomerPhoneNo.ToStr();



                    // Send To Driver









                    string paymentType = objBooking.Gen_PaymentType.PaymentCategoryId == null ? objBooking.Gen_PaymentType.DefaultIfEmpty().PaymentType.ToStr()
                            : objBooking.Gen_PaymentType.Gen_PaymentCategory.CategoryName.ToStr();

                    string strDeviceRegistrationId = ObjDriver.DeviceId.ToStr();
                    string journey = "O/W";

                    //if (objBooking.JourneyTypeId.ToInt() == 2)
                    //{
                    //    journey = "Return";

                    //}
                    if (objBooking.JourneyTypeId.ToInt() == 3)
                    {
                        journey = "W/R";
                    }


                    string IsExtra = (objBooking.CompanyId != null || objBooking.FromLocTypeId == Enums.LOCATION_TYPES.AIRPORT || objBooking.ToLocTypeId == Enums.LOCATION_TYPES.AIRPORT) ? "1" : "0";
                    int i = 1;
                    string viaP = "";



                    if (objBooking.Booking_ViaLocations.Count > 0)
                    {



                        viaP = string.Join(" * ", objBooking.Booking_ViaLocations.Select(c => "(" + i++.ToStr() + ")" + c.ViaLocValue.ToStr()).ToArray<string>());
                    }


                    string mobileNo = objBooking.CustomerMobileNo.ToStr();
                    string telNo = objBooking.CustomerPhoneNo.ToStr();

                    // decimal drvPdaVersion = ObjDriver.Fleet_Driver_PDASettings.Count > 0 ? ObjDriver.Fleet_Driver_PDASettings[0].CurrentPdaVersion.ToDecimal() : 9.40m;


                    if (string.IsNullOrEmpty(mobileNo) && !string.IsNullOrEmpty(telNo))
                    {
                        mobileNo = telNo;
                    }
                    else if (!string.IsNullOrEmpty(mobileNo) && !string.IsNullOrEmpty(telNo))
                    {
                        mobileNo += "/" + telNo;
                    }


                    string pickUpPlot = "";
                    string dropOffPlot = "";
                    string companyName = string.Empty;

                    //if (drvPdaVersion < 11 && objBooking.CompanyId != null && objBooking.Gen_Company.DefaultIfEmpty().AccountTypeId.ToInt() != Enums.ACCOUNT_TYPE.CASH)
                    //    companyName = objBooking.Gen_Company.DefaultIfEmpty().CompanyName;
                    //    else
                    companyName = objBooking.Gen_Company.DefaultIfEmpty().CompanyName.ToStr();


                    //error in 13.4 => if its a plot job, then pickup point is hiding in pda.
                    //if (drvPdaVersion >9 && drvPdaVersion!=13.4m)
                    //{





                    pickUpPlot = objBooking.ZoneId != null ? "<<<" + objBooking.Gen_Zone1.DefaultIfEmpty().ZoneName.ToStr() : "";
                    dropOffPlot = objBooking.DropOffZoneId != null ? "<<<" + objBooking.Gen_Zone.DefaultIfEmpty().ZoneName.ToStr() : "";
                    //  }






                    string startJobPrefix = "JobId:";

                    if (dispatchType == 2)
                        startJobPrefix = "fojJobId:";
                    else if (dispatchType == 3)
                        startJobPrefix = "PreJobId:";
                    //if (Instance.objPolicy.PDAJobAlertOnly.ToBool() &&  ObjDriver.Fleet_Driver_PDASettings[0].CurrentPdaVersion.ToDecimal() >= 8.3m && ObjDriver.Fleet_Driver_PDASettings[0].ShowJobAsAlert.ToBool())
                    //{
                    //    startJobPrefix = "AlertJobId:";                                   

                    //}

                    string fromAddress = objBooking.FromAddress.ToStr().Trim();
                    string toAddress = objBooking.ToAddress.ToStr().Trim();

                    if (objBooking.FromLocTypeId.ToInt() == Enums.LOCATION_TYPES.POSTCODE || objBooking.FromLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT)
                    {
                        fromAddress = objBooking.FromStreet.ToStr() + " " + objBooking.FromAddress.ToStr();

                    }






                    if (objBooking.ToLocTypeId.ToInt() == Enums.LOCATION_TYPES.POSTCODE || objBooking.ToLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT)
                    {
                        toAddress = objBooking.ToStreet.ToStr() + " " + objBooking.ToAddress.ToStr();
                    }




                    //half card and cash
                    string specialRequirements = objBooking.SpecialRequirements.ToStr();
                    if (objBooking.SecondaryPaymentTypeId != null && objBooking.CashFares.ToDecimal() > 0)
                    {

                        specialRequirements += " , Additional Cash Payment : " + objBooking.CashFares.ToDecimal();
                    }

                    //  decimal pdafares = objBooking.GetType().GetProperty(HubProcessor.Instance.objPolicy.PDAFaresPropertyName.ToStr().Trim()).GetValue(objBooking, null).ToDecimal();

                    //  pdafares = objBooking.TotalCharges.ToDecimal();

                    string msg = string.Empty;


                    //string showFaresValue = objBooking.Gen_PaymentType.ShowFaresOnPDA.ToStr().Trim();




                    //string showFares = ",\"ShowFares\":\"" + showFaresValue + "\"";
                    //string showSummary = ",\"ShowSummary\":\"" + showFaresValue + "\"";
                    ////   string showSummary = string.Empty;




                    //string agentDetails = string.Empty;
                    //string parkingandWaiting = string.Empty;
                    //if (objBooking.CompanyId != null)
                    //{


                    //    if (HubProcessor.Instance.objPolicy.PickCommissionDeductionFromJobsTotal.ToBool())
                    //    {

                    //        agentDetails = ",\"AgentFees\":\"" + String.Format("{0:0.00}", objBooking.AgentCommission + objBooking.ServiceCharges.ToDecimal()) + "\"";
                    //    }
                    //    else
                    //    {
                    //        agentDetails = ",\"AgentFees\":\"" + String.Format("{0:0.00}", objBooking.AgentCommission) + "\"";

                    //    }

                    //    parkingandWaiting = ",\"Parking\":\"" + string.Format("{0:0.00}", objBooking.ParkingCharges) + "\",\"Waiting\":\"" + String.Format("{0:0.00}", objBooking.WaitingCharges) + "\"";

                    //}
                    //else
                    //{

                    //    if (HubProcessor.Instance.objPolicy.PickCommissionDeductionFromJobsTotal.ToBool())
                    //    {


                    //        agentDetails = ",\"AgentFees\":\"" + String.Format("{0:0.00}", objBooking.ServiceCharges.ToDecimal()) + "\"";

                    //    }


                    //    parkingandWaiting = ",\"Parking\":\"" + string.Format("{0:0.00}", objBooking.CongtionCharges) + "\",\"Waiting\":\"" + String.Format("{0:0.00}", objBooking.MeetAndGreetCharges) + "\"";
                    //    //


                    //}





                    //string fromdoorno = objBooking.FromDoorNo.ToStr().Trim();
                    //if (fromdoorno.Length > 0 && fromdoorno.WordCount() > 2 && fromdoorno.Contains(" "))
                    //{

                    //    try
                    //    {

                    //        fromdoorno = fromdoorno.Replace(" ", "-");
                    //    }
                    //    catch
                    //    {


                    //    }
                    //}


                    //if (drvPdaVersion == 23.50m && fromAddress.ToStr().Trim().Contains("-"))
                    //{
                    //    fromAddress = fromAddress.Replace("-", "  ");

                    //}


                    decimal pdafares = objBooking.FareRate.ToDecimal();


                    pdafares = pdafares + objBooking.MeetAndGreetCharges.ToDecimal() + objBooking.CongtionCharges.ToDecimal()
                              + objBooking.AgentCommission.ToDecimal() + objBooking.CashRate.ToDecimal() + objBooking.CashFares.ToDecimal() +
                          +objBooking.ExtraDropCharges.ToDecimal() + objBooking.ServiceCharges.ToDecimal();







                    string showFaresValue = objBooking.Gen_PaymentType.ShowFaresOnPDA.ToStr().Trim();

                    if (showFaresValue.ToStr() == "1" && objBooking.CompanyId != null && (objBooking.PaymentTypeId.ToInt() == 2 || objBooking.PaymentTypeId.ToInt() == 6))
                    {
                        pdafares = objBooking.CompanyPrice.ToDecimal() + objBooking.MeetAndGreetCharges.ToDecimal() + objBooking.CongtionCharges.ToDecimal()
                             + objBooking.AgentCommission.ToDecimal() + objBooking.CashRate.ToDecimal() + objBooking.CashFares.ToDecimal() +
                         +objBooking.ExtraDropCharges.ToDecimal() + objBooking.ServiceCharges.ToDecimal();


                    }

                    string showFares = ",\"ShowFares\":\"" + showFaresValue + "\"";
                    string showSummary = ",\"ShowSummary\":\"" + "0" + "\"";
                    //   string showSummary = string.Empty;




                    string agentDetails = string.Empty;
                    string parkingandWaiting = string.Empty;

                    agentDetails = ",\"AgentFees\":\"" + String.Format("{0:0.00}", objBooking.AgentCommission.ToDecimal() + objBooking.CashRate.ToDecimal() + objBooking.CashFares.ToDecimal() + objBooking.ServiceCharges.ToDecimal() + objBooking.ExtraDropCharges.ToDecimal()) + "\"";


                    parkingandWaiting = ",\"Parking\":\"" + string.Format("{0:0.00}", objBooking.CongtionCharges.ToDecimal()) + "\",\"Waiting\":\"" + String.Format("{0:0.00}", objBooking.MeetAndGreetCharges.ToDecimal()) + "\"";


                    string fromdoorno = objBooking.FromDoorNo.ToStr().Trim();
                    if (fromdoorno.Length > 0 && fromdoorno.WordCount() > 2 && fromdoorno.Contains(" "))
                    {

                        try
                        {

                            fromdoorno = fromdoorno.Replace(" ", "-");
                        }
                        catch
                        {


                        }
                    }

                    string appendString = "";


                    //try
                    //{
                    //    appendString = ",\"ShowOnlyPlot\":\"" + "0" + "\"" +
                    //     ",\"ExtraCharges\":\"" + objBooking.ExtraDropCharges.ToDecimal() + "\"" +
                    //      ",\"BookingFee\":\"" + 0.00 + "\"" +
                    //      ",\"BgColor\":\"" + "" + "\"";

                    //    if (objBooking.BookingDate.Value.AddMinutes(10) > objBooking.PickupDateTime.Value)
                    //    {

                    //        appendString += ",\"priority\":\"" + "ASAP" + "\"";
                    //    }

                    //}
                    //catch
                    //{

                    //}


                    try
                    {
                        appendString = ",\"ShowOnlyPlot\":\"" + "0" + "\"" +
                                ",\"BookingType\":\"" + objBooking.BookingType.BookingTypeName.ToStr() + "\"" +
                         ",\"ExtraCharges\":\"" + objBooking.ExtraDropCharges.ToDecimal() + "\"" +
                          ",\"BookingFee\":\"" + 0.00 + "\"" +
                          ",\"BgColor\":\"" + "" + "\"";

                        //if (objBooking.BookingDate.Value.AddMinutes(10) > objBooking.PickupDateTime.Value)
                        //{

                        //    appendString += ",\"priority\":\"" + "ASAP" + "\"";
                        //}

                    }
                    catch
                    {

                    }




                    string toDoorNo = objBooking.ToDoorNo.ToStr().Trim();

                    if (objBooking.ToLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT && objBooking.JourneyTypeId.ToInt() == Enums.JOURNEY_TYPES.RETURN)
                        toDoorNo = string.Empty;
                    else if (toDoorNo.Length > 0)
                        toDoorNo = toDoorNo + "-";

                    if (specialRequirements.ToStr().Contains("\""))
                        specialRequirements = specialRequirements.ToStr().Replace("\"", "-").Trim();

                    msg = startJobPrefix + "{ \"JobId\" :\"" + objBooking.Id.ToStr() +
                                     //  "\", \"Pickup\":\"" + (!string.IsNullOrEmpty(objBooking.FromDoorNo) ? fromdoorno + "-" + fromAddress + pickUpPlot : fromAddress + pickUpPlot) +
                                     // "\", \"Destination\":\"" + (!string.IsNullOrEmpty(objBooking.ToDoorNo) ? objBooking.ToDoorNo + "-" + toAddress + dropOffPlot : toAddress + dropOffPlot) + "\"," +


                                     "\", \"Pickup\":\"" + (!string.IsNullOrEmpty(objBooking.FromDoorNo) ? fromdoorno + "-" + fromAddress + pickUpPlot : fromAddress + pickUpPlot) +
                                           "\", \"Destination\":\"" + toDoorNo + toAddress + dropOffPlot + "\"," +


                                   "\"PickupDateTime\":\"" + string.Format("{0:dd/MM/yyyy   HH:mm}", objBooking.PickupDateTime) + "\"" +
                                   ",\"Cust\":\"" + objBooking.CustomerName + "\",\"Mob\":\"" + mobileNo + " " + "\",\"Fare\":\"" + string.Format("{0:0.00}", pdafares) + "\",\"Vehicle\":\"" + objBooking.Fleet_VehicleType.VehicleType + "\",\"Account\":\"" + companyName + " " + "\"" +
                                     ",\"Lug\":\"" + objBooking.NoofLuggages.ToInt() + "\",\"Passengers\":\"" + objBooking.NoofPassengers.ToInt() + "\",\"Journey\":\"" + journey + "\",\"Payment\":\"" + paymentType + "\",\"Special\":\"" + specialRequirements + " " + "\",\"Extra\":\"" + IsExtra + "\",\"Via\":\"" + viaP + " " + "\"" +

                                     ",\"CompanyId\":\"" + objBooking.CompanyId.ToInt() + "\",\"SubCompanyId\":\"" + objBooking.SubcompanyId.ToInt() + "\",\"QuotedPrice\":\"" + (objBooking.IsQuotedPrice.ToBool() ? "1" : "0") + "\"" +


                                     parkingandWaiting + ",\"DriverFares\":\"" + String.Format("{0:0.00}", objBooking.FareRate) + "\"" +
                                  agentDetails +
                                     ",\"Did\":\"" + ObjDriver.Id + "\",\"BabySeats\":\"" + objBooking.BabySeats.ToStr() + "\"" + showFares + showSummary + appendString + " }";





                    if (msg.Contains("\r\n"))
                    {
                        msg = msg.Replace("\r\n", " ").Trim();
                    }
                    else
                    {
                        if (msg.Contains("\n"))
                        {
                            msg = msg.Replace("\n", " ").Trim();

                        }

                    }

                    if (msg.Contains("&"))
                    {
                        msg = msg.Replace("&", "And");
                    }

                    if (msg.Contains(">"))
                        msg = msg.Replace(">", " ");


                    if (msg.Contains("="))
                        msg = msg.Replace("=", " ");




                    //try
                    //{
                    //    if (ReDespatchJob )
                    //    {


                    //        //if (objBooking.BookingStatusId.ToInt() == Enums.BOOKINGSTATUS.FOJ)
                    //        //{
                    //        //    new Thread(delegate ()
                    //        //    {
                    //        //        ReCallFOJBookingFromReDespatch(JobId, drvId);
                    //        //    }).Start();

                    //        //}
                    //        //else
                    //        //{

                    //            new Thread(delegate ()
                    //            {
                    //                // recover job
                    //                UpdateBookingStatus
                    //                ReCallDespatchBooking(JobId, drvId);
                    //            }).Start();


                    //      //  }


                    //    }
                    //}
                    //catch
                    //{

                    //}


                    //
                    //
                    int? status = 0;
                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        bool offlinejob = false;
                        status = Enums.BOOKINGSTATUS.PENDING;
                        var res = (db.ExecuteQuery<string>("select SetVal from AppSettings where SetKey='DisableAcceptJob'").FirstOrDefault());
                        if (dispatchType == 3 && res=="true")
                        {
                            status = Enums.BOOKINGSTATUS.PENDING_START;
                            offlinejob = true;
                            //
                        }
                        db.ExecuteQuery<int>("exec stp_DespatchedJobWithLogReason {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}", objBooking.Id, ObjDriver.Id
                            , ObjDriver.DriverNo.ToStr()
                            , ObjDriver.HasPDA.ToBool(), true, false, false,
                            "Admin", status, offlinejob, "").FirstOrDefault();


                        //  db.stp_DespatchedJobWithLogReason(objBooking.Id, ObjDriver.Id, ObjDriver.DriverNo.ToStr(), ObjDriver.HasPDA.ToBool(), true, false, false, "Admin", Enums.BOOKINGSTATUS.PENDING, false, "");
                    }
                    //
                    //
                    //using (TaxiDataContext db = new TaxiDataContext())
                    //{
                    //    db.ExecuteQuery<int>("exec stp_DespatchedJobWithLogReason {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}", objBooking.Id, ObjDriver.Id
                    //        , ObjDriver.DriverNo.ToStr()
                    //        , ObjDriver.HasPDA.ToBool(), true, false, false,
                    //        "Admin", Enums.BOOKINGSTATUS.PENDING, false, "", DateTime.Now.GetUtcTimeZone()).FirstOrDefault();


                    //  //  db.stp_DespatchedJobWithLogReason(objBooking.Id, ObjDriver.Id, ObjDriver.DriverNo.ToStr(), ObjDriver.HasPDA.ToBool(), true, false, false, "Admin", Enums.BOOKINGSTATUS.PENDING, false, "");
                    //}
                    if (status == Enums.BOOKINGSTATUS.PENDING_START)
                        General.requestPDA("request pda=" + ObjDriver.Id + "=" + 0 + "=" + "Message>>" + "You have received a Future Jobs" + ">>" + String.Format("{0:MM/dd/yyyy HH:mm:ss}", DateTime.Now) + "=4");
                    else
                        requestPDA("request pda=" + objBooking.Id + "=" + ObjDriver.Id + "=" + msg + "=1=" + ObjDriver.DriverNo);

                    //  General.SendMessageToPDA("request pda=" + objBooking.Id + "=" + ObjDriver.Id + "=" + msg + "=1=" + ObjDriver.DriverNo).Result.ToBool();
                    if (dispatchType == 3)
                        General.CallGetDashboardData();

                }
            }
            catch (Exception ex)
            {
                try
                {

                    File.AppendAllText(physicalPath + "\\OnDespatching_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {


                }
            }




        }

        public static bool ReCallFOJBooking(long jobId, int driverId)
        {

            try
            {
                (new TaxiDataContext()).stp_UpdateJobStatus(jobId, Enums.BOOKINGSTATUS.WAITING);




                if (HubProcessor.Instance.objPolicy.MapType.ToInt() == 1)
                {
                    requestPDA("request pda=" + driverId + "=" + jobId + "=" + "Cancelled Foj Job>>>" + jobId + "=4");
                }
                else
                {
                    requestPDA("request pda=" + driverId + "=" + jobId + "=" + "Cancelled Foj Job>>>" + jobId + "=2");
                }

                return true;

            }
            catch
            {

                return true;


            }




        }
        public static bool ReCallPreBooking(long jobId, int driverId, string UserName, bool recallOnly = false)
        {

            try
            {
                bool rtn = true;

                if (recallOnly == false)
                    (new TaxiDataContext()).stp_RecoverPreJob(jobId, Enums.BOOKINGSTATUS.WAITING, driverId, "", UserName.ToStr());




                if (HubProcessor.Instance.objPolicy.MapType.ToInt() == 1)
                {



                    requestPDA("request pda=" + driverId + "=" + jobId + "=" + "Cancelled Pre Job>>" + jobId + "=4");
                }
                else
                {


                    requestPDA("request pda=" + driverId + "=" + jobId + "=" + "Cancelled Pre Job>>" + jobId + "=2");


                }

                return true;

            }
            catch (Exception ex)
            {


                //  ENUtils.ShowMessage(ex.Message);
                return false;

            }




        }
        public static bool ReCallBooking(long jobId, int driverId, bool toAllRefresh = false)
        {

            try
            {

                bool rtn = true;



                General.ReCallBookingWithStatus(jobId.ToLong(), driverId.ToInt(), Enums.BOOKINGSTATUS.WAITING, Enums.Driver_WORKINGSTATUS.AVAILABLE, Enums.BOOKINGSTATUS.NOSHOW);

                //General.SendMessageToPDA("request broadcast=" + RefreshTypes.REFRESH_ACTIVEBOOKINGS_DASHBOARD + "=" + jobId + "=syncdrivers");


                return rtn;
            }
            catch (Exception ex)
            {

                return false;



            }

        }
        public static void ReCallBookingWithStatus(long jobId, int driverId, int? bookingStatusId, int? driverStatusId, int? recoverJob = null)
        {

            try
            {



                //  bool rtn = true;

                int rankChanged = 0;

                if (HubProcessor.Instance.objPolicy.EnableBookingOtherCharges.ToBool()
                    && (bookingStatusId == Enums.BOOKINGSTATUS.NOPICKUP || bookingStatusId == Enums.BOOKINGSTATUS.CANCELLED || recoverJob.ToInt() == Enums.BOOKINGSTATUS.NOSHOW))
                {

                    FreezeTopRank(jobId, driverId, bookingStatusId.ToIntorNull(), driverStatusId.ToIntorNull());
                    rankChanged = 1;
                }
                else
                {
                    (new TaxiDataContext()).stp_UpdateJob(jobId, driverId, bookingStatusId, driverStatusId, HubProcessor.Instance.objPolicy.SinBinTimer.ToInt());
                }


                requestPDA("request pda=" + driverId + "=" + jobId + "=" + "Cancelled Pre Job>>" + jobId + "=2" + rankChanged);



                if (HubProcessor.Instance.objPolicy.DespatchOfflineJobs.ToBool())
                {
                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        db.stp_SaveOfflineMessage(jobId, driverId, "", "", "Cancelled Job>>" + jobId + "=2");
                    }

                }


            }
            catch (Exception ex)
            {

                //  return false;


            }

        }
        public static void FreezeTopRank(long jobId, int driverId, int? bookingStatusId, int? driverStatusId)
        {
            try
            {
                using (TaxiDataContext db = new TaxiDataContext())
                {
                    db.ExecuteQuery<int>("exec stp_UpdateJobEnd {0},{1},{2},{3},{4},{5},{6}", jobId, driverId, bookingStatusId.ToIntorNull(), driverStatusId.ToIntorNull(), -1, "", "-1");
                }

            }
            catch
            {

            }


        }

        public static string GetPostCodeMatchOpt(string value)
        {



            string postCode = "";

            General.RemoveUK(ref value);

            if (value.ToStr().Contains(","))
            {
                value = value.Replace(",", "").Trim();
            }

            if (value.ToStr().Contains(" "))
            {
                value = value.Replace(" ", " ").Trim();
            }



            string ukAddress = @"^([A-PR-UWYZ](([0-9](([0-9]|[A-HJKSTUW])?)?)|([A-HK-Y][0-9]([0-9]|[ABEHMNPRVWXY])?)) ?[0-9][ABD-HJLNP-UW-Z]{2})$";

            //   string ukAddress = @"^(GIR 0AA)|((([A-PR-UWYZ][0-9][0-9]?)|(([A-PR-UWYZ][A-HK-Y][0-9][0-9]?)|(([A-PR-UWYZ][0-9][A-HJKSTUW])|([A-PR-UWYZ][A-HK-Y][0-9][ABEHMNPRVWXY]))))([ ]?)[0-9][A-BD-HJLNP-UW-Z]{2})$";


            Regex reg = new Regex(ukAddress);
            Match em = reg.Match(value);

            if (em != null)
                postCode = em.Value;

            if (em.Value == "")
            {

                string halfPostcode = string.Empty;

                ukAddress = @"[A-Z]{1,2}[0-9R][0-9A-Z]?";
                reg = new Regex(ukAddress);
                MatchCollection mat = reg.Matches(value);

                foreach (Match item in mat)
                {
                    if (item.Value.ToStr().IsAlpha() == false)
                        halfPostcode += item.Value.ToStr();

                }

                if (value.WordCount() == 1)
                {
                    //if(value.EndsWith(" "))
                    //{
                    //    postCode = halfPostcode + " ";

                    //}
                    //else
                    postCode = halfPostcode;

                }
                else if (halfPostcode.Length > 0 && value.WordCount() == 2)
                {
                    if (value.Trim().Length <= 8 && value.Trim().Contains(" ")
                        && value.Trim().Split(new char[] { ' ' })[1].IsAlpha() == false)
                    {

                        if (value.StartsWith(halfPostcode))
                            postCode = value.Trim();
                        else if (value.EndsWith(halfPostcode))
                            postCode = halfPostcode;
                    }
                    else if (halfPostcode.IsAlpha() == false)
                        postCode = halfPostcode;

                }

            }



            return postCode.Trim();

        }



        public static bool CancelCurrentBookingFromPDA(long jobId, int driverId)
        {
            try
            {
                bool rtn = true;

                using (TaxiDataContext db = new TaxiDataContext())
                {
                    db.stp_UpdateJob(jobId, driverId, Enums.BOOKINGSTATUS.CANCELLED, Enums.Driver_WORKINGSTATUS.AVAILABLE, HubProcessor.Instance.objPolicy.SinBinTimer.ToInt());

                }
                CancelledJobFromController(driverId, jobId);

                if (HubProcessor.Instance.objPolicy.DespatchOfflineJobs.ToBool())
                {
                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        db.stp_SaveOfflineMessage(jobId, driverId, "", "Customer", "Cancelled Job>>" + jobId + "=2");
                    }
                }

                return rtn;
            }
            catch (Exception ex)
            {
                return false;
                //ENUtils.ShowMessage(ex.Message);
            }
        }

        public static void CancelledJobFromController(int driverId, long jobId)
        {

            HubProcessor.Instance.listofJobs.Add(new clsPDA
            {
                DriverId = driverId,
                JobId = jobId,
                MessageDateTime = DateTime.Now.AddSeconds(-30),
                JobMessage = "Cancelled Job>>" + jobId,
                MessageTypeId = 2
            });

            SocketIO.SendToSocket(driverId.ToStr(), "Cancelled Job>>" + jobId.ToStr(), "forceRecoverJob");
        }
        public static string AddZone(Classes.Gen_Zones request)
        {

            string Msg = "";

            try
            {
                foreach (var a in request.Gen_Zone_PolyVertices)
                {
                    a.Latitude1 = a.Latitude.ToString();
                    a.Longitude1 = a.Longitude.ToString();
                    a.Diameter1 = a.Diameter.ToString();
                }
                var JsonPolyVertices = Newtonsoft.Json.JsonConvert.SerializeObject(request.Gen_Zone_PolyVertices);
                using (TaxiDataContext db = new TaxiDataContext())
                {
                    int? ID = request.Id == null ? 0 : request.Id;
                    db.ExecuteQuery<string>("exec SP_InsertZones {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}", request.ZoneName, request.AddBy, request.ShapeCategory
                        , request.ShapeType, request.Linewidth, request.Lineforecolor, request.MinLatitude, request.MaxLatitude, request.MinLongitude, request.MaxLongitude, request.ShortName,
                        request.PlotKind, JsonPolyVertices, ID);
                }
                return Msg;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public List<WebApiClasses.Gen_Zones> GetZonesList()
        {

            List<WebApiClasses.Gen_Zones> lst = new List<WebApiClasses.Gen_Zones>();
            WebApiClasses.ZoneMasterDetailList obj = new WebApiClasses.ZoneMasterDetailList();
            List<WebApiClasses.ZonesMaster> zonesMaster = new List<WebApiClasses.ZonesMaster>();
            List<WebApiClasses.Gen_Zone_PolyVertices> gen_Zone_PolyVertices = new List<WebApiClasses.Gen_Zone_PolyVertices>();
            using (TaxiDataContext db = new TaxiDataContext())
            {
                zonesMaster = db.ExecuteQuery<WebApiClasses.ZonesMaster>(@"SELECT [Id],[ZoneName],[AddOn],[AddBy],[EditOn],[EditBy],[PostCode],[OrderNo],[ShapeCategory],[ShapeType],[Linewidth],[Lineforecolor],[MinLatitude],[MaxLatitude]    
                        ,[MinLongitude],[MaxLongitude],[ShortName],[EnableAutoDespatch],[EnableBidding],[BiddingRadius],[ZoneTypeId],[PlotKind],[DisableDriverRank],[JobDueTime]
                        FROM[Gen_Zones]").ToList();

                gen_Zone_PolyVertices = db.ExecuteQuery<WebApiClasses.Gen_Zone_PolyVertices>(@"SELECT[Id] ,[PostCode],[Latitude],[Longitude],[ZoneId],[Diameter] FROM[Gen_Zone_PolyVertices]").ToList();

            }




            obj.ZoneMaster = zonesMaster;
            obj.ZoneDetail = gen_Zone_PolyVertices;

            foreach (var item in obj.ZoneMaster)
            {
                WebApiClasses.Gen_Zones data = new WebApiClasses.Gen_Zones();

                data.Id = item.Id;
                data.ZoneName = item.ZoneName;
                data.AddOn = item.AddOn;
                data.AddBy = item.AddBy;
                data.EditOn = item.EditOn;
                data.EditBy = item.EditBy;
                data.PostCode = item.PostCode;
                data.OrderNo = item.OrderNo;
                data.ShapeCategory = item.ShapeCategory;
                data.ShapeType = item.ShapeType;
                data.Linewidth = item.Linewidth;
                data.Lineforecolor = item.Lineforecolor;
                data.MinLatitude = item.MinLatitude;
                data.MaxLatitude = item.MaxLatitude;
                data.MinLongitude = item.MinLongitude;
                data.MaxLongitude = item.MaxLongitude;
                data.ShortName = item.ShortName;
                data.EnableAutoDespatch = item.EnableAutoDespatch;
                data.EnableBidding = item.EnableBidding;
                data.BiddingRadius = item.BiddingRadius;
                data.ZoneTypeId = item.ZoneTypeId;
                data.PlotKind = item.PlotKind;
                data.DisableDriverRank = item.DisableDriverRank;
                data.JobDueTime = item.JobDueTime;

                data.Gen_Zone_PolyVertices = obj.ZoneDetail.Where(a => a.ZoneId == data.Id).ToList();
                lst.Add(data);
            }

            return lst;
        }




        public static int? GetZoneId(string address)
        {
            //if (AppVars.objPolicyConfiguration.EnablePDA.ToBool() == false)
            //    return null;

            //if (address != "AS DIRECTED" && string.IsNullOrEmpty(General.GetPostCodeMatch(address)))
            //    return null;

            if (address.Contains(", UK"))
                address = address.Remove(address.LastIndexOf(", UK"));

            int? zoneId = 0;

            try
            {
                if (address == "AS DIRECTED")
                {
                    zoneId = General.GetObject<Gen_Zone>(c => c.ZoneName == address).DefaultIfEmpty().Id;
                }
                else
                {
                    // if (AppVars.listOfAddress.Count(c=>c.AddressLine1.Contains(address.ToStr().ToUpper()))

                    //if (Instance.objPolicy.PriorityPostCodes.ToStr().Trim().Length > 0)
                    //    zoneId = AppVars.listOfAddress.FirstOrDefault(c => c.AddressLine1.Contains(address.ToStr().ToUpper())).DefaultIfEmpty().ZoneId;

                    if (zoneId == 0)
                    {
                        string postCode = GetPostCodeMatch(address);

                        if (address.Contains(",") && HubProcessor.Instance.objPolicy.PriorityPostCodes.ToStr().Trim().Length > 0)
                        {
                            string addr = address.Substring(0, address.LastIndexOf(',')).Trim();

                            if (addr.ToStr().Trim() != string.Empty)
                            {
                                zoneId = General.GetObject<Gen_Location>(c => c.PostCode == postCode && c.LocationName == addr).DefaultIfEmpty().ZoneId.ToInt();
                            }
                        }

                        if (zoneId == 0)
                        {
                            //string postCode = General.GetPostCode(address);
                            Gen_Coordinate objCoord = General.GetObject<Gen_Coordinate>(c => c.PostCode == postCode);
                            if (objCoord != null)
                            {
                                double latitude = 0, longitude = 0;

                                latitude = Convert.ToDouble(objCoord.Latitude);
                                longitude = Convert.ToDouble(objCoord.Longitude);

                                int[] plot = null;


                                plot = (from a in General.GetQueryable<Gen_Zone>(c => (c.ShapeType != null && c.ShapeType == "circle") || (c.MinLatitude != null && (latitude >= c.MinLatitude && latitude <= c.MaxLatitude)
                                                                  && (longitude <= c.MaxLongitude && longitude >= c.MinLongitude)))
                                        orderby a.PlotKind

                                        select a.Id).ToArray<int>();




                                if (plot.Count() > 0)
                                {
                                    var list = (from p in plot
                                                join a in General.GetQueryable<Gen_Zone_PolyVertice>(null) on p equals a.ZoneId
                                                select a).ToList();

                                    foreach (int plotId in plot)
                                    {
                                        if (FindPoint(latitude, longitude, list.Where(c => c.ZoneId == plotId).ToList()))
                                        {
                                            zoneId = plotId;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    if (HubProcessor.Instance.objPolicy.PriorityPostCodes.ToStr().Length > 0)
                                    {
                                        double distPick = Convert.ToDouble(HubProcessor.Instance.objPolicy.CreditCardExtraCharges.ToDecimal());

                                        if (distPick > 0)
                                        {
                                            string[] arr = HubProcessor.Instance.objPolicy.PriorityPostCodes.Split(new char[] { ',' });

                                            if (objCoord.PostCode.ToStr().Contains(" ") && arr.Contains(objCoord.PostCode.Split(new char[] { ' ' })[0]))
                                            {
                                                var zone = (from a in General.GetQueryable<Gen_Zone_PolyVertice>(null).AsEnumerable()
                                                            select new
                                                            {
                                                                a.Gen_Zone.Id,
                                                                a.Gen_Zone.ZoneName,
                                                                DistanceMin = new LatLng(Convert.ToDouble(a.Latitude), Convert.ToDouble(a.Longitude)).DistanceMiles(new LatLng(Convert.ToDouble(objCoord.Latitude), Convert.ToDouble(objCoord.Longitude))),
                                                            }).OrderBy(c => c.DistanceMin).Where(c => c.DistanceMin <= distPick).FirstOrDefault();

                                                if (zone != null)
                                                    zoneId = zone.Id;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }


            if (zoneId == 0)
                zoneId = null;

            return zoneId;
        }

        public static bool FindPoint(double pointLat, double pointLng, List<Gen_Zone_PolyVertice> PontosPolig)
        {
            //                             X               y               
            int sides = PontosPolig.Count();
            int j = sides - 1;
            bool pointStatus = false;

            if (sides == 1)
            {
                double radius = Convert.ToDouble(PontosPolig[0].Diameter) / 2;
                double lat = Convert.ToDouble(PontosPolig[0].Latitude);
                double lng = Convert.ToDouble(PontosPolig[0].Longitude);

                double dist = new DotNetCoords.LatLng(Convert.ToDouble(lat), Convert.ToDouble(lng)).DistanceMiles(new LatLng(pointLat, pointLng));

                if (dist <= radius)
                    pointStatus = true;
            }
            else
            {

                for (int i = 0; i < sides; i++)
                {
                    if (PontosPolig[i].Longitude < pointLng && PontosPolig[j].Longitude >= pointLng ||
                        PontosPolig[j].Longitude < pointLng && PontosPolig[i].Longitude >= pointLng)
                    {
                        if (PontosPolig[i].Latitude + (pointLng - PontosPolig[i].Longitude) /
                            (PontosPolig[j].Longitude - PontosPolig[i].Longitude) * (PontosPolig[j].Latitude - PontosPolig[i].Latitude) < pointLat)
                        {
                            pointStatus = !pointStatus;
                        }
                    }
                    j = i;
                }
            }

            return pointStatus;
        }

        public static double SqrRoot(double t)
        {
            double lb = 0, ub = t, temp = 0;
            int count = 50;

            while (count != 0)
            {
                temp = (lb + ub) / 2;

                if (temp * temp == t)
                {
                    return temp;
                }
                else if (temp * temp > t)
                {
                    ub = temp;
                }
                else
                {
                    lb = temp;
                }

                count--;
            }

            return temp;
        }



        public static string physicalPath = AppContext.BaseDirectory;

        public static void CallGetDashboardData()
        {
            SocketIO.SendToSocket("", "GetDashboardData", "GetDashboardData", "WebApp");


        }


        public static void CallGetDashboardDriversData()
        {
            SocketIO.SendToSocket("", "GetDashboardDriversData", "GetDashboardDriversData", "WebApp");


        }


        public static void MessageToPDA(string message)
        {
            try
            {


                if (message.StartsWith("request broadcast="))
                {

                    try
                    {

                        File.AppendAllText(AppContext.BaseDirectory + "\\requestbroadcastAPI.txt", DateTime.Now.ToStr() + ": Message :" + message + " : cnt" + message.Split('=').Count() + Environment.NewLine);
                    }
                    catch (Exception ex)
                    {

                    }



                    string data = message.Split('=')[1];




                    if (message.Split('=').Count() > 2)
                    {





                        if (data == "refresh required dashboard")
                        {
                            data = "jsonrefresh required dashboard";

                            DateTime? dt = DateTime.Now.ToDateorNull();
                            DateTime recentDays = dt.Value.AddDays(-1);
                            DateTime dtNow = DateTime.Now;
                            DateTime prebookingdays = dt.Value.AddDays(HubProcessor.Instance.objPolicy.HourControllerReport.ToInt()).ToDate();



                            using (TaxiDataContext db = new TaxiDataContext())
                            {
                                //

                                List<stp_GetBookingsDataResult> query = db.ExecuteQuery<stp_GetBookingsDataResult>("exec stp_getbookingsdata {0},{1},{2},{3}", recentDays, prebookingdays, 0, HubProcessor.Instance.objPolicy.DaysInTodayBooking.ToInt()).ToList();

                                data += "|>>>|" + Newtonsoft.Json.JsonConvert.SerializeObject(query);
                            }




                        }
                        else if (data == "refresh active booking dashboard")
                        {
                            data = "jsonrefresh active booking dashboard";
                            DateTime? dt = DateTime.Now.ToDateorNull();
                            DateTime recentDays = dt.Value.AddDays(-1);

                            int BookingHours = HubProcessor.Instance.objPolicy.DaysInTodayBooking.ToInt();

                            DateTime tillDate = dt.Value.AddHours(BookingHours).Date;

                            if (BookingHours > 0)
                                tillDate = DateTime.Now.ToDateTime().AddHours(BookingHours);


                            using (TaxiDataContext db = new TaxiDataContext())
                            {
                                //
                                List<stp_GetBookingsDataResult> query = db.ExecuteQuery<stp_GetBookingsDataResult>("exec stp_getbookingsdata {0},{1},{2},{3}", recentDays, tillDate, 0, BookingHours).ToList();

                                //   var query = db.stp_GetBookingsData(recentDays, dt.Value.AddHours(BookingHours).Date, 0, BookingHours).ToList();
                                data += "|>>>|" + Newtonsoft.Json.JsonConvert.SerializeObject(query);
                            }
                            //

                        }
                    }





                    BroadCastMessage(data);
                    General.CallGetDashboardData();

                    if (message.ToStr().Trim().Contains("=syncdrivers"))
                    {



                        BroadCastMessage(RefreshTypes.REFRESH_DASHBOARD_DRIVER);
                        General.CallGetDashboardDriversData();
                    }
                }
                else if (message.StartsWith("**"))
                {
                    //Clients.Caller.cMessageToDesktop("exceptionOccured" + ex.Message);



                    // List<string> listOfConnections = new List<string>();
                    //  listOfConnections = HubProcessor.Instance.ReturnDesktopConnections();


                    //    HubProcessor.Instance.Clients.Clients(listOfConnections).cMessageToDesktop(message.Trim());
                    General.BroadCastMessage(message.Trim());
                    if (message.ToStr().Contains("**autodespatchmode"))
                    {
                        HubProcessor.Instance.objPolicy = General.GetObject<Gen_SysPolicy_Configuration>(c => c.SysPolicyId == 1);

                    }
                }



                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\MessageToPDA.txt", DateTime.Now.ToStr() + ": Message :" + message + " :  Total Connections = " + HubProcessor.Instance.ReturnDesktopConnections().Count + Environment.NewLine);
                }
                catch (Exception ex)
                {



                }

            }
            catch
            {
                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\MessageToPDA_catch.txt", DateTime.Now.ToStr() + ": Message :" + message + " :  Total Connections = " + HubProcessor.Instance.ReturnDesktopConnections().Count + Environment.NewLine);
                }
                catch (Exception ex)
                {



                }

            }

        }


        public static void MessageToPDAByDriverId(string message, string driverID)
        {
            try
            {

                //try
                //{
                //    File.AppendAllText(physicalPath + "\\MessageToPDAByDriverId.txt", DateTime.Now.ToStr() + " : " + message + Environment.NewLine);


                //}
                //catch
                //{

                //}


                string[] values = message.Split(new char[] { '=' });

                //if (message.StartsWith("update settings<<<"))
                //    updateDriverSettings(message, driverID);
                if (message.Contains("logout auth status>>"))
                {

                    //
                    HubProcessor.Instance.listofJobs.Add(new clsPDA
                    {
                        DriverId = values[1].ToInt(),
                        JobId = values[2].ToLong(),
                        MessageDateTime = DateTime.Now.AddSeconds(-30),
                        JobMessage = values[3].Split(new String[] { ">>" }, StringSplitOptions.RemoveEmptyEntries)[1].ToStr().Trim(),
                        MessageTypeId = values[4].ToInt()
                    });

                }
                else if (message.Contains("auth status>>yes>>") || message.Contains("auth status>>no>>"))
                {
                    authStatus(message, driverID);


                    HubProcessor.Instance.listofJobs.Add(new clsPDA
                    {
                        DriverId = values[1].ToInt(),
                        JobId = values[2].ToLong(),
                        MessageDateTime = DateTime.Now.AddSeconds(-30),
                        JobMessage = values[3].Split(new String[] { ">>" }, StringSplitOptions.RemoveEmptyEntries)[1].ToStr().Trim(),
                        MessageTypeId = values[4].ToInt()
                    });

                    SocketIO.SendToSocket(values[1].ToStr(), values[3].Split(new String[] { ">>" }, StringSplitOptions.RemoveEmptyEntries)[1].ToStr().Trim(), "authStatus");
                    try
                    {
                        File.AppendAllText(physicalPath + "\\MessageToPDAByDriverId.txt", DateTime.Now.ToStr() + " : DriverID:" + values[1].ToStr() + ",Message:" + values[3].Split(new String[] { ">>" }, StringSplitOptions.RemoveEmptyEntries)[1].ToStr().Trim() + Environment.NewLine);


                    }
                    catch
                    {

                    }

                }




            }
            catch
            {

            }

        }

        public static void authStatus(string mesg, string driverID)
        {
            try
            {

                string dataValue = mesg.Replace("auth status>>", "");
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });



                //send acknowledgement message to desktop


                //try
                //{
                //    if (Instance.objPolicy.EnableBookingOtherCharges.ToBool() && values.Count() > 5 && mesg.EndsWith("=13") && values[5].ToStr() == "13")
                //    {
                //        int driverId = Convert.ToInt32(driverID);

                //        BroadCastPostionChanged(driverId);
                //    }
                //}
                //catch
                //{

                //}


                try
                {
                    if (values.Count() > 5 && values[5] == "13")
                    {


                        CallSupplierApi.UpdateStatus(values[2].ToLong(), 2);
                    }
                    else
                    {
                        CallSupplierApi.UpdateStatus(values[2].ToLong(), Enums.BOOKINGSTATUS.NOSHOW.ToInt());
                    }
                }
                catch
                {

                }
            }
            catch (Exception ex)
            {

            }
        }


        public static void requestPDA(string mesg)
        {
            try
            {

                HubProcessor Instance = HubProcessor.Instance;

                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });
                int ddId = 0;

                if (values.Count() > 3)
                {
                    if (values[4].ToInt() == eMessageTypes.MESSAGING)
                    {
                        if (values[1].ToStr().Contains(","))
                        {
                            DateTime dt = DateTime.Now.AddSeconds(-45);
                            foreach (string dId in values[1].Split(','))
                            {
                                Instance.listofJobs.Add(new clsPDA
                                {
                                    DriverId = dId.ToInt(),
                                    JobId = values[2].ToLong(),
                                    MessageDateTime = dt,
                                    JobMessage = values[3].ToStr().Trim(),
                                    MessageTypeId = values[4].ToInt(),

                                });

                                SocketIO.SendToSocket(dId.ToStr(), values[3].ToStr(), "sendMessage");
                            }
                        }
                        else
                        {
                            try
                            {

                                string recordId = string.Empty;
                                recordId = Guid.NewGuid().ToString();

                                Instance.listofJobs.Add(new clsPDA
                                {
                                    DriverId = values[1].ToInt(),
                                    JobId = values[2].ToLong(),
                                    MessageDateTime = DateTime.Now.AddSeconds(-45),
                                    JobMessage = values[3].ToStr().Trim(),
                                    MessageTypeId = values[4].ToInt(),
                                    //Id = recordId
                                });


                                try
                                {
                                    SocketIO.SendToSocket(values[1].ToStr(), values[3].ToStr(), "sendMessage", "", recordId);

                                    File.AppendAllText(AppContext.BaseDirectory + "\\sendmessagetodriver.txt", DateTime.Now.ToStr() + "driverid" + values[1].ToInt() + ":message:" + values[3].ToStr().Trim() + Environment.NewLine);
                                }
                                catch (Exception ex)
                                {



                                }

                                ddId = values[1].ToInt();

                            }
                            catch (Exception ex)
                            {
                                try
                                {
                                    // Thread.Sleep(100);

                                    Instance.listofJobs.Add(new clsPDA
                                    {
                                        DriverId = values[1].ToInt(),
                                        JobId = values[2].ToLong(),
                                        MessageDateTime = DateTime.Now.AddSeconds(-45),
                                        JobMessage = values[3].ToStr().Trim(),
                                        MessageTypeId = values[4].ToInt()
                                    });

                                    File.AppendAllText(physicalPath + "\\exception_servertoclient.txt", DateTime.Now.ToStr() + " FIXED: " + ex.Message + Environment.NewLine);
                                }
                                catch (Exception ex2)
                                {
                                    File.AppendAllText(physicalPath + "\\exception_servertoclient.txt", DateTime.Now.ToStr() + " NOTFIXED: " + ex2.Message + Environment.NewLine);
                                }
                            }
                        }

                        //if (ddId > 0)
                        //{

                        //    clsPDA objc = Instance.listofJobs.LastOrDefault(c => c.DriverId == ddId);

                        //    if (objc != null)
                        //    {
                        //        List<string> listOfConnections = new List<string>();
                        //        listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(objc.DriverId));
                        //        Clients.Clients(listOfConnections).sendMessage(objc.JobMessage.ToStr());


                        //        Instance.listofJobs.Remove(objc);


                        //    }
                        //}
                        //else
                        // {
                        //     List<string> listOfConnections = new List<string>();
                        //     listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
                        //     Clients.Clients(listOfConnections).sendMessage(Instance.listofJobs[0].JobMessage.ToStr());


                        // }

                    }
                    else if (values[4].ToInt() == eMessageTypes.JOB)
                    {
                        int drvId = values[2].ToInt();
                        try
                        {
                            if (Instance.listofJobs.Count(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong() && c.MessageTypeId == eMessageTypes.RECALLJOB) > 0)
                                Instance.listofJobs.RemoveAll(c => c.JobId != 0 && c.JobId != values[1].ToLong() && c.MessageTypeId == eMessageTypes.RECALLJOB);

                            if (Instance.listofJobs.Count(c => c.JobId == values[1].ToLong() && c.DriverId == values[2].ToInt()) > 0)
                                Instance.listofJobs.RemoveAll(c => c.JobId == values[1].ToLong() && c.DriverId == values[2].ToInt());





                            HubProcessor.Instance.listofJobs.Add(new clsPDA
                            {
                                JobId = values[1].ToLong(),
                                DriverId = values[2].ToInt(),
                                MessageDateTime = DateTime.Now,
                                JobMessage = values[3].ToStr().Trim(),
                                MessageTypeId = values[4].ToInt(),
                                DriverNo = values[5].ToStr()
                            });


                            try
                            {

                                File.AppendAllText(physicalPath + "\\LISTCOUNT.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",despatch job count:" + HubProcessor.Instance.listofJobs.Count + Environment.NewLine);
                            }
                            catch
                            {


                            }


                            SocketIO.SendToSocket(values[2].ToStr(), values[3].ToStr(), "despatchBooking");
                        }
                        catch (Exception ex)
                        {
                            try
                            {


                                if (Instance.listofJobs.Count(c => c.JobId == values[1].ToLong() && c.DriverId == values[2].ToInt()) > 0)
                                    Instance.listofJobs.RemoveAll(c => c.JobId == values[1].ToLong() && c.DriverId == values[2].ToInt());

                                Instance.listofJobs.Add(new clsPDA
                                {
                                    JobId = values[1].ToLong(),
                                    DriverId = values[2].ToInt(),
                                    MessageDateTime = DateTime.Now,
                                    JobMessage = values[3].ToStr().Trim(),
                                    MessageTypeId = values[4].ToInt(),
                                    DriverNo = values[5].ToStr()
                                });

                                File.AppendAllText(physicalPath + "\\exception_servertoclient.txt", DateTime.Now.ToStr() + " FIXED: " + ex.Message + Environment.NewLine);
                            }
                            catch (Exception ex2)
                            {
                                try
                                {
                                    File.AppendAllText(physicalPath + "\\exception_servertoclient.txt", DateTime.Now.ToStr() + " NOTFIXED: " + ex2.Message + Environment.NewLine);
                                }
                                catch
                                {

                                }
                            }
                        }


                    }
                    else if (values[4].ToInt() == eMessageTypes.CLEAREDJOB)
                    {
                        try
                        {
                            Instance.listofJobs.Add(new clsPDA
                            {
                                DriverId = values[1].ToInt(),
                                JobId = values[2].ToLong(),
                                MessageDateTime = DateTime.Now.AddSeconds(-30),
                                JobMessage = values[3].ToStr().Trim(),
                                MessageTypeId = values[4].ToInt()
                            });

                        }
                        catch (Exception ex)
                        {
                            try
                            {


                                Instance.listofJobs.Add(new clsPDA
                                {
                                    DriverId = values[1].ToInt(),
                                    JobId = values[2].ToLong(),
                                    MessageDateTime = DateTime.Now.AddSeconds(-30),
                                    JobMessage = values[3].ToStr().Trim(),
                                    MessageTypeId = values[4].ToInt()
                                });

                                File.AppendAllText(physicalPath + "\\exception_servertoclient.txt", DateTime.Now.ToStr() + "force clear job FIXED: " + ex.Message + Environment.NewLine);
                            }
                            catch (Exception ex2)
                            {
                                File.AppendAllText(physicalPath + "\\exception_servertoclient.txt", DateTime.Now.ToStr() + "force clear job NOTFIXED: " + ex2.Message + Environment.NewLine);
                            }
                        }

                        SocketIO.SendToSocket(values[1].ToStr(), values[3].ToStr(), "forceClearJob");
                        //List<string> listOfConnections = new List<string>();
                        //listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
                        //Clients.Clients(listOfConnections).forceClearJob(Instance.listofJobs[0].JobMessage.ToStr());
                    }
                    else if (values[4].ToInt() == eMessageTypes.RECALLJOB)
                    {

                        try
                        {

                            Instance.listofJobs.Add(new clsPDA
                            {
                                DriverId = values[1].ToInt(),
                                JobId = values[2].ToLong(),
                                MessageDateTime = DateTime.Now.AddSeconds(-30),
                                JobMessage = values[3].ToStr().Trim(),
                                MessageTypeId = values[4].ToInt()
                            });

                        }
                        catch (Exception ex)
                        {
                            try
                            {


                                Instance.listofJobs.Add(new clsPDA
                                {
                                    DriverId = values[1].ToInt(),
                                    JobId = values[2].ToLong(),
                                    MessageDateTime = DateTime.Now.AddSeconds(-30),
                                    JobMessage = values[3].ToStr().Trim(),
                                    MessageTypeId = values[4].ToInt()
                                });


                                File.AppendAllText(physicalPath + "\\exception_servertoclient.txt", DateTime.Now.ToStr() + "recover job FIXED: " + ex.Message + Environment.NewLine);
                            }
                            catch (Exception ex2)
                            {
                                File.AppendAllText(physicalPath + "\\exception_servertoclient.txt", DateTime.Now.ToStr() + "recover job NOTFIXED: " + ex2.Message + Environment.NewLine);
                            }
                        }


                        try
                        {
                            CallSupplierApi.UpdateStatus(values[2].ToLong(), Enums.BOOKINGSTATUS.NOSHOW.ToInt());
                        }
                        catch
                        { }


                        SocketIO.SendToSocket(values[1].ToStr(), values[3].ToStr(), "forceRecoverJob");
                        //List<string> listOfConnections = new List<string>();
                        //listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
                        //Clients.Clients(listOfConnections).forceRecoverJob(Instance.listofJobs[0].JobMessage.ToStr());
                    }
                    else if (values[4].ToInt() == eMessageTypes.UPDATEJOB)
                    {
                        int drvId = values[1].ToInt();

                        Instance.listofJobs.Add(new clsPDA
                        {
                            DriverId = values[1].ToInt(),
                            JobId = values[2].ToLong(),
                            MessageDateTime = DateTime.Now.AddSeconds(-15),
                            JobMessage = values[3].ToStr(),
                            MessageTypeId = values[4].ToInt()
                        });


                        SocketIO.SendToSocket(values[1].ToStr(), values[3].ToStr(), "updateJob");

                        // List<string> listOfConnections = new List<string>();
                        // listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs.LastOrDefault(c => c.DriverId == drvId).DriverId));
                        //  Clients.Clients(listOfConnections).updateJob(Instance.listofJobs.LastOrDefault(c => c.DriverId == drvId).JobMessage.ToStr());
                    }
                    else if (values[4].ToInt() == eMessageTypes.AUTHORIZATION)
                    {
                        Instance.listofJobs.Add(new clsPDA
                        {
                            DriverId = values[1].ToInt(),
                            JobId = values[2].ToLong(),
                            MessageDateTime = DateTime.Now.AddSeconds(-30),
                            JobMessage = values[3].ToStr().Replace(">>", "=").Trim(),
                            MessageTypeId = values[4].ToInt()
                        });





                        SocketIO.SendToSocket(values[1].ToStr(), values[3].ToStr(), "authStatus");
                        // List<string> listOfConnections = new List<string>();
                        //listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
                        //  Clients.Clients(listOfConnections).sendMessage(Instance.listofJobs[0].JobMessage.ToStr());
                    }
                    else if (values[4].ToInt() == eMessageTypes.BIDALERT)
                    {
                        Instance.listofJobs.Add(new clsPDA
                        {
                            DriverId = values[1].ToInt(),
                            JobId = values[2].ToLong(),
                            MessageDateTime = DateTime.Now.AddSeconds(-15),
                            JobMessage = values[3].ToStr(),
                            MessageTypeId = values[4].ToInt()
                        });

                        // List<string> listOfConnections = new List<string>();
                        //listOfConnections = Instance.ReturnDriverConnections(values[1].ToInt());
                        // Clients.Clients(listOfConnections).bidAlert(values[3].ToStr());
                        //
                        SocketIO.SendToSocket(values[1].ToStr(), values[3].ToStr(), "bidAlert");
                        try
                        {
                            File.AppendAllText(physicalPath + "\\bidalert.txt", DateTime.Now.ToStr() + Environment.NewLine);
                        }
                        catch
                        {

                        }
                    }
                    else if (values[4].ToInt() == eMessageTypes.BIDPRICEALERT)
                    {
                        string[] driverIds = values[1].ToStr().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                        Instance.listofJobs.AddRange((from a in driverIds
                                                      select new clsPDA
                                                      {
                                                          DriverId = a.ToInt(),
                                                          JobId = values[2].ToLong(),
                                                          MessageDateTime = DateTime.Now.AddSeconds(-30),
                                                          JobMessage = values[3].ToStr(),
                                                          MessageTypeId = values[4].ToInt()

                                                      }));

                        // List<string> listOfConnections = new List<string>();
                        //  listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
                        //  Clients.Clients(listOfConnections).sendMessage(Instance.listofJobs[0].JobMessage.ToStr());
                    }
                    else if (values[4].ToInt() == eMessageTypes.UPDATEPLOT)
                    {
                        Instance.listofJobs.Add(new clsPDA
                        {
                            DriverId = values[1].ToInt(),
                            JobId = values[2].ToLong(),
                            MessageDateTime = DateTime.Now.AddSeconds(-15),
                            JobMessage = values[3].ToStr(),
                            MessageTypeId = values[4].ToInt()
                        });

                        SocketIO.SendToSocket(values[1].ToStr(), values[3].ToStr(), "sendMessage");
                        // List<string> listOfConnections = new List<string>();
                        //   listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
                        //   Clients.Clients(listOfConnections).sendMessage(Instance.listofJobs[0].JobMessage.ToStr());
                    }
                    else if (values[4].ToInt() == eMessageTypes.LOGOUTAUTHORIZATION)
                    {
                        Instance.listofJobs.Add(new clsPDA
                        {
                            DriverId = values[1].ToInt(),
                            JobId = 0,
                            MessageDateTime = DateTime.Now.AddSeconds(-35),
                            JobMessage = values[3].ToStr(),
                            MessageTypeId = values[4].ToInt()
                        });

                        //
                        try
                        {
                            if (values[3].ToStr().ToLower().Contains("yes"))
                            {

                                using (TaxiDataContext db = new TaxiDataContext())
                                {
                                    db.stp_LoginLogoutDriver(values[1].ToInt(), false, null);
                                }

                                BroadCastMessage("**logout>>Driver " + values[2] + " is Logout");
                            }
                        }
                        catch
                        {


                        }
                        //callsocket

                        //   General.CallGetDashboardDriversData();


                        //callsocket


                        SocketIO.SendToSocket(values[1].ToStr(), values[3].ToStr(), "logoutAuthStatus");

                        //  List<string> listOfConnections = new List<string>();
                        //  listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
                        //  Clients.Clients(listOfConnections).sendMessage(Instance.listofJobs[0].JobMessage.ToStr());
                    }
                    else if (values[4].ToInt() == eMessageTypes.FORCE_ACTION_BUTTON)
                    {
                        Instance.listofJobs.Add(new clsPDA
                        {
                            DriverId = values[1].ToInt(),
                            JobId = values[2].ToLong(),
                            MessageDateTime = DateTime.Now.AddSeconds(-45),
                            JobMessage = values[3].ToStr(),
                            MessageTypeId = values[4].ToInt()
                        });


                        if (values[3].ToStr().ToStr().Contains("<<Arrive Job>>"))
                        {
                            SocketIO.SendToSocket(values[1].ToStr(), values[3].ToStr(), "forceArriveJob");

                        }
                        else if (values[3].ToStr().ToStr().Contains("<<POB Job>>"))
                        {
                            SocketIO.SendToSocket(values[1].ToStr(), values[3].ToStr(), "forcePobJob");

                        }


                        //List<string> listOfConnections = new List<string>();
                        // listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
                        //   Clients.Clients(listOfConnections).sendMessage(Instance.listofJobs[0].JobMessage.ToStr());
                    }
                    else if (values[4].ToInt() == eMessageTypes.UPDATE_SETTINGS)
                    {
                        Instance.listofJobs.Add(new clsPDA
                        {
                            DriverId = values[2].ToInt(),
                            JobId = 0,
                            MessageDateTime = DateTime.Now.AddSeconds(-50),
                            JobMessage = values[3].ToStr().Trim(),
                            MessageTypeId = values[4].ToInt()
                        });

                        SocketIO.SendToSocket(values[1].ToStr(), values[3].ToStr(), "updateSetting");
                        //List<string> listOfConnections = new List<string>();
                        //  listOfConnections = Instance.ReturnDriverConnections(Convert.ToInt32(Instance.listofJobs[0].DriverId));
                        // Clients.Clients(listOfConnections).sendMessage(Instance.listofJobs[0].JobMessage.ToStr());
                    }

                    else if (mesg.Contains("auth status>>yes>>") || mesg.Contains("auth status>>no>>"))
                    {
                        //  authStatus(mesg, "0");


                        Instance.listofJobs.Add(new clsPDA
                        {
                            DriverId = values[1].ToInt(),
                            JobId = values[2].ToLong(),
                            MessageDateTime = DateTime.Now.AddSeconds(-30),
                            JobMessage = values[3].Split(new String[] { ">>" }, StringSplitOptions.RemoveEmptyEntries)[1].ToStr().Trim(),
                            MessageTypeId = values[4].ToInt()
                        });


                    }


                }
                else
                {
                    if (mesg.StartsWith("request force logout="))
                    {




                        try
                        {

                            string recordiD = Guid.NewGuid().ToString();

                            Instance.listofJobs.Add(new clsPDA
                            {
                                JobId = 0,
                                DriverId = values[2].ToInt(),
                                MessageDateTime = DateTime.Now.AddSeconds(-40),
                                JobMessage = "force logout",
                                MessageTypeId = eMessageTypes.MESSAGING,
                                DriverNo = values[1].ToStr(),
                                //           Id = recordiD
                            });

                            SocketIO.SendToSocket(values[2].ToStr(), "force logout", "forceLogout", "", recordiD);

                            //   Clients.Caller.forceLogout(objcls.JobMessage.ToStr());
                        }
                        catch (Exception ex)
                        {





                        }
                    }
                }
                //send acknowledgement message to desktop
                //Clients.Caller.cMessageToDesktop("ok");
            }
            catch (Exception ex)
            {
                // File.AppendAllText(physicalPath + "\\exception_servertoclient.txt", DateTime.Now.ToStr() + " FIXED: " + ex.Message + Environment.NewLine);
                // Clients.Caller.cMessageToDesktop("exceptionOccured" + ex.Message);
            }
        }


        public static void BroadCastMessage(string message)
        {
            try
            {
                //
                //send message to all desktop users
                List<string> listOfConnections = new List<string>();
                listOfConnections = HubProcessor.Instance.ReturnDesktopConnections();
                HubProcessor.Instance.Clients.Clients(listOfConnections).cMessageToDesktop(message);


                SocketIO.SendToSocket("", message, "ReceiveData", "WebApp");

            }
            catch
            {

            }
        }

        #endregion


        public static void UpdateJobToDriverPDA(long jobId)
        {
            try
            {


                using (TaxiDataContext db = new TaxiDataContext())
                {
                    var objBooking = db.Bookings.FirstOrDefault(c => c.Id == jobId);


                    if (objBooking != null && objBooking.DriverId != null &&
                  (objBooking.BookingStatusId == Enums.BOOKINGSTATUS.PENDING || objBooking.BookingStatusId == Enums.BOOKINGSTATUS.PENDING_START
                  || objBooking.BookingStatusId == Enums.BOOKINGSTATUS.ONROUTE || objBooking.BookingStatusId == Enums.BOOKINGSTATUS.ARRIVED
                  || objBooking.BookingStatusId == Enums.BOOKINGSTATUS.POB || objBooking.BookingStatusId == Enums.BOOKINGSTATUS.STC
                  || objBooking.BookingStatusId == Enums.BOOKINGSTATUS.FOJ
                  )
                  )

                    {




                        string paymentType = objBooking.Gen_PaymentType.PaymentCategoryId == null ? objBooking.Gen_PaymentType.DefaultIfEmpty().PaymentType.ToStr()
                                : objBooking.Gen_PaymentType.Gen_PaymentCategory.CategoryName.ToStr();


                        string journey = "O/W";


                        if (objBooking.JourneyTypeId.ToInt() == 3)
                        {
                            journey = "W/R";
                        }


                        string IsExtra = (objBooking.CompanyId != null || objBooking.FromLocTypeId == Enums.LOCATION_TYPES.AIRPORT || objBooking.ToLocTypeId == Enums.LOCATION_TYPES.AIRPORT) ? "1" : "0";
                        int i = 1;
                        string viaP = "";



                        if (objBooking.Booking_ViaLocations.Count > 0)
                        {

                            viaP = string.Join(" * ", objBooking.Booking_ViaLocations.Select(c => "(" + i++.ToStr() + ")" + c.ViaLocValue.ToStr()).ToArray<string>());
                        }


                        string mobileNo = objBooking.CustomerMobileNo.ToStr();
                        string telNo = objBooking.CustomerPhoneNo.ToStr();



                        if (string.IsNullOrEmpty(mobileNo) && !string.IsNullOrEmpty(telNo))
                        {
                            mobileNo = telNo;
                        }
                        else if (!string.IsNullOrEmpty(mobileNo) && !string.IsNullOrEmpty(telNo))
                        {
                            mobileNo += "/" + telNo;
                        }


                        string pickUpPlot = "";
                        string dropOffPlot = "";
                        string companyName = string.Empty;

                        if (objBooking.CompanyId != null && objBooking.Gen_Company.DefaultIfEmpty().AccountTypeId.ToInt() != Enums.ACCOUNT_TYPE.CASH)
                            companyName = objBooking.Gen_Company.DefaultIfEmpty().CompanyName;
                        else
                            companyName = objBooking.Gen_Company.DefaultIfEmpty().CompanyName.ToStr();


                        //error in 13.4 => if its a plot job, then pickup point is hiding in pda.

                        pickUpPlot = objBooking.ZoneId != null ? "<<<" + objBooking.Gen_Zone1.DefaultIfEmpty().ZoneName.ToStr() : "";
                        dropOffPlot = objBooking.DropOffZoneId != null ? "<<<" + objBooking.Gen_Zone.DefaultIfEmpty().ZoneName.ToStr() : "";


                        string fromAddress = objBooking.FromAddress.ToStr().Trim();
                        string toAddress = objBooking.ToAddress.ToStr().Trim();

                        if (objBooking.FromLocTypeId.ToInt() == Enums.LOCATION_TYPES.POSTCODE || objBooking.FromLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT)
                        {
                            fromAddress = objBooking.FromStreet.ToStr() + " " + objBooking.FromAddress.ToStr();

                        }

                        if (objBooking.ToLocTypeId.ToInt() == Enums.LOCATION_TYPES.POSTCODE || objBooking.ToLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT)
                        {
                            toAddress = objBooking.ToStreet.ToStr() + " " + objBooking.ToAddress.ToStr();
                        }

                        //half card and cash
                        string specialRequirements = objBooking.SpecialRequirements.ToStr();


                        decimal pdafares = objBooking.GetType().GetProperty(HubProcessor.Instance.objPolicy.PDAFaresPropertyName.ToStr().Trim()).GetValue(objBooking, null).ToDecimal();





                        string showFaresValue = objBooking.Gen_PaymentType.ShowFaresOnPDA.ToStr().Trim();

                        string showFares = ",\"ShowFares\":\"" + showFaresValue + "\"";
                        string showSummary = ",\"ShowSummary\":\"" + showFaresValue + "\"";



                        pdafares = objBooking.FareRate.ToDecimal() + objBooking.MeetAndGreetCharges.ToDecimal() + objBooking.CongtionCharges.ToDecimal()
                                  + objBooking.AgentCommission.ToDecimal() + objBooking.CashRate.ToDecimal()
                              //+ .CashRate.ToDecimal() + .CashFares.ToDecimal() +
                              + objBooking.ExtraDropCharges.ToDecimal() + objBooking.ServiceCharges.ToDecimal();




                        decimal driverfares = objBooking.FareRate.ToDecimal();

                        if (showFaresValue.ToStr() == "1" && objBooking.CompanyId != null && (objBooking.PaymentTypeId.ToInt() == 1 || objBooking.PaymentTypeId.ToInt() == 2 || objBooking.PaymentTypeId.ToInt() == 6))
                        {
                            //if (AppVars.listUserRights.Count(c => c.functionId == "ALWAYS SHOW DRIVER PRICE IN PDA") == 0)
                            //{
                            pdafares = objBooking.CompanyPrice.ToDecimal() + objBooking.MeetAndGreetCharges.ToDecimal() + objBooking.CongtionCharges.ToDecimal()
                             + objBooking.AgentCommission.ToDecimal() + objBooking.CashRate.ToDecimal()
                         //+ .CashRate.ToDecimal() + .CashFares.ToDecimal() +
                         + objBooking.ExtraDropCharges.ToDecimal() + objBooking.ServiceCharges.ToDecimal();


                            driverfares = objBooking.CompanyPrice.ToDecimal();
                            // }
                        }


                        string revertStatus = "";

                        //string showFares = ",\"ShowFares\":\"" + objBooking.Gen_PaymentType.ShowFaresOnPDA.ToStr().Trim() + "\"";
                        //string showSummary = ",\"ShowSummary\":\"" + "1" + "\"";


                        string agentDetails = string.Empty;
                        string parkingandWaiting = string.Empty;
                        if (objBooking.CompanyId != null)
                        {
                            agentDetails = ",\"AgentFees\":\"" + String.Format("{0:0.00}", objBooking.AgentCommission + objBooking.CashRate.ToDecimal() + objBooking.ServiceCharges.ToDecimal() + objBooking.ExtraDropCharges.ToDecimal()) + "\"";

                            //   agentDetails = ",\"AgentFees\":\"" + String.Format("{0:0.00}", objBooking.AgentCommission) + "\"";
                            parkingandWaiting = ",\"Parking\":\"" + string.Format("{0:0.00}", objBooking.ParkingCharges) + "\",\"Waiting\":\"" + String.Format("{0:0.00}", objBooking.WaitingCharges) + "\"";


                            /*   if (objBooking.PaymentTypeId.ToInt() == Enums.PAYMENT_TYPES.CASH)
                               {
                                   pdafares = objBooking.FareRate.ToDecimal() + objBooking.ParkingCharges.ToDecimal() + objBooking.WaitingCharges.ToDecimal() + objBooking.AgentCommission.ToDecimal();


                               }*/
                        }
                        else
                        {
                            agentDetails = ",\"AgentFees\":\"" + String.Format("{0:0.00}", objBooking.ServiceCharges.ToDecimal() + objBooking.ExtraDropCharges.ToDecimal()) + "\"";

                            parkingandWaiting = ",\"Parking\":\"" + string.Format("{0:0.00}", objBooking.CongtionCharges) + "\",\"Waiting\":\"" + String.Format("{0:0.00}", objBooking.MeetAndGreetCharges) + "\"";
                            //

                        }


                        string fromdoorno = objBooking.FromDoorNo.ToStr().Trim();
                        if (fromdoorno.Length > 0 && fromdoorno.WordCount() > 2 && fromdoorno.Contains(" "))
                        {

                            try
                            {

                                fromdoorno = fromdoorno.Replace(" ", "-");
                            }
                            catch
                            {


                            }
                        }


                        if (fromAddress.ToStr().Trim().Contains("-") && objBooking.Fleet_Driver.Fleet_Driver_PDASettings[0].CurrentPdaVersion.ToDecimal() == 23.50m)
                        {
                            fromAddress = fromAddress.Replace("-", "  ");

                        }




                        if (specialRequirements.ToStr().Contains("\""))
                            specialRequirements = specialRequirements.ToStr().Replace("\"", "-").Trim();


                        string appendString = string.Empty;


                        appendString = ",\"ShowOnlyPlot\":\"" + "0" + "\"" +
                                   ",\"BookingType\":\"" + objBooking.BookingType.BookingTypeName.ToStr() + "\"" +
                               ",\"ExtraCharges\":\"" + objBooking.ExtraDropCharges.ToDecimal() + "\"" +
                                ",\"BookingFee\":\"" + 0.00 + "\"";





                        string summary = string.Empty;

                        //if (AppVars.objPolicyConfiguration.PDAVersion.ToDecimal() > 100)
                        //{

                        //    List<BookingSummary> listofSummary = new List<BookingSummary>();

                        //    if (objBooking.CompanyId != null)
                        //        listofSummary.Add(new BookingSummary { label = "Agent Fee", value = string.Format("{0:0.00}", objBooking.AgentCommission.ToDecimal() + objBooking.CashRate.ToDecimal()) });


                        //    listofSummary.Add(new BookingSummary { label = "Parking", value = string.Format("{0:0.00}", objBooking.CongtionCharges.ToDecimal()) });
                        //    listofSummary.Add(new BookingSummary { label = "Waiting", value = string.Format("{0:0.00}", objBooking.MeetAndGreetCharges.ToDecimal()) });

                        //    listofSummary.Add(new BookingSummary { label = "Extras", value = string.Format("{0:0.00}", objBooking.ExtraDropCharges.ToDecimal()) });



                        //    summary = ",\"Summary\":" + Newtonsoft.Json.JsonConvert.SerializeObject(listofSummary);
                        //}


                        string toDoorNo = objBooking.ToDoorNo.ToStr().Trim();

                        if (objBooking.ToLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT && objBooking.JourneyTypeId.ToInt() == Enums.JOURNEY_TYPES.RETURN)
                            toDoorNo = string.Empty;
                        else if (toDoorNo.Length > 0)
                            toDoorNo = toDoorNo + "-";

                        string msg = "Update Job>>" + "{ \"JobId\" :\"" + objBooking.Id.ToStr() +
                                               "\", \"Pickup\":\"" + (!string.IsNullOrEmpty(objBooking.FromDoorNo) ? fromdoorno + "-" + fromAddress + pickUpPlot : fromAddress + pickUpPlot) +
                                                "\", \"Destination\":\"" + toDoorNo + toAddress + dropOffPlot + "\"," +
                                               "\"PickupDateTime\":\"" + string.Format("{0:dd/MM/yyyy   HH:mm}", objBooking.PickupDateTime) + "\"" +
                                               ",\"Cust\":\"" + objBooking.CustomerName + "\",\"Mob\":\"" + mobileNo + " " + "\",\"Fare\":\"" + string.Format("{0:0.00}", pdafares) + "\",\"Vehicle\":\"" + objBooking.Fleet_VehicleType.VehicleType + "\",\"Account\":\"" + companyName + " " + "\"" +
                                                 ",\"Lug\":\"" + objBooking.NoofLuggages.ToInt() + "\",\"Passengers\":\"" + objBooking.NoofPassengers.ToInt() + "\",\"Journey\":\"" + journey + "\",\"Payment\":\"" + paymentType + "\",\"Special\":\"" + specialRequirements + " " + "\",\"Extra\":\"" + IsExtra + "\",\"Via\":\"" + viaP + " " + "\"" +
                                           ",\"CompanyId\":\"" + objBooking.CompanyId.ToInt() + "\",\"SubCompanyId\":\"" + objBooking.SubcompanyId.ToInt() + "\",\"QuotedPrice\":\"" + (objBooking.IsQuotedPrice.ToBool() ? "1" : "0") + "\"" +

                                                 parkingandWaiting + ",\"DriverFares\":\"" + String.Format("{0:0.00}", driverfares) + "\"" +
                                              agentDetails +
                                                 ",\"Did\":\"" + objBooking.DriverId + "\",\"BabySeats\":\"" + objBooking.BabySeats.ToStr() + "\"" + showFares + showSummary + appendString + revertStatus + " }";


                        var driverId = objBooking.DriverId;
                        var bookingId = objBooking.Id;

                        try
                        {


                            HubProcessor.Instance.listofJobs.Add(new clsPDA
                            {
                                DriverId = driverId.ToInt(),
                                JobId = bookingId,
                                MessageDateTime = DateTime.Now.AddSeconds(-15),
                                JobMessage = msg,
                                MessageTypeId = 8
                            });


                        }
                        catch
                        {
                            System.Threading.Thread.Sleep(500);
                            HubProcessor.Instance.listofJobs.Add(new clsPDA
                            {
                                DriverId = driverId.ToInt(),
                                JobId = bookingId,
                                MessageDateTime = DateTime.Now.AddSeconds(-15),
                                JobMessage = msg,
                                MessageTypeId = 8
                            });

                        }


                        //new Thread(delegate ()
                        //{

                        //    General.requestPDA("request pda=" + driverId + "=" + bookingId + "=" + msg + "=8");
                        //}).Start();



                        //if (AppVars.objPolicyConfiguration.DespatchOfflineJobs.ToBool())
                        //{
                        //    using (TaxiDataContext db = new TaxiDataContext())
                        //    {
                        //        db.stp_SaveOfflineMessage(objBooking.Id, objBooking.DriverId, "", AppVars.LoginObj.LoginName.ToStr(), "Update Job>>" + objBooking.DriverId + ">>" + objBooking.Id + ">>" + msg + "=8");
                        //    }

                        //}


                    }


                }
            }
            catch (Exception ex)
            {


            }


        }



        public static void UpdateJobToDriverPDA(Booking objMaster, string parampaymenttype = "")
        {
            try
            {



                if (objMaster.DriverId != null &&
                   (objMaster.BookingStatusId == Enums.BOOKINGSTATUS.PENDING || objMaster.BookingStatusId == Enums.BOOKINGSTATUS.PENDING_START
                   || objMaster.BookingStatusId == Enums.BOOKINGSTATUS.ONROUTE || objMaster.BookingStatusId == Enums.BOOKINGSTATUS.ARRIVED
                   || objMaster.BookingStatusId == Enums.BOOKINGSTATUS.POB || objMaster.BookingStatusId == Enums.BOOKINGSTATUS.STC
                   || objMaster.BookingStatusId == Enums.BOOKINGSTATUS.FOJ
                   ))


                {


                    string paymentType = objMaster.Gen_PaymentType.PaymentCategoryId == null ? objMaster.Gen_PaymentType.DefaultIfEmpty().PaymentType.ToStr()
                            : objMaster.Gen_PaymentType.Gen_PaymentCategory.CategoryName.ToStr();


                    if (parampaymenttype.ToStr().Trim().Length > 0)
                        paymentType = parampaymenttype;

                    string journey = "O/W";


                    if (objMaster.JourneyTypeId.ToInt() == 3)
                    {
                        journey = "W/R";
                    }


                    string IsExtra = (objMaster.CompanyId != null || objMaster.FromLocTypeId == Enums.LOCATION_TYPES.AIRPORT || objMaster.ToLocTypeId == Enums.LOCATION_TYPES.AIRPORT) ? "1" : "0";
                    int i = 1;
                    string viaP = "";



                    if (objMaster.Booking_ViaLocations.Count > 0)
                    {

                        viaP = string.Join(" * ", objMaster.Booking_ViaLocations.Select(c => "(" + i++.ToStr() + ")" + c.ViaLocValue.ToStr()).ToArray<string>());
                    }


                    string mobileNo = objMaster.CustomerMobileNo.ToStr();
                    string telNo = objMaster.CustomerPhoneNo.ToStr();



                    if (string.IsNullOrEmpty(mobileNo) && !string.IsNullOrEmpty(telNo))
                    {
                        mobileNo = telNo;
                    }
                    else if (!string.IsNullOrEmpty(mobileNo) && !string.IsNullOrEmpty(telNo))
                    {
                        mobileNo += "/" + telNo;
                    }


                    string pickUpPlot = "";
                    string dropOffPlot = "";
                    string companyName = string.Empty;

                    if (objMaster.CompanyId != null && objMaster.Gen_Company.DefaultIfEmpty().AccountTypeId.ToInt() != Enums.ACCOUNT_TYPE.CASH)
                        companyName = objMaster.Gen_Company.DefaultIfEmpty().CompanyName;
                    else
                        companyName = objMaster.Gen_Company.DefaultIfEmpty().CompanyName.ToStr();


                    //error in 13.4 => if its a plot job, then pickup point is hiding in pda.

                    pickUpPlot = objMaster.ZoneId != null ? "<<<" + objMaster.Gen_Zone1.DefaultIfEmpty().ZoneName.ToStr() : "";
                    dropOffPlot = objMaster.DropOffZoneId != null ? "<<<" + objMaster.Gen_Zone.DefaultIfEmpty().ZoneName.ToStr() : "";


                    string fromAddress = objMaster.FromAddress.ToStr().Trim();
                    string toAddress = objMaster.ToAddress.ToStr().Trim();

                    if (objMaster.FromLocTypeId.ToInt() == Enums.LOCATION_TYPES.POSTCODE || objMaster.FromLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT)
                    {
                        fromAddress = objMaster.FromStreet.ToStr() + " " + objMaster.FromAddress.ToStr();

                    }

                    if (objMaster.ToLocTypeId.ToInt() == Enums.LOCATION_TYPES.POSTCODE || objMaster.ToLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT)
                    {
                        toAddress = objMaster.ToStreet.ToStr() + " " + objMaster.ToAddress.ToStr();
                    }

                    //half card and cash
                    string specialRequirements = objMaster.SpecialRequirements.ToStr();
                    if (objMaster.SecondaryPaymentTypeId != null && objMaster.CashFares.ToDecimal() > 0)
                    {

                        specialRequirements += " , Additional Cash Payment : " + objMaster.CashFares.ToDecimal();
                    }

                    //   decimal pdafares = objMaster.GetType().GetProperty(Ins AppVars.objPolicyConfiguration.PDAFaresPropertyName.ToStr().Trim()).GetValue(objMaster., null).ToDecimal();





                    string showFaresValue = objMaster.Gen_PaymentType.ShowFaresOnPDA.ToStr().Trim();

                    string showFares = ",\"ShowFares\":\"" + showFaresValue + "\"";
                    string showSummary = ",\"ShowSummary\":\"" + showFaresValue + "\"";



                    decimal pdafares = objMaster.FareRate.ToDecimal() + objMaster.MeetAndGreetCharges.ToDecimal() + objMaster.CongtionCharges.ToDecimal()
                              + objMaster.AgentCommission.ToDecimal()

                          + objMaster.ExtraDropCharges.ToDecimal() + objMaster.ServiceCharges.ToDecimal();

                    if (showFaresValue.ToStr() == "1" && objMaster.CompanyId != null && (objMaster.PaymentTypeId.ToInt() == 2 || objMaster.PaymentTypeId.ToInt() == 6))
                    {
                        pdafares = objMaster.CompanyPrice.ToDecimal() + objMaster.MeetAndGreetCharges.ToDecimal() + objMaster.CongtionCharges.ToDecimal()
                             + objMaster.AgentCommission.ToDecimal()

                         + objMaster.ExtraDropCharges.ToDecimal() + objMaster.ServiceCharges.ToDecimal();
                    }






                    string agentDetails = string.Empty;
                    string parkingandWaiting = string.Empty;
                    if (objMaster.CompanyId != null)
                    {
                        agentDetails = ",\"AgentFees\":\"" + String.Format("{0:0.00}", objMaster.AgentCommission + objMaster.ServiceCharges.ToDecimal() + objMaster.ExtraDropCharges.ToDecimal()) + "\"";


                        parkingandWaiting = ",\"Parking\":\"" + string.Format("{0:0.00}", objMaster.ParkingCharges) + "\",\"Waiting\":\"" + String.Format("{0:0.00}", objMaster.WaitingCharges) + "\"";



                    }
                    else
                    {
                        agentDetails = ",\"AgentFees\":\"" + String.Format("{0:0.00}", objMaster.ServiceCharges.ToDecimal() + objMaster.ExtraDropCharges.ToDecimal()) + "\"";

                        parkingandWaiting = ",\"Parking\":\"" + string.Format("{0:0.00}", objMaster.CongtionCharges) + "\",\"Waiting\":\"" + String.Format("{0:0.00}", objMaster.MeetAndGreetCharges) + "\"";
                        //

                    }


                    string fromdoorno = objMaster.FromDoorNo.ToStr().Trim();
                    if (fromdoorno.Length > 0 && fromdoorno.WordCount() > 2 && fromdoorno.Contains(" "))
                    {

                        try
                        {

                            fromdoorno = fromdoorno.Replace(" ", "-");
                        }
                        catch
                        {


                        }
                    }


                    if (fromAddress.ToStr().Trim().Contains("-") && objMaster.Fleet_Driver.Fleet_Driver_PDASettings[0].CurrentPdaVersion.ToDecimal() == 23.50m)
                    {
                        fromAddress = fromAddress.Replace("-", "  ");

                    }


                    string appendString = string.Empty;



                    string msg = "Update Job>>" + "{ \"JobId\" :\"" + objMaster.Id.ToStr() +
                                           "\", \"Pickup\":\"" + (!string.IsNullOrEmpty(objMaster.FromDoorNo) ? fromdoorno + "-" + fromAddress + pickUpPlot : fromAddress + pickUpPlot) +
                                           "\", \"Destination\":\"" + (!string.IsNullOrEmpty(objMaster.ToDoorNo) ? objMaster.ToDoorNo + "-" + toAddress + dropOffPlot : toAddress + dropOffPlot) + "\"," +
                                           "\"PickupDateTime\":\"" + string.Format("{0:dd/MM/yyyy   HH:mm}", objMaster.PickupDateTime) + "\"" +
                                           ",\"Cust\":\"" + objMaster.CustomerName + "\",\"Mob\":\"" + mobileNo + " " + "\",\"Fare\":\"" + string.Format("{0:0.00}", pdafares) + "\",\"Vehicle\":\"" + objMaster.Fleet_VehicleType.VehicleType + "\",\"Account\":\"" + companyName + " " + "\"" +
                                             ",\"Lug\":\"" + objMaster.NoofLuggages.ToInt() + "\",\"Passengers\":\"" + objMaster.NoofPassengers.ToInt() + "\",\"Journey\":\"" + journey + "\",\"Payment\":\"" + paymentType + "\",\"Special\":\"" + specialRequirements + " " + "\",\"Extra\":\"" + IsExtra + "\",\"Via\":\"" + viaP + " " + "\"" +
                                       ",\"CompanyId\":\"" + objMaster.CompanyId.ToInt() + "\",\"SubCompanyId\":\"" + objMaster.SubcompanyId.ToInt() + "\",\"QuotedPrice\":\"" + (objMaster.IsQuotedPrice.ToBool() ? "1" : "0") + "\"" +

                                             parkingandWaiting + ",\"DriverFares\":\"" + String.Format("{0:0.00}", objMaster.FareRate) + "\"" +
                                          agentDetails +
                                             ",\"Did\":\"" + objMaster.DriverId + "\",\"BabySeats\":\"" + objMaster.BabySeats.ToStr() + "\"" + showFares + showSummary + appendString + " }";

                    string recordId = Guid.NewGuid().ToString();

                    try
                    {


                        HubProcessor.Instance.listofJobs.Add(new clsPDA
                        {
                            DriverId = objMaster.DriverId.ToInt(),
                            JobId = objMaster.Id,
                            MessageDateTime = DateTime.Now.AddSeconds(-15),
                            JobMessage = msg,
                            MessageTypeId = 8,
                            Id = recordId
                        });


                    }
                    catch
                    {
                        System.Threading.Thread.Sleep(500);
                        try
                        {
                            HubProcessor.Instance.listofJobs.Add(new clsPDA
                            {
                                DriverId = objMaster.DriverId.ToInt(),
                                JobId = objMaster.Id,
                                MessageDateTime = DateTime.Now.AddSeconds(-15),
                                JobMessage = msg,
                                MessageTypeId = 8,
                                Id = recordId
                            });
                        }
                        catch
                        {

                        }

                    }



                    SocketIO.SendToSocket(objMaster.DriverId.ToStr(), msg, "updateJob", recordId);




                }
            }
            catch (Exception ex)
            {


            }


        }

        public static string EncodeBASE64(string text)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(text)).TrimEnd('=').Replace('+', '-')
                .Replace('/', '_');
        }

        public static string ProcessJudoPayment(Gen_SysPolicy_PaymentDetail obj, ClsPaymentInformation objCard
         , bool responseInJson = false, bool checkExistingPayment = false, string yourconsumerreference = "")
        {


            if (checkExistingPayment)
            {

                var objBookingPayment = General.GetObject<Booking_Payment>(c => c.BookingId == objCard.BookingId.ToLong() && c.AuthCode != null && c.AuthCode != "");


                if (objBookingPayment != null)
                    return "false:" + "already paid";
            }

            try
            {
                if (objCard.BookingId.ToStr().Trim().Length > 0)
                {
                    if (File.Exists(AppContext.BaseDirectory + "\\Transactions\\" + objCard.BookingId.ToStr() + ".txt"))
                    {

                        //
                        string receiptId = File.ReadAllText(AppContext.BaseDirectory + "\\Transactions\\" + objCard.BookingId.ToStr().Trim() + ".txt");
                        try
                        {
                            //
                            File.AppendAllText(AppContext.BaseDirectory + "\\processjudopayment_receiptfound.txt", DateTime.Now.ToStr() + " bookingid:" + objCard.BookingId.ToStr()
                               + ",receipt:" + receiptId.ToStr() + Environment.NewLine);
                        }
                        catch
                        {

                        }


                        receiptId = receiptId.Split(':')[0].ToStr();
                        return "success:" + receiptId;

                    }
                    else if (File.Exists(AppContext.BaseDirectory + "\\FailedTransactions\\" + objCard.BookingId.ToStr() + ".txt"))
                    {
                        string data = File.ReadAllText(AppContext.BaseDirectory + "\\FailedTransactions\\" + objCard.BookingId.ToStr().Trim() + ".txt");
                        try
                        {
                            //

                            data = data.Split(',')[0];


                            if (objCard.TokenDetails.ToStr().Trim().Contains("receiptid|" + data))
                            {


                                try
                                {
                                    File.AppendAllText(AppContext.BaseDirectory + "\\processjudopayment_declinereceiptfound.txt", DateTime.Now.ToStr() + " bookingid:" + objCard.BookingId.ToStr()
                                  + ",receipt:" + data.ToStr() + Environment.NewLine);
                                }
                                catch
                                {

                                }

                                return "failed:" + data.Split(':')[1];





                            }
                            else
                            {

                                try
                                {
                                    File.AppendAllText(AppContext.BaseDirectory + "\\processjudopayment_declinereceiptnotfound.txt", DateTime.Now.ToStr() + " bookingid:" + objCard.BookingId.ToStr()
                                  + ",oldreceipt:" + data.ToStr() + ",newreceiptid:" + objCard.TokenDetails.ToStr().Trim() + Environment.NewLine);
                                }
                                catch
                                {

                                }
                            }
                        }
                        catch
                        {

                        }





                    }


                }
            }
            catch
            { }




            if (obj == null)
                obj = General.GetObject<Gen_SysPolicy_PaymentDetail>(c => c.SysPolicyId != 0 && (c.PaymentGatewayId == 6));

            string rtn = string.Empty;
            string[] arrtokedetails = objCard.TokenDetails.ToStr().Trim().Split(new string[] { "<<<" }, StringSplitOptions.RemoveEmptyEntries);

            string json2 = string.Empty;
            string rtn2 = string.Empty;
            if (arrtokedetails.Count() > 5 && objCard.TokenDetails.ToStr().Trim().Contains("receiptid"))
            {

                try
                {

                    try
                    {



                        File.AppendAllText(AppContext.BaseDirectory + "\\ProcessJudoPaymentViaToken.txt", DateTime.Now.ToStr() + ": request" + json2 + ",response:" + rtn2 + Environment.NewLine);
                    }
                    catch
                    {


                    }

                    JudoProcessPaymentRequest obje = new JudoProcessPaymentRequest();

                    if (yourconsumerreference.ToStr().Trim().Length > 0)
                        obje.yourConsumerReference = yourconsumerreference;
                    else
                        obje.yourConsumerReference = (objCard.BookingNo.ToStr().Trim() + "/" + objCard.DriverNo);

                    obje.yourPaymentReference = objCard.BookingNo.ToStr().Trim();

                    obje.judoId = obj.PaypalID.ToStr().Replace("-", "").Trim().ToLong();
                    obje.APISecret = obj.MerchantID.ToStr();
                    obje.APIToken = obj.MerchantPassword.ToStr();




                    obje.amount = Convert.ToDouble(Math.Round(objCard.Total.ToDecimal(), 2));
                    //  obje.cardNumber = objCard.CardNumber.ToStr(); // 4976000000003436
                    //   obje.expiryDate = objCard.ExpiryMonth.ToStr() + "/" + objCard.ExpiryYear.ToStr(); // 1220
                    //   obje.cv2 = objCard.CV2.ToStr();  // 458
                    obje.currency = "GBP";


                    try
                    {


                        //     File.AppendAllText(physicalPath + "\\judo3d2.txt", DateTime.Now.ToStr() + ": request" + json2 + ",response:" + rtn2 + Environment.NewLine);
                    }
                    catch
                    {


                    }

                    if (objCard.TokenDetails.ToStr().ToLower().Contains("is3ds"))
                        objCard.TokenDetails = objCard.TokenDetails.Replace("<<<type|2<<<is3DS|false", "").Trim();

                    arrtokedetails = objCard.TokenDetails.ToStr().Trim().Split(new string[] { "<<<" }, StringSplitOptions.RemoveEmptyEntries);


                    obje.cardToken = arrtokedetails[0].Replace("Token|", "").Trim();
                    //  obje.consu = arrtokedetails[2].Replace("consumertoken|", "").Trim();
                    obje.yourConsumerReference = arrtokedetails[1].Replace("consumer|", "").Trim();


                    obje.expiryDate = arrtokedetails[4].Replace("enddate|", "").Trim();

                    if (arrtokedetails.Count() > 5)

                    {
                        obje.receiptId = arrtokedetails[5].Replace("receiptid|", "").Trim().ToLong();

                        if (objCard.TokenDetails.ToStr().Trim().ToLower().Contains("is3dsregisterattempt"))
                            obje.IsThreeDSecureAttempted = arrtokedetails[6].Replace("is3dsregisterattempt|", "").Trim().ToStr();



                        try
                        {

                            if (objCard.BookingNo.ToStr().Trim().Length < 30)
                                obje.yourPaymentReference = objCard.BookingNo.ToStr().Trim() + "-" + arrtokedetails[3].Replace("lastfour|", "").Trim();
                        }
                        catch
                        {

                        }

                    }


                    json2 = new JavaScriptSerializer().Serialize(obje);


                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    HttpClient httpClient = new HttpClient();
                    var stringContent = new StringContent
                    (Newtonsoft.Json.JsonConvert.SerializeObject(obje), Encoding.UTF8, "application/json");

                    HttpResponseMessage response = httpClient.PostAsync("https://api-eurosofttech.co.uk/Judo3DSLive/judopayapi/transaction/payment/cardtoken", stringContent).Result;
                    httpClient.Dispose();
                    rtn2 = response.Content.ReadAsStringAsync().Result;


                    TransactionCompletedResponse objRes = Newtonsoft.Json.JsonConvert.DeserializeObject<TransactionCompletedResponse>(rtn2);


                    try
                    {


                        try
                        {
                            File.AppendAllText(AppContext.BaseDirectory + "\\ProcesswithTokenResponse.txt", DateTime.Now.ToStr() + ": request" + json2 + ",response:" + new JavaScriptSerializer().Serialize(objRes) + Environment.NewLine);

                        }
                        catch
                        {

                        }


                        if (objRes.IsSuccess && objRes.transactionResponse != null && objRes.transactionResponse.result.ToStr().ToLower() == "success")
                        {
                            rtn = "success:" + objRes.receiptId.ToStr();

                            try
                            {
                                File.AppendAllText(AppContext.BaseDirectory + "\\ProcesswithTokenResponseSuccess.txt", DateTime.Now.ToStr() + ": request" + json2 + ",response:" + new JavaScriptSerializer().Serialize(objRes) + Environment.NewLine);

                            }
                            catch
                            {

                            }

                        }
                        else
                        {

                            if (objRes.IsSuccess && objRes.transactionResponse != null && objRes.transactionResponse.result.ToStr().ToLower() != "success")
                            {

                                rtn = "failed:" + objRes.transactionResponse.message.ToStr();


                                try
                                {
                                    File.AppendAllText(AppContext.BaseDirectory + "\\ProcesswithTokenResponseFailed.txt", DateTime.Now.ToStr() + ": request" + json2 + ",response:" + new JavaScriptSerializer().Serialize(objRes) + Environment.NewLine);

                                }
                                catch
                                {

                                }




                                try
                                {
                                    if (Directory.Exists(AppContext.BaseDirectory + "\\FailedTransactions") == false)
                                    {
                                        Directory.CreateDirectory(AppContext.BaseDirectory + "\\FailedTransactions");

                                    }



                                    string attempted = objRes.transactionResponse.threeDSecure != null ? objRes.transactionResponse.threeDSecure.attempted.ToStr() : "false";


                                    File.AppendAllText(AppContext.BaseDirectory + "\\FailedTransactions\\" + objCard.BookingId + ".txt", objRes.transactionResponse.receiptId.ToStr() + ":" + objRes.transactionResponse.message.ToStr() + ":" + objRes.transactionResponse.cardDetails.cardLastFour.ToStr() + ":" + attempted);


                                }
                                catch
                                {

                                }







                            }
                            else
                            {

                                try
                                {
                                    if (objRes.Errors != null)
                                    {
                                        if (objRes.Errors.details != null && objRes.Errors.details.Count > 0)
                                            rtn = "failed:" + objRes.Errors.details[0].message.ToStr();
                                        else
                                            rtn = "failed:" + new JavaScriptSerializer().Deserialize<Detail>(objRes.Errors.message.ToStr()).message;
                                    }
                                    else
                                    {
                                        rtn = "failed:" + objRes.message.ToStr();
                                    }
                                }
                                catch
                                {
                                    rtn = "failed:" + objRes.message.ToStr();

                                }


                                try
                                {
                                    File.AppendAllText(AppContext.BaseDirectory + "\\ProcesswithTokenResponseError.txt", DateTime.Now.ToStr() + ": request" + json2 + ",response:" + new JavaScriptSerializer().Serialize(objRes) + Environment.NewLine);

                                }
                                catch
                                {

                                }

                            }
                        }


                    }
                    catch
                    {


                    }

                    if (responseInJson)
                    {
                        if (objRes.IsSuccess && objRes.transactionResponse.result.ToStr().ToLower() == "success")
                        {
                            rtn = "success:" + objRes.receiptId.ToStr() + ":" + objRes.amount + ":" + objRes.transactionResponse.cardDetails.cardLastFour + ":" + objRes.transactionResponse.authCode.ToStr();

                            try
                            {


                                File.AppendAllText(AppContext.BaseDirectory + "\\processsuccessfromdispatch.txt", DateTime.Now.ToStr() + ": request" + json2 + ",response:" + rtn + ",response:" + rtn + Environment.NewLine);
                            }
                            catch
                            {


                            }

                        }
                        else
                        {

                            if (objRes.IsSuccess && objRes.transactionResponse != null && objRes.transactionResponse.result.ToStr().ToLower() != "success")
                                rtn = "failed:" + objRes.transactionResponse.message.ToStr();
                            else
                            {

                                if (objRes.Errors.details != null && objRes.Errors.details.Count > 0)
                                    rtn = "failed:" + objRes.Errors.details[0].message.ToStr();
                                else
                                    rtn = "failed:" + new JavaScriptSerializer().Deserialize<Detail>(objRes.Errors.message.ToStr()).message;

                            }

                            try
                            {


                                File.AppendAllText(AppContext.BaseDirectory + "\\processfailedfromdispatch.txt", DateTime.Now.ToStr() + ": request" + json2 + ",response:" + rtn2 + ",response:" + rtn + Environment.NewLine);
                            }
                            catch
                            {


                            }

                            //if (objRes.Errors.details != null && objRes.Errors.details.Count > 0)
                            //    rtn = "failed:" + objRes.Errors.details[0].message.ToStr();
                            //else
                            //    rtn = "failed:" + new JavaScriptSerializer().Deserialize<Detail>(objRes.Errors.message.ToStr()).message;
                        }


                    }

                }
                catch (Exception ex)
                {
                    try
                    {


                        File.AppendAllText(AppContext.BaseDirectory + "\\ProcessJudoPaymentViaToken_exception.txt", DateTime.Now.ToStr() + ": request" + json2 + ",response:" + rtn2 + ",exception:" + ex.Message + Environment.NewLine);
                    }
                    catch
                    {


                    }
                }
            }





            return rtn;
        }

        public static void UpdatePoolJob(long onlineBookingId, int bookingTypeId, long jobId, int driverId, int jobstatusId, string eventName)
        {
            try
            {

                if (bookingTypeId == 0)
                {
                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        db.CommandTimeout = 5;

                        var objBook = db.Bookings.Where(c => c.Id == jobId).Select(args => new { args.OnlineBookingId, args.BookingTypeId }).FirstOrDefault();
                        if (objBook.OnlineBookingId != null)
                        {

                            onlineBookingId = objBook.OnlineBookingId.ToLong();
                            bookingTypeId = objBook.BookingTypeId.ToInt();

                        }

                    }
                }

                if (onlineBookingId != 0)
                {
                    if (bookingTypeId.ToInt() == 100)
                    {

                        new System.Threading.Thread(delegate ()
                        {
                            try
                            {

                                General.UpdateThirdPartyJob(null, bookingTypeId.ToInt(), jobId, driverId, onlineBookingId.ToLong(), jobstatusId, eventName, HubProcessor.Instance.objPolicy.DefaultClientId.ToStr(), "");
                            }
                            catch (Exception ex)
                            {
                            }
                        }).Start();

                    }
                }

            }
            catch
            {

            }

        }

        public static bool UpdateThirdPartyJob(Booking objBooking, int bookingTypeId, long jobId, int driverId, long onlineBookingId, int bookingstatusId, string status, string defaultclientId, string bookingNo)
        {

            bool rtn = true;



            try
            {




                //if (objBooking == null)
                //    objBooking = General.GetObject<Booking>(c => c.Id == jobId);


                //   int sysgenId = objBooking.CompanyId != null ? objBooking.Gen_Company.SysGenId.ToInt() : 0;



                if (bookingTypeId == 100)
                {



                    try
                    {

                        using (TaxiDataContext db = new TaxiDataContext())

                        {

                            CabTreasureJobPoolGateway.Models.ClsDriverDetails obj = new CabTreasureJobPoolGateway.Models.ClsDriverDetails();

                            try
                            {
                                var objDriver = db.Fleet_Drivers.Where(c => c.Id == driverId).Select(args => new { args.DriverName, args.DriverNo, args.VehicleColor, args.VehicleMake, args.VehicleModel, args.VehicleNo }).FirstOrDefault();
                                String drvbadge = db.Fleet_Driver_Documents.Where(c => c.DriverId == driverId && c.DocumentId == Enums.DRIVER_DOCUMENTS.PCODriver).Select(c => c.BadgeNumber).FirstOrDefault().ToStr();
                                String vehbadge = db.Fleet_Driver_Documents.Where(c => c.DriverId == driverId && c.DocumentId == Enums.DRIVER_DOCUMENTS.PCOVehicle).Select(c => c.BadgeNumber).FirstOrDefault().ToStr();

                                obj.driverBadge = drvbadge;
                                obj.vehicleBadge = vehbadge;
                                obj.driverNo = objDriver.DriverNo.ToStr();
                                obj.driverName = objDriver.DriverName.ToStr();
                                obj.vehicleColor = objDriver.VehicleColor.ToStr();
                                obj.vehicleMake = objDriver.VehicleMake.ToStr();
                                obj.vehicleModel = objDriver.VehicleModel.ToStr();
                                obj.vehicleNo = objDriver.VehicleNo.ToStr();
                                obj.BookingId = jobId.ToStr();
                                obj.BookingNo = bookingNo.ToStr();

                                obj.photoLinkId = db.Fleet_Driver_Images.Where(c => c.DriverId == driverId).Select(c => c.PhotoLinkId).FirstOrDefault().DefaultIfEmpty().ToStr();


                                string encrypt = Cryptography.Encrypt(obj.BookingNo.ToStr() + ":" + obj.photoLinkId + ":" + Cryptography.Decrypt(System.Configuration.ConfigurationManager.AppSettings["ConnectionString"], "softeuroconnskey", true).ToStr() + ":" + obj.BookingId, "softeuroconnskey", true);
                                encrypt = "http://tradrv.co.uk/tckJP.aspx?q=" + encrypt;
                                obj.ListenerDetails = ToTinyURLS(encrypt);
                            }
                            catch
                            {

                            }




                            var driverDetails = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                            var result = JobPoolAPICaller.JobPoolAPIProxy.ChangeDriverStatus(onlineBookingId.ToStr(), bookingstatusId.ToStr(), defaultclientId, driverDetails);

                        }
                    }
                    catch (Exception ex)
                    {

                    }



                }




            }
            catch (Exception ex)
            {



                rtn = false;
            }

            return rtn;

        }



        public static string ToTinyURLS(string txt)
        {
            Regex regx = null;

            if (txt.Contains("https"))
                regx = new Regex("https://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,]*)?", RegexOptions.IgnoreCase);
            else
                regx = new Regex("http://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,]*)?", RegexOptions.IgnoreCase);

            MatchCollection mactches = regx.Matches(txt);

            foreach (Match match in mactches)
            {
                string tURL = MakeTinyUrl(match.Value);
                txt = txt.Replace(match.Value, tURL);
            }


            if (txt.ToStr().Contains("tinyurl.com") == false)
                txt = GetShortUrl(txt);
            return txt;
        }

        public static string MakeTinyUrl(string Url)
        {
            string text;
            try
            {
                if (Url.Length <= 12)
                {
                    return Url;
                }
                if (!Url.ToLower().StartsWith("http") && !Url.ToLower().StartsWith("ftp"))
                {
                    Url = "http://" + Url;
                }

                WebRequest request = HttpWebRequest.Create("http://tinyurl.com/api-create.php?url=" + Url);
                request.Proxy = null;
                WebResponse response = request.GetResponse();
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {



                    text = reader.ReadToEnd();
                }
                return text;
                //
            }
            catch (Exception)
            {
                return Url;

            }
        }


        public static string GetShortUrl(string URL)
        {

            string rtn = string.Empty;


            string urlParameters = "?key=295ca3b2f465e5562feedd06246ac7ccf08ae&short=" + URL;

            try
            {

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("https://cutt.ly/api/api.php");
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));


                HttpResponseMessage response = client.GetAsync(urlParameters).Result;  // Blocking call! Program will wait here until a response is received or a timeout occurs.
                if (response.IsSuccessStatusCode)
                {

                    var dataObjects = response.Content.ReadAsStringAsync().Result;  //Make sure to add a reference to System.Net.Http.Formatting.dll
                    RootUrl obj = Newtonsoft.Json.JsonConvert.DeserializeObject<RootUrl>(dataObjects);
                    rtn = obj.url.shortLink.ToStr();

                }
                else
                    rtn = URL;
                client.Dispose();
            }
            catch (Exception ex)
            {

                rtn = URL;
            }



            return rtn;

        }

        public class Url
        {
            public int status { get; set; }
            public string fullLink { get; set; }
            public string date { get; set; }
            public string shortLink { get; set; }
            public string title { get; set; }
        }

        public class RootUrl
        {
            public Url url { get; set; }
        }

        public static string GetLocationName(double? latitude, double? longitude)
        {
            string locationName = string.Empty;
            try
            {

                if (Global.googleKey.ToStr().Length == 0)
                {

                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        db.CommandTimeout = 5;
                        Global.googleKey = "&key=" + db.ExecuteQuery<string>("select apikey from mapkeys where maptype='here'").FirstOrDefault();


                    }
                }






                if (Global.googleKey.ToStr().Trim().Length > 0)
                {
                    try
                    {


                        var obj = new
                        {

                            defaultclientid = HubProcessor.Instance.objPolicy.DefaultClientId.ToStr(),
                            keys = Global.googleKey,
                            MapType = 2,
                            geocodeType = "reverse",
                            originLat = latitude,
                            originLng = longitude,
                            destLat = latitude,
                            destLng = longitude

                        };


                        string json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(obj);
                        //  string API = "https://cabtreasureappapi.co.uk/CabTreasureWebApi/Home/GetHereLocation" + "?json=" + json;
                        string API = "https://www.treasureonlineapi.co.uk/CabTreasureWebApi/Home/GetHereLocation" + "?json=" + json;

                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(API);

                        request.ContentType = "application/json; charset=utf-8";
                        request.Accept = "application/json";
                        request.Method = WebRequestMethods.Http.Post;
                        request.Proxy = null;
                        request.ContentLength = 0;

                        using (WebResponse responsea = request.GetResponse())
                        {

                            StreamReader sr = new StreamReader(responsea.GetResponseStream());
                            locationName = sr.ReadToEnd();

                            sr.Close();
                            sr.Dispose();
                        }

                        string formattedAddress = GetFormattedAddress(locationName.ToStr(), Convert.ToDouble(latitude), Convert.ToDouble(longitude));
                        try
                        {


                            File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetLocationName.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": Lat" + latitude + ",lng:" + longitude + ",location:" + locationName + ",formatted:" + formattedAddress.ToStr() + Environment.NewLine);



                        }
                        catch
                        {


                        }

                        if (formattedAddress.ToStr().Length > 0)
                            locationName = formattedAddress.ToStr().Trim().ToUpper();



                        if (locationName.ToStr().Trim().Length > 0 && locationName.ToStr().Contains(","))
                            locationName = locationName.Replace(",", "").Trim();
                    }
                    catch (Exception ex)
                    {
                        try
                        {

                            File.AppendAllText(AppContext.BaseDirectory + "\\" + "GetLocationName_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ":" + ex.Message + Environment.NewLine);
                        }
                        catch
                        {


                        }
                    }
                }












            }
            catch (Exception ex)
            {


            }


            return locationName;
        }

        public static string GetFormattedAddress(string address, double lat, double lng)
        {

            address = address.ToStr().ToUpper().Trim();



            if (address.ToStr().Length == 0)
                return "";



            try
            {

                string postcode1 = General.GetPostCodeMatch(address);


                try
                {

                    if (postcode1.ToStr().Contains(" ") && postcode1.ToStr().Split(' ')[1][0].ToStr().IsNumeric() == false)
                        postcode1 = postcode1.ToStr().Split(' ')[1];

                }
                catch
                {

                }
                //

                if (postcode1.Contains(" ") == false || postcode1.Split(' ')[1].Length < 3)
                {
                    try
                    {
                        using (TaxiDataContext db = new TaxiDataContext())
                        {
                            postcode1 = address.Substring(address.ToStr().IndexOf(postcode1));
                            string postcode2 = db.ExecuteQuery<string>("exec PostCodesNearLatLongParialFullStreet {0},{1},{2}", Convert.ToDouble(lat), Convert.ToDouble(lng), postcode1).FirstOrDefault();


                            if (postcode2.Length > 0)
                                address = address.Replace(postcode1, postcode2).ToStr().Trim();

                        }
                    }
                    catch
                    {

                    }

                }
            }
            catch
            {


            }
            return address;
        }



        //public static string GetLocationName(double? latitude, double? longitude)
        //{
        //    string locationName = string.Empty;
        //    try
        //    {

        //        //if (Global.googleKey.ToStr().Length == 0)
        //        //{

        //        using (TaxiDataContext db = new TaxiDataContext())
        //        {
        //            db.CommandTimeout = 5;
        //            Global.googleKey = "&key=" + db.ExecuteQuery<string>("select apikey from mapkeys where maptype='google'").FirstOrDefault();


        //        }
        //        //}
        //        // Starts Google Geocoding Webservice

        //        string url2 = string.Empty;
        //        DataTable dt = null;
        //        XmlTextReader reader = null;
        //        System.Data.DataSet ds = null;



        //        if (Global.googleKey.ToStr().Trim().Length > 0)
        //        {
        //            try
        //            {
        //                //
        //                url2 = "https://maps.googleapis.com/maps/api/geocode/xml?latlng=" + latitude + "," + longitude + Global.googleKey + "&sensor=false";

        //                reader = new XmlTextReader(url2);
        //                reader.WhitespaceHandling = WhitespaceHandling.Significant;
        //                ds = new System.Data.DataSet();
        //                ds.ReadXml(reader);

        //                dt = ds.Tables["result"];

        //                if (dt != null && dt.Rows.Count > 0)
        //                {
        //                    //
        //                    DataRow row = dt.Rows.OfType<DataRow>().FirstOrDefault();
        //                    if (row != null)
        //                    {
        //                        locationName = row[1].ToStr().Trim();
        //                        //
        //                        if (locationName.ToStr().ToLower().StartsWith("unnamed") || GetPostCodeMatch(locationName.ToStr().ToUpper()) == string.Empty)
        //                        {

        //                            row = dt.Rows.OfType<DataRow>().Skip(1).FirstOrDefault();

        //                            if (row != null)
        //                            {
        //                                locationName = row[1].ToStr().Trim().ToUpper();



        //                            }

        //                        }
        //                    }
        //                }

        //                ds.Dispose();

        //                try
        //                {

        //                    File.AppendAllText(AppContext.BaseDirectory + "\\" + "googleLocation.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": Lat" + latitude + ",lng:" + longitude + ",location:" + locationName + ",url:" + url2 + Environment.NewLine);
        //                }
        //                catch
        //                {


        //                }


        //            }
        //            catch (Exception ex)
        //            {
        //                try
        //                {

        //                    File.AppendAllText(AppContext.BaseDirectory + "\\" + "exception_google.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ":" + ex.Message + Environment.NewLine);
        //                }
        //                catch
        //                {


        //                }
        //            }
        //        }












        //    }
        //    catch (Exception ex)
        //    {


        //    }


        //    return locationName;
        //}



        public static T GetObject<T>(Expression<Func<T, bool>> condition) where T : class
        {

            return new BLInfo<T, Taxi_Model.TaxiDataContext>()
                     .Get<T>(condition);

        }

        public static IPAddress GetLocalIPAddress()
        {
            IPAddress ipAddress = null;
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipAddress = ip;
                    break;
                }
            }

            return ipAddress;
            // throw new Exception("Local IP Address Not Found!");
        }


        public static void SP_SaveBid(long jobId, int? driverId, decimal bidRate, int? bidStatusId, string driverNo, string message)
        {

            using (Taxi_Model.TaxiDataContext db = new TaxiDataContext())
            {
                try
                {
                    //////
                    db.ExecuteCommand("exec stp_UpdateDriverBid {0},{1},{2},{3},{4},{5}", jobId, driverId, bidRate, bidStatusId, driverNo, message);
                    //    db.stp_UpdateBid(jobId, driverId, bidRate, bidStatusId);
                }
                catch (Exception ex)
                {
                    try
                    {
                        File.AppendAllText(AppContext.BaseDirectory + "\\sp_savebid_exception.txt", DateTime.Now.ToStr() + ": ,exception" + ":" + ex.Message.ToStr() + " , request:" + jobId + "," + driverId + "," + bidRate + "," + bidStatusId + "," + driverNo + "," + message + Environment.NewLine);
                    }
                    catch
                    {

                    }

                }

            }
        }

        public static List<T> GetGeneralList<T>(Expression<Func<T, bool>> condition) where T : class
        {

            return new BLInfo<T, Taxi_Model.TaxiDataContext>()
                     .GetAll<T>(condition).ToList();

        }

        public static IQueryable<T> GetQueryable<T>(Expression<Func<T, bool>> condition) where T : class
        {

            return new BLInfo<T, Taxi_Model.TaxiDataContext>()
                     .GetAll<T>(condition);

        }

        public static decimal GetSimpleFareRate(int companyId, int vehicleTypeId, int tempFromLocId, int tempToLocId, string tempFromPostCode
               , string tempToPostCode, int fromLocTypeId, int toLocTypeId, int? fromZoneId, int? toZoneId, decimal miles, Gen_SysPolicy_Configuration objPolicy)
        {

            decimal rtnFare = 0.00m;
            string fromVal = tempFromPostCode;
            string toVal = tempToPostCode;


            bool surchargeRateFromAmountWise = false;
            bool surchargeRateToAmountWise = false;

            decimal surchargeRateFrom = 0.00m;
            decimal surchargeRateTo = 0.00m;

            // bool IsMoreFareWise = false;
            int actualVehicleTypeId = vehicleTypeId;
            try
            {




                string fromSingleHalfPostCode = string.Empty;
                string fromHalfPostCode = string.Empty;
                string startFromPostCode = "";
                //if (tempFromLocId == 0)
                //{


                if (!string.IsNullOrEmpty(tempFromPostCode))
                {
                    string[] fromArr = tempFromPostCode.Split(new char[] { ' ' });
                    startFromPostCode = fromArr[0];

                    fromHalfPostCode = startFromPostCode;

                    startFromPostCode = General.CheckIfSpecialPostCode(startFromPostCode);

                    if (fromArr.Count() > 1)
                    {
                        fromSingleHalfPostCode = fromArr[0] + " " + fromArr[1][0];

                    }
                    else
                    {
                        fromSingleHalfPostCode = startFromPostCode;

                    }
                }

                //   }


                string ToSingleHalfPostCode = string.Empty;
                string toHalfPostCode = string.Empty;
                string startToPostCode = "";
                //if (tempToLocId == 0)
                //{


                if (!string.IsNullOrEmpty(tempToPostCode))
                {
                    string[] toArr = tempToPostCode.Split(new char[] { ' ' });

                    startToPostCode = toArr[0];
                    toHalfPostCode = startToPostCode;
                    startToPostCode = General.CheckIfSpecialPostCode(startToPostCode);

                    if (toArr.Count() > 1)
                    {
                        ToSingleHalfPostCode = toArr[0] + " " + toArr[1][0];
                    }
                    else
                    {
                        ToSingleHalfPostCode = startToPostCode;

                    }
                }
                //}

                if (tempFromPostCode.Length > 0)
                {
                    tempFromPostCode = General.GetPostCodeMatch(tempFromPostCode);
                    surchargeRateFrom = GetSurchargeRate(tempFromPostCode, ref surchargeRateFromAmountWise);
                }

                if (tempToPostCode.Length > 0)
                {
                    tempToPostCode = General.GetPostCodeMatch(tempToPostCode);
                    surchargeRateTo = GetSurchargeRate(tempToPostCode, ref surchargeRateToAmountWise);
                }

                int defaultVehicleId = objPolicy.DefaultVehicleTypeId.ToInt();
                vehicleTypeId = defaultVehicleId;


                List<Fare_ChargesDetail> list = null;


                if (list == null || (list != null && list.Count() == 0))
                {
                    //int? zoneId = fromZoneId;
                    if (fromZoneId != 0)
                    {

                        list = General.GetQueryable<Fare_ChargesDetail>(c => (((c.FromZoneId == fromZoneId)
                                                                       && ((tempToLocId == 0 && ((c.Gen_Location.LocationTypeId == Enums.LOCATION_TYPES.POSTCODE && (c.Gen_Location.PostCode.Equals(ToSingleHalfPostCode) || c.Gen_Location.PostCode.Equals(toHalfPostCode) || c.Gen_Location.PostCode.Equals(startToPostCode) || c.Gen_Location.PostCode.Equals(tempToPostCode))) || (c.DestinationLocationTypeId == Enums.LOCATION_TYPES.ADDRESS && c.ToAddress.ToLower() == toVal.ToLower()))) || c.DestinationId == tempToLocId))

                                                                          )

                                                                     && c.Fare.VehicleTypeId == vehicleTypeId
                                                                      //&& (c.Fare.CompanyId == companyId || companyId == 0)

                                                                      ).ToList();
                    }
                    else if (toZoneId != 0)
                    {
                        list = General.GetQueryable<Fare_ChargesDetail>(c => ((((tempFromLocId == 0 && ((c.Gen_Location1.LocationTypeId == Enums.LOCATION_TYPES.POSTCODE && (c.Gen_Location1.PostCode.Equals(fromSingleHalfPostCode) || c.Gen_Location1.PostCode.Equals(fromHalfPostCode) || c.Gen_Location1.PostCode.Equals(startFromPostCode) || c.Gen_Location1.PostCode.Equals(tempFromPostCode))) || (c.OriginLocationTypeId == Enums.LOCATION_TYPES.ADDRESS && c.FromAddress.ToLower() == fromVal.ToLower()))) || c.OriginId == tempFromLocId)
                                                               && c.ToZoneId == toZoneId)

                                                                  )

                                                             && c.Fare.VehicleTypeId == vehicleTypeId
                                                              //&& (c.Fare.CompanyId == companyId || companyId == 0)

                                                              ).ToList();

                    }
                }


                if (list == null || (list != null && list.Count() == 0))
                {

                    list = General.GetQueryable<Fare_ChargesDetail>(c => ((((tempFromLocId == 0 && ((c.Gen_Location1.LocationTypeId == Enums.LOCATION_TYPES.POSTCODE && (c.Gen_Location1.PostCode.Equals(fromSingleHalfPostCode) || c.Gen_Location1.PostCode.Equals(fromHalfPostCode) || c.Gen_Location1.PostCode.Equals(startFromPostCode) || c.Gen_Location1.PostCode.Equals(tempFromPostCode))) || (c.OriginLocationTypeId == Enums.LOCATION_TYPES.ADDRESS && c.FromAddress.ToLower() == fromVal.ToLower()))) || c.OriginId == tempFromLocId)
                                                                   && ((tempToLocId == 0 && ((c.Gen_Location.LocationTypeId == Enums.LOCATION_TYPES.POSTCODE && (c.Gen_Location.PostCode.Equals(ToSingleHalfPostCode) || c.Gen_Location.PostCode.Equals(toHalfPostCode) || c.Gen_Location.PostCode.Equals(startToPostCode) || c.Gen_Location.PostCode.Equals(tempToPostCode))) || (c.DestinationLocationTypeId == Enums.LOCATION_TYPES.ADDRESS && c.ToAddress.ToLower() == toVal.ToLower()))) || c.DestinationId == tempToLocId))

                                                                      )

                                                                 && c.Fare.VehicleTypeId == vehicleTypeId
                                                                  //&& (c.Fare.CompanyId == companyId || companyId == 0)

                                                                  ).ToList();

                }

                if (list == null || (list != null && list.Count() == 0))
                {
                    list = General.GetQueryable<Fare_ChargesDetail>(c => ((((tempFromLocId == 0 && c.Gen_Location1.LocationTypeId == Enums.LOCATION_TYPES.POSTCODE && (c.Gen_Location1.PostCode.Equals(fromSingleHalfPostCode) || c.Gen_Location1.PostCode.Equals(fromHalfPostCode) || c.Gen_Location1.PostCode.Equals(startFromPostCode) || c.Gen_Location1.PostCode.Equals(tempFromPostCode))) || c.OriginId == tempFromLocId)
                                                                && ((tempToLocId == 0 && c.Gen_Location.LocationTypeId == Enums.LOCATION_TYPES.POSTCODE && (c.Gen_Location.PostCode.Equals(ToSingleHalfPostCode) || c.Gen_Location.PostCode.Equals(toHalfPostCode) || c.Gen_Location.PostCode.Equals(startToPostCode) || c.Gen_Location.PostCode.Equals(tempToPostCode))) || c.DestinationId == tempToLocId))

                                                                || (((tempToLocId == 0 && c.Gen_Location1.LocationTypeId == Enums.LOCATION_TYPES.POSTCODE && (c.Gen_Location1.PostCode.Equals(ToSingleHalfPostCode) || c.Gen_Location1.PostCode.Equals(toHalfPostCode) || c.Gen_Location1.PostCode.Equals(startToPostCode) || c.Gen_Location1.PostCode.Equals(tempToPostCode))) || c.OriginId == tempToLocId)
                                                                && ((tempFromLocId == 0 && c.Gen_Location.LocationTypeId == Enums.LOCATION_TYPES.POSTCODE && (c.Gen_Location.PostCode.Equals(fromSingleHalfPostCode) || c.Gen_Location.PostCode.Equals(fromHalfPostCode) || c.Gen_Location.PostCode.Equals(startFromPostCode) || c.Gen_Location.PostCode.Equals(tempFromPostCode))) || c.DestinationId == tempFromLocId))
                                                                   )

                                                              && c.Fare.VehicleTypeId == vehicleTypeId
                                                               // && (c.Fare.CompanyId == companyId || companyId == 0)

                                                               ).ToList();


                    if (list != null && list.Count > 0)
                    {
                        //   errorMsg = "Reverse found";

                    }

                }

                if ((tempFromLocId != 0 || tempToLocId != 0) && (list == null || (list != null && list.Count() == 0)))
                {
                    if (tempFromLocId > 0)
                    {
                        list = General.GetQueryable<Fare_ChargesDetail>(c => ((((c.Gen_Location1.LocationTypeId == Enums.LOCATION_TYPES.POSTCODE && (c.Gen_Location1.PostCode.Equals(fromSingleHalfPostCode) || c.Gen_Location1.PostCode.Equals(fromHalfPostCode) || c.Gen_Location1.PostCode.Equals(startFromPostCode) || c.Gen_Location1.PostCode.Equals(tempFromPostCode))))
                                                               && ((tempToLocId == 0 && c.Gen_Location.LocationTypeId == Enums.LOCATION_TYPES.POSTCODE && (c.Gen_Location.PostCode.Equals(ToSingleHalfPostCode) || c.Gen_Location.PostCode.Equals(toHalfPostCode) || c.Gen_Location.PostCode.Equals(startToPostCode) || c.Gen_Location.PostCode.Equals(tempToPostCode))) || c.DestinationId == tempToLocId))

                                                               || (((tempToLocId == 0 && c.Gen_Location1.LocationTypeId == Enums.LOCATION_TYPES.POSTCODE && (c.Gen_Location1.PostCode.Equals(ToSingleHalfPostCode) || c.Gen_Location1.PostCode.Equals(toHalfPostCode) || c.Gen_Location1.PostCode.Equals(startToPostCode) || c.Gen_Location1.PostCode.Equals(tempToPostCode))) || c.OriginId == tempToLocId)
                                                               && ((c.Gen_Location.LocationTypeId == Enums.LOCATION_TYPES.POSTCODE && (c.Gen_Location.PostCode.Equals(fromSingleHalfPostCode) || c.Gen_Location.PostCode.Equals(fromHalfPostCode) || c.Gen_Location.PostCode.Equals(startFromPostCode) || c.Gen_Location.PostCode.Equals(tempFromPostCode)))))
                                                                  )

                                                             && c.Fare.VehicleTypeId == vehicleTypeId
                                                              // && (c.Fare.CompanyId == companyId || companyId == 0)

                                                              ).ToList();

                    }

                    if ((list == null || list.Count == 0) && tempToLocId > 0)
                    {
                        list = General.GetQueryable<Fare_ChargesDetail>(c => ((((tempFromLocId == 0 && c.Gen_Location1.LocationTypeId == Enums.LOCATION_TYPES.POSTCODE && (c.Gen_Location1.PostCode.Equals(fromSingleHalfPostCode) || c.Gen_Location1.PostCode.Equals(fromHalfPostCode) || c.Gen_Location1.PostCode.Equals(startFromPostCode) || c.Gen_Location1.PostCode.Equals(tempFromPostCode))) || c.OriginId == tempFromLocId)
                                                                && ((c.Gen_Location.LocationTypeId == Enums.LOCATION_TYPES.POSTCODE && (c.Gen_Location.PostCode.Equals(ToSingleHalfPostCode) || c.Gen_Location.PostCode.Equals(toHalfPostCode) || c.Gen_Location.PostCode.Equals(startToPostCode) || c.Gen_Location.PostCode.Equals(tempToPostCode)))))

                                                                || (((c.Gen_Location1.LocationTypeId == Enums.LOCATION_TYPES.POSTCODE && (c.Gen_Location1.PostCode.Equals(ToSingleHalfPostCode) || c.Gen_Location1.PostCode.Equals(toHalfPostCode) || c.Gen_Location1.PostCode.Equals(startToPostCode) || c.Gen_Location1.PostCode.Equals(tempToPostCode))))
                                                                && ((tempFromLocId == 0 && c.Gen_Location.LocationTypeId == Enums.LOCATION_TYPES.POSTCODE && (c.Gen_Location.PostCode.Equals(fromSingleHalfPostCode) || c.Gen_Location.PostCode.Equals(fromHalfPostCode) || c.Gen_Location.PostCode.Equals(startFromPostCode) || c.Gen_Location.PostCode.Equals(tempFromPostCode))) || c.DestinationId == tempFromLocId))
                                                                   )

                                                              && c.Fare.VehicleTypeId == vehicleTypeId
                                                               // && (c.Fare.CompanyId == companyId || companyId == 0)

                                                               ).ToList();

                    }



                    if ((list == null || list.Count == 0))
                    {
                        list = General.GetQueryable<Fare_ChargesDetail>(c => ((((c.Gen_Location1.LocationTypeId == Enums.LOCATION_TYPES.POSTCODE && (c.Gen_Location1.PostCode.Equals(fromSingleHalfPostCode) || c.Gen_Location1.PostCode.Equals(fromHalfPostCode) || c.Gen_Location1.PostCode.Equals(startFromPostCode) || c.Gen_Location1.PostCode.Equals(tempFromPostCode))))
                                                                && ((c.Gen_Location.LocationTypeId == Enums.LOCATION_TYPES.POSTCODE && (c.Gen_Location.PostCode.Equals(ToSingleHalfPostCode) || c.Gen_Location.PostCode.Equals(toHalfPostCode) || c.Gen_Location.PostCode.Equals(startToPostCode) || c.Gen_Location.PostCode.Equals(tempToPostCode)))))

                                                                || (((c.Gen_Location1.LocationTypeId == Enums.LOCATION_TYPES.POSTCODE && (c.Gen_Location1.PostCode.Equals(ToSingleHalfPostCode) || c.Gen_Location1.PostCode.Equals(toHalfPostCode) || c.Gen_Location1.PostCode.Equals(startToPostCode) || c.Gen_Location1.PostCode.Equals(tempToPostCode))))
                                                                && ((c.Gen_Location.LocationTypeId == Enums.LOCATION_TYPES.POSTCODE && (c.Gen_Location.PostCode.Equals(fromSingleHalfPostCode) || c.Gen_Location.PostCode.Equals(fromHalfPostCode) || c.Gen_Location.PostCode.Equals(startFromPostCode) || c.Gen_Location.PostCode.Equals(tempFromPostCode)))))
                                                                   )

                                                              && c.Fare.VehicleTypeId == vehicleTypeId
                                                               // && (c.Fare.CompanyId == companyId || companyId == 0)

                                                               ).ToList();

                    }

                    if (list != null && list.Count > 0)
                    {
                        //   errorMsg = "Reverse found";

                    }


                }



                //if ((tempFromLocId == 0 && string.IsNullOrEmpty(startFromPostCode)) || (tempToLocId == 0 && string.IsNullOrEmpty(startToPostCode)))
                //    obj = null;


                if (objPolicy.AddFareCalculationType.ToInt() == 2)
                {
                    tempFromPostCode = fromVal;
                    tempToPostCode = toVal;
                }


                // decimal  miles = CalculateOfflineDistance(tempFromPostCode, tempToPostCode);



                //  milesList.Add(miles.ToDecimal());


                Fare_ChargesDetail obj = null;

                if (list != null)
                {
                    if (companyId != 0)
                    {
                        if (list.Count(c => c.Fare.CompanyId == companyId) > 0)
                        {
                            obj = list.FirstOrDefault(c => c.Fare.CompanyId == companyId);

                            //  companyFareExist = true;
                        }
                        else
                        {

                            if (General.GetQueryable<Taxi_Model.Fare>(c => c.CompanyId == companyId).Count() == 0)
                            {
                                obj = list.FirstOrDefault(c => c.Fare.CompanyId == null);
                                //    companyFareExist = true;

                            }


                        }
                    }
                    else
                    {
                        obj = list.FirstOrDefault(c => c.Fare.CompanyId == null);
                    }

                }


                if (obj != null)
                {

                    rtnFare = obj.Rate.ToDecimal();
                    //   deadMileage = 0;
                }
                else
                {


                    // Calculate Fare Mileage Wise                
                    //  ISingleResult<ClsFares> objFare = General.SP_CalculateFares(vehicleTypeId.ToIntorNull(), companyId.ToIntorNull(), milesList.Sum().ToStr(), pickupTime);
                    decimal totalMiles = miles;

                    var objFare = new TaxiDataContext().stp_CalculateGeneralFares(vehicleTypeId, companyId, totalMiles, DateTime.Now);

                    if (objFare != null)
                    {
                        var f = objFare.FirstOrDefault();

                        if (f.Result == "Success" || f.Result.ToStr().IsNumeric())
                        {
                            rtnFare = f.totalFares.ToDecimal();

                            //    companyFareExist = f.CompanyFareExist.ToBool();
                        }
                        //else
                        //    errorMsg = "Error";
                    }
                    //else
                    //    errorMsg = "Error";




                    if (objPolicy.RoundMileageFares.ToBool())
                    {

                        decimal startRateTillMiles = General.GetObject<Fleet_VehicleType>(c => c.Id == vehicleTypeId).DefaultIfEmpty().StartRateValidMiles.ToDecimal();
                        if (startRateTillMiles > 0 && totalMiles > startRateTillMiles)
                        {

                            //  rtnFare = Math.Ceiling((rtnFare);
                            rtnFare = Math.Ceiling(rtnFare);
                        }
                    }
                    else
                    {
                        //rtnFare = (decimal)Math.Ceiling(rtnFare / 0.5m) * 0.5m;


                        decimal roundUp = objPolicy.RoundUpTo.ToDecimal();

                        if (roundUp > 0)
                        {
                            // fareVal = (decimal)Math.Ceiling(fareVal / roundUp) * roundUp;

                            rtnFare = (decimal)Math.Ceiling(rtnFare / roundUp) * roundUp;

                        }

                    }

                }

                if (surchargeRateFromAmountWise == false && surchargeRateToAmountWise == false)
                {

                    decimal totalSurchargePercentage = surchargeRateFrom + surchargeRateTo;

                    decimal fareSurchargePercent = (rtnFare * totalSurchargePercentage) / 100;
                    rtnFare = rtnFare + fareSurchargePercent;

                }
                else if (surchargeRateFromAmountWise == true && surchargeRateToAmountWise == true)
                {

                    rtnFare = rtnFare + surchargeRateFrom + surchargeRateTo;
                }
                else if (surchargeRateFromAmountWise == true && surchargeRateToAmountWise == false)
                {
                    surchargeRateTo = (rtnFare * surchargeRateTo) / 100;

                    rtnFare = rtnFare + surchargeRateFrom + surchargeRateTo;
                }
                else if (surchargeRateFromAmountWise == false && surchargeRateToAmountWise == true)
                {
                    surchargeRateFrom = (rtnFare * surchargeRateFrom) / 100;

                    rtnFare = rtnFare + surchargeRateFrom + surchargeRateTo;
                }




            }
            catch (Exception ex)
            {


                //   MessageBox.Show(ex.Message);
            }
            return rtnFare;
        }

        public static decimal GetSurchargeRate(string postCode, ref bool IsAmountWise)
        {
            decimal value = 0.00m;
            string[] splitPostCode = postCode.Split(new char[] { ' ' });
            if (splitPostCode.Count() > 0)
            {
                string postcode = CheckIfSpecialPostCode(splitPostCode[0].Trim().ToUpper());

                Gen_SysPolicy_SurchargeRate obj = General.GetObject<Gen_SysPolicy_SurchargeRate>(c => c.SysPolicyId != null && c.PostCode.Trim().ToLower() == postcode.ToLower());

                if (obj != null)
                {

                    IsAmountWise = obj.IsAmountWise.ToBool();


                    if (IsAmountWise)
                    {
                        value = obj.Amount.ToDecimal();
                    }
                    else
                    {


                        value = obj.Percentage.ToDecimal();
                    }

                }
            }

            return value;

        }

        public static string RemoveUK(ref string address)
        {


            if (address.ToUpper().EndsWith(", UK"))
            {
                address = address.Remove(address.ToUpper().LastIndexOf(", UK"));
            }

            return address;
        }

        public static string GetPostCodeMatch(string value)
        {
            string postCode = "";

            try
            {
                RemoveUK(ref value);

                if (value.ToStr().Contains(","))
                {
                    value = value.Replace(",", "").Trim();
                }

                if (value.ToStr().Contains(" "))
                {
                    value = value.Replace(" ", " ").Trim();
                }




                //   string ukAddress = @"[[:alnum:]][a-zA-Z0-9_\.\#\' \-]{2,60}$";
                string ukAddress = @"^(GIR 0AA)|((([A-PR-UWYZ][0-9][0-9]?)|(([A-PR-UWYZ][A-HK-Y][0-9][0-9]?)|(([A-PR-UWYZ][0-9][A-HJKSTUW])|([A-PR-UWYZ][A-HK-Y][0-9][ABEHMNPRVWXY])))) [0-9][A-BD-HJLNP-UW-Z]{2})$";
                // string ukAddress = @"^(GIR 0AA|[A-PR-UWYZ]([0-9]{1,2}| ([A-HK-Y][0-9]|[A-HK-Y][0-9]([0-9]| [ABEHMNPRV-Y]))|[0-9][A-HJKPS-UW]) [0-9][ABD-HJLNP-UW-Z]{2})$";


                Regex reg = new Regex(ukAddress);
                Match em = reg.Match(value);

                if (em != null)
                    postCode = em.Value;

                if (em.Value == "")
                {
                    ukAddress = @"[A-Z]{1,2}[0-9R][0-9A-Z]?";
                    reg = new Regex(ukAddress);
                    MatchCollection mat = reg.Matches(value);


                    foreach (Match item in mat)
                    {
                        if (item.Value.ToStr().IsAlpha() == false)
                            postCode += item.Value.ToStr() + " ";

                    }

                    // postCode = em.Value;

                }

            }
            catch
            {

            }

            return postCode.Trim();

        }

        public static string CheckIfSpecialPostCode(string postcode)
        {
            try
            {

                if (((postcode.StartsWith("EC") || postcode.StartsWith("WC") || postcode.StartsWith("SE1") ||
                          postcode.StartsWith("SW1")) && postcode.Length == 4) || postcode.StartsWith("W1") && postcode.Length == 3)
                {

                    if (char.IsLetter(postcode[postcode.Length - 1]))
                    {
                        postcode = postcode.Remove(postcode.Length - 1);
                    }
                }
            }
            catch
            {


            }

            return postcode;

        }

        public static decimal CalculateDistance(string origin, string destination)
        {
            decimal miles = 0.00m;

            try
            {


                origin = GetPostCodeMatch(origin);
                destination = GetPostCodeMatch(destination);

                string url2 = "http://maps.googleapis.com/maps/api/directions/xml?origin=" + origin + ", UK" + "&destination=" + destination + ", UK&sensor=false";



                XmlTextReader reader = new XmlTextReader(url2);
                reader.WhitespaceHandling = WhitespaceHandling.Significant;
                System.Data.DataSet ds = new System.Data.DataSet();
                ds.ReadXml(reader);
                DataTable dt = ds.Tables["distance"];
                if (dt != null)
                {

                    decimal distanceKm = dt.Rows.OfType<DataRow>().Where(c => c[1].ToStr().Contains("km")).Sum(c => c[1].ToStr().Strip("km").Trim().ToDecimal()).ToDecimal() / 2;
                    decimal distanceMeter = dt.Rows.OfType<DataRow>().Where(c => c[1].ToStr().Contains(" m")).Sum(c => c[1].ToStr().Strip("m").Trim().ToDecimal()).ToDecimal() / 2;

                    decimal milKM = 0.621m;
                    decimal milMeter = 0.00062137119m;

                    miles = (milKM * distanceKm) + (milMeter * distanceMeter);

                }

            }
            catch
            {



            }

            return miles;
        }

        public static string GetKey()
        {
            string googleKey = "";

            using (TaxiDataContext dbX = new TaxiDataContext())
            {
                dbX.CommandTimeout = 5;
                googleKey = "&key=" + dbX.ExecuteQuery<string>("select apikey from mapkeys where maptype='google'").FirstOrDefault();


            }
            return googleKey;
        }

        public static decimal CalculateDistanceFromAPI(string pickup, string destination, List<ViaAddresses> viaAddresses = null)
        {
            decimal miles = 0.00m;

            try
            {
                RequestWebApi obj = new RequestWebApi();
                obj.routeInfo = new RouteInfo();
                using (TaxiDataContext db = new TaxiDataContext())
                {
                    var coord = db.stp_getCoordinatesByAddress(pickup, General.GetPostCodeMatch(pickup)).FirstOrDefault();
                    if (coord != null)
                    {
                        obj.routeInfo.pickupAddress = new AddressInfo();
                        obj.routeInfo.pickupAddress.Latitude = coord.Latitude;
                        obj.routeInfo.pickupAddress.Longitude = coord.Longtiude;
                    }
                    coord = db.stp_getCoordinatesByAddress(destination, General.GetPostCodeMatch(destination)).FirstOrDefault();
                    if (coord != null)
                    {
                        obj.routeInfo.destinationAddress = new AddressInfo();
                        obj.routeInfo.destinationAddress.Latitude = coord.Latitude;
                        obj.routeInfo.destinationAddress.Longitude = coord.Longtiude;
                    }

                    string vias = "";
                    if (viaAddresses != null)
                    {

                        foreach (var item in viaAddresses)
                        {
                            coord = db.stp_getCoordinatesByAddress(item.Viaaddress, General.GetPostCodeMatch(item.Viaaddress)).FirstOrDefault();
                            if (coord != null && coord.Latitude != 0)
                            {
                                vias += coord.Latitude + "," + coord.Longtiude + "|";
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(item.ViaCoordinates))
                                {
                                    vias += item.ViaCoordinates + "|";
                                }
                            }

                        }
                        vias = vias.Remove(vias.Length - 1, 1);
                    }

                    string KEY = "avsHjHri-tP5Su5wV7xyPBWwmdqOtEKK2Atn0xgDnrM";


                    if (HubProcessor.Instance.objPolicy.MapType.ToInt() == 1)
                        KEY = db.ExecuteQuery<string>("select APIKey from mapkeys where maptype='google'").FirstOrDefault().ToStr().Trim();

                    var objX = new
                    {
                        originLat = Convert.ToDouble(obj.routeInfo.pickupAddress.Latitude),
                        originLng = Convert.ToDouble(obj.routeInfo.pickupAddress.Longitude),
                        destLat = Convert.ToDouble(obj.routeInfo.destinationAddress.Latitude),
                        destLng = Convert.ToDouble(obj.routeInfo.destinationAddress.Longitude),
                        defaultclientid = HubProcessor.Instance.objPolicy.DefaultClientId.ToStr(),
                        keys = KEY,
                        MapType = HubProcessor.Instance.objPolicy.MapType.ToInt(),
                        sourceType = "hubapi",
                        routeType = "short",
                        vias = vias
                        //vias = obj.routeInfo.viaAddresses.Select(args => new {Via=args.Latitude +","+args.Longitude })
                    };


                    string json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(objX);
                    string API = "https://www.treasureonlineapi.co.uk/CabTreasureWebApi/Home/GetRouteDetails" + "?json=" + json;


                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(API);
                    request.ContentType = "application/json; charset=utf-8";
                    request.Accept = "application/json";
                    request.Method = WebRequestMethods.Http.Post;
                    request.Proxy = null;
                    request.ContentLength = 0;

                    ResponseWebApi response = new ResponseWebApi();

                    using (WebResponse responsea = request.GetResponse())
                    {

                        using (StreamReader sr = new StreamReader(responsea.GetResponseStream()))
                        {
                            response.Data = sr.ReadToEnd();
                        }
                    }



                    RouteCoordinates route = Newtonsoft.Json.JsonConvert.DeserializeObject<RouteCoordinates>(response.Data.ToStr());

                    obj.routeInfo.Distance = route.Distance;
                    miles = route.Distance;
                    //if (pickup.ToStr().Trim().Length > 0 && destination.ToStr().Trim().Length > 0)
                    //    route.fareModel = CalculateFaresFromAPI(obj);
                }
            }
            catch (Exception ex)
            {



            }

            return miles;
        }
    }
}
