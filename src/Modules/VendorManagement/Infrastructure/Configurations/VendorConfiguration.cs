using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.VendorManagement.Infrastructure.Configurations;

public class VendorConfiguration : BaseSoftDeleteEntityConfiguration<Vendor>
{
    public override void Configure(EntityTypeBuilder<Vendor> builder)
    {
        base.Configure(builder);

        builder.ToTable("vendors");

        builder.Property(e => e.VendorCode).HasMaxLength(20).IsRequired();
        builder.Property(e => e.LegalName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.TradeName).HasMaxLength(255);
        builder.Property(e => e.Npwp).HasMaxLength(30);
        builder.Property(e => e.Siup).HasMaxLength(50);
        builder.Property(e => e.Nib).HasMaxLength(30);
        builder.Property(e => e.Address).HasColumnType("TEXT");
        builder.Property(e => e.City).HasMaxLength(100);
        builder.Property(e => e.Province).HasMaxLength(100);
        builder.Property(e => e.PostalCode).HasMaxLength(10);
        builder.Property(e => e.Country).HasMaxLength(100);
        // Cross-module FK columns — no EF navigation, resolved by application layer
        builder.Property(e => e.DefaultPaymentTermId);
        builder.Property(e => e.DefaultCurrencyId);
        builder.Property(e => e.VendorType).HasConversion<string>().HasMaxLength(30);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Tier).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Score).HasColumnType("DECIMAL(5,2)");
        builder.Property(e => e.BlacklistReason).HasColumnType("TEXT");
        builder.Property(e => e.KeycloakGroupId).HasMaxLength(100);
        builder.Property(e => e.PphRate).HasColumnType("DECIMAL(5,2)");

        builder.HasIndex(e => new { e.CompanyId, e.VendorCode }).IsUnique();

        builder.HasMany(e => e.Contacts)
            .WithOne(e => e.Vendor)
            .HasForeignKey(e => e.VendorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Documents)
            .WithOne(e => e.Vendor)
            .HasForeignKey(e => e.VendorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Capabilities)
            .WithOne(e => e.Vendor)
            .HasForeignKey(e => e.VendorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Scores)
            .WithOne(e => e.Vendor)
            .HasForeignKey(e => e.VendorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Users)
            .WithOne(e => e.Vendor)
            .HasForeignKey(e => e.VendorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.BankAccounts)
            .WithOne(e => e.Vendor)
            .HasForeignKey(e => e.VendorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
