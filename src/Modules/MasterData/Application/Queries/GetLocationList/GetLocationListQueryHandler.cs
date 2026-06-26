using ProcureHub.Modules.MasterData.Application.DTOs;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Queries.GetLocationList;

public class GetLocationListQueryHandler : IQueryHandler<GetLocationListQuery, List<LocationDto>>
{
    private readonly ILocationRepository _repo;
    private readonly ICacheService       _cache;

    public GetLocationListQueryHandler(ILocationRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public Task<List<LocationDto>> Handle(GetLocationListQuery query, CancellationToken ct)
        => _cache.GetOrSetAsync(
            $"{CacheKeys.Locations.List}:{query.CompanyId}",
            async () =>
            {
                var list = await _repo.GetAllAsync(query.CompanyId, ct);
                return list.Select(e => new LocationDto(
                    e.Id, e.CompanyId, e.Name, e.Type,
                    e.Address, e.City, e.Province, e.Country, e.IsActive)).ToList();
            },
            CacheTTL.Locations);
}
