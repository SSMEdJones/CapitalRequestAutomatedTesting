using CapitalRequestAutomatedTesting.UI.Enums;

namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class TableDifference
    {
        public string TableName { get; set; }
        public CrudOperationType OperationPredictive { get; set; }
        public CrudOperationType OperationActual { get; set; }
        public List<RecordEntry> OnlyInPredictive { get; set; } = new();
        public List<RecordEntry> OnlyInActual { get; set; } = new();
        public List<RecordDifference> RecordDifferences { get; set; } = new();
        public List<RecordDifference> FieldLevelDifferences { get; set; } = new();
        public List<OperationGroupDifference> OperationGroups { get; set; } = new();


    }
}
