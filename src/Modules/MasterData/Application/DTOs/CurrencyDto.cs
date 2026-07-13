namespace ProcureHub.Modules.MasterData.Application.DTOs;

public record CurrencyDto(
    Guid      Id,
    string    Code,
    string    Name,
    string?   Symbol,
    decimal   ExchangeRate,
    bool      IsBase,
    bool      IsActive,
    DateTime? RateUpdatedAt,
    string?   CreatedByName,
    DateTime  CreatedAt,
    string?   UpdatedByName,
    DateTime  UpdatedAt
);
