using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.UserManagement.Application.Commands.UpdateUser;

public record UpdateUserCommand(
    Guid  Id,
    Guid? DepartmentId,
    string Role,
    bool  IsActive
) : ICommand;
