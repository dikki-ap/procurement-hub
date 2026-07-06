using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.Procurement.Domain.Entities;

public class RFQItem : BaseAuditableEntity
{
    public Guid    RFQId           { get; set; }
    public Guid?   PRItemId        { get; set; }
    public Guid?   MaterialId      { get; set; }
    public string  ItemDescription { get; set; } = string.Empty;
    public decimal Quantity        { get; set; }
    public Guid?   UnitOfMeasureId { get; set; }
    public string? UnitLabel       { get; set; }

    // Navigation
    public RFQ RFQ { get; set; } = null!;
}
