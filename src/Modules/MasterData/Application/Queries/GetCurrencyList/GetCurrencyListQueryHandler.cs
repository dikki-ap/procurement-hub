using ProcureHub.Modules.MasterData.Application.DTOs;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Queries.GetCurrencyList;

public class GetCurrencyListQueryHandler : IQueryHandler<GetCurrencyListQuery, List<CurrencyDto>>
{
    private readonly ICurrencyRepository _repo;
    private readonly ICacheService       _cache;

    public GetCurrencyListQueryHandler(ICurrencyRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public Task<List<CurrencyDto>> Handle(GetCurrencyListQuery query, CancellationToken ct)
        => _cache.GetOrSetAsync(
            CacheKeys.Currencies.List,
            async () =>
            {
                var list = await _repo.GetAllAsync(ct);
                return list.Select(ToDto).ToList();
            },
            CacheTTL.CurrencyList);

    private static CurrencyDto ToDto(ProcureHub.Modules.MasterData.Domain.Entities.Currency c)
        => new(c.Id, c.Code, c.Name, c.Symbol, c.ExchangeRate, c.IsBase, c.IsActive);
}
