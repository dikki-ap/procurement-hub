using ProcureHub.Modules.ApprovalEngine.Domain.Enums;

namespace ProcureHub.Modules.ApprovalEngine.Application.DTOs;

public record ApprovalInboxItemDto(
    Guid           WorkflowId,
    string         ReferenceType,
    string         ReferenceNumber,
    string         ReferenceTitle,
    decimal        TotalValue,
    int            CurrentLevel,
    int            MaxLevel,
    WorkflowStatus Status,
    DateTime       CreatedAt);
