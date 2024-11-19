using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SimpleDotNetOpenAiChat.SampleWebClient.Pages
{
    public class TechSupportModel : PageModel
    {
        private readonly ILogger<TechSupportModel> _logger;
        public string TechSupportSessionId { get; set; }

        public TechSupportModel(ILogger<TechSupportModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            // Check if the session ID already exists
            TechSupportSessionId = HttpContext.Session.GetString("TechSupportSessionId");
            if (string.IsNullOrEmpty(TechSupportSessionId))
            {
                // Generate and store a new session ID
                TechSupportSessionId = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("TechSupportSessionId", TechSupportSessionId);
            }
        }
    }
}
