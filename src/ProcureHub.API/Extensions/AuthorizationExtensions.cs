using System.Security.Claims;
using System.Text.Json;
using Keycloak.AuthServices.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace ProcureHub.API.Extensions;

public static class AuthorizationExtensions
{
    public static class Roles
    {
        public const string SuperAdmin  = "super_admin";
        public const string Requester   = "requester";
        public const string Purchasing  = "purchasing";
        public const string Approver    = "approver";
        public const string Finance     = "finance";
        public const string Management  = "management";
        public const string VendorAdmin = "vendor_admin";
        public const string VendorStaff = "vendor_staff";

        public static readonly string[] AllInternal   = [SuperAdmin, Requester, Purchasing, Approver, Finance, Management];
        public static readonly string[] AllVendor     = [VendorAdmin, VendorStaff];
        public static readonly string[] CanCreatePR   = [SuperAdmin, Requester, Purchasing];
        public static readonly string[] CanManageRFQ  = [SuperAdmin, Purchasing];
        public static readonly string[] CanApprove    = [SuperAdmin, Approver];
        public static readonly string[] CanManageMasterData = [SuperAdmin];
        public static readonly string[] CanViewAnalytics    = [SuperAdmin, Management, Purchasing, Finance];
        public static readonly string[] CanManageFinance    = [SuperAdmin, Finance];
    }

    public static IServiceCollection AddKeycloakAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // The client whose roles are assigned to users (e.g. "procurehub-web").
        // Roles appear in the JWT as resource_access.{clientId}.roles
        var clientId = configuration["Keycloak:Resource"] ?? "procurehub-web";

        services.AddKeycloakWebApiAuthentication(configuration, options =>
        {
            options.RequireHttpsMetadata =
                configuration.GetValue<bool>("Keycloak:RequireHttpsMetadata");
        });

        services.Configure<JwtBearerOptions>(
            JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters.ValidateAudience = false;

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async ctx =>
                    {
                        var identity = ctx.Principal?.Identity as ClaimsIdentity;
                        if (identity is null) return;

                        // Map Keycloak resource_access.{clientId}.roles → ClaimTypes.Role
                        var resourceAccessClaim = ctx.Principal?
                            .FindFirst("resource_access")?.Value;

                        if (!string.IsNullOrEmpty(resourceAccessClaim))
                        {
                            try
                            {
                                using var doc = JsonDocument.Parse(resourceAccessClaim);
                                if (doc.RootElement.TryGetProperty(clientId, out var clientEl) &&
                                    clientEl.TryGetProperty("roles", out var rolesEl))
                                {
                                    foreach (var role in rolesEl.EnumerateArray())
                                    {
                                        var roleValue = role.GetString();
                                        if (!string.IsNullOrEmpty(roleValue))
                                        {
                                            identity.AddClaim(new Claim(ClaimTypes.Role, roleValue));
                                            identity.AddClaim(new Claim("roles", roleValue));
                                        }
                                    }
                                }
                            }
                            catch { /* ignore malformed claim */ }
                        }

                        await Task.CompletedTask;
                    }
                };
            });

        return services;
    }

    public static IServiceCollection AddApplicationAuthorization(
        this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireSuperAdmin",    p => p.RequireRole(Roles.SuperAdmin));
            options.AddPolicy("RequireInternal",      p => p.RequireRole(Roles.AllInternal));
            options.AddPolicy("RequirePurchasing",    p => p.RequireRole(Roles.CanManageRFQ));
            options.AddPolicy("RequireApprover",      p => p.RequireRole(Roles.CanApprove));
            options.AddPolicy("RequireFinance",       p => p.RequireRole(Roles.CanManageFinance));
            options.AddPolicy("RequireManagement",    p => p.RequireRole(Roles.CanViewAnalytics));
            options.AddPolicy("RequireMasterData",    p => p.RequireRole(Roles.CanManageMasterData));
            options.AddPolicy("RequireCreatePR",      p => p.RequireRole(Roles.CanCreatePR));
            options.AddPolicy("RequireVendorUser",    p => p.RequireRole(Roles.AllVendor));
            options.AddPolicy("RequireVendorAdmin",   p => p.RequireRole(Roles.VendorAdmin));
            options.AddPolicy("RequireAuthenticated", p => p.RequireAuthenticatedUser());
        });

        return services;
    }
}
