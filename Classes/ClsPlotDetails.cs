using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRHub
{
    public class ClsPlotDetails
    {
        public string PlotName;
        public string DriverId;
        public string DriverNo;
        public string Version;
        public string Time;
        public string Vehicle;

        public int PlotId;
        public long JobId;
        public string PickupAddress;
        public string DropOff;
    }





  


    public class ClsChangePaymentType
    {
        public string JobId;
        public string DriverId;
        public string NewPaymentType;
        public string OldPaymentType;
        public string DriverNo;
        public string Version;
      
    }
}