using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequest.API.Enums;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.Models;
using SSMWorkflow.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services
{
    public interface IPredictiveWorkflowStepOptionService
    {
        //WorkflowStepOption CreateWorkflowStepOption(vm.Proposal proposal, string OptionType);
        List<WorkflowStepOption> CloseOptions(vm.Proposal proposal, Guid optionId, string OptionType, int? requestedInfoId, string actionType);
        List<WorkflowStepOption> GetFilteredOptions(vm.Proposal proposal, string optionType, int? requestedInfoId);
        List<WorkflowStepOption> CreateWorkflowStepOptions(vm.Proposal proposal, string OptionType, int? requestedInfoId);
    }

    public class PredictiveWorkflowStepOptionService : IPredictiveWorkflowStepOptionService
    {
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly IPredictiveRequestedInfoService _predictiveRequestedInfoService;
        private readonly IUserContextService _userContextService;
        private readonly IMapper _mapper;

        public PredictiveWorkflowStepOptionService(
            ISSMWorkflowServices ssmWorkflowServices,
            ICapitalRequestServices capitalRequestServices,
            IPredictiveRequestedInfoService predictiveRequestedInfoService,
            IUserContextService userContextService,
            IMapper mapper)
        {
            _ssmWorkflowServices = ssmWorkflowServices;
            _capitalRequestServices = capitalRequestServices;
            _predictiveRequestedInfoService = predictiveRequestedInfoService;
            _userContextService = userContextService;
            _mapper = mapper;
        }

        public List<WorkflowStepOption> CreateWorkflowStepOptions(vm.Proposal proposal, string OptionType, int? requestedInfoId)
        {
            var workflowSteps = _ssmWorkflowServices.GetAllWorkFlowSteps((Guid)proposal.WorkflowId).Result;
            var workflowStep = _mapper.Map<WorkflowStep>(workflowSteps.FirstOrDefault(x => !x.IsComplete));

            var reviewerGroups = GetReviewerGroups(proposal, workflowStep)
                .Where(x => x.Id == proposal.RequestedInfo.ReviewerGroupId)
                    .ToList();

            var emailType = _capitalRequestServices
                .GetAllEmailTemplates(new EmailTemplateSearchFilter { Name = Constants.EMAIL_REQUEST_MORE_INFORMATION })
                .Result
                .FirstOrDefault()
                .OptionType;

            var workflowStepOptions = new List<WorkflowStepOption>();
            reviewerGroups.ForEach(rg =>
            {
                //TODO
                //_workflowUtilities.AddStakeHolder(proposal.WorkflowId, rg);

                var reviewers = GetReviewers(proposal)
                    .Where(x => x.ReviewerGroupId == rg.Id)
                    .Select(z => _mapper.Map<vm.Reviewer>(z))
                    .ToList();

                //var reviewers = _reviewerRepo.GetReviewers(_mapper.Map<dto.Proposal>(proposal))
                //    .Where(y => y.ReviewerGroupId == rg.Id)
                //    .Select(z => _mapper.Map<vm.Reviewer>(z))
                //    .ToList();

                reviewers.ForEach(r =>
                {

                    if (emailType == Constants.EMAIL_TYPE_NOTIFY)
                    {
                        return;
                    }

                    if (string.IsNullOrEmpty(r.Email))
                    {
                        r.Email = proposal.AuthorEmail;
                    }

                    var workflowStepOption = new WorkflowStepOption
                    {
                        OptionName = r.Email,
                        WorkflowStepID = workflowStep.WorkflowStepID,
                        ReviewerGroupId = rg.Id,
                        OptionType = emailType,
                        RequestedInfoId = requestedInfoId,
                        CreatedBy = _userContextService.UserId,
                    };

                    workflowStepOptions.Add(workflowStepOption);

                    //_workflowUtilities.AddWorkflowStepOption(workflowStep.WorkflowStepID, rg, r, emailType, requestedInfo.Id);
                });
            });

            return workflowStepOptions;
        }

        private List<vm.Reviewer> GetReviewers(vm.Proposal proposal)
        {
            return _capitalRequestServices.GetAllReviewers(new ReviewerSearchFilter { SegmentId = proposal.SegmentId }).Result;
        }

        public List<WorkflowStepOption> CloseOptions(vm.Proposal proposal, Guid optionId, string optionType, int? requestedInfoId, string actionType)
        {
            var reviewerGroupId = proposal.ReviewerGroupId;

            var workflowStepOptions = GetFilteredOptions(proposal, optionType, requestedInfoId);

            if (actionType == Constants.OPTION_TYPE_REQUEST)
            {
                var workflowStepOption = workflowStepOptions
                                                           .OrderByDescending(x => x.Created)
                                                           .Where(x => x.ReviewerGroupId == reviewerGroupId &&
                                                           x.OptionType == optionType &&
                                                           x.OptionName.ToLower() == _userContextService.Email.ToLower())
                                                           .FirstOrDefault();

                optionId = workflowStepOption.OptionID;
            }


            workflowStepOptions.ForEach(x =>
            {
                x.IsComplete = false;
                x.IsTerminate = false;

                if (x.OptionName.ToLower() == _userContextService.Email.ToLower())
                {
                    if (optionId == Guid.Empty && requestedInfoId == null)
                    {
                        return;
                    }
                    if (x.OptionID == optionId && actionType == Constants.OPTION_TYPE_REQUEST)
                    {
                        return;
                    }
                    if (x.OptionID == optionId)
                    {
                        x.IsComplete = true;
                    }
                    else if (x.OptionID == Guid.Empty)
                    {
                        x.IsTerminate = true;
                    }
                    else
                    {
                        x.IsTerminate = true;
                    }
                }
                else
                {
                    x.IsTerminate = true;
                }
                x.Updated = DateTime.Now;
                x.UpdatedBy = _userContextService.UserId;

            });

            return workflowStepOptions;
        }

        public List<WorkflowStepOption> GetFilteredOptions(vm.Proposal proposal, string optionType, int? requestedInfoId)
        {
            // Logic to close options based on the provided parameters
            var workflowSteps = _ssmWorkflowServices.GetAllWorkFlowSteps((Guid)proposal.WorkflowId).Result;
            var workflowStep = workflowSteps.FirstOrDefault(x => !x.IsComplete);

            var workflowStepOptions = _ssmWorkflowServices.GetAllWorkFlowStepOptions(workflowStep.WorkflowStepID).Result
                .Where(x => x.ReviewerGroupId == proposal.ReviewerGroupId &&
                             x.OptionType == optionType &&
                            (requestedInfoId == null || x.RequestedInfoId == requestedInfoId)
                          )
                .Select(x => _mapper.Map<WorkflowStepOption>(x))
                .ToList();

            return workflowStepOptions;
        }

        public List<vm.ReviewerGroup> GetReviewerGroups(vm.Proposal proposal, WorkflowStep workflowStep)
        {

            var workflowTemplate = _capitalRequestServices
                .GetAllWorkflowTemplates(new WorkflowTemplateSearchFilter { StepName = workflowStep.StepName }).Result
                .FirstOrDefault();

            var allReviewerGroups = _capitalRequestServices.GetAllReviewerGroups(new ReviewerGroupSearchFilter()).Result
                .Select(x => _mapper.Map<vm.ReviewerGroup>(x))
                .ToList();

            var filteredReviewerGroups = allReviewerGroups
                .Where(x => (x.StepNumber <= workflowTemplate.StepNumber && x.ReviewerType == Constants.REVIEW_TYPE_REVIEW) ||
                            (x.Name == Constants.REVIEWER_GROUP_AUTHOR && x.StepNumber == 0))
                .ToList();

            filteredReviewerGroups = FilterReviewerGroups(filteredReviewerGroups, proposal, workflowTemplate.StepNumber);

            return filteredReviewerGroups;
        }

        public List<vm.ReviewerGroup> FilterReviewerGroups(List<vm.ReviewerGroup> reviewerGroups, vm.Proposal proposal, int stepNumber)
        {
            if (proposal.ReviewerGroupId == 0 || stepNumber == Constants.STEP_SIX)
            {

                return reviewerGroups
                        .Where(reviewerGroup =>
                            reviewerGroup.StepNumber == stepNumber &&
                            (stepNumber == Constants.STEP_SIX && reviewerGroup.Name != Constants.PURCHASING_GROUP) ||
                            !(reviewerGroup.Name == Constants.EPMO_GROUP && proposal.IsProjectManagerDesired == (int)ProjectManagerDesired.No ||
                            (reviewerGroup.Name == Constants.ADMIN_GROUP && !proposal.AffectsMultipleSegments) ||
                            (reviewerGroup.Name == Constants.PURCHASING_GROUP && !proposal.IncludePurchasingGroup))
                        )
                        .ToList();
            }
            else
            {

                return reviewerGroups
                    .Where(reviewerGroup =>
                        reviewerGroup.Id != proposal.ReviewerGroupId &&
                        !(reviewerGroup.Name == Constants.EPMO_GROUP && proposal.IsProjectManagerDesired == (int)ProjectManagerDesired.No ||
                         (reviewerGroup.Name == Constants.ADMIN_GROUP && !proposal.AffectsMultipleSegments) ||
                         (reviewerGroup.Name == Constants.PURCHASING_GROUP && !proposal.IncludePurchasingGroup))
                    )
                    .ToList();
            }
        }
    }

}
