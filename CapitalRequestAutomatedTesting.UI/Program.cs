using AutoMapper;
using CapitalRequestAutomatedTesting.UI.Services;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//var mapperConfig = new MapperConfiguration(mc =>
//{
//    mc.AddProfile(new CapitalRequestProfile());
//});

//IMapper mapper = mapperConfig.CreateMapper();

//builder.Services.AddSingleton<WorkflowControllerService>();

//ConfigurationSettings _configuration = new ConfigurationSettings();

//builder.Services.PostConfigureAll<SSMWorkFlowSettings>(options =>
//{
//    options.BaseApiUrl = _configuration.GetAppKeyValueByKey("CapitalRequest", "BaseApiUrl").LookupValue.ToString();
//    options.ProjectReviewLink = _configuration.GetAppKeyValueByKey("CapitalRequest", "ProjectReviewLink").LookupValue.ToString();
//});


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
