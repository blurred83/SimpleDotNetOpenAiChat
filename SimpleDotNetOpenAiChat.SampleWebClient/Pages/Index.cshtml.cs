using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SimpleDotNetOpenAiChat.SampleWebClient.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public string SessionId { get; set; }

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            // Check if the session ID already exists
            SessionId = HttpContext.Session.GetString("SessionId");
            if (string.IsNullOrEmpty(SessionId))
            {
                // Generate and store a new session ID
                SessionId = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("SessionId", SessionId);
            }
        }
    }
}
