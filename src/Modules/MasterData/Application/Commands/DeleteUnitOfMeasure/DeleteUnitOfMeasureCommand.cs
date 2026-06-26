using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Commands.DeleteUnitOfMeasure;

public record DeleteUnitOfMeasureCommand(Guid Id) : ICommand;
