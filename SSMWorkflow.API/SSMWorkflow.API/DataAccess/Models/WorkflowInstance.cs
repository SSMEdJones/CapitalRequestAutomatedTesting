namespace SSMWorkflow.API.DataAccess.Models
{
    public class WorkflowInstance
    {
        public Guid WorkflowInstanceID { get; set; }
        public Guid WorkflowID { get; set; }
        public Guid CurrentWorkflowStepID { get; set; }

        public string CurrentWorkflowState { get; set; }

        public DateTime Created { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? Updated { get; set; }

        public string UpdatedBy { get; set; }

    }
}