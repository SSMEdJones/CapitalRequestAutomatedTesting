using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using System.Collections.Concurrent;

namespace CapitalRequestAutomatedTesting.UI.Services
{
    public interface IScenarioMemoryCache
{
    void Save(int id, ScenarioComparisonResult scenarioResult);
    ScenarioComparisonResult Get(int id);
    void Clear(int id);
}

    public class ScenarioMemoryCache : IScenarioMemoryCache
    {
        private readonly ConcurrentDictionary<int, ScenarioComparisonResult> _cache = new();

        public void Save(int id, ScenarioComparisonResult scenarioResult) => _cache[id] = scenarioResult;

        public ScenarioComparisonResult Get(int id) => _cache.TryGetValue(id, out var scenarioResult) ? scenarioResult : null;

        public void Clear(int id) => _cache.TryRemove(id, out _);
    }

}
