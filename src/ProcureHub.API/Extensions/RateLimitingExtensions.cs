using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace ProcureHub.API.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddApplicationRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            // Global limiter — applies to every request
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                context =>
                {
                    var key = context.User.Identity?.IsAuthenticated == true
                        ? context.User.FindFirst("sub")?.Value ?? "anonymous"
                        : context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        $"global:{key}",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit       = 100,
                            Window            = TimeSpan.FromMinutes(1),
                            QueueLimit        = 10,
                        });
                });

            options.AddFixedWindowLimiter("vendor-register", opts =>
            {
                opts.PermitLimit = 5;
                opts.Window      = TimeSpan.FromMinutes(1);
                opts.QueueLimit  = 0;
            });

            options.AddFixedWindowLimiter("file-upload", opts =>
            {
                opts.PermitLimit = 20;
                opts.Window      = TimeSpan.FromMinutes(1);
                opts.QueueLimit  = 5;
            });

            options.AddFixedWindowLimiter("bid-submit", opts =>
            {
                opts.PermitLimit = 10;
                opts.Window      = TimeSpan.FromMinutes(1);
                opts.QueueLimit  = 0;
            });

            options.AddSlidingWindowLimiter("api-standard", opts =>
            {
                opts.PermitLimit       = 60;
                opts.Window            = TimeSpan.FromMinutes(1);
                opts.SegmentsPerWindow = 6;
                opts.QueueLimit        = 10;
            });

            options.OnRejected = async (context, ct) =>
            {
                context.HttpContext.Response.StatusCode  = 429;
                context.HttpContext.Response.ContentType = "application/json";

                context.Lease.TryGetMetadata(
                    MetadataName.RetryAfter, out var retryAfter);

                context.HttpContext.Response.Headers["Retry-After"] =
                    retryAfter.TotalSeconds.ToString();

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    success           = false,
                    message           = "Too many requests. Please slow down.",
                    retryAfterSeconds = (int)retryAfter.TotalSeconds,
                    timestamp         = DateTime.UtcNow
                }, ct);
            };
        });

        return services;
    }
}
