using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.Procurement.Infrastructure.Configurations;

public class PurchaseRequisitionConfiguration : BaseSoftDeleteEntityConfiguration<PurchaseRequisition>
{
    public override void Configure(EntityTypeBuilder<PurchaseRequisition> builder)
    {
        base.Configure(builder);

        builder.ToTable("purchase_requisitions");

        builder.Property(e => e.PRNumber).HasMaxLength(30).IsRequired();
        builder.Property(e => e.Title).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Description).HasColumnType("TEXT");
        builder.Property(e => e.Department).HasMaxLength(100).IsRequired();
        builder.Property(e => e.DeliveryLocation).HasMaxLength(255);
        builder.Property(e => e.RequiredDate).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.TotalEstimatedValue).HasColumnType("DECIMAL(18,4)");
        builder.Property(e => e.Notes).HasColumnType("TEXT");

        builder.HasIndex(e => new { e.CompanyId, e.PRNumber }).IsUnique();
        builder.HasIndex(e => new { e.CompanyId, e.Status });

        builder.HasMany(e => e.Items)
            .WithOne(e => e.PurchaseRequisition)
            .HasForeignKey(e => e.PurchaseRequisitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
