using MediatR;
using Microsoft.Extensions.Logging;
using ProcureHub.Modules.Procurement.Domain.Events;
using ProcureHub.Modules.VendorManagement.Application.Commands.UpdateVendorScore;

namespace ProcureHub.Modules.Procurement.Infrastructure.EventHandlers;

public class GRNConfirmedEventHandler : INotificationHandler<GRNConfirmedEvent>
{
    private readonly IMediator                      _mediator;
    private readonly ILogger<GRNConfirmedEventHandler> _logger;

    public GRNConfirmedEventHandler(IMediator mediator, ILogger<GRNConfirmedEventHandler> logger)
    {
        _mediator = mediator;
        _logger   = logger;
    }

    public async Task Handle(GRNConfirmedEvent notification, CancellationToken ct)
    {
        _logger.LogInformation(
            "GRN {GRNNumber} confirmed for PO {POId}. Updating vendor score.",
            notification.GRNNumber, notification.POId);

        await _mediator.Send(new UpdateVendorScoreCommand(
            VendorId:         notification.VendorId,
            DeliveredOnTime:  !notification.HasDiscrepancy,
            HasQualityIssues: notification.HasDiscrepancy,
            HasDiscrepancy:   notification.HasDiscrepancy), ct);
    }
}
