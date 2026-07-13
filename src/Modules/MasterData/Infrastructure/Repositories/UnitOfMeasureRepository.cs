using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.MasterData.Domain.Entities;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.MasterData.Infrastructure.Repositories;

public class UnitOfMeasureRepository : IUnitOfMeasureRepository
{
    private readonly ApplicationDbContext _db;

    public UnitOfMeasureRepository(ApplicationDbContext db) => _db = db;

    public Task<List<UnitOfMeasure>> GetAllAsync(Guid companyId, CancellationToken ct = default)
        => _db.Set<UnitOfMeasure>()
              .Where(e => e.CompanyId == companyId)
              .Include(e => e.CreatedBy)
              .Include(e => e.UpdatedBy)
              .OrderBy(e => e.Code)
              .ToListAsync(ct);

    public Task<UnitOfMeasure?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Set<UnitOfMeasure>().FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<bool> ExistsByCodeAsync(Guid companyId, string code, Guid? excludeId = null, CancellationToken ct = default)
        => _db.Set<UnitOfMeasure>()
              .AnyAsync(e => e.CompanyId == companyId
                          && e.Code == code
                          && (excludeId == null || e.Id != excludeId), ct);

    public void Add(UnitOfMeasure entity)    => _db.Set<UnitOfMeasure>().Add(entity);
    public void Update(UnitOfMeasure entity) => _db.Set<UnitOfMeasure>().Update(entity);
    public void Remove(UnitOfMeasure entity) => _db.Set<UnitOfMeasure>().Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
