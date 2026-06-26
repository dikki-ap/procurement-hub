using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Commands.UpdateLocation;

public record UpdateLocationCommand(
    Guid    Id,
    string  Name,
    string  Type,
    string? Address,
    string? City,
    string? Province,
    string  Country,
    bool    IsActive
) : ICommand;
