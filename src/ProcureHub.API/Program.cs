using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using Hangfire;
using MediatR;
using ProcureHub.API.Extensions;
using ProcureHub.API.Middleware;
using ProcureHub.Modules.Notifications.Infrastructure.Hubs;
using ProcureHub.API.Services;
using ProcureHub.Modules.Analytics;
using ProcureHub.Modules.ApprovalEngine;
using ProcureHub.Modules.Audit;
using ProcureHub.Modules.DocumentManagement;
using ProcureHub.Modules.MasterData;
using ProcureHub.Modules.Notifications;
using ProcureHub.Modules.Procurement;
using ProcureHub.Modules.VendorManagement;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.Behaviors;
using ProcureHub.SharedKernel.Database;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}")
        .WriteTo.File("logs/procurehub-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30));

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddDatabase(builder.Configuration);
    builder.Services.AddKeycloakAuthentication(builder.Configuration);
    builder.Services.AddApplicationAuthorization();
    builder.Services.AddApplicationRateLimiting(builder.Configuration);
    builder.Services.AddApplicationCaching();

    if (!builder.Environment.IsProduction())
        builder.Services.AddSwaggerDocumentation();

    builder.Services.AddMediatR(
        typeof(Program).Assembly,
        typeof(MasterDataModule).Assembly,
        typeof(VendorManagementModule).Assembly,
        typeof(ProcurementModule).Assembly,
        typeof(ApprovalEngineModule).Assembly,
        typeof(DocumentManagementModule).Assembly,
        typeof(NotificationsModule).Assembly,
        typeof(AnalyticsModule).Assembly,
        typeof(AuditModule).Assembly);

    // Pipeline order matters — logging first, then validation, then transaction
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

    builder.Services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

    builder.Services.AddMasterDataServices();
    builder.Services.AddVendorManagementServices();
    builder.Services.AddProcurementServices();
    builder.Services.AddApprovalEngineServices();
    builder.Services.AddDocumentManagementServices();
    builder.Services.AddNotificationsModule(builder.Configuration);
    // IStorageService (SharedKernel) is registered by AddVendorManagementServices via the global alias

    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

    if (builder.Environment.IsDevelopment())
        builder.Services.AddHostedService<ViteDevServerService>();

    builder.Services.AddSignalR();
    builder.Services.AddApplicationHangfire(builder.Configuration);

    builder.Services.AddControllers()
        .AddJsonOptions(opts =>
        {
            opts.JsonSerializerOptions.PropertyNamingPolicy        = JsonNamingPolicy.CamelCase;
            opts.JsonSerializerOptions.DefaultIgnoreCondition      = JsonIgnoreCondition.WhenWritingNull;
            opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

    builder.Services.AddCors(opts =>
        opts.AddPolicy("FrontendPolicy", policy =>
        {
            var origins = builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? [];
            policy.WithOrigins(origins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Required for SignalR
        }));

    builder.Services.AddApplicationHealthChecks(builder.Configuration);

    var app = builder.Build();

    // Middleware pipeline — order is critical
    app.UseCorrelationId();
    app.UseSerilogRequestLogging();
    app.UseExceptionHandling();
    app.UseSecurityHeaders();
    app.UseHttpsRedirection();
    app.UseCors("FrontendPolicy");
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseUserSync();
    app.UseAuthorization();
    app.UseDefaultFiles();
    app.UseStaticFiles();

    app.UseSwaggerDocumentation(app.Environment);

    app.UseHangfireDashboard(
        builder.Configuration["Hangfire:DashboardPath"] ?? "/hangfire",
        new DashboardOptions
        {
            Authorization = [new HangfireAuthorizationFilter()],
        });

    app.MapApplicationHealthChecks();
    app.MapControllers();
    app.MapHub<NotificationHub>("/hubs/notifications");
    app.MapFallbackToFile("index.html");

    await app.ApplyMigrationsAsync();
    await app.SeedMasterDataAsync();
    await app.RegisterHangfireJobsAsync();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

public partial class Program { }
