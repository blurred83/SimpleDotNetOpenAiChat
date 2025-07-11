# SimpleDotNetOpenAiChat

`SimpleDotNetOpenAiChat` is a .NET-based framework designed to simplify the integration of a streaming AI chatbot into a web interface. The chatbot is displayed in the bottom-right corner of a web page and leverages **Azure SignalR** for real-time communication and **OpenAI** for AI responses.

This solution is ideal for developers looking to add an interactive chatbot with minimal configuration while maintaining flexibility for custom implementations.

---

## Features

- **Streaming Chat**: Streams AI-generated responses token by token or in chunks for an interactive experience.
- **Customizable Storage**: Use in-memory caching or implement your own storage solution for chat message persistence.
- **Plug-and-Play Frontend**: Includes a lightweight, configurable JavaScript file (`chat-component.js`) for the chat UI.
- **System Message Customization**: Define AI behavior by setting a system message for the assistant.
- **Session Management**: Use session IDs to persist chat history across page refreshes or reconnects.
- **Dependency Injection**: Easily integrate with .NET applications using preconfigured services.

---

## Prerequisites

1. Azure SignalR Connection String
2. OpenAI API Key

---

## Solution Structure

### SimpleDotNetOpenAiChat

A .NET 6 class library containing the core logic for integrating an AI chatbot with Azure SignalR and OpenAI. This library will be distributed as a NuGet package in future releases.

### JS

Contains `chat-component.js`, a JavaScript file for the chat UI and SignalR messaging. Clone the repository and include it directly in your project.

### SimpleDotNetOpenAiChat.SampleWebClient

A .NET 8 Razor Pages project demonstrating how to use the class library and JavaScript component to build a working chatbot.

---

## Quick Start

### Configure Your Web Application

First install the nuget package `SimpleDotNetOpenAiChat`. (Make sure to include pre-release packages if necessary.) )
```
dotnet add package SimpleDotNetOpenAiChat
```

Add the required services to your application by configuring your dependency injection container. Use the provided extension methods to simplify registration.

```
builder.Services.AddSimpleDotNetOpenAiChatServices(opt =>
{
    opt.AzureSignalRConnectionString = builder.Configuration["AzureSignalRConnectionString"];
    opt.OpenAiApiKey = builder.Configuration["OpenAiApiKey"];
    opt.AddMemoryCacheChatMessageRepository = true;
    opt.SlidingExpiration = TimeSpan.FromMinutes(30); // Optional
    opt.OpenAiChatModelId = "gpt-4o-mini"; // Optional
});
```

You can add different bots with different functions. For instance, you can define a tech support bot that helps troubleshoot computer problems, and a joke bot that tells jokes. For each bot, define a configuration like this:

```
builder.Services.Configure<ChatHubConfig>("JokeBot", options =>
{
    options.SystemMessage =
        @"You are a joke telling bot. Your job is to tell jokes about whatever the user wants. Don't talk about anything else; just tell jokes.";
    options.StreamResponse = true;
    options.StreamMessageBuffer = 5;
});
```

Note this configuration is named "JokeBot" - give your configuration a meaningful name; you'll need it soon. 

The arguments here are: 

- **SystemMessage**: Tell the bot what to do.
- **StreamResponse**: Whether to stream the response or just send full messages once they are finished. The default is true.
- **StreamMessageBuffer**: To limit the number of SignalR messages transmitted, this will only send blocks of text from server to client every x tokens received. (The default is one; by default it will stream each token.) If you set this to 5, it will stream chunks of 5 tokens at a time, reducing the number of total SignalR messages sent by 80%.

### Create a Chat Hub

Create a class that extends `ChatHub`. It doesn't need to do anything else, but you could extend it to add custom functionality.

```
using SimpleDotNetOpenAiChat.Hubs;
using SimpleDotNetOpenAiChat.Models;
using SimpleDotNetOpenAiChat.Repository;
using SimpleDotNetOpenAiChat.Services;
using SimpleDotNetOpenAiChat.Utilities;

public class JokeHub : ChatHub
{
    public JokeHub(IChatMessageRepository chatMessageRepository, ChatService chatService,
        NotifyingMemoryStream notifyingMemoryStream, ChatHubConfig config) : base(chatMessageRepository, chatService, notifyingMemoryStream, config)
    {

    }
}
```

