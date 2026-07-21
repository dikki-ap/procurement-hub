using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Commands.UpdateDepartment;

public record UpdateDepartmentCommand(
    Guid   Id,
    string Name,
    string Code,
    bool   IsActive
) : ICommand;
