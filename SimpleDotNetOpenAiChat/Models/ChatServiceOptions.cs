using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDotNetOpenAiChat.Models
{
    public abstract class ChatServiceOptions
    {
        /// <summary>
        /// The Azure SignalR connection string to use for chat messages.
        /// </summary>
        public string AzureSignalRConnectionString { get; set; }
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
