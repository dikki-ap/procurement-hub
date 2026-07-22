using MediatR;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.MasterData.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.Analytics.Application.Queries.GetSpendByCategory;

public class GetSpendByCategoryQueryHandler
    : IRequestHandler<GetSpendByCategoryQuery, List<SpendByCategoryDto>>
{
    private readonly ApplicationDbContext _db;

    public GetSpendByCategoryQueryHandler(ApplicationDbContext db) => _db = db;

    public async Task<List<SpendByCategoryDto>> Handle(
        GetSpendByCategoryQuery request, CancellationToken ct)
    {
        var year = request.Year > 0 ? request.Year : DateTime.UtcNow.Year;

        var rows = await _db.Set<POItem>().AsNoTracking()
            .Where(i => i.PurchaseOrder!.CompanyId == request.CompanyId
                     && i.PurchaseOrder.Status    != POStatus.Cancelled
                     && i.PurchaseOrder.IssuedAt  != null
                     && i.PurchaseOrder.IssuedAt!.Value.Year == year
                     && i.MaterialId != null)
            .Join(_db.Set<Material>().AsNoTracking(),
                  i  => i.MaterialId,
                  m  => m.Id,
                  (i, m) => new { m.CategoryId, i.TotalPrice })
            .Join(_db.Set<MaterialCategory>().AsNoTracking(),
                  x  => x.CategoryId,
                  c  => c.Id,
                  (x, c) => new { c.Name, x.TotalPrice })
            .ToListAsync(ct);

        var grandTotal = rows.Sum(r => r.TotalPrice);
        if (grandTotal == 0) return [];

        return rows
            .GroupBy(r => r.Name)
            .Select(g => new SpendByCategoryDto(
                g.Key,
                g.Sum(r => r.TotalPrice),
                Math.Round(g.Sum(r => r.TotalPrice) / grandTotal * 100, 2)))
            .OrderByDescending(x => x.TotalSpend)
            .Take(10)
            .ToList();
    }
}
