using ProcureHub.Modules.MasterData.Application.DTOs;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Queries.GetMaterialCategoryList;

public class GetMaterialCategoryListQueryHandler : IQueryHandler<GetMaterialCategoryListQuery, List<MaterialCategoryDto>>
{
    private readonly IMaterialCategoryRepository _repo;
    private readonly ICacheService               _cache;

    public GetMaterialCategoryListQueryHandler(IMaterialCategoryRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public Task<List<MaterialCategoryDto>> Handle(GetMaterialCategoryListQuery query, CancellationToken ct)
        => _cache.GetOrSetAsync(
            $"{CacheKeys.MaterialCategories.List}:{query.CompanyId}",
            async () =>
            {
                var list = await _repo.GetAllAsync(query.CompanyId, ct);
                return list.Select(e => new MaterialCategoryDto(
                    e.Id, e.CompanyId, e.Name, e.Code, e.ParentId, e.IsStrategic, e.IsActive)).ToList();
            },
            CacheTTL.MaterialCategories);
}
