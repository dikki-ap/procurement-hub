using MediatR;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.MasterData.Application.Commands.DeleteCurrency;

public class DeleteCurrencyCommandHandler : ICommandHandler<DeleteCurrencyCommand>
{
    private readonly ICurrencyRepository _repo;
    private readonly ICacheService       _cache;

    public DeleteCurrencyCommandHandler(ICurrencyRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public async Task<Unit> Handle(DeleteCurrencyCommand command, CancellationToken ct)
    {
        var currency = await _repo.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("Currency", command.Id);

        _repo.Remove(currency);
        await _repo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.Currencies.Prefix);

        return Unit.Value;
    }
}
