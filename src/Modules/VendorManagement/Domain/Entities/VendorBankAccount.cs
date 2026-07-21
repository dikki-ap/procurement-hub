using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.VendorManagement.Domain.Entities;

public class VendorBankAccount : BaseAuditableEntity
{
    public Guid    VendorId      { get; set; }
    public string  BankName      { get; set; } = string.Empty;
    public string  AccountNumber { get; set; } = string.Empty;
    public string  AccountName   { get; set; } = string.Empty;
    public string? BranchName    { get; set; }
    public string  Currency      { get; set; } = "IDR";
    public bool    IsDefault     { get; set; } = false;
    public string? Notes         { get; set; }

    public Vendor? Vendor { get; set; }
}
