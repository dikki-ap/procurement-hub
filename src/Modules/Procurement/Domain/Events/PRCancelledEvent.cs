using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.Procurement.Domain.Events;

public record PRCancelledEvent(Guid PRId, string PRNumber) : IDomainEvent;
