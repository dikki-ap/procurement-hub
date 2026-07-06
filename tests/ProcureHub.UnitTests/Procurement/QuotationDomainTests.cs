using FluentAssertions;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.Modules.Procurement.Domain.Events;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.UnitTests.Procurement;

public class QuotationDomainTests
{
    private static readonly Guid _rfqItemId1 = Guid.NewGuid();
    private static readonly Guid _rfqItemId2 = Guid.NewGuid();

    private static VendorQuotation CreateDraftQuotation(bool withItems = true)
    {
        var q = new VendorQuotation
        {
            RFQId    = Guid.NewGuid(),
            VendorId = Guid.NewGuid(),
            Status   = QuotationStatus.Draft,
        };

        if (withItems)
        {
            q.Items.Add(new QuotationItem { RFQItemId = _rfqItemId1, UnitPrice = 1000m, Quantity = 5m });
            q.Items.Add(new QuotationItem { RFQItemId = _rfqItemId2, UnitPrice = 500m,  Quantity = 10m });
        }

        return q;
    }

    private static IReadOnlyCollection<Guid> AllRFQItemIds => [_rfqItemId1, _rfqItemId2];
    private static DateTime FutureDeadline => DateTime.UtcNow.AddHours(48);
    private static DateTime PastDeadline   => DateTime.UtcNow.AddHours(-1);

    // ── Submit: valid ────────────────────────────────────────────────────────

    [Fact]
    public void Submit_WithValidItems_ShouldChangeStatusToSubmitted()
    {
        var q = CreateDraftQuotation();

        q.Submit(FutureDeadline, AllRFQItemIds);

        q.Status.Should().Be(QuotationStatus.Submitted);
    }

    [Fact]
    public void Submit_WithValidItems_ShouldCalculateTotalPrice()
    {
        var q = CreateDraftQuotation();

        q.Submit(FutureDeadline, AllRFQItemIds);

        q.TotalPrice.Should().Be(10000m); // 1000*5 + 500*10
    }

    [Fact]
    public void Submit_WithValidItems_ShouldRaiseQuotationSubmittedEvent()
    {
        var q = CreateDraftQuotation();

        q.Submit(FutureDeadline, AllRFQItemIds);

        q.DomainEvents.OfType<QuotationSubmittedEvent>().Should().ContainSingle();
    }

    // ── Submit: after deadline ───────────────────────────────────────────────

    [Fact]
    public void Submit_AfterDeadline_ShouldThrowBusinessRuleException()
    {
        var q = CreateDraftQuotation();

        var act = () => q.Submit(PastDeadline, AllRFQItemIds);

        act.Should().Throw<BusinessRuleException>()
           .WithMessage("*deadline*");
    }

    // ── Submit: missing RFQ items ────────────────────────────────────────────

    [Fact]
    public void Submit_WithMissingRFQItems_ShouldThrowBusinessRuleException()
    {
        var q = CreateDraftQuotation(withItems: false);
        // Only quote one item, but two are required
        q.Items.Add(new QuotationItem { RFQItemId = _rfqItemId1, UnitPrice = 1000m, Quantity = 5m });

        var act = () => q.Submit(FutureDeadline, AllRFQItemIds);

        act.Should().Throw<BusinessRuleException>()
           .WithMessage("*All RFQ items must be quoted*");
    }

    [Fact]
    public void Submit_WithNoItems_ShouldThrowBusinessRuleException()
    {
        var q = CreateDraftQuotation(withItems: false);

        var act = () => q.Submit(FutureDeadline, AllRFQItemIds);

        act.Should().Throw<BusinessRuleException>()
           .WithMessage("*at least one item*");
    }

    [Fact]
    public void Submit_WhenAlreadySubmitted_ShouldThrowBusinessRuleException()
    {
        var q = CreateDraftQuotation();
        q.Submit(FutureDeadline, AllRFQItemIds);

        var act = () => q.Submit(FutureDeadline, AllRFQItemIds);

        act.Should().Throw<BusinessRuleException>();
    }

    // ── Withdraw ─────────────────────────────────────────────────────────────

    [Fact]
    public void Withdraw_FromSubmitted_ShouldChangeStatusToWithdrawn()
    {
        var q = CreateDraftQuotation();
        q.Submit(FutureDeadline, AllRFQItemIds);

        q.Withdraw();

        q.Status.Should().Be(QuotationStatus.Withdrawn);
    }

    [Fact]
    public void Withdraw_FromDraft_ShouldThrowBusinessRuleException()
    {
        var q = CreateDraftQuotation();

        var act = () => q.Withdraw();

        act.Should().Throw<BusinessRuleException>();
    }
}
