using MediatR;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.Analytics.Application.Queries.GetCycleTime;

public class GetCycleTimeQueryHandler : IRequestHandler<GetCycleTimeQuery, List<CycleTimeStageDto>>
{
    private readonly ApplicationDbContext _db;

    public GetCycleTimeQueryHandler(ApplicationDbContext db) => _db = db;

    public async Task<List<CycleTimeStageDto>> Handle(GetCycleTimeQuery request, CancellationToken ct)
    {
        var since = DateTime.UtcNow.AddMonths(-request.Months);

        // Stage 1: PR → RFQ (days from PR creation to linked RFQ creation)
        var prToRfq = await _db.Set<RFQ>().AsNoTracking()
            .Where(r => r.CompanyId             == request.CompanyId
                     && r.PurchaseRequisitionId != null
                     && r.CreatedAt             >= since)
            .Join(_db.Set<PurchaseRequisition>().AsNoTracking(),
                  r  => r.PurchaseRequisitionId,
                  pr => pr.Id,
                  (r, pr) => EF.Functions.DateDiffDay(pr.CreatedAt, r.CreatedAt))
            .Where(days => days >= 0)
            .ToListAsync(ct);

        // Stage 2: RFQ → PO Issued (days from RFQ creation to linked PO issued)
        var rfqToPo = await _db.Set<PurchaseOrder>().AsNoTracking()
            .Where(po => po.CompanyId == request.CompanyId
                      && po.RFQId    != null
                      && po.IssuedAt != null
                      && po.IssuedAt >= since
                      && po.Status   != POStatus.Cancelled)
            .Join(_db.Set<RFQ>().AsNoTracking(),
                  po => po.RFQId,
                  r  => r.Id,
                  (po, r) => EF.Functions.DateDiffDay(r.CreatedAt, po.IssuedAt!.Value))
            .Where(days => days >= 0)
            .ToListAsync(ct);

        // Stage 3: PO Issued → GRN Confirmed
        var poToGrn = await _db.Set<GoodsReceipt>().AsNoTracking()
            .Where(g => g.Status     == GRNStatus.Confirmed
                     && g.ReceivedAt != null
                     && g.ReceivedAt >= since)
            .Join(_db.Set<PurchaseOrder>().AsNoTracking()
                      .Where(po => po.CompanyId == request.CompanyId && po.IssuedAt != null),
                  g  => g.POId,
                  po => po.Id,
                  (g, po) => EF.Functions.DateDiffDay(po.IssuedAt!.Value, g.ReceivedAt!.Value))
            .Where(days => days >= 0)
            .ToListAsync(ct);

        return
        [
            new("PR → RFQ",      prToRfq.Count > 0 ? Math.Round(prToRfq.Average(), 1) : 0),
            new("RFQ → PO",      rfqToPo.Count > 0 ? Math.Round(rfqToPo.Average(), 1) : 0),
            new("PO → Delivery", poToGrn.Count > 0 ? Math.Round(poToGrn.Average(), 1) : 0),
        ];
    }
}
