using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.Procurement.Domain.Events;

public record POIssuedEvent(
    Guid   POId,
    string PONumber,
    Guid   VendorId,
    Guid   CompanyId,
    decimal TotalAmount
) : IDomainEvent;
