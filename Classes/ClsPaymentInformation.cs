using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalRHub
{
    public class ClsPaymentInformation
    {

        public string BookingId;
        public string BookingNo;

        public string DriverId;
        public string DriverNo;

        public string StartDate;
        public string ExpiryDate;
        public string ExpiryMonth;
        public string ExpiryYear;

        public string CV2;
        public string Name;
        public string Address;
        public string Email;
        public string PostCode;
        public string City;
        public string Charges;
        public decimal Price;
        public decimal SurchargeAmount;
        public string SurPercent;
        public string SurAmount;
        public decimal ServiceCharges;
        public decimal AgentFees;
        public decimal ExtraPickup;
        public decimal ExtraDropOff;
        public decimal ParkingCharges;

        public decimal? ExtraDropCharges;
        public decimal? BookingFee;

        public string Tip;
        public string Total;

        public string CardNumber;
        public string TransactionID;
        public string PaymentGatewayID;
        public string CardType;


        public string Category;

        public string NetFares;
        public string Parking;
        public string Waiting;

        public string AuthCode;
        public string TokenDetails;
        public string PaymentStatus;
        public string StripeCustomerId;
    }
}
