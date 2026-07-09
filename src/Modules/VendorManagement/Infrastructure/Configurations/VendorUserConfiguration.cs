using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.VendorManagement.Infrastructure.Configurations;

public class VendorUserConfiguration : BaseAuditableEntityConfiguration<VendorUser>
{
    public override void Configure(EntityTypeBuilder<VendorUser> builder)
    {
        base.Configure(builder);

        builder.ToTable("vendor_users");

        builder.Property(e => e.KeycloakId).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Email).HasMaxLength(255).IsRequired();
        builder.Property(e => e.FullName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Role).HasMaxLength(50).IsRequired();

        builder.HasIndex(e => e.KeycloakId).IsUnique();
        builder.HasIndex(e => new { e.VendorId, e.Email }).IsUnique();

        builder.HasQueryFilter(e => e.Vendor == null || !e.Vendor.IsDeleted);
    }
}
