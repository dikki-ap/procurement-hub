using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ProcureHub.IntegrationTests.Infrastructure;

public class TestAuthOptions : AuthenticationSchemeOptions { }

public class TestAuthHandler : AuthenticationHandler<TestAuthOptions>
{
    public const string SchemeName = "TestAuth";

    public TestAuthHandler(
        IOptionsMonitor<TestAuthOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var request = Request;

        var role   = request.Headers["X-Test-Role"].FirstOrDefault();
        var userId = request.Headers["X-Test-UserId"].FirstOrDefault();
        var email  = request.Headers["X-Test-Email"].FirstOrDefault() ?? "test@test.com";
        var name   = request.Headers["X-Test-Name"].FirstOrDefault() ?? "Test User";

        if (string.IsNullOrEmpty(role))
            return Task.FromResult(AuthenticateResult.NoResult());

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name,  name),
            new(ClaimTypes.Email, email),
            new("name",           name),
            new("email",          email),
            new(ClaimTypes.Role,  role),
            new("roles",          role),
        };

        if (!string.IsNullOrEmpty(userId))
        {
            claims.Add(new Claim("procurehub_user_id", userId));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            claims.Add(new Claim("sub", userId));
        }

        var identity  = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket    = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
