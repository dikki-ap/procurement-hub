using ProcureHub.Modules.Procurement.Domain.Entities;

namespace ProcureHub.Modules.Procurement.Domain.Repositories;

public interface IBidEvaluationRepository
{
    Task<BidEvaluation?> GetByRFQIdAsync(Guid rfqId, CancellationToken ct = default);
    Task<BidEvaluation?> GetByRFQIdWithScoresAsync(Guid rfqId, CancellationToken ct = default);
    void Add(BidEvaluation evaluation);
    void Update(BidEvaluation evaluation);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
