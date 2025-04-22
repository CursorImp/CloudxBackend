using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SignalRHub
{
    public class JobLateSchedular
    {

        public long JobId;
        public string MobileNo;

    }


    public class FCMPushNotification
    {
        public FCMPushNotification()
        {
            // TODO: Add constructor logic here  
        }
        public bool Successful
        {
            get;
            set;
        }
        public string Response
        {
            get;
            set;
        }
        public Exception Error
        {
            get;
            set;
        }

        string serverKey = string.Empty;
        string senderId = string.Empty;
    }
    public class CustomJsonResult : JsonResult
    {
        private const string _dateFormat = "yyyy-MM-dd HH:mm:ss";

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            HttpResponseBase response = context.HttpContext.Response;

            if (!String.IsNullOrEmpty(ContentType))
            {
                response.ContentType = ContentType;
            }
            else
            {
                response.ContentType = "application/json";
            }
            if (ContentEncoding != null)
            {
                response.ContentEncoding = ContentEncoding;
            }
            if (Data != null)
            {
                // Using Json.NET serializer
                var isoConvert = new IsoDateTimeConverter();
                isoConvert.DateTimeFormat = _dateFormat;
                response.Write(JsonConvert.SerializeObject(Data, isoConvert));
            }
        }
    }


    public class ClsDriverInfo
    {
        public int Id;
        public string DriverNo;
        public string DriverName;
        public string MobileNo;
    }



    public class ManualClearJob
    {

        public long QueueId;
        public long JobId;
        public string ClearBy;

    }

    public class eMessageTypes
    {
        public static int JOB = 1;
        public static int RECALLJOB = 2;
        public static int CLEAREDJOB = 3;
        public static int MESSAGING = 4;
        public static int AUTHORIZATION = 5;
        public static int BIDALERT = 6;
        public static int UPDATEPLOT = 7;
        public static int UPDATEJOB = 8;
        public static int ONBIDDESPATCH = 9;
        public static int LOGOUTAUTHORIZATION = 10;
        public static int FORCE_ACTION_BUTTON = 11;
        public static int UPDATE_SETTINGS = 12;
        public static int BIDPRICEALERT = 13;
        public static int PLANJOB = 14;

        public static int STC_ALLOCATED = 15;
        public static int TRIP = 16;
        public static int PAYMENTRESPONSE = 16;
    }

    public class AppUser
    {

        private string _JobPromotion;

        public string JobPromotion
        {
            get { return _JobPromotion; }
            set { _JobPromotion = value; }
        }




        private string _CustomerId;

        public string CustomerId
        {
            get { return _CustomerId; }
            set { _CustomerId = value; }
        }



        private string _PickDetails;





        public string PickDetails
        {
            get { return _PickDetails; }
            set { _PickDetails = value; }
        }

        private string _Code;

        public string Code
        {
            get { return _Code; }
            set { _Code = value; }
        }


        private string _PhoneNo;

        public string PhoneNo
        {
            get { return _PhoneNo; }
            set { _PhoneNo = value; }
        }
        private string _UniqueId;

        public string UniqueId
        {
            get { return _UniqueId; }
            set { _UniqueId = value; }
        }
        private string _DeviceInfo;

        public string DeviceInfo
        {
            get { return _DeviceInfo; }
            set { _DeviceInfo = value; }
        }
        private string _UserName;

        public string UserName
        {
            get { return _UserName; }
            set { _UserName = value; }
        }
        private string _Email;

        public string Email
        {
            get { return _Email; }
            set { _Email = value; }
        }
        private string _Passwrd;

        public string Passwrd
        {
            get { return _Passwrd; }
            set { _Passwrd = value; }
        }
        private string _SendSMS;

        public string SendSMS
        {
            get { return _SendSMS; }
            set { _SendSMS = value; }
        }

        private string _Address;

        public string Address
        {
            get { return _Address; }
            set { _Address = value; }
        }
        private string _Telephone;

        public string Telephone
        {
            get { return _Telephone; }
            set { _Telephone = value; }
        }




    }

}