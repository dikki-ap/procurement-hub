using MediatR;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.Analytics.Application.Queries.GetProcurementFunnelStats;

public class GetProcurementFunnelStatsQueryHandler
    : IRequestHandler<GetProcurementFunnelStatsQuery, FunnelStatsDto>
{
    private readonly ApplicationDbContext _db;

    public GetProcurementFunnelStatsQueryHandler(ApplicationDbContext db) => _db = db;

    public async Task<FunnelStatsDto> Handle(
        GetProcurementFunnelStatsQuery request, CancellationToken ct)
    {
        var year = request.Year > 0 ? request.Year : DateTime.UtcNow.Year;

        var prCount = await _db.Set<PurchaseRequisition>()
            .CountAsync(pr => pr.CompanyId == request.CompanyId && pr.CreatedAt.Year == year, ct);

        var prValue = await _db.Set<PurchaseRequisition>()
            .Where(pr => pr.CompanyId == request.CompanyId && pr.CreatedAt.Year == year)
            .SumAsync(pr => (decimal?)pr.TotalEstimatedValue ?? 0, ct);

        var rfqCount = await _db.Set<RFQ>()
            .CountAsync(r => r.CompanyId == request.CompanyId && r.CreatedAt.Year == year, ct);

        var poCount = await _db.Set<PurchaseOrder>()
            .CountAsync(po => po.CompanyId == request.CompanyId
                           && po.CreatedAt.Year == year
                           && po.Status != POStatus.Cancelled, ct);

        var poValue = await _db.Set<PurchaseOrder>()
            .Where(po => po.CompanyId == request.CompanyId
                      && po.CreatedAt.Year == year
                      && po.Status != POStatus.Cancelled)
            .SumAsync(po => (decimal?)po.TotalAmount ?? 0, ct);

        // GRN has no CompanyId — count GRNs linked to POs of this company
        var companyPoIds = await _db.Set<PurchaseOrder>()
            .Where(po => po.CompanyId == request.CompanyId && po.CreatedAt.Year == year)
            .Select(po => po.Id)
            .ToListAsync(ct);

        var grnCount = await _db.Set<GoodsReceipt>()
            .CountAsync(g => companyPoIds.Contains(g.POId), ct);

        var stages = new List<FunnelStageDto>
        {
            new("Purchase Requisition", prCount,  prValue),
            new("RFQ / Tender",         rfqCount, 0m),
            new("Purchase Order",       poCount,  poValue),
            new("Goods Receipt",        grnCount, 0m),
        };

        return new FunnelStatsDto(stages);
    }
}
