using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.Modules.VendorManagement.Domain.Repositories;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.VendorManagement.Infrastructure.Repositories;

public class VendorBankAccountRepository : IVendorBankAccountRepository
{
    private readonly ApplicationDbContext _db;

    public VendorBankAccountRepository(ApplicationDbContext db) => _db = db;

    public Task<List<VendorBankAccount>> GetByVendorIdAsync(Guid vendorId, CancellationToken ct = default)
        => _db.Set<VendorBankAccount>()
              .Where(b => b.VendorId == vendorId)
              .OrderByDescending(b => b.IsDefault)
              .ThenBy(b => b.BankName)
              .ToListAsync(ct);

    public Task<VendorBankAccount?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Set<VendorBankAccount>().FirstOrDefaultAsync(b => b.Id == id, ct);

    public void Add(VendorBankAccount bankAccount)    => _db.Set<VendorBankAccount>().Add(bankAccount);
    public void Update(VendorBankAccount bankAccount) => _db.Set<VendorBankAccount>().Update(bankAccount);
    public void Remove(VendorBankAccount bankAccount) => _db.Set<VendorBankAccount>().Remove(bankAccount);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
