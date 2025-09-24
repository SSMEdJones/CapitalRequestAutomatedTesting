using AutoMapper;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Controllers;
using CapitalRequestAutomatedTesting.UI.Hubs;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using CapitalRequestAutomatedTesting.UI.Services;
using CapitalRequestAutomatedTesting.UI.Services.Actual;
using CapitalRequestAutomatedTesting.UI.Services.Original;
using CapitalRequestAutomatedTesting.UI.Services.Predictive;
using Infrastructure.ApiDiagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace CapitalRequestAutomatedTesting.Tests
{
    public class ScenarioControllerTests : IntegrationTestBase
    {
        private readonly ILogger<ScenarioController> _logger;
        private readonly IScenarioControllerService _scenarioControllerService;
        private readonly IWorkflowControllerService _workflowControllerService;
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly IPredictiveScenarioService _predictiveScenarioService;
        private readonly IActualScenarioService _actualScenarioService;
        private readonly IOriginalScenarioService _originalScenarioService;
        private readonly IPredictiveSeleniumService _predictiveSeleniumService;
        private readonly IActualSeleniumService _actualSeleniumService;
        private readonly IViewRenderService _viewRenderService;
        private readonly IScenarioMemoryCache _scenarioMemoryCache;
        private readonly ScenarioViewModelBuilder _viewModelBuilder;
        private readonly IScenarioComparer _scenarioComparer;
        private readonly IFormDataContext _formDataContext;
        private readonly IMapper _mapper;
        private readonly IHubContext<ScenarioProgressHub> _hubContext;

        private readonly ScenarioController _controller;

        public ScenarioControllerTests()
        {
            _logger = _provider.GetRequiredService<ILogger<ScenarioController>>();
            _scenarioControllerService = _provider.GetRequiredService<IScenarioControllerService>();
            _workflowControllerService = _provider.GetRequiredService<IWorkflowControllerService>();
            _capitalRequestServices = _provider.GetRequiredService<ICapitalRequestServices>();
            _predictiveScenarioService = _provider.GetRequiredService<IPredictiveScenarioService>();
            _actualScenarioService = _provider.GetRequiredService<IActualScenarioService>();
            _originalScenarioService = _provider.GetRequiredService<IOriginalScenarioService>();
            _predictiveSeleniumService = _provider.GetRequiredService<IPredictiveSeleniumService>();
            _actualSeleniumService = _provider.GetRequiredService<IActualSeleniumService>();
            _viewRenderService = _provider.GetRequiredService<IViewRenderService>();
            _scenarioMemoryCache = _provider.GetRequiredService<IScenarioMemoryCache>();
            _viewModelBuilder = _provider.GetRequiredService<ScenarioViewModelBuilder>();
            _scenarioComparer = _provider.GetRequiredService<IScenarioComparer>();
            _formDataContext = _provider.GetRequiredService<IFormDataContext>();
            _mapper = _provider.GetRequiredService<IMapper>();
            _hubContext = _provider.GetRequiredService<IHubContext<ScenarioProgressHub>>();

            _controller = new ScenarioController(
                _logger,
                _scenarioControllerService,
                _workflowControllerService,
                _capitalRequestServices,
                _predictiveScenarioService,
                _actualScenarioService,
                _originalScenarioService,
                _predictiveSeleniumService,
                _actualSeleniumService,
                _viewRenderService,
                _scenarioMemoryCache,
                _viewModelBuilder,
                _scenarioComparer,
                _formDataContext,
                _mapper,
                _hubContext
            );

            var tempData = new TempDataDictionary(new DefaultHttpContext(), MockTempDataProvider());
            _controller.TempData = tempData;
        }

        [Fact]
        public async Task RunSelected_ShouldRedirectToComparison_WhenPredictionSucceeds()
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
                CanExecuteActualSteps = true
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
            Assert.Equal("ViewComparison", redirect.ActionName);

            Assert.True(_controller.TempData.ContainsKey("Scenario"));
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

    public class MockTempDataProvider : ITempDataProvider
    {
        private Dictionary<string, object> _tempData = new Dictionary<string, object>();

        public IDictionary<string, object> LoadTempData(HttpContext context)
        {
            return _tempData;
        }

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
            _tempData = new Dictionary<string, object>(values);
        }
    }
}
