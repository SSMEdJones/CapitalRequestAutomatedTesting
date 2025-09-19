using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Extensions;
using CapitalRequestAutomatedTesting.UI.Helpers;
using CapitalRequestAutomatedTesting.UI.Models;
using HtmlAgilityPack;
using SSMWorkflow.API.DataAccess.Models;
using EmailNotification = SSMWorkflow.API.Models.EmailNotification;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Rollback
{
    public interface IRollbackEmailNotificationService
    {
        Task DeleteNotificationsAsync(vm.Proposal proposal, string emailType);
        //Task<List<EmailNotification>> GetRequestEmailNotificationsAsync(vm.Proposal proposal, string emailType);
        //List<EmailNotification> FilterEmailNotifications(
        //    List<EmailNotification> allNotifications,
        //    int emailTemplateId,
        //    int? reviewerGroupId = null,
        //    int? requestedInfoId = null);

        //string NormalizeHtml(string html);
    }

    public class RollbackEmailNotificationService : IRollbackEmailNotificationService
    {
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly IUserContextService _userContextService;
        private IMapper _mapper;

        public RollbackEmailNotificationService(ICapitalRequestServices capitalRequestServices,ISSMWorkflowServices ssmWorkflowServices, IUserContextService userContextService, IMapper mapper)
        {
            _capitalRequestServices = capitalRequestServices;
            _ssmWorkflowServices = ssmWorkflowServices;
            _userContextService = userContextService;
            _mapper = mapper;
        }

        public async Task DeleteNotificationsAsync(vm.Proposal proposal, string emailType)
        {
            var workflowSteps = await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId);
            var workflowStep = proposal.WorkflowStep;
            var workflowStepId = workflowStep.WorkflowStepID;
            var reviewerGroupdId = proposal.RequestedInfo.ReviewerGroupId;
            var requestingGroupId = proposal.RequestedInfo.RequestingReviewerGroupId;
            var emailTemplate = (await _capitalRequestServices
                .GetAllEmailTemplates(new EmailTemplateSearchFilter { Name = emailType }))
                .FirstOrDefault();
            if (emailTemplate == null)
            {
                throw new Exception($"Email template '{emailType}' not found.");
            }
            var allEmailNotifications = await _ssmWorkflowServices.GetAllEmailNotifications(new EmailNotificationSearchFilter { WorkflowStepId = workflowStepId });
            // Filter notifications based on the email template and reviewer group
            var relevantNotifications = allEmailNotifications
                .Where(x => x.EmailQueryDetails.WorkflowStepId == workflowStepId.ToString() &&
                            x.EmailQueryDetails.EmailTemplateId == emailTemplate.Id.ToString() &&
                            x.EmailQueryDetails.ReviewerGroupId == reviewerGroupdId.ToString() &&
                            x.EmailQueryDetails.RequestedInfoId == proposal.RequestedInfo.Id.ToString())
                .ToList();

            foreach (var notification in relevantNotifications)
            {
                await _ssmWorkflowServices.DeleteEmailNotification(notification);
            }
        }

        public List<EmailNotification> FilterEmailNotifications(
            List<EmailNotification> allNotifications,
            int emailTemplateId,
            int? reviewerGroupId = null,
            int? requestedInfoId = null)
        {
            var relevantNotifications = EmailNotifcationHelper.FilterRelevantNotifications(allNotifications, emailTemplateId, reviewerGroupId, requestedInfoId);

            return relevantNotifications;
        }

        public async Task<List<EmailNotification>> GetRequestEmailNotificationsAsync(vm.Proposal proposal, string emailType, string requestingUser)
        {
            var workflowSteps = await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId);
            var workflowStep = _mapper.Map<WorkflowStep>(workflowSteps.FirstOrDefault(x => !x.IsComplete));

            var workflowStepId = workflowStep.WorkflowStepID;

            var reviewerGroupdId = proposal.RequestedInfo.ReviewerGroupId;
            var requestingGroupId = proposal.RequestedInfo.RequestingReviewerGroupId;

            var reviewerGroup = await _capitalRequestServices.GetReviewerGroup(reviewerGroupdId);
            var requestingGroup = await _capitalRequestServices.GetReviewerGroup(requestingGroupId);

            var emailTemplate = (await _capitalRequestServices
                .GetAllEmailTemplates(new EmailTemplateSearchFilter { Name = emailType }))
                .FirstOrDefault();

            var workflowTemplate = (await _capitalRequestServices
                .GetAllWorkflowTemplates(new WorkflowTemplateSearchFilter { StepName = workflowStep.StepName }))
                .FirstOrDefault();

            var reviewers = (await GetReviewers(proposal))
                .Where(x => x.ReviewerGroupId == reviewerGroupdId)
                .Select(z => _mapper.Map<vm.Reviewer>(z))
                .ToList();
            
            var fullName = $"{_userContextService.FirstName} {_userContextService.LastName}";

            var action = EmailNotifcationHelper.GenerateActionString(reviewerGroup, requestingGroup, Constants.EMAIL_TEMPLATE_REQUEST_MORE_INFORMATION, fullName, requestingUser);

            var emallQueryViewModel = new EmailQueryViewModel
            {
                WorkflowStepId = workflowStep.WorkflowStepID.ToString(),
                EmailTemplateId = emailTemplate.Id.ToString(),
                ReviewerGroupId = reviewerGroupdId.ToString(),
                Action = action,
                OptionId = proposal.RequestedInfo.WorkflowStepOptionId != null ? $"'{proposal.RequestedInfo.WorkflowStepOptionId}'" : "NULL",
                RequestedInfoId = proposal.RequestedInfo.Id.ToString()
            };
            //EXECUTE dbo.GetCapitalRequestGroupNotifications NULL,'57b740bd-1f2a-f011-a318-0050569736fd','3','3','Edward Jones from IT requested more information from Facilities on 5/30/2025.',NULL,'667'

            var emailNotifications = new List<EmailNotification>();

            var allEmailNotifications = await _ssmWorkflowServices.GetAllEmailNotifications(new EmailNotificationSearchFilter { WorkflowStepId = workflowStepId });

            //TODO uncomment date match
            var relevantNotifications = allEmailNotifications
                .Where(x => x.EmailQueryDetails.WorkflowStepId == workflowStepId.ToString() &&
                            x.EmailQueryDetails.EmailTemplateId == emailTemplate.Id.ToString() &&
                            x.EmailQueryDetails.ReviewerGroupId == reviewerGroupdId.ToString() &&
                            x.EmailQueryDetails.RequestedInfoId == proposal.RequestedInfo.Id.ToString()
                            // &&  x.Created.HasValue && x.Created.Value.IsFuzzyMatch(DateTime.Now, 3)
                            )
                .ToList();

            emailNotifications = (from data in relevantNotifications
                                  from recipient in data.Recipients.Split(',')
                                  join reviewer in reviewers on recipient.Trim() equals reviewer.Email
                                  select data)
                        .Distinct()
                        .ToList();

            return emailNotifications;
        }

        public string NormalizeHtml(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html ?? string.Empty);

            void NormalizeNode(HtmlNode node)
            {
                if (node.NodeType == HtmlNodeType.Text)
                {
                    node.InnerHtml = HtmlEntity.DeEntitize(node.InnerText.Trim());
                }

                if (node.HasAttributes)
                {
                    var sortedAttributes = node.Attributes.OrderBy(a => a.Name).ToList();
                    node.Attributes.RemoveAll();
                    foreach (var attr in sortedAttributes)
                    {
                        node.Attributes.Add(attr);
                    }
                }

                foreach (var child in node.ChildNodes)
                {
                    NormalizeNode(child);
                }
            }

            NormalizeNode(doc.DocumentNode);
            return doc.DocumentNode.OuterHtml;
        }

        private async Task<List<vm.Reviewer>> GetReviewers(vm.Proposal proposal)
        {
            return await _capitalRequestServices.GetAllReviewers(new ReviewerSearchFilter { SegmentId = proposal.SegmentId });
        }

        

    }
}