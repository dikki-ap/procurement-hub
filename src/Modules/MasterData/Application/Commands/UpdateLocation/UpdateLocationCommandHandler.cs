using MediatR;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.MasterData.Application.Commands.UpdateLocation;

public class UpdateLocationCommandHandler : ICommandHandler<UpdateLocationCommand>
{
    private readonly ILocationRepository _repo;
    private readonly ICacheService       _cache;

    public UpdateLocationCommandHandler(ILocationRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public async Task<Unit> Handle(UpdateLocationCommand command, CancellationToken ct)
    {
        var location = await _repo.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("Location", command.Id);

        location.Name     = command.Name;
        location.Type     = command.Type.ToLower();
        location.Address  = command.Address;
        location.City     = command.City;
        location.Province = command.Province;
        location.Country  = command.Country;
        location.IsActive = command.IsActive;

        _repo.Update(location);
        await _repo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.Locations.Prefix);

        return Unit.Value;
    }
}
