using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Commands.DeleteDepartment;

public record DeleteDepartmentCommand(Guid Id) : ICommand;
