using MediatR;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.MasterData.Application.Commands.DeleteDocumentType;

public class DeleteDocumentTypeCommandHandler : ICommandHandler<DeleteDocumentTypeCommand>
{
    private readonly IDocumentTypeRepository _repo;
    private readonly ICacheService           _cache;

    public DeleteDocumentTypeCommandHandler(IDocumentTypeRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public async Task<Unit> Handle(DeleteDocumentTypeCommand command, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("DocumentType", command.Id);

        _repo.Remove(entity);
        await _repo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.DocumentTypes.Prefix);

        return Unit.Value;
    }
}
