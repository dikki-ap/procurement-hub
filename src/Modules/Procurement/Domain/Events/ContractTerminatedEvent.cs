using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.Procurement.Domain.Events;

public record ContractTerminatedEvent(
    Guid   ContractId,
    string ContractNumber,
    string Title,
    Guid   VendorId
) : IDomainEvent;
