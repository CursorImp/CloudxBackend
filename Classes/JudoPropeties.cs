using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace TCS_TcpListener.Judo
{
    //Request Classes
    public class JudoProperties
    {
        public JudoProperties()
        {

            yourPaymentMetaData = new YourPaymentMetaData();
            cardAddress = new CardAddress();
            consumerLocation = new ConsumerLocation();
        }
        public string yourConsumerReference { get; set; }
        public string yourPaymentReference { get; set; }
        public YourPaymentMetaData yourPaymentMetaData { get; set; }
        public string judoId { get; set; }
        public string Acc_Token { get; set; }
        public string Acc_SecretKey { get; set; }
        public double amount { get; set; }
        public string cardNumber { get; set; }
        public string expiryDate { get; set; }
        public string cv2 { get; set; }
        public string currency { get; set; }
        public CardAddress cardAddress { get; set; }
        public ConsumerLocation consumerLocation { get; set; }
        public string mobileNumber { get; set; }
        public string emailAddress { get; set; }
        public bool TestMode { get; set; }

        public string CardToken { get; set; }
        public string ConsumerToken { get; set; }

    }

    public class YourPaymentMetaData
    {
    }

    public class CardAddress
    {
        public string line1 { get; set; }
        public string line2 { get; set; }
        public string line3 { get; set; }
        public string town { get; set; }
        public string postCode { get; set; }
    }

    public class ConsumerLocation
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
    }








    //// Response Classes



    public class JudoCardDetails
    {
        public string cardLastfour { get; set; }
        public string endDate { get; set; }
        public string cardToken { get; set; }
        public int cardType { get; set; }
    }

    public class Consumer
    {
        public string consumerToken { get; set; }
        public string yourConsumerReference { get; set; }
    }

    public class Risks
    {
        public string postCodeCheck { get; set; }
    }

    public class ResponseJudo
    {
        public ResponseJudo()
        {

            cardDetails = new JudoCardDetails();
            consumer = new Consumer();
            risks = new Risks();
        }
        public string receiptId { get; set; }
        public string yourPaymentReference { get; set; }
        public string type { get; set; }
        public string createdAt { get; set; }
        public string result { get; set; }
        public string message { get; set; }
        public int judoId { get; set; }
        public string merchantName { get; set; }
        public string appearsOnStatementAs { get; set; }
        public string originalAmount { get; set; }
        public string netAmount { get; set; }
        public string amount { get; set; }
        public string currency { get; set; }
        public JudoCardDetails cardDetails { get; set; }
        public Consumer consumer { get; set; }
        public Risks risks { get; set; }
    }


    /// <summary>
    /// A payment receipt
    /// </summary>
    /// <remarks>This receipt model contains all the information about the transaction processed, including the outcome (see <see cref="Result"/>)</remarks>
    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global

    public class PaymentReceipt : ITransactionResult
    {



        public WalletType? WalletType { get; set; }

        /// <summary>
        /// Gets or sets the transaction identifier.
        /// </summary>
        /// <value>
        /// The transaction identifier.
        /// </value>

        public long ReceiptId { get; set; }

        /// <summary>
        /// The receipt id of the original payment, if this is a refund or collection
        /// </summary>
        /// <value>
        /// The original transaction identifier.
        /// </value>

        // ReSharper disable once UnusedMember.Global
        public long? OriginalReceiptId { get; set; }

        /// <summary>
        /// Payment, Refund, PreAuth, or Collection
        /// </summary>
        /// <value>
        /// The type.
        /// </value>

        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the created at.
        /// </summary>
        /// <value>
        /// The created at.
        /// </value>

        // ReSharper disable once UnusedMember.Global
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Gets the result of this transaction.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>

        public string Result { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>

        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the judo identifier.
        /// </summary>
        /// <value>
        /// The judo identifier.
        /// </value>

        public long JudoId { get; set; }

        /// <summary>
        /// Gets or sets the name of the merchant.
        /// </summary>
        /// <value>
        /// The name of the merchant.
        /// </value>

        // ReSharper disable once UnusedMember.Global
        public string MerchantName { get; set; }

        /// <summary>
        /// Gets or sets the appears on statement as.
        /// </summary>
        /// <value>
        /// The appears on statement as.
        /// </value>

        public string AppearsOnStatementAs { get; set; }

        /// <summary>
        /// Gets or sets the original amount
        /// </summary>
        /// <value>
        /// The original amount.
        /// </value>

        public decimal? OriginalAmount { get; set; }

        /// <summary>
        /// Refunds and PreAuths will not have this value
        /// </summary>
        /// <value>
        /// The refunds.
        /// </value>

        public decimal Refunds { get; set; }

        /// <summary>
        /// Gets or sets your payment reference.
        /// </summary>
        /// <value>
        /// Your payment reference.
        /// </value>

        public string YourPaymentReference { get; set; }

        /// <summary>
        /// Gets or sets the net amount.
        /// </summary>
        /// <value>
        /// The net amount.
        /// </value>

        public decimal NetAmount { get; set; }

        /// <summary>
        /// If this transaction is a PreAuth then this is the amount of that has already been collected.
        /// </summary>

        public decimal? AmountCollected { get; set; }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>

        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the currency.
        /// </summary>
        /// <value>
        /// The currency.
        /// </value>

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Currency { get; set; }

        /// <summary>
        /// Gets or sets the partner service fee.
        /// </summary>
        /// <value>
        /// The partner service fee.
        /// </value>

        // ReSharper disable once UnusedMember.Global
        public decimal PartnerServiceFee { get; set; }

        /// <summary>
        /// Gets or sets the card details.
        /// </summary>
        /// <value>
        /// The card details.
        /// </value>

        public CardDetails CardDetails { get; set; }

        /// <summary>
        /// Gets or sets the consumer.
        /// </summary>
        /// <value>
        /// The consumer.
        /// </value>

        public Consumer Consumer { get; set; }

        /// <summary>
        /// Gets or sets the Device information.
        /// </summary>

        public Device Device { get; set; }

        /// <summary>
        /// Gets or sets the risk score.
        /// </summary>
        /// <value>
        /// The risk score.
        /// </value>

        // ReSharper disable once UnusedMember.Global
        public int? RiskScore { get; set; }

        /// <summary>
        /// Gets or sets your payment meta data.
        /// </summary>
        /// <value>
        /// Your payment meta data.
        /// </value>

        // ReSharper disable once UnusedMember.Global
        public IDictionary<string, string> YourPaymentMetaData { get; set; }

        /// <summary>
        /// If the payment requested 3d secure, we need to include the result of that authentication process
        /// </summary>
        /// <value>
        /// The three d secure.
        /// </value>

        public ThreeDSecureReceipt ThreeDSecure { get; set; }

        /// <summary>
        /// Transaction processed with recurring authorization 
        /// </summary>

        public bool? Recurring { get; set; }

        /// <summary>
        /// Information about the risks associated with the transaction
        /// </summary>

        public Risks Risks { get; set; }
    }

    /// <summary>
    /// A result of a transaction request
    /// </summary>
    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedMemberInSuper.Global
    public interface ITransactionResult
    {
        /// <summary>
        /// Gets or sets the transaction identifier.
        /// </summary>
        /// <value>
        /// The transaction identifier.
        /// </value>

        long ReceiptId { get; set; }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>

        string Result { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>

        string Message { get; set; }
    }

    /// <summary>
    ///     Details of the card used in the requested operation (add card/payment)
    /// </summary>
    // ReSharper disable UnusedAutoPropertyAccessor.Global

    public class CardDetails
    {
        /// <summary>
        /// Gets or sets the card last four digits.
        /// </summary>
        /// <value>
        /// The card lastfour.
        /// </value>

        public string CardLastfour { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>

        public string EndDate { get; set; }

        /// <summary>
        /// Gets or sets the card token.
        /// </summary>
        /// <value>
        /// The card token.
        /// </value>

        public string CardToken { get; set; }

        /// <summary>
        /// Gets or sets the type of the card.
        /// </summary>
        /// <value>
        /// The type of the card.
        /// </value>

        public CardType CardType { get; set; }

        /// <summary>
        /// Gets or sets the scheme of the card.
        /// </summary>
        /// <value>
        /// Possible values are Visa, Mastercard etc
        /// </value>

        public string CardScheme { get; set; }

        /// <summary>
        /// Gets or sets the funding type of the card.
        /// </summary>
        /// <value>
        /// Possible values are Debit, Credit etc
        /// </value>

        public string CardFunding { get; set; }

        /// <summary>
        /// Gets the category of the card.
        /// </summary>
        /// <value>
        /// Possible values are Corporate, Classic, Platinum
        /// </value>

        public string CardCategory { get; set; }

        /// <summary>
        /// Gets the country the card was issued from in ISO 3166-1 alpha-2 format (2 chararacters)
        /// </summary>
        /// <value>
        /// Possible values are UK, FR, DE
        /// </value>

        public string CardCountry { get; set; }


        /// <summary>
        /// Gets the bank the card was issued from.
        /// </summary>
        /// <value>
        /// Possible values are Credit Industriel Et Commercial, Barclays, Bank of America
        /// </value>

        public string Bank { get; set; }
    }

    /// <summary>
    /// Details of your consumer as used in the requested operation (add card/payment)
    /// </summary>
    // ReSharper disable UnusedAutoPropertyAccessor.Global



    public enum WalletType
    {
        /// <summary>
        /// No digital wallet was used
        /// </summary>
        None = 0,

        /// <summary>
        /// This transaction was processed using Apple Pay
        /// </summary>
        ApplePay = 1
    }

    /// <summary>
    /// Card types
    /// </summary>
    /// <remarks>This enum is fed from two data sources</remarks>
    // ReSharper disable UnusedMember.Global
    // ReSharper disable InconsistentNaming
    public enum CardType
    {

        UNKNOWN = 0,


        VISA = 1,


        MASTERCARD = 2,


        VISA_ELECTRON = 3,


        SWITCH = 4,


        SOLO = 5,


        LASER = 6,

        CHINA_UNION_PAY = 7,


        AMEX = 8,


        JCB = 9,


        MAESTRO = 10,


        VISA_DEBIT = 11,

        /// <summary>
        /// In Europe your more likely to see Maestro than Mastercard Debit, however it does exist.
        /// </summary>

        MASTERCARD_DEBIT = 12,


        VISA_PURCHASING = 13,

        // 8192
        DISCOVER = 14,

        // 16384
        CARNET = 15,

        // 32768
        CARTE_BANCAIRE = 16,

        // 65536
        DINERS_CLUB = 17,

        // 131072
        ELO = 18,

        // 262144
        FARMERS_CARD = 19,

        // 524288
        SORIANA = 20,

        // 1048576
        PRIVATE_LABEL_CARD = 21,

        Q_CARD = 22,

        STYLE = 23,

        TRUE_REWARDS = 24,

        UATP = 25,

        BANKCARD = 26,

        BANAMEX_COSTCO = 27
    }





    // ReSharper restore UnusedAutoPropertyAccessor.Global
    /// <summary>
    /// Details of the device being used in the requested operation (add card/payment)
    /// </summary>
    // ReSharper disable UnusedAutoPropertyAccessor.Global

    public class Device
    {
        /// <summary>
        /// Gets or sets the device identity.
        /// </summary>
        /// <value>
        /// The devices identity.
        /// </value>

        public string Identifier { get; set; }

        // ReSharper restore UnusedAutoPropertyAccessor.Global
    }

    /// <summary>
    /// 3D secure receipt
    /// </summary>
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable UnusedMember.Global

    // ReSharper disable ClassNeverInstantiated.Global
    public class ThreeDSecureReceipt
    // ReSharper restore ClassNeverInstantiated.Global
    {
        /// <summary>
        /// Did the consumer attempt to authenticate through 3d secure
        /// </summary>

        public bool Attempted { get; set; }

        /// <summary>
        /// what was the outcome of their authentication
        /// </summary>

        public string Result { get; set; }
    }
    // ReSharper restore UnusedMember.Global
    // ReSharper restore UnusedAutoPropertyAccessor.Global











}
