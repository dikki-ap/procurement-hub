using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.SharedKernel.Database.Configurations;

/// <summary>
/// Base EF Core configuration for all entities extending BaseAuditableEntity.
/// Explicitly configures CreatedBy/UpdatedBy FK relationships to avoid EF ambiguity
/// when multiple relationships exist between the same two entity types.
/// All module entity configurations should extend this or BaseSoftDeleteEntityConfiguration.
/// </summary>
public abstract class BaseAuditableEntityConfiguration<T> : IEntityTypeConfiguration<T>
    where T : BaseAuditableEntity
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("CHAR(36)")
            .ValueGeneratedNever();

        builder.HasOne(e => e.CreatedBy)
            .WithMany()
            .HasForeignKey(e => e.CreatedById)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.UpdatedBy)
            .WithMany()
            .HasForeignKey(e => e.UpdatedById)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
