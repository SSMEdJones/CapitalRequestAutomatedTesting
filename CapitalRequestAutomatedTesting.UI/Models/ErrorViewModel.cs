using Infrastructure.Middleware;

namespace CapitalRequestAutomatedTesting.UI.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public int StatusCode { get; internal set; }
    public ExceptionDetail ExceptionDetail { get; internal set; }
}
