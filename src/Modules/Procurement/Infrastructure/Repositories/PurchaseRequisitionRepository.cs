using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.Procurement.Infrastructure.Repositories;

public class PurchaseRequisitionRepository : IPurchaseRequisitionRepository
{
    private readonly ApplicationDbContext _db;

    public PurchaseRequisitionRepository(ApplicationDbContext db) => _db = db;

    public Task<List<PurchaseRequisition>> GetAllAsync(Guid companyId, CancellationToken ct = default)
        => _db.Set<PurchaseRequisition>()
              .Include(pr => pr.Items)
              .Include(pr => pr.CreatedBy)
              .Include(pr => pr.UpdatedBy)
              .Where(pr => pr.CompanyId == companyId)
              .OrderByDescending(pr => pr.CreatedAt)
              .ToListAsync(ct);

    public Task<PurchaseRequisition?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Set<PurchaseRequisition>()
              .FirstOrDefaultAsync(pr => pr.Id == id, ct);

    public Task<PurchaseRequisition?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default)
        => _db.Set<PurchaseRequisition>()
              .Include(pr => pr.Items)
              .FirstOrDefaultAsync(pr => pr.Id == id, ct);

    public async Task<string> GenerateNextNumberAsync(Guid companyId, CancellationToken ct = default)
    {
        var year  = DateTime.UtcNow.Year;
        var count = await _db.Set<PurchaseRequisition>()
                             .CountAsync(pr => pr.CompanyId == companyId
                                            && pr.CreatedAt.Year == year, ct);
        return $"PR-{year}-{(count + 1):D6}";
    }

    public void Add(PurchaseRequisition pr)    => _db.Set<PurchaseRequisition>().Add(pr);
    public void Update(PurchaseRequisition pr) => _db.Set<PurchaseRequisition>().Update(pr);
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
