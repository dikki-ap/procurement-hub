using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.ApprovalEngine.Domain.Events;

public record ApprovalCompletedEvent(
    Guid WorkflowId,
    Guid ReferenceId,
    string ReferenceType,
    string ReferenceNumber,
    Guid RequestedById) : IDomainEvent;
