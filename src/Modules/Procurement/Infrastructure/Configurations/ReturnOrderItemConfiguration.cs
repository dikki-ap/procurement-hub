using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.Procurement.Domain.Entities;

namespace ProcureHub.Modules.Procurement.Infrastructure.Configurations;

public class ReturnOrderItemConfiguration : IEntityTypeConfiguration<ReturnOrderItem>
{
    public void Configure(EntityTypeBuilder<ReturnOrderItem> builder)
    {
        builder.ToTable("return_order_items");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnType("CHAR(36)").ValueGeneratedNever();

        builder.Property(e => e.ItemDescription).HasMaxLength(500).IsRequired();
        builder.Property(e => e.Uom).HasMaxLength(30).IsRequired();
        builder.Property(e => e.ReturnReason).HasMaxLength(500);
        builder.Property(e => e.Quantity).HasPrecision(18, 4);

        builder.HasIndex(e => e.ReturnOrderId);
    }
}
