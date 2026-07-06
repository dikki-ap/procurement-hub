using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.Procurement.Domain.Events;

public record GRNConfirmedEvent(
    Guid   GRNId,
    string GRNNumber,
    Guid   POId,
    Guid   VendorId,
    bool   HasDiscrepancy
) : IDomainEvent;
