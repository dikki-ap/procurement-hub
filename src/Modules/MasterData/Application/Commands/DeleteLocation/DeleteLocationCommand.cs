using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Commands.DeleteLocation;

public record DeleteLocationCommand(Guid Id) : ICommand;
