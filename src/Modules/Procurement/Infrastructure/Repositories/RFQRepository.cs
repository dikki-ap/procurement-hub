using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.Procurement.Infrastructure.Repositories;

public class RFQRepository : IRFQRepository
{
    private readonly ApplicationDbContext _db;

    public RFQRepository(ApplicationDbContext db) => _db = db;

    public Task<List<RFQ>> GetAllAsync(Guid companyId, CancellationToken ct = default)
        => _db.Set<RFQ>()
              .Include(r => r.Items)
              .Include(r => r.Vendors)
              .Where(r => r.CompanyId == companyId)
              .OrderByDescending(r => r.CreatedAt)
              .ToListAsync(ct);

    public Task<List<RFQ>> GetOpenWithVendorsAsync(CancellationToken ct = default)
        => _db.Set<RFQ>()
              .Include(r => r.Vendors)
              .Where(r => r.Status == RFQStatus.Open)
              .ToListAsync(ct);

    public Task<RFQ?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Set<RFQ>()
              .FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<RFQ?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
        => _db.Set<RFQ>()
              .Include(r => r.Items)
              .Include(r => r.Vendors)
              .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<string> GenerateNextNumberAsync(Guid companyId, CancellationToken ct = default)
    {
        var year  = DateTime.UtcNow.Year;
        var count = await _db.Set<RFQ>()
                             .CountAsync(r => r.CompanyId == companyId
                                          && r.CreatedAt.Year == year, ct);
        return $"RFQ-{year}-{(count + 1):D6}";
    }

    public void Add(RFQ rfq)    => _db.Set<RFQ>().Add(rfq);
    public void Update(RFQ rfq) => _db.Set<RFQ>().Update(rfq);
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
