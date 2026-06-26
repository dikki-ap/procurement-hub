using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.SharedKernel.Audit;

namespace ProcureHub.SharedKernel.Database.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasColumnType("CHAR(36)")
            .ValueGeneratedNever();

        builder.Property(a => a.EntityType).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Action).IsRequired().HasMaxLength(20);
        builder.Property(a => a.UserEmail).HasMaxLength(256);
        builder.Property(a => a.UserFullName).HasMaxLength(200);
        builder.Property(a => a.IpAddress).HasMaxLength(45);
        builder.Property(a => a.UserAgent).HasMaxLength(500);
        builder.Property(a => a.CorrelationId).HasMaxLength(100);
        builder.Property(a => a.BeforeValues).HasColumnType("TEXT");
        builder.Property(a => a.AfterValues).HasColumnType("TEXT");
        builder.Property(a => a.ChangedColumns).HasColumnType("TEXT");

        builder.HasIndex(a => a.EntityId);
        builder.HasIndex(a => a.EntityType);
        builder.HasIndex(a => a.CreatedAt);
    }
}
