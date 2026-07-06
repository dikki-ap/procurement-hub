using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.Procurement.Application.Services;
using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.Modules.VendorManagement.Domain.Enums;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.Procurement.Infrastructure.Services;

public class VendorChecker : IVendorChecker
{
    private readonly ApplicationDbContext _db;

    public VendorChecker(ApplicationDbContext db) => _db = db;

    public Task<bool> IsBlacklistedAsync(Guid vendorId, CancellationToken ct = default)
        => _db.Set<Vendor>()
              .AnyAsync(v => v.Id == vendorId && v.IsBlacklisted, ct);

    public async Task<IReadOnlyList<Guid>> FilterActiveAsync(
        IEnumerable<Guid> vendorIds,
        CancellationToken ct = default)
    {
        var ids = vendorIds.ToList();
        return await _db.Set<Vendor>()
                        .Where(v => ids.Contains(v.Id) && v.Status == VendorStatus.Active)
                        .Select(v => v.Id)
                        .ToListAsync(ct);
    }
}
