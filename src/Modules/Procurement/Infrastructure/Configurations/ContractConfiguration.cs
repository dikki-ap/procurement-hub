using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.Procurement.Infrastructure.Configurations;

public class ContractConfiguration : BaseSoftDeleteEntityConfiguration<Contract>
{
    public override void Configure(EntityTypeBuilder<Contract> builder)
    {
        base.Configure(builder);

        builder.ToTable("contracts");

        builder.Property(e => e.ContractNumber).HasMaxLength(30).IsRequired();
        builder.Property(e => e.Title).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.FileUrl).HasColumnType("TEXT");
        builder.Property(e => e.Value).HasColumnType("DECIMAL(18,4)");
        builder.Property(e => e.Notes).HasColumnType("TEXT");

        builder.HasIndex(e => e.ContractNumber).IsUnique();
        builder.HasIndex(e => e.VendorId);
    }
}
