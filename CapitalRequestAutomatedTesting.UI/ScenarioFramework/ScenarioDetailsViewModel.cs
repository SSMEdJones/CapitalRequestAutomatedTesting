using Microsoft.AspNetCore.Mvc.Rendering;

namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class ScenarioDetailsViewModel
    {

        public string ScenarioId { get; set; }
        public string PartialViewName { get; set; } 
        public string DisplayText { get; set; } 
        public int RequestingGroupId { get; set; }
        public int TargetGroupId { get; set; }
        public int ReviewerId { get; set; }
        public string UserInformation { get; set; }
        public int SequenceNumber { get; set; }

        public List<SelectListItem> RequestingGroups { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> TargetGroups { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Reviewers { get; set; } = new List<SelectListItem>();
    }

}
