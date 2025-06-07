
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.Services;
using Microsoft.AspNetCore.Mvc;
using ScenarioFramework;

namespace CapitalRequestAutomatedTesting.UI.Controllers
{
    public class WorkflowController : Controller
    {
        private readonly IWorkflowControllerService _workflowControllerService;
        private readonly ITestActionService _testActionService;

        public WorkflowController(
            IWorkflowControllerService workflowControllerService,
            ITestActionService testActionService)
        {
            _workflowControllerService = workflowControllerService;
            _testActionService = testActionService;
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
        public async Task<IActionResult> GetWorkflowDashboardActionsAsync(string id)
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

        [HttpGet]
        public async Task<IActionResult> RunFullScenario()
        {
            var scenario = new TestScenario
            {
                ScenarioId = Guid.NewGuid().ToString(),
                Name = "Delete Reviewer with Open Request",
                CreatedAt = DateTime.UtcNow,
                Steps = new List<ScenarioStep>
                {
                    new ScenarioStep { StepNumber = 1, Description = "Create and submit request", Action = _testActionService.CreateAndSubmitRequest },
                    new ScenarioStep { StepNumber = 2, Description = "Request more info", Action = _testActionService.RequestMoreInfo },
                    new ScenarioStep { StepNumber = 3, Description = "Delete reviewer", Action = _testActionService.DeleteReviewer },
                    new ScenarioStep { StepNumber = 4, Description = "Verify reviewers unlocked", Action = _testActionService.VerifyReviewersUnlocked }
                }
            };

            var runner = new ScenarioRunner();
            await runner.RunScenario(scenario);

            return Json(new
            {
                scenario.ScenarioId,
                scenario.Status,
                Steps = scenario.Steps.Select(s => new
                {
                    s.StepNumber,
                    s.Description,
                    s.Result?.Success,
                    s.Result?.Output,
                    s.Result?.ErrorMessage
                })
            });
        }


        [HttpGet]
        public IActionResult GetAvailableScenarios()
        {
             var scenarios = new List<object>
             {
             new { Id = "delete-reviewer", Name = "Delete Reviewer with Open Request" },
             new { Id = "approve-request", Name = "Approve Request with Multiple Reviewers" },
             new { Id = "escalate-request", Name = "Escalate Request to Manager" }
             };

            return Json(scenarios);
        }

        [HttpPost]
        public async Task<IActionResult> RunFullScenarioById([FromBody] string scenarioId)
        {
            TestScenario scenario = scenarioId switch
            {
                "delete-reviewer" => new TestScenario
                {
                    ScenarioId = Guid.NewGuid().ToString(),
                    Name = "Delete Reviewer with Open Request",
                    CreatedAt = DateTime.UtcNow,
                    Steps = new List<ScenarioStep>
            {
                new ScenarioStep { StepNumber = 1, Description = "Create and submit request", Action = _testActionService.CreateAndSubmitRequest },
                new ScenarioStep { StepNumber = 2, Description = "Request more info", Action = _testActionService.RequestMoreInfo },
                new ScenarioStep { StepNumber = 3, Description = "Delete reviewer", Action = _testActionService.DeleteReviewer },
                new ScenarioStep { StepNumber = 4, Description = "Verify reviewers unlocked", Action = _testActionService.VerifyReviewersUnlocked }
            }
                },
                _ => null
            };

            if (scenario == null)
                return BadRequest("Unknown scenario ID");

            var runner = new ScenarioRunner();
            await runner.RunScenario(scenario);

            return Json(new
            {
                scenario.ScenarioId,
                scenario.Status,
                Steps = scenario.Steps.Select(s => new
                {
                    s.StepNumber,
                    s.Description,
                    s.Result?.Success,
                    s.Result?.Output,
                    s.Result?.ErrorMessage
                })
            });
        }

        [HttpGet]
        public IActionResult Scenarios()
        {
            return View();
        }


        [HttpPost]
        public IActionResult RunScenario(string selectedScenario)
        {
            // Add logic to handle the selected scenario
            ViewBag.Message = $"You selected: {selectedScenario}";
            return View("Scenarios");
        }



    }
}
