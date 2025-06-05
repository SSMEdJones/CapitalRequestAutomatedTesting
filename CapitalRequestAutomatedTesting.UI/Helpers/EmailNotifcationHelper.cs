using CapitalRequestAutomatedTesting.UI.Models;
using HtmlAgilityPack;
using SSMWorkflow.API.Models;

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

        public static ParsedEmailNotification? ParseEmailQuery(string emailQuery, int id)
        {
            try
            {
                // Remove "EXECUTE dbo.GetCapitalRequestGroupNotifications" and split by commas
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
                    EmailNotificationId = id,
                    WorkflowStepId = Guid.Parse(parts[1]),
                    EmailTemplateId = int.TryParse(parts[2], out var emailTemplateId) ? emailTemplateId : 0,
                    ReviewerGroupId = int.TryParse(parts[3], out var reviewerGroupId) ? reviewerGroupId : (int?)null,
                    Message = parts[4],
                    RequestedInfoId = int.TryParse(parts[6], out var requestedInfoId) ? requestedInfoId : (int?)null
                };
            }
            catch (Exception ex)
            {
                // Optionally log or handle parsing errors
                return null;
            }
        }

    }
}
