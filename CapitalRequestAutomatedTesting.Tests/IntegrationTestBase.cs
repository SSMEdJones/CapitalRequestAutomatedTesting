using CapitalRequestAutomatedTesting.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CapitalRequestAutomatedTesting.Tests
{
    public abstract class IntegrationTestBase
    {
        protected readonly ServiceProvider _provider;

        protected IntegrationTestBase()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var services = new ServiceCollection();
            services.AddApplicationServices(configuration);

            _provider = services.BuildServiceProvider();
        }
    }

}
