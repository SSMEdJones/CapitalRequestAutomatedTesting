using CapitalRequest.API.DataAccess.AutoMapper.MappingProfile;
using CapitalRequest.API.DataAccess.ConfigurationSettings;
using CapitalRequest.API.DataAccess.Services.Api;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.Services;
using ScenarioFramework;
using SSMAuthenticationCore;
using SSMWorkflow.API.DataAccess.AutoMapper.MappingProfile;
using SSMWorkflow.API.DataAccess.ConfiguratonSettings;
using SSMWorkflow.API.DataAccess.Services;
using SSMWorkflow.API.DataAccess.Services.Api;
using System.Diagnostics;
using System.Security.Claims;

namespace CapitalRequestAutomatedTesting.UI
{
    public static class ServiceConfigurator
    {

        public static IServiceCollection AddApplicationServices(
         this IServiceCollection services,
         IConfiguration configuration)
        {

            services.Configure<SSMWorkFlowSettings>(configuration.GetSection("ssmWorkFlowAPISettings"));
            services.Configure<CapitalRequestSettings>(configuration.GetSection("capitalRequestAPISettings"));

            var customConfig = new ConfigurationSettings();

            services.PostConfigureAll<SSMWorkFlowSettings>(options =>
            {
                options.BaseApiUrl = customConfig.GetAppKeyValueByKey("CapitalRequest", "SSMWorkflowAPI")?.LookupValue?.ToString();
                options.ProjectReviewLink = customConfig.GetAppKeyValueByKey("CapitalRequest", "ProjectReviewLink")?.LookupValue?.ToString();
            });

            //services.PostConfigureAll<SSMWorkFlowSettings>(options =>
            //{
            //    var baseApiUrl = customConfig.GetAppKeyValueByKey("CapitalRequest", "SSMWorkflowAPI")?.LookupValue?.ToString();
            //    var projectReviewLink = customConfig.GetAppKeyValueByKey("CapitalRequest", "ProjectReviewLink")?.LookupValue?.ToString();

            //    Debug.WriteLine($"🔧 PostConfigureAll setting BaseApiUrl to: {baseApiUrl ?? "NULL"}");
            //    Debug.WriteLine($"🔧 PostConfigureAll setting ProjectReviewLink to: {projectReviewLink ?? "NULL"}");

            //    options.BaseApiUrl = baseApiUrl;
            //    options.ProjectReviewLink = projectReviewLink;
            //});

            services.PostConfigureAll<CapitalRequestSettings>(options =>
            {
                options.BaseApiUrl = customConfig.GetAppKeyValueByKey("CapitalRequest", "CapitalRequestApiUrl")?.LookupValue?.ToString();
            });

            services.AddAutoMapper(typeof(WorkflowProfile), typeof(CapitalRequestProfile));


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
            //services.AddScoped<IPredictiveProposalControllerService, PredictiveProposalControllerService>();


            #endregion

            #region Actual Services
            services.AddScoped<IActualEmailNotificataionService, ActualEmailNotificataionService>();
            #endregion

            #region Scenario Framework
            services.AddScoped<ITestActionService, WorkflowTestActionService>();
            #endregion

            return services;
        }
    }

}
