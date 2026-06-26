using MediatR;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.MasterData.Application.Commands.UpdatePaymentTerm;

public class UpdatePaymentTermCommandHandler : ICommandHandler<UpdatePaymentTermCommand>
{
    private readonly IPaymentTermRepository _repo;
    private readonly ICacheService          _cache;

    public UpdatePaymentTermCommandHandler(IPaymentTermRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public async Task<Unit> Handle(UpdatePaymentTermCommand command, CancellationToken ct)
    {
        var term = await _repo.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("PaymentTerm", command.Id);

        if (await _repo.ExistsByCodeAsync(term.CompanyId, command.Code, command.Id, ct))
            throw new ConflictException("PaymentTerm", "Code", command.Code);

        term.Code        = command.Code.ToUpperInvariant();
        term.Name        = command.Name;
        term.Days        = command.Days;
        term.Description = command.Description;
        term.IsActive    = command.IsActive;

        _repo.Update(term);
        await _repo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.PaymentTerms.Prefix);

        return Unit.Value;
    }
}
