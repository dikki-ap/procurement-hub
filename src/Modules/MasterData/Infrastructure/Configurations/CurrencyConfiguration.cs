using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.MasterData.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.MasterData.Infrastructure.Configurations;

public class CurrencyConfiguration : BaseAuditableEntityConfiguration<Currency>
{
    public override void Configure(EntityTypeBuilder<Currency> builder)
    {
        base.Configure(builder);

        builder.ToTable("currencies");

        builder.Property(e => e.Code).HasMaxLength(5).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Symbol).HasMaxLength(5);
        builder.Property(e => e.ExchangeRate).HasColumnType("DECIMAL(18,6)");

        builder.HasIndex(e => e.Code).IsUnique();
    }
}
