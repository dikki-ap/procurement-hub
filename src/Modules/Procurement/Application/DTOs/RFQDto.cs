using ProcureHub.Modules.Procurement.Domain.Enums;

namespace ProcureHub.Modules.Procurement.Application.DTOs;

public class RFQDto
{
    public Guid               Id                     { get; set; }
    public string             RFQNumber              { get; set; } = string.Empty;
    public string             Title                  { get; set; } = string.Empty;
    public Guid?              PurchaseRequisitionId  { get; set; }
    public DateTime           BidDeadline            { get; set; }
    public DateTime?          DeliveryDate           { get; set; }
    public RFQStatus          Status                 { get; set; }
    public string?            Notes                  { get; set; }
    public string?            Terms                  { get; set; }
    public DateTime           CreatedAt              { get; set; }
    public DateTime           UpdatedAt              { get; set; }
    public List<RFQItemDto>   Items                  { get; set; } = [];
    public List<RFQVendorDto> Vendors                { get; set; } = [];
}
