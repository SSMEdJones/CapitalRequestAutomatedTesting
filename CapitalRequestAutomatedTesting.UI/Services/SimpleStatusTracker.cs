namespace CapitalRequestAutomatedTesting.UI.Services
{
    public static class SimpleStatusTracker
    {
        private static readonly Dictionary<string, ScenarioStatus> _statuses = new();

        public static void UpdateStatus(string scenarioId, int completed, int total, string message)
        {
            _statuses[scenarioId] = new ScenarioStatus
            {
                Completed = completed,
                Total = total,
                Message = message,
                LastUpdated = DateTime.Now,
                IsComplete = completed >= total
            };
        }

        public static ScenarioStatus? GetStatus(string scenarioId)
        {
            _statuses.TryGetValue(scenarioId, out var status);
            return status;
        }

        public static void ClearStatus(string scenarioId)
        {
            _statuses.Remove(scenarioId);
        }
    }

    public class ScenarioStatus
    {
        public int Completed { get; set; }
        public int Total { get; set; }
        public string Message { get; set; } = "";
        public DateTime LastUpdated { get; set; }
        public bool IsComplete { get; set; }
    }
}