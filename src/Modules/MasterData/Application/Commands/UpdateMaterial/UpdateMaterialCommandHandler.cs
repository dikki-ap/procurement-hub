using MediatR;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.MasterData.Application.Commands.UpdateMaterial;

public class UpdateMaterialCommandHandler : ICommandHandler<UpdateMaterialCommand>
{
    private readonly IMaterialRepository _repo;
    private readonly ICacheService       _cache;

    public UpdateMaterialCommandHandler(IMaterialRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public async Task<Unit> Handle(UpdateMaterialCommand command, CancellationToken ct)
    {
        var material = await _repo.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("Material", command.Id);

        if (await _repo.ExistsByCodeAsync(command.Code, command.Id, ct))
            throw new ConflictException("Material", "Code", command.Code);

        material.CategoryId     = command.CategoryId;
        material.Code           = command.Code.ToUpperInvariant();
        material.Name           = command.Name;
        material.Description    = command.Description;
        material.UomId          = command.UomId;
        material.EstimatedPrice = command.EstimatedPrice;
        material.CurrencyId     = command.CurrencyId;
        material.IsStrategic    = command.IsStrategic;
        material.IsActive       = command.IsActive;

        _repo.Update(material);
        await _repo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.Materials.Prefix);

        return Unit.Value;
    }
}
