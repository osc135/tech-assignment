using System;
using Newtonsoft.Json;

namespace HijackPoker.Models
{
    [Serializable]
    public class Winner
    {
        [JsonProperty("seat")]
        public int Seat;

        [JsonProperty("playerId")]
        public int PlayerId;
    }
}
