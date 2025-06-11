using Microsoft.AspNetCore.Mvc.Rendering;

namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{

    public class ScenarioFormViewModel
    {
        public int? RequestId { get; set; }

        public List<SelectListItem> RequestIds { get; set; } = new List<SelectListItem>();

        public List<string> SelectedScenarioIds { get; set; } = new();
        public List<ScenarioDetailsViewModel> ScenarioDetails { get; set; } = new List<ScenarioDetailsViewModel>();
    }

}
