using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.Procurement.Infrastructure.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly ApplicationDbContext _db;

    public InvoiceRepository(ApplicationDbContext db) => _db = db;

    public Task<List<Invoice>> GetAllAsync(CancellationToken ct = default)
        => _db.Set<Invoice>()
              .Include(i => i.CreatedBy)
              .Include(i => i.UpdatedBy)
              .Include(i => i.PurchaseOrder).ThenInclude(p => p!.Vendor)
              .OrderByDescending(i => i.SubmittedAt)
              .ToListAsync(ct);

    public Task<Invoice?> GetByIdWithPOAsync(Guid id, CancellationToken ct = default)
        => _db.Set<Invoice>()
              .Include(i => i.PurchaseOrder).ThenInclude(p => p!.Vendor)
              .FirstOrDefaultAsync(i => i.Id == id, ct);

    public Task<List<Invoice>> GetByPOAsync(Guid poId, CancellationToken ct = default)
        => _db.Set<Invoice>().Where(i => i.POId == poId).ToListAsync(ct);

    public Task<List<Invoice>> GetByVendorAsync(Guid vendorId, CancellationToken ct = default)
        => _db.Set<Invoice>().Where(i => i.VendorId == vendorId).OrderByDescending(i => i.SubmittedAt).ToListAsync(ct);

    public Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Set<Invoice>().FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task<string> GenerateNextNumberAsync(CancellationToken ct = default)
    {
        var year  = DateTime.UtcNow.Year;
        var count = await _db.Set<Invoice>().CountAsync(i => i.SubmittedAt.Year == year, ct);
        return $"INV-{year}-{(count + 1):D6}";
    }

    public void Add(Invoice invoice)    => _db.Set<Invoice>().Add(invoice);
    public void Update(Invoice invoice) => _db.Set<Invoice>().Update(invoice);
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
