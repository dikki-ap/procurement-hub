using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.Procurement.Domain.Events;

public record QuotationSubmittedEvent(
    Guid QuotationId,
    Guid RFQId,
    Guid VendorId) : IDomainEvent;
