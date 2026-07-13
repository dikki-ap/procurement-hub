using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.MasterData.Domain.Entities;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.MasterData.Infrastructure.Repositories;

public class CurrencyRepository : ICurrencyRepository
{
    private readonly ApplicationDbContext _db;

    public CurrencyRepository(ApplicationDbContext db) => _db = db;

    public Task<List<Currency>> GetAllAsync(CancellationToken ct = default)
        => _db.Set<Currency>()
              .Include(e => e.CreatedBy)
              .Include(e => e.UpdatedBy)
              .OrderBy(e => e.Code)
              .ToListAsync(ct);

    public Task<Currency?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Set<Currency>().FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<bool> ExistsByCodeAsync(string code, Guid? excludeId = null, CancellationToken ct = default)
        => _db.Set<Currency>()
              .AnyAsync(e => e.Code == code && (excludeId == null || e.Id != excludeId), ct);

    public Task ClearAllBaseAsync(Guid? exceptId, CancellationToken ct = default)
        => _db.Set<Currency>()
              .Where(c => c.IsBase && (exceptId == null || c.Id != exceptId))
              .ExecuteUpdateAsync(s => s.SetProperty(c => c.IsBase, false), ct);

    public void Add(Currency entity)    => _db.Set<Currency>().Add(entity);
    public void Update(Currency entity) => _db.Set<Currency>().Update(entity);
    public void Remove(Currency entity) => _db.Set<Currency>().Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
