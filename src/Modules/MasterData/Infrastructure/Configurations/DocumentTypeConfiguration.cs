using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.MasterData.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.MasterData.Infrastructure.Configurations;

public class DocumentTypeConfiguration : BaseAuditableEntityConfiguration<DocumentType>
{
    public override void Configure(EntityTypeBuilder<DocumentType> builder)
    {
        base.Configure(builder);

        builder.ToTable("document_types");

        builder.Property(e => e.Name).HasMaxLength(100).IsRequired();
        builder.Property(e => e.AllowedExtensions).HasMaxLength(200);
        builder.Property(e => e.MaxFileSizeMb).HasDefaultValue(10);

        builder.HasIndex(e => e.Name).IsUnique();
    }
}
