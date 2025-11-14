#nullable disable
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using CapitalRequestAutomatedTesting.UI.Services;
using CapitalRequestAutomatedTesting.UI.Services.Actual;
using CapitalRequestAutomatedTesting.UI.Services.Predictive;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

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

            AnalyzeDifferences(result);
            // Assert
            Assert.NotNull(result);
        }

        private void AnalyzeDifferences(ScenarioComparisonResult result)
        {
            Debug.WriteLine("=== DIFFERENCE ANALYSIS ===");

            if (!result.TablesOnlyInPredictive.Any() &&
                !result.TablesOnlyInActual.Any() &&
                !result.DifferingTables.Any())
            {
                Debug.WriteLine("✅ PERFECT MATCH: No differences found!");
                return;
            }

            if (result.TablesOnlyInPredictive.Any())
                Debug.WriteLine($"⚠️ Tables only in Predictive: {string.Join(", ", result.TablesOnlyInPredictive)}");

            if (result.TablesOnlyInActual.Any())
                Debug.WriteLine($"⚠️ Tables only in Actual: {string.Join(", ", result.TablesOnlyInActual)}");

            foreach (var table in result.DifferingTables)
            {
                Debug.WriteLine($"\n📊 Table '{table.TableName}' has differences:");

                // Count total field differences for quick overview
                var fieldDiffCount = (table.OperationGroups ?? new List<OperationGroupDifference>())
                    .SelectMany(og => og.Records)
                    .SelectMany(r => r.FieldDifferences)
                    .Count();

                Debug.WriteLine($"   - {fieldDiffCount} field differences");
                Debug.WriteLine($"   - {table.OnlyInPredictive.Count} records only in Predictive");
                Debug.WriteLine($"   - {table.OnlyInActual.Count} records only in Actual");
            }
        }
    }
}
