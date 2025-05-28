using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CapitalRequestAutomatedTesting.UI.Controllers
{
    public class WorkflowController : Controller
    {
        private readonly WorkflowControllerService _workflowControllerService;

        public WorkflowController(WorkflowControllerService workflowControllerService)
        {
            _workflowControllerService = workflowControllerService;
        }
        // GET: WorkflowController
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetRequestIds()
        {
            await _workflowControllerService.InitializeDashboardItemsAsync();
            var dashboardItems = await _workflowControllerService.GetDashboardItemsFromApiAsync();
            var ids = dashboardItems.Select(item => item.ReqId).Distinct().ToList();
            var result = ids.Select(id => new { id, name = $"{id}" });

            return Json(result);
        }

        [HttpGet]
        public IActionResult GetWorkflowDashboardActions(string id)
        {
            var actions = _workflowControllerService.GetActionsFromWorkflowDashboard(id);

            _workflowControllerService.RegisterWorkflowActions(actions);

            var actionModels = actions.Select(a => new WorkflowAction
            {
                Identifier = a.Identifier,
                ActionName = a.ActionName,
                ScenarioId = _workflowControllerService.GenerateScenarioId(a.Identifier, a.ActionName)
            }).ToList();

            return Json(actionModels);
        }
        [HttpGet]
        public IActionResult RunScenario(string scenario, string id)
        {
            try
            {
                if (!_workflowControllerService._scenarioMap.TryGetValue(scenario, out var testFunc))
                {
                    return Json(new { success = false, message = $"Unknown scenario: {scenario}" });
                }

                var result = testFunc.Invoke(id);

                return Json(new { success = result.Passed, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
        
    }
}
