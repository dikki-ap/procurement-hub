using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.Procurement.Infrastructure.Repositories;

public class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly ApplicationDbContext _db;

    public PurchaseOrderRepository(ApplicationDbContext db) => _db = db;

    public Task<List<PurchaseOrder>> GetAllAsync(Guid companyId, CancellationToken ct = default)
        => _db.Set<PurchaseOrder>()
              .Include(p => p.CreatedBy)
              .Include(p => p.UpdatedBy)
              .Where(p => p.CompanyId == companyId)
              .OrderByDescending(p => p.CreatedAt)
              .ToListAsync(ct);

    public Task<List<PurchaseOrder>> GetByVendorAsync(Guid vendorId, CancellationToken ct = default)
        => _db.Set<PurchaseOrder>()
              .Where(p => p.VendorId == vendorId)
              .OrderByDescending(p => p.CreatedAt)
              .ToListAsync(ct);

    public Task<List<PurchaseOrder>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        => _db.Set<PurchaseOrder>()
              .Where(p => ids.Contains(p.Id))
              .ToListAsync(ct);

    public Task<PurchaseOrder?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Set<PurchaseOrder>().FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<PurchaseOrder?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default)
        => _db.Set<PurchaseOrder>()
              .Include(p => p.Items)
              .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<string> GenerateNextNumberAsync(Guid companyId, CancellationToken ct = default)
    {
        var year  = DateTime.UtcNow.Year;
        var count = await _db.Set<PurchaseOrder>()
                             .CountAsync(p => p.CompanyId == companyId
                                           && p.CreatedAt.Year == year, ct);
        return $"PO-{year}-{(count + 1):D6}";
    }

    public void Add(PurchaseOrder po)    => _db.Set<PurchaseOrder>().Add(po);
    public void Update(PurchaseOrder po) => _db.Set<PurchaseOrder>().Update(po);
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
