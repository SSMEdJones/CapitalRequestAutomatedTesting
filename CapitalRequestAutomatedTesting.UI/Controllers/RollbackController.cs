using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using CapitalRequestAutomatedTesting.UI.Services;
using CapitalRequestAutomatedTesting.UI.Services.Predictive;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CapitalRequestAutomatedTesting.UI.Controllers
{
    public class RollbackController : Controller
    {
        private readonly IRollbackService _rollbackService;
        private readonly IPredictiveScenarioService _predictiveScenarioService;
        private readonly IScenarioComparer _scenarioComparer;


        public RollbackController(IRollbackService rollbackService, IPredictiveScenarioService predictiveScenarioService, IScenarioComparer scenarioComparer)
        {
            _rollbackService = rollbackService;
            _scenarioComparer = scenarioComparer;
            _predictiveScenarioService = predictiveScenarioService;
        }

        public IActionResult Preview()
        {
            //left off here.  
            //    if (string.IsNullOrEmpty(scenarioId))
            //    {
            //        TempData["Error"] = "Missing scenario ID.";
            //        return RedirectToAction("Index", "Home");
            //    }

            //    var scenarioJson = TempData["ScenarioDetail"] as string;
            //    if (string.IsNullOrEmpty(scenarioJson))
            //    {
            //        TempData["Error"] = "Scenario data not found.";
            //        return RedirectToAction("Index", "Home");
            //    }

            //    var scenario = JsonConvert.DeserializeObject<ScenarioDetailsViewModel>(scenarioJson);
            //left off here need to create test for entry point
            var scenarioJson = TempData["Scenario"] as string;
            var scenario = JsonConvert.DeserializeObject<ScenarioDetailsViewModel>(scenarioJson);

            var original = scenario.OriginalData;
            var actual = scenario.ActualData;

            var scenarioComparisonResult = _scenarioComparer.CompareData(original, actual);

            scenarioComparisonResult.ScenarioId = scenario.ScenarioId;
            scenarioComparisonResult.ScenarioName = scenario.DisplayText;
            scenarioComparisonResult.SelectedProperties = new Dictionary<string, string>(scenario.SelectedProperties);
            scenarioComparisonResult.SeleniumComparisons = _scenarioComparer.CompareOutcomes(scenario.PredictedSeleniumOutcome.Expected, scenario.ActualSeleniumOutcome.Expected);
            //TODO develop schema for saving
            scenarioComparisonResult.Id = 1;

            //_scenarioMemoryCache.Save(scenarioComparisonResult.Id, scenarioComparisonResult);

            return View(scenarioComparisonResult);

        }
        //[HttpGet]
        //public async Task<IActionResult> Preview(string scenarioId)
        //{
        //    if (string.IsNullOrEmpty(scenarioId))
        //    {
        //        TempData["Error"] = "Missing scenario ID.";
        //        return RedirectToAction("Index", "Home");
        //    }

        //    var scenarioJson = TempData["ScenarioDetail"] as string;
        //    if (string.IsNullOrEmpty(scenarioJson))
        //    {
        //        TempData["Error"] = "Scenario data not found.";
        //        return RedirectToAction("Index", "Home");
        //    }

        //    var scenario = JsonConvert.DeserializeObject<ScenarioDetailsViewModel>(scenarioJson);

        //    if (scenario.ScenarioId != scenarioId)
        //    {
        //        TempData["Error"] = $"Scenario ID mismatch.";
        //        return RedirectToAction("Index", "Home");
        //    }

        //    //var scenarioDetail = await _predictiveScenarioService.GenerateScenarioDataAsync(scenario);
        //    //var predictiveMethods = scenarioDetail.PredictiveMethods;

        //    var candidates = await _rollbackService.ExecuteRollbackAsync(scenario);
        //    //var candidates = await _rollbackService.ExecuteRollbackAsync(predictiveMethods);

        //    var viewModels = candidates.Select(c => new RollbackCandidateViewModel
        //    {
        //        StepNumber = c.StepNumber,
        //        MethodName = c.MethodName,
        //        RollbackMethodName = c.RollbackMethodName,
        //        Description = c.Description,
        //        PredictiveData = c.PredictiveData,
        //        ActualData = c.ActualData,
        //        RollbackParameters = c.RollbackParameters,
        //        IsSelectedForRollback = c.IsSelectedForRollback
        //    }).ToList();

        //    var previewModel = new RollbackPreviewViewModel
        //    {
        //        ScenarioId = scenarioId,
        //        ScenarioName = scenario.DisplayText ?? "Scenario Preview",
        //        Candidates = viewModels,
        //        CanRollback = viewModels.Any()
        //    };

        //    return View(previewModel);
        //}

        // Show rollback preview UI
        //[HttpGet]
        //public async Task<IActionResult> Preview(string scenarioId)
        //{
        //    //debugging
        //    var scenario = new ScenarioDetailsViewModel
        //    {
        //        ProposalId = 2936,
        //        ScenarioId = "SCN002",
        //        PartialViewName = "_ReplyToRequest",
        //        DisplayText = "Reply to Request",
        //        RequestingGroupId = 4,
        //        ReplyingGroupId = 5,
        //        ReviewerId = 37807,
        //        RequestedInformation = "Supply Chain requesting more information from EPMO as Pam Shumway via Workflow Automated Testing - Request More Information Scenario.",
        //        ReturnedInformation = "EPMO replying to request for more information from Supply Chain as Gavin Harrell via Workflow Automated Testing - Reply to Request Scenario.",
        //        RequestedInfoId = 691,
        //        PredictiveCompletionStep = 0,
        //        CanExecuteActualSteps = true
        //    };
        //    var scenarioDetail = await _predictiveScenarioService.GenerateScenarioDataAsync(scenario);
        //    var predictiveMethods = scenarioDetail.PredictiveMethods;

        //    var candidates = await _rollbackService.ExecuteRollbackAsync(predictiveMethods);

        //    var viewModels = candidates.Select(c => new RollbackCandidateViewModel
        //    {
        //        StepNumber = c.StepNumber,
        //        MethodName = c.MethodName,
        //        RollbackMethodName = c.RollbackMethodName,
        //        Description = c.Description,
        //        PredictiveData = c.PredictiveData,
        //        ActualData = c.ActualData,
        //        RollbackParameters = c.RollbackParameters,
        //        IsSelectedForRollback = c.IsSelectedForRollback
        //    }).ToList();

        //    var previewModel = new RollbackPreviewViewModel
        //    {
        //        ScenarioId = scenarioId,
        //        ScenarioName = "Scenario XYZ", // or fetch from service
        //        Candidates = viewModels,
        //        CanRollback = viewModels.Any()
        //    };

        //    return View(previewModel);
        //}

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
