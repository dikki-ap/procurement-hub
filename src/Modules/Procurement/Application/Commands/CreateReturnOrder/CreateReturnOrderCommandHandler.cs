using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Commands.CreateReturnOrder;

public class CreateReturnOrderCommandHandler : ICommandHandler<CreateReturnOrderCommand, Guid>
{
    private readonly IReturnOrderRepository  _repo;
    private readonly IGoodsReceiptRepository _grnRepo;

    public CreateReturnOrderCommandHandler(
        IReturnOrderRepository  repo,
        IGoodsReceiptRepository grnRepo)
    {
        _repo    = repo;
        _grnRepo = grnRepo;
    }

    public async Task<Guid> Handle(CreateReturnOrderCommand command, CancellationToken ct)
    {
        var grn = await _grnRepo.GetByIdAsync(command.GRNId, ct)
            ?? throw new NotFoundException("GoodsReceipt", command.GRNId);

        if (!command.Items.Any())
            throw new BusinessRuleException("CreateReturn", "Return order must have at least one item.");

        var returnNumber = await _repo.GenerateNextNumberAsync(ct);
        var returnOrder  = ReturnOrder.Create(
            returnNumber, grn.Id, grn.POId, command.VendorId, command.Reason, command.Notes);

        foreach (var item in command.Items)
        {
            returnOrder.Items.Add(new ReturnOrderItem
            {
                ReturnOrderId   = returnOrder.Id,
                POItemId        = item.POItemId,
                ItemDescription = item.ItemDescription,
                Quantity        = item.Quantity,
                Uom             = item.Uom,
                ReturnReason    = item.ReturnReason,
            });
        }

        _repo.Add(returnOrder);
        await _repo.SaveChangesAsync(ct);

        return returnOrder.Id;
    }
}
