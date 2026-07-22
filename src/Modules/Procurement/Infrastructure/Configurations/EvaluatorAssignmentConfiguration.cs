using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.Procurement.Infrastructure.Configurations;

public class EvaluatorAssignmentConfiguration : BaseAuditableEntityConfiguration<EvaluatorAssignment>
{
    public override void Configure(EntityTypeBuilder<EvaluatorAssignment> builder)
    {
        base.Configure(builder);

        builder.ToTable("evaluator_assignments");

        // Match the parent's soft-delete filter so direct EvaluatorAssignment queries
        // exclude rows whose BidEvaluation has been soft-deleted.
        builder.HasQueryFilter(e => !e.Evaluation.IsDeleted);

        builder.Property(e => e.AssignedUserName).HasMaxLength(200).IsRequired();

        builder.HasIndex(e => new { e.BidEvaluationId, e.AssignedUserId }).IsUnique();
        builder.HasIndex(e => e.BidEvaluationId);

        builder.HasMany(e => e.Scores)
            .WithOne(s => s.Assignment)
            .HasForeignKey(s => s.EvaluatorAssignmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
