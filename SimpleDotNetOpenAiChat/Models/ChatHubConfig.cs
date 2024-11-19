using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDotNetOpenAiChat.Models
{
    /// <summary>
    /// Configuration for the chat hub.
    /// </summary>
    public class ChatHubConfig
    {
        /// <summary>
        /// The system message for the OpenAI chat assistant.
        /// </summary>
        /// <remarks>
        /// For example, "You are a technical support assistant. You are here to help users with technical issues."
        /// The default is "You are a helpful assistant."
        /// </remarks>
        public string SystemMessage { get; set; } = "You are a helpful assistant.";

        /// <summary>
        /// Whether to stream the response from the OpenAI chat assistant.
        /// If false, the response will be sent all at once.
        /// </summary>
        public bool StreamResponse = true;

        /// <summary>
        /// The number of messages to buffer before sending to the client.
        /// Useful to limit the number of SignalR messages sent to the client.
        /// </summary>
        public int StreamMessageBuffer = 1;
    }
}
