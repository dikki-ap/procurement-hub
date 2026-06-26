using MediatR;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.MasterData.Application.Commands.UpdateUnitOfMeasure;

public class UpdateUnitOfMeasureCommandHandler : ICommandHandler<UpdateUnitOfMeasureCommand>
{
    private readonly IUnitOfMeasureRepository _repo;
    private readonly ICacheService            _cache;

    public UpdateUnitOfMeasureCommandHandler(IUnitOfMeasureRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public async Task<Unit> Handle(UpdateUnitOfMeasureCommand command, CancellationToken ct)
    {
        var uom = await _repo.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("UnitOfMeasure", command.Id);

        if (await _repo.ExistsByCodeAsync(uom.CompanyId, command.Code, command.Id, ct))
            throw new ConflictException("UnitOfMeasure", "Code", command.Code);

        uom.Code     = command.Code.ToUpperInvariant();
        uom.Name     = command.Name;
        uom.IsActive = command.IsActive;

        _repo.Update(uom);
        await _repo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.UnitOfMeasures.Prefix);

        return Unit.Value;
    }
}
