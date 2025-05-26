namespace CapitalRequest.API.DataAccess.Models
{
    public class ReviewerSearchFilter
    {
        public string? Email { get; set; }
        public int? RegionId { get; set; }
        public int? SegmentId { get; set; }
        public int? ReviewerGroupId { get; set; }
        public int? StepNumber { get; set; }
        public bool? IsVpOfOps { get; set; }

    }
}
