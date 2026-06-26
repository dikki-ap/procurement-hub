using ProcureHub.Modules.MasterData.Domain.Entities;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.MasterData.Application.Commands.CreateMaterial;

public class CreateMaterialCommandHandler : ICommandHandler<CreateMaterialCommand, Guid>
{
    private readonly IMaterialRepository _repo;
    private readonly ICacheService       _cache;

    public CreateMaterialCommandHandler(IMaterialRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public async Task<Guid> Handle(CreateMaterialCommand command, CancellationToken ct)
    {
        if (await _repo.ExistsByCodeAsync(command.Code, null, ct))
            throw new ConflictException("Material", "Code", command.Code);

        var material = new Material
        {
            CategoryId     = command.CategoryId,
            Code           = command.Code.ToUpperInvariant(),
            Name           = command.Name,
            Description    = command.Description,
            UomId          = command.UomId,
            EstimatedPrice = command.EstimatedPrice,
            CurrencyId     = command.CurrencyId,
            IsStrategic    = command.IsStrategic,
            IsActive       = true,
        };

        _repo.Add(material);
        await _repo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.Materials.Prefix);

        return material.Id;
    }
}
