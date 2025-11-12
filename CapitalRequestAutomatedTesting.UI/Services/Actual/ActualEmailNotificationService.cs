using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Extensions;
using CapitalRequestAutomatedTesting.UI.Helpers;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;
using Scriban;
using Scriban.Runtime;
using SSMWorkflow.API.DataAccess.ConfigurationSettings;
using SSMWorkflow.API.DataAccess.Models;
using System.Diagnostics;
using EmailNotification = SSMWorkflow.API.Models.EmailNotification;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Actual
{
    public interface IActualEmailNotificationService
    {

        Task<List<EmailNotification>> GetEmailNotificationsAsync(vm.Proposal proposal, string emailType, string requestingUser);
        Task<List<EmailNotification>> GetSubmitEmailNotificationsAsync(vm.Proposal proposal, ScenarioDetailsViewModel scenarioDetail);

        List<EmailNotification> FilterEmailNotifications(
            List<EmailNotification> allNotifications,
            int emailTemplateId,
            int? reviewerGroupId = null,
            int? requestedInfoId = null);

        string NormalizeHtml(string html);
    }

    public class ActualEmailNotificationService : IActualEmailNotificationService
    {
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly SSMWorkFlowSettings _ssmWorkFlowSettings;
        private readonly IMapper _mapper;

        public ActualEmailNotificationService(ICapitalRequestServices capitalRequestServices,
            ISSMWorkflowServices ssmWorkflowServices,
            IOptionsMonitor<SSMWorkFlowSettings> ssmWorkFlowSettings,
            IMapper mapper)
        {
            _capitalRequestServices = capitalRequestServices;
            _ssmWorkflowServices = ssmWorkflowServices;
            _ssmWorkFlowSettings = ssmWorkFlowSettings.CurrentValue;
            _mapper = mapper;
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

        public async Task<List<EmailNotification>> GetSubmitEmailNotificationsAsync(vm.Proposal proposal, ScenarioDetailsViewModel scenarioDetail)
        {
            var workflowStep = proposal.WorkflowStep;

            var workflowStepId = workflowStep.WorkflowStepID;
            var emailNotifications = await _ssmWorkflowServices.GetAllEmailNotifications(new EmailNotificationSearchFilter { WorkflowStepId = workflowStepId });

            var notifications = new List<EmailNotification>();
            foreach (var reviewerGroup in proposal.ReviewerGroups)
            {
                var reviewerGroupdId = reviewerGroup.Id;
                var reviewers = (await GetReviewers(proposal))
                    .Where(x => x.ReviewerGroupId == reviewerGroupdId)
                    .OrderBy(x => x.Email)
                    .Select(z => _mapper.Map<vm.Reviewer>(z))
                    .ToList();

                foreach (var reviewer in reviewers)
                {
                    var emailActionTemplate = Constants.EMAIL_TEMPLATE_INITIAL_EMAIL;
                    var fullName = reviewer.FullName;

                    var action = emailActionTemplate;

                    var emailTemplateId = (int)reviewerGroup.EmailTemplateId;
                    var emailTemplate = new vm.EmailTemplate();

                    emailTemplate = await _capitalRequestServices.GetEmailTemplate(emailTemplateId);

                    if (emailTemplate.Priority == Constants.EMAIL_PRIORITY_NORMAL)
                    {
                        var existing = notifications.FirstOrDefault(x =>
                            x.Priority == Constants.EMAIL_PRIORITY_NORMAL &&
                            x.ReviewerGroupId == reviewerGroupdId.ToString());

                        if (existing != null)
                        {
                            existing.Recipients += $", {reviewer.Email}";
                            continue;
                        }
                    }

                    var emailMessage = await GenerateEmailMessageAsync(emailTemplate, reviewer, proposal);

                    var emallQueryViewModel = new EmailQueryViewModel
                    {
                        WorkflowStepId = workflowStep.WorkflowStepID.ToString(),
                        EmailTemplateId = "0",
                        ReviewerGroupId = "0",
                        Action = "",
                        OptionId = null,
                        RequestedInfoId = null
                    };

                    var emailQuery = GenerateEmailQuery(emallQueryViewModel);
                    var emailNotification = new EmailNotification
                    {
                        WorkflowStepId = Guid.Empty,
                        WorkflowName = proposal.ProjectName,
                        WorkflowDescription = proposal.ProjectDescription,
                        WorkflowState = workflowStep.StepName,
                        StepName = workflowStep.StepName,
                        StepDescription = workflowStep.StepDescription,
                        Action = workflowStep.StepDescription,
                        EmailMessage = emailMessage,
                        Recipients = reviewer.Email,
                        Subject = emailTemplate.Subject,
                        Priority = emailTemplate.Priority,
                        EmailQuery = emailQuery,
                        ReviewerGroupId = reviewerGroupdId.ToString(),
                        Created = DateTime.Now
                    };


                    notifications.Add(emailNotification);

                }

            }

            // Update actualEmailNotifications ReviewerGroupId based on matches with emailNotifications
            var notificationLookup = notifications
                .GroupBy(en => new { 
                    EmailMessage = en.EmailMessage?.Trim(), 
                    Recipients = en.Recipients?.Trim().ToLowerInvariant() 
                })
                .ToDictionary(g => g.Key, g => g.First().ReviewerGroupId);

            foreach (var emailNotification in emailNotifications)
            {
                var key = new { 
                    EmailMessage = emailNotification.EmailMessage?.Trim(), 
                    Recipients = emailNotification.Recipients?.Trim().ToLowerInvariant() 
                };
                
                if (notificationLookup.TryGetValue(key, out var reviewerGroupId))
                {
                    emailNotification.ReviewerGroupId = reviewerGroupId;
                }
            }

            var predictiveData = scenarioDetail.PredictiveData;
            var tables = predictiveData.Tables;

            var predictiveNotificationLookup = notifications
                .GroupBy(en => new {
                    EmailMessage = en.EmailMessage?.Trim(),
                    Recipients = en.Recipients?.Trim().ToLowerInvariant()
                })
                .ToDictionary(g => g.Key, g => g.First().EmailQuery);

            if (tables.TryGetValue("EmailNotifications", out var emailNotificationsTable))
            {
                foreach (var row in emailNotificationsTable.Rows.Values)
                {
                    var emailMessage = row.Fields.TryGetValue("EmailMessage", out var emailMessageField) ? emailMessageField?.ToString() : null;
                    var recipients = row.Fields.TryGetValue("Recipients", out var recipientsField) ? recipientsField?.ToString() : null;
                    
                    var key = new
                    {
                        EmailMessage = emailMessage?.Trim(),
                        Recipients = recipients?.Trim().ToLowerInvariant()
                    };

                    if (predictiveNotificationLookup.TryGetValue(key, out var emailQuery))
                    {
                        predictiveData.SetValue("EmailNotifications", row.RowId, "EmailQuery", emailQuery);
                    }
                }
            }
            

            return emailNotifications;

        }



        public async Task<List<EmailNotification>> GetEmailNotificationsAsync(vm.Proposal proposal, string emailType, string requestingUser)
        {
            var workflowStep = proposal.WorkflowStep;
            var workflowStepId = workflowStep.WorkflowStepID;

            var reviewerGroupdId = 0;
            var requestingGroupId = 0;
            var reviewerGroup = new vm.ReviewerGroup();
            var requestingGroup = new vm.ReviewerGroup();
            var reviewers = new List<vm.Reviewer>();
            var fullName = string.Empty;

            if (emailType != Constants.EMAIL_INITIAL_EMAIL)
            {
                reviewerGroupdId = proposal.ReviewerGroupId;
                requestingGroupId = proposal.RequestingGroupId;

                reviewerGroup = await _capitalRequestServices.GetReviewerGroup(reviewerGroupdId);
                requestingGroup = await _capitalRequestServices.GetReviewerGroup(requestingGroupId);
                reviewers = (await GetReviewers(proposal))
                     .Where(x => x.ReviewerGroupId == reviewerGroupdId)
                     .Select(z => _mapper.Map<vm.Reviewer>(z))
                     .ToList();

                fullName = proposal.Reviewer.FullName;
            }

            var emailTemplate = (await _capitalRequestServices
                    .GetAllEmailTemplates(new EmailTemplateSearchFilter { Name = emailType }))
                    .FirstOrDefault();

            var workflowTemplate = (await _capitalRequestServices
                    .GetAllWorkflowTemplates(new WorkflowTemplateSearchFilter { StepName = workflowStep.StepName }))
                    .FirstOrDefault();

            var emailTemplateType = emailType == Constants.EMAIL_REQUEST_MORE_INFORMATION
                ? Constants.EMAIL_TEMPLATE_REQUEST_MORE_INFORMATION
                : emailType == Constants.EMAIL_TEMPLATE_RETURN_OF_REQUESTED_INFORMATION
                    ? Constants.EMAIL_INITIAL_EMAIL
                    : Constants.EMAIL_TEMPLATE_INITIAL_EMAIL;


            //var action = EmailNotifcationHelper.GenerateActionString(reviewerGroup.Name, requestingGroup.Name, emailTemplateType, fullName, requestingUser);
            var action = string.Empty;
            if (emailType == Constants.EMAIL_INITIAL_EMAIL)
            {
                action = Constants.EMAIL_TEMPLATE_INITIAL_EMAIL;
            }
            else
            {
                action = EmailNotifcationHelper.GenerateActionString(reviewerGroup, requestingGroup, emailTemplateType, fullName, requestingUser);
            }

            var emallQueryViewModel = new EmailQueryViewModel
            {
                WorkflowStepId = workflowStep.WorkflowStepID.ToString(),
                EmailTemplateId = emailTemplate.Id.ToString(),
                ReviewerGroupId = requestingGroupId.ToString(),
                Action = action,
                OptionId = proposal.RequestedInfo.WorkflowStepOptionId != null ? $"'{proposal.RequestedInfo.WorkflowStepOptionId}'" : "NULL",
                RequestedInfoId = proposal.RequestedInfo.Id.ToString()
            };

            var emailNotifications = new List<EmailNotification>();

            var allEmailNotifications = (await _ssmWorkflowServices.GetAllEmailNotifications(new EmailNotificationSearchFilter { WorkflowStepId = workflowStepId }))
                .Where(x => x.Created.HasValue && x.Created.Value.Date == DateTime.Now.Date)
                .ToList();

            var durationMinutes = proposal.ExecutionDurationMinutes + 5 ?? 3;
            if (emailType != Constants.EMAIL_REQUEST_MORE_INFORMATION)
            {
                reviewerGroupdId = requestingGroupId;
                reviewers = (await GetReviewers(proposal))
                     .Where(x => x.ReviewerGroupId == reviewerGroupdId)
                     .Select(z => _mapper.Map<vm.Reviewer>(z))
                     .ToList();
            }

            var relevantNotifications = new List<EmailNotification>();

            if (emailType == Constants.EMAIL_INITIAL_EMAIL)
            {
                relevantNotifications = allEmailNotifications
                    .Where(x => x.EmailQueryDetails.WorkflowStepId == workflowStepId.ToString() &&
                                x.Created.HasValue && x.Created.Value.IsFuzzyMatch(DateTime.Now, durationMinutes))
                    .ToList();

            }
            else
            {
                relevantNotifications = allEmailNotifications
                    .Where(x => x.EmailQueryDetails.WorkflowStepId == workflowStepId.ToString() &&
                                x.EmailQueryDetails.EmailTemplateId == emailTemplate.Id.ToString() &&
                                x.EmailQueryDetails.ReviewerGroupId == reviewerGroupdId.ToString() &&
                                x.EmailQueryDetails.RequestedInfoId == proposal.RequestedInfo.Id.ToString() &&
                                x.Created.HasValue && x.Created.Value.IsFuzzyMatch(DateTime.Now, durationMinutes))
                    .ToList();

            }

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
            return await _capitalRequestServices.GetAllReviewers(new ReviewerSearchFilter { SegmentId = proposal.SegmentId, RegionId = proposal.Region });
        }

        public async Task<string> GenerateEmailMessageAsync(vm.EmailTemplate emailTemplate, vm.Reviewer reviewer, vm.Proposal proposal)
        {
            var emailStyle = (await _capitalRequestServices
                .GetAllEmailTemplates(new EmailTemplateSearchFilter { Name = "Email Style" }))
                .FirstOrDefault();

            var emailMessage = string.Empty;

            var body = emailTemplate.Body.Replace("[", "{{ ").Replace("]", " }}");
            var firstName = reviewer.FirstName;
            var projectName = proposal.ProjectName;
            var reqId = proposal.Id.ToString();
            var reviewerGroupId = reviewer.ReviewerGroupId.Value;
            var reviewerGroup = await _capitalRequestServices.GetReviewerGroup(reviewerGroupId);

            var projectLink = GenerateProjectLink(
                     _ssmWorkFlowSettings.ProjectReviewLink,
                     proposal.Id,
                     emailTemplate.OptionType,
                     null,
                     null,
                     reviewerGroupId,
                     null
                );

            var emailModel = new Dictionary<string, object>
            {
                ["UserFirstName"] = firstName,
                ["ReviewerGroup"] = reviewerGroup.Name,
                ["ProjectName"] = projectName,
                ["ReqId"] = reqId,
                ["ProjectLink"] = projectLink
            };

            var emailBody = TemplateHelper.Render(body, emailModel);

            emailMessage = $"{emailStyle.Body}{emailBody}";

            return emailMessage;
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
            string paramName;
            string paramValue;
            paramName = "&ReviewerGroupId=";

            paramValue = reviewerGroupId?.ToString();

            //.http://caps-dev.ssmhc.com/CapitalRequest/Proposal/Review?Id=2943&ReviewerGroupId=2&ActionType=Verify

            //.http://caps-dev.ssmhc.com/CapitalRequest/Proposal/Review?Id=2943&ReviewerGroupId=1&ActionType=Notify

            return $"{baseUrl}{idParam}{paramName}{paramValue}&ActionType={optionType}\" target=\"_blank";
        }

        private string GenerateEmailQuery(EmailQueryViewModel emailQueryViewModel)
        {

            var sql = Template.Parse(SqlTemplates.CapitalRequestNotification);
            //"EXECUTE dbo.GetCapitalRequestGroupNotifications NULL,'{{ workflowStepId }}','{{ emailTemplateId }}','{{ reviewerGroupId }}','{{ action }}.',{{ optionId }},'{{ requestedInfoId }}'"

            var workflowStepId = emailQueryViewModel.WorkflowStepId != null
                ? emailQueryViewModel.WorkflowStepId.ToString()
                : "";

            var emailTemplateId = Convert.ToInt32(emailQueryViewModel.EmailTemplateId) > 0
                ? emailQueryViewModel.EmailTemplateId.ToString()
                : "NULL";

            var reviewerGroupId = Convert.ToInt32(emailQueryViewModel.ReviewerGroupId) > 0
                ? emailQueryViewModel.ReviewerGroupId.ToString()
                : "NULL";

            var action = string.IsNullOrWhiteSpace(emailQueryViewModel.Action)
                ? "NULL"
                : emailQueryViewModel.Action;

            var optionId = emailQueryViewModel.OptionId != null
                ? emailQueryViewModel.OptionId.ToString()
                : "NULL";

            var requestedInfoId = emailQueryViewModel.RequestedInfoId != null
                ? emailQueryViewModel.RequestedInfoId.ToString()
                : "NULL";

            Debug.WriteLine($"SqlTemplates.CapitalRequestNotification: {SqlTemplates.CapitalRequestNotification}");
            Debug.WriteLine($"workflowStepId: {workflowStepId}");
            Debug.WriteLine($"emailTemplateId: {emailTemplateId}");
            Debug.WriteLine($"reviewerGroupId: {reviewerGroupId}");
            Debug.WriteLine($"action: {action}");
            Debug.WriteLine($"optionId: {optionId}");
            Debug.WriteLine($"requestedInfoId: {requestedInfoId}");

            var context = new TemplateContext();
            context.PushGlobal(new ScriptObject
            {
                { "workflowStepId", workflowStepId },
                { "emailTemplateId", emailTemplateId },
                { "reviewerGroupId", reviewerGroupId },
                { "action", action },
                { "optionId", optionId },
                { "requestedInfoId", requestedInfoId }
            });

            var emailQuery = sql.Render(context);

            return emailQuery;
        }
    }

}
