using Flurl;
using NLog;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.ApiDiagnostics
{
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