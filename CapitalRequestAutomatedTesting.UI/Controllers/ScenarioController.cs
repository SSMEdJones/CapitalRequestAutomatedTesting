using AutoMapper;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using CapitalRequestAutomatedTesting.UI.Services;
using DinkToPdf;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace CapitalRequestAutomatedTesting.UI.Controllers
{
    public class ScenarioController : Controller
    {
        private readonly IScenarioControllerService _scenarioControllerService;
        private readonly IWorkflowControllerService _workflowControllerService;
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly IPredictiveScenarioService _predictiveScenarioService;
        private readonly IActualScenarioService _actualScenarioService;
        private readonly IPredictiveSeleniumService _predictiveSeleniumService;
        private readonly IActualSeleniumService _actualSeleniumService;
        private readonly IViewRenderService _viewRenderService;
        private readonly IScenarioMemoryCache _scenarioMemoryCache;
        private readonly IScenarioComparer _scenarioComparer;
        private readonly IMapper _mapper;
        private readonly ScenarioViewModelBuilder _viewModelBuilder;

        public ScenarioController(
            IScenarioControllerService scenarioControllerService,
            IWorkflowControllerService workflowControllerService,
            ICapitalRequestServices capitalRequestServices,
            IPredictiveScenarioService predictiveScenarioService,
            IActualScenarioService actualScenarioService,
            IPredictiveSeleniumService predictiveSeleniumService,
            IActualSeleniumService actualSeleniumService,
            IViewRenderService viewRenderService,
            IScenarioMemoryCache scenarioMemoryCache,
            ScenarioViewModelBuilder viewModelBuilder,
            IScenarioComparer scenarioComparer,
            IMapper mapper)
        {
            _scenarioControllerService = scenarioControllerService;
            _workflowControllerService = workflowControllerService;
            _capitalRequestServices = capitalRequestServices;
            _predictiveScenarioService = predictiveScenarioService;
            _actualScenarioService = actualScenarioService;
            _predictiveSeleniumService = predictiveSeleniumService;
            _actualSeleniumService = actualSeleniumService;
            _viewRenderService = viewRenderService;
            _scenarioMemoryCache = scenarioMemoryCache;
            _viewModelBuilder = viewModelBuilder;
            _scenarioComparer = scenarioComparer;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var formModel = await _scenarioControllerService.GenerateScenarioFormViewModel(null);

            return View(formModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetRequestIds()
        {
            var requestList = await _scenarioControllerService.GetRequestSelectListAsync();
            return Json(requestList);
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromForm] ScenarioFormViewModel model, string actionType)
        {

            if (actionType == "RunSelected")
            {
                // Handle the selected scenarios
                var selectedIds = model.SelectedScenarioIds;

                model.ScenarioDetails.ForEach(x =>
                {
                    x.ProposalId = model.RequestId ?? 0;
                });

                TempData["ScenarioModel"] = JsonConvert.SerializeObject(model);

                return RedirectToAction("RunSelected", new { ids = selectedIds });
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetScenariosForRequest(int requestId)
        {
            var model = await _scenarioControllerService.GenerateScenarioFormViewModel(requestId);
            return PartialView("_ScenarioFormPartial", model); // or whatever your partial is called
        }

        [HttpGet]
        public async Task<IActionResult> GetReviewerDetails(int reviewerId, int proposalId, int requestingGroupId, int targetGroupId, string displayText)
        {
            var reviewer = await _scenarioControllerService.GetReviewerByIdAsync(reviewerId);
            var requestingGroup = await _scenarioControllerService.GetReviewerGroupByIdAsync(requestingGroupId);
            var targetGroup = await _scenarioControllerService.GetReviewerGroupByIdAsync(targetGroupId);

            var requestedInfo = $"{requestingGroup.Name} requesting more information from {targetGroup.Name} via Workflow Automated Testing - {displayText} Scenario.";

            return Json(new
            {
                reviewerEmail = reviewer.Email,
                reviewerUserId = reviewer.UserId,
                requestedInformation = requestedInfo
            });
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

            //TODO remove after debugging
            model.SelectedScenarioIds.Add("SCN001");

            var selectedScenarios = model.ScenarioDetails
            .Where(s => model.SelectedScenarioIds.Contains(s.ScenarioId))
            .ToList();

            var scenarioDetails = new List<ScenarioDetailsViewModel>();
            var scenarioDetail = new ScenarioDetailsViewModel();
            // Now you have full access to each selected scenario's form data
            foreach (var scenario in selectedScenarios)
            {
                scenarioDetail = await ProcessScenario(scenario);
                scenarioDetails.Add(scenarioDetail);

                // etc.
            }

            // Store them in TempData or session (TempData uses serialization)
            foreach (var detail in scenarioDetails)
            {
                TempData["Scenario"] = JsonConvert.SerializeObject(detail);

            }

            return RedirectToAction("ViewComparison");
        }

        public IActionResult ViewComparison()
        {

            var scenarioJson = TempData["Scenario"] as string;
            var scenario = JsonConvert.DeserializeObject<ScenarioDetailsViewModel>(scenarioJson);

            var predictive = scenario.PredictiveData;
            var actual = scenario.ActualData;

            var scenarioComparisonResult = _scenarioComparer.CompareData(predictive, actual);

            scenarioComparisonResult.ScenarioId = scenario.ScenarioId;
            scenarioComparisonResult.ScenarioName = scenario.DisplayText;
            scenarioComparisonResult.SelectedProperties = new Dictionary<string, string>(scenario.SelectedProperties);
            scenarioComparisonResult.SeleniumComparisons = _scenarioComparer.CompareOutcomes(scenario.PredictedSeleniumOutcome.Expected, scenario.ActualSeleniumOutcome.Expected);
            //TODO develope schema for actual retult saving
            scenarioComparisonResult.Id = 1;

            _scenarioMemoryCache.Save(scenarioComparisonResult.Id, scenarioComparisonResult);

            return View(scenarioComparisonResult);

        }

        private async Task<ScenarioDetailsViewModel> ProcessScenario(ScenarioDetailsViewModel scenario)
        {
            //var runner = new ScenarioSeleniumRunner(_actualSeleniumService);
            //var outcome = await runner.RunScenarioAsync(scenario);


            // Predictive data
            scenario.PredictiveData = await _predictiveScenarioService.GenerateScenarioDataAsync(scenario);

            // Run Selenium Scenario
            scenario.PredictedSeleniumOutcome = await _predictiveSeleniumService.GenerateSeleniumOutcomeAsync(scenario);
            scenario.ActualSeleniumOutcome = await _actualSeleniumService.GenerateSeleniumOutcomeAsync(scenario);

            // Retrieve data
            scenario.ActualData = await _actualScenarioService.GenerateScenarioDataAsync(scenario);

            //Compare Data
            //scenario.ComparisonResult = _scenarioComparer.CompareData(scenario.PredictiveData, scenario.ActualData);
            //scenario.SeleniumComparisons = _scenarioComparer.CompareOutcomes(scenario.PredictedSeleniumOutcome.Expected, scenario.ActualSeleniumOutcome.Expected);

            return scenario;
        }


        [HttpGet]
        public async Task<IActionResult> LoadScenarioPartial(string scenarioId, int requestId)
        {
            var formModel = await _scenarioControllerService.GenerateScenarioFormViewModel(requestId);
            var detail = formModel.ScenarioDetails.FirstOrDefault(s => s.ScenarioId == scenarioId);

            if (detail == null)
                return NotFound();

            return PartialView(detail.PartialViewName, detail);
        }

        [HttpGet]
        public IActionResult LoadScenarioView(string scenarioId, string requestId)
        {
            //ViewBag.RequestId = requestId;
            var viewName = _scenarioControllerService.GetScenarioViewName(scenarioId);
            return PartialView(viewName);
        }

        //[HttpGet]
        //public async Task<IActionResult> GetRequestIds()
        //{
        //    await _workflowControllerService.InitializeDashboardItemsAsync();
        //    var dashboardItems = await _workflowControllerService.GetDashboardItemsFromApiAsync();
        //    var ids = dashboardItems.Select(item => item.ReqId).Distinct().ToList();
        //    var result = ids.Select(id => new { id, name = $"{id}" });

        //    return Json(result);
        //}

        [HttpGet]
        public async Task<JsonResult> Scenarios()
        {
            var scenarios = new List<object>
            {
                new { id = "SCN001", name = "Request More Information" },
                new { id = "SCN002", name = "Reply to Request" },
                new { id = "SCN003", name = "Verify" },
                new { id = "SCN004", name = "Approve WBS" }
            };

            return Json(scenarios);
        }


        [HttpGet]
        public async Task<JsonResult> GetTargetGroupsAndReviewers(int proposalId, int requestingGroupId)
        {
            var targetGroups = await _scenarioControllerService.GetTargetGroupsByRequestIdAsync(proposalId, requestingGroupId);
            var reviewers = await _scenarioControllerService.GetReviewersByRequestingGroupAsync(proposalId, requestingGroupId);

            return Json(new
            {
                targetGroups,
                reviewers
            });
        }

        //public async Task<IActionResult> PrintScenarioPdf(int scenarioId)
        public async Task<IActionResult> PrintScenarioPdf(int id)
        {
            var scenario = _scenarioMemoryCache.Get(id);
            //var scenario = await _scenarioControllerService.GetScenarioByIdAsync(scenarioId);
            string htmlContent = await _viewRenderService.RenderToStringAsync("Scenario/ViewComparison", scenario);

            var doc = new HtmlToPdfDocument
            {
                GlobalSettings = new GlobalSettings
                {
                    PaperSize = PaperKind.A4,
                    Orientation = Orientation.Portrait,
                    DocumentTitle = "Scenario Report",
                    Margins = new MarginSettings
                    {
                        Top = 20,
                        Bottom = 20,
                        Left = 15,
                        Right = 15
                    }
                }
            };

            doc.Objects.Add(new ObjectSettings
            {
                HtmlContent = htmlContent,
                WebSettings = new WebSettings
                {
                    DefaultEncoding = "utf-8",
                    EnableIntelligentShrinking = false
                },
            });

            var pdf = new SynchronizedConverter(new PdfTools()).Convert(doc);
            return File(pdf, "application/pdf", "ScenarioReport.pdf");
        }

        private void LogModelErrors(string contextLabel)
        {
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                if (state.Errors.Any())
                {
                    Debug.WriteLine($"❌ ModelState error for '{key}':");
                    foreach (var error in state.Errors)
                    {
                        Debug.WriteLine($"In {contextLabel} → {error.ErrorMessage}");
                    }
                }
            }
        }
        //[HttpGet]
        //public IActionResult LoadScenarioView(string scenarioId)
        //{
        //    return scenarioId switch
        //    {
        //        "SCN001" => PartialView("_RequestMoreInfo"),
        //        "SCN002" => PartialView("_ReplyToRequest"),
        //        "SCN003" => PartialView("_Verify"),
        //        "SCN004" => PartialView("_ApproveWBS"),
        //        _ => PartialView("_DefaultScenario")
        //    };



        //[HttpPost]
        //public IActionResult RunSelected(ScenarioFormViewModel model)
        //{

        //    // Check if model.ScenarioDetails has data
        //    if (model.ScenarioDetails == null || !model.ScenarioDetails.Any())
        //    {
        //        // Log or debug here
        //        Debug.WriteLine("ScenarioDetails is empty");
        //    }


        //    foreach (var key in Request.Form.Keys)
        //    {
        //        Console.WriteLine($"{key}: {Request.Form[key]}");
        //    }

        //    var selectedScenarios = model.ScenarioDetails
        //    .Where(s => model.SelectedScenarioIds.Contains(s.ScenarioId))
        //    .ToList();

        //    // Now you have full access to each selected scenario's form data
        //    foreach (var scenario in selectedScenarios)
        //    {
        //        // Process each scenario
        //        var requestingGroupId = scenario.RequestingGroupId;
        //        var targetGroupId = scenario.TargetGroupId;
        //        var reviewerId = scenario.ReviewerId;
        //        var message = scenario.Message;
        //        // etc.
        //    }

        //    //ViewBag.Message = $"You selected: {string.Join(", ", selectedScenarios.Select(s => s.ScenarioId))}";
        //    return View("Index", model);
        //}

    }
}