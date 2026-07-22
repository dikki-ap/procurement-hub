using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.SharedKernel.Domain;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Domain.Entities;

public class Invoice : AggregateRoot
{
    public string        InvoiceNumber    { get; set; } = string.Empty;
    public Guid          POId             { get; set; }
    public Guid          VendorId         { get; set; }
    public InvoiceStatus Status           { get; set; } = InvoiceStatus.Submitted;
    public decimal       Amount           { get; set; }
    public decimal       TaxAmount        { get; set; }
    public decimal       TotalAmount      { get; set; }
    public Guid?         CurrencyId       { get; set; }
    public string?       FileUrl          { get; set; }
    public DateTime?     DueAt            { get; set; }
    public DateTime?     PaidAt           { get; set; }
    public string?       PaymentReference { get; set; }
    public string?       Notes            { get; set; }
    public string?       RejectionReason  { get; set; }
    public DateTime      SubmittedAt      { get; set; }
    public Guid?         ReviewedBy       { get; set; }
    public DateTime?     ReviewedAt       { get; set; }
    public decimal       WithholdingTax   { get; set; } = 0;  // PPh deducted by buyer
    public decimal       NetPayable       { get; set; } = 0;  // TotalAmount - WithholdingTax

    public PurchaseOrder? PurchaseOrder { get; set; }

    public static Invoice Create(
        string invoiceNumber, Guid poId, Guid vendorId,
        decimal amount, decimal taxAmount, Guid? currencyId,
        string? fileUrl, DateTime? dueAt, string? notes)
    {
        return new Invoice
        {
            Id            = UUIDNext.Uuid.NewSequential(),
            InvoiceNumber = invoiceNumber,
            POId          = poId,
            VendorId      = vendorId,
            Amount        = amount,
            TaxAmount     = taxAmount,
            TotalAmount   = amount + taxAmount,
            CurrencyId    = currencyId,
            FileUrl       = fileUrl,
            DueAt         = dueAt,
            Notes         = notes,
            Status        = InvoiceStatus.Submitted,
            SubmittedAt   = DateTime.UtcNow,
        };
    }

    public void StartReview(Guid reviewerId)
    {
        if (Status != InvoiceStatus.Submitted)
            throw new BusinessRuleException("InvoiceReview",
                $"Only submitted invoices can be reviewed. Current status: {Status}");

        Status     = InvoiceStatus.UnderReview;
        ReviewedBy = reviewerId;
        ReviewedAt = DateTime.UtcNow;
    }

    public void Approve(Guid reviewerId)
    {
        if (Status != InvoiceStatus.UnderReview)
            throw new BusinessRuleException("InvoiceApprove",
                $"Only under-review invoices can be approved. Current status: {Status}");

        Status     = InvoiceStatus.Approved;
        ReviewedBy = reviewerId;
        ReviewedAt = DateTime.UtcNow;
    }

    public void ApplyTax(bool isPkp, decimal? pphRate)
    {
        if (isPkp)
            TaxAmount = Math.Round(Amount * 0.11m, 4);

        TotalAmount    = Amount + TaxAmount;
        WithholdingTax = pphRate.HasValue
            ? Math.Round(Amount * (pphRate.Value / 100m), 4)
            : 0;
        NetPayable     = TotalAmount - WithholdingTax;
    }

    public void ConfirmPayment(string paymentReference)
    {
        if (Status != InvoiceStatus.Approved)
            throw new BusinessRuleException("InvoicePayment",
                $"Only approved invoices can be marked as paid. Current status: {Status}");

        Status           = InvoiceStatus.Paid;
        PaidAt           = DateTime.UtcNow;
        PaymentReference = paymentReference;
    }

    public void Reject(Guid reviewerId, string reason)
    {
        if (Status is not (InvoiceStatus.Submitted or InvoiceStatus.UnderReview))
            throw new BusinessRuleException("InvoiceReject",
                $"Invoice cannot be rejected from status: {Status}");

        Status          = InvoiceStatus.Rejected;
        RejectionReason = reason;
        ReviewedBy      = reviewerId;
        ReviewedAt      = DateTime.UtcNow;
    }
}
