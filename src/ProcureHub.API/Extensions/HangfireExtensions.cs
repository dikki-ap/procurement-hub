using System.Transactions;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MySql;
using ProcureHub.Modules.ApprovalEngine.Infrastructure.Jobs;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.Modules.MasterData.Infrastructure.Jobs;
using ProcureHub.Modules.Procurement.Infrastructure.Jobs;
using ProcureHub.Modules.VendorManagement.Infrastructure.Jobs;

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

    public static async Task RegisterHangfireJobsAsync(this WebApplication app)
    {
        RecurringJob.AddOrUpdate<DocumentExpiryCheckJob>(
            "document-expiry-check",
            job => job.ExecuteAsync(CancellationToken.None),
            "0 0 * * *",
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

        RecurringJob.AddOrUpdate<CapabilityExpiryCheckJob>(
            "capability-expiry-check",
            job => job.ExecuteAsync(CancellationToken.None),
            "5 0 * * *",
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

        RecurringJob.AddOrUpdate<BidDeadlineReminderJob>(
            "bid-deadline-reminder",
            job => job.ExecuteAsync(CancellationToken.None),
            Cron.Hourly(),
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

        RecurringJob.AddOrUpdate<ContractExpiryReminderJob>(
            "contract-expiry-reminder",
            job => job.ExecuteAsync(CancellationToken.None),
            "10 0 * * *",
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

        RecurringJob.AddOrUpdate<ApprovalEscalationJob>(
            "approval-escalation",
            job => job.ExecuteAsync(CancellationToken.None),
            "0 */2 * * *",
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

        RecurringJob.AddOrUpdate<POAcknowledgementReminderJob>(
            "po-acknowledgement-reminder",
            job => job.ExecuteAsync(CancellationToken.None),
            "30 7 * * *",
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

        // Register daily exchange rate sync only if auto-sync is enabled in DB settings
        using var scope     = app.Services.CreateScope();
        var       configRepo = scope.ServiceProvider.GetRequiredService<IExchangeRateConfigRepository>();
        var       config     = await configRepo.GetAsync();

        if (config.AutoSync)
            RecurringJob.AddOrUpdate<ExchangeRateJob>(
                "exchange-rate-sync",
                job => job.ExecuteAsync(CancellationToken.None),
                "5 9 * * *",
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
    }
}

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var env = httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
        if (env.IsDevelopment()) return true;
        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole("super_admin");
    }
}
