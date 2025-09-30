using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using CapitalRequestAutomatedTesting.UI.Services;
using CapitalRequestAutomatedTesting.UI.Services.Actual;
using CapitalRequestAutomatedTesting.UI.Services.Original;
using CapitalRequestAutomatedTesting.UI.Services.Predictive;
using Infrastructure.ApiDiagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using NLog;
using System.Diagnostics;
using Microsoft.AspNetCore.SignalR;
using CapitalRequestAutomatedTesting.UI.Hubs;

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
        private readonly IPdfService _pdfService;
        private readonly IHubContext<ScenarioProgressHub> _hubContext;

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
            IPdfService pdfService,
            IMapper mapper,
            IHubContext<ScenarioProgressHub> hubContext)
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
            _pdfService = pdfService;
            _mapper = mapper;
            _hubContext = hubContext;
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
        public async Task<IActionResult> Index([FromForm] ScenarioFormViewModel model, string actionType, string connectionId)
        {
            _logger.LogInformation("Received connectionId: {ConnectionId}", connectionId ?? "NULL");
            
            if (actionType == "RunSelected")
            {
                // Handle the selected scenarios

                var selectedIds = model.SelectedScenarioIds;

                model.ScenarioDetails.ForEach(x =>
                {
                    x.ProposalId = model.RequestId ?? 0;
                });


                TempData["ScenarioModel"] = JsonConvert.SerializeObject(model);
                TempData["ConnectionId"] = connectionId; // Store connectionId in TempData

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
            var connectionId = TempData["ConnectionId"] as string; // Retrieve connectionId
            var model = JsonConvert.DeserializeObject<ScenarioFormViewModel>(modelJson);

            if (model.ScenarioDetails == null || !model.ScenarioDetails.Any())
            {
                Debug.WriteLine("ScenarioDetails is empty");
                return RedirectToAction("Index");
            }

            var selectedScenarios = model.ScenarioDetails
                .Where(s => model.SelectedScenarioIds.Contains(s.ScenarioId))
                .OrderBy(s => GetScenarioPriority(s.ScenarioId))
                .ToList();

            var scenarioDetails = new List<ScenarioDetailsViewModel>();

            // Sequential processing with connectionId passed through
            for (int i = 0; i < selectedScenarios.Count; i++)
            {
                var scenario = selectedScenarios[i];

                _logger.LogInformation("Starting scenario {ScenarioIndex}/{TotalScenarios}: {ScenarioName}",
                    i + 1, selectedScenarios.Count, scenario.DisplayText);

                // Pass the connectionId here
                var detail = await ProcessScenarioWithProgress(scenario, i + 1, selectedScenarios.Count, connectionId);

                if (detail.PredictiveSeleniumFailed)
                {
                    if (detail.CommitStepReached)
                    {
                        TempData["ScenarioDetail"] = JsonConvert.SerializeObject(detail);
                        return RedirectToAction("Preview", "Rollback");
                    }

                    foreach (var completedDetail in scenarioDetails)
                    {
                        TempData["Scenario"] = JsonConvert.SerializeObject(completedDetail);
                    }
                    return RedirectToAction("ViewComparison");
                }

                scenarioDetails.Add(detail);
            }

            TempData["Scenarios"] = JsonConvert.SerializeObject(scenarioDetails);
            return RedirectToAction("ViewComparison");
        }

        private async Task<ScenarioDetailsViewModel> ProcessScenarioWithProgress(ScenarioDetailsViewModel scenario, int currentIndex, int totalScenarios, string connectionId = null)
        {
            var scenarioId = scenario.ScenarioId;
            using (ScopeContext.PushProperty("ScenarioId", scenarioId))
            {
                _logger.LogInformation("Processing scenario {CurrentIndex}/{TotalScenarios}: {ScenarioName} ({ScenarioId})",
                    currentIndex, totalScenarios, scenario.DisplayText, scenarioId);
            }

            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Step 1: Predictive Selenium
                _logger.LogInformation("Step 1/4: Executing Predictive Selenium for {ScenarioName}", scenario.DisplayText);
                
                // Send progress update to client
                if (!string.IsNullOrEmpty(connectionId))
                {
                    await _hubContext.Clients.Group($"scenario-{connectionId}")
                        .SendAsync("UpdateProgress", new {
                            current = currentIndex - 1,
                            total = totalScenarios,
                            scenarioName = scenario.DisplayText,
                            currentStep = "Predictive Selenium"
                        });
                }

                scenario.PredictedSeleniumOutcome = await _predictiveSeleniumService.GenerateSeleniumOutcomeAsync(scenario);



                // Step 2: Predictive Data (only if prediction succeeded)
                if (scenario.PredictedSeleniumOutcome.Success)
                {
                    _logger.LogInformation("Step 2/4: Generating Predictive Data for {ScenarioName}", scenario.DisplayText);
                    
                    if (!string.IsNullOrEmpty(connectionId))
                    {
                        await _hubContext.Clients.Group($"scenario-{connectionId}")
                            .SendAsync("UpdateProgress", new {
                                current = currentIndex - 1,
                                total = totalScenarios,
                                scenarioName = scenario.DisplayText,
                                currentStep = "Predictive Data"
                            });
                    }

                    scenario.PredictiveData = await _predictiveScenarioService.GenerateScenarioDataAsync(scenario);

                    _logger.LogInformation("Step 2.5/4: Generating Original Data for {ScenarioName}", scenario.DisplayText);
                    
                    if (!string.IsNullOrEmpty(connectionId))
                    {
                        await _hubContext.Clients.Group($"scenario-{connectionId}")
                            .SendAsync("UpdateProgress", new {
                                current = currentIndex - 1,
                                total = totalScenarios,
                                scenarioName = scenario.DisplayText,
                                currentStep = "Original Data"
                            });
                    }

                    scenario.OriginalData = await _originalScenarioService.GenerateScenarioDataAsync(scenario);
                }

                // Step 3: Actual Selenium
                _logger.LogInformation("Step 3/4: Executing Actual Selenium for {ScenarioName}", scenario.DisplayText);
                
                if (!string.IsNullOrEmpty(connectionId))
                {
                    await _hubContext.Clients.Group($"scenario-{connectionId}")
                        .SendAsync("UpdateProgress", new {
                            current = currentIndex - 1,
                            total = totalScenarios,
                            scenarioName = scenario.DisplayText,
                            currentStep = "Actual Selenium"
                        });
                }

                scenario.StopWatch = Stopwatch.StartNew();
                scenario.ActualSeleniumOutcome = await _actualSeleniumService.GenerateSeleniumOutcomeAsync(scenario);

                // Step 4: Actual Data
                if (scenario.PredictedSeleniumOutcome.Success)
                {
                    _logger.LogInformation("Step 4/4: Generating Actual Data for {ScenarioName}", scenario.DisplayText);
                    
                    if (!string.IsNullOrEmpty(connectionId))
                    {
                        await _hubContext.Clients.Group($"scenario-{connectionId}")
                            .SendAsync("UpdateProgress", new {
                                current = currentIndex - 1,
                                total = totalScenarios,
                                scenarioName = scenario.DisplayText,
                                currentStep = "Actual Data"
                            });
                    }

                    scenario.ActualData = await _actualScenarioService.GenerateScenarioDataAsync(scenario);

                    stopwatch.Stop();
                    scenario.ActualData.ActualExecutionDuration = stopwatch.Elapsed;
                    scenario.ActualData.ActualExecutionDurationMinutes = (int)Math.Ceiling(stopwatch.Elapsed.TotalMinutes);
                }

                // Send completion update
                if (!string.IsNullOrEmpty(connectionId))
                {
                    await _hubContext.Clients.Group($"scenario-{connectionId}")
                        .SendAsync("ScenarioComplete", new {
                            current = currentIndex,
                            total = totalScenarios,
                            scenarioName = scenario.DisplayText,
                            success = true
                        });
                }

                _logger.LogInformation("Completed processing scenario {ScenarioName} in {ElapsedTime}",
                    scenario.DisplayText, stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                // Send error update
                if (!string.IsNullOrEmpty(connectionId))
                {
                    await _hubContext.Clients.Group($"scenario-{connectionId}")
                        .SendAsync("ScenarioError", new {
                            current = currentIndex,
                            total = totalScenarios,
                            scenarioName = scenario.DisplayText,
                            error = ex.Message
                        });
                }

                _logger.LogError(ex, "Error processing scenario {ScenarioName}: {ErrorMessage}",
                    scenario.DisplayText, ex.Message);
                throw;
            }

            return scenario;
        }

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
        //            if (detail.CommitStepReached)
        //            {
        //                TempData["ScenarioDetail"] = JsonConvert.SerializeObject(detail);

        //                return RedirectToAction("Preview", "Rollback");

        //            }

        //            return RedirectToAction("ViewComparison");

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

        public IActionResult ViewComparison()
        {
            var scenariosJson = TempData["Scenarios"] as string;
            var scenarios = JsonConvert.DeserializeObject<List<ScenarioDetailsViewModel>>(scenariosJson);

            if (scenarios == null || !scenarios.Any())
            {
                return RedirectToAction("Index");
            }

            // Option A: Process all scenarios and show combined results
            var comparisonResults = new List<ScenarioComparisonResult>();
            
            foreach (var scenario in scenarios)
            {
                var predictive = scenario.PredictiveData;
                var actual = scenario.ActualData;

                var scenarioComparisonResult = _scenarioComparer.CompareData(predictive, actual);
                scenarioComparisonResult.ScenarioId = scenario.ScenarioId;
                scenarioComparisonResult.ScenarioName = scenario.DisplayText;
                scenarioComparisonResult.SelectedProperties = new Dictionary<string, string>(scenario.SelectedProperties);
                scenarioComparisonResult.SeleniumComparisons = _scenarioComparer.CompareOutcomes(
                    scenario.PredictedSeleniumOutcome.Expected, 
                    scenario.ActualSeleniumOutcome.Expected);
                scenarioComparisonResult.Id = comparisonResults.Count + 1;

                _scenarioMemoryCache.Save(scenarioComparisonResult.Id, scenarioComparisonResult);
                comparisonResults.Add(scenarioComparisonResult);
            }

            // Return a view that can handle multiple comparison results
            return View("ViewMultipleComparisons", comparisonResults);
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
        //    using (ScopeContext.PushProperty("ScenarioId", scenarioId))
        //    {
        //        _logger.LogInformation("Process started for scenario {ScenarioId}", scenarioId);
        //    }

        //    scenario.PredictedSeleniumOutcome = await _predictiveSeleniumService.GenerateSeleniumOutcomeAsync(scenario);

        //    var completionStep = scenario.PredictiveCompletionStep;

        //    var stopwatch = Stopwatch.StartNew();

        //    // Step 2: Predictive Data (only if prediction succeeded)
        //    if (scenario.PredictedSeleniumOutcome.Success)
        //    {
        //        scenario.PredictiveData = await _predictiveScenarioService.GenerateScenarioDataAsync(scenario);
        //        scenario.OriginalData = await _originalScenarioService.GenerateScenarioDataAsync(scenario);
        //    }

        //    // Step 3: Actual Selenium — even if prediction failed (limited by completion step count)
        //    scenario.StopWatch = Stopwatch.StartNew();
        //    scenario.ActualSeleniumOutcome = await _actualSeleniumService.GenerateSeleniumOutcomeAsync(scenario);

        //    //Step 4: Actual Data(only if prediction succeeded)
        //    if (scenario.PredictedSeleniumOutcome.Success)
        //    {
        //        scenario.ActualData = await _actualScenarioService.GenerateScenarioDataAsync(scenario);
        //        stopwatch.Stop();

        //        scenario.ActualData.ActualExecutionDuration = stopwatch.Elapsed;
        //        scenario.ActualData.ActualExecutionDurationMinutes = (int)Math.Ceiling(stopwatch.Elapsed.TotalMinutes);

        //    }

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
            try
            {
                var model = _scenarioMemoryCache.Get(id);
                //var scenario = await _scenarioControllerService.GetScenarioByIdAsync(scenarioId);

                var htmlContent = await _viewRenderService.RenderToStringAsync("Scenario/ViewComparison", model);
                
                var pdfBytes = await _pdfService.GeneratePdfFromHtmlAsync(htmlContent);
                
                return File(pdfBytes, "application/pdf", "ScenarioReport.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF for scenario {Id}", id);
                return BadRequest("Error generating PDF");
            }
        }

        private int GetScenarioPriority(string scenarioId)
        {
            return scenarioId switch
            {
                "SCN001" => 1, // Request More Information (creates dependency)
                "SCN002" => 2, // Reply to Request (depends on SCN001)
                "SCN003" => 3, // Verify (can depend on others)
                "SCN004" => 4, // Approve WBS (final step)
                _ => 999
            };
        }
        //private void LogModelErrors(string contextLabel)
        //{
        //    foreach (var key in ModelState.Keys)
        //    {
        //        var state = ModelState[key];
        //        if (state.Errors.Any())
        //        {
        //            Debug.WriteLine($"❌ ModelState error for '{key}':");
        //            foreach (var error in state.Errors)
        //            {
        //                Debug.WriteLine($"In {contextLabel} → {error.ErrorMessage}");
        //            }
        //        }
        //    }
        //}
       
    }
}