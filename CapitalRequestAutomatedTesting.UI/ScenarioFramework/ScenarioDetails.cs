using Microsoft.AspNetCore.Mvc.Rendering;

namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class ScenarioDetails
    {
        public int ProposalId { get; set; }
        public string ScenarioId { get; set; }
        public string DisplayText { get; set; }
        public int RequestingGroupId { get; set; }
        public int ReplyingGroupId { get; set; }
        public int VerifyingGroupId { get; set; }

        public int TargetGroupId { get; set; }
        public int ReviewerId { get; set; }
        public string ReviewerEmail { get; set; }
        public string ReviewerUserId { get; set; }
        public string RequestedInformation { get; set; }
        public string ReturnedInformation { get; set; }
        public int SequenceNumber { get; set; }
        public int RequestedInfoId { get; set; }
        public string SubmitUserId { get; set; }
        public string SubmittedBy { get; set; } = string.Empty;

        public string Message { get; set; }

        public SeleniumScenarioOutcome PredictedSeleniumOutcome { get; set; }
        public SeleniumScenarioOutcome ActualSeleniumOutcome { get; set; }

        public ScenarioDataViewModel PredictiveData { get; set; }
        public ScenarioDataViewModel ActualData { get; set; }

        public List<IFormFile>? AddInfoFiles { get; set; }
        public List<FileUploadData> FileUploads { get; set; } = new List<FileUploadData>();
        public bool VerifyAndSendToVPFinance { get; set; }

    }
}
