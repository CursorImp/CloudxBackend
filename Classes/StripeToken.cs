using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SignalRHub.Classes.KonnectSupplier
{
    public class StripeLocation
    {
        public bool isSuccess { get; set; }
        public string LocationId { get; set; }
        public string error { get; set; }
    }
    public class DriverLocation
    {
        public int? DriverId { get; set; }
        public int? JobId { get; set; }
    }
    public class StripeTerminalDTO
    {
        public bool isSuccess { get; set; }
        public string error { get; set; }
        public List<StripeTerminalsKP> terminals { get; set; }
    }
    public class StripeTerminalsKP
    {
        public long id { get; set; }
        public string displayName { get; set; }
        public string expressClient { get; set; }
        public string terminalLocationId { get; set; }
        public string driverExpressAccountId { get; set; }
        public bool isActive { get; set; }
    }
    public class PaymentCaptureDto
    {
        public long bookingId { get; set; }
        public string bookingRef { get; set; }
        public string description { get; set; }
        public int countryId { get; set; }
        public string connectedAccountId { get; set; }
        public decimal applicationFee { get; set; }
        public decimal otherCharges { get; set; }
        public string key { get; set; }
        public string secret { get; set; }
        public long amount { get; set; }
        public decimal displayAmount { get; set; }
        [MaxLength(3)]
        public string currency { get; set; }
        public string defaultClientId { get; set; }
        public string location { get; set; }
        public string companyName { get; set; }
        public string paymentIntentId { get; set; }
        public string status { get; set; }

        public PaymentCaptureDto() { }
    }
    public class PaymentCaptureResponse
    {

        public string status { get; set; }

        public decimal amount { get; set; }
        public bool isSuccess { get; set; }
        public string paymentId { get; set; }
        public string error { get; set; }

    }
    public class PaymentWithExistingCustomerResponse
    {

        public string status { get; set; }

        public decimal amount { get; set; }
        public bool isSuccess { get; set; }
        public string paymentIntentId { get; set; }
        public string error { get; set; }
        public bool isAuthorized { get; set; }
        public long? bookingId { get; set; }

    }
    public class PaymentRefundResponse
    {

        public string status { get; set; }

        public decimal amount { get; set; }
        public bool isSuccess { get; set; }
        public string paymentId { get; set; }
        public dynamic error { get; set; }

    }

    public enum GatewayTypesConnect { CardTerminal = 1, TapToPay = 2, PayByLink = 3, QRCode = 5 }
    public class StripePaymentRequestDto
    {
        [Required(ErrorMessage = "Payment Type is required")]
        public bool isAuthorized { get; set; }
        public string key { get; set; }
        public string secret { get; set; }
        public int countryId { get; set; }
        public string connectedAccountId { get; set; }
        public decimal applicationFee { get; set; }
        public decimal otherCharges { get; set; }
        [Required(ErrorMessage = "Booking Id is required")]
        public long bookingId { get; set; }
        [MaxLength(20)]
        [Required(ErrorMessage = "Booking Ref is required")]
        public string bookingRef { get; set; }
        [Required(ErrorMessage = "Amount is required")]
        public long amount { get; set; }
        public decimal displayAmount { get; set; }
        [MaxLength(3)]

        [Required(ErrorMessage = "Currency is required")]
        public string currency { get; set; }
        [MaxLength(50)]
        [Required(ErrorMessage = "Description is required")]
        public string description { get; set; }
        public string paymentMethodId { get; set; }
        public string customerId { get; set; }
        public string customerName { get; set; }
        public string email { get; set; }
        public string phoneNumber { get; set; }
        public string lastfour { get; set; }
        public string expiry { get; set; }
        public string cardtype { get; set; }
        public string companyName { get; set; }
        public string defaultClientId { get; set; }
        public string location { get; set; }
        public string verificationWebhook { get; set; }
        public string paymentUpdateWebhook { get; set; }

        public string UpdatePaymentURL { get; set; }
        public string PreAuthUrl { get; set; }
        public string OperatorName { get; set; }
        public decimal? ReturnAmount { get; set; }
        public string PayByDispatch { get; set; }
        public StripePaymentRequestDto() { }
    }
    public class PaymentExistDto
    {
        public bool isSuccess { get; set; }
        public string paymentIntentId { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public PaymentExistDto() { }
    }
    public class PaymentUpdateDto
    {
        public string requestdata { get; set; }
        public string customerid { get; set; }
        public string paymentMethodId { get; set; }
        public decimal? amount { get; set; }
        public long? bookingId { get; set; }
        public bool isSuccess { get; set; }
        public string paymentIntentId { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public bool isAuthorized { get; set; }
        public PaymentUpdateDto() { }
    }

    public class RegisterCardDto
    {
        public int countryId { get; set; }
        public string connectedAccountId { get; set; }
        public string key { get; set; }
        public string secret { get; set; }
        public string description { get; set; }
        public string customerId { get; set; }
        public string customerName { get; set; }
        public string email { get; set; }
        public string phoneNumber { get; set; }
        public string companyName { get; set; }
        public string defaultClientId { get; set; }
        public string location { get; set; }
        public string PreAuthUrl { get; set; }
        public string UpdatePaymentURL { get; set; }
        public string paymentUpdateWebhook { get; set; }
        public bool sendLinkToCustomer { get; set; }
        public RegisterCardDto() { }
    }
    public class RegisterCardUpdateDto
    {
        public string phoneNumber { get; set; }
        public string customerName { get; set; }
        public string customerid { get; set; }
        public string paymentMethodId { get; set; }
        public string status { get; set; }
        public bool isSuccess { get; set; }
        public string message { get; set; }

        public RegisterCardUpdateDto() { }
    }

    public class CanclePaymentDTO
    {

        public long bookingId { get; set; }
        public string bookingRef { get; set; }
        public string description { get; set; }
        public int countryId { get; set; }
        public string connectedAccountId { get; set; }
        public decimal applicationFee { get; set; }
        public decimal otherCharges { get; set; }
        public string key { get; set; }
        public string secret { get; set; }
        public long amount { get; set; }
        public decimal displayAmount { get; set; }
        [MaxLength(3)]
        public string currency { get; set; }
        public string defaultClientId { get; set; }
        public string location { get; set; }
        public string companyName { get; set; }
        public string paymentIntentId { get; set; }
        public string status { get; set; }
    }

    public class RefundPaymentDto
    {
        public long bookingId { get; set; }
        public string bookingRef { get; set; }
        public string description { get; set; }
        public string key { get; set; }
        public string secret { get; set; }
        public long amount { get; set; }
        public decimal displayAmount { get; set; }
        [MaxLength(3)]
        public string currency { get; set; }
        public string defaultClientId { get; set; }
        public string location { get; set; }
        public string companyName { get; set; }
        public string paymentIntentId { get; set; }
        public string status { get; set; }
        public bool isRefundApplicationFee { get; set; }
        public int countryId { get; set; }
        public string connectedAccountId { get; set; }
        public RefundPaymentDto() { }
    }

    public class TerminalPaymentIntentRequestDto
    {
        public string key { get; set; }
        public string secret { get; set; }
        public int countryId { get; set; }
        public string connectedAccountId { get; set; }
        public decimal applicationFee { get; set; }
        public decimal otherCharges { get; set; }
        public long bookingId { get; set; }
        public string bookingRef { get; set; }
        public long amount { get; set; }
        public decimal displayAmount { get; set; }
        [MaxLength(3)]
        public string currency { get; set; }
        public string description { get; set; }
        public string companyName { get; set; }
        public string defaultClientId { get; set; }
        public string location { get; set; }
        public TerminalPaymentIntentRequestDto() { }
    }

    public class StripeKonnectPaymentRequestModel
    {
        public int driverId { get; set; }
        public int jobId { get; set; }
        public String selectedGateway { get; set; }
        public decimal Fare { get; set; }
        public string sendType { get; set; }
        public string mobileno { get; set; }
        public string email { get; set; }
        public string mobile { get; set; }
        public string serialnumber { get; set; }
    }
    public class CheckPaymentStatus
    {
        public int jobId { get; set; }
    }

    public class CheckPaymentStatusResponse
    {
        public int jobId { get; set; }
        public int paymentStatus { get; set; }
        public string TransactionId { get; set; }
        public string TransactionMessage { get; set; }

    }

    public class TransferPaymentDto
    {
        public string key { get; set; }
        public string secret { get; set; }
        public string masterAccountId { get; set; }
        public long amount { get; set; }
        public decimal displayAmount { get; set; }
        [MaxLength(3)]
        public string currency { get; set; }
        public string description { get; set; }
        public short countryId { get; set; }
        public string connectedAccountId { get; set; }
        public string distinationconnectedAccountId { get; set; }
        public string companyName { get; set; }
        public string defaultClientId { get; set; }
        public string location { get; set; }

    }

}