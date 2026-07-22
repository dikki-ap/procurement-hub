using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.Procurement.Domain.Events;

public record ContractActivatedEvent(
    Guid      ContractId,
    string    ContractNumber,
    string    Title,
    Guid      VendorId,
    DateTime? StartDate,
    DateTime? EndDate
) : IDomainEvent;
