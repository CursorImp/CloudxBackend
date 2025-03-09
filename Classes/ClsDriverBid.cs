using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalRHub
{
    public class ClsDriverBid
    {

        public long JobId;
        public int? DriverId;
        public string DriverNo;
        public decimal DriverPrice;
        public decimal JobPrice;
        public int? JobZoneId;
        public string JobZoneName;
        public string Pickup;
        public string DropOff;
        public string PickupDateTime;
        public string JobVehicle;
        public string DeviceType;
        public string AppVersion;



        // DONT NEED TO SET BELOW PROPERTIES
        public int BiddingType;
        public DateTime? BiddingDateTime;
        public DateTime? ElapsedTime;
        public String JobMessage;
        public string Status;


        private void AddJobElapsedTime()
        {




        }



    }
}
