using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.ApprovalEngine.Domain.Events;

public record ApprovalRequestedEvent(
    Guid WorkflowId,
    Guid ReferenceId,
    string ReferenceType,
    string ReferenceNumber,
    int MaxLevel) : IDomainEvent;
