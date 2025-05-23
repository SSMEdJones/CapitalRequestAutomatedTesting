namespace CapitalRequestAutomatedTesting.UI.Models
{
    public class WorkflowAction
    {
        public string ReqId { get; set; }
        public string Identifier { get; set; }      // e.g., "1 -IT"
        public string ActionName { get; set; }      // e.g., "Verify"
        public string ScenarioId { get; set; }      // e.g., "1_it_verify"
        public string MethodName { get; set; }      // e.g., "RunLoadVerifyButtonTest"
        public string TargetId { get; set; }        // e.g., "1_it"

    }
}
