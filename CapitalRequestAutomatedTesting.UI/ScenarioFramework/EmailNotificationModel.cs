namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class EmailNotificationModel
    {
        public Guid WorkflowStepId { get; set; }
        public string WorkflowName { get; set; }
        public string WorkflowDescription { get; set; }
        public string WorkflowState { get; set; }
        public string StepName { get; set; }
        public string StepDescription { get; set; }
        public string Action { get; set; }
        public string EmailMessage { get; set; }
        [RowKey]
        public string Recipients { get; set; }
        public string Subject { get; set; }
        public string Priority { get; set; }
        public string EmailQuery { get; set; }
        public DateTime? Created { get; set; }

    }
}
