namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class ProvidedInfoModel
    {
        public int RequestedInfoId { get; set; }

        public int ReviewerGroupId { get; set; }

        public int? ReviewerId { get; set; }

        public string ProvidedInformation { get; set; }

        public string Action { get; set; }

        public DateTime Created { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? Updated { get; set; }

        public string UpdatedBy { get; set; }
    }
}
