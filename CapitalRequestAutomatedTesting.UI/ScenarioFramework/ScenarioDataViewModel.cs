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

    }
}