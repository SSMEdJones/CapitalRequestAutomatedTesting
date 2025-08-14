using Microsoft.AspNetCore.Mvc;

namespace CapitalRequestAutomatedTesting.UI.Controllers
{
    public class RollbackController : Controller
    {
        private readonly IRollbackService _rollbackService;

        public RollbackController(IRollbackService rollbackService)
        {
            _rollbackService = rollbackService;
        }

        // Show rollback preview UI
        [HttpGet]
        public IActionResult Preview(Guid scenarioId)
        {
            //left off here
            var previewModel = _rollbackService.GetRollbackPreview(scenarioId);
            return View(previewModel); // returns a Razor view
        }

        // Execute rollback and redirect
        [HttpPost]
        public IActionResult Execute(Guid scenarioId)
        {
            //_rollbackService.ExecuteRollback(scenarioId);
            return RedirectToAction("Details", "Scenario", new { id = scenarioId });
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
