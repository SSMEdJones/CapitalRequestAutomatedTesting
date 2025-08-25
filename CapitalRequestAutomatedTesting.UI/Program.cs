using CapitalRequestAutomatedTesting.UI;
using CapitalRequestAutomatedTesting.UI.Helpers;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.StaticFiles;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
using System.Diagnostics;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using NLogLevel = NLog.LogLevel;


AppContext.SetSwitch("Microsoft.Data.SqlClient.DisableSqlConnectionPoolPerformanceCounters", true);
AppContext.SetSwitch("Microsoft.Data.SqlClient.DisablePermissionDemand", true);

//TODO Remove when debugging complete
AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
{
    Debug.WriteLine($"Unhandled Exception: {args.ExceptionObject}");
};


var logger = LogManager.Setup()
    .LoadConfigurationFromFile("NLog.config")
    .GetCurrentClassLogger();
try
{
    //logger.Info("Starting application");
    //logger = NLog.LogManager.GetCurrentClassLogger();
    //logger.Error("Testing SQL logging — this should go to the database.");

    var builder = WebApplication.CreateBuilder(args);

    // Clear default providers and use NLog

    
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(LogLevel.Trace);
    builder.Logging.AddNLog();
    builder.Host.UseNLog();

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

    builder.Services.AddApplicationServices(configuration, builder.Environment);

    var sqlEnv = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEB_SQL_ENV", EnvironmentVariableTarget.Machine))
        ? "DEV"
        : Environment.GetEnvironmentVariable("WEB_SQL_ENV", EnvironmentVariableTarget.Machine);

    builder.Configuration.GetConnectionString($"CapitalRequest_{sqlEnv}");
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddHttpClient();

    var app = builder.Build();

    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            var exceptionFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            var exception = exceptionFeature?.Error;

            var formData = ErrorContextHelper.CaptureFormData(context.Request);
            var formXml = ErrorContextHelper.SerializeFormData(formData);

            var logEvent = new LogEventInfo(NLogLevel.Error, "", exception?.Message ?? "Unhandled exception");
            logEvent.Exception = exception;
            logEvent.Properties["FormData"] = formXml;
            logEvent.Properties["ScenarioId"] = "SCN002"; // example
            logEvent.Properties["RollbackStatus"] = "Failed"; // example

            var logger = LogManager.GetCurrentClassLogger();
            logger.Log(logEvent);

            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("An unexpected error occurred.");
        });
    });


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
    app.UseStaticFiles(new StaticFileOptions
    {
        ContentTypeProvider = new FileExtensionContentTypeProvider
        {
            Mappings = { [".js"] = "application/javascript" }
        }
    });

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


    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
    {
        if (asm.FullName.Contains("System.Data.SqlClient"))
        {
            Debug.WriteLine($"Assembly: {asm.FullName}");
            Debug.WriteLine($"Location: {asm.Location}");
        }
    }

    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
    {
        try
        {
            Debug.WriteLine($"Assembly: {asm.FullName}");
            Debug.WriteLine($"Location: {asm.Location}");
        }
        catch (NotSupportedException)
        {
            Debug.WriteLine($"Assembly: {asm.FullName} (dynamic, no location)");
        }
    }

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Application stopped due to exception");
    throw;
}
finally
{
    LogManager.Shutdown(); // Flush and close log files
}

