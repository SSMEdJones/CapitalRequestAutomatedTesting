using System.Net;

namespace CapitalRequestAutomatedTesting.UI.Services
{
    public abstract class BaseHttpService
    {
        protected readonly HttpClient _httpClient;
        protected readonly ILogger _logger;

        protected BaseHttpService(HttpClient httpClient, ILogger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            
            // Configure timeout and connection settings
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        protected async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, int maxRetries = 3)
        {
            Exception lastException = null;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    return await operation();
                }
                catch (HttpRequestException ex) when (attempt < maxRetries)
                {
                    lastException = ex;
                    var delay = TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * 1000); // Exponential backoff
                    _logger.LogWarning("HTTP request failed on attempt {Attempt}/{MaxRetries}. Retrying in {Delay}ms. Error: {Error}", 
                        attempt, maxRetries, delay.TotalMilliseconds, ex.Message);
                    await Task.Delay(delay);
                }
                catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException && attempt < maxRetries)
                {
                    lastException = ex;
                    _logger.LogWarning("HTTP request timed out on attempt {Attempt}/{MaxRetries}. Retrying...", attempt, maxRetries);
                    await Task.Delay(1000 * attempt); // Linear backoff for timeouts
                }
                catch (Exception ex) when (ex.Message.Contains("NetworkStream") || ex.Message.Contains("disposed"))
                {
                    lastException = ex;
                    if (attempt < maxRetries)
                    {
                        _logger.LogWarning("Network connection issue on attempt {Attempt}/{MaxRetries}. Retrying...", attempt, maxRetries);
                        await Task.Delay(2000 * attempt);
                    }
                }
            }

            throw lastException ?? new Exception("Operation failed after maximum retries");
        }

        protected async Task<HttpResponseMessage> SafeHttpGetAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(25)); // Slightly less than client timeout

                var response = await _httpClient.GetAsync(requestUri, cts.Token);
                response.EnsureSuccessStatusCode();
                return response;
            });
        }

        protected async Task<HttpResponseMessage> SafeHttpPostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(25));

                var response = await _httpClient.PostAsync(requestUri, content, cts.Token);
                response.EnsureSuccessStatusCode();
                return response;
            });
        }
    }
}