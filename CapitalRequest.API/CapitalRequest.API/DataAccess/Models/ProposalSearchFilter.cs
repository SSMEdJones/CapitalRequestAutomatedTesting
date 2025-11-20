namespace CapitalRequest.API.DataAccess.Models
{
    public class ProposalSearchFilter
    {
        public string? ProjectName { get; set; }
        public int? Region { get; set; }
        public int? SegmentId { get; set; }
        public int? CapitalPool { get; set; }
        public int? CapitalPoolIdentifiers { get; set; }
        public bool? OverrideWorkflow { get; set; }
        public string? UserId { get; set; }
        public DateTime? Overridden { get; set; }
        public string? OverriddenBy { get; set; }


    }
}
