namespace SSMWorkflow.API.DataAccess.Models
{
    public class WorkflowStepOption
    {
        public Guid WorkflowStepID { get; set; }
        public Guid OptionID { get; set; }

        public string OptionName { get; set; }

        public int? NumberRequired { get; set; }
        public Guid? NextStepID { get; set; }
        public bool IsComplete { get; set; }
        public bool IsTerminate { get; set; }
        public int? ReviewerGroupId { get; set; }

        public string OptionType { get; set; }

        public int? RequestedInfoId { get; set; }
        public DateTime Created { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? Updated { get; set; }

        public string UpdatedBy { get; set; }

    }
}