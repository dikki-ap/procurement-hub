using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.Procurement.Domain.Events;

public record RFQClosedEvent(Guid RFQId, string RFQNumber) : IDomainEvent;
