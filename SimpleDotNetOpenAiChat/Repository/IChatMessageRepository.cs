using OpenAI.Chat;

namespace SimpleDotNetOpenAiChat.Repository
{
    /// <summary>
    /// Repository for chat messages (e.g., to store chat messages in memory, or in a database, etc.).
    /// </summary>
    /// <remarks>
    /// The whole point of this repository is to abstract away the storage of chat messages. It wouldn't be optimal to
    /// send the entire chat history to the OpenAI API every time a new message is sent. Instead, we can store the chat
    /// in memory (or in a database) and only send the new messages to the OpenAI API. This repository is used to get
    /// and add chat messages for a connection ID.
    /// </remarks>
    public interface IChatMessageRepository
    {
        /// <summary>
        /// Get the chat messages for a connection ID.
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        List<ChatMessage> GetChatMessages(string connectionId);

        void AddChatMessage(string connectionId, ChatMessage chatMessage);
    }
}
