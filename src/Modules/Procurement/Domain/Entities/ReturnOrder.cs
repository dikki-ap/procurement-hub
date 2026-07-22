using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.SharedKernel.Domain;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Domain.Entities;

public class ReturnOrder : AggregateRoot
{
    public string       ReturnNumber    { get; set; } = string.Empty;
    public Guid         GRNId           { get; set; }
    public Guid         POId            { get; set; }
    public Guid         VendorId        { get; set; }
    public ReturnStatus Status          { get; set; } = ReturnStatus.Created;
    public string?      Reason          { get; set; }
    public string?      Notes           { get; set; }
    public DateTime?    AcknowledgedAt  { get; set; }
    public DateTime?    ShippedAt       { get; set; }
    public DateTime?    ReceivedAt      { get; set; }

    public GoodsReceipt?              GoodsReceipt { get; set; }
    public ICollection<ReturnOrderItem> Items      { get; set; } = [];

    public static ReturnOrder Create(
        string returnNumber, Guid grnId, Guid poId, Guid vendorId,
        string? reason, string? notes)
    {
        return new ReturnOrder
        {
            Id           = UUIDNext.Uuid.NewSequential(),
            ReturnNumber = returnNumber,
            GRNId        = grnId,
            POId         = poId,
            VendorId     = vendorId,
            Reason       = reason,
            Notes        = notes,
            Status       = ReturnStatus.Created,
        };
    }

    public void Acknowledge()
    {
        if (Status != ReturnStatus.Created)
            throw new BusinessRuleException("ReturnAcknowledge",
                $"Only created returns can be acknowledged. Current status: {Status}");

        Status          = ReturnStatus.Acknowledged;
        AcknowledgedAt  = DateTime.UtcNow;
    }

    public void MarkShipped()
    {
        if (Status != ReturnStatus.Acknowledged)
            throw new BusinessRuleException("ReturnShip",
                $"Return must be acknowledged before shipping. Current status: {Status}");

        Status     = ReturnStatus.Shipped;
        ShippedAt  = DateTime.UtcNow;
    }

    public void ConfirmReceived()
    {
        if (Status != ReturnStatus.Shipped)
            throw new BusinessRuleException("ReturnReceived",
                $"Return must be shipped before confirming receipt. Current status: {Status}");

        Status     = ReturnStatus.Received;
        ReceivedAt = DateTime.UtcNow;
    }
}
