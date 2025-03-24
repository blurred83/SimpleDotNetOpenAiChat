using Microsoft.AspNetCore.SignalR;
using SimpleDotNetOpenAiChat.Hubs;
using SimpleDotNetOpenAiChat.Models;
using SimpleDotNetOpenAiChat.Repository;
using SimpleDotNetOpenAiChat.Services;
using SimpleDotNetOpenAiChat.Utilities;

namespace SimpleDotNetOpenAiChat.SampleWebClient.Hubs
{
    /// <summary>
    /// An example chat hub implementation for the OpenAI chat assistant.
    /// </summary>
    public class StoreHub : ChatHub
    {
        /// <summary>
        /// Subclass of <see cref="ChatHub"/> class. Different subclasses allow for multiple chatbots with different purposes.
        /// </summary>
        /// <param name="chatMessageRepository"></param>
        /// <param name="chatService"></param>
        /// <param name="notifyingMemoryStream"></param>
        /// <param name="config"></param>
        public StoreHub(IChatMessageRepository chatMessageRepository, ChatService chatService,
            NotifyingMemoryStream notifyingMemoryStream, ChatHubConfig config) : base(chatMessageRepository, chatService, notifyingMemoryStream, config)
        {

        }
    }
}
