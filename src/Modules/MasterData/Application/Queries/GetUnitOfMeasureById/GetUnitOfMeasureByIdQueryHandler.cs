using ProcureHub.Modules.MasterData.Application.DTOs;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.MasterData.Application.Queries.GetUnitOfMeasureById;

public class GetUnitOfMeasureByIdQueryHandler : IQueryHandler<GetUnitOfMeasureByIdQuery, UnitOfMeasureDto>
{
    private readonly IUnitOfMeasureRepository _repo;
    private readonly ICacheService            _cache;

    public GetUnitOfMeasureByIdQueryHandler(IUnitOfMeasureRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public Task<UnitOfMeasureDto> Handle(GetUnitOfMeasureByIdQuery query, CancellationToken ct)
        => _cache.GetOrSetAsync(
            CacheKeys.UnitOfMeasures.ById(query.Id),
            async () =>
            {
                var uom = await _repo.GetByIdAsync(query.Id, ct)
                    ?? throw new NotFoundException("UnitOfMeasure", query.Id);
                return new UnitOfMeasureDto(
                    uom.Id, uom.CompanyId, uom.Code, uom.Name, uom.IsActive,
                    uom.CreatedBy?.FullName, uom.CreatedAt, uom.UpdatedBy?.FullName, uom.UpdatedAt);
            },
            CacheTTL.UnitOfMeasures);
}
