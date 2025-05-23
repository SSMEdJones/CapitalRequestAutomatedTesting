namespace SSMWorkflow.API.DataAccess.Models
{
    public class WorkflowStepResponder
    {
        public Guid WorkflowStepID { get; set; }
        public Guid ResponderID { get; set; }
        public bool isGroup { get; set; }

        public string Responder { get; set; }

        public int? ReviewerGroupId { get; set; }
        public Guid WorkflowStepOptionID { get; set; }

        public string ResponderType { get; set; }

        public DateTime Created { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? Updated { get; set; }

        public string UpdatedBy { get; set; }

    }
}