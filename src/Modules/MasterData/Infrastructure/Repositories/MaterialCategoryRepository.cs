using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.MasterData.Domain.Entities;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.MasterData.Infrastructure.Repositories;

public class MaterialCategoryRepository : IMaterialCategoryRepository
{
    private readonly ApplicationDbContext _db;

    public MaterialCategoryRepository(ApplicationDbContext db) => _db = db;

    public Task<List<MaterialCategory>> GetAllAsync(Guid companyId, CancellationToken ct = default)
        => _db.Set<MaterialCategory>()
              .Where(e => e.CompanyId == companyId)
              .OrderBy(e => e.Code)
              .ToListAsync(ct);

    public Task<MaterialCategory?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Set<MaterialCategory>().FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<bool> ExistsByCodeAsync(Guid companyId, string code, Guid? excludeId = null, CancellationToken ct = default)
        => _db.Set<MaterialCategory>()
              .AnyAsync(e => e.CompanyId == companyId
                          && e.Code == code
                          && (excludeId == null || e.Id != excludeId), ct);

    public void Add(MaterialCategory entity)    => _db.Set<MaterialCategory>().Add(entity);
    public void Update(MaterialCategory entity) => _db.Set<MaterialCategory>().Update(entity);
    public void Remove(MaterialCategory entity) => _db.Set<MaterialCategory>().Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
