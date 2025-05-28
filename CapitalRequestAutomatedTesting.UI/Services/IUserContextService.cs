using System.Security.Claims;

namespace CapitalRequestAutomatedTesting.UI.Services
{

    public interface IUserContextService
    {
        string UserId { get; }
        string Domain { get; }
        string FullIdentity { get; }
        ClaimsPrincipal User { get; }
    }

    public class UserContextService : IUserContextService
    {

        public string UserId { get; }
        public string Domain { get; }
        public string FullIdentity { get; }
        public ClaimsPrincipal User { get; }

        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            var user = httpContextAccessor.HttpContext?.User;
            User = user ?? new ClaimsPrincipal();

            FullIdentity = user?.Identity?.Name ?? string.Empty;

            if (!string.IsNullOrEmpty(FullIdentity) && FullIdentity.Contains("\\"))
            {
                var parts = FullIdentity.Split('\\');
                Domain = parts[0];
                UserId = parts.Length > 1 ? parts[1] : string.Empty;
            }
            else
            {
                Domain = string.Empty;
                UserId = FullIdentity;
            }
        }

    }
}
