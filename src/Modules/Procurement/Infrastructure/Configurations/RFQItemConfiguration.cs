using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.Procurement.Infrastructure.Configurations;

public class RFQItemConfiguration : BaseAuditableEntityConfiguration<RFQItem>
{
    public override void Configure(EntityTypeBuilder<RFQItem> builder)
    {
        base.Configure(builder);

        builder.ToTable("rfq_items");

        builder.Property(e => e.ItemDescription).HasMaxLength(500).IsRequired();
        builder.Property(e => e.Quantity).HasColumnType("DECIMAL(18,4)").IsRequired();
        builder.Property(e => e.UnitLabel).HasMaxLength(30);

        builder.HasIndex(e => e.RFQId);

        builder.HasQueryFilter(e => !e.RFQ.IsDeleted);
    }
}
