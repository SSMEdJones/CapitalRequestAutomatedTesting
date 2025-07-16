using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;


namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class ScenarioDetailsViewModel
    {
        public int ProposalId { get; set; }

        [ValidateNever]

        public string ScenarioId { get; set; }

        public string PartialViewName { get; set; }

        public string DisplayText { get; set; }

        public Dictionary<string, string> SelectedProperties { get; set; } = new();

        [DisplayName("Requesting Group:")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid requesting group.")]
        public int? RequestingGroupId { get; set; }

        [DisplayName("Target Group:")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid target group.")]
        public int? TargetGroupId { get; set; }

        [DisplayName("Reviewer:")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a reviewer.")]
        public int? ReviewerId { get; set; }

        [DisplayName("Reviewer Email:")]
        [ValidateNever]

        public string ReviewerEmail { get; set; }

        [DisplayName("Reviewer UserId:")]
        [ValidateNever]

        public string ReviewerUserId { get; set; }

        [ValidateNever]
        public int SequenceNumber { get; set; }

        [DisplayName("Requested Information:")]
        [Required(ErrorMessage = "Requested Information is required.")]
        public string RequestedInformation { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public List<SelectListItem> RequestingGroups { get; set; } = new();
        public List<SelectListItem> TargetGroups { get; set; } = new();
        public List<SelectListItem> Reviewers { get; set; } = new();

        public int RequestCount { get; set; }

        [ValidateNever]
        public SeleniumScenarioOutcome PredictedSeleniumOutcome { get; set; }

        [ValidateNever]
        public SeleniumScenarioOutcome ActualSeleniumOutcome { get; set; }

        [ValidateNever]

        public ScenarioDataViewModel PredictiveData { get; set; } = new();

        [ValidateNever]

        public ScenarioDataViewModel ActualData { get; set; } = new();

        public int PredictiveCompletionStep { get; set; }
        public string PredictiveStopReason { get; set; }

        public bool CanExecuteActualSteps { get; set; } = true;
    }

}
