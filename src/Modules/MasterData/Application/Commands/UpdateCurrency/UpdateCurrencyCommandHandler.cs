using MediatR;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.MasterData.Application.Commands.UpdateCurrency;

public class UpdateCurrencyCommandHandler : ICommandHandler<UpdateCurrencyCommand>
{
    private readonly ICurrencyRepository _repo;
    private readonly ICacheService       _cache;

    public UpdateCurrencyCommandHandler(ICurrencyRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public async Task<Unit> Handle(UpdateCurrencyCommand command, CancellationToken ct)
    {
        var currency = await _repo.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("Currency", command.Id);

        if (await _repo.ExistsByCodeAsync(command.Code, command.Id, ct))
            throw new ConflictException("Currency", "Code", command.Code);

        if (command.IsBase)
            await _repo.ClearAllBaseAsync(command.Id, ct);

        currency.Code         = command.Code.ToUpperInvariant();
        currency.Name         = command.Name;
        currency.Symbol       = command.Symbol;
        currency.ExchangeRate = command.IsBase ? 1m : command.ExchangeRate;
        currency.IsBase       = command.IsBase;
        currency.IsActive     = command.IsActive;

        _repo.Update(currency);
        await _repo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.Currencies.Prefix);

        return Unit.Value;
    }
}
