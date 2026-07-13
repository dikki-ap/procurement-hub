using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetRFQList;

public class GetRFQListQueryHandler : IQueryHandler<GetRFQListQuery, List<RFQListDto>>
{
    private readonly IRFQRepository _repo;
    private readonly ICacheService  _cache;

    public GetRFQListQueryHandler(IRFQRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public async Task<List<RFQListDto>> Handle(GetRFQListQuery query, CancellationToken ct)
    {
        var cacheKey = CacheKeys.RFQs.List(query.CompanyId);
        var cached   = _cache.Get<List<RFQListDto>>(cacheKey);
        if (cached is not null) return cached;

        var rfqs = await _repo.GetAllAsync(query.CompanyId, ct);
        var dtos = rfqs.Select(r => new RFQListDto
        {
            Id                    = r.Id,
            RFQNumber             = r.RFQNumber,
            Title                 = r.Title,
            PurchaseRequisitionId = r.PurchaseRequisitionId,
            BidDeadline           = r.BidDeadline,
            Status                = r.Status,
            ItemCount             = r.Items.Count,
            VendorCount           = r.Vendors.Count,
            CreatedAt             = r.CreatedAt,
            CreatedByName         = r.CreatedBy?.FullName,
            UpdatedByName         = r.UpdatedBy?.FullName,
            UpdatedAt             = r.UpdatedAt,
        }).ToList();

        _cache.Set(cacheKey, dtos, CacheTTL.RFQList);
        return dtos;
    }
}
