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
    public class OpenAiChatServicesOptions : ChatServiceOptions
    {
        /// <summary>
        /// The OpenAI API key to use for chat messages.
        /// </summary>
        public string OpenAiApiKey { get; set; }
        /// <summary>
        /// The OpenAI chat model ID to use. The default is "gpt-4o-mini".
        /// </summary>
        public string OpenAiChatModelId { get; set; } = "gpt-4o-mini";
    }
}
