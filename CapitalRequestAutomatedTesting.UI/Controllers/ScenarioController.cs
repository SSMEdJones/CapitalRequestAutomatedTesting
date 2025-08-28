using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using CapitalRequestAutomatedTesting.UI.Services;
using CapitalRequestAutomatedTesting.UI.Services.Actual;
using CapitalRequestAutomatedTesting.UI.Services.Original;
using CapitalRequestAutomatedTesting.UI.Services.Predictive;
using DinkToPdf;
using Infrastructure.ApiDiagnostics;
using Infrastructure.Utilities.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using NLog;
using System.Diagnostics;

namespace CapitalRequestAutomatedTesting.UI.Controllers
{
    public class ScenarioController : Controller
    {
        private readonly ILogger<ScenarioController> _logger;
        private readonly IScenarioControllerService _scenarioControllerService;
        private readonly IWorkflowControllerService _workflowControllerService;
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly IPredictiveScenarioService _predictiveScenarioService;
        private readonly IActualScenarioService _actualScenarioService;
        private readonly IOriginalScenarioService _originalScenarioService;
        private readonly IPredictiveSeleniumService _predictiveSeleniumService;
        private readonly IActualSeleniumService _actualSeleniumService;
        private readonly IViewRenderService _viewRenderService;
        private readonly IScenarioMemoryCache _scenarioMemoryCache;
        private readonly IScenarioComparer _scenarioComparer;
        private readonly IFormDataContext _formDataContext;
        private readonly IMapper _mapper;
        private readonly ScenarioViewModelBuilder _viewModelBuilder;

