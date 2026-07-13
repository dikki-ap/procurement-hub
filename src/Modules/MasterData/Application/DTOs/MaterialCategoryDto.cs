namespace ProcureHub.Modules.MasterData.Application.DTOs;

public record MaterialCategoryDto(
    Guid     Id,
    Guid     CompanyId,
    string   Name,
    string   Code,
    Guid?    ParentId,
    bool     IsStrategic,
    bool     IsActive,
    string?  CreatedByName,
    DateTime CreatedAt,
    string?  UpdatedByName,
    DateTime UpdatedAt
);
