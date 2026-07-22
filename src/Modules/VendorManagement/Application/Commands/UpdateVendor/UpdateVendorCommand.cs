using ProcureHub.Modules.VendorManagement.Domain.Enums;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.VendorManagement.Application.Commands.UpdateVendor;

public record UpdateVendorCommand(
    Guid       VendorId,
    string     LegalName,
    string?    TradeName,
    VendorType VendorType,
    string?    Npwp,
    string?    Siup,
    string?    Nib,
    string?    Address,
    string?    City,
    string?    Province,
    string?    PostalCode,
    string?    Country,
    Guid?      DefaultPaymentTermId,
    Guid?      DefaultCurrencyId,
    bool       IsPkp   = false,
    decimal?   PphRate = null) : ICommand;
