using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.Procurement.Domain.Entities;

public class ReturnOrderItem : BaseEntity
{
    public Guid    ReturnOrderId   { get; set; }
    public Guid?   POItemId        { get; set; }
    public string  ItemDescription { get; set; } = string.Empty;
    public decimal Quantity        { get; set; }
    public string  Uom             { get; set; } = string.Empty;
    public string? ReturnReason    { get; set; }

    public ReturnOrder? ReturnOrder { get; set; }
}
