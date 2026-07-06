using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.Modules.VendorManagement.Domain.Enums;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.Audit;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.UnitTests.Audit;

/// <summary>
/// Tests that ApplicationDbContext.CaptureAuditEntries() correctly captures
/// Created / Updated / Deleted actions and populates Before/After JSON.
/// </summary>
public class AuditCaptureTests : IDisposable
{
    private readonly ApplicationDbContext _db;
    private readonly Mock<ICurrentUserService> _userMock = new();

    public AuditCaptureTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _userMock.Setup(u => u.UserId).Returns(Guid.NewGuid());
        _userMock.Setup(u => u.Email).Returns("auditor@test.com");
        _userMock.Setup(u => u.FullName).Returns("Audit User");

        _db = new ApplicationDbContext(options, _userMock.Object, new Mock<IHttpContextAccessor>().Object);
    }

    public void Dispose() => _db.Dispose();

    private static Vendor MakeVendor(string code = "V-001", string name = "Test Corp") => new()
    {
        CompanyId  = Guid.NewGuid(),
        VendorCode = code,
        LegalName  = name,
        VendorType = VendorType.Manufacturer,
        Status     = VendorStatus.Pending,
    };

    // ── Created ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_ShouldProduceAuditLog_WithNullBeforeValues()
    {
        var vendor = MakeVendor();
        _db.Set<Vendor>().Add(vendor);
        await _db.SaveChangesAsync();

        var log = await _db.Set<AuditLog>()
            .FirstOrDefaultAsync(a => a.EntityType == "Vendor" && a.Action == "Created");

        log.Should().NotBeNull();
        log!.BeforeValues.Should().BeNull("Created actions have no previous state");
        log.AfterValues.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Create_AfterValues_ShouldContainEntityFields()
    {
        var vendor = MakeVendor(name: "Acme Inc");
        _db.Set<Vendor>().Add(vendor);
        await _db.SaveChangesAsync();

        var log = await _db.Set<AuditLog>()
            .FirstAsync(a => a.EntityType == "Vendor" && a.Action == "Created");

        log.AfterValues.Should().Contain("Acme Inc");
    }

    [Fact]
    public async Task Create_ShouldRecordActorFromCurrentUserService()
    {
        _db.Set<Vendor>().Add(MakeVendor());
        await _db.SaveChangesAsync();

        var log = await _db.Set<AuditLog>()
            .FirstAsync(a => a.EntityType == "Vendor" && a.Action == "Created");

        log.UserEmail.Should().Be("auditor@test.com");
        log.UserFullName.Should().Be("Audit User");
    }

    // ── Updated ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_ShouldProduceAuditLog_WithBothBeforeAndAfterValues()
    {
        var vendor = MakeVendor(name: "Original Name");
        _db.Set<Vendor>().Add(vendor);
        await _db.SaveChangesAsync();

        vendor.LegalName = "Updated Name";
        await _db.SaveChangesAsync();

        var log = await _db.Set<AuditLog>()
            .FirstOrDefaultAsync(a => a.EntityType == "Vendor" && a.Action == "Updated");

        log.Should().NotBeNull();
        log!.BeforeValues.Should().Contain("Original Name");
        log.AfterValues.Should().Contain("Updated Name");
        log.ChangedColumns.Should().Contain("LegalName");
    }

    [Fact]
    public async Task Update_WithNoActualChange_ShouldNotProduceAuditLog()
    {
        var vendor = MakeVendor(name: "Stable Corp");
        _db.Set<Vendor>().Add(vendor);
        await _db.SaveChangesAsync();

        var countBefore = await _db.Set<AuditLog>().CountAsync();

        // Mark modified but don't change anything
        _db.Entry(vendor).State = EntityState.Modified;
        vendor.LegalName = "Stable Corp"; // same value
        await _db.SaveChangesAsync();

        var countAfter = await _db.Set<AuditLog>().CountAsync();
        countAfter.Should().Be(countBefore, "no actual column change means no audit row");
    }

    // ── Deleted (soft delete) ─────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ShouldProduceAuditLog_WithBeforeValues()
    {
        var vendor = MakeVendor(name: "Soon Deleted Corp");
        _db.Set<Vendor>().Add(vendor);
        await _db.SaveChangesAsync();

        _db.Set<Vendor>().Remove(vendor);
        await _db.SaveChangesAsync();

        var log = await _db.Set<AuditLog>()
            .FirstOrDefaultAsync(a => a.EntityType == "Vendor" && a.Action == "Deleted");

        log.Should().NotBeNull();
        log!.BeforeValues.Should().NotBeNull("Deleted action must capture the previous state");
        log.BeforeValues.Should().Contain("Soon Deleted Corp");
    }

    [Fact]
    public async Task Delete_ShouldConvertToSoftDelete_SettingIsDeletedTrue()
    {
        var vendor = MakeVendor();
        _db.Set<Vendor>().Add(vendor);
        await _db.SaveChangesAsync();

        _db.Set<Vendor>().Remove(vendor);
        await _db.SaveChangesAsync();

        // IgnoreQueryFilters to read soft-deleted records
        var persisted = await _db.Set<Vendor>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(v => v.Id == vendor.Id);

        persisted.Should().NotBeNull();
        persisted!.IsDeleted.Should().BeTrue("hard delete is intercepted and converted to soft delete");
    }
}
