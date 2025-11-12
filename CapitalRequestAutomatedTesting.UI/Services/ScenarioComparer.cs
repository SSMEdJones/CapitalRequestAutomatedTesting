using CapitalRequestAutomatedTesting.UI.Extensions;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using Infrastructure.ApiDiagnostics;
using Infrastructure.Utilities.Xml;
using Newtonsoft.Json;
using System.Collections;
using System.Diagnostics;

namespace CapitalRequestAutomatedTesting.UI.Services
{
    public interface IScenarioComparer
    {
        ScenarioComparisonResult CompareData(ScenarioDataViewModel predictiveData, ScenarioDataViewModel actualData);
        List<SeleniumStepComparison> CompareOutcomes(SeleniumScenarioResult expected, SeleniumScenarioResult actual);
    }
    public class ScenarioComparer : IScenarioComparer
    {
        private readonly ILogger<ScenarioComparer> _logger;
        private readonly IFormDataContext _formDataContext;

        public ScenarioComparer(ILogger<ScenarioComparer> logger, IFormDataContext formDataContext)
        {
            _logger = logger;
            _formDataContext = formDataContext;
        }

        public ScenarioComparisonResult CompareData(ScenarioDataViewModel predictiveData, ScenarioDataViewModel actualData)
        {
            var result = new ScenarioComparisonResult();
            var actualExecutionDurationMinutes = actualData.ActualExecutionDurationMinutes ?? 3; // Default to 3 minutes if not set
            var predictiveTableNames = predictiveData.Tables.Keys.ToHashSet();
            var actualTableNames = actualData.Tables.Keys.ToHashSet();

            result.TablesOnlyInPredictive = predictiveTableNames.Except(actualTableNames).ToList();
            result.TablesOnlyInActual = actualTableNames.Except(predictiveTableNames).ToList();

            var sharedTables = predictiveTableNames.Intersect(actualTableNames);
            var comparisonData = new Dictionary<string, object>();

            foreach (var tableName in sharedTables)
            {
                var tableA = predictiveData.Tables[tableName];
                var tableB = actualData.Tables[tableName];

                var tableDiff = new TableDifference
                {
                    TableName = tableName,
                    OperationPredictive = tableA.Operation,
                    OperationActual = tableB.Operation
                };

                var groupedA = tableA.Records.GroupBy(r => r.Operation).ToDictionary(g => g.Key, g => g.ToList());
                var groupedB = tableB.Records.GroupBy(r => r.Operation).ToDictionary(g => g.Key, g => g.ToList());

                foreach (var opType in groupedA.Keys.Union(groupedB.Keys))
                {
                    var recordsA = groupedA.ContainsKey(opType) ? groupedA[opType] : new List<RecordEntry>();
                    var recordsB = groupedB.ContainsKey(opType) ? groupedB[opType] : new List<RecordEntry>();
                    var operationDiffs = new List<RecordDifference>();

                    int rowCount = Math.Min(recordsA.Count, recordsB.Count);
                    for (int i = 0; i < rowCount; i++)
                    {
                        var recordA = recordsA[i];
                        var recordB = recordsB[i];
                        if (TableTypeRegistry.TableTypes.TryGetValue(tableName, out var type))
                        {
                            var predictiveJson = JsonConvert.SerializeObject(recordA.Data);
                            var actualJson = JsonConvert.SerializeObject(recordB.Data);

                            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                            {
                                var itemType = type.GetGenericArguments()[0];
                                LogRowKeyDiagnostics(tableName, itemType);
                                var listType = typeof(List<>).MakeGenericType(itemType);

                                var predictiveList = (IEnumerable)JsonConvert.DeserializeObject(predictiveJson, listType);
                                var actualList = (IEnumerable)JsonConvert.DeserializeObject(actualJson, listType);

                                string GetRowKey(object obj)
                                {
                                    var rowKeyProperties = itemType
                                        .GetProperties()
                                        .Where(p => Attribute.IsDefined(p, typeof(RowKeyAttribute)))
                                        .OrderBy(p => p.Name)
                                        .ToArray();

                                    var keyParts = rowKeyProperties
                                        .Select(p => p.GetValue(obj)?.ToString() ?? "null");

                                    var key = string.Join("|", keyParts);
                                    Debug.WriteLine($"[{tableName}] Operation: {opType} — RowKey: {key} (Properties: {string.Join(", ", rowKeyProperties.Select(p => p.Name))})");

                                    return key;
                                }

                                var seenRowKeys = new HashSet<string>();
                                var duplicateRowKeys = new List<string>();
                                var duplicateDetails = new List<object>();

                                // Process predictive list and log duplicate values
                                foreach (var obj in predictiveList.Cast<object>())
                                {
                                    var key = GetRowKey(obj);
                                    if (!seenRowKeys.Add(key))
                                    {
                                        duplicateRowKeys.Add(key);
                                        duplicateDetails.Add(obj);

                                        // Log full values of duplicated keys similar to other services
                                        var fullValueJson = JsonConvert.SerializeObject(obj, Formatting.Indented);
                                        Debug.WriteLine($"[DUPLICATE] Predictive RowKey: {key}");
                                        Debug.WriteLine($"[DUPLICATE] Full Predictive Value: {fullValueJson}");
                                        _logger.LogWarning("Duplicate predictive row key found in table {TableName}, operation {Operation}: {RowKey}. Full value: {FullValue}",
                                            tableName, opType, key, fullValueJson);
                                    }
                                }

                                // Process actual list and log duplicate values
                                foreach (var obj in actualList.Cast<object>())
                                {
                                    var key = GetRowKey(obj);
                                    if (!seenRowKeys.Add(key))
                                    {
                                        duplicateRowKeys.Add(key);
                                        duplicateDetails.Add(obj);

                                        // Log full values of duplicated keys
                                        var fullValueJson = JsonConvert.SerializeObject(obj, Formatting.Indented);
                                        Debug.WriteLine($"[DUPLICATE] Actual RowKey: {key}");
                                        Debug.WriteLine($"[DUPLICATE] Full Actual Value: {fullValueJson}");
                                        _logger.LogWarning("Duplicate actual row key found in table {TableName}, operation {Operation}: {RowKey}. Full value: {FullValue}",
                                            tableName, opType, key, fullValueJson);
                                    }
                                }

                                // Store comparison data for FormData replacement
                                if (duplicateRowKeys.Any())
                                {
                                    Debug.WriteLine($"Duplicate RowKeys detected in table '{tableName}', operation '{opType}': {string.Join(", ", duplicateRowKeys)}");

                                    // Add comparison data for later FormData replacement
                                    comparisonData[$"DuplicateKeys.{tableName}.{opType}"] = string.Join(", ", duplicateRowKeys);
                                    comparisonData[$"DuplicateDetails.{tableName}.{opType}"] = duplicateDetails;
                                    comparisonData[$"DuplicateCount.{tableName}.{opType}"] = duplicateRowKeys.Count;
                                }

                                var dictA = predictiveList.Cast<object>().ToDictionary(GetRowKey);
                                var dictB = actualList.Cast<object>().ToDictionary(GetRowKey);

                                var allKeys = dictA.Keys.Union(dictB.Keys);
                                var orderedKeys = allKeys.OrderBy(k => GetSortableRowKey(k, itemType));

                                foreach (var key in orderedKeys)
                                {
                                    dictA.TryGetValue(key, out var a);
                                    dictB.TryGetValue(key, out var b);

                                    operationDiffs.Add(new RecordDifference
                                    {
                                        RecordPredictive = a,
                                        RecordActual = b,
                                        FieldDifferences = a != null && b != null ? CompareFields(a, b, actualExecutionDurationMinutes) : new List<FieldDifference>(),
                                        RowKey = key
                                    });
                                }
                            }
                            else
                            {
                                var predictiveTyped = JsonConvert.DeserializeObject(predictiveJson, type);
                                var actualTyped = JsonConvert.DeserializeObject(actualJson, type);

                                var fieldDiffs = CompareFields(predictiveTyped, actualTyped, actualExecutionDurationMinutes);

                                operationDiffs.Add(new RecordDifference
                                {
                                    RecordPredictive = predictiveTyped,
                                    RecordActual = actualTyped,
                                    FieldDifferences = fieldDiffs,
                                    RowKey = $"{opType}|{i}"
                                });
                            }

                            if (operationDiffs.Any())
                            {
                                if (tableDiff.OperationGroups == null)
                                    tableDiff.OperationGroups = new List<OperationGroupDifference>();

                                tableDiff.OperationGroups.Add(new OperationGroupDifference
                                {
                                    Operation = opType,
                                    Records = operationDiffs
                                });
                            }
                        }
                    }
                }

                // Existing logic
                var recordsOnlyInA = tableA.Records.Except(tableB.Records, new RecordEntryComparer()).ToList();
                var recordsOnlyInB = tableB.Records.Except(tableA.Records, new RecordEntryComparer()).ToList();

                tableDiff.OnlyInPredictive = recordsOnlyInA;
                tableDiff.OnlyInActual = recordsOnlyInB;

                if (tableDiff.OnlyInPredictive.Any() || tableDiff.OnlyInActual.Any() || tableDiff.FieldLevelDifferences.Any())
                {
                    result.DifferingTables.Add(tableDiff);
                }
            }

            result.PredictiveTables = predictiveData.Tables;
            result.ActualTables = actualData.Tables;

            // Add comparison summary data
            comparisonData["Comparison.PredictiveTableCount"] = predictiveTableNames.Count;
            comparisonData["Comparison.ActualTableCount"] = actualTableNames.Count;
            comparisonData["Comparison.SharedTableCount"] = sharedTables.Count();
            comparisonData["Comparison.TablesOnlyInPredictive"] = result.TablesOnlyInPredictive;
            comparisonData["Comparison.TablesOnlyInActual"] = result.TablesOnlyInActual;
            comparisonData["Comparison.DifferingTablesCount"] = result.DifferingTables.Count;

            // Clear and replace FormData and MethodContext
            ClearAndReplaceFormData(comparisonData);

            LogStructure(predictiveData.Tables, $"Predictive Record ");
            LogStructure(actualData.Tables, $"Actual Record ");
            LogStructure(result.DifferingTables, $"Differing Tables Record ");

            return result;
        }

