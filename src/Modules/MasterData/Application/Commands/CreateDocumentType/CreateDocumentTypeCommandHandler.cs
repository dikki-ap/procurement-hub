using ProcureHub.Modules.MasterData.Domain.Entities;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.MasterData.Application.Commands.CreateDocumentType;

public class CreateDocumentTypeCommandHandler : ICommandHandler<CreateDocumentTypeCommand, Guid>
{
    private readonly IDocumentTypeRepository _repo;
    private readonly ICacheService           _cache;

    public CreateDocumentTypeCommandHandler(IDocumentTypeRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public async Task<Guid> Handle(CreateDocumentTypeCommand command, CancellationToken ct)
    {
        var name = command.Name.Trim().ToUpperInvariant();

        if (await _repo.ExistsByNameAsync(name, null, ct))
            throw new ConflictException("DocumentType", "Name", name);

        var entity = new DocumentType
        {
            Name              = name,
            IsActive          = true,
            AllowedExtensions = command.AllowedExtensions?.Trim().ToLowerInvariant(),
            MaxFileSizeMb     = command.MaxFileSizeMb,
        };

        _repo.Add(entity);
        await _repo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.DocumentTypes.Prefix);

        return entity.Id;
    }
}
