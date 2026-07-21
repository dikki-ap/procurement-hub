using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.VendorManagement.Domain.Entities;

public class VendorCapability : BaseAuditableEntity
{
    public Guid     VendorId           { get; set; }
    public Guid     MaterialCategoryId { get; set; }
    public decimal? MinOrderQty        { get; set; }
    public decimal? MaxOrderQty        { get; set; }
    public string?  Uom                { get; set; }
    public int?     LeadTimeDays       { get; set; }
    public DateOnly? EffectiveDate     { get; set; }
    public DateOnly? ExpiryDate        { get; set; }
    public bool     IsExpired          { get; set; } = false;
    public string?  Notes              { get; set; }

    public Vendor? Vendor { get; set; }
}
