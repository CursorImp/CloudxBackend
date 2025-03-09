using DotNetCoords;
using SignalRHub.Classes;
using SignalRHub.Classes.KonnectSupplier;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Taxi_BLL;
using Taxi_Model;
using Utils;


namespace SignalRHub
{
    public class ThirdPartyViewModel
    {
       
           
       
        public string DispatchTripID { get; set; }
        public string SupplierTripID { get; set; }
        public string SupplierID { get; set; }
        //public RequestType RequestType { get; set; }
        public string FromUser { get; set; }
        public string ToUser { get; set; }
        public string MessageText { get; set; }
    }

    [System.Web.Http.AllowAnonymous]
    [System.Web.Http.RoutePrefix("api/supplier")]
    public class SupplierController : ApiController
    {

        #region JUDO INTEGRATION

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("getregistercardtransactiondetails")]
        public string getregistercardtransactiondetails(string receiptId)
        {
            string rtn = string.Empty;
            try
            {

                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\gettransactiondetails.txt", DateTime.Now.ToStr() + " receiptId" + receiptId + Environment.NewLine);

                }
                catch
                {

                }


                string URL = "";


                using (TaxiDataContext db = new TaxiDataContext())
                {
                    var obj = db.Gen_SysPolicy_PaymentDetails.Where(c => c.PaymentGatewayId == Enums.PAYMENT_GATEWAY.JUDO).Select(args => new { MerchantID = args.ApiUsername, MerchantPassword = args.ApiPassword }).FirstOrDefault();


                    //   string json = Newtonsoft.Json.JsonConvert.SerializeObject(c);
                    string baseUrl = System.Configuration.ConfigurationManager.AppSettings["huburl"].ToStr();
                    //   URL = baseUrl + "/api/Supplier/gettransactiondetails?mesg=" + receiptId;
                    URL = "https://api-eurosofttech.co.uk/Judo3DSLive/judopayapi/gettransaction?receiptId=" + receiptId.ToLower() + "&APIToken=" + obj.MerchantPassword.ToStr() + "&APISecret=" + obj.MerchantID.ToStr();
                    System.Net.WebRequest request = System.Net.HttpWebRequest.Create(URL);
                    request.Headers.Add("Authorization", "");
                    System.Net.WebRequest.DefaultWebProxy = null;
                    request.Proxy = System.Net.WebRequest.DefaultWebProxy;
                    System.Net.WebResponse response = request.GetResponse();
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        System.Web.Script.Serialization.JavaScriptSerializer parser = new System.Web.Script.Serialization.JavaScriptSerializer();
                        rtn = reader.ReadToEnd();
                    }

                }

                TransactionCompletedResponse res = Newtonsoft.Json.JsonConvert.DeserializeObject<TransactionCompletedResponse>(rtn);


                string attempted = res.transactionResponse.threeDSecure != null ? res.transactionResponse.threeDSecure.attempted.ToStr() : "false";

