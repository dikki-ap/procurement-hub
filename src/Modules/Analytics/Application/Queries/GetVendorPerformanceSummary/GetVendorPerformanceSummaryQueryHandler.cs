using MediatR;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.Analytics.Application.Queries.GetVendorPerformanceSummary;

public class GetVendorPerformanceSummaryQueryHandler
    : IRequestHandler<GetVendorPerformanceSummaryQuery, VendorPerformanceSummaryDto>
{
    private readonly ApplicationDbContext _db;

    public GetVendorPerformanceSummaryQueryHandler(ApplicationDbContext db) => _db = db;

    public async Task<VendorPerformanceSummaryDto> Handle(
        GetVendorPerformanceSummaryQuery request, CancellationToken ct)
    {
        // Get vendors with their latest score in one query
        var vendors = await _db.Set<Vendor>().AsNoTracking()
            .Where(v => v.CompanyId == request.CompanyId)
            .Select(v => new
            {
                v.Id,
                v.LegalName,
                v.Tier,
                v.Score,
                LatestScore = v.Scores
                    .OrderByDescending(s => s.PeriodYear)
                    .ThenByDescending(s => s.PeriodQuarter)
                    .FirstOrDefault(),
            })
            .OrderByDescending(v => v.Score)
            .Take(request.TopN)
            .ToListAsync(ct);

        // Get PO spend per vendor
        var vendorIds = vendors.Select(v => v.Id).ToList();
        var spendByVendor = await _db.Set<PurchaseOrder>().AsNoTracking()
            .Where(po => po.CompanyId == request.CompanyId
                      && vendorIds.Contains(po.VendorId)
                      && po.Status != POStatus.Cancelled)
            .GroupBy(po => po.VendorId)
            .Select(g => new { VendorId = g.Key, TotalSpend = g.Sum(po => po.TotalAmount), Count = g.Count() })
            .ToListAsync(ct);

        var result = vendors.Select(v =>
        {
            var spend = spendByVendor.FirstOrDefault(s => s.VendorId == v.Id);
            return new VendorPerformanceDto(
                v.Id,
                v.LegalName,
                v.Tier.ToString(),
                v.Score,
                v.LatestScore?.DeliveryScore ?? 0m,
                v.LatestScore?.QualityScore  ?? 0m,
                spend?.TotalSpend ?? 0m,
                spend?.Count      ?? 0);
        }).ToList();

        return new VendorPerformanceSummaryDto(result);
    }
}
