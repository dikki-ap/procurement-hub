using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.Modules.VendorManagement.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.VendorManagement.Application.Commands.RegisterVendor;

public class RegisterVendorCommandHandler : ICommandHandler<RegisterVendorCommand, Guid>
{
    private readonly IVendorRepository _repo;
    private readonly ICacheService     _cache;

    public RegisterVendorCommandHandler(IVendorRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public async Task<Guid> Handle(RegisterVendorCommand command, CancellationToken ct)
    {
        var vendorCode = await _repo.GenerateNextCodeAsync(command.CompanyId, ct);

        var vendor = new Vendor
        {
            CompanyId  = command.CompanyId,
            VendorCode = vendorCode,
            LegalName  = command.LegalName,
            TradeName  = command.TradeName,
            VendorType = command.VendorType,
            Npwp       = command.Npwp,
            Siup       = command.Siup,
            Nib        = command.Nib,
            Address    = command.Address,
            City       = command.City,
            Province   = command.Province,
            PostalCode = command.PostalCode,
            Country    = command.Country,
        };

        vendor.Contacts.Add(new VendorContact
        {
            Name      = command.ContactName,
            Position  = command.ContactPosition,
            Email     = command.ContactEmail,
            Phone     = command.ContactPhone,
            IsPrimary = true,
        });

        _repo.Add(vendor);
        await _repo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.Vendors.Prefix);

        return vendor.Id;
    }
}
