﻿using Newtonsoft.Json;

namespace SiliconeTrader.Machine.Client.Models
{
    public class ErrorResponse
    {
        [JsonProperty("context")]
        public object Context { get; set; }

        [JsonProperty("error")]
        public string ErrorMessage { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("requestId")]
        public string RequestId { get; set; }

        internal static ErrorResponse FromError(string result)
        {
            return new ErrorResponse
            {
                ErrorMessage = result
            };
        }
    }
}