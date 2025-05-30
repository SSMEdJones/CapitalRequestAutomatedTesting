
//using System;
//using System.Linq;
//using CapitalRequest.DATA.Interface;
//using CapitalRequest.DATA.Model;
//using CapitalRequest.UI.Models;
//using CapitalRequest.UI.Utilities;
//using Microsoft.Extensions.DependencyInjection;
using SSMWorkflow.API.Models;
using CapitalRequest.API.Models;
using CapitalRequestAutomatedTesting.Data;
using AutoMapper;
using CapitalRequestAutomatedTesting.UI.Models;

namespace CapitalRequestAutomatedTesting.UI.Services;
public class PredictiveRequestedInfoService
{

    private readonly ISSMWorkflowServices _ssmWorkflowServices;
    private readonly ICapitalRequestServices _capitalRequestServices;
    private readonly IUserContextService _userContextService;
    private readonly IMapper _mapper;

    public PredictiveRequestedInfoService(
        ISSMWorkflowServices ssmWorkflowServices,
        ICapitalRequestServices capitalRequestServices,
        IUserContextService userContextService,
        IMapper mapper)
    {
        _ssmWorkflowServices = ssmWorkflowServices;
        _capitalRequestServices = capitalRequestServices;
        _userContextService = userContextService;
        _mapper = mapper;
    }

    public RequestedInfo Generate(Proposal proposal)
    {
        // Resolve WorkflowStepOptionId
        var workflowStep = _ssmWorkflowServices.GetWorkflowStep((Guid)proposal.WorkflowId).Result;
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

        // Generate RequestedInfo object
        var requestedInfo = new RequestedInfo
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
            Action = $"{proposal.Author} from IT requested more information from Unknown Group on {DateTime.Today:MM/dd/yyyy}.",
            WorkflowStepOptionId = workflowStepOption?.OptionID ?? Guid.Empty,
            IsOpen = true
        };

        return requestedInfo;
    }
}
