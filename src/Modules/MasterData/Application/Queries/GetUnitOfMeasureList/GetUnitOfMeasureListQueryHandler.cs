using ProcureHub.Modules.MasterData.Application.DTOs;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Queries.GetUnitOfMeasureList;

public class GetUnitOfMeasureListQueryHandler : IQueryHandler<GetUnitOfMeasureListQuery, List<UnitOfMeasureDto>>
{
    private readonly IUnitOfMeasureRepository _repo;
    private readonly ICacheService            _cache;

    public GetUnitOfMeasureListQueryHandler(IUnitOfMeasureRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public Task<List<UnitOfMeasureDto>> Handle(GetUnitOfMeasureListQuery query, CancellationToken ct)
        => _cache.GetOrSetAsync(
            $"{CacheKeys.UnitOfMeasures.List}:{query.CompanyId}",
            async () =>
            {
                var list = await _repo.GetAllAsync(query.CompanyId, ct);
                return list.Select(e => new UnitOfMeasureDto(e.Id, e.CompanyId, e.Code, e.Name, e.IsActive)).ToList();
            },
            CacheTTL.UnitOfMeasures);
}
