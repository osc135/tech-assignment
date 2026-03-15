using System;
using Newtonsoft.Json;

namespace HijackPoker.Models
{
    [Serializable]
    public class HealthResponse
    {
        [JsonProperty("service")]
        public string Service;

        [JsonProperty("status")]
        public string Status;

        [JsonProperty("timestamp")]
        public string Timestamp;
    }
}
