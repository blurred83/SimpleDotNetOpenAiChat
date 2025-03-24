using System.Text;
using Azure.AI.OpenAI;
using Azure;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.Chat;
using SimpleDotNetOpenAiChat.Models;
using SimpleDotNetOpenAiChat.Repository;
using SimpleDotNetOpenAiChat.Services;
using SimpleDotNetOpenAiChat.Utilities;
using Azure.AI.OpenAI.Chat;

namespace SimpleDotNetOpenAiChat.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the SimpleDotNetOpenAiChat services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="options">The options for configuring the services.
        /// You must provide the Azure SignalR connection string and OpenAI API key.
        /// Also, you either must register an IChatMessageRepository implementation or set AddMemoryCacheChatMessageRepository to true.
        /// </param>
        /// <exception cref="ArgumentException"></exception>
        public static void AddSimpleDotNetOpenAiChatServices(this IServiceCollection services, 
            Action<OpenAiChatServicesOptions> options)
        {
            var opt = new OpenAiChatServicesOptions();
            options?.Invoke(opt);

            var errorMessageBuilder = new StringBuilder();
            if (string.IsNullOrWhiteSpace(opt.AzureSignalRConnectionString))
            {
                errorMessageBuilder.AppendLine("Azure SignalR connection string is required.");
            }
            if (string.IsNullOrWhiteSpace(opt.OpenAiApiKey))
            {
                errorMessageBuilder.AppendLine("OpenAI API key is required.");
            }
            if (!string.IsNullOrWhiteSpace(errorMessageBuilder.ToString()))
            {
                throw new ArgumentException(errorMessageBuilder.ToString());
            }

            services.AddSignalR().AddAzureSignalR(options =>
            {
                options.ConnectionString = opt.AzureSignalRConnectionString;
            });

            services.AddTransient<NotifyingMemoryStream>();

            services.AddScoped<ChatService>();

            services.AddScoped(c => new ChatClient(opt.OpenAiChatModelId, opt.OpenAiApiKey));

            if (opt.AddMemoryCacheChatMessageRepository)
            {
                services.AddMemoryCache();
                services.AddScoped<IChatMessageRepository>(c =>
                    new MemoryCacheChatMessageRepository(c.GetRequiredService<IMemoryCache>())
                        { SlidingExpiration = opt.SlidingExpiration });
            }
        }

        public static void AddAzureAiFoundryChatServices(this IServiceCollection services,
            Action<AzureAiFoundryChatServicesOptions> options)
        {
            var opt = new AzureAiFoundryChatServicesOptions();
            options?.Invoke(opt);

            var errorMessageBuilder = new StringBuilder();
            if (string.IsNullOrWhiteSpace(opt.AzureSignalRConnectionString))
            {
                errorMessageBuilder.AppendLine("Azure SignalR connection string is required.");
            }
            if (string.IsNullOrWhiteSpace(opt.OpenAiEndpoint))
            {
                errorMessageBuilder.AppendLine("OpenAI Endpoint key is required.");
            }
            if (string.IsNullOrWhiteSpace(opt.OpenAiKey))
            {
                errorMessageBuilder.AppendLine("OpenAI key is required.");
            }
            if (string.IsNullOrWhiteSpace(opt.DeploymentName))
            {
                errorMessageBuilder.AppendLine("Deployment name is required.");
            }
            if (!string.IsNullOrWhiteSpace(errorMessageBuilder.ToString()))
            {
                throw new ArgumentException(errorMessageBuilder.ToString());
            }

            services.AddSignalR().AddAzureSignalR(options =>
            {
                options.ConnectionString = opt.AzureSignalRConnectionString;
            });

            services.AddTransient<NotifyingMemoryStream>();

            services.AddScoped<ChatService>();

            AzureKeyCredential credential = new(opt.OpenAiKey!); // Add your OpenAI API key here  
            AzureOpenAIClient azureClient = new(
                new Uri(opt.OpenAiEndpoint!),
                credential
            );

            ChatCompletionOptions chatCompletionOptions = new ChatCompletionOptions()
            {
                Temperature = opt.Temperature,
                MaxOutputTokenCount = opt.MaxTokens,
                TopP = opt.TopP,
                FrequencyPenalty = opt.FrequencyPenalty,
                PresencePenalty = opt.PresencePenalty,
            };

            if (!string.IsNullOrWhiteSpace(opt.SearchEndpoint) && !string.IsNullOrWhiteSpace(opt.SearchIndex) 
                && !string.IsNullOrWhiteSpace(opt.SearchKey))
            {
#pragma warning disable AOAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                chatCompletionOptions.AddDataSource(new AzureSearchChatDataSource()
                {
                    Endpoint = new Uri(opt.SearchEndpoint),
                    IndexName = opt.SearchIndex,
                    Authentication = DataSourceAuthentication.FromApiKey(opt.SearchKey), // Add your Azure AI Search admin key here  
                });
#pragma warning restore AOAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            }

            services.AddSingleton(chatCompletionOptions);

            ChatClient chatClient = azureClient.GetChatClient(opt.DeploymentName);

            services.AddScoped(c => chatClient);

            if (opt.AddMemoryCacheChatMessageRepository)
            {
                services.AddMemoryCache();
                services.AddScoped<IChatMessageRepository>(c =>
                    new MemoryCacheChatMessageRepository(c.GetRequiredService<IMemoryCache>())
                        { SlidingExpiration = opt.SlidingExpiration });
            }
        }
    }
}
