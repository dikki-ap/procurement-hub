using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Commands.UpdatePaymentTerm;

public record UpdatePaymentTermCommand(
    Guid    Id,
    string  Code,
    string  Name,
    int     Days,
    string? Description,
    bool    IsActive
) : ICommand;
