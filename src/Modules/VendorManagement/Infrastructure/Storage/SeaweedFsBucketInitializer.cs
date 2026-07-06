using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ProcureHub.Modules.VendorManagement.Infrastructure.Storage;

public sealed class SeaweedFsBucketInitializer : IHostedService
{
    private readonly IServiceScopeFactory            _scopeFactory;
    private readonly ILogger<SeaweedFsBucketInitializer> _logger;

    private static readonly string[] RequiredBuckets = ["vendor-documents"];

    public SeaweedFsBucketInitializer(
        IServiceScopeFactory scopeFactory,
        ILogger<SeaweedFsBucketInitializer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        await using var scope   = _scopeFactory.CreateAsyncScope();
        var             storage = scope.ServiceProvider.GetRequiredService<IStorageService>();

        foreach (var bucket in RequiredBuckets)
        {
            try
            {
                await storage.EnsureBucketExistsAsync(bucket, ct);
                _logger.LogInformation("SeaweedFS bucket ensured: {Bucket}", bucket);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not ensure SeaweedFS bucket '{Bucket}' — uploads will fail until resolved", bucket);
            }
        }
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
