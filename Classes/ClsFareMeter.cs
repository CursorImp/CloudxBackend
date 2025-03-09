using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalRHub
{
    public class ClsFareMeter
    {
        public string VehicleType;
        public int? VehicleId;
        public int? VehicleTypeId;
        public decimal? AutoStopWaitingOnSpeed;
        public decimal? AutoStartWaitingMinDist { get; set; }
        public decimal? DrvWaitingChargesPerMin;
        public decimal? AccWaitingChargesPerMin;
        public int? AutoStartWaitingBelowSpeedSeconds;
        public bool? AutoStartWaiting;
        public decimal? AutoStartWaitingBelowSpeed;
        public int? NoofPassengers;
        public bool? HasMeter;
    }

    public class MeterTarrif
    {
        public decimal? StartRate;
        public decimal? StartRateValidMiles;
        public decimal? FromMile;
        public decimal? TillMile;
        public decimal? Rate;

        public int AutoStartWaiting = 0;
        public decimal? AutoStartWaitingBelowSpeed = 0;
        public int? AutoStartWaitingBelowSpeedSeconds = 0;
        public int? AutoStopWaitingOnSpeed = 0;
        public decimal? DrvWaitingChargesPerMin = 0;


        public int FullRoundFares;
        public decimal RoundUpTo;
        public int WaitingSecondsToDivide;
        public int FreeWaitingMins;
        public decimal RoundJourneyMiles;
        public decimal? AutoStartWaitingMinDist { get; set; }
    }
}
