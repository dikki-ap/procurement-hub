using MediatR;
using ProcureHub.Modules.VendorManagement.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.VendorManagement.Application.Commands.UpdateVendorCapability;

public class UpdateVendorCapabilityCommandHandler : ICommandHandler<UpdateVendorCapabilityCommand>
{
    private readonly IVendorCapabilityRepository _capRepo;
    private readonly ICacheService               _cache;

    public UpdateVendorCapabilityCommandHandler(IVendorCapabilityRepository capRepo, ICacheService cache)
    {
        _capRepo = capRepo;
        _cache   = cache;
    }

    public async Task<Unit> Handle(UpdateVendorCapabilityCommand command, CancellationToken ct)
    {
        var capability = await _capRepo.GetByIdAsync(command.CapabilityId, ct)
            ?? throw new NotFoundException("VendorCapability", command.CapabilityId);

        capability.MinOrderQty  = command.MinOrderQty;
        capability.LeadTimeDays = command.LeadTimeDays;
        capability.Notes        = command.Notes;

        _capRepo.Update(capability);
        await _capRepo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.Vendors.Prefix);

        return Unit.Value;
    }
}
