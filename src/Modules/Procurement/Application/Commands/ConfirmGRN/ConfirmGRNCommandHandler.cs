using MediatR;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Commands.ConfirmGRN;

public class ConfirmGRNCommandHandler : ICommandHandler<ConfirmGRNCommand>
{
    private readonly IGoodsReceiptRepository  _grnRepo;
    private readonly IPurchaseOrderRepository _poRepo;

    public ConfirmGRNCommandHandler(
        IGoodsReceiptRepository grnRepo,
        IPurchaseOrderRepository poRepo)
    {
        _grnRepo = grnRepo;
        _poRepo  = poRepo;
    }

    public async Task<Unit> Handle(ConfirmGRNCommand command, CancellationToken ct)
    {
        var grn = await _grnRepo.GetByIdWithItemsAsync(command.GRNId, ct)
                  ?? throw new NotFoundException("GoodsReceipt", command.GRNId);

        var po = await _poRepo.GetByIdWithItemsAsync(grn.POId, ct)
                 ?? throw new NotFoundException("PurchaseOrder", grn.POId);

        // Update received qty on PO items
        foreach (var grnItem in grn.Items)
        {
            var poItem = po.Items.FirstOrDefault(i => i.Id == grnItem.POItemId);
            poItem?.UpdateReceivedQty(grnItem.ReceivedQty);
        }

        grn.Confirm(command.VendorId);

        // Auto-complete PO if all items fully received
        var allFulfilled = po.Items.All(i => i.ReceivedQty >= i.Quantity);
        if (allFulfilled && po.Status is Domain.Enums.POStatus.Acknowledged or Domain.Enums.POStatus.InDelivery)
            po.Complete();

        _grnRepo.Update(grn);
        _poRepo.Update(po);
        await _grnRepo.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
