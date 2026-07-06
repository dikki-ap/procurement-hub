using ProcureHub.Modules.ApprovalEngine.Domain.Entities;

namespace ProcureHub.Modules.ApprovalEngine.Domain.Repositories;

public interface IApprovalPolicyRepository
{
    Task<List<ApprovalPolicy>> GetByCompanyAsync(Guid companyId, CancellationToken ct = default);
    Task<ApprovalPolicy?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid companyId, string referenceType, decimal minValue, CancellationToken ct = default);
    void Add(ApprovalPolicy policy);
    void Update(ApprovalPolicy policy);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
