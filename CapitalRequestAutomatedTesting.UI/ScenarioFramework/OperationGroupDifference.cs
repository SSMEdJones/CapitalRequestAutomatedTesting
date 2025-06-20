using CapitalRequestAutomatedTesting.UI.Enums;

namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class OperationGroupDifference
    {
        public int OperationType { get; set; } // e.g., 0 = Insert, 1 = Update
        public CrudOperationType Operation { get; set; } // 0 = Insert, 1 = Update, 2 = Delete
        public List<RecordDifference> Records { get; set; } = new();
        public List<RecordDifference> FieldLevelDifferences { get; set; } = new();
        public List<OperationGroupDifference> OperationGroups { get; set; } = new();

    }
}
