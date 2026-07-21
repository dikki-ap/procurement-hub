using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.VendorManagement.Application.Commands.UpdateVendorCapability;

public record UpdateVendorCapabilityCommand(
    Guid     CapabilityId,
    decimal? MinOrderQty,
    string?  Uom,
    int?     LeadTimeDays,
    string?  Notes) : ICommand;
