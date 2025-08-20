using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using CapitalRequestAutomatedTesting.UI.Services;
using CapitalRequestAutomatedTesting.UI.Services.Actual;
using CapitalRequestAutomatedTesting.UI.Services.Original;
using CapitalRequestAutomatedTesting.UI.Services.Predictive;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NLog;
using System.Diagnostics;

namespace CapitalRequestAutomatedTesting.UI.Controllers
{
    public class RollbackController : Controller
    {
        private readonly IRollbackService _rollbackService;
        private readonly IPredictiveScenarioService _predictiveScenarioService;
        private readonly IScenarioComparer _scenarioComparer;

        //todo remove
        private readonly IActualScenarioService _actualScenarioService;
        private readonly IOriginalScenarioService _originalScenarioService;
        private readonly IPredictiveSeleniumService _predictiveSeleniumService;



        public RollbackController(IRollbackService rollbackService, IPredictiveScenarioService predictiveScenarioService, IScenarioComparer scenarioComparer
            ,IActualScenarioService actualScenarioService,
            IOriginalScenarioService originalScenarioService,
            IPredictiveSeleniumService predictiveSeleniumService,
            IActualSeleniumService actualSeleniumService
)
        {
            _rollbackService = rollbackService;
            _scenarioComparer = scenarioComparer;
            _predictiveScenarioService = predictiveScenarioService;

            //todo remove

            _predictiveScenarioService = predictiveScenarioService;
            _actualScenarioService = actualScenarioService;
            _originalScenarioService = originalScenarioService;
            _predictiveSeleniumService = predictiveSeleniumService;

        }

        private async Task<ScenarioDetailsViewModel> ProcessScenario(ScenarioDetailsViewModel scenario)
        {

            var scenarioJson = JsonConvert.SerializeObject(scenario, Formatting.Indented,
            new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            Debug.WriteLine($"Scenario Contents:\n{scenarioJson}");

            // Step 1: Predictive Selenium
            var scenarioId = scenario.ScenarioId;

            scenario.PredictedSeleniumOutcome = await _predictiveSeleniumService.GenerateSeleniumOutcomeAsync(scenario);

            var completionStep = scenario.PredictiveCompletionStep;

            var stopwatch = Stopwatch.StartNew();

            // Step 2: Predictive Data (only if prediction succeeded)
            if (scenario.PredictedSeleniumOutcome.Success)
            {
                scenario.PredictiveData = await _predictiveScenarioService.GenerateScenarioDataAsync(scenario);
                scenario.OriginalData = await _originalScenarioService.GenerateScenarioDataAsync(scenario);
            }

            ////todo remove
            //return scenario;

            // Step 3: Actual Selenium — even if prediction failed (limited by completion step count)
            //scenario.ActualSeleniumOutcome = await _actualSeleniumService.GenerateSeleniumOutcomeAsync(scenario);

            // Step 4: Actual Data (only if prediction succeeded)
            //if (scenario.PredictedSeleniumOutcome.Success)
            //{
            scenario.ActualData = await _actualScenarioService.GenerateScenarioDataAsync(scenario);
            stopwatch.Stop();

            scenario.ActualData.ActualExecutionDuration = stopwatch.Elapsed;
            scenario.ActualData.ActualExecutionDurationMinutes = (int)Math.Ceiling(stopwatch.Elapsed.TotalMinutes);

            //TODO remove
            scenario.PredictedSeleniumOutcome.Success = false;
            //}

            return scenario;
        }

        public async Task<IActionResult> RunSelected()
        {

            var modelJson = TempData["ScenarioModel"] as string;
            var model = JsonConvert.DeserializeObject<ScenarioFormViewModel>(modelJson);

            // Check if model.ScenarioDetails has data
            if (model.ScenarioDetails == null || !model.ScenarioDetails.Any())
            {
                // Log or debug here
                Debug.WriteLine("ScenarioDetails is empty");
            }


            var selectedScenarios = model.ScenarioDetails
            .Where(s => model.SelectedScenarioIds.Contains(s.ScenarioId))
            .ToList();

            var scenarioDetails = new List<ScenarioDetailsViewModel>();
            var scenarioDetail = new ScenarioDetailsViewModel();
            // Now you have full access to each selected scenario's form data
            foreach (var scenario in selectedScenarios)
            {

                var detail = await ProcessScenario(scenario);

                if (detail.PredictiveSeleniumFailed)
                {
                    //TempData["Error"] = $"Predictive Selenium failed for scenario {detail.ScenarioId}.";
                    TempData["ScenarioDetail"] = JsonConvert.SerializeObject(detail);

                    return Preview();
                    //return RedirectToAction("Preview", "Rollback", new { scenarioId = detail.ScenarioId });
                }

                scenarioDetails.Add(detail);

                // etc.
            }

            // Store them in TempData or session (TempData uses serialization)
            foreach (var detail in scenarioDetails)
            {
                TempData["Scenario"] = JsonConvert.SerializeObject(detail);

            }

            return RedirectToAction("ViewComparison");
        }

        public IActionResult Preview()
        {
            var scenarioJson = TempData["ScenarioDetail"] as string;
            if (string.IsNullOrEmpty(scenarioJson))
            {
                // Handle missing data
                return RedirectToAction("Index", "Home");
            }

            var scenario = JsonConvert.DeserializeObject<ScenarioDetailsViewModel>(scenarioJson);
            
            // Make sure we have the required data
            if (scenario == null || scenario.OriginalData == null || scenario.PredictiveData == null)
            {
                // Log the error - missing data
                return RedirectToAction("Index", "Home");
            }

            var original = scenario.OriginalData;
            var actual = scenario.PredictiveData;

            var scenarioComparisonResult = _scenarioComparer.CompareData(original, actual);

            scenarioComparisonResult.ScenarioId = scenario.ScenarioId;
            scenarioComparisonResult.ScenarioName = scenario.DisplayText ?? "Unknown Scenario";
            scenarioComparisonResult.SelectedProperties = new Dictionary<string, string>(
                scenario.SelectedProperties ?? new Dictionary<string, string>());
            
            // Add null check for selenium outcomes
            if (scenario.PredictedSeleniumOutcome?.Expected != null && 
                scenario.ActualSeleniumOutcome?.Expected != null)
            {
                scenarioComparisonResult.SeleniumComparisons = 
                    _scenarioComparer.CompareOutcomes(
                        scenario.PredictedSeleniumOutcome.Expected, 
                        scenario.ActualSeleniumOutcome.Expected);
            }
            else
            {
                scenarioComparisonResult.SeleniumComparisons = new List<SeleniumStepComparison>();
            }

            //left off here not loading view
            return View("Preview");
            //return View("Preview",scenarioComparisonResult);

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
