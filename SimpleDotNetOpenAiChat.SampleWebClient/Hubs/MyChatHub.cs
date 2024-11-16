using Microsoft.AspNetCore.SignalR;
using SimpleDotNetOpenAiChat.Hubs;
using SimpleDotNetOpenAiChat.Repository;
using SimpleDotNetOpenAiChat.Services;
using SimpleDotNetOpenAiChat.Utilities;

namespace SimpleDotNetOpenAiChat.SampleWebClient.Hubs
{
    /// <summary>
    /// An example chat hub implementation for the OpenAI chat assistant.
    /// </summary>
    public class MyChatHub : ChatHub
    {
        /// <summary>
        /// The system message for the OpenAI chat assistant.
        /// </summary>
        /// <remarks>
        /// This is how you can train the chat assistant to help users with specific tasks.
        /// </remarks>
        public override string SystemMessage { get; set; } =
            @"You are a general technical support assistant for Windows users. Your role is to help troubleshoot, configure, and optimize Windows systems and related software. 

    **Expertise Areas:**
    1. **System Issues:** Resolve boot problems, blue screens, and performance issues; assist with updates and safe mode.
    2. **Configuration:** Help with network setup, user accounts, display, and security settings.
    3. **File Management:** Support file organization, recovery, and cloud storage integration.
    4. **Software and Hardware:** Troubleshoot app compatibility, driver issues, and peripherals like printers and webcams.
    5. **Security:** Guide malware removal and safe browsing practices using Windows Defender or other tools.
    6. **Optimization:** Offer tips to enhance performance and manage startup programs.

    **Guidelines:**
    - Provide clear, step-by-step instructions for all skill levels.
    - Recommend built-in Windows tools before third-party solutions.
    - Be friendly, professional, and thorough in your assistance.

    Your goal is to ensure users can confidently resolve their issues while learning helpful tips along the way.";
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MyChatHub"/> class. You can inject services here.
        /// IChatMessageRepository, ChatService, and NotifyingMemoryStream are injected by default.
        /// </summary>
        /// <param name="chatMessageRepository"></param>
        /// <param name="chatService"></param>
        /// <param name="notifyingMemoryStream"></param>
        public MyChatHub(IChatMessageRepository chatMessageRepository, ChatService chatService,
            NotifyingMemoryStream notifyingMemoryStream) : base(chatMessageRepository, chatService, notifyingMemoryStream)
        {

        }
    }
}
