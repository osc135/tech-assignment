using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HijackPoker.Models
{
    [Serializable]
    public class GameState
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("tableId")]
        public int TableId;

        [JsonProperty("tableName")]
        public string TableName;

        [JsonProperty("gameNo")]
        public int GameNo;

        [JsonProperty("handStep")]
        public int HandStep;

        [JsonProperty("stepName")]
        public string StepName;

        [JsonProperty("dealerSeat")]
        public int DealerSeat;

        [JsonProperty("smallBlindSeat")]
        public int SmallBlindSeat;

        [JsonProperty("bigBlindSeat")]
        public int BigBlindSeat;

        [JsonProperty("communityCards")]
        public List<string> CommunityCards;

        [JsonProperty("pot")]
        public float Pot;

        [JsonProperty("sidePots")]
        public List<SidePot> SidePots;

        [JsonProperty("move")]
        public int Move;

        [JsonProperty("status")]
        public string Status;

        [JsonProperty("smallBlind")]
        public float SmallBlind;

        [JsonProperty("bigBlind")]
        public float BigBlind;

        [JsonProperty("maxSeats")]
        public int MaxSeats;

        [JsonProperty("currentBet")]
        public float CurrentBet;

        [JsonProperty("winners")]
        public List<Winner> Winners;

        public bool IsShowdown => HandStep >= 12;

        public bool IsHandComplete => StepName == "RECORD_STATS_AND_NEW_HAND";
    }
}
