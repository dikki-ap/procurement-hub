using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.Modules.VendorManagement.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.VendorManagement.Application.Commands.AddVendorCapability;

public class AddVendorCapabilityCommandHandler : ICommandHandler<AddVendorCapabilityCommand, Guid>
{
    private readonly IVendorRepository           _vendorRepo;
    private readonly IVendorCapabilityRepository _capRepo;
    private readonly ICacheService               _cache;

    public AddVendorCapabilityCommandHandler(
        IVendorRepository           vendorRepo,
        IVendorCapabilityRepository capRepo,
        ICacheService               cache)
    {
        _vendorRepo = vendorRepo;
        _capRepo    = capRepo;
        _cache      = cache;
    }

    public async Task<Guid> Handle(AddVendorCapabilityCommand command, CancellationToken ct)
    {
        var vendor = await _vendorRepo.GetByIdAsync(command.VendorId, ct)
            ?? throw new NotFoundException("Vendor", command.VendorId);

        if (await _capRepo.ExistsAsync(vendor.Id, command.MaterialCategoryId, ct))
            throw new BusinessRuleException("AddVendorCapability",
                "A capability for this material category already exists for this vendor.");

        var capability = new VendorCapability
        {
            VendorId           = vendor.Id,
            MaterialCategoryId = command.MaterialCategoryId,
            MinOrderQty        = command.MinOrderQty,
            Uom                = command.Uom,
            LeadTimeDays       = command.LeadTimeDays,
            Notes              = command.Notes,
        };

        _capRepo.Add(capability);
        await _capRepo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.Vendors.Prefix);

        return capability.Id;
    }
}
