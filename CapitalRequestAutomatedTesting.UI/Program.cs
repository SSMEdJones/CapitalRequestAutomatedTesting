using CapitalRequest.API.DataAccess.ConfigurationSettings;
using CapitalRequestAutomatedTesting.UI;
using Microsoft.AspNetCore.Authentication.Negotiate;
using SSMAuthenticationCore;
using SSMWorkflow.API.DataAccess.ConfiguratonSettings;
using System.Diagnostics;

//TODO Remove when debugging complete
AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
{
    Debug.WriteLine($"Unhandled Exception: {args.ExceptionObject}");
};

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDistributedMemoryCache(); // Required for session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20); // Set timeout as needed
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews()
                .AddSessionStateTempDataProvider(); // Ensures TempData uses session instead of cookies


builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
   .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy.
    options.FallbackPolicy = options.DefaultPolicy;
});


var configuration = new ConfigurationBuilder()
 .SetBasePath(Directory.GetCurrentDirectory())
 .AddJsonFile("appsettings.json")
 .Build();

builder.Services.AddApplicationServices(configuration);

var env = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEB_SQL_ENV", EnvironmentVariableTarget.Machine))
    ? "DEV"
    : Environment.GetEnvironmentVariable("WEB_SQL_ENV", EnvironmentVariableTarget.Machine);

builder.Configuration.GetConnectionString($"CapitalRequest_{env}");

ConfigurationSettings _configuration = new ConfigurationSettings();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

var owasp = builder.Configuration.GetSection("OWASP");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Content-Security-Policy", owasp.GetValue<string>("Content-Security-Policy"));
    context.Response.Headers.Add("Referrer-Policy", owasp.GetValue<string>("Referrer-Policy"));
    context.Response.Headers.Add("Feature-Policy", owasp.GetValue<string>("Feature-Policy"));
    context.Response.Headers.Add("X-Frame-Options", owasp.GetValue<string>("X-Frame-Options"));
    context.Response.Headers.Add("X-XSS-Protection", owasp.GetValue<string>("X-XSS-Protection"));
    context.Response.Headers.Add("X-Content-Type-Options", owasp.GetValue<string>("X-Content-Type-Options"));
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
