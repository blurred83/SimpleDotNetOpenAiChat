using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SimpleDotNetOpenAiChat.SampleWebClient.Pages
{
    public class StoreModel : PageModel
    {
        private readonly ILogger<StoreModel> _logger;
        public string StoreSessionId { get; set; }

        public StoreModel(ILogger<StoreModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            // Check if the session ID already exists
            StoreSessionId = HttpContext.Session.GetString("StoreSessionId");
            if (string.IsNullOrEmpty(StoreSessionId))
            {
                // Generate and store a new session ID
                StoreSessionId = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("StoreSessionId", StoreSessionId);
            }
        }
    }
}
