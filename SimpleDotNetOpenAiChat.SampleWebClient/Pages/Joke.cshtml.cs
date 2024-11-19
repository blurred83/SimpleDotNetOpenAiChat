using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SimpleDotNetOpenAiChat.SampleWebClient.Pages
{
    public class JokeModel : PageModel
    {
        private readonly ILogger<JokeModel> _logger;
        public string JokeSessionId { get; set; }

        public JokeModel(ILogger<JokeModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            // Check if the session ID already exists
            JokeSessionId = HttpContext.Session.GetString("JokeSessionId");
            if (string.IsNullOrEmpty(JokeSessionId))
            {
                // Generate and store a new session ID
                JokeSessionId = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("JokeSessionId", JokeSessionId);
            }
        }
    }
}
