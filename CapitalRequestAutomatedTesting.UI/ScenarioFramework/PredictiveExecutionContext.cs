using CapitalRequestAutomatedTesting.UI.Enums;

namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class PredictiveExecutionContext
    {
        public int StepNumber { get; set; }
        public string ServiceName { get; set; }
        public string MethodName { get; set; }
        public CrudOperationType OperationType { get; set; }
        public object Result { get; set; }
    }

}
