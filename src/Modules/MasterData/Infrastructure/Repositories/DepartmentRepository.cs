using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Database;
using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.MasterData.Infrastructure.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly ApplicationDbContext _db;

    public DepartmentRepository(ApplicationDbContext db) => _db = db;

    public Task<List<Department>> GetAllByCompanyAsync(Guid companyId, CancellationToken ct = default)
        => _db.Departments
              .Where(d => d.CompanyId == companyId)
              .OrderBy(d => d.Name)
              .ToListAsync(ct);

    public Task<Department?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Departments.FirstOrDefaultAsync(d => d.Id == id, ct);

    public Task<bool> ExistsByCodeAsync(Guid companyId, string code, Guid? excludeId, CancellationToken ct = default)
        => _db.Departments.AnyAsync(
            d => d.CompanyId == companyId && d.Code == code && (excludeId == null || d.Id != excludeId), ct);

    public Task<bool> IsUsedByUsersAsync(Guid id, CancellationToken ct = default)
        => _db.Users.AnyAsync(u => u.DepartmentId == id, ct);

    public void Add(Department entity)    => _db.Departments.Add(entity);
    public void Update(Department entity) => _db.Departments.Update(entity);
    public void Remove(Department entity) => _db.Departments.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
