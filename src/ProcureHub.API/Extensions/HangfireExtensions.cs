using System.Transactions;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MySql;

namespace ProcureHub.API.Extensions;

public static class HangfireExtensions
{
    public static IServiceCollection AddApplicationHangfire(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration["Database:ConnectionStrings:MariaDb"]!;

        services.AddHangfire(cfg => cfg
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseStorage(new MySqlStorage(connectionString,
                new MySqlStorageOptions
                {
                    TransactionIsolationLevel = IsolationLevel.ReadCommitted,
                    QueuePollInterval         = TimeSpan.FromSeconds(15),
                    PrepareSchemaIfNecessary  = true,
                    DashboardJobListLimit     = 50_000,
                    TransactionTimeout        = TimeSpan.FromMinutes(1),
                })));

        services.AddHangfireServer(opts =>
        {
            opts.WorkerCount = configuration.GetValue<int>("Hangfire:WorkerCount", 5);
            opts.Queues      = ["critical", "default", "low"];
        });

        return services;
    }

    public static void RegisterHangfireJobs(this WebApplication app)
    {
        // Uncomment each job as the corresponding module is built:

        // RecurringJob.AddOrUpdate<DocumentExpiryCheckJob>(
        //     "document-expiry-check",
        //     job => job.ExecuteAsync(CancellationToken.None),
        //     "0 0 * * *",
        //     new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

        // RecurringJob.AddOrUpdate<BidDeadlineReminderJob>(
        //     "bid-deadline-reminder",
        //     job => job.ExecuteAsync(CancellationToken.None),
        //     Cron.Hourly());

        // RecurringJob.AddOrUpdate<ApprovalEscalationJob>(
        //     "approval-escalation",
        //     job => job.ExecuteAsync(CancellationToken.None),
        //     "0 */2 * * *");
    }
}

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole("super_admin");
    }
}
