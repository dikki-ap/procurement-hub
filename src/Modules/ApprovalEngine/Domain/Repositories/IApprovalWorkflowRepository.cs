using ProcureHub.Modules.ApprovalEngine.Domain.Entities;
using ProcureHub.Modules.ApprovalEngine.Domain.Enums;

namespace ProcureHub.Modules.ApprovalEngine.Domain.Repositories;

public interface IApprovalWorkflowRepository
{
    Task<ApprovalWorkflow?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApprovalWorkflow?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<ApprovalWorkflow?> GetByReferenceAsync(string referenceType, Guid referenceId, CancellationToken ct = default);
    Task<List<ApprovalWorkflow>> GetInboxAsync(Guid userId, Guid companyId, CancellationToken ct = default);
    Task<List<ApprovalWorkflow>> GetPendingEscalationsAsync(DateTime cutoffTime, CancellationToken ct = default);
    void Add(ApprovalWorkflow workflow);
    void Update(ApprovalWorkflow workflow);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
