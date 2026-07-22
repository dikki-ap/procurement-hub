using ProcureHub.Modules.Procurement.Domain.Entities;

namespace ProcureHub.Modules.Procurement.Domain.Repositories;

public interface IReturnOrderRepository
{
    Task<List<ReturnOrder>> GetAllAsync(Guid companyId, CancellationToken ct = default);
    Task<List<ReturnOrder>> GetByVendorAsync(Guid vendorId, CancellationToken ct = default);
    Task<ReturnOrder?>      GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ReturnOrder?>      GetByIdWithItemsAsync(Guid id, CancellationToken ct = default);
    Task<string>            GenerateNextNumberAsync(CancellationToken ct = default);
    void                    Add(ReturnOrder entity);
    void                    Update(ReturnOrder entity);
    Task<int>               SaveChangesAsync(CancellationToken ct = default);
}
