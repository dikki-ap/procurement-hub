using ProcureHub.Modules.VendorManagement.Domain.Entities;

namespace ProcureHub.Modules.VendorManagement.Domain.Repositories;

public interface IVendorScoreRepository
{
    Task<VendorScore?> GetCurrentAsync(Guid vendorId, CancellationToken ct = default);
    Task<List<VendorScore>> GetHistoryAsync(Guid vendorId, CancellationToken ct = default);
    void Add(VendorScore score);
    void Update(VendorScore score);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
