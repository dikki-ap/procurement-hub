using ProcureHub.Modules.MasterData.Domain.Entities;

namespace ProcureHub.Modules.MasterData.Domain.Repositories;

public interface IMaterialCategoryRepository
{
    Task<List<MaterialCategory>> GetAllAsync(Guid companyId, CancellationToken ct = default);
    Task<MaterialCategory?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByCodeAsync(Guid companyId, string code, Guid? excludeId = null, CancellationToken ct = default);
    void Add(MaterialCategory entity);
    void Update(MaterialCategory entity);
    void Remove(MaterialCategory entity);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
