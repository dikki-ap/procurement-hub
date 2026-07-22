using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetReturnOrdersByVendor;

public class GetReturnOrdersByVendorQueryHandler : IQueryHandler<GetReturnOrdersByVendorQuery, List<ReturnOrderListDto>>
{
    private readonly IReturnOrderRepository _repo;

    public GetReturnOrdersByVendorQueryHandler(IReturnOrderRepository repo) => _repo = repo;

    public async Task<List<ReturnOrderListDto>> Handle(GetReturnOrdersByVendorQuery query, CancellationToken ct)
    {
        var list = await _repo.GetByVendorAsync(query.VendorId, ct);
        return list.Select(r => new ReturnOrderListDto(
            r.Id, r.ReturnNumber, r.GRNId, r.POId, r.VendorId,
            null, // vendor knows their own name
            r.Status, r.Reason, r.CreatedAt, r.AcknowledgedAt, r.ShippedAt, r.ReceivedAt
        )).ToList();
    }
}
