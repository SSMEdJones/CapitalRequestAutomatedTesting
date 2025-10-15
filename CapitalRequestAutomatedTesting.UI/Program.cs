using CapitalRequestAutomatedTesting.UI;
using CapitalRequestAutomatedTesting.UI.Hubs;
using CapitalRequestAutomatedTesting.UI.Services;
using Infrastructure.Middleware;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.StaticFiles;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
using System.Diagnostics;
using System.Reflection;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;


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
    var builder = WebApplication.CreateBuilder(args);

    // Clear default providers and use NLog


    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(LogLevel.Trace);
    builder.Logging.AddNLog();
    builder.Host.UseNLog();

    //Persist DataProtection keys to a location outside the temp folder for load-balanced scenarios
    //var appName = Assembly.GetEntryAssembly()?.GetName().Name;

    //builder.Services.AddDataProtection()
    //    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Path.GetTempPath(), $"{appName}-Keys")))
    //    .SetApplicationName(appName);


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


    //var configuration = new ConfigurationBuilder()
    // .SetBasePath(Directory.GetCurrentDirectory())
    // .AddJsonFile("appsettings.json")
    // .Build();

    var configuration = builder.Configuration;

    builder.Services.AddApplicationServices(configuration, builder.Environment);

    var sqlEnv = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEB_SQL_ENV", EnvironmentVariableTarget.Machine))
        ? "DEV"
        : Environment.GetEnvironmentVariable("WEB_SQL_ENV", EnvironmentVariableTarget.Machine);

    builder.Configuration.GetConnectionString($"CapitalRequest_{sqlEnv}");
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddHttpClient();
    builder.Services.AddHostedService<ScenarioHeartbeatService>();

    //builder.Services.AddSignalR();

    builder.Services.AddSignalR(options =>
    {
        options.KeepAliveInterval = TimeSpan.FromSeconds(15); // default is 15s
        options.ClientTimeoutInterval = TimeSpan.FromSeconds(30); // default is 30s
    });


    builder.Logging.AddConsole();
    builder.Logging.AddDebug();
    var app = builder.Build();

    ////TODO Uncomment when debugging complete

    app.UseMiddleware<GlobalExceptionMiddleware>();
    //app.UseStatusCodePagesWithReExecute("/Error/Error/{0}");


    ////TODO Comment when debugging complete
    //if (app.Environment.IsDevelopment())
    //{
    //    app.UseDeveloperExceptionPage();
    //}
    //else
    //{
    //    app.UseExceptionHandler("/Error");
    //    app.UseStatusCodePagesWithReExecute("/Error/{0}");
    //    app.UseHsts();
    //}
    ///


    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseStaticFiles(new StaticFileOptions
    {
        ContentTypeProvider = new FileExtensionContentTypeProvider
        {
            Mappings = { [".js"] = "application/javascript" }
        }
    });

    var owasp = builder.Configuration.GetSection("OWASP");
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

    app.MapHub<ScenarioProgressHub>("/scenarioProgressHub");

    app.Use(async (context, next) =>
    {
        Console.WriteLine($"Request started: {context.Connection.Id}");
        await next.Invoke();
        Console.WriteLine($"Request ended: {context.Connection.Id}");
    });

    app.Use(async (context, next) =>
    {
        try
        {
            await next();
        }
        catch (OperationCanceledException)
        {
            // Quietly swallow or log as info
            Console.WriteLine("Request was canceled by client.");
        }
    });



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
        Debug.WriteLine($"Assembly: {asm.FullName}");

        if (!asm.IsDynamic)
        {
            Debug.WriteLine($"Location: {asm.Location}");
        }
        else
        {
            Debug.WriteLine("Location: (dynamic assembly, no physical path)");
        }
    }


    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Application stopped due to exception at startup. This is a startup exception, not a request exception.");
    throw; // Re-throw to let the hosting layer handle it
}
finally
{
    LogManager.Shutdown(); // Flush and close log files
}

