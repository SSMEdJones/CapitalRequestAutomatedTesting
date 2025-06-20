using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;

namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class ScenarioDetailsViewModel
    {

        public int ProposalId { get; set; }
        public string ScenarioId { get; set; }
        public string PartialViewName { get; set; }
        public string DisplayText { get; set; }

        public Dictionary<string, string> SelectedProperties { get; set; } = new();
        [DisplayName("Requesting Group:")]
        public int RequestingGroupId { get; set; }

        [DisplayName("Target Group:")]
        public int TargetGroupId { get; set; }

        [DisplayName("Reviewer:")]
        public int ReviewerId { get; set; }

        [DisplayName("Reviewer Email:")]
        public string ReviewerEmail { get; set; }


        [DisplayName("Reviewer UserId:")]
        public string ReviewerUserId { get; set; }
        public int SequenceNumber { get; set; }
        [DisplayName("Requested Information:")]

        public string RequestedInformation { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public List<SelectListItem> RequestingGroups { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> TargetGroups { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Reviewers { get; set; } = new List<SelectListItem>();

        public ScenarioDataViewModel PredictiveData { get; set; } = new ScenarioDataViewModel();
        public ScenarioDataViewModel ActualData { get; set; } = new ScenarioDataViewModel();
        public ScenarioComparisonResult ComparisonResult { get; set; } = new ScenarioComparisonResult();

    }

}
