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
    public class SimpleDotNetOpenAiChatServicesOptions
    {
        /// <summary>
        /// The Azure SignalR connection string to use for chat messages.
        /// </summary>
        public string AzureSignalRConnectionString { get; set; }
        /// <summary>
        /// The OpenAI API key to use for chat messages.
        /// </summary>
        public string OpenAiApiKey { get; set; }
        /// <summary>
        /// The OpenAI chat model ID to use. The default is "gpt-4o-mini".
        /// </summary>
        public string OpenAiChatModelId { get; set; } = "gpt-4o-mini";
        /// <summary>
        /// Whether to add a memory cache chat message repository.
        /// Default value is false. If you don't provide an IChatMessageRepository implementation, you must set this to true.
        /// </summary>
        public bool AddMemoryCacheChatMessageRepository { get; set; }
        /// <summary>
        /// The sliding expiration for the memory cache chat message repository.
        /// After this amount of time, a chat conversation will be removed from the cache.
        /// </summary>
        public TimeSpan SlidingExpiration { get; set; } = TimeSpan.FromMinutes(30);
    }
}
