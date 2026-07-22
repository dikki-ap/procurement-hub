using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.UpdateContract;

public record UpdateContractCommand(
    Guid      Id,
    string    Title,
    Guid?     POId,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal?  Value,
    Guid?     CurrencyId,
    string?   Notes
) : ICommand;
