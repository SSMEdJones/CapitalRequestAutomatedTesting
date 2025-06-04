using CapitalRequest.API.DataAccess.AutoMapper.MappingProfile;
using CapitalRequest.API.DataAccess.ConfigurationSettings;
using CapitalRequest.API.DataAccess.Services.Api;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.Services;
using SSMAuthenticationCore;
using SSMWorkflow.API.DataAccess.AutoMapper.MappingProfile;
using SSMWorkflow.API.DataAccess.ConfiguratonSettings;
using SSMWorkflow.API.DataAccess.Services;
using SSMWorkflow.API.DataAccess.Services.Api;
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
                options.BaseApiUrl = customConfig.GetAppKeyValueByKey("CapitalRequest", "BaseApiUrl")?.LookupValue?.ToString();
            });

            services.PostConfigureAll<CapitalRequestSettings>(options =>
            {
                options.BaseApiUrl = customConfig.GetAppKeyValueByKey("CapitalRequest", "CapitalRequestApiUrl")?.LookupValue?.ToString();
            });

            // Register services...


            // Register all required services
            
            services.AddAutoMapper(typeof(WorkflowProfile), typeof(CapitalRequestProfile));

            //services.AddAutoMapper(typeof(WorkflowProfile));
            //services.AddAutoMapper(typeof(CapitalRequestProfile));

            #region Context Accessor
            var context = new DefaultHttpContext();
            context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                    new Claim(ClaimTypes.Email, "edward.jones@ssmhealth.com"),
                    new Claim(ClaimTypes.Name, "DS\\ejones08"),
                    new Claim(ClaimTypes.GivenName, "Edward"),
                    new Claim(ClaimTypes.Surname, "Jones")
            }, "TestAuth"));

            services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor { HttpContext = context });
            #endregion

            #region Workflow API
            services.AddScoped<WorkflowControllerService>();
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


            #endregion

            return services;
        }
    }

}
