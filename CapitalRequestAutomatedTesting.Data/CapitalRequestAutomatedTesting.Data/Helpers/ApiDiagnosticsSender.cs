using CapitalRequestAutomatedTesting.Data.Services;
using Flurl;
using Microsoft.AspNetCore.Http;
using NLog;
using System.Net.Http.Json;

namespace CapitalRequestAutomatedTesting.Data.Helpers
{
    public interface IApiDiagnosticsSender
    {
        Task<T> SendWithDiagnosticsAsync<T>(HttpMethod method, Url fullUrl, object payload = null, object formContext = null, CancellationToken cancellationToken = default);

        Task<T> GetWithDiagnosticsAsync<T>(Url fullUrl, CancellationToken cancellationToken = default);

        Task<T> PostWithDiagnosticsAsync<T>(Url fullUrl, object payload, CancellationToken cancellationToken = default);

        Task<T> PutWithDiagnosticsAsync<T>(Url fullUrl, object payload, CancellationToken cancellationToken = default);

        Task<T> DeleteWithDiagnosticsAsync<T>(Url fullUrl, CancellationToken cancellationToken = default);
    }

    public class ApiDiagnosticsSender : IApiDiagnosticsSender
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IFormDataContext _formDataContext;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public ApiDiagnosticsSender(IHttpClientFactory httpClientFactory, IFormDataContext formDataContext)
        {
            _httpClientFactory = httpClientFactory;
            _formDataContext = formDataContext;
        }

        public async Task<T> SendWithDiagnosticsAsync<T>(HttpMethod method, Url fullUrl, object payload = null, object formContext = null, CancellationToken cancellationToken = default)
        {

            var client = _httpClientFactory.CreateClient();

            var request = new HttpRequestMessage(method, fullUrl.ToString());

            if (payload != null && method != HttpMethod.Get)
                request.Content = JsonContent.Create(payload);

            try
            {
                var response = await client.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                var formDataXml = _formDataContext.Get();

                var logEvent = new LogEventInfo(LogLevel.Error, _logger.Name, $"{method} to {fullUrl} failed");
                logEvent.Exception = ex;
                logEvent.Properties["FormData"] = formDataXml;
                _logger.Log(logEvent);
                throw;
            }
        }

        // Convenience GET wrapper
        public Task<T> GetWithDiagnosticsAsync<T>(Url fullUrl, CancellationToken cancellationToken = default)
        {
            return SendWithDiagnosticsAsync<T>(HttpMethod.Get, fullUrl, null, null, cancellationToken);
        }

        public Task<T> PostWithDiagnosticsAsync<T>(Url fullUrl, object payload, CancellationToken cancellationToken = default)
        {
            return SendWithDiagnosticsAsync<T>(HttpMethod.Post, fullUrl, payload, null, cancellationToken);
        }

        public Task<T> PutWithDiagnosticsAsync<T>(Url fullUrl, object payload, CancellationToken cancellationToken = default)
        {
            return SendWithDiagnosticsAsync<T>(HttpMethod.Put, fullUrl, payload, null, cancellationToken);
        }

        public Task<T> DeleteWithDiagnosticsAsync<T>(Url fullUrl, CancellationToken cancellationToken = default)
        {
            return SendWithDiagnosticsAsync<T>(HttpMethod.Delete, fullUrl, null, null, cancellationToken);
        }

    }

}
