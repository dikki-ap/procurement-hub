using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.Modules.VendorManagement.Domain.Repositories;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.VendorManagement.Infrastructure.Repositories;

public class VendorRepository : IVendorRepository
{
    private readonly ApplicationDbContext _db;

    public VendorRepository(ApplicationDbContext db) => _db = db;

    public Task<List<Vendor>> GetAllAsync(Guid companyId, CancellationToken ct = default)
        => _db.Set<Vendor>()
              .Include(v => v.CreatedBy)
              .Include(v => v.UpdatedBy)
              .Where(v => v.CompanyId == companyId)
              .OrderBy(v => v.LegalName)
              .ToListAsync(ct);

    public Task<Vendor?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Set<Vendor>()
              .FirstOrDefaultAsync(v => v.Id == id, ct);

    public Task<Vendor?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
        => _db.Set<Vendor>()
              .Include(v => v.Contacts)
              .Include(v => v.Documents)
              .Include(v => v.Capabilities)
              .FirstOrDefaultAsync(v => v.Id == id, ct);

    public Task<List<Vendor>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        => _db.Set<Vendor>()
              .Where(v => ids.Contains(v.Id))
              .ToListAsync(ct);

    public Task<bool> ExistsByCodeAsync(Guid companyId, string vendorCode, Guid? excludeId = null, CancellationToken ct = default)
        => _db.Set<Vendor>()
              .AnyAsync(v => v.CompanyId == companyId
                          && v.VendorCode == vendorCode
                          && (excludeId == null || v.Id != excludeId), ct);

    public async Task<string> GenerateNextCodeAsync(Guid companyId, CancellationToken ct = default)
    {
        var year  = DateTime.UtcNow.Year;
        var count = await _db.Set<Vendor>()
                             .CountAsync(v => v.CompanyId == companyId && v.CreatedAt.Year == year, ct);
        return $"VND-{year}-{(count + 1):D6}";
    }

    public void Add(Vendor vendor)    => _db.Set<Vendor>().Add(vendor);
    public void Update(Vendor vendor) => _db.Set<Vendor>().Update(vendor);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
