using MediatR;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.MasterData.Application.Commands.DeletePaymentTerm;

public class DeletePaymentTermCommandHandler : ICommandHandler<DeletePaymentTermCommand>
{
    private readonly IPaymentTermRepository _repo;
    private readonly ICacheService          _cache;

    public DeletePaymentTermCommandHandler(IPaymentTermRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public async Task<Unit> Handle(DeletePaymentTermCommand command, CancellationToken ct)
    {
        var term = await _repo.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("PaymentTerm", command.Id);

        _repo.Remove(term);
        await _repo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.PaymentTerms.Prefix);

        return Unit.Value;
    }
}
