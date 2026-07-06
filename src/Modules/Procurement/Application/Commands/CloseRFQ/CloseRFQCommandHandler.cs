using MediatR;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Commands.CloseRFQ;

public class CloseRFQCommandHandler : ICommandHandler<CloseRFQCommand>
{
    private readonly IRFQRepository _repo;
    private readonly ICacheService  _cache;

    public CloseRFQCommandHandler(IRFQRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public async Task<Unit> Handle(CloseRFQCommand command, CancellationToken ct)
    {
        var rfq = await _repo.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("RFQ", command.Id);

        rfq.Close();

        _repo.Update(rfq);
        await _repo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.RFQs.Prefix);
        return Unit.Value;
    }
}
