using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.Helpers;
using CapitalRequestAutomatedTesting.UI.Models;
using Microsoft.Extensions.Options;
using Scriban;
using SSMWorkflow.API.DataAccess.ConfiguratonSettings;
using SSMWorkflow.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services
{
    public interface IPredictiveEmailNotificationService
    {
        List<SSMWorkflow.API.DataAccess.Models.EmailNotification> CreateEmailNotifications(vm.Proposal proposal, string emailType);
        string GenerateActionString(vm.ReviewerGroup reviewerGroup, vm.ReviewerGroup requestingGroup, string emailActionTemplate, string fullname);
    }
    public class PredictiveEmailNotificationService : IPredictiveEmailNotificationService
    {
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly IUserContextService _userContextService;
        private readonly SSMWorkFlowSettings _ssmWorkFlowSettings;
        private readonly IMapper _mapper;

        public PredictiveEmailNotificationService(
            ISSMWorkflowServices ssmWorkflowServices,
            ICapitalRequestServices capitalRequestServices,
            IUserContextService userContextService,
            IOptionsMonitor<SSMWorkFlowSettings> ssmWorkFlowSettings,
            IMapper mapper)
        {
            _ssmWorkflowServices = ssmWorkflowServices;
            _capitalRequestServices = capitalRequestServices;
            _userContextService = userContextService;
            _ssmWorkFlowSettings = ssmWorkFlowSettings.CurrentValue;
            _mapper = mapper;
        }

        public List<SSMWorkflow.API.DataAccess.Models.EmailNotification> CreateEmailNotifications(vm.Proposal proposal, string emailType)
        {
            var url = _ssmWorkFlowSettings.BaseApiUrl;
            var link = _ssmWorkFlowSettings.ProjectReviewLink;

            var emailNotifications = new List<EmailNotification>();

            var workflowSteps = _ssmWorkflowServices.GetAllWorkFlowSteps((Guid)proposal.WorkflowId).Result;
            var workflowStep = _mapper.Map<WorkflowStep>(workflowSteps.FirstOrDefault(x => !x.IsComplete));

            var worflowStepId = workflowStep.WorkflowStepID;

            var reviewerGroupdId = proposal.RequestedInfo.ReviewerGroupId;
            var requestingGroupId = proposal.RequestedInfo.RequestingReviewerGroupId.Value;

            var reviewerGroup = _capitalRequestServices.GetReviewerGroup(reviewerGroupdId).Result;
            var requestingGroup = _capitalRequestServices.GetReviewerGroup(requestingGroupId).Result;

            var emailTemplate = _capitalRequestServices
                .GetAllEmailTemplates(new EmailTemplateSearchFilter { Name = emailType })
                .Result
                .FirstOrDefault();

            var workflowTemplate = _capitalRequestServices.GetAllWorkflowTemplates(new WorkflowTemplateSearchFilter { StepName = workflowStep.StepName })
                .Result
                .FirstOrDefault();

            var reviewers = GetReviewers(proposal)
                    .Where(x => x.ReviewerGroupId == reviewerGroupdId)
                    .Select(z => _mapper.Map<vm.Reviewer>(z))
                    .ToList();

            reviewers.ForEach(reviewer =>
            {
                var fullName = $"{_userContextService.FirstName} {_userContextService.LastName}";
                var action = GenerateActionString(reviewerGroup, requestingGroup, Constants.EMAIL_ACTION_REQUEST_MORE_INFORMATION, fullName);
                var emailMessage = GenerateEmailMessage(emailTemplate, reviewer, requestingGroup, proposal);

                var emallQueryViewModel = new EmailQueryViewModel
                {
                    WorkflowStepId = workflowStep.WorkflowStepID.ToString(),
                    EmailTemplateId = emailTemplate.Id.ToString(),
                    ReviewerGroupId = reviewerGroupdId.ToString(),
                    Action = action,
                    OptionId = proposal.RequestedInfo.WorkflowStepOptionId != null ? $"'{proposal.RequestedInfo.WorkflowStepOptionId}'" : "NULL",
                    RequestedInfoId = proposal.RequestedInfo.Id.ToString()
                };

                var emailQuery = GenerateEmailQuery(emallQueryViewModel);
                var emailNotification = new SSMWorkflow.API.DataAccess.Models.EmailNotification
                {
                    WorkflowStepId = workflowStep.WorkflowStepID,
                    WorkflowName = proposal.ProjectName,
                    WorkflowDescription = proposal.ProjectDescription,
                    WorkflowState = workflowTemplate.StepName,
                    StepName = workflowTemplate.StepName,
                    StepDescription = workflowTemplate.StepDescription,
                    Action = workflowStep.StepDescription,
                    EmailMessage = emailMessage,
                    Recipients = reviewer.Email,
                    Subject = emailTemplate.Subject,
                    Priority = emailTemplate.Priority,
                    EmailQuery = emailQuery,
                    Created = DateTime.Now
                };

                emailNotifications.Add(emailNotification);
            });
           
            return emailNotifications;
        }

        private string GenerateEmailQuery(EmailQueryViewModel emailQueryViewModel)
        {

            var sql = Template.Parse(SqlTemplates.CapitalRequestNotification);
           
            var emailQuery = sql.Render(new
            {
                workflowStepId = emailQueryViewModel.WorkflowStepId,
                emailTemplateId = emailQueryViewModel.EmailTemplateId,
                reviewerGroupId = emailQueryViewModel.ReviewerGroupId,
                action = emailQueryViewModel.Action,
                OptionId = emailQueryViewModel.OptionId,
                RequestedInfoId = emailQueryViewModel.RequestedInfoId
            });

            return emailQuery;
        }

        private string GenerateEmailMessage(vm.EmailTemplate emailTemplate, vm.Reviewer reviewer, vm.ReviewerGroup requestingGroup, vm.Proposal proposal)
        {
            var emailStyle = _capitalRequestServices
                .GetAllEmailTemplates(new EmailTemplateSearchFilter { Name  = "Email Style"}).Result
                .FirstOrDefault();

            var emailMessage = string.Empty;

            if (emailTemplate.Name == Constants.EMAIL_REQUEST_MORE_INFORMATION)
            {

                var body = emailTemplate.Body.Replace("[","{{ ").Replace("]"," }}");
                var firstName = reviewer.FirstName;
                var requestingGroupName = requestingGroup.Name;
                var projectName = proposal.ProjectName;
                var reqId = proposal.Id.ToString();
                var requestedInformation = proposal.RequestedInfo.RequestedInformation;

                var requestedInfoId = proposal.RequestedInfo.Id;
                var reviewerGroupId = proposal.RequestedInfo.ReviewerGroupId;

                var projectLink = GenerateProjectLink(
                 _ssmWorkFlowSettings.ProjectReviewLink,
                 proposal.Id,
                 emailTemplate.OptionType,
                 proposal.RequestedInfo?.Id,
                 reviewer.Id,
                 proposal.ReviewerGroupId,
                 proposal.RequestedInfo.RequestingReviewerGroupId
                );

                //var projectLink = $"{_ssmWorkFlowSettings.ProjectReviewLink} ?Id={reqId}";

                //if (requestedInfoId)

                var emailModel = new Dictionary<string, object>
                {
                    ["UserFirstName"] = firstName,
                    ["RequestReviewerGroup"] = requestingGroupName,
                    ["ProjectName"] = projectName,
                    ["ReqId"] = reqId,
                    ["RequestedInformation"] = requestedInformation,
                    ["ProjectLink"] = projectLink
                };

                var emailBody = TemplateHelper.Render(body, emailModel);

                emailMessage = $"{emailStyle.Body}{emailBody}";
                //emailMessage = RenderEmailTemplate(body, new
                //{
                //    UserFirstName = firstName,
                //    RequestReviewerGroup = requestingGroupName,
                //    ProjectName = projectName,
                //    ReqId = reqId,
                //    RequestedInformation = requestedInformation,
                //    ProjectLink = projectLink
                //});

            }

            return emailMessage;
        }


        public string RenderEmailTemplate(string templateText, object model)
        {
            var template = Template.Parse(templateText);
            return template.Render(model);
        }

        public string GenerateActionString(vm.ReviewerGroup reviewerGroup, vm.ReviewerGroup requestingGroup, string emailActionTemplate, string fullName)
        {
            var requestingGroupName = requestingGroup.Name;
            var requestedGroup = reviewerGroup.Name;
            var requestDate = DateTime.Now.ToString("MM/dd/yyyy");

            var model = new Dictionary<string, object>
            {
                ["fullName"] = fullName,
                ["requestingGroupName"] = requestingGroupName,
                ["requestedGroup"] = requestedGroup,
                ["requestDate"] = requestDate
            };

            var actionTemplate = Template.Parse(emailActionTemplate);

            var action = TemplateHelper.Render(emailActionTemplate,model);

            return action;
        }

        public string GenerateProjectLink(
            string baseUrl,
            int proposalId,
            string optionType,
            int? requestedInfoId,
            int? reviewerId,
            int? reviewerGroupId,
            int? requestingReviewerGroupId)
        {
            string idParam = $"?Id={proposalId}";

            // Determine parameter name
            string paramName = (requestedInfoId == null || reviewerId != null) && optionType != "AddInfo"
                ? "&ReviewerGroupId="
                : "&RequestedInfoId=";

            // Determine parameter value
            string paramValue;
            if (requestedInfoId != null && reviewerId != null && optionType == "Verify")
            {
                paramValue = requestingReviewerGroupId?.ToString();
            }
            else if (requestedInfoId != null && optionType == "AddInfo")
            {
                paramValue = requestedInfoId.ToString();
            }
            else if (requestedInfoId == null || reviewerId != null)
            {
                paramValue = reviewerGroupId?.ToString();
            }
            else
            {
                paramValue = reviewerId?.ToString();
            }

            return $"{baseUrl}{idParam}{paramName}{paramValue}&ActionType={optionType}\" target=\"_blank";
        }

        private List<vm.Reviewer> GetReviewers(vm.Proposal proposal)
        {
            return _capitalRequestServices.GetAllReviewers(new ReviewerSearchFilter { SegmentId = proposal.SegmentId }).Result;
        }


        private string FormatSqlValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "NULL" : $"'{value}'";
        }

    }

}
