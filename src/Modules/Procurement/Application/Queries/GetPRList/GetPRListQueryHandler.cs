using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetPRList;

public class GetPRListQueryHandler : IQueryHandler<GetPRListQuery, List<PRListDto>>
{
    private readonly IPurchaseRequisitionRepository _repo;
    private readonly ICacheService                  _cache;

    public GetPRListQueryHandler(IPurchaseRequisitionRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public async Task<List<PRListDto>> Handle(GetPRListQuery query, CancellationToken ct)
    {
        var cacheKey = CacheKeys.PurchaseRequisitions.List(query.CompanyId);
        var cached   = _cache.Get<List<PRListDto>>(cacheKey);
        if (cached is not null) return cached;

        var prs = await _repo.GetAllAsync(query.CompanyId, ct);
        var dtos = prs.Select(pr => new PRListDto
        {
            Id                  = pr.Id,
            PRNumber            = pr.PRNumber,
            Title               = pr.Title,
            Department          = pr.Department,
            RequiredDate        = pr.RequiredDate,
            Status              = pr.Status,
            TotalEstimatedValue = pr.TotalEstimatedValue,
            ItemCount           = pr.Items.Count,
            CreatedAt           = pr.CreatedAt,
        }).ToList();

        _cache.Set(cacheKey, dtos, CacheTTL.PRList);
        return dtos;
    }
}
