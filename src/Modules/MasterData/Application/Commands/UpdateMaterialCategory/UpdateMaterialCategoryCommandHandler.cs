using MediatR;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.MasterData.Application.Commands.UpdateMaterialCategory;

public class UpdateMaterialCategoryCommandHandler : ICommandHandler<UpdateMaterialCategoryCommand>
{
    private readonly IMaterialCategoryRepository _repo;
    private readonly ICacheService               _cache;

    public UpdateMaterialCategoryCommandHandler(IMaterialCategoryRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public async Task<Unit> Handle(UpdateMaterialCategoryCommand command, CancellationToken ct)
    {
        var category = await _repo.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("MaterialCategory", command.Id);

        if (await _repo.ExistsByCodeAsync(category.CompanyId, command.Code, command.Id, ct))
            throw new ConflictException("MaterialCategory", "Code", command.Code);

        category.Name        = command.Name;
        category.Code        = command.Code.ToUpperInvariant();
        category.ParentId    = command.ParentId;
        category.IsStrategic = command.IsStrategic;
        category.IsActive    = command.IsActive;

        _repo.Update(category);
        await _repo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.MaterialCategories.Prefix);

        return Unit.Value;
    }
}
