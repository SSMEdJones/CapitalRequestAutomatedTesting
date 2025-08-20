using AutoMapper;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.Controllers;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using CapitalRequestAutomatedTesting.UI.Services;
using CapitalRequestAutomatedTesting.UI.Services.Actual;
using CapitalRequestAutomatedTesting.UI.Services.Original;
using CapitalRequestAutomatedTesting.UI.Services.Predictive;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace CapitalRequestAutomatedTesting.Tests
{
    public class RollbackControllerTests : IntegrationTestBase
    {
        private readonly IRollbackService _rollbackService;
        private readonly IPredictiveScenarioService _predictiveScenarioService;
        private readonly IScenarioComparer _scenarioComparer;

        //todo remove
        private readonly IActualScenarioService _actualScenarioService;
        private readonly IOriginalScenarioService _originalScenarioService;
        private readonly IPredictiveSeleniumService _predictiveSeleniumService;
        private readonly IActualSeleniumService _actualSeleniumService;

        private readonly RollbackController _controller;

        public RollbackControllerTests()
        {
            _rollbackService = _provider.GetRequiredService<IRollbackService>();
            _predictiveScenarioService = _provider.GetRequiredService<IPredictiveScenarioService>();
            _scenarioComparer = _provider.GetRequiredService<IScenarioComparer>();
            _actualScenarioService = _provider.GetRequiredService<IActualScenarioService>();
            _predictiveSeleniumService = _provider.GetRequiredService<IPredictiveSeleniumService>();
            _actualSeleniumService = _provider.GetRequiredService<IActualSeleniumService>();
            _originalScenarioService = _provider.GetRequiredService<IOriginalScenarioService>();

            _controller = new RollbackController(
                _rollbackService,
                _predictiveScenarioService,
                _scenarioComparer,
                _actualScenarioService,
                _originalScenarioService,
                _predictiveSeleniumService,
                _actualSeleniumService 
            );

            var tempData = new TempDataDictionary(new DefaultHttpContext(), MockTempDataProvider());
            _controller.TempData = tempData;


        }
        
        

        [Fact]
        public async Task RunSelected_ShouldRedirectToRollback_WhenPredictionFails()
        {
            // Arrange
            var scenario = new ScenarioDetailsViewModel
            {
                ScenarioId = "SCN002",
                ProposalId = 2936,
                RequestedInformation = "Test Request",
                ReturnedInformation = "Test Return",
                RequestingGroupId = 4,
                ReplyingGroupId = 5,
                ReviewerId = 37807,
                RequestedInfoId = 691,
                PredictiveCompletionStep = 0,
                CanExecuteActualSteps = true,
                PredictedSeleniumOutcome = new SeleniumScenarioOutcome
                {
                    Success = false,
                    //StopReason = "Forced failure for rollback test"
                }
            };

            var model = new ScenarioFormViewModel
            {
                SelectedScenarioIds = new List<string> { "SCN002" },
                ScenarioDetails = new List<ScenarioDetailsViewModel> { scenario }
            };

            var modelJson = JsonConvert.SerializeObject(model);
            _controller.TempData["ScenarioModel"] = modelJson;

            // Act
            var result = await _controller.RunSelected();

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Preview", redirect.ActionName);
            Assert.Equal("Rollback", redirect.ControllerName);
            Assert.Equal("SCN002", redirect.RouteValues["scenarioId"]);

            Assert.True(_controller.TempData.ContainsKey("ScenarioDetail"));
        }

        private ITempDataProvider MockTempDataProvider()
        {
            return new MockTempDataProvider(); // You can implement a simple mock or use Moq
        }
    }

    
}
