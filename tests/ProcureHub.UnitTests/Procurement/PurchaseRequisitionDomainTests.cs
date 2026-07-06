using FluentAssertions;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.Modules.Procurement.Domain.Events;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.UnitTests.Procurement;

public class PurchaseRequisitionDomainTests
{
    private static PurchaseRequisition CreateDraftPR(bool withItems = true)
    {
        var pr = new PurchaseRequisition
        {
            CompanyId     = Guid.NewGuid(),
            PRNumber      = "PR-2026-000001",
            Title         = "Office Supplies Q3",
            Department    = "Operations",
            RequiredDate  = DateTime.UtcNow.AddDays(14),
            Status        = PRStatus.Draft,
            RequestedById = Guid.NewGuid(),
        };

        if (withItems)
        {
            pr.Items.Add(new PRItem
            {
                ItemDescription    = "A4 Paper Ream",
                Quantity           = 10,
                EstimatedUnitPrice = 50_000,
            });
        }

        return pr;
    }

    // ── Submit ───────────────────────────────────────────────────────────────

    [Fact]
    public void Submit_FromDraft_WithItems_ShouldSetStatusSubmitted()
    {
        var pr = CreateDraftPR();

        pr.Submit();

        pr.Status.Should().Be(PRStatus.Submitted);
    }

    [Fact]
    public void Submit_FromDraft_ShouldRaisePRSubmittedEvent()
    {
        var pr = CreateDraftPR();

        pr.Submit();

        pr.DomainEvents.Should().ContainSingle(e => e is PRSubmittedEvent);
        var evt = pr.DomainEvents.OfType<PRSubmittedEvent>().Single();
        evt.PRNumber.Should().Be(pr.PRNumber);
        evt.PRId.Should().Be(pr.Id);
    }

    [Fact]
    public void Submit_WhenNotDraft_ShouldThrowBusinessRuleException()
    {
        var pr = CreateDraftPR();
        pr.Submit();

        var act = () => pr.Submit();

        act.Should().Throw<BusinessRuleException>()
           .WithMessage("*Only draft PRs can be submitted*");
    }

    [Fact]
    public void Submit_WithNoItems_ShouldThrowBusinessRuleException()
    {
        var pr = CreateDraftPR(withItems: false);

        var act = () => pr.Submit();

        act.Should().Throw<BusinessRuleException>()
           .WithMessage("*at least one item*");
    }

    // ── Cancel ───────────────────────────────────────────────────────────────

    [Fact]
    public void Cancel_FromDraft_ShouldSetStatusCancelled()
    {
        var pr = CreateDraftPR();

        pr.Cancel();

        pr.Status.Should().Be(PRStatus.Cancelled);
    }

    [Fact]
    public void Cancel_FromSubmitted_ShouldSetStatusCancelled()
    {
        var pr = CreateDraftPR();
        pr.Submit();

        pr.Cancel();

        pr.Status.Should().Be(PRStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenApproved_ShouldThrowBusinessRuleException()
    {
        var pr = CreateDraftPR();
        pr.Status = PRStatus.Approved;

        var act = () => pr.Cancel();

        act.Should().Throw<BusinessRuleException>()
           .WithMessage("*cannot be cancelled*");
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ShouldThrowBusinessRuleException()
    {
        var pr = CreateDraftPR();
        pr.Cancel();

        var act = () => pr.Cancel();

        act.Should().Throw<BusinessRuleException>()
           .WithMessage("*cannot be cancelled*");
    }

    // ── RecalculateTotalValue ─────────────────────────────────────────────────

    [Fact]
    public void RecalculateTotalValue_ShouldSumAllLineItems()
    {
        var pr = CreateDraftPR(withItems: false);
        pr.Items.Add(new PRItem { Quantity = 5,  EstimatedUnitPrice = 100_000, ItemDescription = "Item A" });
        pr.Items.Add(new PRItem { Quantity = 10, EstimatedUnitPrice = 50_000,  ItemDescription = "Item B" });

        pr.RecalculateTotalValue();

        pr.TotalEstimatedValue.Should().Be(1_000_000m);
    }
}
