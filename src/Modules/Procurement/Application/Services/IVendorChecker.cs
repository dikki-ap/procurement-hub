namespace ProcureHub.Modules.Procurement.Application.Services;

public interface IVendorChecker
{
    Task<bool> IsBlacklistedAsync(Guid vendorId, CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> FilterActiveAsync(IEnumerable<Guid> vendorIds, CancellationToken ct = default);
}
