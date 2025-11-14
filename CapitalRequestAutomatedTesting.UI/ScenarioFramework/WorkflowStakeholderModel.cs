namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class WorkflowStakeholderModel
    {

        public Guid WorkflowID { get; set; }
        public bool isGroup { get; set; }
        public bool isExternal { get; set; }
        [RowKey]
        public string Stakeholder { get; set; }


    }
}
