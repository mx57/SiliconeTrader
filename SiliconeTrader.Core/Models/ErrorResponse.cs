using System.Collections.Generic;
using Newtonsoft.Json; // For JsonProperty

namespace SiliconeTrader.Core.Models
{
    // Merged definition
    public class ErrorResponse
    {
        [JsonProperty("context")]
        public object Context { get; set; }

        [JsonProperty("error")]
        public string ErrorMessage { get; set; } // From original Machine.Client.Models

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("requestId")]
        public string RequestId { get; set; }

        // Properties from the new definition that was in Core temporarily
        public List<string> Errors { get; set; } = new List<string>();
        public bool IsUserVisible { get; set; }

        // Static factory method from original Machine.Client.Models
        // Making it public if it needs to be accessed by BaseManager or other classes.
        // If it was internal and only used within Machine.Client, this might need review.
        // For now, making it public to resolve BaseManager's potential access.
        public static ErrorResponse FromError(string result)
        {
            return new ErrorResponse
            {
                ErrorMessage = result
                // Or, if preferred to use the List<string> Errors:
                // Errors = new List<string> { result }
            };
        }
    }
}
