using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static SignalRHub.DriverAppController;
using Taxi_Model;
using Utils;
using Taxi_BLL;

namespace SignalRHub.Classes.KonnectPay
{

    public class PaymentConfig
    {
        public bool? EnableMobileIntegration { get; set; }

        public string PaypalID { get; set; }

        public string IPNListenerUrl { get; set; }

        public string PrivateKeyPassword { get; set; }

        public string ApiCertificate { get; set; }

        public string ApplicationId { get; set; }

        public string ApiSignature { get; set; }

        public string ApiUsername { get; set; }

        public int? PaymentGatewayId { get; set; }

        public string MerchantPassword { get; set; }

        public string MerchantID { get; set; }

        public int SysPolicyId { get; set; }

        public int Id { get; set; }
        public string ApiPassword { get; set; }

        public int SubCompanyId { get; set; }

     


    }
    public class KonnectPayOptions
    {
        public int OptionID { get; set; }
        public string OptionName { get; set; }
        public string OptionType { get; set; }
    }

    public class SendPaymentLinkKP 
    {
        public long bookingId { get; set; }
        public string BookingNo { get; set; }
        public decimal numTotalCharges { get; set; }
        public string HubUrl { get; set; }
        public string subCompanyName { get; set; }
        public string PayByDispatch { get; set; }

        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string UserName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhoneNo { get; set; }
        public string CustomerMobileNo { get; set; }
        public decimal? ReturnFareRate { get; set; }
        public bool IsAuthorize { get; set; }
        public bool IsIncremental { get; set; }
        public bool sendLinkToCustomer { get; set; }
        public string CardToken { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal ExtraHold { get; set; }

    }

    public class CCDetailsKP 
    {

        public long Id { get; set; }
        public string customerName { get; set; }
        public string cardNumber { get; set; }
        public string cardExpiry { get; set; }

        public bool? IsDefault { get; set; }


        public int RecordId { get; set; }


        public string CCDetails { get; set; }

        public string CardToken { get; set; }
        public string CardType { get; set; }
    }
   
}