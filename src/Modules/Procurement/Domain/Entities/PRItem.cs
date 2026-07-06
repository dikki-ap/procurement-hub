using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.Procurement.Domain.Entities;

public class PRItem : BaseAuditableEntity
{
    public Guid    PurchaseRequisitionId { get; set; }
    public Guid?   MaterialId            { get; set; }
    public string  ItemDescription       { get; set; } = string.Empty;
    public decimal Quantity              { get; set; }
    public Guid?   UnitOfMeasureId       { get; set; }
    public string? UnitLabel             { get; set; }
    public decimal EstimatedUnitPrice    { get; set; }
    public string? Notes                 { get; set; }

    // Navigation
    public PurchaseRequisition PurchaseRequisition { get; set; } = null!;
}
