using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.Procurement.Infrastructure.Configurations;

public class GRNItemConfiguration : BaseAuditableEntityConfiguration<GRNItem>
{
    public override void Configure(EntityTypeBuilder<GRNItem> builder)
    {
        base.Configure(builder);

        builder.ToTable("grn_items");

        builder.Property(e => e.ReceivedQty).HasColumnType("DECIMAL(18,4)").IsRequired();
        builder.Property(e => e.RejectedQty).HasColumnType("DECIMAL(18,4)");
        builder.Property(e => e.QualityStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.RejectReason).HasColumnType("TEXT");
        builder.Property(e => e.Notes).HasColumnType("TEXT");

        builder.HasIndex(e => e.GRNId);
        builder.HasIndex(e => e.POItemId);
    }
}
