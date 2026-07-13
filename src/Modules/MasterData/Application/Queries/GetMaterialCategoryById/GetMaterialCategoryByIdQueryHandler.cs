using ProcureHub.Modules.MasterData.Application.DTOs;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.MasterData.Application.Queries.GetMaterialCategoryById;

public class GetMaterialCategoryByIdQueryHandler : IQueryHandler<GetMaterialCategoryByIdQuery, MaterialCategoryDto>
{
    private readonly IMaterialCategoryRepository _repo;
    private readonly ICacheService               _cache;

    public GetMaterialCategoryByIdQueryHandler(IMaterialCategoryRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public Task<MaterialCategoryDto> Handle(GetMaterialCategoryByIdQuery query, CancellationToken ct)
        => _cache.GetOrSetAsync(
            CacheKeys.MaterialCategories.ById(query.Id),
            async () =>
            {
                var cat = await _repo.GetByIdAsync(query.Id, ct)
                    ?? throw new NotFoundException("MaterialCategory", query.Id);
                return new MaterialCategoryDto(
                    cat.Id, cat.CompanyId, cat.Name, cat.Code, cat.ParentId, cat.IsStrategic, cat.IsActive,
                    cat.CreatedBy?.FullName, cat.CreatedAt, cat.UpdatedBy?.FullName, cat.UpdatedAt);
            },
            CacheTTL.MaterialCategories);
}
