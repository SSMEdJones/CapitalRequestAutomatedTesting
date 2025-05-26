namespace CapitalRequest.API.DataAccess.Models
{
    public class ReviewerGroupSearchFilter
    {
        public string? Name { get; set; }
        public int? EmailTemplateId { get; set; }
        public string? ReviewerType { get; set; }
        public int? StepNumber { get; set; }
        public bool? AdminReviewer { get; set; }
    }
}
