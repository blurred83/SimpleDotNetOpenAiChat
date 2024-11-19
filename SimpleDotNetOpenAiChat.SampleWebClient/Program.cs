using Microsoft.Extensions.Options;
using SimpleDotNetOpenAiChat.Extensions;
using SimpleDotNetOpenAiChat.Hubs;
using SimpleDotNetOpenAiChat.Models;
using SimpleDotNetOpenAiChat.Repository;
using SimpleDotNetOpenAiChat.SampleWebClient.Hubs;
using SimpleDotNetOpenAiChat.Services;
using SimpleDotNetOpenAiChat.Utilities;

namespace SimpleDotNetOpenAiChat.SampleWebClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddEnvironmentVariables();

            // Add services to the container.
            builder.Services.AddRazorPages();

            // Add session services
            builder.Services.AddDistributedMemoryCache(); // Required for session state
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true; // For GDPR compliance
            });

            builder.Services.AddSimpleDotNetOpenAiChatServices(opt =>
            {
                opt.AzureSignalRConnectionString = builder.Configuration["AzureSignalRConnectionString"];
                opt.OpenAiApiKey = builder.Configuration["OpenAiApiKey"];
                opt.AddMemoryCacheChatMessageRepository = true;
                opt.SlidingExpiration = TimeSpan.FromMinutes(30); // 30 minutes is default, but you can change this here.
                opt.OpenAiChatModelId = "gpt-4o-mini"; // "gpt-4o-mini" is default, but you can change this here.
            });

            builder.Services.Configure<ChatHubConfig>("TechSupport", options =>
            {
                options.SystemMessage = @"You are a general technical support assistant for Windows users. Your role is to help troubleshoot, configure, and optimize Windows systems and related software. 

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

    Your goal is to ensure users can confidently resolve their issues while learning helpful tips along the way.
    Don't reply to questions about topics other than tech support.";
                options.StreamResponse = true;
                options.StreamMessageBuffer = 5;
            });

            builder.Services.Configure<ChatHubConfig>("JokeBot", options =>
            {
                options.SystemMessage =
                    @"You are a joke telling bot. Your job is to tell jokes about whatever the user wants. 
Don't talk about anything else; just tell jokes.";
                options.StreamResponse = true;
                options.StreamMessageBuffer = 5;
            });

            // Register hubs with their respective configurations
            builder.Services.AddTransient<TechSupportHub>(sp =>
            {
                var chatMessageRepository = sp.GetRequiredService<IChatMessageRepository>();
                var chatService = sp.GetRequiredService<ChatService>();
                var notifyingMemoryStream = sp.GetRequiredService<NotifyingMemoryStream>();
                var config = sp.GetRequiredService<IOptionsSnapshot<ChatHubConfig>>().Get("TechSupport");
                return new TechSupportHub(chatMessageRepository, chatService, notifyingMemoryStream, config);
            });

            builder.Services.AddTransient<JokeHub>(sp =>
            {
                var chatMessageRepository = sp.GetRequiredService<IChatMessageRepository>();
                var chatService = sp.GetRequiredService<ChatService>();
                var notifyingMemoryStream = sp.GetRequiredService<NotifyingMemoryStream>();
                var config = sp.GetRequiredService<IOptionsSnapshot<ChatHubConfig>>().Get("JokeBot");
                return new JokeHub(chatMessageRepository, chatService, notifyingMemoryStream, config);
            });

            var app = builder.Build();

            app.MapGet("/", context =>
            {
                context.Response.Redirect("/Joke");
                return Task.CompletedTask;
            });

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            // Enable session middleware
            app.UseSession();

            app.MapRazorPages();

            app.MapHub<TechSupportHub>("/techSupportHub");
            app.MapHub<JokeHub>("/jokeHub");

            app.Run();
        }
    }
}
