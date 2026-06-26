using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Commands.CreateLocation;

public record CreateLocationCommand(
    Guid    CompanyId,
    string  Name,
    string  Type,
    string? Address,
    string? City,
    string? Province,
    string  Country
) : ICommand<Guid>;
