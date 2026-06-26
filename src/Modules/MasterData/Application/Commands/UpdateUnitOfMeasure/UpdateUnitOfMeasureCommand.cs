using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Commands.UpdateUnitOfMeasure;

public record UpdateUnitOfMeasureCommand(
    Guid   Id,
    string Code,
    string Name,
    bool   IsActive
) : ICommand;
