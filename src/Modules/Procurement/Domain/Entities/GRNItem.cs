using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.Procurement.Domain.Entities;

public class GRNItem : BaseAuditableEntity
{
    public Guid          GRNId         { get; set; }
    public Guid          POItemId      { get; set; }
    public decimal       ReceivedQty   { get; set; }
    public decimal       RejectedQty   { get; set; }
    public QualityStatus QualityStatus { get; set; } = QualityStatus.Accepted;
    public string?       RejectReason  { get; set; }
    public string?       Notes         { get; set; }

    public GoodsReceipt? GoodsReceipt { get; set; }
    public POItem?       POItem       { get; set; }
}
