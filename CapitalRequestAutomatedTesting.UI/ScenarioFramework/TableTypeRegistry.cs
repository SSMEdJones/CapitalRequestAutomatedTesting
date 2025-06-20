using CapitalRequest.API.DataAccess.Models;
using SSMWorkflow.API.DataAccess.Models;

namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public static class TableTypeRegistry
    {
        public static readonly Dictionary<string, Type> TableTypes = new()
        {
            { "RequestedInfo", typeof(RequestedInfoModel) },
            { "WorkflowStepResponder", typeof(WorkflowStepResponderModel) },
            { "WorkflowStepOption", typeof(List<WorkflowStepOptionModel>) },
            { "EmailNotification", typeof(List<EmailNotificationModel>) }
        // Add more table-to-type mappings here
        };
    }
}
