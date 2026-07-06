using ProcureHub.Modules.Procurement.Domain.Entities;

namespace ProcureHub.Modules.Procurement.Domain.Repositories;

public interface IRFQRepository
{
    Task<List<RFQ>> GetAllAsync(Guid companyId, CancellationToken ct = default);
    Task<List<RFQ>> GetOpenWithVendorsAsync(CancellationToken ct = default);
    Task<RFQ?>      GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<RFQ?>      GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<string>    GenerateNextNumberAsync(Guid companyId, CancellationToken ct = default);
    void            Add(RFQ rfq);
    void            Update(RFQ rfq);
    Task<int>       SaveChangesAsync(CancellationToken ct = default);
}
