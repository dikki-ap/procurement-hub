using MediatR;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Commands.CreateGRN;

public class CreateGRNCommandHandler : ICommandHandler<CreateGRNCommand, Guid>
{
    private readonly IGoodsReceiptRepository  _grnRepo;
    private readonly IPurchaseOrderRepository _poRepo;

    public CreateGRNCommandHandler(
        IGoodsReceiptRepository grnRepo,
        IPurchaseOrderRepository poRepo)
    {
        _grnRepo = grnRepo;
        _poRepo  = poRepo;
    }

    public async Task<Guid> Handle(CreateGRNCommand command, CancellationToken ct)
    {
        var po = await _poRepo.GetByIdWithItemsAsync(command.POId, ct)
                 ?? throw new NotFoundException("PurchaseOrder", command.POId);

        foreach (var item in command.Items)
        {
            var poItem = po.Items.FirstOrDefault(i => i.Id == item.POItemId)
                         ?? throw new NotFoundException("POItem", item.POItemId);

            var alreadyReceived = poItem.ReceivedQty;
            var totalAfter      = alreadyReceived + item.ReceivedQty;

            if (totalAfter > poItem.Quantity)
                throw new BusinessRuleException("GRNQuantity",
                    $"Received qty ({totalAfter}) exceeds ordered qty ({poItem.Quantity}) for item '{poItem.Description}'.");
        }

        var grnNumber = await _grnRepo.GenerateNextNumberAsync(ct);

        var grn = GoodsReceipt.Create(
            grnNumber,
            command.POId,
            command.ReceivedBy,
            command.DeliveryNoteNo,
            command.Notes);

        foreach (var item in command.Items)
        {
            grn.Items.Add(new GRNItem
            {
                GRNId         = grn.Id,
                POItemId      = item.POItemId,
                ReceivedQty   = item.ReceivedQty,
                RejectedQty   = item.RejectedQty,
                QualityStatus = item.QualityStatus,
                RejectReason  = item.RejectReason,
                Notes         = item.Notes,
            });
        }

        _grnRepo.Add(grn);
        await _grnRepo.SaveChangesAsync(ct);

        return grn.Id;
    }
}
