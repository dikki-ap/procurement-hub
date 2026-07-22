using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.Procurement.Infrastructure.Configurations;

public class BidEvaluationConfiguration : BaseAuditableEntityConfiguration<BidEvaluation>
{
    public override void Configure(EntityTypeBuilder<BidEvaluation> builder)
    {
        base.Configure(builder);

        builder.ToTable("bid_evaluations");

        builder.Property(e => e.PriceWeight).HasColumnType("DECIMAL(5,2)").IsRequired();
        builder.Property(e => e.QualityWeight).HasColumnType("DECIMAL(5,2)").IsRequired();
        builder.Property(e => e.DeliveryWeight).HasColumnType("DECIMAL(5,2)").IsRequired();
        builder.Property(e => e.Status).IsRequired();

        builder.HasIndex(e => e.RFQId).IsUnique();

        builder.HasMany(e => e.Scores)
               .WithOne(s => s.Evaluation)
               .HasForeignKey(s => s.BidEvaluationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Evaluators)
               .WithOne(a => a.Evaluation)
               .HasForeignKey(a => a.BidEvaluationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