        public ScenarioController(ILogger<ScenarioController> logger,
            IScenarioControllerService scenarioControllerService,
            IWorkflowControllerService workflowControllerService,
            ICapitalRequestServices capitalRequestServices,
            IPredictiveScenarioService predictiveScenarioService,
            IActualScenarioService actualScenarioService,
            IOriginalScenarioService originalScenarioService,
            IPredictiveSeleniumService predictiveSeleniumService,
            IActualSeleniumService actualSeleniumService,
            IViewRenderService viewRenderService,
            IScenarioMemoryCache scenarioMemoryCache,
            ScenarioViewModelBuilder viewModelBuilder,
            IScenarioComparer scenarioComparer,
            IFormDataContext formDataContext,
            IMapper mapper)
        {
            _logger = logger;
            _scenarioControllerService = scenarioControllerService;
            _workflowControllerService = workflowControllerService;
            _capitalRequestServices = capitalRequestServices;
            _predictiveScenarioService = predictiveScenarioService;
            _actualScenarioService = actualScenarioService;
            _originalScenarioService = originalScenarioService;
            _predictiveSeleniumService = predictiveSeleniumService;
            _actualSeleniumService = actualSeleniumService;
            _viewRenderService = viewRenderService;
            _scenarioMemoryCache = scenarioMemoryCache;
            _viewModelBuilder = viewModelBuilder;
            _scenarioComparer = scenarioComparer;
            _formDataContext = formDataContext;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Basic log test at {Time}", DateTime.UtcNow);

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


                TempData["ScenarioModel"] = JsonConvert.SerializeObject(model); ;

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

        public async Task<IActionResult> GetPartialViewForScenario(string scenarioId, int requestId)
        {
            if (string.IsNullOrWhiteSpace(scenarioId))
            {
                return BadRequest("Invalid scenario ID");
            }

            var detail = await _scenarioControllerService.GetScenarioDetail(scenarioId, requestId);

            if (detail == null)
            {
                return NotFound($"Scenario ID '{scenarioId}' not recognized");
            }

            var viewData = new ViewDataDictionary<ScenarioDetailsViewModel>(ViewData, detail)
            {
                TemplateInfo = { HtmlFieldPrefix = $"ScenarioDetails[{detail.SequenceNumber - 1}]" }
            };

            ViewData = viewData; // <-- This line is key

            return PartialView(detail.PartialViewName, detail);
        }


        [HttpGet]
        public async Task<IActionResult> GetReviewerDetails(int reviewerId, int proposalId, int? requestingGroupId, int? targetGroupId, int? replyingGroupId, string displayText)
        {
            var reviewer = await _scenarioControllerService.GetReviewerByIdAsync(reviewerId);
            var reviewerEmail = reviewer.Email;
            var reviewerUserId = reviewer.UserId;
            var requestedInfoId = 0;
            var requestedInformation = string.Empty;
            var returnedInformation = string.Empty;
            var requestingGroup = new CapitalRequest.API.Models.ReviewerGroup();
            var targetGroup = new CapitalRequest.API.Models.ReviewerGroup();
            var replyingGroup = new CapitalRequest.API.Models.ReviewerGroup();

            var requestedInfos = await _capitalRequestServices.GetAllRequestedInfos(
            new RequestedInfoSearchFilter
            {
                ProposalId = proposalId,
                IsOpen = true,
                ReviewerGroupId = replyingGroupId
            });

            CapitalRequest.API.Models.RequestedInfo requestedInfo = null;

            if (requestingGroupId.HasValue)
            {
                requestedInfo = requestedInfos
                    .FirstOrDefault(r => r.RequestingReviewerGroupId == requestingGroupId.Value);
            }
            else
            {
                requestedInfo = requestedInfos.FirstOrDefault(); // fallback to first open request
            }

            if (replyingGroupId.HasValue)
            {

                if (requestedInfo != null)
                {
                    requestedInformation = requestedInfo.RequestedInformation;
                    requestedInfoId = requestedInfo.Id;
                }

                if (!requestingGroupId.HasValue)
                {
                    requestingGroupId = requestedInfo.RequestingReviewerGroupId;
                }

                requestingGroup = await _scenarioControllerService.GetReviewerGroupByIdAsync(requestingGroupId.Value);
                replyingGroup = await _scenarioControllerService.GetReviewerGroupByIdAsync(replyingGroupId.Value);
                returnedInformation = $"{replyingGroup.Name} replying to request for more information from {requestingGroup.Name} as {reviewer.FullName} via Workflow Automated Testing - {displayText} Scenario.";
            }
            else if (requestingGroupId.HasValue)
            {
                requestingGroup = await _scenarioControllerService.GetReviewerGroupByIdAsync(requestingGroupId.Value);
                if (targetGroupId.HasValue)
                {
                    targetGroup = await _scenarioControllerService.GetReviewerGroupByIdAsync(targetGroupId.Value);
                }

                if (requestingGroupId.HasValue && targetGroupId.HasValue)
                {
                    requestedInformation = $"{requestingGroup.Name} requesting more information from {targetGroup.Name} as {reviewer.FullName} via Workflow Automated Testing - {displayText} Scenario.";
                }

            }

            return Json(new
            {
                reviewerEmail,
                reviewerUserId,
                requestedInformation,
                returnedInformation,
                requestedInfoId,
                requestingGroupId
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

                    return RedirectToAction("Preview", "Rollback");
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
            //TODO develop schema for saving
            scenarioComparisonResult.Id = 1;

            _scenarioMemoryCache.Save(scenarioComparisonResult.Id, scenarioComparisonResult);

            return View(scenarioComparisonResult);

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
            using (ScopeContext.PushProperty("ScenarioId", scenarioId))
            {
                _logger.LogInformation("Process started for scenario {ScenarioId}", scenarioId);
            }

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
            scenario.ActualData = await _actualScenarioService.GenerateScenarioDataAsync(scenario);
            stopwatch.Stop();
            scenario.PredictedSeleniumOutcome.Success = false;

            return scenario;


            // Step 3: Actual Selenium — even if prediction failed (limited by completion step count)
            scenario.ActualSeleniumOutcome = await _actualSeleniumService.GenerateSeleniumOutcomeAsync(scenario);

            //Step 4: Actual Data(only if prediction succeeded)
            if (scenario.PredictedSeleniumOutcome.Success)
            {
                scenario.ActualData = await _actualScenarioService.GenerateScenarioDataAsync(scenario);
                stopwatch.Stop();

                scenario.ActualData.ActualExecutionDuration = stopwatch.Elapsed;
                scenario.ActualData.ActualExecutionDurationMinutes = (int)Math.Ceiling(stopwatch.Elapsed.TotalMinutes);

            }

            return scenario;
        }

        //private async Task<ScenarioDetailsViewModel> ProcessScenario(ScenarioDetailsViewModel scenario)
        //{
        //    // Predictive Selenium outcome
        //    scenario.PredictedSeleniumOutcome = await _predictiveSeleniumService.GenerateSeleniumOutcomeAsync(scenario);

        //    // Check if predictive failed
        //    if (!scenario.PredictedSeleniumOutcome.Success)
        //    {
        //        scenario.CanExecuteActualSteps = false;
        //        scenario.PredictiveStopReason = "Predictive Selenium outcome failed — halting actual execution.";
        //        return scenario;
        //    }

        //    // ⏱ Measure actual execution time
        //    var stopwatch = Stopwatch.StartNew();

        //    // Predictive data
        //    scenario.PredictiveData = await _predictiveScenarioService.GenerateScenarioDataAsync(scenario);

        //    // Actual Selenium outcome (match steps up to prediction limit if needed)
        //    scenario.ActualSeleniumOutcome = await _actualSeleniumService.GenerateSeleniumOutcomeAsync(scenario);

        //    stopwatch.Stop();

        //    // Conditional actual data retrieval
        //    if (scenario.CanExecuteActualSteps)
        //    {
        //        scenario.ActualData = await _actualScenarioService.GenerateScenarioDataAsync(scenario);
        //        scenario.ActualData.ActualExecutionDuration = stopwatch.Elapsed;
        //        scenario.ActualData.ActualExecutionDurationMinutes = (int)Math.Ceiling(stopwatch.Elapsed.TotalMinutes);
        //    }

        //    return scenario;
        //}

        //private async Task<ScenarioDetailsViewModel> ProcessScenario(ScenarioDetailsViewModel scenario)
        //{

        //    // Predictive Selenium outcome
        //    scenario.PredictedSeleniumOutcome = await _predictiveSeleniumService.GenerateSeleniumOutcomeAsync(scenario);

        //    // ⏱ Measure actual execution time
        //    var stopwatch = Stopwatch.StartNew();

        //    // Predictive data
        //    scenario.PredictiveData = await _predictiveScenarioService.GenerateScenarioDataAsync(scenario);

        //    scenario.ActualSeleniumOutcome = await _actualSeleniumService.GenerateSeleniumOutcomeAsync(scenario);
        //    stopwatch.Stop();

        //    // Retrieve actual data
        //    scenario.ActualData = await _actualScenarioService.GenerateScenarioDataAsync(scenario);

        //    scenario.ActualData.ActualExecutionDuration = stopwatch.Elapsed;
        //    // Store rounded-up duration in minutes

        //    scenario.ActualData.ActualExecutionDurationMinutes = (int)Math.Ceiling(stopwatch.Elapsed.TotalMinutes);

        //    return scenario;
        //}


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
        public async Task<JsonResult> GetTargetGroupsAndReviewers(int proposalId, int groupId, string groupType)
        {
            var targetGroups = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
            var reviewers = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();

            if (groupType == "requesting")
            {
                targetGroups = await _scenarioControllerService.GetTargetGroupsByRequestIdAsync(proposalId, groupId);
                reviewers = await _scenarioControllerService.GetReviewersBySelectedGroupAsync(proposalId, groupId);
            }
            else if (groupType == "replying")
            {
                targetGroups = await _scenarioControllerService.GetRequestingGroupsByReplyingIdAsync(proposalId, groupId);
                reviewers = await _scenarioControllerService.GetReviewersBySelectedGroupAsync(proposalId, groupId);
            }

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