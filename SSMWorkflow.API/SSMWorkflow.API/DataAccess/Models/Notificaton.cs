namespace SSMWorkflow.API.DataAccess.Models
{
    public class Notification
    {
        public Guid WorkflowInstanceID { get; set; }
        public Guid WorkflowStepID { get; set; }

        public string WorkflowName { get; set; }

        public string WorkflowDescription { get; set; }

        public string WorkflowState { get; set; }

        public string StepName { get; set; }

        public string StepDescription { get; set; }

        public string Action { get; set; }

        public string EmailMessage { get; set; }

        public string Recipients { get; set; }

        public string Requester { get; set; }

        public string Subject { get; set; }

        public string Priority { get; set; }
    }
}