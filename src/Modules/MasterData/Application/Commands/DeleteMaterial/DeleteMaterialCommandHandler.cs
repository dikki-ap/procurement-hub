using MediatR;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.MasterData.Application.Commands.DeleteMaterial;

public class DeleteMaterialCommandHandler : ICommandHandler<DeleteMaterialCommand>
{
    private readonly IMaterialRepository _repo;
    private readonly ICacheService       _cache;

    public DeleteMaterialCommandHandler(IMaterialRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public async Task<Unit> Handle(DeleteMaterialCommand command, CancellationToken ct)
    {
        var material = await _repo.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("Material", command.Id);

        _repo.Remove(material);
        await _repo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.Materials.Prefix);

        return Unit.Value;
    }
}
