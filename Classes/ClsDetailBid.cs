using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRHub
{
    public class ClsPlotBidDetail
    {
        public ClsPlotBidDetail()
        {

        }


        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
      
      
    
        public DateTime? PickupDateTime { get; set; }
  
        public long JobId { get; set; }
    
     
        public double longitude { get; set; }
       
        public double? JobLongitude { get; set; }
     
        public double? JobLatitude { get; set; }
     
        public int? biddingradius { get; set; }
      
        public string ZoneName { get; set; }
    
        public int? ZoneId { get; set; }
   
        public int? DriverId { get; set; }
   
        public double latitude { get; set; }
     
        public string VehicleType { get; set; }
    }
}
