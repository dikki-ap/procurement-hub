using MediatR;
using Microsoft.Extensions.Logging;
using ProcureHub.Modules.VendorManagement.Domain.Events;
using ProcureHub.Modules.VendorManagement.Domain.Repositories;
using ProcureHub.SharedKernel.Notifications;
using ProcureHub.SharedKernel.Abstractions;

namespace ProcureHub.Modules.VendorManagement.Infrastructure.EventHandlers;

public class VendorApprovedEventHandler : INotificationHandler<VendorApprovedEvent>
{
    private readonly IVendorRepository               _vendorRepo;
    private readonly IEmailService                   _email;
    private readonly ILogger<VendorApprovedEventHandler> _logger;

    public VendorApprovedEventHandler(
        IVendorRepository               vendorRepo,
        IEmailService                   email,
        ILogger<VendorApprovedEventHandler> logger)
    {
        _vendorRepo = vendorRepo;
        _email      = email;
        _logger     = logger;
    }

    public async Task Handle(VendorApprovedEvent notification, CancellationToken ct)
    {
        _logger.LogInformation(
            "Vendor approved — VendorId: {VendorId}, LegalName: {LegalName}",
            notification.VendorId, notification.LegalName);

        var vendor = await _vendorRepo.GetByIdWithDetailsAsync(notification.VendorId, ct);
        if (vendor is null) return;

        var primaryContact = vendor.Contacts.FirstOrDefault(c => c.IsPrimary) ?? vendor.Contacts.FirstOrDefault();
        if (primaryContact?.Email is null) return;

        var html = EmailTemplates.VendorApproved(notification.LegalName);
        await _email.SendAsync(primaryContact.Email, "Your vendor account has been approved", html, ct);
    }
}
