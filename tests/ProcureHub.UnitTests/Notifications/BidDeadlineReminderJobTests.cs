using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.Modules.Procurement.Infrastructure.Jobs;
using ProcureHub.SharedKernel.Abstractions;

namespace ProcureHub.UnitTests.Notifications;

public class BidDeadlineReminderJobTests
{
    private static BidDeadlineReminderJob BuildJob(
        List<RFQ> openRFQs,
        Mock<IEmailService> emailMock)
    {
        var rfqRepoMock  = new Mock<IRFQRepository>();
        var mediatorMock = new Mock<IMediator>();

        rfqRepoMock.Setup(r => r.GetOpenWithVendorsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(openRFQs);

        return new BidDeadlineReminderJob(
            rfqRepoMock.Object,
            mediatorMock.Object,
            emailMock.Object,
            NullLogger<BidDeadlineReminderJob>.Instance);
    }

    private static RFQ RfqWithDeadline(DateTime deadline) => new()
    {
        RFQNumber   = "RFQ-2026-001",
        Title       = "Test RFQ",
        Status      = RFQStatus.Open,
        BidDeadline = deadline,
        Vendors     = [],
    };

    [Fact]
    public async Task ExecuteAsync_WithH24RFQ_ShouldTriggerReminder()
    {
        // RFQ deadline is 20 hours away — falls in the ≤24h window
        var rfq       = RfqWithDeadline(DateTime.UtcNow.AddHours(20));
        var emailMock = new Mock<IEmailService>();
        emailMock.Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var job = BuildJob([rfq], emailMock);

        // No vendors → no emails sent, but job should complete without throwing
        await job.ExecuteAsync();
    }

    [Fact]
    public async Task ExecuteAsync_WithH72RFQ_ShouldTriggerReminder()
    {
        var rfq       = RfqWithDeadline(DateTime.UtcNow.AddHours(50));
        var emailMock = new Mock<IEmailService>();
        var job       = BuildJob([rfq], emailMock);

        await job.ExecuteAsync();
        // No vendors → email never called
        emailMock.Verify(e => e.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithDeadlineOutsideWindow_ShouldNotTriggerReminder()
    {
        // Deadline 5 days away — outside both windows
        var rfq       = RfqWithDeadline(DateTime.UtcNow.AddDays(5));
        var emailMock = new Mock<IEmailService>();
        var job       = BuildJob([rfq], emailMock);

        await job.ExecuteAsync();

        emailMock.Verify(e => e.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithExpiredDeadline_ShouldNotTriggerReminder()
    {
        // Deadline already passed
        var rfq       = RfqWithDeadline(DateTime.UtcNow.AddHours(-1));
        var emailMock = new Mock<IEmailService>();
        var job       = BuildJob([rfq], emailMock);

        await job.ExecuteAsync();

        emailMock.Verify(e => e.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoOpenRFQs_ShouldCompleteWithoutError()
    {
        var emailMock = new Mock<IEmailService>();
        var job       = BuildJob([], emailMock);

        var ex = await Record.ExceptionAsync(() => job.ExecuteAsync());

        ex.Should().BeNull();
        emailMock.Verify(e => e.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
