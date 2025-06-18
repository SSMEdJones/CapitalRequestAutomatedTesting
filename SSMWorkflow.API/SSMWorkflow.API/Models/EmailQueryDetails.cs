#nullable disable

namespace SSMWorkflow.API.Models
{
    public class EmailQueryDetails
    {
        public string WorkflowStepId { get; set; }
        public string EmailTemplateId { get; set; }
        public string ReviewerGroupId { get; set; }
        public string Action { get; set; }
        public string OptionId { get; set; }
        public string RequestedInfoId { get; set; }
    }
}
