using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.CreateContract;

public record CreateContractCommand(
    Guid      CompanyId,
    Guid      VendorId,
    string    Title,
    Guid?     POId,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal?  Value,
    Guid?     CurrencyId,
    string?   Notes
) : ICommand<Guid>;
