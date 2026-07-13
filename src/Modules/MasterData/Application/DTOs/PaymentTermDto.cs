namespace ProcureHub.Modules.MasterData.Application.DTOs;

public record PaymentTermDto(
    Guid     Id,
    Guid     CompanyId,
    string   Code,
    string   Name,
    int      Days,
    string?  Description,
    bool     IsActive,
    string?  CreatedByName,
    DateTime CreatedAt,
    string?  UpdatedByName,
    DateTime UpdatedAt
);
