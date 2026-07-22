using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.SharedKernel.Database;
using ProcureHub.SharedKernel.Domain;
using System.Security.Claims;

namespace ProcureHub.API.Middleware;

public class UserSyncMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly HashSet<string> InternalRoles =
    [
        "super_admin", "purchasing", "approver",
        "finance", "requester", "management"
    ];

    public UserSyncMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext ctx, ApplicationDbContext db)
    {
        if (ctx.User.Identity?.IsAuthenticated == true)
        {
            var keycloakId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? ctx.User.FindFirstValue("sub");

            if (!string.IsNullOrEmpty(keycloakId))
            {
                var role = ctx.User.FindFirstValue("roles")
                        ?? ctx.User.FindFirstValue(ClaimTypes.Role)
                        ?? string.Empty;

                if (InternalRoles.Contains(role))
                {
                    var (internalId, companyId) = await SyncInternalUserAsync(ctx, db, keycloakId, role);
                    if (internalId.HasValue)
                        ctx.Items["LocalUserId"] = internalId.Value;
                    if (companyId.HasValue)
                        ctx.Items["LocalCompanyId"] = companyId.Value;
                }
                else
                {
                    // Vendor users are stored in vendor_users, NOT in users.
                    // LocalUserId must only ever hold a users.id (FK target), so we
                    // keep it null for vendor users to avoid FK constraint violations.
                    var vendorId = await SyncVendorUserAsync(ctx, db, keycloakId);
                    if (vendorId.HasValue)
                        ctx.Items["VendorLocalUserId"] = vendorId.Value;
                }
            }
        }

        await _next(ctx);
    }

    private static async Task<(Guid? UserId, Guid? CompanyId)> SyncInternalUserAsync(
        HttpContext ctx, ApplicationDbContext db, string keycloakId, string role)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.KeycloakId == keycloakId);

        if (user is null)
        {
            var company = await db.Companies.FirstOrDefaultAsync();
            if (company is null) return (null, null);

            user = new User
            {
                KeycloakId = keycloakId,
                Email      = ctx.User.FindFirstValue(ClaimTypes.Email)
                          ?? ctx.User.FindFirstValue("email")
                          ?? string.Empty,
                FullName   = ctx.User.FindFirstValue("name")
                          ?? ctx.User.FindFirstValue(ClaimTypes.Name)
                          ?? string.Empty,
                Role       = role,
                CompanyId  = company.Id,
                IsActive   = true,
            };
            db.Users.Add(user);

            try
            {
                await db.SaveChangesAsync();
            }
            catch
            {
                // Concurrent first login — re-fetch the record created by the other request
                user = await db.Users.FirstOrDefaultAsync(u => u.KeycloakId == keycloakId);
            }
        }

        return (user?.Id, user?.CompanyId);
    }

    private static async Task<Guid?> SyncVendorUserAsync(
        HttpContext ctx, ApplicationDbContext db, string keycloakId)
    {
        var vendorUser = await db.Set<VendorUser>()
            .FirstOrDefaultAsync(u => u.KeycloakId == keycloakId);

        if (vendorUser is not null)
            return vendorUser.Id;

        // First login: auto-link to vendor by matching the primary contact email
        var email = ctx.User.FindFirstValue(ClaimTypes.Email)
                 ?? ctx.User.FindFirstValue("email");
        if (string.IsNullOrEmpty(email)) return null;

        var contact = await db.Set<VendorContact>()
            .FirstOrDefaultAsync(c => c.Email == email);
        if (contact is null) return null;

        var role = ctx.User.FindFirstValue("roles")
                ?? ctx.User.FindFirstValue(ClaimTypes.Role)
                ?? "vendor_admin";

        vendorUser = new VendorUser
        {
            VendorId   = contact.VendorId,
            KeycloakId = keycloakId,
            Email      = email,
            FullName   = ctx.User.FindFirstValue("name")
                      ?? ctx.User.FindFirstValue(ClaimTypes.Name)
                      ?? email,
            Role       = role,
            IsActive   = true,
        };
        db.Set<VendorUser>().Add(vendorUser);

        try
        {
            await db.SaveChangesAsync();
        }
        catch
        {
            vendorUser = await db.Set<VendorUser>()
                .FirstOrDefaultAsync(u => u.KeycloakId == keycloakId);
        }

        return vendorUser?.Id;
    }
}

public static class UserSyncMiddlewareExtensions
{
    public static IApplicationBuilder UseUserSync(this IApplicationBuilder app)
        => app.UseMiddleware<UserSyncMiddleware>();
}
