using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.MasterData.Domain.Repositories;

public interface IDepartmentRepository
{
    Task<List<Department>> GetAllByCompanyAsync(Guid companyId, CancellationToken ct = default);
    Task<Department?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByCodeAsync(Guid companyId, string code, Guid? excludeId, CancellationToken ct = default);
    Task<bool> IsUsedByUsersAsync(Guid id, CancellationToken ct = default);
    void Add(Department entity);
    void Update(Department entity);
    void Remove(Department entity);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
