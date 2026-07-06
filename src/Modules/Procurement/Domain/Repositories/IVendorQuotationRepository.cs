using ProcureHub.Modules.Procurement.Domain.Entities;

namespace ProcureHub.Modules.Procurement.Domain.Repositories;

public interface IVendorQuotationRepository
{
    Task<List<VendorQuotation>> GetByRFQIdAsync(Guid rfqId, CancellationToken ct = default);
    Task<VendorQuotation?>      GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<VendorQuotation?>      GetByIdWithItemsAsync(Guid id, CancellationToken ct = default);
    Task<VendorQuotation?>      GetByRFQAndVendorAsync(Guid rfqId, Guid vendorId, CancellationToken ct = default);
    Task<List<VendorQuotation>> GetByVendorIdAsync(Guid vendorId, CancellationToken ct = default);
    void Add(VendorQuotation quotation);
    void Update(VendorQuotation quotation);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
