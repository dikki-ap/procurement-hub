using ProcureHub.Modules.ApprovalEngine.Domain.Enums;

namespace ProcureHub.Modules.ApprovalEngine.Application.DTOs;

public record ApprovalWorkflowDto(
    Guid                       Id,
    string                     ReferenceType,
    Guid                       ReferenceId,
    string                     ReferenceNumber,
    string                     ReferenceTitle,
    decimal                    TotalValue,
    int                        CurrentLevel,
    int                        MaxLevel,
    WorkflowStatus             Status,
    int                        Iteration,
    DateTime?                  CompletedAt,
    DateTime                   CreatedAt,
    List<ApprovalHistoryDto>   History,
    List<ApproverAssignmentDto> Assignments);

public record ApproverAssignmentDto(
    Guid   AssignedUserId,
    string AssignedUserName,
    int    Level,
    bool   IsDelegate);
