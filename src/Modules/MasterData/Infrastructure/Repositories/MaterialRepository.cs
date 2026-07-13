using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.MasterData.Domain.Entities;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.MasterData.Infrastructure.Repositories;

public class MaterialRepository : IMaterialRepository
{
    private readonly ApplicationDbContext _db;

    public MaterialRepository(ApplicationDbContext db) => _db = db;

    public Task<List<Material>> GetAllAsync(CancellationToken ct = default)
        => _db.Set<Material>()
              .Include(e => e.Category)
              .Include(e => e.Uom)
              .Include(e => e.Currency)
              .Include(e => e.CreatedBy)
              .Include(e => e.UpdatedBy)
              .OrderBy(e => e.Code)
              .ToListAsync(ct);

    public Task<Material?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Set<Material>()
              .Include(e => e.Category)
              .Include(e => e.Uom)
              .Include(e => e.Currency)
              .FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<bool> ExistsByCodeAsync(string code, Guid? excludeId = null, CancellationToken ct = default)
        => _db.Set<Material>()
              .AnyAsync(e => e.Code == code && (excludeId == null || e.Id != excludeId), ct);

    public void Add(Material entity)    => _db.Set<Material>().Add(entity);
    public void Update(Material entity) => _db.Set<Material>().Update(entity);
    public void Remove(Material entity) => _db.Set<Material>().Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
