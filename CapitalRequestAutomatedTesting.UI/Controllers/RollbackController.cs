using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using CapitalRequestAutomatedTesting.UI.Services;
using CapitalRequestAutomatedTesting.UI.Services.Actual;
using CapitalRequestAutomatedTesting.UI.Services.Original;
using CapitalRequestAutomatedTesting.UI.Services.Predictive;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog;
using System.Diagnostics;
using System.Xml.Linq;

namespace CapitalRequestAutomatedTesting.UI.Controllers
{
    public class RollbackController : Controller
    {
        private readonly ILogger<RollbackController> _logger;

        private readonly IRollbackService _rollbackService;
        private readonly IPredictiveScenarioService _predictiveScenarioService;
        private readonly IScenarioComparer _scenarioComparer;

        //todo remove
        private readonly IActualScenarioService _actualScenarioService;
        private readonly IOriginalScenarioService _originalScenarioService;
        private readonly IPredictiveSeleniumService _predictiveSeleniumService;



        public RollbackController(ILogger<RollbackController> logger,
            IRollbackService rollbackService, IPredictiveScenarioService predictiveScenarioService, IScenarioComparer scenarioComparer
            ,IActualScenarioService actualScenarioService,
            IOriginalScenarioService originalScenarioService,
            IPredictiveSeleniumService predictiveSeleniumService
)
        {
            _logger = logger;
            _rollbackService = rollbackService;
            _scenarioComparer = scenarioComparer;
            _predictiveScenarioService = predictiveScenarioService;

            //todo remove

            _predictiveScenarioService = predictiveScenarioService;
            _actualScenarioService = actualScenarioService;
            _originalScenarioService = originalScenarioService;
            _predictiveSeleniumService = predictiveSeleniumService;

        }

        //private async Task<ScenarioDetailsViewModel> ProcessScenario(ScenarioDetailsViewModel scenario)
        //{

        //    var scenarioJson = JsonConvert.SerializeObject(scenario, Formatting.Indented,
        //    new JsonSerializerSettings
        //    {
        //        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        //    });
        //    Debug.WriteLine($"Scenario Contents:\n{scenarioJson}");

        //    // Step 1: Predictive Selenium
        //    var scenarioId = scenario.ScenarioId;

        //    scenario.PredictedSeleniumOutcome = await _predictiveSeleniumService.GenerateSeleniumOutcomeAsync(scenario);

        //    var completionStep = scenario.PredictiveCompletionStep;

        //    var stopwatch = Stopwatch.StartNew();

        //    // Step 2: Predictive Data (only if prediction succeeded)
        //    if (scenario.PredictedSeleniumOutcome.Success)
        //    {
        //        scenario.PredictiveData = await _predictiveScenarioService.GenerateScenarioDataAsync(scenario);
        //        scenario.OriginalData = await _originalScenarioService.GenerateScenarioDataAsync(scenario);
        //    }

        //    ////todo remove
        //    //return scenario;

        //    // Step 3: Actual Selenium — even if prediction failed (limited by completion step count)
        //    //scenario.ActualSeleniumOutcome = await _actualSeleniumService.GenerateSeleniumOutcomeAsync(scenario);

        //    // Step 4: Actual Data (only if prediction succeeded)
        //    //if (scenario.PredictedSeleniumOutcome.Success)
        //    //{
        //    scenario.ActualData = await _actualScenarioService.GenerateScenarioDataAsync(scenario);
        //    stopwatch.Stop();

        //    scenario.ActualData.ActualExecutionDuration = stopwatch.Elapsed;
        //    scenario.ActualData.ActualExecutionDurationMinutes = (int)Math.Ceiling(stopwatch.Elapsed.TotalMinutes);

        //    //TODO remove
        //    scenario.PredictedSeleniumOutcome.Success = false;
        //    //}

        //    return scenario;
        //}

        //public async Task<IActionResult> RunSelected()
        //{

        //    var modelJson = TempData["ScenarioModel"] as string;
        //    var model = JsonConvert.DeserializeObject<ScenarioFormViewModel>(modelJson);

        //    // Check if model.ScenarioDetails has data
        //    if (model.ScenarioDetails == null || !model.ScenarioDetails.Any())
        //    {
        //        // Log or debug here
        //        Debug.WriteLine("ScenarioDetails is empty");
        //    }


        //    var selectedScenarios = model.ScenarioDetails
        //    .Where(s => model.SelectedScenarioIds.Contains(s.ScenarioId))
        //    .ToList();

        //    var scenarioDetails = new List<ScenarioDetailsViewModel>();
        //    var scenarioDetail = new ScenarioDetailsViewModel();
        //    // Now you have full access to each selected scenario's form data
        //    foreach (var scenario in selectedScenarios)
        //    {

        //        var detail = await ProcessScenario(scenario);

        //        if (detail.PredictiveSeleniumFailed)
        //        {
        //            //TempData["Error"] = $"Predictive Selenium failed for scenario {detail.ScenarioId}.";
        //            TempData["ScenarioDetail"] = JsonConvert.SerializeObject(detail);

        //            return Preview();
        //            //return RedirectToAction("Preview", "Rollback", new { scenarioId = detail.ScenarioId });
        //        }

        //        scenarioDetails.Add(detail);

        //        // etc.
        //    }

        //    // Store them in TempData or session (TempData uses serialization)
        //    foreach (var detail in scenarioDetails)
        //    {
        //        TempData["Scenario"] = JsonConvert.SerializeObject(detail);

        //    }

        //    return RedirectToAction("ViewComparison");
        //}

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
            {                // Log the error - missing data
                return RedirectToAction("Index", "Home");
            }

            var original = scenario.OriginalData;            
            var current = scenario.ActualData;
            //var actual = scenario.PredictiveData;

            var scenarioComparisonResult = _scenarioComparer.CompareData(original, current);

            scenarioComparisonResult.ScenarioId = scenario.ScenarioId;
            scenarioComparisonResult.ScenarioName = scenario.DisplayText ?? "Unknown Scenario";
            scenarioComparisonResult.SelectedProperties = new Dictionary<string, string>(
                scenario.SelectedProperties ?? new Dictionary<string, string>());
            
            return View(scenarioComparisonResult);

        }

        
    }
}
