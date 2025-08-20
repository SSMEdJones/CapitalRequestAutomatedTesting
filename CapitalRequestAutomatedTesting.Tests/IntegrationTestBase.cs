using CapitalRequestAutomatedTesting.Tests.Models;
using CapitalRequestAutomatedTesting.UI;
using CapitalRequestAutomatedTesting.UI.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SSMWorkflow.API.DataAccess.ConfiguratonSettings;
using System.Diagnostics;
using System.Security.Claims;

namespace CapitalRequestAutomatedTesting.Tests
{
    public abstract class IntegrationTestBase
    {
        protected readonly ServiceProvider _provider;

        protected IntegrationTestBase(TestUser testUser = null)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var services = new ServiceCollection();

            services.AddSingleton<DiagnosticSource>(new DiagnosticListener("TestDiagnosticListener"));
            services.AddSingleton<DiagnosticListener>(new DiagnosticListener("TestDiagnosticListener"));

            // Other service registrations...
            // Register logging so ILogger<T> can be injected
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
            });

            var builder = WebApplication.CreateBuilder();

            // Register necessary services for MVC
            services.AddControllersWithViews();
            services.AddRazorPages();
            
            // Register our mock view render service instead of the real one
            services.AddScoped<IViewRenderService, MockViewRenderService>();
            
            services.AddApplicationServices(configuration, builder.Environment);

            // Use default test user if none provided
            testUser ??= new TestUser
            {
                Email = "edward.jones@ssmhealth.com",
                DomainUsername = "DS\\ejones08",
                GivenName = "Edward",
                Surname = "Jones",
                UserId = "ejones08"
            };

            var context = new DefaultHttpContext();
            context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, testUser.Email),
                new Claim(ClaimTypes.Name, testUser.DomainUsername),
                new Claim(ClaimTypes.GivenName, testUser.GivenName),
                new Claim(ClaimTypes.Surname, testUser.Surname),
                new Claim(ClaimTypes.NameIdentifier, testUser.UserId)
            }, "TestAuth"));

            services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor { HttpContext = context });
            services.AddSingleton<IConfiguration>(configuration);

            _provider = services.BuildServiceProvider();

            var settings = _provider.GetRequiredService<IOptionsMonitor<SSMWorkFlowSettings>>();
            Debug.WriteLine($"✅ Test loaded BaseApiUrl: {settings.CurrentValue.BaseApiUrl}");
        }
    }
}
