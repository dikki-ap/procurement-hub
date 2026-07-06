using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.ApprovalEngine.Domain.Events;

public record ApprovalRevisedEvent(
    Guid WorkflowId,
    Guid ReferenceId,
    string ReferenceType,
    string ReferenceNumber,
    Guid RequestedById,
    string Reason) : IDomainEvent;
