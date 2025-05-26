namespace CapitalRequest.API.DataAccess.Models
{
    public class RequestedInfoSearchFilter
    {
        public int? ProposalId { get; set; }
        public int? RequestingReviewerGroupId { get; set; }
        public int? RequestingReviewerId { get; set; }
        public int? ReviewerGroupId { get; set; }
        public Guid? WorkflowStepOptionId { get; set; }
        public bool? IsOpen { get; set; }
    }
}
