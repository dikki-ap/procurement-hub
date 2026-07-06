using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.Modules.Procurement.Domain.Events;
using ProcureHub.SharedKernel.Domain;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Domain.Entities;

public class GoodsReceipt : AggregateRoot
{
    public string     GRNNumber       { get; set; } = string.Empty;
    public Guid       POId            { get; set; }
    public GRNStatus  Status          { get; set; } = GRNStatus.Draft;
    public Guid       ReceivedBy      { get; set; }
    public DateTime?  ReceivedAt      { get; set; }
    public string?    DeliveryNoteNo  { get; set; }
    public string?    Notes           { get; set; }

    public ICollection<GRNItem> Items { get; set; } = [];
    public PurchaseOrder? PurchaseOrder { get; set; }

    public static GoodsReceipt Create(
        string grnNumber, Guid poId, Guid receivedBy,
        string? deliveryNoteNo, string? notes)
    {
        return new GoodsReceipt
        {
            Id             = UUIDNext.Uuid.NewSequential(),
            GRNNumber      = grnNumber,
            POId           = poId,
            ReceivedBy     = receivedBy,
            ReceivedAt     = DateTime.UtcNow,
            DeliveryNoteNo = deliveryNoteNo,
            Notes          = notes,
            Status         = GRNStatus.Draft,
        };
    }

    public void Confirm(Guid vendorId)
    {
        if (Status != GRNStatus.Draft)
            throw new BusinessRuleException("GRNConfirm",
                $"Only draft GRNs can be confirmed. Current status: {Status}");

        if (!Items.Any())
            throw new BusinessRuleException("GRNConfirm", "GRN must have at least one item.");

        var hasDiscrepancy = Items.Any(i =>
            i.QualityStatus != QualityStatus.Accepted || i.RejectedQty > 0);

        Status = hasDiscrepancy ? GRNStatus.Discrepancy : GRNStatus.Confirmed;

        AddDomainEvent(new GRNConfirmedEvent(Id, GRNNumber, POId, vendorId, hasDiscrepancy));
    }
}
