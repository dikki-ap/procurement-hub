using ProcureHub.Modules.MasterData.Application.DTOs;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Queries.GetPaymentTermList;

public class GetPaymentTermListQueryHandler : IQueryHandler<GetPaymentTermListQuery, List<PaymentTermDto>>
{
    private readonly IPaymentTermRepository _repo;
    private readonly ICacheService          _cache;

    public GetPaymentTermListQueryHandler(IPaymentTermRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public Task<List<PaymentTermDto>> Handle(GetPaymentTermListQuery query, CancellationToken ct)
        => _cache.GetOrSetAsync(
            $"{CacheKeys.PaymentTerms.List}:{query.CompanyId}",
            async () =>
            {
                var list = await _repo.GetAllAsync(query.CompanyId, ct);
                return list.Select(e => new PaymentTermDto(
                    e.Id, e.CompanyId, e.Code, e.Name, e.Days, e.Description, e.IsActive,
                    e.CreatedBy?.FullName, e.CreatedAt, e.UpdatedBy?.FullName, e.UpdatedAt)).ToList();
            },
            CacheTTL.PaymentTerms);
}
