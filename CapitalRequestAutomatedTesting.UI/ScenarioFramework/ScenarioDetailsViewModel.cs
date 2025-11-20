using CapitalRequestAutomatedTesting.UI.CustomAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class ScenarioDetailsViewModel
    {
        public int ProposalId { get; set; }

        [ValidateNever]
        public string ScenarioId { get; set; }

        public string PartialViewName { get; set; }

        public string DisplayText { get; set; }

        [IgnoreForLogging]
        public Dictionary<string, string> SelectedProperties { get; set; } = new();

        [DisplayName("Requesting Group:")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid requesting group.")]
        public int? RequestingGroupId { get; set; }

        [DisplayName("Replying Group:")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid replying group.")]
        public int? ReplyingGroupId { get; set; }

        [DisplayName("Target Group:")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid target group.")]
        public int? TargetGroupId { get; set; }

        [DisplayName("Reviewer Group:")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid reviewer group.")]
        public int? VerifyingGroupId { get; set; }

        [DisplayName("Reviewer:")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a reviewer.")]
        public int? ReviewerId { get; set; }

        [DisplayName("Reviewer Email:")]
        [ValidateNever]
        public string ReviewerEmail { get; set; }

        [DisplayName("Reviewer UserId:")]
        [ValidateNever]
        public string ReviewerUserId { get; set; }

        
        [DisplayName("Submit User:")]
        [ValidateNever]
        public string SubmitUserId { get; set; }

        [DisplayName("Verify User:")]
        [ValidateNever]
        public string VerifyUserId { get; set; }

        [ValidateNever]
        public int SequenceNumber { get; set; }

        [DisplayName("Requested Information:")]
        [Required(ErrorMessage = "Requested Information is required.")]
        public string RequestedInformation { get; set; } = string.Empty;

        [DisplayName("Returned Information:")]
        [Required(ErrorMessage = "Returned Information is required.")]
        public string ReturnedInformation { get; set; } = string.Empty;

        [ValidateNever]
        public List<string> FileUploadPaths { get; set; } = new();

        public bool RequiresFileUpload => FileUploadPaths?.Any() == true;

        public List<IFormFile>? AttachmentFiles { get; set; }

        public List<IFormFile>? AddInfoFiles { get; set; }

        public List<FileUploadData> FileUploads { get; set; } = new List<FileUploadData>();

        public string Message { get; set; } = string.Empty;

        [IgnoreForLogging]
        public List<SelectListItem> RequestingGroups { get; set; } = new();
        [IgnoreForLogging]

        public List<SelectListItem> SecondaryRequestingGroups { get; set; } = new();
        [IgnoreForLogging]

        public List<SelectListItem> ReplyingGroups { get; set; } = new();
        [IgnoreForLogging]

        public List<SelectListItem> TargetGroups { get; set; } = new();
        [IgnoreForLogging]

        public List<SelectListItem> VerifyingGroups { get; set; } = new();
        [IgnoreForLogging]

        public List<SelectListItem> Reviewers { get; set; } = new();

        public List<SelectListItem> SubmitUsers { get; set; } = new();
        public List<SelectListItem> VerifyUsers { get; set; } = new();

        public int RequestCount { get; set; }

        [ValidateNever]
        public SeleniumScenarioOutcome PredictedSeleniumOutcome { get; set; } = new();

        [ValidateNever]
        public SeleniumScenarioOutcome ActualSeleniumOutcome { get; set; } = new();

        [ValidateNever]
        public ScenarioDataViewModel PredictiveData { get; set; } = new();

        [ValidateNever]
        public ScenarioDataViewModel OriginalData { get; set; } = new();

        [ValidateNever]
        public ScenarioDataViewModel ActualData { get; set; } = new();

        [IgnoreForLogging]
        public List<PredictiveMethod> PredictiveMethods { get; set; } = new();

        public int PredictiveCompletionStep { get; set; }
        public string PredictiveStopReason { get; set; }
        
        [IgnoreForLogging]
        public bool CanExecuteActualSteps { get; set; } = true;
        public int RequestedInfoId { get; set; }

        public bool PredictiveSeleniumFailed => !PredictedSeleniumOutcome?.Success ?? true;

        public Stopwatch StopWatch { get; set; }
        public TimeSpan? ExecutionDuration { get; set; }
        public int? ExecutionDurationMinutes { get; set; }

        public bool CommitStepReached { get; set; } = false;

        public bool PauseBeforeSubmit { get; set; }
        public bool VerifyAndSendToVPFinance { get; set; }
    }
}
