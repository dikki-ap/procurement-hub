using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Enums;

namespace ProcureHub.Modules.Procurement.Domain.Repositories;

public interface IPurchaseRequisitionRepository
{
    Task<List<PurchaseRequisition>> GetAllAsync(Guid companyId, CancellationToken ct = default);
    Task<PurchaseRequisition?>      GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PurchaseRequisition?>      GetByIdWithItemsAsync(Guid id, CancellationToken ct = default);
    Task<string>                    GenerateNextNumberAsync(Guid companyId, CancellationToken ct = default);
    void                            Add(PurchaseRequisition pr);
    void                            Update(PurchaseRequisition pr);
    Task<int>                       SaveChangesAsync(CancellationToken ct = default);
}
