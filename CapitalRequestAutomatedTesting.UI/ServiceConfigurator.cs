using CapitalRequest.API.DataAccess.AutoMapper.MappingProfile;
using CapitalRequest.API.DataAccess.ConfigurationSettings;
using CapitalRequest.API.DataAccess.Services.Api;
using CapitalRequestAutomatedTesting.Data.Helpers;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.AutoMapper.MappingProfile;
using CapitalRequestAutomatedTesting.UI.Helpers;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using CapitalRequestAutomatedTesting.UI.Services;
using CapitalRequestAutomatedTesting.UI.Services.Actual;
using CapitalRequestAutomatedTesting.UI.Services.Error;
using CapitalRequestAutomatedTesting.UI.Services.Original;
using CapitalRequestAutomatedTesting.UI.Services.Predictive;
using CapitalRequestAutomatedTesting.UI.Services.Rollback;
using DinkToPdf;
using DinkToPdf.Contracts;
using SSMWorkflow.API.DataAccess.AutoMapper.MappingProfile;
using SSMWorkflow.API.DataAccess.ConfigurationSettings;
using SSMWorkflow.API.DataAccess.Services;
using SSMWorkflow.API.DataAccess.Services.Api;

namespace CapitalRequestAutomatedTesting.UI
{
    public static class ServiceConfigurator
    {

