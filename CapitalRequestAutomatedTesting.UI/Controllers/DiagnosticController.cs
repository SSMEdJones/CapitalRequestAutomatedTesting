using Microsoft.AspNetCore.Mvc;

namespace CapitalRequestAutomatedTesting.UI.Controllers
{
    public class DiagnosticController : Controller
    {
        private readonly ILogger<DiagnosticController> _logger;

        public DiagnosticController(ILogger<DiagnosticController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            _logger.LogInformation("Diagnostic page accessed at {Time}", DateTime.UtcNow);
            return View();
        }

        [HttpGet]
        public IActionResult Status()
        {
            var uptime = DateTime.Now - System.Diagnostics.Process.GetCurrentProcess().StartTime;
            return Json(new 
            { 
                status = "alive", 
                uptime = uptime.ToString(@"mm\:ss"),
                timestamp = DateTime.UtcNow,
                processId = Environment.ProcessId
            });
        }
    }
}