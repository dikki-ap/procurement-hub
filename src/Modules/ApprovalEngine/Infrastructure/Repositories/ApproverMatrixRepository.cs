using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.ApprovalEngine.Domain.Entities;
using ProcureHub.Modules.ApprovalEngine.Domain.Repositories;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.ApprovalEngine.Infrastructure.Repositories;

public class ApproverMatrixRepository : IApproverMatrixRepository
{
    private readonly ApplicationDbContext _db;

    public ApproverMatrixRepository(ApplicationDbContext db) => _db = db;

    public Task<List<ApproverMatrixEntry>> GetAllByCompanyAsync(Guid companyId, CancellationToken ct = default)
        => _db.Set<ApproverMatrixEntry>()
              .Include(e => e.CreatedBy)
              .Include(e => e.UpdatedBy)
              .Where(e => e.CompanyId == companyId)
              .OrderBy(e => e.ReferenceType).ThenBy(e => e.Level)
              .ToListAsync(ct);

    public Task<List<ApproverMatrixEntry>> GetByCompanyAndTypeAsync(
        Guid companyId, string referenceType, int maxLevel, CancellationToken ct = default)
        => _db.Set<ApproverMatrixEntry>()
              .Where(e => e.CompanyId == companyId
                       && e.ReferenceType == referenceType
                       && e.Level <= maxLevel)
              .OrderBy(e => e.Level)
              .ToListAsync(ct);

    public Task<ApproverMatrixEntry?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Set<ApproverMatrixEntry>().FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<bool> ExistsAsync(
        Guid companyId, string referenceType, int level, string email,
        Guid? excludeId, CancellationToken ct = default)
        => _db.Set<ApproverMatrixEntry>()
              .AnyAsync(e => e.CompanyId == companyId
                          && e.ReferenceType == referenceType
                          && e.Level == level
                          && e.Email == email
                          && (excludeId == null || e.Id != excludeId.Value), ct);

    public Task<bool> IsApproverAsync(Guid companyId, string email, CancellationToken ct = default)
        => _db.Set<ApproverMatrixEntry>()
              .AnyAsync(e => e.CompanyId == companyId && e.Email == email, ct);

    public void Add(ApproverMatrixEntry entry)    => _db.Set<ApproverMatrixEntry>().Add(entry);
    public void Update(ApproverMatrixEntry entry) => _db.Set<ApproverMatrixEntry>().Update(entry);
    public void Remove(ApproverMatrixEntry entry) => _db.Set<ApproverMatrixEntry>().Remove(entry);

    public async Task SaveChangesAsync(CancellationToken ct = default) => await _db.SaveChangesAsync(ct);
}
