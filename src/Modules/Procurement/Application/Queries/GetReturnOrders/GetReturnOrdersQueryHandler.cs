using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetReturnOrders;

public class GetReturnOrdersQueryHandler : IQueryHandler<GetReturnOrdersQuery, List<ReturnOrderListDto>>
{
    private readonly IReturnOrderRepository _repo;

    public GetReturnOrdersQueryHandler(IReturnOrderRepository repo) => _repo = repo;

    public async Task<List<ReturnOrderListDto>> Handle(GetReturnOrdersQuery query, CancellationToken ct)
    {
        var list = await _repo.GetAllAsync(query.CompanyId, ct);
        return list.Select(r => new ReturnOrderListDto(
            r.Id, r.ReturnNumber, r.GRNId, r.POId, r.VendorId,
            r.GoodsReceipt?.PurchaseOrder?.Vendor?.LegalName,
            r.Status, r.Reason, r.CreatedAt, r.AcknowledgedAt, r.ShippedAt, r.ReceivedAt
        )).ToList();
    }
}
