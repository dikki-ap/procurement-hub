using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.VendorManagement.Domain.Entities;

namespace ProcureHub.Modules.VendorManagement.Infrastructure.Configurations;

public class VendorScoreConfiguration : IEntityTypeConfiguration<VendorScore>
{
    public void Configure(EntityTypeBuilder<VendorScore> builder)
    {
        builder.ToTable("vendor_scores");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("CHAR(36)")
            .ValueGeneratedNever();

        builder.Property(e => e.DeliveryScore).HasColumnType("DECIMAL(5,2)");
        builder.Property(e => e.QualityScore).HasColumnType("DECIMAL(5,2)");
        builder.Property(e => e.PriceScore).HasColumnType("DECIMAL(5,2)");
        builder.Property(e => e.ResponseScore).HasColumnType("DECIMAL(5,2)");
        builder.Property(e => e.DocScore).HasColumnType("DECIMAL(5,2)");
        builder.Property(e => e.TotalScore).HasColumnType("DECIMAL(5,2)");
        builder.Property(e => e.Tier).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Notes).HasColumnType("TEXT");

        builder.HasIndex(e => new { e.VendorId, e.PeriodYear, e.PeriodQuarter }).IsUnique();

        builder.HasQueryFilter(e => e.Vendor == null || !e.Vendor.IsDeleted);
    }
}
