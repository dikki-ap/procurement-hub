using MediatR;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.MasterData.Application.Commands.DeleteUnitOfMeasure;

public class DeleteUnitOfMeasureCommandHandler : ICommandHandler<DeleteUnitOfMeasureCommand>
{
    private readonly IUnitOfMeasureRepository _repo;
    private readonly ICacheService            _cache;

    public DeleteUnitOfMeasureCommandHandler(IUnitOfMeasureRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public async Task<Unit> Handle(DeleteUnitOfMeasureCommand command, CancellationToken ct)
    {
        var uom = await _repo.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("UnitOfMeasure", command.Id);

        _repo.Remove(uom);
        await _repo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.UnitOfMeasures.Prefix);

        return Unit.Value;
    }
}
