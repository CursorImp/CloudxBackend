using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRHub.Classes
{
    public class DeviceModel
    {
        public int Id { get; set; }
        public string ConnectedAccountId { get; set; }
        public string RegistrationCode { get; set; }
        public string Label { get; set; }
        public string LocationId { get; set; }
        public string SerialNumber { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool? Motostatus { get; set; }
    }
}