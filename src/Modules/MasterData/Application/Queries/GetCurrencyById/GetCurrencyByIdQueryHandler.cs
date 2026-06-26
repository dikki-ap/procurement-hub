using ProcureHub.Modules.MasterData.Application.DTOs;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.MasterData.Application.Queries.GetCurrencyById;

public class GetCurrencyByIdQueryHandler : IQueryHandler<GetCurrencyByIdQuery, CurrencyDto>
{
    private readonly ICurrencyRepository _repo;
    private readonly ICacheService       _cache;

    public GetCurrencyByIdQueryHandler(ICurrencyRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public Task<CurrencyDto> Handle(GetCurrencyByIdQuery query, CancellationToken ct)
        => _cache.GetOrSetAsync(
            CacheKeys.Currencies.ById(query.Id),
            async () =>
            {
                var currency = await _repo.GetByIdAsync(query.Id, ct)
                    ?? throw new NotFoundException("Currency", query.Id);
                return new CurrencyDto(
                    currency.Id, currency.Code, currency.Name,
                    currency.Symbol, currency.ExchangeRate, currency.IsBase, currency.IsActive);
            },
            CacheTTL.CurrencyById);
}
