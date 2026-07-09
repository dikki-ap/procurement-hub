using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.VendorManagement.Infrastructure.Configurations;

public class VendorContactConfiguration : BaseAuditableEntityConfiguration<VendorContact>
{
    public override void Configure(EntityTypeBuilder<VendorContact> builder)
    {
        base.Configure(builder);

        builder.ToTable("vendor_contacts");

        builder.Property(e => e.Name).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Position).HasMaxLength(100);
        builder.Property(e => e.Email).HasMaxLength(255);
        builder.Property(e => e.Phone).HasMaxLength(30);

        builder.HasQueryFilter(e => e.Vendor == null || !e.Vendor.IsDeleted);
    }
}
