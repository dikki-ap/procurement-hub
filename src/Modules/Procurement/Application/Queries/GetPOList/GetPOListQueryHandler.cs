using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetPOList;

public class GetPOListQueryHandler : IQueryHandler<GetPOListQuery, List<POListDto>>
{
    private readonly IPurchaseOrderRepository _repo;

    public GetPOListQueryHandler(IPurchaseOrderRepository repo) => _repo = repo;

    public async Task<List<POListDto>> Handle(GetPOListQuery query, CancellationToken ct)
    {
        var pos = await _repo.GetAllAsync(query.CompanyId, ct);
        return pos.Select(p => new POListDto(
            p.Id,
            p.PONumber,
            p.VendorId,
            p.VendorId.ToString(),
            p.Status,
            p.TotalAmount,
            p.ExpectedDelivery,
            p.IssuedAt,
            p.CreatedAt,
            p.CreatedBy?.FullName,
            p.UpdatedBy?.FullName,
            p.UpdatedAt)).ToList();
    }
}
