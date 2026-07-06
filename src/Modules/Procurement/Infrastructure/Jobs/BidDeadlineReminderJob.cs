using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;
using ProcureHub.SharedKernel.Notifications;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.Modules.VendorManagement.Application.Queries.GetVendorById;
using ProcureHub.SharedKernel.Abstractions;

namespace ProcureHub.Modules.Procurement.Infrastructure.Jobs;

public class BidDeadlineReminderJob
{
    private readonly IRFQRepository                  _rfqRepo;
    private readonly IMediator                       _mediator;
    private readonly IEmailService                   _email;
    private readonly ILogger<BidDeadlineReminderJob> _logger;

    public BidDeadlineReminderJob(
        IRFQRepository                  rfqRepo,
        IMediator                       mediator,
        IEmailService                   email,
        ILogger<BidDeadlineReminderJob> logger)
    {
        _rfqRepo  = rfqRepo;
        _mediator = mediator;
        _email    = email;
        _logger   = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        var now    = DateTime.UtcNow;
        var openRFQs = await _rfqRepo.GetOpenWithVendorsAsync(ct);

        foreach (var rfq in openRFQs)
        {
            var hoursLeft = (rfq.BidDeadline - now).TotalHours;
            int? reminder = hoursLeft is > 0 and <= 24 ? 24
                          : hoursLeft is > 24 and <= 72 ? 72
                          : null;

            if (reminder is null) continue;

            _logger.LogInformation(
                "Bid deadline H-{Hours} reminder — RFQId: {RFQId}, RFQNumber: {RFQNumber}",
                reminder, rfq.Id, rfq.RFQNumber);

            foreach (var rfqVendor in rfq.Vendors)
            {
                try
                {
                    var vendor = await _mediator.Send(new GetVendorByIdQuery(rfqVendor.VendorId), ct);
                    var email  = vendor.Contacts.FirstOrDefault(c => c.IsPrimary)?.Email
                              ?? vendor.Contacts.FirstOrDefault()?.Email;

                    if (email is null) continue;

                    var html = EmailTemplates.BidDeadlineReminder(
                        vendor.LegalName, rfq.RFQNumber, rfq.Title, rfq.BidDeadline, reminder.Value);

                    await _email.SendAsync(email, $"Bid Deadline Reminder ({reminder}h) — {rfq.RFQNumber}", html, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send deadline reminder to vendor {VendorId}", rfqVendor.VendorId);
                }
            }
        }

        _logger.LogInformation(
            "BidDeadlineReminderJob completed. Processed: {Count} open RFQs.", openRFQs.Count);
    }
}
