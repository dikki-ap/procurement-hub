using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.Modules.VendorManagement.Domain.Repositories;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.VendorManagement.Infrastructure.Repositories;

public class VendorScoreRepository : IVendorScoreRepository
{
    private readonly ApplicationDbContext _db;

    public VendorScoreRepository(ApplicationDbContext db) => _db = db;

    public Task<VendorScore?> GetCurrentAsync(Guid vendorId, CancellationToken ct = default)
    {
        var now     = DateTime.UtcNow;
        var quarter = ((now.Month - 1) / 3) + 1;
        return _db.Set<VendorScore>()
                  .FirstOrDefaultAsync(s => s.VendorId      == vendorId
                                         && s.PeriodYear    == now.Year
                                         && s.PeriodQuarter == quarter, ct);
    }

    public Task<List<VendorScore>> GetHistoryAsync(Guid vendorId, CancellationToken ct = default)
        => _db.Set<VendorScore>()
              .Where(s => s.VendorId == vendorId)
              .OrderByDescending(s => s.PeriodYear)
              .ThenByDescending(s => s.PeriodQuarter)
              .ToListAsync(ct);

    public void Add(VendorScore score)    => _db.Set<VendorScore>().Add(score);
    public void Update(VendorScore score) => _db.Set<VendorScore>().Update(score);
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
