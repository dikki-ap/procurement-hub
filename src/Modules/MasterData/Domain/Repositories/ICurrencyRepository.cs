using ProcureHub.Modules.MasterData.Domain.Entities;

namespace ProcureHub.Modules.MasterData.Domain.Repositories;

public interface ICurrencyRepository
{
    Task<List<Currency>> GetAllAsync(CancellationToken ct = default);
    Task<Currency?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByCodeAsync(string code, Guid? excludeId = null, CancellationToken ct = default);
    void Add(Currency entity);
    void Update(Currency entity);
    void Remove(Currency entity);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
