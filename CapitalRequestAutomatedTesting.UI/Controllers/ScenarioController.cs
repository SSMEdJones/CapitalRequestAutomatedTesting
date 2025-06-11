
using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using CapitalRequestAutomatedTesting.UI.Services;
using CapitalRequestAutomatedTesting.UI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public ScenarioController(IScenarioControllerService scenarioControllerService, IWorkflowControllerService workflowControllerService, ICapitalRequestServices capitalRequestServices)
        {
            _scenarioControllerService = scenarioControllerService;
            _workflowControllerService = workflowControllerService;
            _capitalRequestServices = capitalRequestServices;
        }

        public async Task<IActionResult> Index()
        {
            var formModel = await _scenarioControllerService.GenerateScenarioFormViewModel(null);

            return View(formModel);
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

        private List<SelectListItem> GetRequestingGroups()
        {
            var groups = _capitalRequestServices.GetAllReviewerGroups(new ReviewerGroupSearchFilter { ReviewerType = "Reviewer" });
            throw new NotImplementedException();
        }

        [HttpGet]
        public IActionResult LoadScenarioView(string scenarioId, string requestId)
        {
            ViewBag.RequestId = requestId;
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



        [HttpPost]
        public IActionResult RunSelected(ScenarioFormViewModel model)
        {

            // Check if model.ScenarioDetails has data
            if (model.ScenarioDetails == null || !model.ScenarioDetails.Any())
            {
                // Log or debug here
                Debug.WriteLine("ScenarioDetails is empty");
            }


            foreach (var key in Request.Form.Keys)
            {
                Console.WriteLine($"{key}: {Request.Form[key]}");
            }

            var selectedScenarios = model.ScenarioDetails
            .Where(s => model.SelectedScenarioIds.Contains(s.ScenarioId))
            .ToList();

            // Now you have full access to each selected scenario's form data
            foreach (var scenario in selectedScenarios)
            {
                // Process each scenario
                var requestingGroupId = scenario.RequestingGroupId;
                var targetGroupId = scenario.TargetGroupId;
                var reviewerId = scenario.ReviewerId;
                var message = scenario.Message;
                // etc.
            }

            ViewBag.Message = $"You selected: {string.Join(", ", selectedScenarios.Select(s => s.ScenarioId))}";
            return View("Index", model);
        }

    }
}