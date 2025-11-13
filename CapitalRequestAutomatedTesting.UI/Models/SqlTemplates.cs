namespace CapitalRequestAutomatedTesting.UI.Models
{

    public static class SqlTemplates
    {
        public static readonly string CapitalRequestNotification = @"EXECUTE dbo.GetCapitalRequestGroupNotifications NULL,'{{ workflowStepId }}','{{ emailTemplateId }}','{{ reviewerGroupId }}','{{ action }}.',{{ optionId }},'{{ requestedInfoId }}'";
        public static readonly string CapitalRequestSubmitNotification = @"EXECUTE dbo.GetCapitalRequestGroupNotifications NULL,'{{ workflowStepId }}',{{ emailTemplateId }},{{ reviewerGroupId }},{{ action }},{{ optionId }},{{ requestedInfoId }}";
    }
}
