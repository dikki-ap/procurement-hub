using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.Procurement.Domain.Entities;

public class POItem : BaseAuditableEntity
{
    public Guid     POId        { get; set; }
    public Guid?    MaterialId  { get; set; }
    public string   Description { get; set; } = string.Empty;
    public decimal  Quantity    { get; set; }
    public Guid?    UomId       { get; set; }
    public decimal  UnitPrice   { get; set; }
    public decimal  TotalPrice  { get; set; }
    public decimal  ReceivedQty { get; set; }

    public PurchaseOrder? PurchaseOrder { get; set; }

    public void UpdateReceivedQty(decimal qty) => ReceivedQty += qty;
}
