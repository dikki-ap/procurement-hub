using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.VendorManagement.Application.Commands.UpdateVendorProfile;

public record UpdateVendorProfileCommand(
    Guid    VendorId,
    string? TradeName,
    string? Npwp,
    string? Siup,
    string? Nib,
    string? Address,
    string? City,
    string? Province,
    string? PostalCode,
    string? Country
) : ICommand;
