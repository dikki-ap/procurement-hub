using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.VendorManagement.Infrastructure.Configurations;

public class VendorCapabilityConfiguration : BaseAuditableEntityConfiguration<VendorCapability>
{
    public override void Configure(EntityTypeBuilder<VendorCapability> builder)
    {
        base.Configure(builder);

        builder.ToTable("vendor_capabilities");

        builder.Property(e => e.MinOrderQty).HasColumnType("DECIMAL(18,4)");
        builder.Property(e => e.Notes).HasColumnType("TEXT");

        builder.HasIndex(e => new { e.VendorId, e.MaterialCategoryId }).IsUnique();

        builder.HasQueryFilter(e => e.Vendor == null || !e.Vendor.IsDeleted);
    }
}
