using ProcureHub.Modules.MasterData.Domain.Entities;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Commands.CreateLocation;

public class CreateLocationCommandHandler : ICommandHandler<CreateLocationCommand, Guid>
{
    private readonly ILocationRepository _repo;
    private readonly ICacheService       _cache;

    public CreateLocationCommandHandler(ILocationRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public async Task<Guid> Handle(CreateLocationCommand command, CancellationToken ct)
    {
        var location = new Location
        {
            CompanyId = command.CompanyId,
            Name      = command.Name,
            Type      = command.Type.ToLower(),
            Address   = command.Address,
            City      = command.City,
            Province  = command.Province,
            Country   = command.Country,
            IsActive  = true,
        };

        _repo.Add(location);
        await _repo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.Locations.Prefix);

        return location.Id;
    }
}
