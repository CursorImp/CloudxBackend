using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalRHub
{
    public  class JobMeter
    {

        public decimal? Miles;
        public decimal? Fares;
        public string IsWaiting;
        public decimal? WaitingCharges;
        public int? WaitingTime;
        public string VehicleType;
        public string PickupDateTime;
        public string JobID;      
        public string DriverNo;
        public decimal? Speed;
        public decimal? WaitingSpeed;
        public int? SpeedSecs;
        public string CompanyId="0";
        public string SubCompanyId="0";
        public string QuotedPrice="0";

        public string lg = "";
        public string lt = "";

    }
}
