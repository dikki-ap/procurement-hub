using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.MasterData.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.MasterData.Infrastructure.Configurations;

public class PaymentTermConfiguration : BaseAuditableEntityConfiguration<PaymentTerm>
{
    public override void Configure(EntityTypeBuilder<PaymentTerm> builder)
    {
        base.Configure(builder);

        builder.ToTable("payment_terms");

        builder.Property(e => e.Code).HasMaxLength(20).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Description).HasColumnType("TEXT");

        builder.HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();
    }
}
