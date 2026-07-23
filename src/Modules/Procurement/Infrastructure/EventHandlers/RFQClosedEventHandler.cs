using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProcureHub.Modules.Procurement.Domain.Events;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.Database;
using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.Procurement.Infrastructure.EventHandlers;

public class RFQClosedEventHandler : INotificationHandler<RFQClosedEvent>
{
    private readonly IRFQRepository                    _rfqRepo;
    private readonly ApplicationDbContext              _db;
    private readonly INotificationService              _notifications;
    private readonly ILogger<RFQClosedEventHandler>    _logger;

    public RFQClosedEventHandler(
        IRFQRepository                 rfqRepo,
        ApplicationDbContext           db,
        INotificationService           notifications,
        ILogger<RFQClosedEventHandler> logger)
    {
        _rfqRepo       = rfqRepo;
        _db            = db;
        _notifications = notifications;
        _logger        = logger;
    }

    public async Task Handle(RFQClosedEvent notification, CancellationToken ct)
    {
        _logger.LogInformation(
            "RFQ closed — RFQId: {RFQId}, RFQNumber: {RFQNumber}",
            notification.RFQId,
            notification.RFQNumber);

        var rfq = await _rfqRepo.GetByIdWithDetailsAsync(notification.RFQId, ct);
        if (rfq is null) return;

        await NotifyPurchasingTeamAsync(rfq.CompanyId, notification.RFQNumber, rfq.Id, ct);
        await NotifyInvitedVendorsAsync(rfq.Vendors.Select(v => v.VendorId).ToList(), notification.RFQNumber, ct);
    }

    private async Task NotifyPurchasingTeamAsync(Guid companyId, string rfqNumber, Guid rfqId, CancellationToken ct)
    {
        var purchasingUsers = await _db.Set<User>()
            .Where(u => u.CompanyId == companyId && u.Role == "purchasing" && u.IsActive)
            .ToListAsync(ct);

        foreach (var user in purchasingUsers)
        {
            try
            {
                await _notifications.SendAsync(
                    user.Id,
                    $"RFQ {rfqNumber} — Bidding Closed",
                    $"Bidding for {rfqNumber} has closed. Proceed to bid evaluation.",
                    link: $"/procurement/rfqs/{rfqId}",
                    ct: ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to notify purchasing user {UserId} for RFQ {RFQNumber}",
                    user.Id, rfqNumber);
            }
        }

        _logger.LogInformation(
            "Notified {Count} purchasing user(s) that RFQ {RFQNumber} is closed.",
            purchasingUsers.Count, rfqNumber);
    }

    private async Task NotifyInvitedVendorsAsync(List<Guid> vendorIds, string rfqNumber, CancellationToken ct)
    {
        if (vendorIds.Count == 0) return;

        var vendorUsers = await _db.Set<VendorUser>()
            .Where(u => vendorIds.Contains(u.VendorId) && u.IsActive && u.Role == "vendor_admin")
            .ToListAsync(ct);

        foreach (var vendorUser in vendorUsers)
        {
            try
            {
                await _notifications.SendToVendorUserAsync(
                    vendorUser.Id,
                    $"Bidding Closed — {rfqNumber}",
                    $"The bidding period for RFQ {rfqNumber} has ended. No further submissions are accepted.",
                    ct: ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to notify vendor user {VendorUserId} for RFQ {RFQNumber}",
                    vendorUser.Id, rfqNumber);
            }
        }

        _logger.LogInformation(
            "Notified {Count} vendor user(s) that RFQ {RFQNumber} is closed.",
            vendorUsers.Count, rfqNumber);
    }
}