Go back to the Program.cs, and add a block like this (right where you left off, after defining the named configuration): 

```
builder.Services.AddTransient<JokeHub>(sp =>
{
    var chatMessageRepository = sp.GetRequiredService<IChatMessageRepository>();
    var chatService = sp.GetRequiredService<ChatService>();
    var notifyingMemoryStream = sp.GetRequiredService<NotifyingMemoryStream>();
    var config = sp.GetRequiredService<IOptionsSnapshot<ChatHubConfig>>().Get("JokeBot");
    return new JokeHub(chatMessageRepository, chatService, notifyingMemoryStream, config);
});
```

Note here you applied the name of the configuration at the end of the "var config..."  line.

Finally, right before app.Run(), add this:

```
app.MapHub<JokeHub>("/jokeHub");
```

### Add the Frontend

Include the FontAwesome and SignalR javascript libraries in your layout file. 

```
<script src="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/js/fontawesome.min.js"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/js/solid.min.js"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/5.0.15/signalr.min.js"></script>
```

Add the chatbot widget to your page. Add the required hidden fields for configuration, such as the SignalR hub URL and the initial assistant message. The SignalR hub URL should match your program.cs. 

The session ID is not required, but you can set it if you want to store chat history across page reloads. Any string you pass for session ID will load the history for that session, as long as the chat history is retained in the message repository. If you do not pass a session ID, the session ID will use the signalr connection ID, and will reset with each page reload.

Note the chat-component.js can be delivered through jsdelivr, or you can get the javascript file from the /js folder in the repository and embed a local copy.

```
<div id="chatWidget" class="chat-widget card shadow-lg position-fixed bottom-0 end-0 m-3" style="width: 400px; z-index: 9999;">
    <div class="card-header bg-primary text-white d-flex justify-content-between align-items-center">
        <span>AI Chat</span>
        <button id="toggleChat" class="btn btn-sm btn-outline-light">
            <i id="toggleIcon" class="fas fa-plus"></i>
        </button>
    </div>
    <div id="chatBody" class="card-body overflow-auto" style="height: 250px; display: none;"></div>
    <div id="chatFooter" class="card-footer p-2" style="display: none;">
        <textarea id="messageInput" class="form-control" placeholder="Type your message..." rows="1"></textarea>
        <button id="sendButton" class="btn btn-primary" disabled>Send</button>
    </div>
</div>
<input type="hidden" id="hdnFirstAssistantMessage" value="Hello! How can I assist you today?" />
<input type="hidden" id="hdnChatHubUrl" value="/myChatHub" />
<input type="hidden" id="hdnSessionId" value="@Model.SessionId" />
<script src="https://cdn.jsdelivr.net/gh/blurred83/SimpleDotNetOpenAiChat@v1.0.0-alpha.1/js/chat-component.js"></script>
```

---

## How It Works

- **SignalR Integration**: The `chat-component.js` file handles real-time communication with the server-side SignalR hub.
- **Session Management**: The session ID ensures continuity of chat history. If a session ID is not provided, the SignalR connection ID is used as a fallback.
- **Chat Message Repository**: Messages can be stored in memory or in a custom repository. Implement the `IChatMessageRepository` interface to provide persistent storage.

---

## Customization

### Options for Service Registration

- Azure SignalR Connection String
- OpenAI API Key
- Memory Cache for Chat Messages
- Sliding Expiration for Cache
- OpenAI Chat Model ID

### Chat Hub Properties

- System Message: Defines the AI assistant's behavior.
- Stream Response: Determines if responses are streamed token by token.
- Stream Message Buffer: Specifies the number of tokens to buffer before sending updates to the client.

---

## Notes

- **FontAwesome Requirement**: Ensure FontAwesome is included to render icons correctly in the chat widget.
- **Session Management**: Use a custom session ID to persist chat history across page loads. A persistent repository is required for long-term storage.
- **JavaScript File**: The `chat-component.js` file must be included in your project or served via CDN.

---

## Future Plans

1. Enhance the chat UI with additional features, such as themes and improved chat history management.
2. add unit tests.

---

## Contributing

Contributions are welcome! Fork the repository, make your changes, and submit a pull request.

---

## License

This project is licensed under the MIT License. See the `LICENSE` file for details.
