using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.Modules.Procurement.Domain.Events;
using ProcureHub.SharedKernel.Domain;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Domain.Entities;

public class RFQ : AggregateRoot
{
    public const int MinimumVendors = 3;

    public Guid      CompanyId                { get; set; }
    public string    RFQNumber                { get; set; } = string.Empty;
    public string    Title                    { get; set; } = string.Empty;
    public Guid?     PurchaseRequisitionId    { get; set; }
    public DateTime  BidDeadline              { get; set; }
    public DateTime? DeliveryDate             { get; set; }
    public RFQStatus Status                   { get; set; } = RFQStatus.Draft;
    public string?   Notes                    { get; set; }
    public string?   Terms                    { get; set; }

    // Navigation
    public ICollection<RFQItem>   Items   { get; set; } = [];
    public ICollection<RFQVendor> Vendors { get; set; } = [];

    // ── Domain methods ──────────────────────────────────────────────────────

    public void Open()
    {
        if (Status != RFQStatus.Draft)
            throw new BusinessRuleException("RFQOpen", $"Only draft RFQs can be opened. Current status: {Status}");

        if (!Items.Any())
            throw new BusinessRuleException("RFQOpen", "RFQ must have at least one item before opening.");

        var activeVendors = Vendors.Count(v => v.Status == RFQVendorStatus.Invited);
        if (activeVendors < MinimumVendors)
            throw new BusinessRuleException("RFQOpen",
                $"At least {MinimumVendors} vendors must be invited before opening. Currently invited: {activeVendors}");

        if (BidDeadline <= DateTime.UtcNow)
            throw new BusinessRuleException("RFQOpen", "Bid deadline must be in the future.");

        Status = RFQStatus.Open;
        AddDomainEvent(new RFQCreatedEvent(
            Id, RFQNumber,
            Vendors.Select(v => v.VendorId).ToList()));
    }

    public void Close()
    {
        if (Status != RFQStatus.Open)
            throw new BusinessRuleException("RFQClose", $"Only open RFQs can be closed. Current status: {Status}");

        Status = RFQStatus.Closed;
        AddDomainEvent(new RFQClosedEvent(Id, RFQNumber));
    }

    public void Cancel()
    {
        if (Status is RFQStatus.Closed or RFQStatus.Cancelled)
            throw new BusinessRuleException("RFQCancel", $"RFQ cannot be cancelled from status: {Status}");

        Status = RFQStatus.Cancelled;
    }
}
