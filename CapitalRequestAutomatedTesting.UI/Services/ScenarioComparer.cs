using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using Newtonsoft.Json;
using OpenQA.Selenium.BiDi.Modules.Input;
using System.Collections;
using System.Diagnostics;
using System.Reflection;

namespace CapitalRequestAutomatedTesting.UI.Services
{
    public interface IScenarioComparer
    {
        ScenarioComparisonResult CompareData(ScenarioDataViewModel predictiveData, ScenarioDataViewModel actualData);
    }
    public class ScenarioComparer : IScenarioComparer
    {
        public ScenarioComparisonResult CompareData(ScenarioDataViewModel predictiveData, ScenarioDataViewModel actualData)
        {
            var result = new ScenarioComparisonResult();

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
                                var listType = typeof(List<>).MakeGenericType(itemType);

                                var predictiveList = (IEnumerable)JsonConvert.DeserializeObject(predictiveJson, listType);
                                var actualList = (IEnumerable)JsonConvert.DeserializeObject(actualJson, listType);

                                string GetRowKey(object obj)
                                {
                                    var keyParts = itemType
                                        .GetProperties()
                                        .Where(p => Attribute.IsDefined(p, typeof(RowKeyAttribute)))
                                        .Select(p => p.GetValue(obj)?.ToString());

                                    var key = string.Join("|", keyParts);
                                    Debug.WriteLine($"[{tableName}] Operation: {opType} — RowKey: {key}");

                                    return key;
                                    //return itemType
                                    //    .GetProperties()
                                    //    .Where(p => Attribute.IsDefined(p, typeof(RowKeyAttribute)))
                                    //    .Select(p => p.GetValue(obj)?.ToString())
                                    //    .Aggregate((a, b) => $"{a}|{b}");
                                }

                                var dictA = predictiveList.Cast<object>().ToDictionary(GetRowKey);
                                var dictB = actualList.Cast<object>().ToDictionary(GetRowKey);

                                foreach (var key in dictA.Keys.Union(dictB.Keys).OrderBy(k => k))
                                {
                                    dictA.TryGetValue(key, out var a);
                                    dictB.TryGetValue(key, out var b);

                                    operationDiffs.Add(new RecordDifference
                                    {
                                        RecordPredictive = a,
                                        RecordActual = b,
                                        FieldDifferences = a != null && b != null ? CompareFields(a, b) : new List<FieldDifference>(),
                                        RowKey = key
                                    });
                                }

                            }
                            else
                            {
                                var predictiveTyped = JsonConvert.DeserializeObject(predictiveJson, type);
                                var actualTyped = JsonConvert.DeserializeObject(actualJson, type);

                                var fieldDiffs = CompareFields(predictiveTyped, actualTyped);
                                if (fieldDiffs.Any())
                                {
                                    operationDiffs.Add(new RecordDifference
                                    {
                                        RecordPredictive = predictiveTyped,
                                        RecordActual = actualTyped,
                                        FieldDifferences = fieldDiffs,
                                        RowKey = $"{opType}|{i}"
                                    });
                                }
                                //var predictiveTyped = JsonConvert.DeserializeObject(predictiveJson, type);
                                //var actualTyped = JsonConvert.DeserializeObject(actualJson, type);

                                //var fieldDiffs = CompareFields(predictiveTyped, actualTyped);
                                //if (fieldDiffs.Any())
                                //{
                                //    tableDiff.FieldLevelDifferences.Add(new RecordDifference
                                //    {
                                //        RecordPredictive = predictiveTyped,
                                //        RecordActual = actualTyped,
                                //        FieldDifferences = fieldDiffs
                                //    });
                                //}
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

        public static List<FieldDifference> CompareFields(object predictiveRecord, object actualRecord)
        {
            var differences = new List<FieldDifference>();
            if (predictiveRecord == null || actualRecord == null) return differences;

            var type = predictiveRecord.GetType();
            if (type != actualRecord.GetType()) return differences;

            foreach (var prop in type.GetProperties())
            {
                // 🚫 Skip indexers (like object[index])
                if (prop.GetIndexParameters().Length > 0)
                    Debug.WriteLine($"Skipping indexed property: {prop.Name}");
                if (prop.GetIndexParameters().Length > 0)
                    continue;

                object predictiveValue;
                object actualValue;

                try
                {
                    predictiveValue = prop.GetValue(predictiveRecord);
                    actualValue = prop.GetValue(actualRecord);
                }
                catch (TargetParameterCountException)
                {
                    // 🛡️ Skip any property that still blows up
                    continue;
                }

                if (!Equals(predictiveValue, actualValue))
                {
                    differences.Add(new FieldDifference
                    {
                        FieldName = prop.Name,
                        ValuePredictive = predictiveValue,
                        ValueActual = actualValue
                    });
                }
            }

            return differences;
        }

        private string GetRowKey(object obj)
        {
            var type = obj.GetType();
            var keys = type.GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof(RowKeyAttribute)))
                .Select(p => p.GetValue(obj)?.ToString());

            return string.Join("|", keys);
        }

        public static void LogStructure(object obj, string label = "Object")
        {
            var formatted = JsonConvert.SerializeObject(obj, Formatting.Indented);
            Debug.WriteLine($"===== {label} =====");
            Debug.WriteLine(formatted);
        }


    }

}
