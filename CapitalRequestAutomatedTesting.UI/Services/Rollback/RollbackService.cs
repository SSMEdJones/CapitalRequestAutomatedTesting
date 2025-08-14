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

            var candidate = new RollbackCandidate
            {
                MethodName = method.MethodName,
                Description = $"Rollback for step {method.StepNumber}: {method.MethodName}",
                RollbackServiceName = rollback.ServiceName,
                RollbackMethodName = rollback.MethodName,
                PredictiveData = ConvertToDictionary(method.Parameters),
                IsSelectedForRollback = true
            };

            // Optional: Execute rollback and capture actual data
            //var actualResult = await ExecuteRollbackMethodAsync(rollback);
            //candidate.ActualData = ConvertToDictionary(actualResult);

            candidates.Add(candidate);
        }

        return candidates;
    }

    //public async Task<List<RollbackCandidate>> ExecuteRollbackAsync(IEnumerable<PredictiveMethod> methods)
    //{
    //    var candidates = new List<RollbackCandidate>();

    //    foreach (var method in methods)
    //    {
    //        // Logic to reverse or flag the method
    //        var candidate = new RollbackCandidate
    //        {
    //            MethodName = method.MethodName,
    //            Description = $"Rollback for {method.MethodName} initiated."
    //            // Add more metadata as needed
    //        };

    //        candidates.Add(candidate);
    //    }

    //    return candidates;
    //}

    private Dictionary<string, string> ConvertToDictionary(object data)
    {
        var dict = new Dictionary<string, string>();

        try
        {
            if (data == null) return dict;

            var props = data.GetType().GetProperties();
            int counter = 0;

            foreach (var prop in props)
            {
                counter++;

                // Skip indexers (e.g. Item[int])
                if (prop.GetIndexParameters().Length > 0)
                {
                    dict[$"Property[{counter}]"] = $"Skipped indexer: {prop.Name}";
                    continue;
                }

                try
                {
                    var value = prop.GetValue(data)?.ToString() ?? "null";
                    dict[$"Property[{counter}]:{prop.Name}"] = value;
                }
                catch (Exception innerEx)
                {
                    dict[$"Property[{counter}]:{prop.Name}"] = $"Error: {innerEx.Message}";
                }
            }
        }
        catch (Exception ex)
        {
            dict["__error"] = $"Conversion failed: {ex.Message}";
        }

        return dict;
    }


}
