using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.Procurement.Infrastructure.Configurations;

public class VendorQuotationConfiguration : BaseAuditableEntityConfiguration<VendorQuotation>
{
    public override void Configure(EntityTypeBuilder<VendorQuotation> builder)
    {
        base.Configure(builder);

        builder.ToTable("vendor_quotations");

        builder.Property(e => e.Status).IsRequired();
        builder.Property(e => e.TotalPrice).HasColumnType("DECIMAL(18,4)").IsRequired();
        builder.Property(e => e.Notes).HasMaxLength(2000);
        builder.Property(e => e.FileKey).HasMaxLength(500);
        builder.Property(e => e.FileName).HasMaxLength(255);

        builder.HasIndex(e => new { e.RFQId, e.VendorId }).IsUnique();
        builder.HasIndex(e => e.VendorId);

        builder.HasMany(e => e.Items)
               .WithOne(i => i.Quotation)
               .HasForeignKey(i => i.QuotationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
