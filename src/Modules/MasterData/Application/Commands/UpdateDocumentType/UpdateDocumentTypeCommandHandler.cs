using MediatR;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.MasterData.Application.Commands.UpdateDocumentType;

public class UpdateDocumentTypeCommandHandler : ICommandHandler<UpdateDocumentTypeCommand>
{
    private readonly IDocumentTypeRepository _repo;
    private readonly ICacheService           _cache;

    public UpdateDocumentTypeCommandHandler(IDocumentTypeRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public async Task<Unit> Handle(UpdateDocumentTypeCommand command, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("DocumentType", command.Id);

        var name = command.Name.Trim().ToUpperInvariant();

        if (await _repo.ExistsByNameAsync(name, command.Id, ct))
            throw new ConflictException("DocumentType", "Name", name);

        entity.Name              = name;
        entity.IsActive          = command.IsActive;
        entity.AllowedExtensions = command.AllowedExtensions?.Trim().ToLowerInvariant();
        entity.MaxFileSizeMb     = command.MaxFileSizeMb;

        _repo.Update(entity);
        await _repo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.DocumentTypes.Prefix);

        return Unit.Value;
    }
}
