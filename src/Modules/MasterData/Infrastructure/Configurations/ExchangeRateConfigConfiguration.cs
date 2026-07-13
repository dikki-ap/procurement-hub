using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.MasterData.Domain.Entities;

namespace ProcureHub.Modules.MasterData.Infrastructure.Configurations;

public class ExchangeRateConfigConfiguration : IEntityTypeConfiguration<ExchangeRateConfig>
{
    public void Configure(EntityTypeBuilder<ExchangeRateConfig> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("CHAR(36)")
            .ValueGeneratedNever();

        builder.ToTable("exchange_rate_config");
        builder.Property(e => e.AutoSync).IsRequired();
    }
}
