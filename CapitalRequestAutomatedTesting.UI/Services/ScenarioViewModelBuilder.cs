using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CapitalRequestAutomatedTesting.UI.Services
{
    public interface IScenarioViewModelBuilder
    {
        Task<ScenarioDetailsViewModel> PopulateListsAsync(ScenarioDetailsViewModel vm, int proposalId);
    }

    public class ScenarioViewModelBuilder
    {
        private readonly IScenarioControllerService _scenarioControllerService;

        public ScenarioViewModelBuilder(IScenarioControllerService scenarioControllerService)
        {
            _scenarioControllerService = scenarioControllerService;
        }

        public async Task<ScenarioDetailsViewModel> PopulateListsAsync(ScenarioDetailsViewModel vm, int proposalId)
        {
            if (vm.ScenarioId == "SCN001")
            {
                (var requestingGroups, var targetGroups) =
                    await _scenarioControllerService.BuildRequestingAndTargetGroupsAsync(proposalId, vm.RequestingGroupId);

                vm.RequestingGroups = requestingGroups;
                vm.TargetGroups = targetGroups;
                vm.Reviewers = await _scenarioControllerService.GetReviewersBySelectedGroupAsync(proposalId, vm.RequestingGroupId ?? 0);

                // Set selected items
                vm.RequestingGroups.ForEach(x => x.Selected = x.Value == vm.RequestingGroupId?.ToString());
                vm.TargetGroups.ForEach(x => x.Selected = x.Value == vm.TargetGroupId?.ToString());
                vm.Reviewers.ForEach(x => x.Selected = x.Value == vm.ReviewerId?.ToString());
            }

            return vm;
        }


        public async Task<List<SelectListItem>> BuildRequestSelectListAsync(int? selectedRequestId)
        {
            var list = await _scenarioControllerService.GetRequestSelectListAsync();
            var selectedValue = selectedRequestId?.ToString();

            list.ForEach(x =>
            {
                x.Selected = x.Value == selectedValue;
            });

            return list;
        }

    }

}
