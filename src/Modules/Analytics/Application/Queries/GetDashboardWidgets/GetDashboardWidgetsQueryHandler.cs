using MediatR;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.ApprovalEngine.Domain.Entities;
using ProcureHub.Modules.ApprovalEngine.Domain.Enums;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.Modules.VendorManagement.Domain.Enums;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.Analytics.Application.Queries.GetDashboardWidgets;

public class GetDashboardWidgetsQueryHandler : IRequestHandler<GetDashboardWidgetsQuery, object>
{
    private readonly ApplicationDbContext _db;

    public GetDashboardWidgetsQueryHandler(ApplicationDbContext db) => _db = db;

    public async Task<object> Handle(GetDashboardWidgetsQuery request, CancellationToken ct)
    {
        if (request.Role == "vendor" && request.VendorId.HasValue)
            return await GetVendorWidgets(request.VendorId.Value, ct);

        return await GetInternalWidgets(request.CompanyId, ct);
    }

    private async Task<DashboardWidgetsDto> GetInternalWidgets(Guid companyId, CancellationToken ct)
    {
        var now        = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var spendThisMonth = await _db.Set<PurchaseOrder>().AsNoTracking()
            .Where(po => po.CompanyId == companyId
                      && po.IssuedAt >= monthStart
                      && po.Status != POStatus.Cancelled)
            .SumAsync(po => (decimal?)po.TotalAmount ?? 0, ct);

        var pendingApprovals = await _db.Set<ApprovalWorkflow>().AsNoTracking()
            .CountAsync(w => w.CompanyId == companyId && w.Status == WorkflowStatus.Pending, ct);

        var openRFQs = await _db.Set<RFQ>().AsNoTracking()
            .CountAsync(r => r.CompanyId == companyId && r.Status == RFQStatus.Open, ct);

        var activePOs = await _db.Set<PurchaseOrder>().AsNoTracking()
            .CountAsync(po => po.CompanyId == companyId
                           && (po.Status == POStatus.Issued
                            || po.Status == POStatus.Acknowledged
                            || po.Status == POStatus.InDelivery), ct);

        // Invoice has no CompanyId — join through PO
        var pendingInvoices = await _db.Set<Invoice>().AsNoTracking()
            .Join(_db.Set<PurchaseOrder>().AsNoTracking(),
                  inv => inv.POId, po => po.Id,
                  (inv, po) => new { inv.Status, po.CompanyId })
            .CountAsync(x => x.CompanyId == companyId
                          && (x.Status == InvoiceStatus.Submitted
                           || x.Status == InvoiceStatus.UnderReview), ct);

        var totalVendors = await _db.Set<Vendor>().AsNoTracking()
            .CountAsync(v => v.CompanyId == companyId && v.Status == VendorStatus.Active, ct);

        var totalPRs = await _db.Set<PurchaseRequisition>().AsNoTracking()
            .CountAsync(pr => pr.CompanyId == companyId, ct);

        return new DashboardWidgetsDto(
            spendThisMonth, pendingApprovals, openRFQs,
            activePOs, pendingInvoices, totalVendors, totalPRs);
    }

    private async Task<VendorDashboardWidgetsDto> GetVendorWidgets(Guid vendorId, CancellationToken ct)
    {
        var activePOs = await _db.Set<PurchaseOrder>().AsNoTracking()
            .CountAsync(po => po.VendorId == vendorId
                           && (po.Status == POStatus.Issued
                            || po.Status == POStatus.Acknowledged
                            || po.Status == POStatus.InDelivery), ct);

        var pendingInvoices = await _db.Set<Invoice>().AsNoTracking()
            .CountAsync(inv => inv.VendorId == vendorId
                            && (inv.Status == InvoiceStatus.Submitted
                             || inv.Status == InvoiceStatus.UnderReview), ct);

        var latestScore = await _db.Set<VendorScore>().AsNoTracking()
            .Where(s => s.VendorId == vendorId)
            .OrderByDescending(s => s.PeriodYear)
            .ThenByDescending(s => s.PeriodQuarter)
            .Select(s => new { s.TotalScore, s.Tier })
            .FirstOrDefaultAsync(ct);

        return new VendorDashboardWidgetsDto(
            activePOs,
            pendingInvoices,
            latestScore?.TotalScore ?? 0m,
            latestScore?.Tier.ToString() ?? "Bronze");
    }
}
