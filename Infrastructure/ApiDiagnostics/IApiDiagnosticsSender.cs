using Flurl;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.ApiDiagnostics
{
    public interface IApiDiagnosticsSender
    {
        Task<T> SendWithDiagnosticsAsync<T>(HttpMethod method, Url fullUrl, object payload = null, object formContext = null, CancellationToken cancellationToken = default);
        Task<T> GetWithDiagnosticsAsync<T>(Url fullUrl, CancellationToken cancellationToken = default);
        Task<T> PostWithDiagnosticsAsync<T>(Url fullUrl, object payload, CancellationToken cancellationToken = default);
        Task<T> PutWithDiagnosticsAsync<T>(Url fullUrl, object payload, CancellationToken cancellationToken = default);
        Task<T> DeleteWithDiagnosticsAsync<T>(Url fullUrl, CancellationToken cancellationToken = default);
    }
}