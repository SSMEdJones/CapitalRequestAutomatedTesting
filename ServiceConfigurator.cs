using CapitalRequest.API.DataAccess.AutoMapper.MappingProfile;
using CapitalRequest.API.DataAccess.ConfigurationSettings;
using CapitalRequest.API.DataAccess.Services.Api;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.Services;
using DinkToPdf.Contracts;
using DinkToPdf;
using SSMWorkflow.API.DataAccess.AutoMapper.MappingProfile;
using SSMWorkflow.API.DataAccess.ConfiguratonSettings;
using SSMWorkflow.API.DataAccess.Services;
using SSMWorkflow.API.DataAccess.Services.Api;
using CapitalRequestAutomatedTesting.UI.Helpers;
using CapitalRequestAutomatedTesting.UI.AutoMapper.MappingProfile;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;

namespace CapitalRequestAutomatedTesting.UI
{
    public static class ServiceConfigurator
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
        {
            services.Configure<SSMWorkFlowSettings>(configuration.GetSection("ssmWorkFlowAPISettings"));
            services.Configure<CapitalRequestSettings>(configuration.GetSection("capitalRequestAPISettings"));

            // Create and register the AppConfiguration service
            services.AddSingleton<IAppConfigurationService>(provider => 
            {
                var logger = provider.GetRequiredService<ILogger<AppConfigurationService>>();
                return new AppConfigurationService(configuration, logger);
            });

            // Get the service for configuration lookups
            var serviceProvider = services.BuildServiceProvider();
            var appConfigService = serviceProvider.GetRequiredService<IAppConfigurationService>();

            services.PostConfigureAll<SSMWorkFlowSettings>(options =>
            {
                options.BaseApiUrl = appConfigService.GetAppKeyValueByKey("CapitalRequest", "SSMWorkflowAPI").LookupValue;
                options.ProjectReviewLink = appConfigService.GetAppKeyValueByKey("CapitalRequest", "ProjectReviewLink").LookupValue;
            });

            services.PostConfigureAll<CapitalRequestSettings>(options =>
            {
                options.BaseApiUrl = appConfigService.GetAppKeyValueByKey("CapitalRequest", "CapitalRequestApiUrl").LookupValue;
            });

            services.AddAutoMapper(typeof(WorkflowProfile), typeof(CapitalRequestProfile), typeof(AutomatedTestingProfile));

            #region UI
            services.AddScoped<IWorkflowControllerService, WorkflowControllerService>();
            // AppConfigurationService is already registered above
            #endregion

            // The rest of your service registrations remain unchanged
            #region Workflow API
            services.AddScoped<ISSMWorkflowServices, SSMWorkflowServices>();
            // ... rest of service registrations
            #endregion

            return services;
        }
    }
}