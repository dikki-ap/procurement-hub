using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.Procurement.Domain.Events;

public record BidAwardedEvent(
    Guid EvaluationId,
    Guid RFQId,
    Guid AwardedQuotationId,
    Guid AwardedVendorId) : IDomainEvent;
