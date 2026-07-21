using ProcureHub.Modules.VendorManagement.Domain.Entities;

namespace ProcureHub.Modules.VendorManagement.Domain.Repositories;

public interface IVendorCapabilityRepository
{
    Task<List<VendorCapability>> GetByVendorIdAsync(Guid vendorId, CancellationToken ct = default);
    Task<VendorCapability?>      GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool>                   ExistsAsync(Guid vendorId, Guid materialCategoryId, CancellationToken ct = default);
    Task<List<VendorCapability>> GetExpiredAsync(CancellationToken ct = default);
    void                         Add(VendorCapability capability);
    void                         Update(VendorCapability capability);
    void                         Remove(VendorCapability capability);
    Task<int>                    SaveChangesAsync(CancellationToken ct = default);
}
