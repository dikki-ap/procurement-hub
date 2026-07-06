using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.ApprovalEngine.Domain.Entities;
using ProcureHub.Modules.ApprovalEngine.Domain.Repositories;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.ApprovalEngine.Infrastructure.Repositories;

public class ApprovalPolicyRepository : IApprovalPolicyRepository
{
    private readonly ApplicationDbContext _db;

    public ApprovalPolicyRepository(ApplicationDbContext db) => _db = db;

    public Task<List<ApprovalPolicy>> GetByCompanyAsync(Guid companyId, CancellationToken ct = default)
        => _db.Set<ApprovalPolicy>()
              .Where(p => p.CompanyId == companyId)
              .OrderBy(p => p.ReferenceType).ThenBy(p => p.MinValue)
              .ToListAsync(ct);

    public Task<ApprovalPolicy?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Set<ApprovalPolicy>().FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<bool> ExistsAsync(Guid companyId, string referenceType, decimal minValue, CancellationToken ct = default)
        => _db.Set<ApprovalPolicy>()
              .AnyAsync(p => p.CompanyId == companyId
                          && p.ReferenceType == referenceType
                          && p.MinValue == minValue, ct);

    public void Add(ApprovalPolicy policy)    => _db.Set<ApprovalPolicy>().Add(policy);
    public void Update(ApprovalPolicy policy) => _db.Set<ApprovalPolicy>().Update(policy);
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
