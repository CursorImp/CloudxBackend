using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRHub.Classes
{
    public class TerminalTokenResponse
    {
        [JsonProperty("client_secret")]
        public string ClientSecret { get; set; }

        [JsonProperty("object")]
        public string Object { get; set; }
    }
}