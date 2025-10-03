using System.Collections.Concurrent;

namespace CapitalRequestAutomatedTesting.UI.Services
{
    public interface IProgressTracker
    {
        void UpdateProgress(string sessionId, int current, int total, string scenarioName, string message);
        void CompleteProgress(string sessionId, string redirectUrl = null);
        ProgressInfo GetProgress(string sessionId);
        void ClearProgress(string sessionId);
    }

    public class ProgressInfo
    {
        public int Current { get; set; }
        public int Total { get; set; }
        public string ScenarioName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsComplete { get; set; }
        public string RedirectUrl { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    public class ProgressTracker : IProgressTracker
    {
        private readonly ConcurrentDictionary<string, ProgressInfo> _progressMap = new();
        private readonly ILogger<ProgressTracker> _logger;

        public ProgressTracker(ILogger<ProgressTracker> logger)
        {
            _logger = logger;
        }

        public void UpdateProgress(string sessionId, int current, int total, string scenarioName, string message)
        {
            var progress = new ProgressInfo
            {
                Current = current,
                Total = total,
                ScenarioName = scenarioName,
                Message = message,
                IsComplete = false,
                LastUpdated = DateTime.UtcNow
            };

            _progressMap.AddOrUpdate(sessionId, progress, (key, oldValue) => progress);
            _logger.LogInformation("Progress updated for session {SessionId}: {Current}/{Total} - {ScenarioName}", 
                sessionId, current, total, scenarioName);
        }

        public void CompleteProgress(string sessionId, string redirectUrl = null)
        {
            if (_progressMap.TryGetValue(sessionId, out var progress))
            {
                progress.IsComplete = true;
                progress.RedirectUrl = redirectUrl ?? string.Empty;
                progress.Message = "All scenarios completed";
                progress.LastUpdated = DateTime.UtcNow;
                
                _logger.LogInformation("Progress completed for session {SessionId}", sessionId);
            }
        }

        public ProgressInfo GetProgress(string sessionId)
        {
            return _progressMap.TryGetValue(sessionId, out var progress) ? progress : null;
        }

        public void ClearProgress(string sessionId)
        {
            _progressMap.TryRemove(sessionId, out _);
            _logger.LogInformation("Progress cleared for session {SessionId}", sessionId);
        }
    }
}