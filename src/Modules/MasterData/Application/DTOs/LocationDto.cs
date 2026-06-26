namespace ProcureHub.Modules.MasterData.Application.DTOs;

public record LocationDto(
    Guid    Id,
    Guid    CompanyId,
    string  Name,
    string  Type,
    string? Address,
    string? City,
    string? Province,
    string  Country,
    bool    IsActive
);
