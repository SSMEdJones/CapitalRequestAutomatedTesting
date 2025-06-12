using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using CapitalRequestAutomatedTesting.UI.Services;
using dto = CapitalRequest.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;
using SSMWorkflow.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using CapitalRequestAutomatedTesting.Data;
using Microsoft.Extensions.DependencyInjection;

public class ScenarioMethodExecutionTests
{
    private readonly Mock<ICapitalRequestServices> _capitalRequestMock;
    private readonly Mock<ISSMWorkflowServices> _ssmWorkflowMock;
    private readonly Mock<IWorkflowControllerService> _workflowControllerMock;
    private readonly Mock<IPredictiveRequestedInfoService> _requestedInfoMock;
    private readonly Mock<IPredictiveWorkflowStepResponderService> _responderMock;
    private readonly Mock<IPredictiveWorkflowStepOptionService> _optionMock;
    private readonly Mock<IPredictiveEmailNotificationService> _emailMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;

    private readonly PredictiveScenarioService _predictiveScenarioService;

    public ScenarioMethodExecutionTests()
    {
        // Initialize mocks
        _capitalRequestMock = new Mock<ICapitalRequestServices>();
        _ssmWorkflowMock = new Mock<ISSMWorkflowServices>();
        _workflowControllerMock = new Mock<IWorkflowControllerService>();
        _requestedInfoMock = new Mock<IPredictiveRequestedInfoService>();
        _responderMock = new Mock<IPredictiveWorkflowStepResponderService>();
        _optionMock = new Mock<IPredictiveWorkflowStepOptionService>();
        _emailMock = new Mock<IPredictiveEmailNotificationService>();
        _serviceProviderMock = new Mock<IServiceProvider>();

        

        _predictiveScenarioService = new PredictiveScenarioService(
            _capitalRequestMock.Object,
            _ssmWorkflowMock.Object,
            _workflowControllerMock.Object,
            _requestedInfoMock.Object,
            _responderMock.Object,
            _optionMock.Object,
            _emailMock.Object,
            _serviceProviderMock.Object);
    }

    [Fact]
    public void Should_Invoke_Methods_Dynamically_And_Return_Valid_Results()
    {
        // Set up mock return values for verification
        var workflowStepOptionID = Guid.NewGuid();

        var mockRequestedInfo = new dto.RequestedInfo { Id = 1001, RequestedInformation = "Mocked Info" };
        var mockResponder = new WorkflowStepResponder { WorkflowStepOptionID = workflowStepOptionID, Responder = "Mocked Responder" };

        _requestedInfoMock.Setup(x => x.CreateRequestedInfoAsync(It.IsAny<vm.Proposal>(), It.IsAny<int>()))
                          .ReturnsAsync(mockRequestedInfo);

        _responderMock.Setup(x => x.CreateWorkflowStepResponderAsync(It.IsAny<vm.Proposal>(), It.IsAny<string>()))
                      .ReturnsAsync(mockResponder);
        var scenarioDetailViewModel = new ScenarioDetailsViewModel
        {
            ScenarioId = "SCN001",
            ProposalId = 2844
        };

        var tables = new List<PredictiveTable> { new PredictiveTable { TableName = "RequestedInfo" } };

        var methods = new List<PredictiveMethod>
        {
            new PredictiveMethod { ServiceName = "IPredictiveRequestedInfoService", MethodName = "CreateRequestedInfoAsync" },
            new PredictiveMethod { ServiceName = "IPredictiveWorkflowStepResponderService", MethodName = "CreateWorkflowStepResponderAsync" }
        };

        var services = new ServiceCollection();
        services.AddScoped<IPredictiveRequestedInfoService, PredictiveRequestedInfoService>(); // Register correctly
        var serviceProvider = services.BuildServiceProvider();

        // Try resolving the service again
        var serviceInstance = serviceProvider.GetService<IPredictiveRequestedInfoService>();
        Console.WriteLine($"Resolved Service Instance: {serviceInstance}");

        var executor = new PredictiveScenarioService(
                            _capitalRequestMock.Object,
                            _ssmWorkflowMock.Object,
                            _workflowControllerMock.Object,
                            _requestedInfoMock.Object,
                            _responderMock.Object,
                            _optionMock.Object,
                            _emailMock.Object,
                            serviceProvider);

        // Act
        var result = _predictiveScenarioService.ExecuteScenarioMethods(tables, methods, scenarioDetailViewModel);

        // Assert - Properly compare expected results
        Assert.NotNull(result.RequestedInfo);
        Assert.Equal(mockRequestedInfo.Id, result.RequestedInfo.Id);
        Assert.Equal(mockRequestedInfo.RequestedInformation, result.RequestedInfo.RequestedInformation);

        Assert.NotNull(result.WorkflowStepResponder);
        Assert.Equal(mockResponder.WorkflowStepOptionID, result.WorkflowStepResponder.WorkflowStepOptionID);
        Assert.Equal(mockResponder.Responder, result.WorkflowStepResponder.Responder);
    }
}

//using Xunit;
//using Moq;
//using System;
//using System.Reflection;
//using System.Collections.Generic;
//using CapitalRequestAutomatedTesting.UI.Services;
//using dto = CapitalRequest.API.DataAccess.Models;
//using vm = CapitalRequest.API.Models;
//using SSMWorkflow.API.DataAccess.Models;
//using CapitalRequestAutomatedTesting.UI.Models;
//using CapitalRequestAutomatedTesting.Data;
//using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;

