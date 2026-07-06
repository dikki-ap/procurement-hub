using ProcureHub.Modules.Procurement.Domain.Enums;

namespace ProcureHub.Modules.Procurement.Application.DTOs;

public class RFQListDto
{
    public Guid      Id                    { get; set; }
    public string    RFQNumber             { get; set; } = string.Empty;
    public string    Title                 { get; set; } = string.Empty;
    public Guid?     PurchaseRequisitionId { get; set; }
    public DateTime  BidDeadline           { get; set; }
    public RFQStatus Status                { get; set; }
    public int       ItemCount             { get; set; }
    public int       VendorCount           { get; set; }
    public DateTime  CreatedAt             { get; set; }
}
