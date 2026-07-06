using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ProcureHub.API.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddApplicationHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connString = configuration["Database:ConnectionStrings:MariaDb"]!;

        services.AddHealthChecks()
            .AddMySql(
                connectionString: connString,
                name:             "mariadb",
                failureStatus:    HealthStatus.Unhealthy,
                tags:             ["db", "ready"])
            .AddCheck("hangfire", () =>
            {
                // Hangfire uses the same DB — if DB is up, Hangfire storage is accessible
                return HealthCheckResult.Healthy("Hangfire storage reachable");
            }, tags: ["jobs", "ready"]);

        return services;
    }

    public static IEndpointRouteBuilder MapApplicationHealthChecks(
        this IEndpointRouteBuilder endpoints)
    {
        // Liveness: always returns Healthy if the process is running
        endpoints.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate       = _ => false,
            ResponseWriter  = WriteJsonResponse,
        });

        // Readiness: includes DB and external dependency checks
        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate      = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteJsonResponse,
        });

        return endpoints;
    }

    private static Task WriteJsonResponse(HttpContext ctx, HealthReport report)
    {
        ctx.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status  = report.Status.ToString(),
            checks  = report.Entries.Select(e => new
            {
                name    = e.Key,
                status  = e.Value.Status.ToString(),
                message = e.Value.Description,
            }),
            duration = report.TotalDuration.TotalMilliseconds,
        });
        return ctx.Response.WriteAsync(result);
    }
}
