using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.Procurement.Infrastructure.Repositories;

public class GoodsReceiptRepository : IGoodsReceiptRepository
{
    private readonly ApplicationDbContext _db;

    public GoodsReceiptRepository(ApplicationDbContext db) => _db = db;

    public Task<List<GoodsReceipt>> GetByPOAsync(Guid poId, CancellationToken ct = default)
        => _db.Set<GoodsReceipt>()
              .Where(g => g.POId == poId)
              .OrderByDescending(g => g.CreatedAt)
              .ToListAsync(ct);

    public Task<GoodsReceipt?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Set<GoodsReceipt>().FirstOrDefaultAsync(g => g.Id == id, ct);

    public Task<GoodsReceipt?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default)
        => _db.Set<GoodsReceipt>()
              .Include(g => g.Items)
              .FirstOrDefaultAsync(g => g.Id == id, ct);

    public async Task<string> GenerateNextNumberAsync(CancellationToken ct = default)
    {
        var year  = DateTime.UtcNow.Year;
        var count = await _db.Set<GoodsReceipt>().CountAsync(g => g.CreatedAt.Year == year, ct);
        return $"GRN-{year}-{(count + 1):D6}";
    }

    public void Add(GoodsReceipt grn)    => _db.Set<GoodsReceipt>().Add(grn);
    public void Update(GoodsReceipt grn) => _db.Set<GoodsReceipt>().Update(grn);
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
