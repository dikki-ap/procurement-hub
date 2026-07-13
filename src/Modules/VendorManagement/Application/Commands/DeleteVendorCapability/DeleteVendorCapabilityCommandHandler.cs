using MediatR;
using ProcureHub.Modules.VendorManagement.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.VendorManagement.Application.Commands.DeleteVendorCapability;

public class DeleteVendorCapabilityCommandHandler : ICommandHandler<DeleteVendorCapabilityCommand>
{
    private readonly IVendorCapabilityRepository _capRepo;
    private readonly ICacheService               _cache;

    public DeleteVendorCapabilityCommandHandler(IVendorCapabilityRepository capRepo, ICacheService cache)
    {
        _capRepo = capRepo;
        _cache   = cache;
    }

    public async Task<Unit> Handle(DeleteVendorCapabilityCommand command, CancellationToken ct)
    {
        var capability = await _capRepo.GetByIdAsync(command.CapabilityId, ct)
            ?? throw new NotFoundException("VendorCapability", command.CapabilityId);

        _capRepo.Remove(capability);
        await _capRepo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.Vendors.Prefix);

        return Unit.Value;
    }
}
