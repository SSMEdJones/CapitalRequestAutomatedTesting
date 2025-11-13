#nullable disable
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using CapitalRequestAutomatedTesting.UI.Services;
using CapitalRequestAutomatedTesting.UI.Services.Actual;
using CapitalRequestAutomatedTesting.UI.Services.Predictive;
using Microsoft.Extensions.DependencyInjection;

namespace CapitalRequestAutomatedTesting.Tests
{
    public class ScenarioComparer : IntegrationTestBase
    {
        private readonly IScenarioComparer _service;
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly IPredictiveScenarioService _predictiveScenarioService;
        private readonly IActualScenarioService _actualScenarioService;

        public ScenarioComparer()
        {
            _service = _provider.GetRequiredService<IScenarioComparer>();
            _capitalRequestServices = _provider.GetRequiredService<ICapitalRequestServices>();
            _ssmWorkflowServices = _provider.GetRequiredService<ISSMWorkflowServices>();
            _predictiveScenarioService = _provider.GetRequiredService<IPredictiveScenarioService>();
            _actualScenarioService = _provider.GetRequiredService<IActualScenarioService>();

        }

        [Fact]
        public async Task CompareData_WithValidData_ReturnsScenarioComparisonResult()
        {

            // Arrange
            int proposalId = 2948;
            var proposal = await _capitalRequestServices.GetProposal(proposalId);

            proposal.WorkflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId))
                .Where(x => !x.IsComplete)
                .FirstOrDefault();

            var nullField = "NULL";
            var scenario = new ScenarioDetailsViewModel
            {
                ScenarioId = "SCN003",
                ProposalId = proposalId,
                SubmitUserId = "tfujim"
            };
            scenario.PredictiveData = await _predictiveScenarioService.GenerateScenarioDataAsync(scenario);
            scenario.ActualData = await _actualScenarioService.GenerateScenarioDataAsync(scenario);

            var predictive = scenario.PredictiveData;
            var actual = scenario.ActualData;

           //var scenarioComparisonResult = _service.CompareData(predictive, actual);

            var result = _service.CompareData(predictive, actual);

            
            // Assert
            Assert.NotNull(result);
        }

    }
}
