namespace CapitalRequestAutomatedTesting.UI.Models
{
    public class ParsedEmailNotification
    {
        public int EmailNotificationId { get; set; }
        public Guid WorkflowStepId { get; set; }
        public int? EmailTemplateId { get; set; }
        public int? ReviewerGroupId { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? OptionId { get; set; }
        public int? RequestedInfoId { get; set; }
    }
}
