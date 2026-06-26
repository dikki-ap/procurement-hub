using ProcureHub.Modules.MasterData.Domain.Entities;

namespace ProcureHub.Modules.MasterData.Domain.Repositories;

public interface IPaymentTermRepository
{
    Task<List<PaymentTerm>> GetAllAsync(Guid companyId, CancellationToken ct = default);
    Task<PaymentTerm?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByCodeAsync(Guid companyId, string code, Guid? excludeId = null, CancellationToken ct = default);
    void Add(PaymentTerm entity);
    void Update(PaymentTerm entity);
    void Remove(PaymentTerm entity);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
