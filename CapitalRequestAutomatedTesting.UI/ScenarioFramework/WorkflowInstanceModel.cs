namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class WorkflowInstanceModel
    {

        [RowKey]
        public Guid WorkflowID { get; set; }

        public Guid CurrentWorkflowStepID { get; set; }

        public string CurrentWorkflowState { get; set; }

        public DateTime Created { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? Updated { get; set; }

        public string UpdatedBy { get; set; }

    }
}
