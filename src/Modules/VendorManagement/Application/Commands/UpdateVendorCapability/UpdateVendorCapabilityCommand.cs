using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.VendorManagement.Application.Commands.UpdateVendorCapability;

public record UpdateVendorCapabilityCommand(
    Guid      CapabilityId,
    decimal?  MinOrderQty,
    decimal?  MaxOrderQty,
    string?   Uom,
    int?      LeadTimeDays,
    DateOnly? EffectiveDate,
    DateOnly? ExpiryDate,
    string?   Notes) : ICommand;
