namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class WorkflowStepOptionModel
    {
        public Guid WorkflowStepID { get; set; }

        [RowKey]
        public string OptionName { get; set; }

        public bool IsComplete { get; set; }
        [RowKey]
        public bool IsTerminate { get; set; }
        [RowKey]
        public int? ReviewerGroupId { get; set; }

        [RowKey]
        public string OptionType { get; set; }

        public int? RequestedInfoId { get; set; }
    
        public DateTime Created { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? Updated { get; set; }

        public string UpdatedBy { get; set; }

    }
}
