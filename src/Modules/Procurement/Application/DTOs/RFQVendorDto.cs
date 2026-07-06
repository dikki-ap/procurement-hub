using ProcureHub.Modules.Procurement.Domain.Enums;

namespace ProcureHub.Modules.Procurement.Application.DTOs;

public class RFQVendorDto
{
    public Guid            Id             { get; set; }
    public Guid            VendorId       { get; set; }
    public DateTime        InvitedAt      { get; set; }
    public RFQVendorStatus Status         { get; set; }
    public string?         DeclinedReason { get; set; }
}
