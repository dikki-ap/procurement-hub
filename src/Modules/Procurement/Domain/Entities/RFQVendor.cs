using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.Procurement.Domain.Entities;

public class RFQVendor : BaseAuditableEntity
{
    public Guid            RFQId          { get; set; }
    public Guid            VendorId       { get; set; }
    public DateTime        InvitedAt      { get; set; } = DateTime.UtcNow;
    public RFQVendorStatus Status         { get; set; } = RFQVendorStatus.Invited;
    public string?         DeclinedReason { get; set; }

    // Navigation
    public RFQ RFQ { get; set; } = null!;
}
