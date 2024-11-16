let hubUrl = "/chatHub";
let hdnChatHubUrl = document.getElementById("hdnChatHubUrl");
if (hdnChatHubUrl) {
    hubUrl = hdnChatHubUrl.value;
}

let firstAssistantMessage = "How can I help you?";
let hdnFirstAssistantMessage = document.getElementById("hdnFirstAssistantMessage");
if (hdnFirstAssistantMessage) {
    firstAssistantMessage = hdnFirstAssistantMessage.value;
}

let chatConnection = new signalR.HubConnectionBuilder().withUrl(hubUrl).build();
let isAssistantResponding = false;
let currentAssistantDiv = null;

function appendMessageToChat(user, message) {
    let messageElement = document.createElement("div");
    messageElement.classList.add("d-flex", "align-items-start", "my-2");

    // Text content
    let messageText = document.createElement("div");
    messageText.classList.add("flex-grow-1", "p-2", "bg-light", "rounded");
    messageText.textContent = `${user}: ${message}`;

    // Copy button
    let copyButton = document.createElement("button");
    copyButton.classList.add("btn", "btn-outline-secondary", "btn-sm", "ms-2");
    copyButton.innerHTML = `<i class="fa fa-copy"></i>`;
    copyButton.onclick = function () {
        navigator.clipboard.writeText(message);
    };

    // Append message and copy button
    messageElement.appendChild(messageText);
    messageElement.appendChild(copyButton);

    document.getElementById("chatBody").appendChild(messageElement);
    document.getElementById("chatBody").scrollTop = document.getElementById("chatBody").scrollHeight; // Auto-scroll to the latest message
}

function sendMessage() {
    let sessionId = document.getElementById("hdnSessionId").value;
    let userMessage = document.getElementById("messageInput").value;
    if (userMessage.trim() === '') return; // Don't send empty messages

    // Disable only the send button and Enter key during assistant response
    document.getElementById("sendButton").disabled = true;
    isAssistantResponding = true;

    // Append user message to chat
    appendMessageToChat("You", userMessage);

    // Send the message to OpenAI via the SignalR hub
    chatConnection.invoke("SendMessage", sessionId, userMessage).catch(function (err) {
        return console.error(err.toString());
    });

    // Clear input field
    document.getElementById("messageInput").value = '';
    resizeTextarea();
}

chatConnection.on("ReceiveMessage", function (message) {
    if (message === "StartAssistantResponse") {
        // Create a new message element
        currentAssistantDiv = document.createElement("div");
        currentAssistantDiv.classList.add("d-flex", "align-items-start", "my-2");

        // Text content container for the assistant message
        let messageText = document.createElement("div");
        messageText.classList.add("flex-grow-1", "p-2", "bg-light", "rounded");
        messageText.textContent = "Assistant: "; // Initial label for assistant

        // Copy button
        let copyButton = document.createElement("button");
        copyButton.classList.add("btn", "btn-outline-secondary", "btn-sm", "ms-2");
        copyButton.innerHTML = `<i class="fa fa-copy"></i>`;
        copyButton.onclick = function () {
            navigator.clipboard.writeText(messageText.textContent);
        };

        // Append message text and copy button
        currentAssistantDiv.appendChild(messageText);
        currentAssistantDiv.appendChild(copyButton);

        document.getElementById("chatBody").appendChild(currentAssistantDiv);
        isAssistantResponding = true;
    } else if (message === "EndAssistantResponse") {
        isAssistantResponding = false;
        document.getElementById("sendButton").disabled = false;
        currentAssistantDiv = null;
        document.getElementById("chatBody").scrollTop = document.getElementById("chatBody").scrollHeight;
    } else if (message === "StartUserResponse") {
        // Create a new message element
        currentAssistantDiv = document.createElement("div");
        currentAssistantDiv.classList.add("d-flex", "align-items-start", "my-2");

        // Text content container for the assistant message
        let messageText = document.createElement("div");
        messageText.classList.add("flex-grow-1", "p-2", "bg-light", "rounded");
        messageText.textContent = "You: "; // Initial label for assistant

        // Copy button
        let copyButton = document.createElement("button");
        copyButton.classList.add("btn", "btn-outline-secondary", "btn-sm", "ms-2");
        copyButton.innerHTML = `<i class="fa fa-copy"></i>`;
        copyButton.onclick = function () {
            navigator.clipboard.writeText(messageText.textContent);
        };

        // Append message text and copy button
        currentAssistantDiv.appendChild(messageText);
        currentAssistantDiv.appendChild(copyButton);

        document.getElementById("chatBody").appendChild(currentAssistantDiv);
        isAssistantResponding = true;
    } else if (message === "EndUserResponse") {
        isAssistantResponding = false;
        document.getElementById("sendButton").disabled = false;
        currentAssistantDiv = null;
        document.getElementById("chatBody").scrollTop = document.getElementById("chatBody").scrollHeight;
    } else if (isAssistantResponding && currentAssistantDiv !== null) {
        // Append each message chunk to the assistant's message text
        currentAssistantDiv.querySelector(".flex-grow-1").textContent += message;
        document.getElementById("chatBody").scrollTop = document.getElementById("chatBody").scrollHeight;
    }
});


chatConnection.start().then(function () {
    // Initialize chat with the first assistant message
    appendMessageToChat("Assistant", firstAssistantMessage);

    let sessionId = document.getElementById("hdnSessionId").value;
    chatConnection.invoke("GetAllMessages", sessionId);

    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    console.error(err.toString());
    appendMessageToChat("Error", "Failed to connect to the chat server. Please try again later.");
});

// Add click event for send button
document.getElementById("sendButton").addEventListener("click", function (event) {
    sendMessage();
    event.preventDefault();
});

// Add keyup event to submit on Enter key
document.getElementById("messageInput").addEventListener("keyup", function (event) {
    if (!isAssistantResponding && event.key === "Enter") {
        sendMessage();
        event.preventDefault();
    }
});

// Resize textarea dynamically
function resizeTextarea() {
    const textarea = document.getElementById("messageInput");
    textarea.style.height = 'auto';
    textarea.style.height = Math.min(textarea.scrollHeight, 60) + 'px'; // Max 3 lines (60px)
}

// Add input event for dynamically resizing textarea
document.getElementById("messageInput").addEventListener("input", resizeTextarea);

// Toggle chat window collapse/expand
document.getElementById("toggleChat").addEventListener("click", function () {
    const chatBody = document.getElementById("chatBody");
    const chatFooter = document.getElementById("chatFooter");
    const toggleIcon = document.getElementById("toggleIcon");

    if (chatBody.style.display === "none") {
        chatBody.style.display = "block";
        chatFooter.style.display = "block";
        toggleIcon.classList.remove("fa-plus");
        toggleIcon.classList.add("fa-minus"); // Show collapse icon
        document.getElementById("chatBody").scrollTop = document.getElementById("chatBody").scrollHeight;
    } else {
        chatBody.style.display = "none";
        chatFooter.style.display = "none";
        toggleIcon.classList.remove("fa-minus");
        toggleIcon.classList.add("fa-plus"); // Show expand icon
    }
});