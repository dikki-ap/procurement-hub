using MediatR;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Commands.AcknowledgePO;

public class AcknowledgePOCommandHandler : ICommandHandler<AcknowledgePOCommand>
{
    private readonly IPurchaseOrderRepository _repo;

    public AcknowledgePOCommandHandler(IPurchaseOrderRepository repo) => _repo = repo;

    public async Task<Unit> Handle(AcknowledgePOCommand command, CancellationToken ct)
    {
        var po = await _repo.GetByIdAsync(command.POId, ct)
                 ?? throw new NotFoundException("PurchaseOrder", command.POId);

        po.Acknowledge();
        _repo.Update(po);
        await _repo.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
