using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Commands.UpdateMaterialCategory;

public record UpdateMaterialCategoryCommand(
    Guid   Id,
    string Name,
    string Code,
    Guid?  ParentId,
    bool   IsStrategic,
    bool   IsActive
) : ICommand;
