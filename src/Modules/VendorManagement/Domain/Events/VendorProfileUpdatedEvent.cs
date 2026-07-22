using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.VendorManagement.Domain.Events;

public record VendorProfileUpdatedEvent(
    Guid   VendorId,
    string VendorCode,
    string LegalName
) : IDomainEvent;
