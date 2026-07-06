using ProcureHub.Modules.VendorManagement.Domain.Entities;

namespace ProcureHub.Modules.VendorManagement.Domain.Repositories;

public interface IVendorRepository
{
    Task<List<Vendor>> GetAllAsync(Guid companyId, CancellationToken ct = default);
    Task<Vendor?>      GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Vendor?>      GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<List<Vendor>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<bool>         ExistsByCodeAsync(Guid companyId, string vendorCode, Guid? excludeId = null, CancellationToken ct = default);
    Task<string>       GenerateNextCodeAsync(Guid companyId, CancellationToken ct = default);
    void               Add(Vendor vendor);
    void               Update(Vendor vendor);
    Task<int>          SaveChangesAsync(CancellationToken ct = default);
}
