using ProcureHub.Modules.MasterData.Domain.Entities;

namespace ProcureHub.Modules.MasterData.Domain.Repositories;

public interface ILocationRepository
{
    Task<List<Location>> GetAllAsync(Guid companyId, CancellationToken ct = default);
    Task<Location?> GetByIdAsync(Guid id, CancellationToken ct = default);
    void Add(Location entity);
    void Update(Location entity);
    void Remove(Location entity);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
