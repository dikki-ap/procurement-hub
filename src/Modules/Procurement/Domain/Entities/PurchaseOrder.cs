using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.Modules.Procurement.Domain.Events;
using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.SharedKernel.Domain;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Domain.Entities;

public class PurchaseOrder : AggregateRoot
{
    public Guid      CompanyId          { get; set; }
    public string    PONumber           { get; set; } = string.Empty;
    public Guid?     RFQId              { get; set; }
    public Guid      VendorId           { get; set; }
    public POStatus  Status             { get; set; } = POStatus.Draft;
    public decimal   TotalAmount        { get; set; }
    public Guid?     CurrencyId         { get; set; }
    public Guid?     PaymentTermId      { get; set; }
    public Guid?     DeliveryLocationId { get; set; }
    public DateTime? ExpectedDelivery   { get; set; }
    public DateTime? ActualDelivery     { get; set; }
    public string?   FileUrl            { get; set; }
    public string?   Notes              { get; set; }
    public string?   TermsConditions    { get; set; }
    public DateTime? IssuedAt           { get; set; }
    public DateTime? AcknowledgedAt     { get; set; }
    public DateTime? CompletedAt        { get; set; }
    public DateTime? CancelledAt        { get; set; }
    public string?   CancelledReason    { get; set; }

    public Vendor?             Vendor { get; set; }
    public ICollection<POItem> Items  { get; set; } = [];

    public static PurchaseOrder Create(
        Guid companyId, string poNumber, Guid vendorId,
        Guid? rfqId, Guid? currencyId, Guid? paymentTermId,
        Guid? deliveryLocationId, DateTime? expectedDelivery,
        string? notes, string? termsConditions)
    {
        return new PurchaseOrder
        {
            Id                 = UUIDNext.Uuid.NewSequential(),
            CompanyId          = companyId,
            PONumber           = poNumber,
            VendorId           = vendorId,
            RFQId              = rfqId,
            CurrencyId         = currencyId,
            PaymentTermId      = paymentTermId,
            DeliveryLocationId = deliveryLocationId,
            ExpectedDelivery   = expectedDelivery,
            Notes              = notes,
            TermsConditions    = termsConditions,
            Status             = POStatus.Draft,
        };
    }

    public void Issue(string fileUrl)
    {
        if (Status != POStatus.Approved)
            throw new BusinessRuleException("POIssue",
                $"Only approved POs can be issued. Current status: {Status}");

        if (!Items.Any())
            throw new BusinessRuleException("POIssue", "PO must have at least one item.");

        Status   = POStatus.Issued;
        FileUrl  = fileUrl;
        IssuedAt = DateTime.UtcNow;

        AddDomainEvent(new POIssuedEvent(Id, PONumber, VendorId, CompanyId, TotalAmount));
    }

    public void Acknowledge()
    {
        if (Status != POStatus.Issued)
            throw new BusinessRuleException("POAcknowledge",
                $"Only issued POs can be acknowledged. Current status: {Status}");

        Status          = POStatus.Acknowledged;
        AcknowledgedAt  = DateTime.UtcNow;
    }

    public void MarkInDelivery()
    {
        if (Status != POStatus.Acknowledged)
            throw new BusinessRuleException("POInDelivery",
                $"PO must be acknowledged before marking in-delivery. Current status: {Status}");

        Status = POStatus.InDelivery;
    }

    public void Complete()
    {
        if (Status is not (POStatus.Acknowledged or POStatus.InDelivery))
            throw new BusinessRuleException("POComplete",
                $"PO cannot be completed from status: {Status}");

        Status      = POStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Cancel(string reason)
    {
        if (Status is POStatus.Completed or POStatus.Cancelled)
            throw new BusinessRuleException("POCancel",
                $"PO cannot be cancelled from status: {Status}");

        Status          = POStatus.Cancelled;
        CancelledAt     = DateTime.UtcNow;
        CancelledReason = reason;
    }

    public void Approve()
    {
        if (Status != POStatus.PendingApproval)
            throw new BusinessRuleException("POApprove",
                $"Only pending-approval POs can be approved. Current status: {Status}");

        Status = POStatus.Approved;
    }

    public void RecalculateTotal() =>
        TotalAmount = Items.Sum(i => i.TotalPrice);
}
