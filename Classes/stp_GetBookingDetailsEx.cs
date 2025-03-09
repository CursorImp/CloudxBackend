using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRHub
{
    public class stp_GetBookingDetailsExResult
    {

        public long Id { get; set; }
        public int? JourneyTypeId { get; set; }
       
        public string SpecialRequirements { get; set; }
      
        public string ViaString { get; set; }
      
        public string CustomerEmail { get; set; }
     
        public int? CompanyId { get; set; }
    
        public decimal? CustomerPrice { get; set; }
       
        public decimal? CompanyPrice { get; set; }
      
        public decimal? FareRate { get; set; }
     
        public bool? DisableDriverSMS { get; set; }
    
        public string CustomerPhoneNo { get; set; }
       
        public int? DriverId { get; set; }
      
        public int? VehicleTypeId { get; set; }
    
        public string CustomerName { get; set; }
     
        public int? BookingStatusId { get; set; }
      
        public string ToAddress { get; set; }
     
        public string FromAddress { get; set; }
    
        public DateTime? PickupDateTime { get; set; }
     
        public string BookingNo { get; set; }
      
        public string CustomerMobileNo { get; set; }
     
        public bool? DisablePassengerSMS { get; set; }

        public int? BookingTypeId { get; set; }
        public long? OnlineBookingId { get; set; }


        public bool? IsQuotedPrice { get; set; }


        public int? FromLocTypeId { get; set; }

        public decimal? JourneyMiles { get; set; }
        public DateTime? ArrivalDateTime { get; set; }

        public int? PaymentTypeId { get; set; }


        public decimal? maxDiscount { get; set; }
        public decimal? minFares { get; set; }

        public string type { get; set; }

        public decimal? promoValue { get; set; }

        public bool? PayDriver { get; set; }




        public bool? enableSurge;
        public string surgeText;
    }
}