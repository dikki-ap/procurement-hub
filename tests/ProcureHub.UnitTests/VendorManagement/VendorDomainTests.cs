using Moq;
using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.Modules.VendorManagement.Domain.Enums;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.UnitTests.VendorManagement;

public class VendorDomainTests
{
    private static Vendor CreatePendingVendor() => new()
    {
        CompanyId  = Guid.NewGuid(),
        VendorCode = "VND-2026-000001",
        LegalName  = "PT Test Vendor",
        VendorType = VendorType.Manufacturer,
        Status     = VendorStatus.Pending,
    };

    private static Vendor CreateActiveVendor()
    {
        var v = CreatePendingVendor();
        v.Approve(Guid.NewGuid());
        return v;
    }

    // ── Approve ──────────────────────────────────────────────────────────────

    [Fact]
    public void Approve_FromPending_ShouldSetStatusActive()
    {
        var vendor = CreatePendingVendor();
        var approvedById = Guid.NewGuid();

        vendor.Approve(approvedById);

        vendor.Status.Should().Be(VendorStatus.Active);
        vendor.ApprovedAt.Should().NotBeNull();
        vendor.ApprovedById.Should().Be(approvedById);
    }

    [Fact]
    public void Approve_WhenAlreadyActive_ShouldThrowConflictException()
    {
        var vendor = CreateActiveVendor();

        var act = () => vendor.Approve(Guid.NewGuid());

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void Approve_ShouldRaiseVendorApprovedEvent()
    {
        var vendor = CreatePendingVendor();

        vendor.Approve(Guid.NewGuid());

        vendor.DomainEvents.Should().ContainSingle(e =>
            e.GetType().Name == "VendorApprovedEvent");
    }

    // ── Suspend ──────────────────────────────────────────────────────────────

    [Fact]
    public void Suspend_WhenActive_ShouldSetStatusSuspended()
    {
        var vendor = CreateActiveVendor();

        vendor.Suspend();

        vendor.Status.Should().Be(VendorStatus.Suspended);
    }

    [Fact]
    public void Suspend_WhenPending_ShouldThrowBusinessRuleException()
    {
        var vendor = CreatePendingVendor();

        var act = () => vendor.Suspend();

        act.Should().Throw<BusinessRuleException>()
            .Which.RuleName.Should().Be("VendorSuspend");
    }

    [Fact]
    public void Suspend_WhenBlacklisted_ShouldThrowBusinessRuleException()
    {
        var vendor = CreateActiveVendor();
        vendor.Blacklist("Fraud detected", Guid.NewGuid());

        var act = () => vendor.Suspend();

        act.Should().Throw<BusinessRuleException>();
    }

    // ── Blacklist ─────────────────────────────────────────────────────────────

    [Fact]
    public void Blacklist_WithValidReason_ShouldSetBlacklistedState()
    {
        var vendor          = CreateActiveVendor();
        var blacklistedById = Guid.NewGuid();

        vendor.Blacklist("Fraudulent activity detected", blacklistedById);

        vendor.Status.Should().Be(VendorStatus.Blacklisted);
        vendor.IsBlacklisted.Should().BeTrue();
        vendor.BlacklistReason.Should().Be("Fraudulent activity detected");
        vendor.BlacklistedById.Should().Be(blacklistedById);
        vendor.BlacklistedAt.Should().NotBeNull();
    }

    [Fact]
    public void Blacklist_WithEmptyReason_ShouldThrowBusinessRuleException()
    {
        var vendor = CreateActiveVendor();

        var act = () => vendor.Blacklist("", Guid.NewGuid());

        act.Should().Throw<BusinessRuleException>()
            .Which.RuleName.Should().Be("VendorBlacklist");
    }

    [Fact]
    public void Blacklist_WithWhitespaceReason_ShouldThrowBusinessRuleException()
    {
        var vendor = CreateActiveVendor();

        var act = () => vendor.Blacklist("   ", Guid.NewGuid());

        act.Should().Throw<BusinessRuleException>();
    }

    // ── Reinstate ─────────────────────────────────────────────────────────────

    [Fact]
    public void Reinstate_FromSuspended_ShouldSetStatusActive()
    {
        var vendor = CreateActiveVendor();
        vendor.Suspend();

        vendor.Reinstate();

        vendor.Status.Should().Be(VendorStatus.Active);
        vendor.IsBlacklisted.Should().BeFalse();
    }

    [Fact]
    public void Reinstate_FromBlacklisted_ShouldClearBlacklistState()
    {
        var vendor = CreateActiveVendor();
        vendor.Blacklist("Test reason", Guid.NewGuid());

        vendor.Reinstate();

        vendor.Status.Should().Be(VendorStatus.Active);
        vendor.IsBlacklisted.Should().BeFalse();
        vendor.BlacklistReason.Should().BeNull();
    }

    [Fact]
    public void Reinstate_WhenPending_ShouldThrowBusinessRuleException()
    {
        var vendor = CreatePendingVendor();

        var act = () => vendor.Reinstate();

        act.Should().Throw<BusinessRuleException>()
            .Which.RuleName.Should().Be("VendorReinstate");
    }

    [Fact]
    public void Reinstate_WhenActive_ShouldThrowBusinessRuleException()
    {
        var vendor = CreateActiveVendor();

        var act = () => vendor.Reinstate();

        act.Should().Throw<BusinessRuleException>();
    }
}
