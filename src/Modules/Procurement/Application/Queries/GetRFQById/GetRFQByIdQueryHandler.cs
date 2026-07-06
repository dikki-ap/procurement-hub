using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetRFQById;

public class GetRFQByIdQueryHandler : IQueryHandler<GetRFQByIdQuery, RFQDto>
{
    private readonly IRFQRepository _repo;
    private readonly ICacheService  _cache;

    public GetRFQByIdQueryHandler(IRFQRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public async Task<RFQDto> Handle(GetRFQByIdQuery query, CancellationToken ct)
    {
        var cacheKey = CacheKeys.RFQs.ById(query.Id);
        var cached   = _cache.Get<RFQDto>(cacheKey);
        if (cached is not null) return cached;

        var rfq = await _repo.GetByIdWithDetailsAsync(query.Id, ct)
            ?? throw new NotFoundException("RFQ", query.Id);

        var dto = new RFQDto
        {
            Id                    = rfq.Id,
            RFQNumber             = rfq.RFQNumber,
            Title                 = rfq.Title,
            PurchaseRequisitionId = rfq.PurchaseRequisitionId,
            BidDeadline           = rfq.BidDeadline,
            DeliveryDate          = rfq.DeliveryDate,
            Status                = rfq.Status,
            Notes                 = rfq.Notes,
            Terms                 = rfq.Terms,
            CreatedAt             = rfq.CreatedAt,
            UpdatedAt             = rfq.UpdatedAt,
            Items = rfq.Items.Select(i => new RFQItemDto
            {
                Id              = i.Id,
                PRItemId        = i.PRItemId,
                MaterialId      = i.MaterialId,
                ItemDescription = i.ItemDescription,
                Quantity        = i.Quantity,
                UnitOfMeasureId = i.UnitOfMeasureId,
                UnitLabel       = i.UnitLabel,
            }).ToList(),
            Vendors = rfq.Vendors.Select(v => new RFQVendorDto
            {
                Id             = v.Id,
                VendorId       = v.VendorId,
                InvitedAt      = v.InvitedAt,
                Status         = v.Status,
                DeclinedReason = v.DeclinedReason,
            }).ToList(),
        };

        _cache.Set(cacheKey, dto, CacheTTL.RFQById);
        return dto;
    }
}
