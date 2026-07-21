using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.SharedKernel.Database.Configurations;

namespace ProcureHub.Modules.VendorManagement.Infrastructure.Configurations;

public class VendorBankAccountConfiguration : BaseAuditableEntityConfiguration<VendorBankAccount>
{
    public override void Configure(EntityTypeBuilder<VendorBankAccount> builder)
    {
        base.Configure(builder);

        builder.ToTable("vendor_bank_accounts");

        builder.Property(e => e.BankName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.AccountNumber).HasMaxLength(50).IsRequired();
        builder.Property(e => e.AccountName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.BranchName).HasMaxLength(150);
        builder.Property(e => e.Currency).HasMaxLength(10).HasDefaultValue("IDR");
        builder.Property(e => e.Notes).HasColumnType("TEXT");

        builder.HasQueryFilter(e => e.Vendor == null || !e.Vendor.IsDeleted);
    }
}
