using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.Procurement.Infrastructure.Repositories;

public class ReturnOrderRepository : IReturnOrderRepository
{
    private readonly ApplicationDbContext _db;

    public ReturnOrderRepository(ApplicationDbContext db) => _db = db;

    public Task<List<ReturnOrder>> GetAllAsync(Guid companyId, CancellationToken ct = default)
        => _db.Set<ReturnOrder>()
              .Include(r => r.Items)
              .Where(r => r.GoodsReceipt != null && r.GoodsReceipt.PurchaseOrder != null
                       && r.GoodsReceipt.PurchaseOrder.CompanyId == companyId)
              .OrderByDescending(r => r.CreatedAt)
              .ToListAsync(ct);

    public Task<List<ReturnOrder>> GetByVendorAsync(Guid vendorId, CancellationToken ct = default)
        => _db.Set<ReturnOrder>()
              .Include(r => r.Items)
              .Where(r => r.VendorId == vendorId)
              .OrderByDescending(r => r.CreatedAt)
              .ToListAsync(ct);

    public Task<ReturnOrder?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Set<ReturnOrder>().FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<ReturnOrder?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default)
        => _db.Set<ReturnOrder>()
              .Include(r => r.Items)
              .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<string> GenerateNextNumberAsync(CancellationToken ct = default)
    {
        var year  = DateTime.UtcNow.Year;
        var count = await _db.Set<ReturnOrder>().CountAsync(r => r.CreatedAt.Year == year, ct);
        return $"RET-{year}-{(count + 1):D6}";
    }

    public void Add(ReturnOrder entity)    => _db.Set<ReturnOrder>().Add(entity);
    public void Update(ReturnOrder entity) => _db.Set<ReturnOrder>().Update(entity);
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
