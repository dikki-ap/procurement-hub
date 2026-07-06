using MediatR;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Commands.SubmitPR;

public class SubmitPRCommandHandler : ICommandHandler<SubmitPRCommand>
{
    private readonly IPurchaseRequisitionRepository _repo;
    private readonly ICacheService                  _cache;

    public SubmitPRCommandHandler(IPurchaseRequisitionRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public async Task<Unit> Handle(SubmitPRCommand command, CancellationToken ct)
    {
        var pr = await _repo.GetByIdWithItemsAsync(command.Id, ct)
            ?? throw new NotFoundException("PurchaseRequisition", command.Id);

        pr.Submit();

        _repo.Update(pr);
        await _repo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.PurchaseRequisitions.Prefix);
        return Unit.Value;
    }
}
