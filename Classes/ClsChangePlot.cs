using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Web;

namespace SignalRHub
{
    //  {"BookingFee":0.0,"Destination":"TESSALL FARM CROFT BIRMINGHAM B31 2UW\u003c\u003c\u003cLONG BRIDGE","DriverNo":"test2","ExtraDropCharges":0.0,"JobStatus":4,"ParkingCharges":"0.00","Pickup":"TESSALL LANE BIRMINGHAM B31 2SE\u003c\u003c\u003cLONG BRIDGE","PickupDateTime":"15/10/2020   17:41","ShowBookingFees":"0","ShowExtraCharges":"0","ShowParkingCharges":"0"}

    public class ClsChangePlot
    {

        public string JobId;
        public string DrvId;




        public string ParkingCharges;
        public string ExtraDropCharges;
        public decimal? BookingFee;

        public string Passengers;
        public string Dropoff;
        public string Pickup;


        public string DrvNo;
        public double? Latitude;
        public double? Longitude;

        public int JobStatus;

        public string ShowParkingCharges;
        public string ShowExtraCharges;

        public int IsUpdateParkingCharge;
        public int IsUpdateExtraCharges;

        public string PickupDateTime;
        public int ZoneId;
        public string ZoneName;
    }



    public class ClsGen_SysPolicy_SurchargeRates
    {


        public long Id;



        public System.Nullable<decimal> Percentage;


        public System.Nullable<bool> IsAmountWise;

        public System.Nullable<decimal> Amount;

        public System.Nullable<int> zoneid;

        public System.Nullable<bool> ZoneWiseSurcharge;

        public System.Nullable<bool> EnableSurcharge;

        public System.Nullable<int> ApplicableFromDay;

        public System.Nullable<int> ApplicableToDay;

        public System.Nullable<System.DateTime> ApplicableFromDateTime;

        public System.Nullable<System.DateTime> ApplicableToDateTime;

        public System.Nullable<int> CriteriaBy;

        public string Holidays;

        public decimal? Parking;

        public decimal? Waiting;



    }


    public class stp_getbiddingSTCjobsResult
    {
        public stp_getbiddingSTCjobsResult()
        {
        }

       
        public int? driverid { get; set; }
      
        public int? zoneid { get; set; }
      
        public string zonename { get; set; }
     
        public int? biddingradius { get; set; }
  
        public double? JobLatitude { get; set; }
  
        public double? JobLongitude { get; set; }
  
        public double latitude { get; set; }
  
        public double longitude { get; set; }


        public double? DestLat { get; set; }

        public double? DestLon { get; set; }
    }


    public class stp_getbiddingAvailablejobsResult
    {
        public stp_getbiddingAvailablejobsResult()
        {
        }


        public int? driverid { get; set; }

        public int? zoneid { get; set; }

        public string zonename { get; set; }

        public int? biddingradius { get; set; }

        public double? JobLatitude { get; set; }

        public double? JobLongitude { get; set; }

        public double latitude { get; set; }

        public double longitude { get; set; }


      
    }

    public class ClsBidAction
    {
        public string DrvNo;
        public string Status;
        public string version;
        public long AllocatedJobId;
        public string Message;
        public double? Latitude;
        public double? Longitude;
        public string Dropoff;
        public long JobId;
    }


    public class ClsNotification
    {
        public long AllocatedJobId;
        public bool HasError;
        public string Message;
        
    }
}