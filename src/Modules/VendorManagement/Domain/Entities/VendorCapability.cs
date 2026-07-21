using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.VendorManagement.Domain.Entities;

public class VendorCapability : BaseAuditableEntity
{
    public Guid     VendorId           { get; set; }
    public Guid     MaterialCategoryId { get; set; }
    public decimal? MinOrderQty        { get; set; }
    public string?  Uom                { get; set; }
    public int?     LeadTimeDays       { get; set; }
    public string?  Notes              { get; set; }

    public Vendor? Vendor { get; set; }
}
