using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using ProcureHub.Modules.Analytics.Application.Queries.GetDashboardWidgets;
using ProcureHub.Modules.ApprovalEngine.Domain.Entities;
using ProcureHub.Modules.ApprovalEngine.Domain.Enums;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.Modules.VendorManagement.Domain.Enums;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.UnitTests.Analytics;

public class DashboardWidgetsTests : IDisposable
{
    private readonly ApplicationDbContext _db;
    private static readonly Guid CompanyId   = Guid.NewGuid();
    private static readonly Guid VendorId    = Guid.NewGuid();

    public DashboardWidgetsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var currentUser = new Mock<ICurrentUserService>();
        var httpContext = new Mock<IHttpContextAccessor>();

        _db = new ApplicationDbContext(options, currentUser.Object, httpContext.Object);
    }

    public void Dispose() => _db.Dispose();

    private GetDashboardWidgetsQueryHandler Handler() => new(_db);

    // ── Internal widgets ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetInternalWidgets_ShouldReturnCorrectSpendThisMonth()
    {
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        _db.Set<PurchaseOrder>().Add(new PurchaseOrder
        {
            CompanyId   = CompanyId,
            VendorId    = VendorId,
            PONumber    = "PO-001",
            Status      = POStatus.Issued,
            TotalAmount = 500_000m,
            IssuedAt    = monthStart.AddDays(1),
        });
        // Cancelled PO — should be excluded from spend
        _db.Set<PurchaseOrder>().Add(new PurchaseOrder
        {
            CompanyId   = CompanyId,
            VendorId    = VendorId,
            PONumber    = "PO-002",
            Status      = POStatus.Cancelled,
            TotalAmount = 999_000m,
            IssuedAt    = monthStart.AddDays(2),
        });
        await _db.SaveChangesAsync();

        var result = (DashboardWidgetsDto) await Handler().Handle(
            new GetDashboardWidgetsQuery(CompanyId, "internal"), default);

        result.SpendThisMonth.Should().Be(500_000m);
    }

    [Fact]
    public async Task GetInternalWidgets_ShouldCountOnlyActivePOs()
    {
        _db.Set<PurchaseOrder>().AddRange(
            new PurchaseOrder { CompanyId = CompanyId, VendorId = VendorId, PONumber = "PO-A", Status = POStatus.Issued },
            new PurchaseOrder { CompanyId = CompanyId, VendorId = VendorId, PONumber = "PO-B", Status = POStatus.Acknowledged },
            new PurchaseOrder { CompanyId = CompanyId, VendorId = VendorId, PONumber = "PO-C", Status = POStatus.InDelivery },
            new PurchaseOrder { CompanyId = CompanyId, VendorId = VendorId, PONumber = "PO-D", Status = POStatus.Cancelled },
            new PurchaseOrder { CompanyId = CompanyId, VendorId = VendorId, PONumber = "PO-E", Status = POStatus.Draft }
        );
        await _db.SaveChangesAsync();

        var result = (DashboardWidgetsDto) await Handler().Handle(
            new GetDashboardWidgetsQuery(CompanyId, "internal"), default);

        result.ActivePOs.Should().Be(3);
    }

    [Fact]
    public async Task GetInternalWidgets_ShouldCountActiveVendorsOnly()
    {
        _db.Set<Vendor>().AddRange(
            new Vendor { CompanyId = CompanyId, VendorCode = "V-001", LegalName = "Active Vendor A", Status = VendorStatus.Active },
            new Vendor { CompanyId = CompanyId, VendorCode = "V-002", LegalName = "Pending Vendor",  Status = VendorStatus.Pending },
            new Vendor { CompanyId = CompanyId, VendorCode = "V-003", LegalName = "Active Vendor B", Status = VendorStatus.Active }
        );
        await _db.SaveChangesAsync();

        var result = (DashboardWidgetsDto) await Handler().Handle(
            new GetDashboardWidgetsQuery(CompanyId, "internal"), default);

        result.TotalVendors.Should().Be(2);
    }

    [Fact]
    public async Task GetInternalWidgets_ShouldCountPendingApprovalsOnly()
    {
        _db.Set<ApprovalWorkflow>().AddRange(
            new ApprovalWorkflow { CompanyId = CompanyId, ReferenceType = "PR", ReferenceId = Guid.NewGuid(), ReferenceNumber = "PR-001", ReferenceTitle = "PR 1", RequestedById = Guid.NewGuid(), Status = WorkflowStatus.Pending },
            new ApprovalWorkflow { CompanyId = CompanyId, ReferenceType = "PR", ReferenceId = Guid.NewGuid(), ReferenceNumber = "PR-002", ReferenceTitle = "PR 2", RequestedById = Guid.NewGuid(), Status = WorkflowStatus.Approved },
            new ApprovalWorkflow { CompanyId = CompanyId, ReferenceType = "PR", ReferenceId = Guid.NewGuid(), ReferenceNumber = "PR-003", ReferenceTitle = "PR 3", RequestedById = Guid.NewGuid(), Status = WorkflowStatus.Pending }
        );
        await _db.SaveChangesAsync();

        var result = (DashboardWidgetsDto) await Handler().Handle(
            new GetDashboardWidgetsQuery(CompanyId, "internal"), default);

        result.PendingApprovals.Should().Be(2);
    }

    [Fact]
    public async Task GetInternalWidgets_ShouldNotReturnDataFromOtherCompanies()
    {
        var otherCompany = Guid.NewGuid();
        _db.Set<PurchaseOrder>().Add(new PurchaseOrder
        {
            CompanyId   = otherCompany,
            VendorId    = VendorId,
            PONumber    = "PO-OTHER",
            Status      = POStatus.Issued,
            TotalAmount = 1_000_000m,
            IssuedAt    = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();

        var result = (DashboardWidgetsDto) await Handler().Handle(
            new GetDashboardWidgetsQuery(CompanyId, "internal"), default);

        result.SpendThisMonth.Should().Be(0);
        result.ActivePOs.Should().Be(0);
    }

    // ── Vendor widgets ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetVendorWidgets_ShouldReturnOnlyVendorOwnActivePOs()
    {
        var otherVendor = Guid.NewGuid();
        _db.Set<PurchaseOrder>().AddRange(
            new PurchaseOrder { CompanyId = CompanyId, VendorId = VendorId,    PONumber = "PO-V1", Status = POStatus.Issued },
            new PurchaseOrder { CompanyId = CompanyId, VendorId = VendorId,    PONumber = "PO-V2", Status = POStatus.InDelivery },
            new PurchaseOrder { CompanyId = CompanyId, VendorId = otherVendor, PONumber = "PO-X1", Status = POStatus.Issued }
        );
        await _db.SaveChangesAsync();

        var result = (VendorDashboardWidgetsDto) await Handler().Handle(
            new GetDashboardWidgetsQuery(CompanyId, "vendor", VendorId), default);

        result.MyActivePOs.Should().Be(2);
    }

    [Fact]
    public async Task GetVendorWidgets_ShouldReturnLatestScoreAndTier()
    {
        _db.Set<VendorScore>().AddRange(
            new VendorScore
            {
                VendorId      = VendorId,
                PeriodYear    = 2025,
                PeriodQuarter = 4,
                TotalScore    = 72m,
                Tier          = VendorTier.Bronze,
            },
            new VendorScore
            {
                VendorId      = VendorId,
                PeriodYear    = 2026,
                PeriodQuarter = 1,
                TotalScore    = 85m,
                Tier          = VendorTier.Silver,
            }
        );
        await _db.SaveChangesAsync();

        var result = (VendorDashboardWidgetsDto) await Handler().Handle(
            new GetDashboardWidgetsQuery(CompanyId, "vendor", VendorId), default);

        result.MyLatestScore.Should().Be(85m);
        result.MyTier.Should().Be("Silver");
    }

    [Fact]
    public async Task GetVendorWidgets_WithNoScore_ShouldDefaultToBronzeAndZero()
    {
        var result = (VendorDashboardWidgetsDto) await Handler().Handle(
            new GetDashboardWidgetsQuery(CompanyId, "vendor", VendorId), default);

        result.MyLatestScore.Should().Be(0m);
        result.MyTier.Should().Be("Bronze");
    }

    [Fact]
    public async Task GetVendorWidgets_ShouldNotCountOtherVendorInvoices()
    {
        var otherVendor = Guid.NewGuid();
        var po = new PurchaseOrder
        {
            CompanyId = CompanyId,
            VendorId  = otherVendor,
            PONumber  = "PO-X",
            Status    = POStatus.Issued,
        };
        _db.Set<PurchaseOrder>().Add(po);
        await _db.SaveChangesAsync();

        _db.Set<Invoice>().Add(new Invoice
        {
            POId          = po.Id,
            VendorId      = otherVendor,
            Status        = InvoiceStatus.Submitted,
            InvoiceNumber = "INV-X",
            TotalAmount   = 100_000m,
        });
        await _db.SaveChangesAsync();

        var result = (VendorDashboardWidgetsDto) await Handler().Handle(
            new GetDashboardWidgetsQuery(CompanyId, "vendor", VendorId), default);

        result.MyPendingInvoices.Should().Be(0);
    }
}
