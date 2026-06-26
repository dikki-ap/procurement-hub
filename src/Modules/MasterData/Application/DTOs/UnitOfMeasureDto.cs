namespace ProcureHub.Modules.MasterData.Application.DTOs;

public record UnitOfMeasureDto(
    Guid   Id,
    Guid   CompanyId,
    string Code,
    string Name,
    bool   IsActive
);
