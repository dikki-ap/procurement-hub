using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.Modules.VendorManagement.Domain.Repositories;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.VendorManagement.Infrastructure.Repositories;

public class VendorCapabilityRepository : IVendorCapabilityRepository
{
    private readonly ApplicationDbContext _db;

    public VendorCapabilityRepository(ApplicationDbContext db) => _db = db;

    public Task<List<VendorCapability>> GetByVendorIdAsync(Guid vendorId, CancellationToken ct = default)
        => _db.Set<VendorCapability>()
              .Where(c => c.VendorId == vendorId)
              .OrderBy(c => c.MaterialCategoryId)
              .ToListAsync(ct);

    public Task<VendorCapability?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Set<VendorCapability>().FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> ExistsAsync(Guid vendorId, Guid materialCategoryId, CancellationToken ct = default)
        => _db.Set<VendorCapability>()
              .AnyAsync(c => c.VendorId == vendorId && c.MaterialCategoryId == materialCategoryId, ct);

    public Task<List<VendorCapability>> GetExpiredAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return _db.Set<VendorCapability>()
                  .Where(c => c.ExpiryDate.HasValue && c.ExpiryDate.Value < today && !c.IsExpired)
                  .ToListAsync(ct);
    }

    public void Add(VendorCapability capability)    => _db.Set<VendorCapability>().Add(capability);
    public void Update(VendorCapability capability) => _db.Set<VendorCapability>().Update(capability);
    public void Remove(VendorCapability capability) => _db.Set<VendorCapability>().Remove(capability);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
