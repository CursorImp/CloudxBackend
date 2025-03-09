using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRHub
{
   
    public class FareEx
    {
        public FareEx()
        {

        }

      
    
        public DateTime? TillDateTime { get; set; }
  
        public DateTime? FromDateTime { get; set; }
   
        public DateTime? TillSpecialDate { get; set; }
 
        public DateTime? FromSpecialDate { get; set; }
    
        public string SpecialDayName { get; set; }

        public string TillDayName { get; set; }

        public string FromDayName { get; set; }
    
        public decimal? StartRate { get; set; }

        public decimal? StartRateValidMiles { get; set; }
    
        public DateTime? EditOn { get; set; }

        public bool? IsDayWise { get; set; }

        public decimal? PerMinJourneyCharges { get; set; }

        public int? SubCompanyId { get; set; }
        public int? CompanyId { get; set; }
    
        public bool? IsCompanyWise { get; set; }
    
        public bool? IsVehicleWise { get; set; }

     
        public string DayValue { get; set; }
      
        public int? VehicleTypeId { get; set; }
   
        public int Id { get; set; }

        public decimal? FromMile
        { get; set; }
        public decimal? ToMile
        { get; set; }
        public decimal? Rate     { get; set; }

        public decimal? WaitingCharges { get; set; }
        public int? WaitingChargesPerSeconds { get; set; }
        public int? WaitingSecondsFree { get; set; }


     

    }
}