namespace CapitalRequestAutomatedTesting.UI.Models
{
    public class WorkflowTestContext
    {
        public string ReqId { get; set; }
        public string Identifier { get; set; }
        public string ButtonText { get; set; }
        public string WaitForElementId { get; set; }

        // New fields for impersonation and additional inputs
        public string ImpersonatedUserId { get; set; }
        public string SelectedAction { get; set; }
        public string RequestedFrom { get; set; }
        public string RequestDetails { get; set; }

        // Optional: Step metadata
        public string StepType { get; set; }
        public string MethodName { get; set; }
    }
}
