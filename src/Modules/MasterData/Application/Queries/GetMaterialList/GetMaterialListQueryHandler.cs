using ProcureHub.Modules.MasterData.Application.DTOs;
using ProcureHub.Modules.MasterData.Domain.Entities;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Queries.GetMaterialList;

public class GetMaterialListQueryHandler : IQueryHandler<GetMaterialListQuery, List<MaterialDto>>
{
    private readonly IMaterialRepository _repo;
    private readonly ICacheService       _cache;

    public GetMaterialListQueryHandler(IMaterialRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public Task<List<MaterialDto>> Handle(GetMaterialListQuery query, CancellationToken ct)
        => _cache.GetOrSetAsync(
            CacheKeys.Materials.List,
            async () =>
            {
                var list = await _repo.GetAllAsync(ct);
                return list.Select(ToDto).ToList();
            },
            CacheTTL.MaterialList);

    private static MaterialDto ToDto(Material m)
        => new(m.Id, m.CategoryId, m.Category?.Name ?? string.Empty,
               m.Code, m.Name, m.Description,
               m.UomId, m.Uom?.Code ?? string.Empty,
               m.EstimatedPrice, m.CurrencyId, m.Currency?.Code,
               m.IsStrategic, m.IsActive,
               m.CreatedBy?.FullName, m.CreatedAt, m.UpdatedBy?.FullName, m.UpdatedAt);
}
