using ProcureHub.Modules.MasterData.Application.DTOs;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.MasterData.Application.Queries.GetLocationById;

public class GetLocationByIdQueryHandler : IQueryHandler<GetLocationByIdQuery, LocationDto>
{
    private readonly ILocationRepository _repo;
    private readonly ICacheService       _cache;

    public GetLocationByIdQueryHandler(ILocationRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public Task<LocationDto> Handle(GetLocationByIdQuery query, CancellationToken ct)
        => _cache.GetOrSetAsync(
            CacheKeys.Locations.ById(query.Id),
            async () =>
            {
                var loc = await _repo.GetByIdAsync(query.Id, ct)
                    ?? throw new NotFoundException("Location", query.Id);
                return new LocationDto(
                    loc.Id, loc.CompanyId, loc.Name, loc.Type,
                    loc.Address, loc.City, loc.Province, loc.Country, loc.IsActive,
                    loc.CreatedBy?.FullName, loc.CreatedAt, loc.UpdatedBy?.FullName, loc.UpdatedAt);
            },
            CacheTTL.Locations);
}
