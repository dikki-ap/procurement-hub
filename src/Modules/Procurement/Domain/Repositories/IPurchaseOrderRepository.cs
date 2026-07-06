using ProcureHub.Modules.Procurement.Domain.Entities;

namespace ProcureHub.Modules.Procurement.Domain.Repositories;

public interface IPurchaseOrderRepository
{
    Task<List<PurchaseOrder>> GetAllAsync(Guid companyId, CancellationToken ct = default);
    Task<List<PurchaseOrder>> GetByVendorAsync(Guid vendorId, CancellationToken ct = default);
    Task<PurchaseOrder?>      GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PurchaseOrder?>      GetByIdWithItemsAsync(Guid id, CancellationToken ct = default);
    Task<string>              GenerateNextNumberAsync(Guid companyId, CancellationToken ct = default);
    void                      Add(PurchaseOrder po);
    void                      Update(PurchaseOrder po);
    Task<int>                 SaveChangesAsync(CancellationToken ct = default);
}
