using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Helpers;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.Utilities;
using Scriban;
using SSMWorkflow.API.Models;
using dto = CapitalRequest.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Predictive;

public interface IPredictiveRequestedInfoService
{
    Task<RequestedInfo> CreateRequestedInfoAsync(vm.Proposal proposal, int increment);
    Task<RequestedInfo> UpdateRequestedInfoAsync(vm.Proposal proposal);
    Task<RequestedInfo> GetRequestedInfoAsync(vm.Proposal proposal);
}

public class PredictiveRequestedInfoService : IPredictiveRequestedInfoService
{

    private readonly ISSMWorkflowServices _ssmWorkflowServices;
    private readonly ICapitalRequestServices _capitalRequestServices;
    private readonly IUserContextService _userContextService;
    private readonly IPredictiveEmailNotificationService _predictiveEmailNotificationService;
    private readonly IMapper _mapper;

    public PredictiveRequestedInfoService(
        ISSMWorkflowServices ssmWorkflowServices,
        ICapitalRequestServices capitalRequestServices,
        IUserContextService userContextService,
        IPredictiveEmailNotificationService predictiveEmailNotificationService,
        IMapper mapper)
    {
        _ssmWorkflowServices = ssmWorkflowServices;
        _capitalRequestServices = capitalRequestServices;
        _userContextService = userContextService;
        _predictiveEmailNotificationService = predictiveEmailNotificationService;
        _mapper = mapper;
    }

    public async Task<RequestedInfo> CreateRequestedInfoAsync(vm.Proposal proposal, int increment)
    {
        var reviewer = await _capitalRequestServices.GetReviewer(proposal.ReviewerId.HasValue ? proposal.ReviewerId.Value : 0);

        WorkFlowStepViewModel? workflowStep = await GetWorkflowStepAsync(proposal);
        // Resolve WorkflowStepOptionId

        var workflowStepOptions = (await _ssmWorkflowServices.GetAllWorkFlowStepOptions(workflowStep.WorkflowStepID))
            .Where(x => !x.IsComplete && !x.IsTerminate)
            .ToList();

        WorkFlowStepOptionViewModel workflowStepOption = null;
        if (workflowStepOptions.Any())
        {
            var optionsByGroup = workflowStepOptions
                .Where(x => x.ReviewerGroupId == proposal.ReviewerGroupId);

            if (optionsByGroup.Any())
            {
                workflowStepOption = workflowStepOptions
                    .Where(x => x.ReviewerGroupId == proposal.ReviewerGroupId &&
                                x.OptionType == Constants.OPTION_TYPE_VERIFY &&
                                x.OptionName.ToLower() == proposal.Reviewer.Email.ToLower())
                    .FirstOrDefault();
            }
        }

        var reviewerGroup = await _capitalRequestServices.GetReviewerGroup(proposal.RequestedInfo.ReviewerGroupId);
        var requestingGroup = await _capitalRequestServices.GetReviewerGroup(proposal.ReviewerGroupId);

        //var reviewer = (await _capitalRequestServices.GetReviewers(proposal.SegmentId))
        //    .Where(x => x.ReviewerGroupId == proposal.RequestedInfo.ReviewerGroupId &&
        //                x.Email.ToLower() == _userContextService.Email.ToLower())
        //    .FirstOrDefault();

        var fullName = reviewer.FullName;

        var action = _predictiveEmailNotificationService.GenerateActionString(reviewerGroup, requestingGroup, Constants.EMAIL_TEMPLATE_REQUEST_MORE_INFORMATION, fullName, null);

        //var requestingReviewer = (await _capitalRequestServices
        //        .GetReviewers(proposal.SegmentId))
        //        .FirstOrDefault(x => x.ReviewerGroupId == proposal.ReviewerGroupId &&
        //                    x.Email.ToLower() == _userContextService.Email.ToLower());

        var requestingReviewerId = 0;

        if (reviewer != null)
        {
            requestingReviewerId = reviewer.Id;
        }

        // Generate RequestedInfo object
        var requestedInfo = new vm.RequestedInfo
        {
            ProposalId = proposal.Id,
            RequestingReviewerGroupId = proposal.ReviewerGroupId,
            RequestingReviewerId = requestingReviewerId,
            ReviewerGroupId = proposal.RequestedInfo.ReviewerGroupId,
            RequestedInformation = TextManipulation.ConvertToHtmlText(proposal.RequestedInfo.RequestedInformation),
            Action = $"{action}.",
            WorkflowStepOptionId = workflowStepOption?.OptionID ?? Guid.Empty,
            IsOpen = true,
            Created = DateTime.Now,
            CreatedBy = reviewer.UserId
        };

        var requestedinfoId = (await _capitalRequestServices.GetAllRequestedInfos(new RequestedInfoSearchFilter())).Max(x => x.Id) + increment;
        requestedInfo.Id = requestedinfoId;

        return _mapper.Map<RequestedInfo>(requestedInfo);
    }

    public async Task<RequestedInfo> GetRequestedInfoAsync(vm.Proposal proposal)
    {

        var requestedInfo = ( await _capitalRequestServices.GetAllRequestedInfos(new RequestedInfoSearchFilter
                {
                    ProposalId = proposal.Id,
                    ReviewerGroupId = proposal.RequestedInfo.ReviewerGroupId,
                    RequestingReviewerGroupId = proposal.ReviewerGroupId,
                    IsOpen = true
                })
            ).FirstOrDefault();


        return _mapper.Map<RequestedInfo>(requestedInfo);
    }

    //public string GetActionString(vm.RequestedInfo requestedInfo, string action)
    //{
    //    var filter = new ReviewerGroupSearchFilter();

    //    var reviewerGroups = _capitalRequestServices
    //        .GetAllReviewerGroups(filter)
    //        .Result
    //        .ToList();

    //    var requestingGroup = reviewerGroups
    //        .Where(x => x.Id == requestedInfo.RequestingReviewerGroupId)
    //        .First()
    //        .Name;

    //    var requestingUser = $"{_userContextService.FirstName} {_userContextService.LastName}";

    //    var requestedGroup = reviewerGroups
    //        .Where(x => x.Id == requestedInfo.ReviewerGroupId)
    //        .First()
    //        .Name;

    //    return $"{requestingUser} from {requestingGroup} {action.ToLower()} from {requestedGroup} on {DateTime.Today.ToShortDateString()}.";
    //}

    public async Task<WorkFlowStepViewModel?> GetWorkflowStepAsync(vm.Proposal proposal)
    {
        var workflowSteps = await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId);

        return workflowSteps.FirstOrDefault(x => !x.IsComplete);
    }

    public async Task<RequestedInfo> UpdateRequestedInfoAsync(vm.Proposal proposal)
    {
        RequestedInfo requestedInfo = new RequestedInfo();
        if (proposal.RequestedInfoId != null && proposal.RequestedInfoId > 0)
        {
            requestedInfo = _mapper.Map<RequestedInfo>( await _capitalRequestServices.GetRequestedInfo(proposal.RequestedInfoId));
        }
        else
        {
            requestedInfo = await GetRequestedInfoAsync(proposal);
        }

        requestedInfo.IsOpen = false;
        requestedInfo.Updated = DateTime.Now;
        requestedInfo.UpdatedBy = proposal.Reviewer.UserId;

        return _mapper.Map< RequestedInfo>(requestedInfo);
    }
}
