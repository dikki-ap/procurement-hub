using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.CompanyManagement.Domain.Repositories;
using ProcureHub.SharedKernel.Database;
using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.CompanyManagement.Infrastructure.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly ApplicationDbContext _db;

    public CompanyRepository(ApplicationDbContext db) => _db = db;

    public Task<Company?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Companies.FirstOrDefaultAsync(c => c.Id == id, ct);

    public void Update(Company entity) => _db.Companies.Update(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
