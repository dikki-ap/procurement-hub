using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.SharedKernel.Database.Configurations;

/// <summary>
/// Base EF Core configuration for all entities extending BaseSoftDeleteEntity.
/// Adds DeletedBy FK on top of the auditable base configuration.
/// </summary>
public abstract class BaseSoftDeleteEntityConfiguration<T> : BaseAuditableEntityConfiguration<T>
    where T : BaseSoftDeleteEntity
{
    public override void Configure(EntityTypeBuilder<T> builder)
    {
        base.Configure(builder);

        builder.HasOne(e => e.DeletedBy)
            .WithMany()
            .HasForeignKey(e => e.DeletedById)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
