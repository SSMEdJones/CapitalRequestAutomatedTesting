namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public static class TableTypeRegistry
    {
        public static readonly Dictionary<string, Type> TableTypes = new()
        {
            { "RequestedInfo", typeof(RequestedInfoModel) },
            { "WorkflowStepResponder", typeof(WorkflowStepResponderModel) },
            { "WorkflowStepOption", typeof(List<WorkflowStepOptionModel>) },
            { "EmailNotification", typeof(List<EmailNotificationModel>) },
            { "Attachment", typeof(List<AttachmentModel>) }
        // Add more table-to-type mappings here
        };
    }
}
