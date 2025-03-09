using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRHub
{

    public class ClsDriverPayDetails
    {
        public string url;
        public string driverId;
        public string driverNo;
        public float balanceAmount;
        public string message;

    }


    public class CardStreamSettings
    {

        public string merchantId { get; set; }

        public string SharedKey { get; set; }
        public string action { get; set; }
        public int transType { get; set; }
        public string uniqueIdentifier { get; set; }
        public int currencyCode { get; set; }
        public decimal amount { get; set; }
        public String orderRef { get; set; }
        public string cardNumber { get; set; }
        public string cardExpiryMM { get; set; }
        public string cardExpiryYY { get; set; }
        public string cardCVV { get; set; }
        public string customerName { get; set; }
        public string customerEmail { get; set; }
        public string customerPhone { get; set; }
        public string customerAddress { get; set; }
        public int countryCode { get; set; }
        public string customerPostcode { get; set; }
        public string threeDSMD { get; set; }
        public string threeDSPaRes { get; set; }
        public string threeDSPaReq { get; set; }
        public string threeDSACSURL { get; set; }
        public string FetchString { get; set; }
        public string TableType { get; set; }


        public int driverId { get; set; }
        public long StatementId { get; set; }
    }
}