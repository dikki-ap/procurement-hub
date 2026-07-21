using Hangfire;
using Microsoft.Extensions.Logging;
using ProcureHub.Modules.VendorManagement.Domain.Repositories;

namespace ProcureHub.Modules.VendorManagement.Infrastructure.Jobs;

public class CapabilityExpiryCheckJob
{
    private readonly IVendorCapabilityRepository      _capRepo;
    private readonly ILogger<CapabilityExpiryCheckJob> _logger;

    public CapabilityExpiryCheckJob(
        IVendorCapabilityRepository       capRepo,
        ILogger<CapabilityExpiryCheckJob> logger)
    {
        _capRepo = capRepo;
        _logger  = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        var expired = await _capRepo.GetExpiredAsync(ct);

        foreach (var cap in expired)
        {
            cap.IsExpired = true;
            _logger.LogInformation(
                "Capability expired — CapId: {CapId}, VendorId: {VendorId}, ExpiryDate: {ExpiryDate}",
                cap.Id, cap.VendorId, cap.ExpiryDate);
        }

        if (expired.Count > 0)
            await _capRepo.SaveChangesAsync(ct);

        _logger.LogInformation(
            "CapabilityExpiryCheckJob completed. Marked expired: {Count} capabilities.", expired.Count);
    }
}
