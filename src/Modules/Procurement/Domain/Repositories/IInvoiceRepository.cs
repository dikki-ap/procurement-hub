using ProcureHub.Modules.Procurement.Domain.Entities;

namespace ProcureHub.Modules.Procurement.Domain.Repositories;

public interface IInvoiceRepository
{
    Task<List<Invoice>> GetAllAsync(CancellationToken ct = default);
    Task<List<Invoice>> GetByPOAsync(Guid poId, CancellationToken ct = default);
    Task<List<Invoice>> GetByVendorAsync(Guid vendorId, CancellationToken ct = default);
    Task<Invoice?>      GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<string>        GenerateNextNumberAsync(CancellationToken ct = default);
    void                Add(Invoice invoice);
    void                Update(Invoice invoice);
    Task<int>           SaveChangesAsync(CancellationToken ct = default);
}
