using MediatR;
using Microsoft.Extensions.Logging;
using ProcureHub.Modules.Procurement.Domain.Events;

namespace ProcureHub.Modules.Procurement.Infrastructure.EventHandlers;

public class RFQClosedEventHandler : INotificationHandler<RFQClosedEvent>
{
    private readonly ILogger<RFQClosedEventHandler> _logger;

    public RFQClosedEventHandler(ILogger<RFQClosedEventHandler> logger)
        => _logger = logger;

    public Task Handle(RFQClosedEvent notification, CancellationToken ct)
    {
        _logger.LogInformation(
            "RFQ closed — RFQId: {RFQId}, RFQNumber: {RFQNumber}",
            notification.RFQId,
            notification.RFQNumber);

        // TODO: Notify purchasing team and vendors that bidding has closed.
        return Task.CompletedTask;
    }
}
