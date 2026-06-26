using ProcureHub.Modules.MasterData.Domain.Entities;

namespace ProcureHub.Modules.MasterData.Domain.Repositories;

public interface IMaterialRepository
{
    Task<List<Material>> GetAllAsync(CancellationToken ct = default);
    Task<Material?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByCodeAsync(string code, Guid? excludeId = null, CancellationToken ct = default);
    void Add(Material entity);
    void Update(Material entity);
    void Remove(Material entity);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
