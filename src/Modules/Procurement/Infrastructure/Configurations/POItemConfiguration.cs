using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.Procurement.Infrastructure.Configurations;

public class POItemConfiguration : BaseAuditableEntityConfiguration<POItem>
{
    public override void Configure(EntityTypeBuilder<POItem> builder)
    {
        base.Configure(builder);

        builder.ToTable("po_items");

        builder.Property(e => e.Description).HasColumnType("TEXT").IsRequired();
        builder.Property(e => e.Quantity).HasColumnType("DECIMAL(18,4)").IsRequired();
        builder.Property(e => e.UnitPrice).HasColumnType("DECIMAL(18,4)").IsRequired();
        builder.Property(e => e.TotalPrice).HasColumnType("DECIMAL(18,4)").IsRequired();
        builder.Property(e => e.ReceivedQty).HasColumnType("DECIMAL(18,4)");

        builder.HasIndex(e => e.POId);

        builder.HasQueryFilter(e => e.PurchaseOrder == null || !e.PurchaseOrder.IsDeleted);
    }
}
