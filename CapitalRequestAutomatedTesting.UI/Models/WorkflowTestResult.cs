namespace CapitalRequestAutomatedTesting.UI.Models
{
    public class WorkflowTestResult
    {
        public bool Passed { get; set; }
        public string Message { get; set; }
        public List<string> RequestIds { get; set; }
        public List<WorkflowAction> WorkflowActions { get; set; }
        public List<DashboardModel> Dashboard { get; set; }
    }
}
