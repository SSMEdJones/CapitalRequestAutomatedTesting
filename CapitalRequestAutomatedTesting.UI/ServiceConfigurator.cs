using CapitalRequest.API.DataAccess.AutoMapper.MappingProfile;
using CapitalRequest.API.DataAccess.ConfigurationSettings;
using CapitalRequest.API.DataAccess.Services.Api;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.Services;
using DinkToPdf.Contracts;
using DinkToPdf;
using ScenarioFramework;
using SSMAuthenticationCore;
using SSMWorkflow.API.DataAccess.AutoMapper.MappingProfile;
using SSMWorkflow.API.DataAccess.ConfiguratonSettings;
using SSMWorkflow.API.DataAccess.Services;
using SSMWorkflow.API.DataAccess.Services.Api;
using CapitalRequestAutomatedTesting.UI.Helpers;
using CapitalRequestAutomatedTesting.UI.AutoMapper.MappingProfile;

namespace CapitalRequestAutomatedTesting.UI
{
    public static class ServiceConfigurator
    {

        public static IServiceCollection AddApplicationServices(this IServiceCollection services,IConfiguration configuration, IWebHostEnvironment env)
        {

            services.Configure<SSMWorkFlowSettings>(configuration.GetSection("ssmWorkFlowAPISettings"));
            services.Configure<CapitalRequestSettings>(configuration.GetSection("capitalRequestAPISettings"));

            var customConfig = new ConfigurationSettings();

            services.PostConfigureAll<SSMWorkFlowSettings>(options =>
            {
                options.BaseApiUrl = customConfig.GetAppKeyValueByKey("CapitalRequest", "SSMWorkflowAPI")?.LookupValue?.ToString();
                options.ProjectReviewLink = customConfig.GetAppKeyValueByKey("CapitalRequest", "ProjectReviewLink")?.LookupValue?.ToString();
            });


            services.PostConfigureAll<CapitalRequestSettings>(options =>
            {
                options.BaseApiUrl = customConfig.GetAppKeyValueByKey("CapitalRequest", "CapitalRequestApiUrl")?.LookupValue?.ToString();
            });

            services.AddAutoMapper(typeof(WorkflowProfile), typeof(CapitalRequestProfile),typeof(AutomatedTestingProfile) );


            #region UI
            services.AddScoped<IWorkflowControllerService, WorkflowControllerService>();
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

            return services;
        }
    }

}
