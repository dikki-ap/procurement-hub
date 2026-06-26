using ProcureHub.Modules.MasterData.Application.DTOs;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.MasterData.Application.Queries.GetMaterialById;

public class GetMaterialByIdQueryHandler : IQueryHandler<GetMaterialByIdQuery, MaterialDto>
{
    private readonly IMaterialRepository _repo;
    private readonly ICacheService       _cache;

    public GetMaterialByIdQueryHandler(IMaterialRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public Task<MaterialDto> Handle(GetMaterialByIdQuery query, CancellationToken ct)
        => _cache.GetOrSetAsync(
            CacheKeys.Materials.ById(query.Id),
            async () =>
            {
                var m = await _repo.GetByIdAsync(query.Id, ct)
                    ?? throw new NotFoundException("Material", query.Id);
                return new MaterialDto(
                    m.Id, m.CategoryId, m.Category?.Name ?? string.Empty,
                    m.Code, m.Name, m.Description,
                    m.UomId, m.Uom?.Code ?? string.Empty,
                    m.EstimatedPrice, m.CurrencyId, m.Currency?.Code,
                    m.IsStrategic, m.IsActive);
            },
            CacheTTL.MaterialById);
}
