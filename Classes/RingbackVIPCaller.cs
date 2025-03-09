using SignalRHub;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Utils;
public class RingbackVIPCaller
{
    public string APIAddress = "https://portal.vipvoip.co.uk/vipVoipAPI/makeCall";

    /// <summary>
    /// Copy this methord where you want to test.
    /// </summary>
    public static void TestMethod()
    {
      
    
    }



    public string RingBackEmerald(RingbackVIPCallerRequest requestData)
    {

        try
        {
            //  string APIBaseURL = "http://eurosoftlines.co.uk/misscall.php";

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string[] arr = requestData.token.ToStr().Trim().Split(new char[] { ',' });


            string userAuth = arr[0].ToStr();
            string pasAuth = arr[1].ToStr().Replace("-321", "").Trim();


            string number = requestData.destination.ToStr().Trim();

            string APIBaseURL = "https://portal.emeraldvoip.com/config.php?client=" + userAuth + "&password=" + pasAuth.Replace("#", "%23").Replace("@", "%40") + "&number=" + number + "&hash=ringback";

            try
            {
                File.AppendAllText(AppContext.BaseDirectory + "\\ringbacklog.txt", "datastring:" + APIBaseURL + DateTime.Now.ToStr());

            }
            catch
            {


            }

            string rtn = string.Empty;
            using (WebClient wc = new WebClient())
            {

                wc.Proxy = null;
                rtn = wc.DownloadString(new System.Uri(APIBaseURL));


            }

            if (rtn.ToStr().ToLower().Contains("success"))
            {
                rtn = "success:RingBack Delivered";

            }
            else
            {

                rtn = "failed:RingBack Delivery Failed";
            }

            //string number = requestData.destination.ToStr().Trim();

            //WebMethods webService = new WebMethods(APIBaseURL);
            //webService.PreInvoke();

            //webService.AddParameter("userAuth", userAuth);
            //webService.AddParameter("pasAuth", pasAuth);
            //webService.AddParameter("number", number);

            //try
            //{
            //    webService.Invoke("playbackArrival", "urn:missCallStats");
            //}
            //finally { webService.PosInvoke(); }

            //string rtn = webService.ResultString;

            //if (rtn.ToStr().ToLower().Contains("<playback>"))
            //{
            //    rtn = "success:RingBack Delivered";

            //}
            //else
            //{

            //    rtn = "failed:RingBack Delivery Failed";
            //}

            return rtn;


        }
        catch (Exception exe)
        {

            return "failed:RingBack Delivery Failed";
        }





    }


    //public string RingBackEmerald(RingbackVIPCallerRequest requestData)
    //{

    //    try
    //    {
    //        //  string APIBaseURL = "http://eurosoftlines.co.uk/misscall.php";

    //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
    //        string[] arr = requestData.token.ToStr().Trim().Split(new char[] { ',' });


    //        string userAuth = arr[0].ToStr();
    //        string pasAuth = arr[1].ToStr();


    //        string number = requestData.destination.ToStr().Trim();

    //        string APIBaseURL = "https://portal.emeraldvoip.com/config.php?client=" + userAuth + "&password=" + pasAuth.Replace("#", "%23").Replace("@", "%40") + "&number=" + number + "&hash=ringback";

    //        try
    //        {
    //            File.AppendAllText(AppContext.BaseDirectory + "\\ringbacklog.txt", "datastring:" + APIBaseURL + DateTime.Now.ToStr());

    //        }
    //        catch
    //        {


    //        }

    //        string rtn = string.Empty;
    //        using (WebClient wc = new WebClient())
    //        {

    //            wc.Proxy = null;
    //            rtn = wc.DownloadString(new System.Uri(APIBaseURL));


    //        }

    //        if (rtn.ToStr().ToLower().Contains("success"))
    //        {
    //            rtn = "success:RingBack Delivered";

    //        }
    //        else
    //        {

    //            rtn = "failed:RingBack Delivery Failed";
    //        }

    //        //string number = requestData.destination.ToStr().Trim();

    //        //WebMethods webService = new WebMethods(APIBaseURL);
    //        //webService.PreInvoke();

    //        //webService.AddParameter("userAuth", userAuth);
    //        //webService.AddParameter("pasAuth", pasAuth);
    //        //webService.AddParameter("number", number);

    //        //try
    //        //{
    //        //    webService.Invoke("playbackArrival", "urn:missCallStats");
    //        //}
    //        //finally { webService.PosInvoke(); }

    //        //string rtn = webService.ResultString;

    //        //if (rtn.ToStr().ToLower().Contains("<playback>"))
    //        //{
    //        //    rtn = "success:RingBack Delivered";

    //        //}
    //        //else
    //        //{

