using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.Procurement.Infrastructure.Configurations;

public class EvaluationScoreConfiguration : BaseAuditableEntityConfiguration<EvaluationScore>
{
    public override void Configure(EntityTypeBuilder<EvaluationScore> builder)
    {
        base.Configure(builder);

        builder.ToTable("evaluation_scores");

        builder.Property(e => e.PriceScore).HasColumnType("DECIMAL(5,2)").IsRequired();
        builder.Property(e => e.QualityScore).HasColumnType("DECIMAL(5,2)").IsRequired();
        builder.Property(e => e.DeliveryScore).HasColumnType("DECIMAL(5,2)").IsRequired();
        builder.Property(e => e.WeightedTotal).HasColumnType("DECIMAL(5,2)").IsRequired();

        builder.HasIndex(e => e.BidEvaluationId);
        builder.HasIndex(e => e.QuotationId);
    }
}
