using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRHub.Classes
{
    public class FlightData
    {
        public DateTime? ScheduleDateTime { get; set; }
        public string StrScheduleDateTime { get; set; }
        public DateTime? DelayedDateTime { get; set; }
        public string StrDelayedDateTime { get; set; }
        public string ArrivalTerminal { get; set; }
        public string ArrivingFrom { get; set; }
        public string FlightNo { get; set; }
        public string DefaultClientId { get; set; }
        public string APIKey { get; set; }
        public string Status { get; set; }
        public string DateTime { get; set; }
        public string Message { get; set; }
        public int? AllowanceMins { get; set; }
        public string FlightInformation { get; set; }
        public string InputDateTime { get; set; }
    }
}