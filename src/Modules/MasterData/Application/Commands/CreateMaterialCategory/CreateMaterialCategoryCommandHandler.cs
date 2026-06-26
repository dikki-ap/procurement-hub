using ProcureHub.Modules.MasterData.Domain.Entities;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.MasterData.Application.Commands.CreateMaterialCategory;

public class CreateMaterialCategoryCommandHandler : ICommandHandler<CreateMaterialCategoryCommand, Guid>
{
    private readonly IMaterialCategoryRepository _repo;
    private readonly ICacheService               _cache;

    public CreateMaterialCategoryCommandHandler(IMaterialCategoryRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public async Task<Guid> Handle(CreateMaterialCategoryCommand command, CancellationToken ct)
    {
        if (await _repo.ExistsByCodeAsync(command.CompanyId, command.Code, null, ct))
            throw new ConflictException("MaterialCategory", "Code", command.Code);

        var category = new MaterialCategory
        {
            CompanyId   = command.CompanyId,
            Name        = command.Name,
            Code        = command.Code.ToUpperInvariant(),
            ParentId    = command.ParentId,
            IsStrategic = command.IsStrategic,
            IsActive    = true,
        };

        _repo.Add(category);
        await _repo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.MaterialCategories.Prefix);

        return category.Id;
    }
}
