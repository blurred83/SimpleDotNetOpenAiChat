using OpenAI.Chat;

namespace SimpleDotNetOpenAiChat.Models
{
    /// <summary>
    /// The result of continuing a chat conversation, including token usage, the new message, and the full conversation.
    /// </summary>
    public class ContinueChatResult
    {
        /// <summary>
        /// The new message that was added to the chat conversation.
        /// </summary>
        public string NewMessage { get; set; }
        /// <summary>
        /// The full chat conversation.
        /// </summary>
        public List<ChatMessage> Messages { get; set; }
        /// <summary>
        /// The number of tokens used for the output message.
        /// </summary>
        public int OutputTokenCount { get; set; }
        /// <summary>
        /// The number of tokens used for the input message.
        /// </summary>
        public int InputTokenCount { get; set; }
        /// <summary>
        /// The total number of tokens used for the chat conversation.
        /// </summary>
        public int TotalTokenCount { get; set; }
    }
}
