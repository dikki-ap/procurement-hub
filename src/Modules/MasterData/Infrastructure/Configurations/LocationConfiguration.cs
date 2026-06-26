using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.MasterData.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.MasterData.Infrastructure.Configurations;

public class LocationConfiguration : BaseAuditableEntityConfiguration<Location>
{
    public override void Configure(EntityTypeBuilder<Location> builder)
    {
        base.Configure(builder);

        builder.ToTable("locations");

        builder.Property(e => e.Name).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Type).HasMaxLength(30).IsRequired();
        builder.Property(e => e.Address).HasColumnType("TEXT");
        builder.Property(e => e.City).HasMaxLength(100);
        builder.Property(e => e.Province).HasMaxLength(100);
        builder.Property(e => e.Country).HasMaxLength(100).HasDefaultValue("Indonesia");
    }
}
