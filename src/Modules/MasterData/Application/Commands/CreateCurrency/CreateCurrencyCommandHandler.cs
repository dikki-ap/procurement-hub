using ProcureHub.Modules.MasterData.Domain.Entities;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.MasterData.Application.Commands.CreateCurrency;

public class CreateCurrencyCommandHandler : ICommandHandler<CreateCurrencyCommand, Guid>
{
    private readonly ICurrencyRepository _repo;
    private readonly ICacheService       _cache;

    public CreateCurrencyCommandHandler(ICurrencyRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public async Task<Guid> Handle(CreateCurrencyCommand command, CancellationToken ct)
    {
        if (await _repo.ExistsByCodeAsync(command.Code, null, ct))
            throw new ConflictException("Currency", "Code", command.Code);

        if (command.IsBase)
            await _repo.ClearAllBaseAsync(null, ct);

        var currency = new Currency
        {
            Code         = command.Code.ToUpperInvariant(),
            Name         = command.Name,
            Symbol       = command.Symbol,
            ExchangeRate = command.ExchangeRate,
            IsBase       = command.IsBase,
        };

        _repo.Add(currency);
        await _repo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.Currencies.Prefix);

        return currency.Id;
    }
}
