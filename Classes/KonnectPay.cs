using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRHub.Classes.KonnectPay
{
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