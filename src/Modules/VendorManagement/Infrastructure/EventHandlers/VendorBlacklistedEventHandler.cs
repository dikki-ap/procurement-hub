using MediatR;
using Microsoft.Extensions.Logging;
using ProcureHub.SharedKernel.Notifications;
using ProcureHub.Modules.VendorManagement.Domain.Events;
using ProcureHub.Modules.VendorManagement.Domain.Repositories;
using ProcureHub.SharedKernel.Abstractions;

namespace ProcureHub.Modules.VendorManagement.Infrastructure.EventHandlers;

public class VendorBlacklistedEventHandler : INotificationHandler<VendorBlacklistedEvent>
{
    private readonly IVendorRepository                   _vendorRepo;
    private readonly IEmailService                       _email;
    private readonly ILogger<VendorBlacklistedEventHandler> _logger;

    public VendorBlacklistedEventHandler(
        IVendorRepository                   vendorRepo,
        IEmailService                       email,
        ILogger<VendorBlacklistedEventHandler> logger)
    {
        _vendorRepo = vendorRepo;
        _email      = email;
        _logger     = logger;
    }

    public async Task Handle(VendorBlacklistedEvent notification, CancellationToken ct)
    {
        _logger.LogWarning(
            "Vendor blacklisted — VendorId: {VendorId}, Reason: {Reason}",
            notification.VendorId, notification.Reason);

        var vendor = await _vendorRepo.GetByIdWithDetailsAsync(notification.VendorId, ct);
        if (vendor is null) return;

        var primaryContact = vendor.Contacts.FirstOrDefault(c => c.IsPrimary) ?? vendor.Contacts.FirstOrDefault();
        if (primaryContact?.Email is null) return;

        var html = EmailTemplates.VendorBlacklisted(notification.LegalName, notification.Reason ?? "Not specified");
        await _email.SendAsync(primaryContact.Email, "Your vendor account status has been updated", html, ct);
    }
}
