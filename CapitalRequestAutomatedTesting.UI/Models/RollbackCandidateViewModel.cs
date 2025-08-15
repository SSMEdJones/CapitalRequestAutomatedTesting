using CapitalRequestAutomatedTesting.UI.ScenarioFramework;

namespace CapitalRequestAutomatedTesting.UI.Models
{
    public class RollbackCandidateViewModel
    {
        public int StepNumber { get; set; }
        public ScenarioDataViewModel OriginalData { get; set; } = new();
        public ScenarioDataViewModel ActualData { get; set; } = new();

        public string MethodName { get; set; }
        public string RollbackMethodName { get; set; }
        public string Description { get; set; }
        public bool IsSelectedForRollback { get; set; }

        public Dictionary<string, string> PredictiveData { get; set; }
        public Dictionary<string, string> RollbackParameters { get; set; }
    }

}
