namespace ProcureHub.Modules.MasterData.Application.DTOs;

public record CurrencyDto(
    Guid    Id,
    string  Code,
    string  Name,
    string? Symbol,
    decimal ExchangeRate,
    bool    IsBase,
    bool    IsActive
);
