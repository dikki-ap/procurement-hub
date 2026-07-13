namespace ProcureHub.Modules.ApprovalEngine.Application.DTOs;

public record ApproverMatrixEntryDto(
    Guid     Id,
    string   ReferenceType,
    int      Level,
    string   Name,
    string   Position,
    string   Email,
    string?  CreatedByName,
    DateTime CreatedAt,
    string?  UpdatedByName,
    DateTime UpdatedAt
);
