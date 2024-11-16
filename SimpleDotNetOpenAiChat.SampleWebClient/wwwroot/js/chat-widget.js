class ChatWidget extends HTMLElement {
    constructor() {
        super();

        // Attach Shadow DOM for encapsulation
        this.attachShadow({ mode: 'open' });

        // Get the chatHubUrl dynamically from a hidden field (default: /chatHub)
        this.chatHubUrl = '/chatHub';
        const hdnChatHubUrl = document.getElementById('hdnChatHubUrl');
        if (hdnChatHubUrl) {
            this.chatHubUrl = hdnChatHubUrl.value;
        }

        // Add FontAwesome styles dynamically
        const fontAwesomeLink = document.createElement('link');
        fontAwesomeLink.setAttribute('rel', 'stylesheet');
        fontAwesomeLink.setAttribute('href', 'https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css');

        const styles = `
            .chat-widget {
                position: fixed;
                bottom: 0;
                right: 0;
                width: 400px;
                border: 1px solid #ddd;
                box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
                font-family: Arial, sans-serif;
                background-color: #fff;
                z-index: 1000;
            }
            .header {
                background-color: #007bff;
                color: white;
                padding: 8px;
                display: flex;
                justify-content: space-between;
                align-items: center;
                cursor: pointer;
            }
            .body {
                display: none;
                padding: 10px;
                height: 250px;
                overflow-y: auto;
                border-top: 1px solid #ddd;
            }
            .footer {
                display: none;
                padding: 8px;
                border-top: 1px solid #ddd;
                background: #f1f1f1;
            }
            textarea {
                width: calc(100% - 16px);
                margin: 4px;
                padding: 8px;
                resize: none;
            }
            button {
                background-color: #007bff;
                color: white;
                border: none;
                padding: 6px 12px;
                cursor: pointer;
            }
            button[disabled] {
                background-color: #aaa;
                cursor: not-allowed;
            }
        `;

        const template = `
            <div class="chat-widget">
                <div class="header">
                    <span>AI Chat</span>
                    <button id="toggleChat" class="btn btn-sm btn-outline-light">
                        <i id="toggleIcon" class="fas fa-plus"></i> <!-- FontAwesome icon -->
                    </button>
                </div>
                <div class="body">
                    <div class="messages"></div>
                </div>
                <div class="footer">
                    <textarea rows="1" placeholder="Type your message..."></textarea>
                    <button disabled>Send</button>
                </div>
            </div>
        `;

        const style = document.createElement('style');
        style.textContent = styles;

        // Add FontAwesome and component styles to Shadow DOM
        this.shadowRoot.appendChild(fontAwesomeLink);
        this.shadowRoot.appendChild(style);
        this.shadowRoot.innerHTML += template;

        // Element references
        this.body = this.shadowRoot.querySelector('.body');
        this.footer = this.shadowRoot.querySelector('.footer');
        this.messagesDiv = this.shadowRoot.querySelector('.messages');
        this.textarea = this.shadowRoot.querySelector('textarea');
        this.sendButton = this.shadowRoot.querySelector('button');

        // Event listeners
        this.shadowRoot.querySelector('.header').addEventListener('click', () => this.toggleChat());
        this.textarea.addEventListener('input', (e) => this.handleInput(e));
        this.sendButton.addEventListener('click', () => this.sendMessage());

        // Initialize SignalR connection
        this.initSignalR();
    }

    toggleChat() {
        const isVisible = this.body.style.display === 'block';
        this.body.style.display = isVisible ? 'none' : 'block';
        this.footer.style.display = isVisible ? 'none' : 'block';

        const toggleIcon = this.shadowRoot.querySelector('#toggleIcon');
        toggleIcon.classList.toggle('fa-plus', isVisible);
        toggleIcon.classList.toggle('fa-minus', !isVisible);
    }

    handleInput(e) {
        this.sendButton.disabled = !e.target.value.trim();
    }

    async initSignalR() {
        try {
            // Create SignalR connection
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl(this.chatHubUrl)
                .build();

            // Register SignalR events
            this.connection.on('ReceiveMessage', (message) => {
                this.appendMessage('AI', message);
            });

            // Start the connection
            await this.connection.start();
            this.appendMessage('AI', 'Chat is ready. Ask me anything!');
        } catch (error) {
            console.error('SignalR connection failed:', error);
            this.appendMessage('System', 'Failed to connect to the chat server.');
        }
    }

    async sendMessage() {
        const message = this.textarea.value.trim();
        if (!message) return;

        this.appendMessage('You', message);

        const systemMessage = document.getElementById('hdnSystemMessage')?.value || null;

        try {
            await this.connection.invoke('SendToOpenAI', message, systemMessage);
        } catch (error) {
            console.error('Failed to send message:', error);
            this.appendMessage('System', 'Failed to send your message.');
        }

        this.textarea.value = '';
        this.handleInput({ target: this.textarea });
    }

    appendMessage(sender, message) {
        const messageDiv = document.createElement('div');
        messageDiv.innerHTML = `<strong>${sender}:</strong> ${message}`;
        this.messagesDiv.appendChild(messageDiv);
        this.messagesDiv.scrollTop = this.messagesDiv.scrollHeight; // Auto-scroll
    }
}

// Define the custom element
customElements.define('chat-widget', ChatWidget);
