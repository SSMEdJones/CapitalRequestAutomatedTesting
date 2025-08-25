using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequest.API.Models;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Helpers;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.Utilities;
using Scriban;
using SSMWorkflow.API.Models;
using dto = CapitalRequest.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Predictive;

public interface IPredictiveProvidedInfoService
{
    Task<dto.ProvidedInfo> CreateProvidedInfoAsync(vm.Proposal proposal, int increment);
}

public class PredictiveProvidedInfoService : IPredictiveProvidedInfoService
{

    private readonly ISSMWorkflowServices _ssmWorkflowServices;
    private readonly ICapitalRequestServices _capitalRequestServices;
    private readonly IUserContextService _userContextService;
    private readonly IPredictiveEmailNotificationService _predictiveEmailNotificationService;
    private readonly IMapper _mapper;

    public PredictiveProvidedInfoService(
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

    public async Task<dto.ProvidedInfo> CreateProvidedInfoAsync(vm.Proposal proposal, int increment)
    {
        var reviewer = proposal.Reviewer;
        var fullName = reviewer.FullName;
        var requestingGroupName = proposal.ReplyingGroup.Name;
        var replyingGroupName = proposal.ReplyingGroup.Name;
        var requestingUser = (await _capitalRequestServices.GetReviewer(proposal.RequestedInfo.RequestingReviewerId)).FullName;
        var requestedGroup = proposal.RequestingGroup.Name;
        var reviewerGroup = await _capitalRequestServices.GetReviewerGroup(proposal.RequestedInfo.ReviewerGroupId);
        var requestDate = DateTime.Now.ToShortDateString();
        var replyingGroup = proposal.ReplyingGroup;

        var lastProvidedInfo = (await _capitalRequestServices.GetAllProvidedInfos(new ProvidedInfoSearchFilter())).Max(x => x.Id);

        var providedInfo = _mapper.Map<dto.ProvidedInfo>(proposal);
        var action = _predictiveEmailNotificationService.GenerateActionString(reviewerGroup, replyingGroup, Constants.EMAIL_TEMPLATE_RETURN_OF_REQUESTED_INFORMATION, fullName, requestingUser);

        providedInfo.Action = action;
        providedInfo.ProvidedInformation = TextManipulation.ConvertToHtmlText(proposal.ReturnedInformation);//replace \n to <br >
        providedInfo.Id = lastProvidedInfo + increment;

        return providedInfo;
    }

    public async Task<dto.RequestedInfo> GetRequestedInfoAsync(vm.Proposal proposal)
    {

        var requestedInfo = ( await _capitalRequestServices.GetAllRequestedInfos(new RequestedInfoSearchFilter
            {
                ProposalId = proposal.Id,
                ReviewerGroupId = proposal.RequestedInfo.ReviewerGroupId,
                RequestingReviewerGroupId = proposal.ReviewerGroupId,
                IsOpen = true
            }))
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

    public async Task<WorkFlowStepViewModel?> GetWorkflowStepAsync(vm.Proposal proposal)
    {
        var workflowSteps = await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId);

        return workflowSteps.FirstOrDefault(x => !x.IsComplete);
    }

    
}
