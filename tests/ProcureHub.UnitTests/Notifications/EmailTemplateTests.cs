using ProcureHub.SharedKernel.Notifications;

namespace ProcureHub.UnitTests.Notifications;

public class EmailTemplateTests
{
    [Fact]
    public void VendorApproved_ShouldContainVendorNameAndApprovalMessage()
    {
        var html = EmailTemplates.VendorApproved("Acme Corp");

        html.Should().Contain("Acme Corp");
        html.Should().Contain("approved");
        html.Should().Contain("ProcureHub");
    }

    [Fact]
    public void VendorBlacklisted_ShouldContainVendorNameAndReason()
    {
        var html = EmailTemplates.VendorBlacklisted("Evil Corp", "Non-compliant practices");

        html.Should().Contain("Evil Corp");
        html.Should().Contain("Non-compliant practices");
        html.Should().Contain("suspended");
    }

    [Fact]
    public void RFQInvitation_ShouldContainAllRequiredFields()
    {
        var deadline = new DateTime(2026, 8, 15, 10, 0, 0, DateTimeKind.Utc);

        var html = EmailTemplates.RFQInvitation(
            "Vendor A", "RFQ-2026-001", "Office Supplies", deadline);

        html.Should().Contain("Vendor A");
        html.Should().Contain("RFQ-2026-001");
        html.Should().Contain("Office Supplies");
        html.Should().Contain("2026");
    }

    [Fact]
    public void BidDeadlineReminder_ShouldContainHoursLeftAndRFQNumber()
    {
        var deadline = DateTime.UtcNow.AddHours(20);

        var html = EmailTemplates.BidDeadlineReminder(
            "Vendor B", "RFQ-2026-002", "IT Equipment", deadline, 24);

        html.Should().Contain("Vendor B");
        html.Should().Contain("RFQ-2026-002");
        html.Should().Contain("24");
        html.Should().Contain("reminder");
    }

    [Fact]
    public void ApprovalEscalation_ShouldContainApproverAndReference()
    {
        var html = EmailTemplates.ApprovalEscalation("John Doe", "PR-2026-001", 3);

        html.Should().Contain("John Doe");
        html.Should().Contain("PR-2026-001");
        html.Should().Contain("3");
        html.Should().Contain("escalation");
    }

    [Fact]
    public void AllTemplates_ShouldBeValidHtml()
    {
        var templates = new[]
        {
            EmailTemplates.VendorApproved("X"),
            EmailTemplates.VendorBlacklisted("X", "Y"),
            EmailTemplates.RFQInvitation("X", "RFQ-001", "Title", DateTime.UtcNow),
            EmailTemplates.BidDeadlineReminder("X", "RFQ-001", "Title", DateTime.UtcNow, 24),
            EmailTemplates.ApprovalRequired("X", "PR-001", "Desc", "User"),
            EmailTemplates.ApprovalEscalation("X", "PR-001", 3),
        };

        foreach (var html in templates)
        {
            html.Should().StartWith("<!DOCTYPE html>");
            html.Should().Contain("</html>");
            html.Should().Contain("ProcureHub");
        }
    }
}
