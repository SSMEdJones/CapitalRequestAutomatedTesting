using CapitalRequestAutomatedTesting.UI.Enums;

namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class ScenarioDataViewModel
    {
        public string ScenarioId { get; internal set; }
        public int? ActualExecutionDurationMinutes { get; set; }
        public TimeSpan? ActualExecutionDuration { get; set; }
        public Dictionary<string, TableData> Tables { get; internal set; } = new Dictionary<string, TableData>();

        // Stores field-level data grouped by table and row
        //public Dictionary<string, Dictionary<string, Dictionary<string, object>>> Data { get; set; } = new();

        // Add or update a value
        public void SetValue(string tableName, string rowId, string fieldName, object newValue)
        {
            if (!Tables.ContainsKey(tableName))
                Tables[tableName] = new TableData { TableName = tableName };

            var table = Tables[tableName];

            if (!table.Rows.ContainsKey(rowId))
                table.Rows[rowId] = new RowData { RowId = rowId };

            var row = table.Rows[rowId];

            if (!row.Fields.ContainsKey(fieldName))
            {
                row.Fields[fieldName] = new FieldData
                {
                    OriginalValue = null,
                    NewValue = newValue,
                    Operation = CrudOperationType.Insert
                };
            }
            else
            {
                var field = row.Fields[fieldName];
                field.NewValue = newValue;
                field.Operation = CrudOperationType.Update;
            }
        }

        // Retrieve a value
        public object GetValue(string tableName, string rowId, string fieldName)
        {
            return Tables.TryGetValue(tableName, out var table) &&
                   table.Rows.TryGetValue(rowId, out var row) &&
                   row.Fields.TryGetValue(fieldName, out var field)
                ? field.NewValue
                : null;
        }

        // Get all changes for rollback or comparison
        public IEnumerable<PredictedChange> GetChanges()
        {
            foreach (var table in Tables.Values)
            {
                foreach (var row in table.Rows.Values)
                {
                    foreach (var field in row.Fields)
                    {
                        yield return new PredictedChange
                        {
                            TableName = table.TableName,
                            RowId = row.RowId,
                            FieldName = field.Key,
                            OriginalValue = field.Value.OriginalValue,
                            NewValue = field.Value.NewValue,
                            Type = field.Value.Operation
                        };
                    }
                }
            }
        }

        public void ApplyChanges(IEnumerable<PredictedChange> changes)
        {
            foreach (var change in changes)
            {
                if (!Tables.ContainsKey(change.TableName))
                    Tables[change.TableName] = new TableData { TableName = change.TableName };

                var table = Tables[change.TableName];

                if (!table.Rows.ContainsKey(change.RowId))
                    table.Rows[change.RowId] = new RowData { RowId = change.RowId };

                var row = table.Rows[change.RowId];

                row.Fields[change.FieldName] = new FieldData
                {
                    OriginalValue = change.OriginalValue,
                    NewValue = change.NewValue,
                    Operation = change.Type
                };
            }
        }

        public IEnumerable<PredictedChange> Diff(ScenarioDataViewModel other)
        {
            foreach (var change in GetChanges())
            {
                var actualValue = other.GetValue(change.TableName, change.RowId, change.FieldName);
                if (!Equals(change.OriginalValue, actualValue))
                {
                    yield return new PredictedChange
                    {
                        TableName = change.TableName,
                        RowId = change.RowId,
                        FieldName = change.FieldName,
                        OriginalValue = change.OriginalValue,
                        NewValue = actualValue,
                        Type = CrudOperationType.Update
                    };
                }
            }
        }

        public IEnumerable<PredictedChange> GetRollbackChanges()
        {
            foreach (var table in Tables.Values)
            {
                foreach (var row in table.Rows.Values)
                {
                    foreach (var field in row.Fields)
                    {
                        var operation = field.Value.Operation switch
                        {
                            CrudOperationType.Insert => CrudOperationType.Delete,
                            CrudOperationType.Update => CrudOperationType.Update,
                            CrudOperationType.Delete => CrudOperationType.Insert,
                            _ => CrudOperationType.Update
                        };

                        yield return new PredictedChange
                        {
                            TableName = table.TableName,
                            RowId = row.RowId,
                            FieldName = field.Key,
                            OriginalValue = field.Value.NewValue, // reverse direction
                            NewValue = field.Value.OriginalValue,
                            Type = operation
                        };
                    }
                }
            }
        }

        

    }
}