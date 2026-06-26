using MediatR;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.MasterData.Application.Commands.DeleteLocation;

public class DeleteLocationCommandHandler : ICommandHandler<DeleteLocationCommand>
{
    private readonly ILocationRepository _repo;
    private readonly ICacheService       _cache;

    public DeleteLocationCommandHandler(ILocationRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public async Task<Unit> Handle(DeleteLocationCommand command, CancellationToken ct)
    {
        var location = await _repo.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("Location", command.Id);

        _repo.Remove(location);
        await _repo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.Locations.Prefix);

        return Unit.Value;
    }
}