//public class ScenarioMethodExecutionTests
//{
//    private readonly Mock<ICapitalRequestServices> _capitalRequestMock;
//    private readonly Mock<ISSMWorkflowServices> _ssmWorkflowMock;
//    private readonly Mock<IWorkflowControllerService> _workflowControllerMock;
//    private readonly Mock<IPredictiveRequestedInfoService> _requestedInfoMock;
//    private readonly Mock<IPredictiveWorkflowStepResponderService> _responderMock;
//    private readonly Mock<IPredictiveWorkflowStepOptionService> _optionMock;
//    private readonly Mock<IPredictiveEmailNotificationService> _emailMock;
//    private readonly Mock<IServiceProvider> _serviceProviderMock;

//    private readonly PredictiveScenarioService _predictiveScenarioService;

//    public ScenarioMethodExecutionTests()
//    {
//        _capitalRequestMock = new Mock<ICapitalRequestServices>();
//        _ssmWorkflowMock = new Mock<ISSMWorkflowServices>();
//        _workflowControllerMock = new Mock<IWorkflowControllerService>();
//        _requestedInfoMock = new Mock<IPredictiveRequestedInfoService>();
//        _responderMock = new Mock<IPredictiveWorkflowStepResponderService>();
//        _optionMock = new Mock<IPredictiveWorkflowStepOptionService>();
//        _emailMock = new Mock<IPredictiveEmailNotificationService>();
//        _serviceProviderMock = new Mock<IServiceProvider>();

//        // Mock service provider resolution
//        _serviceProviderMock.Setup(sp => sp.GetService(typeof(IPredictiveRequestedInfoService)))
//                            .Returns(_requestedInfoMock.Object);
//        _serviceProviderMock.Setup(sp => sp.GetService(typeof(IPredictiveWorkflowStepResponderService)))
//                            .Returns(_responderMock.Object);
//        _serviceProviderMock.Setup(sp => sp.GetService(typeof(IPredictiveWorkflowStepOptionService)))
//                            .Returns(_optionMock.Object);
//        _serviceProviderMock.Setup(sp => sp.GetService(typeof(IPredictiveEmailNotificationService)))
//                            .Returns(_emailMock.Object);

//        // Instantiate service with mocked dependencies
//        _predictiveScenarioService = new PredictiveScenarioService(
//            _capitalRequestMock.Object,
//            _ssmWorkflowMock.Object,
//            _workflowControllerMock.Object,
//            _requestedInfoMock.Object,
//            _responderMock.Object,
//            _optionMock.Object,
//            _emailMock.Object,
//            _serviceProviderMock.Object);

//        // Mocking services
//        var requestedInfoServiceMock = new Mock<IPredictiveRequestedInfoService>();
//        var proposal = new vm.Proposal();

//        requestedInfoServiceMock.Setup(x => x.CreateRequestedInfoAsync(It.IsAny<vm.Proposal>(), It.IsAny<int>())).ReturnsAsync(It.IsAny<dto.RequestedInfo>());

//        var workflowServiceMock = new Mock<IPredictiveWorkflowStepResponderService>();
//        workflowServiceMock.Setup(x => x.CreateWorkflowStepResponderAsync(It.IsAny<vm.Proposal>(), It.IsAny<string>())).ReturnsAsync(It.IsAny<WorkflowStepResponder>());

//        // Inject mocks into the service provider
//        _serviceProviderMock.Setup(sp => sp.GetService(typeof(IPredictiveRequestedInfoService)))
//                            .Returns(requestedInfoServiceMock.Object);
//        _serviceProviderMock.Setup(sp => sp.GetService(typeof(IPredictiveWorkflowStepResponderService)))
//                            .Returns(workflowServiceMock.Object);
//    }

//    [Fact]
//    public void Should_Invoke_Methods_Dynamically()
//    {
//        var scenarioDetailViewModel = new ScenarioDetailsViewModel
//        {
//            ScenarioId = "SCN001",
//            PartialViewName = "_RequestMoreInfo",
//            DisplayText = "Request more info",
//            ProposalId = 2844,
//            SequenceNumber = 1,
//            RequestingGroupId = 2,
//            TargetGroupId = 3,
//            ReviewerId = 25
//        };

//        // Arrange
//        var tables = new List<PredictiveTable>
//        {
//        new PredictiveTable { DatabaseName = "SSMWorkflow", TableName = "EmailNotifications" },
//        new PredictiveTable { DatabaseName = "SSMWorkflow", TableName = "WorkflowInstanceActionHistory" },
//        new PredictiveTable { DatabaseName = "SSMWorkflow", TableName = "WorkflowStepOption" },
//        new PredictiveTable { DatabaseName = "SSMWorkflow", TableName = "WorkflowStepResponder" },
//        new PredictiveTable { DatabaseName = "CapitalRequest", TableName = "RequestedInfo" }
//        };

//        var methods = new List<PredictiveMethod>
//        {
//            new PredictiveMethod { ServiceName = "IPredictiveRequestedInfoService", MethodName = "CreateRequestedInfoAsync" },
//            new PredictiveMethod { ServiceName = "IPredictiveWorkflowStepResponderService", MethodName = "CreateWorkflowStepResponderAsync" }
//        };

//        var executor = new PredictiveScenarioService(
//                    _capitalRequestMock.Object,
//                    _ssmWorkflowMock.Object,
//                    _workflowControllerMock.Object,
//                    _requestedInfoMock.Object,
//                    _responderMock.Object,
//                    _optionMock.Object,
//                    _emailMock.Object,
//                    _serviceProviderMock.Object);

//        // Act
//        var result = executor.ExecuteScenarioMethods(tables, methods, scenarioDetailViewModel);

//        // Assert
//          Assert.NotNull(result.RequestedInfo);
//        Assert.Equal(It.IsAny<dto.RequestedInfo>(), result.RequestedInfo);
//        Assert.Equal(It.IsAny<WorkflowStepResponder>(), result.WorkflowStepResponder);
//    }
//}


