namespace ProcureHub.SharedKernel.Abstractions;

public interface ICurrentUserService
{
    /// <summary>Internal database user UUID (not the Keycloak UUID).</summary>
    Guid? UserId { get; }

    /// <summary>Keycloak subject claim (sub).</summary>
    string? KeycloakId { get; }

    string? Email { get; }
    string? FullName { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsAuthenticated { get; }

    bool HasRole(string role);
    bool HasAnyRole(params string[] roles);
}
