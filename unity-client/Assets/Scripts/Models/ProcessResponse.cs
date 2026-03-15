using System;
using Newtonsoft.Json;

namespace HijackPoker.Models
{
    [Serializable]
    public class ProcessResponse
    {
        [JsonProperty("success")]
        public bool Success;

        [JsonProperty("result")]
        public ProcessResult Result;

        [JsonProperty("error")]
        public string Error;
    }

    [Serializable]
    public class ProcessResult
    {
        [JsonProperty("status")]
        public string Status;

        [JsonProperty("tableId")]
        public int TableId;

        [JsonProperty("step")]
        public int Step;

        [JsonProperty("stepName")]
        public string StepName;
    }
}
