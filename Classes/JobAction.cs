using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalRHub
{
    public  class JobAction
    {
        public string TransId;
        public string JobId;
        public string DrvId;
        public string JStatus;
        public string DStatus;
        public decimal? TotalFares;
        public string IsMeter;
        public decimal? Miles;
        public decimal? Fares;
        public string WaitingTime;
        public decimal? WaitingCharges;
        public String Signature;
        public decimal? ParkingCharges;
        public decimal? ExtraDropCharges;
        public decimal? BookingFee;

        public string Passengers;
        public string Dropoff;
        public string QuotedPrice;

        public string DrvNo;
        public double? Latitude;
        public double? Longitude;

        public int? PaymentGatewayID;

        public string ExtrasDetail;



        public string Account;
        public string version;
        public int ChangePlot;

        public string IsAuto = "";
        public int NavType = 0;
    }
    public class PauseMeter
    {

        public string JobId;
        public string DrvId;
        public string JStatus;
        public string DStatus;

        public string IsMeter;
        public decimal? Miles;
        public decimal? Fares;
        public string WaitingTime;
        public decimal? WaitingCharges;

        public string Dropoff;


        public string DrvNo;
        public double? Latitude;
        public double? Longitude;

        public string version;
        public bool isPause;
    }
    public class JobActionEx
    {
        public string TransId;
        public string JobId;
        public string DrvId;
        public string JStatus;
        public string DStatus;
        public decimal? TotalFares;
        public string IsMeter;
        public decimal? Miles;
        public decimal? Fares;
        public string WaitingTime;
        public decimal? WaitingCharges;
        public String Signature;
        public decimal? ParkingCharges;
        public decimal? ExtraDropCharges;
        public decimal? BookingFee;

        public string Passengers;
        public string Dropoff;


        public string DrvNo;
        public double? Latitude;
        public double? Longitude;



        public string ExtrasDetail;



        public string Account;
        public string version;



        public string IsQuoted = "0";
        public string Pickup = "";
        public string JourneyType = "";
        public decimal Charges;
        public string PickupDateTime;
        public string Message;

        public string IsAuto = "";

        public decimal Congestion;
        public MeterTarrif objMeterTariff;
        public long? TripId;
        public string Via;
    }

    public class ResponseDataX
    {
        public bool IsSuccess;
        public string Message;
        public object Data;
    }
}
