using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.Procurement.Infrastructure.Jobs;

public class POAcknowledgementReminderJob
{
    private readonly IPurchaseOrderRepository          _poRepo;
    private readonly ApplicationDbContext              _db;
    private readonly IEmailService                     _email;
    private readonly INotificationService              _notifications;
    private readonly ILogger<POAcknowledgementReminderJob> _logger;

    public POAcknowledgementReminderJob(
        IPurchaseOrderRepository              poRepo,
        ApplicationDbContext                  db,
        IEmailService                         email,
        INotificationService                  notifications,
        ILogger<POAcknowledgementReminderJob> logger)
    {
        _poRepo        = poRepo;
        _db            = db;
        _email         = email;
        _notifications = notifications;
        _logger        = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        var now    = DateTime.UtcNow;
        var orders = await _poRepo.GetIssuedUnacknowledgedAsync(ct);

        foreach (var po in orders)
        {
            var deadline    = po.AcknowledgementDeadline!.Value;
            var hoursLeft   = (deadline - now).TotalHours;
            var isOverdue   = hoursLeft <= 0;
            var isDueTomorrow = hoursLeft is > 0 and <= 24;

            if (!isOverdue && !isDueTomorrow)
                continue;

            // Load vendor admin users to notify
            var vendorUsers = await _db.Set<VendorUser>()
                .Where(u => u.VendorId == po.VendorId && u.IsActive && u.Role == "vendor_admin")
                .ToListAsync(ct);

            foreach (var vendorUser in vendorUsers)
            {
                try
                {
                    var subject = isOverdue
                        ? $"[OVERDUE] PO {po.PONumber} — Acknowledgement Required Immediately"
                        : $"[Reminder] PO {po.PONumber} — Acknowledgement Due Tomorrow";

                    var body = isOverdue
                        ? $"<p>Purchase Order <strong>{po.PONumber}</strong> was due for acknowledgement on <strong>{deadline:d MMMM yyyy}</strong> and is now overdue. Please acknowledge immediately.</p>"
                        : $"<p>Purchase Order <strong>{po.PONumber}</strong> must be acknowledged by <strong>{deadline:d MMMM yyyy HH:mm} UTC</strong>. Please log in to acknowledge before the deadline.</p>";

                    await _email.SendAsync(vendorUser.Email, subject, body, ct);

                    await _notifications.SendToVendorUserAsync(
                        vendorUser.Id,
                        isOverdue ? "PO Acknowledgement Overdue" : "PO Acknowledgement Reminder",
                        isOverdue
                            ? $"{po.PONumber} acknowledgement is overdue. Action required immediately."
                            : $"{po.PONumber} acknowledgement is due tomorrow.",
                        ct: ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to send acknowledgement reminder to vendor user {VendorUserId} for PO {PONumber}",
                        vendorUser.Id, po.PONumber);
                }
            }

            _logger.LogInformation(
                "PO acknowledgement {Type} sent for PO {PONumber} (deadline: {Deadline}). Notified {Count} vendor user(s).",
                isOverdue ? "OVERDUE escalation" : "H-24 reminder",
                po.PONumber, deadline, vendorUsers.Count);
        }

        _logger.LogInformation(
            "POAcknowledgementReminderJob completed. Processed {Count} unacknowledged POs.", orders.Count);
    }
}
