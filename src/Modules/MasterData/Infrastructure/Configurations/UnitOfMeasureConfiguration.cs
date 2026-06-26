using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.MasterData.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.MasterData.Infrastructure.Configurations;

public class UnitOfMeasureConfiguration : BaseAuditableEntityConfiguration<UnitOfMeasure>
{
    public override void Configure(EntityTypeBuilder<UnitOfMeasure> builder)
    {
        base.Configure(builder);

        builder.ToTable("unit_of_measures");

        builder.Property(e => e.Code).HasMaxLength(10).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(50).IsRequired();

        builder.HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();
    }
}
