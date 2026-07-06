using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.ApprovalEngine.Domain.Entities;
using ProcureHub.Modules.ApprovalEngine.Domain.Enums;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.ApprovalEngine.Infrastructure.Configurations;

public class ApprovalWorkflowConfiguration : BaseSoftDeleteEntityConfiguration<ApprovalWorkflow>
{
    public override void Configure(EntityTypeBuilder<ApprovalWorkflow> builder)
    {
        base.Configure(builder);

        builder.ToTable("approval_workflows");

        builder.Property(e => e.ReferenceType).HasMaxLength(50).IsRequired();
        builder.Property(e => e.ReferenceNumber).HasMaxLength(50).IsRequired();
        builder.Property(e => e.ReferenceTitle).HasMaxLength(300).IsRequired();
        builder.Property(e => e.TotalValue).HasColumnType("DECIMAL(18,4)").IsRequired();
        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(e => new { e.ReferenceType, e.ReferenceId });
        builder.HasIndex(e => e.CompanyId);
        builder.HasIndex(e => e.Status);

        builder.HasMany(e => e.History)
               .WithOne(h => h.Workflow)
               .HasForeignKey(h => h.WorkflowId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Assignments)
               .WithOne(a => a.Workflow)
               .HasForeignKey(a => a.WorkflowId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
