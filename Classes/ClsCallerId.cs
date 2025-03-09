using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRHub
{
    public class ClsCallerId
    {
        public string accountId { get; set; }
        public string accountName { get; set; }
        public string organisationId { get; set; }
        public string organisationName { get; set; }
        public string parentCallId { get; set; }
        public string callId { get; set; }
        public string pbxId { get; set; }
        public string caller { get; set; }
        public string called { get; set; }
        public string extension { get; set; }
        public string @event { get; set; }
        public string state { get; set; }
        public long timestamp { get; set; }
    }
}