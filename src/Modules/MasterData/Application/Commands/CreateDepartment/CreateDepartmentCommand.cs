using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Commands.CreateDepartment;

public record CreateDepartmentCommand(
    Guid   CompanyId,
    string Name,
    string Code,
    bool   IsActive = true
) : ICommand<Guid>;
