using MediatR;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.Analytics.Application.Queries.GetVendorConcentration;

public class GetVendorConcentrationQueryHandler
    : IRequestHandler<GetVendorConcentrationQuery, VendorConcentrationDto>
{
    private readonly ApplicationDbContext _db;

    public GetVendorConcentrationQueryHandler(ApplicationDbContext db) => _db = db;

    public async Task<VendorConcentrationDto> Handle(
        GetVendorConcentrationQuery request, CancellationToken ct)
    {
        var rows = await _db.Set<PurchaseOrder>().AsNoTracking()
            .Where(po => po.CompanyId == request.CompanyId
                      && po.Status    != POStatus.Cancelled
                      && po.IssuedAt  != null)
            .GroupBy(po => po.VendorId)
            .Select(g => new { VendorId = g.Key, TotalSpend = g.Sum(po => po.TotalAmount) })
            .ToListAsync(ct);

        var grandTotal = rows.Sum(r => r.TotalSpend);
        if (grandTotal == 0)
            return new VendorConcentrationDto([], false);

        var vendorIds = rows.Select(r => r.VendorId).ToList();
        var vendors   = await _db.Set<Vendor>().AsNoTracking()
            .Where(v => vendorIds.Contains(v.Id))
            .Select(v => new { v.Id, v.LegalName, v.TradeName })
            .ToListAsync(ct);

        var nameMap = vendors.ToDictionary(v => v.Id, v => v.TradeName ?? v.LegalName);

        var top5 = rows
            .OrderByDescending(r => r.TotalSpend)
            .Take(5)
            .Select(r => new VendorSpendShareDto(
                r.VendorId,
                nameMap.GetValueOrDefault(r.VendorId, "Unknown"),
                r.TotalSpend,
                Math.Round(r.TotalSpend / grandTotal * 100, 2)))
            .ToList();

        var hasRisk = top5.Any(v => v.PctOfTotal > 40m);

        return new VendorConcentrationDto(top5, hasRisk);
    }
}
