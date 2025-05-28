namespace CapitalRequest.API.Models
{
    public class WorkflowAction
    {
        public int? ProposalId { get; set; }
        public int? ReviewerGroupId { get; set; }
        public int? ReviewerId { get; set; }
        public int? RequestedInfoId { get; set; }
        public string ActionType { get; set; }
        public Guid? OptionId { get; set; }
        public string Email { get; set; }
        public string WorkflowPortion { get; set; }
        public string ButtonCaption { get; set; }
        public string ReqId { get; set; }
        public string Identifier { get; set; }
        public string ActionName { get; set; }
        public string ScenarioId { get; set; }
        public string MethodName { get; set; }
        public string TargetId { get; set; }
    }
}
