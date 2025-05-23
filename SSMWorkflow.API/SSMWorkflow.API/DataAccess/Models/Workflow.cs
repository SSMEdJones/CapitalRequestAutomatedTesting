namespace SSMWorkflow.API.DataAccess.Models
{
    public class Workflow
    {
        public Guid WorkflowID { get; set; }

        public string WorkflowName { get; set; }


        public string WorkflowDescription { get; set; }


        public string StakeholderNotificationType { get; set; }


        public string CompleteMessage { get; set; }


        public string CancelledMessage { get; set; }

        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public string? ProjectReviewLink { get; set; }
        public DateTime Created { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? Updated { get; set; }

        public string UpdatedBy { get; set; }

    }
}