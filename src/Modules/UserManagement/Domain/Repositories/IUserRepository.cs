using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.UserManagement.Domain.Repositories;

public interface IUserRepository
{
    Task<List<User>> GetAllByCompanyAsync(Guid companyId, CancellationToken ct = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    void Update(User entity);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
