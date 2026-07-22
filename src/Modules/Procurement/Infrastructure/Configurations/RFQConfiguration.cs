using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.Procurement.Infrastructure.Configurations;

public class RFQConfiguration : BaseSoftDeleteEntityConfiguration<RFQ>
{
    public override void Configure(EntityTypeBuilder<RFQ> builder)
    {
        base.Configure(builder);

        builder.ToTable("rfqs");

        builder.Property(e => e.RFQNumber).HasMaxLength(30).IsRequired();
        builder.Property(e => e.Title).HasMaxLength(255).IsRequired();
        builder.Property(e => e.BidDeadline).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Notes).HasColumnType("TEXT");
        builder.Property(e => e.Terms).HasColumnType("TEXT");
        builder.Property(e => e.FileKey).HasMaxLength(500);
        builder.Property(e => e.FileName).HasMaxLength(255);

        builder.HasIndex(e => new { e.CompanyId, e.RFQNumber }).IsUnique();
        builder.HasIndex(e => new { e.CompanyId, e.Status });

        builder.HasMany(e => e.Items)
            .WithOne(e => e.RFQ)
            .HasForeignKey(e => e.RFQId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Vendors)
            .WithOne(e => e.RFQ)
            .HasForeignKey(e => e.RFQId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
