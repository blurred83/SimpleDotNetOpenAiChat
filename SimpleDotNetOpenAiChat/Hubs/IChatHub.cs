namespace SimpleDotNetOpenAiChat.Hubs
{
    /// <summary>
    /// Interface for the chat hub.
    /// </summary>
    public interface IChatHub
    {
        /// <summary>
        /// The system message for the OpenAI chat assistant.
        /// </summary>
        string SystemMessage { get; }
    }
}