        public static IServiceCollection AddApplicationServices(this IServiceCollection services,IConfiguration configuration, IWebHostEnvironment env)
        {

            services.Configure<SSMWorkFlowSettings>(configuration.GetSection("ssmWorkFlowAPISettings"));
            services.Configure<CapitalRequestSettings>(configuration.GetSection("capitalRequestAPISettings"));

            var appConfigService = new AppConfigurationService(
                        configuration,
                        LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<AppConfigurationService>()
                    );

            // Register the existing instance as a singleton
            services.AddSingleton<IAppConfigurationService>(appConfigService);
            services.PostConfigureAll<SSMWorkFlowSettings>(options =>
            {
                options.BaseApiUrl = appConfigService.GetAppKeyValueByKey("CapitalRequest", "SSMWorkflowAPI").LookupValue;
                options.ProjectReviewLink = appConfigService.GetAppKeyValueByKey("CapitalRequest", "ProjectReviewLink").LookupValue;
            });

            services.PostConfigureAll<CapitalRequestSettings>(options =>
            {
                options.BaseApiUrl = appConfigService.GetAppKeyValueByKey("CapitalRequest", "CapitalRequestApiUrl").LookupValue;
            });

            services.AddAutoMapper(typeof(WorkflowProfile), typeof(CapitalRequestProfile),typeof(AutomatedTestingProfile) );


            #region UI
            services.AddScoped<IWorkflowControllerService, WorkflowControllerService>();
            // Register the AppConfiguration service
            services.AddSingleton<IAppConfigurationService, AppConfigurationService>();
            #endregion

            #region Workflow API
            services.AddScoped<ISSMWorkflowServices, SSMWorkflowServices>();
            services.AddScoped<IDashboards, Dashboards>();
            services.AddScoped<ISSMNotification, SSMNotification>();
            services.AddScoped<ISSMWorkFlow, SSMWorkFlow>();
            services.AddScoped<ISSMWorkFlowInstance, SSMWorkFlowInstance>();
            services.AddScoped<ISSMWorkFlowInstanceActionHistory, SSMWorkFlowInstanceActionHistory>();
            services.AddScoped<ISSMWorkFlowStakeholder, SSMWorkFlowStakeholder>();
            services.AddScoped<ISSMWorkFlowStep, SSMWorkFlowStep>();
            services.AddScoped<ISSMWorkFlowStepOption, SSMWorkFlowStepOption>();
            services.AddScoped<ISSMWorkFlowStepResponder, SSMWorkFlowStepResponder>();
            services.AddScoped<IWorkflowServices, WorkflowServices>();
            services.AddScoped<ISSMWorkflowServices, SSMWorkflowServices>();
            services.AddScoped<IEmailNotifications, EmailNotifications>();
            services.AddScoped<IDeletedReviewers, DeletedReviewers>();

            #endregion

            #region CapitalRequest API
            services.AddScoped<IApplicationUsers, ApplicationUsers>();
            services.AddScoped<IAssets, Assets>();
            services.AddScoped<IAttachments, Attachments>();
            services.AddScoped<IEmailTemplates, EmailTemplates>();
            services.AddScoped<IProposals, Proposals>();
            services.AddScoped<IProvidedInfos, ProvidedInfos>();
            services.AddScoped<IQuotes, Quotes>();
            services.AddScoped<IRequestedInfos, RequestedInfos>();
            services.AddScoped<IReviewerGroups, ReviewerGroups>();
            services.AddScoped<IReviewers, Reviewers>();
            services.AddScoped<IWBSs, WBSs>();
            services.AddScoped<IWorkflowActions, WorkflowActions>();
            services.AddScoped<IWorkflowTemplates, WorkflowTemplates>();
            services.AddScoped<ICapitalRequestServices, CapitalRequestServices>();
            services.AddScoped<IUserContextService, UserContextService>();

            #endregion

            #region Predictive Services
            services.AddScoped<IPredictiveRequestedInfoService, PredictiveRequestedInfoService>();
            services.AddScoped<IPredictiveWorkflowStepResponderService, PredictiveWorkflowStepResponderService>();
            services.AddScoped<IPredictiveWorkflowStepOptionService, PredictiveWorkflowStepOptionService>();
            services.AddScoped<IPredictiveWorkflowStepService, PredictiveWorkflowStepService>();
            services.AddScoped<IPredictiveEmailNotificationService, PredictiveEmailNotificationService>();
            services.AddScoped<IPredictiveScenarioService, PredictiveScenarioService>();
            services.AddScoped<IPredictiveWorkflowActionService, PredictiveWorkflowActionService>();
            services.AddScoped<IPredictiveProvidedInfoService, PredictiveProvidedInfoService>();
            services.AddScoped<IPredictiveFileService, PredictiveFileService>();

            //services.AddScoped<IPredictiveProposalControllerService, PredictiveProposalControllerService>();


            #endregion

            #region Actual Services
            services.AddScoped<IActualEmailNotificationService, ActualEmailNotificationService>();
            services.AddScoped<IActualRequestedInfoService, ActualRequestedInfoService>();
            services.AddScoped<IActualWorkflowStepOptionService, ActualWorkflowStepOptionService>();
            services.AddScoped<IActualWorkflowStepResponderService, ActualWorkflowStepResponderService>();
            services.AddScoped<IActualDashboardService, ActualDashboardService>();
            services.AddScoped<IActualScenarioService, ActualScenarioService>();
            services.AddScoped<IActualSeleniumService, ActualSeleniumService>();
            services.AddScoped<IActualProvidedInfoService, ActualProvidedInfoService>();
            #endregion
            services.AddScoped<IOriginalScenarioService, OriginalScenarioService>();
            
            #region Rollback Services
            services.AddScoped<IRollbackProvidedInfoService, RollbackProvidedInfoService>();
            services.AddScoped<IRollbackServiceFactory, RollbackServiceFactory>();
            services.AddScoped<IRollbackService, RollbackService>();
            services.AddScoped<IRollbackFileService, RollbackFileService>();
            services.AddScoped<IRollbackWorkflowStepOptionService, RollbackWorkflowStepOptionService>();
            services.AddScoped<IRollbackRequestedInfoService, RollbackRequestedInfoService>();
            services.AddScoped<IRollbackEmailNotificationService, RollbackEmailNotificationService>();


            #endregion

            #region Scenario Framework
            services.AddScoped<ITestActionService, WorkflowTestActionService>();
            services.AddScoped<IScenarioControllerService, ScenarioControllerService>();
            services.AddScoped<IScenarioComparer, ScenarioComparer>();
            services.AddScoped<ScenarioViewModelBuilder>();
            #endregion
            #region Selenium Services
            services.AddScoped<IPredictiveSeleniumService, PredictiveSeleniumService>();
            services.AddScoped<IPredictiveDashboardService, PredictiveDashboardService>();
            #endregion

            #region pdf/save services
            // Load native library for wkhtmltox
            var nativePath = Path.Combine(env.ContentRootPath, "NativeBinaries", "libwkhtmltox.dll");
            new CustomAssemblyLoadContext().LoadUnmanagedLibrary(nativePath);

            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
            services.AddScoped<IViewRenderService, ViewRenderService>();
            services.AddSingleton<IScenarioMemoryCache, ScenarioMemoryCache>();

            #endregion

            // Register the ErrorLogService
            services.AddScoped<IErrorLogService, ErrorLogService>();
            services.AddScoped<Infrastructure.ApiDiagnostics.IApiDiagnosticsSender, Infrastructure.ApiDiagnostics.ApiDiagnosticsSender>();
            services.AddSingleton<Infrastructure.Services.IInfrastructureErrorLogService, Infrastructure.Services.SqlErrorLogService>();
            services.AddSingleton<Infrastructure.ApiDiagnostics.IFormDataContext, CapitalRequestAutomatedTesting.UI.Services.FormDataContext>();

            return services;
        }
    }

}
