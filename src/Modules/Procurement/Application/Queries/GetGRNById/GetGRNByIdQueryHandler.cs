using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetGRNById;

public class GetGRNByIdQueryHandler : IQueryHandler<GetGRNByIdQuery, GRNDto>
{
    private readonly IGoodsReceiptRepository  _grnRepo;
    private readonly IPurchaseOrderRepository _poRepo;

    public GetGRNByIdQueryHandler(IGoodsReceiptRepository grnRepo, IPurchaseOrderRepository poRepo)
    {
        _grnRepo = grnRepo;
        _poRepo  = poRepo;
    }

    public async Task<GRNDto> Handle(GetGRNByIdQuery query, CancellationToken ct)
    {
        var grn = await _grnRepo.GetByIdWithItemsAsync(query.Id, ct)
                  ?? throw new NotFoundException("GoodsReceipt", query.Id);

        var po = await _poRepo.GetByIdWithItemsAsync(grn.POId, ct);

        return new GRNDto(
            grn.Id,
            grn.GRNNumber,
            grn.POId,
            po?.PONumber ?? string.Empty,
            grn.Status,
            grn.ReceivedBy,
            grn.ReceivedAt,
            grn.DeliveryNoteNo,
            grn.Notes,
            grn.CreatedAt,
            grn.Items.Select(i =>
            {
                var poItem = po?.Items.FirstOrDefault(p => p.Id == i.POItemId);
                return new GRNItemDto(
                    i.Id,
                    i.POItemId,
                    poItem?.Description ?? string.Empty,
                    i.ReceivedQty,
                    i.RejectedQty,
                    i.QualityStatus,
                    i.RejectReason,
                    i.Notes);
            }).ToList());
    }
}
