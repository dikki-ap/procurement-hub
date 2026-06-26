using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Commands.CreateUnitOfMeasure;

public record CreateUnitOfMeasureCommand(
    Guid   CompanyId,
    string Code,
    string Name
) : ICommand<Guid>;
