using MediatR;
using ProcureHub.Modules.Procurement.Application.Services;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Commands.InviteVendors;

public class InviteVendorsCommandHandler : ICommandHandler<InviteVendorsCommand>
{
    private readonly IRFQRepository  _repo;
    private readonly IVendorChecker  _vendorChecker;
    private readonly ICacheService   _cache;

    public InviteVendorsCommandHandler(
        IRFQRepository repo,
        IVendorChecker vendorChecker,
        ICacheService cache)
    {
        _repo          = repo;
        _vendorChecker = vendorChecker;
        _cache         = cache;
    }

    public async Task<Unit> Handle(InviteVendorsCommand command, CancellationToken ct)
    {
        var rfq = await _repo.GetByIdWithDetailsAsync(command.RFQId, ct)
            ?? throw new NotFoundException("RFQ", command.RFQId);

        if (rfq.Status != RFQStatus.Draft)
            throw new BusinessRuleException("InviteVendors", "Vendors can only be invited to a draft RFQ.");

        foreach (var vendorId in command.VendorIds)
        {
            if (await _vendorChecker.IsBlacklistedAsync(vendorId, ct))
                throw new BusinessRuleException("InviteVendors",
                    $"Vendor {vendorId} is blacklisted and cannot be invited to an RFQ.");

            var alreadyInvited = rfq.Vendors.Any(v => v.VendorId == vendorId);
            if (alreadyInvited) continue;

            rfq.Vendors.Add(new RFQVendor
            {
                RFQId     = rfq.Id,
                VendorId  = vendorId,
                InvitedAt = DateTime.UtcNow,
            });
        }

        _repo.Update(rfq);
        await _repo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.RFQs.Prefix);
        return Unit.Value;
    }
}
