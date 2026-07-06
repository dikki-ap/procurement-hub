using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.Procurement.Domain.Events;

public record PRSubmittedEvent(Guid PRId, string PRNumber, Guid RequestedById) : IDomainEvent;
