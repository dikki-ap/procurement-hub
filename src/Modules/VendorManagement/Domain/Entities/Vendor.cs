using ProcureHub.Modules.VendorManagement.Domain.Enums;
using ProcureHub.Modules.VendorManagement.Domain.Events;
using ProcureHub.SharedKernel.Domain;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.VendorManagement.Domain.Entities;

public class Vendor : AggregateRoot
{
    public Guid         CompanyId        { get; set; }
    public string       VendorCode       { get; set; } = string.Empty;
    public string       LegalName        { get; set; } = string.Empty;
    public string?      TradeName        { get; set; }
    public string?      Npwp             { get; set; }
    public string?      Siup             { get; set; }
    public string?      Nib              { get; set; }
    public string?      Address          { get; set; }
    public string?      City             { get; set; }
    public string?      Province         { get; set; }
    public string?      PostalCode       { get; set; }
    public string?      Country              { get; set; }
    public Guid?        DefaultPaymentTermId { get; set; }
    public Guid?        DefaultCurrencyId    { get; set; }
    public VendorType   VendorType           { get; set; }
    public VendorStatus Status           { get; set; } = VendorStatus.Pending;
    public VendorTier   Tier             { get; set; } = VendorTier.Bronze;
    public decimal      Score            { get; set; } = 0;
    public bool         IsBlacklisted    { get; set; } = false;
    public string?      BlacklistReason  { get; set; }
    public DateTime?    BlacklistedAt    { get; set; }
    public Guid?        BlacklistedById  { get; set; }
    public DateTime?    ApprovedAt       { get; set; }
    public Guid?        ApprovedById     { get; set; }
    public string?      KeycloakGroupId  { get; set; }

    // Navigation
    public ICollection<VendorContact>     Contacts     { get; set; } = [];
    public ICollection<VendorDocument>    Documents    { get; set; } = [];
    public ICollection<VendorCapability>  Capabilities { get; set; } = [];
    public ICollection<VendorScore>       Scores       { get; set; } = [];
    public ICollection<VendorUser>        Users        { get; set; } = [];
    public ICollection<VendorBankAccount> BankAccounts { get; set; } = [];

    // ── Domain methods ──────────────────────────────────────────────────────

    public void Approve(Guid approvedById)
    {
        if (Status == VendorStatus.Active)
            throw new ConflictException("Vendor", "Status", "already active");

        Status       = VendorStatus.Active;
        ApprovedAt   = DateTime.UtcNow;
        ApprovedById = approvedById;

        AddDomainEvent(new VendorApprovedEvent(Id, LegalName, approvedById));
    }

    public void Suspend()
    {
        if (Status != VendorStatus.Active)
            throw new BusinessRuleException("VendorSuspend", $"Only active vendors can be suspended. Current status: {Status}");

        Status = VendorStatus.Suspended;
        AddDomainEvent(new VendorSuspendedEvent(Id, LegalName));
    }

    public void Blacklist(string reason, Guid blacklistedById)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessRuleException("VendorBlacklist", "Blacklist reason is required.");

        Status          = VendorStatus.Blacklisted;
        IsBlacklisted   = true;
        BlacklistReason = reason;
        BlacklistedAt   = DateTime.UtcNow;
        BlacklistedById = blacklistedById;

        AddDomainEvent(new VendorBlacklistedEvent(Id, LegalName, reason, blacklistedById));
    }

    public void Reinstate()
    {
        if (Status != VendorStatus.Suspended && Status != VendorStatus.Blacklisted)
            throw new BusinessRuleException("VendorReinstate", $"Only suspended or blacklisted vendors can be reinstated. Current status: {Status}");

        Status        = VendorStatus.Active;
        IsBlacklisted = false;
        BlacklistReason = null;

        AddDomainEvent(new VendorReinstatedEvent(Id, LegalName));
    }
}
