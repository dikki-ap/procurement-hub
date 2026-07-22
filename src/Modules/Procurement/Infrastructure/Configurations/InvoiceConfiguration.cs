using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.Procurement.Infrastructure.Configurations;

public class InvoiceConfiguration : BaseSoftDeleteEntityConfiguration<Invoice>
{
    public override void Configure(EntityTypeBuilder<Invoice> builder)
    {
        base.Configure(builder);

        builder.ToTable("invoices");

        builder.Property(e => e.InvoiceNumber).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Amount).HasColumnType("DECIMAL(18,4)").IsRequired();
        builder.Property(e => e.TaxAmount).HasColumnType("DECIMAL(18,4)");
        builder.Property(e => e.TotalAmount).HasColumnType("DECIMAL(18,4)").IsRequired();
        builder.Property(e => e.WithholdingTax).HasColumnType("DECIMAL(18,4)");
        builder.Property(e => e.NetPayable).HasColumnType("DECIMAL(18,4)");
        builder.Property(e => e.FileUrl).HasColumnType("TEXT");
        builder.Property(e => e.PaymentReference).HasMaxLength(100);
        builder.Property(e => e.Notes).HasColumnType("TEXT");
        builder.Property(e => e.RejectionReason).HasColumnType("TEXT");

        builder.HasIndex(e => e.InvoiceNumber).IsUnique();
        builder.HasIndex(e => e.POId);
        builder.HasIndex(e => e.VendorId);
        builder.HasIndex(e => e.Status);
    }
}
