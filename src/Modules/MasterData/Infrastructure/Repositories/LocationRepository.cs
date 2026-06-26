using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.MasterData.Domain.Entities;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.MasterData.Infrastructure.Repositories;

public class LocationRepository : ILocationRepository
{
    private readonly ApplicationDbContext _db;

    public LocationRepository(ApplicationDbContext db) => _db = db;

    public Task<List<Location>> GetAllAsync(Guid companyId, CancellationToken ct = default)
        => _db.Set<Location>()
              .Where(e => e.CompanyId == companyId)
              .OrderBy(e => e.Name)
              .ToListAsync(ct);

    public Task<Location?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Set<Location>().FirstOrDefaultAsync(e => e.Id == id, ct);

    public void Add(Location entity)    => _db.Set<Location>().Add(entity);
    public void Update(Location entity) => _db.Set<Location>().Update(entity);
    public void Remove(Location entity) => _db.Set<Location>().Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
