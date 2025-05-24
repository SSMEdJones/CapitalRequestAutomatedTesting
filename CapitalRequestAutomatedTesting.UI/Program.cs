using AutoMapper;
using CapitalRequestAutomatedTesting.UI.Services;
using SSMAuthenticationCore;
using SSMWorkflow.API.DataAccess.AutoMapper.MappingProfile;
using SSMWorkflow.API.DataAccess.ConfiguratonSettings;
using SSMWorkflow.API.DataAccess.Services.Api;
using SSMWorkflow.API.DataAccess.Services;
using CapitalRequestAutomatedTesting.Data;
using System.Diagnostics;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new WorkflowProfile());
});
IMapper mapper = mapperConfig.CreateMapper();

//builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddSingleton(mapper);

#region Workflow API
builder.Services.AddScoped<WorkflowControllerService>();
builder.Services.AddScoped<ISSMWorkflowServices, SSMWorkflowServices>();
builder.Services.AddScoped<IDashboards, Dashboards>();
builder.Services.AddScoped<ISSMNotification, SSMNotification>();
builder.Services.AddScoped<ISSMWorkFlow, SSMWorkFlow>();
builder.Services.AddScoped<ISSMWorkFlowInstance, SSMWorkFlowInstance>();
builder.Services.AddScoped<ISSMWorkFlowInstanceActionHistory, SSMWorkFlowInstanceActionHistory>();
builder.Services.AddScoped<ISSMWorkFlowStakeholder, SSMWorkFlowStakeholder>();
builder.Services.AddScoped<ISSMWorkFlowStep, SSMWorkFlowStep>();
builder.Services.AddScoped<ISSMWorkFlowStepOption, SSMWorkFlowStepOption>();
builder.Services.AddScoped<ISSMWorkFlowStepResponder, SSMWorkFlowStepResponder>();
builder.Services.AddScoped<IWorkflowServices, WorkflowServices>();
#endregion

var env = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEB_SQL_ENV", EnvironmentVariableTarget.Machine))
    ? "DEV"
    : Environment.GetEnvironmentVariable("WEB_SQL_ENV", EnvironmentVariableTarget.Machine);

builder.Configuration.GetConnectionString($"CapitalRequest_{env}");

ConfigurationSettings _configuration = new ConfigurationSettings();

var baseUrl = string.Empty;
//baseUrl = _configuration.GetAppKeyValueByKey("CapitalRequest", "BaseApiUrl").LookupValue.ToString();

builder.Services.PostConfigureAll<SSMWorkFlowSettings>(options =>
{
    options.BaseApiUrl = _configuration.GetAppKeyValueByKey("CapitalRequest", "BaseApiUrl").LookupValue.ToString();
    //baseUrl = options.BaseApiUrl;
});

//Debug.WriteLine($"BaseApiUrl configured as: {baseUrl}");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
