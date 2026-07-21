using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.UserManagement.Application.Commands.DeactivateUser;

public record DeactivateUserCommand(Guid Id) : ICommand;
