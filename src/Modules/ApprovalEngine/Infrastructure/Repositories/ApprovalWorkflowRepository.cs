using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.ApprovalEngine.Domain.Entities;
using ProcureHub.Modules.ApprovalEngine.Domain.Enums;
using ProcureHub.Modules.ApprovalEngine.Domain.Repositories;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.ApprovalEngine.Infrastructure.Repositories;

public class ApprovalWorkflowRepository : IApprovalWorkflowRepository
{
    private readonly ApplicationDbContext _db;

    public ApprovalWorkflowRepository(ApplicationDbContext db) => _db = db;

    public Task<ApprovalWorkflow?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Set<ApprovalWorkflow>().FirstOrDefaultAsync(w => w.Id == id, ct);

    public Task<ApprovalWorkflow?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
        => _db.Set<ApprovalWorkflow>()
              .Include(w => w.History)
              .Include(w => w.Assignments)
              .FirstOrDefaultAsync(w => w.Id == id, ct);

    public Task<ApprovalWorkflow?> GetByReferenceAsync(
        string referenceType, Guid referenceId, CancellationToken ct = default)
        => _db.Set<ApprovalWorkflow>()
              .FirstOrDefaultAsync(w => w.ReferenceType == referenceType
                                     && w.ReferenceId == referenceId, ct);

    public async Task<List<ApprovalWorkflow>> GetInboxAsync(
        Guid userId, Guid companyId, CancellationToken ct = default)
        => await _db.Set<ApprovalWorkflow>()
                    .Include(w => w.Assignments)
                    .Where(w => w.CompanyId == companyId
                             && w.Status == WorkflowStatus.Pending
                             && w.Assignments.Any(a => a.AssignedUserId == userId
                                                    && a.Level == w.CurrentLevel))
                    .OrderBy(w => w.CreatedAt)
                    .ToListAsync(ct);

    public async Task<List<ApprovalWorkflow>> GetPendingEscalationsAsync(
        DateTime cutoffTime, CancellationToken ct = default)
        => await _db.Set<ApprovalWorkflow>()
                    .Include(w => w.History)
                    .Include(w => w.Assignments)
                    .Where(w => w.Status == WorkflowStatus.Pending
                             && w.CreatedAt < cutoffTime
                             && (w.LastEscalationSentAt == null
                              || w.LastEscalationSentAt < cutoffTime))
                    .ToListAsync(ct);

    public void Add(ApprovalWorkflow workflow)    => _db.Set<ApprovalWorkflow>().Add(workflow);
    public void Update(ApprovalWorkflow workflow) => _db.Set<ApprovalWorkflow>().Update(workflow);
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
