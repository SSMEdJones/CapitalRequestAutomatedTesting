using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using CapitalRequestAutomatedTesting.UI.Services.Predictive;
using Microsoft.Extensions.DependencyInjection;


namespace CapitalRequestAutomatedTesting.Tests
{
    public class RollbackServiceTests : IntegrationTestBase
    {
        private readonly IPredictiveScenarioService _predictiveScenarioService;

        public RollbackServiceTests()
        {
            _predictiveScenarioService = _provider.GetRequiredService<IPredictiveScenarioService>();
        }


        [Fact]
        public async Task DiscoverRollbackServicesForScenario()
        {
            // Arrange
            var scenario = new ScenarioDetailsViewModel
            {
                ProposalId = 2936,
                ScenarioId = "SCN002",
                PartialViewName = "_ReplyToRequest",
                DisplayText = "Reply to Request",
                RequestingGroupId = 4,
                ReplyingGroupId = 5,
                ReviewerId = 37807,
                RequestedInformation = "Supply Chain requesting more information from EPMO as Pam Shumway via Workflow Automated Testing - Request More Information Scenario.",
                ReturnedInformation = "EPMO replying to request for more information from Supply Chain as Gavin Harrell via Workflow Automated Testing - Reply to Request Scenario.",
                RequestedInfoId = 691,
                PredictiveCompletionStep = 0,
                CanExecuteActualSteps = true
            };
            var scenarioDetail = await _predictiveScenarioService.GenerateScenarioDataAsync(scenario);
            var methods = scenarioDetail.PredictiveMethods;
            // Act
            //var results = await _service.ExecuteRollbackAsync(methods);

            // Assert
       //     foreach (var candidate in results)
       //     {
       //         Debug.WriteLine($"Rollback Candidate:");
       //         Debug.WriteLine($"  Method: {candidate.MethodName}");
       //         Debug.WriteLine($"  Rollback Method: {candidate.RollbackMethodName}");
       //         Debug.WriteLine($"  Description: {candidate.Description}");
       //         Debug.WriteLine($"  Predictive Data: {string.Join(", ", candidate.PredictiveData.Select(kv => $"{kv.Key}={kv.Value}"))}");
       //         Debug.WriteLine($"  Actual Data: {string.Join(", ", candidate.ActualData.Select(kv => $"{kv.Key}={kv.Value}"))}");
       //     }

       // Assert: You can add checks for missing rollback services or unexpected results

       //Assert.True(results.Count > 0, "No rollback candidates found.");
        }
    }
}
