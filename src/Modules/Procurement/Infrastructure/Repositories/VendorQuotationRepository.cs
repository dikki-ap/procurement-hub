using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.Procurement.Infrastructure.Repositories;

public class VendorQuotationRepository : IVendorQuotationRepository
{
    private readonly ApplicationDbContext _db;

    public VendorQuotationRepository(ApplicationDbContext db) => _db = db;

    public Task<List<VendorQuotation>> GetByRFQIdAsync(Guid rfqId, CancellationToken ct = default)
        => _db.Set<VendorQuotation>()
              .Include(q => q.Items)
              .Where(q => q.RFQId == rfqId)
              .OrderBy(q => q.CreatedAt)
              .ToListAsync(ct);

    public Task<VendorQuotation?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Set<VendorQuotation>()
              .FirstOrDefaultAsync(q => q.Id == id, ct);

    public Task<VendorQuotation?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default)
        => _db.Set<VendorQuotation>()
              .Include(q => q.Items)
              .FirstOrDefaultAsync(q => q.Id == id, ct);

    public Task<VendorQuotation?> GetByRFQAndVendorAsync(Guid rfqId, Guid vendorId, CancellationToken ct = default)
        => _db.Set<VendorQuotation>()
              .Include(q => q.Items)
              .FirstOrDefaultAsync(q => q.RFQId == rfqId && q.VendorId == vendorId, ct);

    public Task<List<VendorQuotation>> GetByVendorIdAsync(Guid vendorId, CancellationToken ct = default)
        => _db.Set<VendorQuotation>()
              .Where(q => q.VendorId == vendorId)
              .OrderByDescending(q => q.CreatedAt)
              .ToListAsync(ct);

    public void Add(VendorQuotation quotation)    => _db.Set<VendorQuotation>().Add(quotation);
    public void Update(VendorQuotation quotation) => _db.Set<VendorQuotation>().Update(quotation);
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
