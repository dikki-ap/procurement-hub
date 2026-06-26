using ProcureHub.Modules.MasterData.Domain.Entities;

namespace ProcureHub.Modules.MasterData.Domain.Repositories;

public interface IUnitOfMeasureRepository
{
    Task<List<UnitOfMeasure>> GetAllAsync(Guid companyId, CancellationToken ct = default);
    Task<UnitOfMeasure?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByCodeAsync(Guid companyId, string code, Guid? excludeId = null, CancellationToken ct = default);
    void Add(UnitOfMeasure entity);
    void Update(UnitOfMeasure entity);
    void Remove(UnitOfMeasure entity);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
