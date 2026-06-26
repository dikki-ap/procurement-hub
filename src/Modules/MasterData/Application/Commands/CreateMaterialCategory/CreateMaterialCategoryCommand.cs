using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Commands.CreateMaterialCategory;

public record CreateMaterialCategoryCommand(
    Guid   CompanyId,
    string Name,
    string Code,
    Guid?  ParentId,
    bool   IsStrategic
) : ICommand<Guid>;
