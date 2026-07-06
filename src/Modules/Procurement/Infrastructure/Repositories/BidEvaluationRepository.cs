using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.Procurement.Infrastructure.Repositories;

public class BidEvaluationRepository : IBidEvaluationRepository
{
    private readonly ApplicationDbContext _db;

    public BidEvaluationRepository(ApplicationDbContext db) => _db = db;

    public Task<BidEvaluation?> GetByRFQIdAsync(Guid rfqId, CancellationToken ct = default)
        => _db.Set<BidEvaluation>()
              .FirstOrDefaultAsync(e => e.RFQId == rfqId, ct);

    public Task<BidEvaluation?> GetByRFQIdWithScoresAsync(Guid rfqId, CancellationToken ct = default)
        => _db.Set<BidEvaluation>()
              .Include(e => e.Scores)
              .FirstOrDefaultAsync(e => e.RFQId == rfqId, ct);

    public void Add(BidEvaluation evaluation)    => _db.Set<BidEvaluation>().Add(evaluation);
    public void Update(BidEvaluation evaluation) => _db.Set<BidEvaluation>().Update(evaluation);
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
