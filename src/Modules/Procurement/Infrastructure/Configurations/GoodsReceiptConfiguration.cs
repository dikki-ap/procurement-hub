using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.Procurement.Infrastructure.Configurations;

public class GoodsReceiptConfiguration : BaseSoftDeleteEntityConfiguration<GoodsReceipt>
{
    public override void Configure(EntityTypeBuilder<GoodsReceipt> builder)
    {
        base.Configure(builder);

        builder.ToTable("goods_receipts");

        builder.Property(e => e.GRNNumber).HasMaxLength(30).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.DeliveryNoteNo).HasMaxLength(100);
        builder.Property(e => e.Notes).HasColumnType("TEXT");

        builder.HasIndex(e => e.GRNNumber).IsUnique();
        builder.HasIndex(e => e.POId);

        builder.HasMany(e => e.Items)
            .WithOne(e => e.GoodsReceipt)
            .HasForeignKey(e => e.GRNId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
