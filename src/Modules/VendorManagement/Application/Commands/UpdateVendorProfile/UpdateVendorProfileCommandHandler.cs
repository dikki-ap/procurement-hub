using MediatR;
using ProcureHub.Modules.VendorManagement.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.VendorManagement.Application.Commands.UpdateVendorProfile;

public class UpdateVendorProfileCommandHandler : ICommandHandler<UpdateVendorProfileCommand>
{
    private readonly IVendorRepository _repo;
    private readonly ICacheService     _cache;

    public UpdateVendorProfileCommandHandler(IVendorRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public async Task<Unit> Handle(UpdateVendorProfileCommand command, CancellationToken ct)
    {
        var vendor = await _repo.GetByIdAsync(command.VendorId, ct)
            ?? throw new NotFoundException("Vendor", command.VendorId);

        vendor.TradeName  = command.TradeName;
        vendor.Npwp       = command.Npwp;
        vendor.Siup       = command.Siup;
        vendor.Nib        = command.Nib;
        vendor.Address    = command.Address;
        vendor.City       = command.City;
        vendor.Province   = command.Province;
        vendor.PostalCode = command.PostalCode;
        vendor.Country    = command.Country;

        vendor.RaiseProfileUpdated();

        _repo.Update(vendor);
        await _repo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.Vendors.Prefix);

        return Unit.Value;
    }
}
