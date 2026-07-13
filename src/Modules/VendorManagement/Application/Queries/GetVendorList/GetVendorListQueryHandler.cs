using ProcureHub.Modules.VendorManagement.Application.DTOs;
using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.Modules.VendorManagement.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.VendorManagement.Application.Queries.GetVendorList;

public class GetVendorListQueryHandler : IQueryHandler<GetVendorListQuery, List<VendorDto>>
{
    private readonly IVendorRepository _repo;
    private readonly ICacheService     _cache;

    public GetVendorListQueryHandler(IVendorRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public Task<List<VendorDto>> Handle(GetVendorListQuery query, CancellationToken ct)
        => _cache.GetOrSetAsync(
            $"{CacheKeys.Vendors.List}:{query.CompanyId}",
            async () =>
            {
                var vendors = await _repo.GetAllAsync(query.CompanyId, ct);
                return vendors.Select(ToDto).ToList();
            },
            CacheTTL.VendorList);

    private static VendorDto ToDto(Vendor v) => new(
        v.Id,
        v.VendorCode,
        v.LegalName,
        v.TradeName,
        v.Npwp,
        v.Siup,
        v.Nib,
        v.VendorType,
        v.Status,
        v.Tier,
        v.Score,
        v.IsBlacklisted,
        v.BlacklistReason,
        v.ApprovedAt,
        v.CreatedAt,
        v.CreatedBy?.FullName,
        v.UpdatedBy?.FullName,
        v.UpdatedAt);
}
