using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.MasterData.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.MasterData.Infrastructure.Configurations;

public class MaterialCategoryConfiguration : BaseAuditableEntityConfiguration<MaterialCategory>
{
    public override void Configure(EntityTypeBuilder<MaterialCategory> builder)
    {
        base.Configure(builder);

        builder.ToTable("material_categories");

        builder.Property(e => e.Name).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Code).HasMaxLength(20).IsRequired();

        builder.HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();

        builder.HasOne(e => e.Parent)
            .WithMany(e => e.Children)
            .HasForeignKey(e => e.ParentId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
