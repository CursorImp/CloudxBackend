using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SignalRHub
{
    public class Stripe3DS
    {


        public string CompanyName { get; set; }

        public string BookingRefNo { get; set; }
        public string PaymentStatusURL { get; set; }
        public string BookingId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string UpdatePaymentURL { get; set; }
        public string Description { get; set; }
        public string APIkey { get; set; }
        public string APISecret { get; set; }
        public string ImageUrl { get; set; }

        public string paymentIntentId { get; set; }
        public string status { get; set; }

        public bool IsSuccess { get; set; }
        public string Message { get; set; }

        public string MobileNo { get; set; }

        public string Email { get; set; }


        public string PreAuthUrl { get; set; }
        public string OperatorName { get; set; }


        public decimal IncreasedAmount { get; set; }


        public decimal ActualAmount { get; set; }
        public decimal OneWayAmount { get; set; }
        public decimal ReturnAmount { get; set; }
        public DateTime? PickupDateTime { get; set; }
        public DateTime? ReturnPickupDateTime { get; set; }


    }

    public class StripeHoldCard
    {


        public string BookingId { get; set; }
        public double ActualFares { get; set; }
        public int Amount { get; set; }
        public string Currency { get; set; }

        public string Description { get; set; }
        public string APIkey { get; set; }
        public string APISecret { get; set; }


        public string paymentIntentId { get; set; }
        public string status { get; set; }

        public bool IsSuccess { get; set; }
        public string Message { get; set; }

        public string MobileNo { get; set; }

        public string Email { get; set; }


        public string OperatorName { get; set; }
        public string PaymentMethodId { get; set; }
        public string CustomerId { get; set; }


        public string CardToken { get; set; }
        public string StripeCustomerId { get; set; }


        public string EncodeBASE64(string text)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(text)).TrimEnd('=').Replace('+', '-')
                .Replace('/', '_');
        }

        public static string Decode(string text)
        {
            text = text.Replace('_', '/').Replace('-', '+');
            switch (text.Length % 4)
            {
                case 2:
                    text += "==";
                    break;
                case 3:
                    text += "=";
                    break;
            }
            return Encoding.UTF8.GetString(Convert.FromBase64String(text));
        }
    }
}