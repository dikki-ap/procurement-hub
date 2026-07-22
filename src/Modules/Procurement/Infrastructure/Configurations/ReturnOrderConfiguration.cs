using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.Procurement.Infrastructure.Configurations;

public class ReturnOrderConfiguration : BaseSoftDeleteEntityConfiguration<ReturnOrder>
{
    public override void Configure(EntityTypeBuilder<ReturnOrder> builder)
    {
        base.Configure(builder);

        builder.ToTable("return_orders");

        builder.Property(e => e.ReturnNumber).HasMaxLength(30).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Reason).HasMaxLength(500);
        builder.Property(e => e.Notes).HasColumnType("TEXT");
        builder.Property(e => e.VendorId).HasColumnType("CHAR(36)").IsRequired();
        builder.Property(e => e.POId).HasColumnType("CHAR(36)").IsRequired();

        builder.HasIndex(e => e.ReturnNumber).IsUnique();
        builder.HasIndex(e => e.GRNId);
        builder.HasIndex(e => e.VendorId);

        builder.HasMany(e => e.Items)
            .WithOne(e => e.ReturnOrder)
            .HasForeignKey(e => e.ReturnOrderId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
