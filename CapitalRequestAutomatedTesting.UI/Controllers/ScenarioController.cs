
using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using CapitalRequestAutomatedTesting.UI.Services;
using CapitalRequestAutomatedTesting.UI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using ScenarioFramework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CapitalRequestAutomatedTesting.UI.Controllers
{
    public class ScenarioController : Controller
    {
        private readonly IScenarioControllerService _scenarioControllerService;
        private readonly IWorkflowControllerService _workflowControllerService;
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly IPredictiveScenarioService _predictiveScenarioService;
        private readonly IActualScenarioService _actualScenarioService;
        private readonly IScenarioComparer _scenarioComparer;

        public ScenarioController(
            IScenarioControllerService scenarioControllerService,
            IWorkflowControllerService workflowControllerService,
            ICapitalRequestServices capitalRequestServices,
            IPredictiveScenarioService predictiveScenarioService,
            IActualScenarioService actualScenarioService,
            IScenarioComparer scenarioComparer)
        {
            _scenarioControllerService = scenarioControllerService;
            _workflowControllerService = workflowControllerService;
            _capitalRequestServices = capitalRequestServices;
            _predictiveScenarioService = predictiveScenarioService;
            _actualScenarioService = actualScenarioService;
            _scenarioComparer = scenarioComparer;
        }

        public async Task<IActionResult> Index()
        {
            var formModel = await _scenarioControllerService.GenerateScenarioFormViewModel(null);

            return View(formModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromForm] ScenarioFormViewModel model, string actionType)
        {
            if (actionType == "SelectRequest" && model.RequestId.HasValue)
            {
                model = await _scenarioControllerService.GenerateScenarioFormViewModel(model.RequestId.Value);
            }
            else if (actionType == "SelectRequest")
            {
                model.RequestIds = await _scenarioControllerService.GetRequestSelectListAsync();
            }

            if (actionType == "RequestingGroupChanged")
            {
                var proposalId = model.RequestId.Value;


                var detail = model.ScenarioDetails.FirstOrDefault(d => d.ScenarioId == "SCN001");

                if (detail != null)
                {
                    detail.TargetGroups = await _scenarioControllerService.GetTargetGroupsByRequestIdAsync(proposalId, detail.RequestingGroupId);
                    detail.Reviewers = await _scenarioControllerService.GetReviewersByRequestingGroupAsync(proposalId, detail.RequestingGroupId);

                    var requestingGroups = await _scenarioControllerService.GetRequestingGroupsAsync(proposalId);

                    requestingGroups.ForEach(x =>
                    {
                        x.Selected = x.Value == detail.RequestingGroupId.ToString();
                    });

                    detail.RequestingGroups = requestingGroups;

                }

                var requestList = await _scenarioControllerService.GetRequestSelectListAsync();
                requestList.ForEach(x =>
                {
                    x.Selected = x.Value == model.RequestId?.ToString();
                });

                model.RequestIds = requestList;


            }

            if (actionType == "ReviewerSelected")
            {
                var proposalId = model.RequestId.Value;

                var detail = model.ScenarioDetails.FirstOrDefault(d => d.ScenarioId == "SCN001");
                if (detail != null)
                {
                    var reviewer = await _scenarioControllerService.GetReviewerByIdAsync(detail.ReviewerId);

                    detail.ProposalId = proposalId;
                    detail.ReviewerUserId = reviewer.UserId;
                    detail.ReviewerEmail = reviewer.Email;

                    // Rebuild dropdowns
                    model.RequestIds = await _scenarioControllerService.GetRequestSelectListAsync();
                    model.RequestIds.ForEach(x => x.Selected = x.Value == proposalId.ToString());
                    detail.RequestingGroups = await _scenarioControllerService.GetRequestingGroupsAsync(model.RequestId.Value);
                    detail.TargetGroups = await _scenarioControllerService.GetTargetGroupsByRequestIdAsync(model.RequestId.Value, detail.RequestingGroupId);
                    detail.Reviewers = await _scenarioControllerService.GetReviewersByRequestingGroupAsync(model.RequestId.Value, detail.RequestingGroupId);

                    // Set selected items
                    detail.RequestingGroups.ForEach(x => x.Selected = x.Value == detail.RequestingGroupId.ToString());
                    detail.TargetGroups.ForEach(x => x.Selected = x.Value == detail.TargetGroupId.ToString());
                    detail.Reviewers.ForEach(x => x.Selected = x.Value == detail.ReviewerId.ToString());
                }


            }

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
                scenarioDetail =  await ProcessScenario(scenario);
                scenarioDetails.Add(scenarioDetail);

                // etc.
            }

            // Store them in TempData or session (TempData uses serialization)
            foreach (var detail in scenarioDetails)
            {
                TempData["Scenario"] = JsonConvert.SerializeObject(detail);
                //TempData[$"Scenario_{detail.ScenarioId}"] = JsonConvert.SerializeObject(detail);
                //TempData["Predictive"] = JsonConvert.SerializeObject(detail.PredictiveData);
                //TempData["Actual"] = JsonConvert.SerializeObject(detail.ActualData);

            }

            return RedirectToAction("ViewComparison");
        }

        public IActionResult ViewComparison()
        {
            
            //var predictiveJson = TempData["Predictive"] as string;
            //var actualJson = TempData["Actual"] as string;
            
            //var predictive = JsonConvert.DeserializeObject<ScenarioDataViewModel>(predictiveJson);
            //var actual = JsonConvert.DeserializeObject<ScenarioDataViewModel>(actualJson);

            var scenarioJson = TempData["Scenario"] as string;
            var scenario = JsonConvert.DeserializeObject<ScenarioDetailsViewModel>(scenarioJson);

            var predictive = scenario.PredictiveData;
            var actual = scenario.ActualData;

            var result = _scenarioComparer.CompareData(predictive, actual);

            result.ScenarioId = scenario.ScenarioId;
            result.ScenarioName = scenario.DisplayText;
            result.SelectedProperties = new Dictionary<string, string>(scenario.SelectedProperties);

            return View(result);
        
        }

        private async Task<ScenarioDetailsViewModel> ProcessScenario(ScenarioDetailsViewModel scenario)
        {
            // Process each scenario
            var requestingGroupId = scenario.RequestingGroupId;
            var targetGroupId = scenario.TargetGroupId;
            var reviewerId = scenario.ReviewerId;
            var proposalId = scenario.ProposalId;
            var requestedInformation = "This message brought to you by Workflow Automated Testing.";


            requestingGroupId = 2;
            targetGroupId = 3;
            reviewerId = 37798;
            proposalId = 2884;

            scenario.RequestingGroupId = requestingGroupId;
            scenario.TargetGroupId = targetGroupId;
            scenario.ReviewerId = reviewerId;
            scenario.ProposalId = proposalId;
            scenario.RequestedInformation = requestedInformation;

            var message = scenario.Message;

            // Predictive data
            scenario.PredictiveData = await _predictiveScenarioService.GenerateScenarioDataAsync(scenario);

            // Run Selenium Scenario

            // Retrieve data
            scenario.ActualData = await _actualScenarioService.GenerateScenarioDataAsync(scenario);

            //Compare Data
            scenario.ComparisonResult = _scenarioComparer.CompareData(scenario.PredictiveData, scenario.ActualData);

            return scenario;
        }

  

        //[HttpPost]
        //public async Task<IActionResult> Index(ScenarioFormViewModel model)
        //{
        //    if (model.RequestId.HasValue)
        //    {
        //        // Rebuild the full model with scenarios for the selected RequestId
        //        model = await _scenarioControllerService.GenerateScenarioFormViewModel(model.RequestId.Value);
        //    }
        //    else
        //    {
        //        model.RequestIds = await _scenarioControllerService.GetRequestSelectListAsync();
        //    }

        //    return View(model);
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
        public async Task<IActionResult> GetRequestIds()
        {
            await _workflowControllerService.InitializeDashboardItemsAsync();
            var dashboardItems = await _workflowControllerService.GetDashboardItemsFromApiAsync();
            var ids = dashboardItems.Select(item => item.ReqId).Distinct().ToList();
            var result = ids.Select(id => new { id, name = $"{id}" });

            return Json(result);
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