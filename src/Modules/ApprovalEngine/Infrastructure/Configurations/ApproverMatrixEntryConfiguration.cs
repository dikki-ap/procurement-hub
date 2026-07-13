using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.ApprovalEngine.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.ApprovalEngine.Infrastructure.Configurations;

public class ApproverMatrixEntryConfiguration : BaseAuditableEntityConfiguration<ApproverMatrixEntry>
{
    public override void Configure(EntityTypeBuilder<ApproverMatrixEntry> builder)
    {
        base.Configure(builder);

        builder.ToTable("approver_matrix_entries");

        builder.Property(e => e.ReferenceType).HasMaxLength(20).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Position).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Email).HasMaxLength(256).IsRequired();

        builder.HasIndex(e => new { e.CompanyId, e.ReferenceType, e.Level, e.Email }).IsUnique();
        builder.HasIndex(e => e.CompanyId);
    }
}
