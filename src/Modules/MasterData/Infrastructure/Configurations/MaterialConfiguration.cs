using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.MasterData.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.MasterData.Infrastructure.Configurations;

public class MaterialConfiguration : BaseSoftDeleteEntityConfiguration<Material>
{
    public override void Configure(EntityTypeBuilder<Material> builder)
    {
        base.Configure(builder);

        builder.ToTable("materials");

        builder.Property(e => e.Code).HasMaxLength(30).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Description).HasColumnType("TEXT");
        builder.Property(e => e.EstimatedPrice).HasColumnType("DECIMAL(18,4)");

        builder.HasIndex(e => e.Code).IsUnique();

        builder.HasOne(e => e.Category)
            .WithMany(e => e.Materials)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Uom)
            .WithMany(e => e.Materials)
            .HasForeignKey(e => e.UomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Currency)
            .WithMany(e => e.Materials)
            .HasForeignKey(e => e.CurrencyId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
