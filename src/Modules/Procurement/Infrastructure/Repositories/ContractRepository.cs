using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.Procurement.Infrastructure.Repositories;

public class ContractRepository : IContractRepository
{
    private readonly ApplicationDbContext _db;

    public ContractRepository(ApplicationDbContext db) => _db = db;

    public Task<List<Contract>> GetAllAsync(Guid companyId, CancellationToken ct = default)
        => _db.Set<Contract>()
              .Where(c => c.CompanyId == companyId)
              .OrderByDescending(c => c.CreatedAt)
              .ToListAsync(ct);

    public Task<List<Contract>> GetByVendorAsync(Guid vendorId, CancellationToken ct = default)
        => _db.Set<Contract>()
              .Where(c => c.VendorId == vendorId)
              .OrderByDescending(c => c.CreatedAt)
              .ToListAsync(ct);

    public Task<Contract?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Set<Contract>().FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<List<Contract>> GetExpiringAsync(DateTime threshold, CancellationToken ct = default)
        => _db.Set<Contract>()
              .Where(c => c.Status == ContractStatus.Active
                       && c.EndDate.HasValue
                       && c.EndDate.Value <= threshold)
              .ToListAsync(ct);

    public Task<List<Contract>> GetExpiredActiveAsync(CancellationToken ct = default)
        => _db.Set<Contract>()
              .Where(c => c.Status == ContractStatus.Active
                       && c.EndDate.HasValue
                       && c.EndDate.Value < DateTime.UtcNow)
              .ToListAsync(ct);

    public async Task<string> GenerateNextNumberAsync(Guid companyId, CancellationToken ct = default)
    {
        var year  = DateTime.UtcNow.Year;
        var count = await _db.Set<Contract>()
                             .Where(c => c.CompanyId == companyId && c.CreatedAt.Year == year)
                             .CountAsync(ct);
        return $"CONT-{year}-{(count + 1):D5}";
    }

    public void Add(Contract contract)    => _db.Set<Contract>().Add(contract);
    public void Update(Contract contract) => _db.Set<Contract>().Update(contract);
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
