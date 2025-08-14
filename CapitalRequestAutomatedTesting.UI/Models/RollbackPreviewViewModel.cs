namespace CapitalRequestAutomatedTesting.UI.Models
{
    public class RollbackPreviewViewModel
    {
        public Guid ScenarioId { get; set; }
        public string ScenarioName { get; set; }
        public List<RollbackCandidateViewModel> Candidates { get; set; }
        public bool CanRollback { get; set; }
    }


}
