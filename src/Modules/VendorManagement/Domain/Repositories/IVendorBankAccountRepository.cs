using ProcureHub.Modules.VendorManagement.Domain.Entities;

namespace ProcureHub.Modules.VendorManagement.Domain.Repositories;

public interface IVendorBankAccountRepository
{
    Task<List<VendorBankAccount>> GetByVendorIdAsync(Guid vendorId, CancellationToken ct = default);
    Task<VendorBankAccount?>      GetByIdAsync(Guid id, CancellationToken ct = default);
    void                          Add(VendorBankAccount bankAccount);
    void                          Update(VendorBankAccount bankAccount);
    void                          Remove(VendorBankAccount bankAccount);
    Task<int>                     SaveChangesAsync(CancellationToken ct = default);
}
