namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class SeleniumStepResultDetail
    {
        public int StepNumber { get; set; }                     // Helpful for ordering and traceability
        public string Description { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }

        public string StepType { get; set; }                    // e.g., "Validation", "Navigation", "Interaction"
        public string ExpectedCondition { get; set; }           // Optional: describe what was being tested
        public string DataSource { get; set; }                  // e.g., "proposal.ReviewerGroupId"
        public DateTime Timestamp { get; set; } = DateTime.Now; // When this result was evaluated
    }

}
