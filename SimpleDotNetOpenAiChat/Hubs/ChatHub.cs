using System.Text;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Identity.Client;
using OpenAI.Chat;
using SimpleDotNetOpenAiChat.Repository;
using SimpleDotNetOpenAiChat.Services;
using SimpleDotNetOpenAiChat.Utilities;

namespace SimpleDotNetOpenAiChat.Hubs
{
    /// <summary>
    /// The chat hub for the OpenAI chat assistant. 
    /// </summary>
    /// <remarks>
    /// This is the base class for your own chat hub implementation.
    /// Your chat hub implementation should inherit from this class and implement the SystemMessage property.
    /// The SystemMessage property is the system message for the OpenAI chat assistant.
    /// (e.g. "You are a technical support assistant. You are here to help users with technical issues.")
    /// </remarks>
    public abstract class ChatHub : Hub
    {
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly ChatService _chatService;
        private readonly NotifyingMemoryStream _notifyingMemoryStream;

        /// <summary>
        /// The system message for the OpenAI chat assistant.
        /// </summary>
        /// <remarks>
        /// For example, "You are a technical support assistant. You are here to help users with technical issues."
        /// </remarks>
        public abstract string SystemMessage { get; set; }

        protected Func<string> GetConnectionFunc = null;

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

        public ChatHub(IChatMessageRepository chatMessageRepository, ChatService chatService,
            NotifyingMemoryStream notifyingMemoryStream)
        {
            _chatMessageRepository = chatMessageRepository;
            _chatService = chatService;
            _notifyingMemoryStream = notifyingMemoryStream;
        }

        /// <summary>
        /// Sends a message to the OpenAI chat assistant.
        /// </summary>
        /// <param name="userMessage"></param>
        /// <returns></returns>
        public async Task SendMessage(string userMessage)
        {
            var connectionId = Context.ConnectionId;

            var sessionId = connectionId;

            if (GetConnectionFunc != null)
            {
                sessionId = GetConnectionFunc();
            }

            // memoryCache should have a dictionary of chat messages for each sessionId; get or create it
            var chatMessages = _chatMessageRepository.GetChatMessages(sessionId);

            if (!string.IsNullOrWhiteSpace(SystemMessage))
            {
                // check if _chatMessages has any system messages
                if (chatMessages.Count > 0 && chatMessages[0] is SystemChatMessage)
                {
                    chatMessages[0] = new SystemChatMessage(SystemMessage);
                }
                else
                {
                    chatMessages.Insert(0, new SystemChatMessage(SystemMessage));
                }
            }

            var userChatMessage = new UserChatMessage(userMessage);
            chatMessages.Add(userChatMessage);
            _chatMessageRepository.AddChatMessage(sessionId, userChatMessage);

            await Clients.Caller.SendAsync("ReceiveMessage", "StartAssistantResponse");

            int streamMessageBufferCount = 0;
            StringBuilder streamMessageBuffer = new StringBuilder();

            if (StreamResponse)
            {
                _notifyingMemoryStream.DataWritten += async (buffer, offset, count) =>
                {
                    var text = Encoding.UTF8.GetString(buffer, offset, count);

                    if (StreamMessageBuffer > 1)
                    {
                        streamMessageBufferCount++;
                        streamMessageBuffer.Append(text);
                        if (streamMessageBufferCount > StreamMessageBuffer)
                        {
                            await Clients.Caller.SendAsync("ReceiveMessage", streamMessageBuffer.ToString());
                            streamMessageBufferCount = 0;
                            streamMessageBuffer.Clear();
                        }

                        var messages = text.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var message in messages)
                        {
                            await Clients.Caller.SendAsync("ReceiveMessage", message);
                        }
                    }
                    else
                    {
                        await Clients.Caller.SendAsync("ReceiveMessage", text);
                    }
                };
            }

            var continueChatResult = await _chatService.ContinueChatStreaming(chatMessages, _notifyingMemoryStream);

            if (!string.IsNullOrWhiteSpace(streamMessageBuffer.ToString()))
            {
                await Clients.Caller.SendAsync("ReceiveMessage", streamMessageBuffer.ToString());
            }

            if (!StreamResponse)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", continueChatResult.NewMessage);
            }

            _chatMessageRepository.AddChatMessage(sessionId, continueChatResult.NewMessage);

            await Clients.Caller.SendAsync("ReceiveMessage", "EndAssistantResponse");
        }

        public async Task GetAllMessages()
        {
            var connectionId = Context.ConnectionId;

            var sessionId = connectionId;

            if (GetConnectionFunc != null)
            {
                sessionId = GetConnectionFunc();
            }

            // memoryCache should have a dictionary of chat messages for each sessionId; get or create it
            var chatMessages = _chatMessageRepository.GetChatMessages(sessionId);

            foreach (var chatMessage in chatMessages)
            {
                if (chatMessage.GetType() == typeof(AssistantChatMessage))
                {
                    await Clients.Caller.SendAsync("ReceiveMessage", "StartAssistantResponse");
                    await Clients.Caller.SendAsync("ReceiveMessage", chatMessage.Content[0].Text);
                    await Clients.Caller.SendAsync("ReceiveMessage", "EndAssistantResponse");
                }
                else if (chatMessage.GetType() == typeof(UserChatMessage))
                {
                    await Clients.Caller.SendAsync("ReceiveMessage", "StartUserResponse");
                    await Clients.Caller.SendAsync("ReceiveMessage", chatMessage.Content[0].Text);
                    await Clients.Caller.SendAsync("ReceiveMessage", "EndUserResponse");
                }
            }
        }
    }
}
