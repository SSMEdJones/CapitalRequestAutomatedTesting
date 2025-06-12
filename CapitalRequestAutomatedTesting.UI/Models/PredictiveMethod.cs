namespace CapitalRequestAutomatedTesting.UI.Models
{
    public class PredictiveMethod
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public string ScenarioId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string MethodName { get; set; } = string.Empty;
        public string ParameterList { get; set; } = string.Empty;
        public string[]? Parameters { get; internal set; }
    }
}
