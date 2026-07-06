using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.Notifications.Domain;

namespace ProcureHub.Modules.Notifications.Infrastructure.Persistence;

public class InAppNotificationConfiguration : IEntityTypeConfiguration<InAppNotification>
{
    public void Configure(EntityTypeBuilder<InAppNotification> builder)
    {
        builder.ToTable("in_app_notifications");

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).HasColumnType("CHAR(36)").ValueGeneratedNever();

        builder.Property(n => n.UserId).HasColumnType("CHAR(36)").IsRequired();
        builder.Property(n => n.Title).HasMaxLength(200).IsRequired();
        builder.Property(n => n.Message).HasMaxLength(1000).IsRequired();
        builder.Property(n => n.Link).HasMaxLength(500);
        builder.Property(n => n.IsRead).HasDefaultValue(false);
        builder.Property(n => n.CreatedAt).IsRequired();

        builder.HasIndex(n => new { n.UserId, n.CreatedAt });
    }
}
