using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HijackPoker.Models
{
    [Serializable]
    public class SidePot
    {
        [JsonProperty("amount")]
        public float Amount;

        [JsonProperty("eligibleSeats")]
        public List<int> EligibleSeats;
    }
}
