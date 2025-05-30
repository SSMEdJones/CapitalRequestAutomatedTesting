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
        public async Task<IActionResult> RunScenario(string scenario, string id, string selectedAction)
        {
            try
            {
                var actions = _workflowControllerService.GetActionsFromWorkflowDashboard(id)
                    .Where(x => x.ScenarioId == scenario)
                    .ToList();

                actions.ForEach(a => a.SelectedAction = selectedAction);
                _workflowControllerService.RegisterWorkflowActions(actions);

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

                    //var context = new WorkflowTestContext
                    //{
                    //    ReqId = action,
                    //    Identifier = ,
                    //    // You can hardcode or later pass these values
                    //    ButtonText = "Verify",
                    //    WaitForElementId = "btnRequestMoreInfo"
                    //};

                    result = _workflowControllerService.RunLoadVerifyButtonTest(context);
                }
                else if (_workflowControllerService._scenarioMap.TryGetValue(scenario, out var testFunc))
                {
                    result = testFunc.Invoke(id);
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

        //[HttpGet]

        //public async Task<IActionResult> RunScenario(string scenario, string id, string selectedAction)
        //{
        //    try
        //    {
        //        var actions = _workflowControllerService.GetActionsFromWorkflowDashboard(id)
        //            .Where(x => x.ScenarioId == scenario)
        //            .ToList();

        //        actions.ForEach(a => a.SelectedAction = selectedAction);
        //        _workflowControllerService.RegisterWorkflowActions(actions);
        //        //_workflowControllerService._workflowActions =  _workflowControllerService.GetWorkflowActionsFromApiAsync(Convert.ToInt32(id)).Result;

        //        if (!_workflowControllerService._scenarioMap.TryGetValue(scenario, out var testFunc))
        //        {
        //            return Json(new { success = false, message = $"Unknown scenario: {scenario}" });
        //        }

        //        var result = testFunc.Invoke(id);

        //        return Json(new { success = result.Passed, message = result.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = $"Error: {ex.Message}" });
        //    }
        //}

    }
}
