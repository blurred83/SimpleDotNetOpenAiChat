using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDotNetOpenAiChat.Models
{
    /// <summary>
    /// Options for configuring the SimpleDotNetOpenAiChat services.
    /// </summary>
    public class AzureAiFoundryChatServicesOptions : ChatServiceOptions
    {
        public string OpenAiEndpoint { get; set; }
        public string OpenAiKey { get; set; }
        public string DeploymentName { get; set; }
        public string SearchEndpoint { get; set; }
        public string SearchKey { get; set; }
        public string SearchIndex { get; set; }
        public float Temperature { get; set; } = .7f;
        public float TopP { get; set; } = .95f;
        public float FrequencyPenalty { get; set; } = 0;
        public float PresencePenalty { get; set; } = 0;
        public int MaxTokens { get; set; } = 800;
    }
}
