using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace GettAPICall
{


    public class ClsSupplierData
    {

        public long JobId { get; set; }
        public int BookingStatusId { get; set; }
        public string UserName { get; set; }
        public string Reason { get; set; }
        public int DriverId { get; set; }


        public string Status { get; set; }

    }

    public class Apicalling
    {

        string BASE_URL = "https://api-eurosofttech.co.uk/CT-Supplier-API/";
      //  string BASE_URL = "https://api-eurosofttech.co.uk/Sandbox-Supplierapi/";
        public Response<string> BookingstatusUpdate(string json)
        {
            Response<string> response = new Response<string>();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            UpdatebookingstatustoSupplier updatebookingstatustoSupplier = new UpdatebookingstatustoSupplier();
            updatebookingstatustoSupplier =new  System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<UpdatebookingstatustoSupplier>(json);

            try
            {
                ////
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(BASE_URL);
                   // client.DefaultRequestHeaders.Authorization =
                    //new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($"{CLIENT_ID}:{CLIENT_SECRET}")));
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
                    var stringContent = new StringContent(new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(updatebookingstatustoSupplier), Encoding.UTF8, "application/json");
                    var postTask =  client.PostAsync(BASE_URL + "api/fleetbooking/SupplierBookingStatusUpdate", stringContent).Result;
                    var readTask = postTask.Content.ReadAsStringAsync().Result;
                 //   var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
                    response = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Response<string>>(readTask);

                }

            }
            catch (Exception exe)
            {
                response.HasError = true;
                response.ResponseCode = ResponseCodes.DataError;
                response.message = exe.Message;
                response.bookingId = long.Parse(updatebookingstatustoSupplier.ctbookingid);
            }
            return response;
        }

    }

    #region --Responseclass--
    public class Response<T>
    {
       
        private string _message = "Data Successfully Process.";
        private long _bookingId;
        private bool _status = false;
        private ResponseCodes _responseCode;

        public Response()
        {
        }



        public string message
        {
            get { return _message; }
            set { _message = value; }
        }


        public long bookingId
        {
            get { return _bookingId; }
            set { _bookingId = value; }
        }

        public bool HasError
        {
            get { return _status; }
            set { _status = value; }
        }

        public ResponseCodes ResponseCode
        {
            get { return _responseCode; }
            set { _responseCode = value; }
        }





    }

    public enum ResponseCodes : int
    {
        Success = 0,
        DataError = 1,
        ServiceNotAvailable = 2,
        NotAuthorised = 3,
    }
    #endregion


    #region--request Class---
    public class UpdatebookingstatustoSupplier
    {
        public UpdatebookingstatustoSupplier()
        {
            driver_location = new driver_location();
            vehicleDetail = new VehicleDetail();
        }
        [Required]
        public string SupplierID { get; set; }
        public bool Isbookingaccepted { get; set; }
        [Required]
        public string jobidentifier { get; set; }

        [Required]
        public string ctbookingid { get; set; }


        public string eventname { get; set; }

        //clientid,clientsecret,token =>gett

        public string SupplierCredentials { get; set; }

        //public string GettClient_Secret { get; set; }

        public string ClientName { get; set; }

        //public string GettFleetToken { get; set; }
        public string eta { get; set; }
        public string arrived_at { get; set; }
        public string started_at { get; set; }
        public string ended_at { get; set; }


        public VehicleDetail vehicleDetail { get; set; }
        public driver_location driver_location { get; set; }



    }
    public class VehicleDetail
    {
       
        [Required]
        public string DriverName { get; set; }
        public string DriverID { get; set; }
        public double driver_rating { get; set; }
       
        [Required]
        public string Phonenumber { get; set; }

       
        public string VehicleNumber { get; set; }
        public string BadgeNumber { get; set; }
        public string VehicleMake { get; set; }
        public string VehicleModel { get; set; }
        public string VehicleColor { get; set; }


    }
    public class driver_location
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }
    #endregion


    public class stp_getbookingdriverdetails
    {
        public stp_getbookingdriverdetails() {


        }

        public double? Speed { get; set; }
     
        public double? Longitude { get; set; }
     
        public double? Latitude { get; set; }
      

        public string VehicleNo { get; set; }
     
        public long? CurrentJobId { get; set; }

        public string VehicleModel { get; set; }
       
        public string VehicleColor { get; set; }

     
     
        public string DriverName { get; set; }
   
        public string DriverNo { get; set; }
    
      
        public string VehicleMake { get; set; }
        public string SupplierCredentials { get; set; }
        public string PhoneNumber { get; set; }
        public string OrderNo { get; set; }
        public string PHCDriver { get; set; }
        public int? BookingStatusId { get; set; }
        public int? SupplierId { get; set; }
        public long? OnlineBookingId { get; set; }

        public int? CompanyId { get; set; }
        public int? BookingTypeId { get; set; }
        public long BookingId { get; set; }
        public string EventName { get; set; }
    }
}
