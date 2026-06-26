using ProcureHub.Modules.MasterData.Application.DTOs;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.MasterData.Application.Queries.GetPaymentTermById;

public class GetPaymentTermByIdQueryHandler : IQueryHandler<GetPaymentTermByIdQuery, PaymentTermDto>
{
    private readonly IPaymentTermRepository _repo;
    private readonly ICacheService          _cache;

    public GetPaymentTermByIdQueryHandler(IPaymentTermRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public Task<PaymentTermDto> Handle(GetPaymentTermByIdQuery query, CancellationToken ct)
        => _cache.GetOrSetAsync(
            CacheKeys.PaymentTerms.ById(query.Id),
            async () =>
            {
                var term = await _repo.GetByIdAsync(query.Id, ct)
                    ?? throw new NotFoundException("PaymentTerm", query.Id);
                return new PaymentTermDto(
                    term.Id, term.CompanyId, term.Code, term.Name,
                    term.Days, term.Description, term.IsActive);
            },
            CacheTTL.PaymentTerms);
}
