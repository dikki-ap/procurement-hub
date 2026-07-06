using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.Procurement.Domain.Entities;

public class QuotationItem : BaseAuditableEntity
{
    public Guid    QuotationId { get; set; }
    public Guid    RFQItemId   { get; set; }
    public decimal UnitPrice   { get; set; }
    public decimal Quantity    { get; set; }
    public string? Notes       { get; set; }

    // Navigation
    public VendorQuotation Quotation { get; set; } = null!;
}
