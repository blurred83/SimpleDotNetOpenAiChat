using System.ClientModel;
using System.Text;
using OpenAI.Chat;
using SimpleDotNetOpenAiChat.Models;
using SimpleDotNetOpenAiChat.Utilities;

namespace SimpleDotNetOpenAiChat.Services
{
    public class ChatService
    {
        private readonly ChatClient _chatClient;
        public ChatService(ChatClient chatClient)
        {
            this._chatClient = chatClient;
        }

        public async Task<ContinueChatResult> ContinueChatStreaming(List<ChatMessage> messages, NotifyingMemoryStream notifyingMemoryStream = null)
        {
            var newMessage = new StringBuilder();

            bool requiresAction;

            ContinueChatResult result = new ContinueChatResult();

            do
            {
                requiresAction = false;
                AsyncCollectionResult<StreamingChatCompletionUpdate> chatUpdates
                    = _chatClient.CompleteChatStreamingAsync(messages);

                await foreach (StreamingChatCompletionUpdate chatUpdate in chatUpdates)
                {
                    if (chatUpdate.Usage != null)
                    {
                        result.InputTokenCount = chatUpdate.Usage.InputTokenCount;
                        result.OutputTokenCount = chatUpdate.Usage.OutputTokenCount;
                        result.TotalTokenCount = chatUpdate.Usage.TotalTokenCount;
                    }

                    // Accumulate the text content as new updates arrive.
                    foreach (ChatMessageContentPart contentPart in chatUpdate.ContentUpdate)
                    {
                        newMessage.Append(contentPart.Text);
                        if (notifyingMemoryStream != null)
                        {
                            notifyingMemoryStream.Write(Encoding.UTF8.GetBytes(contentPart.Text));
                        }
                    }

                    // Handle tool call completion
                    switch (chatUpdate.FinishReason)
                    {
                        case ChatFinishReason.ToolCalls:
                            requiresAction = true;
                            break;
                    }
                }
            } while (requiresAction);

            result.NewMessage = newMessage.ToString();

            messages.Add(new AssistantChatMessage(newMessage.ToString()));

            result.Messages = messages;

            return result;
        }
    }
}
