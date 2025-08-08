using CapitalRequestAutomatedTesting.UI.Interfaces;
using CapitalRequestAutomatedTesting.UI.Services.Rollback;

public interface IRollbackServiceFactory
{
    IRollbackHandler GetService(string serviceName);
}


public class RollbackServiceFactory : IRollbackServiceFactory
{
    private readonly IServiceProvider _provider;

    public RollbackServiceFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public IRollbackHandler GetService(string serviceName)
    {
        return serviceName switch
        {
            nameof(RollbackProvidedInfoService) => _provider.GetRequiredService<RollbackProvidedInfoService>(),
            //nameof(RollbackWorkflowStepOptionService) => _provider.GetRequiredService<RollbackWorkflowStepOptionService>(),
            _ => throw new ArgumentException($"Unknown rollback service: {serviceName}")
        };
    }
}
