using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.ApprovalEngine.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.ApprovalEngine.Infrastructure.Configurations;

public class ApprovalHistoryConfiguration : BaseAuditableEntityConfiguration<ApprovalHistory>
{
    public override void Configure(EntityTypeBuilder<ApprovalHistory> builder)
    {
        base.Configure(builder);

        builder.ToTable("approval_histories");

        builder.Property(e => e.Action)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.ActorName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Reason).HasMaxLength(2000);

        builder.HasIndex(e => e.WorkflowId);

        builder.HasOne(e => e.Workflow)
               .WithMany(w => w.History)
               .HasForeignKey(e => e.WorkflowId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(e => e.Workflow == null || !e.Workflow.IsDeleted);
    }
}
