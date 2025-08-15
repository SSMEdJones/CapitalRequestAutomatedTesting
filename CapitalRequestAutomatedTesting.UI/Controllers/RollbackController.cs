using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using CapitalRequestAutomatedTesting.UI.Services.Predictive;
using Microsoft.AspNetCore.Mvc;

namespace CapitalRequestAutomatedTesting.UI.Controllers
{
    public class RollbackController : Controller
    {
        private readonly IRollbackService _rollbackService;
        private readonly IPredictiveScenarioService _predictiveScenarioService;

        public RollbackController(IRollbackService rollbackService, IPredictiveScenarioService predictiveScenarioService)
        {
            _rollbackService = rollbackService;
            _predictiveScenarioService = predictiveScenarioService;
        }

        // Show rollback preview UI
        [HttpGet]
        public async Task<IActionResult> Preview(string scenarioId)
        {
            //debugging
            var scenario = new ScenarioDetailsViewModel
            {
                ProposalId = 2936,
                ScenarioId = "SCN002",
                PartialViewName = "_ReplyToRequest",
                DisplayText = "Reply to Request",
                RequestingGroupId = 4,
                ReplyingGroupId = 5,
                ReviewerId = 37807,
                RequestedInformation = "Supply Chain requesting more information from EPMO as Pam Shumway via Workflow Automated Testing - Request More Information Scenario.",
                ReturnedInformation = "EPMO replying to request for more information from Supply Chain as Gavin Harrell via Workflow Automated Testing - Reply to Request Scenario.",
                RequestedInfoId = 691,
                PredictiveCompletionStep = 0,
                CanExecuteActualSteps = true
            };
            var scenarioDetail = await _predictiveScenarioService.GenerateScenarioDataAsync(scenario);
            var predictiveMethods = scenarioDetail.PredictiveMethods;

            var candidates = await _rollbackService.ExecuteRollbackAsync(predictiveMethods);

            var viewModels = candidates.Select(c => new RollbackCandidateViewModel
            {
                StepNumber = c.StepNumber,
                MethodName = c.MethodName,
                RollbackMethodName = c.RollbackMethodName,
                Description = c.Description,
                PredictiveData = c.PredictiveData,
                ActualData = c.ActualData,
                RollbackParameters = c.RollbackParameters,
                IsSelectedForRollback = c.IsSelectedForRollback
            }).ToList();

            var previewModel = new RollbackPreviewViewModel
            {
                ScenarioId = scenarioId,
                ScenarioName = "Scenario XYZ", // or fetch from service
                Candidates = viewModels,
                CanRollback = viewModels.Any()
            };

            return View(previewModel);
        }

        // Execute rollback and redirect
        [HttpPost]
        public IActionResult Execute(Guid scenarioId)
        {
            //_rollbackService.ExecuteRollback(scenarioId);
            return RedirectToAction("Details", "Scenario", new { id = scenarioId });
        }

        [HttpGet]
        public IActionResult Step(Guid scenarioId, int stepIndex = 0)
        {
            //var candidates = _rollbackService.GetRollbackCandidates(scenarioId);
            //if (stepIndex < 0 || stepIndex >= candidates.Count)
            //    return RedirectToAction("Step", new { scenarioId, stepIndex = 0 });

            //var model = candidates[stepIndex];
            //ViewBag.StepIndex = stepIndex;
            //ViewBag.TotalSteps = candidates.Count;
            //ViewBag.ScenarioId = scenarioId;
            return View("Step");
            //return View("Step", model);
        }

        [HttpPost]
        public IActionResult ConfirmStep(Guid scenarioId, int stepIndex, RollbackCandidateViewModel model)
        {
            //_rollbackService.UpdateCandidateSelection(scenarioId, stepIndex, model.IsSelectedForRollback);
            return RedirectToAction("Step", new { scenarioId, stepIndex = stepIndex + 1 });
        }

        


        public IActionResult Index()
        {
            return View();
        }
    }
}
