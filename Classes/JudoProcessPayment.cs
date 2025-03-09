using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRHub
{
    public class UpdatePaymentRequest
    {
        public string TransactionType { get; set; }
        public long? ReceiptId { get; set; }
        public string CardToken { get; set; }
        public string Status { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNumber { get; set; }
        public string yourPaymentReference { get; set; }
        public string yourConsumerReference { get; set; }
        public bool IsSuccess { get; set; }
        public double? Amount { get; set; }
        public string expiryDate { get; set; }
        public string Message { get; set; }
        public bool Is3DSCardRegister { get; set; }
        public string BookingId { get; set; }
        public string CardLastfour { get; set; }
        public string ConsumerToken { get; set; }
        public string status { get; set; }
        public string CreatedAt { get; set; }
        public string TransactionDetails { get; set; }


        public string IsThreeDSecureAttempted { get; set; }
    }


    public class JudoProcessPaymentRequest
    {
        public long receiptId { get; set; }
        public string cardToken { get; set; }
        public string Status { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNumber { get; set; }
        public string yourPaymentReference { get; set; }
        public string yourConsumerReference { get; set; }
        public bool IsSuccess { get; set; }
        public double amount { get; set; }
        public string expiryDate { get; set; }
        public string Message { get; set; }
        public bool Is3DSCardRegister { get; set; }
        public string currency { get; set; }
        public long judoId;
        public string APIToken;
        public string APISecret;

        public string IsThreeDSecureAttempted { get; set; }

        //     "cardToken": "QDPR8X0wmoBSTAYujBKmHNASUjsLyjoO",
        //"expiryDate": "0323",
        //"amount": 1.0,
        //"currency": "GBP",
        //"yourPaymentReference": "9de2b9e4-b730-4b82-a466-c3956a7dsd240c8232",
        //"yourConsumerReference": "f2ddc8dd-731f-416f-a8c3-ae932f2e68bc",
        //"judoId": 100031426,
        //"APIToken": "6dBoLBD5dFkHn37h",
        //"APISecret": "06899e76155f6853796095bfbbd003cec736709229b6dca79e6f08b28fd1a2df",
        //"receiptId": 810881470230130688
        public long BookingId;
    }


    public class TransactionCompletedResponse
    {
        public bool IsSuccess { get; set; }
        public string Status { get; set; }
        public Error Errors { get; set; }
        public string receiptId { get; set; }
        public string amount { get; set; }
        public string yourPaymentReference { get; set; }
        public TransactionResponse transactionResponse { get; set; }


        public string message { get; set; }
    }

    public class TransactionResponse
    {
        public string receiptId { get; set; }
        public string originalReceiptId { get; set; }
        public string yourPaymentReference { get; set; }
        public string type { get; set; }
        public DateTime createdAt { get; set; }
        public string result { get; set; }
        public string message { get; set; }
        public long judoId { get; set; }
        public string merchantName { get; set; }
        public string appearsOnStatementAs { get; set; }
        public string originalAmount { get; set; }
        public string amountCollected { get; set; }
        public string netAmount { get; set; }
        public string amount { get; set; }
        public string currency { get; set; }
        public string acquirerTransactionId { get; set; }
        public string externalBankResponseCode { get; set; }
        public string authCode { get; set; }
        public string postCodeCheckResult { get; set; }
        public int walletType { get; set; }
        public string acquirer { get; set; }
        public string webPaymentReference { get; set; }
        public int noOfAuthAttempts { get; set; }
        public CardDetails cardDetails { get; set; }
        public BillingAddress billingAddress { get; set; }
        public Consumer consumer { get; set; }
        public Device device { get; set; }
        public YourPaymentMetaData yourPaymentMetaData { get; set; }
        public ThreeDSecure threeDSecure { get; set; }
        public Risks risks { get; set; }

    }


    public class YourPaymentMetaData
    {
    
    }


    public class Detail
    {
        public int code { get; set; }
        public string fieldName { get; set; }
        public string message { get; set; }
    }
    public class Error
    {
        public List<Detail> details { get; set; }
        public string message { get; set; }

        public int code { get; set; }

        public int category { get; set; }


    }
    public class CreatedAt
    {
    }

    public class CardDetails
    {
        public string cardLastFour { get; set; }
        public string endDate { get; set; }
        public string cardToken { get; set; }
        public int cardType { get; set; }
        public string startDate { get; set; }
        public string cardScheme { get; set; }
        public string cardFunding { get; set; }
        public string cardCategory { get; set; }
        public int cardQualifier { get; set; }
        public string cardCountry { get; set; }
        public string bank { get; set; }
        public string cardHolderName { get; set; }
    }

    public class BillingAddress
    {
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string address3 { get; set; }
        public string town { get; set; }
        public string postCode { get; set; }
        public int countryCode { get; set; }
    }

    public class Consumer
    {
        public string consumerToken { get; set; }
        public string yourConsumerReference { get; set; }
    }

    public class Device
    {
        public string identifier { get; set; }
    }

    //public class YourPaymentMetaData
    //{
    //    public string internalLocationRef { get; set; }
    //    public int internalId { get; set; }
    //}

    public class ThreeDSecure
    {
        public bool attempted { get; set; }
        public string result { get; set; }
        public string eci { get; set; }
    }

    public class Risks
    {
        public string postCodeCheck { get; set; }
        public string cv2Check { get; set; }
        public string merchantSuggestion { get; set; }
    }
}