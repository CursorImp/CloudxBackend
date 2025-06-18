using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRHub.Classes
{
    public class TerminalReaderResponse
    {
        [JsonProperty("isSuccess")]
        public bool IsSuccess { get; set; }

        [JsonProperty("readerId")]
        public string ReaderId { get; set; }

        [JsonProperty("response")]
        public ReaderData Response { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }

    public class ReaderData
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("action")]
        public object Action { get; set; }

        [JsonProperty("deleted")]
        public object Deleted { get; set; }

        [JsonProperty("deviceSwVersion")]
        public string DeviceSwVersion { get; set; }

        [JsonProperty("deviceType")]
        public string DeviceType { get; set; }

        [JsonProperty("ipAddress")]
        public string IpAddress { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("livemode")]
        public bool Livemode { get; set; }

        [JsonProperty("locationId")]
        public string LocationId { get; set; }

        [JsonProperty("location")]
        public object Location { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("serialNumber")]
        public string SerialNumber { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("rawJObject")]
        public RawJObject RawJObject { get; set; }

        [JsonProperty("stripeResponse")]
        public object StripeResponse { get; set; }
    }

    public class Metadata
    {
        [JsonProperty("ClientId")]
        public string ClientId { get; set; }
    }

    public class RawJObject
    {
        [JsonProperty("id")]
        public List<object> Id { get; set; }

        [JsonProperty("object")]
        public List<object> Object { get; set; }

        [JsonProperty("action")]
        public List<object> Action { get; set; }

        [JsonProperty("device_sw_version")]
        public List<object> DeviceSwVersion { get; set; }

        [JsonProperty("device_type")]
        public List<object> DeviceType { get; set; }

        [JsonProperty("ip_address")]
        public List<object> IpAddress { get; set; }

        [JsonProperty("label")]
        public List<object> Label { get; set; }

        [JsonProperty("last_seen_at")]
        public List<object> LastSeenAt { get; set; }

        [JsonProperty("livemode")]
        public List<object> Livemode { get; set; }

        [JsonProperty("location")]
        public List<object> Location { get; set; }

        [JsonProperty("metadata")]
        public List<List<List<object>>> Metadata { get; set; }

        [JsonProperty("serial_number")]
        public List<object> SerialNumber { get; set; }

        [JsonProperty("status")]
        public List<object> Status { get; set; }
    }

}