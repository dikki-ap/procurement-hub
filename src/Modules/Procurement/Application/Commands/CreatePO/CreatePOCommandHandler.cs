using MediatR;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.CreatePO;

public class CreatePOCommandHandler : ICommandHandler<CreatePOCommand, Guid>
{
    private readonly IPurchaseOrderRepository _repo;

    public CreatePOCommandHandler(IPurchaseOrderRepository repo) => _repo = repo;

    public async Task<Guid> Handle(CreatePOCommand command, CancellationToken ct)
    {
        var poNumber = await _repo.GenerateNextNumberAsync(command.CompanyId, ct);

        var po = PurchaseOrder.Create(
            command.CompanyId,
            poNumber,
            command.VendorId,
            command.RFQId,
            command.CurrencyId,
            command.PaymentTermId,
            command.DeliveryLocationId,
            command.ExpectedDelivery,
            command.Notes,
            command.TermsConditions);

        foreach (var item in command.Items)
        {
            var total = item.Quantity * item.UnitPrice;
            po.Items.Add(new POItem
            {
                POId        = po.Id,
                MaterialId  = item.MaterialId,
                Description = item.Description,
                Quantity    = item.Quantity,
                UomId       = item.UomId,
                UnitPrice   = item.UnitPrice,
                TotalPrice  = total,
            });
        }

        po.RecalculateTotal();
        _repo.Add(po);
        await _repo.SaveChangesAsync(ct);

        return po.Id;
    }
}
