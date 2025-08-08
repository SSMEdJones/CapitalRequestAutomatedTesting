namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class RollbackCandidate
    {
        public string MethodName { get; set; }
        public string Description { get; set; }

        public Dictionary<string, string> PredictiveData { get; set; }
        public Dictionary<string, string> ActualData { get; set; }

        public List<string> ChangedFields => PredictiveData.Keys
            .Where(k => ActualData.ContainsKey(k) && PredictiveData[k] != ActualData[k])
            .ToList();

        public bool IsSelectedForRollback { get; set; } = true;
        public string RollbackMethodName { get; set; }
    }


}
