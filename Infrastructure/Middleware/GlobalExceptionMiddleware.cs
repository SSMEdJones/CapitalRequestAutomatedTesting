using Infrastructure.ApiDiagnostics;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog;

namespace Infrastructure.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IFormDataContext _formDataContext;
        private readonly IInfrastructureErrorLogService _errorLogService;
        private readonly NLog.Logger _nlog;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IFormDataContext formDataContext,
            IInfrastructureErrorLogService errorLogService)
        {
            _next = next;
            _logger = logger;
            _formDataContext = formDataContext;
            _errorLogService = errorLogService;
            _nlog = NLog.LogManager.GetCurrentClassLogger();
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var methodContext = _formDataContext.GetInvocationContext();
            var formData = _formDataContext.Get();

            var methodParameters = string.Empty;
            if (methodContext != null)
            {
                methodParameters = (methodContext.Parameters != null && methodContext.Parameters.Any())
                    ? string.Join(", ", methodContext.Parameters.Select((p, i) => $"Param{i + 1}: {p}"))
                    : "None";             
            }

            var exceptionDetail = new ExceptionDetail
            {
                Logged = DateTime.Now,
                Level = "Error",
                Message = exception?.Message,
                Logger = $"{methodContext?.ServiceName ?? "Unknown"}.{methodContext?.MethodName ?? "Unknown"}",
                ExceptionType = exception?.GetType().FullName,
                ExceptionMessage = exception?.Message,
                StackTrace = $"{exception?.GetType().FullName}\n{exception?.Message}\n{exception?.StackTrace}",
                Url = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}",
                UserName = context.User?.Identity?.Name ?? "anonymous",
                //ScenarioId = methodContext?.ScenarioId,
                //RollbackStatus = methodContext?.RollbackStatus,
                FormData = formData,
                Endpoint = context.Request.Path,
                ResponseBody = null,
                InvokedService = methodContext?.ServiceName ?? string.Empty,
                InvokedMethod = methodContext?.MethodName ?? string.Empty,
                InvokedParameters = methodParameters
            };

            // Insert into DB and grab PK Id
            int errorId = await _errorLogService.CreateErrorLogAsync(exceptionDetail);

            var evt = new NLog.LogEventInfo(NLog.LogLevel.Error, _nlog.Name, exception.Message)
            {
                Exception = exception
            };

            evt.Properties["InvokedService"] = methodContext?.ServiceName ?? "Unknown";
            evt.Properties["InvokedMethod"] = methodContext?.MethodName ?? "Unknown";
            evt.Properties["InvokedParameters"] = methodContext?.Parameters != null
                ? string.Join(", ", methodContext.Parameters)
                : string.Empty;

            _nlog.Log(evt);

            //var logEvent = new LogEventInfo(NLog.LogLevel.Error, GetType().FullName, exception.Message);
            //logEvent.Exception = exception;
            //logEvent.Properties["InvokedService"] = methodContext?.ServiceName ?? "Unknown";
            //logEvent.Properties["InvokedMethod"] = methodContext?.MethodName ?? "Unknown";
            //logEvent.Properties["InvokedParameters"] = methodContext?.Parameters != null
            //    ? string.Join(", ", methodContext.Parameters)
            //    : string.Empty;
            //NLog.LogManager.GetLogger(logEvent.LoggerName).Log(logEvent);


            //logEvent.Exception = exception;

            //// populate normalized fields as event properties
            //logEvent.Properties["InvokedService"] = methodContext?.ServiceName ?? "Unknown";
            //logEvent.Properties["InvokedMethod"] = methodContext?.MethodName ?? "Unknown";
            //logEvent.Properties["InvokedParameters"] = methodContext?.Parameters != null
            //    ? string.Join(", ", methodContext.Parameters)
            //    : string.Empty;

            //_logger.Log(logEvent);

            //// Still log to NLog/file/etc.
            //_logger.LogError(exception,
            //    "Unhandled exception in {ServiceName}.{MethodName}",
            //    $"{methodContext?.ServiceName ?? "Unknown"}",
            //    $"{methodContext?.MethodName ?? "Unknown"}");

            // Detect AJAX
            bool isAjax =
                string.Equals(context.Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase)
                || context.Request.Headers["Accept"].Any(a =>
                    a.Contains("application/json", StringComparison.OrdinalIgnoreCase) ||
                    a.Contains("*/*", StringComparison.OrdinalIgnoreCase)) // treat */* from fetch() as AJAX
                || "fetch".Equals(context.Request.Headers["Sec-Fetch-Mode"], StringComparison.OrdinalIgnoreCase);

            var redirectUrl = $"/Error/Details/{errorId}";

            if (isAjax)
            {
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.Headers["X-Error-Redirect"] = redirectUrl;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new
                {
                    error = "Unhandled exception",
                    errorId,
                    redirectUrl
                }));
            }
            else
            {
                context.Response.Clear();
                context.Response.Redirect(redirectUrl, false);
                await context.Response.CompleteAsync();
            }
        }

    }
}
