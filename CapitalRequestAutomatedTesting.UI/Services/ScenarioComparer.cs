using CapitalRequestAutomatedTesting.UI.Extensions;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
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
        public ScenarioComparisonResult CompareData(ScenarioDataViewModel predictiveData, ScenarioDataViewModel actualData)
        {
            var result = new ScenarioComparisonResult();
            var actualExecutionDurationMinutes = actualData.ActualExecutionDurationMinutes ?? 3; // Default to 3 minutes if not set
            var predictiveTableNames = predictiveData.Tables.Keys.ToHashSet();
            var actualTableNames = actualData.Tables.Keys.ToHashSet();

            result.TablesOnlyInPredictive = predictiveTableNames.Except(actualTableNames).ToList();
            result.TablesOnlyInActual = actualTableNames.Except(predictiveTableNames).ToList();

            var sharedTables = predictiveTableNames.Intersect(actualTableNames);
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
                                LogRowKeyDiagnostics(tableName, itemType); // Add this line
                                var listType = typeof(List<>).MakeGenericType(itemType);

                                var predictiveList = (IEnumerable)JsonConvert.DeserializeObject(predictiveJson, listType);
                                var actualList = (IEnumerable)JsonConvert.DeserializeObject(actualJson, listType);

                                string GetRowKey(object obj)
                                {
                                    var rowKeyProperties = itemType
                                        .GetProperties()
                                        .Where(p => Attribute.IsDefined(p, typeof(RowKeyAttribute)))
                                        .OrderBy(p => p.Name) // Ensure consistent ordering of key parts
                                        .ToArray();

                                    var keyParts = rowKeyProperties
                                        .Select(p => p.GetValue(obj)?.ToString() ?? "null");

                                    var key = string.Join("|", keyParts);
                                    Debug.WriteLine($"[{tableName}] Operation: {opType} — RowKey: {key} (Properties: {string.Join(", ", rowKeyProperties.Select(p => p.Name))})");

                                    return key;
                                }

                                var seenRowKeys = new HashSet<string>();
                                var duplicateRowKeys = new List<string>();

                                foreach (var obj in predictiveList.Cast<object>())
                                {
                                    var key = GetRowKey(obj);
                                    if (!seenRowKeys.Add(key))
                                    {
                                        duplicateRowKeys.Add(key);
                                        Debug.WriteLine($"[DUPLICATE] Predictive RowKey: {key}");
                                    }
                                }

                                foreach (var obj in actualList.Cast<object>())
                                {
                                    var key = GetRowKey(obj);
                                    if (!seenRowKeys.Add(key))
                                    {
                                        duplicateRowKeys.Add(key);
                                        Debug.WriteLine($"[DUPLICATE] Actual RowKey: {key}");
                                    }
                                }

                                // Optionally, log all duplicates at once
                                if (duplicateRowKeys.Any())
                                {
                                    Debug.WriteLine($"Duplicate RowKeys detected in table '{tableName}', operation '{opType}': {string.Join(", ", duplicateRowKeys)}");
                                }

                                var dictA = predictiveList.Cast<object>().ToDictionary(GetRowKey);
                                var dictB = actualList.Cast<object>().ToDictionary(GetRowKey);

                                // Replacement starts here
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
                                // Replacement ends here

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

            LogStructure(predictiveData.Tables, $"Predictive Record ");
            LogStructure(actualData.Tables, $"Actual Record ");
            LogStructure(result.DifferingTables, $"Differing Tables Record ");

            return result;
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
    }

}
