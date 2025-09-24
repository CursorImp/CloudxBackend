using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRHub
{
    public class ClsPlotBidding
    {
        public int ZoneId;
        public string ZoneName;
        public int Drivers;
        public int J15;
        public int J30;
        public int Bid;
        public int BidDetails;
        public string Rank;
        public string DriverWorkStatus;
        public double Distance;
    }
    public class ClsPlotBiddingNearestPlot
    {
        public int ZoneId;
        public string ZoneName;
        public int Drivers;
        public int J15;
        public int J30;
        public int Bid;
        public int BidDetails;
        public string Rank;
        public string DriverWorkStatus;
        //    public int OrderNo;
        public DotNetCoords.LatLng centerLatLng;
        public double Distance;



    }
}