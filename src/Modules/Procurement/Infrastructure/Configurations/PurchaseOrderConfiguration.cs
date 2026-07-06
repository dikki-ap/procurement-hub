using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.Procurement.Infrastructure.Configurations;

public class PurchaseOrderConfiguration : BaseSoftDeleteEntityConfiguration<PurchaseOrder>
{
    public override void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        base.Configure(builder);

        builder.ToTable("purchase_orders");

        builder.Property(e => e.PONumber).HasMaxLength(30).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.TotalAmount).HasColumnType("DECIMAL(18,4)");
        builder.Property(e => e.FileUrl).HasColumnType("TEXT");
        builder.Property(e => e.Notes).HasColumnType("TEXT");
        builder.Property(e => e.TermsConditions).HasColumnType("TEXT");
        builder.Property(e => e.CancelledReason).HasColumnType("TEXT");

        builder.HasIndex(e => new { e.CompanyId, e.PONumber }).IsUnique();
        builder.HasIndex(e => new { e.CompanyId, e.Status });
        builder.HasIndex(e => e.VendorId);

        builder.HasMany(e => e.Items)
            .WithOne(e => e.PurchaseOrder)
            .HasForeignKey(e => e.POId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
