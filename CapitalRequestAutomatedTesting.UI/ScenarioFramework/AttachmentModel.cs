namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class AttachmentModel
    {

        public int ProposalId { get; set; }

        [RowKey]
        public string FileName { get; set; }
        
        [RowKey]
        public int? ProvidedInfoId { get; set; }

        public DateTime? DateUploaded { get; set; }

    }
}
