using Hangfire;
using Microsoft.Extensions.Logging;
using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.Modules.Procurement.Domain.Repositories;

namespace ProcureHub.Modules.Procurement.Infrastructure.Jobs;

public class BidDeadlineReminderJob
{
    private readonly IRFQRepository                  _rfqRepo;
    private readonly ILogger<BidDeadlineReminderJob> _logger;

    public BidDeadlineReminderJob(IRFQRepository rfqRepo, ILogger<BidDeadlineReminderJob> logger)
    {
        _rfqRepo = rfqRepo;
        _logger  = _logger = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        var now    = DateTime.UtcNow;
        var h72    = now.AddHours(72);
        var h24    = now.AddHours(24);

        // Fetch all open RFQs — repository returns from shared DB context
        var openRFQs = await _rfqRepo.GetOpenWithVendorsAsync(ct);

        foreach (var rfq in openRFQs)
        {
            var hoursLeft = (rfq.BidDeadline - now).TotalHours;

            if (hoursLeft > 0 && hoursLeft <= 24)
            {
                _logger.LogInformation(
                    "Bid deadline H-24 reminder — RFQId: {RFQId}, RFQNumber: {RFQNumber}, Deadline: {Deadline}",
                    rfq.Id, rfq.RFQNumber, rfq.BidDeadline);
                // TODO: Send H-24 reminder emails to invited vendors via IEmailService.
            }
            else if (hoursLeft > 24 && hoursLeft <= 72)
            {
                _logger.LogInformation(
                    "Bid deadline H-72 reminder — RFQId: {RFQId}, RFQNumber: {RFQNumber}, Deadline: {Deadline}",
                    rfq.Id, rfq.RFQNumber, rfq.BidDeadline);
                // TODO: Send H-72 reminder emails to invited vendors via IEmailService.
            }
        }

        _logger.LogInformation(
            "BidDeadlineReminderJob completed. Processed: {Count} open RFQs.", openRFQs.Count);
    }
}
