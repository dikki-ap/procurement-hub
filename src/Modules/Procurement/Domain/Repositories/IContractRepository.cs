using ProcureHub.Modules.Procurement.Domain.Entities;

namespace ProcureHub.Modules.Procurement.Domain.Repositories;

public interface IContractRepository
{
    Task<List<Contract>> GetAllAsync(Guid companyId, CancellationToken ct = default);
    Task<List<Contract>> GetByVendorAsync(Guid vendorId, CancellationToken ct = default);
    Task<Contract?>      GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Contract>> GetExpiringAsync(DateTime threshold, CancellationToken ct = default);
    Task<List<Contract>> GetExpiredActiveAsync(CancellationToken ct = default);
    Task<string>         GenerateNextNumberAsync(Guid companyId, CancellationToken ct = default);
    void                 Add(Contract contract);
    void                 Update(Contract contract);
    Task<int>            SaveChangesAsync(CancellationToken ct = default);
}
