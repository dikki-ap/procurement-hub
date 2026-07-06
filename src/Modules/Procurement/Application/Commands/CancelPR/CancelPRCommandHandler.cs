using MediatR;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Commands.CancelPR;

public class CancelPRCommandHandler : ICommandHandler<CancelPRCommand>
{
    private readonly IPurchaseRequisitionRepository _repo;
    private readonly ICacheService                  _cache;

    public CancelPRCommandHandler(IPurchaseRequisitionRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public async Task<Unit> Handle(CancelPRCommand command, CancellationToken ct)
    {
        var pr = await _repo.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("PurchaseRequisition", command.Id);

        pr.Cancel();

        _repo.Update(pr);
        await _repo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.PurchaseRequisitions.Prefix);
        return Unit.Value;
    }
}
