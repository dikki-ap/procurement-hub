using MediatR;
using Microsoft.Extensions.Logging;
using ProcureHub.Modules.ApprovalEngine.Domain.Events;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;

namespace ProcureHub.Modules.Procurement.Infrastructure.EventHandlers;

public class RFQApprovalCompletedEventHandler : INotificationHandler<ApprovalCompletedEvent>
{
    private readonly IRFQRepository                              _rfqRepo;
    private readonly ICacheService                              _cache;
    private readonly ILogger<RFQApprovalCompletedEventHandler>  _logger;

    public RFQApprovalCompletedEventHandler(
        IRFQRepository                             rfqRepo,
        ICacheService                              cache,
        ILogger<RFQApprovalCompletedEventHandler>  logger)
    {
        _rfqRepo = rfqRepo;
        _cache   = cache;
        _logger  = logger;
    }

    public async Task Handle(ApprovalCompletedEvent notification, CancellationToken ct)
    {
        if (!string.Equals(notification.ReferenceType, "RFQ", StringComparison.OrdinalIgnoreCase))
            return;

        var rfq = await _rfqRepo.GetByIdWithDetailsAsync(notification.ReferenceId, ct);
        if (rfq is null)
        {
            _logger.LogWarning("RFQ {RFQId} not found when processing approval completion.", notification.ReferenceId);
            return;
        }

        rfq.Open();

        _rfqRepo.Update(rfq);
        await _rfqRepo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.RFQs.Prefix);

        _logger.LogInformation(
            "RFQ {RFQNumber} auto-opened after approval workflow completed (WorkflowId: {WorkflowId}).",
            rfq.RFQNumber, notification.WorkflowId);
    }
}
