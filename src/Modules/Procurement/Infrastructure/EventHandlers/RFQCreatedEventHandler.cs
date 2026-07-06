using MediatR;
using Microsoft.Extensions.Logging;
using ProcureHub.Modules.Procurement.Domain.Events;

namespace ProcureHub.Modules.Procurement.Infrastructure.EventHandlers;

public class RFQCreatedEventHandler : INotificationHandler<RFQCreatedEvent>
{
    private readonly ILogger<RFQCreatedEventHandler> _logger;

    public RFQCreatedEventHandler(ILogger<RFQCreatedEventHandler> logger)
        => _logger = logger;

    public Task Handle(RFQCreatedEvent notification, CancellationToken ct)
    {
        _logger.LogInformation(
            "RFQ opened — RFQId: {RFQId}, RFQNumber: {RFQNumber}, VendorsInvited: {Count}",
            notification.RFQId,
            notification.RFQNumber,
            notification.InvitedVendorIds.Count);

        // TODO: Send RFQ invitation email to each invited vendor via IEmailService.
        return Task.CompletedTask;
    }
}
