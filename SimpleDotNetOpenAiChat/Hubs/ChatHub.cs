﻿using System.Text;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Identity.Client;
using OpenAI.Chat;
using SimpleDotNetOpenAiChat.Models;
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
    public class ChatHub : Hub
    {
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly ChatService _chatService;
        private readonly NotifyingMemoryStream _notifyingMemoryStream;

        // Event to notify subscribers that a message has been received
        public delegate void MessageReceivedEventHandler(object sender, ChatMessage message);
        public event MessageReceivedEventHandler MessageReceived;

        /// <summary>
        /// The system message for the OpenAI chat assistant.
        /// </summary>
        /// <remarks>
        /// For example, "You are a technical support assistant. You are here to help users with technical issues."
        /// The default is "You are a helpful assistant."
        /// </remarks>
        private readonly string _systemMessage = "You are a helpful assistant.";
        
        /// <summary>
        /// Whether to stream the response from the OpenAI chat assistant.
        /// If false, the response will be sent all at once.
        /// </summary>
        private readonly bool _streamResponse = true;

        /// <summary>
        /// The number of messages to buffer before sending to the client.
        /// Useful to limit the number of SignalR messages sent to the client.
        /// </summary>
        private readonly int _streamMessageBuffer = 1;

        protected ChatHub(IChatMessageRepository chatMessageRepository, ChatService chatService,
            NotifyingMemoryStream notifyingMemoryStream, ChatHubConfig config)
        {
            _chatMessageRepository = chatMessageRepository;
            _chatService = chatService;
            _notifyingMemoryStream = notifyingMemoryStream;
            if (config != null)
            {
                _systemMessage = config.SystemMessage;
                _streamResponse = config.StreamResponse;
                _streamMessageBuffer = config.StreamMessageBuffer;
            }
        }

        /// <summary>
        /// Sends a message to the OpenAI chat assistant.
        /// </summary>
        /// <remarks>
        /// The session ID uniquely identifies the chat. Use this to identify a chat session.
        /// For instance, if you open a chat session in a new browser tab, and you pass the same session ID
        /// from the last tab, this will load the chat history into the window.
        /// If you don't specify a session ID, the SignalR connection ID will be used as the session ID.
        /// In that case, whenever the page refreshes, the chat history will be lost.
        /// </remarks>
        /// <param name="userMessage"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public async Task SendMessage(string sessionId, string userMessage)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                var connectionId = Context.ConnectionId;

                sessionId = connectionId;
            }

            // memoryCache should have a dictionary of chat messages for each sessionId; get or create it
            var chatMessages = _chatMessageRepository.GetChatMessages(sessionId);

            if (!string.IsNullOrWhiteSpace(_systemMessage))
            {
                // check if _chatMessages has any system messages
                if (chatMessages.Count > 0 && chatMessages[0] is SystemChatMessage)
                {
                    chatMessages[0] = new SystemChatMessage(_systemMessage);
                }
                else
                {
                    chatMessages.Insert(0, new SystemChatMessage(_systemMessage));
                }
            }

            var userChatMessage = new UserChatMessage(userMessage);
            chatMessages.Add(userChatMessage);
            _chatMessageRepository.AddChatMessage(sessionId, userChatMessage);

            await Clients.Caller.SendAsync("ReceiveMessage", "StartAssistantResponse");

            int streamMessageBufferCount = 0;
            StringBuilder streamMessageBuffer = new StringBuilder();

            if (_streamResponse)
            {
                _notifyingMemoryStream.DataWritten += async (buffer, offset, count) =>
                {
                    var text = Encoding.UTF8.GetString(buffer, offset, count);

                    if (_streamMessageBuffer > 1)
                    {
                        streamMessageBufferCount++;
                        streamMessageBuffer.Append(text);
                        if (streamMessageBufferCount > _streamMessageBuffer)
                        {
                            await Clients.Caller.SendAsync("ReceiveMessage", streamMessageBuffer.ToString());
                            streamMessageBufferCount = 0;
                            streamMessageBuffer.Clear();
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

            if (!_streamResponse)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", continueChatResult.NewMessage);
            }

            _chatMessageRepository.AddChatMessage(sessionId, continueChatResult.NewMessage);

            MessageReceived?.Invoke(this, continueChatResult.NewMessage);

            await Clients.Caller.SendAsync("ReceiveMessage", "EndAssistantResponse");
        }

        public async Task GetAllMessages(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                var connectionId = Context.ConnectionId;

                sessionId = connectionId;
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
