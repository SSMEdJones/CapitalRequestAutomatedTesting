using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CapitalRequestAutomatedTesting.UI.Controllers
{
    public class WorkflowController : Controller
    {
        private readonly IWorkflowControllerService _workflowControllerService;

        public WorkflowController(IWorkflowControllerService workflowControllerService)
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
        public async Task<IActionResult> GetWorkflowDashboardActions(string id)
        {
            var actions = await _workflowControllerService.GetActionsFromWorkflowDashboardAsync(id);
            var actionModels = actions.Select(a => new WorkflowAction
            {
                Identifier = a.Identifier,
                ActionName = a.ActionName,
                ScenarioId = _workflowControllerService.GenerateScenarioId(a.Identifier, a.ActionName)
            }).ToList();

            return Json(actionModels);
        }

        [HttpGet]
        public async Task<IActionResult> RunScenario(string scenario, string id, string selectedAction)
        {
            try
            {
                var actions = (await _workflowControllerService.GetActionsFromWorkflowDashboardAsync(id))
                    .Where(x => x.ScenarioId == scenario)
                    .ToList();

                actions.ForEach(a => a.SelectedAction = selectedAction);

                WorkflowTestResult result;

                if (scenario.ToLower().Contains("verify"))
                {
                    var context = actions
                        .Select(x => new WorkflowTestContext
                        {
                            ReqId = x.ReqId,
                            Identifier = x.Identifier,
                            ButtonText = x.ActionName,
                            WaitForElementId = "btnRequestMoreInfo",
                            ImpersonatedUserId = x.SelectedAction, // Assuming SelectedAction is the impersonated user ID
                            SelectedAction = x.SelectedAction,
                            RequestedFrom = x.RequestedFrom,
                            RequestDetails = x.RequestDetails,
                            StepType = "Verify",
                            MethodName = $"RunLoad{x.ActionName.Replace(" ", string.Empty)}ButtonTest"
                        })
                        .FirstOrDefault();


                    result = await _workflowControllerService.RunLoadVerifyButtonTestAsync(context);
                }

                else
                {
                    return Json(new { success = false, message = $"Unknown scenario: {scenario}" });
                }

                return Json(new { success = result.Passed, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        

    }
}
