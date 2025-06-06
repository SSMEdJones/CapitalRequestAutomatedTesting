using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.Helpers;
using CapitalRequestAutomatedTesting.UI.Models;
using Scriban;
using SSMWorkflow.API.Models;
using dto = CapitalRequest.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services;

public interface IPredictiveRequestedInfoService
{
    dto.RequestedInfo CreateRequestedInfo(vm.Proposal proposal, int increment);
    dto.RequestedInfo GetRequestedInfo(vm.Proposal proposal);
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

    public dto.RequestedInfo CreateRequestedInfo(vm.Proposal proposal, int increment)
    {
        WorkFlowStepViewModel? workflowStep = GetWorkflowStep(proposal);

        // Resolve WorkflowStepOptionId

        var workflowStepOptions = _ssmWorkflowServices.GetAllWorkFlowStepOptions(workflowStep.WorkflowStepID).Result
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
                                x.OptionName.ToLower() == _userContextService.Email.ToLower())
                    .FirstOrDefault();
            }
        }

        var reviewerGroup = _capitalRequestServices.GetReviewerGroup(proposal.RequestedInfo.ReviewerGroupId).Result;
        var requestingGroup = _capitalRequestServices.GetReviewerGroup(proposal.ReviewerGroupId).Result;
        var reviewer = _capitalRequestServices.GetReviewers(proposal.SegmentId).Result
            .Where(x => x.ReviewerGroupId == proposal.RequestedInfo.ReviewerGroupId &&
                        x.Email.ToLower() == _userContextService.Email.ToLower())
            .FirstOrDefault();

        var fullName = $"{_userContextService.FirstName} {_userContextService.LastName}";

        var action = _predictiveEmailNotificationService.GenerateActionString(reviewerGroup, requestingGroup, Constants.EMAIL_ACTION_REQUEST_MORE_INFORMATION, fullName);
        // Generate RequestedInfo object
        var requestedInfo = new vm.RequestedInfo
        {
            ProposalId = proposal.Id,
            RequestingReviewerGroupId = proposal.ReviewerGroupId,
            RequestingReviewerId = _capitalRequestServices
                .GetReviewers(proposal.SegmentId).Result
                .Where(x => x.ReviewerGroupId == proposal.ReviewerGroupId &&
                            x.Email.ToLower() == _userContextService.Email.ToLower())
                .First()
                .Id,
            ReviewerGroupId = proposal.RequestedInfo.ReviewerGroupId,
            RequestedInformation = "This message brought to you by Workflow Automated Testing.",
            Action = action,
            WorkflowStepOptionId = workflowStepOption?.OptionID ?? Guid.Empty,
            IsOpen = true,
            Created = DateTime.Now,
            CreatedBy = _userContextService.UserId
        };

        var requestedinfoId = _capitalRequestServices.GetAllRequestedInfos(new RequestedInfoSearchFilter()).Result.Max(x => x.Id) + increment;
        requestedInfo.Id = requestedinfoId;

        return _mapper.Map<dto.RequestedInfo>(requestedInfo);
    }

    public dto.RequestedInfo GetRequestedInfo(vm.Proposal proposal)
    {

        var requestedInfo = _capitalRequestServices.GetAllRequestedInfos(new RequestedInfoSearchFilter
            {
                ProposalId = proposal.Id,
                ReviewerGroupId = proposal.RequestedInfo.ReviewerGroupId,
                RequestingReviewerGroupId = proposal.ReviewerGroupId,
                IsOpen = true
            })
            .Result
            .FirstOrDefault();

        return _mapper.Map<dto.RequestedInfo>(requestedInfo);
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

    public WorkFlowStepViewModel? GetWorkflowStep(vm.Proposal proposal)
    {
        var workflowSteps = _ssmWorkflowServices.GetAllWorkFlowSteps((Guid)proposal.WorkflowId).Result;
        var workflowStep = workflowSteps.FirstOrDefault(x => !x.IsComplete);
        return workflowStep;
    }

    
}
