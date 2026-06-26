using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Commands.CreatePaymentTerm;

public record CreatePaymentTermCommand(
    Guid    CompanyId,
    string  Code,
    string  Name,
    int     Days,
    string? Description
) : ICommand<Guid>;
