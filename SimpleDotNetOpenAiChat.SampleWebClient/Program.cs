using SimpleDotNetOpenAiChat.Extensions;
using SimpleDotNetOpenAiChat.Hubs;
using SimpleDotNetOpenAiChat.SampleWebClient.Hubs;

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

            builder.Services.AddSimpleDotNetOpenAiChatServices(opt =>
            {
                opt.AzureSignalRConnectionString = builder.Configuration["AzureSignalRConnectionString"];
                opt.OpenAiApiKey = builder.Configuration["OpenAiApiKey"];
                opt.AddMemoryCacheChatMessageRepository = true;
                opt.SlidingExpiration = TimeSpan.FromMinutes(30); // 30 minutes is default, but you can change this here.
                opt.OpenAiChatModelId = "gpt-4o-mini"; // "gpt-4o-mini" is default, but you can change this here.
            });

            var app = builder.Build();

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

            app.MapRazorPages();

            app.MapHub<MyChatHub>("/myChatHub");

            app.Run();
        }
    }
}
