using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Web;

namespace SignalRHub
{
    public class stp_GetAreaPlotsByVehicleResultEx
    {
        public stp_GetAreaPlotsByVehicleResultEx() { }

        [Column(Storage = "_ZoneName", DbType = "VarChar(50)")]
        public string ZoneName { get; set; }
        [Column(Storage = "_Drivers", DbType = "Int")]
        public int? Drivers { get; set; }
        [Column(Storage = "_ExpiryJobs1", DbType = "Int")]
        public int? ExpiryJobs1 { get; set; }
        [Column(Storage = "_ExpiryJobs2", DbType = "Int")]
        public int? ExpiryJobs2 { get; set; }
        [Column(Storage = "_orderno", DbType = "Int")]
        public int Id { get; set; }



    }
}