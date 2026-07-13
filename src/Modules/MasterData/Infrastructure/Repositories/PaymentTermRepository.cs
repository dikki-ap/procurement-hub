using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.MasterData.Domain.Entities;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.MasterData.Infrastructure.Repositories;

public class PaymentTermRepository : IPaymentTermRepository
{
    private readonly ApplicationDbContext _db;

    public PaymentTermRepository(ApplicationDbContext db) => _db = db;

    public Task<List<PaymentTerm>> GetAllAsync(Guid companyId, CancellationToken ct = default)
        => _db.Set<PaymentTerm>()
              .Where(e => e.CompanyId == companyId)
              .Include(e => e.CreatedBy)
              .Include(e => e.UpdatedBy)
              .OrderBy(e => e.Code)
              .ToListAsync(ct);

    public Task<PaymentTerm?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Set<PaymentTerm>().FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<bool> ExistsByCodeAsync(Guid companyId, string code, Guid? excludeId = null, CancellationToken ct = default)
        => _db.Set<PaymentTerm>()
              .AnyAsync(e => e.CompanyId == companyId
                          && e.Code == code
                          && (excludeId == null || e.Id != excludeId), ct);

    public void Add(PaymentTerm entity)    => _db.Set<PaymentTerm>().Add(entity);
    public void Update(PaymentTerm entity) => _db.Set<PaymentTerm>().Update(entity);
    public void Remove(PaymentTerm entity) => _db.Set<PaymentTerm>().Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
