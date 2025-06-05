namespace CapitalRequestAutomatedTesting.UI.Models
{

    public static class SqlTemplates
    {
        public static readonly string CapitalRequestNotification = @"
EXECUTE dbo.GetCapitalRequestGroupNotifications 
    NULL,
    '{{ workflowStepId }}',
    '{{ emailTemplateId }}',
    '{{ reviewerGroupId }}',
    '{{ action }}.',
    {{ optionId }},
    '{{ RequestedInfoId }}'
";
    }

}
