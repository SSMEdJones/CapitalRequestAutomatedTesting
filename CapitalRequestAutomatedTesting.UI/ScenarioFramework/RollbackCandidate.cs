namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class RollbackCandidate
    {
        public int StepNumber { get; set; }

        public string MethodName { get; set; }
        public string Description { get; set; }

        public string RollbackServiceName { get; set; }
        public string RollbackMethodName { get; set; }
        public Dictionary<string, string> RollbackParameters { get; set; }

        public Dictionary<string, string> PredictiveData { get; set; } = new();
        public Dictionary<string, string> ActualData { get; set; } = new();

        public List<string> ChangedFields => PredictiveData.Keys
            .Where(k => ActualData.ContainsKey(k) && PredictiveData[k] != ActualData[k])
            .ToList();

        public bool IsSelectedForRollback { get; set; } = true;
    }

}
