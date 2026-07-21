using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.VendorManagement.Application.Commands.AddVendorCapability;

public record AddVendorCapabilityCommand(
    Guid      VendorId,
    Guid      MaterialCategoryId,
    decimal?  MinOrderQty,
    decimal?  MaxOrderQty,
    string?   Uom,
    int?      LeadTimeDays,
    DateOnly? EffectiveDate,
    DateOnly? ExpiryDate,
    string?   Notes) : ICommand<Guid>;
