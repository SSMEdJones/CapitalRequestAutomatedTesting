using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;

public interface IRollbackService
{
    Task<List<RollbackCandidate>> ExecuteRollbackAsync(IEnumerable<PredictiveMethod> methods);
}

public class RollbackService : IRollbackService
{
    private readonly IRollbackServiceFactory _serviceFactory;

    public RollbackService(IRollbackServiceFactory serviceFactory)
    {
        _serviceFactory = serviceFactory;
    }

    public async Task<List<RollbackCandidate>> ExecuteRollbackAsync(IEnumerable<PredictiveMethod> methods)
    {
        var candidates = new List<RollbackCandidate>();

        foreach (var method in methods.Where(m => m.Rollback != null))
        {
            var rollback = method.Rollback;

            var handler = _serviceFactory.GetService(rollback.ServiceName);
            var result = await handler.RollbackAsync(rollback.Parameters ?? new List<object>());

            var candidate = new RollbackCandidate
            {
                MethodName = method.MethodName,
                RollbackMethodName = rollback.MethodName,
                Description = $"Rollback for step {method.StepNumber}: {method.MethodName}",
                PredictiveData = ConvertToDictionary(method.Parameters),
                ActualData = ConvertToDictionary(result),
                IsSelectedForRollback = true
            };

            candidates.Add(candidate);
        }

        return candidates;
    }

    private Dictionary<string, string> ConvertToDictionary(object data)
    {
        if (data == null) return new();

        var dict = new Dictionary<string, string>();
        foreach (var prop in data.GetType().GetProperties())
        {
            var value = prop.GetValue(data)?.ToString() ?? "null";
            dict[prop.Name] = value;
        }
        return dict;
    }
}
