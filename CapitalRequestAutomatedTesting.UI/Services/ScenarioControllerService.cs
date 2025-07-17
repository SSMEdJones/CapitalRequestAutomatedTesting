using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using vm = CapitalRequest.API.Models;


namespace CapitalRequestAutomatedTesting.UI.Services
{
    public interface IScenarioControllerService
    {
        //Task<List<object>> GetAvailableScenariosAsync();
        IActionResult GetScenarioPartialView(string scenarioId);
        Task<List<SelectListItem>> GetReviewersBySelectedGroupAsync(int proposalId, int requestingGroupId);

        string GetScenarioViewName(string scenarioId);
        Task<ScenarioFormViewModel> GenerateScenarioFormViewModel(int? requestId);
        Task<List<SelectListItem>> GetTargetGroupsByRequestIdAsync(int proposalId, int requestingGroupId);
        Task<List<SelectListItem>> GetRequestSelectListAsync();
        Task<List<SelectListItem>> GetRequestingGroupsAsync(int proposalId);
        Task<CapitalRequest.API.Models.Reviewer> GetReviewerByIdAsync(int id);
        Task<SeleniumStepResult> ValidateTargetGroupIdAsync(vm.Proposal proposal, int requestingGroupId, int targetGroupId);
        Task<CapitalRequest.API.Models.ReviewerGroup> GetReviewerGroupByIdAsync(int id);
        Task<(List<SelectListItem> RequestingGroups, List<SelectListItem> TargetGroups)> BuildRequestingAndTargetGroupsAsync(int proposalId, int? requestingGroupId);

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
                    DisplayText  = "Request More Information",
                    SequenceNumber = 1,
                    RequestingGroups = requestId.HasValue ? await GetRequestingGroupsAsync(requestId.Value) : new List<SelectListItem>(),
                    //TargetGroups = requestId.HasValue ? await GetTargetGroupsByRequestIdAsync(requestId.Value) : new List<SelectListItem>(),
                    //Reviewers = requestId.HasValue ? await GetReviewersByRequestIdAsync(requestId.Value) : new List<SelectListItem>()
                },
                new ScenarioDetailsViewModel
                {
                    ScenarioId = "SCN002",
                    PartialViewName = "_ReplyToRequest",
                    DisplayText  = "Reply to Request",
                    SequenceNumber = 2,
                    ReplyingGroups = requestId.HasValue ? await GetReplyingGroupsAsync(requestId.Value) : new List<SelectListItem>(),
                    //TargetGroups = requestId.HasValue ? await GetTargetGroupsByRequestIdAsync(requestId.Value, null) : new List<SelectListItem>(),
                    //Reviewers = requestId.HasValue ? await GetReviewersByRequestIdAsync(requestId.Value) : new List<SelectListItem>()
                }
            };

            return new ScenarioFormViewModel
            {
                RequestIds = new List<SelectListItem>(),
                RequestId = requestId ?? 0,
                ScenarioDetails = scenarioDetails
            };
        }

        public async Task<List<SelectListItem>> GetRequestSelectListAsync()
        {
            return (await _workflowControllerService.GetDashboardItemsFromApiAsync())
                    .Select(x => new SelectListItem
                    {
                        Text = x.ReqId.ToString(),
                        Value = x.ReqId.ToString()
                    }).ToList();
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

        public async Task<List<SelectListItem>> GetReplyingGroupsAsync(int proposalId)
        {

            var WorkflowPortions = (await _workflowControllerService.GetWorkflowActionsFromApiAsync(proposalId, Constants.ACTION_TYPE_ADD_INFO))
                .Select(x => x.WorkflowPortion)
                .Distinct()
                .ToList();

            var groups = await GetAvailableNamesAsync(WorkflowPortions);

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

        private async Task<List<vm.ReviewerGroup>> GetAvailableNamesAsync(List<string> WorkflowPortions)
        {
            var availableNames = new List<string>();

            WorkflowPortions.ForEach(x =>
            {
                var groupName = ExtractGroupName(x);
                availableNames.Add(groupName);

            });

            var groups = await _capitalRequestServices.GetAllReviewerGroups(new ReviewerGroupSearchFilter { ReviewerType = Constants.REVIEW_TYPE_REVIEW });

            groups = (from data in groups
                      join name in availableNames on data.Name equals name
                      select data)
                      .ToList();

            return groups;
        }

        public async Task<(List<SelectListItem> RequestingGroups, List<SelectListItem> TargetGroups)> BuildRequestingAndTargetGroupsAsync(int proposalId, int? requestingGroupId)
        {
            var baseGroups = await GetFilteredReviewerGroups(proposalId, null);
            var authorGroup = await _capitalRequestServices
                .GetAllReviewerGroups(new ReviewerGroupSearchFilter { Name = Constants.REVIEWER_GROUP_AUTHOR })
                .ContinueWith(t => t.Result.FirstOrDefault());

            var requestingList = baseGroups
                .Select(g => new SelectListItem
                {
                    Text = g.Name,
                    Value = g.Id.ToString()
                })
                .ToList();

            var targetList = baseGroups
                .Select(g =>
                {
                    var group = CloneGroup(g);

                    if (group.Id == requestingGroupId && authorGroup != null)
                    {
                        group.Id = authorGroup.Id;
                        group.Name = authorGroup.Name;
                        group.EmailTemplateId = authorGroup.EmailTemplateId;
                        group.StepNumber = authorGroup.StepNumber;
                    }

                    return new SelectListItem
                    {
                        Text = group.Name,
                        Value = group.Id.ToString()
                    };
                })
                .ToList();

            return (requestingList, targetList);
        }

        private vm.ReviewerGroup CloneGroup(vm.ReviewerGroup group)
        {
            return new vm.ReviewerGroup
            {
                Id = group.Id,
                Name = group.Name,
                StepNumber = group.StepNumber,
                EmailTemplateId = group.EmailTemplateId,
                // add other fields as needed
            };
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
        public async Task<List<vm.ReviewerGroup>> GetFilteredReviewerGroups(int proposalId, int? requestingGroupId)
        {
            var proposal = await _capitalRequestServices.GetProposal(proposalId);
            var WorkflowPortions = (await _workflowControllerService.GetWorkflowActionsFromApiAsync(proposalId))
                .Select(x => x.WorkflowPortion)
                .Distinct()
                .ToList();

            var groups = await GetAvailableNamesAsync(WorkflowPortions);

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

        public async Task<SeleniumStepResult> ValidateTargetGroupIdAsync (vm.Proposal proposal,  int requestingGroupId, int targetGroupId)
        {
            bool isValid = (await GetFilteredReviewerGroups(proposal.Id, requestingGroupId)).Where(X => X.Id == targetGroupId).Any();

            return new SeleniumStepResult
            {
                Success = isValid,
                Message = isValid
                    ? "Target Reviewer Group validation passed."
                    : "Target Reviewer Group not found for this Request."
            };
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

        public async Task<List<SelectListItem>> GetReviewersBySelectedGroupAsync(int proposalId, int reviewerGroupId)
        {
            var proposal = await _capitalRequestServices.GetProposal(proposalId);
            var workflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps((Guid)proposal.WorkflowId)).FirstOrDefault();

            var workflowTemplate = (await _capitalRequestServices.GetAllWorkflowTemplates(new WorkflowTemplateSearchFilter { StepName = workflowStep.StepName })).FirstOrDefault();

            var filter = new ReviewerSearchFilter
            {
                RegionId = proposal.Region,
                SegmentId = proposal.SegmentId,
                StepNumber = workflowTemplate?.StepNumber,
                ReviewerGroupId = reviewerGroupId
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

        public async Task<CapitalRequest.API.Models.Reviewer> GetReviewerByIdAsync(int id)
        {
            return  await _capitalRequestServices.GetReviewer(id);
        }

        public async Task<CapitalRequest.API.Models.ReviewerGroup> GetReviewerGroupByIdAsync(int id)
        {
            return await _capitalRequestServices.GetReviewerGroup(id);
        }
    }
}
