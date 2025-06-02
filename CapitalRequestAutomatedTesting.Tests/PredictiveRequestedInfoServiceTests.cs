using AutoMapper;
using CapitalRequest.API.Models;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using SSMWorkflow.API.DataAccess.AutoMapper.MappingProfile;

public class PredictiveRequestedInfoServiceTests
{
    private readonly PredictiveRequestedInfoService _service;

    public PredictiveRequestedInfoServiceTests()
    {
        // Setup DI container or manually instantiate services
        var services = new ServiceCollection();

        // Register real or test implementations
        services.AddScoped<ISSMWorkflowServices, SSMWorkflowServices>(); // Replace with your actual implementation
        services.AddScoped<ICapitalRequestServices, CapitalRequestServices>(); // Replace with your actual implementation
        services.AddScoped<IUserContextService, UserContextService>(); // Replace with your actual implementation
        services.AddAutoMapper(typeof(WorkflowProfile)); // Or your AutoMapper profile

        var provider = services.BuildServiceProvider();

        _service = new PredictiveRequestedInfoService(
            provider.GetRequiredService<ISSMWorkflowServices>(),
            provider.GetRequiredService<ICapitalRequestServices>(),
            provider.GetRequiredService<IUserContextService>(),
            provider.GetRequiredService<IMapper>()
        );
    }

    [Fact]
    public void Generate_ShouldReturnRequestedInfo()
    {
        // Arrange
        var proposal = new Proposal
        {
            Id = 2884,
            ReviewerGroupId = 2,
            SegmentId = 5,
            Author = "Edward Jones",
            WorkflowId = Guid.Parse("6E5DD951-4D33-F011-A318-0050569736FD"),
            RequestedInfo = new RequestedInfo
            {
                ReviewerGroupId = 3
            }
        };

        // Act
        var result = _service.CreateRequestedInfo(proposal);

        // Assert
        Assert.NotNull(result);
        Console.WriteLine($"RequestedInfo: {System.Text.Json.JsonSerializer.Serialize(result)}");
    }
}
