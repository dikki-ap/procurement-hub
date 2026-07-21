using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.VendorManagement.Application.Commands.AddVendorCapability;

public record AddVendorCapabilityCommand(
    Guid     VendorId,
    Guid     MaterialCategoryId,
    decimal? MinOrderQty,
    string?  Uom,
    int?     LeadTimeDays,
    string?  Notes) : ICommand<Guid>;
