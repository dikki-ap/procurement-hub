using ProcureHub.Modules.ApprovalEngine.Domain.Entities;

namespace ProcureHub.Modules.ApprovalEngine.Domain.Repositories;

public interface IApproverMatrixRepository
{
    Task<List<ApproverMatrixEntry>> GetAllByCompanyAsync(Guid companyId, CancellationToken ct = default);
    Task<List<ApproverMatrixEntry>> GetByCompanyAndTypeAsync(Guid companyId, string referenceType, int maxLevel, CancellationToken ct = default);
    Task<ApproverMatrixEntry?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid companyId, string referenceType, int level, string email, Guid? excludeId, CancellationToken ct = default);
    Task<bool> IsApproverAsync(Guid companyId, string email, CancellationToken ct = default);
    void Add(ApproverMatrixEntry entry);
    void Update(ApproverMatrixEntry entry);
    void Remove(ApproverMatrixEntry entry);
    Task SaveChangesAsync(CancellationToken ct = default);
}
