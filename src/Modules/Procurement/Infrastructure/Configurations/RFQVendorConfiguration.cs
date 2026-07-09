using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.Procurement.Infrastructure.Configurations;

public class RFQVendorConfiguration : BaseAuditableEntityConfiguration<RFQVendor>
{
    public override void Configure(EntityTypeBuilder<RFQVendor> builder)
    {
        base.Configure(builder);

        builder.ToTable("rfq_vendors");

        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.DeclinedReason).HasColumnType("TEXT");
        builder.Property(e => e.InvitedAt).IsRequired();

        builder.HasIndex(e => new { e.RFQId, e.VendorId }).IsUnique();
        builder.HasIndex(e => e.VendorId);

        builder.HasQueryFilter(e => !e.RFQ.IsDeleted);
    }
}
