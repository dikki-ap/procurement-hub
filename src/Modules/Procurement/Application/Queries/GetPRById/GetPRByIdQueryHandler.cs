using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetPRById;

public class GetPRByIdQueryHandler : IQueryHandler<GetPRByIdQuery, PRDto>
{
    private readonly IPurchaseRequisitionRepository _repo;
    private readonly ICacheService                  _cache;

    public GetPRByIdQueryHandler(IPurchaseRequisitionRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public async Task<PRDto> Handle(GetPRByIdQuery query, CancellationToken ct)
    {
        var cacheKey = CacheKeys.PurchaseRequisitions.ById(query.Id);
        var cached   = _cache.Get<PRDto>(cacheKey);
        if (cached is not null) return cached;

        var pr = await _repo.GetByIdWithItemsAsync(query.Id, ct)
            ?? throw new NotFoundException("PurchaseRequisition", query.Id);

        var dto = new PRDto
        {
            Id                  = pr.Id,
            PRNumber            = pr.PRNumber,
            Title               = pr.Title,
            Description         = pr.Description,
            Department          = pr.Department,
            DeliveryLocation    = pr.DeliveryLocation,
            RequiredDate        = pr.RequiredDate,
            Status              = pr.Status,
            TotalEstimatedValue = pr.TotalEstimatedValue,
            Notes               = pr.Notes,
            RequestedById       = pr.RequestedById,
            CreatedAt           = pr.CreatedAt,
            UpdatedAt           = pr.UpdatedAt,
            Items               = pr.Items.Select(i => new PRItemDto
            {
                Id                 = i.Id,
                MaterialId         = i.MaterialId,
                ItemDescription    = i.ItemDescription,
                Quantity           = i.Quantity,
                UnitOfMeasureId    = i.UnitOfMeasureId,
                UnitLabel          = i.UnitLabel,
                EstimatedUnitPrice = i.EstimatedUnitPrice,
                Notes              = i.Notes,
            }).ToList(),
        };

        _cache.Set(cacheKey, dto, CacheTTL.PRById);
        return dto;
    }
}
