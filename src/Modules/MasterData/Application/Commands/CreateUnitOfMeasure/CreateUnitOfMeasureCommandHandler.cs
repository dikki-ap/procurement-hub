using ProcureHub.Modules.MasterData.Domain.Entities;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.MasterData.Application.Commands.CreateUnitOfMeasure;

public class CreateUnitOfMeasureCommandHandler : ICommandHandler<CreateUnitOfMeasureCommand, Guid>
{
    private readonly IUnitOfMeasureRepository _repo;
    private readonly ICacheService            _cache;

    public CreateUnitOfMeasureCommandHandler(IUnitOfMeasureRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public async Task<Guid> Handle(CreateUnitOfMeasureCommand command, CancellationToken ct)
    {
        if (await _repo.ExistsByCodeAsync(command.CompanyId, command.Code, null, ct))
            throw new ConflictException("UnitOfMeasure", "Code", command.Code);

        var uom = new UnitOfMeasure
        {
            CompanyId = command.CompanyId,
            Code      = command.Code.ToUpperInvariant(),
            Name      = command.Name,
            IsActive  = true,
        };

        _repo.Add(uom);
        await _repo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.UnitOfMeasures.Prefix);

        return uom.Id;
    }
}
