using System.Security.Claims;

namespace CapitalRequestAutomatedTesting.UI.Services
{

    public interface IUserContextService
    {
        string UserId { get; }
        string Email { get; }
        string Domain { get; }
        string FullIdentity { get; }
        string FirstName { get; }
        string LastName { get; }

        ClaimsPrincipal User { get; }
    }

    public class UserContextService : IUserContextService
    {

        public string UserId { get; }
        public string Email { get; }

        public string Domain { get; }
        public string FullIdentity { get; }
        public string FirstName { get; }
        public string LastName { get; }

        public ClaimsPrincipal User { get; }

        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            var user = httpContextAccessor.HttpContext?.User;
            User = user ?? new ClaimsPrincipal();
            Email = user?.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
            FullIdentity = user?.Identity?.Name ?? string.Empty;

            FirstName = user?.FindFirst(ClaimTypes.GivenName)?.Value ?? string.Empty;
            LastName = user?.FindFirst(ClaimTypes.Surname)?.Value ?? string.Empty;

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
