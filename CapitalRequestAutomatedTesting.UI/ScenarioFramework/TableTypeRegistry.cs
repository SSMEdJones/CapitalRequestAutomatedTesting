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
            { "Attachment", typeof(List<AttachmentModel>) },
            { "Workflow", typeof(WorkflowModel) },
            { "WorkflowStep", typeof(WorkflowStepModel) },
            { "WorkflowInstance", typeof(WorkflowInstanceModel) },
            { "WorkflowStakeHolder", typeof(List<WorkflowStakeholderModel?>) },

        // Add more table-to-type mappings here
        };
    }
}
