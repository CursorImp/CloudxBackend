using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRHub
{
    public class clsZones
    {
        public int Id;
        public string Area;
        public string PostCode;
        public string ZoneType;
        public bool? IsBaseZone;
        public double? MinLat;
        public double? MaxLat;
        public double? MinLng;
        public double? MaxLng;


        public string PlotEntranceMessage;
        public string PlotOverLimitMessage;
        public int? PlotLimit;
        public string shapeType = "";

        public double radius;
        public bool? DisableRank;
        public int? PlotKind;
        public int? OrderNo;
    }
}