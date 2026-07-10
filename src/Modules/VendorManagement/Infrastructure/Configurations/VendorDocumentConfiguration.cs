using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.VendorManagement.Infrastructure.Configurations;

public class VendorDocumentConfiguration : BaseSoftDeleteEntityConfiguration<VendorDocument>
{
    public override void Configure(EntityTypeBuilder<VendorDocument> builder)
    {
        base.Configure(builder);

        builder.ToTable("vendor_documents");

        builder.Property(e => e.DocumentType).HasMaxLength(100).IsRequired();
        builder.Property(e => e.DocumentNumber).HasMaxLength(100);
        builder.Property(e => e.FileUrl).HasMaxLength(1000).IsRequired();
        builder.Property(e => e.FileName).HasMaxLength(255);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Notes).HasColumnType("TEXT");
    }
}
