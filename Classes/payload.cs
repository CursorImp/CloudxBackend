using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRHub
{
    public class payload
    {
        public string imageName { get; set; }

        public string file { get; set; }
        public string message { get; set; }
        public string filePath { get; set; }

        public string BaseUrl { get; set; }
    }
}