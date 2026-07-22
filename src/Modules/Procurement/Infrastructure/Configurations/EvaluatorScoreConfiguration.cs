using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.Procurement.Domain.Entities;

namespace ProcureHub.Modules.Procurement.Infrastructure.Configurations;

public class EvaluatorScoreConfiguration : IEntityTypeConfiguration<EvaluatorScore>
{
    public void Configure(EntityTypeBuilder<EvaluatorScore> builder)
    {
        builder.ToTable("evaluator_scores");

        // Propagate the soft-delete filter from the grandparent BidEvaluation.
        builder.HasQueryFilter(e => !e.Assignment.Evaluation.IsDeleted);

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnType("CHAR(36)").ValueGeneratedNever();

        builder.Property(e => e.QualityScore).HasColumnType("DECIMAL(5,2)").IsRequired();
        builder.Property(e => e.DeliveryScore).HasColumnType("DECIMAL(5,2)").IsRequired();

        builder.HasIndex(e => new { e.EvaluatorAssignmentId, e.QuotationId }).IsUnique();
        builder.HasIndex(e => e.EvaluatorAssignmentId);
    }
}
