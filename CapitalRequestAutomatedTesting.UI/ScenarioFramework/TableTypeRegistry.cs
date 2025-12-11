namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public static class TableTypeRegistry
    {
        public static readonly Dictionary<string, Type> TableTypes = new()
        {
            { "RequestedInfo", typeof(RequestedInfoModel) },
            { "ProvidedInfo", typeof(ProvidedInfoModel) },
            { "WorkflowStepResponder", typeof(WorkflowStepResponderModel) },
            { "WorkflowStepOption", typeof(List<WorkflowStepOptionModel>) },
            { "EmailNotification", typeof(List<EmailNotificationModel>) },
            { "Attachment", typeof(List<AttachmentModel>) },
            { "Workflow", typeof(WorkflowModel) },
            { "WorkflowStep", typeof(WorkflowStepModel) },
            { "WorkflowInstance", typeof(WorkflowInstanceModel) },
            { "WorkflowInstanceActionHistory", typeof(List<WorkflowInstanceActionHistoryModel>) }, // ✅ FIXED
            { "WorkflowStakeHolder", typeof(List<WorkflowStakeholderModel?>) },
            { "Wbs", typeof(List<WbsModel>) },
        };
    }
}
