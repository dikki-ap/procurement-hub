using ProcureHub.Modules.ApprovalEngine.Domain.Enums;

namespace ProcureHub.Modules.ApprovalEngine.Application.DTOs;

public record ApprovalHistoryDto(
    Guid               Id,
    int                Level,
    ApprovalActionType Action,
    string             ActorName,
    string?            Reason,
    DateTime           ActedAt);
