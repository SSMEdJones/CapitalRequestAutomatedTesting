namespace CapitalRequestAutomatedTesting.UI.Models
{

    public class DashboardInitializationResult
    {
        public List<CapitalRequest.API.Models.WorkflowAction> WorkflowActions { get; set; }
        public List<SSMWorkflow.API.Models.Dashboard> DashboardItems { get; set; }
    }

}