        /// <summary>
        /// Clears the current FormData and MethodContext, then replaces with comparison data
        /// </summary>
        private void ClearAndReplaceFormData(Dictionary<string, object> comparisonData)
        {
            try
            {
                // Log current context before clearing
                var currentInvocationContext = _formDataContext.GetInvocationContext();
                var currentFormData = _formDataContext.Get();

                _logger.LogInformation("Clearing FormData and MethodContext. Previous context: {PreviousContext}, Previous FormData length: {FormDataLength}",
                    JsonConvert.SerializeObject(currentInvocationContext),
                    currentFormData?.Length ?? 0);

                // Set new invocation context for comparison
                _formDataContext.SetInvocationContext(new MethodInvocationContext
                {
                    ServiceName = "ScenarioComparer",
                    MethodName = "CompareData",
                    Parameters = new List<object> { "Comparison completed with duplicate key analysis" }
                });

                // Convert comparison data to labeled dictionary for XML building with string values
                var labeledComparisonData = comparisonData.ToDictionary(
                    kvp => $"Comparison.{kvp.Key}",
                    kvp => ConvertToString(kvp.Value));

                // Build new FormData XML with comparison data
                var newFormDataXml = FormDataXmlBuilder.Build(labeledComparisonData);
                _formDataContext.Set(newFormDataXml);

                _logger.LogInformation("FormData and MethodContext replaced with comparison data. New FormData length: {NewFormDataLength}",
                    newFormDataXml?.Length ?? 0);

                Debug.WriteLine("=== FormData Replacement Complete ===");
                Debug.WriteLine($"New FormData XML Preview: {newFormDataXml?.Substring(0, Math.Min(200, newFormDataXml.Length))}...");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing and replacing FormData with comparison data");
                // Don't throw - this is supplementary functionality
            }
        }

