using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CapitalRequestAutomatedTesting.UI.Services
{
    public interface IScenarioControllerService
    {
        //Task<List<object>> GetAvailableScenariosAsync();
        IActionResult GetScenarioPartialView(string scenarioId);
        Task<List<SelectListItem>> GetReviewersByRequestingGroupAsync(int proposalId, int requestingGroupId);

        string GetScenarioViewName(string scenarioId);
        Task<ScenarioFormViewModel> GenerateScenarioFormViewModel(int? requestId);
        Task<List<SelectListItem>> GetTargetGroupsByRequestIdAsync(int proposalId, int requestingGroupId);
    }

    public class ScenarioControllerService : IScenarioControllerService
    {

        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly IWorkflowControllerService _workflowControllerService;


        public ScenarioControllerService(ICapitalRequestServices capitalRequestServices, ISSMWorkflowServices ssmWorkflowServices, IWorkflowControllerService workflowControllerService)
        {
            _capitalRequestServices = capitalRequestServices;
            _ssmWorkflowServices = ssmWorkflowServices;
            _workflowControllerService = workflowControllerService;
        }

        public async Task<ScenarioFormViewModel> GenerateScenarioFormViewModel(int? requestId = null)
        {
            var scenarioDetails = new List<ScenarioDetailsViewModel>
            {
                new ScenarioDetailsViewModel
                {
                    ScenarioId = "SCN001",
                    PartialViewName = "_RequestMoreInfo",
                    DisplayText  = "Request more info",
                    SequenceNumber = 1,
                    RequestingGroups = requestId.HasValue ? await GetRequestingGroupsAsync(requestId.Value) : new List<SelectListItem>(),
                    //TargetGroups = requestId.HasValue ? await GetTargetGroupsByRequestIdAsync(requestId.Value) : new List<SelectListItem>(),
                    //Reviewers = requestId.HasValue ? await GetReviewersByRequestIdAsync(requestId.Value) : new List<SelectListItem>()
                },
                new ScenarioDetailsViewModel
                {
                    ScenarioId = "SCN002",
                    PartialViewName = "_ReplyToRequest",
                    DisplayText  = "Reply to request",
                    SequenceNumber = 2,
                    RequestingGroups = requestId.HasValue ? await GetRequestingGroupsAsync(requestId.Value) : new List<SelectListItem>(),
                    //TargetGroups = requestId.HasValue ? await GetTargetGroupsByRequestIdAsync(requestId.Value, null) : new List<SelectListItem>(),
                    //Reviewers = requestId.HasValue ? await GetReviewersByRequestIdAsync(requestId.Value) : new List<SelectListItem>()
                }
            };

            return new ScenarioFormViewModel
            {
                RequestId = requestId ?? 0,
                ScenarioDetails = scenarioDetails
            };
        }


        public async Task<List<SelectListItem>> GetRequestingGroupsAsync(int proposalId)
        {
            var groups = await GetFilteredReviewerGroups(proposalId, null);

            return groups
                .ToList()
                .ConvertAll(x =>
                {
                 return new SelectListItem()
                 {
                     Text = x.Name,
                     Value = x.Id.ToString()
                 };
             });
           
        }

        public async Task<List<SelectListItem>> GetTargetGroupsByRequestIdAsync(int proposalId, int requestingGroupId)
        {
            var groups = await GetFilteredReviewerGroups(proposalId, requestingGroupId);

            return groups
                .ToList()
                .ConvertAll(x =>
                {
                    return new SelectListItem()
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    };
                });

        }
        public async Task<List<CapitalRequest.API.Models.ReviewerGroup>> GetFilteredReviewerGroups(int proposalId, int? requestingGroupId)
        {
            var proposal = await _capitalRequestServices.GetProposal(proposalId);            
            var workflowActions = await _workflowControllerService.GetWorkflowActionsFromApiAsync(proposalId);
            var availableNames = new List<string>();
            
            workflowActions.ForEach(x =>
            {
                var groupName = ExtractGroupName(x.WorkflowPortion);
                availableNames.Add(groupName);

            });

            var groups = await _capitalRequestServices.GetAllReviewerGroups(new ReviewerGroupSearchFilter { ReviewerType = Constants.REVIEW_TYPE_REVIEW });

            groups = (from data in groups
                      join name in availableNames on data.Name equals name
                      select data)
                      .ToList();

            var author = (await _capitalRequestServices
                .GetAllReviewerGroups(new ReviewerGroupSearchFilter { Name = Constants.REVIEWER_GROUP_AUTHOR, StepNumber = null }))
                .FirstOrDefault();

            if (requestingGroupId != null)
            {

                foreach (var group in groups.Where(x => x.Id == requestingGroupId))
                {
                    group.Id = author.Id;
                    group.Name = author.Name;
                    group.EmailTemplateId = author.EmailTemplateId;
                    group.StepNumber = author.StepNumber;
                }
            }


            return groups;
        }

        public async Task<List<CapitalRequest.API.Models.Reviewer>> GetFilteredReviewers(int proposalId, int requestingGroupId)
        {
            var proposal = await _capitalRequestServices.GetProposal(proposalId);
            var reviewerGroup = await _capitalRequestServices.GetReviewerGroup(requestingGroupId);

            var filter = new ReviewerSearchFilter 
                { RegionId = proposal.Region, 
                  SegmentId = proposal.SegmentId, 
                  StepNumber = reviewerGroup.StepNumber, 
                  ReviewerGroupId = reviewerGroup.Id
            };

            var reviewers = await _capitalRequestServices.GetAllReviewers(filter);

            return reviewers;
        }

        public static string ExtractGroupName(string dashboardText)
        {
            // Split on the first dash and trim the result
            var parts = dashboardText.Split('-', 2);
            var returnVal = parts.Length > 1 ? parts[1].Trim() : dashboardText.Trim();
            return returnVal;
            //return parts.Length > 1 ? parts[1].Trim().Replace(" ", string.Empty) : dashboardText.Trim();
        }

        //public async Task<List<object>> GetAvailableScenariosAsync()
        //{
        //    return new List<object>
        //{
        //    new { id = "SCN001", name = "Request More Information" },
        //    new { id = "SCN002", name = "Reply to Request" },
        //    new { id = "SCN003", name = "Verify" },
        //    new { id = "SCN004", name = "Approve WBS" }
        //};
        //}

        public IActionResult GetScenarioPartialView(string scenarioId)
        {
            return scenarioId switch
            {
                "SCN001" => new PartialViewResult { ViewName = "_RequestMoreInfo" },
                "SCN002" => new PartialViewResult { ViewName = "_ReplyToRequest" },
                _ => new PartialViewResult { ViewName = "_DefaultScenario" }
            };
        }

        public async Task<List<SelectListItem>> GetReviewersByRequestingGroupAsync(int proposalId, int requestingGroupId)
        {
            var proposal = await _capitalRequestServices.GetProposal(proposalId);
            var workflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps((Guid)proposal.WorkflowId)).FirstOrDefault();

            var workflowTemplate = (await _capitalRequestServices.GetAllWorkflowTemplates(new WorkflowTemplateSearchFilter { StepName = workflowStep.StepName })).FirstOrDefault();

            var filter = new ReviewerSearchFilter
            {
                RegionId = proposal.Region,
                SegmentId = proposal.SegmentId,
                StepNumber = workflowTemplate?.StepNumber,
                ReviewerGroupId = requestingGroupId
            };
            var reviewers = await _capitalRequestServices.GetAllReviewers(filter);

            return reviewers.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.FullName
            }).ToList();

        }
        public string GetScenarioViewName(string scenarioId)
        {
            return scenarioId switch
            {
                "SCN001" => "_RequestMoreInfo",
                "SCN002" => "_ReplyToRequest",
                _ => "_DefaultScenario"
            };
        }

        public async Task<Request> GetRequestByIdAsync(int id)
        {
            var initResult = await _workflowControllerService.InitializeDashboardItemsAsync();
            var actions = initResult.WorkflowActions;
            var dashboardItems = initResult.DashboardItems;

            var item = dashboardItems.FirstOrDefault(d => d.ReqId == id);
            if (item == null) return null;

            var request = new Request
            {
                Id = item.ReqId,
                ITReviewStatus = item.ITReviewStatus,
                FacilitiesReviewStatus = item.FacilitiesReviewStatus,
                SupplyChainReviewStatus = item.SupplyChainReviewStatus,
                EPMOReviewStatus = item.EPMOReviewStatus,
                PurchasingReviewStatus = item.PurchasingReviewStatus,
                FinanceReviewStatus = item.FinanceReviewStatus,
                VPOpsReviewStatus = item.VPOpsReviewStatus,
                VPFinanceReviewStatus = item.VPFinanceReviewStatus
            };

            return request;
        }
    }
}
