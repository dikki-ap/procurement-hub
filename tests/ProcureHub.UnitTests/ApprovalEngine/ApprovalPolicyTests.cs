using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using ProcureHub.Modules.ApprovalEngine.Domain.Entities;
using ProcureHub.Modules.ApprovalEngine.Domain.Services;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.UnitTests.ApprovalEngine;

public class ApprovalPolicyTests
{
    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var user = new Mock<ICurrentUserService>();
        user.Setup(u => u.UserId).Returns(Guid.NewGuid());
        var http = new Mock<IHttpContextAccessor>();
        return new ApplicationDbContext(options, user.Object, http.Object);
    }

    private static ApprovalStateMachine CreateStateMachine(ApplicationDbContext db)
        => new(db);

    private static ApprovalPolicy MakePolicy(
        Guid companyId, decimal min, decimal? max, int levels,
        bool strategicOverride = false, bool singleSourceOverride = false)
    => new()
    {
        CompanyId              = companyId,
        ReferenceType          = "PR",
        Name                   = $"Level {levels} policy",
        MinValue               = min,
        MaxValue               = max,
        RequiredLevels         = levels,
        IsStrategicOverride    = strategicOverride,
        IsSingleSourceOverride = singleSourceOverride,
        IsActive               = true,
    };

    // ── 1. Level determined by value ─────────────────────────────────────────

    [Fact]
    public async Task DetermineRequiredLevels_ByValue_ReturnsCorrectLevel()
    {
        var db        = CreateDbContext();
        var companyId = Guid.NewGuid();

        db.Set<ApprovalPolicy>().AddRange(
            MakePolicy(companyId, 0, 50_000, 1),
            MakePolicy(companyId, 50_000, null, 2));
        await db.SaveChangesAsync();

        var sm = CreateStateMachine(db);

        var low  = await sm.DetermineRequiredLevels("PR", 10_000m, false, false, companyId);
        var high = await sm.DetermineRequiredLevels("PR", 80_000m, false, false, companyId);

        low.Should().Be(1);
        high.Should().Be(2);
    }

    // ── 2. Strategic override ────────────────────────────────────────────────

    [Fact]
    public async Task DetermineRequiredLevels_StrategicItem_AddsOneLevel()
    {
        var db        = CreateDbContext();
        var companyId = Guid.NewGuid();

        db.Set<ApprovalPolicy>().AddRange(
            MakePolicy(companyId, 0, 50_000, 1, strategicOverride: true),
            MakePolicy(companyId, 50_000, null, 2, strategicOverride: true));
        await db.SaveChangesAsync();

        var sm = CreateStateMachine(db);

        var result = await sm.DetermineRequiredLevels("PR", 10_000m, true, false, companyId);

        result.Should().Be(2); // 1 + 1 strategic override
    }

    // ── 3. Single source override ────────────────────────────────────────────

    [Fact]
    public async Task DetermineRequiredLevels_SingleSource_AddsOneLevel()
    {
        var db        = CreateDbContext();
        var companyId = Guid.NewGuid();

        db.Set<ApprovalPolicy>().AddRange(
            MakePolicy(companyId, 0, 50_000, 1, singleSourceOverride: true),
            MakePolicy(companyId, 50_000, null, 2, singleSourceOverride: true));
        await db.SaveChangesAsync();

        var sm = CreateStateMachine(db);

        var result = await sm.DetermineRequiredLevels("PR", 10_000m, false, true, companyId);

        result.Should().Be(2); // 1 + 1 single source override
    }

    [Fact]
    public async Task DetermineRequiredLevels_NoPolicies_ReturnsOne()
    {
        var db        = CreateDbContext();
        var companyId = Guid.NewGuid();
        var sm        = CreateStateMachine(db);

        var result = await sm.DetermineRequiredLevels("PR", 99_999m, false, false, companyId);

        result.Should().Be(1);
    }
}
