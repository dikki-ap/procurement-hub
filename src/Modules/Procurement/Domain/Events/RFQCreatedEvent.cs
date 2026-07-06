using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.Procurement.Domain.Events;

public record RFQCreatedEvent(Guid RFQId, string RFQNumber, List<Guid> InvitedVendorIds) : IDomainEvent;
