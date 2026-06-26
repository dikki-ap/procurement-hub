using ProcureHub.Modules.MasterData.Domain.Entities;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.MasterData.Application.Commands.CreatePaymentTerm;

public class CreatePaymentTermCommandHandler : ICommandHandler<CreatePaymentTermCommand, Guid>
{
    private readonly IPaymentTermRepository _repo;
    private readonly ICacheService          _cache;

    public CreatePaymentTermCommandHandler(IPaymentTermRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public async Task<Guid> Handle(CreatePaymentTermCommand command, CancellationToken ct)
    {
        if (await _repo.ExistsByCodeAsync(command.CompanyId, command.Code, null, ct))
            throw new ConflictException("PaymentTerm", "Code", command.Code);

        var term = new PaymentTerm
        {
            CompanyId   = command.CompanyId,
            Code        = command.Code.ToUpperInvariant(),
            Name        = command.Name,
            Days        = command.Days,
            Description = command.Description,
            IsActive    = true,
        };

        _repo.Add(term);
        await _repo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.PaymentTerms.Prefix);

        return term.Id;
    }
}