                if (res.Status.ToStr().ToLower() == "success")
                    rtn = receiptId.ToStr() + ":" + res.amount + ":" + res.transactionResponse.cardDetails.cardLastFour + ":" + attempted + ":" + res.transactionResponse.consumer.consumerToken + ":" + res.transactionResponse.receiptId;
                else
                {


                    if (res.Errors.details != null && res.Errors.details.Count > 0)
                        rtn = "failed:" + res.Errors.details[0].message.ToStr();
                    else
                        rtn = "failed:" + new JavaScriptSerializer().Deserialize<Detail>(res.Errors.message.ToStr()).message;
                }
                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\gettransactiondetailsresponse.txt", DateTime.Now.ToStr() + " receiptId" + receiptId + ",response:" + rtn + Environment.NewLine);

                }
                catch
                {

                }

            }
            catch (Exception ex)
            {
                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\gettransactiondetailsresponse_exception.txt", DateTime.Now.ToStr() + " receiptId" + receiptId + ",response:" + rtn + ",exception:" + ex.Message + Environment.NewLine);

                }
                catch
                {

                }
            }
            return rtn;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("gettransactiondetails")]
        public string gettransactiondetails(string receiptId)
        {
            string rtn = string.Empty;
            try
            {

                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\gettransactiondetails.txt", DateTime.Now.ToStr() + " receiptId" + receiptId + Environment.NewLine);

                }
                catch
                {

                }


                string URL = "";


                using (TaxiDataContext db = new TaxiDataContext())
                {
                    var obj = db.Gen_SysPolicy_PaymentDetails.Where(c => c.PaymentGatewayId == Enums.PAYMENT_GATEWAY.JUDO).Select(args => new { args.MerchantID, args.MerchantPassword }).FirstOrDefault();


                    //   string json = Newtonsoft.Json.JsonConvert.SerializeObject(c);
                    string baseUrl = System.Configuration.ConfigurationManager.AppSettings["huburl"].ToStr();
                    //   URL = baseUrl + "/api/Supplier/gettransactiondetails?mesg=" + receiptId;
                    URL = "https://api-eurosofttech.co.uk/Judo3DSLive/judopayapi/gettransaction?receiptId=" + receiptId.ToLower() + "&APIToken=" + obj.MerchantPassword.ToStr() + "&APISecret=" + obj.MerchantID.ToStr();
                    System.Net.WebRequest request = System.Net.HttpWebRequest.Create(URL);
                    request.Headers.Add("Authorization", "");
                    System.Net.WebRequest.DefaultWebProxy = null;
                    request.Proxy = System.Net.WebRequest.DefaultWebProxy;
                    System.Net.WebResponse response = request.GetResponse();
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        System.Web.Script.Serialization.JavaScriptSerializer parser = new System.Web.Script.Serialization.JavaScriptSerializer();
                        rtn = reader.ReadToEnd();
                    }

                }

                TransactionCompletedResponse res = Newtonsoft.Json.JsonConvert.DeserializeObject<TransactionCompletedResponse>(rtn);


                if (res.Status.ToStr().ToLower() == "success")
                    rtn = receiptId.ToStr() + ":" + res.amount + ":" + res.transactionResponse.cardDetails.cardLastFour;
                else
                {


                    if (res.Errors.details != null && res.Errors.details.Count > 0)
                        rtn = "failed:" + res.Errors.details[0].message.ToStr();
                    else
                        rtn = "failed:" + new JavaScriptSerializer().Deserialize<Detail>(res.Errors.message.ToStr()).message;
                }
                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\gettransactiondetailsresponse.txt", DateTime.Now.ToStr() + " receiptId" + receiptId + ",response:" + rtn + Environment.NewLine);

                }
                catch
                {

                }

            }
            catch (Exception ex)
            {
                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\gettransactiondetailsresponse_exception.txt", DateTime.Now.ToStr() + " receiptId" + receiptId + ",response:" + rtn + ",exception:" + ex.Message + Environment.NewLine);

                }
                catch
                {

                }
            }
            return rtn;
        }




        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("manualclearjob")]
        public void manualclearjob(string json)
        {
            // Stripe3DS obj = null;
            ManualClearJob obj = null;


            try
            {


                obj = new JavaScriptSerializer().Deserialize<ManualClearJob>(json);
                try
                {
                    //
                    File.AppendAllText(AppContext.BaseDirectory + "\\manualclearjob.txt", DateTime.Now.ToStr() + " json" + json + Environment.NewLine);
                }
                catch
                {

                }



                if (true)
                {







                    using (TaxiDataContext db = new TaxiDataContext())
                    {

                        Fleet_DriverQueueList objDriverQueueBO = db.Fleet_DriverQueueLists.FirstOrDefault(c => c.Id == obj.QueueId);

                        if (objDriverQueueBO != null)
                        {
                            string recordId = Guid.NewGuid().ToString();

                            long? jobId = objDriverQueueBO.CurrentJobId;

                            int driverId = objDriverQueueBO.DriverId.ToInt();

                            objDriverQueueBO.QueueDateTime = DateTime.Now;
                            objDriverQueueBO.CurrentJobId = null;
                            objDriverQueueBO.CurrentDestinationPostCode = string.Empty;
                            objDriverQueueBO.DriverWorkStatusId = Enums.Driver_WORKINGSTATUS.AVAILABLE;
                            objDriverQueueBO.WaitSinceOn = DateTime.Now;

                            db.SubmitChanges();
                            try
                            {
                                try
                                {
                                    HubProcessor.Instance.listofJobs.Add(new clsPDA
                                    {
                                        DriverId = driverId,
                                        JobId = jobId.ToLong(),
                                        MessageDateTime = DateTime.Now.AddSeconds(-30),
                                        JobMessage = "<<Cleared Job>>" + jobId.ToLong().ToStr(),
                                        MessageTypeId = 3,
                                        Id = recordId
                                    });
                                }
                                catch { }

                                SocketIO.SendToSocket(driverId.ToStr(), "<<Cleared Job>>" + jobId.ToLong().ToStr(), "forceClearJob",recordId);

                            }
                            catch (Exception ex)
                            {
                                try
                                {

                                    try
                                    {
                                        //
                                        File.AppendAllText(AppContext.BaseDirectory + "\\manualclearjob_exception1.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                                    }
                                    catch
                                    {

                                    }


                                    System.Threading.Thread.Sleep(100);


                                    SocketIO.SendToSocket(driverId.ToStr(), "<<Cleared Job>>" + jobId.ToLong().ToStr(), "forceClearJob", recordId);
                                    HubProcessor.Instance.listofJobs.Add(new clsPDA
                                    {
                                        DriverId = driverId,
                                        JobId = jobId.ToLong(),
                                        MessageDateTime = DateTime.Now.AddSeconds(-30),
                                        JobMessage = "<<Cleared Job>>",
                                        MessageTypeId = 3,
                                         Id=recordId
                                    });




                                }
                                catch (Exception ex2)
                                {

                                }
                            }
                            db.stp_BookingLog(jobId.ToLong(), obj.ClearBy.ToStr(), "Job is Manually cleared by Controller (" + obj.ClearBy.ToStr() + ")");

                            db.stp_UpdateJobStatus(jobId, Enums.BOOKINGSTATUS.DISPATCHED);


                            try
                            {
                                var bookingDetails = db.Bookings.Where(c => c.Id == jobId && c.PaymentTypeId == Enums.PAYMENT_TYPES.CREDIT_CARD && c.POBDateTime != null)
                                                    .Select(args => new { args.Id, args.BookingNo, args.CustomerCreditCardDetails }).FirstOrDefault();

                                if (bookingDetails != null && bookingDetails.CustomerCreditCardDetails.ToStr().Trim().Length > 0)
                                {




                                    try
                                    {


                                        if (File.Exists(AppContext.BaseDirectory + "\\Transactions\\" + bookingDetails.Id + ".txt"))
                                        {
                                            try
                                            {
                                                //
                                                File.AppendAllText(AppContext.BaseDirectory + "\\manualclearjob_receiptfound.txt", DateTime.Now.ToStr() + " json" + json + Environment.NewLine);
                                            }
                                            catch
                                            {

                                            }

                                            string receiptId = File.ReadAllText(AppContext.BaseDirectory + "\\Transactions\\" + bookingDetails.Id + ".txt");



                                            receiptId = receiptId.Split(':')[0].ToStr();
                                            decimal fares = receiptId.Split(':')[1].ToDecimal();

                                            db.ExecuteQuery<int>("exec stp_manualClearJob {0},{1},{2},{3}", jobId, fares, receiptId, obj.ClearBy.ToStr());
                                            try
                                            {
                                                //
                                                File.AppendAllText(AppContext.BaseDirectory + "\\manualclearjob_receiptfoundupdated.txt", DateTime.Now.ToStr() + " json" + json + Environment.NewLine);
                                            }
                                            catch
                                            {

                                            }
                                        }





                                    }
                                    catch (Exception ex)
                                    {

                                    }




                                    //var paymentDetails = GetPaymentDetails(bookingDetails.BookingNo.ToStr(), bookingDetails.CustomerCreditCardDetails.ToStr());


                                    //if (paymentDetails.ToStr().Trim().Length > 0)
                                    //{
                                    //    var r = paymentDetails.ToStr().Trim().Replace("\\", "").ToStr().Trim().Substring(1);
                                    //    r = r.Remove(r.LastIndexOf('"'));

                                    //    Taxi_AppMain_Judo.PaymentReceipt objCls = Newtonsoft.Json.JsonConvert.DeserializeObject<Taxi_AppMain_Judo.PaymentReceipt>(r.ToStr().Trim());
                                    //    if ((objCls.Result.ToStr().ToLower() == "success"))
                                    //    {


                                    //        string transId = objCls.Message.ToStr().Replace(" ", "").Trim();
                                    //        decimal fares = objCls.OriginalAmount.ToDecimal();

                                    //        db.ExecuteQuery<int>("exec stp_manualClearJob {0},{1},{2},{3}", jobId, fares, transId, AppVars.LoginObj.UserName);

                                    //    }
                                    //}


                                }

                            }
                            catch (Exception ex)
                            {

                            }

                        }



                        DateTime? dt = DateTime.Now.ToDateorNull();
                        DateTime recentDays = dt.Value.AddDays(-1);
                        DateTime dtNow = DateTime.Now;
                        DateTime prebookingdays = dt.Value.AddDays(HubProcessor.Instance.objPolicy.HourControllerReport.ToInt()).ToDate();

                        string data = string.Empty;

                        using (TaxiDataContext dbX = new TaxiDataContext())
                        {


                            List<stp_GetBookingsDataResult> query = dbX.ExecuteQuery<stp_GetBookingsDataResult>("exec stp_getbookingsdata {0},{1},{2},{3}", recentDays, prebookingdays, 0, HubProcessor.Instance.objPolicy.DaysInTodayBooking.ToInt()).ToList();

                            data = "jsonrefresh required dashboard";
                            data += "|>>>|" + Newtonsoft.Json.JsonConvert.SerializeObject(query);
                        }




                        General.BroadCastMessage(data);



                        General.BroadCastMessage("refresh dashboard drivers");



                    }


                }


            }
            catch (Exception ex)
            {
                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\manualclearjob_exception.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }


            }


        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("verifypayment")]
        public UpdatePaymentRequest verifypayment(UpdatePaymentRequest obj)
        {
            // Stripe3DS obj = null;

            string json = string.Empty;
            try
            {
                json = new JavaScriptSerializer().Serialize(obj);

                // obj = new JavaScriptSerializer().Deserialize<Stripe3DS>(mesg);
                try
                {
                    //
                    File.AppendAllText(AppContext.BaseDirectory + "\\VerifyPaymentrequest.txt", DateTime.Now.ToStr() + " json" + json + Environment.NewLine);
                }
                catch
                {

                }

                long jobId = obj.BookingId.ToLong();

                using (TaxiDataContext db = new TaxiDataContext())
                {
                    var objBooking = db.Bookings.Where(c => c.Id == jobId && c.PaymentTypeId == Enums.PAYMENT_TYPES.CREDIT_CARD_PAID)
                              .Select(args => new { args.PaymentComments }).FirstOrDefault();
                    if (objBooking != null)
                    {
                        obj.IsSuccess = true;
                        obj.status = "PAID";
                        obj.ReceiptId = db.Booking_Payments.Where(C => C.BookingId == jobId).Select(c => c.AuthCode).FirstOrDefault().ToLong();
                        obj.CreatedAt = string.Format("{0:dd/MM/yyyy}", DateTime.Now);

                        obj.TransactionDetails = obj.ReceiptId.ToStr() + ":" + "3.0:1225";
                    }

                }





                //obj.IsSuccess = true;
                //obj.status = "PAID";
                //obj.ReceiptId =new TaxiDataContext().Booking_Payments.Where(C => C.BookingId == jobId).Select(c => c.AuthCode).FirstOrDefault().ToLong();
                //obj.CreatedAt = string.Format("{0:dd/MM/yyyy}", DateTime.Now);

                //obj.TransactionDetails = obj.ReceiptId.ToStr() + ":" + "3.0:1225";

                json = new JavaScriptSerializer().Serialize(obj);

                try
                {
                    //
                    File.AppendAllText(AppContext.BaseDirectory + "\\VerifyPaymentresponse.txt", DateTime.Now.ToStr() + " json" + json + Environment.NewLine);
                }
                catch
                {

                }

            }
            catch (Exception ex)
            {
                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\AuthStripePayment_exception.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }


            }

            return obj;
        }


        private Gen_SysPolicy_PaymentDetail GetJudoCredentials(bool getnon3ds, bool get3ds)
        {

            Gen_SysPolicy_PaymentDetail cc = new Gen_SysPolicy_PaymentDetail();
            try
            {
                using (TaxiDataContext db = new TaxiDataContext())
                {
                    var obj = db.Gen_SysPolicy_PaymentDetails.Where(c => c.PaymentGatewayId == Enums.PAYMENT_GATEWAY.JUDO)
                         .Select(args => new { secret = args.ApiUsername, token = args.ApiPassword, judoId = args.PaypalID }).FirstOrDefault();


                    if (obj != null)
                    {

                        cc.PaypalID = obj.judoId;
                        cc.MerchantID = obj.secret;
                        cc.MerchantPassword = obj.token;

                    }

                }
            }
            catch
            {

            }

            return cc;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetToJudoPay")]
        public string GetToJudoPay(string value)
        {
            JudopayPayment obj = null;
            string rtn = "false";
            string apiurl = string.Empty;
            string json = value;
            try
            {

                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\GetToJudoPay.txt", DateTime.Now.ToStr() + " json" + json + Environment.NewLine);

                }
                catch
                {

                }
                //


                obj = new JavaScriptSerializer().Deserialize<JudopayPayment>(json);
                string url = obj.UpdateUrl.ToStr();
                obj.UpdateUrl = url + "/api/Supplier/updatepayment";
                obj.VerifyPaymentUrl = url + "/api/Supplier/verifypayment";


                var credentials = GetJudoCredentials(false, true);

                obj.JudoId = credentials.PaypalID.ToStr();
                //   obj.APISecret = "3d02c49a3037b5b3bd55f9bdcc50ef28485a5ee431455cf78304a2d0a504bbe8";
                //  obj.APIToken = "NoVAEyt9E3x1AMRE";
                obj.APISecret = credentials.MerchantID.ToStr();
                obj.APIToken = credentials.MerchantPassword.ToStr();



                obj.APISecret = "";
                obj.APIToken = "";
                string data = Newtonsoft.Json.JsonConvert.SerializeObject(obj);



                //paybylink sandbox


                //obj.APISecret = "34b0e9493af71ddc45c1fc90d48f160d5b30b15cd072d7b6a9b29c3c60cad3ae";
                //obj.APIToken = "ZikiCp6hyxSyTklf";








                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\GetToJudoPaydecoded.txt", DateTime.Now.ToStr() + " data=" + data + Environment.NewLine);
                }
                catch
                {

                }



                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\GetToJudoPayencoded.txt", DateTime.Now.ToStr() + " data=" + EncodeBASE64(data) + Environment.NewLine);
                }
                catch
                {

                }



                apiurl = "https://api-eurosofttech.co.uk/Judo3DSLive/Judopay/redirect?data=" + EncodeBASE64(data);

                apiurl = General.ToTinyURLS(apiurl);
                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\GetToJudoPay.txt", DateTime.Now.ToStr() + " url=" + apiurl + Environment.NewLine);
                }
                catch
                {

                }






                // HubProcessor.Instance.listofSMS.Add("request dispatchsms = " + obj.CustomerNumber.Trim() + " =" + "Please click on the link to register your card to process the payment for your journey" + Environment.NewLine + apiurl);

                //try
                //{
                //    using (TaxiDataContext db = new TaxiDataContext())
                //    {
                //        db.stp_BookingLog(obj.BookingId.ToLong(), "Payment", "REGISTER CARD LINK SENT TO : " + obj.CustomerNumber.ToStr());

                //    }
                //}
                //catch
                //{

                //}

            }
            catch (Exception ex)
            {
                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\GetToJudoPay_exception.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
                rtn = "false";

            }

            return rtn + ":" + apiurl;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("ProcessByTokenToJudoPay")]
        public string ProcessByTokenToJudoPay(string value)
        {
            JudopayPayment obj = null;
            string rtn = "true";
            string json = value;
            try
            {

                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\ProcessByTokenToJudoPay.txt", DateTime.Now.ToStr() + " json" + json + Environment.NewLine);

                }
                catch
                {

                }
                //


                obj = new JavaScriptSerializer().Deserialize<JudopayPayment>(json);
                string url = obj.UpdateUrl.ToStr();
                obj.UpdateUrl = url + "/api/Supplier/updatepayment";

                string data = new JavaScriptSerializer().Serialize(obj);

                ClsPaymentInformation objCls = new ClsPaymentInformation();
                objCls.TokenDetails = obj.CardToken.ToStr().Trim();
                objCls.Total = Math.Round(obj.Amount, 2).ToStr();
                objCls.BookingNo = obj.BookingNo.ToStr().Trim();
                objCls.BookingId = obj.BookingId.ToStr();

                rtn = General.ProcessJudoPayment(null, objCls, obj.ResponseInJson);




            }
            catch (Exception ex)
            {
                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\ProcessByTokenToJudoPay_exception.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
                rtn = "false";

            }

            return rtn;
        }




        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("SendToJudoPay")]
        public string SendToJudoPay(string value)
        {
            JudopayPayment obj = null;
            string rtn = "";
            string json = value;
            try
            {

                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\SendToJudoPay.txt", DateTime.Now.ToStr() + " json" + json + Environment.NewLine);

                }
                catch
                {

                }
                //


                obj = new JavaScriptSerializer().Deserialize<JudopayPayment>(json);


                string url = obj.UpdateUrl.ToStr();
                obj.UpdateUrl = url + "/api/Supplier/updatepayment";





                var credentials = GetJudoCredentials(false, true);




                if (credentials != null && credentials.MerchantID.ToStr().Trim().Length > 0)
                {
                    obj.JudoId = credentials.PaypalID.ToStr();

                    obj.APISecret = credentials.MerchantID.ToStr();
                    obj.APIToken = credentials.MerchantPassword.ToStr();





                    //






                    string data = new JavaScriptSerializer().Serialize(obj);


                    //sandbox
                    //  string apiurl = "https://api-eurosofttech.co.uk/Sandbox-Judo3DSPayment/Judopay/Checkout?data=" + EncodeBASE64(data);

                    //live
                    string apiurl = "https://api-eurosofttech.co.uk/Judo3DSLive/Judopay/Checkout?data=" + EncodeBASE64(data);



                    apiurl = General.ToTinyURLS(apiurl);
                    try
                    {
                        if (obj.IsRegisterCard == false)
                        {
                            File.AppendAllText(AppContext.BaseDirectory + "\\SendPaymentLink.txt", DateTime.Now.ToStr() + " json" + json + Environment.NewLine);

                        }
                        else
                        {
                            File.AppendAllText(AppContext.BaseDirectory + "\\SendRegisterCardByLink.txt", DateTime.Now.ToStr() + " url=" + apiurl + Environment.NewLine);
                        }
                    }
                    catch
                    {

                    }









                    try
                    {



                        using (TaxiDataContext db = new TaxiDataContext())
                        {

                            if (obj.SendType.ToInt() == 2 && obj.CustomerEmail.ToStr().Trim().Length > 0)
                            {

                                var genSubCompany = db.Gen_SubCompanies.Where(c => c.CompanyName.ToLower() == obj.SubCompanyName.ToLower())
                                        .Select(args => new
                                        {

                                            args.EmailAddress,

                                            args.SmtpHost,
                                            args.SmtpPassword,
                                            args.SmtpUserName,
                                            args.SmtpPort,
                                            args.SmtpHasSSL,
                                            args.EmailCC
                                        }
                                        ).FirstOrDefault();

                                if (genSubCompany == null)
                                {
                                    genSubCompany = db.Gen_SubCompanies.Where(c => c.Id == 1)
                                       .Select(args => new
                                       {

                                           args.EmailAddress,

                                           args.SmtpHost,
                                           args.SmtpPassword,
                                           args.SmtpUserName,
                                           args.SmtpPort,
                                           args.SmtpHasSSL,
                                           args.EmailCC
                                       }
                                       ).FirstOrDefault();
                                }


                                ClsEASendEmail cls = new ClsEASendEmail(genSubCompany.EmailAddress, obj.CustomerEmail.ToStr().Trim(), "Link for Card Payment " + obj.BookingNo.ToStr(), "Please click on the link to process the payment for your journey " + obj.BookingNo.ToStr() + ".<br/>" + "<a href='" + apiurl + "'>" + apiurl + "</a>", genSubCompany.SmtpHost, genSubCompany.EmailCC);
                                cls.Send("", genSubCompany.SmtpUserName, genSubCompany.SmtpPassword);



                                try
                                {

                                    File.AppendAllText(AppContext.BaseDirectory + "\\SendPaymentByLink_email.txt", DateTime.Now.ToStr() + " json" + obj.CustomerEmail.ToStr().Trim() + "," + apiurl.ToStr() + Environment.NewLine);

                                }
                                catch
                                {

                                }

                                obj.CustomerNumber = obj.CustomerEmail.ToStr();




                            }
                            else
                            {


                                if (obj.IsRegisterCard)
                                    HubProcessor.Instance.listofSMS.Add("request dispatchsms = " + obj.CustomerNumber.Trim() + " =" + "Please click on the link to register your card to process the payment for your journey" + Environment.NewLine + apiurl);
                                else
                                    HubProcessor.Instance.listofSMS.Add("request dispatchsms = " + obj.CustomerNumber.Trim() + " =" + "Please click on the link to process the payment for your journey " + obj.BookingNo.ToStr() + Environment.NewLine + apiurl);



                            }



                            if (obj.IsRegisterCard)
                                db.stp_BookingLog(obj.BookingId.ToLong(), obj.UserName.ToStr(), "Register Card link sent to : " + obj.CustomerNumber.ToStr());
                            else
                            {

                                string msglog = string.Empty;

                                if (obj.ReturnAmount > 0)
                                    msglog = "Payment link for £" + Math.Round(obj.Amount, 2) + "(included return journey) sent to : " + obj.CustomerNumber.ToStr();
                                else
                                    msglog = "Payment link for £" + Math.Round(obj.Amount, 2) + " sent to : " + obj.CustomerNumber.ToStr();

                                db.stp_BookingLog(obj.BookingId.ToLong(), obj.UserName.ToStr(), msglog);

                                try
                                {
                                    if (obj.ReturnBookingNo.ToStr().Trim().Length == 0)
                                        obj.ReturnBookingNo = " ";

                                    if (Directory.Exists(AppContext.BaseDirectory + "\\PayLink") == false)
                                    {
                                        Directory.CreateDirectory(AppContext.BaseDirectory + "\\PayLink");

                                    }

                                    File.WriteAllText(AppContext.BaseDirectory + "\\PayLink\\" + obj.BookingId + ".txt", obj.ReturnBookingId + "," + obj.ReturnBookingNo.ToStr() + "," + obj.OneWayAmount + "," + obj.ReturnAmount + "," + obj.BookingNo.ToStr() + Environment.NewLine);
                                }
                                catch
                                {

                                }

                            }


                        }

                        rtn = "success";
                    }
                    catch (Exception ex)
                    {
                        try
                        {

                            File.AppendAllText(AppContext.BaseDirectory + "\\SendToJudoPay_failed.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                        }
                        catch
                        {

                        }

                    }

                }
                else
                {
                    rtn = "failed:" + "3DS Merchant details not found";

                }

            }
            catch (Exception ex)
            {
                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\SendToJudoPay_exception.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
                rtn = "false";

            }

            return rtn;
        }




        public string EncodeBASE64(string text)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(text)).TrimEnd('=').Replace('+', '-')
                .Replace('/', '_');
        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("updatepayment")]
        public string updatepayment(UpdatePaymentRequest obj)
        {
            //   Stripe3DS obj = null;
            string rtn = "true";
            string json = string.Empty;
            try
            {
                json = new JavaScriptSerializer().Serialize(obj);
                try
                {
                    //
                    File.AppendAllText(AppContext.BaseDirectory + "\\UpdatePayment.txt", DateTime.Now.ToStr() + " json" + json + Environment.NewLine);
                }
                catch
                {

                }



                using (TaxiDataContext db = new TaxiDataContext())
                {
                    ////

                    if (obj.IsSuccess)
                    {

                        if (obj.TransactionType.ToStr().ToLower() == "payment")
                        {

                            long jobId = obj.BookingId.ToLong();
                            //if (obj.status.ToStr().ToLower() == "succeeded")
                            //{
                            //


                            try
                            {
                                string customerCreditCardDetails = "Token|" + obj.CardToken.ToStr() + "<<<consumer|" + obj.yourConsumerReference.ToStr() + "<<<consumertoken|" + obj.ConsumerToken.ToStr() + "<<<lastfour|" + obj.CardLastfour + "<<<enddate|" + obj.expiryDate.ToStr() + "<<<receiptid|" + obj.ReceiptId.ToStr() + "<<<is3dsregisterattempt|" + obj.IsThreeDSecureAttempted.ToStr();



                                string msg = "Date: " + string.Format("{0:dd/MMM/yyyy HH:mm}", DateTime.Now) + Environment.NewLine + "Receipt ID: " + obj.ReceiptId.ToStr() + Environment.NewLine + "Taken on JUDO PayByLink: " + string.Format("{0:f2}", obj.Amount);

                                db.ExecuteQuery<int>("update booking set CustomerCreditCardDetails='" + customerCreditCardDetails + "',Paymenttypeid=6,PaymentComments='" + msg + "' where id=" + jobId);
                                //      msg += Environment.NewLine + "(Payment taken from Pay by link)";
                                db.stp_BookingLog(jobId, "Customer", msg);

                                if ((db.ExecuteQuery<int>("select count(*) from booking_payment where bookingId=" + jobId).FirstOrDefault()) == 0)
                                {

                                    db.ExecuteQuery<int>("insert into Booking_Payment(bookingid,AuthCode)VALUES(" + jobId + ",'" + obj.ReceiptId.ToStr() + "')");
                                }

                                using (TaxiDataContext db2 = new TaxiDataContext())
                                {
                                    General.UpdateJobToDriverPDA(db2.Bookings.FirstOrDefault(c => c.Id == jobId), "Credit Card(PAID)");
                                }
                                //

                                if (File.Exists(AppContext.BaseDirectory + "\\PayLink\\" + obj.BookingId.ToStr() + ".txt"))
                                {

                                    try
                                    {
                                        string data = File.ReadAllText(AppContext.BaseDirectory + "\\PayLink\\" + obj.BookingId.ToStr() + ".txt");
                                        var arr = data.Split(',');

                                        long returnBookingId = arr[0].ToLong();
                                        string returnBookingNo = arr[1].ToStr();
                                        //  onewayAmount = arr[2].ToDecimal();
                                        decimal returnAmount = arr[3].ToDecimal();
                                        string BookingNo = arr[4].ToStr();


                                        if (returnBookingId > 0 && returnAmount > 0)
                                        {


                                            customerCreditCardDetails = "Token|" + obj.CardToken.ToStr() + "<<<consumer|" + obj.yourConsumerReference.ToStr() + "<<<consumertoken|" + obj.ConsumerToken.ToStr() + "<<<lastfour|" + obj.CardLastfour + "<<<enddate|" + obj.expiryDate.ToStr() + "<<<receiptid|" + obj.ReceiptId.ToStr() + "<<<is3dsregisterattempt|" + obj.IsThreeDSecureAttempted.ToStr();



                                            msg = "Date: " + string.Format("{0:dd/MMM/yyyy HH:mm}", DateTime.Now) + Environment.NewLine + "Receipt ID: " + obj.ReceiptId.ToStr() + Environment.NewLine + "PAID FROM: " + BookingNo.ToStr();

                                            db.ExecuteQuery<int>("update booking set CustomerCreditCardDetails='" + customerCreditCardDetails + "',Paymenttypeid=6,PaymentComments='" + msg + "' where id=" + returnBookingId);
                                            msg += Environment.NewLine + "(Payment taken from Pay by link)";
                                            db.stp_BookingLog(returnBookingId, "Customer", msg);

                                            if ((db.ExecuteQuery<int>("select count(*) from booking_payment where bookingId=" + returnBookingId).FirstOrDefault()) == 0)
                                            {

                                                db.ExecuteQuery<int>("insert into Booking_Payment(bookingid,AuthCode)VALUES(" + returnBookingId + ",'" + obj.ReceiptId.ToStr() + "')");
                                            }

                                            using (TaxiDataContext db2 = new TaxiDataContext())
                                            {
                                                General.UpdateJobToDriverPDA(db.Bookings.FirstOrDefault(c => c.Id == returnBookingId), "Credit Card(PAID)");
                                            }

                                        }




                                    }
                                    catch (Exception ex)

                                    {
                                        try
                                        {

                                            File.AppendAllText(AppContext.BaseDirectory + "\\updatepaymentreturn_exception.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                                        }
                                        catch
                                        {

                                        }
                                    }


                                }




                            }
                            catch (Exception ex)
                            {
                                try
                                {

                                    File.AppendAllText(AppContext.BaseDirectory + "\\updatepaymentsuccess_exception.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                                }
                                catch
                                {

                                }
                            }
                            //}
                            //else
                            //{



                            //    db.ExecuteQuery<int>("update booking set PaymentComments='PAYMENT FAILED : " + obj.Message.ToStr() + "'  where id=" + jobId);

                            //    db.stp_BookingLog(jobId, "Customer", "PAYMENT FAILED - " + obj.Message.ToStr());

                            //}

                        }
                        else
                        {

                            string customerCreditCardDetails = "Token|" + obj.CardToken.ToStr() + "<<<consumer|" + obj.yourConsumerReference.ToStr() + "<<<consumertoken|" + obj.ConsumerToken.ToStr() + "<<<lastfour|" + obj.CardLastfour + "<<<enddate|" + obj.expiryDate.ToStr() + "<<<receiptid|" + obj.ReceiptId.ToStr() + "<<<is3dsregisterattempt|" + obj.IsThreeDSecureAttempted.ToStr();

                            db.ExecuteQuery<int>("update booking set PaymentComments='CARD REGISTERED',CustomerCreditCardDetails='" + customerCreditCardDetails + "' where id=" + obj.BookingId.ToLong());






                            try
                            {
                                db.stp_BookingLog(obj.BookingId.ToLong(), "Customer", "Card Registered - last four :" + obj.CardLastfour.ToStr());
                            }
                            catch
                            {

                            }


                            if (obj.CustomerNumber.ToStr().Trim().Length > 0)
                            {
                                try
                                {

                                    string number = obj.CustomerNumber.Trim();
                                    Customer objCust = db.Customers.Where(c => c.MobileNo == number
                                     ).FirstOrDefault();



                                    if (objCust != null)
                                    {
                                        bool IsNewCard = true;



                                        if (objCust.CreditCardDetails.ToStr().Trim().Length > 0)
                                            IsNewCard = false;


                                        objCust.CreditCardDetails = customerCreditCardDetails.ToStr();




                                        if (objCust.Id == 0)
                                        {
                                            db.Customers.InsertOnSubmit(objCust);


                                        }
                                        db.SubmitChanges();




                                        string query = string.Empty;

                                        try
                                        {

                                            query = "if not exists(select * from customer_ccdetails where customerid=" + objCust.Id + " and ccdetails='" + objCust.CreditCardDetails + "') 'insert into customer_ccdetails (customerId,CCDetails,AddOn,AddBy,IsDefault) values(" + objCust.Id + ",'" + objCust.CreditCardDetails + "',getdate(),'" + "Customer" + "','" + IsNewCard + "') else select -1";

                                            //
                                            db.ExecuteQuery<int>(query);
                                        }
                                        catch (Exception ex)
                                        {
                                            try
                                            {

                                                File.AppendAllText(AppContext.BaseDirectory + "\\updatepayment_customerccnotfound.txt", DateTime.Now.ToStr() + " json" + json + ",query=" + query + ",exception:" + ex.Message + Environment.NewLine);
                                            }
                                            catch
                                            {

                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    try
                                    {

                                        File.AppendAllText(AppContext.BaseDirectory + "\\updatepayment_customerprofilenotfound.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                                    }
                                    catch
                                    {

                                    }

                                }




                                //
                            }
                        }
                        // db.stp_BookingLog(obj.BookingId.ToLong(), "Customer", "CARD REGISTERED " + obj.paymentIntentId.ToStr() + " | " + obj.Message.ToStr() + " | Pre-authorised £" + Convert.ToDouble((obj.Amount) / 100));
                        //
                    }
                    else
                    {

                        if (obj.Message.ToStr().Trim().Length > 0)
                        {

                            if (obj.Message.ToStr().Trim().Contains("'"))
                                obj.Message = obj.Message.Replace("'", "").Trim();

                            if (obj.Message.ToStr().Trim().Contains("\u0027"))
                                obj.Message = obj.Message.Replace("\u0027", "").Trim();


                            if (obj.TransactionType.ToStr().ToLower() == "payment")
                            {
                                db.ExecuteQuery<int>("update booking set PaymentComments='PAYMENT FAILED : " + obj.Message.ToStr() + "'  where id=" + obj.BookingId.ToLong());

                                db.stp_BookingLog(obj.BookingId.ToLong(), "Customer", "PAYMENT FAILED - " + obj.Message.ToStr());


                            }
                            else
                            {

                                if (obj.TransactionType.ToStr().Trim().Length > 0)
                                    db.ExecuteQuery<int>("update booking set PaymentComments='" + obj.Message + "'  where id=" + obj.BookingId.ToLong());

                                db.stp_BookingLog(obj.BookingId.ToLong(), "Customer", obj.Message.ToStr());
                            }
                        }

                    }
                }


                RefreshActiveBookingDashboard();
                //   RefreshRequiredDashboard(obj.BookingId.ToStr());

            }
            catch (Exception ex)
            {
                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\updatepayment_exception.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
                rtn = "false";

            }

            return rtn;
        }



        private void RefreshActiveBookingDashboard()
        {
            try
            {
                string data = "jsonrefresh active booking dashboard";
                DateTime? dt = DateTime.Now.ToDateorNull();
                DateTime recentDays = dt.Value.AddDays(-1);

                int BookingHours = HubProcessor.Instance.objPolicy.DaysInTodayBooking.ToInt();

                DateTime tillDate = dt.Value.AddHours(BookingHours).Date;

                if (BookingHours > 0)
                    tillDate = DateTime.Now.ToDateTime().AddHours(BookingHours);


                using (TaxiDataContext db = new TaxiDataContext())
                {

                    List<stp_GetBookingsDataResult> query = db.ExecuteQuery<stp_GetBookingsDataResult>("exec stp_getbookingsdata {0},{1},{2},{3}", recentDays, tillDate, 0, BookingHours).ToList();

                    data += "|>>>|" + Newtonsoft.Json.JsonConvert.SerializeObject(query);
                }

                General.BroadCastMessage(data);
            }
            catch (Exception ex)
            {
                try
                {
                    //
                    File.AppendAllText(AppContext.BaseDirectory + "\\RefreshActiveBookingDashboard_exception.txt", DateTime.Now.ToStr() + Environment.NewLine);
                }
                catch
                {

                }
            }
        }

        #endregion


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("CallJobPool")]
        public JsonResult CallJobPool(string json)
        {
            string rtn = string.Empty;
            PoolBooking obj = null;
            CustomJsonResult rtn2 = new CustomJsonResult();
            RequestResponse<JobPoolAPICaller.TrackDriverDetail> obj2 = null;
            try
            {

                try
                {
                    //
                    File.AppendAllText(AppContext.BaseDirectory + "\\calljobool.txt", DateTime.Now.ToStr() + " json" + json + Environment.NewLine);
                }
                catch
                {

                }


                obj = new JavaScriptSerializer().Deserialize<PoolBooking>(json);


                using (TaxiDataContext db = new TaxiDataContext())
                {
                    //
                    db.DeferredLoadingEnabled = false;


                    if (obj.EventType == CabTreasureJobPoolGateway.Models.EventType.TransferredJob)
                    {

                        var objBooking = db.Bookings.FirstOrDefault(m => m.Id == obj.JobId);


                        if (objBooking.SubcompanyId != null)
                            objBooking.CompanyCreditCardDetails = db.Gen_SubCompanies.Where(a => a.Id == objBooking.SubcompanyId).Select(a => a.CompanyNumber).FirstOrDefault().ToStr();


                        objBooking.BoundType = db.Fleet_VehicleTypes.Where(a => a.Id == objBooking.VehicleTypeId).Select(a => a.VehicleType).FirstOrDefault().ToStr();



                        JobPoolAPICaller.TransferJobDTO c = new JobPoolAPICaller.TransferJobDTO();
                        c.BookingInformation = Newtonsoft.Json.JsonConvert.SerializeObject(objBooking);
                        c.JobOriginatorId = obj.JobProviderDefaultClientId;
                        c.JobOriginatorName = objBooking.CompanyCreditCardDetails;
                        c.NearestClientsIds = "";

                        var bookingInformation = Newtonsoft.Json.JsonConvert.SerializeObject(c);
                        var result = JobPoolAPICaller.JobPoolAPIProxy.TransferJob(bookingInformation);
                        JobPoolResponse objResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<JobPoolResponse>(result);


                        if (objResponse.HasError == false)
                        {


                            string query2 = "insert into booking_log(bookingid,[user],beforeupdate,afterupdate,updatedate)values(" + obj.JobId + ",'" + obj.UserName.ToStr() + "','','" + "Job Transferred to Pool',getdate())";


                            db.stp_RunProcedure("update Booking set BookingStatusId=25,FlightDepartureDate=getdate() where Id=" + obj.JobId + ";" + query2);
                            rtn = "success";
                            RefreshActiveBookingDashboard();
                        }
                        else
                        {
                            rtn = "failed:" + objResponse.Message.ToStr();


                        }
                    }
                    else if (obj.EventType == CabTreasureJobPoolGateway.Models.EventType.CancelJob)
                    {

                        db.DeferredLoadingEnabled = false;

                        var objBooking = db.Bookings.FirstOrDefault(m => m.Id == obj.JobId);


                        if (objBooking.SubcompanyId != null)
                            objBooking.CompanyCreditCardDetails = db.Gen_SubCompanies.Where(a => a.Id == objBooking.SubcompanyId).Select(a => a.CompanyNumber).FirstOrDefault().ToStr();

                        objBooking.BoundType = db.Fleet_VehicleTypes.Where(a => a.Id == objBooking.VehicleTypeId).Select(a => a.VehicleType).FirstOrDefault().ToStr();

                        //

                        JobPoolAPICaller.TransferJobDTO c = new JobPoolAPICaller.TransferJobDTO();
                        c.BookingInformation = Newtonsoft.Json.JsonConvert.SerializeObject(objBooking);
                        c.JobOriginatorId = obj.JobProviderDefaultClientId;
                        c.JobOriginatorName = objBooking.CompanyCreditCardDetails;
                        c.NearestClientsIds = "";

                        var bookingInformation = Newtonsoft.Json.JsonConvert.SerializeObject(c);
                        var result = JobPoolAPICaller.JobPoolAPIProxy.CancelJob(bookingInformation);


                        JobPoolResponse objResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<JobPoolResponse>(result);


                        if (objResponse.HasError == false)
                        {


                            string query2 = "insert into booking_log(bookingid,[user],beforeupdate,afterupdate,updatedate)values(" + obj.JobId.ToLong() + ",'" + obj.UserName.ToStr() + "','','" + "Job Transferred to Pool',getdate())";


                            db.stp_RunProcedure("update Booking set BookingStatusId=25,FlightDepartureDate=getdate() where Id=" + obj.JobId.ToLong() + ";" + query2);
                            rtn = "success";
                        }
                        else
                        {

                            rtn = "failed:" + objResponse.Message.ToStr();

                        }


                    }
                    else if (obj.EventType == CabTreasureJobPoolGateway.Models.EventType.UpdateJob)
                    {

                        long jobId = obj.JobId.ToLong();
                        var objBooking = db.Bookings.FirstOrDefault(m => m.Id == jobId);


                        if (objBooking.SubcompanyId != null)
                            objBooking.CompanyCreditCardDetails = db.Gen_SubCompanies.Where(a => a.Id == objBooking.SubcompanyId).Select(a => a.CompanyNumber).FirstOrDefault().ToStr();


                        objBooking.BoundType = db.Fleet_VehicleTypes.Where(a => a.Id == objBooking.VehicleTypeId).Select(a => a.VehicleType).FirstOrDefault().ToStr();



                        JobPoolAPICaller.TransferJobDTO c = new JobPoolAPICaller.TransferJobDTO();
                        c.BookingInformation = Newtonsoft.Json.JsonConvert.SerializeObject(objBooking);
                        c.JobOriginatorId = obj.JobProviderDefaultClientId;
                        c.JobOriginatorName = objBooking.CompanyCreditCardDetails;
                        c.NearestClientsIds = "";

                        var bookingInformation = Newtonsoft.Json.JsonConvert.SerializeObject(c);
                        var result = JobPoolAPICaller.JobPoolAPIProxy.UpdateJob(bookingInformation);
                        JobPoolResponse objResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<JobPoolResponse>(result);


                        if (objResponse.HasError == false)
                        {


                            string query2 = "insert into booking_log(bookingid,[user],beforeupdate,afterupdate,updatedate)values(" + jobId + ",'" + obj.UserName.ToStr() + "','','" + "Job Transferred to Pool',getdate())";


                            db.stp_RunProcedure("update Booking set BookingStatusId=25,FlightDepartureDate=getdate() where Id=" + jobId + ";" + query2);
                            rtn = "success";
                        }
                        else
                        {


                            rtn = "failed:" + objResponse.Message.ToStr();

                        }
                    }
                    else if (obj.EventType.ToInt() == 7)
                    {


                        rtn = JobPoolAPICaller.JobPoolAPIProxy.TrackDriver(obj.JobId.ToStr(), "", "", "");




                        try
                        {
                            obj2 = Newtonsoft.Json.JsonConvert.DeserializeObject<RequestResponse<JobPoolAPICaller.TrackDriverDetail>>(rtn.ToStr());
                            rtn2.Data = obj2.Data;
                        }
                        catch
                        {
                            rtn2.Data = rtn.ToStr();

                        }



                        try
                        {

                            File.AppendAllText(AppContext.BaseDirectory + "\\json2.txt", DateTime.Now.ToStr() + " json" + rtn + Environment.NewLine);
                            //  rtn = "failed:" + ex.Message.ToStr();
                        }
                        catch
                        {

                        }


                    }

                    else if (obj.EventType.ToInt() == 8)
                    {

                        long jobId = obj.JobId.ToLong();
                        var objBooking = db.Bookings.FirstOrDefault(m => m.Id == jobId);

                        if (objBooking.BookingStatusId == 21)
                        {
                            rtn = "failed:You cannot Recover Job as job is already Transferred";
                            //  MessageBox.Show("You cannot Recover Job as job is already Transferred");
                            //   return;

                        }
                        else
                        {

                            objBooking.BookingStatusId = Enums.BOOKINGSTATUS.WAITING;
                            objBooking.Booking_Logs.Add(new Booking_Log { User = obj.UserName.ToStr(), UpdateDate = DateTime.Now, AfterUpdate = "Job Recovered from Pool" });

                            db.SubmitChanges();

                            //  row.Delete();
                            //    BroadCastMessage(DispatchHub.RefreshTypes.REFRESH_BOOKING_DASHBOARD);
                            General.BroadCastMessage(RefreshTypes.REFRESH_ACTIVEBOOKINGS_DASHBOARD);
                            //
                            //  rtn= JobPoolAPICaller.JobPoolAPIProxy.SendBackToPool(jobId.ToStr(),obj.JobAcceptorDefaultClientId.ToStr());
                            rtn = JobPoolAPICaller.JobPoolAPIProxy.GetRecoverJob(jobId.ToStr(), HubProcessor.Instance.objPolicy.DefaultClientId.ToStr());


                            General.BroadCastMessage("**internalmessage>>request recoverfrompool=" + jobId);
                        }



                    }

                }

            }
            catch (Exception ex)
            {
                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\calljobool_exception.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                    rtn = "failed:" + ex.Message.ToStr();
                }
                catch
                {

                }


            }

            if (rtn2.Data == null)
                rtn2.Data = rtn;

            //    return new CustomJsonResult { Data = obj2.Data };
            return rtn2;
            //     return rtn;

        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("UpdateSupplierStatus")]
        public string UpdateSupplierStatus(string mesg)
        {
            try
            {
                File.AppendAllText(AppContext.BaseDirectory + "\\UpdateSupplierStatus.txt", DateTime.Now.ToStr() + "request:" + mesg + Environment.NewLine);
            }
            catch
            {

            }

            try
            {
                GettAPICall.ClsSupplierData obj = Newtonsoft.Json.JsonConvert.DeserializeObject<GettAPICall.ClsSupplierData>(mesg);




                //

                if (obj.Status.ToStr().ToLower() == "cancelled")
                    obj.BookingStatusId = Enums.BOOKINGSTATUS.CANCELLED.ToInt();

                return CallSupplierApi.UpdateStatus(obj.JobId, obj.BookingStatusId);


            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\UpdateSupplierStatus_exception.txt", DateTime.Now.ToStr() + "request:" + mesg + Environment.NewLine);
                }
                catch
                {

                }
                return ex.Message;

            }

        }





        private List<stp_GetBookingsDataResult> GetRequiredData()
        {

            
            List<stp_GetBookingsDataResult> query = null;
            try
            {
                DateTime? dt = DateTime.Now.ToDateorNull();
                DateTime recentDays = dt.Value.AddDays(-1);
                DateTime dtNow = DateTime.Now;
                DateTime prebookingdays = dt.Value.AddDays(HubProcessor.Instance.objPolicy.HourControllerReport.ToInt()).ToDate();



                using (TaxiDataContext db = new TaxiDataContext())
                {
                    //

                    query = db.ExecuteQuery<stp_GetBookingsDataResult>("exec stp_getbookingsdata {0},{1},{2},{3}", recentDays, prebookingdays, 0, HubProcessor.Instance.objPolicy.DaysInTodayBooking.ToInt()).ToList();

                    //   data += "|>>>|" + Newtonsoft.Json.JsonConvert.SerializeObject(query);
                }

            }
            catch
            {

            }

            return query;
        }



       

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("PostAlert")]
        public JsonResult PostAlert(ArrivalAirportRoot value)
        {
            string json1 = "";

            try
            {
                json1 = Newtonsoft.Json.JsonConvert.SerializeObject(value);





                try
                {

                    System.IO.File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "\\PostAlert.txt", DateTime.Now.ToStr() + json1 + "estimatedRunwayArrival :" + value.alert.flightStatus.operationalTimes.estimatedRunwayArrival.dateLocal + Environment.NewLine);
                }
                catch
                {


                }
                //FlightCustomizeClass alert = new FlightCustomizeClass()
                //{
                //    arrival = value.alert.rule.arrival,
                //    flightNumber = value.alert.rule.carrierFsCode + value.alert.rule.flightNumber,
                //    status = value.alert.flightStatus.status,
                //    dateLocal = value.alert.flightStatus.arrivalDate.dateLocal,

                //    dateUtc = value.alert.flightStatus.operationalTimes.estimatedRunwayArrival.dateLocal

                //};

                //  return Json(value, JsonRequestBehavior.AllowGet);

                string flightNo = value.alert.rule.carrierFsCode + value.alert.rule.flightNumber;
                DateTime newPickupTime = value.alert.flightStatus.operationalTimes.estimatedRunwayArrival.dateLocal;


                using (TaxiDataContext db = new TaxiDataContext())
                {
                    DateTime now = DateTime.Now.ToDate();
                    var objBooking = db.Bookings.FirstOrDefault(c => c.FromLocTypeId == 1
                    && c.FromDoorNo == flightNo && c.BookingStatusId != 2 && c.PickupDateTime.Value.Date == now);

                    if (objBooking != null)
                    {

                        try
                        {

                            System.IO.File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "\\PostAlertBookingfound.txt", DateTime.Now.ToStr() + json1 + "estimatedRunwayArrival :" + value.alert.flightStatus.operationalTimes.estimatedRunwayArrival.dateLocal + ",jobid:" + objBooking.Id + Environment.NewLine);
                        }
                        catch
                        {


                        }


                        if (objBooking.BookingStatusId != 13 && objBooking.PickupDateTime.ToDate() == now && newPickupTime != null && newPickupTime != objBooking.PickupDateTime)
                        {

                            string newPickupdate = string.Format("{0:dd/MM/yyyy HH:mm}", newPickupTime);

                            if (newPickupdate.ToStr().Trim().Length > 0)
                            {



                                objBooking.Booking_Logs.Add(new Booking_Log { BookingId = objBooking.Id, AfterUpdate = "Alert Flight | PickupDateTime : " + newPickupdate, BeforeUpdate = "PickupDateTime :" + string.Format("{0:dd/MM/yyyy HH:mm}", objBooking.PickupDateTime), User = "Flight", UpdateDate = DateTime.Now });

                                objBooking.PickupDateTime = newPickupTime;




                                decimal? allowanceMins = 0;



                                try
                                {
                                    string locName = objBooking.FromAddress.ToStr().Trim();

                                    if (locName.Contains(","))
                                        locName = locName.Replace(",", "").Trim();


                                    if (locName.Contains("  "))
                                        locName = locName.Replace("  ", " ").Trim();




                                    locName = locName.ToUpper();

                                    int locId = db.Gen_Locations.Where(c => c.LocationTypeId == Enums.LOCATION_TYPES.AIRPORT && c.LocationName.ToUpper() == locName).Select(c => c.Id)
                                     .FirstOrDefault();

                                    try
                                    {
                                        allowanceMins = db.ExecuteQuery<decimal?>("SELECT AllownceMins FROM Gen_SysPolicy_AirportPickupCharges where airportId=" + locId).FirstOrDefault();
                                    }
                                    catch
                                    {

                                    }



                                    if (allowanceMins.ToInt() > 0)
                                        objBooking.PickupDateTime = objBooking.PickupDateTime.Value.AddMinutes(10);
                                }
                                catch
                                {

                                }



                                try
                                {
                                    DateTime dtNew = newPickupTime.AddMinutes(allowanceMins.ToInt());

                                    string hour = dtNew.Hour.ToStr();
                                    if (hour.Length == 1)
                                        hour = "0" + hour;


                                    string min = dtNew.Minute.ToStr();
                                    if (min.Length == 1)
                                        min = "0" + min;




                                    string CityCode = value.alert.rule.departureAirportFsCode.ToStr();
                                    string sub = string.Empty;
                                    for (int s = 0; s < value.appendix.airports.airport.Count; s++)
                                    {
                                        if (CityCode == value.appendix.airports.airport[s].cityCode || CityCode == value.appendix.airports.airport[s].fs)
                                        {
                                            sub = " from " + value.appendix.airports.airport[s].city;

                                            if (value.alert.flightStatus != null && value.alert.flightStatus.airportResources != null && value.alert.flightStatus.airportResources.arrivalTerminal.ToStr() != "")
                                                sub += " in t" + value.alert.flightStatus.airportResources.arrivalTerminal;
                                            break;
                                        }
                                    }



                                    string newStreet = "Arriving at " + hour + min + sub;

                                    objBooking.FromStreet = newStreet.ToStr();

                                }
                                catch
                                {

                                }



                                db.SubmitChanges();


                                ClsBookingsInfo cls = new ClsBookingsInfo();
                                cls.listofBookings = GetRequiredData();
                                cls.flightnumber = objBooking.FromDoorNo.ToStr();
                                cls.BookingNo = objBooking.BookingNo.ToStr();
                                cls.JobId = objBooking.Id;
                                cls.shownotification = true;
                                cls.notificationappearon = 1;
                                cls.NewPickupDate = newPickupdate;
                                cls.notificationtitle = "Flight Information Changed [" + objBooking.FromDoorNo + "]";
                                cls.notificationcontent = "<html><b><span style=font-size:medium><color=Blue>Job Ref# " + objBooking.BookingNo + "</span></b></html>";
                                cls.soundfilename = "message1.wav";
                                cls.notificationautoclosedelay = 30;
                                cls.shownotificationimage = true;
                                //cls.notificationcolor = "Blue";
                                string posting = "**flightalertinfo>>" + Newtonsoft.Json.JsonConvert.SerializeObject(cls);
                                General.BroadCastMessage(posting);



                            }
                        }



                    }
                    else
                    {
                        try
                        {

                            System.IO.File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "\\PostAlertBookingnotfound.txt", DateTime.Now.ToStr() + json1 + "estimatedRunwayArrival :" + value.alert.flightStatus.operationalTimes.estimatedRunwayArrival.dateLocal + ",jobid:" + Environment.NewLine);
                        }
                        catch
                        {


                        }
                    }
                }





            }
            catch (Exception ex)
            {

                try
                {
                    System.IO.File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "\\ExceptionFlightData.txt", DateTime.Now.ToStr() + ",exception:" + ex.ToString() + ",value=" + json1 + Environment.NewLine + Environment.NewLine);
                }
                catch
                {

                }
            }

            return new JsonResult();
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("UploadEscortFile")]
        public payload UploadEscortFile(payload payload)
        {
            string rtn = string.Empty;
            string filePath = string.Empty;
            try
            {
                ////
                try
                {
                    //
                    File.AppendAllText(AppContext.BaseDirectory + "\\UploadEscortFile.txt", DateTime.Now.ToStr() + " request=" + payload.file.ToStr() + Environment.NewLine);
                }
                catch
                {

                }
                string fileName = payload.imageName;
                string baseUrl = payload.BaseUrl.ToStr();
                filePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Documents/Escort/") + fileName;
                try
                {

                    File.WriteAllBytes(filePath, Convert.FromBase64String(payload.file));
                    payload = new payload();
                    payload.message = "SUCCESS";
                    payload.filePath = baseUrl + "/Documents/Escort/" + fileName;



                }
                catch (Exception ex)
                {
                    payload = new payload();
                    payload.message = ex.Message;

                    //rtn = "FAILED:" + ex.Message.ToString();
                }


            }
            catch (Exception ex)
            {
                File.AppendAllText(AppContext.BaseDirectory + "\\UploadFile_exception.txt", DateTime.Now.ToStr() + " request" + payload.file.ToStr() + Environment.NewLine);

            }
            //
            return payload;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("UploadFile")]
        public payload UploadFile(payload payload)
        {
            string rtn = string.Empty;
            string filePath = string.Empty;
            try
            {
                ////
                try
                {
                    //
                    File.AppendAllText(AppContext.BaseDirectory + "\\UploadFile.txt", DateTime.Now.ToStr() + " request=" + payload.file.ToStr() + Environment.NewLine);
                }
                catch
                {

                }
                string fileName = payload.imageName;
                string baseUrl = payload.BaseUrl.ToStr();
                filePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Documents/") + fileName;
                try
                {

                    File.WriteAllBytes(filePath, Convert.FromBase64String(payload.file));
                    payload = new payload();
                    payload.message = "SUCCESS";
                    payload.filePath = baseUrl + "/Documents/" + fileName;



                }
                catch (Exception ex)
                {
                    payload = new payload();
                    payload.message = ex.Message;

                    //rtn = "FAILED:" + ex.Message.ToString();
                }


            }
            catch (Exception ex)
            {
                File.AppendAllText(AppContext.BaseDirectory + "\\UploadFile_exception.txt", DateTime.Now.ToStr() + " request" + payload.file.ToStr() + Environment.NewLine);

            }
            //
            return payload;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("ReceiveCallerId")]
        public void ReceiveCallerId(ClsCallerId obj)
        {
            //
            //try
            //{
            //    File.AppendAllLines[[]
            //}
            //catch
            //{
            //}
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("authstripepayment")]
        public string AuthStripePayment(Stripe3DS obj)
        {
            //   Stripe3DS obj = null;
            string rtn = "true";
            string json = string.Empty;
            try
            {
                json = new JavaScriptSerializer().Serialize(obj);
                try
                {
                    //
                    File.AppendAllText(AppContext.BaseDirectory + "\\AuthStripePayment.txt", DateTime.Now.ToStr() + " json" + json + Environment.NewLine);
                }
                catch
                {

                }


                //   obj = new JavaScriptSerializer().Deserialize<Stripe3DS>(json);









                using (TaxiDataContext db = new TaxiDataContext())
                {
                    ////

                    if (obj.IsSuccess)
                    {
                        db.ExecuteQuery<int>("update booking set PaymentComments='Authorised for £" + string.Format("{0:f2}", Convert.ToDouble((obj.Amount / 100))) + "',CompanyCreditCardDetails='" + obj.status.ToStr() + "',CustomerCreditCardDetails='" + obj.paymentIntentId.ToStr() + "' where id=" + obj.BookingId.ToLong());

                        db.stp_BookingLog(obj.BookingId.ToLong(), "Customer", "Secure Card Transaction " + obj.paymentIntentId.ToStr() + " | " + obj.Message.ToStr() + " | Pre-authorised £" + Convert.ToDouble((obj.Amount) / 100));
                        //
                    }
                    else
                    {

                        if (obj.Message.ToStr().Trim().Contains("'"))
                            obj.Message = obj.Message.Replace("'", "").Trim();

                        if (obj.Message.ToStr().Trim().Contains("\u0027"))
                            obj.Message = obj.Message.Replace("\u0027", "").Trim();



                        db.ExecuteQuery<int>("update booking set PaymentComments='" + obj.Message + "'  where id=" + obj.BookingId.ToLong());

                        db.stp_BookingLog(obj.BookingId.ToLong(), "Customer", obj.Message.ToStr());


                    }
                }




            }
            catch (Exception ex)
            {
                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\AuthStripePayment_exception.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
                rtn = "false";

            }

            return rtn;
        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("sendpreauthlink")]
        public string SendPreAuthLink(string value)
        {
            Stripe3DS obj = null;
            string rtn = "true";
            string json = value;
            try
            {
                //  json = new JavaScriptSerializer().Serialize(obj);
                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\SendPreAuthLink.txt", DateTime.Now.ToStr() + " json" + json + Environment.NewLine);
                }
                catch
                {

                }
                //

                obj = new JavaScriptSerializer().Deserialize<Stripe3DS>(json);



                if (obj.PreAuthUrl.ToStr().Contains("tinyurl") == false)
                {


                    obj.PreAuthUrl = General.GetShortUrl(obj.PreAuthUrl.ToStr());
                    try
                    {

                        File.AppendAllText(AppContext.BaseDirectory + "\\SendPreAuthCustomLink.txt", DateTime.Now.ToStr() + " url=" + obj.PreAuthUrl + Environment.NewLine);
                    }
                    catch
                    {

                    }

                }


                var arr = obj.Description.ToStr().Split('|');

                HubProcessor.Instance.listofSMS.Add("request dispatchsms = " + obj.MobileNo.Trim() + " =" + "Please click on the link to authorize your payment for your journey with " + arr[0] + Environment.NewLine + "Ref No - " + arr[1].ToStr().Trim() + Environment.NewLine + "Cars will only be dispatched when payment authorised." + Environment.NewLine + obj.PreAuthUrl.ToStr());

                using (TaxiDataContext db = new TaxiDataContext())
                {
                    db.stp_BookingLog(obj.BookingId.ToLong(), obj.OperatorName.ToStr(), "PAYMENT LINK for £" + string.Format("{0:f2}", Convert.ToDouble((obj.Amount / 100))) + " SENT TO : " + obj.MobileNo.ToStr());

                }

            }
            catch (Exception ex)
            {
                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\SendPreAuthLink_exception.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
                rtn = "false";

            }

            return rtn;
        }












        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("getstripebookingtoken")]
        public string GetStripeBookingToken(string value)
        {

            string rtn = "true";
            string json = value;

            try
            {

                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\GetStripeBookingToken.txt", DateTime.Now.ToStr() + " json" + json + Environment.NewLine);
                }
                catch
                {

                }

                string publishKey = "", secretKey = "";
                string companyName = string.Empty;
                using (TaxiDataContext db = new TaxiDataContext())
                {

                    var objPaymentGateway = db.Gen_SysPolicy_PaymentDetails.FirstOrDefault(c => c.PaymentGatewayId == Enums.PAYMENT_GATEWAY.STRIPE);

                    publishKey = objPaymentGateway.ApplicationId.ToStr();
                    secretKey = objPaymentGateway.PaypalID.ToStr();


                    companyName = db.Gen_SubCompanies.Select(c => c.CompanyName).FirstOrDefault();
                }


                StripeHoldCard obj = new JavaScriptSerializer().Deserialize<StripeHoldCard>(value);
                //


                double increasedAmount = obj.ActualFares + Convert.ToDouble(((Convert.ToDouble(obj.ActualFares) * 20) / 100));
                obj.Amount = (increasedAmount * 100).ToInt();


       


                obj.Description = companyName + " | " + obj.BookingId.ToStr() + " | " + "Fares : " + obj.ActualFares + " - " + increasedAmount.ToStr() + " GBP" + " | " + obj.Description.ToStr();
                obj.Currency = "GBP";
                obj.APIkey = publishKey;
                obj.APISecret = secretKey;
                obj.OperatorName = "APP";


                json = obj.EncodeBASE64(new JavaScriptSerializer().Serialize(obj));

                string request = string.Empty;

                using (var client = new System.Net.Http.HttpClient())
                {
                    //    var BASE_URL= "https://api-eurosofttech.co.uk/sanbox_stripePayment-api/";
                    var BASE_URL = "https://api-eurosofttech.co.uk/StripePayment-api/";
                    client.BaseAddress = new Uri(BASE_URL);
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));
                    request = BASE_URL + "CardTokenHoldPayment?json=" + json;
                    var postTask = client.PostAsync(request, null).Result;
                    rtn = postTask.Content.ReadAsStringAsync().Result;



                }


                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\GetStripeBookingTokenResponse.txt", DateTime.Now.ToStr() + " request : " + request + ",response:" + rtn + Environment.NewLine);
                }
                catch
                {

                }

                //

            }
            catch (Exception ex)
            {
                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\GetStripeBookingToken_exception.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
                rtn = "false";

            }

            return rtn;
        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("SendPayByLink")]
        public string SendPayByLink(string value)
        {
            Stripe3DS obj = null;
            string rtn = "true";
            string json = value;
            try
            {
                //  json = new JavaScriptSerializer().Serialize(obj);
                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\SendPayByLink.txt", DateTime.Now.ToStr() + " json" + json + Environment.NewLine);

                }
                catch
                {

                }
                //


                obj = new JavaScriptSerializer().Deserialize<Stripe3DS>(json);
                string url = obj.UpdatePaymentURL.ToStr();
                obj.UpdatePaymentURL = url + "/api/Supplier/VerifyPaymentStripe";
                obj.PaymentStatusURL = url + "/api/Supplier/UpdateDataFromPayByLink";


                var arr = obj.Description.ToStr().Split('|');
                obj.ImageUrl = string.Empty;
                obj.CompanyName = arr[0].ToStr();

                obj.BookingRefNo = arr[1].ToStr().Trim();

                string data = new JavaScriptSerializer().Serialize(obj);




                obj.PreAuthUrl = "https://api-eurosofttech.co.uk/Stripe3DSPayment/Home/StripeCheckout?data=" + EncodeBASE64(data);

                if (obj.PreAuthUrl.ToStr().Contains("tinyurl") == false)
                {
                    //

                    obj.PreAuthUrl = General.ToTinyURLS(obj.PreAuthUrl.ToStr());
                    try
                    {

                        File.AppendAllText(AppContext.BaseDirectory + "\\SendPayByLinkCustomLink.txt", DateTime.Now.ToStr() + " url=" + obj.PreAuthUrl + Environment.NewLine);
                    }
                    catch
                    {

                    }

                }




                HubProcessor.Instance.listofSMS.Add("request dispatchsms = " + obj.MobileNo.Trim() + " =" + "Please click on the link to process your payment for your journey with " + arr[0] + Environment.NewLine + "Ref No - " + arr[1].ToStr().Trim() + Environment.NewLine + obj.PreAuthUrl.ToStr());

                using (TaxiDataContext db = new TaxiDataContext())
                {

                    string returnPayment = string.Empty;

                    if (obj.ReturnAmount > 0)
                    {

                        returnPayment = " (Inc. Return)";

                    }

                    db.stp_BookingLog(obj.BookingId.ToLong(), obj.OperatorName.ToStr(), "PAYMENT LINK for £" + string.Format("{0:f2}", Convert.ToDouble((obj.Amount / 100))) + returnPayment + " SENT TO : " + obj.MobileNo.ToStr());

                }


                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\SendPayByLinksms.txt", DateTime.Now.ToStr() + obj.MobileNo.ToStr() + " Please click on the link to process your payment for your journey with " + arr[0] + Environment.NewLine + "Ref No - " + arr[1].ToStr().Trim() + Environment.NewLine + obj.PreAuthUrl.ToStr() + Environment.NewLine);



                    if (obj.ReturnAmount > 0)
                    {

                        File.WriteAllText(AppContext.BaseDirectory + "\\paybylink\\" + obj.BookingId.ToStr() + ".txt", obj.ReturnAmount.ToStr());



                    }


                }
                catch
                {

                }

            }
            catch (Exception ex)
            {
                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\SendPayByLink_exception.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
                rtn = "false";

            }

            return rtn;
        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("UpdateDataFromPayByLink")]
        public string UpdateDataFromPayByLink(Stripe3DS obj)
        {
            // Stripe3DS obj = null;
            string rtn = "true";
            string json = string.Empty;
            try
            {
                json = new JavaScriptSerializer().Serialize(obj);

                // obj = new JavaScriptSerializer().Deserialize<Stripe3DS>(mesg);
                try
                {
                    //
                    File.AppendAllText(AppContext.BaseDirectory + "\\UpdateDataFromPayByLink.txt", DateTime.Now.ToStr() + " json" + json + Environment.NewLine);
                }
                catch
                {

                }


                //   obj = new JavaScriptSerializer().Deserialize<Stripe3DS>(json);






                obj.Message = obj.Message.ToStr();

                if (obj.Message.ToStr().Trim().Contains("'"))
                    obj.Message = obj.Message.Replace("'", "").Trim();

                if (obj.Message.ToStr().Trim().Contains("\u0027"))
                    obj.Message = obj.Message.Replace("\u0027", "").Trim();


                using (TaxiDataContext db = new TaxiDataContext())
                {
                    ////

                    if (obj.status.ToStr().ToLower() == "succeeded" || obj.IsSuccess)
                    {
                        //

                        string msg = "PAID £" + string.Format("{0:f2}", obj.Amount) + " | TRANSACTION - " + obj.paymentIntentId.ToStr() + " | " + string.Format("{0:dd/MM/yyyy HH:mm}", DateTime.Now);

                        db.ExecuteQuery<int>("update booking set Paymenttypeid=6,PaymentComments='" + msg + "' where id=" + obj.BookingId.ToLong());

                        db.stp_BookingLog(obj.BookingId.ToLong(), "Customer", msg);

                        if ((db.ExecuteQuery<int>("select count(*) from booking_payment where bookingId=" + obj.BookingId.ToLong()).FirstOrDefault()) == 0)
                        {

                            db.ExecuteQuery<int>("insert into Booking_Payment(bookingid,AuthCode)VALUES(" + obj.BookingId.ToLong() + ",'" + obj.paymentIntentId.ToStr() + "')");
                        }



                        try
                        {
                            long jobId = obj.BookingId.ToLong();
                            using (TaxiDataContext db2 = new TaxiDataContext())
                            {
                                General.UpdateJobToDriverPDA(db2.Bookings.FirstOrDefault(c => c.Id == jobId), "Credit Card(PAID)");
                            }
                        }
                        catch
                        {

                        }

                        try
                        {
                            if (File.Exists(AppContext.BaseDirectory + "\\paybylink\\" + obj.BookingId.ToStr() + ".txt"))
                            {

                                long returnBookingId = db.ExecuteQuery<long>("select Id from booking where masterjobid=" + obj.BookingId.ToLower()).FirstOrDefault();
                                if (returnBookingId > 0)
                                {

                                    db.ExecuteQuery<int>("update booking set Paymenttypeid=6,PaymentComments='" + "TRANSACTION - " + obj.paymentIntentId.ToStr() + " | " + string.Format("{0:dd/MM/yyyy HH:mm}", DateTime.Now) + "' where Id=" + returnBookingId);

                                    db.ExecuteQuery<int>("insert into Booking_Payment(bookingid,AuthCode)VALUES(" + returnBookingId + ",'" + obj.paymentIntentId.ToStr() + "')");

                                    using (TaxiDataContext db2 = new TaxiDataContext())
                                    {
                                        General.UpdateJobToDriverPDA(db.Bookings.FirstOrDefault(c => c.Id == returnBookingId), "Credit Card(PAID)");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            try
                            {

                                File.AppendAllText(AppContext.BaseDirectory + "\\updatedatafrompaybylink_exception1.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                            }
                            catch
                            {

                            }
                        }
                        //
                    }
                    else
                    {





                        db.ExecuteQuery<int>("update booking set PaymentComments='PAYMENT FAILED : " + obj.Message.ToStr() + "'  where id=" + obj.BookingId.ToLong());

                        db.stp_BookingLog(obj.BookingId.ToLong(), "Customer", "PAYMENT FAILED - " + obj.Message.ToStr());







                    }
                }


                RefreshRequiredDashboard(obj.BookingId.ToStr());



            }
            catch (Exception ex)
            {
                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\AuthStripePayment_exception.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
                rtn = "false";

            }

            return rtn;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("VerifyPayment")]
        public Stripe3DS VerifyPayment(Stripe3DS obj)
        {
            // Stripe3DS obj = null;

            string json = string.Empty;
            try
            {
                json = new JavaScriptSerializer().Serialize(obj);

                // obj = new JavaScriptSerializer().Deserialize<Stripe3DS>(mesg);
                try
                {
                    //
                    File.AppendAllText(AppContext.BaseDirectory + "\\VerifyPaymentrequest.txt", DateTime.Now.ToStr() + " json" + json + Environment.NewLine);
                }
                catch
                {

                }

                long jobId = obj.BookingId.ToLong();

                using (TaxiDataContext db = new TaxiDataContext())
                {
                    var objBooking = db.Bookings.Where(c => c.Id == jobId && c.PaymentTypeId == Enums.PAYMENT_TYPES.CREDIT_CARD_PAID)
                              .Select(args => new { args.PaymentComments }).FirstOrDefault();
                    if (objBooking != null)
                    {
                        obj.IsSuccess = true;
                        obj.status = "PAID";
                        obj.paymentIntentId = objBooking.PaymentComments;
                    }

                }







                // json = new JavaScriptSerializer().Serialize(obj);

                try
                {
                    //
                    File.AppendAllText(AppContext.BaseDirectory + "\\VerifyPaymentresponse.txt", DateTime.Now.ToStr() + " json" + json + Environment.NewLine);
                }
                catch
                {

                }

            }
            catch (Exception ex)
            {
                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\AuthStripePayment_exception.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }


            }

            return obj;
        }


      


        private void RefreshRequiredDashboard(string bookingId = "")
        {
            try
            {

                try
                {
                    //
                    File.AppendAllText(AppContext.BaseDirectory + "\\RefreshRequiredDashboard.txt", DateTime.Now.ToStr() + ",bookingId:" + bookingId + Environment.NewLine);
                }
                catch
                {

                }

                string data = "jsonrefresh required dashboard";

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


                //   List<string> listOfConnections = new List<string>();
                //   listOfConnections = HubProcessor.Instance.ReturnDesktopConnections();
                //     HubProcessor.Instance.Clients.Clients(listOfConnections).cMessageToDesktop(data);
                General.BroadCastMessage(data);
            }
            catch (Exception ex)
            {
                try
                {
                    //
                    File.AppendAllText(AppContext.BaseDirectory + "\\RefreshRequiredDashboard_exception.txt", DateTime.Now.ToStr() + " exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
            }
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("startarrivewaiting")]
        public string startarrivewaiting(string mesg)
        {
            string respo = "true";
            JobActionEx objAction = null;
            try
            {
                try
                {
                    //
                    File.AppendAllText(AppContext.BaseDirectory + "\\startarrivewaiting.txt", DateTime.Now.ToStr() + " request" + mesg + Environment.NewLine);
                }
                catch
                {

                }



                string dataValue = mesg;
                dataValue = dataValue.Trim();
                objAction = new JavaScriptSerializer().Deserialize<JobActionEx>(mesg);
                //   RemoveUK(ref objAction.Dropoff);

                long jobId = objAction.JobId.ToLong();



                DateTime pickupDate = DateTime.Now;
                //DateTime.Now;new TaxiDataContext().Bookings.Where(c => c.Id == jobId).Select(a => a.PickupDateTime).FirstOrDefault();

                //DateTime pickupdatetimeval=

                if (objAction.PickupDateTime.ToStr().Trim().Length > 0)
                {
                    try
                    {
                        string pickupDateTime = string.Format("{0:dd/MM/yyyy HH:mm}", objAction.PickupDateTime.ToStr().Trim().Replace("   ", " ").Trim());

                        pickupDateTime = string.Format("{0:dd/MM/yyyy HH:mm}", objAction.PickupDateTime.ToStr().Trim().Replace("  ", " ").Trim());

                        pickupDate = DateTime.Parse(pickupDateTime, CultureInfo.GetCultureInfo("en-gb"));


                    }
                    catch (Exception ex)
                    {

                        try
                        {

                            pickupDate = DateTime.Now;
                            File.AppendAllText(AppContext.BaseDirectory + "\\" + "exception_formatpickupdatetime_startarrivewaiting.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", message: " + mesg + "," + ex.Message + ", pickup : " + objAction.PickupDateTime.ToStr().Trim() + Environment.NewLine);
                        }
                        catch
                        {


                        }

                    }
                }
                else
                {
                    pickupDate = DateTime.Now;

                }


                if (pickupDate > DateTime.Now)
                {

                    respo = "You cannot press Start Waiting before pickup time";
                }

            }

            catch (Exception ex)
            {
                try
                {
                    objAction.Message = ex.Message;
                    File.AppendAllText(AppContext.BaseDirectory + "\\startarrivewaiting_exception2.txt", DateTime.Now.ToStr() + " request" + mesg + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
            }

            return respo;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestarrive")]
        public string requestARRIVE(string mesg)
        {
            string respo = "true";
            JobActionEx objAction = null;
            try
            {
                try
                {
                    //
                    File.AppendAllText(AppContext.BaseDirectory + "\\requestarrive.txt", DateTime.Now.ToStr() + " request" + mesg + Environment.NewLine);
                }
                catch
                {

                }


                string dataValue = mesg;
                dataValue = dataValue.Trim();
                objAction = new JavaScriptSerializer().Deserialize<JobActionEx>(mesg);
                RemoveUK(ref objAction.Dropoff);

                long jobId = objAction.JobId.ToLong();
                using (TaxiDataContext db = new TaxiDataContext())
                {

                    try
                    {


                        if (true)
                        {
                            //
                            if (HubProcessor.Instance.objPolicy.RestrictMilesPOBAction.ToDecimal() > 0)

                            {


                                string dropOff = objAction.Dropoff.ToStr();
                                if (dropOff.Contains("<<<"))
                                {
                                    dropOff = dropOff.Remove(dropOff.IndexOf("<<<"));

                                }


                                string pickup = GetPostCodeMatch(dropOff);



                                var coord = General.GetObject<Gen_Coordinate>(c => c.PostCode == pickup);

                                decimal distance = 0.00m;


                                if (coord == null)
                                {
                                    try
                                    {

                                        var oobj = db.stp_getCoordinatesByAddress(pickup, pickup).FirstOrDefault();


                                        if (oobj != null && oobj.Latitude != null)
                                        {
                                            coord = new Gen_Coordinate();
                                            coord.Latitude = oobj.Latitude;
                                            coord.Longitude = oobj.Longtiude;
                                        }

                                    }
                                    catch
                                    {


                                    }

                                }

                                if (coord != null)
                                {
                                    distance = Math.Round(new DotNetCoords.LatLng(Convert.ToDouble(objAction.Latitude), Convert.ToDouble(objAction.Longitude)).DistanceMiles(new LatLng(Convert.ToDouble(coord.Latitude), Convert.ToDouble(coord.Longitude))).ToDecimal(), 1);


                                }

                                if (coord != null && distance > HubProcessor.Instance.objPolicy.RestrictMilesPOBAction.ToDecimal())
                                {

                                    int RemoveRestriction = 0;
                                    try
                                    {


                                        using (TaxiDataContext db2 = new TaxiDataContext())
                                        {
                                            db2.CommandTimeout = 3;

                                            int? objBooker = db2.Bookings.Where(c => c.Id == jobId)
                                            .Select(args => args.OnHoldWaitingMins).FirstOrDefault();

                                            if (objBooker != null)
                                            {
                                                //   
                                                RemoveRestriction = objBooker.ToInt();
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {


                                    }


                                    if (RemoveRestriction == 0)
                                    {
                                        objAction.Message = "false:You are far away from Pickup";

                                        respo = "false";
                                    }
                                    else
                                    {


                                    }

                                }

                            }
                        }



                        //  Clients.Caller.jobArrived(respo);

                        if (respo == "true")
                        {
                            (new TaxiDataContext()).stp_UpdateJob(jobId, objAction.DrvId.ToInt(), 6, 6, HubProcessor.Instance.objPolicy.SinBinTimer.ToInt());

                            if (HubProcessor.Instance.objPolicy.EnableArrivalBookingText.ToBool())
                            {
                                try
                                {
                                    Booking job = General.GetObject<Booking>(c => c.Id == jobId);

                                    if (job != null && job.JobCode.ToStr().Trim().Length == 0)
                                    {
                                        // onlineBookingId = job.OnlineBookingId.ToLong();
                                        if (!string.IsNullOrEmpty(job.CustomerMobileNo))
                                        {
                                            // ADDED ON 20/APRIL/2016 ON REQUEST OF COMMERCIAL CARS => DISABLE ARRIVAL TEXT FOR PARTICULAR ACCOUNT JOBS
                                            if (job.CompanyId == null || job.Gen_Company.DisableArrivalText.ToBool() == false)
                                            {

                                                string arrivalText = string.Empty;

                                                if (job.FromLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT)
                                                {
                                                    arrivalText = HubProcessor.Instance.objPolicy.ArrivalAirportBookingText.ToStr().Trim();
                                                }
                                                else
                                                {
                                                    arrivalText = HubProcessor.Instance.objPolicy.ArrivalBookingText.ToStr().Trim();
                                                }

                                                if (!string.IsNullOrEmpty(arrivalText))
                                                {

                                                    new System.Threading.Thread(delegate ()
                                                    {
                                                        try
                                                        {
                                                            AddSMS(job.CustomerMobileNo.ToStr().Trim(), GetMessage(arrivalText, job, jobId), job.SMSType.ToInt());
                                                        }
                                                        catch
                                                        {

                                                        }
                                                    }).Start();
                                                }
                                            }
                                        }
                                        else if (!string.IsNullOrEmpty(job.CustomerPhoneNo.ToStr().Trim()))
                                        {

                                            if (job.CompanyId == null || job.Gen_Company.DisableArrivalText.ToBool() == false)
                                            {

                                                string arrivalText = string.Empty;

                                                if (job.FromLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT)
                                                {
                                                    arrivalText = HubProcessor.Instance.objPolicy.ArrivalAirportBookingText.ToStr().Trim();
                                                }
                                                else
                                                {
                                                    arrivalText = HubProcessor.Instance.objPolicy.ArrivalBookingText.ToStr().Trim();
                                                }

                                                if (!string.IsNullOrEmpty(arrivalText))
                                                {

                                                    new System.Threading.Thread(delegate ()
                                                    {
                                                        try
                                                        {
                                                            AddSMS(job.CustomerPhoneNo.ToStr().Trim(), GetMessage(arrivalText, job, jobId), job.SMSType.ToInt());
                                                        }
                                                        catch { }
                                                    }).Start();
                                                }
                                            }



                                            // RingBackCall("Arrived Call to Customer", job.CustomerName.ToStr(), job.CustomerPhoneNo.ToStr().Trim());

                                        }
                                    }

                                }
                                catch
                                {

                                }


                            }
                            int? vehicleTypeId = 0;


                            Booking objBook = db.ExecuteQuery<Booking>("select Id,vehicletypeid,pickupdatetime,FromLocTypeId,FromAddress from booking where id=" + jobId).FirstOrDefault();

                            string query = string.Empty;
                            FareEx jobTariff = null;
                            try
                            {

                                vehicleTypeId = objBook.VehicleTypeId.ToInt();

                                int? fareId = null;

                                try
                                {
                                    fareId = db.ExecuteQuery<int?>("exec stp_GetFareId {0},{1},{2},{3},{4}", vehicleTypeId, 0, 0.00m, objBook.PickupDateTime, 1).FirstOrDefault();

                                    if (fareId.ToInt() > 0)
                                        query = "select StartRateValidMiles,FromDateTime,TillDateTime,FromSpecialDate,TillSpecialDate,DayValue,IsDayWise,WaitingCharges,WaitingChargesPerSeconds,WaitingSecondsFree from fare  where id=" + fareId;


                                    if (query.ToStr().Trim().Length > 0)
                                        jobTariff = db.ExecuteQuery<FareEx>(query).FirstOrDefault();

                                }
                                catch
                                {

                                }

                              

                          


                                objAction.objMeterTariff = new MeterTarrif();
                                if (jobTariff != null)
                                {
                                    objAction.objMeterTariff.FreeWaitingMins = jobTariff.WaitingSecondsFree.ToInt();
                                    objAction.objMeterTariff.WaitingSecondsToDivide = jobTariff.WaitingChargesPerSeconds.ToInt();
                                    objAction.objMeterTariff.DrvWaitingChargesPerMin = jobTariff.WaitingCharges.ToDecimal();


                                }



                                if (objAction.objMeterTariff.DrvWaitingChargesPerMin.ToDecimal() == 0)
                                {
                                    try
                                    {
                                        if (Global.listofMeter == null)
                                            Global.ReloadMeterList();

                                        var obj = Global.listofMeter.FirstOrDefault(c => c.VehicleTypeId == vehicleTypeId).DefaultIfEmpty();

                                        if (obj != null)
                                        {


                                            objAction.objMeterTariff.FreeWaitingMins = 0;
                                            objAction.objMeterTariff.WaitingSecondsToDivide = obj.AccWaitingChargesPerMin.ToInt();
                                            objAction.objMeterTariff.DrvWaitingChargesPerMin = obj.DrvWaitingChargesPerMin.ToDecimal();

                                        }
                                    }
                                    catch
                                    {

                                    }

                                }

                                AirportLocationData objAirportWaitingCharges = null;
                                if (objBook.FromLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT)
                                {
                                    string query2 = string.Empty;
                                    try
                                    {

                                        string locName = objBook.FromAddress.ToStr().Trim().Replace("  ", " ").Trim().Replace(",", "").Trim();
                                        query2 = "select l.LocationName, l.PostCode, e.AllowanceMins, e.WaitingSecondsFree, e.WaitingCharges, e.WaitingChargesPerSeconds from Gen_Syspolicy_LocationExpiry e inner join Gen_Locations l on e.LocationId = l.Id where replace( (replace(l.FullLocationName,'  ',' ')),',','')= '" + locName + "' and WaitingCharges is not null";


                                        objAirportWaitingCharges = db.ExecuteQuery<AirportLocationData>(query2).FirstOrDefault();

                                    }
                                    catch (Exception ex)
                                    {
                                        try
                                        {
                                            objAction.Message = ex.Message;
                                            File.AppendAllText(AppContext.BaseDirectory + "\\requestarrive_airportexception.txt", DateTime.Now.ToStr() + " request" + mesg + ",query=" + query2 + "exception:" + ex.Message + Environment.NewLine);
                                        }
                                        catch
                                        {

                                        }
                                    }

                                    if (objAirportWaitingCharges != null && objAirportWaitingCharges.WaitingCharges.ToDecimal() > 0)
                                    {
                                        try
                                        {



                                            objAction.objMeterTariff.FreeWaitingMins = objAirportWaitingCharges.WaitingSecondsFree.ToInt();
                                            objAction.objMeterTariff.WaitingSecondsToDivide = objAirportWaitingCharges.WaitingChargesPerSeconds.ToInt();
                                            objAction.objMeterTariff.DrvWaitingChargesPerMin = objAirportWaitingCharges.WaitingCharges.ToDecimal();


                                            try
                                            {

                                                File.AppendAllText(AppContext.BaseDirectory + "\\requestarrive_airportwaitingapplied.txt", DateTime.Now.ToStr() + " request" + mesg + ",query=" + query + Environment.NewLine);
                                            }
                                            catch
                                            {

                                            }



                                        }
                                        catch
                                        {

                                        }


                                    }

                                }

                                if (respo == "true")
                                {
                                    General.BroadCastMessage("**action>>" + objAction.JobId.ToStr() + ">>" + objAction.DrvId.ToStr() + ">>6");
                                }

                                respo = new JavaScriptSerializer().Serialize(objAction);


                            }
                            catch (Exception ex2)
                            {
                                try
                                {
                                    objAction.Message = ex2.Message;
                                    File.AppendAllText(AppContext.BaseDirectory + "\\requestarrive_innerexception.txt", DateTime.Now.ToStr() + " request" + mesg + ",query=" + query + "exception:" + ex2.Message + Environment.NewLine);
                                }
                                catch
                                {

                                }
                            }
                            //






                            //









                        }
                        else
                        {
                            respo = objAction.Message;

                        }
                        // if(respo.StartsWith("failed")==false)

                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            objAction.Message = ex.Message;
                            File.AppendAllText(AppContext.BaseDirectory + "\\requestarrive_exception1.txt", DateTime.Now.ToStr() + " request" + mesg + ",exception:" + ex.Message + Environment.NewLine);
                        }
                        catch
                        {

                        }
                    }

                }

            }

            catch (Exception ex)
            {
                try
                {
                    objAction.Message = ex.Message;
                    File.AppendAllText(AppContext.BaseDirectory + "\\requestarrive_exception2.txt", DateTime.Now.ToStr() + " request" + mesg + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
            }

            return respo;
        }







        private string GetMessage(string message, Booking objBooking, long jobId)
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
                                                string encrypt = Cryptography.Encrypt(objBooking.BookingNo.ToStr() + ":" + linkId + ":" + "Data Source=157.90.210.155,58525;Initial Catalog=BurnhamCars;User ID=bur321;Password=bur321!;Trusted_Connection=False;" + ":" + objBooking.Id, "tcloudX@@!", true);


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
               
                // ENUtils.ShowMessage(ex.Message);
                return "";
            }
        }

        protected string ToTinyURLS(string txt)
        {
            Regex regx = new Regex("http://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,]*)?", RegexOptions.IgnoreCase);

            MatchCollection mactches = regx.Matches(txt);

            foreach (Match match in mactches)
            {
                string tURL = MakeTinyUrl(match.Value);
                txt = txt.Replace(match.Value, tURL);
            }

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


        private void AddSMS(string mobileNo, string message, int smsType)
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

            }
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestrevertstatus")]
        public JobModel requestrevertstatus(string mesg)
        {
            JobModel obj = null;
            try
            {

                try
                {

                    try
                    {
                        File.AppendAllText(AppContext.BaseDirectory + "\\requestrevertstatus.txt", DateTime.Now.ToStr() + " request" + mesg + Environment.NewLine);
                    }
                    catch
                    {

                    }
                    obj = new JavaScriptSerializer().Deserialize<JobModel>(mesg);

                    var arr = obj.revertstatus.ToStr().Split('|');


                    try
                    {
                        //       revertStatus = ",\"revertstatus\":\"" + (objMaster.Current.Id.ToStr() + "|" + objMaster.Current.DriverId.ToStr() + "|" + objMaster.Current.DropOffZoneId.ToInt().ToStr() + "|" + objMaster.Current.BookingStatusId.ToStr() + "|" + AppVars.LoginObj.UserName.ToStr()) + "\"";


                        using (TaxiDataContext db = new TaxiDataContext())
                        {
                            db.ExecuteQuery<int>("exec stp_UpdateRevertDriverPlotByJob {0},{1},{2},{3},{4},{5},{6},{7}", obj.JobId, obj.DriverId.ToInt(), obj.DriverNo.ToStr(), arr[2].ToInt(), "", "", arr[3].ToInt(), arr[4].ToStr());

                        }

                        if (arr[3].ToInt() == 8)
                        {
                            obj.revertstatus = "pob";
                            Global.AddSTCReminder(obj.JobId, obj.DriverId.ToInt());
                            General.BroadCastMessage("**action>>" + obj.JobId.ToStr() + ">>" + obj.DriverId.ToStr() + ">>" + Enums.BOOKINGSTATUS.POB);

                        }
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            File.AppendAllText(AppContext.BaseDirectory + "\\requestrevertstatus_exception1.txt", DateTime.Now.ToStr() + " request" + mesg + ",exception:" + ex.Message + Environment.NewLine);
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
                        File.AppendAllText(AppContext.BaseDirectory + "\\requestrevertstatus_exception.txt", DateTime.Now.ToStr() + " request" + mesg + ",exception:" + ex.Message + Environment.NewLine);
                    }
                    catch

                    {
                    }



                }
            }
            catch
            {

            }

            return obj;
        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("ResetApp")]
        public string ResetApp(string value)
        {


            try
            {
                //

                File.AppendAllText(AppContext.BaseDirectory + "\\ResetApp.txt", DateTime.Now.ToStr() + " request" + value + Environment.NewLine);
            
            }
            catch
            {

            }
            return "true";

        }

            [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("changeAutoDispatchMode")]
        public string ChangeAutoDispatchMode(string value)
        {


            try
            {

                File.AppendAllText(AppContext.BaseDirectory + "\\ChangeAutoDispatchMode.txt", DateTime.Now.ToStr() + " request" + value + Environment.NewLine);
            }
            catch
            {

            }
            //
            string rtn = "";

            try
            {
                string[] arr = value.Split('|');



                using (TaxiDataContext db = new TaxiDataContext())
                {

                    if (arr[0].ToStr().StartsWith("modify"))
                    {



                        Global.AutoDispatchSetting = db.ExecuteQuery<AutoDispatchSetting>("SELECT * FROM autodispatchsettings where autodispatchmodetype=" + HubProcessor.Instance.objPolicy.AutoDespatchDriverCategoryPriority).FirstOrDefault();
                    }
                    else
                    {
                        int modeType = arr[0].ToInt();
                        db.ExecuteQuery<int>("update gen_syspolicy_configurations set AutoDespatchDriverCategoryPriority=" + modeType);
                        Global.AutoDispatchSetting = db.ExecuteQuery<AutoDispatchSetting>("SELECT * FROM autodispatchsettings where autodispatchmodetype=" + modeType).FirstOrDefault();
                        HubProcessor.Instance.objPolicy.AutoDespatchDriverCategoryPriority = modeType;
                        value = value.Replace("|", ">>");

                        General.BroadCastMessage("**modifyautodispatchmode>>" + value);

                    }

                }


                rtn = "success";
            }
            catch (Exception ex)
            {
                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\ChangeAutoDispatchMode_excpetion.txt", DateTime.Now.ToStr() + " request" + value.ToStr() + ",response:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
                rtn = ex.Message;
            }
            //
            return rtn;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("acceptfoj")]
        public string Acceptfoj(string mesg)
        {
            string IsAvalable = "false";
            //
            try
            {
             

                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

              

                if (new TaxiDataContext().Bookings.Count(c => c.Id == values[1].ToLong() && c.DriverId == values[2].ToInt()) > 0)
                    IsAvalable = "true";

              

                int jobstatusId = values[3].ToInt();

                if (IsAvalable == "true")
                {
                    new TaxiDataContext().stp_UpdateJobStatus(values[1].ToLong(), jobstatusId);

                    General.BroadCastMessage("**action>>" + values[1].ToStr() + ">>" + values[2].ToStr() + ">>" + values[3].ToInt());

                    if (HubProcessor.Instance.listofJobs.Count(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong()) > 0)
                    {
                        HubProcessor.Instance.listofJobs.RemoveAll(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong());
                    }
                }
            }
            catch (Exception ex)
            {
               
            }


            return IsAvalable;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestplots")]
        public string requestPlots(string mesg)
        {
            try
            {
              

                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                string driverPlot = "";
                string[] arr = null;

                using (TaxiDataContext db = new TaxiDataContext())
                {
                    try
                    {
                        db.CommandTimeout = 4;

                        string driverId = values[1].ToStr().Trim();

                        
                      

                        var result = db.stp_GetAreaPlotsByVehicle(driverId.ToInt()
                                ,HubProcessor.Instance.objPolicy.PlotsJobExpiryValue1, HubProcessor.Instance.objPolicy.PlotsJobExpiryValue2);



                       
                           

                           
                                arr = (from a in result
                                       orderby a.Drivers descending, a.orderno
                                       select (a.ZoneName + "," +
                                                a.ExpiryJobs1 + "," + a.ExpiryJobs2
                                           + "," + a.Drivers)).ToArray<string>();
                           
                           
                       
                      
                    }
                    catch (Exception ex)
                    {
                        //try
                        //{
                        //    File.AppendAllText(applic + "\\exception_plots.txt", DateTime.Now.ToStr() + ": " + ex.Message + Environment.NewLine);
                        //}
                        //catch
                        //{

                        //}
                    }
                }

                //send message back to PDA
               

                return (driverPlot + string.Join(">>", arr));
               
            }
            catch (Exception ex)
            {
                return "exceptionoccurred";
            }
        }


     


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestsendingmessage")]
        public string requestSendingMessage(string msg)
        {
            string rtn = "false";
            try
            {
              
                string dataValue = msg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                if (values.Count() >= 7)
                {
                    //send acknowledgement message to PDA
                    rtn="true";

                   
                }

                (new TaxiDataContext()).stp_SendMessage(values[1].ToInt(), values[2].ToInt(), values[3].ToStr(), "", values[4].ToStr(), values[5].ToStr());
                General.BroadCastMessage("**message>>" + values[1].ToStr() + ">>" + values[3].ToStr() + ">>" + values[4].ToStr());
            }
            catch (Exception ex)
            {
                rtn = "exceptionoccurred";
            }

            return rtn;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestlogout")]
        public string requestLogout(string dataValue)
        {
            string rtn = "";
            try
            {
                string[] values = dataValue.Split(new char[] { '=' });

              

                if (values.Count() >= 9)
                {
                    (new TaxiDataContext()).stp_LogoutDriverPenalty(values[1].ToInt(), false, true);
                    General.BroadCastMessage("**logout>>" + values[1] + ">>" + values[7] + ">>Driver " + values[7] + " is Logout(OverBreak)");
                }
                else
                {
                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        db.stp_LoginLogoutDriver(values[1].ToInt(), false, null);
                    }

                    General.BroadCastMessage("**logout>>Driver " + values[2] + " is Logout");
                }

              

                rtn = "true";
            }
            catch (Exception ex)
            {
                rtn = "exceptionoccurred";
            }

            return rtn;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("actionbutton")]
        public string actionButton(string mesg)
        {

            string respo = "true";
            string dataValue = mesg;
            dataValue = dataValue.Trim();

            string[] values = dataValue.Split(new char[] { '=' });

            int valCnt = values.Count();

            try
            {

                int jobStatusId = values[3].ToInt();


                if (valCnt >= 7)
                {






                    if (jobStatusId == Enums.BOOKINGSTATUS.ONROUTE)
                    {
                        int driverId = values[2].ToInt();

                        using (TaxiDataContext db = new TaxiDataContext())
                        {
                            if (dataValue.ToStr().Contains("verify"))
                            {
                                if ((db.Bookings.Count(c => c.Id == values[1].ToLong()
                                    && (c.BookingStatusId == Enums.BOOKINGSTATUS.PENDING || c.BookingStatusId == Enums.BOOKINGSTATUS.NOTACCEPTED
                                    || c.BookingStatusId == Enums.BOOKINGSTATUS.BID
                                    || c.BookingStatusId == Enums.BOOKINGSTATUS.FOJ
                                    || c.BookingStatusId == Enums.BOOKINGSTATUS.PENDING_START
                                    || (c.BookingStatusId == Enums.BOOKINGSTATUS.ONROUTE && c.DriverId == driverId)
                                    )) > 0)
                                    )
                                {
                                    if (db.Fleet_DriverQueueLists.Count(c => c.Status == true && c.DriverId != values[2].ToInt() && c.CurrentJobId == values[1].ToLong()
                                        && c.DriverWorkStatusId == Enums.Driver_WORKINGSTATUS.ONROUTE) > 0)
                                    {
                                        respo = "false:This Job is no longer available";

                                        db.stp_UpdateJobStatus(values[1].ToLong(), Enums.BOOKINGSTATUS.ONROUTE);

                                        General.BroadCastMessage("refresh active dashboard");

                                        //try
                                        //{
                                        //    File.AppendAllText("settledoubledrvlog.txt", "datastring:" + dataValue + " on time :" + DateTime.Now.ToStr());
                                        //}
                                        //catch
                                        //{

                                        //}
                                    }
                                    else
                                    {
                                        db.stp_UpdateJob(values[1].ToLong(), values[2].ToInt(), values[3].ToInt(), values[4].ToInt(), HubProcessor.Instance.objPolicy.SinBinTimer.ToInt());
                                    }
                                }
                                else
                                {
                                    respo = "false:This Job is no longer available";

                                    //try
                                    //{
                                    //    File.AppendAllText("settledoubledrvlogcond2.txt", "datastring:" + dataValue + " on time :" + DateTime.Now.ToStr());
                                    //}
                                    //catch
                                    //{

                                    //}
                                }
                            }
                            else
                            {
                                db.stp_UpdateJob(values[1].ToLong(), values[2].ToInt(), values[3].ToInt(), values[4].ToInt(), HubProcessor.Instance.objPolicy.SinBinTimer.ToInt());
                            }

                            //send message back to PDA
                            //  Clients.Caller.jobAccepted(respo);
                            respo = "true";
                            //  DispatchJobSMS(values[1].ToLong(), jobStatusId);

                        }
                    }


                    if (jobStatusId == Enums.BOOKINGSTATUS.DISPATCHED)
                    {

                        (new TaxiDataContext()).stp_UpdateJobAndRoute(values[1].ToLong(), values[2].ToInt(), values[3].ToInt(), values[4].ToInt(), "", values[5].ToDecimal(), null);
                    }
                    else
                    {
                        if (jobStatusId != Enums.BOOKINGSTATUS.ONROUTE && jobStatusId != Enums.BOOKINGSTATUS.ARRIVED && jobStatusId != Enums.BOOKINGSTATUS.STC)
                        {
                            (new TaxiDataContext()).stp_UpdateJob(values[1].ToLong(), values[2].ToInt(), values[3].ToInt(), values[4].ToInt(), HubProcessor.Instance.objPolicy.SinBinTimer.ToInt());

                        }

                        if (jobStatusId == Enums.BOOKINGSTATUS.ARRIVED)
                        {


                            try
                            {


                                if (dataValue.ToStr().Contains("=jsonstring|") && values[7].ToStr().Contains("jsonstring|"))
                                {

                                    if (HubProcessor.Instance.objPolicy.RestrictMilesPOBAction.ToDecimal() > 0)

                                    {
                                        //    objAction.Dropoff.ToStr().Trim().Length > 0 && objAction.DrvNo.ToStr().Trim().Length > 0)
                                        string json = values[7].ToStr().Replace("jsonstring|", "").Trim();
                                        JobAction objAction = new JavaScriptSerializer().Deserialize<JobAction>(json);

                                        string dropOff = objAction.Dropoff.ToStr();
                                        if (dropOff.Contains("<<<"))
                                        {
                                            dropOff = dropOff.Remove(dropOff.IndexOf("<<<"));

                                        }


                                        string pickup = GetPostCodeMatch(dropOff);



                                        var coord = General.GetObject<Gen_Coordinate>(c => c.PostCode == pickup);

                                        decimal distance = 0.00m;


                                        if (coord == null)
                                        {
                                            try
                                            {
                                                using (TaxiDataContext db = new TaxiDataContext())
                                                {
                                                    var oobj = db.stp_getCoordinatesByAddress(pickup, pickup).FirstOrDefault();


                                                    if (oobj != null && oobj.Latitude != null)
                                                    {
                                                        coord = new Gen_Coordinate();
                                                        coord.Latitude = oobj.Latitude;
                                                        coord.Longitude = oobj.Longtiude;
                                                    }
                                                }
                                            }
                                            catch
                                            {


                                            }

                                        }

                                        if (coord != null)
                                        {
                                            distance = Math.Round(new DotNetCoords.LatLng(Convert.ToDouble(objAction.Latitude), Convert.ToDouble(objAction.Longitude)).DistanceMiles(new LatLng(Convert.ToDouble(coord.Latitude), Convert.ToDouble(coord.Longitude))).ToDecimal(), 1);


                                        }

                                        if (coord != null && distance > HubProcessor.Instance.objPolicy.RestrictMilesPOBAction.ToDecimal())
                                        {

                                            int RemoveRestriction = 0;
                                            try
                                            {

                                                long jobId = values[1].ToLong();
                                                using (TaxiDataContext db2 = new TaxiDataContext())
                                                {
                                                    db2.CommandTimeout = 3;

                                                    int? objBooker = db2.Bookings.Where(c => c.Id == jobId)
                                                    .Select(args => args.OnHoldWaitingMins).FirstOrDefault();

                                                    if (objBooker != null)
                                                    {
                                                        //   
                                                        RemoveRestriction = objBooker.ToInt();
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {


                                            }


                                            if (RemoveRestriction == 0)
                                            {
                                                respo = "false:You are far away from Pickup";

                                                //try
                                                //{

                                                //    File.AppendAllText(physicalPath + "\\restrictionlog.txt", DateTime.Now + ": datavalue=" + dataValue + ",(arrivejob)distance to : " + pickup + ",distance:" + distance + Environment.NewLine);
                                                //}
                                                //catch
                                                //{


                                                //}
                                            }
                                            else
                                            {


                                            }

                                        }

                                    }
                                }


                                respo = "true";
                                //  Clients.Caller.jobArrived(respo);

                                if (respo == "true")
                                {
                                    (new TaxiDataContext()).stp_UpdateJob(values[1].ToLong(), values[2].ToInt(), values[3].ToInt(), values[4].ToInt(), HubProcessor.Instance.objPolicy.SinBinTimer.ToInt());

                                    //  DispatchJobSMS(values[1].ToLong(), jobStatusId);

                                }
                                // if(respo.StartsWith("failed")==false)

                            }
                            catch (Exception ex)
                            {
                                // Clients.Caller.exceptionOccured(ex.Message);
                                // Clients.Caller.jobArrived("exceptionoccurred");
                                //try
                                //{

                                //    File.AppendAllText(physicalPath + "\\arrived_exception.txt", DateTime.Now.ToStr() + " request" + dataValue + ",exception:" + ex.Message + Environment.NewLine);
                                //}
                                //catch
                                //{

                                //}
                            }
                        }
                        else if (jobStatusId == Enums.BOOKINGSTATUS.STC)
                        {


                            string isAuto = "";
                            string driverNo = "";

                            if (dataValue.ToStr().Contains("=jsonstring|") && values[7].ToStr().Contains("jsonstring|"))
                            {
                                string json = values[7].ToStr().Replace("jsonstring|", "").Trim();
                                JobAction objAction = new JavaScriptSerializer().Deserialize<JobAction>(json);



                                isAuto = objAction.IsAuto.ToStr().Trim();
                                driverNo = objAction.DrvNo.ToStr();
                                if (HubProcessor.Instance.objPolicy.RestrictMilesOnSTC.ToDecimal() > 0)

                                {

                                

                                    if (HubProcessor.Instance.objPolicy.RestrictMilesOnSTC.ToDecimal() > 0 && isAuto.ToStr() != "1" && objAction.Dropoff.ToStr().Trim().Length > 0 && objAction.DrvNo.ToStr().Trim().Length > 0)
                                    {
                                        int RemoveRestriction = 0;

                                        string dropOff = objAction.Dropoff.ToStr();
                                        if (dropOff.Contains("<<<"))
                                        {
                                            dropOff = dropOff.Remove(dropOff.IndexOf("<<<"));

                                        }


                                        using (TaxiDataContext db = new TaxiDataContext())
                                        {
                                            db.CommandTimeout = 3;

                                            string pickup = "";
                                            string destination = GetPostCodeMatch(dropOff);

                                            int journeyTypeId = 0;
                                            int pickupZoneId = 0;
                                            int dropOffZoneId = 0;
                                            try
                                            {

                                                long jobId = values[1].ToLong();

                                                var objBooker = db.Bookings.Select(args => new {
                                                    args.Id,
                                                    args.JourneyTypeId,
                                                    args.FromAddress,
                                                    args.ZoneId,
                                                    args.ToAddress,
                                                    args.DropOffZoneId,
                                                    args.OnHoldWaitingMins
                                                })
                                                                         .FirstOrDefault(c => c.Id == jobId);

                                                if (objBooker != null)
                                                {
                                                    pickup = objBooker.FromAddress.ToStr().Trim();
                                                    destination = objBooker.ToAddress.ToStr().Trim().ToUpper();
                                                    journeyTypeId = objBooker.JourneyTypeId.ToInt();
                                                    pickupZoneId = objBooker.ZoneId.ToInt();
                                                    dropOffZoneId = objBooker.DropOffZoneId.ToInt();
                                                    RemoveRestriction = objBooker.OnHoldWaitingMins.ToInt();
                                                }

                                            }
                                            catch
                                            {


                                            }



                                            stp_getCoordinatesByAddressResult coord = null;


                                            if (journeyTypeId == Enums.JOURNEY_TYPES.WAITANDRETURN)
                                            {

                                                destination = GetPostCodeMatch(pickup);
                                                coord = db.stp_getCoordinatesByAddress(destination, destination).FirstOrDefault();
                                                dropOffZoneId = pickupZoneId;
                                            }
                                            else
                                            {
                                                destination = GetPostCodeMatch(destination);
                                                coord = db.stp_getCoordinatesByAddress(destination, destination).FirstOrDefault();

                                            }




                                            decimal distance = 0.00m;

                                            if (coord != null && coord.Latitude != null && coord.Latitude != 0)
                                            {
                                                distance = Math.Round(new DotNetCoords.LatLng(Convert.ToDouble(objAction.Latitude), Convert.ToDouble(objAction.Longitude)).DistanceMiles(new LatLng(Convert.ToDouble(coord.Latitude), Convert.ToDouble(coord.Longtiude))).ToDecimal(), 1);


                                            }


                                            if (coord == null)
                                            {


                                                bool isfound = true;


                                                if (dropOffZoneId != 0)
                                                {
                                                    isfound = FindPoint(Convert.ToDouble(objAction.Latitude), Convert.ToDouble(objAction.Longitude), db.Gen_Zone_PolyVertices.Where(c => c.ZoneId == dropOffZoneId).ToList());

                                                    if (isfound)
                                                    {
                                                        coord = null;
                                                    }
                                                    else
                                                    {
                                                        coord = new stp_getCoordinatesByAddressResult();
                                                        distance = -1;

                                                    }

                                                }


                                                //  }                                    


                                            }




                                            if (distance == -1 || (coord != null && distance > HubProcessor.Instance.objPolicy.RestrictMilesOnSTC.ToDecimal()))
                                            {
                                                if (RemoveRestriction == 1)
                                                {

                                                    //try
                                                    //{

                                                    //    File.AppendAllText(physicalPath + "//restrictionRemoved.txt", DateTime.Now + ": datavalue=" + dataValue + ",(stcjob)distance to : " + pickup + ",distance:" + distance + Environment.NewLine);
                                                    //}
                                                    //catch
                                                    //{


                                                    //}


                                                }
                                                else
                                                {
                                                    respo = "false:You are far away from Destination";

                                                    //try
                                                    //{

                                                    //    File.AppendAllText(physicalPath + "//restrictionlog.txt", DateTime.Now + ": datavalue=" + dataValue + ",(stcjob)distance to : " + pickup + ",distance:" + distance + Environment.NewLine);
                                                    //}
                                                    //catch
                                                    //{


                                                    //}
                                                }

                                            }






                                            //

                                        }
                                    }




                                }

                            }



                            //  Clients.Caller.jobStc(respo);
                            respo = "true";

                            if (respo == "true")
                            {
                                using (TaxiDataContext db = new TaxiDataContext())
                                {
                                    try
                                    {
                                        db.stp_UpdateJob(values[1].ToLong(), values[2].ToInt(), values[3].ToInt(), values[4].ToInt(), HubProcessor.Instance.objPolicy.SinBinTimer.ToInt());

                                        if (isAuto.ToStr() == "1")
                                        {
                                            try
                                            {
                                                long currjobId = values[1].ToLong();


                                                string query = "INSERT INTO BOOKING_LOG (bookingid,[user],beforeupdate,afterupdate,updatedate)values(" + currjobId + ",'" + "Driver" + "','" + "" + "','" + "Driver " + driverNo + " Auto STC" + "',getdate());";
                                                db.ExecuteQuery<int>(query);

                                            }
                                            catch (Exception ex)
                                            {
                                                try
                                                {
                                                    //
                                                    //
                                                    File.AppendAllText(AppContext.BaseDirectory + "\\autostclog_exception.txt", DateTime.Now.ToStr() + ": request" + mesg + ",exception:" + ex.Message + Environment.NewLine);
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
                                }

                            }

                            //   }
                        }
                        else if (jobStatusId == Enums.BOOKINGSTATUS.NOPICKUP)
                        {
                            //send message back to PDA
                            // Clients.Caller.noPickup(respo);
                            respo = "true";
                        }
                    }
                }
                else if (valCnt == 6)
                {
                    if (dataValue.Contains("ACK"))
                    {
                        //send message back to PDA
                        //  Clients.Caller.jobReject("true");
                        respo = "true";

                        (new TaxiDataContext()).stp_UpdateJobAndRoute(values[1].ToLong(), values[2].ToInt(), values[3].ToInt(), values[4].ToInt(), "", 0, null);
                    }
                    else
                    {
                        //try
                        //{

                        //    File.AppendAllText(physicalPath + "\\called2.txt", DateTime.Now.ToStr() + " request" + dataValue + Environment.NewLine);
                        //}
                        //catch
                        //{

                        //}
                        (new TaxiDataContext()).stp_UpdateJobAndRoute(values[1].ToLong(), values[2].ToInt(), values[3].ToInt(), values[4].ToInt(), "", values[5].ToDecimal(), null);
                    }
                }
                else
                {
                    if (values[3].ToInt() == Enums.BOOKINGSTATUS.NOTACCEPTED && HubProcessor.Instance.objPolicy.EnableFOJ.ToBool())
                    {
                        new TaxiDataContext().stp_UpdateJobStatus(values[1].ToLong(), values[3].ToInt());
                    }
                    else
                    {
                        //try
                        //{

                        //    File.AppendAllText(physicalPath + "\\called3.txt", DateTime.Now.ToStr() + " request" + dataValue + Environment.NewLine);
                        //}
                        //catch
                        //{

                        //}
                        (new TaxiDataContext()).stp_UpdateJob(values[1].ToLong(), values[2].ToInt(), values[3].ToInt(), values[4].ToInt(), HubProcessor.Instance.objPolicy.SinBinTimer.ToInt());
                    }

                    //send message back to PDA
                    //   Clients.Caller.jobNotAccepted("true");
                    respo = "true";
                }



                try
                {
                    if (respo == "true")
                    {
                        General.BroadCastMessage("**action>>" + values[1].ToStr() + ">>" + values[2].ToStr() + ">>" + values[3].ToInt());
                    }

                    if (HubProcessor.Instance.listofJobs.Count(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong()) > 0)
                    {
                        HubProcessor.Instance.listofJobs.RemoveAll(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong());
                    }

                    HubProcessor.Instance.listofJobs.RemoveAll(c => c.DriverId == values[2].ToInt() && c.JobId != 0 && (c.MessageTypeId == 9 || c.MessageTypeId == 1));
                }
                catch
                {

                }
            }
            catch (Exception ex)
            {
                try
                {
                    //   File.AppendAllText(physicalPath + "\\actionbutton_exception.txt", DateTime.Now + ": datavalue=" + dataValue + ",exception" + ex.Message);
                    HubProcessor.Instance.listofJobs.RemoveAll(c => c.DriverId == values[2].ToInt() && c.JobId == values[1].ToLong());
                    // Clients.Caller.exceptionOccured(ex.Message);
                }
                catch (Exception e)
                {
                    //  Clients.Caller.exceptionOccured(e.Message);
                }
            }

            return respo;
        }




        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestjoblist")]
        public JobObject requestJobList(string mesg)
        {
            JobObject obj = null;

            try
            {

                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\requestJobList.txt", DateTime.Now.ToStr() + " request" + mesg + Environment.NewLine);
                }
                catch
                {

                }


                obj = new JavaScriptSerializer().Deserialize<JobObject>(mesg);



                int driverId = obj.DrvId.ToInt();



                bool shiftJobs = true;
                //
                DateTime fromDate = DateTime.Now;
                DateTime tillDate = DateTime.Now;

                if (obj.FilterFrom.ToStr().Trim().Length > 0)
                {
                    DateTime.TryParseExact(obj.FilterFrom.ToStr().Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDate);
                    // from = fromDate;

                    shiftJobs = false;
                }

                if (obj.FilterTo.ToStr().Trim().Length > 0)
                {
                    //
                    DateTime.TryParseExact(obj.FilterTo.ToStr().Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out tillDate);
                    tillDate = tillDate.AddDays(1).ToDate();
                    //  till = tillDate;
                    //  till = till.Value.AddDays(1).ToDate();
                    shiftJobs = false;
                }

                if (tillDate.Subtract(fromDate).TotalDays > 30)
                {

                    obj.Message = "You cannot filter more than 30 days Records";

                    obj.JobSummary = new JobSummaryModel();

                    if (obj.JobSummary.TotalCash == null)
                    {

                        obj.JobSummary.TotalCash = 0.00m;
                        obj.JobSummary.TotalCashJobs = 0;
                        obj.JobSummary.TotalCharges = 0.00m;
                        obj.JobSummary.Fares = 0.00m;
                        obj.JobSummary.AgentFee = 0.00m;
                        obj.JobSummary.BookingFee = 0.00m;
                        obj.JobSummary.Congestion = 0.00m;
                        obj.JobSummary.Extra = 0.00m;
                        obj.JobSummary.Parking = 0.00m;
                        obj.JobSummary.Waiting = 0.00m;
                        obj.JobSummary.TotalAccount = 0.00m;
                        obj.JobSummary.TotalAccountJobs = 0;
                        obj.JobSummary.TotalCard = 0.00m;
                        obj.JobSummary.TotalCashJobs = 0;

                    }

                    obj.Joblist = new List<JobModel>();


                }

                if (obj.Message.ToStr().Trim().Length == 0)
                {
                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        ////



                        if (obj.FetchType == 1)
                        {
                            var objlist = db.ExecuteQuery<JobSummaryModel>("exec stp_getjobslistSummary {0},{1},{2},{3},{4},{5}", driverId, fromDate, tillDate, 0, 0, shiftJobs).FirstOrDefault();
                            //
                            obj.JobSummary = objlist;


                            if (obj.JobSummary != null)
                            {

                                if (obj.JobSummary.TotalCash == null)
                                {

                                    obj.JobSummary.TotalCash = 0.00m;
                                    obj.JobSummary.TotalCashJobs = 0;
                                    obj.JobSummary.TotalCharges = 0.00m;
                                    obj.JobSummary.Fares = 0.00m;
                                    obj.JobSummary.AgentFee = 0.00m;
                                    obj.JobSummary.BookingFee = 0.00m;
                                    obj.JobSummary.Congestion = 0.00m;
                                    obj.JobSummary.Extra = 0.00m;
                                    obj.JobSummary.Parking = 0.00m;
                                    obj.JobSummary.Waiting = 0.00m;
                                    obj.JobSummary.TotalAccount = 0.00m;
                                    obj.JobSummary.TotalAccountJobs = 0;
                                    obj.JobSummary.TotalCard = 0.00m;
                                    obj.JobSummary.TotalCashJobs = 0;

                                }

                            }

                        }
                        else
                        {
                            List<JobModel> list = null;
                            if (obj.JobId == 0)
                                list = db.ExecuteQuery<JobModel>("exec stp_getjobslist {0},{1},{2},{3},{4},{5}", driverId, fromDate, tillDate, 0, 0, shiftJobs).ToList();

                            else
                                list = db.ExecuteQuery<JobModel>("exec stp_getjobdata {0},{1}", driverId, obj.JobId).ToList();

                            obj.Joblist = list;


                            //if (obj.Joblist != null && obj.Joblist.Count > 0)
                            //{

                            //    for (int x= 0; x<500; x++)
                            //    {
                            //        obj.Joblist.Add(obj.Joblist[0]);
                            //    }


                            //}
                        }
                    }


                }

            }
            catch (Exception ex)
            {
                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\requestJobList_exception.txt", DateTime.Now.ToStr() + " request" + mesg + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }

            }
            return obj;
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestfuturejoblist")]
        public string requestFutureJobList(string mesg)
        {

            try
            {
                //

                string dataValue = mesg;
                dataValue = dataValue.Trim();

                string[] values = dataValue.Split(new char[] { '=' });

                int driverId = values[0].ToInt();
                string driverNo = values[1].ToStr();

                using (TaxiDataContext db = new TaxiDataContext())
                {
                    db.CommandTimeout = 5;
                    var bookings = db.Bookings.Where(c => c.DriverId == driverId && c.BookingStatusId == Enums.BOOKINGSTATUS.PENDING_START)
                        .OrderBy(c => c.PickupDateTime).ToList();


                    StringBuilder s = new StringBuilder();
                    foreach (var objBooking in bookings)

                    {
                        if (objBooking != null)
                        {

                            string customerMobileNo = objBooking.CustomerMobileNo.ToStr().Trim();

                            string customerName = objBooking.CustomerName;

                            string via = string.Join(",", objBooking.Booking_ViaLocations.Select(c => c.ViaLocValue.ToStr()).ToArray<string>());

                            if (!string.IsNullOrEmpty(via.Trim()))
                                via = "Via: " + via;

                            string specialReq = objBooking.SpecialRequirements.ToStr().Trim();
                            if (!string.IsNullOrEmpty(specialReq))
                                specialReq = "Special Req: " + specialReq;




                            string custNo = !string.IsNullOrEmpty(objBooking.CustomerMobileNo) ? objBooking.CustomerMobileNo : objBooking.CustomerPhoneNo;



                            // Send To Driver






                            string paymentType = objBooking.Gen_PaymentType.PaymentCategoryId == null ? objBooking.Gen_PaymentType.DefaultIfEmpty().PaymentType.ToStr()
                                     : objBooking.Gen_PaymentType.Gen_PaymentCategory.CategoryName.ToStr();



                            //  string strDeviceRegistrationId = ObjDriver.DeviceId.ToStr();
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




                            if (objBooking.CompanyId != null)
                                companyName = objBooking.Gen_Company.DefaultIfEmpty().CompanyName.ToStr();





                            pickUpPlot = objBooking.ZoneId != null ? "<<<" + objBooking.Gen_Zone1.ZoneName.ToStr() : "";
                            dropOffPlot = objBooking.DropOffZoneId != null ? "<<<" + objBooking.Gen_Zone.ZoneName.ToStr() : "";




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




                            decimal pdafares = objBooking.GetType().GetProperty(HubProcessor.Instance.objPolicy.PDAFaresPropertyName.ToStr().Trim()).GetValue(objBooking, null).ToDecimal();


                            string msg = string.Empty;




                            string showFaresValue = objBooking.IsQuotedPrice.ToBool() == true ? "1" : objBooking.Gen_PaymentType.ShowFaresOnPDA.ToStr().Trim();

                            string showFares = ",\"ShowFares\":\"" + showFaresValue + "\"";
                            string showSummary = ",\"ShowSummary\":\"" + showFaresValue + "\"";

                            string agentDetails = string.Empty;
                            string parkingandWaiting = string.Empty;
                            if (objBooking.CompanyId != null)
                            {
                                agentDetails = ",\"AgentFees\":\"" + String.Format("{0:0.00}", objBooking.AgentCommission) + "\"";
                                parkingandWaiting = ",\"Parking\":\"" + string.Format("{0:0.00}", objBooking.ParkingCharges) + "\",\"Waiting\":\"" + String.Format("{0:0.00}", objBooking.WaitingCharges) + "\"";

                            }
                            else
                            {

                                parkingandWaiting = ",\"Parking\":\"" + string.Format("{0:0.00}", objBooking.CongtionCharges) + "\",\"Waiting\":\"" + String.Format("{0:0.00}", objBooking.MeetAndGreetCharges) + "\"";
                                //

                            }




                            msg = "{ \"JobId\" :\"" + objBooking.Id +
                                  "\", \"Pickup\":\"" + (!string.IsNullOrEmpty(objBooking.FromDoorNo) ? fromdoorno + "-" + fromAddress + pickUpPlot : fromAddress + pickUpPlot) +
                                  "\", \"Destination\":\"" + (!string.IsNullOrEmpty(objBooking.ToDoorNo) ? objBooking.ToDoorNo + "-" + toAddress + dropOffPlot : toAddress + dropOffPlot) + "\"," +
                                  "\"PickupDateTime\":\"" + string.Format("{0:dd/MM/yyyy   HH:mm}", objBooking.PickupDateTime) + "\"" +
                                  ",\"Cust\":\"" + objBooking.CustomerName + "\",\"Mob\":\"" + mobileNo + " " + "\",\"Fare\":\"" + pdafares + "\",\"Vehicle\":\"" + objBooking.Fleet_VehicleType.VehicleType + "\",\"Account\":\"" + companyName + " " + "\"" +
                                 ",\"Lug\":\"" + objBooking.NoofLuggages.ToInt() + "\",\"Passengers\":\"" + objBooking.NoofPassengers.ToInt() + "\",\"Journey\":\"" + journey + "\",\"Payment\":\"" + paymentType + "\",\"Special\":\"" + specialRequirements + " " + "\",\"Extra\":\"" + IsExtra + "\",\"Via\":\"" + viaP + " " + "\"" +

                               ",\"CompanyId\":\"" + objBooking.CompanyId.ToInt() + "\",\"SubCompanyId\":\"" + objBooking.SubcompanyId.ToInt() + "\",\"QuotedPrice\":\"" + (objBooking.IsQuotedPrice.ToBool() ? "1" : "0") + "\"" +


                                 parkingandWaiting + ",\"DriverFares\":\"" + String.Format("{0:0.00}", objBooking.FareRate) + "\"" +
                                   agentDetails +


                                 ",\"Did\":\"" + driverId + "\",\"BabySeats\":\"" + objBooking.BabySeats.ToStr() + "\"" + showFares + showSummary + " }";





                            //








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


                            msg += ",";

                            s.Append(msg);








                        }

                    }



                    string resp = s.ToStr();

                    if (resp.ToStr().Trim().EndsWith(","))
                        resp = resp.Remove(resp.ToStr().LastIndexOf(","));

                    if (resp.ToStr().Trim().Length > 10)
                    {

                        resp = "[" + resp + "]";

                    }
                    //

                    if (resp.ToStr().Trim() == string.Empty)
                    {
                        if (values.Count() >= 3 && values[2].ToStr().IsNumeric() && values[23].ToDecimal() > 41.68m)
                            resp = "clearjob:1";
                        else
                            resp = "[]";

                    }


                    return resp;

                }
            }
            catch (Exception ex)
            {

                return "exceptionoccurred";
            }
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestdrivercclist")]
        public string requestDriverCCList(string mesg)
        {
            string result = "";
            try
            {


                string dataValue = mesg;
                dataValue = dataValue.Trim();

                ClsCCDetails obj = new JavaScriptSerializer().Deserialize<ClsCCDetails>(dataValue);



                using (TaxiDataContext db = new TaxiDataContext())
                {

                    db.CommandTimeout = 4;

                    int driverId = obj.RecordId;


                    //


                    var arr = db.ExecuteQuery<ClsCCDetails>("select Id,CCDetails,IsDefault from  fleet_driver_ccdetails where driverid=" + driverId).ToList();

                    List<ClsCCDetails> list = new List<ClsCCDetails>();

                    foreach (var item in arr)
                    {
                        string decrypt = Cryptography.Decrypt(item.CCDetails, driverId.ToStr(), true);

                        ClsCCDetails objCls = new JavaScriptSerializer().Deserialize<ClsCCDetails>(decrypt);
                        objCls.Id = item.Id;
                        objCls.IsDefault = item.IsDefault.ToBool();
                        list.Add(objCls);
                    }

                    result = new JavaScriptSerializer().Serialize(list);









                }

                //send message back to PDA


                return result;

            }
            catch (Exception ex)
            {
                return "exceptionoccurred";
            }
        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestadddrivercc")]
        public string requestAddDriverCC(string mesg)
        {
            string result = "";
            ClsCCDetails obj = null;
            try
            {


                string dataValue = mesg;
                dataValue = dataValue.Trim();

                obj = new JavaScriptSerializer().Deserialize<ClsCCDetails>(dataValue);



                using (TaxiDataContext db = new TaxiDataContext())
                {



                    int driverId = obj.RecordId;


                    //

                    int ii = obj.IsDefault.ToBool() ? 1 : 0;

                    string encrypt = Cryptography.Encrypt(dataValue, driverId.ToStr(), true);
                    string query = "insert into fleet_driver_ccdetails (driverId,CCDetails,AddOn,AddBy,IsDefault) values(" + driverId + ",'" + encrypt + "',getdate(),'Driver'," + ii + ")";

                    //
                    db.ExecuteQuery<int>(query);

                    decimal Id = db.ExecuteQuery<decimal>("select @@Identity").FirstOrDefault();
                    //   result = "success:" + Id.ToStr();

                    obj.Id = Id.ToLong();
                    obj.message = "Card Added Successfully";

                    if (obj.url.ToStr().Trim().Length > 0 && obj.IsDefault.ToBool())
                    {
                        try
                        {
                            string url = obj.url.ToStr().Trim();
                            string baseUrl = url.Remove(obj.url.IndexOf("?Data=") + 6);
                            url = url.Replace(baseUrl, "").Trim();
                            url = url.Replace(" ", "+");

                            //
                            url = Cryptography.Decrypt(url, "tcloudX@@!", true);
                            CardStreamSettings objCard = new JavaScriptSerializer().Deserialize<CardStreamSettings>(url);


                            objCard.cardNumber = obj.cardNumber;
                            objCard.cardExpiryMM = obj.cardExpiryMM;
                            objCard.cardExpiryYY = obj.cardExpiryYY;
                            objCard.customerName = obj.customerName;
                            objCard.customerPostcode = obj.customerPostcode;
                            objCard.customerAddress = obj.customerAddress;



                            /* else
                             {
                                 var item = db.ExecuteQuery<ClsCCDetails>("select Id,CCDetails,IsDefault from  fleet_driver_ccdetails where isdefault=1 and driverid=" + driverId).FirstOrDefault();

                                 List<ClsCCDetails> list = new List<ClsCCDetails>();


                                     string decrypt = Cryptography.Decrypt(item.CCDetails, driverId.ToStr(), true);



                                 CardStreamSettings objCard2 = new JavaScriptSerializer().Deserialize<CardStreamSettings>(decrypt);
                                 objCard.cardNumber = objCard2.cardNumber;
                                 objCard.cardExpiryMM = objCard2.cardExpiryMM;
                                 objCard.cardExpiryYY = objCard2.cardExpiryYY;
                                 objCard.customerName = objCard2.customerName;
                                 objCard.customerPostcode = objCard2.customerPostcode;
                                 objCard.customerAddress = objCard2.customerAddress;




                             }*/

                            url = new JavaScriptSerializer().Serialize(objCard);

                            url = Cryptography.Encrypt(url, "tcloudX@@!", true);
                            obj.url = baseUrl + url;
                        }
                        catch
                        {

                        }

                    }
                }

                result = new JavaScriptSerializer().Serialize(obj);
                //send message back to PDA


                return result;

            }
            catch (Exception ex)
            {
                obj.message = ex.Message;
                result = new JavaScriptSerializer().Serialize(obj);
                //send message back to PDA


                return result;
            }
        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestremovedrivercc")]

        public string requestRemoveDriverCC(string mesg)
        {
            string result = "";
            try
            {


                string dataValue = mesg;
                dataValue = dataValue.Trim();

                ClsCCDetails obj = new JavaScriptSerializer().Deserialize<ClsCCDetails>(dataValue);



                using (TaxiDataContext db = new TaxiDataContext())
                {



                    int driverId = obj.RecordId;


                    //

                    string query = "delete from fleet_driver_CCDetails where id=" + obj.Id;

                    db.ExecuteQuery<int>(query);


                    result = "success:Card removed successfully!";



                }

                //send message back to PDA


                return result;

            }
            catch (Exception ex)
            {
                result = "failed:" + ex.Message;
                return result;
            }
        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestgetlocation")]
        public string requestGetLocation(string mesg)
        {
            string rtn = "failed:";
            try
            {

                string[] values = mesg.Split('=');

                //

                int driverId = values[0].ToInt();

                double lat = Convert.ToDouble(values[1]);
                double lng = Convert.ToDouble(values[2]);

                //if(driverId==110)
                //{

                //    lat = 51.47797871009979;
                //    lng = -0.6124613061547279;
                //}

                string locationName = General.GetLocationName(lat, lng);

                //
                if (locationName.Length > 0)
                {

                    RemoveUK(ref locationName);
                    rtn = "success:" + locationName.ToStr().ToUpper();

                    try
                    {

                        File.AppendAllText(AppContext.BaseDirectory + "\\requestGetLocation.txt", DateTime.Now.ToStr() + " request" + mesg + Environment.NewLine);
                    }
                    catch
                    {

                    }

                }
            }
            catch (Exception ex)
            {
                rtn = "failed:" + ex.Message;

            }

            return rtn;
        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requeststc")]
        public JobActionEx requestSTC(string mesg)
        {

            JobActionEx obj = null;
            try
            {
                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\requestSTC.txt", DateTime.Now.ToStr() + " request" + mesg + Environment.NewLine);
                }
                catch
                {

                }


                string dataValue = mesg;
                dataValue = dataValue.Trim();
                obj = new JavaScriptSerializer().Deserialize<JobActionEx>(mesg);
                RemoveUK(ref obj.Dropoff);

                long jobId = obj.JobId.ToLong();
                using (TaxiDataContext db = new TaxiDataContext())
                {



                    try
                    {
                        //
                        //

                        if (HubProcessor.Instance.objPolicy.RestrictMilesOnSTC.ToDecimal() > 0
                            && obj.Dropoff.ToStr().Trim().Length > 0 && obj.DrvNo.ToStr().Trim().Length > 0
                            && obj.IsAuto.ToStr() != "1")
                        {
                            int RemoveRestriction = 0;

                            string dropOff = obj.Dropoff.ToStr();
                            if (dropOff.Contains("<<<"))
                            {
                                dropOff = dropOff.Remove(dropOff.IndexOf("<<<"));

                            }



                            db.CommandTimeout = 3;

                            string pickup = "";
                            string destination = GetPostCodeMatch(dropOff);

                            int journeyTypeId = 0;
                            int pickupZoneId = 0;
                            int dropOffZoneId = 0;
                            try
                            {



                                var objBooker = db.Bookings.Select(args => new {
                                    args.Id,
                                    args.JourneyTypeId,
                                    args.FromAddress,
                                    args.ZoneId,
                                    args.ToAddress,
                                    args.DropOffZoneId,
                                    args.OnHoldWaitingMins,
                                    args.CashFares,
                                    args.POBDateTime,
                                    args.PickupDateTime
                                })
                                                         .FirstOrDefault(c => c.Id == jobId);

                                if (objBooker != null)
                                {
                                    pickup = objBooker.FromAddress.ToStr().Trim();
                                    destination = objBooker.ToAddress.ToStr().Trim().ToUpper();
                                    journeyTypeId = objBooker.JourneyTypeId.ToInt();
                                    pickupZoneId = objBooker.ZoneId.ToInt();
                                    dropOffZoneId = objBooker.DropOffZoneId.ToInt();
                                    RemoveRestriction = objBooker.OnHoldWaitingMins.ToInt();
                                }

                            }
                            catch
                            {


                            }



                            stp_getCoordinatesByAddressResult coord = null;


                            if (journeyTypeId == Enums.JOURNEY_TYPES.WAITANDRETURN)
                            {

                                destination = GetPostCodeMatch(pickup);
                                coord = db.stp_getCoordinatesByAddress(destination, destination).FirstOrDefault();
                                dropOffZoneId = pickupZoneId;
                            }
                            else
                            {
                                destination = GetPostCodeMatch(destination);
                                coord = db.stp_getCoordinatesByAddress(destination, destination).FirstOrDefault();

                            }




                            decimal distance = 0.00m;

                            if (coord != null && coord.Latitude != null && coord.Latitude != 0)
                            {
                                distance = Math.Round(new DotNetCoords.LatLng(Convert.ToDouble(obj.Latitude), Convert.ToDouble(obj.Longitude)).DistanceMiles(new LatLng(Convert.ToDouble(coord.Latitude), Convert.ToDouble(coord.Longtiude))).ToDecimal(), 1);


                            }


                            if (coord == null)
                            {

                                //
                                bool isfound = true;


                                if (dropOffZoneId != 0)
                                {
                                    isfound = FindPoint(Convert.ToDouble(obj.Latitude), Convert.ToDouble(obj.Longitude), db.Gen_Zone_PolyVertices.Where(c => c.ZoneId == dropOffZoneId).ToList());

                                    if (isfound)
                                    {
                                        coord = null;
                                    }
                                    else
                                    {
                                        coord = new stp_getCoordinatesByAddressResult();
                                        distance = -1;

                                    }

                                }


                                //  }                                    


                            }




                            if (distance == -1 || (coord != null && distance > HubProcessor.Instance.objPolicy.RestrictMilesOnSTC.ToDecimal()))
                            {
                                if (RemoveRestriction == 1)
                                {

                                    //try
                                    //{

                                    //    File.AppendAllText(physicalPath + "//restrictionRemoved.txt", DateTime.Now + ": datavalue=" + dataValue + ",(stcjob)distance to : " + pickup + ",distance:" + distance + Environment.NewLine);
                                    //}
                                    //catch
                                    //{


                                    //}


                                }
                                else
                                {
                                    obj.Message = "You are far away from Destination";

                                    try
                                    {

                                        File.AppendAllText(AppContext.BaseDirectory + "//restrictionlog.txt", DateTime.Now + ": datavalue=" + dataValue + ",(stcjob)distance to : " + pickup + ",distance:" + distance + Environment.NewLine);
                                    }
                                    catch
                                    {


                                    }
                                }

                            }






                            //


                        }




                        if (obj.Message.ToStr().Trim().Length == 0)
                        {





                            var booking = db.Bookings.FirstOrDefault(aa => aa.Id == jobId);
                            var route = db.Booking_RoutePaths.Where(c => c.BookingId == jobId && c.UpdateDate >= booking.POBDateTime).ToList();




                            DateTime pickupDateTime = DateTime.Now;

                            //if (obj.PickupDateTime.ToStr().Trim().Length > 0)
                            //{
                            //    string pickupDateTimeStd = obj.PickupDateTime.ToStr().Trim().Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Trim();

                            //    DateTime.TryParseExact(pickupDateTimeStd, "dd/M/yyyy HH:mm",
                            //CultureInfo.InvariantCulture,
                            //DateTimeStyles.None,
                            //out pickupDateTime);
                            //}
                            //else
                            //{
                            //    ////
                            //    pickupDateTime = DateTime.Now;
                            //}

                          //  var zone = db.Gen_Zones.FirstOrDefault(c => c.ZoneName == "Congestion");


                            decimal congestion = 0.00m;
                            //foreach (var item in db.Gen_SysPolicy_CongestionCharges)
                            //{

                            //    if (item.ZoneId == zone.Id || item.ZoneId == zone.Id)
                            //    {

                            //        DateTime? fromtime = item.FromDateTime;

                            //        DateTime? tilltime = item.TillDateTime;
                            //        if (item.IsDayWise.ToBool())
                            //        {

                            //            string fromday = item.FromDay.ToStr();

                            //            string toDay = item.TillDay.ToStr();




                            //            int day = pickupDateTime.DayOfWeek.ToInt();

                            //            if (day == 0)
                            //                day = 7;

                            //            int fromDayId = GetDayId(fromday);

                            //            int toDayId = GetDayId(toDay);


                            //            if ((day >= fromDayId && day <= toDayId)
                            //                &&
                            //                   (pickupDateTime.TimeOfDay >= fromtime.ToDateTime().TimeOfDay && (pickupDateTime.TimeOfDay <= tilltime.ToDateTime().TimeOfDay))
                            //                )
                            //            {
                            //                congestion += item.Amount.ToDecimal();
                            //                break;

                            //            }

                            //        }
                            //        else
                            //        {
                            //            if (pickupDateTime >= fromtime && pickupDateTime <= tilltime)
                            //            {
                            //                congestion += item.Amount.ToDecimal();
                            //                break;
                            //            }
                            //        }
                            //    }
                            //}


                            bool found = false;

                            //var list = db.Gen_Zone_PolyVertices.Where(c => c.ZoneId == zone.Id).ToList();


                            //foreach (var item in route)
                            //{

                            //    //
                            //    double endLat = Convert.ToDouble(item.Latitude);
                            //    double endLong = Convert.ToDouble(item.Longitude);


                            //    if (FindPoint(endLat, endLong, list) == true)
                            //    {

                            //        found = true;

                            //        break;
                            //    }


                            //}



                            if (found)
                            {
                                if (booking.CashFares.ToDecimal() == 0)
                                    obj.Charges = congestion;

                            }
                            else
                            {
                                if (booking.CashFares.ToDecimal() > 0)
                                {

                                    obj.Charges = -booking.CashFares.ToDecimal();
                                    congestion = 0;
                                }
                                else
                                {
                                    congestion = 0;

                                }


                            }


                            obj.Message = "";


                            if (booking.CashFares.ToDecimal() != congestion)
                            {

                                db.ExecuteQuery<int>("update booking set cashfares=" + congestion + " where id=" + jobId);


                            }

                            try
                            {
                                (new TaxiDataContext()).stp_UpdateJob(jobId, obj.DrvId.ToInt(), Enums.BOOKINGSTATUS.STC, 5, 0);

                                General.BroadCastMessage("**action>>" + jobId.ToStr() + ">>" + obj.DrvId.ToStr() + ">>" + Enums.BOOKINGSTATUS.STC);



                                Global.RemoveSTCReminder(jobId, obj.DrvId.ToInt());


                                if (obj.IsAuto.ToStr() == "1")
                                {
                                    try
                                    {





                                        string query = "INSERT INTO BOOKING_LOG (bookingid,[user],beforeupdate,afterupdate,updatedate)values(" + jobId + ",'" + "Driver" + "','" + "" + "','" + "Driver " + obj.DrvNo.ToStr() + " Auto STC" + "',getdate());";
                                        db.ExecuteQuery<int>(query);

                                    }
                                    catch (Exception ex)
                                    {
                                        try
                                        {
                                            //
                                            //
                                            File.AppendAllText(AppContext.BaseDirectory + "\\PaymentFailed_exception.txt", DateTime.Now.ToStr() + ": request" + mesg + ",exception:" + ex.Message + Environment.NewLine);
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
                        }
                    }

                    catch (Exception ex)
                    {


                    }
                }

            }

            catch (Exception ex)
            {
                try
                {
                    obj.Message = ex.Message;
                    File.AppendAllText(AppContext.BaseDirectory + "\\requestSTC_exception.txt", DateTime.Now.ToStr() + " request" + mesg + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
            }

            return obj;
        }

        private int GetDayId(string dayName)
        {
            dayName = dayName.ToStr().Trim();

            if (dayName.ToLower() == "mon")
                return 1;
            else if (dayName.ToLower() == "tue")
                return 2;
            else if (dayName.ToLower() == "wed")
                return 3;
            else if (dayName.ToLower() == "thurs")
                return 4;
            else if (dayName.ToLower() == "fri")
                return 5;
            else if (dayName.ToLower() == "sat")
                return 6;
            else if (dayName.ToLower() == "sun")
                return 7;
            else return 0;


        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestupdatedestination")]
        public JobModel requestUpdateDestination(string mesg)
        {

            JobModel obj = null;
            try
            {

                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\requestUpdateDestination.txt", DateTime.Now.ToStr() + " request" + mesg + Environment.NewLine);
                }
                catch
                {

                }


                string dataValue = mesg;
                dataValue = dataValue.Trim();


                obj = new JavaScriptSerializer().Deserialize<JobModel>(mesg);

                RemoveUK(ref obj.ToAddress);

                //
                //   stri//ng[] values = dataValue.Split(new char[] { '=' });

                string plotName = "unknown";

                int? zoneId = General.GetZoneId(obj.ToAddress.ToStr().ToUpper());
                int? bookingStatusId = 0;
                //  GetZone(values[5].ToStr().ToUpper().Trim(), ref zoneId, ref plotName);

                if (zoneId == 0)
                    zoneId = null;

                using (TaxiDataContext db = new TaxiDataContext())
                {
                    db.stp_UpdateJobAddress(obj.JobId, obj.DriverId.ToInt(), obj.DriverNo, zoneId, obj.ToAddress, obj.OldToAddress.ToStr());



                    try
                    {

                        var booking = db.Bookings.FirstOrDefault(aa => aa.Id == obj.JobId);


                        //
                        BookingInformation info = new BookingInformation();
                        info.FromAddress = booking.FromAddress;

                        bookingStatusId = booking.BookingStatusId;

                        var objCoor = db.stp_getCoordinatesByAddress(booking.FromAddress.ToStr().ToUpper(), GetPostCodeMatch(booking.FromAddress.ToStr().ToUpper())).FirstOrDefault();

                        if (objCoor != null && objCoor.Latitude != null)
                            info.fromLatLng = objCoor.Latitude + "," + objCoor.Longtiude;

                        objCoor = db.stp_getCoordinatesByAddress(booking.ToAddress.ToStr().ToUpper(), GetPostCodeMatch(booking.ToAddress.ToStr().ToUpper())).FirstOrDefault();

                        if (objCoor != null && objCoor.Latitude != null)
                            info.toLatLng = objCoor.Latitude + "," + objCoor.Longtiude;



                        info.ToAddress = booking.ToAddress;
                        info.FromType = booking.FromLocTypeId == 1 ? "airport" : "address";
                        info.ToType = booking.ToLocTypeId == 1 ? "airport" : "address";
                        info.CompanyId = booking.CompanyId.ToInt();
                        info.PickupDateTime = string.Format("{0:dd/MM/yyyy HH:mm}", booking.PickupDateTime);
                        info.RouteCoordinates = "-2";
                        info.VehicleTypeId = booking.VehicleTypeId.ToInt();
                        info.SubCompanyId = 1;
                        try
                        {
                            if (booking.Booking_ViaLocations.Count > 0)
                            {
                                info.Via = new ViaAddresses[booking.Booking_ViaLocations.Count];
                                int cnt = 0;
                                foreach (var item in booking.Booking_ViaLocations)
                                {
                                    info.Via[cnt] = new ViaAddresses();
                                    info.Via[cnt].Viatype = "Address";
                                    info.Via[cnt].Viaaddress = item.ViaLocValue.ToStr();
                                    cnt++;

                                }

                            }

                        }
                        catch
                        {


                        }

                        info.MapKey = db.ExecuteQuery<string>("select APIKey from mapkeys where maptype='google'").FirstOrDefault().ToStr().Trim();





                        //CabTreasureAppAPI.ClsDispatchFares objFares = new JavaScriptSerializer().Deserialize<CabTreasureAppAPI.ClsDispatchFares>(data);
                        info.Vehicle = db.Fleet_VehicleTypes.Where(a => a.Id == info.VehicleTypeId).Select(b => b.VehicleType).FirstOrDefault();

                        //
                        //AppAPISer.AppAPISoapClient c = new AppAPISer.AppAPISoapClient();
                        //string data = c.GetAllFaresFromDispatch("5139", "LOCAL", new JavaScriptSerializer().Serialize(info), "51394321orue");
                        //data = data.Replace("\\", "");
                        //int startIndex = data.IndexOf("[{") + 1;

                        ////
                        //data = data.Substring(startIndex);
                        //int lastIndex = data.IndexOf("}]") + 1;
                        //data = data.Substring(0, lastIndex);


                        ////try
                        ////{
                        ////    //
                        ////    File.AppendAllText(AppContext.BaseDirectory + "\\requestAddStopStep4.txt", DateTime.Now.ToStr() + " request" + mesg + Environment.NewLine);
                        ////}
                        ////catch
                        ////{

                        ////}

                        //CabTreasureAppAPI.ClsDispatchFares objFares = new JavaScriptSerializer().Deserialize<CabTreasureAppAPI.ClsDispatchFares>(data);


                        //obj.Fares = objFares.Fare;
                        //obj.Extra = objFares.ExtraCharges;
                        //obj.Congestion = objFares.Congestion.ToDecimal();
                        //obj.AgentFee = objFares.AgentCharge.ToDecimal() + objFares.AgentFees.ToDecimal();
                        //obj.Parking = objFares.Parking.ToDecimal();
                       
                        //if (booking.PaymentTypeId.ToInt() == Enums.PAYMENT_TYPES.CREDIT_CARD && objFares.CompanyPrice.ToDecimal() > objFares.Fare.ToDecimal())
                        //{
                        //    obj.Fares = objFares.CompanyPrice.ToDecimal();
                        //    booking.CompanyPrice = objFares.CompanyPrice.ToDecimal();
                        //}

                        //obj.UpdateCharges = true;


                        //booking.IsQuotedPrice = objFares.IsQuoted.ToStr() == "1" ? true : false;
                        //booking.CashRate = objFares.AgentCharge.ToDecimal();
                        //booking.AgentCommission = objFares.AgentFees.ToDecimal();
                        //booking.FareRate = objFares.Fare.ToDecimal();
                        //booking.ExtraDropCharges = objFares.ExtraCharges.ToDecimal();
                        //booking.CashFares = objFares.Congestion.ToDecimal();



                        //db.SubmitChanges();


                        //try
                        //{

                        //    File.AppendAllText(AppContext.BaseDirectory + "\\requestupdatedestinationrespo.txt", DateTime.Now.ToStr() + " request" + mesg + ", response:" + data + Environment.NewLine);
                        //}
                        //catch
                        //{

                        //}

                    }

                    catch (Exception ex)
                    {


                    }
                }


                if (obj.BookingStatusId.ToInt() == 8)
                {
                    try
                    {

                        string oldToAddress = obj.OldToAddress.ToStr().ToUpper();
                        if (oldToAddress.ToStr().Contains("\u003c\u003c\u003c"))
                            oldToAddress = oldToAddress.Remove(oldToAddress.IndexOf("\u003c\u003c\u003c")).ToStr().Trim().ToUpper();

                        int? oldZoneId = General.GetZoneId(oldToAddress);
                        if (zoneId.ToInt() != oldZoneId)
                        {

                            using (TaxiDataContext db = new TaxiDataContext())
                            {
                                db.ExecuteQuery<int>("exec stp_revertjobstatus {0},{1},{2},{3},{4},{5}", obj.JobId, obj.DriverId.ToInt(), obj.DriverNo.ToStr(), zoneId.ToInt(), oldZoneId.ToInt(), Enums.Driver_WORKINGSTATUS.NOTAVAILABLE);

                            }

                            obj.revertstatus = "pob";

                            Global.AddSTCReminder(obj.JobId, obj.DriverId.ToInt());

                            General.BroadCastMessage("**action>>" + obj.JobId.ToStr() + ">>" + obj.DriverId.ToStr() + ">>" + Enums.BOOKINGSTATUS.POB);


                        }
                    }
                    catch (Exception ex)
                    {

                        try
                        {
                            //
                            File.AppendAllText(AppContext.BaseDirectory + "\\requestupdatedestination_exception.txt", DateTime.Now.ToStr() + " request" + mesg + ",exception:" + ex.Message + Environment.NewLine);
                        }
                        catch
                        {

                        }

                    }
                }
                else
                {
                    General.BroadCastMessage("**changeaddress>>" + obj.DriverId.ToStr() + ">>" + obj.JobId.ToStr() + ">>" + obj.DriverNo.ToStr() + ">>" + obj.ToAddress.ToStr().ToUpper());



                }
                // obj.Fares = 3;
                //  obj.Extra = 2;
                // obj.Parking = 6;
                // obj.Waiting = 8;
                //  obj.AgentFee = 22;

                //  obj.UpdateCharges = false;
                //  General.BroadCastMessage("**changeaddress>>" + values[2].ToInt() + ">>" + values[1].ToLong() + ">>" + values[3].ToStr() + ">>" + values[5].ToStr());
            }
            catch (Exception ex)
            {
                // Clients.Caller.changeDestination(ex.Message);
            }

            return obj;
        }




        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestaddstop")]
        public ClsAddStop requestAddStop(string mesg)
        {

            ClsAddStop obj = null;
            try
            {



                try
                {
                    //
                    File.AppendAllText(AppContext.BaseDirectory + "\\requestAddStop.txt", DateTime.Now.ToStr() + " request" + mesg + Environment.NewLine);
                }
                catch
                {

                }

                string dataValue = mesg;
                dataValue = dataValue.Trim();

                obj = new JavaScriptSerializer().Deserialize<ClsAddStop>(mesg);

                var stop = requestGetLocation(obj.DrvId + "=" + obj.Latitude + "=" + obj.Longitude);

                if (stop.StartsWith("success"))
                    stop = stop.Replace("success:", "").Trim().ToStr().ToUpper().Trim();
                else if (stop.StartsWith("failed:"))
                {
                    obj.Message = "Cannot add stop";

                    try
                    {
                        //
                        File.AppendAllText(AppContext.BaseDirectory + "\\requestAddStop_denied.txt", DateTime.Now.ToStr() + " request : " + mesg + ",stop:" + stop.ToStr() + Environment.NewLine);

                    }
                    catch
                    {

                    }
                    return obj;

                }

                //try
                //{
                //    //
                //    File.AppendAllText(AppContext.BaseDirectory + "\\requestAddStopStep1.txt", DateTime.Now.ToStr() + " request" + mesg + Environment.NewLine);
                //}
                //catch
                //{

                //}

                using (TaxiDataContext db = new TaxiDataContext())
                {

                    try
                    {
                        long jobId = obj.JobId.ToLong();

                        var booking = db.Bookings.FirstOrDefault(aa => aa.Id == jobId);


                        //
                        BookingInformation info = new BookingInformation();
                        info.FromAddress = booking.FromAddress;

                        var objCoor = db.stp_getCoordinatesByAddress(booking.FromAddress.ToStr().ToUpper(), GetPostCodeMatch(booking.FromAddress.ToStr().ToUpper())).FirstOrDefault();

                        if (objCoor != null && objCoor.Latitude != null)
                            info.fromLatLng = objCoor.Latitude + "," + objCoor.Longtiude;

                        objCoor = db.stp_getCoordinatesByAddress(booking.ToAddress.ToStr().ToUpper(), GetPostCodeMatch(booking.ToAddress.ToStr().ToUpper())).FirstOrDefault();

                        if (objCoor != null && objCoor.Latitude != null)
                            info.toLatLng = objCoor.Latitude + "," + objCoor.Longtiude;



                        info.ToAddress = booking.ToAddress;
                        info.FromType = booking.FromLocTypeId == 1 ? "airport" : "address";
                        info.ToType = booking.ToLocTypeId == 1 ? "airport" : "address";
                        info.CompanyId = booking.CompanyId.ToInt();
                        
                        try
                        {
                            info.PickupDateTime = string.Format("{0:dd/MM/yyyy HH:mm}", booking.PickupDateTime);
                        }
                        catch
                        {

                        }
                        info.RouteCoordinates = "-2";
                        info.VehicleTypeId = booking.VehicleTypeId.ToInt();
                        info.SubCompanyId = 1;
                     
                        bool stopAlreadyExist = false;

                        try
                        {

                            if (booking.Booking_ViaLocations.Count > 0)
                            {
                                info.Via = new ViaAddresses[booking.Booking_ViaLocations.Count + 1];
                                int cnt = 0;
                                foreach (var item in booking.Booking_ViaLocations)
                                {
                                    if (item.ViaLocValue.ToStr().ToUpper() == stop.ToStr().ToUpper())
                                    {
                                        stopAlreadyExist = true;
                                        break;
                                    }
                                    info.Via[cnt] = new ViaAddresses();
                                    info.Via[cnt].Viatype = "Address";
                                    info.Via[cnt].Viaaddress = item.ViaLocValue.ToStr();
                                    cnt++;

                                }

                                info.Via[cnt] = new ViaAddresses();
                                info.Via[cnt].Viatype = "Address";
                                info.Via[cnt].Viaaddress = stop.ToStr();
                             

                                try
                                {
                                    string postcodeStop = GetPostCodeMatch(stop.ToStr().ToUpper());

                                    if (postcodeStop.Length == 0 || postcodeStop.Contains(" ") == false)
                                        info.Via[cnt].ViaCoordinates = obj.Latitude + "," + obj.Longitude;



                                }
                                catch
                                {

                                }
                            }
                            else
                            {

                                info.Via = new ViaAddresses[1];
                                info.Via[0] = new ViaAddresses();
                                info.Via[0].Viatype = "Address";
                                info.Via[0].Viaaddress = stop.ToStr().ToUpper();

                                try
                                {
                                    string postcodeStop = GetPostCodeMatch(stop.ToStr().ToUpper());

                                    if(postcodeStop.Length==0 || postcodeStop.Contains(" ")==false)
                                    info.Via[0].ViaCoordinates = obj.Latitude + "," + obj.Longitude;



                                }
                                catch
                                {

                                }
                            }

                        }
                        catch
                        {


                        }

                        if (stopAlreadyExist)
                        {
                            throw new Exception("Stop already added on this location");

                        }



                        info.MapKey = db.ExecuteQuery<string>("select APIKey from mapkeys where maptype='google'").FirstOrDefault().ToStr().Trim();





                        info.Vehicle = db.Fleet_VehicleTypes.Where(a => a.Id == info.VehicleTypeId).Select(b => b.VehicleType).FirstOrDefault();


                        //AppAPISer.AppAPISoapClient c = new AppAPISer.AppAPISoapClient();
                        //string data = c.GetAllFaresFromDispatch("5139", "LOCAL", new JavaScriptSerializer().Serialize(info), "51394321orue");
                        //data = data.Replace("\\", "");
                        //int startIndex = data.IndexOf("[{") + 1;

                        ////
                        //data = data.Substring(startIndex);
                        //int lastIndex = data.IndexOf("}]") + 1;
                        //data = data.Substring(0, lastIndex);



                        //CabTreasureAppAPI.ClsDispatchFares objFares = new JavaScriptSerializer().Deserialize<CabTreasureAppAPI.ClsDispatchFares>(data);


                        ////try
                        ////{
                        ////    //
                        ////    File.AppendAllText(AppContext.BaseDirectory + "\\requestAddStopStep5.txt", DateTime.Now.ToStr() + " request" + mesg + Environment.NewLine);
                        ////}
                        ////catch
                        ////{

                        ////}

                        //obj.Fares = objFares.Fare;
                        //obj.Extra = booking.ExtraDropCharges.ToDecimal();
                        //obj.Waiting = booking.MeetAndGreetCharges.ToDecimal();

                        ////   obj.Extra = objFares.ExtraCharges;
                        //obj.Congestion = objFares.Congestion.ToDecimal();
                        //obj.AgentFee = objFares.AgentCharge.ToDecimal() + objFares.AgentFees.ToDecimal();
                        //obj.Parking = 0.00m;
                        //obj.StopName = stop;
                        //obj.BookingFee = 0.00m;


                        //if (booking.PaymentTypeId.ToInt() == Enums.PAYMENT_TYPES.CREDIT_CARD && objFares.CompanyPrice.ToDecimal() > objFares.Fare.ToDecimal())
                        //{
                        //    obj.Fares = objFares.CompanyPrice.ToDecimal();
                        //    booking.CompanyPrice = objFares.CompanyPrice.ToDecimal();
                        //}

                        //obj.UpdateCharges = true;


                        //booking.IsQuotedPrice = false;
                        //booking.CashRate = objFares.AgentCharge.ToDecimal();
                        //booking.AgentCommission = objFares.AgentFees.ToDecimal();
                        //booking.FareRate = objFares.Fare.ToDecimal();
                        ////   booking.ExtraDropCharges = objFares.ExtraCharges.ToDecimal();
                        //booking.CashFares = objFares.Congestion.ToDecimal();


                        //booking.Booking_ViaLocations.Add(new Booking_ViaLocation { BookingId = booking.Id, ViaLocValue = stop, ViaLocTypeLabel = "Via Address", ViaLocTypeId = 7 });

                        //booking.Booking_Logs.Add(new Booking_Log { BookingId = booking.Id, UpdateDate = DateTime.Now, User = "Driver", AfterUpdate = "ADD STOP : " + stop });

                        //db.SubmitChanges();


                        try
                        {

                            File.AppendAllText(AppContext.BaseDirectory + "\\requestAddStopResp.txt", DateTime.Now.ToStr() + " request" + mesg + ", response:" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
                        }
                        catch
                        {

                        }

                    }

                    catch (Exception ex)
                    {

                        obj.Message = ex.Message;
                    }
                }


            }
            catch (Exception ex)
            {

                try
                {
                    //
                    File.AppendAllText(AppContext.BaseDirectory + "\\requestAddStop_exception.txt", DateTime.Now.ToStr() + " request : " + mesg + ",exception:" + ex.Message + Environment.NewLine);
                    obj.Message = ex.Message;
                }
                catch
                {

                }
                // Clients.Caller.changeDestination(ex.Message);
            }

            return obj;
        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("requestupdatedrivercc")]
        public string requestUpdateDriverCC(string mesg)
        {
            string result = "";
            ClsCCDetails obj = null;
            try
            {


                string dataValue = mesg;
                dataValue = dataValue.Trim();

                obj = new JavaScriptSerializer().Deserialize<ClsCCDetails>(dataValue);



                using (TaxiDataContext db = new TaxiDataContext())
                {



                    int driverId = obj.RecordId;


                    //
                    string encrypt = Cryptography.Encrypt(dataValue, driverId.ToStr(), true);



                    int ii = obj.IsDefault.ToBool() == true ? 1 : 0;

                    string query = "update fleet_driver_CCDetails set CCDetails='" + encrypt + "',IsDefault=" + ii + " where driverid=" + obj.RecordId + " and id=" + obj.Id;

                    db.ExecuteQuery<int>(query);

                    if (obj.IsDefault.ToBool())
                    {
                        db.ExecuteQuery<int>("update fleet_driver_CCDetails set isdefault=0 where id!=" + obj.Id);
                        ///

                    }
                    obj.message = "Card updated successfully!";


                    if (obj.url.ToStr().Trim().Length > 0 && obj.IsDefault.ToBool())
                    {
                        try
                        {
                            //
                            string url = obj.url.ToStr().Trim();
                            string baseUrl = url.Remove(obj.url.IndexOf("?Data=") + 6);
                            url = url.Replace(baseUrl, "").Trim();
                            url = url.Replace(" ", "+");


                            //   url = url.Remove(url.LastIndexOf("u0026"));
                            //   url = url.Remove(url.LastIndexOf("u0026"));

                            url = Cryptography.Decrypt(url, "tcloudX@@!", true);
                            CardStreamSettings objCard = new JavaScriptSerializer().Deserialize<CardStreamSettings>(url);


                            objCard.cardNumber = obj.cardNumber;
                            objCard.cardExpiryMM = obj.cardExpiryMM;
                            objCard.cardExpiryYY = obj.cardExpiryYY;
                            objCard.customerName = obj.customerName;
                            objCard.customerPostcode = obj.customerPostcode;
                            objCard.customerAddress = obj.customerAddress;





                            url = new JavaScriptSerializer().Serialize(objCard);

                            url = Cryptography.Encrypt(url, "tcloudX@@!", true);
                            obj.url = baseUrl + url;

                        }
                        catch
                        {

                        }
                    }

                    result = new JavaScriptSerializer().Serialize(obj);

                }

                //send message back to PDA


                return result;

            }
            catch (Exception ex)
            {
                obj.Id = 0;
                obj.message = ex.Message;
                //  result = "failed:" + ex.Message;
                result = new JavaScriptSerializer().Serialize(obj);
                return result;
            }
        }

        #region KonnectPay

        /// <summary>
        /// This method send pre authorize payment link to customer
        /// </summary>
        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("SendPreAuthLinkKP")]
        public string SendPreAuthLinkKP(string value)
        {
            StripePaymentRequestDto obj = null;
            string rtn = "true";
            string json = value;
            bool IsSent = false;
            string DefaultCurrencySign = Global.DefaultCurrencySign;//System.Configuration.ConfigurationManager.AppSettings["DefaultCurrencySign"];
            try
            {
                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\SendPreAuthLinkKP.txt", DateTime.Now.ToStr() + " json" + json + Environment.NewLine);
                }
                catch
                {

                }
                obj = new JavaScriptSerializer().Deserialize<StripePaymentRequestDto>(json);
                string url = obj.UpdatePaymentURL.ToStr();
                obj.verificationWebhook = url + "/api/Supplier/VerifyPaymentKOnnectPay";
                obj.paymentUpdateWebhook = url + "/api/Supplier/UpdateDataFromPayByLinkKOnnectPay";

                string data = new JavaScriptSerializer().Serialize(obj);

                string StripeAPIBaseURL = System.Configuration.ConfigurationManager.AppSettings["StripeAPIBaseURL"];
                obj.PreAuthUrl = StripeAPIBaseURL + "/checkout?data=" + EncodeBASE64(data);

                if (obj.PreAuthUrl.ToStr().Contains("tinyurl") == false)
                {
                    obj.PreAuthUrl = General.ToTinyURLS(obj.PreAuthUrl.ToStr());
                    try
                    {
                        File.AppendAllText(AppContext.BaseDirectory + "\\SendPreAuthLinkKP.txt", DateTime.Now.ToStr() + " url=" + obj.PreAuthUrl + Environment.NewLine);
                    }
                    catch
                    {
                    }
                }
                if (!string.IsNullOrEmpty(obj?.phoneNumber))
                {
                    string returnPayment = string.Empty;
                    if (obj.ReturnAmount > 0)
                    {
                        returnPayment = " (Inc. Return)";
                    }
                    HubProcessor.Instance.listofSMS.Add("request dispatchsms = " + obj?.phoneNumber.Trim() + " =" + "Please click on the link to authorize your payment for your journey with " + obj?.companyName + Environment.NewLine + "Ref No - " + obj?.bookingRef.ToStr().Trim() + Environment.NewLine + "Cars will only be dispatched when payment authorised." + Environment.NewLine + obj.PreAuthUrl.ToStr());
                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        db.stp_BookingLog(obj.bookingId.ToLong(), obj.OperatorName.ToStr(), "Authorization LINK for " + DefaultCurrencySign + string.Format("{0:f2}", Convert.ToDouble((obj.amount / 100))) + returnPayment + " SENT TO : " + obj.phoneNumber.ToStr());

                    }
                }
                if (!string.IsNullOrEmpty(obj?.email))
                {
                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        var genSubCompany = db.Gen_SubCompanies

                                .Select(args => new
                                {
                                    args.CompanyName,
                                    args.EmailAddress,
                                    args.SmtpHost,
                                    args.SmtpPassword,
                                    args.SmtpUserName,
                                    args.SmtpPort,
                                    args.SmtpHasSSL,
                                    args.EmailCC
                                }
                                ).FirstOrDefault();
                        if (genSubCompany == null)
                        {
                            genSubCompany = db.Gen_SubCompanies.Where(c => c.Id == 1)
                               .Select(args => new
                               {
                                   args.CompanyName,
                                   args.EmailAddress,
                                   args.SmtpHost,
                                   args.SmtpPassword,
                                   args.SmtpUserName,
                                   args.SmtpPort,
                                   args.SmtpHasSSL,
                                   args.EmailCC
                               }
                               ).FirstOrDefault();
                        }
                        string Note = "Please note that if the captured amount is less than the hold amount, the remaining balance will be refunded within five to ten days!";
                        ClsEASendEmail cls = new ClsEASendEmail(genSubCompany.EmailAddress, obj.email.ToStr().Trim(), "Link for Card Authorization " + obj?.bookingRef.ToStr(), "Please click on the link to authorize your payment for your journey with " + obj?.companyName + Environment.NewLine + "Ref No - " + obj?.bookingRef.ToStr().Trim() + ".<br/>" + "<a href='" + obj.PreAuthUrl.ToStr() + "'>" + obj.PreAuthUrl.ToStr() + "</a>", genSubCompany.SmtpHost, genSubCompany.EmailCC);
                        IsSent = cls.Send(obj.email.ToStr(), genSubCompany.SmtpUserName, genSubCompany.SmtpPassword);

                        if (IsSent)
                        {
                            try
                            {
                                File.AppendAllText(AppContext.BaseDirectory + "\\SendPayByLinkKP_email.txt", DateTime.Now.ToStr() + " json" + obj.email.ToStr().Trim() + "," + obj.PreAuthUrl.ToStr() + " ,from:" + genSubCompany.SmtpUserName + ", Password: " + genSubCompany.SmtpPassword + Environment.NewLine);
                            }
                            catch
                            {
                            }
                            string returnPayment = string.Empty;
                            if (obj.ReturnAmount > 0)
                            {
                                returnPayment = " (Inc. Return)";
                            }
                            db.stp_BookingLog(obj.bookingId.ToLong(), obj.OperatorName.ToStr(), "Authorization LINK for " + DefaultCurrencySign + string.Format("{0:f2}", Convert.ToDouble((obj.amount / 100))) + returnPayment + " SENT TO : " + obj.email.ToStr());
                            rtn = "true";
                        }
                        else
                            rtn = "false";

                    }
                }


            }
            catch (Exception ex)
            {
                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\SendPreAuthLinkKP_exception.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
                rtn = "false";

            }

            return rtn;
        }

        /// <summary>
        /// This method send payment link to customer
        /// </summary>
        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("SendPayByLinkKP")]
        public string SendPayByLinkKP(string value)
        {
            StripePaymentRequestDto obj = null;
            string rtn = "true";
            string json = value;
            bool IsSent = false;

            string DefaultCurrencySign = Global.DefaultCurrencySign; // System.Configuration.ConfigurationManager.AppSettings["DefaultCurrencySign"];
            try
            {
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\SendPayByLinkKP.txt", DateTime.Now.ToStr() + " json" + json + Environment.NewLine);
                }
                catch
                {
                }
                //
                obj = new JavaScriptSerializer().Deserialize<StripePaymentRequestDto>(json);
                string url = obj.UpdatePaymentURL.ToStr();
                obj.verificationWebhook = url + "/api/Supplier/VerifyPaymentKOnnectPay";
                obj.paymentUpdateWebhook = url + "/api/Supplier/UpdateDataFromPayByLinkKOnnectPay";

                string data = new JavaScriptSerializer().Serialize(obj);
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\SendPayByLinkKP.txt", DateTime.Now.ToStr() + " json" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
                }
                catch
                {
                }
                string StripeAPIBaseURL = System.Configuration.ConfigurationManager.AppSettings["StripeAPIBaseURL"];
                obj.PreAuthUrl = StripeAPIBaseURL + "/checkout?data=" + EncodeBASE64(data);

                if (obj.PreAuthUrl.ToStr().Contains("tinyurl") == false)
                {
                    obj.PreAuthUrl = General.ToTinyURLS(obj.PreAuthUrl.ToStr());
                    try
                    {
                        File.AppendAllText(AppContext.BaseDirectory + "\\SendPayByLinkCustomLink.txt", DateTime.Now.ToStr() + " url=" + obj.PreAuthUrl + Environment.NewLine);
                    }
                    catch
                    {
                    }
                }
                if (obj.PayByDispatch == "1")
                {
                    rtn = obj.PreAuthUrl;
                }
                else
                {
                    if (!string.IsNullOrEmpty(obj?.phoneNumber))
                    {
                        string returnPayment = string.Empty;
                        if (obj.ReturnAmount > 0)
                        {
                            returnPayment = " (Inc. Return)";
                        }
                        HubProcessor.Instance.listofSMS.Add("request dispatchsms = " + obj.phoneNumber.Trim() + " =" + "Please click on the link to process your payment for your journey with " + obj?.companyName + Environment.NewLine + "Ref No - " + obj?.bookingRef.ToStr().Trim() + Environment.NewLine + obj.PreAuthUrl.ToStr());
                        using (TaxiDataContext db = new TaxiDataContext())
                        {
                            db.stp_BookingLog(obj.bookingId.ToLong(), obj.OperatorName.ToStr(), "PAYMENT LINK for " + DefaultCurrencySign + string.Format("{0:f2}", Convert.ToDouble((obj.amount / 100))) + returnPayment + " SENT TO : " + obj.phoneNumber.ToStr());

                        }
                        rtn = "true";
                    }
                    if (!string.IsNullOrEmpty(obj?.email))
                    {
                        using (TaxiDataContext db = new TaxiDataContext())
                        {
                            var genSubCompany = db.Gen_SubCompanies

                                    .Select(args => new
                                    {
                                        args.CompanyName,
                                        args.EmailAddress,
                                        args.SmtpHost,
                                        args.SmtpPassword,
                                        args.SmtpUserName,
                                        args.SmtpPort,
                                        args.SmtpHasSSL,
                                        args.EmailCC
                                    }
                                    ).FirstOrDefault();
                            if (genSubCompany == null)
                            {
                                genSubCompany = db.Gen_SubCompanies.Where(c => c.Id == 1)
                                   .Select(args => new
                                   {
                                       args.CompanyName,
                                       args.EmailAddress,
                                       args.SmtpHost,
                                       args.SmtpPassword,
                                       args.SmtpUserName,
                                       args.SmtpPort,
                                       args.SmtpHasSSL,
                                       args.EmailCC
                                   }
                                   ).FirstOrDefault();
                            }
                            ClsEASendEmail cls = new ClsEASendEmail(genSubCompany.EmailAddress, obj.email.ToStr().Trim(), "Link for Card Payment " + obj?.bookingRef.ToStr(), "Please click on the link to process the payment for your journey with " + obj?.companyName.ToStr() + Environment.NewLine + "Ref No - " + obj?.bookingRef.ToStr().Trim() + ".<br/>" + "<a href='" + obj.PreAuthUrl.ToStr() + "'>" + obj.PreAuthUrl.ToStr() + "</a>", genSubCompany.SmtpHost, genSubCompany.EmailCC);
                            IsSent = cls.Send(obj.email.ToStr(), genSubCompany.SmtpUserName, genSubCompany.SmtpPassword);

                            if (IsSent)
                            {
                                try
                                {
                                    File.AppendAllText(AppContext.BaseDirectory + "\\SendPayByLinkKP_email.txt", DateTime.Now.ToStr() + " json" + obj.email.ToStr().Trim() + "," + obj.PreAuthUrl.ToStr() + " ,from:" + genSubCompany.SmtpUserName + ", Password: " + genSubCompany.SmtpPassword + Environment.NewLine);
                                }
                                catch
                                {
                                }
                                string returnPayment = string.Empty;
                                if (obj.ReturnAmount > 0)
                                {
                                    returnPayment = " (Inc. Return)";
                                }
                                db.stp_BookingLog(obj.bookingId.ToLong(), obj.OperatorName.ToStr(), "PAYMENT LINK for " + DefaultCurrencySign + string.Format("{0:f2}", Convert.ToDouble((obj.amount / 100))) + returnPayment + " SENT TO : " + obj.email.ToStr());
                                rtn = "true";
                            }
                            else
                                rtn = "false";

                        }
                    }

                    try
                    {
                        File.AppendAllText(AppContext.BaseDirectory + "\\SendPayByLinkKPsms.txt", DateTime.Now.ToStr() + obj.phoneNumber.ToStr() + " Please click on the link to process your payment for your journey with " + obj?.companyName.ToStr() + Environment.NewLine + "Ref No - " + obj?.bookingRef.ToStr().Trim() + Environment.NewLine + obj.PreAuthUrl.ToStr() + Environment.NewLine);
                        if (obj.ReturnAmount > 0)
                        {
                            File.WriteAllText(AppContext.BaseDirectory + "\\paybylink\\" + obj.bookingId.ToStr() + ".txt", obj.ReturnAmount.ToStr());
                        }
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
                    File.AppendAllText(AppContext.BaseDirectory + "\\SendPayByLinkKP_exception.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {
                }
                rtn = "false";
            }
            return rtn;
        }

        /// <summary>
        /// This is webhook method handle pre auth and pay by link call back 
        /// </summary>
        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("UpdateDataFromPayByLinkKOnnectPay")]
        public string UpdateDataFromPayByLinkKOnnectPay(PaymentUpdateDto obj)
        {

            string DefaultCurrencySign = Global.DefaultCurrencySign;// System.Configuration.ConfigurationManager.AppSettings["DefaultCurrencySign"];
            string rtn = "true";
            string json = string.Empty;
            try
            {
                json = new JavaScriptSerializer().Serialize(obj);

                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\UpdateDataFromPayByLinkKOnnectPay.txt", DateTime.Now.ToStr() + " json" + json + Environment.NewLine);
                }
                catch
                {
                }
                if (obj == null) { return rtn = "false"; }

                obj.message = obj.message.ToStr();
                if (obj.message.ToStr().Trim().Contains("'"))
                    obj.message = obj.message.Replace("'", "").Trim();
                if (obj.message.ToStr().Trim().Contains("\u0027"))
                    obj.message = obj.message.Replace("\u0027", "").Trim();
                if (obj.isAuthorized)
                {
                    //this method is for handle Pre Auth Call back konnect pay
                    rtn = UpdateAuthStripePaymentKP(obj);
                }
                else
                {
                    bool IsPayByDriverApp = false;
                    int DriverId = 0;
                    ResponseData DriverMsgObj = new ResponseData();
                    //this is for pay by link Call back konnect pay
                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        var objBooking = db.Bookings.Where(c => c.Id == obj.bookingId).FirstOrDefault();
                        if (objBooking != null && objBooking.DriverId != null && objBooking.DriverId > 0)
                        {
                            IsPayByDriverApp = true;
                            DriverId = objBooking.DriverId.ToInt();
                        }
                        if (obj.status.ToStr().ToLower() == "succeeded" || obj.isSuccess)
                        {
                            string msg = "PAID " + DefaultCurrencySign + string.Format("{0:f2}", obj.amount) + " | TRANSACTION - " + obj.paymentIntentId.ToStr() + " | " + string.Format("{0:dd/MM/yyyy HH:mm}", DateTime.Now);

                            db.ExecuteQuery<int>("update booking set Paymenttypeid=6,IsQuotedPrice=1,PaymentComments='" + msg + "' where id=" + obj.bookingId.ToLong());

                            db.stp_BookingLog(obj.bookingId.ToLong(), "Customer", msg);

                            if ((db.ExecuteQuery<int>("select count(*) from booking_payment where bookingId=" + obj.bookingId.ToLong()).FirstOrDefault()) == 0)
                            {
                                // db.ExecuteQuery<int>("insert into Booking_Payment(bookingid,PaymentGatewayId,AuthCode)VALUES(" + obj.bookingId.ToLong() + ",15,'" + obj.paymentIntentId.ToStr() + "')");
                                db.ExecuteQuery<int>("insert into Booking_Payment(bookingid,PaymentGatewayId,AuthCode,Status,TotalAmount)VALUES(" + obj.bookingId.ToLong() + ",15,'" + obj.paymentIntentId.ToStr() + "','Paid'," + obj.amount + ")");
                            }
                            try
                            {    // set Obj for driver app
                                DriverMsgObj.IsSuccess = true;
                                DriverMsgObj.Message = "Transaction successful";
                                DriverMsgObj.Data = obj.paymentIntentId.ToStr();

                                long jobId = obj.bookingId.ToLong();
                                using (TaxiDataContext db2 = new TaxiDataContext())
                                {
                                    General.UpdateJobToDriverPDA(db2.Bookings.FirstOrDefault(c => c.Id == jobId), "Credit Card(PAID)");
                                }

                                //var objBooking = db.Bookings.Where(c => c.Id == jobId).FirstOrDefault();
                                //if (objBooking != null && objBooking.CustomerId > 0 && !string.IsNullOrEmpty(obj.customerid))
                                //{
                                if (objBooking != null && !string.IsNullOrEmpty(obj.customerid))
                                {
                                    // if (objBooking.CustomerId == null || objBooking.CustomerId == 0)
                                    //{
                                    Customer objcustomer = db.Customers.Where(c => c.MobileNo == objBooking.CustomerMobileNo).FirstOrDefault();
                                    objBooking.CustomerId = objcustomer.Id;
                                    // }
                                    db.ExecuteQuery<int>("update Customer set CreditCardDetails='" + obj.customerid.ToStr() + "' where Id=" + objBooking.CustomerId);
                                    if ((db.ExecuteQuery<int>("select count(*) from Customer_CCDetails where CustomerId=" + objBooking.CustomerId.ToLong()).FirstOrDefault()) > 0)
                                    {
                                        db.ExecuteQuery<int>("update Customer_CCDetails set IsDefault=0 where CustomerId=" + objBooking.CustomerId);
                                    }

                                    // string CardDetails = $"Token: {obj.paymentMethodId} | {obj.message}";
                                    string CardDetails = $"KonnectPayToken: {obj.paymentMethodId} | {obj.message}";
                                    db.ExecuteQuery<int>("insert into Customer_CCDetails(CustomerId,CCDetails,AddOn,AddBy,IsDefault)VALUES(" + objBooking.CustomerId + ",'" + CardDetails + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','Eurosoft',1)");

                                }
                            }
                            catch
                            {
                            }
                            try
                            {
                                //if (File.Exists(AppContext.BaseDirectory + "\\paybylink\\" + obj.bookingId.ToStr() + ".txt"))

                                long returnBookingId = db.ExecuteQuery<long>("select Id from booking where masterjobid=" + obj.bookingId).FirstOrDefault();
                                if (returnBookingId > 0)
                                {
                                    string returnmsg = "PAID " + DefaultCurrencySign + string.Format("{0:f2}", obj.amount) + " | TRANSACTION - " + obj.paymentIntentId.ToStr() + " | " + string.Format("{0:dd/MM/yyyy HH:mm}", DateTime.Now);

                                    db.ExecuteQuery<int>("update booking set Paymenttypeid=6,IsQuotedPrice=1,PaymentComments='" + returnmsg + "' where id=" + returnBookingId);
                                    db.ExecuteQuery<int>("insert into Booking_Payment(bookingid,PaymentGatewayId,AuthCode,Status)VALUES(" + returnBookingId + ",15,'" + obj.paymentIntentId.ToStr() + "','Paid')");
                                    using (TaxiDataContext db2 = new TaxiDataContext())
                                    {
                                        General.UpdateJobToDriverPDA(db2.Bookings.FirstOrDefault(c => c.Id == returnBookingId), "Credit Card(PAID)");
                                    }

                                    try
                                    {
                                        File.AppendAllText(AppContext.BaseDirectory + "\\UpdateDataFromPayByLinkKOnnectPay.txt", DateTime.Now.ToStr() + " updating ReturnBooking ID : " + returnBookingId + Environment.NewLine);
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
                                    File.AppendAllText(AppContext.BaseDirectory + "\\UpdateDataFromPayByLinkKOnnectPay_exception1.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                                }
                                catch
                                {
                                }
                            }
                        }
                        else
                        {
                            db.ExecuteQuery<int>("update booking set PaymentComments='PAYMENT FAILED : " + obj.status.ToStr() + "'  where id=" + obj.bookingId.ToLong());
                            db.stp_BookingLog(obj.bookingId.ToLong(), "Customer", "PAYMENT FAILED - " + obj.status.ToStr());

                            // set Obj for driver app
                            DriverMsgObj.IsSuccess = false;
                            DriverMsgObj.Message = "PAYMENT FAILED";
                            DriverMsgObj.Data = obj.status.ToStr();
                        }
                    }
                    RefreshRequiredDashboard(obj.bookingId.ToStr());
                    if (IsPayByDriverApp && DriverId > 0)
                    {
                        string jsonMsg = new JavaScriptSerializer().Serialize(DriverMsgObj);
                        SocketIO.SendToSocket(DriverId.ToStr(), jsonMsg.ToStr(), "paymentresponse");

                    }

                }
            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\AuthUpdateDataFromPayByLinkKOnnectPay_exception.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {
                }
                rtn = "false";
            }
            return rtn;
        }
        /// <summary>
        /// This is webhook method handle  pay by link from driver app call back
        /// </summary>
        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("UpdateDataFromDriverAppKOnnectPay")]
        public string UpdateDataFromDriverAppKOnnectPay(PaymentUpdateDto obj)
        {

            string DefaultCurrencySign = Global.DefaultCurrencySign;// System.Configuration.ConfigurationManager.AppSettings["DefaultCurrencySign"];
            string rtn = "true";
            string json = string.Empty;
            try
            {
                json = new JavaScriptSerializer().Serialize(obj);

                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\UpdateDataFromDriverAppKOnnectPay.txt", DateTime.Now.ToStr() + " json" + json + Environment.NewLine);
                }
                catch
                {
                }
                if (obj == null) { return rtn = "false"; }

                obj.message = obj.message.ToStr();
                if (obj.message.ToStr().Trim().Contains("'"))
                    obj.message = obj.message.Replace("'", "").Trim();
                if (obj.message.ToStr().Trim().Contains("\u0027"))
                    obj.message = obj.message.Replace("\u0027", "").Trim();
                if (obj.isAuthorized)
                {
                    //this method is for handle Pre Auth Call back konnect pay
                    //  rtn = UpdateAuthStripePaymentKP(obj);
                }
                else
                {
                    bool IsPayByDriverApp = false;
                    int DriverId = 0;
                    ResponseData DriverMsgObj = new ResponseData();
                    //this is for pay by link Call back konnect pay
                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        var objBooking = db.Bookings.Where(c => c.Id == obj.bookingId).FirstOrDefault();
                        if (objBooking != null && objBooking.DriverId != null && objBooking.DriverId > 0)
                        {
                            IsPayByDriverApp = true;
                            DriverId = objBooking.DriverId.ToInt();
                        }
                        if (obj.status.ToStr().ToLower() == "succeeded" || obj.isSuccess)
                        {
                            string msg = "PAID " + DefaultCurrencySign + string.Format("{0:f2}", obj.amount) + " | TRANSACTION - " + obj.paymentIntentId.ToStr() + " | " + string.Format("{0:dd/MM/yyyy HH:mm}", DateTime.Now);

                            db.ExecuteQuery<int>("update booking set Paymenttypeid=6,IsQuotedPrice=1,PaymentComments='" + msg + "' where id=" + obj.bookingId.ToLong());

                            db.stp_BookingLog(obj.bookingId.ToLong(), "Customer", msg);

                            if ((db.ExecuteQuery<int>("select count(*) from booking_payment where bookingId=" + obj.bookingId.ToLong()).FirstOrDefault()) == 0)
                            {
                                // db.ExecuteQuery<int>("insert into Booking_Payment(bookingid,PaymentGatewayId,AuthCode)VALUES(" + obj.bookingId.ToLong() + ",15,'" + obj.paymentIntentId.ToStr() + "')");
                                db.ExecuteQuery<int>("insert into Booking_Payment(bookingid,PaymentGatewayId,AuthCode,Status,TotalAmount)VALUES(" + obj.bookingId.ToLong() + ",15,'" + obj.paymentIntentId.ToStr() + "','Paid'," + obj.amount + ")");
                            }
                            try
                            {    // set Obj for driver app
                                DriverMsgObj.IsSuccess = true;
                                DriverMsgObj.Message = "Transaction successful";
                                DriverMsgObj.Data = obj.paymentIntentId.ToStr();

                                long jobId = obj.bookingId.ToLong();


                                //var objBooking = db.Bookings.Where(c => c.Id == jobId).FirstOrDefault();
                                //if (objBooking != null && objBooking.CustomerId > 0 && !string.IsNullOrEmpty(obj.customerid))
                                //{
                                if (objBooking != null && !string.IsNullOrEmpty(obj.customerid))
                                {
                                    //if (objBooking.CustomerId == null || objBooking.CustomerId == 0)
                                    // {
                                    Customer objcustomer = db.Customers.Where(c => c.MobileNo == objBooking.CustomerMobileNo).FirstOrDefault();
                                    objBooking.CustomerId = objcustomer.Id;
                                    // }
                                    db.ExecuteQuery<int>("update Customer set CreditCardDetails='" + obj.customerid.ToStr() + "' where Id=" + objBooking.CustomerId);
                                    if ((db.ExecuteQuery<int>("select count(*) from Customer_CCDetails where CustomerId=" + objBooking.CustomerId.ToLong()).FirstOrDefault()) > 0)
                                    {
                                        db.ExecuteQuery<int>("update Customer_CCDetails set IsDefault=0 where CustomerId=" + objBooking.CustomerId);
                                    }

                                    // string CardDetails = $"Token: {obj.paymentMethodId} | {obj.message}";
                                    string CardDetails = $"KonnectPayToken: {obj.paymentMethodId} | {obj.message}";
                                    db.ExecuteQuery<int>("insert into Customer_CCDetails(CustomerId,CCDetails,AddOn,AddBy,IsDefault)VALUES(" + objBooking.CustomerId + ",'" + CardDetails + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','Eurosoft',1)");

                                }
                            }
                            catch
                            {
                            }
                            try
                            {

                                //long returnBookingId = db.ExecuteQuery<long>("select Id from booking where masterjobid=" + obj.bookingId).FirstOrDefault();
                                //if (returnBookingId > 0)
                                //{
                                //    string returnmsg = "PAID " + DefaultCurrencySign + string.Format("{0:f2}", obj.amount) + " | TRANSACTION - " + obj.paymentIntentId.ToStr() + " | " + string.Format("{0:dd/MM/yyyy HH:mm}", DateTime.Now);

                                //    db.ExecuteQuery<int>("update booking set Paymenttypeid=6,IsQuotedPrice=1,PaymentComments='" + returnmsg + "' where id=" + returnBookingId);
                                //    db.ExecuteQuery<int>("insert into Booking_Payment(bookingid,PaymentGatewayId,AuthCode,Status)VALUES(" + returnBookingId + ",15,'" + obj.paymentIntentId.ToStr() + "','Paid')");

                                //    try
                                //    {
                                //        File.AppendAllText(AppContext.BaseDirectory + "\\UpdateDataFromDriverAppKOnnectPay.txt", DateTime.Now.ToStr() + " updating ReturnBooking ID : " + returnBookingId + Environment.NewLine);
                                //    }
                                //    catch
                                //    {
                                //    }
                                //}


                            }
                            catch (Exception ex)
                            {
                                try
                                {
                                    File.AppendAllText(AppContext.BaseDirectory + "\\UpdateDataFromDriverAppKOnnectPay_exception.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                                }
                                catch
                                {
                                }
                            }
                        }
                        else
                        {
                            db.ExecuteQuery<int>("update booking set PaymentComments='PAYMENT FAILED : " + obj.status.ToStr() + "'  where id=" + obj.bookingId.ToLong());
                            db.stp_BookingLog(obj.bookingId.ToLong(), "Customer", "PAYMENT FAILED - " + obj.status.ToStr());

                            // set Obj for driver app
                            DriverMsgObj.IsSuccess = false;
                            DriverMsgObj.Message = "PAYMENT FAILED";
                            DriverMsgObj.Data = obj.status.ToStr();
                        }
                    }
                    RefreshRequiredDashboard(obj.bookingId.ToStr());
                    if (IsPayByDriverApp && DriverId > 0)
                    {
                        string jsonMsg = new JavaScriptSerializer().Serialize(DriverMsgObj);
                        SocketIO.SendToSocket(DriverId.ToStr(), jsonMsg.ToStr(), "paymentresponse");

                    }

                }
            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\UpdateDataFromDriverAppKOnnectPay_exception.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {
                }
                rtn = "false";
            }
            return rtn;
        }


        /// <summary>
        /// This  method handle pre auth callback
        /// </summary>
        public string UpdateAuthStripePaymentKP(PaymentUpdateDto obj)
        {
            string DefaultCurrencySign = Global.DefaultCurrencySign; // System.Configuration.ConfigurationManager.AppSettings["DefaultCurrencySign"];
            string rtn = "true";
            string json = string.Empty;
            try
            {
                json = new JavaScriptSerializer().Serialize(obj);
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\AuthStripePaymentKP.txt", DateTime.Now.ToStr() + " json" + json + Environment.NewLine);
                }
                catch
                {

                }

                using (TaxiDataContext db = new TaxiDataContext())
                {

                    if (obj.isSuccess)
                    {
                        string PaymentComment = "Authorised for " + DefaultCurrencySign + obj?.amount; // Convert.ToDouble((obj.amount / 100));
                        db.ExecuteQuery<int>("update booking set PaymentComments='" + PaymentComment + "',CompanyCreditCardDetails='" + obj?.status.ToStr() + "',CustomerCreditCardDetails='" + obj.paymentIntentId.ToStr() + "' where id=" + obj.bookingId.ToLong());

                        db.stp_BookingLog(obj.bookingId.ToLong(), "Customer", "Secure Card Transaction " + obj.paymentIntentId.ToStr() + " | " + obj.message.ToStr() + " | Pre-authorised " + DefaultCurrencySign + obj.amount);

                        var objBooking = db.Bookings.Where(c => c.Id == obj.bookingId).FirstOrDefault();
                        if (objBooking != null && objBooking.CustomerId > 0 && !string.IsNullOrEmpty(obj.customerid))
                        {
                            db.ExecuteQuery<int>("update Customer set CreditCardDetails='" + obj.customerid.ToStr() + "' where Id=" + objBooking.CustomerId);
                            if ((db.ExecuteQuery<int>("select count(*) from Customer_CCDetails where CustomerId=" + objBooking.CustomerId.ToLong()).FirstOrDefault()) > 0)
                            {
                                db.ExecuteQuery<int>("update Customer_CCDetails set IsDefault=0 where CustomerId=" + objBooking.CustomerId);
                            }

                            // string CardDetails = $"Token: {obj.paymentMethodId} | {obj.message}";
                            string CardDetails = $"KonnectPayToken: {obj.paymentMethodId} | {obj.message}";
                            db.ExecuteQuery<int>("insert into Customer_CCDetails(CustomerId,CCDetails,AddOn,AddBy,IsDefault)VALUES(" + objBooking.CustomerId + ",'" + CardDetails + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','Eurosoft',1)");

                        }

                    }
                    else
                    {

                        if (obj.message.ToStr().Trim().Contains("'"))
                            obj.message = obj.message.Replace("'", "").Trim();

                        if (obj.message.ToStr().Trim().Contains("\u0027"))
                            obj.message = obj.message.Replace("\u0027", "").Trim();



                        db.ExecuteQuery<int>("update booking set PaymentComments='" + obj.message + "'  where id=" + obj.bookingId.ToLong());

                        db.stp_BookingLog(obj.bookingId.ToLong(), "Customer", obj.message.ToStr());


                    }
                }


            }
            catch (Exception ex)
            {
                try
                {

                    File.AppendAllText(AppContext.BaseDirectory + "\\AuthStripePaymentKP_exception.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {

                }
                rtn = "false";

            }

            return rtn;

        }

        /// <summary>
        /// This is payment verification method for konnect pay
        /// </summary>

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("VerifyPaymentKOnnectPay/{id}")]
        public PaymentExistDto VerifyPaymentKOnnectPay(long id)
        {

            PaymentExistDto obj = new PaymentExistDto();
            string json = string.Empty;
            try
            {

                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\VerifyPaymentKOnnectPay.txt", DateTime.Now.ToStr() + " json" + id + Environment.NewLine);
                }
                catch
                {
                }
                obj.isSuccess = false;
                using (TaxiDataContext db = new TaxiDataContext())
                {
                    var objBooking = db.Bookings.Where(c => c.Id == id && c.PaymentTypeId == Enums.PAYMENT_TYPES.CREDIT_CARD_PAID)
                              .Select(args => new { args.PaymentComments }).FirstOrDefault();

                    var ObjPreAuth = db.Bookings.Where(c => c.Id == id).Select(args => new { args.PaymentComments, args.CustomerCreditCardDetails }).FirstOrDefault();

                    if (objBooking != null)
                    {
                        obj.isSuccess = true;
                        obj.status = "PAID";
                        obj.paymentIntentId = objBooking.PaymentComments;
                        obj.message = "successed";
                    }
                    else if (ObjPreAuth != null && (!string.IsNullOrEmpty(ObjPreAuth.PaymentComments) && ObjPreAuth.PaymentComments.ToLower().Contains("authorised") && ObjPreAuth.CustomerCreditCardDetails.ToStr().Trim().StartsWith("pi_") && !ObjPreAuth.CustomerCreditCardDetails.ToStr().Trim().Contains("secret_")))
                    {
                        obj.isSuccess = true;
                        obj.status = "Authorised";
                        obj.paymentIntentId = ObjPreAuth.PaymentComments + " | TRANSACTION " + ObjPreAuth.CustomerCreditCardDetails;
                        obj.message = "successed";

                    }

                }

                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\VerifyPaymentKOnnectPay.txt", DateTime.Now.ToStr() + " json" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
                }
                catch
                {
                }
            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\VerifyPaymentKOnnectPay_exception.txt", DateTime.Now.ToStr() + " json" + new JavaScriptSerializer().Serialize(obj) + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {
                }
            }
            return obj;
        }



        /// <summary>
        /// This is Register card method for konnect pay
        /// </summary>
        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("RegisterCardKP")]
        public string RegisterCardKP(string value)
        {
            RegisterCardDto obj = null;
            string rtn = string.Empty;
            string json = value;
            try
            {
                obj = new JavaScriptSerializer().Deserialize<RegisterCardDto>(json);
                string url = obj.UpdatePaymentURL.ToStr();
                obj.paymentUpdateWebhook = url + "/api/Supplier/UpdateDataFromCardRegisterKOnnectPay";

                string data = new JavaScriptSerializer().Serialize(obj);
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\RegisterCardKP.txt", DateTime.Now.ToStr() + " json" + new JavaScriptSerializer().Serialize(data) + Environment.NewLine);
                }
                catch
                {
                }
                string StripeAPIBaseURL = System.Configuration.ConfigurationManager.AppSettings["StripeAPIBaseURL"];
                obj.PreAuthUrl = StripeAPIBaseURL + "/registercard?data=" + EncodeBASE64(data);

                if (obj.PreAuthUrl.ToStr().Contains("tinyurl") == false)
                {
                    obj.PreAuthUrl = General.ToTinyURLS(obj.PreAuthUrl.ToStr());
                    rtn = obj.PreAuthUrl;
                }
                if (obj.sendLinkToCustomer)
                {
                    if (!string.IsNullOrEmpty(obj?.phoneNumber))
                    {
                        HubProcessor.Instance.listofSMS.Add("request dispatchsms = " + obj.phoneNumber.Trim() + " =" + "Please click on the link to Register card" + Environment.NewLine + obj.PreAuthUrl.ToStr());
                    }
                    if (!string.IsNullOrEmpty(obj?.email))
                    {
                        using (TaxiDataContext db = new TaxiDataContext())
                        {
                            var genSubCompany = db.Gen_SubCompanies

                                    .Select(args => new
                                    {
                                        args.CompanyName,
                                        args.EmailAddress,
                                        args.SmtpHost,
                                        args.SmtpPassword,
                                        args.SmtpUserName,
                                        args.SmtpPort,
                                        args.SmtpHasSSL,
                                        args.EmailCC
                                    }
                                    ).FirstOrDefault();
                            if (genSubCompany == null)
                            {
                                genSubCompany = db.Gen_SubCompanies.Where(c => c.Id == 1)
                                   .Select(args => new
                                   {
                                       args.CompanyName,
                                       args.EmailAddress,
                                       args.SmtpHost,
                                       args.SmtpPassword,
                                       args.SmtpUserName,
                                       args.SmtpPort,
                                       args.SmtpHasSSL,
                                       args.EmailCC
                                   }
                                   ).FirstOrDefault();
                            }
                            ClsEASendEmail cls = new ClsEASendEmail(genSubCompany.EmailAddress, obj.email.ToStr().Trim(), "Link for Register card ", "Please click on the link to Register Card for your journey with " + obj?.companyName.ToStr() + Environment.NewLine + ".<br/>" + "<a href='" + obj.PreAuthUrl.ToStr() + "'>" + obj.PreAuthUrl.ToStr() + "</a>", genSubCompany.SmtpHost, genSubCompany.EmailCC);
                            bool IsSent = cls.Send(obj.email.ToStr(), genSubCompany.SmtpUserName, genSubCompany.SmtpPassword);

                        }
                    }
                    rtn = "true";
                }

            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\RegisterCardKP_exception.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {
                }

            }
            return rtn;
        }



        /// <summary>
        /// This is Webhook method for Register card method. handle register card callback 
        /// </summary>

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("UpdateDataFromCardRegisterKOnnectPay")]
        public string UpdateDataFromCardRegisterKOnnectPay(RegisterCardUpdateDto obj)
        {
            string rtn = "true";
            string json = string.Empty;
            try
            {
                json = new JavaScriptSerializer().Serialize(obj);

                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\UpdateDataFromCardRegisterKOnnectPay.txt", DateTime.Now.ToStr() + " json" + json + Environment.NewLine);
                }
                catch
                {
                }

                obj.message = obj.message.ToStr();
                if (obj.message.ToStr().Trim().Contains("'"))
                    obj.message = obj.message.Replace("'", "").Trim();
                if (obj.message.ToStr().Trim().Contains("\u0027"))
                    obj.message = obj.message.Replace("\u0027", "").Trim();

                using (TaxiDataContext db = new TaxiDataContext())
                {

                    if (obj.status.ToStr().ToLower() == "succeeded" || obj.isSuccess)
                    {
                        string CustomerPhoneNumber = obj.phoneNumber;
                        long Customerid = 0;
                        if (!string.IsNullOrEmpty(obj.customerName) && obj.customerName.Contains('/'))
                        {
                            var dt = obj.customerName.Split('/');
                            if (dt != null && dt.Length > 0) { Customerid = Convert.ToInt32(dt[1]); }
                        }



                        Customer objcustomer = db.Customers.Where(c => c.MobileNo == CustomerPhoneNumber).FirstOrDefault();
                        //if (Customerid > 0)
                        //{
                        //    objcustomer = db.Customers.Where(c => c.Id == Customerid).FirstOrDefault();
                        //}
                        //else
                        //{
                        //    objcustomer = db.Customers.Where(c => c.MobileNo == CustomerPhoneNumber).FirstOrDefault();
                        //}

                        if (objcustomer != null && !string.IsNullOrEmpty(obj.customerid) && !string.IsNullOrEmpty(obj.paymentMethodId))
                        {
                            db.ExecuteQuery<int>("update Customer set CreditCardDetails='" + obj.customerid.ToStr() + "' where Id=" + objcustomer.Id);
                            if ((db.ExecuteQuery<int>("select count(*) from Customer_CCDetails where CustomerId=" + objcustomer.Id.ToLong()).FirstOrDefault()) > 0)
                            {
                                db.ExecuteQuery<int>("update Customer_CCDetails set IsDefault=0 where CustomerId=" + objcustomer.Id);
                            }

                            // string CardDetails = $"Token: {obj.paymentMethodId} | {obj.message}";
                            string CardDetails = $"KonnectPayToken: {obj.paymentMethodId} | {obj.message}";
                            db.ExecuteQuery<int>("insert into Customer_CCDetails(CustomerId,CCDetails,AddOn,AddBy,IsDefault)VALUES(" + objcustomer.Id + ",'" + CardDetails + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','Eurosoft',1)");

                            rtn = "true";

                        }
                        else
                        {
                            try
                            {
                                File.AppendAllText(AppContext.BaseDirectory + "\\UpdateDataFromCardRegisterKOnnectPay.txt", DateTime.Now.ToStr() + "Error in saving Card Details of Customer :" + new JavaScriptSerializer().Serialize(objcustomer) + Environment.NewLine);
                            }
                            catch
                            {
                            }

                        }

                    }

                }

            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\UpdateDataFromCardRegisterKOnnectPay_exception.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {
                }
                rtn = "false";
            }
            return rtn;
        }



        /// <summary>
        /// This method handle payment process for registered customer
        /// </summary>

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("ProcessPaymentWithRegisterCustomer")]
        public string ProcessPaymentWithRegisterCustomer(string value)
        {
            StripePaymentRequestDto ReqObj = null;
            PaymentWithExistingCustomerResponse obj = null;
            string rtn = "true";
            string json = value;

            string DefaultCurrencySign = Global.DefaultCurrencySign;// System.Configuration.ConfigurationManager.AppSettings["DefaultCurrencySign"];
            try
            {
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\ProcessPaymentWithRegisterCustomer.txt", DateTime.Now.ToStr() + " json" + json + Environment.NewLine);
                }
                catch
                {
                }
                ReqObj = new JavaScriptSerializer().Deserialize<StripePaymentRequestDto>(json);


                var DataObj = Newtonsoft.Json.JsonConvert.SerializeObject(ReqObj);
                string StripeAPIBaseURL = System.Configuration.ConfigurationManager.AppSettings["StripeAPIBaseURL"];
                using (var client = new HttpClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    client.BaseAddress = new Uri(StripeAPIBaseURL);
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    var content = new StringContent(DataObj, Encoding.UTF8, "application/json");
                    var postTask = client.PostAsync(StripeAPIBaseURL + "/v1/PaymentWithExistingCustomer", content).Result;
                    string PayResp = postTask.Content.ReadAsStringAsync().Result;
                    obj = new JavaScriptSerializer().Deserialize<PaymentWithExistingCustomerResponse>(PayResp);

                    if (obj != null)
                    {
                        obj.bookingId = ReqObj.bookingId;
                        if (obj.isAuthorized)
                        {
                            using (TaxiDataContext db = new TaxiDataContext())
                            {

                                if (obj.isSuccess)
                                {
                                    string PaymentComment = "Authorised for " + DefaultCurrencySign + obj.amount; // Convert.ToDouble((obj.amount / 100));
                                    db.ExecuteQuery<int>("update booking set PaymentComments='" + PaymentComment + "',CompanyCreditCardDetails='" + obj.status.ToStr() + "',CustomerCreditCardDetails='" + obj.paymentIntentId.ToStr() + "' where id=" + obj.bookingId.ToLong());

                                    db.stp_BookingLog(obj.bookingId.ToLong(), "Customer", "Secure Card Transaction " + obj.paymentIntentId.ToStr() + " | Pre-authorised " + DefaultCurrencySign + obj.amount);

                                    var objBooking = db.Bookings.Where(c => c.Id == obj.bookingId).FirstOrDefault();


                                }
                                else
                                {

                                    rtn = "Error in payment process!";
                                    db.ExecuteQuery<int>("update booking set PaymentComments='Authorization FAILED : " + obj.status.ToStr() + "'  where id=" + obj.bookingId.ToLong());
                                    db.stp_BookingLog(obj.bookingId.ToLong(), "Customer", "PAYMENT FAILED - " + obj.error.ToStr());
                                    if (!string.IsNullOrEmpty(obj.error)) { rtn = obj.error; }


                                }
                            }
                        }
                        else
                        {
                            using (TaxiDataContext db = new TaxiDataContext())
                            {

                                if (obj.status.ToStr().ToLower() == "succeeded" || obj.isSuccess)
                                {
                                    rtn = "true |" + obj.paymentIntentId;

                                }
                                else
                                {
                                    rtn = "Error in payment process!";
                                    db.ExecuteQuery<int>("update booking set PaymentComments='PAYMENT FAILED : " + obj.status.ToStr() + "'  where id=" + obj.bookingId.ToLong());
                                    db.stp_BookingLog(obj.bookingId.ToLong(), "Customer", "PAYMENT FAILED - " + obj.error.ToStr());
                                    if (!string.IsNullOrEmpty(obj.error)) { rtn = obj.error; }
                                }

                            }
                        }

                        try
                        {
                            File.AppendAllText(AppContext.BaseDirectory + "\\ProcessPaymentWithRegisterCustomer.txt", DateTime.Now.ToStr() + " payment response " + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
                        }
                        catch
                        {
                        }
                        //RefreshRequiredDashboard(obj.bookingId.ToStr());
                    }

                }




            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\ProcessPaymentWithRegisterCustomer_exception.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {
                }
                rtn = "Error in payment process!";
            }
            return rtn;
        }


        /// <summary>
        /// This method handle cancel payment intent
        /// </summary>
        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("CancelPaymentKP")]
        public string CancelPaymentKP(string value)
        {
            CanclePaymentDTO ReqDTO = null;
            string rtn = "true";
            string json = value;
            PaymentCaptureResponse resp = new PaymentCaptureResponse();
            string DefaultCurrencySign = Global.DefaultCurrencySign; // System.Configuration.ConfigurationManager.AppSettings["DefaultCurrencySign"];
            try
            {

                ReqDTO = new JavaScriptSerializer().Deserialize<CanclePaymentDTO>(json);


                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\CancelPaymentKonnectPay.txt", DateTime.Now.ToStr() + " json" + new JavaScriptSerializer().Serialize(ReqDTO) + Environment.NewLine);
                }
                catch
                {

                }
                Gen_SysPolicy_PaymentDetail KPDetails = General.GetObject<Gen_SysPolicy_PaymentDetail>(c => c.PaymentGatewayId == 15);
                if (KPDetails == null)
                {
                    return rtn = "false";
                }
                string StripeAPIBaseURL = System.Configuration.ConfigurationManager.AppSettings["StripeAPIBaseURL"];
                Taxi_Model.Booking objBooking = General.GetObject<Taxi_Model.Booking>(c => c.Id == ReqDTO.bookingId);
                if (objBooking == null) { return rtn = "false"; }

                ReqDTO.bookingRef = objBooking.BookingNo;
                ReqDTO.paymentIntentId = objBooking.CustomerCreditCardDetails;
                ReqDTO.status = objBooking.CompanyCreditCardDetails;
                ReqDTO.connectedAccountId = KPDetails.PaypalID;
                ReqDTO.countryId = KPDetails.ApplicationId.ToInt();

                string data = new JavaScriptSerializer().Serialize(ReqDTO);
                using (var client = new HttpClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    client.BaseAddress = new Uri(StripeAPIBaseURL);
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    var content = new StringContent(data, Encoding.UTF8, "application/json");
                    var postTask = client.PostAsync(StripeAPIBaseURL + "/v1/CancelPayment", content).Result;
                    string result = postTask.Content.ReadAsStringAsync().Result;
                    resp = new JavaScriptSerializer().Deserialize<PaymentCaptureResponse>(result);
                    if (resp.isSuccess || resp.status.Contains("canceled"))
                    {
                        rtn = resp.status;
                        using (TaxiDataContext db = new TaxiDataContext())
                        {
                            db.stp_BookingLog(ReqDTO.bookingId.ToLong(), "Eurosoft", "Payment Intent " + ReqDTO.paymentIntentId + " is Cancelled");

                        }
                    }
                    else { rtn = resp.error; }
                    try
                    {
                        File.AppendAllText(AppContext.BaseDirectory + "\\CancelPaymentKonnectPay.txt", DateTime.Now.ToStr() + " Api resp :" + new JavaScriptSerializer().Serialize(resp) + Environment.NewLine);
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
                    File.AppendAllText(AppContext.BaseDirectory + "\\CancelPaymentKonnectPay_exception.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {
                }
                rtn = "false";
            }
            return rtn;
        }

        /// <summary>
        /// This method handle refund process for konnect pay
        /// </summary>
        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("RefundPaymentKonnectPay")]
        public string RefundPaymentKonnectPay(string value)
        {
            RefundPaymentDto ReqDTO = null;
            string rtn = "true";
            string json = value;
            PaymentRefundResponse resp = new PaymentRefundResponse();
            string DefaultCurrencySign = Global.DefaultCurrencySign;// System.Configuration.ConfigurationManager.AppSettings["DefaultCurrencySign"];
            string DefaultCurrency = System.Configuration.ConfigurationManager.AppSettings["DefaultCurrency"];
            try
            {

                ReqDTO = new JavaScriptSerializer().Deserialize<RefundPaymentDto>(json);


                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\RefundPaymentKonnectPay.txt", DateTime.Now.ToStr() + " json" + new JavaScriptSerializer().Serialize(ReqDTO) + Environment.NewLine);
                }
                catch
                {
                }
                Gen_SysPolicy_PaymentDetail KPDetails = General.GetObject<Gen_SysPolicy_PaymentDetail>(c => c.PaymentGatewayId == 15);
                if (KPDetails == null)
                {
                    return rtn = "false";
                }
                string StripeAPIBaseURL = System.Configuration.ConfigurationManager.AppSettings["StripeAPIBaseURL"];
                Taxi_Model.Booking objBooking = General.GetObject<Taxi_Model.Booking>(c => c.Id == ReqDTO.bookingId);
                if (objBooking == null) { return rtn = "false"; }
                string paymentIntentId = string.Empty;
                if (objBooking?.BookingPayment != null && !string.IsNullOrEmpty(objBooking?.BookingPayment?.AuthCode))
                {
                    paymentIntentId = objBooking?.BookingPayment?.AuthCode.ToStr();
                    if (!string.IsNullOrEmpty(paymentIntentId) && paymentIntentId.Contains('"'))
                    {
                        paymentIntentId = paymentIntentId.Replace("\"", "");

                    }
                }
                else { paymentIntentId = objBooking.CustomerCreditCardDetails; }

                ReqDTO.bookingRef = objBooking.BookingNo ?? ReqDTO.bookingId.ToString();
                ReqDTO.description = ReqDTO.description;

                if (ReqDTO.description.Contains("'"))
                {
                    ReqDTO.description = ReqDTO.description.Replace("'", "");
                }

                ReqDTO.paymentIntentId = paymentIntentId;
                ReqDTO.status = objBooking.CompanyCreditCardDetails;
                ReqDTO.connectedAccountId = KPDetails.PaypalID;
                ReqDTO.countryId = KPDetails.ApplicationId.ToInt();
                ReqDTO.isRefundApplicationFee = false;
                ReqDTO.currency = DefaultCurrency;
                ReqDTO.status = "paid";
                ReqDTO.companyName = objBooking?.Gen_SubCompany?.CompanyName ?? "";
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\RefundPaymentKonnectPay.txt", DateTime.Now.ToStr() + " ReqDTO" + new JavaScriptSerializer().Serialize(ReqDTO) + Environment.NewLine);
                }
                catch
                {
                }
                string data = new JavaScriptSerializer().Serialize(ReqDTO);
                using (var client = new HttpClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    client.BaseAddress = new Uri(StripeAPIBaseURL);
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    var content = new StringContent(data, Encoding.UTF8, "application/json");
                    var postTask = client.PostAsync(StripeAPIBaseURL + "/v1/RefundPayment", content).Result;
                    string result = postTask.Content.ReadAsStringAsync().Result;
                    resp = new JavaScriptSerializer().Deserialize<PaymentRefundResponse>(result);
                    if (resp.isSuccess)
                    {
                        rtn = "Payment Refunded Successfully!";
                        using (TaxiDataContext db = new TaxiDataContext())
                        {
                            db.stp_BookingLog(ReqDTO.bookingId.ToLong(), "Eurosoft", ReqDTO.description);

                        }
                    }
                    else
                    {

                        if (resp.error != null)
                        {
                            rtn = resp.error[0];
                        }
                    }
                }
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\RefundPaymentKonnectPay.txt", DateTime.Now.ToStr() + " ReqDTO" + new JavaScriptSerializer().Serialize(ReqDTO) + " json" + new JavaScriptSerializer().Serialize(resp) + Environment.NewLine);
                }
                catch
                {
                }
            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\RefundPaymentKonnectPay_exception.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {
                }
                rtn = "Error in Refund Process";
            }
            return rtn;
        }


        /// <summary>
        /// This method process pre authorize payment for konnect pay
        /// </summary>
        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("ProcessPreAuthPaymentKP")]
        public PaymentCaptureResponse ProcessPreAuthPaymentKP(string value)
        {
            PaymentCaptureDto ReqObj = null;
            PaymentCaptureResponse obj = null;
            string json = value;


            try
            {
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\ProcessPreAuthPaymentKP.txt", DateTime.Now.ToStr() + " json" + json + Environment.NewLine);
                }
                catch
                {
                }
                ReqObj = new JavaScriptSerializer().Deserialize<PaymentCaptureDto>(json);


                var DataObj = Newtonsoft.Json.JsonConvert.SerializeObject(ReqObj);
                string StripeAPIBaseURL = System.Configuration.ConfigurationManager.AppSettings["StripeAPIBaseURL"];
                using (var client = new HttpClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    client.BaseAddress = new Uri(StripeAPIBaseURL);
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    var content = new StringContent(DataObj, Encoding.UTF8, "application/json");
                    var postTask = client.PostAsync(StripeAPIBaseURL + "/v1/CapturePayment/IncrementAuthorization", content).Result;
                    string PayResp = postTask.Content.ReadAsStringAsync().Result;
                    obj = new JavaScriptSerializer().Deserialize<PaymentCaptureResponse>(PayResp);

                    if (obj != null)
                    {
                        try
                        {
                            File.AppendAllText(AppContext.BaseDirectory + "\\ProcessPreAuthPaymentKP.txt", DateTime.Now.ToStr() + " CapturePayment response " + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
                        }
                        catch
                        { }
                    }

                }




            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\ProcessPreAuthPaymentKP_exception.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {
                }
            }
            return obj;
        }

        /// <summary>
        /// This method process  payment for Customer app
        /// </summary>
        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("ProcessPaymentKPCustomerApp")]
        public PaymentWithExistingCustomerResponse ProcessPaymentKPCustomerApp(string value)
        {
            Classes.KonnectSupplier.StripePaymentRequestDto ReqObj = null;
            PaymentWithExistingCustomerResponse obj = new PaymentWithExistingCustomerResponse();
            string rtn = string.Empty;
            string json = value;

            try
            {
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\ProcessPaymentKPCustomerApp.txt", DateTime.Now.ToStr() + " json" + json + Environment.NewLine);
                }
                catch
                {
                }
                ReqObj = new JavaScriptSerializer().Deserialize<Classes.KonnectSupplier.StripePaymentRequestDto>(json);
                Gen_SysPolicy_PaymentDetail KPDetails = General.GetObject<Gen_SysPolicy_PaymentDetail>(c => c.PaymentGatewayId == 15);
                if (KPDetails == null || string.IsNullOrEmpty(KPDetails.PaypalID) || string.IsNullOrEmpty(KPDetails.ApplicationId))
                {
                    obj.isSuccess = false;
                    obj.error = "System config is missing for KonnectPay!";
                    return obj;
                }
                ReqObj.connectedAccountId = KPDetails.PaypalID;
                ReqObj.countryId = KPDetails.ApplicationId.ToInt();

                var DataObj = Newtonsoft.Json.JsonConvert.SerializeObject(ReqObj);
                string StripeAPIBaseURL = System.Configuration.ConfigurationManager.AppSettings["StripeAPIBaseURL"];
                using (var client = new HttpClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    client.BaseAddress = new Uri(StripeAPIBaseURL);
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    var content = new StringContent(DataObj, Encoding.UTF8, "application/json");
                    var postTask = client.PostAsync(StripeAPIBaseURL + "/v1/PaymentWithExistingCustomer", content).Result;
                    rtn = postTask.Content.ReadAsStringAsync().Result;
                    obj = new JavaScriptSerializer().Deserialize<PaymentWithExistingCustomerResponse>(rtn);
                    try
                    {
                        File.AppendAllText(AppContext.BaseDirectory + "\\ProcessPaymentKPCustomerApp.txt", DateTime.Now.ToStr() + " Stripe API Response" + rtn + Environment.NewLine);
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
                    File.AppendAllText(AppContext.BaseDirectory + "\\ProcessPaymentKPCustomerApp_exception.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {
                }
                obj.isSuccess = false;
                obj.error = "Error in processing Payment!";
            }
            return obj;
        }
        public class ResponseSupplierApi
        {
            public bool HasError { get; set; }
            public string Message { get; set; }
            public object Data { get; set; }
        }
        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetClientKonnectPayConfig")]
        public ResponseSupplierApi GetClientKonnectPayConfig()
        {
            ResponseSupplierApi response = new ResponseSupplierApi();
            try
            {

                string StripeAPIURL = System.Configuration.ConfigurationManager.AppSettings["StripeAPIBaseURL"];
                Gen_SysPolicy_PaymentDetail KPinfo = General.GetObject<Gen_SysPolicy_PaymentDetail>(c => c.PaymentGatewayId == 15);
                response.Data = new { KonnectAPIURL = StripeAPIURL, AccountID = KPinfo?.PaypalID, CountryID = KPinfo?.ApplicationId };
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.HasError = true;
            }

            return response;
        }
        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("SendPayByLinkDriverAppKP")]
        public string SendPayByLinkDriverAppKP(string value)
        {
            StripePaymentRequestDto obj = null;
            string rtn = "true";
            string json = value;
            bool IsSent = false;

            string DefaultCurrencySign = Global.DefaultCurrencySign; // System.Configuration.ConfigurationManager.AppSettings["DefaultCurrencySign"];
            try
            {
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\SendPayByLinkDriverAppKP.txt", DateTime.Now.ToStr() + " json" + json + Environment.NewLine);
                }
                catch
                {
                }
                //
                obj = new JavaScriptSerializer().Deserialize<StripePaymentRequestDto>(json);
                string url = obj.UpdatePaymentURL.ToStr();
                obj.verificationWebhook = url + "/api/Supplier/VerifyPaymentKOnnectPay";
                obj.paymentUpdateWebhook = url + "/api/Supplier/UpdateDataFromPayByLinkDriverAppKOnnectPay";

                string data = new JavaScriptSerializer().Serialize(obj);
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\SendPayByLinkDriverAppKP.txt", DateTime.Now.ToStr() + " json" + new JavaScriptSerializer().Serialize(obj) + Environment.NewLine);
                }
                catch
                {
                }
                string StripeAPIBaseURL = System.Configuration.ConfigurationManager.AppSettings["StripeAPIBaseURL"];
                obj.PreAuthUrl = StripeAPIBaseURL + "/checkout?data=" + EncodeBASE64(data);

                if (obj.PreAuthUrl.ToStr().Contains("tinyurl") == false)
                {
                    obj.PreAuthUrl = General.ToTinyURLS(obj.PreAuthUrl.ToStr());
                    try
                    {
                        File.AppendAllText(AppContext.BaseDirectory + "\\SendPayByLinkCustomLink.txt", DateTime.Now.ToStr() + " url=" + obj.PreAuthUrl + Environment.NewLine);
                    }
                    catch
                    {
                    }
                }
                if (obj.PayByDispatch == "1")
                {
                    rtn = obj.PreAuthUrl;
                }
                else
                {
                    if (!string.IsNullOrEmpty(obj?.phoneNumber))
                    {
                        string returnPayment = string.Empty;
                        if (obj.ReturnAmount > 0)
                        {
                            returnPayment = " (Inc. Return)";
                        }
                        HubProcessor.Instance.listofSMS.Add("request dispatchsms = " + obj.phoneNumber.Trim() + " =" + "Please click on the link to process your payment for your journey with " + obj?.companyName + Environment.NewLine + "Ref No - " + obj?.bookingRef.ToStr().Trim() + Environment.NewLine + obj.PreAuthUrl.ToStr());
                        using (TaxiDataContext db = new TaxiDataContext())
                        {
                            db.stp_BookingLog(obj.bookingId.ToLong(), obj.OperatorName.ToStr(), "PAYMENT LINK for " + DefaultCurrencySign + string.Format("{0:f2}", Convert.ToDouble((obj.amount / 100))) + returnPayment + " SENT TO : " + obj.phoneNumber.ToStr());

                        }
                        rtn = "true";
                    }
                    if (!string.IsNullOrEmpty(obj?.email))
                    {
                        using (TaxiDataContext db = new TaxiDataContext())
                        {
                            var genSubCompany = db.Gen_SubCompanies

                                    .Select(args => new
                                    {
                                        args.CompanyName,
                                        args.EmailAddress,
                                        args.SmtpHost,
                                        args.SmtpPassword,
                                        args.SmtpUserName,
                                        args.SmtpPort,
                                        args.SmtpHasSSL,
                                        args.EmailCC
                                    }
                                    ).FirstOrDefault();
                            if (genSubCompany == null)
                            {
                                genSubCompany = db.Gen_SubCompanies.Where(c => c.Id == 1)
                                   .Select(args => new
                                   {
                                       args.CompanyName,
                                       args.EmailAddress,
                                       args.SmtpHost,
                                       args.SmtpPassword,
                                       args.SmtpUserName,
                                       args.SmtpPort,
                                       args.SmtpHasSSL,
                                       args.EmailCC
                                   }
                                   ).FirstOrDefault();
                            }
                            ClsEASendEmail cls = new ClsEASendEmail(genSubCompany.EmailAddress, obj.email.ToStr().Trim(), "Link for Card Payment " + obj?.bookingRef.ToStr(), "Please click on the link to process the payment for your journey with " + obj?.companyName.ToStr() + Environment.NewLine + "Ref No - " + obj?.bookingRef.ToStr().Trim() + ".<br/>" + "<a href='" + obj.PreAuthUrl.ToStr() + "'>" + obj.PreAuthUrl.ToStr() + "</a>", genSubCompany.SmtpHost, genSubCompany.EmailCC);
                            IsSent = cls.Send(obj.email.ToStr(), genSubCompany.SmtpUserName, genSubCompany.SmtpPassword);

                            if (IsSent)
                            {
                                try
                                {
                                    File.AppendAllText(AppContext.BaseDirectory + "\\SendPayByLinkDriverAppKP_email.txt", DateTime.Now.ToStr() + " json" + obj.email.ToStr().Trim() + "," + obj.PreAuthUrl.ToStr() + " ,from:" + genSubCompany.SmtpUserName + ", Password: " + genSubCompany.SmtpPassword + Environment.NewLine);
                                }
                                catch
                                {
                                }
                                string returnPayment = string.Empty;
                                if (obj.ReturnAmount > 0)
                                {
                                    returnPayment = " (Inc. Return)";
                                }
                                db.stp_BookingLog(obj.bookingId.ToLong(), obj.OperatorName.ToStr(), "PAYMENT LINK for " + DefaultCurrencySign + string.Format("{0:f2}", Convert.ToDouble((obj.amount / 100))) + returnPayment + " SENT TO : " + obj.email.ToStr());
                                rtn = "true";
                            }
                            else
                                rtn = "false";

                        }
                    }

                    try
                    {
                        File.AppendAllText(AppContext.BaseDirectory + "\\SendPayByLinkDriverAppKPsms.txt", DateTime.Now.ToStr() + obj.phoneNumber.ToStr() + " Please click on the link to process your payment for your journey with " + obj?.companyName.ToStr() + Environment.NewLine + "Ref No - " + obj?.bookingRef.ToStr().Trim() + Environment.NewLine + obj.PreAuthUrl.ToStr() + Environment.NewLine);
                        if (obj.ReturnAmount > 0)
                        {
                            File.WriteAllText(AppContext.BaseDirectory + "\\paybylink\\" + obj.bookingId.ToStr() + ".txt", obj.ReturnAmount.ToStr());
                        }
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
                    File.AppendAllText(AppContext.BaseDirectory + "\\SendPayByLinkDriverAppKP_exception.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {
                }
                rtn = "false";
            }
            return rtn;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("UpdateDataFromPayByLinkDriverAppKOnnectPay")]
        public string UpdateDataFromPayByLinkDriverAppKOnnectPay(PaymentUpdateDto obj)
        {

            string DefaultCurrencySign = Global.DefaultCurrencySign;// System.Configuration.ConfigurationManager.AppSettings["DefaultCurrencySign"];
            string rtn = "true";
            string json = string.Empty;
            try
            {
                json = new JavaScriptSerializer().Serialize(obj);

                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\UpdateDataFromPayByLinkDriverAppKOnnectPay.txt", DateTime.Now.ToStr() + " json" + json + Environment.NewLine);
                }
                catch
                {
                }
                if (obj == null) { return rtn = "false"; }

                obj.message = obj.message.ToStr();
                if (obj.message.ToStr().Trim().Contains("'"))
                    obj.message = obj.message.Replace("'", "").Trim();
                if (obj.message.ToStr().Trim().Contains("\u0027"))
                    obj.message = obj.message.Replace("\u0027", "").Trim();
                if (obj.isAuthorized)
                {
                    //this method is for handle Pre Auth Call back konnect pay
                    rtn = UpdateAuthStripePaymentKP(obj);
                }
                else
                {
                    bool IsPayByDriverApp = false;
                    int DriverId = 0;
                    ResponseData DriverMsgObj = new ResponseData();
                    //this is for pay by link Call back konnect pay
                    using (TaxiDataContext db = new TaxiDataContext())
                    {
                        var objBooking = db.Bookings.Where(c => c.Id == obj.bookingId).FirstOrDefault();
                        if (objBooking != null && objBooking.DriverId != null && objBooking.DriverId > 0)
                        {
                            IsPayByDriverApp = true;
                            DriverId = objBooking.DriverId.ToInt();
                        }
                        if (obj.status.ToStr().ToLower() == "succeeded" || obj.isSuccess)
                        {
                            string msg = "PAID " + DefaultCurrencySign + string.Format("{0:f2}", obj.amount) + " | TRANSACTION - " + obj.paymentIntentId.ToStr() + " | " + string.Format("{0:dd/MM/yyyy HH:mm}", DateTime.Now);

                            db.ExecuteQuery<int>("update booking set Paymenttypeid=6,IsQuotedPrice=1,PaymentComments='" + msg + "' where id=" + obj.bookingId.ToLong());

                            db.stp_BookingLog(obj.bookingId.ToLong(), "Customer", msg);

                            if ((db.ExecuteQuery<int>("select count(*) from booking_payment where bookingId=" + obj.bookingId.ToLong()).FirstOrDefault()) == 0)
                            {
                                // db.ExecuteQuery<int>("insert into Booking_Payment(bookingid,PaymentGatewayId,AuthCode)VALUES(" + obj.bookingId.ToLong() + ",15,'" + obj.paymentIntentId.ToStr() + "')");
                                db.ExecuteQuery<int>("insert into Booking_Payment(bookingid,PaymentGatewayId,AuthCode,Status,TotalAmount)VALUES(" + obj.bookingId.ToLong() + ",15,'" + obj.paymentIntentId.ToStr() + "','Paid'," + obj.amount + ")");
                            }
                            try
                            {    // set Obj for driver app
                                DriverMsgObj.IsSuccess = true;
                                DriverMsgObj.Message = "Transaction successful";
                                DriverMsgObj.Data = obj.paymentIntentId.ToStr();

                                long jobId = obj.bookingId.ToLong();
                                using (TaxiDataContext db2 = new TaxiDataContext())
                                {
                                    General.UpdateJobToDriverPDA(db2.Bookings.FirstOrDefault(c => c.Id == jobId), "Credit Card(PAID)");
                                }

                                if (objBooking != null && !string.IsNullOrEmpty(obj.customerid))
                                {
                                    Customer objcustomer = db.Customers.Where(c => c.MobileNo == objBooking.CustomerMobileNo).FirstOrDefault();
                                    objBooking.CustomerId = objcustomer.Id;

                                    db.ExecuteQuery<int>("update Customer set CreditCardDetails='" + obj.customerid.ToStr() + "' where Id=" + objBooking.CustomerId);
                                    if ((db.ExecuteQuery<int>("select count(*) from Customer_CCDetails where CustomerId=" + objBooking.CustomerId.ToLong()).FirstOrDefault()) > 0)
                                    {
                                        db.ExecuteQuery<int>("update Customer_CCDetails set IsDefault=0 where CustomerId=" + objBooking.CustomerId);
                                    }

                                    string CardDetails = $"KonnectPayToken: {obj.paymentMethodId} | {obj.message}";
                                    db.ExecuteQuery<int>("insert into Customer_CCDetails(CustomerId,CCDetails,AddOn,AddBy,IsDefault)VALUES(" + objBooking.CustomerId + ",'" + CardDetails + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','Eurosoft',1)");

                                }
                            }
                            catch
                            {
                            }
                            try
                            {

                                //long returnBookingId = db.ExecuteQuery<long>("select Id from booking where masterjobid=" + obj.bookingId).FirstOrDefault();
                                //if (returnBookingId > 0)
                                //{
                                //    string returnmsg = "PAID " + DefaultCurrencySign + string.Format("{0:f2}", obj.amount) + " | TRANSACTION - " + obj.paymentIntentId.ToStr() + " | " + string.Format("{0:dd/MM/yyyy HH:mm}", DateTime.Now);

                                //    db.ExecuteQuery<int>("update booking set Paymenttypeid=6,IsQuotedPrice=1,PaymentComments='" + returnmsg + "' where id=" + returnBookingId);
                                //    db.ExecuteQuery<int>("insert into Booking_Payment(bookingid,PaymentGatewayId,AuthCode,Status)VALUES(" + returnBookingId + ",15,'" + obj.paymentIntentId.ToStr() + "','Paid')");
                                //    using (TaxiDataContext db2 = new TaxiDataContext())
                                //    {
                                //        General.UpdateJobToDriverPDA(db2.Bookings.FirstOrDefault(c => c.Id == returnBookingId), "Credit Card(PAID)");
                                //    }

                                //    try
                                //    {
                                //        File.AppendAllText(AppContext.BaseDirectory + "\\UpdateDataFromPayByLinkDriverAppKOnnectPay.txt", DateTime.Now.ToStr() + " updating ReturnBooking ID : " + returnBookingId + Environment.NewLine);
                                //    }
                                //    catch
                                //    {
                                //    }
                                //}


                            }
                            catch (Exception ex)
                            {
                                try
                                {
                                    File.AppendAllText(AppContext.BaseDirectory + "\\UpdateDataFromPayByLinkDriverAppKOnnectPay_exception1.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                                }
                                catch
                                {
                                }
                            }
                        }
                        else
                        {
                            db.ExecuteQuery<int>("update booking set PaymentComments='PAYMENT FAILED : " + obj.status.ToStr() + "'  where id=" + obj.bookingId.ToLong());
                            db.stp_BookingLog(obj.bookingId.ToLong(), "Customer", "PAYMENT FAILED - " + obj.status.ToStr());

                            // set Obj for driver app
                            DriverMsgObj.IsSuccess = false;
                            DriverMsgObj.Message = "PAYMENT FAILED";
                            DriverMsgObj.Data = obj.status.ToStr();
                        }
                    }
                    RefreshRequiredDashboard(obj.bookingId.ToStr());
                    if (IsPayByDriverApp && DriverId > 0)
                    {
                        string jsonMsg = new JavaScriptSerializer().Serialize(DriverMsgObj);
                        SocketIO.SendToSocket(DriverId.ToStr(), jsonMsg.ToStr(), "paymentresponse");

                    }

                }
            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(AppContext.BaseDirectory + "\\UpdateDataFromPayByLinkDriverAppKOnnectPay_exception.txt", DateTime.Now.ToStr() + " json" + json + ",exception:" + ex.Message + Environment.NewLine);
                }
                catch
                {
                }
                rtn = "false";
            }
            return rtn;
        }
        #endregion


        private static string RemoveUK(ref string address)
        {
            if (address.ToUpper().EndsWith(", UK"))
            {
                address = address.Remove(address.ToUpper().LastIndexOf(", UK"));
            }

            return address;
        }

        private static string GetPostCodeMatch(string value)
        {
            string postCode = "";

            if (value.ToStr().Contains(","))
            {
                value = value.Replace(",", "").Trim();
            }

            if (value.ToStr().Contains(" "))
            {
                value = value.Replace(" ", " ").Trim();
            }

            RemoveUK(ref value);

            //string ukAddress = @"[[:alnum:]][a-zA-Z0-9_\.\#\' \-]{2,60}$";
            string ukAddress = @"^(GIR 0AA)|((([A-PR-UWYZ][0-9][0-9]?)|(([A-PR-UWYZ][A-HK-Y][0-9][0-9]?)|(([A-PR-UWYZ][0-9][A-HJKSTUW])|([A-PR-UWYZ][A-HK-Y][0-9][ABEHMNPRVWXY])))) [0-9][A-BD-HJLNP-UW-Z]{2})$";
            //string ukAddress = @"^(GIR 0AA|[A-PR-UWYZ]([0-9]{1,2}| ([A-HK-Y][0-9]|[A-HK-Y][0-9]([0-9]| [ABEHMNPRV-Y]))|[0-9][A-HJKPS-UW]) [0-9][ABD-HJLNP-UW-Z]{2})$";

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

            return postCode.Trim();
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

                double dist = new LatLng(Convert.ToDouble(lat), Convert.ToDouble(lng)).DistanceMiles(new LatLng(pointLat, pointLng));

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

    }
}