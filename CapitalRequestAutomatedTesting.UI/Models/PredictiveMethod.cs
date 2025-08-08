using CapitalRequestAutomatedTesting.UI.Enums;

namespace CapitalRequestAutomatedTesting.UI.Models
{
    public class PredictiveMethod
    {
        public int Id { get; set; }
        public int StepNumber { get; set; }
        public int Order { get; set; }

        public string ScenarioId { get; set; }

        // Predictive execution
        public string ServiceName { get; set; } = string.Empty;
        public string MethodName { get; set; } = string.Empty;
        public List<object>? Parameters { get; set; }
        public CrudOperationType Operation { get; set; }

        // Rollback execution
        public string? RollbackServiceName { get; set; }
        public string? RollbackMethodName { get; set; }
        public List<object>? RollbackParameters { get; set; }
        public CrudOperationType? RollbackOperation { get; set; }
    }

}
