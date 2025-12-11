using System.ComponentModel.DataAnnotations.Schema;

namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class WorkflowInstanceActionHistoryModel
    {
        [RowKey]
        public Guid WorkflowInstanceID { get; set; }

        public Guid WorkflowStepID { get; set; }

        [RowKey]
        public string Action { get; set; }

        public DateTime Completed { get; set; }

        public string CompletedBy { get; set; }

    }
}
