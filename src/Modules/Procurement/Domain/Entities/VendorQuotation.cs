using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.Modules.Procurement.Domain.Events;
using ProcureHub.SharedKernel.Domain;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Domain.Entities;

public class VendorQuotation : AggregateRoot
{
    public Guid             RFQId    { get; set; }
    public Guid             VendorId { get; set; }
    public QuotationStatus  Status   { get; set; } = QuotationStatus.Draft;
    public decimal          TotalPrice { get; set; }
    public string?          Notes      { get; set; }
    public string?          FileKey    { get; set; }
    public string?          FileName   { get; set; }

    // Navigation
    public ICollection<QuotationItem> Items { get; set; } = [];

    // ── Domain methods ──────────────────────────────────────────────────────

    public void Submit(DateTime bidDeadline, IReadOnlyCollection<Guid> rfqItemIds)
    {
        if (Status != QuotationStatus.Draft)
            throw new BusinessRuleException("QuotationSubmit",
                $"Only draft quotations can be submitted. Current status: {Status}");

        if (DateTime.UtcNow > bidDeadline)
            throw new BusinessRuleException("QuotationSubmit", "Bid deadline has passed.");

        if (!Items.Any())
            throw new BusinessRuleException("QuotationSubmit", "Quotation must include at least one item.");

        var quotedIds = Items.Select(i => i.RFQItemId).ToHashSet();
        if (rfqItemIds.Any(id => !quotedIds.Contains(id)))
            throw new BusinessRuleException("QuotationSubmit", "All RFQ items must be quoted.");

        RecalculateTotalPrice();
        Status = QuotationStatus.Submitted;
        AddDomainEvent(new QuotationSubmittedEvent(Id, RFQId, VendorId));
    }

    public void Withdraw()
    {
        if (Status != QuotationStatus.Submitted)
            throw new BusinessRuleException("QuotationWithdraw",
                $"Only submitted quotations can be withdrawn. Current status: {Status}");

        Status = QuotationStatus.Withdrawn;
    }

    public void MarkAwarded()  => Status = QuotationStatus.Awarded;
    public void MarkRejected() => Status = QuotationStatus.Rejected;

    public void RecalculateTotalPrice()
        => TotalPrice = Items.Sum(i => i.UnitPrice * i.Quantity);
}
