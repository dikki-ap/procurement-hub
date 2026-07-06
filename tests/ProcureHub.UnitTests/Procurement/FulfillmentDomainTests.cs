using Moq;
using ProcureHub.Modules.Procurement.Application.Commands.CreateGRN;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.Modules.Procurement.Domain.Events;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.UnitTests.Procurement;

public class FulfillmentDomainTests
{
    // ── PurchaseOrder state machine ───────────────────────────────────────────

    private static PurchaseOrder CreateApprovedPO()
    {
        var po = PurchaseOrder.Create(
            Guid.NewGuid(), "PO-2026-000001", Guid.NewGuid(),
            null, null, null, null, null, null, null);
        po.Status = POStatus.Approved;
        po.Items.Add(new POItem
        {
            Description = "Widget A",
            Quantity    = 10,
            UnitPrice   = 50_000,
            TotalPrice  = 500_000,
        });
        return po;
    }

    [Fact]
    public void Issue_ApprovedPO_ShouldSetStatusIssuedAndRaiseEvent()
    {
        var po = CreateApprovedPO();

        po.Issue("data:application/pdf;base64,dGVzdA==");

        po.Status.Should().Be(POStatus.Issued);
        po.FileUrl.Should().NotBeNullOrEmpty();
        po.IssuedAt.Should().NotBeNull();
        po.DomainEvents.OfType<POIssuedEvent>().Should().HaveCount(1);
    }

    [Fact]
    public void Issue_DraftPO_ShouldThrowBusinessRuleException()
    {
        var po = PurchaseOrder.Create(
            Guid.NewGuid(), "PO-2026-000002", Guid.NewGuid(),
            null, null, null, null, null, null, null);

        var act = () => po.Issue("some-url");

        act.Should().Throw<BusinessRuleException>()
            .Which.RuleName.Should().Be("POIssue");
    }

    [Fact]
    public void Issue_ApprovedPOWithNoItems_ShouldThrowBusinessRuleException()
    {
        var po = PurchaseOrder.Create(
            Guid.NewGuid(), "PO-2026-000003", Guid.NewGuid(),
            null, null, null, null, null, null, null);
        po.Status = POStatus.Approved;

        var act = () => po.Issue("some-url");

        act.Should().Throw<BusinessRuleException>()
            .Which.RuleName.Should().Be("POIssue");
    }

    [Fact]
    public void Acknowledge_IssuedPO_ShouldSetStatusAcknowledged()
    {
        var po = CreateApprovedPO();
        po.Issue("url");

        po.Acknowledge();

        po.Status.Should().Be(POStatus.Acknowledged);
        po.AcknowledgedAt.Should().NotBeNull();
    }

    [Fact]
    public void Acknowledge_NonIssuedPO_ShouldThrow()
    {
        var po = CreateApprovedPO();

        var act = () => po.Acknowledge();

        act.Should().Throw<BusinessRuleException>()
            .Which.RuleName.Should().Be("POAcknowledge");
    }

    [Fact]
    public void Cancel_CompletedPO_ShouldThrow()
    {
        var po = CreateApprovedPO();
        po.Status = POStatus.Completed;

        var act = () => po.Cancel("reason");

        act.Should().Throw<BusinessRuleException>()
            .Which.RuleName.Should().Be("POCancel");
    }

    // ── GoodsReceipt state machine ────────────────────────────────────────────

    private static GoodsReceipt CreateDraftGRNWithItems()
    {
        var grn = GoodsReceipt.Create(
            "GRN-2026-000001", Guid.NewGuid(), Guid.NewGuid(), null, null);
        grn.Items.Add(new GRNItem
        {
            POItemId      = Guid.NewGuid(),
            ReceivedQty   = 10,
            RejectedQty   = 0,
            QualityStatus = QualityStatus.Accepted,
        });
        return grn;
    }

    [Fact]
    public void Confirm_AllAccepted_ShouldSetStatusConfirmed()
    {
        var grn      = CreateDraftGRNWithItems();
        var vendorId = Guid.NewGuid();

        grn.Confirm(vendorId);

        grn.Status.Should().Be(GRNStatus.Confirmed);
        grn.DomainEvents.OfType<GRNConfirmedEvent>().Should().HaveCount(1);
        grn.DomainEvents.OfType<GRNConfirmedEvent>().Single().HasDiscrepancy.Should().BeFalse();
    }