        public static List<FieldDifference> CompareFields(
            object predictive,
            object actual,
            int fuzzyMinutes,
            bool updatePredictive = true)
        {
            var differences = new List<FieldDifference>();

            if (predictive == null && actual == null)
                return differences;

            if (predictive == null || actual == null)
            {
                differences.Add(new FieldDifference
                {
                    FieldName = "[Entire Object]",
                    PredictiveValue = predictive,
                    ActualValue = actual
                });
                return differences;
            }

            var type = predictive.GetType();

            foreach (var prop in type.GetProperties())
            {
                var valA = prop.GetValue(predictive);
                var valB = prop.GetValue(actual);

                if (valA == null && valB == null)
                    continue;

                if (valA is DateTime dtA && valB is DateTime dtB)
                {
                    if (!dtA.IsFuzzyMatch(dtB, fuzzyMinutes))
                    {
                        differences.Add(new FieldDifference
                        {
                            FieldName = prop.Name,
                            PredictiveValue = dtA,
                            ActualValue = dtB
                        });
                    }
                    else if (updatePredictive)
                    {
                        prop.SetValue(predictive, dtB);
                    }
                }
                else if (!Equals(valA, valB))
                {
                    differences.Add(new FieldDifference
                    {
                        FieldName = prop.Name,
                        PredictiveValue = valA,
                        ActualValue = valB
                    });
                }
            }

            return differences;
        }

