using CapitalRequest.API.DataAccess.Models;
using SSMWorkflow.API.DataAccess.Models;

namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class ScenarioDataViewModel
    {
        public RequestedInfo RequestedInfo { get; internal set; }
        public WorkflowStepResponder WorkflowStepResponder { get; internal set; }
        public object WorkflowStepOptions { get; internal set; }
        public object EmailNotifications { get; internal set; }
    }
}