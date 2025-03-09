using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalRHub
{
    public class JudopayPayment
    {
        public long BookingId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNumber { get; set; }
        public string CustomerEmail { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string UpdateUrl { get; set; }
        public string JudoId { get; set; }
        public string APIToken { get; set; }
        public string APISecret { get; set; }
        public string yourPaymentReference { get; set; }
        public string yourConsumerReference { get; set; }
        public bool IsRegisterCard { get; set; }
        public string Description { get; set; }
        public string CardLastfour { get; set; }


        public string CardToken { get; set; }
        public string BookingNo { get; set; }
        public bool ResponseInJson { get; set; }

        public string VerifyPaymentUrl { get; set; }
        public string UserName { get; set; }




        public long ReturnBookingId { get; set; }
        public string ReturnBookingNo { get; set; }
        public decimal OneWayAmount { get; set; }
        public decimal ReturnAmount { get; set; }
        public string SubCompanyName { get; set; }


        public decimal DisplayAmount { get; set; }
        public DateTime? PickupDateTime { get; set; }


        public DateTime? ReturnPickupDateTime { get; set; }
        public int SendType { get; set; }
    }
}
