using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection; // If you're doing deeper reflection
using System.Threading.Tasks;


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
                StepNumber = method.StepNumber,
                MethodName = method.MethodName,
                Description = $"Rollback for step {method.StepNumber}: {method.MethodName}",
                RollbackServiceName = rollback.ServiceName,
                RollbackMethodName = rollback.MethodName,
                PredictiveData = ConvertToDictionary(method.Parameters),
                RollbackParameters = ConvertToDictionary(rollback.Parameters),
                IsSelectedForRollback = true
            };

            // Optional: Execute rollback and capture actual data
            //var actualResult = await ExecuteRollbackMethodAsync(rollback);
            //candidate.ActualData = ConvertToDictionary(actualResult);

            candidates.Add(candidate);
        }

        return candidates;
    }

    private Dictionary<string, string> ConvertToDictionary(object data, string prefix = "")
    {
        var dict = new Dictionary<string, string>();

        if (data == null)
        {
            dict[prefix + "null"] = "null";
            return dict;
        }

        var type = data.GetType();

        // Handle primitive types directly
        if (type.IsPrimitive || data is string || data is DateTime || data is decimal)
        {
            dict[prefix.TrimEnd('.')] = data.ToString();
            return dict;
        }

        // Handle collections
        if (data is IEnumerable enumerable && data is not string && data is not byte[] && data is not System.IO.Stream)
        {
            int index = 0;
            foreach (var item in enumerable)
            {
                var nested = ConvertToDictionary(item, $"{prefix}[{index}].");
                foreach (var kvp in nested)
                    dict[kvp.Key] = kvp.Value;
                index++;
            }
            return dict;
        }

        // Handle complex objects (POCOs)
        var props = type.GetProperties();
        foreach (var prop in props)
        {
            if (prop.GetIndexParameters().Length > 0)
            {
                dict[$"{prefix}{prop.Name}"] = "Skipped indexer";
                continue;
            }

            try
            {
                var value = prop.GetValue(data);
                var nested = ConvertToDictionary(value, $"{prefix}{prop.Name}.");
                foreach (var kvp in nested)
                    dict[kvp.Key] = kvp.Value;
            }
            catch (Exception ex)
            {
                dict[$"{prefix}{prop.Name}"] = $"Error: {ex.Message}";
            }
        }

        return dict;
    }



}
