namespace ProcureHub.Modules.MasterData.Application.DTOs;

public record DepartmentDto(
    Guid     Id,
    Guid     CompanyId,
    string   Name,
    string   Code,
    bool     IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