    [Fact]
    public void Confirm_WithRejectedItems_ShouldSetStatusDiscrepancy()
    {
        var grn = CreateDraftGRNWithItems();
        grn.Items.First().RejectedQty   = 2;
        grn.Items.First().QualityStatus = QualityStatus.Partial;

        grn.Confirm(Guid.NewGuid());

        grn.Status.Should().Be(GRNStatus.Discrepancy);
        grn.DomainEvents.OfType<GRNConfirmedEvent>().Single().HasDiscrepancy.Should().BeTrue();
    }

    [Fact]
    public void Confirm_AlreadyConfirmed_ShouldThrow()
    {
        var grn = CreateDraftGRNWithItems();
        grn.Confirm(Guid.NewGuid());

        var act = () => grn.Confirm(Guid.NewGuid());

        act.Should().Throw<BusinessRuleException>()
            .Which.RuleName.Should().Be("GRNConfirm");
    }

    // ── CreateGRN qty validation (handler-level) ──────────────────────────────

    [Fact]
    public async Task CreateGRN_ExceedsOrderedQty_ShouldThrowBusinessRuleException()
    {
        // Arrange: PO with item that has ordered=10, already received=3
        var poId   = Guid.NewGuid();
        var poItem = new POItem
        {
            Description = "Widget A",
            Quantity    = 10,
            ReceivedQty = 3,
        };
        var po = PurchaseOrder.Create(
            Guid.NewGuid(), "PO-2026-000004", Guid.NewGuid(),
            null, null, null, null, null, null, null);
        po.Items.Add(poItem);

        var mockPoRepo  = new Mock<IPurchaseOrderRepository>();
        var mockGrnRepo = new Mock<IGoodsReceiptRepository>();
        mockPoRepo.Setup(r => r.GetByIdWithItemsAsync(poId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(po);

        var handler = new CreateGRNCommandHandler(mockGrnRepo.Object, mockPoRepo.Object);

        // 3 already received + 8 new = 11 > 10 → should throw
        var command = new CreateGRNCommand(
            POId:           poId,
            ReceivedBy:     Guid.NewGuid(),
            DeliveryNoteNo: null,
            Notes:          null,
            Items:
            [
                new GRNItemInput(
                    POItemId:      poItem.Id,
                    ReceivedQty:   8,
                    RejectedQty:   0,
                    QualityStatus: QualityStatus.Accepted,
                    RejectReason:  null,
                    Notes:         null),
            ]);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task CreateGRN_ExactRemainingQty_ShouldNotThrow()
    {
        // Arrange: PO with item that has ordered=10, already received=3 → 7 remaining
        var poId   = Guid.NewGuid();
        var poItem = new POItem
        {
            Description = "Widget B",
            Quantity    = 10,
            ReceivedQty = 3,
        };
        var po = PurchaseOrder.Create(
            Guid.NewGuid(), "PO-2026-000005", Guid.NewGuid(),
            null, null, null, null, null, null, null);
        po.Items.Add(poItem);

        var mockPoRepo  = new Mock<IPurchaseOrderRepository>();
        var mockGrnRepo = new Mock<IGoodsReceiptRepository>();
        mockPoRepo.Setup(r => r.GetByIdWithItemsAsync(poId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(po);
        mockGrnRepo.Setup(r => r.GenerateNextNumberAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("GRN-2026-000001");

        var handler = new CreateGRNCommandHandler(mockGrnRepo.Object, mockPoRepo.Object);

        // 3 + 7 = 10 == 10 → OK
        var command = new CreateGRNCommand(
            POId:           poId,
            ReceivedBy:     Guid.NewGuid(),
            DeliveryNoteNo: null,
            Notes:          null,
            Items:
            [
                new GRNItemInput(
                    POItemId:      poItem.Id,
                    ReceivedQty:   7,
                    RejectedQty:   0,
                    QualityStatus: QualityStatus.Accepted,
                    RejectReason:  null,
                    Notes:         null),
            ]);

        // No BusinessRuleException means handler completes without throwing
        var ex = await Record.ExceptionAsync(
            () => handler.Handle(command, CancellationToken.None));

        ex.Should().BeNull("handler should not throw for valid remaining qty");
    }
}
