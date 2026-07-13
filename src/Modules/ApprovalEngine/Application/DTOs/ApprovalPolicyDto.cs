namespace ProcureHub.Modules.ApprovalEngine.Application.DTOs;

public record ApprovalPolicyDto(
    Guid     Id,
    Guid     CompanyId,
    string   ReferenceType,
    string   Name,
    decimal  MinValue,
    decimal? MaxValue,
    int      RequiredLevels,
    bool     IsStrategicOverride,
    bool     IsSingleSourceOverride,
    bool     IsActive,
    DateTime CreatedAt,
    string?  CreatedByName,
    string?  UpdatedByName,
    DateTime UpdatedAt);
