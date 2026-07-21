using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.CompanyManagement.Domain.Repositories;

public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(Guid id, CancellationToken ct = default);
    void Update(Company entity);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