    //        //    rtn = "failed:RingBack Delivery Failed";
    //        //}

    //        return rtn;


    //    }
    //    catch (Exception exe)
    //    {

    //        return "failed:RingBack Delivery Failed";
    //    }





    //}

    //public string RingBackEmerald(RingbackVIPCallerRequest requestData)
    //{

    //    try
    //    {
    //      //  string APIBaseURL = "http://eurosoftlines.co.uk/misscall.php";


    //        string[] arr = requestData.token.ToStr().Trim().Split(new char[] { ',' });


    //        string userAuth = arr[0].ToStr();
    //        string pasAuth = arr[1].ToStr();


    //        string number = requestData.destination.ToStr().Trim();

    //        string APIBaseURL = "http://emeraldtel.co.uk/include.php?client=" + userAuth + "&password=" + pasAuth + "&customer_number=" + number + "&value=wallboardlogs&method=ringback";
    //        string rtn = string.Empty;
    //        using (WebClient wc = new WebClient())
    //        {

    //            wc.Proxy = null;
    //            rtn = wc.DownloadString(new System.Uri(APIBaseURL));


    //        }

    //        if (rtn.ToStr().ToLower().Contains("success"))
    //        {
    //            rtn = "success:RingBack Delivered";

    //        }
    //        else
    //        {

    //            rtn = "failed:RingBack Delivery Failed";
    //        }

    //        //string number = requestData.destination.ToStr().Trim();

    //        //WebMethods webService = new WebMethods(APIBaseURL);
    //        //webService.PreInvoke();

    //        //webService.AddParameter("userAuth", userAuth);
    //        //webService.AddParameter("pasAuth", pasAuth);
    //        //webService.AddParameter("number", number);

    //        //try
    //        //{
    //        //    webService.Invoke("playbackArrival", "urn:missCallStats");
    //        //}
    //        //finally { webService.PosInvoke(); }

    //        //string rtn = webService.ResultString;

    //        //if (rtn.ToStr().ToLower().Contains("<playback>"))
    //        //{
    //        //    rtn = "success:RingBack Delivered";

    //        //}
    //        //else
    //        //{

    //        //    rtn = "failed:RingBack Delivery Failed";
    //        //}

    //        return rtn;


    //    }
    //    catch (Exception exe)
    //    {

    //        return "failed:RingBack Delivery Failed";
    //    }





    //}

    public string RingbackYestech(RingbackVIPCallerRequest requestData)
    {
        string rtn = "";
        try
        {

            // Create a request using a URL that can receive a post.
            WebRequest request = WebRequest.Create(APIAddress);
            // Set the Method property of the request to POST.  
            request.Method = "POST";
            // Create POST data and convert it to a byte array.  

            string postData = GetQueryString(requestData);

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            // Set the ContentType property of the WebRequest.  
            request.ContentType = "application/x-www-form-urlencoded";
            // Set the ContentLength property of the WebRequest.  
            request.ContentLength = byteArray.Length;
            // Get the request stream.  
            Stream dataStream = request.GetRequestStream();
            // Write the data to the request stream.  
            dataStream.Write(byteArray, 0, byteArray.Length);
            // Close the Stream object.  
            dataStream.Close();
            // Get the response.  
            WebResponse response = request.GetResponse();
            // Display the status.  
            rtn=((HttpWebResponse)response).StatusDescription;
            // Get the stream containing content returned by the server.  
            dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.  
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.  
            string responseFromServer = reader.ReadToEnd();
            // Display the content.  
           // Console.WriteLine(responseFromServer);
            // Clean up the streams.  
            reader.Close();
            dataStream.Close();
            response.Close();

            if (rtn.ToStr().ToLower() == "ok")
                rtn = "success:RingBack Delivered";
           
        }
        catch (Exception exe)
        {

            rtn = "failed:RingBack Delivery Failed";

        }

        return rtn;

    }
    private string GetQueryString(object obj)
    {
        var properties = from p in obj.GetType().GetProperties()
                         where p.GetValue(obj, null) != null
                         select p.Name + "=" + (p.GetValue(obj, null).ToString());

        return String.Join("&", properties.ToArray());
    }

}

public class RingbackVIPCallerRequest
{
    /// <summary>
    /// Your API token.
    /// </summary>
    public string token { get; set; }
    /// <summary>
    /// Destination phone number.
    /// </summary>
    public string destination { get; set; }

    /// <summary>
    /// A silent message player.
    /// </summary>
    public string extension { get; set; }

    /// <summary>
    /// The caller ID you want presented to the dialled number.
    /// </summary>
    public string callerId { get; set; }

    /// <summary>
    /// Indicates the call should be cut off as soon as the destination starts ringing.
    /// </summary>
    public bool ringback { get; set; }
}

