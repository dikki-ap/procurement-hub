using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.Procurement.Infrastructure.Configurations;

public class QuotationItemConfiguration : BaseAuditableEntityConfiguration<QuotationItem>
{
    public override void Configure(EntityTypeBuilder<QuotationItem> builder)
    {
        base.Configure(builder);

        builder.ToTable("quotation_items");

        builder.Property(e => e.UnitPrice).HasColumnType("DECIMAL(18,4)").IsRequired();
        builder.Property(e => e.Quantity).HasColumnType("DECIMAL(18,4)").IsRequired();
        builder.Property(e => e.Notes).HasMaxLength(500);

        builder.HasIndex(e => e.QuotationId);
        builder.HasIndex(e => e.RFQItemId);
    }
}
