using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.VendorManagement.Application.Commands.AddVendorCapability;

public record AddVendorCapabilityCommand(
    Guid     VendorId,
    Guid     MaterialCategoryId,
    decimal? MinOrderQty,
    int?     LeadTimeDays,
    string?  Notes) : ICommand<Guid>;
