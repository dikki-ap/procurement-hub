using ProcureHub.Modules.Procurement.Domain.Entities;

namespace ProcureHub.Modules.Procurement.Domain.Repositories;

public interface IGoodsReceiptRepository
{
    Task<List<GoodsReceipt>> GetByPOAsync(Guid poId, CancellationToken ct = default);
    Task<GoodsReceipt?>      GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<GoodsReceipt?>      GetByIdWithItemsAsync(Guid id, CancellationToken ct = default);
    Task<string>             GenerateNextNumberAsync(CancellationToken ct = default);
    void                     Add(GoodsReceipt grn);
    void                     Update(GoodsReceipt grn);
    Task<int>                SaveChangesAsync(CancellationToken ct = default);
}
