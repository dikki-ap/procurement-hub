using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetPOById;

public class GetPOByIdQueryHandler : IQueryHandler<GetPOByIdQuery, PODto>
{
    private readonly IPurchaseOrderRepository _repo;

    public GetPOByIdQueryHandler(IPurchaseOrderRepository repo) => _repo = repo;

    public async Task<PODto> Handle(GetPOByIdQuery query, CancellationToken ct)
    {
        var po = await _repo.GetByIdWithItemsAsync(query.Id, ct)
                 ?? throw new NotFoundException("PurchaseOrder", query.Id);

        return new PODto(
            po.Id,
            po.PONumber,
            po.RFQId,
            po.VendorId,
            po.VendorId.ToString(),
            po.Status,
            po.TotalAmount,
            po.CurrencyId,
            null,
            po.PaymentTermId,
            null,
            po.DeliveryLocationId,
            null,
            po.ExpectedDelivery,
            po.ActualDelivery,
            po.FileUrl,
            po.Notes,
            po.TermsConditions,
            po.IssuedAt,
            po.AcknowledgedAt,
            po.CompletedAt,
            po.CancelledAt,
            po.CancelledReason,
            po.CreatedAt,
            po.Items.Select(i => new POItemDto(
                i.Id,
                i.MaterialId,
                i.Description,
                i.Quantity,
                null,
                i.UnitPrice,
                i.TotalPrice,
                i.ReceivedQty)).ToList());
    }
}
