using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.ApprovalEngine.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.ApprovalEngine.Infrastructure.Configurations;

public class ApproverAssignmentConfiguration : BaseAuditableEntityConfiguration<ApproverAssignment>
{
    public override void Configure(EntityTypeBuilder<ApproverAssignment> builder)
    {
        base.Configure(builder);

        builder.ToTable("approver_assignments");

        builder.Property(e => e.AssignedUserName).HasMaxLength(200).IsRequired();

        builder.HasIndex(e => e.WorkflowId);
        builder.HasIndex(e => new { e.WorkflowId, e.Level });

        builder.HasOne(e => e.Workflow)
               .WithMany(w => w.Assignments)
               .HasForeignKey(e => e.WorkflowId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(e => e.Workflow == null || !e.Workflow.IsDeleted);
    }
}
