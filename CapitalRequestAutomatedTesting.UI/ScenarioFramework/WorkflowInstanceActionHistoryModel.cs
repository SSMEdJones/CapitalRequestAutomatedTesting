using System.ComponentModel.DataAnnotations.Schema;

namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class WorkflowInstanceActionHistoryModel
    {

        public Guid WorkflowInstanceID { get; set; }

        public Guid WorkflowStepID { get; set; }

        public Guid? OptionID { get; set; }

        public string Action { get; set; }

        public DateTime Completed { get; set; }

        public string CompletedBy { get; set; }

    }
}
