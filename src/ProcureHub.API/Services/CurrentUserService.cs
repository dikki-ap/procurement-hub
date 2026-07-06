using System.Security.Claims;
using ProcureHub.SharedKernel.Abstractions;

namespace ProcureHub.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal? User  => _httpContextAccessor.HttpContext?.User;
    private HttpContext? Context   => _httpContextAccessor.HttpContext;

    public bool IsAuthenticated    => User?.Identity?.IsAuthenticated ?? false;

    public string? KeycloakId
        => User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User?.FindFirstValue("sub");

    public string? Email
        => User?.FindFirstValue(ClaimTypes.Email)
        ?? User?.FindFirstValue("email");

    public string? FullName
        => User?.FindFirstValue("name")
        ?? User?.FindFirstValue(ClaimTypes.Name);

    public string? IpAddress  => Context?.Connection?.RemoteIpAddress?.ToString();
    public string? UserAgent  => Context?.Request.Headers.UserAgent.ToString();

    /// <summary>
    /// Internal DB user ID resolved from the custom JWT claim "procurehub_user_id".
    /// This claim is injected when the user profile is first synced to the local database.
    /// </summary>
    public Guid? UserId
    {
        get
        {
            // Set by UserSyncMiddleware on every authenticated request
            if (Context?.Items["LocalUserId"] is Guid id) return id;
            // Fallback for test auth handler which injects the claim directly
            var claim = User?.FindFirstValue("procurehub_user_id");
            return claim is not null && Guid.TryParse(claim, out var jwtId) ? jwtId : null;
        }
    }

    public IReadOnlyList<string> Roles
    {
        get
        {
            var rolesClaim = User?.FindAll("roles")
                ?? User?.FindAll(ClaimTypes.Role)
                ?? [];
            return rolesClaim.Select(c => c.Value).ToList().AsReadOnly();
        }
    }

    public bool HasRole(string role)      => Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
    public bool HasAnyRole(params string[] roles) => roles.Any(HasRole);
}
