using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.UserManagement.Domain.Repositories;
using ProcureHub.SharedKernel.Database;
using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.UserManagement.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;

    public UserRepository(ApplicationDbContext db) => _db = db;

    public Task<List<User>> GetAllByCompanyAsync(Guid companyId, CancellationToken ct = default)
        => _db.Users
              .AsNoTracking()
              .Where(u => u.CompanyId == companyId)
              .OrderBy(u => u.FullName)
              .ToListAsync(ct);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public void Update(User entity) => _db.Users.Update(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
