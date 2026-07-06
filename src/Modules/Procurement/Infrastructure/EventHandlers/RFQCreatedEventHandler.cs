using MediatR;
using Microsoft.Extensions.Logging;
using ProcureHub.SharedKernel.Notifications;
using ProcureHub.Modules.Procurement.Domain.Events;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.Modules.VendorManagement.Application.Queries.GetVendorById;
using ProcureHub.SharedKernel.Abstractions;

namespace ProcureHub.Modules.Procurement.Infrastructure.EventHandlers;

public class RFQCreatedEventHandler : INotificationHandler<RFQCreatedEvent>
{
    private readonly IMediator                       _mediator;
    private readonly IRFQRepository                  _rfqRepo;
    private readonly IEmailService                   _email;
    private readonly ILogger<RFQCreatedEventHandler> _logger;

    public RFQCreatedEventHandler(
        IMediator                       mediator,
        IRFQRepository                  rfqRepo,
        IEmailService                   email,
        ILogger<RFQCreatedEventHandler> logger)
    {
        _mediator = mediator;
        _rfqRepo  = rfqRepo;
        _email    = email;
        _logger   = logger;
    }

    public async Task Handle(RFQCreatedEvent notification, CancellationToken ct)
    {
        _logger.LogInformation(
            "RFQ opened — RFQId: {RFQId}, RFQNumber: {RFQNumber}, VendorsInvited: {Count}",
            notification.RFQId, notification.RFQNumber, notification.InvitedVendorIds.Count);

        var rfq = await _rfqRepo.GetByIdWithDetailsAsync(notification.RFQId, ct);
        if (rfq is null) return;

        foreach (var vendorId in notification.InvitedVendorIds)
        {
            try
            {
                var vendor = await _mediator.Send(new GetVendorByIdQuery(vendorId), ct);
                var email  = vendor.Contacts.FirstOrDefault(c => c.IsPrimary)?.Email
                          ?? vendor.Contacts.FirstOrDefault()?.Email;

                if (email is null) continue;

                var html = EmailTemplates.RFQInvitation(
                    vendor.LegalName, rfq.RFQNumber, rfq.Title, rfq.BidDeadline);

                await _email.SendAsync(email, $"RFQ Invitation — {rfq.RFQNumber}", html, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send RFQ invitation email to vendor {VendorId}", vendorId);
            }
        }
    }
}
