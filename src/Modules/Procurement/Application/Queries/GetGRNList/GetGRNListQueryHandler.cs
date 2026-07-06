using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetGRNList;

public class GetGRNListQueryHandler : IQueryHandler<GetGRNListQuery, List<GRNListDto>>
{
    private readonly IGoodsReceiptRepository  _grnRepo;
    private readonly IPurchaseOrderRepository _poRepo;

    public GetGRNListQueryHandler(IGoodsReceiptRepository grnRepo, IPurchaseOrderRepository poRepo)
    {
        _grnRepo = grnRepo;
        _poRepo  = poRepo;
    }

    public async Task<List<GRNListDto>> Handle(GetGRNListQuery query, CancellationToken ct)
    {
        var po   = await _poRepo.GetByIdAsync(query.POId, ct);
        var grns = await _grnRepo.GetByPOAsync(query.POId, ct);

        return grns.Select(g => new GRNListDto(
            g.Id,
            g.GRNNumber,
            g.POId,
            po?.PONumber ?? string.Empty,
            g.Status,
            g.ReceivedAt,
            g.CreatedAt)).ToList();
    }
}
