using CapitalRequestAutomatedTesting.UI.Enums;

namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class PredictiveMethod
    {
        public int Id { get; set; }
        public int StepNumber { get; set; }
        public string StepName { get; set; }

        public int Order { get; set; }

        public string ScenarioId { get; set; }

        // Predictive execution
        public string ServiceName { get; set; } = string.Empty;
        public string MethodName { get; set; } = string.Empty;
        public List<object>? Parameters { get; set; }
        public CrudOperationType Operation { get; set; }

        // ✅ Refactored rollback
        public RollbackMethod? Rollback { get; set; }
    }


}
