namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class RequestedInfoModel
    {
        public int ProposalId { get; set; }

        public int? RequestingReviewerGroupId { get; set; }

        public int? RequestingReviewerId { get; set; }

        public int ReviewerGroupId { get; set; }

        public string RequestedInformation { get; set; }

        public string Action { get; set; }

        public Guid? WorkflowStepOptionId { get; set; }

        public bool? IsOpen { get; set; }

        public DateTime Created { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? Updated { get; set; }

        public string UpdatedBy { get; set; }
    }
}