        public static void LogStructure(object obj, string label = "Object")
        {
            var formatted = JsonConvert.SerializeObject(obj, Formatting.Indented);
            Debug.WriteLine($"===== {label} =====");
            Debug.WriteLine(formatted);
        }

        public List<SeleniumStepComparison> CompareOutcomes(SeleniumScenarioResult expected, SeleniumScenarioResult actual)
        {
            var stepComparisons = new List<SeleniumStepComparison>();
            var expectedMap = expected.Steps.ToDictionary(x => x.StepNumber);

            foreach (var actualStep in actual.Steps)
            {
                var comparison = new SeleniumStepComparison
                {
                    StepNumber = actualStep.StepNumber,
                    Description = actualStep.Description,
                    ActualMessage = actualStep.Result?.Message,
                    ActualSuccess = actualStep.Result?.Success
                };

                if (expectedMap.TryGetValue(actualStep.StepNumber, out var expectedStep))
                {
                    comparison.ExpectedMessage = expectedStep.Result?.Message;
                    comparison.ExpectedSuccess = expectedStep.Result?.Success;
                }

                stepComparisons.Add(comparison);
            }

            return stepComparisons;
        }

        private static string GetSortableRowKey(string rowKey, Type itemType)
        {
            var parts = rowKey.Split('|');
            var rowKeyProperties = itemType
                .GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof(RowKeyAttribute)))
                .OrderBy(p => p.Name)
                .ToArray();

            // Create sortable versions of each part
            var sortableParts = new List<string>();
            for (int i = 0; i < parts.Length && i < rowKeyProperties.Length; i++)
            {
                var part = parts[i];
                var property = rowKeyProperties[i];

                // Special handling for different types
                if (property.PropertyType == typeof(int) || property.PropertyType == typeof(int?))
                {
                    if (int.TryParse(part, out var intValue))
                    {
                        sortableParts.Add(intValue.ToString("D10")); // Zero-pad for proper sorting
                    }
                    else
                    {
                        sortableParts.Add(part);
                    }
                }
                else
                {
                    sortableParts.Add(part);
                }
            }

            return string.Join("|", sortableParts);
        }

        private void LogRowKeyDiagnostics(string tableName, Type itemType)
        {
            var rowKeyProperties = itemType
                .GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof(RowKeyAttribute)))
                .OrderBy(p => p.Name)
                .ToArray();

            Debug.WriteLine($"=== RowKey Diagnostics for {tableName} ===");
            Debug.WriteLine($"Item Type: {itemType.Name}");
            Debug.WriteLine($"RowKey Properties: {string.Join(", ", rowKeyProperties.Select(p => $"{p.Name} ({p.PropertyType.Name})"))}");

            if (!rowKeyProperties.Any())
            {
                Debug.WriteLine("⚠️ WARNING: No properties marked with [RowKey] attribute!");
            }
        }

        // Add this private static method to ScenarioComparer class to fix CS0103
        private static string ConvertToString(object value)
        {
            if (value == null)
                return string.Empty;
            if (value is string s)
                return s;
            if (value is IEnumerable enumerable && !(value is IDictionary))
                return JsonConvert.SerializeObject(enumerable, Formatting.None);
            if (value is IDictionary dict)
                return JsonConvert.SerializeObject(dict, Formatting.None);
            if (value.GetType().IsPrimitive || value is decimal)
                return value.ToString();
            return JsonConvert.SerializeObject(value, Formatting.None);
        }
    }
}
