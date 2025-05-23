namespace SSMWorkflow.API.DataAccess.Models
{
    public class CreateUpdateWorkFlowStakeholder
    {
        public Guid WorkflowID { get; set; }
        public bool isGroup { get; set; }
        public bool isExternal { get; set; }

        public string Stakeholder { get; set; }

        public DateTime Created { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? Updated { get; set; }

        public string UpdatedBy { get; set; }

    }
}