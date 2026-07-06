using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.ApprovalEngine.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.ApprovalEngine.Infrastructure.Configurations;

public class ApprovalPolicyConfiguration : BaseAuditableEntityConfiguration<ApprovalPolicy>
{
    public override void Configure(EntityTypeBuilder<ApprovalPolicy> builder)
    {
        base.Configure(builder);

        builder.ToTable("approval_policies");

        builder.Property(e => e.ReferenceType).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.MinValue).HasColumnType("DECIMAL(18,4)").IsRequired();
        builder.Property(e => e.MaxValue).HasColumnType("DECIMAL(18,4)");

        builder.HasIndex(e => new { e.CompanyId, e.ReferenceType, e.MinValue }).IsUnique();
        builder.HasIndex(e => e.CompanyId);
    }
}
