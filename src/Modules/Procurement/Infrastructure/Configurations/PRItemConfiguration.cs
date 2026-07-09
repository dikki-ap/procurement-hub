using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.Procurement.Infrastructure.Configurations;

public class PRItemConfiguration : BaseAuditableEntityConfiguration<PRItem>
{
    public override void Configure(EntityTypeBuilder<PRItem> builder)
    {
        base.Configure(builder);

        builder.ToTable("pr_items");

        builder.Property(e => e.ItemDescription).HasMaxLength(500).IsRequired();
        builder.Property(e => e.Quantity).HasColumnType("DECIMAL(18,4)").IsRequired();
        builder.Property(e => e.UnitLabel).HasMaxLength(30);
        builder.Property(e => e.EstimatedUnitPrice).HasColumnType("DECIMAL(18,4)");
        builder.Property(e => e.Notes).HasColumnType("TEXT");

        builder.HasIndex(e => e.PurchaseRequisitionId);

        builder.HasQueryFilter(e => !e.PurchaseRequisition.IsDeleted);
    }
}
