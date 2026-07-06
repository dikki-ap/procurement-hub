using FluentAssertions;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.Modules.Procurement.Domain.Events;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.UnitTests.Procurement;

public class RFQDomainTests
{
    private static RFQ CreateDraftRFQ(int vendorCount = 0, bool withItems = true)
    {
        var rfq = new RFQ
        {
            CompanyId   = Guid.NewGuid(),
            RFQNumber   = "RFQ-2026-000001",
            Title       = "Procurement of Office Supplies",
            BidDeadline = DateTime.UtcNow.AddDays(7),
            Status      = RFQStatus.Draft,
        };

        if (withItems)
        {
            rfq.Items.Add(new RFQItem
            {
                ItemDescription = "A4 Paper Ream",
                Quantity        = 10,
            });
        }

        for (var i = 0; i < vendorCount; i++)
        {
            rfq.Vendors.Add(new RFQVendor
            {
                VendorId  = Guid.NewGuid(),
                InvitedAt = DateTime.UtcNow,
                Status    = RFQVendorStatus.Invited,
            });
        }

        return rfq;
    }

    // ── Open ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Open_WithEnoughVendors_ShouldSetStatusOpen()
    {
        var rfq = CreateDraftRFQ(vendorCount: RFQ.MinimumVendors);

        rfq.Open();

        rfq.Status.Should().Be(RFQStatus.Open);
    }

    [Fact]
    public void Open_ShouldRaiseRFQCreatedEvent()
    {
        var rfq = CreateDraftRFQ(vendorCount: RFQ.MinimumVendors);

        rfq.Open();

        rfq.DomainEvents.Should().ContainSingle(e => e is RFQCreatedEvent);
        var evt = rfq.DomainEvents.OfType<RFQCreatedEvent>().Single();
        evt.InvitedVendorIds.Should().HaveCount(RFQ.MinimumVendors);
    }

    [Fact]
    public void Open_WithFewerThanMinimumVendors_ShouldThrowBusinessRuleException()
    {
        var rfq = CreateDraftRFQ(vendorCount: RFQ.MinimumVendors - 1);

        var act = () => rfq.Open();

        act.Should().Throw<BusinessRuleException>()
           .WithMessage($"*At least {RFQ.MinimumVendors} vendors*");
    }

    [Fact]
    public void Open_WithNoItems_ShouldThrowBusinessRuleException()
    {
        var rfq = CreateDraftRFQ(vendorCount: RFQ.MinimumVendors, withItems: false);

        var act = () => rfq.Open();

        act.Should().Throw<BusinessRuleException>()
           .WithMessage("*at least one item*");
    }

    [Fact]
    public void Open_WhenNotDraft_ShouldThrowBusinessRuleException()
    {
        var rfq = CreateDraftRFQ(vendorCount: RFQ.MinimumVendors);
        rfq.Open();

        var act = () => rfq.Open();

        act.Should().Throw<BusinessRuleException>()
           .WithMessage("*Only draft RFQs*");
    }

    // ── Close ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Close_WhenOpen_ShouldSetStatusClosed()
    {
        var rfq = CreateDraftRFQ(vendorCount: RFQ.MinimumVendors);
        rfq.Open();

        rfq.Close();

        rfq.Status.Should().Be(RFQStatus.Closed);
    }

    [Fact]
    public void Close_WhenOpen_ShouldRaiseRFQClosedEvent()
    {
        var rfq = CreateDraftRFQ(vendorCount: RFQ.MinimumVendors);
        rfq.Open();
        rfq.ClearDomainEvents();

        rfq.Close();

        rfq.DomainEvents.Should().ContainSingle(e => e is RFQClosedEvent);
    }

    [Fact]
    public void Close_WhenNotOpen_ShouldThrowBusinessRuleException()
    {
        var rfq = CreateDraftRFQ(vendorCount: RFQ.MinimumVendors);

        var act = () => rfq.Close();

        act.Should().Throw<BusinessRuleException>()
           .WithMessage("*Only open RFQs can be closed*");
    }

    // ── Cancel ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Cancel_FromDraft_ShouldSetStatusCancelled()
    {
        var rfq = CreateDraftRFQ();

        rfq.Cancel();

        rfq.Status.Should().Be(RFQStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenClosed_ShouldThrowBusinessRuleException()
    {
        var rfq = CreateDraftRFQ(vendorCount: RFQ.MinimumVendors);
        rfq.Open();
        rfq.Close();

        var act = () => rfq.Cancel();

        act.Should().Throw<BusinessRuleException>()
           .WithMessage("*cannot be cancelled*");
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ShouldThrowBusinessRuleException()
    {
        var rfq = CreateDraftRFQ();
        rfq.Cancel();

        var act = () => rfq.Cancel();

        act.Should().Throw<BusinessRuleException>()
           .WithMessage("*cannot be cancelled*");
    }
}
