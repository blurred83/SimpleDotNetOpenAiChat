using Microsoft.Extensions.Caching.Memory;
using OpenAI.Chat;

namespace SimpleDotNetOpenAiChat.Repository
{
    /// <summary>
    /// Repository for chat messages (to store chat messages in memory).
    /// </summary>
    /// <remarks>
    /// This is a simple implementation of IChatMessageRepository that stores chat messages in memory using IMemoryCache.
    /// More info about IChatMessageRepository can be found in the interface definition.
    /// </remarks>
    public class MemoryCacheChatMessageRepository : IChatMessageRepository
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheChatMessageRepository(IMemoryCache memoryCache)
        {
            this._memoryCache = memoryCache;
        }

        /// <summary>
        /// The sliding expiration for the chat messages in memory. 30 minutes by default.
        /// </summary>
        public TimeSpan SlidingExpiration { get; set; } = TimeSpan.FromMinutes(30);

        public List<ChatMessage> GetChatMessages(string connectionId)
        {
            var chatMessages = _memoryCache.Get<List<ChatMessage>>(connectionId);
            return chatMessages ?? new List<ChatMessage>();
        }

        public void AddChatMessage(string connectionId, ChatMessage chatMessage)
        {
            _memoryCache.GetOrCreate(connectionId, entry =>
            {
                entry.SlidingExpiration = SlidingExpiration;
                return new List<ChatMessage>();
            }).Add(chatMessage);
        }
    }
}
