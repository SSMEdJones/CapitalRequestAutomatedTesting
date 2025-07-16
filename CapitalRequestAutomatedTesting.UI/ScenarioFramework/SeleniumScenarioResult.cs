using Microsoft.AspNetCore.Http;

namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class SeleniumScenarioResult
    {
        public string ScenarioName { get; set; }
        public List<SeleniumScenarioStep> Steps { get; set; } = new();
        public bool Passed => Steps.All(s => s.Result?.Success == true);
        public List<string> Messages { get; set; } = new();
        public bool Success { get; set; }
        public int PredictiveCompletionStep { get; set; }
        public string PredictiveStopReason { get; set; }

    }
}
