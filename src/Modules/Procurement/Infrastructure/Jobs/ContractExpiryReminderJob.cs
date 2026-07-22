using Hangfire;
using Microsoft.Extensions.Logging;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.Modules.VendorManagement.Domain.Repositories;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.Notifications;

namespace ProcureHub.Modules.Procurement.Infrastructure.Jobs;

public class ContractExpiryReminderJob
{
    private static readonly int[] ReminderDays = [30, 14, 7];

    private readonly IContractRepository                  _contractRepo;
    private readonly IVendorRepository                    _vendorRepo;
    private readonly IEmailService                        _email;
    private readonly ILogger<ContractExpiryReminderJob>   _logger;

    public ContractExpiryReminderJob(
        IContractRepository                  contractRepo,
        IVendorRepository                    vendorRepo,
        IEmailService                        email,
        ILogger<ContractExpiryReminderJob>   logger)
    {
        _contractRepo = contractRepo;
        _vendorRepo   = vendorRepo;
        _email        = email;
        _logger       = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow.Date;

        // Auto-expire contracts whose EndDate has passed
        var expired = await _contractRepo.GetExpiredActiveAsync(ct);
        foreach (var c in expired)
        {
            c.Expire();
            _contractRepo.Update(c);
            _logger.LogInformation(
                "Auto-expired contract {ContractNumber} (Id: {ContractId})",
                c.ContractNumber, c.Id);
        }
        if (expired.Count > 0)
            await _contractRepo.SaveChangesAsync(ct);

        // Send reminders for contracts expiring in 30 / 14 / 7 days
        foreach (var days in ReminderDays)
        {
            var targetDate = now.AddDays(days);
            var contracts  = await _contractRepo.GetExpiringAsync(targetDate.AddDays(1), ct);
            var onDay      = contracts.Where(c => c.EndDate!.Value.Date == targetDate).ToList();

            foreach (var contract in onDay)
            {
                try
                {
                    var vendor = await _vendorRepo.GetByIdWithDetailsAsync(contract.VendorId, ct);
                    if (vendor is null) continue;

                    var contact = vendor.Contacts.FirstOrDefault(c => c.IsPrimary)
                               ?? vendor.Contacts.FirstOrDefault();
                    if (contact?.Email is null) continue;

                    var html = EmailTemplates.ContractExpiring(
                        vendor.LegalName,
                        contract.ContractNumber,
                        contract.Title,
                        contract.EndDate!.Value,
                        days);

                    await _email.SendAsync(
                        contact.Email,
                        $"Contract Expiry Reminder ({days}d) — {contract.ContractNumber}",
                        html, ct);

                    _logger.LogInformation(
                        "Sent D-{Days} expiry reminder for contract {ContractNumber} to {Email}",
                        days, contract.ContractNumber, contact.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to send expiry reminder for contract {ContractId}", contract.Id);
                }
            }
        }

        _logger.LogInformation(
            "ContractExpiryReminderJob completed. Auto-expired: {ExpiredCount}.", expired.Count);
    }
}
