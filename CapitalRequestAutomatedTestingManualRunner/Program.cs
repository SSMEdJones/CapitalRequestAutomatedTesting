using System;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using AutoMapper;
using CapitalRequest.API.Models;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.Services;
using SSMWorkflow.API.DataAccess.AutoMapper.MappingProfile;
using SSMWorkflow.API.DataAccess.Services.Api;
using CapitalRequest.API.DataAccess.Services.Api;
using SSMWorkflow.API.DataAccess.Services;
using CapitalRequest.API.DataAccess.ConfigurationSettings;
using Microsoft.Extensions.Configuration;
using SSMWorkflow.API.DataAccess.ConfiguratonSettings;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using static Dapper.SqlMapper;
using dto = CapitalRequest.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;


class Program
{
    static void Main(string[] args)
    {
        var services = new ServiceCollection();

        // Simulate a user context
        var context = new DefaultHttpContext();
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, "edward.jones@ssmhealth.com"),
            new Claim(ClaimTypes.Name, "DOMAIN\\Jones, Edward"),
            new Claim(ClaimTypes.GivenName, "Edward"),
            new Claim(ClaimTypes.Surname, "Jones")
        };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "ejones08"));

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
        Debug.WriteLine($"📁 Looking for config at: {filePath}");
        Debug.WriteLine($"📄 File exists: {File.Exists(filePath)}");

        var configuration = new ConfigurationBuilder()
         .SetBasePath(Directory.GetCurrentDirectory())
         .AddJsonFile("appsettings.json", optional: false)
         .Build();

        services.Configure<SSMWorkFlowSettings>(configuration.GetSection("ssmWorkFlowAPISettings"));
        services.Configure<CapitalRequestSettings>(configuration.GetSection("capitalRequestAPISettings"));

        services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor { HttpContext = context });

        // Register Workflow API services
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

        // Register CapitalRequest API services
        services.AddScoped<IApplicationUsers, ApplicationUsers>();
        services.AddScoped<IAssets, Assets>();
        services.AddScoped<IAttachments, Attachments>();
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

        // Register AutoMapper
        services.AddAutoMapper(typeof(WorkflowProfile));

        // Register your service
        services.AddScoped<PredictiveRequestedInfoService>();


        var testSettings = new SSMWorkFlowSettings();
        configuration.GetSection("ssmWorkFlowAPISettings").Bind(testSettings);
        Debug.WriteLine($"🔍 Direct bind test: {testSettings.BaseApiUrl}");


        var provider = services.BuildServiceProvider();

        var service = provider.GetRequiredService<PredictiveRequestedInfoService>();
        var workflowSettings = provider.GetRequiredService<IOptions<SSMWorkFlowSettings>>().Value;
        var captitalRequestSettings = provider.GetRequiredService<IOptions<CapitalRequestSettings>>().Value;

        Debug.WriteLine($"✅ Loaded WorkflowApiUrl: {workflowSettings.BaseApiUrl}");
        Debug.WriteLine($"✅ Loaded CapitalRequestApiUrl: {captitalRequestSettings.BaseApiUrl}");


        var capitalRequestService = provider.GetRequiredService<ICapitalRequestServices>();

        int proposalId = 2884;
        var proposal =  capitalRequestService.GetProposal(proposalId).Result;
        proposal.ReviewerGroupId = 2;
        proposal.RequestedInfo.ReviewerGroupId = 3;
        if (proposal != null)
        {
            Console.WriteLine($"✅ Proposal Retrieved: ID = {proposal.Id}, Author = {proposal.Author}, WorkflowId = {proposal.WorkflowId}");
        }
        else
        {
            Console.WriteLine("⚠️ Proposal not found.");
        }

        var result = service.Generate(proposal);
        Debug.WriteLine($"RequestedInfo: {System.Text.Json.JsonSerializer.Serialize(result)}");
    }

    
}
