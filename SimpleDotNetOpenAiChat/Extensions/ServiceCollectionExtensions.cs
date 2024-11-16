using System.Text;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.Chat;
using SimpleDotNetOpenAiChat.Models;
using SimpleDotNetOpenAiChat.Repository;
using SimpleDotNetOpenAiChat.Services;
using SimpleDotNetOpenAiChat.Utilities;

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
            Action<SimpleDotNetOpenAiChatServicesOptions> options)
        {
            var opt = new SimpleDotNetOpenAiChatServicesOptions();
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
    }
}
