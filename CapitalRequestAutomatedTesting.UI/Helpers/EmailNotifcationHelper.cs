using CapitalRequestAutomatedTesting.UI.Models;
using HtmlAgilityPack;
using Scriban;
using SSMWorkflow.API.Models;
using System.Text.RegularExpressions;

namespace CapitalRequestAutomatedTesting.UI.Helpers
{
    public static class EmailNotifcationHelper
    {

        public static List<EmailNotification> FilterRelevantNotifications(
        List<EmailNotification> allNotifications,
        int emailTemplateId,
        int? reviewerGroupId = null,
        int? requestedInfoId = null)
        {
            var relevant = new List<EmailNotification>();

            foreach (var notification in allNotifications)
            {
                var parsed = ParseEmailQuery(notification.EmailQuery, notification.Id);
                if (parsed == null)
                    continue;

                if (parsed.EmailTemplateId != emailTemplateId)
                    continue;

                if (reviewerGroupId.HasValue && parsed.ReviewerGroupId != reviewerGroupId.Value)
                    continue;

                if (requestedInfoId.HasValue && parsed.RequestedInfoId != requestedInfoId.Value)
                    continue;

                relevant.Add(notification);
            }

            return relevant;
        }

        public static ParsedEmailNotification? ParseEmailQuery(string emailQuery, int? id)
        {
            try
            {
                // Remove "EXECUTE dbo.GetCapitalRequestGroupNotifications" and split by commas
//EXECUTE dbo.GetCapitalRequestGroupNotifications
//NULL,'57b740bd-1f2a-f011-a318-0050569736fd','3','3','Edward Jones from IT requested more information from Facilities on 5/30/2025.',NULL,'667'

                var parts = emailQuery
                    .Replace("EXECUTE dbo.GetCapitalRequestGroupNotifications", string.Empty)
                    .Trim()
                    .Split(',')
                    .Select(p => p.Trim().Trim('\''))
                    .ToList();

                if (parts.Count < 7)
                    return null;

                return new ParsedEmailNotification
                {
                    EmailNotificationId = id.Value,
                    WorkflowStepId = Guid.Parse(parts[1]),
                    EmailTemplateId = int.TryParse(parts[2], out var emailTemplateId) ? emailTemplateId : 0,
                    ReviewerGroupId = int.TryParse(parts[3], out var reviewerGroupId) ? reviewerGroupId : (int?)null,
                    Message = parts[4],
                    OptionId = Guid.Parse(parts[5]),
                    RequestedInfoId = int.TryParse(parts[6], out var requestedInfoId) ? requestedInfoId : (int?)null
                };
            }
            catch (Exception ex)
            {
                // Optionally log or handle parsing errors
                return null;
            }
        }

        public static string GenerateActionString(string  reviewerGroupName, string requestingGroupName, string emailActionTemplate, string fullName)
        {
            var requestDate = DateTime.Now.ToString("MM/dd/yyyy");

            var model = new Dictionary<string, object>
            {
                ["fullName"] = fullName,
                ["requestingGroupName"] = requestingGroupName,
                ["requestedGroup"] = reviewerGroupName,
                ["requestDate"] = requestDate
            };

            var actionTemplate = Template.Parse(emailActionTemplate);

            var action = TemplateHelper.Render(emailActionTemplate, model);

            return action;
        }

        public static EmailActionData ParseActionString(string actionString)
        {
            var pattern = @"(?<fullName>.*?) from (?<requestingGroupName>.*?) requested more information from (?<requestedGroup>.*?) on (?<requestDate>\d{1,2}/\d{1,2}/\d{4})\.";

            var match = Regex.Match(actionString, pattern);

            if (!match.Success) return null; // Handle invalid cases

            return new EmailActionData
            {
                FullName = match.Groups["fullName"].Value,
                RequestingGroupName = match.Groups["requestingGroupName"].Value,
                RequestedGroup = match.Groups["requestedGroup"].Value,
                RequestDate = match.Groups["requestDate"].Value
            };
        }

    }
}
