namespace ProcureHub.Modules.MasterData.Application.DTOs;

public record MaterialDto(
    Guid     Id,
    Guid     CategoryId,
    string   CategoryName,
    string   Code,
    string   Name,
    string?  Description,
    Guid     UomId,
    string   UomCode,
    decimal? EstimatedPrice,
    Guid?    CurrencyId,
    string?  CurrencyCode,
    bool     IsStrategic,
    bool     IsActive
);
