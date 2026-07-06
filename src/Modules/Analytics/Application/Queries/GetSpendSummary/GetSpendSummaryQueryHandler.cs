using MediatR;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.Analytics.Application.Queries.GetSpendSummary;

public class GetSpendSummaryQueryHandler : IRequestHandler<GetSpendSummaryQuery, SpendSummaryDto>
{
    private readonly ApplicationDbContext _db;

    public GetSpendSummaryQueryHandler(ApplicationDbContext db) => _db = db;

    public async Task<SpendSummaryDto> Handle(GetSpendSummaryQuery request, CancellationToken ct)
    {
        var now   = DateTime.UtcNow;
        var start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc)
            .AddMonths(-(request.Months - 1));

        var rows = await _db.Set<PurchaseOrder>()
            .Where(po => po.CompanyId == request.CompanyId
                      && po.IssuedAt >= start
                      && po.Status != POStatus.Cancelled
                      && po.IssuedAt != null)
            .Select(po => new { po.IssuedAt, po.TotalAmount })
            .ToListAsync(ct);

        var monthly = Enumerable.Range(0, request.Months)
            .Select(i => start.AddMonths(i))
            .Select(month => new MonthlySpendDto(
                month.ToString("MMM yyyy"),
                rows.Where(r => r.IssuedAt!.Value.Year  == month.Year
                             && r.IssuedAt!.Value.Month == month.Month)
                    .Sum(r => r.TotalAmount)))
            .ToList();

        var thisYear = rows.Where(r => r.IssuedAt!.Value.Year == now.Year).Sum(r => r.TotalAmount);
        var lastYear = rows.Where(r => r.IssuedAt!.Value.Year == now.Year - 1).Sum(r => r.TotalAmount);

        return new SpendSummaryDto(monthly, thisYear, lastYear);
    }
}
