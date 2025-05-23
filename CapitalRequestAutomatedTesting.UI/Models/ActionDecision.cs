using CapitalRequestAutomatedTesting.UI.Enums;

namespace CapitalRequestAutomatedTesting.UI.Models
{
    public class ActionDecision
    {
        public WorkflowActionType ActionType { get; set; }
        public string? ElementId { get; set; } // Only used if ActionType is ClickButton
        public string? ExpectedMessage { get; set; } // Only used if ActionType is ExpectMessage
    }
}
