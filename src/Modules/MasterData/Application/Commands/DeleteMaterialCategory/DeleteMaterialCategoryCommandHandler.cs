using MediatR;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.MasterData.Application.Commands.DeleteMaterialCategory;

public class DeleteMaterialCategoryCommandHandler : ICommandHandler<DeleteMaterialCategoryCommand>
{
    private readonly IMaterialCategoryRepository _repo;
    private readonly ICacheService               _cache;

    public DeleteMaterialCategoryCommandHandler(IMaterialCategoryRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public async Task<Unit> Handle(DeleteMaterialCategoryCommand command, CancellationToken ct)
    {
        var category = await _repo.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("MaterialCategory", command.Id);

        _repo.Remove(category);
        await _repo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.MaterialCategories.Prefix);

        return Unit.Value;
    }
}
