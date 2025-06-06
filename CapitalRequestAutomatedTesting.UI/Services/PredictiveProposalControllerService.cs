using AutoMapper;
using CapitalRequestAutomatedTesting.Data;
using SSMWorkflow.API.Models;
using vm = CapitalRequest.API.Models;
using CapitalRequest.API.DataAccess.Models;
using System.Reflection.Metadata;
using CapitalRequestAutomatedTesting.UI.Models;

namespace CapitalRequestAutomatedTesting.UI.Services
{
    public interface IPredictiveProposalControllerService
    {

        
    }

    public class PredictiveProposalControllerService : IPredictiveProposalControllerService
    {

        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly IPredictiveWorkflowStepService _predictiveWorkflowStepService;
        private readonly IUserContextService _userContextService;
        private readonly IMapper _mapper;

        public PredictiveProposalControllerService(
         ISSMWorkflowServices ssmWorkflowServices,
         ICapitalRequestServices capitalRequestServices,
         IPredictiveWorkflowStepService predictiveWorkflowStepService,
         IUserContextService userContextService,
         IMapper mapper)
        {
            _ssmWorkflowServices = ssmWorkflowServices;
            _capitalRequestServices = capitalRequestServices;
            _predictiveWorkflowStepService = predictiveWorkflowStepService;
            _userContextService = userContextService;
            _mapper = mapper;
        }

        public vm.Proposal SendRequestForMoreInfo(vm.Proposal proposal)
        {
            var workflowStep = _predictiveWorkflowStepService.GetWorkflowStep(proposal);

            var emailTemplate = _capitalRequestServices
                .GetAllEmailTemplates(new EmailTemplateSearchFilter { Name = Constants.EMAIL_PROVIDE_MORE_INFORMATION }).Result
                .FirstOrDefault();

            var workflowStepOption = new WorkFlowStepOptionViewModel();

            var workflowStepOptions = _ssmWorkflowServices.GetAllWorkFlowStepOptions(workflowStep.WorkflowStepID)
                    .Result
                    .Where(x => !x.IsComplete && !x.IsTerminate)
                    .ToList();

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
                        .First();
                }
            }

            //TODO Determine why proposal.Segment is null.
            //Work around was done in Map to use SegmentId since it is not null

            var reviewer = _capitalRequestServices
             .GetAllReviewers(new ReviewerSearchFilter
             {
                 Email = _userContextService.Email.ToLower(),
                 RegionId = proposal.Region,
                 SegmentId = proposal.SegmentId,
                 ReviewerGroupId = proposal.ReviewerGroupId
             })
             .Result
             .FirstOrDefault();

            proposal.RequestedInfo.RequestingReviewerId = reviewer?.Id;

            proposal.RequestedInfo.RequestedInformation = TextManipulation.ConvertToHtmlText(proposal.RequestedInfo.RequestedInformation);
            proposal.RequestedInfo.WorkflowStepOptionId = workflowStepOption.OptionID;
            var requestedInfo = AddRequestedInfo(proposal);

            _workflowUtilities.AddWorkflowStepResponder(workflowStepOption, requestedInfo.RequestingReviewerGroupId, Constants.ACTION_TYPE_REQUEST, _controllerSharedService.AuthUser);

            _workflowUtilities.CloseOptions(workflowStepOptions, requestedInfo.RequestingReviewerGroupId, Guid.Empty, Constants.OPTION_TYPE_VERIFY, _controllerSharedService.AuthUser, null);

            var emailTemplates = _emailTemplateRepo.GetEmailTemplates();

            var reviewerGroups = GetReviewerGroups(proposal)
                .Where(x => x.Id == requestedInfo.ReviewerGroupId)
                .ToList();

            reviewerGroups.ForEach(rg =>
            {
                _workflowUtilities.AddStakeHolder(proposal.WorkflowId, rg);

                var reviewers = _reviewerRepo.GetReviewers(_mapper.Map<dto.Proposal>(proposal))
                    .Where(y => y.ReviewerGroupId == rg.Id)
                    .Select(z => _mapper.Map<vm.Reviewer>(z))
                    .ToList();

                reviewers.ForEach(r =>
                {
                    var emailType = emailTemplates
                        .Where<EmailTemplate>(y => y.Name == Constants.EMAIL_REQUEST_MORE_INFORMATION)
                        .First()
                        .OptionType;

                    if (emailType == Constants.EMAIL_TYPE_NOTIFY)
                    {
                        return;
                    }

                    if (string.IsNullOrEmpty(r.Email))
                    {
                        r.Email = proposal.AuthorEmail;
                    }

                    _workflowUtilities.AddWorkflowStepOption(workflowStep.WorkflowStepID, rg, r, emailType, requestedInfo.Id);
                });
            });

            _workflowUtilities.SendNotification(workflowStep.WorkflowStepID, emailTemplate.Id, requestedInfo.ReviewerGroupId, requestedInfo.Action, Guid.Empty, requestedInfo.Id);

            proposal.ResponseMessage = Constants.RESPONSE_REQUEST_FOR_MORE_INFORMATION_SENT;
        }


    }
}
