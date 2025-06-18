using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRHub.Classes
{
    public class TerminalLogModel
    {
        public string TerminalId { get; set; }
        public string RequestPayload { get; set; }
        public string ResponsePayload { get; set; }
    }
}